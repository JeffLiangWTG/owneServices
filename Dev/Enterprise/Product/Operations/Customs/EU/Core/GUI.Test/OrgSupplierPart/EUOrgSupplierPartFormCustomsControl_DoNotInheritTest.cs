using Enterprise.Customs.Common;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(EUOrgSupplierPartFormCustomsControlForTest))]
	sealed class EUOrgSupplierPartFormCustomsControl_DoNotInheritTest : EUOrgSupplierPartFormCustomsControlTest
	{
		public void TestGetSupplierPartTaxUserControlType()
		{
			var part = Factory.NewWithValidTestData<Business.MasterFiles.OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationType.IMP;
			using var form = new ZForm(part);
			using var control = new EUOrgSupplierPartFormCustomsControlForTest();
			form.Controls.Add(control);
			form.Show();

			AssertEquals("GetSupplierPartTaxUserControlType", typeof(OrgSupplierPartTaxUserControl), control.GetSupplierPartTaxUserControlTypeExposed());
		}
	}
}
