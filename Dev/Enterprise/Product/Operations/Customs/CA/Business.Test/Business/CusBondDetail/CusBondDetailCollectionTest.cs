using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;
using static Enterprise.Customs.CA.Business.CusBondDetailCollection;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusBondDetailCollection))]
	sealed class CusBondDetailCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDoesNotShowOtherRows()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var baseDetail1 = Factory.New<MasterFiles.Business.CusBondDetail>();
			var baseDetail2 = Factory.New<MasterFiles.Business.CusBondDetail>();
			var baseDetail3 = Factory.New<MasterFiles.Business.CusBondDetail>();
			baseDetail1.Parent = org;
			baseDetail2.Parent = org;
			baseDetail3.Parent = org;
			var bondCollection = new CusBondDetailCollection(org);
			bondCollection.Load();
			Assert(!bondCollection.Contains(baseDetail1));
			Assert(!bondCollection.Contains(baseDetail2));
			Assert(!bondCollection.Contains(baseDetail3));
			baseDetail2.PW_ApplicationCode = ApplicationCodeList.Codes.EuNcts;
			baseDetail1.PW_ApplicationCode = ApplicationCodeList.Codes.CACustoms;
			bondCollection = new CusBondDetailCollection(org);
			bondCollection.Load();
			Assert(bondCollection.Contains(baseDetail1));
			Assert(!bondCollection.Contains(baseDetail2));
			Assert(!bondCollection.Contains(baseDetail3));
			baseDetail1.PW_ApplicationCode = ApplicationCodeList.Codes.EuNcts;
			baseDetail3.PW_ApplicationCode = ApplicationCodeList.Codes.INConsolManifest;
			bondCollection = new CusBondDetailCollection(org);
			bondCollection.Load();
			Assert(!bondCollection.Contains(baseDetail1));
			Assert(!bondCollection.Contains(baseDetail2));
			Assert(!bondCollection.Contains(baseDetail3));
			var bondData = bondCollection.AddNew();
			AssertEquals(ApplicationCodeList.Codes.CACustoms, bondData.PW_ApplicationCode);
			Assert(bondCollection.Contains(bondData));
		}

		public void TestGetActiveBondDetailData_SpecificBondType()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var bondCollection = new CusBondDetailCollection(org);
			var bondData1 = bondCollection.AddNew();
			bondData1.PW_BondType = BondTypeList.Codes.ContinuousBond;
			bondData1.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-4);

			var bondData2 = bondCollection.AddNew();
			bondData2.PW_BondType = BondTypeList.Codes.SingleTransactionBond;
			bondData2.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-3);

			var bondData3 = bondCollection.AddNew();
			bondData3.PW_BondType = BondTypeList.Codes.NotOnPortal;
			bondData3.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-2);

			var bondData4 = bondCollection.AddNew();
			bondData4.PW_BondType = BondTypeList.Codes.OnPortal;
			bondData4.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-1);

			var bondData5 = bondCollection.AddNew();
			bondData5.PW_BondType = BondTypeList.Codes.ContinuousBond;
			bondData5.PW_BondEffectiveDate = ZDateTime.Today.AddDays(1);

			var bondData6 = bondCollection.AddNew();
			bondData6.PW_BondType = BondTypeList.Codes.SingleTransactionBond;
			bondData6.PW_BondEffectiveDate = ZDateTime.Today.AddDays(2);

			var bondData7 = bondCollection.AddNew();
			bondData7.PW_BondType = BondTypeList.Codes.NotOnPortal;
			bondData7.PW_BondEffectiveDate = ZDateTime.Today.AddDays(3);

			var bondData8 = bondCollection.AddNew();
			bondData8.PW_BondType = BondTypeList.Codes.OnPortal;
			bondData8.PW_BondEffectiveDate = ZDateTime.Today.AddDays(4);

			AssertEquals(bondData1, bondCollection.GetActiveBondDetailData(BondTypeList.Codes.ContinuousBond, ZDateTime.Today));
			AssertEquals(bondData5, bondCollection.GetActiveBondDetailData(BondTypeList.Codes.ContinuousBond, ZDateTime.Today.AddDays(5)));
			AssertNull(bondCollection.GetActiveBondDetailData(BondTypeList.Codes.ContinuousBond, ZDateTime.Today.AddDays(-5)));

			AssertEquals(bondData2, bondCollection.GetActiveBondDetailData(BondTypeList.Codes.SingleTransactionBond, ZDateTime.Today));
			AssertEquals(bondData6, bondCollection.GetActiveBondDetailData(BondTypeList.Codes.SingleTransactionBond, ZDateTime.Today.AddDays(5)));
			AssertNull(bondCollection.GetActiveBondDetailData(BondTypeList.Codes.SingleTransactionBond, ZDateTime.Today.AddDays(-5)));

			AssertEquals(bondData3, bondCollection.GetActiveBondDetailData(BondTypeList.Codes.NotOnPortal, ZDateTime.Today));
			AssertEquals(bondData7, bondCollection.GetActiveBondDetailData(BondTypeList.Codes.NotOnPortal, ZDateTime.Today.AddDays(5)));
			AssertNull(bondCollection.GetActiveBondDetailData(BondTypeList.Codes.NotOnPortal, ZDateTime.Today.AddDays(-5)));

			AssertEquals(bondData4, bondCollection.GetActiveBondDetailData(BondTypeList.Codes.OnPortal, ZDateTime.Today));
			AssertEquals(bondData8, bondCollection.GetActiveBondDetailData(BondTypeList.Codes.OnPortal, ZDateTime.Today.AddDays(5)));
			AssertNull(bondCollection.GetActiveBondDetailData(BondTypeList.Codes.OnPortal, ZDateTime.Today.AddDays(-5)));
		}

		public void TestGetActiveBondDetailData_EmptyBondType()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var bondCollection = new CusBondDetailCollection(org);

			var bondData1 = bondCollection.AddNew();
			bondData1.PW_BondType = BondTypeList.Codes.OnPortal;
			bondData1.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-1);

			var bondData2 = bondCollection.AddNew();
			bondData2.PW_BondType = BondTypeList.Codes.OnPortal;
			bondData2.PW_BondEffectiveDate = ZDateTime.Today.AddDays(4);

			AssertEquals(bondData1, bondCollection.GetActiveBondDetailData(ZString.Empty, ZDateTime.Today));
			AssertEquals(bondData2, bondCollection.GetActiveBondDetailData(ZString.Empty, ZDateTime.Today.AddDays(5)));
			AssertNull(bondCollection.GetActiveBondDetailData(ZString.Empty, ZDateTime.Today.AddDays(-5)));

			var bondData3 = bondCollection.AddNew();
			bondData3.PW_BondType = BondTypeList.Codes.SingleTransactionBond;
			bondData3.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-2);

			var bondData4 = bondCollection.AddNew();
			bondData4.PW_BondType = BondTypeList.Codes.SingleTransactionBond;
			bondData4.PW_BondEffectiveDate = ZDateTime.Today.AddDays(3);

			AssertEquals(bondData3, bondCollection.GetActiveBondDetailData(ZString.Empty, ZDateTime.Today));
			AssertEquals(bondData4, bondCollection.GetActiveBondDetailData(ZString.Empty, ZDateTime.Today.AddDays(5)));
			AssertNull(bondCollection.GetActiveBondDetailData(ZString.Empty, ZDateTime.Today.AddDays(-5)));

			var bondData5 = bondCollection.AddNew();
			bondData5.PW_BondType = BondTypeList.Codes.ContinuousBond;
			bondData5.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-3);

			var bondData6 = bondCollection.AddNew();
			bondData6.PW_BondType = BondTypeList.Codes.ContinuousBond;
			bondData6.PW_BondEffectiveDate = ZDateTime.Today.AddDays(2);

			AssertEquals(bondData5, bondCollection.GetActiveBondDetailData(ZString.Empty, ZDateTime.Today));
			AssertEquals(bondData6, bondCollection.GetActiveBondDetailData(ZString.Empty, ZDateTime.Today.AddDays(5)));
			AssertNull(bondCollection.GetActiveBondDetailData(ZString.Empty, ZDateTime.Today.AddDays(-5)));

			var bondData7 = bondCollection.AddNew();
			bondData7.PW_BondType = BondTypeList.Codes.NotOnPortal;
			bondData7.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-4);

			var bondData8 = bondCollection.AddNew();
			bondData8.PW_BondType = BondTypeList.Codes.NotOnPortal;
			bondData8.PW_BondEffectiveDate = ZDateTime.Today.AddDays(1);

			AssertEquals(bondData7, bondCollection.GetActiveBondDetailData(ZString.Empty, ZDateTime.Today));
			AssertEquals(bondData8, bondCollection.GetActiveBondDetailData(ZString.Empty, ZDateTime.Today.AddDays(5)));
			AssertNull(bondCollection.GetActiveBondDetailData(ZString.Empty, ZDateTime.Today.AddDays(-5)));
		}

		public void TestGetBondDetailsStatus()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var bondCollection = new CusBondDetailCollection(org);
			AssertEquals(BondDetailsStatus.NoBond, bondCollection.GetBondDetailsStatus(ZString.Empty, ZDateTime.Today));
			var bondData1 = bondCollection.AddNew();
			bondData1.PW_BondType = BondTypeList.Codes.SingleTransactionBond;
			bondData1.PW_BondEffectiveDate = ZDateTime.Today.AddDays(10);
			AssertEquals(BondDetailsStatus.AllExpired, bondCollection.GetBondDetailsStatus(BondTypeList.Codes.SingleTransactionBond, ZDateTime.Today));
			bondData1.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-10);
			AssertEquals(BondDetailsStatus.BondExist, bondCollection.GetBondDetailsStatus(BondTypeList.Codes.SingleTransactionBond, ZDateTime.Today));
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new CusBondDetailCollection(Organisation);

		OrgHeader organisation;
		OrgHeader Organisation => organisation ?? (organisation = Factory.NewWithValidTestData<OrgHeader>());
	}
}
