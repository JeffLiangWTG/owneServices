using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ProcessManagement.Business.Test;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Test
{
	[TestedType(typeof(IncidentClosureDispositionCollectionView))]
	public class IncidentClosureDispositionCollectionViewTest : CodeDescriptionBoolTreeViewTest<IncidentClosureDispositionCollectionView>
	{
		public void TestAllowNew()
		{
			var allNodes = new IncidentClosureDispositionCollection();

			var view1 = new IncidentClosureDispositionCollectionView(allNodes, ZGuid.Empty);
			AssertEquals(true, view1.AllowNew);
		}

		protected override IncidentClosureDispositionCollectionView GetCollectionToTest()
		{
			var allNodes = new IncidentClosureDispositionCollection();
			return new IncidentClosureDispositionCollectionView(allNodes, ZGuid.Empty);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = new IncidentClosureDisposition();
			return result;
		}
	}
}
