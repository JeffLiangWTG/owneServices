using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Controls.Extensions.Testing
{
	sealed class FinalTypeRetrieverTest : TestCase
	{
		public void TestTraverseHierarchyToGetFinalTypeWithProperty()
		{
			var finalType = FinalTypeRetriever.Retrieve(typeof(DummyBusinessObject), "Collection");
			AssertEquals("Should be DummyBase!! Oi!", typeof(DummyChildBusinessObject).FullName, finalType.FullName);
		}
	}
}
