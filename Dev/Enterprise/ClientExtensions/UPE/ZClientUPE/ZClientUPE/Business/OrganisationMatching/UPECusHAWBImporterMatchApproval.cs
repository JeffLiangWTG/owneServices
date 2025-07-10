using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business
{
	public class UPECusHAWBImporterMatchApproval : CusHAWBImporterMatchApproval
	{
		public UPECusHAWBImporterMatchApproval(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new UPECusHAWB Parent
		{
			get { return (UPECusHAWB)base.Parent; }
		}

		protected override void OnMatchApproved(ZGuid orgMatchPK)
		{
			base.OnMatchApproved(orgMatchPK);
			Helper.OnMatchApproved(orgMatchPK);
		}

		protected override ZString ParentOwnerCode
		{
			get { return Helper.ParentOwnerCode; }
		}

		protected override ZString ParentUNLOCO
		{
			get { return Parent.CS_RL_NKDestination; }
		}

		protected override ZString ParentFax
		{
			get { return Helper.ParentFax; }
		}

		public override void CopyDetailsToOrganisation(OrgHeader organisation)
		{
			organisation.MustPerformCheckForDuplicateOrganisations = false;
			base.CopyDetailsToOrganisation(organisation);
		}

		#region Implementation

		OrgMatchApprovalHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new OrgMatchApprovalHelper(this, delegate
					{ return Parent.Level1Record._401000; }, "Importer");
				}
				return fHelper;
			}
		}
		OrgMatchApprovalHelper fHelper;

		#endregion
	}
}
