using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class PartPivotLayoutSupportingDocumentsFieldsControlTest : TestCase
{
	public void TestGetLayout()
	{
		using (var control = new PartPivotLayoutSupportingDocumentsFieldsControlForTest())
		{
			AssertType<PartPivotSupportingDocumentFieldsLayout>("Layout", control.GetLayoutExposed());
		}
	}

	class PartPivotLayoutSupportingDocumentsFieldsControlForTest : PartPivotLayoutSupportingDocumentsFieldsControl
	{
		public IPanelLayoutProvider GetLayoutExposed() => base.GetLayout();
	}
}
