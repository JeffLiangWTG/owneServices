using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageBuilders;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	public abstract class ILMessageBuilderBaseTest : TestCaseWithFactory
	{
		[TestDate(2024, 09, 09)]
		public void TestPopulateMessages()
		{
			var credential = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(currentCompany).GlbExternalPassword;
			credential.GP_MailBoxID = "560038416";

			var expectedMessageText = GetExpectedMessageText();
			var messageBuilder = GetMessageBuilder();
			var result = messageBuilder.PopulateMessages();
			Factory.Save();

			CombineAssertions("When PopulateMessages", () =>
			{
				AssertNotNull(result);
				Assert("Success", result.IsSuccess);

				var linkedObject = GetLinkedObject();
				var query = new ZQuery();

				if (linkedObject != null)
				{
					query.AddToFilter(EDIMessageSchema.EM_LinkTable, linkedObject.TableName);
					query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, linkedObject.PK);
				}
				else
				{
					query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ILEDIInterchange.ApplicationCodes.ILCustoms);
					query.AddToFilter(EDIMessageSchema.EM_MessageType, GetExpectedMessageType());
					query.AddToFilter(EDIMessageSchema.EM_MessageSubType, GetExpectedMessageSubType());
				}

				var message = Factory.LoadTop1<EDIMessage>(query);

				AssertNotNull("Message was created", message);
				AssertEquals("EM_ApplicationCode", "ILC", message.EM_ApplicationCode);
				AssertEquals("EM_MessageType", GetExpectedMessageType(), message.EM_MessageType);
				AssertEquals("EM_MessageSubType", GetExpectedMessageSubType(), message.EM_MessageSubType);
				AssertEquals("EM_Status", "QUE", message.EM_Status);
				AssertEquals("EM_ReceiveTransmit", "TRX", message.EM_ReceiveTransmit);

				var messageOwnerCollection = GetMessageOwnerCollection();
				if (messageOwnerCollection != null)
				{
					AssertEquals("the message connected to parent", 1, messageOwnerCollection.Count);
				}
				var actualValue = EliminateMeaninglessDiffs(message.EM_MessageText.ToString());
				AssertEquals("EM_MessageText", expectedMessageText, actualValue);
				AssertEquals("EM_GP", Guid.Empty, message.EM_GP);
				AssertEquals("EM_MessageOwner", "ILCOM_REGISTERNO", message.EM_MessageOwner);
			});
		}

		string EliminateMeaninglessDiffs(string xmlContent)
		{
			return xmlContent.Replace("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"");
		}

		protected override BusinessObjectFactory NewFactory() => GlbCompany.CurrentCompany.Factory;

		protected abstract string GetExpectedMessageText();

		protected abstract IMessageBuilder GetMessageBuilder();

		protected abstract string GetExpectedMessageType();

		protected abstract string GetExpectedMessageSubType();

		protected abstract BusinessObject GetLinkedObject();

		protected abstract IBusinessObjectCollection GetMessageOwnerCollection();

		protected override void SetUp()
		{
			base.SetUp();

			currentCompany = GlbCompany.CurrentCompany;
			currentCustomsRegistryNo = currentCompany.GC_CustomsRegistrationNo;
			currentCompany.GC_CustomsRegistrationNo = "ILCOM_REGISTERNO";
			disposable = ILCustomsDataRegistry.Instance.SendILCustomsMessagesWithoutDigitalSignature.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDown()
		{
			currentCompany.GC_CustomsRegistrationNo = currentCustomsRegistryNo;
			disposable.Dispose();
			base.TearDown();
		}

		protected GlbCompany currentCompany;
		ZString currentCustomsRegistryNo;
		IDisposable disposable;
	}
}
