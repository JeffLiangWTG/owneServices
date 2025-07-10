using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(MultiCodesSelectForm))]
	class MultiCodesSelectFormTest : ZFormBasherTest
	{
		public void TestShowOptions()
		{
			using (var form = new MultiCodesSelectForm(GetCodesForTesting()))
			{
				form.Show();

				AssertContainsExactElementsInExactOrder("Descriptions", new[] { "01 - TEST1", "02 - TEST2", "03 - TEST3" },
					form.CheckedListBox.BindingItems.Cast<ZBoolDescriptionPair>().Select(x => x.Description));
			}
		}

		public void TestShowModal()
		{
			using (var parentForm = new ZForm())
			using (var findBox = new MultiCodesFindBox())
			using (var form = new MultiCodesSelectForm(GetCodesForTesting()))
			{
				findBox.CodeBox.Text = "";
				form.ShowModal(findBox, parentForm);

				AssertContainsExactElementsInExactOrder("Values", new[] { ZBool.False, ZBool.False, ZBool.False },
					form.CheckedListBox.BindingItems.Cast<ZBoolDescriptionPair>().Select(x => x.Value));

				form.Close();
				findBox.CodeBox.Text = "01,03";
				form.ShowModal(findBox, parentForm);

				AssertContainsExactElementsInExactOrder("Values", new[] { ZBool.True, ZBool.False, ZBool.True },
					form.CheckedListBox.BindingItems.Cast<ZBoolDescriptionPair>().Select(x => x.Value));
			}
		}

		public void TestClickCancelButton()
		{
			using (var parentForm = new ZForm())
			using (var findBox = new MultiCodesFindBox())
			using (var form = new MultiCodesSelectForm(GetCodesForTesting()))
			{
				form.Show();
				form.ShowModal(findBox, parentForm);
				form.CheckedListBox.BindingItems.ForEach(x => x.Value = true);
				form.CancelButton.PerformClick();

				AssertEquals("findBox.Code value should be", "", (findBox as IFindBox).Code);
			}
		}

		public void TestClickOKButton()
		{
			using (var parentForm = new ZForm())
			using (var findBox = new MultiCodesFindBox())
			using (var form = new MultiCodesSelectForm(GetCodesForTesting()))
			{
				form.Show();
				form.ShowModal(findBox, parentForm);
				form.CheckedListBox.BindingItems.ForEach(x => x.Value = true);
				form.OKButton.PerformClick();

				AssertEquals("findBox.Code value should be", "01,02,03", (findBox as IFindBox).Code);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new MultiCodesSelectForm(GetCodesForTesting());
		}

		CodeDescriptionPairList GetCodesForTesting()
		{
			var codeDescriptionPairList = new CodeDescriptionPairList();
			codeDescriptionPairList.AddPair("01", "TEST1");
			codeDescriptionPairList.AddPair("02", "TEST2");
			codeDescriptionPairList.AddPair("03", "TEST3");

			return codeDescriptionPairList;
		}
	}
}
