#if DEBUG

using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class InvoicePreviewForm
	{
		public ZGrid InvoicesGrid_ForTestOnly
		{
			get { return InvoicesGrid; }
			set { InvoicesGrid = value; }
		}

		public ZButton PreviewAndDeliverButton_ForTestOnly
		{
			get { return PreviewAndDeliverButton; }
			set { PreviewAndDeliverButton = value; }
		}

		public ZButton PreviewOnlyButton_ForTestOnly
		{
			get { return PreviewOnlyButton; }
			set { PreviewOnlyButton = value; }
		}

		public void Run_ForTestOnly(bool isPreviewOnly)
		{
			Run(isPreviewOnly);
		}
	}
}

#endif
