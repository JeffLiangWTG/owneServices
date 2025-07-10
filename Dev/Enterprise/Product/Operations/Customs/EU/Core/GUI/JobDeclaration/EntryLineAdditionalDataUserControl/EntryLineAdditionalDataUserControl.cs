using System;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class EntryLineAdditionalDataUserControl : ZUserControl
	{
		public EntryLineAdditionalDataUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DutyAndTaxDetails.UserControlType = GetDutyAndTaxDetailsUserControlType();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			ChangeControlsVisibility();
		}

		protected virtual Type GetDutyAndTaxDetailsUserControlType() => typeof(EntryLineTaxAndFeeUserControl);

		void ChangeControlsVisibility()
		{
			var declaration = (JobDeclaration)CurrentDataItem;
			SupportingDocumentsTabPage.TabVisible = declaration.Configuration.EntryLineConfiguration.SupportingDocumentsSupport(declaration);
		}
	}
}
