using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	class CHGOFFCusTempStorageLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOwnerReferenceTypeList()
		{
			CombineAssertions(() =>
			{
				declaration.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
				AssertEquals("AWB", "AWB, ULD", storageLine.Lookups.OwnerReferenceTypeList.CodesAsString);
				AssertSame("Cached", storageLine.Lookups.OwnerReferenceTypeList, Factory.GetCachedValue("DE|CHGOFFCusTempStorageLineLookups|OwnerReferenceTypeList|AWB", () => new CodeDescriptionPairList()));

				declaration.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
				AssertEquals("REG: Count", 0, storageLine.Lookups.OwnerReferenceTypeList.Count);

				declaration.STH_IdentificationIndicator = ZString.Empty;
				AssertEquals("Empty: Count", 0, storageLine.Lookups.OwnerReferenceTypeList.Count);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<CHGOFFCusTempStorageDec>();
			storageLine = declaration.CusTempStorageLines.AddNew();
		}
		CHGOFFCusTempStorageDec declaration;
		CHGOFFCusTempStorageLine storageLine;
	}
}
