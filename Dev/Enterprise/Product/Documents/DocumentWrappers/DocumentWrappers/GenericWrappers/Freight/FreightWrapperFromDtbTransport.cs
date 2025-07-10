using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public abstract class FreightWrapperFromDtbTransport<T> : FreightWrapper, IDocTypeCode
		where T : DtbTransport
	{
		protected FreightWrapperFromDtbTransport(T transport, BusinessObjectFactory factory)
			: base(transport ?? factory.GetNull<T>(), factory)
		{
		}

		#region GetJobNumberHeading

		protected override ZString GetJobNumberHeading()
		{
			return Res.GetString("e0267c3d-bf92-4bed-ace8-290369b3660b", "Transport Booking");
		}

		#endregion

		#region GetJobNumber

		protected override ZString GetJobNumber()
		{
			return Transport.KM_JobID;
		}

		#endregion

		#region GetSecondaryNumber

		protected override ZString GetSecondaryNumber()
		{
			return ParentWrapper != null ? ParentWrapper.JobNumber : ZString.Empty;
		}

		#endregion

		#region GetTransportReference

		protected override ZString GetTransportReference()
		{
			return Transport.KM_TransportReference;
		}

		#endregion

		#region GetJob

		protected override Job GetJob()
		{
			return Transport == null ? null : (Job)new JobHeader.Loader(Transport).Load();
		}

		#endregion

		#region GetContainers

		protected override ContainerWrapperCollection GetContainers()
		{
			return new ContainerWrapperCollection(Transport, Factory);
		}

		#endregion

		#region GetCustomsEntries

		protected override CustomsEntryWrapperCollection GetCustomsEntries()
		{
			return new CustomsEntryWrapperCollection(Transport, Factory);
		}

		#endregion

		#region GetPackages

		protected override PackageWrapperCollection GetPackages()
		{
			return new PackageWrapperCollection(Transport, Factory);
		}

		#endregion

		#region GetBookingInstructions

		protected override InstructionWrapperCollection GetBookingInstructions()
		{
			var bookingInstructions = new InstructionWrapperCollection(Factory);
			foreach (DtbTransportInstruction instruction in Transport.Instructions)
			{
				AddWrappersFromBookingInstruction(bookingInstructions, instruction);
			}
			return bookingInstructions;
		}

		#region AddWrappersFromBookingInstruction

		void AddWrappersFromBookingInstruction(InstructionWrapperCollection bookingInstructions, DtbTransportInstruction instruction)
		{
			if (instruction.PackageDivots.Count > 0)
			{
				AddWrappersFromInstructionPkgDivot(bookingInstructions, instruction);
			}
			else if (instruction.Confirmations.Count > 0)
			{
				AddWrappersFromInstructionConfirmations(bookingInstructions, instruction);
			}
			else
			{
				var instructionWrapper = GetInstructionWrapper(instruction, Factory);
				bookingInstructions.Add(instructionWrapper);
			}
		}

		#endregion

		#region AddWrappersFromInstructionPkgDivot

		void AddWrappersFromInstructionPkgDivot(InstructionWrapperCollection bookingInstructions, DtbTransportInstruction instruction)
		{
			foreach (DtbTransportInstructionPkgDivot instructionPkgDivot in instruction.PackageDivots)
			{
				if (instructionPkgDivot.Confirmations.Count > 0)
				{
					foreach (DtbTransportConfirmation bookingConfirmation in instructionPkgDivot.Confirmations)
					{
						var instructionWrapper = GetInstructionWrapper(instruction, instructionPkgDivot, bookingConfirmation, Factory);
						bookingInstructions.Add(instructionWrapper);
					}
				}
				else
				{
					var instructionWrapper = GetInstructionWrapper(instruction, instructionPkgDivot, null, Factory);
					bookingInstructions.Add(instructionWrapper);
				}
			}
		}

		#endregion

		#region AddWrappersFromInstructionConfirmations

		void AddWrappersFromInstructionConfirmations(InstructionWrapperCollection bookingInstructions, DtbTransportInstruction instruction)
		{
			foreach (DtbTransportConfirmation confirmation in instruction.Confirmations)
			{
				var instructionWrapper = GetInstructionWrapper(instruction, null, confirmation, Factory);
				bookingInstructions.Add(instructionWrapper);
			}
		}

		#endregion

		protected abstract TransportInstructionWrapper GetInstructionWrapper(DtbTransportInstruction instruction, BusinessObjectFactory factory);
		protected abstract TransportInstructionWrapper GetInstructionWrapper(DtbTransportInstruction instruction, DtbTransportInstructionPkgDivot divot, DtbTransportConfirmation confirmation, BusinessObjectFactory factory);

		#endregion

		#region GetUNDGs

		protected override UNDGSubstanceWrapperCollection GetUNDGs()
		{
			return new UNDGSubstanceWrapperCollection(Transport, Factory);
		}

		#endregion

		#region GetTextNotes

		protected override NoteWrapperCollection GetTextNotes()
		{
			var notes = base.GetTextNotes();
			if (ParentWrapper != null)
			{
				notes.AddNotes(GetTextNotesCore);
			}

			return notes;
		}

		protected virtual BusinessObject GetTextNotesCore
		{
			get { return Transport; }
		}

		#endregion

		#region GetShipmentOuterPacksQty

		protected override PackQTYWrapper GetShipmentOuterPacksQty()
		{
			var total = 0;
			var unit = ZString.Empty;

			foreach (Package_PackageView package in Transport.Packages_PackageView)
			{
				if (!package.Package.IsContainer)
				{
					total += package.QuantityFromInstructions;

					if (unit.IsEmpty)
					{
						unit = package.Package.KP_F3_NKPackType;
					}
					else if (unit != package.Package.KP_F3_NKPackType)
					{
						unit = Constants.PkgUnit.Package;
					}
				}
			}

			return new PackQTYWrapper(total, unit, BindToLists.GetCachedLists(Factory).OuterPackTypes.GetAsCodeDescriptionPair(), Factory);
		}

		#endregion

		#region GetWeight

		protected override WeightWrapper GetWeight()
		{
			var total = ZWeight.Empty;

			var packages = Transport.Packages_PackageView;
			if (packages.Any())
			{
				var unitCount = packages.Cast<Package_PackageView>().Select(p => p.WeightFromInstructions.Unit).Distinct().Count();
				if (unitCount == 1)
				{
					var firstPackage = (Package_PackageView)packages.FirstOrDefault();
					total = firstPackage.WeightFromInstructions;

					foreach (Package_PackageView package in packages.Skip(1))
					{
						total += package.WeightFromInstructions;
					}
				}
				else
				{
					foreach (Package_PackageView package in packages)
					{
						total += package.WeightFromInstructions;
					}
				}
			}

			return new WeightWrapper(total.Amount, total.Unit, WeightWrapper.StandardDecimalPlaces, Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight), Factory);
		}

		#endregion

		#region GetVolume

		protected override VolumeWrapper GetVolume()
		{
			var total = ZVolume.Empty;

			var packages = Transport.Packages_PackageView;
			if (packages.Any())
			{
				var unitCount = packages.Cast<Package_PackageView>().Select(p => p.VolumeFromInstructions.Unit).Distinct().Count();
				if (unitCount == 1)
				{
					var firstPackage = (Package_PackageView)packages.FirstOrDefault();
					total = firstPackage.VolumeFromInstructions;

					foreach (Package_PackageView package in packages.Skip(1))
					{
						total += package.VolumeFromInstructions;
					}
				}
				else
				{
					foreach (Package_PackageView package in packages)
					{
						total += package.VolumeFromInstructions;
					}
				}
			}

			return new VolumeWrapper(total.Amount, total.Unit, Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume), Factory);
		}

		#endregion

		#region GetTransportAddresses

		protected override AddressWrapperCollection GetTransportAddresses()
		{
			var addresses = new AddressWrapperCollection(Factory);

			foreach (DtbTransportInstruction instruction in Transport.Instructions)
			{
				AddAddressIfNotTheSameAsLast(addresses, instruction.Address);
			}

			return addresses;
		}

		void AddAddressIfNotTheSameAsLast(AddressWrapperCollection addresses, JobDocAddress docAddress)
		{
			if (docAddress != null)
			{
				var lastAddress = addresses.Cast<AddressWrapper>().LastOrDefault();
				if (lastAddress == null || !docAddress.IsTheSameAddressAs((IDocAddress)lastAddress.WrappedObject))
				{
					addresses.Add(new AddressWrapper(docAddress, Factory));
				}
			}
		}

		#endregion
		#region IDocTypeCode Memebers

		ZString IDocTypeCode.DocTypeCode { get; set; }

		#endregion

		#region Transport

		protected T Transport
		{
			get { return (T)WrappedBO; }
		}

		#endregion

		#region ParentWrapper

		protected FreightWrapper ParentWrapper
		{
			get
			{
				if (parentWrapper == null)
				{
					parentWrapper = GetParentWrapperCore();
				}

				return parentWrapper;
			}
		}

		FreightWrapper parentWrapper;

		protected abstract FreightWrapper GetParentWrapperCore();

		#endregion
	}
}
