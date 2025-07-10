
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.DataImport.Level1FileFormat;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business
{
	public delegate _OrganisationLine GetLevel1RecordOrganisationDelegate();

	public class OrgMatchApprovalHelper
	{
		public OrgMatchApprovalHelper(OrgMatchApproval matchApproval, GetLevel1RecordOrganisationDelegate level1RecordOrganisationDelegate, ZString organisationType)
		{
			this.MatchApproval = matchApproval;
			this.Level1RecordOrganisationDelegate = level1RecordOrganisationDelegate;
			this.OrganisationType = organisationType;
		}

		public void OnMatchApproved(ZGuid orgMatchPK)
		{
			OrgHeader org = (OrgHeader)Factory.Load(typeof(OrgHeader), orgMatchPK);
			if (org != null)
			{
				if (org.MainAddress.OA_Phone.IsEmpty)
				{
					if (Level1RecordOrganisation != null)
					{
						ZString phone = Level1RecordOrganisation.Phone;
						org.MainAddress.OA_Phone = phone.Left(org.MainAddress.OA_PhoneInfo.MaxLength);
					}
				}
				if (org.MainAddress.OA_Fax.IsEmpty)
				{
					org.MainAddress.OA_Fax = ParentFax.Left(org.MainAddress.OA_FaxInfo.MaxLength);
				}
			}
			CusHAWB.LogMatchEvent(false, OrganisationType + ": " + org.OH_Code);
		}

		public ZString ParentOwnerCode
		{
			get
			{
				ZString result = "";
				if (MatchApproval.AddressToBeMatched != null && !MatchApproval.AddressToBeMatched.P3_Code.IsEmpty)
				{
					result = MatchApproval.AddressToBeMatched.P3_Code;
				}
				else if (Level1RecordOrganisation != null)
				{
					result = Level1RecordOrganisation.AccountNumber;
				}
				return result;
			}
		}

		public ZString ParentFax
		{
			get { return Level1RecordOrganisation == null ? ZString.Empty : Level1RecordOrganisation.Fax; }
		}

		#region Implementation

		readonly OrgMatchApproval MatchApproval;
		readonly GetLevel1RecordOrganisationDelegate Level1RecordOrganisationDelegate;
		readonly ZString OrganisationType;

		_OrganisationLine Level1RecordOrganisation
		{
			get { return CusHAWB.Level1Record == null ? null : Level1RecordOrganisationDelegate(); }
		}

		BusinessObjectFactory Factory
		{
			get { return MatchApproval.Factory; }
		}

		UPECusHAWB CusHAWB
		{
			get { return (UPECusHAWB)MatchApproval.Parent; }
		}

		#endregion
	}
}
