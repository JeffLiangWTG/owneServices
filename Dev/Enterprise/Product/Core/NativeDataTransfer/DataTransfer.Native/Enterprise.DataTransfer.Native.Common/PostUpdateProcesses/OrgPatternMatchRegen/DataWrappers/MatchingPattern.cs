using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Common.PostUpdateProcesses.OrgPatternMatchRegen
{
	[TableName(OrgPatternMatchSchema.Constants.TableName)]
	class MatchingPattern : Wrapper, IOrgPatternMatch
	{
		public ZString OS_Address1
		{
			get { return GetValue(OrgPatternMatchSchema.OS_Address1); }
			set { SetValue(OrgPatternMatchSchema.OS_Address1, value); }
		}

		public ZString OS_Address2
		{
			get { return GetValue(OrgPatternMatchSchema.OS_Address2); }
			set { SetValue(OrgPatternMatchSchema.OS_Address2, value); }
		}

		public ZString OS_Address3
		{
			get { return GetValue(OrgPatternMatchSchema.OS_Address3); }
			set { SetValue(OrgPatternMatchSchema.OS_Address3, value); }
		}

		public ZString OS_Address4
		{
			get { return GetValue(OrgPatternMatchSchema.OS_Address4); }
			set { SetValue(OrgPatternMatchSchema.OS_Address4, value); }
		}

		public ZString OS_BusinessRegNo
		{
			get { return GetValue(OrgPatternMatchSchema.OS_BusinessRegNo); }
			set { SetValue(OrgPatternMatchSchema.OS_BusinessRegNo, value); }
		}

		public ZString OS_City
		{
			get { return GetValue(OrgPatternMatchSchema.OS_City); }
			set { SetValue(OrgPatternMatchSchema.OS_City, value); }
		}

		public ZString OS_CompanyName1
		{
			get { return GetValue(OrgPatternMatchSchema.OS_CompanyName1); }
			set { SetValue(OrgPatternMatchSchema.OS_CompanyName1, value); }
		}

		public ZString OS_CompanyName2
		{
			get { return GetValue(OrgPatternMatchSchema.OS_CompanyName2); }
			set { SetValue(OrgPatternMatchSchema.OS_CompanyName2, value); }
		}

		public ZString OS_CompanyName3
		{
			get { return GetValue(OrgPatternMatchSchema.OS_CompanyName3); }
			set { SetValue(OrgPatternMatchSchema.OS_CompanyName3, value); }
		}

		public ZString OS_CompanyName4
		{
			get { return GetValue(OrgPatternMatchSchema.OS_CompanyName4); }
			set { SetValue(OrgPatternMatchSchema.OS_CompanyName4, value); }
		}

		public ZString OS_Domain
		{
			get { return GetValue(OrgPatternMatchSchema.OS_Domain); }
			set { SetValue(OrgPatternMatchSchema.OS_Domain, value); }
		}

		public ZString OS_Email
		{
			get { return GetValue(OrgPatternMatchSchema.OS_Email); }
			set { SetValue(OrgPatternMatchSchema.OS_Email, value); }
		}

		public ZString OS_FaxNum
		{
			get { return GetValue(OrgPatternMatchSchema.OS_FaxNum); }
			set { SetValue(OrgPatternMatchSchema.OS_FaxNum, value); }
		}

		public ZString OS_FullCompanyName
		{
			get { return GetValue(OrgPatternMatchSchema.OS_FullCompanyName); }
			set { SetValue(OrgPatternMatchSchema.OS_FullCompanyName, value); }
		}

		public ZBool OS_IsCorporation
		{
			get { return GetValue(OrgPatternMatchSchema.OS_IsCorporation); }
			set { SetValue(OrgPatternMatchSchema.OS_IsCorporation, value); }
		}

		public ZBool OS_IsPOBox
		{
			get { return GetValue(OrgPatternMatchSchema.OS_IsPOBox); }
			set { SetValue(OrgPatternMatchSchema.OS_IsPOBox, value); }
		}

		public ZGuid OS_OA
		{
			get { return GetValue(OrgPatternMatchSchema.OS_OA); }
			set { SetValue(OrgPatternMatchSchema.OS_OA, value); }
		}

		public ZGuid OS_OH
		{
			get { return GetValue(OrgPatternMatchSchema.OS_OH); }
			set { SetValue(OrgPatternMatchSchema.OS_OH, value); }
		}

		public ZString OS_POBoxNumber
		{
			get { return GetValue(OrgPatternMatchSchema.OS_POBoxNumber); }
			set { SetValue(OrgPatternMatchSchema.OS_POBoxNumber, value); }
		}

		public ZString OS_Phone
		{
			get { return GetValue(OrgPatternMatchSchema.OS_Phone); }
			set { SetValue(OrgPatternMatchSchema.OS_Phone, value); }
		}

		public ZString OS_PostCode
		{
			get { return GetValue(OrgPatternMatchSchema.OS_PostCode); }
			set { SetValue(OrgPatternMatchSchema.OS_PostCode, value); }
		}

		public ZString OS_State
		{
			get { return GetValue(OrgPatternMatchSchema.OS_State); }
			set { SetValue(OrgPatternMatchSchema.OS_State, value); }
		}

		public ZString OS_StreetNumber
		{
			get { return GetValue(OrgPatternMatchSchema.OS_StreetNumber); }
			set { SetValue(OrgPatternMatchSchema.OS_StreetNumber, value); }
		}

		public ZString OS_UNLOCO
		{
			get { return GetValue(OrgPatternMatchSchema.OS_UNLOCO); }
			set { SetValue(OrgPatternMatchSchema.OS_UNLOCO, value); }
		}

		protected override SchemaPKColumn PKSchemaColumn
		{
			get { return OrgPatternMatchSchema.PK; }
		}
	}
}
