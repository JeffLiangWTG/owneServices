using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	class IFindBoxTest : FindBoxTestFramework
	{
		#region IFindBox Tester Class

		protected class IFindBoxTester : ZCodeFindBox
		{
			public new IFindBoxPopup PopupForm
			{
				get { return base.PopupForm; }
			}

			protected override IFindBoxPopup GetNewPopupForm()
			{
				return new FindBoxPopupTester();
			}

			IFindBoxListProvider fListProvider;
			protected override IFindBoxListProvider ListProvider
			{
				get
				{
					if (fListProvider == null)
					{
						fListProvider = new FindBoxListProviderTester(this);
					}

					return fListProvider;
				}
			}
		}

		#endregion

		#region IFindBoxPopup Tester

		protected class FindBoxPopupTester : IFindBoxPopup
		{
			public void ShowModal(IFindBox findBox, Form parentForm)
			{
				IsShown = true;
				findBox.Code = "AUSYD";
				findBox.Description = "Sydney";
			}

			public SilentSelectResult SelectFromPopupWithoutDisplaying(IFindBox findBox, EmbeddedModulePopup popup)
			{
				IsShown = false;
				findBox.Code = "AUSYD";
				findBox.Description = "Sydney";
				return SilentSelectResult.FoundOne;
			}

			public void Dispose()
			{
				if (Closed != null)
				{
					Closed(this, EventArgs.Empty);
				}
			}

			public void SelectRowByPK(ZGuid pK)
			{
			}

			public bool IsShown;

			public event EventHandler Closed;
		}

		#endregion

		#region IFindBoxListProvider Tester

		protected class FindBoxListProviderTester : IFindBoxListProvider
		{
			public FindBoxListProviderTester(IFindBoxTester findBoxTester)
			{
				this.FindBoxTester = findBoxTester;
			}

			readonly IFindBoxTester FindBoxTester;

			#region IFindBoxListProvider Members

			public (string, bool) NearestMatch(string code, bool explicitAutoComplete, int cursor)
			{
				switch (code)
				{
					case "AU":
						return ("AUSYD", true);
					case "GB":
						return ("GBLON", true);
					default:
						return ("", true);
				}
			}

			public (string, bool) NearestMatchCore(string code, bool explicitAutoComplete)
			{
				switch (code)
				{
					case "AU":
						return ("AUSYD", true);
					case "GB":
						return ("GBLON", true);
					default:
						return ("", true);
				}
			}

			public string DescriptionFromCode(string code)
			{
				switch (code)
				{
					case "AUSYD":
						return "Sydney";
					case "GBLON":
						return "London";
					default:
						return "";
				}
			}

			public string DescriptionFromPrimaryKey(ZGuid pK)
			{
				throw new Exception("DIE");
			}

			public ZGuid PrimaryKeyFromCode(string code)
			{
				throw new Exception("DIE");
			}

			public string CodeFromPrimaryKey(ZGuid pK)
			{
				throw new Exception("DIE");
			}

			public IEnumerable<BusinessObject> GetBusinessObjectsFromCode(string code)
			{
				throw new Exception("DIE");
			}

			public IEnumerable<BusinessObject> GetBusinessObjectsFromCodeWithoutFilter(string code)
			{
				throw new Exception("DIE");
			}

			public BusinessObject GetBusinessObjectFromCode(string code)
			{
				throw new Exception("DIE");
			}

			public BusinessObject GetBusinessObjectFromCodeWithoutFilter(string code)
			{
				throw new Exception("DIE");
			}

			public ICodeDescription GetCustomCodeDescription(BusinessObject bizo)
				=> throw new Exception("DIE");

			public IBusinessObjectCollection List
			{
				get { return (IBusinessObjectCollection)FindBoxTester.List; }
			}

			public bool AutoCompleteOnCommit
			{
				get { return false; }
			}

			#endregion
		}

		#endregion

		public void TestCodeAndDescriptionSetFromPopup()
		{
			using (var testForm = new ZChildForm())
			{
				CreateControls(testForm, "SS_Dummy");
				testForm.Show();
				FindBox.Focus();

				FindBoxTester.PopupForm.ShowModal(FindBoxTester, testForm);
				Assert("Should have shown PopupForm", ((FindBoxPopupTester)FindBoxTester.PopupForm).IsShown);

				AssertEquals("Code set by Popup", "AUSYD", IFindBox.Code);
				AssertEquals("Description set by Popup", "Sydney", IFindBox.Description);

				ChangeFocusToInvokeBinding();
				AssertEquals("Code set to BusinessObject", new ZString("AUSYD"), Dummy.SS_Dummy);
			}
		}

		public void TestDescriptionSetWhenBindingFormats()
		{
			using (var testForm = new ZChildForm())
			{
				CreateControls(testForm, "");
				testForm.Show();
				FindBox.Focus();

				IFindBox.Code = "AUSYD";
				AssertEquals("Code", "AUSYD", FindBox.CodeBox.Text);

				ChangeFocusToInvokeBinding();
				UserIdleWorker.Flush();

				AssertEquals("Code set to BusinessObject", new ZString("AUSYD"), Dummy.SS_Dummy);
				AssertEquals("Description set by BindingFormat", "Sydney", ((IFindBox)FindBox).Description);
			}
		}

		#region Implementation

		protected override void CreateControls(ZChildForm testForm, string acceptableBindForTextBox)
		{
			base.CreateControls(testForm, "SS_Dummy");

			FindBox = new IFindBoxTester();
			testForm.Controls.Add(FindBox);

			FindBox.BindTo = "SS_Dummy";
			FindBox.BindToList = "Dummies";
			testForm.SetDataBinding(Dummy, "");
		}

		IFindBox IFindBox
		{
			get
			{
				return (IFindBox)FindBox;
			}
		}

		IFindBoxTester FindBoxTester
		{
			get
			{
				return (IFindBoxTester)FindBox;
			}
		}

		#endregion
	}
}
