using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI.Testing
{
	public class AdditionalInfosUserControlWithGridTest : TestCaseWithFactory
	{
		public void TestAdditionalInfosGroupBoxCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var frm = new ZForm())
			using (var control = new AdditionalInfosUserControlWithGrid())
			{
				frm.Controls.Add(control);
				frm.SetDataBinding(declaration, null);
				frm.Show();
				var groupBox = control.FindSingle<ZGroupBox>("AdditionalInfosGroupBox");

				AssertEquals("AdditionalInfosGroupBox.Caption", "Additional Documents", groupBox.CaptionResourceString.Caption);
			}
		}

		public void TestGridColumnOrder()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var frm = new ZForm())
			using (var control = new AdditionalInfosUserControlWithGrid())
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
			using (var control = new AdditionalInfosUserControlWithGridForTest())
			{
				AssertType<AdditionalInformationDetailsLayout>(control.CreateNewInvoiceHeaderAdditionalInformationDetailsLayoutExposed());
			}
		}

		public void TestCreateNewInvoiceLineAdditionalInformationDetailsLayout()
		{
			using (var control = new AdditionalInfosUserControlWithGridForTest())
			{
				AssertType<AdditionalInformationDetailsLayout>(control.CreateNewInvoiceLineAdditionalInformationDetailsLayoutExposed());
			}
		}

		public void TestCreateNewExitSummaryAdditionalInformationDetailsLayout()
		{
			using (var control = new AdditionalInfosUserControlWithGridForTest())
			{
				AssertType<AdditionalInformationDetailsLayout>(control.CreateNewExitSummaryAdditionalInformationDetailsLayoutExposed());
			}
		}

		public void TestCreateNewClassPartPivotAdditionalInformationDetailsLayout()
		{
			using (var control = new AdditionalInfosUserControlWithGridForTest())
			{
				AssertType<AdditionalInformationDetailsLayout>(control.CreateNewClassPartPivotAdditionalInformationDetailsLayoutExposed());
			}
		}

		public IReadOnlyList<string> OrderedColumns => new[]
		{
				CusSupportingInfo.Schema.CSI_SubType,
				CusSupportingInfo.Schema.CSI_Code,
				CusSupportingInfo.Schema.CSI_ReferenceNumber,
				CusSupportingInfo.Schema.CSI_Description,
		};

		class AdditionalInfosUserControlWithGridForTest : AdditionalInfosUserControlWithGrid
		{
			public IPanelLayoutProvider CreateNewInvoiceHeaderAdditionalInformationDetailsLayoutExposed() => base.CreateNewInvoiceHeaderAdditionalInformationDetailsLayout();
			public IPanelLayoutProvider CreateNewInvoiceLineAdditionalInformationDetailsLayoutExposed() => base.CreateNewInvoiceLineAdditionalInformationDetailsLayout();
			public IPanelLayoutProvider CreateNewExitSummaryAdditionalInformationDetailsLayoutExposed() => base.CreateNewExitSummaryAdditionalInformationDetailsLayout();
			public IPanelLayoutProvider CreateNewClassPartPivotAdditionalInformationDetailsLayoutExposed() => base.CreateNewClassPartPivotAdditionalInformationDetailsLayout();
		}
	}
}
