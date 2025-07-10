using Enterprise.DocumentEngine;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.SWT.GUI
{
	partial class SWTPrinterSelectionForm : ZChildForm
	{
		public SWTPrinterSelectionForm(DeliveryInstructions instructions) : base(instructions)
		{
		}

		public override string FormCaption
		{
			get { return "'Not Yet Arrived' Report"; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
