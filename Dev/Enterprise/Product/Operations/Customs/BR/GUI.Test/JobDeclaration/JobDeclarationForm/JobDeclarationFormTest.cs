using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.BR.GUI.Testing
{
	public class JobDeclarationFormTest : TestCaseWithFactory
	{
		public void TestGetNewMessagingActionsController()
		{
			using (var form = new JobDeclarationFormForTesting(Factory.New<JobDeclaration>()))
			{
				var control = form.GetNewMessagingActionsController();
				AssertType<SendsMessagesToCustomsGUI>(control);
			}
		}

		public void TestPlugins()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			using (var form = new JobDeclarationForm(dec))
			{
				form.Show();
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.Routing));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.LandedCosting));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocAddresses));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.CartagePlugin));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DtbBooking));
			}

			dec.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			using (var form = new JobDeclarationForm(dec))
			{
				form.Show();
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.Routing));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.LandedCosting));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocAddresses));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
				AssertNull(form.PlugIns.GetPlugIn(ControllerIDs.CartagePlugin));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DtbBooking));
			}

			dec.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
			using (var form = new JobDeclarationForm(dec))
			{
				form.Show();
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.Routing));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.LandedCosting));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocAddresses));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
				AssertNull(form.PlugIns.GetPlugIn(ControllerIDs.CartagePlugin));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DtbBooking));
			}
		}
	}

	class JobDeclarationFormForTesting : JobDeclarationForm
	{
		public JobDeclarationFormForTesting(JobDeclaration declaration) : base(declaration)
		{
		}

		public new Customs.GUI.SendsMessagesToCustomsGUI GetNewMessagingActionsController() => base.GetNewMessagingActionsController();
	}
}
