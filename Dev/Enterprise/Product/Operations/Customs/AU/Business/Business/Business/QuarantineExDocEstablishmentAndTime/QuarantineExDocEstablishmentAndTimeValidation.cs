//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoQuarantineExDocEstablishmentAndTimeValidation
//
//    This class should be used for overriding validation in AutoQuarantineExDocEstablishmentAndTimeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineExDocEstablishmentAndTimeValidation : AutoQuarantineExDocEstablishmentAndTimeValidation
	{
		public QuarantineExDocEstablishmentAndTimeValidation(AutoQuarantineExDocEstablishmentAndTime parent)
			: base(parent)
		{
		}

		protected bool IsValidationRequired => Parent.JobDeclaration?.IsQuarantine ?? false;

		protected override void CheckEE_TreatmentDuration()
		{
			base.CheckEE_TreatmentDuration();
			MessageErrorIfNoHorOrGrn(Parent.EE_TreatmentDurationInfo);
			MandatoryValidation.CheckNotNegative(Parent.EE_TreatmentDurationInfo);
			MandatoryValidation.AddErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.EE_TreatmentDurationInfo, Parent.EE_TreatmentDurationUQInfo);
		}

		protected override void CheckEE_TreatmentDurationUQ()
		{
			base.CheckEE_TreatmentDurationUQ();
			MessageErrorIfNoHorOrGrn(Parent.EE_TreatmentDurationUQInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.EE_TreatmentDurationUQInfo);
			MandatoryValidation.AddErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.EE_TreatmentDurationUQInfo, Parent.EE_TreatmentDurationInfo);
		}

		protected override void CheckEE_TreatmentConcentration()
		{
			base.CheckEE_TreatmentConcentration();
			MessageErrorIfNoHorOrGrn(Parent.EE_TreatmentConcentrationInfo);
			MandatoryValidation.CheckNotNegative(Parent.EE_TreatmentConcentrationInfo);
			MandatoryValidation.AddErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.EE_TreatmentConcentrationInfo, Parent.EE_TreatmentConcentrationUQInfo);
		}

		protected override void CheckEE_TreatmentConcentrationUQ()
		{
			base.CheckEE_TreatmentConcentrationUQ();
			MessageErrorIfNoHorOrGrn(Parent.EE_TreatmentConcentrationUQInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.EE_TreatmentConcentrationUQInfo);
			MandatoryValidation.AddErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.EE_TreatmentConcentrationUQInfo, Parent.EE_TreatmentConcentrationInfo);
		}

		protected override void CheckEE_TreatmentTemperature()
		{
			base.CheckEE_TreatmentTemperature();
			MessageErrorIfNoHorOrGrn(Parent.EE_TreatmentTemperatureInfo);
		}

		protected override void CheckEE_TreatmentTemperatureUQ()
		{
			base.CheckEE_TreatmentTemperatureUQ();
			MessageErrorIfNoHorOrGrn(Parent.EE_TreatmentTemperatureUQInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.EE_TreatmentTemperatureUQInfo);
			MandatoryValidation.AddErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.EE_TreatmentTemperatureUQInfo, Parent.EE_TreatmentTemperatureInfo);
		}

		protected override void CheckEE_AuthorisationEstablishmentID()
		{
			base.CheckEE_AuthorisationEstablishmentID();
			if (IsValidationRequired)
			{
				switch (Parent.EE_ProcessingType)
				{
					case EXDOCProcessTypeCodes.Codes.Harvest:
					case EXDOCProcessTypeCodes.Codes.Treatment:
						// EE_AuthorisationEstablishmentID may be omitted
						break;
					case EXDOCProcessTypeCodes.Codes.AquacultureFarm:
					case EXDOCProcessTypeCodes.Codes.CatcherVessel:
						if (!Parent.EE_AuthorisationEstablishmentID.IsEmpty)
						{
							Parent.EE_AuthorisationEstablishmentIDInfo.AddMessageError("Process establishment ID must not be entered");
						}
						break;
					default:
						if (Parent.EE_AuthorisationEstablishmentID.IsEmpty && IsAddressEmpty)
						{
							Parent.EE_AuthorisationEstablishmentIDInfo.AddMessageError("Process establishment address or ID must be entered");
						}
						break;
				}
			}
		}

		protected override void CheckEE_E2_Address()
		{
			if (IsValidationRequired)
			{
				switch (Parent.EE_ProcessingType)
				{
					case EXDOCProcessTypeCodes.Codes.Harvest:
					case EXDOCProcessTypeCodes.Codes.Treatment:
						// EE_E2_Address may be omitted
						break;
					case EXDOCProcessTypeCodes.Codes.AquacultureFarm:
					case EXDOCProcessTypeCodes.Codes.CatcherVessel:
						if (IsAddressEmpty)
						{
							Parent.EE_E2_AddressInfo.AddMessageError("Process establishment address must be entered");
						}
						break;
					default:
						if (IsAddressEmpty && Parent.EE_AuthorisationEstablishmentID.IsEmpty)
						{
							Parent.EE_E2_AddressInfo.AddMessageError("Process establishment address or ID must be entered");
						}
						break;
				}
			}
		}

		protected override void CheckEE_EndDate()
		{
			base.CheckEE_EndDate();
			if (IsValidationRequired)
			{
				if (!Parent.EE_StartDate.IsEmpty &&
				Parent.EE_EndDate.IsEmpty &&
				Parent.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.Harvest)
				{
					if (!Parent.IsNEXDOCSActive || Parent.EE_HarvestArea.IsEmpty)
					{
						Parent.EE_EndDateInfo.AddMessageError("Process end date must be entered.");
					}
				}
				if (Parent.EE_EndDate < Parent.EE_StartDate)
				{
					Parent.EE_EndDateInfo.AddMessageError("Process end date must be greater than or equal to process start date.");
				}
				if (Parent.QuarantineExDocHeader != null &&
					Parent.EE_EndDate > Parent.QuarantineExDocHeader.QH_InspectionRequestedDate &&
					(Parent.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.Packing ||
					Parent.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.Processing))
				{
					Parent.EE_EndDateInfo.AddMessageError("Process end date must be less then or equal to inspection requested date.");
				}
				if (Parent.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.Packing && Parent.EE_EndDate > ZDateTime.Now)
				{
					Parent.EE_EndDateInfo.AddMessageError("Process end date must be less then or equal to todays date.");
				}
			}
		}

		protected override void CheckEE_StartDate()
		{
			base.CheckEE_StartDate();
			if (IsValidationRequired)
			{
				if (Parent.EE_StartDate.IsEmpty)
				{
					var isProcessCTorAQ =
						Parent.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.CatcherVessel ||
						Parent.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.AquacultureFarm;
					if (!isProcessCTorAQ || IsAddressEmpty)
					{
						Parent.EE_StartDateInfo.AddMessageError("Process start date must be entered.");
					}
					if (!Parent.EE_EndDate.IsEmpty)
					{
						Parent.EE_StartDateInfo.AddMessageError("Process start date is required when process end date is entered.");
					}
				}
				else
				{
					if (Parent.EE_StartDate > Parent.EE_EndDate)
					{
						Parent.EE_StartDateInfo.AddMessageError("Process start date must be less than or equal to process end date.");
					}
					if (Parent.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.Treatment && Parent.EE_StartDate > ZDateTime.Now)
					{
						Parent.EE_StartDateInfo.AddMessageError("Process start date cannot be greater than today.");
					}
				}
			}
		}

		protected override void CheckEE_Depuration()
		{
			base.CheckEE_Depuration();
			if (IsValidationRequired)
			{
				if (Parent.EE_ProcessingType != EXDOCProcessTypeCodes.Codes.Harvest && !Parent.EE_Depuration.IsEmpty)
				{
					Parent.EE_DepurationInfo.AddMessageError("Depuration date can only be entered on a harvest process.");
				}
				if (Parent.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.Harvest &&
					!Parent.EE_AuthorisationEstablishmentID.IsEmpty &&
					Parent.EE_Depuration.IsEmpty)
				{
					Parent.EE_DepurationInfo.AddMessageError("Depuration date required when depuration plant number entered.");
				}
			}
		}

		protected override void CheckEE_HarvestArea()
		{
			base.CheckEE_HarvestArea();
			if (IsValidationRequired)
			{
				if (Parent.EE_ProcessingType != EXDOCProcessTypeCodes.Codes.Harvest && !Parent.EE_HarvestArea.IsEmpty)
				{
					Parent.EE_HarvestAreaInfo.AddMessageError("Harvest area can only be entered on a harvest process.");
				}
				if (!Parent.IsNEXDOCSActive && Parent.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.Harvest && Parent.EE_HarvestArea.IsEmpty)
				{
					Parent.EE_HarvestAreaInfo.AddMessageError("Harvest area must be entered.");
				}
			}
		}

		protected override void CheckEE_LeaseNumber()
		{
			base.CheckEE_LeaseNumber();
			if (IsValidationRequired)
			{
				if (Parent.EE_ProcessingType != EXDOCProcessTypeCodes.Codes.Harvest && !Parent.EE_LeaseNumber.IsEmpty)
				{
					Parent.EE_LeaseNumberInfo.AddMessageError("Lease number can only be entered on a harvest process.");
				}
			}
		}

		protected override void CheckEE_TreatmentCode()
		{
			base.CheckEE_TreatmentCode();
			if (IsValidationRequired)
			{
				if (Parent.EE_ProcessingType != EXDOCProcessTypeCodes.Codes.Treatment && !Parent.EE_TreatmentCode.IsEmpty)
				{
					Parent.EE_TreatmentCodeInfo.AddMessageError("Treatment code can only be entered on a treatment process.");
				}
				if (Parent.EE_TreatmentCode.IsEmpty && !Parent.EE_TreatmentInfo.IsEmpty)
				{
					Parent.EE_TreatmentCodeInfo.AddMessageError("Treatment code can not be empty when treatment information is entered.");
				}
				ValidateEE_TreatmentInfo();
			}
		}

		protected override void CheckEE_TreatmentInfo()
		{
			base.CheckEE_TreatmentInfo();
			if (IsValidationRequired)
			{
				if (Parent.EE_ProcessingType != EXDOCProcessTypeCodes.Codes.Treatment && !Parent.EE_TreatmentInfo.IsEmpty)
				{
					Parent.EE_TreatmentInfoInfo.AddMessageError("Treatment information can only be entered on a treatment process.");
				}
				if (Parent.EE_TreatmentInfo.IsEmpty && !Parent.EE_TreatmentCode.IsEmpty)
				{
					Parent.EE_TreatmentInfoInfo.AddMessageError("Treatment information can not be empty when treatment code is entered.");
				}
				ValidateEE_TreatmentCode();
			}
		}

		protected override void CheckEE_ProcessingType()
		{
			base.CheckEE_ProcessingType();
			if (IsValidationRequired)
			{
				if (Parent.QuarantineExDocHeader != null)
				{
					AddHarvestError();
					AddCatcherVesselError();
					AddFreezingError();
					AddPackingError();
					AddSlaughterError();
					AddAquacultureFarmError();
					AddStorageError();
				}
				Parent.QuarantineExDocLine.Validation.ValidateQL_ProduceType();
			}
		}

		void AddHarvestError()
		{
			if (Parent.ParentCollection.ProcessCounts.Harvest &&
				Parent.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Fish &&
				Parent.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.Harvest)
			{
				Parent.EE_ProcessingTypeInfo.AddMessageError("Harvest process may only be supplied for produce type fish.");
			}
		}

		void AddCatcherVesselError()
		{
			if (Parent.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.CatcherVessel &&
				Parent.ParentCollection.ProcessCounts.CatcherVessel > 0)
			{
				if (Parent.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Fish &&
					Parent.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Meat)
				{
					Parent.EE_ProcessingTypeInfo.AddMessageError("Catcher vessel process may only be supplied for produce type fish and meat.");
				}
				if (Parent.ParentCollection.ProcessCounts.CatcherVessel > 1)
				{
					Parent.EE_ProcessingTypeInfo.AddMessageError("Only one catcher vessel process may be supplied");
				}
			}
		}

		void AddFreezingError()
		{
			if (Parent.ParentCollection.ProcessCounts.Freezing > 0 &&
				Parent.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.Freezing &&
				Parent.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Meat &&
				Parent.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Fish &&
				Parent.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Dairy)
			{
				Parent.EE_ProcessingTypeInfo.AddMessageError("Freezing process may only be supplied for produce type fish, meat and dairy.");
			}
			if (Parent.ParentCollection.ProcessCounts.Freezing > 1)
			{
				Parent.EE_ProcessingTypeInfo.AddMessageError("Only one freezing process may be supplied.");
			}
			if (Parent.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.Freezing &&
				Parent.ParentCollection.ProcessCounts.FreezingStartDate < Parent.ParentCollection.ProcessCounts.PackingStartDate)
			{
				Parent.EE_ProcessingTypeInfo.AddMessageError("Freezing start date must be on or after packing start date.");
			}
		}

		void AddPackingError()
		{
			if (Parent.ParentCollection.ProcessCounts.Packing &&
				Parent.QuarantineExDocHeader.QH_ProduceType == EXDOCCommodityCodes.Codes.GrainsAndPlants &&
				Parent.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.Packing)
			{
				Parent.EE_ProcessingTypeInfo.AddMessageError("Packing process may not be supplied for produce type grains and plants.");
			}
		}

		void AddSlaughterError()
		{
			if (Parent.ParentCollection.ProcessCounts.Slaughter &&
			Parent.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Meat &&
			Parent.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.Slaughter)
			{
				Parent.EE_ProcessingTypeInfo.AddMessageError("Slaughter process may only be supplied for produce type meat.");
			}
			if (Parent.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.Slaughter &&
				Parent.ParentCollection.ProcessCounts.PackingEndDate < Parent.ParentCollection.ProcessCounts.SlaughterEndDate &&
				Parent.EE_EndDate == Parent.ParentCollection.ProcessCounts.SlaughterEndDate)
			{
				Parent.EE_ProcessingTypeInfo.AddMessageError("Slaughter end date must be before or equal to packing end date.");
			}
		}

		void AddAquacultureFarmError()
		{
			if (Parent.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.AquacultureFarm &&
				Parent.ParentCollection.ProcessCounts.AquacultureFarm > 0)
			{
				if (Parent.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Fish)
				{
					Parent.EE_ProcessingTypeInfo.AddMessageError("Aquaculture farm process may only be supplied for produce type fish.");
				}
				if (Parent.ParentCollection.ProcessCounts.AquacultureFarm > 1)
				{
					Parent.EE_ProcessingTypeInfo.AddMessageError("Only one aquaculture farm process may be supplied");
				}
			}
		}

		void AddStorageError()
		{
			if (Parent.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.Storage)
			{
				EXDOCValidationHelper.CheckForProduceTypeIsHorticultureOrGrainsAndPlants(Parent.QuarantineExDocHeader.QH_ProduceType, Parent.EE_ProcessingTypeInfo, "Processing Type of ST");
			}
		}

		void MessageErrorIfNoHorOrGrn(ZPropertyInfo propInfo)
		{
			var produceType = new ZString(Parent.QuarantineExDocLine?.QL_ProduceType);

			if (!propInfo.Value.IsEmpty && produceType != EXDOCCommodityCodes.Codes.Horticulture && produceType != EXDOCCommodityCodes.Codes.GrainsAndPlants)
			{
				propInfo.AddMessageError(Res.GetString("QuarantineExDocEstablishmentAndTimeValidation|MessageErrorIfNoHorOrGrn", "{0} may only be present when Produce Type is Horticulture or Grains and Seeds", propInfo.HumanReadableName));
			}
		}

		protected bool IsAddressEmpty => Parent.Address?.E2_CompanyName.IsEmpty ?? true;

		protected new QuarantineExDocEstablishmentAndTime Parent => (QuarantineExDocEstablishmentAndTime)base.Parent;
	}
}
