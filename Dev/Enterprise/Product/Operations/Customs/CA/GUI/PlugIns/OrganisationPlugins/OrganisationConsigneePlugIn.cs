using Enterprise.Customs.CA.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public class OrganisationConsigneePlugIn : ZPlugIn
	{
		public OrganisationConsigneePlugIn(OrgHeader organisation)
			: base(organisation)
		{
			this.organisation = organisation;
		}
		readonly OrgHeader organisation;

		protected override CargoWise.Types.ZBool HasUserControl
		{
			get { return true; }
		}

		protected override System.Windows.Forms.Control GetNewUserControl()
		{
			var label = new ZArchitecture.ZLabel();
			label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			label.Dock = System.Windows.Forms.DockStyle.Fill;
			label.Text = Res.GetString("0c1e83bf-aacb-4b22-8ed7-d5d3683f3896", "Please see Details->Config tab for Canadian specific organization data");
			return label;
		}

		protected override CargoWise.EntityFramework.IBusiness GetBusinessEntityForPlugIn()
		{
			return AddInfo;
		}

		protected override Licensing.LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Core; }
		}

		public override string Name
		{
			get { return "CA Details"; }
		}

		protected override string TextOverride
		{
			get { return "CA Details"; }
		}

		OrgImpAddInfo AddInfo
		{
			get { return addInfo ?? (addInfo = OrgImpAddInfo.Get(organisation)); }
		}
		OrgImpAddInfo addInfo;
	}
}
