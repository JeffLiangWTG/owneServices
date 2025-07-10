using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(AsycudaTax))]
	public class AsycudaTaxTest : EnterpriseBusinessObjectTestCase
	{
		public void TestClusterKeyFromAsycudaBill()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_AMA = manifestHeader.PK;
			var tax = Factory.NewWithValidTestData<AsycudaTax>();
			tax.AET_ABL = bill.PK;
			AssertEquals(0, tax.AET_ClusterKey);

			Factory.Save();
			AssertEquals("ClusterKey generated onSaving(There is already one AsycudaManifestHeader saved in Setup() method).", 2, tax.AET_ClusterKey);
		}

		public void TestClusterKeyFromAsycudaPackedItem()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_AMA = manifestHeader.PK;

			var packedItem = Factory.New<AsycudaPackedItem>();
			packedItem.API_ABL_Bill = bill.PK;

			var tax = Factory.NewWithValidTestData<AsycudaTax>();
			tax.AET_API_AsycudaPackedItem = packedItem.PK;
			AssertEquals(0, tax.AET_ClusterKey);

			Factory.Save();
			AssertEquals("ClusterKey generated onSaving(There is already one AsycudaManifestHeader saved in Setup() method).", 2, tax.AET_ClusterKey);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			var tax = factory.New<AsycudaTax>();
			header.AMA_JobReference = "X";
			header.AMA_RN_NKCountry = "SB";
			header.AMA_Nature = "ABC";
			var bill = header.Bills.AddNew();
			tax.AET_ABL = bill.PK;
			tax.AET_MethodOfCalculation = "X";
			return tax;
		}

		public void TestAET_BaseValue()
		{
			AssertEquals(info.AET_BaseValue, (ZDecimal)10);
		}

		public void TestAET_Rate()
		{
			AssertEquals(info.AET_Rate, (ZDecimal)15);
		}

		public void TestAET_ChargeAmount()
		{
			AssertEquals(info.AET_ChargeAmount, (ZDecimal)20);
		}

		public void TestBill()
		{
			var bill = Factory.New<AsycudaManifestHeader>().Bills.AddNew();
			var tax = Factory.New<AsycudaTax>();
			AssertNull(tax.Bill);
			tax.AET_ABL = bill.PK;
			AssertEquals(tax.Bill, bill);
		}

		public void TestPackedItem()
		{
			var packedItem = Factory.New<AsycudaManifestHeader>().Bills.AddNew().PackedItems.AddNew();
			var tax = Factory.New<AsycudaTax>();
			AssertNull(tax.PackedItem);
			tax.AET_API_AsycudaPackedItem = packedItem.PK;
			AssertEquals(tax.PackedItem, packedItem);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => info;

		protected override void SetUp()
		{
			base.SetUp();
			Factory.Save();
			SetData();
		}

		void SetData()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			info = Factory.New<AsycudaTax>();
			info.AET_BaseValue = 10;
			info.AET_Rate = 15;
			info.AET_ChargeAmount = 20;
			info.AET_MethodOfCalculation = "%";
			info.AET_ABL = bill.PK;
			info.AET_ClusterKey = header.AMA_ClusterKey;
		}

		public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues() => new List<ZString>() { AsycudaTax.Schema.AET_ABL };

		AsycudaManifestHeader header;
		AsycudaTax info;
	}
}
