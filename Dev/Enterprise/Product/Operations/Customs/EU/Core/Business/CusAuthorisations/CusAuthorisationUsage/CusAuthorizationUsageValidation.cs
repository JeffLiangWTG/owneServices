//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusAuthorizationUsageValidation
//
//    This class should be used for overriding validation in AutoCusAuthorizationUsageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using CusEntryInstruction = Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.EU.Business
{
	public class CusAuthorizationUsageValidation : AutoCusAuthorizationUsageValidation
	{
		public CusAuthorizationUsageValidation(AutoCusAuthorizationUsage parent) : base(parent)
		{
		}

		public new CusAuthorizationUsage Parent => (CusAuthorizationUsage)base.Parent;

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();

			base.ValidateAll();
			ValidateRuleR0010();
			ValidateEffectiveReferenceNumber();
		}

		protected override void CheckAGC_Code()
		{
			base.CheckAGC_Code();

			var sourceValue = Parent.AGC_Code;
			var propertyInfo = Parent.AGC_CodeInfo;
			MandatoryValidation.CheckEntered(propertyInfo);
			CheckAGC_CodeIsInvalid(propertyInfo);
			CheckLegalForIND();

			if (sourceValue == CusAuthorizationHeaderTypeList.Codes.CentralizedClearance)
			{
				CheckRuleR0675(propertyInfo);
			}
		}

		void CheckRuleR0675(ZPropertyInfo propertyInfo)
		{
			var parent = Parent;
			if (parent.Lookups.JobDeclaration is { IsUCC6AndIsExport: true } declaration
				&& parent.Parent is ICusAuthorizationUsageProviderWithValidationDecider { ValidationDecider.IsRuleR0675Active: true }
				&& !declaration.CustomsOffices.ContainsCode(EuOfficeCodesTypes.Codes.OfficeOfPresentation))
			{
				propertyInfo.AddMessageError(Res.GetString("B4FA8FEB-06BA-4827-B820-054A7D1AAD5B",
					"[R0675] Customs office of Presentation is required for centralized clearance (denoted via Authorization Type CCL)."));
			}
		}

		protected override void CheckAGC_Number()
		{
			base.CheckAGC_Number();

			if (!UseEffectiveReferenceNumberValidation)
			{
				var parent = Parent;
				var propertyInfo = parent.AGC_NumberInfo;
				var provider = parent.AuthorisationHeaderProvider;

				if (provider != null && provider.EnableAdHoc)
				{
					CheckAuthorizationOrNumberIsEntered(propertyInfo);
				}
				else
				{
					CheckAGC_NumberMandatory(propertyInfo);
				}

				CheckReferenceNumberAndOwnerCombination(propertyInfo);
			}
		}

		protected override void CheckAGC_OH_Owner()
		{
			base.CheckAGC_OH_Owner();

			if (Parent.AGC_OH_Owner == OrgHeader.UnmatchedOrganisationPK)
			{
				Parent.AGC_OH_OwnerInfo.AddWarning(Res.GetString("1A7FD21E-146C-406E-B6EB-1605143FFE16", "Unmatched Owner has been selected - probably due to missing owner when importing Universal Shipment. Please select a valid Owner"));
			}
		}

		protected override void CheckAGC_CPH_Authorization()
		{
			base.CheckAGC_CPH_Authorization();

			if (!UseEffectiveReferenceNumberValidation)
			{
				var parent = Parent;
				var provider = parent.AuthorisationHeaderProvider;

				if (provider != null && provider.EnableAdHoc)
				{
					CheckAuthorizationOrNumberIsEntered(parent.AGC_CPH_AuthorizationInfo);
				}
			}
		}

		void CheckAuthorizationOrNumberIsEntered(ZPropertyInfo propertyInfo)
		{
			if (Parent.AGC_Number.IsEmpty && Parent.AGC_CPH_Authorization.IsEmpty)
			{
				propertyInfo.AddError(Res.GetString("29E04EC2-C1DE-47DF-AB1F-AC3BD88B20AB", "Either Authorization or Number must have a value."));
			}
		}

		void ValidateRuleR0010()
		{
			var parent = Parent;
			var declaration = parent.Lookups.JobDeclaration;
			if (declaration != null && parent.Parent is ICusAuthorizationUsageProviderWithValidationDecider provider
				&& provider.ValidationDecider is ICusAuthorizationUsageValidationDecider validationDecider
				&& validationDecider.IsRuleR0010Active)
			{
				var ruleR0010MessageError = Res.GetString("682E1792-76F0-4489-8FAA-FFDF676294E1", "[R0010] Value can’t be entered in both Entry Instruction and Invoice lines.");
				var code = parent.AGC_Code;
				var number = parent.EffectiveReferenceNumber;
				var cusEntryInstruction = parent.Instruction;
				if (cusEntryInstruction != null)
				{
					if (cusEntryInstruction.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.CusAuthorizationUsages).Any(x => x.AGC_Code == code && x.EffectiveReferenceNumber == number))
					{
						Parent.AddRowMessageError(ruleR0010MessageError);
					}
				}
				else
				{
					if (parent.InvoiceLine?.EntryInstruction is CusEntryInstruction instruction)
					{
						if (instruction.CusAuthorizationUsages.Any(x => x.AGC_Code == code && x.EffectiveReferenceNumber == number))
						{
							Parent.AddRowMessageError(ruleR0010MessageError);
						}
					}
				}
			}
		}

		protected virtual void CheckAGC_NumberMandatory(ZPropertyInfo agcNumberInfo) => MandatoryValidation.CheckEntered(agcNumberInfo);

		protected virtual void CheckAGC_CodeIsInvalid(ZPropertyInfo agcCodeInfo) => ListValidation.MessageErrorIfInvalidCode(agcCodeInfo);

		void CheckLegalForIND()
		{
			var parent = Parent;
			var instruction = parent.Instruction;

			if (instruction?.JobDeclaration is JobDeclaration declaration && declaration.ShouldCheckLegalByDeclarantType)
			{
				if (declaration.JE_DeclarantType == RepresentationTypeList.Codes._3Indirect)
				{
					if (parent.AGC_Code == CusAuthorizationHeaderTypeList.Codes.EndUse)
					{
						parent.AGC_CodeInfo.AddMessageError(Res.GetString("220FE7D8-772C-4D58-BB1B-D965F757F19E", "It is legally forbidden to use Indirect representation for End-Use procedure, you should consider not using EUS authorization or changing Representation Type on Misc Tab."));
					}
					else if (parent.AGC_Code == CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP && instruction.HasIntoWarehouseProcedure)
					{
						parent.AGC_CodeInfo.AddMessageError(Res.GetString("89B10672-DBF1-49AF-8147-784F1354707A", "It is legally forbidden to use Indirect representation for private customs warehouses, you should consider using a public customs warehouse (authorization codes CW1 and CW2) or changing Representation Type on Misc Tab."));
					}
				}
			}
		}

		public void ValidateEffectiveReferenceNumber()
		{
			ValidateCalculatedProperty(Parent.EffectiveReferenceNumberInfo);
		}

		protected virtual bool UseEffectiveReferenceNumberValidation => Parent.UseEffectiveReferenceNumber;

		protected virtual void CheckEffectiveReferenceNumber()
		{
			var parent = Parent;

			if (UseEffectiveReferenceNumberValidation)
			{
				var propertyInfo = parent.EffectiveReferenceNumberInfo;

				MandatoryValidation.CheckEntered(propertyInfo);

				if (!parent.EffectiveReferenceNumber.IsEmpty)
				{
					if (parent.AuthorisationHeader is CusAuthorisationHeader authorisationHeader
						&& authorisationHeader.CPH_OH_PermitHolder != parent.AGC_OH_Owner)
					{
						AddReferenceNumberAndOwnerCombinationWarning(propertyInfo, parent.AGC_Code, authorisationHeader.CPH_Number);
					}
					else
					{
						CheckReferenceNumberAndOwnerCombination(propertyInfo);
					}
				}
			}
		}

		void CheckReferenceNumberAndOwnerCombination(ZPropertyInfo propertyInfo)
		{
			var parent = Parent;
			var code = parent.AGC_Code;
			var number = parent.AGC_Number;
			var owner = parent.AGC_OH_Owner;
			if (!code.IsEmpty && !number.IsEmpty && !owner.IsEmpty
				&& (!parent.Instruction?.JobDeclaration?.Configuration.InstructionConfiguration.UseEoriForAuthorisationReference ?? true)
				&& !parent.Lookups.NumberList.Any(x => x.CPH_Type == code && x.CPH_Number == number && x.CPH_OH_PermitHolder == owner))
			{
				AddReferenceNumberAndOwnerCombinationWarning(propertyInfo, code, number);
			}
		}

		protected virtual void AddReferenceNumberAndOwnerCombinationWarning(ZPropertyInfo propertyInfo, ZString code, ZString number)
		{
			propertyInfo.AddWarning(Res.GetString("FB8B2C03-5BBB-4E3F-AD8D-105CD66481C3", "Authorization number: {0} doesn't exist for Code: {1}, Owner: {2}", number, code, Parent.Owner?.OH_Code));
		}
	}
}

