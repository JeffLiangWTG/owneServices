using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.CH.ServiceTasks.Testing;

[TestedType(typeof(BordereauMessageService))]
sealed class BordereauMessageServiceTest : ServiceTaskTestCase<BordereauMessageService>
{
	protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

	public void TestHostedServiceAttribute()
	{
		var hostedServiceAttributes = GetHostedServiceAttributes();
		AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
		var hostedServiceAttribute = hostedServiceAttributes.Single();

		CombineAssertions(() =>
		{
			AssertEquals("Code", ServiceTaskApplicationCodeList.Codes.BordereauMessageSender, hostedServiceAttribute.Code);
			AssertEquals("Description", ServiceTaskApplicationCodeList.Descriptions.BordereauMessageSender, hostedServiceAttribute.Description);
			AssertEquals("Category", "CHC", hostedServiceAttribute.Category);
			AssertEquals("RequiresCompanyInCountry", Enterprise.Core.Constants.CountryCodes.Switzerland, hostedServiceAttribute.RequiresCompanyInCountry);
			AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			AssertEquals("AllowsMultipleInstances", false, hostedServiceAttribute.AllowsMultipleInstances);
			AssertEquals("DefaultScheduleStartAtUtc", "3hours", hostedServiceAttribute.DefaultScheduleStartAtUtc);
		});
	}

	public void TestRunTaskWhenEnabled() => AssertTaskRun(true, true);

	public void TestRunTaskWhenDisabled() => AssertTaskRun(false, false);

	void AssertTaskRun(bool isEnabled, bool expectedRun) => CombineAssertions(() =>
	{
		CredentialsTestHelper.CreateCurrentCompanyCertificateCredential();

		using (CHCustomsDataRegistry.Instance.EdecBordereauConfig.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new EdecBordereauConfig() { IsEnabled = isEnabled }))
		{
			var logs = InitialiseAndRunTaskSchedule(new BordereauMessageService());
			AssertEquals(logs.ToString(), expectedRun, logs.ToString().Contains($"Information|\t1 Bordereau List message(s) for company {GlbCompany.CurrentCompany.GC_Code} has been processed"));
		}
	});
}
