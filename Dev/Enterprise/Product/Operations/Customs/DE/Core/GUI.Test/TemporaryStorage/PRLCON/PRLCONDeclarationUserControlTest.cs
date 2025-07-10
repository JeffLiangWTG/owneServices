using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class PRLCONDeclarationUserControlTest : TestCaseWithFactory
	{
		public void TestColumnSTH_AdditionalInformation_CharacterCasing()
		{
			using (var control = new PRLCONDeclarationUserControl())
			{
				var grid = control.FindSingle<ZGrid>("DeclarationsGrid");
				AssertEquals(CharacterCasing.Normal, grid.GetColumnStyle("STH_AdditionalInformation").CharacterCasing);
			}
		}

		public void TestMessagesTabPage()
		{
			var storageJobHeader = Factory.New<CusTempStorageJobHeader>();

			using (var form = new ZForm())
			using (var control = new PRLCONDeclarationUserControl())
			{
				control.SetDataBinding(storageJobHeader, ZString.Empty);
				form.Controls.Add(control);
				form.Show();

				var messagesTabPage = (ZTabPage)control.Controls.Find("MessagesTabPage", true).First();
				AssertEquals("Messages", messagesTabPage.CaptionResourceString.Caption);
			}
		}

		public void TestReferenceNumberCaption()
		{
			var storageJobHeader = Factory.New<CusTempStorageJobHeader>();

			using (var form = new ZForm())
			using (var control = new PRLCONDeclarationUserControl())
			{
				control.SetDataBinding(storageJobHeader, ZString.Empty);
				form.Controls.Add(control);
				form.Show();

				var grid = control.FindSingle<ZGrid>("DeclarationsGrid");
				AssertEquals("New Reference", grid.GetColumnCaption("ReferenceNumber"));
			}
		}
	}
}
