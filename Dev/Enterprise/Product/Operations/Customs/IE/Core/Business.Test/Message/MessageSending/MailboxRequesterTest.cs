using System;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Business.Testing
{
	class MailboxRequesterTest : TestCaseWithFactory
	{
		public void TestMailboxRequestIsCreated_UseDefaultXml()
		{
			new RefSysConfig.Loader(Factory).Load("IEMAILCOLR", ZDateTime.Now)?.Delete();
			Factory.Save();
			AssertMailboxRequestIsCreated($@"<crq:MailboxCollectRequest xmlns:crq=""http://www.ros.ie/schemas/customs/collectrequest/v1"" />");
		}

		public void TestMailboxRequestIsCreated_UseRefSysConfigXml()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefSysConfigType("IEMAILCOLR", "IE Mailbox Collect Request XML", "IE Mailbox Collect Request XML");
			var xml = "<Greeting>Hello</Greeting>";
			var sysConfig = helper.CreateRefSysConfig("IEMAILCOLR", xml, new ZDateTime(2022, 01, 01), new ZDateTime(2079, 01, 01));
			((INeedRow)sysConfig).Row[RefSysConfig.Schema.ZRC_StringValue] = " " + xml + " "; // we need to start and end spaces to bypass Constraint_ZRC_StringValue
			Factory.Save();
			AssertMailboxRequestIsCreated(xml);
		}

		void AssertMailboxRequestIsCreated(string expectedBodyText)
		{
			var helper = new WebServiceEndPointProviderTestHelper(Factory);
			var url = helper.SetupMailboxCollectURL();
			var ieData = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland);
			var wrapper = GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(ieData.company);
			var certificationData = wrapper.GlbExternalPassword;
			SetupOutgoingInterchange(ieData.branch.PK, certificationData.PK);
			Factory.Save();
			CombineAssertions(() =>
			{
				MailboxRequester.Request(new Logger(), CancellationToken.None);
				var query = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.IECustomsCommon);
				query.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, CommonInterchangeTypeList.Codes.MailboxRequest);
				query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
				query.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT);
				query.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Queued);
				query.AddToFilter(EDIInterchangeSchema.EI_GP, certificationData.PK);
				query.AddToFilter(EDIInterchangeSchema.EI_GB, ieData.branch.PK);
				var requestInterchange = Factory.Load<EDIInterchange>(query).Single();
				AssertEquals("requestInterchange.EI_HeaderText", $@"{{""custom.IE.Endpoint"":""{url}""}}", requestInterchange.EI_HeaderText);
				AssertNotEquals("requestInterchange.EI_SessionGUID", ZGuid.Empty, requestInterchange.EI_SessionGUID);
				AssertMultilineASCIIEquals("requestInterchange.EI_BodyText", expectedBodyText, InterchangeProcessorTestHelper.FormatXml(requestInterchange.EI_BodyText));
			});
		}

		public void TestMailboxRequestIsCreatedForIECompanyWithMessagesInLastNoOfDays()
		{
			var noOfDays = 15;
			using (IECustomsDataRegistry.Instance.MessageProcessingNoOfDays.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, noOfDays))
			{
				var helper = new WebServiceEndPointProviderTestHelper(Factory);
				helper.SetupMailboxCollectURL();
				var ieData1 = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland);
				var wrapper1 = GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(ieData1.company);
				var ieCertificationData1 = wrapper1.GlbExternalPassword;
				var ieData2 = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "I2");
				var wrapper2 = GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(ieData2.company);
				var ieCertificationData2 = wrapper2.GlbExternalPassword;
				var ieData3 = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "I3");
				var wrapper3 = GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(ieData3.company);
				var ieCertificationData3_EMCS = wrapper3.EMCSGlbExternalPasswordCollection.AddNew();
				var ieCertificationData3_IER = wrapper3.GlbExternalPassword;
				var auData = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Australia);
				var auCertificationData = Factory.New<GlbCompanyCredential>();
				auCertificationData.GP_GC = auData.company.PK;
				var ieInterchange1 = SetupOutgoingInterchange(ieData1.branch.PK, ieCertificationData1.PK);
				ieInterchange1.EI_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1 + -noOfDays);
				SetupOutgoingInterchange(ieData2.branch.PK, ieCertificationData2.PK);
				SetupOutgoingInterchange(ieData3.branch.PK, ieCertificationData3_EMCS.PK, EDIInterchange.ApplicationCodes.IECustomsEMCS);
				var auInterchange = SetupOutgoingInterchange(auData.branch.PK, auCertificationData.PK);
				auInterchange.EI_GP = auCertificationData.PK;
				Factory.Save();
				CombineAssertions(() =>
				{
					var logger = new Logger();
					MailboxRequester.Request(logger, CancellationToken.None);
					var query = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, new[] { EDIInterchange.ApplicationCodes.IECustomsCommon, EDIInterchange.ApplicationCodes.IECustomsEMCS });
					query.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, CommonInterchangeTypeList.Codes.MailboxRequest);
					query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
					query.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT);
					query.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Queued);
					var requestInterchanges = Factory.Load<EDIInterchange>(query);
					AssertEquals("Sent interchange with IER credential only", ieCertificationData2.PK, requestInterchanges.Single(ei => ei.EI_GB == ieData2.branch.PK).EI_GP);
					AssertEquals("Sent no interchange with IER credential alongside IEM", 0, requestInterchanges.Where(ei => ei.EI_GB == ieData3.branch.PK && ei.EI_GP == ieCertificationData3_IER.PK).Count());
					AssertEquals("Sent interchange with IEM credential", 1, requestInterchanges.Where(ei => ei.EI_GB == ieData3.branch.PK && ei.EI_GP == ieCertificationData3_EMCS.PK).Count());
					AssertHasMBRCreateLog(logger, requestInterchanges);
				});
			}
		}

		public void TestMailboxRequestIsCreatedForEMCS()
		{
			var helper = new WebServiceEndPointProviderTestHelper(Factory);
			helper.SetupMailboxCollectURL();

			var ieTransactionIDData = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "I1");
			var wrapper1 = GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(ieTransactionIDData.company);
			var ieCertificationData1 = wrapper1.EMCSGlbExternalPasswordCollection.AddNew();
			SetupOutgoingInterchange(ieTransactionIDData.branch.PK, ieCertificationData1.PK, EDIInterchange.ApplicationCodes.IECustomsEMCS, CommonInterchangeTypeList.Codes.TransactionID);

			var ieCommonEMCSData = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "I2");
			var wrapper2 = GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(ieCommonEMCSData.company);
			var ieCertificationData2 = wrapper2.EMCSGlbExternalPasswordCollection.AddNew();
			SetupOutgoingInterchange(ieCommonEMCSData.branch.PK, ieCertificationData2.PK, EDIInterchange.ApplicationCodes.IECustomsEMCS);
			Factory.Save();

			var logger = new Logger();
			MailboxRequester.Request(logger, CancellationToken.None);
			var query = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.IECustomsEMCS);
			query.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, CommonInterchangeTypeList.Codes.MailboxRequest);
			query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
			query.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT);
			query.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Queued);
			var requestInterchanges = Factory.Load<EDIInterchange>(query);
			AssertEquals("Should not sent TransactionID interchange with IEM credential", 1, requestInterchanges.Length);
			AssertEquals("Sent common interchange with IEM credential", ieCommonEMCSData.branch.PK, requestInterchanges.Single().EI_GB);
		}

		public void TestMailboxRequestIsCreatedForNCTS()
		{
			var helper = new WebServiceEndPointProviderTestHelper(Factory);
			var url = helper.SetupMailboxCollectURL();
			var ieData = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland);
			var wrapper = GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(ieData.company);
			var certificationData = wrapper.GlbExternalPassword;
			SetupOutgoingInterchange(ieData.branch.PK, certificationData.PK, EDIInterchange.ApplicationCodes.IECustomsNCTS, "015");
			Factory.Save();
			CombineAssertions(() =>
			{
				var logger = new Logger();
				MailboxRequester.Request(logger, CancellationToken.None);
				var query = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.IECustomsCommon);
				query.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, CommonInterchangeTypeList.Codes.MailboxRequest);
				query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
				query.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT);
				query.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Queued);
				query.AddToFilter(EDIInterchangeSchema.EI_GP, certificationData.PK);
				query.AddToFilter(EDIInterchangeSchema.EI_GB, ieData.branch.PK);
				var requestInterchange = Factory.Load<EDIInterchange>(query).Single();
				AssertEquals("requestInterchange.EI_HeaderText", $@"{{""custom.IE.Endpoint"":""{url}""}}", requestInterchange.EI_HeaderText);
				AssertNotEquals("requestInterchange.EI_SessionGUID", ZGuid.Empty, requestInterchange.EI_SessionGUID);
				AssertHasMBRCreateLog(logger, requestInterchange);
			});
		}

		public void TestMailboxRequestIsCreatedForAISUCC5()
		{
			var helper = new WebServiceEndPointProviderTestHelper(Factory);
			var url = helper.SetupMailboxCollectURL();
			var ieData = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland);
			var wrapper = GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(ieData.company);
			var certificationData = wrapper.GlbExternalPassword;
			SetupOutgoingInterchange(ieData.branch.PK, certificationData.PK, EDIInterchange.ApplicationCodes.IECustomsUCC5Import, "515");
			Factory.Save();
			CombineAssertions(() =>
			{
				var logger = new Logger();
				MailboxRequester.Request(logger, CancellationToken.None);
				var query = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.IECustomsCommon);
				query.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, CommonInterchangeTypeList.Codes.MailboxRequest);
				query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
				query.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT);
				query.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Queued);
				query.AddToFilter(EDIInterchangeSchema.EI_GP, certificationData.PK);
				query.AddToFilter(EDIInterchangeSchema.EI_GB, ieData.branch.PK);
				var requestInterchange = Factory.Load<EDIInterchange>(query).Single();
				AssertEquals("requestInterchange.EI_HeaderText", $@"{{""custom.IE.Endpoint"":""{url}""}}", requestInterchange.EI_HeaderText);
				AssertNotEquals("requestInterchange.EI_SessionGUID", ZGuid.Empty, requestInterchange.EI_SessionGUID);
				AssertHasMBRCreateLog(logger, requestInterchange);
			});
		}

		public void TestMailboxRequestIsCreatedForSpecifiedCredential()
		{
			var helper = new WebServiceEndPointProviderTestHelper(Factory);
			var url = helper.SetupMailboxCollectURL();
			(var company, var branch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland);
			var branch2 = company.Branches.AddNew();
			branch2.GB_Code = "B!@";
			branch2.GB_BranchName = $"TEST IE2 BRANCH";
			branch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			branch2.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			var companyCredential = GlbCompanyWrapper.Get(company).GlbExternalPassword;
			Factory.Save();
			CombineAssertions(() =>
			{
				var interchange = MailboxRequester.RequestForSpecificCredentialPk(companyCredential.PK, branch.PK, false);
				var query = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.IECustomsCommon);
				query.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, CommonInterchangeTypeList.Codes.MailboxRequest);
				query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
				query.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT);
				query.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Queued);
				query.AddToFilter(EDIInterchangeSchema.EI_GP, companyCredential.PK);
				query.AddToFilter(EDIInterchangeSchema.EI_GB, branch.PK);
				var requestInterchange = Factory.Load<EDIInterchange>(query).Single();
				AssertEquals("PK", interchange.PK, requestInterchange.PK);
				AssertEquals("requestInterchange.EI_HeaderText", $@"{{""custom.IE.Endpoint"":""{url}""}}", requestInterchange.EI_HeaderText);
				AssertNotEquals("requestInterchange.EI_SessionGUID", ZGuid.Empty, requestInterchange.EI_SessionGUID);
			});
		}

		public void TestMailboxRequestIsCreatedForSpecifiedCredential_EMCS()
		{
			var helper = new WebServiceEndPointProviderTestHelper(Factory);
			var url = helper.SetupEMCSMailboxCollectURL();
			(var company, var branch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland);
			var companyWrapper = GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(company);
			var emcsCredential = companyWrapper.EMCSGlbExternalPasswordCollection.AddNew();
			var branch2 = company.Branches.AddNew();
			branch2.GB_Code = "B!@";
			branch2.GB_BranchName = $"TEST IE2 BRANCH";
			branch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			branch2.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			Factory.Save();
			CombineAssertions(() =>
			{
				var interchange = MailboxRequester.RequestForSpecificCredentialPk(emcsCredential.PK, branch.PK, true);
				var query = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.IECustomsEMCS);
				query.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, CommonInterchangeTypeList.Codes.MailboxRequest);
				query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
				query.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT);
				query.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Queued);
				query.AddToFilter(EDIInterchangeSchema.EI_GP, emcsCredential.PK);
				query.AddToFilter(EDIInterchangeSchema.EI_GB, branch.PK);
				var requestInterchange = Factory.Load<EDIInterchange>(query).Single();
				AssertEquals("PK", interchange.PK, requestInterchange.PK);
				AssertEquals("requestInterchange.EI_HeaderText", $@"{{""custom.IE.Endpoint"":""{url}""}}", requestInterchange.EI_HeaderText);
				AssertNotEquals("requestInterchange.EI_SessionGUID", ZGuid.Empty, requestInterchange.EI_SessionGUID);
			});
		}

		void AssertHasMBRCreateLog(Logger logger, params EDIInterchange[] requestInterchanges)
		{
			var logs = logger.ToString();
			requestInterchanges.ForEach(ei =>
			{
				AssertContains(string.Format("Created MBR Interchange '{0}'.", ei.EI_InterchangeNum), logs);
			});
		}

		EDIInterchange SetupOutgoingInterchange(ZGuid branchPK, ZGuid certificationPK, string applicationCode = EDIInterchange.ApplicationCodes.IECustomsExport, string interchangeType = AESOutgoingMessageTypeList.Codes.ArrivalAtExit)
		{
			const string messageText = "<GREETING>HELLO</GREETING>";
			var outgoingInterchange = InterchangeCreator.CreateOutgoingInterchange(Factory, applicationCode, interchangeType, branchPK, messageText, "https://www.where.com");
			outgoingInterchange.EI_GP = certificationPK;
			outgoingInterchange.EI_Status = EDIInterchange.Status.Sent;
			return outgoingInterchange;
		}
	}
}
