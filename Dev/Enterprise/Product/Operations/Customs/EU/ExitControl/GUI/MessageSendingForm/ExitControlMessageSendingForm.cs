using System;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public partial class ExitControlMessageSendingForm : MessageSendingFormWithValidationDetails
	{
		[Obsolete("Do not call. Only for designer use.")]
		public ExitControlMessageSendingForm()
		{
		}

		public ExitControlMessageSendingForm(ExitControlMessageSendingObjectParent parent) : base(parent)
		{
		}

		public override string FormHeading => Res.GetString("4DC53183-8FE7-462A-9A0B-F0CB9CAF162D", "Send Exit Control Declaration");

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override ZUserControl GetBottomSectionUserControl()
		{
			var control = new ZUserControl();
			control.Visible = false;
			control.CaptionRenderingEnabled = true;
			return control;
		}

		protected override bool PreviewMessageCheckboxVisible => true;
	}
}
