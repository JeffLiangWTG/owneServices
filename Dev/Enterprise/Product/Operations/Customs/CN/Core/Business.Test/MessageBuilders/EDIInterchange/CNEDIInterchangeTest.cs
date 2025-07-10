using System;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CNEDIInterchange))]
	public class CNEDIInterchangeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetInterchangeNumber()
		{
			using (CNSWClientSettingCheckerTest.TemporarilySetCNSWClientSetting(Factory, Env.CurrentCompanyPK, Guid.Empty))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				var entry = declaration.ActiveEntryHeaders.AddNew();

				var message = Factory.New<CNEDIMessage>();
				message.EM_MessageType = EDIMessageTypeList.Codes.ACD;
				message.EM_MessageText = "ACD MESSAGE TEXT";
				message.EM_LinkedObject = entry;
				var messageCollection = new NonDependentEDIMessageCollection(Factory) { message };
				var interchangeProvider = new CSWInterchangeProvider(messageCollection);
				interchangeProvider.PackCollatedMessagesIntoInterchanges();
				Factory.Save();

				var interchange = interchangeProvider.Interchanges[0];

				AssertEquals("Length of interchange number.", 20, interchange.EI_InterchangeNum.Length);
			}
		}
	}
}
