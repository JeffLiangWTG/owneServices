using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(DepositBalanceCollection))]
	public class DepositBalanceCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DepositBalanceCollection>
	{
		public void TestLoad()
		{
			DepositBalanceCollection.LoadFromDb(null);
			DepositBalanceCollection.LoadFromDb(Array.Empty<Guid>());
			DepositBalanceCollection.LoadFromDb(new Guid[] { Company.LC_OH.ToGuid() });

			var defaultList = new CodeDescriptionPairList();
			defaultList.AddPair("STLDEPOSIT", "");
			defaultList.AddPair("DEPOSIT", "");
			EDIDataRegistry.Instance.DepositChargeCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultList);
			var list = new DepositBalanceCollection(Company);
			list.Load();
			AssertEquals(2, list.Count);
			AssertEquals(0m, list[0].Amount);
			AssertEquals(0m, list[1].Amount);
			AssertEquals("DEPOSIT", list[0].ChargeCode);
			AssertEquals("STLDEPOSIT", list[1].ChargeCode);

			string sql =
@"INSERT INTO dbo.EdiDepositBalance (DEB_OH, DEB_ChargeCode, DEB_RX_NKCurrency, DEB_Amount, DEB_Tax, DEB_LastTransactionUtc, DEB_LastUpdatedUtc, DEB_IsValid)
VALUES (@OrgPk, 'DEPOSIT', 'AUD', 123, '12.3', '2015-8-1','2015-8-1', 0)";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@OrgPk", SqlDbType.UniqueIdentifier, Company.LC_OH.ToGuid());
				cmd.ExecuteNonQuery();
			}

			list.Load();
			AssertEquals(123m, list[0].Amount);
			AssertEquals(12.3m, list[0].Tax);
			AssertEquals("AUD", list[0].CurrencyCode);
			AssertEquals(false, list[0].IsValid);
			AssertEquals(new ZDateTime(2015, 8, 1), list[0].LastTransactionUtc);
			AssertEquals(true, list.HasChanges);
			Factory.Save();
			AssertEquals(false, list.HasChanges);

			var dbList = DepositBalanceCollection.LoadFromDb(null);
			AssertEquals(1, dbList.Count);
			AssertEquals(123m, dbList[0].Amount);
			AssertEquals(12.3m, dbList[0].Tax);
			AssertEquals(false, dbList[0].IsValid);

			dbList = DepositBalanceCollection.LoadFromDb(Array.Empty<Guid>());
			AssertEquals(0, dbList.Count);

			dbList = DepositBalanceCollection.LoadFromDb(new Guid[] { Company.LC_OH.ToGuid() });
			AssertEquals(1, dbList.Count);
			AssertEquals(123m, dbList[0].Amount);
			AssertEquals(12.3m, dbList[0].Tax);
			AssertEquals(false, dbList[0].IsValid);
		}

		public void TestAllowNew()
		{
			AssertEquals(false, Collection.AllowNew);
		}

		#region Implementation

		protected override DepositBalanceCollection GetCollectionToTest()
		{
			return new DepositBalanceCollection(Company);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DepositBalance(Company, "");
		}

		#endregion

		LicenceCompany Company;

		protected override void SetUp()
		{
			base.SetUp();

			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = "DDDSYD";
			org.CreateAndLoadLicenceForOrg();
			Factory.Save();
			Company = org.LicCompany;
		}
	}
}
