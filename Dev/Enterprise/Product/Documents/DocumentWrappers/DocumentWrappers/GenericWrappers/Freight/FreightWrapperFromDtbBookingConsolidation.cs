using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromDtbBookingConsolidation : FreightWrapper, IPackingParentWrapper, IDocTypeCode
	{
		public FreightWrapperFromDtbBookingConsolidation(BusinessObject dtbBookingConsolidationBO, BusinessObjectFactory factory)
			: base(dtbBookingConsolidationBO ?? factory.GetNull<DtbBookingConsolidation>(), factory)
		{
		}

		#region Properties

		#region GetJobNumberHeading

		protected override ZString GetJobNumberHeading()
		{
			return Res.GetString("58715453-abdd-4421-a6fd-d228e3eb6e95", "Consolidation ID");
		}

		#endregion

		#region GetJobNumber

		protected override ZString GetJobNumber()
		{
			return ConsolidationBO.KB_JobID;
		}

		#endregion

		#region GetShipmentType

		protected override CodeAndDescriptionWrapper GetShipmentType()
		{
			var parent = ParentJob;
			return parent != null ? parent.ShipmentType : null;
		}

		#endregion

		#region GetConsolDateCreated

		protected override ZDateTime GetConsolDateCreated()
		{
			var earliestLog = ConsolidationBO.Logs.EarliestLogByEventTime(Events.AddedARecordToTheSystem, log => !log.SL_IsCancelled);

			return earliestLog != null ? earliestLog.SL_EventTime : ZDateTime.Empty;
		}

		#endregion

		#endregion

		#region Wrappers

		protected override FreightWrapper GetParentJob()
		{
			var frieghtWrappers = ConsolidationBO.Parent != null && ConsolidationBO.Parent.ParentWithWorkflow != null ? FreightWrapper.New(ConsolidationBO.Parent.ParentWithWorkflow, Factory) : Array.Empty<FreightWrapper>();
			return frieghtWrappers.Any() ? frieghtWrappers[0] : base.GetParentJob();
		}

		protected override OrganisationWrapper GetCarrier()
		{
			return FirstTransportBooking != null ? new OrganisationWrapper(OrganisationUsageType.TransportCompany, FirstTransportBooking.Address, ConsolidationBO.Factory) : null;
		}

		#endregion

		#region Collections

		#region GetTransportBookings

		protected override FreightWrapperCollection GetTransportBookings()
		{
			var bookings = new FreightWrapperCollection(Factory);

			foreach (DtbBooking booking in ConsolidationBO.Bookings)
			{
				var tempBookingWrapper = new FreightWrapperFromDtbBooking(booking, Factory);

				foreach (BookingInstructionWrapper bookingInstructionWrapper in tempBookingWrapper.BookingInstructions) // BookingInstructions are already flattened out with Confirmations/Pivots
				{
					var signleBookingWrapper = new FreightWrapperFromDtbBooking(booking, Factory);
					signleBookingWrapper.BookingInstructions.RemoveAll();
					signleBookingWrapper.BookingInstructions.Add(bookingInstructionWrapper);
					bookings.Add(signleBookingWrapper);
				}
			}

			return bookings;
		}

		#endregion

		#region GetContainers

		protected override ContainerWrapperCollection GetContainers()
		{
			return new ContainerWrapperCollection(ConsolidationBO, Factory);
		}

		#endregion

		#region GetPackages

		protected override PackageWrapperCollection GetPackages()
		{
			return new PackageWrapperCollection(ConsolidationBO, Factory);
		}

		#endregion

		#region GetCustomerReference

		protected override ZString GetCustomerReference()
		{
			return ParentJob.CustomerReference;
		}

		#endregion

		#region GetUNDGs

		protected override UNDGSubstanceWrapperCollection GetUNDGs()
		{
			return new UNDGSubstanceWrapperCollection(ConsolidationBO, Factory);
		}

		#endregion

		protected override ZString GetShippersReference()
		{
			return ParentJob.ShippersReference;
		}

		protected override CustomsEntryWrapperCollection GetCustomsEntries()
		{
			return new CustomsEntryWrapperCollection(ConsolidationBO, Factory);
		}

		#endregion

		#region Implementation

		DtbBookingConsolidation ConsolidationBO
		{
			get { return (DtbBookingConsolidation)WrappedBO; }
		}

		DtbBooking FirstTransportBooking
		{
			get { return ConsolidationBO.Bookings.Count > 0 ? ConsolidationBO.Bookings[0] : null; }
		}

		#endregion

		#region IPackingParentWrapper

		AddressWrapper IPackingParentWrapper.GetPickupAddress(PkgPackage package)
		{
			var instruction = GetTopLevelInstruction(package, LocalCartageJobOrgTypeList.Codes.CNR, InstructionTypes.Codes.PickUp);
			return instruction != null ? new AddressWrapper(instruction.Address, Factory) : null;
		}

		AddressWrapper IPackingParentWrapper.GetDeliveryAddress(PkgPackage package)
		{
			var instruction = GetTopLevelInstruction(package, LocalCartageJobOrgTypeList.Codes.CNE, InstructionTypes.Codes.Delivery);
			return instruction != null ? new AddressWrapper(instruction.Address, Factory) : null;
		}

		ZString IPackingParentWrapper.GetOwnerReference(PkgPackage package)
		{
			return GetSingleReferenceFromConfirmations(package, LocalCartageJobOrgTypeList.Codes.CNR, InstructionTypes.Codes.PickUp, c => c.IsPickUp);
		}

		ZString IPackingParentWrapper.GetCustomerReference(PkgPackage package)
		{
			return GetSingleReferenceFromConfirmations(package, LocalCartageJobOrgTypeList.Codes.CNE, InstructionTypes.Codes.Delivery, c => c.IsDelivery);
		}

		ZString GetSingleReferenceFromConfirmations(PkgPackage package, string organisationType, string instructionType, Predicate<DtbBookingConfirmation> isPickUpOrDelivery)
		{
			var result = ZString.Empty;

			var instruction = GetTopLevelInstruction(package, organisationType, instructionType);
			if (instruction != null)
			{
				var confirmations = instruction.Confirmations.Where(c => !c.KK_ReferenceNum.IsEmpty && isPickUpOrDelivery(c)).ToArray();
				var firstConfirmation = confirmations.FirstOrDefault();
				if (firstConfirmation != null && confirmations.All(c => c.KK_ReferenceNum.IsEmpty || c.KK_ReferenceNum == firstConfirmation.KK_ReferenceNum))
				{
					result = firstConfirmation.KK_ReferenceNum;
				}
			}

			return result;
		}

		ZDateTime IPackingParentWrapper.GetDeliveryRequiredBy(PkgPackage package)
		{
			var result = ZDateTime.Empty;

			var instruction = GetTopLevelInstruction(package, LocalCartageJobOrgTypeList.Codes.CNE, InstructionTypes.Codes.Delivery);
			if (instruction != null)
			{
				var confirmations = instruction.Confirmations.Where(c => !c.KK_RequiredTo.IsEmpty && c.IsDelivery).ToArray();
				var divotConfirmations = confirmations.Where(c => c.PackageDivot != null).ToArray();
				var firstDivotConfirmation = divotConfirmations.FirstOrDefault();
				var firstInstructionConfirmation = confirmations.FirstOrDefault();

				if (firstDivotConfirmation != null && divotConfirmations.All(c => c.KK_RequiredTo.Date == firstDivotConfirmation.KK_RequiredTo.Date))
				{
					result = firstDivotConfirmation.KK_RequiredTo.Date;
				}
				// if there are no divot confirmations then fall back to instruction confirmations as the confirmation collection only contains instruction confirmations
				else if (firstDivotConfirmation == null && firstInstructionConfirmation != null && confirmations.All(c => c.KK_RequiredTo.Date == firstInstructionConfirmation.KK_RequiredTo.Date))
				{
					result = firstInstructionConfirmation.KK_RequiredTo.Date;
				}
			}
			return result;
		}

		ZString IPackingParentWrapper.GetTransportReference(PkgPackage package)
		{
			var instruction = GetTopLevelInstruction(package, LocalCartageJobOrgTypeList.Codes.CNE, InstructionTypes.Codes.Delivery);
			return instruction != null ? instruction.Booking.KM_TransportReference : ZString.Empty;
		}

		OrganisationWrapper IPackingParentWrapper.GetTransportCompany(PkgPackage package)
		{
			var instruction = GetTopLevelInstruction(package, LocalCartageJobOrgTypeList.Codes.CNE, InstructionTypes.Codes.Delivery);
			return instruction != null ? new OrganisationWrapper(OrganisationUsageType.TransportCompany, instruction.Booking.Address, Factory) : null;
		}

		CarrierServiceLevelWrapper IPackingParentWrapper.GetCarrierServiceLevel(PkgPackage package)
		{
			var instruction = GetTopLevelInstruction(package, LocalCartageJobOrgTypeList.Codes.CNE, InstructionTypes.Codes.Delivery);
			return instruction != null ? new CarrierServiceLevelWrapper(instruction.Booking.KM_PL_NKCarrierServiceLevel, instruction.Booking.Lookups.CarrierServiceLevels, Factory) : null;
		}
		#endregion

		#region IPackingParentWrapper helper methods

		DtbBookingInstruction GetTopLevelInstruction(PkgPackage package, ZString instType, ZString jobType)
		{
			var instruction = GetOneAndOnly(GetDeliveryDivots(package, instType, jobType).Select(e => e.Instruction).Distinct());
			if (instruction == null && package != null)
			{
				if (InstructionDictionary.ContainsKey(package))
				{
					instruction = InstructionDictionary[package];
				}
				else if (package.ParentPackage != null)
				{
					instruction = GetTopLevelInstruction(package.ParentPackage, instType, jobType);
					InstructionDictionary.Add(package, instruction);
				}
			}

			return instruction;
		}

		Dictionary<PkgPackage, DtbBookingInstruction> InstructionDictionary
		{
			get { return instructionDictionary ?? (instructionDictionary = new Dictionary<PkgPackage, DtbBookingInstruction>()); }
		}

		Dictionary<PkgPackage, DtbBookingInstruction> instructionDictionary;

		IEnumerable<DtbBookingInstructionPkgDivot> GetDeliveryDivots(PkgPackage package, ZString orgType, ZString instType)
		{
			if (package != null)
			{
				var divotsToPackage = Factory.Load<DtbBookingInstructionPkgDivot>(new ZQuery(DtbBookingInstructionPkgDivotSchema.KD_KP_Package, package.PK));
				return divotsToPackage.Where(
					e =>
					e.Instruction != null
					&& e.Instruction.KN_InstructionType == instType
					&& e.Instruction.OrganisationType == orgType);
			}

			return Array.Empty<DtbBookingInstructionPkgDivot>();
		}

		/// <summary>
		/// Only return the one element of the collection. If none or more than one, return null.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collection"></param>
		/// <returns></returns>
		T GetOneAndOnly<T>(IEnumerable<T> collection) where T : BusinessObject
		{
			T result = null;

			if (collection.Count() == 1)
			{
				result = collection.First();
			}

			return result;
		}

		#endregion

		#region IDocTypeCode Members

		ZString IDocTypeCode.DocTypeCode { get; set; }

		#endregion
	}
}
