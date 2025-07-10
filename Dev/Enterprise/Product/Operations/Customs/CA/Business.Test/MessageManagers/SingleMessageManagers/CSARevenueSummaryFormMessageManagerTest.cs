using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	[TestedType(typeof(CSARevenueSummaryFormMessageManager))]
	sealed class CSARevenueSummaryFormMessageManagerTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var rsf = Factory.New<CusStatementHeader>();
			return new CSARevenueSummaryFormMessageManager(rsf, MessageSubTypes.Create);
		}

		public void TestSendMessage_TestingMode()
		{
			var statementHeader = Factory.New<CusStatementHeader>();
			statementHeader.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
			var manager = new CSARevenueSummaryFormMessageManager(statementHeader, MessageSubTypes.Create);
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
			manager.SendMessage();
			var lastMessage = statementHeader.Messages.LastOrDefault() as EDIMessage;
			AssertNotNull("Should exist message", lastMessage);
			Assert("Should not be test message", !lastMessage.EM_IsTestMessage);
		}
	}
}
