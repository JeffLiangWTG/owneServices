using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static NUnit.Framework.XmlAssertions;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class TransitWarehouseEventSyncServiceTest : TestCaseWithFactory
	{
		[TestDate(2021, 10, 01)]
		public void TestSendUniversalEventAfterSaved()
		{
			var factory = NewFactory();
			var mawb = factory.New<CusMAWB>();
			mawb.CM_MAWB = "MB20211001";

			var hawb = mawb.ChildBills.AddNew();
			hawb.FillWithValidTestData();

			hawb.CS_HAWB = "HB20211001";

			hawb.RegisterSyncEvent(mawb.CM_MAWB, hawb.CS_HAWB);
			hawb.CS_CustomsStatus = "CLR";

			factory.Save();

			var hawbInNewFactory = NewFactory().Load<CusHAWB>(hawb.PK);
			AssertHasSentUEvent(hawbInNewFactory, true, "Should publish universal event from the house bill as the status is changed.", mawb.CM_MAWB, hawb.CS_HAWB);

			var dataExportFailureLog = hawbInNewFactory.Logs.Find((l) => l.SL_SE_NKEvent == Events.DataExportFailureCode).Single();

			AssertNotNull("Should create a Data Export Failure Log as there is no matched consignment data.", dataExportFailureLog);
			AssertEquals("Should log the failure reason and source customs status.", "|RES=Warning - No Module found a Business Entity to link this Universal Event to.|STA=CLR|TYP=CES", dataExportFailureLog.SL_Reference);
		}

		void AssertHasSentUEvent(IStmALogParent logParent, bool expectedValue, string message, string mawbValue, string hawbValue)
		{
			var exportLog = logParent.Logs.Find(c => c.SL_SE_NKEvent == AutoEvents.DataExportCode).FirstOrDefault();
			AssertEquals(message, expectedValue, exportLog != null);

			if (exportLog != null)
			{
				var query = new ZQuery();
				query.AddToFilter(GenPivotSchema.XX_Relation1TableCode, StmALogSchema.Constants.Prefix);
				query.AddToFilter(GenPivotSchema.XX_Relation1ID, exportLog.PK);
				query.AddToFilter(GenPivotSchema.XX_RelationType, Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage);

				var pivot = NewFactory().LoadTop1<GenPivot>(query);
				AssertNotNull(message, pivot);

				var ediMessage = Factory.Load<EDIMessage>(pivot.XX_Relation2ID);
				AssertEquals("Should create an UDM message.", ApplicationCodeList.Codes.UniversalDataMessaging, ediMessage.EM_ApplicationCode);
				AssertEquals("The message type should be internal for processing directly.", ReceiveTransmitList.Codes.Internal, ediMessage.EM_ReceiveTransmit);

				AssertIsXml("Should built an expected xml with correct recipient role details, customs status and bill numbers.", ediMessage.EM_FormattedMessageText)
					.HavingExactlyOneChildNode("Event/ContextCollection/Context",
						node => node.HavingAtLeastOneChildNode(child =>
							child.WithName("Type")
								 .WithValue("MAWBNumber")
						)
						.HavingAtLeastOneChildNode(child =>
							child.WithName("Value")
								 .WithValue(mawbValue)
						)
					).HavingExactlyOneChildNode("Event/ContextCollection/Context",
						node => node.HavingAtLeastOneChildNode(child =>
							child.WithName("Type")
								 .WithValue("HAWBNumber")
						)
						.HavingAtLeastOneChildNode(child =>
							child.WithName("Value")
								 .WithValue(hawbValue)
						)
					).HavingExactlyOneChildNode("Event/EventType",
						node => node.WithValue("CES")
					).HavingExactlyOneChildNode("Event/DataContext/RecipientRoleCollection/RecipientRole",
						node => node.HavingAtLeastOneChildNode(child =>
							child.WithName("Code")
								 .WithValue("ATW")
						)
						.HavingAtLeastOneChildNode(child =>
							child.WithName("ServiceCode")
								 .WithValue("TWR")
						)
					);
			}
		}
	}
}
