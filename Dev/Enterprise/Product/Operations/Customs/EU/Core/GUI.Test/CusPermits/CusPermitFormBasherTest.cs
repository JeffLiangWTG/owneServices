using System.Windows.Forms;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(CusPermitForm))]
	public class CusPermitFormBasherTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		public override void TestBoundListsAreNotLoadedOnAccess()
		{
			Assert("PermitCountrySpecificInstruction.GetFullTypeList() must be loaded on access, because we need the list count to determine which one to use.", true);
		}

		public void TestUnitOOfMeasureDropDownIsVisible()
		{
			var permitHeader = Factory.NewWithValidTestData<CusPermitHeader>();
			using (var form = new CusPermitFormForTest(permitHeader))
			{
				form.Show();
				Assert(form.Visible);
				Assert(form.UnitOfMeasureZDropEdit.Visible);
				Assert(!form.UnitOfMeasureZTextBox.Visible);
			}
		}

		protected override Form GetFormToBashCore() => new CusPermitForm(Factory.NewWithValidTestData<CusPermitHeader>());

		protected override bool AllowHasChangesOnFormOpen => true;

		sealed class CusPermitFormForTest : CusPermitForm
		{
			public CusPermitFormForTest(CusPermitHeader permitHeader)
				: base(permitHeader)
			{
			}

			public ZArchitecture.GUI.ZDropEdit UnitOfMeasureZDropEdit => base.unitOfMeasureZDropEdit;
			public new ZArchitecture.ZTextBox UnitOfMeasureZTextBox => base.UnitOfMeasureZTextBox;
		}
	}
}
