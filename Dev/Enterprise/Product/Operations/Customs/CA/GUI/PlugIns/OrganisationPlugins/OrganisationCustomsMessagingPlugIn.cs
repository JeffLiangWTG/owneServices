using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public class OrganisationCustomsMessagingPlugIn : CustomsPlugIn
	{
		public OrganisationCustomsMessagingPlugIn(ICustomsMessagingPlugInSupport plugInSupport)
			: base(plugInSupport.Master)
		{
			Argument.NotNull(plugInSupport, "plugInSupport");
			PlugInSupport = plugInSupport;
			Organisation = OrgHeaderTCPMessageWrapper.New(plugInSupport.Master as OrgHeader);
			plugInSupport.PlugInVisibilityDataChanged += ChangeTheVisibility;
			ChangeTheVisibility();
		}

		void ChangeTheVisibility(object sender, System.EventArgs e)
		{
			ChangeTheVisibility();
		}

		#region Overrides of ZPlugIn

		public override string Name
		{
			get { return Res.GetString("36bdc3a7-be13-4281-a171-501017b59505", "Customs Messaging"); }
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Core; }
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				PlugInSupport.PlugInVisibilityDataChanged -= ChangeTheVisibility;
			}

			base.Dispose(disposing);
		}

		#endregion

		#region Overrides of CustomsPlugIn

		protected override Control GetNewUserControl()
		{
			return new OrganisationCustomsMessagingPlugInUserControl();
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			return new OrganisationCustomsMessagingPlugInMenu(Organisation);
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return Organisation;
		}

		protected override sealed void ChangeTheVisibilityCore()
		{
			Enabled = PlugInSupport.PlugInVisible;
		}

		#endregion

		protected readonly OrgHeaderTCPMessageWrapper Organisation;
		protected internal readonly ICustomsMessagingPlugInSupport PlugInSupport;
	}
}
