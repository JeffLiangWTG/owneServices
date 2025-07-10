using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	public abstract class BaseFindBoxTest : FindBoxTestFramework
	{
		public void TestAutoComplete()
		{
			CreateDummies();

			using (var testForm = new ZChildForm())
			{
				CreateControls(testForm, "");
				testForm.Show();

				AssertEquals("Initial SelectionStart", 0, FindBox.CodeBox.SelectionStart);
				AssertEquals("Initial SelectionLength", 0, FindBox.CodeBox.SelectionLength);

				SendKeyPressToCodeBox('=');
				AssertEquals("SelectionStart when AutoCompleting Empty", 0, FindBox.CodeBox.SelectionStart);
				AssertEquals("SelectionLength when AutoCompleting Empty", FindBox.CodeBox.Text.Length, FindBox.CodeBox.SelectionLength);

				FindBox.CodeBox.Text = "A";
				FindBox.CodeBox.SelectionStart = 1;
				FindBox.CodeBox.SelectionLength = 0;

				SendKeyPressToCodeBox('=');
				AssertEquals("SelectionStart when AutoCompleting A", 1, FindBox.CodeBox.SelectionStart);
				AssertEquals("SelectionLength when AutoCompleting A", FindBox.CodeBox.Text.Length - 1, FindBox.CodeBox.SelectionLength);

				FindBox.CodeBox.Text = "XX";
				FindBox.CodeBox.SelectionStart = 2;
				FindBox.CodeBox.SelectionLength = 0;

				//literal = tests - which don't apply to every subclass generically

				SendKeyPressToCodeBox('=');
				if (FindBox.CodeBox.Text == "AAAA")
				{ return; } // filter out ZAutoCompleteFindBoxText which overrides to always return AAAA
				if (FindBox.CodeBox.Text == "XX=YY DESCRIPTION")
				{ return; } // filter out ZDescriptionGridFindBoxTest which checks descriptions
				if (FindBox.CodeBox.Text == "XX")
				{ return; } // filter outOrganisationalUnitPickerFindBoxTest

				AssertEquals("XX= (literal =)", "XX=", FindBox.CodeBox.Text);
				AssertEquals("SelectionStart", 3, FindBox.CodeBox.SelectionStart);
				AssertEquals("SelectionLength", 0, FindBox.CodeBox.SelectionLength);

				SendKeyPressToCodeBox('=');
				AssertEquals("XX=YY (autocompleted)", "XX=YY", FindBox.CodeBox.Text);
				AssertEquals("SelectionStart", 3, FindBox.CodeBox.SelectionStart);
				AssertEquals("SelectionLength", 2, FindBox.CodeBox.SelectionLength);

				FindBox.CodeBox.Text = "XXY";
				FindBox.CodeBox.SelectionStart = 2;
				FindBox.CodeBox.SelectionLength = 0;

				SendKeyPressToCodeBox('=');
				AssertEquals("XX=Y (literal =)", "XX=Y", FindBox.CodeBox.Text);
				AssertEquals("SelectionStart", 3, FindBox.CodeBox.SelectionStart);
				AssertEquals("SelectionLength", 0, FindBox.CodeBox.SelectionLength);

				SendKeyPressToCodeBox('=');
				AssertEquals("XX=YY (autocompleted)", "XX=YY", FindBox.CodeBox.Text);
				AssertEquals("SelectionStart", 4, FindBox.CodeBox.SelectionStart);
				AssertEquals("SelectionLength", 1, FindBox.CodeBox.SelectionLength);
			}
		}

		#region Implementation

		protected override void CreateControls(ZChildForm testForm, string acceptableBindForTextBox)
		{
			base.CreateControls(testForm, "SS_Dummy");

			FindBox = NewFindBoxTester;
			testForm.Controls.Add(FindBox);

			SetBindTo(FindBox);
			testForm.SetDataBinding(Dummy, "");
		}

		protected override void TearDown()
		{
			base.TearDown();

			if (FindBox != null)
			{
				FindBox.Dispose();
			}
		}

		protected virtual ZFindBoxUserControl NewFindBoxTester
		{
			get { return new ZAutoCompleteFindBoxTester(); }
		}

		internal class ZAutoCompleteFindBoxTester : ZAutoCompleteFindBox, IFindBoxListProvider
		{
			protected override IFindBoxListProvider ListProvider
			{
				get { return this; }
			}

			protected internal override IFindBoxPopup PopupForm
			{
				get { return null; }
			}

			#region IFindBoxListProvider Members

			(string, bool) IFindBoxListProvider.NearestMatch(string code, bool explicitAutoComplete, int cursor)
			{
				return ("AAAA", true);
			}

			(string, bool) IFindBoxListProvider.NearestMatchCore(string code, bool explicitAutoComplete)
			{
				return ("AAAA", true);
			}

			string IFindBoxListProvider.DescriptionFromCode(string code)
			{
				return "AAAA Description";
			}

			string IFindBoxListProvider.DescriptionFromPrimaryKey(ZGuid pK)
			{
				return "AAAA Description";
			}

			public ZGuid PrimaryKeyFromCode(string code)
			{
				throw new Exception("Unsupported");
			}

			public string CodeFromPrimaryKey(ZGuid pK)
			{
				throw new Exception("Unsupported");
			}

			public BusinessObject GetBusinessObjectFromCode(string code)
			{
				throw new Exception("Unsupported");
			}

			public BusinessObject GetBusinessObjectFromCodeWithoutFilter(string code)
			{
				throw new Exception("Unsupported");
			}

			public IEnumerable<BusinessObject> GetBusinessObjectsFromCode(string code)
			{
				throw new Exception("Unsupported");
			}

			public IEnumerable<BusinessObject> GetBusinessObjectsFromCodeWithoutFilter(string code)
			{
				throw new Exception("Unsupported");
			}

			public ICodeDescription GetCustomCodeDescription(BusinessObject bizo)
				=> throw new Exception("Unsupported");

			public new bool AutoCompleteOnCommit
			{
				get { return base.AutoCompleteOnCommit; }
			}

			IBusinessObjectCollection IFindBoxListProvider.List
			{
				get { return (IBusinessObjectCollection)List; }
			}

			protected override object DataSourceCore
			{
				get { return null; }
			}

			#endregion

			public IFindBox FindBoxExposed => IFindBox;

			public string CodeForFindingExposed => CodeForFinding;
		}

		protected virtual void SetBindTo(ZFindBoxUserControl findBox)
		{
			findBox.BindTo = "SS_Dummy";
			findBox.BindToList = "Dummies";
		}

		#endregion
	}
}
