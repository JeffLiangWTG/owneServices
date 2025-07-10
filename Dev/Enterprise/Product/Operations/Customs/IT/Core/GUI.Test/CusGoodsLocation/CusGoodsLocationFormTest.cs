using System.Windows.Forms;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(CusGoodsLocationForm))]
sealed class CusGoodsLocationFormTest : ZFormBasherTest
{
	protected override Form GetFormToBashCore()
	{
		var provider = Factory.New<JobDeclaration>();
		return new CusGoodsLocationForm(provider);
	}

	public void TestClearFieldsButton_Caption()
	{
		var mockProvider = new Mock<ICusGoodsLocationProvider>();
		using (var form = new CusGoodsLocationForm(mockProvider.Object))
		{
			form.Show();
			AssertEquals("Caption", "Clear Fields", form.ClearFieldsButton.CaptionResourceString.Caption);
		}
	}

	[ExpectNoExceptions]
	[RequiresSTA]
	public void TestClearFieldsButton_Click()
	{
		var mockProvider = new Mock<ICusGoodsLocationProvider>();
		using (var form = new CusGoodsLocationForm(mockProvider.Object))
		{
			form.Show();
			mockProvider.Setup(x => x.ClearGoodsLocation());
			form.ClearFieldsButton.PerformClick();
			mockProvider.Verify(x => x.ClearGoodsLocation(), Times.Once);
		}
	}
}
