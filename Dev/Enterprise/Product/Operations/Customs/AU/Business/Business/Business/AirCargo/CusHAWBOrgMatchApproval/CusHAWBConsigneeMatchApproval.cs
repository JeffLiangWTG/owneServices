using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[TestedAsNonPersistentBusinessObject]
	public class CusHAWBConsigneeMatchApproval : CusHAWBConsigneeConsignorMatchApproval, Integration.Customs.AU.ICusHAWBConsigneeMatchApproval
	{
		public CusHAWBConsigneeMatchApproval(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override void CopyDetailsToOrganisation(OrgHeader organisation)
		{
			base.CopyDetailsToOrganisation(organisation);
			organisation.OH_IsConsignee = true;
		}

		public override OrgMatchApprovalType MatchType
		{
			get { return OrgMatchApprovalType.AirCargoConsignee; }
		}

		protected override void OnMatchApproved(ZGuid orgMatchPK)
		{
			base.OnMatchApproved(orgMatchPK);
			SetCusHAWBConsigneeFKOnMatchApproved(orgMatchPK);
		}

		protected virtual void SetCusHAWBConsigneeFKOnMatchApproved(ZGuid orgMatchPK)
		{
			var address = Factory.Load<OrgHeader>(orgMatchPK)?.MainAddress;
			if (address != null)
			{
				Parent.CS_OA_ConsigneeAddress = address.PK;
			}
		}

		public override ZString OrganisationType
		{
			get { return "Consignee"; }
		}

		protected override ZString ParentOwnerCode
		{
			get { return Parent.CS_OtherSystemConsigneeCode; }
		}

		protected override ZString ParentCompanyName
		{
			get { return Parent.CS_ConsigneeName; }
		}

		protected override ZString ParentStreet
		{
			get { return Parent.CS_ConsigneeStreet; }
		}

		protected override ZString ParentStreet2
		{
			get { return Parent.CS_ConsigneeStreet2; }
		}

		protected override ZString ParentCity
		{
			get { return Parent.CS_ConsigneeCity; }
		}

		protected override ZString ParentUNLOCO
		{
			get { return Parent.CS_RN_NKConsigneeCountry; }
		}

		protected override ZString ParentState
		{
			get { return Parent.CS_ConsigneeState; }
		}

		protected override ZString ParentPostCode
		{
			get { return Parent.CS_ConsigneePostcode; }
		}

		protected override ZString ParentPhone
		{
			get { return Parent.CS_ConsigneePhone; }
		}

		protected override ZString ParentFax
		{
			get { return ""; }
		}
	}
}
