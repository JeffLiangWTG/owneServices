using System;
using CargoWise.Customs.GB.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.GovernmentGateway.NCTS.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Moq;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.NCTS.Testing
{
	public class GBCustomsRequestTest : TestCaseWithFactory
	{
		public void TestDataMapping()
		{
			var enterpriseCode = GBExtensions.GetEnterpriseCode();
			var nctsHeader = SetupHeaderWithCredentials();
			var outboundMessage = nctsHeader.Messages.AddNew();
			outboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCommonTransitConvention;
			outboundMessage.EM_ApplicationReference = "Ref1";
			outboundMessage.EM_MessageSubType = "015";

			var mockMsg = Factory.NewMoq<EDIMessage>();
			mockMsg.Object.EM_LinkedObject = nctsHeader;
			mockMsg.Setup(m => m.EM_MessageSubType).Returns("015");
			mockMsg.Setup(m => m.EM_ApplicationCode).Returns(EDIInterchange.ApplicationCodes.GbCommonTransitConvention);

			var request = GovernmentGatewayExtensions.NewGBCustomsRequest(mockMsg.Object);

			var expectedCredKey = $"{enterpriseCode}.GB123456789000.XXX";

			CombineAssertions(() =>
			{
				AssertEquals("Service", ServiceType.Depart, request.Service);
				AssertEquals("Provider", ProviderType.CTCGB, request.Provider);
				AssertEquals("JobNumber", "NCT00000001", request.JobNumber);
				AssertEquals("Credentials.Key", expectedCredKey, request.Credentials.Key);
				AssertEquals("ServiceReference", "Ref1", request.ServiceReference);
				AssertEquals("ContentType", "XML", request.ContentType);
				AssertEquals("Version", "1.0", request.Version);
			});
			mockMsg.VerifyAll();

			outboundMessage.EM_MessageSubType = ZString.Empty;
			outboundMessage.EM_MessageType = "015";
			outboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCustomsNCTS;
			mockMsg.Setup(m => m.EM_ApplicationCode).Returns(EDIInterchange.ApplicationCodes.GbCustomsNCTS);
			AssertExceptionThrown<NotSupportedException>("Message type is not supported", () => request = GovernmentGatewayExtensions.NewGBCustomsRequest(mockMsg.Object));
			mockMsg.Setup(m => m.EM_MessageType).Returns("015");

			request = GovernmentGatewayExtensions.NewGBCustomsRequest(mockMsg.Object);
			CombineAssertions(() =>
			{
				AssertEquals("Service", ServiceType.Depart, request.Service);
				AssertEquals("Provider", ProviderType.CTCGB, request.Provider);
				AssertEquals("JobNumber", "NCT00000001", request.JobNumber);
				AssertEquals("Credentials.Key", expectedCredKey, request.Credentials.Key);
				AssertEquals("ServiceReference", "Ref1", request.ServiceReference);
				AssertEquals("ContentType", "XML", request.ContentType);
				AssertEquals("Version", "2.0", request.Version);
			});
			mockMsg.VerifyAll();
		}

		public void TestServiceTypes()
		{
			var nctsHeader = SetupHeaderWithCredentials();
			GBCustomsRequest request;
			var gctMockMsg = Factory.NewMoq<EDIMessage>();
			gctMockMsg.Object.EM_LinkedObject = nctsHeader;
			gctMockMsg.Setup(m => m.EM_ApplicationCode).Returns(EDIInterchange.ApplicationCodes.GbCommonTransitConvention);
			gctMockMsg.SetupSequence(m => m.EM_MessageSubType)
				.Returns("015").Returns("015")
				.Returns("014").Returns("014")
				.Returns("007").Returns("007")
				.Returns("044").Returns("044")
				.Returns("013").Returns("013")
				.Returns("170").Returns("170")
				.Returns("BAD").Returns("BAD");

			var gbnMockMsg = Factory.NewMoq<EDIMessage>();
			gbnMockMsg.Object.EM_LinkedObject = nctsHeader;
			gbnMockMsg.Setup(m => m.EM_ApplicationCode).Returns(EDIInterchange.ApplicationCodes.GbCustomsNCTS);
			gbnMockMsg.SetupSequence(m => m.EM_MessageType)
				.Returns("015").Returns("015")
				.Returns("014").Returns("014")
				.Returns("007").Returns("007")
				.Returns("044").Returns("044")
				.Returns("013").Returns("013")
				.Returns("170").Returns("170")
				.Returns("BAD").Returns("BAD");

			Mock<EDIMessage>[] mockMessages = { gctMockMsg, gbnMockMsg };
			foreach (var mockMsg in mockMessages)
			{
				CombineAssertions(() =>
				{
					request = GovernmentGatewayExtensions.NewGBCustomsRequest(mockMsg.Object);
					AssertEquals("Depart 015", ServiceType.Depart, request.Service);
					request = GovernmentGatewayExtensions.NewGBCustomsRequest(mockMsg.Object);
					AssertEquals("UpdateDepart 014", ServiceType.UpdateDepart, request.Service);
					request = GovernmentGatewayExtensions.NewGBCustomsRequest(mockMsg.Object);
					AssertEquals("Arrive 007", ServiceType.Arrive, request.Service);
					request = GovernmentGatewayExtensions.NewGBCustomsRequest(mockMsg.Object);
					AssertEquals("UpdateArrival 044", ServiceType.UpdateArrival, request.Service);
					request = GovernmentGatewayExtensions.NewGBCustomsRequest(mockMsg.Object);
					AssertEquals("Amendment 013", ServiceType.UpdateDepart, request.Service);
					request = GovernmentGatewayExtensions.NewGBCustomsRequest(mockMsg.Object);
					AssertEquals("Pre-Lodged Declaration 170", ServiceType.UpdateDepart, request.Service);

					AssertExceptionThrown<NotSupportedException>("Not supported subtype", () => request = GovernmentGatewayExtensions.NewGBCustomsRequest(mockMsg.Object));
				});
				mockMsg.VerifyAll();
			}
		}

		public void TestNonNctsHeaderMessage()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var mockMsg = Factory.NewMoq<EDIMessage>();
			mockMsg.Object.EM_LinkedObject = dummy;

			var request = GovernmentGatewayExtensions.NewGBCustomsRequest(mockMsg.Object);

			AssertNull("Not a valid NctsHeader", request);
		}

		NctsHeader SetupHeaderWithCredentials()
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_JobReference = "NCT00000001";
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "Ncts001";

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ORG";
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789000", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var aaaBranch = Factory.New<GlbBranch>();
			aaaBranch.GB_Code = "ABC";
			aaaBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			aaaBranch.GB_OH_OrgProxy = org1.PK;
			nctsHeader.Declarant.E2_OA_Address = org1.MainAddress.PK;
			nctsHeader.BH_GB = aaaBranch.PK;

			CTCMessagingTests.CreateCredential(nctsHeader, "GB123456789000", "XXX");

			return nctsHeader;
		}
	}
}
