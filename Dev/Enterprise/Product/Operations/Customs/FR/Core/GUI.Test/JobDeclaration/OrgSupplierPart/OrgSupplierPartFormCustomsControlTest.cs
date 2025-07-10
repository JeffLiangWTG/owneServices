using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.Declaration.Testing
{
	class OrgSupplierPartFormCustomsControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var form = new ZForm())
			using (var control = new OrgSupplierPartFormCustomsControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.Controls.Find("supportingDocsTabPage", true).First().Show();
				var supportingDocument = control.Controls.Find("SupportingDocumentsUserControl", true).First() as ZDynamicControlCreationUserControl;
				AssertEquals(typeof(SupportingDocumentsUserControl), supportingDocument.UserControlType);

				var additionalInfosTabPage = (ZTabPage)control.Controls.Find("additionalInfosTabPage", true).FirstOrDefault();
				AssertNull(additionalInfosTabPage);

				CombineAssertions("national code tab are not present", () =>
				{
					AssertNull("nationalAdditionalCodeTabPage is null", control.FindSingleOrDefault<ZTabPage>("NationalAdditionalCodeTabPage"));
				});
			}
		}

		public void TestTariffFindBoxAndColumn()
		{
			var part = Factory.NewWithValidTestData<Business.MasterFiles.OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationType.IMP;

			using (var form = new ZForm(part))
			using (var control = new OrgSupplierPartFormCustomsControl())
			{
				form.Controls.Add(control);
				form.Show();
				var pivotGrid = (ZGrid)control.Controls.Find("PivotGrid", true)[0];
				var tariffColumnStyleInfo = pivotGrid.GetColumnStyle("CI_FormattedTariffNum") as Universal.GUI.TariffColumnStyleInfo;
				var tariffFindBox = control.Controls.Find("TariffFindBox", true)[0] as Universal.GUI.TariffFindBox;
				AssertNotNull("TariffColumnStyleInfo", tariffColumnStyleInfo);
				AssertNotNull(tariffFindBox);

				AssertEquals("IMP", tariffColumnStyleInfo.GetTariffType.Invoke());
				AssertEquals("IMP", tariffFindBox.GetTariffType.Invoke());

				pivot.CI_ChildType = ClassificationType.Both;
				AssertEquals("IMP", tariffColumnStyleInfo.GetTariffType.Invoke());
				AssertEquals("IMP", tariffFindBox.GetTariffType.Invoke());

				pivot.CI_ChildType = ClassificationType.EXP;
				AssertEquals("IMP", tariffColumnStyleInfo.GetTariffType.Invoke());
				AssertEquals("IMP", tariffFindBox.GetTariffType.Invoke());
			}
		}
	}
}
