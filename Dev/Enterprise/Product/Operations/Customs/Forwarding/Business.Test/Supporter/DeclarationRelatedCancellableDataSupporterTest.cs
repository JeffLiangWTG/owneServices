using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Forwarding.Business.Testing
{
	sealed class DeclarationRelatedCancellableDataSupporterTest : RelatedCancellableDataSupporterAbstractTest
	{
		[ExpectNoExceptions]
		protected override void AssertCanCancel(BaseRelatedCancellableDataSupporter supporter, IBusiness parent)
		{
			var expectedMessage = string.Empty;
			var actualMessage = supporter.CanCancel(parent);
			NUnit.Framework.Assert.That(actualMessage, Is.EqualTo(expectedMessage), "Should default to empty.");
			var cusInBondHeader = GetCusInBondHeader(parent);
			var bondMoveHeader = Factory.New<Integration.Customs.US.InBond.ICusInBondMoveHeader>();
			((BusinessObject)bondMoveHeader).FillWithValidTestData();
			bondMoveHeader.BM_BH = cusInBondHeader.PK;
			bondMoveHeader.BM_CustomsStatus = "AEO";
			Factory.Save();
			expectedMessage = @"This record cannot be deactivated as one of its related records cannot be deactivated due to the following reason.
In-Bond I0000: This record cannot be deactivated as there are Movements that are still waiting for a response from Customs.";
			actualMessage = supporter.CanCancel(parent);
			NUnit.Framework.Assert.That(actualMessage, Is.EqualTo(expectedMessage), "Should not be empty as the cusInBondHeader can not cancel.");
		}

		[ExpectNoExceptions]
		protected override void AssertSetIsCancelled(BaseRelatedCancellableDataSupporter supporter, IBusiness parent)
		{
			var cusInBondHeader = GetCusInBondHeader(parent);
			Factory.Save();
			supporter.SetIsCancelled(parent, true);
			NUnit.Framework.Assert.That(cusInBondHeader.IsCancelled, Is.True, $"Should be true on {cusInBondHeader.HumanReadableName} as the parent is cancelled.");
			supporter.SetIsCancelled(parent, false);
			NUnit.Framework.Assert.That(!cusInBondHeader.IsCancelled, Is.True, $"Should be false on {cusInBondHeader.HumanReadableName} as the parent is not cancelled.");
		}

		Integration.Customs.US.InBond.ICusInBondHeader GetCusInBondHeader(IBusiness parent)
		{
			var cusInBondHeader = Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			((BusinessObject)cusInBondHeader).FillWithValidTestData();
			cusInBondHeader.BH_JobReference = "I0000";
			cusInBondHeader.BH_ParentID = parent.Identifier;
			cusInBondHeader.BH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			return cusInBondHeader;
		}

		protected override BaseRelatedCancellableDataSupporter GetSupporter()
		{
			return new DeclarationRelatedCancellableDataSupporter();
		}

		protected override IBusiness GetParent()
		{
			var result = Factory.New<Integration.Customs.US.IJobDeclaration>();
			((BusinessObject)result).FillWithValidTestData();
			return result;
		}
	}
}
