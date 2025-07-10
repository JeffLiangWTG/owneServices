using System.Windows.Forms;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(WarehouseAreaIDsForm))]
	public class WarehouseAreaIDsFormTest : ZFormBasherTest
	{
		public void TestCaptions()
		{
			using (var form = GetFormToBashCore() as WarehouseAreaIDsForm)
			{
				AssertEquals("Caption should be", "Warehouse Area ID", form.FormCaption);
			}
		}

		public void TestProperties()
		{
			using (var form = GetFormToBashCore() as WarehouseAreaIDsForm)
			{
				CombineAssertions(() =>
				{
					AssertType<ZGrid>("WarehouseAreaIDGrid should be Type ZGrid", form.WarehouseAreaIDGrid);
					AssertType<ZButton>("WarehouseAreaIDGrid should be Type ZButton", form.CloseButton);
					AssertType<ZButton>("WarehouseAreaIDGrid should be Type ZButton", form.OKButton);
				});
			}
		}

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			Factory.Save();
			return new WarehouseAreaIDsForm(declaration);
		}
	}
}
