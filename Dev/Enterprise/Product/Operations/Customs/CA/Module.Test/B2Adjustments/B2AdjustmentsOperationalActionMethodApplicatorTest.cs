using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(B2AdjustmentsOperationalActionMethodApplicator))]
	sealed class B2AdjustmentsOperationalActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		[TestDate(2019, 2, 20)]
		public void TestApply()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			declaration.CA_B2Type = B2TypeList.Codes.Blanket;
			declaration.JE_DeclarationReference = "B00001001";

			Factory.Save();

			Applicator.SubmissionDate = new ZDateTime(2019, 2, 25);

			var targets = new BusinessObject[] { declaration };
			var log = SimulateRun(targets, false);

			Assert(log.MessagesString().Contains("INFO: Job [HL B00001001]: Submitted Date has been updated."));
			AssertEquals(new ZDateTime(2019, 2, 25), declaration.CA_B2SubmissionDate);
		}

		[TestDate(2019, 2, 20)]
		public void TestApply_OverrideExisting()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			declaration.CA_B2Type = B2TypeList.Codes.Blanket;
			declaration.JE_DeclarationReference = "B00001001";
			declaration.CA_B2SubmissionDate = new ZDateTime(2019, 2, 24);

			Factory.Save();

			Applicator.SubmissionDate = new ZDateTime(2019, 2, 25);

			var targets = new BusinessObject[] { declaration };
			var log = SimulateRun(targets, false);

			Assert(log.MessagesString().Contains("WARNING: Job [HL B00001001]: Submitted Date can't be updated as it already exists."));
			AssertEquals(new ZDateTime(2019, 2, 24), declaration.CA_B2SubmissionDate);

			Applicator.OverrideExisting = true;

			log = SimulateRun(targets, false);

			Assert(log.MessagesString().Contains("INFO: Job [HL B00001001]: Submitted Date has been updated."));
			AssertEquals(new ZDateTime(2019, 2, 25), declaration.CA_B2SubmissionDate);
		}

		[TestDate(2019, 2, 20)]
		public void TestApply_ForceUpdate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			declaration.CA_B2Type = B2TypeList.Codes.Blanket;
			declaration.JE_DeclarationReference = "B00001001";

			Factory.Save();

			Applicator.SubmissionDate = new ZDateTime(2019, 2, 15);

			var targets = new BusinessObject[] { declaration };
			var log = SimulateRun(targets, false);

			Assert(log.MessagesString().Contains("WARNING: Job [HL B00001001]: Submitted Date can't be updated as it's earlier than job created date."));
			Assert(declaration.CA_B2SubmissionDate.IsEmpty);

			Applicator.ForceUpdate = true;

			log = SimulateRun(targets, false);

			Assert(log.MessagesString().Contains("INFO: Job [HL B00001001]: Submitted Date has been updated."));
			AssertEquals(new ZDateTime(2019, 2, 15), declaration.CA_B2SubmissionDate);
		}

		protected override BusinessObject GetNewBusinessObject() => new B2AdjustmentsOperationalActionMethodApplicator(Factory);

		new B2AdjustmentsOperationalActionMethodApplicator Applicator => (B2AdjustmentsOperationalActionMethodApplicator)base.Applicator;
	}
}
