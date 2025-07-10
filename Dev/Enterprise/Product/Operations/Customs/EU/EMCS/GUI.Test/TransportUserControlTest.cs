using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	class TransportUserControlTest : TestCaseWithFactory
	{
		public void TestSealDetailsWordWrapTextBox_UpperLowerCase()
		{
			using (var form = new ZForm(declaration))
			using (var control = new TransportUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var sealDetailsWordWrapTextBox = control.FindSingle<ZTextBox>("SealDetailsWordWrapTextBox");
				AssertEquals("Seal Details", sealDetailsWordWrapTextBox.Text);
			}
		}

		public void TestCommentWordWrapTextBox_UpperLowerCase()
		{
			using (var form = new ZForm(declaration))
			using (var control = new TransportUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var commentWordWrapTextBox = control.FindSingle<ZTextBox>("CommentWordWrapTextBox");
				AssertEquals("Container Comment", commentWordWrapTextBox.Text);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<EMCSJobDeclaration>();
			container = declaration.CusContainers.AddNew();
			container.SealDetails = "Seal Details";
			container.Comment = "Container Comment";
		}
		EMCSJobDeclaration declaration;
		EMCSCusContainer container;
	}
}
