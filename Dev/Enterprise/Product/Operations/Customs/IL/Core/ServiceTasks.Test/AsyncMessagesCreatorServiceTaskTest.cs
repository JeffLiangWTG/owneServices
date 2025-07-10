using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.IL.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.IL;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using EDIInterchange = Enterprise.Messaging.Business.EDIInterchange;
using GlbCompanyWrapper = Enterprise.Customs.IL.Business.GlbCompanyWrapper;

namespace Enterprise.Customs.IL.ServiceTasks.Testing
{
	[TestedType(typeof(AsyncMessagesCreatorServiceTask))]
	sealed class AsyncMessagesCreatorServiceTaskTest : ServiceTaskTestCase<AsyncMessagesCreatorServiceTask>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "ILY", hostedServiceAttribute.Code);
				AssertEquals("Description", "IL Async Messages Creator", hostedServiceAttribute.Description);
				AssertEquals("Category", "ILC", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "10minute", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.Israel, hostedServiceAttribute.RequiresCompanyInCountry);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			});
		}

		[TestDate(2024, 09, 09)]
		public void TestAsyncMessageCreation()
		{
			var companies = GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.Israel);
			AssertEquals("PRE-CONDITION: there should be defined 3 IL companies", 3, companies.Length);

			var logger = InitialiseAndRunTaskSchedule(new AsyncMessagesCreatorServiceTask());

			var query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIInterchange.ApplicationCodes.ILCustoms);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, ILMessageTypeList.Codes.GEN);
			query.AddToFilter(EDIMessageSchema.EM_MessageSubType, ILEDIMessageSubTypeList.Codes.SyncOutgoingMessageRequest);
			var messages = Factory.Load<EDIMessage>(query);

			AssertNotNull("Messages were created", messages);
			AssertEquals("There were 3 messages created", 3, messages.Length);
			var logs = logger.ToString();
			AssertContains(@"Information|Company DAN, Generate A-Sync Message - Get all messages by filter Service1 from date 08-Sep-24 23:50:00 to date 09-Sep-24 00:00:00", logs);
			AssertContains(@"Information|Company DAN, Generate A-Sync Message - Get all messages by filter Service2 from date 08-Sep-24 23:50:00 to date 09-Sep-24 00:00:00", logs);
			AssertContains(@"Information|Company EDI, Generate A-Sync Message - Get new messages", logs);
			AssertContains(@"Information|Company VKD is not enabled for Pull A-Sync Message", logs);

			var messageCompany1 = messages.Single(x => x.EM_GC == company1.PK);
			AssertEquals("EM_ApplicationCode", "ILC", messageCompany1.EM_ApplicationCode);
			AssertEquals("EM_Status", "QUE", messageCompany1.EM_Status);
			AssertEquals("EM_ReceiveTransmit", "TRX", messageCompany1.EM_ReceiveTransmit);

			var messageXml = XDocument.Parse(messageCompany1.EM_MessageText);
			var getOptionsNode = messageXml.Descendants().Single(x => x.Name.LocalName == "GetOptions");
			Assert("There should not be any service name contained", getOptionsNode.IsEmpty);
			var senderIDForCompany1 = messageXml.Descendants().Single(x => x.Name.LocalName == "SenderID");
			AssertEquals("Ensure the sender of Company1 is correct.", "560038401", senderIDForCompany1.LastNode.ToString());

			var messagesCompany2 = messages.Where(x => x.EM_GC == company2.PK);
			AssertEquals("There should be 2 messages for company2", 2, messagesCompany2.Count());
			foreach (var messageCompany2 in messagesCompany2)
			{
				AssertEquals("EM_ApplicationCode", "ILC", messageCompany2.EM_ApplicationCode);
				AssertEquals("EM_Status", "QUE", messageCompany2.EM_Status);
				AssertEquals("EM_ReceiveTransmit", "TRX", messageCompany2.EM_ReceiveTransmit);
			}

			var serviceNames = messagesCompany2.Select(m =>
			{
				var messageXml = XDocument.Parse(m.EM_MessageText);
				var getOptionsNode = messageXml.Descendants().Single(x => x.Name.LocalName == "GetOptions");
				var serviceName = getOptionsNode.Descendants().Single(x => x.Name.LocalName == "ServiceName");

				return serviceName.Value;
			}).ToList();
			AssertContainsExactElementsInAnyOrder("Service names must be contained", new string[] { "Service1", "Service2" }, serviceNames);

			var messageCompany2Xml = XDocument.Parse(messagesCompany2.First().EM_MessageText);
			var senderIDForCompany2 = messageCompany2Xml.Descendants().Single(x => x.Name.LocalName == "SenderID");
			AssertEquals("Ensure the sender of Company2 is correct.", "560038402", senderIDForCompany2.LastNode.ToString());
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();

			company1 = GlbCompany.CurrentCompany;
			var companyWrapper = (IILGlbCompanyWrapper)GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(company1);
			var externalPassword = companyWrapper.GetGlbExternalPasswordOrCreateNew();
			externalPassword.GP_MailBoxID = "560038401";
			var factory = externalPassword.Factory;
			var dcaParameters_Company1 = new DCAParameters() { PeekWay = PeekWayList.Codes._2, MaxMessagesPerIteration = 300 };
			disposableResources.Add(ILCustomsDataRegistry.Instance.DCAParametersForASyncMessage.SetTemporaryValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, dcaParameters_Company1));
			disposableResources.Add(ILCustomsDataRegistry.Instance.EnablePullASyncMessage.SetTemporaryValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, true));

			company2 = factory.NewWithValidTestData<GlbCompany>();
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Israel;
			var company2_Branch = company2.Branches.AddNew();
			company2_Branch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Israel)).RL_Code;
			var companyWrapper2 = (IILGlbCompanyWrapper)GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(company2);
			var externalPassword2 = companyWrapper2.GetGlbExternalPasswordOrCreateNew();
			externalPassword2.GP_MailBoxID = "560038402";

			var dcaParameters_Company2 = new DCAParameters() { PeekWay = PeekWayList.Codes._3, MaxMessagesPerIteration = 400 };
			dcaParameters_Company2.Services.Add(new DCAService() { Name = "Service1" });
			dcaParameters_Company2.Services.Add(new DCAService() { Name = "Service2" });

			disposableResources.Add(ILCustomsDataRegistry.Instance.DCAParametersForASyncMessage.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, dcaParameters_Company2));
			disposableResources.Add(ILCustomsDataRegistry.Instance.EnablePullASyncMessage.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, true));

			var company3PullAsyncMessageNotEnabled = factory.New<GlbCompany>();
			company3PullAsyncMessageNotEnabled.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Israel;
			var company3_Branch = company3PullAsyncMessageNotEnabled.Branches.AddNew();
			company3_Branch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Israel)).RL_Code;
			company3_Branch.GB_Code = "ABC";
			var companyWrapper3 = (IILGlbCompanyWrapper)GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(company3PullAsyncMessageNotEnabled);
			var externalPassword3 = companyWrapper3.GetGlbExternalPasswordOrCreateNew();
			externalPassword3.GP_MailBoxID = "560038403";

			var dcaParameters_company3 = new DCAParameters() { PeekWay = PeekWayList.Codes._3, MaxMessagesPerIteration = 400 };
			dcaParameters_company3.Services.Add(new DCAService() { Name = "Service1" });
			dcaParameters_company3.Services.Add(new DCAService() { Name = "Service2" });
			disposableResources.Add(ILCustomsDataRegistry.Instance.DCAParametersForASyncMessage.SetTemporaryValue(company3PullAsyncMessageNotEnabled.PK.ToGuid(), Guid.Empty, Guid.Empty, dcaParameters_company3));

			factory.Save();
		}

		protected override void TearDownCore()
		{
			base.TearDownCore();

			foreach (var tempValue in disposableResources)
			{
				tempValue?.Dispose();
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		readonly List<IDisposable> disposableResources = new List<IDisposable>();
		GlbCompany company1;
		GlbCompany company2;
	}
}
