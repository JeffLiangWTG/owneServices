using System;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.OrgPatternMatching;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Common.PostUpdateProcesses.OrgPatternMatchRegen
{
	[TableName(OrgAddressSchema.Constants.TableName)]
	class MatchingAddress : Wrapper, IMatchingAddress
	{
		#region Data Row Properties

		public ZString OA_Address1
		{
			get { return GetValue(OrgAddressSchema.OA_Address1); }
		}

		public ZString OA_Address2
		{
			get { return GetValue(OrgAddressSchema.OA_Address2); }
		}

		public ZString OA_City
		{
			get { return GetValue(OrgAddressSchema.OA_City); }
		}

		public ZString OA_Code
		{
			get { return GetValue(OrgAddressSchema.OA_Code); }
		}

		public ZString OA_CompanyNameOverride
		{
			get { return GetValue(OrgAddressSchema.OA_CompanyNameOverride); }
		}

		public ZString OA_Email
		{
			get { return GetValue(OrgAddressSchema.OA_Email); }
		}

		public ZString OA_Fax
		{
			get { return GetValue(OrgAddressSchema.OA_Fax); }
		}

		public ZBool OA_IsActive
		{
			get { return GetValue(OrgAddressSchema.OA_IsActive); }
		}

		public ZString OA_Language
		{
			get { return GetValue(OrgAddressSchema.OA_Language); }
		}

		public ZString OA_Mobile
		{
			get { return GetValue(OrgAddressSchema.OA_Mobile); }
		}

		public ZString OA_Phone
		{
			get { return GetValue(OrgAddressSchema.OA_Phone); }
		}

		public ZString OA_PostCode
		{
			get { return GetValue(OrgAddressSchema.OA_PostCode); }
		}

		public ZString OA_RL_NKRelatedPortCode
		{
			get { return GetValue(OrgAddressSchema.OA_RL_NKRelatedPortCode); }
			set { SetValue(OrgAddressSchema.OA_RL_NKRelatedPortCode, value); }
		}
		public ZString OA_ValidationStatus
		{
			get { return GetValue(OrgAddressSchema.OA_ValidationStatus); }
			set { SetValue(OrgAddressSchema.OA_ValidationStatus, value); }
		}

		public ZBool OA_RL_NKRelatedPortCodeInfoHasChanges
		{
			get { throw new NotImplementedException(); }
		}

		public ZString OA_State
		{
			get { return GetValue(OrgAddressSchema.OA_State); }
		}

		protected override SchemaPKColumn PKSchemaColumn
		{
			get { return OrgAddressSchema.PK; }
		}

		#endregion

		#region Main Address Management - Should not be required.

		public void SetMainAddress()
		{
			throw new NotImplementedException();
		}

		public bool IsMainAddress
		{
			get { throw new NotImplementedException(); }
		}

		#endregion

		public IDisposable CachePortAndCountryNames()
		{
			return null;
		}

		public ZString PortName
		{
			get { return Factory.GetPortInfo(OA_RL_NKRelatedPortCode).PortName; }
		}

		public MultilingualString CountryName
		{
			get { return (NoResString)Factory.GetPortInfo(OA_RL_NKRelatedPortCode).CountryName; }
		}

		public ZString Contact { get; set; }

		public ZString CountryCode { get; set; }

		public ZString OA_AdditionalAddressInformation { get; set; }
	}
}
