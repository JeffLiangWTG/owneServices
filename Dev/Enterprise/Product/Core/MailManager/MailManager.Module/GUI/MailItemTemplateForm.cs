using System;
using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MailManager.GUI
{
	public partial class MailItemTemplateForm : ZForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public MailItemTemplateForm()
		{
			InitializeComponent();
		}

		public MailItemTemplateForm(MailItemTemplate bo)
			: base(bo)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
		}

		#region FormCaption

		public override string FormCaption
		{
			get { return BusinessEntity != null ? ((MailItemTemplate)BusinessEntity).HumanReadableShortcutName.ToString() : base.FormCaption; }
		}

		#endregion
	}
}
