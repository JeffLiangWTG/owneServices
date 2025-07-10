using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	public class PersonalItemsDeclarationUserControlTest : TestCaseWithFactory
	{
		public void TestEntryHeaderColumns()
		{
			using (var control = new PersonalItemsDeclarationUserControl())
			{
				var grid = (ZGrid)control.Controls.Find("FamilyMembersGrid", true)[0];
				Assert(grid, "CPN_PER_Person", typeof(ZGuidFindBoxColumnStyleInfo));
				Assert(grid, "PersonBirthDate", typeof(ZDateEditColumnStyleInfo));
				Assert(grid, "RelationshipToDeclarant", typeof(ZDropEditColumnStyleInfo));
				Assert(grid, "Passport", typeof(ZTextBoxColumnStyleInfo));
				Assert(grid, "JobCode", typeof(ZDropEditColumnStyleInfo));
				Assert(grid, "EntryStatus", typeof(ZDropEditColumnStyleInfo));
				Assert(grid, "ResidencyStartDate", typeof(ZDateEditColumnStyleInfo));
				Assert(grid, "ResidencyEndDate", typeof(ZDateEditColumnStyleInfo));

				grid = (ZGrid)control.Controls.Find("ItemDetailsGrid", true)[0];
				Assert(grid, "JI_ProductTypeCode", typeof(ZDropEditColumnStyleInfo));
				Assert(grid, "JI_InvoiceUQ", typeof(ZDropEditColumnStyleInfo));
				Assert(grid, "JI_NDescription", typeof(ZTextBoxColumnStyleInfo));
				Assert(grid, "JI_BrandName", typeof(ZTextBoxColumnStyleInfo));
				Assert(grid, "JI_Model", typeof(ZTextBoxColumnStyleInfo));
				Assert(grid, "JI_CustomsQuantity", typeof(ZCalcEditColumnStyleInfo));
				Assert(grid, "JI_LinePrice", typeof(ZCalcEditColumnStyleInfo));
				Assert(grid, "JI_InvoiceQuantity", typeof(ZCalcEditColumnStyleInfo));
			}

			void Assert(ZGrid grid, ZString controlName, Type controlType)
			{
				var checkColumn = grid.GetColumnStyle(controlName);
				AssertNotNull("Column exists", checkColumn);
				AssertEquals(controlType, checkColumn.GetType());
			}
		}

		public void TestItemDetailsGroupBoxVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.PersonalItems;
			declaration.OnHasItemsChanging += OnHasItemsChanging;
			declaration.InvoiceLines.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.InvoiceLines.AddNew();
			AssertEquals(YesNoList.Codes.Yes, declaration.PIDHasItems);
			AssertEquals(3, declaration.InvoiceLines.Count);

			using (var form = new MiscDeclarationForm(declaration))
			{
				form.Show();
				var tabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				var mainTabPage = tabControl.FindSingle<ZTabPage>("MainTabPage");
				AssertEquals(mainTabPage.Controls[0].GetType(), typeof(PersonalItemsDeclarationUserControl));

				using (PersonalItemsDeclarationUserControl control = (PersonalItemsDeclarationUserControl)mainTabPage.Controls[0])
				{
					var groupBox = control.FindSingle<ZGroupBox>("ItemDetailsGroupBox");
					Assert(groupBox.Visible);

					UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.No);
					declaration.PIDHasItems = YesNoList.Codes.No;
					AssertContains("There are lines entered. If you proceed, system will delete all the lines. Are you sure?", UnitTestUserNotification.Instance.LastMessage.Text);
					groupBox = control.FindSingle<ZGroupBox>("ItemDetailsGroupBox");
					Assert(groupBox.Visible);
					AssertEquals(YesNoList.Codes.Yes, declaration.PIDHasItems);

					UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Yes);
					declaration.PIDHasItems = YesNoList.Codes.No;
					AssertContains("There are lines entered. If you proceed, system will delete all the lines. Are you sure?", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					groupBox = control.FindSingle<ZGroupBox>("ItemDetailsGroupBox");
					Assert(!groupBox.Visible);
				}
			}
			void OnHasItemsChanging(object sender, CancelEventArgs e)
			{
			}
		}
	}
}
