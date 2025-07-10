using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	sealed class CUSPRLCusTempStorageLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTransportNumberTypeList()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			storageHeader.SJH_OH_Customer = organisation.PK;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_C0754, "C0754");
			var codeList1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_C0754, "DE01", "DE01 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeList2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_C0754, "DE02", "DE02 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var transportNumberTypeList = storageLine.Lookups.TransportNumberTypeList;
			transportNumberTypeList.Load();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Elements", new[] { "DE01", "DE02" }, transportNumberTypeList.Select(x => x.ZZD_Code));
				AssertSame("Cached", transportNumberTypeList, storageLine.Lookups.TransportNumberTypeList);
			});
		}

		public void TestOwnerReferenceTypeList()
		{
			var ownerReferenceTypeList = storageLine.Lookups.OwnerReferenceTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("Values", "AWB, SIN, ULD, ZZZ", ownerReferenceTypeList.CodesAsString);
				AssertSame("Cached", ownerReferenceTypeList, Factory.GetCachedValue("DE|CUSPRLCusTempStorageLineLookups|OwnerReferenceTypeList", () => new CodeDescriptionPairList()));
			});
		}

		public void TestUnionStatusList()
		{
			storageHeader.SJH_PreviousReferenceType = ZString.Empty;
			var unionStatusList = storageLine.Lookups.UnionStatusList;
			CombineAssertions(() =>
			{
				AssertEquals("Values", "C, F, N", unionStatusList.CodesAsString);
				AssertSame("Cached", unionStatusList, storageLine.Lookups.UnionStatusList);
			});
		}

		public void TestUnionStatusList_444T2()
		{
			storageHeader.SJH_PreviousReferenceType = PreviousReferenceType.Codes._444T2;
			AssertEquals("C, F, N", storageLine.Lookups.UnionStatusList.CodesAsString);
		}

		public void TestUnionStatusList_OHNE()
		{
			storageHeader.SJH_PreviousReferenceType = PreviousReferenceType.Codes._OHNE;
			AssertEquals("N", storageLine.Lookups.UnionStatusList.CodesAsString);
		}

		public void TestUnionStatusList_199()
		{
			storageHeader.SJH_PreviousReferenceType = PreviousReferenceType.Codes._199;
			AssertEquals("C, D, F, N, X", storageLine.Lookups.UnionStatusList.CodesAsString);
		}

		public void TestUnionStatusList_200()
		{
			storageHeader.SJH_PreviousReferenceType = PreviousReferenceType.Codes._200;
			AssertEquals("C, D, F, N, X", storageLine.Lookups.UnionStatusList.CodesAsString);
		}

		public void TestUnionStatusList_T()
		{
			storageHeader.SJH_PreviousReferenceType = PreviousReferenceType.Codes._T;
			AssertEquals("C, D, F, N, X", storageLine.Lookups.UnionStatusList.CodesAsString);
		}

		public void TestUnionStatusList_T2()
		{
			storageHeader.SJH_PreviousReferenceType = PreviousReferenceType.Codes._T2;
			AssertEquals("C, D, F, N, X", storageLine.Lookups.UnionStatusList.CodesAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			storageHeader = Factory.New<CusTempStorageJobHeader>();
			var storageDec = CUSPRLCusTempStorageDec.LoadOrCreate(storageHeader);
			storageLine = storageDec.CusTempStorageLines.AddNew();
		}
		CusTempStorageJobHeader storageHeader;
		CUSPRLCusTempStorageLine storageLine;
	}
}
