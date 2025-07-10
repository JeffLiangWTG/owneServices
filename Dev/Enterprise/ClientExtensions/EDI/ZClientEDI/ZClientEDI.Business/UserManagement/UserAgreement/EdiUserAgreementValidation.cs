//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiUserAgreementValidation
//
//    This class should be used for overriding validation in AutoEdiUserAgreementValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Business.Res;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public class EdiUserAgreementValidation : AutoEdiUserAgreementValidation
	{
		public EdiUserAgreementValidation(AutoEdiUserAgreement parent) : base(parent)
		{
		}

		protected new EdiUserAgreement Parent => (EdiUserAgreement)base.Parent;

		protected override void CheckERA_VersionNumber()
		{
			base.CheckERA_VersionNumber();

			var duplicateQuery = new ZQuery(EdiUserAgreementSchema.ERA_Type, Parent.ERA_Type);
			duplicateQuery.AddToFilter(EdiUserAgreementSchema.ERA_RN_NKCountryCode, Parent.ERA_RN_NKCountryCode);
			duplicateQuery.AddToFilter(EdiUserAgreementSchema.ERA_VersionNumber, Parent.ERA_VersionNumber);
			duplicateQuery.AddToFilter(EdiUserAgreementSchema.ERA_MinorVersion, Parent.ERA_MinorVersion);
			duplicateQuery.AddToFilter(EdiUserAgreementSchema.ERA_VariantCode, Parent.ERA_VariantCode);
			duplicateQuery.AddToFilter(EdiUserAgreementSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

			if (Parent.Factory.Exists(typeof(EdiUserAgreement), duplicateQuery))
			{
				Parent.ERA_VersionNumberInfo.AddError(Res.GetString("4ba7ade7-c5ac-4b83-8b50-1cd36f100aa6", "An existing Agreement of this Type, Variant and Country/Region Code already exists with this Version Number. Please try recreating this Agreement to regenerate a Version Number."));
			}
		}

		protected override void CheckERA_EffectiveTimeUtc()
		{
			base.CheckERA_EffectiveTimeUtc();

			if (Parent.ERA_EffectiveTimeUtc.IsValid)
			{
				if ((!Parent.IsInDatabase || Parent.ERA_EffectiveTimeUtcInfo.HasChanges) && Parent.ERA_EffectiveTimeUtc < ZDateTime.UtcNow.AddMinutes(-5))
				{
					Parent.ERA_EffectiveTimeUtcInfo.AddError(Res.GetString("e87db3e4-a203-41ac-9d5e-df976051f01c", "Effective Time must be in the future."));
				}

				var existingCurrentQuery = new ZQuery(EdiUserAgreementSchema.ERA_Type, Parent.ERA_Type);
				existingCurrentQuery.AddToFilter(EdiUserAgreementSchema.ERA_RN_NKCountryCode, Parent.ERA_RN_NKCountryCode);
				existingCurrentQuery.AddToFilter(EdiUserAgreementSchema.ERA_VariantCode, Parent.ERA_VariantCode);

				if (Parent.ERA_EffectiveTimeUtc == ZDateTime.MaxSmallDateTimeUtc)
				{
					existingCurrentQuery.AddToFilter(EdiUserAgreementSchema.ERA_EffectiveTimeUtc, SQLComparisonOperator.Equal, Parent.ERA_EffectiveTimeUtc);
				}
				else
				{
					existingCurrentQuery.AddToFilter(EdiUserAgreementSchema.ERA_EffectiveTimeUtc, SQLComparisonOperator.GreaterThan, Parent.ERA_EffectiveTimeUtc.AddMinutes(-1));
					existingCurrentQuery.AddToFilter(EdiUserAgreementSchema.ERA_EffectiveTimeUtc, SQLComparisonOperator.LessThan, Parent.ERA_EffectiveTimeUtc.AddMinutes(1));
				}

				existingCurrentQuery.AddToFilter(EdiUserAgreementSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				if (Parent.Factory.Exists(typeof(EdiUserAgreement), existingCurrentQuery))
				{
					Parent.ERA_EffectiveTimeUtcInfo.AddError(Res.GetString("8e6a2f00-8b1e-471c-bd19-f7ea31c6a8d0", "There is another Agreement with this Type, Variant and Country/Region Code which has the same Effective Time. Only one Agreement can be current for each Type, Variant and Country/Region Code combination."));
				}
			}
		}

		protected override void CheckERA_VersionNumberIsNotEmpty()
		{
		}

		protected override void CheckERA_Title()
		{
			base.CheckERA_Title();

			MandatoryValidation.CheckEntered(Parent.ERA_TitleInfo);
		}

		protected override void CheckERA_Type()
		{
			base.CheckERA_Type();

			MandatoryValidation.CheckEntered(Parent.ERA_TypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ERA_TypeInfo);
		}

		protected override void CheckERA_VariantCode()
		{
			base.CheckERA_VariantCode();
			if (EdiUserAgreementTypesMapper.IsVariantMandatory(Parent.ERA_Type))
			{
				MandatoryValidation.CheckEntered(Parent.ERA_VariantCodeInfo);
			}
		}

		protected override void CheckERA_VariantDescription()
		{
			if (!Parent.ERA_VariantCode.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.ERA_VariantDescriptionInfo);
			}
			base.CheckERA_VariantDescription();
		}

		protected override void CheckERA_RN_NKCountryCode()
		{
			base.CheckERA_RN_NKCountryCode();
			if(!Parent.ERA_RN_NKCountryCode.IsEmpty && EdiUserAgreementTypesMapper.IsVariantEnabled(Parent.ERA_Type))
			{
				Parent.ERA_RN_NKCountryCodeInfo.AddError(ResString.GetMultilingualString("efe0a357-e20f-4d3a-9ca9-3ca558cd8261", "The country/region should not be set. Please try setting a variant code for the agreement."));
			}
		}
	}
}
