using System;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.GUI.JobDeclarationForms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using JobDeclaration = Enterprise.Customs.GB.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.GB.GUI
{
	public partial class GBGuaranteesUserControl : GuaranteesUserControl
	{
		protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;
		public GBGuaranteesUserControl()
		{
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			var declaration = Declaration;
			if (declaration != null)
			{
				declaration.JE_ApplicationCodeInfo.ValueChanged += JE_ApplicationCodeInfo_ValueChanged;
				JE_ApplicationCodeInfo_ValueChanged(null, null);
			}
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			var declaration = Declaration;
			if (declaration != null)
			{
				declaration.JE_ApplicationCodeInfo.ValueChanged -= JE_ApplicationCodeInfo_ValueChanged;
			}
			base.OnCurrentDataItemChanging(e);
		}

		void JE_ApplicationCodeInfo_ValueChanged(object sender, EventArgs e)
		{
			var declaration = Declaration;
			GuaranteesGrid.SetAvailability(true, Business.Declaration.GBGuarantee.Schema.EntryInstructionID);
		}

		protected override void GridColumnStyleDecider()
		{
			GuaranteesGrid.GetColumnStyle(GuaranteeForDeclaration.Schema.PW_Password).IsVisible = true;
			GuaranteesGrid.GetColumnStyle(GuaranteeForDeclaration.Schema.PW_HolderIdentification).IsVisible = true;
			GuaranteesGrid.GetColumnStyle(GuaranteeForDeclaration.Schema.PW_BondNumber2).IsVisible = true;
			GuaranteesGrid.GetColumnStyle(GuaranteeForDeclaration.Schema.PW_BondNumber).IsVisible = true;

			GuaranteesGrid.GetColumnStyle(GuaranteeForDeclaration.Schema.PW_BondType).IsVisible = false;
			GuaranteesGrid.GetColumnStyle(GuaranteeForDeclaration.Schema.PW_BondType).IsUnavailable = true;

			GuaranteesGrid.GetColumnStyle(GuaranteeForDeclaration.Schema.PW_BondAmount).IsVisible = false;
			GuaranteesGrid.GetColumnStyle(GuaranteeForDeclaration.Schema.PW_RX_NKCurrency).IsVisible = false;
			GuaranteesGrid.GetColumnStyle(GuaranteeForDeclaration.Schema.PW_BondFiledPort).IsVisible = false;
			GuaranteesGrid.GetColumnStyle(GuaranteeForDeclaration.Schema.PW_ValidityLimitation).IsVisible = false;

			GuaranteesGrid.GetColumnStyle(GuaranteeForDeclaration.Schema.PW_BondNumber2).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(210);
			GuaranteesGrid.GetColumnStyle(GuaranteeForDeclaration.Schema.PW_HolderIdentification).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);

			GuaranteesGrid.GetColumnStyle(GuaranteeForDeclaration.Schema.PW_HolderIdentification).CaptionResourceString = null;
			GuaranteesGrid.GetColumnStyle(GuaranteeForDeclaration.Schema.PW_BondNumber).CaptionResourceString = null;
			GuaranteesGrid.GetColumnStyle(GuaranteeForDeclaration.Schema.PW_BondNumber2).CaptionResourceString = null;
			GuaranteesGrid.GetColumnStyle(GuaranteeForDeclaration.Schema.PW_Password).CaptionResourceString = null;

			GuaranteesGrid.ReOrderColumns(OrderedColumns);
		}

		protected override void AddAndRemoveColumns()
		{
			base.AddAndRemoveColumns();
			var colstyle = GuaranteesGrid.GetColumnStyle(CusBondDetail.Schema.PW_Password);
			GuaranteesGrid.ColumnStyles.Remove(colstyle);
			var accesssPINDropEditColumn = new ZDropEditColumnStyleInfo();
			accesssPINDropEditColumn.ColumnName = CusBondDetail.Schema.PW_Password;
			accesssPINDropEditColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			GuaranteesGrid.ColumnStyles.Add(accesssPINDropEditColumn);
		}

		public void SetCaptions()
		{
			GuaranteesGrid.RefreshColumnCaptions(typeof(GBGuarantee), Declaration?.MultipleKeysToUse);
		}

		string[] OrderedColumns => new string[] {
			GuaranteeForDeclaration.Schema.PW_Password,
			GuaranteeForDeclaration.Schema.PW_HolderIdentification,
			GuaranteeForDeclaration.Schema.PW_BondNumber2,
			GuaranteeForDeclaration.Schema.PW_BondNumber,
			GuaranteeForDeclaration.Schema.EntryInstructionID,
			GuaranteeForDeclaration.Schema.PW_BondAmount,
			GuaranteeForDeclaration.Schema.PW_RX_NKCurrency,
			GuaranteeForDeclaration.Schema.PW_BondFiledPort,
			GuaranteeForDeclaration.Schema.PW_ValidityLimitation
		};
	}
}
