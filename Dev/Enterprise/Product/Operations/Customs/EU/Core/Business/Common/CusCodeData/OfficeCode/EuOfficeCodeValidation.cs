using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business
{
	public class EuOfficeCodeValidation : CusCodeDataValidation
	{
		public EuOfficeCodeValidation(EuOfficeCode parent) : base(parent)
		{
		}

		#region Check CY_Code

		protected override void CheckCY_Code()
		{
			var sourceValue = Parent.CY_Code;

			CheckIfOfficeTypeEmptyOrInvalid();

			if (!sourceValue.IsEmpty)
			{
				if (Parent.Requirement is CustomsOfficeRequirement requirement)
				{
					var maxOfficeCountLimit = requirement.MaxOfficeCountLimit;
					CheckTooManyOfficesOfThisTypeExist(maxOfficeCountLimit);
				}

				if (sourceValue == EuOfficeCodesTypes.Codes.OfficeOfPresentation)
				{
					CheckRuleR0676();
				}
			}
		}

		public void CheckRuleR0676()
		{
			if (Parent.Parent is JobDeclaration declaration
				&& (ValidationDecider?.IsRuleR0676Active ?? false)
				&& !declaration.CustomsEntryInstructions
					.All(x => x.CusAuthorizationUsages
						.Any(authorisation => authorisation.AGC_Code == CusAuthorizationHeaderTypeList.Codes.CentralizedClearance)))
			{
				Parent.CY_CodeInfo.AddMessageError(Res.GetString("2F828300-D393-441D-9A55-29108F77FB21"
					, "[R0676] Presentation Customs Office exist but Authorization with type code CCL(C513) is missing."));
			}
		}

		protected virtual void CheckIfOfficeTypeEmptyOrInvalid()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CY_CodeInfo);
		}

		void CheckTooManyOfficesOfThisTypeExist(ZInt? maxSupported)
		{
			if (maxSupported.HasValue && IsTypeRepeatedMoreThanMax(maxSupported.Value))
			{
				var message = GetTooManyOfficesMessage(maxSupported.Value);
				Parent.CY_CodeInfo.AddNotification(OfficeTypeRepeatedMoreThanMaxNotificationType, message);
			}
		}

		protected virtual string GetTooManyOfficesMessage(int maxSupported) => maxSupported == 1
			? Res.GetString("A24C81AC-FF40-4E08-BD3D-71A31DED6FD0", "Only one office of type {0} is allowed", Parent.CY_Code)
			: Res.GetString("523DEAA4-E01F-4C8E-ACCF-9CBCF19EAFDD", "Only a maximum of {0} Customs Offices with Purpose '{1}' may be specified.", maxSupported, Parent.CY_Code);

		protected virtual bool IsTypeRepeatedMoreThanMax(int max) => OfficeCodeProvider.CustomsOffices.Count(office => office.CY_Code == Parent.CY_Code) > max;

		protected virtual CargoWise.ComponentModel.INotificationType OfficeTypeRepeatedMoreThanMaxNotificationType => NotificationType.MessageError;

		#endregion

		#region Check CY_Data

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();

			var factory = Parent.Factory;
			var sourceValue = Parent.CY_Data;
			var targetInfo = Parent.CY_DataInfo;

			if (sourceValue.IsEmpty)
			{
				targetInfo.AddNotification(NotificationTypeForEmptyCY_Data, NotificationMessageForEmptyCY_Data);
			}
			else if (!Regex.IsMatch(sourceValue, "^[A-Z][A-Z].{6}$")) // GB000001 or IEDUB001
			{
				targetInfo.AddMessageError(Res.GetString("84C03ECA-1596-4FE2-AC8A-956EE1E9F999", "EU customs office codes should start with the country/region prefix and then contain a further 6 characters"));
			}
			else
			{
				string countryCode = sourceValue.Left(2);
				var country = factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, countryCode));

				if (IsCountryCodeXI(countryCode) || (country != null && IsCountryOk(targetInfo, country)))
				{
					var office = Parent.Office;
					var roles = Parent.CY_RoleCodes;
					if (office == null)
					{
						targetInfo.AddMessageError(ErrorMessageForInvalidCY_Data);
					}
					else if (roles != null && roles.Any())
					{
						if (HasOfficeAnyRolOnAttribute(roles, office))
						{
							targetInfo.AddMessageError(Res.GetString("A2FF13ED-C833-492B-B282-FE995CF407C8", "According to reference data, this office does not fulfill this role"));
						}
					}
				}
				else if (country == null)
				{
					targetInfo.AddWarning(Res.GetString("75A2547F-EED0-4B70-9DC7-744672EA5023", "{0} is not listed as a country. Check the value of your office code or check that your list of economic groupings is up to date.", countryCode));
				}
			}
		}

		protected virtual bool HasOfficeAnyRolOnAttribute(IEnumerable<ZString> roles, ZZRefCusCodeListCombined office) => roles.All(x => !office.HasAttribute(RefCusCodeListAttributeTypes.Codes.ROLE, x));

		protected virtual CargoWise.ComponentModel.INotificationType NotificationTypeForEmptyCY_Data => NotificationType.MessageError;

		protected virtual ZString NotificationMessageForEmptyCY_Data => Res.GetString("275AFF93-04FE-4797-B862-DCAF418CBE3A", "An office code is needed. Example: FR000010.");

		protected virtual ZString ErrorMessageForInvalidCY_Data => Res.GetString("2C2C5825-B9FD-441F-96CC-5EE3BD4EE224", "Entered office code is not a valid office");

		bool IsCountryCodeXI(string countryCode) => countryCode == Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes;

		bool IsCountryOk(ZPropertyInfo targetInfo, RefCountry country)
		{
			bool isCountryOk = true;
			foreach (var countryValidationRule in CountryValidationRules)
			{
				var validationRule = countryValidationRule.Value;
				if (validationRule.IsApplied)
				{
					var validationResult = validationRule.Validate(country);
					if (!validationResult.IsValid)
					{
						isCountryOk = false;
						targetInfo.AddNotification(validationRule.NotificationSeverity, validationResult.Message);
					}
				}
			}
			return isCountryOk;
		}

		Dictionary<string, ValidationRule> CountryValidationRules => Parent.Factory.GetValue(ref countryValidationRules, GetCountryValidationRulesCore);
		CachedProperty<Dictionary<string, ValidationRule>> countryValidationRules;

		protected virtual Dictionary<string, ValidationRule> GetCountryValidationRulesCore()
		{
			return new Dictionary<string, ValidationRule>
			{
				{ nameof(MustBeMemberOfEU), new MustBeMemberOfEU() }
			};
		}

		#endregion

		protected new EuOfficeCode Parent => (EuOfficeCode)base.Parent;
		protected IEuOfficeCodeProvider OfficeCodeProvider => Parent.Parent as IEuOfficeCodeProvider;

		internal ICustomsOfficeValidationDecider ValidationDecider => Parent.Factory.GetValue(ref validationDeciderCached, GetValidationDecider);
		CachedProperty<ICustomsOfficeValidationDecider> validationDeciderCached;

		ICustomsOfficeValidationDecider GetValidationDecider() => Parent.Parent is JobDeclaration declaration ?
			declaration.Configuration.GetCustomsOfficeValidationDecider(declaration)
			: null;
	}
}
