using System;
using System.Collections.Generic;
using System.Threading;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.AU.ServiceTasks.Testing
{
	[TestedType(typeof(InterchangeRetrieverService))]
	sealed class InterchangeRetrieverServiceTest : ServiceTaskTestCase<InterchangeRetrieverService>
	{
		public void TestRun()
		{
			var auCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			auCompany1.GC_Code = "AU1";
			auCompany1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var branch1 = auCompany1.Branches.AddNew();
			branch1.GB_Code = "BR1";
			var aucompany2 = Factory.NewWithValidTestData<GlbCompany>();
			aucompany2.GC_Code = "AU2";
			aucompany2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var branch2 = aucompany2.Branches.AddNew();
			branch2.GB_Code = "BR2";
			var usCompany = Factory.NewWithValidTestData<GlbCompany>();
			usCompany.GC_Code = "US1";
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var usBranch = usCompany.Branches.AddNew();
			usBranch.GB_Code = "USB";
			Factory.Save();

			int aucInterchangeRetrieverExecuted = 0;

			var service = new InterchangeRetrieverServiceForTesting();
			service.AUCInterchangeRetrieverExecuted += (s, e) => aucInterchangeRetrieverExecuted++;
			service.RunTask();

			var logger = service.ServiceLogger;
			AssertEquals("AUCInterchangeRetriever should have been executed once and only once.", 1, aucInterchangeRetrieverExecuted);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						MailDBItemsSchema.Constants.TableName,
						InterchangeRetrieverService.Description,
						MailDBItemsSchema.Constants.MI_Direction + "=RCV",
						MailDBItemsSchema.Constants.MI_Status + "=QUE",
						MailDBItemsSchema.Constants.MI_Application + "=AUI"),
				};
			}
		}
	}

	sealed class InterchangeRetrieverServiceForTesting : InterchangeRetrieverService
	{
		protected override AUCInterchangeRetriever CreateAUCInterchangeRetriever()
		{
			var result = new AUCInterchangeRetrieverForTesting();
			result.Executed += AUCInterchangeRetrieverExecuted;
			return result;
		}

		public event EventHandler AUCInterchangeRetrieverExecuted;
	}

	sealed class AUCInterchangeRetrieverForTesting : AUCInterchangeRetriever
	{
		protected override void Execute(CancellationToken token)
		{
			base.Execute(token);
			Executed?.Invoke(this, new EventArgs());
		}

		public event EventHandler Executed;
	}
}
