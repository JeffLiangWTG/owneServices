using System;
using Enterprise.Customs.GB.CDS;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.CDSDIS
{
	public partial class CDSDISQueryForm : ZTemplateForm
	{
		public CDSDISQueryForm(CDSDISQueryMessage businessEntity) : base(businessEntity)
		{
			InitializeComponent();
			message = businessEntity;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (message != null && message.ReadOnly)
			{
				DeclarationCategoryTextBox.Visible = false;
				DeclarationStatusTextBox.Visible = false;
				DateFrom.Visible = false;
				DateTo.Visible = false;
				PageNumber.Visible = false;
			}
		}

		readonly CDSDISQueryMessage message;

		protected override bool SupportsEDocs => false;
	}
}
