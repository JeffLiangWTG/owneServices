using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ResourceStrings.Module.Testing
{
	public class CheckOutsRoutinesTest : TestCaseWithFactory
	{
		public void TestMyCheckoutsButton_Click()
		{
			var eNG = ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English);
			eNG.Put("code1", new ResourceStringData("code1", "", "", "caption", "full description"));
			eNG.Put("code2", new ResourceStringData("code2", "", "", "caption", "full description"));

			Form.Controls.Add(Module.EmbeddedControl);
			Form.Show();
			ResourceStringsFilterControl filterControl = (ResourceStringsFilterControl)Module.EmbeddedControl;

			CheckOutsRoutines.FindMyCheckOuts(filterControl);
			AssertEquals("Nothing checked out", 0, filterControl.FilteredGrid.List.Count);

			HelpDataString checkedOut1 = new HelpDataString();
			checkedOut1.HD_Language = Core.SharedConstants.Languages.French;
			checkedOut1.HD_Code = "code1";
			checkedOut1.HD_Caption = "in1";
			checkedOut1.HD_FullDescription = "descr";
			checkedOut1.HD_IsCheckedOut = true;

			HelpDataString checkedOut2 = new HelpDataString();
			checkedOut2.HD_Language = Core.SharedConstants.Languages.French;
			checkedOut2.HD_Code = "code2";
			checkedOut2.HD_Caption = "in2";
			checkedOut2.HD_FullDescription = "descr";
			checkedOut2.HD_IsCheckedOut = true;

			ResourceStringsFactory.Save("TST", checkedOut1, checkedOut2);

			CheckOutsRoutines.FindMyCheckOuts(filterControl);
			HelpDataStringCollection collection = (HelpDataStringCollection)filterControl.FilteredGrid.List;
			AssertEquals("Resources checked out", 2, collection.Count);
			AssertEquals("Selected elements", 2, filterControl.FilteredGrid.SelectedElements.Length);
		}

		#region Implementation

		static ToolStripItem FindToolStripButton(ToolStripItemCollection items, string caption)
		{
			foreach (ToolStripItem item in items)
			{
				if (item.Text == caption)
				{
					return item;
				}

				ToolStripDropDownButton button = item as ToolStripDropDownButton;
				if (button != null)
				{
					ToolStripItem subitem = FindToolStripButton(button.DropDown.Items, caption);
					if (subitem != null)
					{
						return subitem;
					}
				}
			}

			return null;
		}

		ZForm Form
		{
			get
			{
				if (form == null)
				{
					form = new ZForm();
				}
				return form;
			}
		}
		ZForm form;

		ResourceStringsModule Module
		{
			get
			{
				if (module == null)
				{
					module = new ResourceStringsModule();
				}
				return module;
			}
		}
		ResourceStringsModule module;

		protected override void SetUp()
		{
			base.SetUp();
			mockSources = ResourceStringsFactory.MockSources();
		}

		protected override void TearDown()
		{
			base.TearDown();

			if (form != null)
			{
				form.Dispose();
			}
			if (module != null)
			{
				module.Dispose();
			}
			if (mockSources != null)
			{
				mockSources.Dispose();
			}
		}

		IDisposable mockSources;

		#endregion
	}
}
