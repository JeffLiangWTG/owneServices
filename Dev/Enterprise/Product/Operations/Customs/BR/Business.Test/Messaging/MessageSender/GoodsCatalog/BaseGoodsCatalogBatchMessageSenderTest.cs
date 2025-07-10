using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	public abstract class BaseGoodsCatalogBatchMessageSenderTest : TestCaseWithFactory
	{
		public void TestSendMessagesAndInterchange()
		{
			var catalogs = CreateGoodsCatalogs(Factory, CreateOwner(Factory));
			using (BRCustomsDataRegistry.Instance.MaxNumberOfRowsInProductCatalogMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				var log = new DummyOperationalActionSectionLog();
				CreateMessageSender(catalogs, log).SendMessagesAndInterchange();

				CombineAssertions(() =>
				{
					AssertMultilineASCIIEquals(GetExpectedSendingLog(), log.MessagesString());

					foreach (var catalog in catalogs.Where(x => x.Messages.Count > 0))
					{
						AssertEquals("1 message should be sent", 1, catalog.Messages.Count);
						AssertMessageGenerated(catalog.Messages[0], EDIMessageSubTypeList.Codes.Original);
						AssertEquals("Log added with CatalogAction ", SendAction, catalog.Logs.MostRecentLogByEventTime(Events.MessageSent).SL_Reference);
					}
				});

				void AssertMessageGenerated(EDIMessage message, string messageSubType)
				{
					AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.BRCustoms, message.EM_ApplicationCode);
					AssertEquals("EM_MessageType", MessageTypeList.Codes.CAT, message.EM_MessageType);
					AssertEquals("EM_MessageSubType", messageSubType, message.EM_MessageSubType);
					AssertEquals("EM_ReceiveTransmit", EDIInterchange.Direction.Transmit, message.EM_ReceiveTransmit);
					AssertEquals("EM_GP", BRGlbStaffWrapper.Get(GlbStaff.CurrentUser).CCTPassword.PK, message.EM_GP);

					if (messageSubType == EDIMessageSubTypeList.Codes.Original)
					{
						AssertContains("EM_MessageText", $"\"situacao\": \"{ExpectedCatalogSituation}\"", message.EM_MessageText);
						AssertLessThanOrEqualTo("EM_MessageNum", int.Parse(message.EM_MessageNum), 2);
					}
				}
			}
		}

		public string GetExpectedSendingLog() => ExpectedSendingLog + ($"\r\nINFO: {ExpectedCountOfGeneratedMessages} Message(s) have been generated and packed into {ExpectedCountOfGeneratedInterchanges} Interchange(s).");

		public void TestSendMessagesAndInterchange_InvalidCertificate()
		{
			var catalogs = CreateGoodsCatalogs(Factory, null);
			using (BRCustomsDataRegistry.Instance.MaxNumberOfRowsInProductCatalogMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				var log = new DummyOperationalActionSectionLog();
				CreateMessageSender(catalogs, log).SendMessagesAndInterchange();

				var expectedSendingLog = string.Join("\r\n", ExpectedSendingLog.Split(new[] { "\r\n" }, StringSplitOptions.None).Select(x =>
						x.EndsWith("has been sent.") ? x.Replace("INFO", "WARNING").Replace("has been sent.",
						"has not been sent because the digital certificate for the Catalog Staff mentioned in the Consignee is missing, expired, or invalid.") : x))
					+ ("\r\nINFO: 0 Message(s) have been generated and packed into 0 Interchange(s).");
				
				AssertMultilineASCIIEquals(expectedSendingLog, log.MessagesString());
			}
		}

		protected abstract BaseGoodsCatalogBatchMessageSender CreateMessageSender(IEnumerable<CusGoodsCatalog> goodsCatalogs, IOperationalActionSectionLog log);

		protected abstract string SendAction { get; }

		protected abstract string ExpectedSendingLog { get; }

		protected virtual int ExpectedCountOfGeneratedMessages => 3;

		protected virtual int ExpectedCountOfGeneratedInterchanges => 2;

		protected abstract string ExpectedCatalogSituation { get; }

		public static OrgHeader CreateOwner(BusinessObjectFactory factory)
		{
			var staff = factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			var password = BRGlbStaffWrapper.Get(staff).CCTPassword;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(10);
			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;

			var owner = factory.NewWithValidTestData<OrgHeader>();
			BROrgImpAddInfo.Get(owner).ZO_BrokerCode = staff.GS_Code;
			factory.Save();

			return owner;
		}

		public static IEnumerable<CusGoodsCatalog> CreateGoodsCatalogs(BusinessObjectFactory factory, OrgHeader owner)
		{
			var catalogs = new List<CusGoodsCatalog>();

			foreach (ZString authorityStatus in new GoodsCatalogStatusTypeList().GetAllCodes().Append(string.Empty))
			{
				foreach (ZString messageStatus in new[] { BRMessageStatusList.Codes.NotSent, BRMessageStatusList.Codes.Accepted })
				{
					for (var i = 1; i <= 3; i++)
					{
						var catalog = factory.NewWithValidTestData<CusGoodsCatalog>();
						catalog.CGC_AuthorityStatus = authorityStatus;
						catalog.CGC_MessageStatus = messageStatus;
						catalog.CGC_CatalogCode = $"TC_{(authorityStatus.IsEmpty ? "X" : authorityStatus)}_{(messageStatus.IsEmpty ? "NST" : messageStatus)}_{i}";
						catalog.ForeignOperators.AddNew();
						catalog.ForeignOperators.AddNew();
						catalog.CGC_OH_Owner = owner?.PK ?? ZGuid.Empty;

						catalogs.Add(catalog);
					}
				}
			}

			return catalogs;
		}
	}
}
