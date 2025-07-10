using NUnit.Framework;

namespace Enterprise.Customs.Common.SG.Testing
{
	class CustomsEntryStatusListTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestCustomsEntryStatusList()
		{
			var list = new CustomsEntryStatusList();
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode("AAA"), Is.Null, "AAA is not in the list");
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(EntryStatus.NotSentForFilter), Is.EqualTo("Not Sent"));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(Core.SGConstants.DeclarationStatus.DeclarationPending), Is.EqualTo("Pending Declaration Queued."));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(Core.SGConstants.DeclarationStatus.DeclarationSent), Is.EqualTo("Declaration Sent Waiting Response."));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(Core.SGConstants.DeclarationStatus.DeclarationRejectedByCustoms), Is.EqualTo("Declaration Rejected."));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(Core.SGConstants.DeclarationStatus.DeclarationHadSyntaxErrors), Is.EqualTo("Declaration in Error."));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(Core.SGConstants.DeclarationStatus.DeclarationPermitReceived), Is.EqualTo("Permit Approved."));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(Core.SGConstants.DeclarationStatus.DeclarationQuery), Is.EqualTo("Declaration Queried."));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(Core.SGConstants.DeclarationStatus.AmendmentPending), Is.EqualTo("Pending Amendment Declaration Queued."));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(Core.SGConstants.DeclarationStatus.AmendmentSent), Is.EqualTo("Amendment Declaration Sent Waiting Response."));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(Core.SGConstants.DeclarationStatus.AmendmentRejectedByCustoms), Is.EqualTo("Amendment Declaration Rejected."));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(Core.SGConstants.DeclarationStatus.AmendmentHadSyntaxErrors), Is.EqualTo("Amendment Declaration in Error."));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(Core.SGConstants.DeclarationStatus.AmendmentPermitReceived), Is.EqualTo("Amendment Permit Approved."));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(Core.SGConstants.DeclarationStatus.CancellationPending), Is.EqualTo("Pending Cancellation Queued."));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(Core.SGConstants.DeclarationStatus.CancellationSent), Is.EqualTo("Cancellation Sent Waiting Response."));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(Core.SGConstants.DeclarationStatus.CancellationRejectedByCustoms), Is.EqualTo("Cancellation Rejected."));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(Core.SGConstants.DeclarationStatus.CancellationHadSyntaxErrors), Is.EqualTo("Cancellation in Error."));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(Core.SGConstants.DeclarationStatus.CancellationAccepted), Is.EqualTo("Cancellation Accepted."));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(Core.SGConstants.DeclarationStatus.RefundPending), Is.EqualTo("Pending Refund Request Queued."));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(Core.SGConstants.DeclarationStatus.RefundSent), Is.EqualTo("Refund Request Sent Waiting Response."));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(Core.SGConstants.DeclarationStatus.RefundRejectedByCustoms), Is.EqualTo("Refund Request Rejected."));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(Core.SGConstants.DeclarationStatus.RefundHadSyntaxErrors), Is.EqualTo("Refund Request Error."));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(Core.SGConstants.DeclarationStatus.RefundPermitReceived), Is.EqualTo("Refund Request Approved."));
		}
	}
}
