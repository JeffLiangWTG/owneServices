using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.CA.Business.BatchProcessor;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using WTG.NUnit;

namespace Enterprise.Customs.CA.ServiceTasks.Testing
{
	[TestedType(typeof(EHubInterchangeProcessorServiceTask))]
	sealed class EHubInterchangeProcessorServiceTaskTest : ServiceTaskTestCase<EHubInterchangeProcessorServiceTask>
	{
		[ExpectNoExceptions]
		public void TestRunInAnyBranch()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			company.GC_Code = "Z1Z";
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "Z1Z";
			branch.GB_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			var interchangeToProcess = CreateInterchange(branch.PK);
			NUnit.Framework.Assert.That(interchangeToProcess.EI_Status, NUnit.Framework.Is.EqualTo(EDIInterchange.Status.Queued).Using(CustomComparers.TypeComparison), "Status is Queued");

			using (Env.Instance.TemporaryServiceTaskContext(EHubInterchangeProcessorServiceTask.InterchangeProcessorServiceCode, canRunInAnyBranch: true))
			{
				var serviceTask = new EHubInterchangeProcessorServiceTask();
				serviceTask.ServiceLogger = new TestServiceLogger();
				serviceTask.RunTask();
			}

			interchangeToProcess.Reload();
			NUnit.Framework.Assert.That(interchangeToProcess.EI_Status, NUnit.Framework.Is.EqualTo(EDIInterchange.Status.Received).Using(CustomComparers.TypeComparison), "Status is updated");
		}

		[ExpectNoExceptions]
		public void TestRunInConfiguredBranch()
		{
			var configuredCompany = Factory.NewWithValidTestData<GlbCompany>();
			configuredCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			configuredCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			configuredCompany.GC_Code = "ZCA";
			var configuredBranch = configuredCompany.Branches.AddNew();
			configuredBranch.GB_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			configuredBranch.GB_Code = "ZCA";

			var caCompany = Factory.NewWithValidTestData<GlbCompany>();
			caCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			caCompany.GC_Code = "Z1Z";
			caCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var caBranch = caCompany.Branches.AddNew();
			caBranch.GB_Code = "Z1Z";
			caBranch.GB_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			CACustomsDataRegistry.Instance.DefaultBranchForServiceTasks.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configuredBranch.PK.ToGuid());
			Factory.Save();

			var interchangeToProcess = CreateInterchange(caBranch.PK);
			NUnit.Framework.Assert.That(interchangeToProcess.EI_Status, NUnit.Framework.Is.EqualTo(EDIInterchange.Status.Queued).Using(CustomComparers.TypeComparison), "Status is Queued");

			using (Env.Instance.TemporaryServiceTaskContext(EHubInterchangeProcessorServiceTask.InterchangeProcessorServiceCode, canRunInAnyBranch: true))
			{
				var serviceTask = new EHubInterchangeProcessorServiceTask();
				serviceTask.ServiceLogger = new TestServiceLogger();
				serviceTask.RunTask();

				NUnit.Framework.Assert.That(serviceTask.LastTaskBranchPKForTesting, NUnit.Framework.Is.EqualTo(configuredBranch.PK), "Task branch is Configured Branch");
			}

