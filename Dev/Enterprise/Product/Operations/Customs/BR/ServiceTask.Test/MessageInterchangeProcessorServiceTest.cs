using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.BR.Business.Testing;
using Enterprise.Customs.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.BR.ServiceTasks.Testing
{
	[TestedType(typeof(MessageInterchangeProcessorService))]
	class MessageInterchangeProcessorServiceTest : ServiceTaskTestCase<MessageInterchangeProcessorService>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "BRI", hostedServiceAttribute.Code);
				AssertEquals("Description", "Brazil Customs Interchange Processor", hostedServiceAttribute.Description);
				AssertEquals("Category", "BRC", hostedServiceAttribute.Category);
				AssertEquals("RequiresCompanyInCountry", Enterprise.Core.Constants.CountryCodes.Brazil, hostedServiceAttribute.RequiresCompanyInCountry);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
				AssertEquals("AllowsMultipleInstances", false, hostedServiceAttribute.AllowsMultipleInstances);
				AssertEquals("MinimumPeriod", "30seconds", hostedServiceAttribute.MinimumPeriod);
			});
		}

		public void TestHostedServiceRequirement()
		{
			var methodInfo = typeof(MessageInterchangeProcessorService).GetMethod(nameof(MessageInterchangeProcessorService.IsRequired));
			Assert("HostedServiceRequirement is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));

			CertificateRequirementChecker.ResetForTesting();
			var isRequired = "";
			AssertNoExceptionThrown(() => isRequired = MessageInterchangeProcessorService.IsRequired());
			AssertEquals("There is no Certificate configured in Brazil.", MessageInterchangeProcessorService.IsRequired());
			GlbExternalPasswordHelperTest.SetupGlbExternalPassword_CCT(Factory);
			CertificateRequirementChecker.ResetForTesting();
			AssertEquals(string.Empty, MessageInterchangeProcessorService.IsRequired());
		}

		public void TestRunTask()
		{
			var caCompany = Factory.NewWithValidTestData<GlbCompany>();
			caCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			var caBranch = Factory.NewWithValidTestData<GlbBranch>();
			caCompany.Branches.Add(caBranch);
			Factory.Save();

			var interchange1 = CreateInterchange(EDIInterchange.ApplicationCodes.BRCustoms, MessageTypeList.Codes.CDE);
			var interchange2 = CreateInterchange(EDIInterchange.ApplicationCodes.BRCustoms, MessageTypeList.Codes.CDC);
			var interchange3 = CreateInterchange(EDIInterchange.ApplicationCodes.BRCustoms, MessageTypeList.Codes.CDI);
			var interchange4 = CreateInterchange(EDIInterchange.ApplicationCodes.BRCustoms, MessageTypeList.Codes.XER);
			var interchange5 = CreateInterchange(EDIInterchange.ApplicationCodes.CACustoms, MessageTypeList.Codes.CDE);
			var interchange6 = CreateInterchange(EDIInterchange.ApplicationCodes.BRCustoms, MessageTypeList.Codes.CDE);
			interchange6.EI_GB = caBranch.PK;
			Factory.Save();

			CombineAssertions(() =>
			{
				InitialiseAndRunTaskSchedule(new MessageInterchangeProcessorService());

				var anotherFactory = new BusinessObjectFactory();
				AssertNotEquals("BRC|CDE processed", EDIInterchange.Status.Queued, anotherFactory.Load<EDIInterchange>(interchange1.PK).EI_Status);
				AssertNotEquals("BRC|CDC processed", EDIInterchange.Status.Queued, anotherFactory.Load<EDIInterchange>(interchange2.PK).EI_Status);
				AssertNotEquals("BRC|CDI processed", EDIInterchange.Status.Queued, anotherFactory.Load<EDIInterchange>(interchange3.PK).EI_Status);
				AssertNotEquals("BRC|XER processed", EDIInterchange.Status.Queued, anotherFactory.Load<EDIInterchange>(interchange4.PK).EI_Status);
				AssertEquals("CAC|CDE not processed", EDIInterchange.Status.Queued, anotherFactory.Load<EDIInterchange>(interchange5.PK).EI_Status);
				AssertNotEquals("BRC|CDE in CA branch processed", EDIInterchange.Status.Queued, anotherFactory.Load<EDIInterchange>(interchange6.PK).EI_Status);
			});
		}

		EDIInterchange CreateInterchange(string applicationCode, string messageType)
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_From = "BRCustoms.TEST";
			interchange.EI_To = "TEST";
			interchange.EI_ApplicationCode = applicationCode;
			interchange.EI_InterchangeType = messageType;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = "TEST";
			return interchange;
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						MessageInterchangeProcessorService.FriendlyName,
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.BRCustoms),
				};
			}
		}
	}
}
