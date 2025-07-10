using System;
using System.Collections.Generic;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.DataTransfer.eNett_Integration;
using Enterprise.Accounting.DataTransfer.eNett_Integration.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.ServiceTasks.DataTransfer.eNett_Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ServiceTasks.SDataTransfer.eNett_Integration.Testing
{
	[TestedType(typeof(eNettInboundTransactionProcessorTask))]
	class eNettInboundTransactionProcessorTaskTest : ServiceTaskTestCase<eNettInboundTransactionProcessorTask>
	{
		public void TestRunTask()
		{
			AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new EnettRegistrationCode() { RegistrationCode = "201649", AuthenticationCode = "EJPx7yyuHu", OrganisationPK = TestObjectCreator.AALSHI.PK });

			eNettInboundTransactionProcessorTaskForTesting task = new eNettInboundTransactionProcessorTaskForTesting();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);

			using (Env.Instance.TemporaryServiceTaskContext(eNettInboundTransactionProcessorTask.Code, canRunInAnyBranch: true))
			{
				task.RunTask();
			}
		}

		// No nudging: no queue table; polls external web service.
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		public void TestHasNoQueueProvider()
		{
			var queueProvider = GetHostedServiceQueueProviderInstanceByServiceTaskCode("CPM");
			AssertNull("No queue table for this service task; polls external web service.", queueProvider);
		}

		public void TestHostedServiceRequirementIsApplied()
		{
			var methodInfo = typeof(eNettInboundTransactionProcessorTask).GetMethod(nameof(eNettInboundTransactionProcessorTask.AtLeastOneActiveAustralianCompanyHasENettRegistration));
			Assert("HostedServiceRequirement for eNettInboundTransactionProcessorTask is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));
		}

		/// <summary>
		/// This unit test is testing scenario without an active Australian company 
		/// </summary>
		public void TestENettRegistrationCodePropertyWithoutActiveAustralianCompany()
		{
			AssertEquals("Precondition", Core.Constants.CountryCodes.Australia, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand))
			{
				var expectedErrorMessage = "System should have at least one active Australian Company.";
				TestHaveENettRegistrationMethod(expectedErrorMessage);
			}
		}

		/// <summary>
		/// This unit test is testing the data scenario where no Registration Code has been configured for the client at 
		/// all in the registry, which is the default scenario
		/// </summary>
		public void TestENettRegistrationCodePropertyWithoutRegistrationCodeConfigured()
		{
			var expectedErrorMessage = "At least one of these Australian Companies needs to have the ComPay Client Number value configured under the registry setting 'Accounting -> ComPay -> ComPay Registration' configured ('EDI').";
			TestHaveENettRegistrationMethod(expectedErrorMessage);
		}

		/// <summary>
		/// This unit test is testing the data scenario where only first Company has RegistrionCode configured
		/// </summary>
		public void TestENettRegistrationCodePropertyWithFirstCompanyRegistrationCodeConfigured()
		{
			SetupENettRegistrationData(0);
			TestHaveENettRegistrationMethod(string.Empty);
		}

		/// <summary>
		/// This unit test is testing the data scenario where only the second Company has RegistrationCode (ComPay Client Number) configured
		/// The reason for testing this second company is to make sure the code is actually working under the realistic scenario where the first company
		/// is non-Australian company therefore not using ComPay, while the second company is Australian Company and using ComPay.
		/// Before the code change, only the first company's RegistrationCode (ComPay Client Number) was checked.
		/// </summary>
		public void TestENettRegistrationCodePropertyWithSecondCompanyRegistrationCodeConfigured()
		{
			SetupENettRegistrationData(1);
			TestHaveENettRegistrationMethod(string.Empty);
		}

		void TestHaveENettRegistrationMethod(string expectedError)
		{
			//If the Process Controller is running without a Company context, the default observed behaviour locally
			using (Env.SetTemporaryUserContext(new UserContext(Guid.Empty, Guid.Empty, Guid.Empty)))
			{
				var error = eNettInboundTransactionProcessorTask.AtLeastOneActiveAustralianCompanyHasENettRegistration();
				AssertEquals(expectedError, error);
			}

			//If the Process Controller is running under the context of a particular company and branch,
			var companyList = GlbCompany.GetActiveCompanies();
			var firstCompany = companyList[0];
			var branch = firstCompany.FirstActiveBranch;
			using (Env.SetTemporaryUserContext(new UserContext(Guid.Empty, branch.PK.ToGuid(), Guid.Empty)))
			{
				var error = eNettInboundTransactionProcessorTask.AtLeastOneActiveAustralianCompanyHasENettRegistration();
				AssertEquals(expectedError, error);
			}
		}

		void SetupENettRegistrationData(int companyIndex)
		{
			var companyList = GlbCompany.GetActiveCompanies();

			var companyWithRegistrationCode = companyList[companyIndex];
			companyWithRegistrationCode.SetCountry(Core.Constants.CountryCodes.Australia);

			var code = new EnettRegistrationCode();
			code.RegistrationCode = "123456";
			code.OrganisationPK = Guid.NewGuid();

			AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(companyWithRegistrationCode.PK.ToGuid(),
				Guid.Empty, Guid.Empty, code);
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}

		TestObjectCreator testObjectCreator;
	}

	class eNettInboundTransactionProcessorTaskForTesting : eNettInboundTransactionProcessorTask
	{
		protected override eNettInboundTransactionProcessor GetProcessor()
		{
			return new eNettInboundTransactionProcessorForTesting();
		}
	}
}
