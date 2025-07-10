using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(GLJournalApprovalThresholdCollection))]
	public class GLJournalApprovalThresholdCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<GLJournalApprovalThresholdCollection>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override GLJournalApprovalThresholdCollection GetCollectionToTest()
		{
			return new GLJournalApprovalThresholdCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new GLJournalApprovalThreshold();
		}

		#endregion
	}
}
