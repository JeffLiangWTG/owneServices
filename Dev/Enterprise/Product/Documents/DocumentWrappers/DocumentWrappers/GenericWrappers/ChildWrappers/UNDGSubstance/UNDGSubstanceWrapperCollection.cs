using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Business;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class UNDGSubstanceWrapperCollection : GenericWrapperCollection<UNDGSubstanceWrapper>
	{
		#region Constructors

		public UNDGSubstanceWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public UNDGSubstanceWrapperCollection(PackLine[] packLines, BusinessObjectFactory factory)
			: this(factory)
		{
			if (packLines != null)
			{
				foreach (PackLine packline in packLines)
				{
					PackageWrapper packageWrapper = new PackageWrapperFromFreightPackage(packline, factory);
					AddRange(packageWrapper.UNDGSubstances);
				}
			}
		}

		public UNDGSubstanceWrapperCollection(List<BasePackage> packages, BusinessObjectFactory factory)
			: this(factory)
		{
			if (packages != null)
			{
				foreach (BasePackage package in packages)
				{
					PackageWrapper packageWrapper = new PackageWrapperFromCustomsPackage(package, factory);
					AddRange(packageWrapper.UNDGSubstances);
				}
			}
		}

		public UNDGSubstanceWrapperCollection(DtbTransport transport, BusinessObjectFactory factory)
			: this(factory)
		{
			if (transport != null)
			{
				AddUNDGsFromTransportBooking(new List<PkgPackage>(), transport);
			}
		}

		public UNDGSubstanceWrapperCollection(DtbBooking booking, BusinessObjectFactory factory)
			: this(factory)
		{
			if (booking != null)
			{
				AddUNDGsFromTransportBooking(new List<PkgPackage>(), booking);
			}
		}

		public UNDGSubstanceWrapperCollection(DtbBookingConsolidation bookingConsolidation, BusinessObjectFactory factory)
			: this(factory)
		{
			if (bookingConsolidation != null)
			{
				var listOfAddedPackages = new List<PkgPackage>();
				foreach (DtbBooking booking in bookingConsolidation.Bookings)
				{
					AddUNDGsFromTransportBooking(listOfAddedPackages, booking);
				}
			}
		}

		void AddUNDGsFromTransportBooking(List<PkgPackage> listOfAddedPackages, DtbTransport transport)
		{
			foreach (DtbTransportInstruction instruction in transport.Instructions)
			{
				foreach (DtbTransportInstructionPkgDivot instructionPkgDivot in instruction.PackageDivots)
				{
					PkgPackage package = instructionPkgDivot.Package;
					AddUNDGsFromPackage(listOfAddedPackages, package);
				}
			}
		}

		void AddUNDGsFromTransportBooking(List<PkgPackage> listOfAddedPackages, DtbBooking booking)
		{
			foreach (var instruction in booking.Instructions)
			{
				foreach (var instructionPkgDivot in instruction.PackageDivots)
				{
					var package = instructionPkgDivot.Package;
					AddUNDGsFromPackage(listOfAddedPackages, package);
				}
			}
		}

		void AddUNDGsFromPackage(List<PkgPackage> listOfAddedPackages, PkgPackage package)
		{
			if (!listOfAddedPackages.Contains(package))
			{
				var packageWrapper = new PackageWrapperFromPkgPackage(package, Factory);
				foreach (UNDGDataItem undgDataItem in package.UNDGs)
				{
					var undgDataItemWrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
					undgDataItemWrapper.ContainingPackage = packageWrapper;
					Add(undgDataItemWrapper);
				}
				listOfAddedPackages.Add(package);

				foreach (var childPackage in package.Packages)
				{
					AddUNDGsFromPackage(listOfAddedPackages, childPackage);
				}
			}
		}

		#region UNDGSubstanceWrapperCollection (WhsDocket)

		public UNDGSubstanceWrapperCollection(WhsDocket docket, BusinessObjectFactory factory)
			: this(factory)
		{
			if (docket != null)
			{
				AddUNDGsFromWhsDocket(docket);
			}
		}

		void AddUNDGsFromWhsDocket(WhsDocket docket)
		{
			var undgItemsDictionary = new Dictionary<UNDGDataItem, ZDecimal>();
			foreach (var docketLine in docket.Lines)
			{
				var part = docketLine.SupplierPart;
				if (part != null)
				{
					foreach (var undgDataItem in part.UNDGs)
					{
						if (!undgItemsDictionary.ContainsKey(undgDataItem))
						{
							undgItemsDictionary.Add(undgDataItem, 0m);
						}
						undgItemsDictionary[undgDataItem] += docketLine.WE_TransactionQuantity;
					}
				}
			}

			foreach (var pair in undgItemsDictionary)
			{
				Add(new UNDGSubstanceWrapperForWhs(pair.Key, Factory, pair.Value));
			}
		}

		#endregion

		public UNDGSubstanceWrapperCollection(PkgPackage package, BusinessObjectFactory factory)
			: this(factory)
		{
			if (package != null)
			{
				var packageWrapper = new PackageWrapperFromPkgPackage(package, Factory);
				foreach (var undgDataItem in package.UNDGs)
				{
					var undgDataItemWrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
					undgDataItemWrapper.ContainingPackage = packageWrapper;
					Add(undgDataItemWrapper);
				}
			}
		}

		#endregion

		#region Properties

		public ZString UNNumbers
		{
			get
			{
				var list = new List<string>();
				foreach (UNDGSubstanceWrapper undg in this)
				{
					if (!undg.UNNumber.IsEmpty && !list.Contains(undg.UNNumber))
					{
						list.Add(undg.UNNumber);
					}
				}

				return string.Join(", ", list.ToArray());
			}
		}

		public ZString UNNumbersFallBackToIMOClasses
		{
			get
			{
				var list = new List<string>();
				foreach (UNDGSubstanceWrapper undg in this)
				{
					if (!undg.UNNumber.IsEmpty)
					{
						if (!list.Contains(undg.UNNumber))
						{
							list.Add(undg.UNNumber);
						}
					}
					else if (!undg.IMOClass.IsEmpty && !list.Contains(undg.IMOClass))
					{
						list.Add(undg.IMOClass);
					}
				}

				return string.Join(", ", list.ToArray());
			}
		}

		public ContactWrapper UNDGContact
		{
			get
			{
				foreach (UNDGSubstanceWrapper wrapper in this)
				{
					if (!wrapper.DGContact.FullName.IsEmpty)
					{
						return wrapper.DGContact;
					}
				}

				return new ContactWrapper(Factory.GetNull<OrgContact>(), Factory);
			}
		}

		public ZString FormattedIMOClassAndPSAGroupWithLabel => GetUniqueFormattedText(undg => ZString.Format("{0} {1}", undg.IMOClass, undg.PSAGroupWithLabel));

		public ZString FormattedIMOClassAndPSAGroupWithLabelAndProperShippingName => GetUniqueFormattedText(undg => ZString.Format("{0} {1} ({2})", undg.IMOClass, undg.PSAGroupWithLabel, undg.ProperShippingName));

		#endregion

		#region Implementation

		ZString GetUniqueFormattedText(Func<UNDGSubstanceWrapper, ZString> textFormatter, string seperator = ", ")
		{
			var list = new List<ZString>();
			foreach (UNDGSubstanceWrapper undg in this)
			{
				var text = textFormatter(undg);
				if (!list.Contains(text))
				{
					list.Add(text);
				}
			}

			return string.Join(seperator, list.ToArray());
		}

		#endregion
	}
}
