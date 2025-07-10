using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[SuppressFormDesignerAnalysis]
	sealed partial class ZGridActiveCollectionBindingTestCaseForm : ZForm
	{
		public ZGridActiveCollectionBindingTestCaseForm(ActiveBusinessObjectCollection<DummyWithDependentsBusinessObject> collection)
			: base(collection)
		{
		}
	}
}
