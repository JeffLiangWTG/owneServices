using System.Windows.Forms;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(CusPermitForm))]
public class CusPermitFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
{
	public void TestFields()
	{
		var permitHeader = Factory.NewWithValidTestData<CusPermitHeader>();
		using (var form = new CusPermitFormForTesting(permitHeader))
		{
			form.Show();
			CombineAssertions(() =>
			{
				AssertEquals($"{nameof(form.PermitTypeZDropEdit)}.ReadOnly", false, form.PermitTypeZDropEdit.ReadOnly);
				AssertEquals($"{nameof(form.PermitSubTypeZDropEdit)}.Visible", false, form.PermitSubTypeZDropEdit.Visible);
				AssertEquals($"{nameof(form.UnitOfMeasureZDropEdit)}.Visible", true, form.UnitOfMeasureZDropEdit.Visible);
				AssertEquals($"{nameof(form.UnitOfMeasureZTextBox)}.Visible", false, form.UnitOfMeasureZTextBox.Visible);
			});
		}
	}

	protected override Form GetFormToBashCore() => new CusPermitForm(Factory.NewWithValidTestData<CusPermitHeader>());

	protected override bool AllowHasChangesOnFormOpen => true;

	sealed class CusPermitFormForTesting : CusPermitForm
	{
		public CusPermitFormForTesting(CusPermitHeader permitHeader)
			: base(permitHeader)
		{
		}

		internal new ZDropEdit PermitTypeZDropEdit => base.PermitTypeZDropEdit;
		internal new ZDropEdit PermitSubTypeZDropEdit => base.PermitSubTypeZDropEdit;
		internal new ZTextBox UnitOfMeasureZTextBox => base.UnitOfMeasureZTextBox;
	}
}
