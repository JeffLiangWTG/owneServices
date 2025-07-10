using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public class CPQAPlugIn : ZPlugIn
	{
		public CPQAPlugIn(OrgHeader hostBusinessEntity) : base(hostBusinessEntity)
		{
		}

		#region Override

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Broker; }
		}

		public override string Name
		{
			get { return "Default CP Questions and Answers"; }
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return OrgCPQA;
		}

		protected override Control GetNewUserControl()
		{
			Control result = new CPQACollectionUserControl();
			result.Dock = DockStyle.Fill;
			return result;
		}

		#endregion

		protected OrgHeader Organisation
		{
			get { return (OrgHeader)HostBusinessEntity; }
		}

		public OrganisationCPQA OrgCPQA
		{
			get
			{
				if (fOrgCPQA == null)
				{
					fOrgCPQA = new OrganisationCPQA(Organisation);
				}
				return fOrgCPQA;
			}
		}
		OrganisationCPQA fOrgCPQA;
	}
}
