using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[TestedAsNonPersistentBusinessObject]
	public class CusHAWBConsignorMatchApproval : CusHAWBConsigneeConsignorMatchApproval, Integration.Customs.AU.ICusHAWBConsignorMatchApproval
	{
		public CusHAWBConsignorMatchApproval(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override void CopyDetailsToOrganisation(OrgHeader organisation)
		{
			base.CopyDetailsToOrganisation(organisation);
			organisation.OH_IsConsignor = true;
		}

		public override OrgMatchApprovalType MatchType
		{
			get { return OrgMatchApprovalType.AirCargoConsignor; }
		}

		protected override void OnMatchApproved(ZGuid orgMatchPK)
		{
			base.OnMatchApproved(orgMatchPK);
			var address = ApprovedOrgMatch?.MainAddress;
			if (address != null)
			{
				Parent.CS_OA_ConsignorAddress = address.PK;
			}
		}

		public override ZString OrganisationType
		{
			get { return "Consignor"; }
		}

		protected override ZString ParentOwnerCode
		{
			get { return Parent.CS_OtherSystemConsignorCode; }
		}

		protected override ZString ParentCompanyName
		{
			get { return Parent.CS_ConsignorName; }
		}

		protected override ZString ParentStreet
		{
			get { return Parent.CS_ConsignorStreet; }
		}

		protected override ZString ParentStreet2
		{
			get { return Parent.CS_ConsignorStreet2; }
		}

		protected override ZString ParentCity
		{
			get { return Parent.CS_ConsignorCity; }
		}

		protected override ZString ParentUNLOCO
		{
			get { return Parent.CS_RN_NKConsignorCountry; }
		}

		protected override ZString ParentState
		{
			get { return Parent.CS_ConsignorState; }
		}

		protected override ZString ParentPostCode
		{
			get { return Parent.CS_ConsignorPostcode; }
		}

		protected override ZString ParentPhone
		{
			get { return Parent.CS_ConsignorPhone; }
		}

		protected override ZString ParentFax
		{
			get { return ""; }
		}
	}
}
