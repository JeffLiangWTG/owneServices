using System;
using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.CA.ServiceTasks.Testing
{
	[TestedType(typeof(AVSQueryServiceTask))]
	sealed class AVSQueryServiceTaskTest : ServiceTaskTestCase<AVSQueryServiceTask>
	{
		[ExpectNoExceptions]
		public void TestRunTask()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			var caCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			caCompany1.GC_Code = "CA1";
			caCompany1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			caCompany1.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Canada;
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "AIR";
			caCompany1.GC_OH_OrgProxy = org1.PK;
			var branch1 = caCompany1.Branches.AddNew();
			branch1.GB_Code = "AAA";

			var caCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			caCompany2.GC_Code = "CA2";
			caCompany2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			caCompany2.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Canada;
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "SEA";
			caCompany2.GC_OH_OrgProxy = org2.PK;
			var branch2 = caCompany2.Branches.AddNew();
			branch2.GB_Code = "BBB";

			var caInactiveCompany = Factory.NewWithValidTestData<GlbCompany>();
			caInactiveCompany.GC_Code = "CA3";
			caInactiveCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			caInactiveCompany.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Canada;
			caInactiveCompany.GC_IsActive = false;
			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "INA";
			caInactiveCompany.GC_OH_OrgProxy = org2.PK;
			var branch3 = caInactiveCompany.Branches.AddNew();
			branch3.GB_Code = "III";
			branch3.GB_IsActive = false;

			var usCompany = Factory.NewWithValidTestData<GlbCompany>();
			usCompany.GC_Code = "USA";
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			usCompany.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			var org4 = Factory.New<OrgHeader>();
			org4.OH_Code = "CHI";
			usCompany.GC_OH_OrgProxy = org1.PK;
			var branch4 = usCompany.Branches.AddNew();
			branch4.GB_Code = "CHI";
			Factory.Save();

			var logger = new TestServiceLogger();
			var task = new AVSQueryServiceTask();
			task.ServiceLogger = logger;
			InitialiseTaskSchedule(task);

			using (new User.IsBatchProcessorOverride(Env.CurrentUser))
			{
				using (CACustomsDataRegistry.Instance.AIRSValidationKey.SetTemporaryValue(caCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty, "TEST"))
				{
					RunTaskSchedule(task);
				}
			}

			NUnit.Framework.Assert.That(logger.Count, NUnit.Framework.Is.EqualTo(2), "AIRS Validation Query executing for 2 Companies");
			NUnit.Framework.Assert.That(GetloggerContent(logger[0]), NUnit.Framework.Is.EqualTo(true));
			NUnit.Framework.Assert.That(logger[1], NUnit.Framework.Is.Not.EqualTo(logger[0]));
			NUnit.Framework.Assert.That(GetloggerContent(logger[1]), NUnit.Framework.Is.EqualTo(true));
		}

		[ExpectNoExceptions]
		public void TestCompanyWithoutBranch()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			var caCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			caCompany1.GC_Code = "CA1";
			caCompany1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			caCompany1.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Canada;
			Factory.Save();

			var logger = new TestServiceLogger();
			var task = new AVSQueryServiceTask();
			task.ServiceLogger = logger;
			InitialiseTaskSchedule(task);
			using (new User.IsBatchProcessorOverride(Env.CurrentUser))
			{
				using (CACustomsDataRegistry.Instance.AIRSValidationKey.SetTemporaryValue(caCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty, "TEST"))
				{
					RunTaskSchedule(task);
				}
			}
			NUnit.Framework.Assert.That(ErrorReporter.TotalErrorCount, NUnit.Framework.Is.EqualTo(0));
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"CA Customs CFIA message outbound",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.CFIAQuery,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
				};
			}
		}

		bool GetloggerContent(string logger)
		{
			return logger == "Information|AIRS Validation Query executing for Company CA1." || logger == "Warning|AIRS Validation Key hasn't been setup for Company CA2. The key is allocated by CFIA. Please set it up in Registry -> Customs -> Country or Region Specific -> Canada -> Import -> Declaration -> CFIA -> AIRS Validation Key";
		}
	}
}
