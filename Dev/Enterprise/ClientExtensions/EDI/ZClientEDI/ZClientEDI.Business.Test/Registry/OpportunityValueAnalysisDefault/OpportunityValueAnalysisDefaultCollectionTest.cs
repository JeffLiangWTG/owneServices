using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(OpportunityValueAnalysisDefaultCollection))]
	internal sealed class OpportunityValueAnalysisDefaultCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<OpportunityValueAnalysisDefaultCollection>
	{
		public new void TestAddNew()
		{
			OpportunityValueAnalysisDefaultCollection collection = new OpportunityValueAnalysisDefaultCollection();
			OpportunityValueAnalysisDefault item = collection.AddNew();

			AssertEquals(1, collection.Count);
			AssertEquals(item, collection[0]);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override OpportunityValueAnalysisDefaultCollection GetCollectionToTest()
		{
			return new OpportunityValueAnalysisDefaultCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OpportunityValueAnalysisDefault();
		}

		#endregion
	}
}
