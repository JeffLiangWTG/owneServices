using System;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestsSubclassesOf(typeof(AccTaxRateRegistryItemWrapper<>))]
	public abstract class AccTaxRateRegistryItemWrapperTestCase<T> : StronglyTypedRegistryItemTestCase<T>
	{
		public void TestDefaultTaxRate()
		{
			AccTaxRateRegistryItemWrapper<T> registryItem = (AccTaxRateRegistryItemWrapper<T>)Activator.CreateInstance(AccTaxRateRegistryItemWrapperType, new object[] { "AccTaxRateRegistryItemWrapper", null, null, null, "MOO" });
			AssertEquals("DefaultTaxRate", "MOO", registryItem.DefaultTaxRate);

			registryItem = (AccTaxRateRegistryItemWrapper<T>)Activator.CreateInstance(AccTaxRateRegistryItemWrapperType, new object[] { "AccTaxRateRegistryItemWrapper", null, null, null, "OOF" });
			AssertEquals("DefaultTaxRate", "OOF", registryItem.DefaultTaxRate);
		}

		public void TestStorage()
		{
			AssertEquals("Storage", RegistryStorageFlags.Company, registryItem.Storage);
		}

		public void TestDefaultValue()
		{
			string currencyNK = "EUR";

			Guid companyPK1 = Guid.NewGuid();
			Guid companyPK2 = Guid.NewGuid();

			Guid taxRatePK1 = Guid.NewGuid();
			Guid taxRatePK2 = Guid.NewGuid();
			Guid taxRatePK3 = Guid.NewGuid();

			string insertSql =
				GetInsertCompanySql(companyPK1, "ZZ1", currencyNK, EnvProxy.Instance.CurrentCompany.Country.Code) +
				GetInsertCompanySql(companyPK2, "ZZ2", currencyNK, EnvProxy.Instance.CurrentCompany.Country.Code) +
				GetInsertTaxRateSql(taxRatePK1, "XYZGST", EnvProxy.Instance.CurrentCompany.Country.Code) +
				GetInsertTaxRateSql(taxRatePK2, "ZZZGST", EnvProxy.Instance.CurrentCompany.Country.Code) +
				GetInsertTaxRateSql(taxRatePK3, "ABCGST", EnvProxy.Instance.CurrentCompany.Country.Code);

			Db.Connection.ExecuteNonQuery(insertSql);

			IRegistryItemInternals registryItemInternals = registryItem;

			AssertEquals("DefaultValue", true, IsEqual(taxRatePK3, registryItem.DefaultValue));
			AssertEquals("Value", true, IsEqual(taxRatePK3, registryItem.Value));

			AssertEquals("DefaultValue for CompanyPK1", true, IsEqual(taxRatePK3, registryItemInternals.GetDefaultValue(companyPK1, Guid.Empty, Guid.Empty)));
			AssertEquals("DefaultValue for CompanyPK2", true, IsEqual(taxRatePK3, registryItemInternals.GetDefaultValue(companyPK2, Guid.Empty, Guid.Empty)));
			AssertEquals("DefaultValue for EnvProxy.Instance.CurrentCompany.PK", true, IsEqual(taxRatePK3, registryItemInternals.GetDefaultValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty)));
			AssertEquals("DefaultValue for invalid Company", true, IsEqual(Guid.Empty, registryItemInternals.GetDefaultValue(Guid.Empty, Guid.Empty, Guid.Empty)));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			registryItem = (AccTaxRateRegistryItemWrapper<T>)Activator.CreateInstance(AccTaxRateRegistryItemWrapperType, new object[] { "AccTaxRateRegistryItemWrapper", null, null, null, "ABCGST" });
		}

		protected override FallbackLevel Fallback
		{
			get { return new FallbackLevel(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		string GetInsertCompanySql(Guid companyPK, string companyCode, string currencyNK, string countryCode)
		{
			return @$"
				INSERT INTO {GlbCompanySchema.Constants.TableName}
					({GlbCompanySchema.Constants.PK}, {GlbCompanySchema.Constants.GC_Code}, {GlbCompanySchema.Constants.GC_Name}, {GlbCompanySchema.Constants.GC_RX_NKLocalCurrency}, {GlbCompanySchema.Constants.GC_RN_NKCountryCode})
				values
					('{companyPK.ToString()}', '{companyCode}', 'Company', '{currencyNK}', '{countryCode}');";
		}

		string GetInsertTaxRateSql(Guid chargeCodePK, string code, string countryCode)
		{
			return string.Format(@"
				INSERT INTO {0}
					({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})
				VALUES
					('{9}', '{10}', '{11}', '{12}', GetUtcDate(), '~BP', GetUtcDate(), '~BP');",
				AccTaxRateSchema.Constants.TableName,
				AccTaxRateSchema.Constants.PK, AccTaxRateSchema.Constants.AT_Code, AccTaxRateSchema.Constants.AT_Type, AccTaxRateSchema.Constants.AT_RN_NKCountry, AccTaxRateSchema.Constants.AT_SystemCreateTimeUtc, AccTaxRateSchema.Constants.AT_SystemCreateUser, AccTaxRateSchema.Constants.AT_SystemLastEditTimeUtc, AccTaxRateSchema.Constants.AT_SystemLastEditUser,
				chargeCodePK.ToString(), code, "RAT", countryCode);
		}

		protected abstract Type AccTaxRateRegistryItemWrapperType { get; }
		protected abstract bool IsEqual(Guid actualTaxRatePK, object obtainedDefaultValue);
		AccTaxRateRegistryItemWrapper<T> registryItem;

		#endregion
	}
}
