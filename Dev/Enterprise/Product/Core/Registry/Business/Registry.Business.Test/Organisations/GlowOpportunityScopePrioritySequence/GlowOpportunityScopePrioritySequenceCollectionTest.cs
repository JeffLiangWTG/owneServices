using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(GlowOpportunityScopePrioritySequenceCollection))]
	sealed class GlowOpportunityScopePrioritySequenceCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<GlowOpportunityScopePrioritySequenceCollection>
	{
		protected override GlowOpportunityScopePrioritySequenceCollection GetCollectionToTest()
		{
			return new GlowOpportunityScopePrioritySequenceCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new GlowOpportunityScopePrioritySequence();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}
	}
}
