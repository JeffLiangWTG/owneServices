using System.Linq;
using System.Windows.Forms;
using Enterprise.Client.EDI.Billing.GenericCollection;
using Enterprise.Client.EDI.Popups;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;
using ZClientEDI.Business.Billing.ClientLicencePriceHeader;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing;

[TestedType(typeof(FilterPopupForm))]
public class FilterPopupFormTest : ZFormBasherTest
{
	public void TestFormWillInstantiateFilterControl()
	{
		using (var form = new FilterPopupForm(MockFilterable().Object))
		{
			var filterControl = form.Controls.OfType<ZFilterStripBaseControl>().FirstOrDefault();

			AssertNotNull(filterControl);
		}
	}

	public void TestApplyButtonWillInvokeApplyFilterMethod()
	{
		var mockFilterable = MockFilterable();
		using (var form = new FilterPopupForm(mockFilterable.Object))
		{
			form.Show();
			form.AcceptButton.PerformClick();
			mockFilterable.Verify(b => b.ApplyFilter(), Times.Once);
			Assert(true);
		}
	}

	protected override Form GetFormToBashCore()
	{
		var form = new FilterPopupForm(MockFilterable().Object);
		return form;
	}

	Mock<IFilterableCollection> MockFilterable()
	{
		var mockFilterable = new Mock<IFilterableCollection>();
		mockFilterable.Setup(f => f.FilterObject).Returns(new ClientLicencePriceHeaderFilterBusinessObject());

		return mockFilterable;
	}
}
