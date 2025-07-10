using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class BMBufferTimespanForm : ZTemplateForm
	{
		public BMBufferTimespanForm()
		{
			InitializeComponent();
		}

		public BMBufferTimespanForm(BMBufferTimespan businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();
		}

		protected override bool ShowNotesTab => false;
		protected override bool SupportsEDocs => false;
	}
}
