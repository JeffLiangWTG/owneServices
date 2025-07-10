using System;
using CargoWise.Application;
using CargoWise.Customs.GB.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Customs.GB.EMCS.Business;
using Enterprise.Customs.GB.EMCS.Messaging;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.EMCS.ServiceTasks.Testing
{
	public class EMCSExtensionsTest : TestCaseWithFactory
	{
		public void TestNewGBCustomsRequestNew()
		{
			TestDataHelper.CreateCredentials(Factory, GlbCompany.CurrentCompany.PK, "EMC", "12345123451234", PasswordTypesList.Codes.CDS);
			var declaration = EMCSMessageSenderTestHelper.CreateDeclaration(Factory, GlbBranch.CurrentBranch.PK.ToGuid());
			declaration.JE_CustomsProfile = "";
			var message = EMCSMessageSenderTestHelper.CreateAndPopulateMessage(Factory, declaration, EMCSGBOutgoingMessageTypeList.Codes.SubmitDraftEAD);
			var req = EMCSExtensions.NewGBCustomsRequest(message);
			AssertNull(req);

			declaration.JE_CustomsProfile = "EDIDAT.12345123451234.EMC";
			message = EMCSMessageSenderTestHelper.CreateAndPopulateMessage(Factory, declaration, EMCSGBOutgoingMessageTypeList.Codes.SubmitDraftEAD);
			req = EMCSExtensions.NewGBCustomsRequest(message);

			CombineAssertions(() =>
			{
				AssertEquals(ProviderType.EMCS, req.Provider);
				AssertEquals(ServiceType.Consignor, req.Service);
				AssertEquals("XML", req.ContentType);
				AssertEquals("1.0", req.Version);
			});
		}

		public void TestNewGBCustomsRequestAmendment()
		{
			TestDataHelper.CreateCredentials(Factory, GlbCompany.CurrentCompany.PK, "EMC", "12345123451234", PasswordTypesList.Codes.CDS);
			var declaration = EMCSMessageSenderTestHelper.CreateDeclaration(Factory, GlbBranch.CurrentBranch.PK.ToGuid());
			declaration.JE_CustomsProfile = "EDIDAT.12345123451234.EMC";
			var message = EMCSMessageSenderTestHelper.CreateAndPopulateMessage(Factory, declaration, EMCSGBOutgoingMessageTypeList.Codes.ChangeOfDestination);
			var req = EMCSExtensions.NewGBCustomsRequest(message);

			CombineAssertions(() =>
			{
				AssertEquals(ProviderType.EMCS, req.Provider);
				AssertEquals(ServiceType.Consignor, req.Service);
				AssertEquals("XML", req.ContentType);
				AssertEquals("1.0", req.Version);
			});
		}

		public void TestNewGBCustomsRequestWithNoLinkedObject()
		{
			var message = Factory.New<EMCSOutboundEDIMessage>();
			var req = EMCSExtensions.NewGBCustomsRequest(message);
			AssertNull(req);
		}

		public void TestNewGBCustomsRequestWithNoCredential()
		{
			var declaration = EMCSMessageSenderTestHelper.CreateDeclaration(Factory, GlbBranch.CurrentBranch.PK.ToGuid(), true);
			var message = EMCSMessageSenderTestHelper.CreateAndPopulateMessage(Factory, declaration, EMCSGBOutgoingMessageTypeList.Codes.ChangeOfDestination);
			var req = EMCSExtensions.NewGBCustomsRequest(message);
			AssertNull(req);
		}

		public void TestNewGBCustomsRequestServiceReference()
		{
			TestDataHelper.CreateCredentials(Factory, GlbCompany.CurrentCompany.PK, "EMC", "12345123451234", PasswordTypesList.Codes.CDS);
			var declaration = EMCSMessageSenderTestHelper.CreateDeclaration(Factory, GlbBranch.CurrentBranch.PK.ToGuid());
			declaration.JE_CustomsProfile = "EDIDAT.12345123451234.EMC";
			var message = EMCSMessageSenderTestHelper.CreateAndPopulateMessage(Factory, declaration, EMCSGBOutgoingMessageTypeList.Codes.SubmitDraftEAD);
			declaration.Messages.Add(message);
			var req = EMCSExtensions.NewGBCustomsRequest(message);
			AssertEquals(string.Empty, req.ServiceReference);

			message.EM_Status = EDIMessage.Status.Acknowledged;
			message.EM_ApplicationReference = "APPLICATIONREFERENCEVALUE";
			var message1 = EMCSMessageSenderTestHelper.CreateAndPopulateMessage(Factory, declaration, EMCSGBOutgoingMessageTypeList.Codes.SubmitDraftEAD);
			declaration.Messages.Add(message1);
			req = EMCSExtensions.NewGBCustomsRequest(message1);
			AssertEquals("APPLICATIONREFERENCEVALUE", req.ServiceReference);
		}

		public void TestServiceTypeMappings()
		{
			var message = Factory.New<EMCSOutboundEDIMessage>();
			message.EM_MessageType = EMCSGBOutgoingMessageTypeList.Codes.CancellationOfEAD;
			AssertEquals(ServiceType.Consignor, EMCSExtensions.GetServiceType(message));
			message.EM_MessageType = EMCSGBOutgoingMessageTypeList.Codes.ChangeOfDestination;
			AssertEquals(ServiceType.Consignor, EMCSExtensions.GetServiceType(message));
			message.EM_MessageType = EMCSGBOutgoingMessageTypeList.Codes.SubmitDraftEAD;
			AssertEquals(ServiceType.Consignor, EMCSExtensions.GetServiceType(message));
			message.EM_MessageType = EMCSGBOutgoingMessageTypeList.Codes.ReportOfReceipt;
			AssertEquals(ServiceType.Consignee, EMCSExtensions.GetServiceType(message));
			message.EM_MessageType = EMCSGBOutgoingMessageTypeList.Codes.AlertOrRejectionOfAnEAD;
			AssertEquals(ServiceType.Consignee, EMCSExtensions.GetServiceType(message));
			message.EM_MessageType = EMCSGBOutgoingMessageTypeList.Codes.Splitting;
			AssertEquals(ServiceType.IE825, EMCSExtensions.GetServiceType(message));
			AssertExplanationServiceType(EMCSGBOutgoingMessageTypeList.Codes.ExplanationOnDelayForDelivery, ServiceType.IE837);
			AssertExplanationServiceType(EMCSGBOutgoingMessageTypeList.Codes.ExplanationOnReasonForShortage, ServiceType.IE871);
			message.EM_MessageType = EMCSGBOutgoingMessageTypeList.Codes.PreValidateTrader;
			AssertEquals(ServiceType.PVT, EMCSExtensions.GetServiceType(message));
			message.EM_MessageType = "xxx";
			AssertExceptionThrown(typeof(NotSupportedException), "Message type xxx is not supported.", () => EMCSExtensions.GetServiceType(message));
		}

		void AssertExplanationServiceType(string messageType, ServiceType serviceType)
		{
			CombineAssertions(() =>
			{
				var message = Factory.New<EMCSOutboundEDIMessage>();
				message.EM_MessageType = messageType;
				AssertEquals(serviceType, EMCSExtensions.GetServiceType(message));

				var declaration = Factory.New<Business.EMCSJobDeclaration>();
				declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
				message.EM_LinkedObject = declaration;
				AssertEquals(ServiceType.Consignee, EMCSExtensions.GetServiceType(message));

				declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
				AssertEquals(ServiceType.Consignor, EMCSExtensions.GetServiceType(message));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "HYE";
			registrationKey.ServerCodeForTest = "CMT";
		}
	}
}
