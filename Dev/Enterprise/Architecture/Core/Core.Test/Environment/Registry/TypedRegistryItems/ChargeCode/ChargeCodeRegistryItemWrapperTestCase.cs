using System;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestsSubclassesOf(typeof(ChargeCodeRegistryItemWrapper<>))]
	public abstract class ChargeCodeRegistryItemWrapperTestCase<T> : StronglyTypedRegistryItemTestCase<T>
	{
		public virtual void TestDefaultChargeCode()
		{
			ChargeCodeRegistryItemWrapper<T> registryItem = (ChargeCodeRegistryItemWrapper<T>)Activator.CreateInstance(ChargeCodeRegistryItemWrapperType, new object[] { "ChargeCodeRegistryItemWrapper", null, null, null, "MOO" });
			AssertEquals("DefaultChargeCode", "MOO", registryItem.DefaultChargeCode);

			registryItem = (ChargeCodeRegistryItemWrapper<T>)Activator.CreateInstance(ChargeCodeRegistryItemWrapperType, new object[] { "ChargeCodeRegistryItemWrapper", null, null, null, "OOF" });
			AssertEquals("DefaultChargeCode", "OOF", registryItem.DefaultChargeCode);
		}

		public void TestStorage()
		{
			AssertEquals("Storage", RegistryStorageFlags.Company, RegistryItem.Storage);
		}

		public virtual void TestDefaultValue()
		{
			string currencyNK = "AUD";

			Guid companyPK1 = Guid.NewGuid();
			Guid companyPK2 = Guid.NewGuid();

			Guid chargeCodePK1 = Guid.NewGuid();
			Guid chargeCodePK2 = Guid.NewGuid();
			Guid chargeCodePK3 = Guid.NewGuid();

			string insertSql =
				GetInsertCompanySql(companyPK1, "ZZ1", currencyNK, EnvProxy.Instance.CurrentCompany.Country.Code) +
				GetInsertCompanySql(companyPK2, "ZZ2", currencyNK, EnvProxy.Instance.CurrentCompany.Country.Code) +
				GetInsertChargeCodeSql(chargeCodePK1, companyPK1) +
				GetInsertChargeCodeSql(chargeCodePK2, companyPK2) +
				GetInsertChargeCodeSql(chargeCodePK3, EnvProxy.Instance.CurrentCompany.PK);

			Db.Connection.ExecuteNonQuery(insertSql);

			IRegistryItemInternals registryItemInternals = RegistryItem;

			AssertEquals("DefaultValue", true, IsEqual(chargeCodePK3, RegistryItem.DefaultValue));
			AssertEquals("Value", true, IsEqual(chargeCodePK3, RegistryItem.Value));

			AssertEquals("DefaultValue for CompanyPK1", true, IsEqual(chargeCodePK1, registryItemInternals.GetDefaultValue(companyPK1, Guid.Empty, Guid.Empty)));
			AssertEquals("DefaultValue for CompanyPK2", true, IsEqual(chargeCodePK2, registryItemInternals.GetDefaultValue(companyPK2, Guid.Empty, Guid.Empty)));
			AssertEquals("DefaultValue for EnvProxy.Instance.CurrentCompany.PK", true, IsEqual(chargeCodePK3, registryItemInternals.GetDefaultValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty)));
			AssertEquals("DefaultValue for invalid Company", true, IsEqual(Guid.Empty, registryItemInternals.GetDefaultValue(Guid.Empty, Guid.Empty, Guid.Empty)));
		}

		public void TestDefaultValueDBHits()
		{
			string currencyNK = "AUD";

			Guid companyPK1 = Guid.NewGuid();
			Guid companyPK2 = Guid.NewGuid();

			Guid chargeCodePK1 = Guid.NewGuid();
			Guid chargeCodePK2 = Guid.NewGuid();
			Guid chargeCodePK3 = Guid.NewGuid();

			string insertSql =
				GetInsertCompanySql(companyPK1, "ZZ1", currencyNK, EnvProxy.Instance.CurrentCompany.Country.Code) +
				GetInsertCompanySql(companyPK2, "ZZ2", currencyNK, EnvProxy.Instance.CurrentCompany.Country.Code) +
				GetInsertChargeCodeSql(chargeCodePK1, companyPK1) +
				GetInsertChargeCodeSql(chargeCodePK2, companyPK2) +
				GetInsertChargeCodeSql(chargeCodePK3, EnvProxy.Instance.CurrentCompany.PK);

			Db.Connection.ExecuteNonQuery(insertSql);

			IRegistryItemInternals registryItemInternals = RegistryItem;

			int prevExecutedCommandCount = Db.Connection.ExecutedCommandCount;

			int expectedDBHitCount = 0;
			for (int i = 0; i < 10; i++)
			{
				registryItemInternals.GetDefaultValue(companyPK1, Guid.Empty, Guid.Empty);
				registryItemInternals.GetDefaultValue(companyPK2, Guid.Empty, Guid.Empty);
				registryItemInternals.GetDefaultValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				registryItemInternals.GetDefaultValue(Guid.Empty, Guid.Empty, Guid.Empty);
				if (i == 0)
				{
					expectedDBHitCount = Db.Connection.ExecutedCommandCount - prevExecutedCommandCount;
				}
			}

			AssertEquals("ExecutedCommandCount", expectedDBHitCount, Db.Connection.ExecutedCommandCount - prevExecutedCommandCount);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			registryItem = (ChargeCodeRegistryItemWrapper<T>)GetNewRegistryItem();
		}

		protected override StronglyTypedRegistryItem<T, T> GetNewRegistryItem()
		{
			return (StronglyTypedRegistryItem<T, T>)Activator.CreateInstance(ChargeCodeRegistryItemWrapperType, new object[] { "ChargeCodeRegistryItemWrapper", null, null, null, "MOO" });
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

		string GetInsertChargeCodeSql(Guid chargeCodePK, Guid companyPK)
		{
			return string.Format(@"
				INSERT INTO {0}
					({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})
				VALUES
					('{9}', 'MOO', '{10}', 'NGC', GetUtcDate(), '~BP', GetUtcDate(), '~BP');",
				AccChargeCodeSchema.Constants.TableName,
				AccChargeCodeSchema.Constants.PK, AccChargeCodeSchema.Constants.AC_Code, AccChargeCodeSchema.Constants.AC_GC,
				AccChargeCodeSchema.Constants.AC_ChargeGroup, AccChargeCodeSchema.Constants.AC_SystemCreateTimeUtc, AccChargeCodeSchema.Constants.AC_SystemCreateUser, AccChargeCodeSchema.Constants.AC_SystemLastEditTimeUtc, AccChargeCodeSchema.Constants.AC_SystemLastEditUser, chargeCodePK.ToString(), companyPK.ToString());
		}

		protected abstract Type ChargeCodeRegistryItemWrapperType { get; }
		protected abstract bool IsEqual(Guid actualChargeCodePK, object obtainedDefaultValue);
		protected ChargeCodeRegistryItemWrapper<T> RegistryItem
		{
			get { return registryItem; }
		}
		ChargeCodeRegistryItemWrapper<T> registryItem;

		#endregion
	}
}
