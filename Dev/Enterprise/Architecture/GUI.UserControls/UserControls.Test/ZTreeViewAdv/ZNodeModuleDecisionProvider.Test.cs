using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI.Tests
{
	class ZNodeModuleDecisionProviderTest : TestCaseWithFactory
	{
		public void TestHandleFindBoxOKButton_AddsChildren()
		{
			var popup = new EmbeddedModulePopup();
			var dummyModel = new DummyTreeModel(Factory, new[] { Factory.New<DummyBusinessObject>() });
			var provider = new DummyNodeModuleDecisionProvider(dummyModel.RootNodes.Single());
			provider.Popup = popup;

			provider.HandleFindBoxOKButton(System.Array.Empty<DummyBusinessObject>());

			AssertEquals(true, provider.TryAddChildrenCalled);
		}

		public void TestProperty()
		{
			var dummyModel = new DummyTreeModel(Factory, new[] { Factory.New<DummyBusinessObject>() });
			var provider = new DummyNodeModuleDecisionProvider(dummyModel.RootNodes.Single());

			AssertEquals(false, provider.ShouldDisplayNotifications);
			AssertEquals(false, provider.ShouldIgnoreAdditionalFilter);
		}
	}

	class DummyNodeModuleDecisionProvider : ZNodeModuleDecisionProvider<DummyBusinessObject>
	{
		public DummyNodeModuleDecisionProvider(ZNode<DummyBusinessObject> parentNode)
			: base(parentNode)
		{
		}

		protected override bool TryAddChildren(IEnumerable<DummyBusinessObject> children)
		{
			TryAddChildrenCalled = true;
			return true;
		}

		public bool TryAddChildrenCalled { get; private set; }
	}
}
