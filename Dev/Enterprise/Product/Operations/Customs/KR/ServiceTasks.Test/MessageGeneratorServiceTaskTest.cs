using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using Constants = Enterprise.Customs.KR.Messaging.Constants;
using EDIMessage = Enterprise.Customs.KR.Business.EDIMessage;

namespace Enterprise.Customs.KR.ServiceTasks.Testing
{
	[TestedType(typeof(MessageGeneratorServiceTask))]
	sealed class MessageGeneratorServiceTaskTest : ServiceTaskTestCase<MessageGeneratorServiceTask>
	{
		public void TestHostedServiceAttributeParameters()
		{
			HostedServiceAttribute hostedServiceAttribute = GetHostedServiceAttributes().Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "KRD", hostedServiceAttribute.Code);
				AssertEquals("Description", "KR Customs DLT Message Generator", hostedServiceAttribute.Description);
				AssertEquals("Category", "KRC", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "2minutes", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.KoreaSouth, hostedServiceAttribute.RequiresCompanyInCountry);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			});
		}

		public void TestRunTask()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			company1.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.KoreaRepublicOf;
			company1.GC_Code = "KRA";
			company1.GC_IsActive = true;
			var branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "KRB";
			branch1.GB_IsActive = true;

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, "6N002");
			var password1 = Factory.NewWithValidTestData<GlbCompanyCredential>();
			password1.GP_GC = company1.PK;
			password1.GP_PasswordType = "KRB";
			password1.GP_Certificate = new byte[] { 48, 130, 8 };

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			company2.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.KoreaRepublicOf;
			company2.GC_Code = "KRC";
			company2.GC_IsActive = false;

			Factory.Save();

			var logs = InitialiseAndRunTaskSchedule(new MessageGeneratorServiceTask());

			var messageFilter = new ZQuery();
			messageFilter.AddToFilter(EDIMessageSchema.EM_MessageType, Constants.EDIInterchangeType.DLT);
			messageFilter.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.KRCustoms);
			messageFilter.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			var messages = Factory.Load<EDIMessage>(messageFilter);

			AssertEquals("Message Count", 1, messages.Length);
			CombineAssertions("Logs", () =>
			{
				AssertEquals("Log Count", 1, logs.Count);
				AssertEquals($"Log line1", true, logs[0].EndsWith("DLT message has been created successfully."));
			});
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
