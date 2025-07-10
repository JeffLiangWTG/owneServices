using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	public class REXDISCusTempStorageSumALineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOwnerReferenceTypeList_AWB()
		{
			CombineAssertions(() =>
			{
				declaration.STH_IdentificationIndicator = OwnerReferenceTypeList.Codes.AWB;
				var ownerReferenceTypeList = storageLine.Lookups.OwnerReferenceTypeList;
				AssertEquals("Codes", "AWB, ULD", ownerReferenceTypeList.CodesAsString);
				AssertSame("Cached", ownerReferenceTypeList, Factory.GetCachedValue("DE|REXDISCusTempStorageSumALineLookups|OwnerReferenceTypeList|AWB", () => new CodeDescriptionPairList()));
			});
		}

		public void TestOwnerReferenceTypeList_SIN()
		{
			declaration.STH_IdentificationIndicator = OwnerReferenceTypeList.Codes.SIN;
			AssertEquals("Codes", "SIN", storageLine.Lookups.OwnerReferenceTypeList.CodesAsString);
		}

		public void TestOwnerReferenceTypeList_Invalid()
		{
			declaration.STH_IdentificationIndicator = "Z@E";
			AssertEquals("Codes", ZString.Empty, storageLine.Lookups.OwnerReferenceTypeList.CodesAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<REXDISCusTempStorageDec>();
			storageLine = declaration.CusTempStorageLines.AddNew().SumALine;
		}
		REXDISCusTempStorageSumALine storageLine;
		REXDISCusTempStorageDec declaration;
	}
}
