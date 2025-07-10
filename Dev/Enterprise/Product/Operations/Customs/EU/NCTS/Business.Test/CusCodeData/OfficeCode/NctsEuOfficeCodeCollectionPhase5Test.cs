using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsEuOfficeCodeCollection))]
	public class NctsEuOfficeCodeCollectionPhase5Test : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var parent = Factory.New<NctsHeader>();
			parent.SetMovementType(NctsMovementType.Codes.Departure);
			return new NctsEuOfficeCodeCollection(parent.MovementHeader);
		}

		public void TestCusCodeDataShouldHaveCorrectParent()
		{
			var header = Factory.New<NctsHeader>();

			header.SetMovementType(NctsMovementType.Codes.Departure);
			var parent = header.MovementHeader;

			var cusCodes = Factory.Load<EuOfficeCode>(new ZQuery(CusCodeDataSchema.CY_Type, EU.Business.CusCodeDataTypeList.Codes.OfficeCode).AddToFilter(CusCodeDataSchema.CY_ParentID, parent.PK));
			AssertEquals("There should be 1 office filled.", 2, cusCodes.Length);
			var cusCode = cusCodes[0];
			AssertEquals("The auto filled CusCodeData should have correct parent set.", parent.PK, cusCode.CY_ParentID);
			AssertEquals("The auto filled CusCodeData should have correct parent set.", parent, cusCode.Parent);
		}

		public void TestParentTableForNctsCommonMovementHeader() => CombineAssertions(() =>
		{
			var movementHeader = Factory.New<NctsDepartureMovementHeader>();
			var collection = new NctsEuOfficeCodeCollection(movementHeader);
			var officeCode = collection.AddNew();
			AssertEquals("Prefix", CusInBondMoveHeaderSchema.Constants.Prefix, officeCode.CY_ParentTableCode);
			AssertEquals("Parent", movementHeader, officeCode.Parent);
		});
	}
}
