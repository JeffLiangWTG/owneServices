using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsEuOfficeCodeCollection))]
	public class NctsEuOfficeCodeCollectionPhase4Test : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var parent = Factory.New<NctsHeader>();
			parent.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			parent.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			return new NctsEuOfficeCodeCollection(parent);
		}

		public void TestCusCodeDataShouldHaveCorrectParent()
		{
			var parent = Factory.New<NctsHeader>();
			parent.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			parent.SetMovementType(NctsMovementType.Codes.Departure);

			var cusCodes = Factory.Load<EuOfficeCode>(new ZQuery(CusCodeDataSchema.CY_Type, EU.Business.CusCodeDataTypeList.Codes.OfficeCode).AddToFilter(CusCodeDataSchema.CY_ParentID, parent.PK));
			AssertEquals("There should be 1 office filled.", 2, cusCodes.Length);
			var cusCode = cusCodes[0];
			AssertEquals("The auto filled CusCodeData should have correct parent set.", parent.PK, cusCode.CY_ParentID);
			AssertEquals("The auto filled CusCodeData should have correct parent set.", parent, cusCode.Parent);
		}
	}
}
