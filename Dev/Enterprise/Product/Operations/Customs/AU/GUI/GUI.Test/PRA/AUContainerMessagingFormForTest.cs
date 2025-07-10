using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.PRA.GUI.Testing
{
	sealed partial class AUContainerMessagingFormForTest : ZForm
	{
		public AUContainerMessagingFormForTest()
		{
			InitializeComponent();
		}

		public AUContainerMessagingFormForTest(ContainerNonDependentCollection containerCollection)
			: base(containerCollection)
		{
			ConstructMe(containerCollection);
		}

		void ConstructMe(ContainerNonDependentCollection containerCollection)
		{
			fContainerCollection = containerCollection;
			ZFormPostingButtonsStrategy.SetupPosting(this, oPostingButtonsUserControl);
			PlugIns.AddCurrentDependentPlugIn(ControllerIDs.AUContainerMessaging, zGrid1);
		}

		protected override ZTabControl TopLevelTabControl => zTabControl1;
	}
}
