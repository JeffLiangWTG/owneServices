using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[TestedAsNonPersistentBusinessObject]
	public class CusHAWBImporterMatchApproval : CusHAWBConsigneeConsignorMatchApproval, Integration.Customs.AU.ICusHAWBImporterMatchApproval
	{
		public CusHAWBImporterMatchApproval(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override OrgMatchApprovalType MatchType
		{
			get { return OrgMatchApprovalType.AirCargoImporter; }
		}

		public override ZString OrganisationType
		{
			get { return "Importer"; }
		}

		protected override ZString ParentOwnerCode
		{
			get { return AddressToBeMatched.P3_Code; }
		}

		protected override ZString ParentCompanyName
		{
			get { return AddressToBeMatched.P3_CompanyName; }
		}

		protected override ZString ParentStreet
		{
			get { return AddressToBeMatched.P3_Address1; }
		}

		protected override ZString ParentStreet2
		{
			get { return AddressToBeMatched.P3_Address2; }
		}

		protected override ZString ParentCity
		{
			get { return AddressToBeMatched.P3_City; }
		}

		protected override ZString ParentUNLOCO
		{
			get { return Parent.CS_RN_NKConsigneeCountry; }
		}

		protected override ZString ParentState
		{
			get { return AddressToBeMatched.P3_State; }
		}

		protected override ZString ParentPostCode
		{
			get { return AddressToBeMatched.P3_PostCode; }
		}

		protected override ZString ParentPhone
		{
			get { return AddressToBeMatched.P3_Phone; }
		}

		protected override ZString ParentFax
		{
			get { return AddressToBeMatched.P3_Fax; }
		}
	}
}
