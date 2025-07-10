using Enterprise.Customs.JP.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(CertificateLayoutTemplate))]
	sealed class CertificateLayoutTemplateTest : TestCase
	{
		public void TestControls()
		{
			using (var control = new CertificateLayoutTemplate())
			{
				Assert("CommonControlNumberTextBox", control.FindSingle<ZTextBox>("CommonControlNumberTextBox").Visible);
				Assert("FoodHygieneCertificateTypeDropEdit", control.FindSingle<ZDropEdit>("FoodHygieneCertificateTypeDropEdit").Visible);
				Assert("PlantProtectionCertificateTypeDropEdit", control.FindSingle<ZDropEdit>("PlantProtectionCertificateTypeDropEdit").Visible);
				Assert("AnimalQuarantineCertificateTypeDropEdit", control.FindSingle<ZDropEdit>("AnimalQuarantineCertificateTypeDropEdit").Visible);
				Assert("TradeControlOrderDropEdit", control.FindSingle<ZDropEdit>("TradeControlOrderDropEdit").Visible);
				Assert("CommercialValueTypeDropEdit", control.FindSingle<ZDropEdit>("CommercialValueTypeDropEdit").Visible);
			}
		}

		public void TestOtherLawsGrid()
		{
			using (var control = new CertificateLayoutTemplate())
			{
				CombineAssertions(() =>
				{
					var otherLawsAndRegulationsGroupBox = control.FindSingle<ZGroupBox>("OtherLawsAndRegulationsGroupBox");
					AssertEquals("English Caption", "Other Laws and Regulations", otherLawsAndRegulationsGroupBox?.CaptionResourceString.Caption);

					var otherLawsGrid = control.FindSingle<ZGrid>("OtherLawsGrid");
					AssertEquals("Must be in OtherLawsAndRegulationsGroupBox", true, otherLawsAndRegulationsGroupBox.Contains(otherLawsGrid));
					AssertEndsWith("Binds to correct field", "OtherLaws", control.BindingSource.GetBindingMember(otherLawsGrid));

					AssertEquals("MaximumRows of OtherLawsGrid must be equal to CusOtherLawReferenceCollection.MaxRowCount", CusOtherLawReferenceCollection<CusOtherLawReference>.MaxRowCount, otherLawsGrid.MaximumRows);
				});
			}
		}

		public void TestApprovalCertificateInfoGrid()
		{
			using (var control = new CertificateLayoutTemplate())
			{
				var grid = control.FindSingle<ZGrid>("ApprovalCertificateInfoGrid");
				var codeColumn = grid.GetColumnStyle("CSI_Code");
				var numberColumn = grid.GetColumnStyle("CSI_ReferenceNumber");
				var descriptionColumn = grid.GetColumnStyle("ReferenceNumberDescription");

				AssertType<ZDropEditColumnStyleInfo>(codeColumn);
				AssertType<ZDropEditColumnStyleInfo>(numberColumn);
				AssertType<ZTextBoxColumnStyleInfo>(descriptionColumn);
				AssertEquals("MaximumRows of ApprovalCertificateInfoGrid must be equal to ApprovalCertificateInfoCollection.MaxCountForImport", ApprovalCertificateInfoCollection.MaxCountForImport, grid.MaximumRows);
			}
		}
	}
}
