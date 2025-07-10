using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.Customs.FR.NCTS.Messaging.Testing
{
	internal class TP5MessageSendingObjectLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRegularJustificationCodesList()
		{
			var header = Factory.New<FR.Business.NCTS.NctsHeader>();
			header.SetMovementType("D");
			var movementHeader = header.MovementHeader;
			var messageSendingObject = new TP5MessageSendingObject(header);
			var lookups = messageSendingObject.Lookups;

			AssertContainsExactElementsInAnyOrder(new string[] { "1", "2" }, lookups.RegularJustificationCodesList.GetAllCodes());
		}

		public void TestQueryIdentifierCodesList()
		{
			SetUpCusCodeList_CL054(Factory);

			var header = Factory.New<FR.Business.NCTS.NctsHeader>();
			var messageSendingObject = new TP5MessageSendingObject(header);
			var lookups = messageSendingObject.Lookups;

			AssertEquals("QueryIdentifierCodesList should contain data for codeType: CL054 and grouping EUN", "Identifier1, Identifier2", lookups.QueryIdentifierCodesList.CodesAsString);
		}

		public void TestRequesterRoleCodesList()
		{
			SetUpCusCodeList_CL156(Factory);

			var header = Factory.New<FR.Business.NCTS.NctsHeader>();
			var messageSendingObject = new TP5MessageSendingObject(header);
			var lookups = messageSendingObject.Lookups;

			AssertEquals("RequesterRoleCodesList should contain data for codeType: CL156 and grouping EUN", "Role1, Role2", lookups.RequesterRoleCodesList.CodesAsString);
		}

		internal static void SetUpCusCodeList_CL054(BusinessObjectFactory factory)
		{
			var refCusCodeListType = EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL054;
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateCusCodeType(refCusCodeListType, "NCTS Query Identifier");
			helper.CreateCusCodeList("EUN", refCusCodeListType, "Identifier1", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("EUN", refCusCodeListType, "Identifier2", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("FR", refCusCodeListType, "Identifier3", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("EUN", refCusCodeListType, "Identifier4", ZDateTime.Now.AddDays(-10), ZDateTime.Now.AddDays(-5));
			factory.Save();
		}

		internal static void SetUpCusCodeList_CL156(BusinessObjectFactory factory)
		{
			var refCusCodeListType = EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL156;
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateCusCodeType(refCusCodeListType, "NCTS Role of Requester");
			helper.CreateCusCodeList("EUN", refCusCodeListType, "Role1", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("EUN", refCusCodeListType, "Role2", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("FR", refCusCodeListType, "Role3", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusCodeList("EUN", refCusCodeListType, "Role4", ZDateTime.Now.AddDays(-10), ZDateTime.Now.AddDays(-5));
			factory.Save();
		}
	}
}