			interchangeToProcess.Reload();
			NUnit.Framework.Assert.That(interchangeToProcess.EI_Status, NUnit.Framework.Is.EqualTo(EDIInterchange.Status.Received).Using(CustomComparers.TypeComparison), "Status is updated");
		}

		[ExpectNoExceptions]
		public void TestRunInOtherContext()
		{
			var caCompany = Factory.NewWithValidTestData<GlbCompany>();
			caCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			caCompany.GC_Code = "Z1Z";
			caCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var caBranch = caCompany.Branches.AddNew();
			caBranch.GB_Code = "Z1Z";
			caBranch.GB_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			var usCompany = Factory.NewWithValidTestData<GlbCompany>();
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			usCompany.GC_Code = "ZUS";
			usCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var usBranch = usCompany.Branches.AddNew();
			usBranch.GB_Code = "ZUS";
			usBranch.GB_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			usBranch.GB_IsActive = true;
			Factory.Save();

			var usInterchangeToProcess = CreateInterchange(usBranch.PK);
			NUnit.Framework.Assert.That(usInterchangeToProcess.EI_Status, NUnit.Framework.Is.EqualTo(EDIInterchange.Status.Queued).Using(CustomComparers.TypeComparison), "Status is Queued");

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				using (Env.Instance.TemporaryServiceTaskContext(EHubInterchangeProcessorServiceTask.InterchangeProcessorServiceCode, canRunInAnyBranch: true))
				{
					var serviceTask = new EHubInterchangeProcessorServiceTask();
					serviceTask.ServiceLogger = new TestServiceLogger();
					serviceTask.RunTask();

					NUnit.Framework.Assert.That(serviceTask.LastTaskBranchPKForTesting, NUnit.Framework.Is.EqualTo(caBranch.PK), "Task branch is CA Branch");
				}

				usInterchangeToProcess.Reload();
				NUnit.Framework.Assert.That(usInterchangeToProcess.EI_Status, NUnit.Framework.Is.EqualTo(EDIInterchange.Status.Received).Using(CustomComparers.TypeComparison), "Status is updated");
			}
		}

		[ExpectNoExceptions]
		public void TestCheckCompanyInCanadaOrAppliesAllCountries()
		{
			company = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "EDI");
			AssertCheckCompanyInCanadaOrAppliesAllCountries(false, false, "There is no company in Canada, and the registry setting 'Customs -> Country or Region Specific -> Canada -> Service Tasks -> Run CA Messaging Service Tasks?' has not been enabled.");
			AssertCheckCompanyInCanadaOrAppliesAllCountries(true, true, string.Empty);
			AssertCheckCompanyInCanadaOrAppliesAllCountries(true, false, string.Empty);
			AssertCheckCompanyInCanadaOrAppliesAllCountries(false, true, string.Empty);
		}

		[ExpectNoExceptions]
		public void TestCheckSendCAViaEHub()
		{
			company = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "EDI");
			AssertCheckSendCAViaEHub(true, string.Empty);
			AssertCheckSendCAViaEHub(false, "The registry setting 'eServices -> Customs Service Bureau -> CA -> Send Via eHub' requires a value other than 'False'.");
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"CA Customs ACI Interchanges inbound",
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.CAACI),

					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"CA Customs EXP Interchanges inbound",
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.CAEXP),

					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"CA Customs IMP Interchanges inbound",
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.CAIMP),

					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"CA Customs Interchanges inbound",
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.CACustoms),
				};
			}
		}

		EDIInterchange CreateInterchange(ZGuid branchPK)
		{
			var interchangeToProcess = Factory.New<EDIInterchange>();
			interchangeToProcess.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchangeToProcess.EI_ApplicationCode = EDIInterchange.ApplicationCodes.CACustoms;
			interchangeToProcess.EI_InterchangeType = EDIInterchange.ApplicationCodes.CACustoms;
			interchangeToProcess.EI_Status = EDIInterchange.Status.Queued;
			interchangeToProcess.EI_InterchangeNum = "1234";
			interchangeToProcess.EI_From = "CACustoms";
			interchangeToProcess.EI_To = "XXXXXXX";
			interchangeToProcess.EI_BodyText = "UNB+UNOA:3+INETCECPT+XXXXXXX+090224:0604+106++++++1'UNG+CUSRES+CCR+U41091N1+090224:0604+108+UN+D:00A'UNH+103000001+CUSRES:D:00A:UN'BGM+:::687+8010S00001256D+11'DTM+9:200902240032:203'GIS+1'UNT+5+103000001'UNE+1+108'UNZ+1+106'";
			interchangeToProcess.EI_GB = branchPK;
			Factory.Save();

			return interchangeToProcess;
		}

		[ExpectNoExceptions]
		void AssertCheckCompanyInCanadaOrAppliesAllCountries(bool isCompanyInCanada, bool isRunServiceProviderClientServiceTaskAppliesAllCountries, string expectedValue)
		{
			var countryCode = isCompanyInCanada ? Constants.CountryCodes.Canada : Constants.CountryCodes.Australia;
			using (company.TemporarilySetCountry(countryCode))
			{
				BatchProcessorUtilities.ResetCompanyInCanadaForTesting();
				CACustomsDataRegistry.Instance.RunServiceProviderClientServiceTaskAppliesAllCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isRunServiceProviderClientServiceTaskAppliesAllCountries);
				NUnit.Framework.Assert.That(EHubInterchangeProcessorServiceTask.CheckCompanyInCanadaOrAppliesAllCountries(), NUnit.Framework.Is.EqualTo(expectedValue), "CheckCompanyInCanadaOrAppliesAllCountries");
			}
		}

		[ExpectNoExceptions]
		void AssertCheckSendCAViaEHub(bool isSendCAViaEHub, string expectedValue)
		{
			eHubMessagingRegistry.Instance.SendCAViaEHub.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isSendCAViaEHub);
			NUnit.Framework.Assert.That(EHubInterchangeProcessorServiceTask.CheckSendCAViaEHub(), NUnit.Framework.Is.EqualTo(expectedValue), "CheckSendCAViaEHub");
		}

		GlbCompany company;
	}
}
