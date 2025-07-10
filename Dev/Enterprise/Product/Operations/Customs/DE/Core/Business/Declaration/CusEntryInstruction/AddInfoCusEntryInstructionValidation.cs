using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class AddInfoCusEntryInstructionValidation : EU.Business.Declaration.AddInfoCusEntryInstructionValidation
	{
		public AddInfoCusEntryInstructionValidation(EU.Business.Declaration.AddInfoCusEntryInstruction parent) : base(parent)
		{
		}

		CusEntryInstruction Instruction => (CusEntryInstruction)Parent.Parent;

		protected override void CheckZG_SimplifiedGrantAuthorization()
		{
			base.CheckZG_SimplifiedGrantAuthorization();

			var instruction = Instruction;
			if (instruction != null && instruction.EnabledInwardProcessing)
			{
				var targetInfo = Parent.ZG_SimplifiedGrantAuthorizationInfo;
				var simplifiedGrantAuthorization = instruction.CEI_SimplifiedGrantAuthorization;

				ListValidation.MessageErrorIfInvalidCodeOrEmpty(targetInfo, instruction.Lookups.SimplifiedGrantAuthorizationList);

				if (simplifiedGrantAuthorization == SimplifiedGrantAuthorizationList.Codes.J)
				{
					if (!instruction.InwardProcessingPlaces.Any())
					{
						targetInfo.AddMessageError(Res.GetString("86F610AA-E458-4819-82C7-68D5245CEF0C", "You should enter at least one Inward Processing Place."));
					}

					if (!instruction.CompletionCustomsOffices.Any())
					{
						targetInfo.AddMessageError(Res.GetString("0360769C-2A4D-4DA2-B473-E2D0F7497503", "You should enter at least one Completion Customs Office."));
					}
				}
			}
		}

		protected override void CheckZG_SealsCount()
		{
			// NOOP, In DE we have no Seal Quantity on Screen and in Messaging. 
		}

		protected override void CheckZG_AuthorisationNumber()
		{
			base.CheckZG_AuthorisationNumber();
			var instruction = Instruction;
			if (instruction?.JobDeclaration?.IsImport ?? false)
			{
				if (!instruction.CEI_AuthorisationNumberInfo.ReadOnly)
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ZG_AuthorisationNumberInfo, instruction.Lookups.InwardProcessingAuthorizationNumberList);
				}
			}
		}

		protected override void CheckZG_CompletionDuration()
		{
			base.CheckZG_CompletionDuration();
			var instruction = Instruction;
			if (instruction != null
				&& instruction.EnabledInwardProcessing
				&& instruction.CEI_SimplifiedGrantAuthorization == SimplifiedGrantAuthorizationList.Codes.J)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ZG_CompletionDurationInfo);
				MandatoryValidation.MessageErrorIfIsNegative(Parent.ZG_CompletionDurationInfo);
			}
		}

		protected override void CheckZG_CriteriaType()
		{
			base.CheckZG_CriteriaType();
			var instruction = Instruction;
			if (instruction != null
					&& instruction.EnabledInwardProcessing
					&& instruction.CEI_SimplifiedGrantAuthorization == SimplifiedGrantAuthorizationList.Codes.J)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ZG_CriteriaTypeInfo, instruction.Lookups.CriteriaTypeList);
			}
		}

		protected override void CheckZG_ExitDate()
		{
			base.CheckZG_ExitDate();
			var cusEntryInstruction = Instruction;
			if (cusEntryInstruction != null)
			{
				var declaration = cusEntryInstruction.JobDeclaration;
				if (declaration != null && declaration.IsExport)
				{
					var exitDate = cusEntryInstruction.ZG_ExitDate;
					var targetInfo = Parent.ZG_ExitDateInfo;
					if (exitDate.IsEmpty)
					{
						if (cusEntryInstruction.SubStyle1stDigitIs1())
						{
							MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
						}
					}
					else if (exitDate.IsInTheFutureDatePartOnly)
					{
						targetInfo.AddMessageError(Res.GetString("a59bef7a-1eac-4cfa-8344-bd09f04bff56", "The Exit Date must not be in the future."));
					}
				}
			}
		}

		protected override void CheckZG_LocalClearanceDate()
		{
			base.CheckZG_LocalClearanceDate();

			var instruction = Instruction;
			if (instruction.JobDeclaration?.IsImport ?? false)
			{
				if (!instruction.IsLocalClearanceDateReadonly)
				{
					var parent = Parent;
					var localClearanceDate = parent.ZG_LocalClearanceDate.Date;
					var propertyInfo = parent.ZG_LocalClearanceDateInfo;
					var currentDate = ZDateTime.Now.Date;
					if (localClearanceDate.IsEmpty)
					{
						propertyInfo.AddMessageError(Res.GetString("02D39F91-3203-475D-B2DB-77B2D9F180AF", "Please enter a Local Clearance Date."));
					}
					else if (localClearanceDate > currentDate)
					{
						propertyInfo.AddMessageError(Res.GetString("845F910B-B691-4EF5-91B0-C4E75FB1A6C8", "Local Clearance Date cannot be in the future."));
					}
					else if (localClearanceDate < currentDate.AddMonths(-3) && instruction.CEI_Style.In(new ZString[] { ImportDeclarationTypeList.Codes.AAV, ImportDeclarationTypeList.Codes.AZ }))
					{
						propertyInfo.AddMessageError(Res.GetString("23E51E0F-0FF3-4093-B239-BE1B4B5305CC", "Local Clearance Date must not be more than three months in the past."));
					}
				}
			}
		}

		protected override void CheckZG_EarlyClearanceFlag()
		{
			base.CheckZG_EarlyClearanceFlag();
			var instruction = Instruction;
			if (instruction.IsEarlyClearanceFlagApplicable)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(instruction.CEI_EarlyClearanceFlagInfo);
			}
		}

		protected override void CheckZG_PartyConstellation()
		{
			base.CheckZG_PartyConstellation();

			var instruction = Instruction;
			var jobDeclaration = instruction?.JobDeclaration;
			if (jobDeclaration != null && jobDeclaration.IsExport)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ZG_PartyConstellationInfo);

				instruction.Validation.ValidatePartyConstellationToCheckIfSupplierNeeded();
				jobDeclaration.ExporterDocAddress.Validation.ValidateOrganisationPK();
				jobDeclaration.ContractualPartnerDocAddress.Validation.ValidateOrganisationPK();
				jobDeclaration.Validation.ValidateJE_OA_Representative();
				jobDeclaration.Validation.ValidateJE_OA_SellerAddress();
			}
		}
	}
}
