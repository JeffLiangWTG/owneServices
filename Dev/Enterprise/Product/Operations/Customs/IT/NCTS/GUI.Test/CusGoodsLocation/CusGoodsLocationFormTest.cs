using System.Windows.Forms;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.GUI;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

[TestedType(typeof(CusGoodsLocationForm))]
sealed class CusGoodsLocationFormTest : ZFormBasherTest
{
	protected override Form GetFormToBashCore()
	{
		var provider = Factory.NewDepartureNctsHeader();
		return new CusGoodsLocationForm(provider.MovementHeader);
	}

	public void TestClearFieldsButton_Caption()
	{
		var mockProvider = new Mock<ICusGoodsLocationProvider>();
		using (var form = new CusGoodsLocationForm(mockProvider.Object))
		{
			var clearFieldsButton = form.FindSingle<ZButton>("ClearFieldsButton");
			form.Show();
			AssertEquals("Caption", "Clear Fields", clearFieldsButton.CaptionResourceString.Caption);
		}
	}

	[ExpectNoExceptions]
	[RequiresSTA]
	public void TestClearFieldsButton_Click()
	{
		var mockProvider = new Mock<ICusGoodsLocationProvider>();
		using (var form = new CusGoodsLocationForm(mockProvider.Object))
		{
			var clearFieldsButton = form.FindSingle<ZButton>("ClearFieldsButton");
			form.Show();
			mockProvider.Setup(x => x.ClearGoodsLocation());
			clearFieldsButton.PerformClick();
			mockProvider.Verify(x => x.ClearGoodsLocation(), Times.Once);
		}
	}
}
