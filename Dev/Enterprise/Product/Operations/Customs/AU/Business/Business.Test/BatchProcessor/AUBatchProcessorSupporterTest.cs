using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUBatchProcessorSupporterTest : TestCaseWithFactory
	{
		public void TestSendEmail()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = company1.Branches.AddNew();
			branch1.FillWithValidTestData();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			var branch2 = company2.Branches.AddNew();
			branch2.FillWithValidTestData();

			var group = Factory.New<GlbGroup>();
			group.Staff.AddNew().GS_EmailAddress = "abc@abc.com";
			var postmasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			var staff = postmasterGroup.Staff.AddNew();
			staff.FillWithValidTestData();
			staff.GS_EmailAddress = "dfg@dfg.com";

			Factory.Save();

			Env.Registry.RawRegistry.AirCargoSendErrorsToGroup.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, group.PK.ToGuid());
			Env.Registry.RawRegistry.AirCargoSendErrorsToGroup.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);

			using (DisposableEnvironment.ForBranch(branch2.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				AUBatchProcessorSupporter.SendEmail(Factory, branch1, "TEST", "TEST", Env.Registry.RawRegistry.AirCargoSendErrorsToGroup);
				AssertEquals("abc@abc.com", Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients[0].Email);
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				AUBatchProcessorSupporter.SendEmail(Factory, null, "TEST", "TEST", Env.Registry.RawRegistry.AirCargoSendErrorsToGroup);
				AssertEquals("dfg@dfg.com", Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients[0].Email);
			}
		}
	}
}
