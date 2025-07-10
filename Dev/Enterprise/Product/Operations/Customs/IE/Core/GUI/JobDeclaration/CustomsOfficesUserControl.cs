using System;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public partial class CustomsOfficesUserControl : EU.GUI.CustomsOfficesUserControl
	{
		public CustomsOfficesUserControl()
		{
			InitializeComponent();
			CustomsOfficesGrid.SetColumnWidth(OfficeCode.Schema.CY_OfficeDescription, 156);
			CustomsOfficesGrid.SetAvailability(false, OfficeCode.Schema.CY_Date);
		}

		public JobDeclaration Declaration => (JobDeclaration)JobDeclaration;

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (Declaration is JobDeclaration declaration)
			{
				declaration.JE_MessageTypeInfo.ValueChanged += JE_MessageTypeInfo_ValueChanged;
			}

			JE_MessageTypeInfo_ValueChanged(null, null);
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			if (Declaration is JobDeclaration declaration)
			{
				declaration.JE_MessageTypeInfo.ValueChanged -= JE_MessageTypeInfo_ValueChanged;
			}
		}

		public override void HandleDeclarationControlVisibilityChanged()
		{
			// Do not call base as it will change CustomsOfficeFindBox caption
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (Declaration is JobDeclaration declaration)
				{
					declaration.JE_MessageTypeInfo.ValueChanged -= JE_MessageTypeInfo_ValueChanged;
				}
			}

			base.Dispose(isNotFinalizing);
		}

		void JE_MessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			RefreshCaptionsWithMessageType();
		}

		#region Caption Resource Strings with IMP/EXP

		void RefreshCaptionsWithMessageType()
		{
			CustomsOfficeFindBox.CaptionResourceString = Declaration?.IsExitSummary ?? false ? Enterprise.Customs.IE.GUI.Res.GetData("Enterprise.Customs.IE.Business.Declaration.JobDeclaration|JE_CustomsOffice_IsExitSummary", "Office of Lodgement") : null;
			CustomsOfficeFindBox.UpdateCaption();
		}

		#endregion
	}
}
