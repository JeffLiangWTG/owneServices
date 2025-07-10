using System.Globalization;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class LicenceCompanyValidation : AutoLicenceCompanyValidation
	{
		public LicenceCompanyValidation(AutoLicenceCompany parent)
			: base(parent)
		{
			MasterHeader = (EDIOrgHeader)parent.Header;
		}

		protected new LicenceCompany Parent
		{
			get { return (LicenceCompany)base.Parent; }
		}

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			if (info.Name == LicenceCompanySchema.Constants.LC_RX_NKCurrency)
			{
				return false;
			}
			else
			{
				return base.ShouldValidateFKToCancelledRecord(info);
			}
		}

		#region Local Members

		readonly EDIOrgHeader MasterHeader;

		#endregion

		protected override void CheckLC_RX_NKCurrency()
		{
			base.CheckLC_RX_NKCurrency();
			ListValidation.ErrorIfInvalidCode(Parent.LC_RX_NKCurrencyInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "This is meant to specify an exact product name.")]
		protected override void CheckLC_CompanyCountry()
		{
			base.CheckLC_CompanyCountry();

			if (MasterHeader != null && MasterHeader.UNLOCO != null && Parent.LC_CompanyCountry != MasterHeader.UNLOCO.RL_RN_NKCountryCode)
			{
				Parent.LC_CompanyCountryInfo.AddWarning(string.Format(CultureInfo.CurrentCulture, "The selected country ({0}) is different to the country ({1}) this organization locates.",
					Parent.LC_CompanyCountry, MasterHeader.UNLOCO.RL_RN_NKCountryCode));
			}

			foreach (LicenceHeader licence in Parent.LicHeadersForAllDatabases)
			{
				if (licence.Database.LD_Product == ProductTypes.Codes.Enterprise &&
					!Country.IsSupportedForLicenceBuilder(Parent.LC_CompanyCountry))
				{
					Parent.LC_CompanyCountryInfo.AddError(SelectedCountryNotSupported);
					break;
				}
			}
		}

		protected override void CheckLC_CompanyCode()
		{
			if (MasterHeader != null)
			{
				var reason = GlbCompanyValidation.CheckCode(Parent.LC_CompanyCode);
				if (reason != null)
				{
					if (!Parent.IsInDatabase || Parent.LC_CompanyCodeInfo.HasChanges)
					{
						Parent.LC_CompanyCodeInfo.AddError(reason);
					}
					else
					{
						Parent.LC_CompanyCodeInfo.AddWarning(reason);
					}
				}

				// Look for records which have the same enterprise parent, and the same company code, but are not the same company
				ZDBOnlyQuery duplicateCodeCheck = new ZDBOnlyQuery(typeof(LicenceCompany));
				duplicateCodeCheck.AddToFilter(JoinCondition.And, LicenceCompanySchema.LC_CompanyCode, SQLComparisonOperator.Equal, Parent.LC_CompanyCode);
				duplicateCodeCheck.AddToFilter(JoinCondition.And, LicenceCompanySchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				ZDBOnlySubQuery enterpriseJoin = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceCompanySchema.LC_LE);
				enterpriseJoin.AddToFilter(JoinCondition.And, LicenceEnterpriseSchema.LE_EnterpriseID, SQLComparisonOperator.Equal, MasterHeader.LicenceEnterpriseID);
				duplicateCodeCheck.AddSubQuery(enterpriseJoin, JoinCondition.And);

				BusinessObjectFactory factory = new BusinessObjectFactory();
				var hasMatchingCompany = factory.ExistsInDatabase(LicenceCompany.Schema.TableName, duplicateCodeCheck);

				if (hasMatchingCompany)
				{
					Parent.LC_CompanyCodeInfo.AddError(CodeInUse);
				}
			}
		}

		public const string SelectedCountryNotSupported = @"The selected country is currently not supported by the licence system. Development work is required to add support for the country. Please find out the following info and then contact Core team to add support for this country.

1) What the GST/VAT or other Value Added tax code is for the new country if any � and if not, are there any sales taxes or other tax regimes that are similar?
2) The Currency code and whether Exchange rates are Reciprocal or not 
3) What types of business registrations are their such as Company Numbers, Business Numbers, GST/VAT Numbers or other common registration numbers � there may be multiple please describe and indicate which are primary and / or more common.
4) Provide the number / currency formatting required for this country (e.g. R$1.234,01 or $1,234.01)";
		const string CodeInUse = "The specified code is already in use";
	}
}

