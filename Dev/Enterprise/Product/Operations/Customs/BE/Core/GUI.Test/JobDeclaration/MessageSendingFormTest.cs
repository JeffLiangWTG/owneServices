using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.BE.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.GUI.Testing;

[TestedType(typeof(MessageSendingForm<BEJobDeclarationMessageSendingObjectParentForTesting>))]
class MessageSendingFormTest : ZFormBasherTest
{
	protected override Form GetFormToBashCore()
	{
		var declaration = Factory.New<JobDeclaration>();
		var declarationWrapper = new BEJobDeclarationMessageSendingObjectParentForTesting(declaration);
		return new MessageSendingForm<BEJobDeclarationMessageSendingObjectParentForTesting>(declarationWrapper);
	}

	public void TestIsTestDeclarationColumnAvailable()
	{
		using (var userControl = GetFormToBashCore())
		{
			userControl.Show();
			var grid = userControl.FindSingle<ZGrid>("MessageSendingObjectsGrid");
			var column = grid.GetColumnStyle("IsTestDeclaration");
			CombineAssertions(() =>
			{
				AssertNotNull("IsTestDeclaration-column", column);

				var columnStyle = grid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == "IsTestDeclaration");

				AssertType<ZCheckBoxColumnStyleInfo>(columnStyle);
			});
		}
	}
}
