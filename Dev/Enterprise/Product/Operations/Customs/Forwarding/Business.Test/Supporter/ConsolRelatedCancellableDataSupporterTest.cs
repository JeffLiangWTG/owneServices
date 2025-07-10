using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Forwarding.Business.Testing
{
	sealed class ConsolRelatedCancellableDataSupporterTest : RelatedCancellableDataSupporterAbstractTest
	{
		[ExpectNoExceptions]
		protected override void AssertCanCancel(BaseRelatedCancellableDataSupporter supporter, IBusiness parent)
		{
			var expectedMessage = string.Empty;
			var actualMessage = supporter.CanCancel(parent);
			NUnit.Framework.Assert.That(actualMessage, Is.EqualTo(expectedMessage), "Should default to empty.");
			var afrHeader = GetAFRHeader(parent);
			Factory.Save();
			expectedMessage = $@"This record cannot be deactivated as one of its related records cannot be deactivated due to the following reason.
Advance Filing Rules (JOB: {afrHeader.JPH_JobReference}): This job may not be deactivated because status indicates that the Advance Filing Rules portion of the job is active with Japan Customs. The bills would need to be deleted from Japan Customs before the job is deactivated.";
			actualMessage = supporter.CanCancel(parent);
			NUnit.Framework.Assert.That(actualMessage, Is.EqualTo(expectedMessage), "Should not be empty as the related AFRHeader can not cancel.");
		}

		[ExpectNoExceptions]
		protected override void AssertSetIsCancelled(BaseRelatedCancellableDataSupporter supporter, IBusiness parent)
		{
			var header = GetAFRHeader(parent);
			Factory.Save();
			supporter.SetIsCancelled(parent, true);
			NUnit.Framework.Assert.That(header.IsCancelled, Is.True, $"Should be true on {header.HumanReadableName} as the parent is cancelled.");
			supporter.SetIsCancelled(parent, false);
			NUnit.Framework.Assert.That(!header.IsCancelled, Is.True, $"Should be false on {header.HumanReadableName} as the parent is not cancelled.");
		}

		protected override BaseRelatedCancellableDataSupporter GetSupporter()
		{
			return new ConsolRelatedCancellableDataSupporter();
		}

		Integration.Customs.JP.AFR.IJPAFRHeader GetAFRHeader(IBusiness parent)
		{
			var header = Factory.New<Integration.Customs.JP.AFR.IJPAFRHeader>();
			header.JPH_ParentId = parent.Identifier;
			header.JPH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			header.JPH_MessageStatus = "AHC";
			return header;
		}

		protected override IBusiness GetParent()
		{
			return Factory.NewWithValidTestData<ForwardingConsol>();
		}
	}
}
