using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Packing.Business;
using Enterprise.Rating.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportConsignment.Business;
using PkgPackageCollection = Enterprise.Packing.Business.PkgPackageCollection;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class PackageWrapperCollection : GenericWrapperCollection<PackageWrapper>
	{
		#region Constructors

		public PackageWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Consol

		public PackageWrapperCollection(CommonConsol consolBO, BusinessObjectFactory factory)
			: this(consolBO, false, false, factory)
		{
		}

		public PackageWrapperCollection(CommonConsol consolBO, ZBool includeDuplicates, ZBool includeMastersOnly, BusinessObjectFactory factory)
			: base(factory)
		{
			if (consolBO != null)
			{
				CommonShipment[] shipments = consolBO.Shipments.ToArray<CommonShipment>();
				Array.Sort(shipments, new HouseBillComparer());

				foreach (CommonShipment shipmentBO in shipments)
				{
					foreach (PackLine package in shipmentBO.OuterPackLines)
					{
						if ((!includeMastersOnly && !ContainsWrappedObject(package.PK)) || includeDuplicates || (includeMastersOnly && (shipmentBO.JS_JS_ColoadMasterShipment.IsEmpty || shipmentBO.CoLoadMasterShipment.IsBuyersConsolLead)))
						{
							var wrapper = new PackageWrapperFromFreightPackage(package, factory);
							wrapper.SetParentConsol(consolBO);
							if (includeDuplicates || includeMastersOnly)
							{
								wrapper.SetParentShipment(shipmentBO);
							}
							Add(wrapper);
						}
					}
				}
			}
		}

		#endregion

		#region Shipment

		public PackageWrapperCollection(CommonShipment shipmentBO, BusinessObjectFactory factory)
			: base(factory)
		{
			AddPackLines(shipmentBO != null ? shipmentBO.OuterPackLines.Cast<PackLine>() : Enumerable.Empty<PackLine>(), factory);
		}

		public PackageWrapperCollection(IEnumerable<PackLine> packLines, BusinessObjectFactory factory)
			: base(factory)
		{
			AddPackLines(packLines ?? Enumerable.Empty<PackLine>(), factory);
		}

		void AddPackLines(IEnumerable<PackLine> packLines, BusinessObjectFactory factory)
		{
			foreach (PackLine package in packLines)
			{
				Add(new PackageWrapperFromFreightPackage(package, factory));
			}
		}

		#endregion

		#region Declaration

		public PackageWrapperCollection(BaseJobDeclaration declaration, BusinessObjectFactory factory)
			: base(factory)
		{
			if (declaration != null)
			{
				foreach (BasePackage package in declaration.Packages)
				{
					Add(new PackageWrapperFromCustomsPackage(package, factory));
				}
			}
		}

		#endregion

		#region Cartage

		public PackageWrapperCollection(CommonCartage cartage, BusinessObjectFactory factory)
			: base(factory)
		{
			if (cartage != null)
			{
				foreach (CommonBookedCtgMove move in cartage.LooseBookedMoves)
				{
					Add(new PackageWrapperFromCartagePackage(move, factory));
				}
			}
		}

		#endregion

		#region PickUpDeliveryConfirmation

		public PackageWrapperCollection(CommonPickupDeliveryConfirm deliveryConfirm, BusinessObjectFactory factory)
			: base(factory)
		{
			if (deliveryConfirm != null)
			{
				foreach (CommonConfirmDivot divot in deliveryConfirm.Divots)
				{
					if (divot.J8_PackagesDelivered > 0)
					{
						Add(new PackageWrapperFromFreightPackage(divot.PackLine, Factory));
					}
				}
			}
		}

		#endregion

		#region Quote

		public PackageWrapperCollection(RateOneOffShipment oneOffQuote, BusinessObjectFactory factory)
			: base(factory)
		{
			if (oneOffQuote != null)
			{
				foreach (var pack in oneOffQuote.LooseCargo)
				{
					Add(new PackageWrapperFromOneOffContainer(pack, factory));
				}
			}
		}

		#endregion

		#region Land Transport

		public PackageWrapperCollection(DtbConsignment consignment, BusinessObjectFactory factory)
			: base(factory)
		{
			if (consignment != null)
			{
				var listOfAddedPackages = new List<PkgPackage>();
				foreach (var package in consignment.PackageJob.Packages)
				{
					if (!package.IsContainer && !listOfAddedPackages.Contains(package))
					{
						var packageWrapper = new PackageWrapperFromPkgPackage(package, Factory);
						Add(packageWrapper);
						listOfAddedPackages.Add(package);
					}
				}
			}
		}

		#endregion

		#region Transport

		public PackageWrapperCollection(DtbTransport transport, BusinessObjectFactory factory)
			: base(factory)
		{
			if (transport != null)
			{
				AddPackagesFromTransportBooking(new List<PkgPackage>(), transport);
			}
		}

		public PackageWrapperCollection(DtbBooking booking, BusinessObjectFactory factory)
			: base(factory)
		{
			if (booking != null)
			{
				AddPackagesFromTransportBooking(new List<PkgPackage>(), booking);
			}
		}

		#endregion

		#region Transport Booking

		public PackageWrapperCollection(DtbBookingConsolidation bookingConsolidation, BusinessObjectFactory factory)
			: base(factory)
		{
			if (bookingConsolidation != null)
			{
				var listOfAddedPackages = new List<PkgPackage>();
				foreach (DtbBooking booking in bookingConsolidation.Bookings)
				{
					AddPackagesFromTransportBooking(listOfAddedPackages, booking);
				}
			}
		}

		#endregion

		public PackageWrapperCollection(CommonContainer container, BusinessObjectFactory factory)
			: base(factory)
		{
			if (container != null)
			{
				foreach (PackLine packLine in container.PackLines)
				{
					if (packLine.Shipment != null)
					{
						Add(new PackageWrapperFromFreightPackage(packLine, Factory));
					}
				}
			}
		}

		#region PackageWrapperCollection from dbo.PkgPackageJob

		public enum PackSelection
		{
			Selected,
			All,
		}

		public enum PackLevel
		{
			All = -1,
			First = 0,
			Second = 1,
		}

		public PackageWrapperCollection(IEnumerable<PkgPackageJob> packageJobs, PackLevel packLevel, PackSelection packSelection, BusinessObjectFactory factory, bool checkPackageID = true)
			: base(factory)
		{
			foreach (var packageJob in packageJobs)
			{
				if (packSelection == PackSelection.Selected)
				{
					AddPackages(packageJob.Selected.SelectedPackages, packLevel, 0, checkPackageID);
				}
				else
				{
					AddPackages(packageJob.Packages, packLevel, 0, checkPackageID);
				}
			}
		}

		void AddPackages(IEnumerable<PkgPackage> packages, PackLevel levelWanted, int currentLevel, bool checkPackageID)
		{
			foreach (var package in packages)
			{
				if (!checkPackageID || !package.KP_PackageID.IsEmpty)
				{
					Add(new PackageWrapperFromPkgPackage(package, Factory));
				}

				if (currentLevel != (int)levelWanted)
				{
					AddPackages(package.Packages, levelWanted, currentLevel + 1, checkPackageID);
				}
			}
		}

		public PackageWrapperCollection(PkgPackageJob packageJob, BusinessObjectFactory factory)
			: base(factory)
		{
			if (packageJob != null)
			{
				ZInt displayOrder = 1;
				AddPackagesFromPackageCollection(packageJob.Packages, ref displayOrder, 0);
			}
		}

		void AddPackagesFromPackageCollection(PkgPackageCollection packages, ref ZInt displayOrder, ZInt indent)
		{
			var comparer = new PkgPackageComparer();
			using (comparer.CacheSortingProperties())
			{
				packages.ApplySort(comparer);

				foreach (var package in packages)
				{
					AddPackedItems(package, displayOrder, indent);
					displayOrder++;
					AddPackagesFromPackageCollection(package.Packages, ref displayOrder, indent + 1);
				}
			}
		}

		void AddPackedItems(PkgPackage package, ZInt displayOrder, int indent)
		{
			if (package.PackedItems.Count == 0)
			{
				var packageWrapper = new PackageWrapperFromPkgPackage(package, Factory, displayOrder, indent);
				Add(packageWrapper);
			}
			else
			{
				var groupedPackedItems = package.PackedItems.Typed.GroupBy(d => d.Key);
				foreach (var group in groupedPackedItems)
				{
					var packageWrapper = new PackageWrapperFromPkgPackage(package, group.ToArray(), Factory, displayOrder, indent);
					Add(packageWrapper);
				}
			}
		}

		#endregion

		void AddPackagesFromTransportBooking(List<PkgPackage> listOfAddedPackages, DtbTransport transport)
		{
			foreach (DtbTransportInstruction instruction in transport.Instructions)
			{
				foreach (DtbTransportInstructionPkgDivot instructionPkgDivot in instruction.PackageDivots)
				{
					PkgPackage package = instructionPkgDivot.Package;
					if (!package.IsContainer && !listOfAddedPackages.Contains(package))
					{
						var packageWrapper = new PackageWrapperFromPkgPackage(package, Factory);
						Add(packageWrapper);
						listOfAddedPackages.Add(package);
					}
				}
			}
		}

		void AddPackagesFromTransportBooking(List<PkgPackage> listOfAddedPackages, DtbBooking booking)
		{
			foreach (var instruction in booking.Instructions)
			{
				foreach (var instructionPkgDivot in instruction.PackageDivots)
				{
					var package = instructionPkgDivot.Package;
					if (!package.IsContainer && !listOfAddedPackages.Contains(package))
					{
						var packageWrapper = new PackageWrapperFromPkgPackage(package, Factory);
						Add(packageWrapper);
						listOfAddedPackages.Add(package);
					}
				}
			}
		}

		#endregion

		#region Properties

		public ZString PackagesSummaryDangerousOnly
		{
			get
			{
				List<string> result = new List<string>();
				List<string> uNDGItems = new List<string>();
				ZString item = "";

				foreach (PackageWrapper package in this)
				{
					if (HasDangerousGoods(package))
					{
						foreach (UNDGSubstanceWrapper substance in package.UNDGSubstances)
						{
							if (!uNDGItems.Contains(package.Commodity.Code + package.HarmonizedCode + substance.UNNumber + substance.PSAGroup))
							{
								item = BuildItem(item, package);
								if (item.Length > 0)
								{ item += " - "; }
								item += substance.Summary;

								result.Add(item);
								uNDGItems.Add(package.Commodity.Code + package.HarmonizedCode + substance.UNNumber + substance.PSAGroup);
							}
						}
					}
				}

				var allUNDGs = this.OfType<PackageWrapper>()
					.SelectMany(package => package.UNDGSubstances)
					.OfType<UNDGSubstanceWrapper>()
					.ToArray();

				var helper = new UNDGSubstanceWrapperHelper();
				var summary = helper.GetUNDGPackagesSummary(allUNDGs);
				if (!summary.IsEmpty)
				{
					result.Add(summary);
				}

				if (result.Count == 0)
				{ return ZString.Empty; }
				else
				{ return string.Join(System.Environment.NewLine, result.ToArray()); }
			}
		}

		public ZString PackagesSummaryAll
		{
			get
			{
				List<string> result = new List<string>();
				List<string> commodityItems = new List<string>();
				ZString item = "";

				foreach (PackageWrapper package in this)
				{
					if (!HasDangerousGoods(package) && !commodityItems.Contains(package.Commodity.Code + package.HarmonizedCode))
					{
						item = BuildItem(item, package);
						result.Add(item);
						commodityItems.Add(package.Commodity.Code + package.HarmonizedCode);
					}
				}

				ZString hazardousPackages = this.PackagesSummaryDangerousOnly;

				var trimWhiteSpaceAndNewLine = new List<char>();
				trimWhiteSpaceAndNewLine.Add(' ');
				trimWhiteSpaceAndNewLine.AddRange(System.Environment.NewLine.ToCharArray());
				return (hazardousPackages != "" ? hazardousPackages + System.Environment.NewLine : "") + string.Join(System.Environment.NewLine, result.ToArray()).Trim(trimWhiteSpaceAndNewLine.ToArray());
			}
		}

		bool HasDangerousGoods(PackageWrapper package)
		{
			foreach (UNDGSubstanceWrapper substance in package.UNDGSubstances)
			{
				if (!substance.UNNumber.IsEmpty)
				{
					return true;
				}
			}

			return false;
		}

		ZString BuildItem(ZString item, PackageWrapper package)
		{
			item = package.Commodity.Code;

			if (!package.Commodity.Description.IsEmpty && package.Commodity.Code != package.Commodity.Description)
			{
				item += " (" + package.Commodity.Description + ")";
			}

			if (!package.HarmonizedCode.IsEmpty)
			{
				item += item.IsEmpty ? "" : " - ";
				item += package.HarmonizedCode;
			}

			return item;
		}

		#endregion

		#region Implementation

		class HouseBillComparer : IComparer
		{
			#region IComparer Members

			public int Compare(object x, object y)
			{
				return string.Compare(((CommonShipment)x).JS_HouseBill, ((CommonShipment)y).JS_HouseBill);
			}

			#endregion
		}

		#endregion
	}
}
