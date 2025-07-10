using CargoWise.Application;
using Enterprise.BufferManagement.Integration;
using Enterprise.PAVE.MENT.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.PAVE.MENT.GUI
{
	public partial class MENTAgedScoreQueryForm : ZTemplateForm
	{
		public MENTAgedScoreQueryForm(MENTAgedScoreQuery businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();
			VisualisationConfigurationTabPage.TabVisible = ObjectFactory.Get<IBMSRegistry>().EnableMENTSections;
		}

		public MENTAgedScoreQueryForm()
		{
			InitializeComponent();
			VisualisationConfigurationTabPage.TabVisible = ObjectFactory.Get<IBMSRegistry>().EnableMENTSections;
		}

		protected override bool SupportsEDocs
		{
			get { return true; }
		}
	}
}
