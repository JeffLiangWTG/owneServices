using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStoragePack))]
	sealed class TemporaryStoragePackTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			AssertEquals("Storage Pack", Factory.New<TemporaryStoragePack>().HumanReadableName);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var container = header.Containers.AddNew();
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = container.PK;
			return pack;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		public void TestReadOnlyPropertiesUnderTSDStatus()
		{
			ZString[] noEditAllowedStatus = new ZString[]
			{
				UniversalReferenceConstants.PNTS.CustomsStatus.TSDInvalidated,
				UniversalReferenceConstants.PNTS.CustomsStatus.UnderControl,
				UniversalReferenceConstants.PNTS.CustomsStatus.IntendedControl,
				UniversalReferenceConstants.PNTS.CustomsStatus.IrregularityUnderInvestigation,
				UniversalReferenceConstants.PNTS.CustomsStatus.ProofOfUnionStatusPresented,
				UniversalReferenceConstants.PNTS.CustomsStatus.MeasuresRequired,
				UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStorageEnded
			};

			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			header.ENSReuse = 0;
			var bill = header.Bills.AddNew();
			var container = header.Containers.AddNew();
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = container.PK;

			foreach (var status in noEditAllowedStatus)
			{
				header.CustomsStatus = status;
				var propertyInfos = pack.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(propertyInfo.ReadOnly);
				}
			}
		}

		public void TestAPA_LineNo()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var pack1 = bill.Packs.AddNew();
			var pack2 = bill.Packs.AddNew();
			var pack3 = bill.Packs.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("Line number 1", (ZShort)1, pack1.APA_LineNo);
				AssertEquals("Line number 2", (ZShort)2, pack2.APA_LineNo);
				AssertEquals("Line number 3", (ZShort)3, pack3.APA_LineNo);

				bill.Packs.RemoveAndDelete(pack2);
				AssertEquals("Line number 1 stay same", (ZShort)1, pack1.APA_LineNo);
				AssertEquals("Line number 3 change to 2", (ZShort)2, pack3.APA_LineNo);
			});
		}

		public void TestLookups()
		{
			var package = Factory.New<TemporaryStoragePack>();
			AssertType<TemporaryStoragePackLookups>("Lookups Type", package.Lookups);
		}

		public void TestIsBulk()
		{
			SetUpPackageTypes();
			Factory.Save();

			CombineAssertions(() =>
			{
				var package = Factory.New<TemporaryStoragePack>();
				package.APA_PackUQ = "PP";
				AssertEquals("When Pack Type is 'PP' (no BULK/BREAKBULK attribute), IsBulk", false, package.IsBulk);

				package.APA_PackUQ = "VG";
				AssertEquals("When Pack Type is 'VG' (BULK attribute), IsBulk", true, package.IsBulk);

				package.APA_PackUQ = "NE";
				AssertEquals("When Pack Type is 'NE' (BREAKBULK attribute), IsBulk", false, package.IsBulk);
			});
		}

		public void TestIsBreakBulk()
		{
			SetUpPackageTypes();
			Factory.Save();

			CombineAssertions(() =>
			{
				var package = Factory.New<TemporaryStoragePack>();
				package.APA_PackUQ = "PP";
				AssertEquals("When Pack Type is 'PP' (no BULK/BREAKBULK attribute), IsBreakBulk", false, package.IsBreakBulk);

				package.APA_PackUQ = "VG";
				AssertEquals("When Pack Type is 'VG' (BULK attribute), IsBreakBulk", false, package.IsBreakBulk);

				package.APA_PackUQ = "NE";
				AssertEquals("When Pack Type is 'NE' (BREAKBULK attribute), IsBreakBulk", true, package.IsBreakBulk);
			});
		}

		void SetUpPackageTypes()
		{
			const string dataGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations;
			const string codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(dataGrouping);
			helper.CreateNewOrGetExistingCusCodeType(codeType, "UN Package code List");
			helper.CreateCusCodeList(dataGrouping, codeType, "PP", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListWithAttribute(dataGrouping, codeType, "VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			helper.CreateCusCodeListWithAttribute(dataGrouping, codeType, "NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
		}
	}
}
