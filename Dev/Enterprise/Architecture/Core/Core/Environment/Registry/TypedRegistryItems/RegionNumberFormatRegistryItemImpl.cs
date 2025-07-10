using System;
using System.Globalization;
using System.Text.Json;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Environment
{
	class RegionNumberFormatRegistryItemImpl : RegistryItemImpl
	{
		public RegionNumberFormatRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options)
			: base(name, category, caption, hint, dataType, storage, options)
		{
		}

		protected override void SetValueCore(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			base.SetValueCore(companyOrOwnerPK, branchPK, departmentPK, newValue);
			Culture.fCurrentCompanyCountryCulture = null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Baseline WI00536012 and WI00744192")]
		protected static CultureInfo GetOriginalCulture(Guid companyPK)
		{
			Culture.fCurrentCompanyCountryCulture = null;
			var currentCompany = EnvProxy.Instance.CurrentCompany;

			CultureInfo originalCulture;
			if (currentCompany.PK == companyPK)
			{
				originalCulture = Culture.GetCulture(currentCompany.Country.Code);
#if DEBUG
				var currentCulture = currentCompany.Country.Culture;
				if (currentCulture.Name != originalCulture.Name)
				{
					if (!Globals.IsTest)
					{
						throw new NotSupportedException("Setting of culture directly is only supported in test cases.");
					}
					return currentCulture;
				}
#endif
			}
			else
			{
				using (var command = Db.Connection.Command("select RN_Code from dbo.RefCountry join dbo.GlbCompany on GC_RN_NKCountryCode = RN_Code where GC_PK = @Code"))
				{
					command.AddParameterBasedOnDbColumn("@Code", companyPK, GlbCompanySchema.PK);
					var countryCode = (string)command.ExecuteScalar();
					originalCulture = Culture.GetCulture(countryCode);
				}
			}
			return originalCulture;
		}

		protected static string GetDefaultGroupSizes(int[] currencyGroupSeparator)
		{
			var currentCurrencyGroupSizes = JsonSerializer.Serialize(currencyGroupSeparator);
			var defaultNCurrencyGroupSizes = currentCurrencyGroupSizes.Substring(1, currentCurrencyGroupSizes.Length - 2);
			return defaultNCurrencyGroupSizes;
		}
	}
}
