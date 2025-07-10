using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class NEXDOCMessageSenderTest : TestCaseWithFactory
	{
		public void TestIsNEXDOCSActive()
		{
			var sender = new NEXDOCMessageSender(quarantineHeader, NEXDOCMessageType.Codes.REXForward);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			{
				Assert("Should be false as the NEXDOC is not active.", !sender.SendMessage());
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				Assert("Should be true as the NEXDOC is active.", sender.SendMessage());
			}
		}

		public void TestSendMessage()
		{
			var helper = new ZTestHelper(Factory);

			void AssertSendMessage(string messageType, string declarationReference)
			{
				helper.PopulateSimpleQuarantineDeclaration(declarationReference);

				var declaration = helper.Declaration;

				var header = helper.Header1.QuarantineExDocHeader;
				header.QH_RequestForPermitNumber = "TEST000" + messageType;
				header.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;

				var sender = new NEXDOCMessageSender(header, messageType);

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
				using (AUCustomsDataRegistry.Instance.NEXDOCSTestingSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					Assert("Should be true as the NEXDOC is active.", sender.SendMessage());

					var message = NewFactory()
						.Load<QuarantineExDocHeader>(header.PK)
						.Messages
						.Cast<EDIMessage>()
						.FirstOrDefault(c => c.EM_ApplicationCode == ApplicationCodeList.Codes.AUCustomsNEXDOC);

					var interchange = message.Interchange;

					CombineAssertions(() =>
					{
						AssertEquals("EI_InterchangeType", ApplicationCodeList.Codes.AUCustomsNEXDOC, interchange.EI_InterchangeType);
						AssertEquals("EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
						AssertEquals("EI_From", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
						AssertEquals("EI_To", "NEXDOCSTest", interchange.EI_To);
						AssertEquals("EI_Status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
						AssertEquals("EI_GB", quarantineHeader.InvoiceHeader.JobDeclaration.JE_GB, interchange.EI_GB);

						AssertXMLEquals("EI_BodyText", GetExpectedBodyText(messageType), interchange.EI_BodyText);

						AssertEquals("ContainedMessages.Count", 1, interchange.ContainedMessages.Count);

						Assert("declaration.IsInDatabase", declaration.IsInDatabase);
						AssertEquals("declaration.JE_MessageStatus", RFPMessage.Status.AwaitingResponse, declaration.JE_MessageStatus);
					});
				}
			}

			AssertSendMessage(NEXDOCMessageType.Codes.REXForward, "B0000002");
			AssertSendMessage(NEXDOCMessageType.Codes.REXTransfer, "B0000003");
			AssertSendMessage(NEXDOCMessageType.Codes.WithdrawalOwnership, "B0000004");
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();

			quarantineHeader = helper.Header1.QuarantineExDocHeader;
			quarantineHeader.QH_RequestForPermitNumber = "TEST000";
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
		}
		QuarantineExDocHeader quarantineHeader;

		string GetExpectedBodyText(string messageType)
		{
			var fileName = "";
			switch (messageType)
			{
				case NEXDOCMessageType.Codes.REXForward:
					fileName = "RexForwardInterchangeBodyText.xml";
					break;
				case NEXDOCMessageType.Codes.REXTransfer:
					fileName = "RexTransferInterchangeBodyText.xml";
					break;
				case NEXDOCMessageType.Codes.WithdrawalOwnership:
					fileName = "RexWithdrawnInterchangeBodyText.xml";
					break;
			}

			return new EmbeddedResourceRetriever().GetString("Enterprise.Customs.AU.Declaration.Business.Testing.MessageBuilders.NEXDOC.TestFiles." + fileName)
				.Replace("@Sender", GlbCompany.CurrentCompany.LicenceKeyIdentifier)
				.Replace("@User", StaticCurrentFetcher.Instance.CurrentUserCode);
		}
	}
}
