using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI.Testing
{
	sealed class ImportAdditionalInfosUserControlWithGridTest : TestCaseWithFactory
	{
		public void TestAdditionalInfosGroupBoxCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			using var ucc5 = ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC5Core", true);
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var additionalReference = invoiceLine.AdditionalInfos.AddNew();
			additionalReference.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			using var form = new ZForm();
			using var control = new ImportAdditionalInfosUserControlWithGrid();
			form.Controls.Add(control);
			form.SetDataBinding(declaration, ".");
			control.BindingSource.SetBindingMember(control.Grid, "FilteredInvoiceLines.AdditionalInfos");
			form.Show();

			var grid = control.Grid;
			grid.CurrentRowIndex = 0;

			var groupBox = control.FindSingle<ZGroupBox>("AdditionalInfosGroupBox");
			AssertEquals("AdditionalInfosGroupBox.Caption", "Additional Documents", groupBox.CaptionResourceString.Caption);

			var additionalInfo = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			grid.CurrentRowIndex = 1;
			AssertEquals("AdditionalInfosGroupBox.Caption", "[2/2] Additional Information", groupBox.CaptionResourceString.Caption);
		}

		public void TestGridColumnOrder()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var frm = new ZForm())
			using (var control = new ImportAdditionalInfosUserControlWithGrid())
			{
				frm.Controls.Add(control);
				frm.SetDataBinding(declaration, null);
				frm.Show();
				var grid = control.FindSingle<ZGrid>("AdditionalInfosGrid");
				AssertSequencesEqual(OrderedColumns, grid.DefaultColumns.Where(x => x.IsVisible).Select(x => x.ColumnName));

				var descriptionColumnStyle = grid.GetColumnStyle(CusSupportingInfo.Schema.CSI_Description);
				Assert("CSI_Description.IsMandatory", !descriptionColumnStyle.IsMandatory);
			}
		}

		public void TestCreateNewInvoiceHeaderAdditionalInformationDetailsLayout()
		{
			using (var control = new UCC6ImportAdditionalInfosUserControlWithGridForTesting())
			{
				AssertType<ImportAdditionalInformationDetailsLayout>(control.CreateNewInvoiceHeaderAdditionalInformationDetailsLayoutExposed());
			}
		}

		public void TestCreateNewInvoiceLineAdditionalInformationDetailsLayout()
		{
			using (var control = new UCC6ImportAdditionalInfosUserControlWithGridForTesting())
			{
				AssertType<ImportAdditionalInformationDetailsLayout>(control.CreateNewInvoiceLineAdditionalInformationDetailsLayoutExposed());
			}
		}

		public void TestCreateNewExitSummaryAdditionalInformationDetailsLayout()
		{
			using (var control = new UCC6ImportAdditionalInfosUserControlWithGridForTesting())
			{
				AssertType<ImportAdditionalInformationDetailsLayout>(control.CreateNewExitSummaryAdditionalInformationDetailsLayoutExposed());
			}
		}

		public void TestCreateNewClassPartPivotAdditionalInformationDetailsLayout()
		{
			using (var control = new UCC6ImportAdditionalInfosUserControlWithGridForTesting())
			{
				AssertType<ImportAdditionalInformationDetailsLayout>(control.CreateNewClassPartPivotAdditionalInformationDetailsLayoutExposed());
			}
		}

		string[] OrderedColumns => new[]
		{
				CusSupportingInfo.Schema.CSI_SubType,
				CusSupportingInfo.Schema.CSI_Code,
				CusSupportingInfo.Schema.CSI_ReferenceNumber,
				CusSupportingInfo.Schema.CSI_Description,
		};

		class UCC6ImportAdditionalInfosUserControlWithGridForTesting : ImportAdditionalInfosUserControlWithGrid
		{
			public IPanelLayoutProvider CreateNewInvoiceHeaderAdditionalInformationDetailsLayoutExposed() => base.CreateNewInvoiceHeaderAdditionalInformationDetailsLayout();
			public IPanelLayoutProvider CreateNewInvoiceLineAdditionalInformationDetailsLayoutExposed() => base.CreateNewInvoiceLineAdditionalInformationDetailsLayout();
			public IPanelLayoutProvider CreateNewExitSummaryAdditionalInformationDetailsLayoutExposed() => base.CreateNewExitSummaryAdditionalInformationDetailsLayout();
			public IPanelLayoutProvider CreateNewClassPartPivotAdditionalInformationDetailsLayoutExposed() => base.CreateNewClassPartPivotAdditionalInformationDetailsLayout();
		}
	}
}
