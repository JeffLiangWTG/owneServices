using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers.Organisations
{
	public sealed class AviationSecurityWrapper : GenericWrapper
	{
		public AviationSecurityWrapper(OrgCountryData countryData, BusinessObjectFactory factory)
			: base(countryData, factory)
		{
			this.countryData = countryData ?? factory.GetNull<OrgCountryData>();
		}

		readonly OrgCountryData countryData;

		#region Properties / Fields

		public CodeAndDescriptionWrapper ApprovalType
		{
			get { return approvalType ?? (approvalType = new CodeAndDescriptionWrapper(countryData.OV_EXApprovedOrMajorExporter, countryData.Lookups.AviationSecuritySchemeMembershipList, Factory)); }
		}
		CodeAndDescriptionWrapper approvalType;

		public ZString ApprovalNumber
		{
			get { return countryData.OV_EXApprovalNumber; }
		}

		public ZDate ExpiryDate
		{
			get { return countryData.OV_EXApprovalExpiryDate; }
		}

		public ZDateTime LastReviewedDate
		{
			get { return countryData.OV_SystemLastEditTimeUtc; }
		}

		public ZString Details
		{
			get { return countryData.OV_EXExportPermissionDetails; }
		}

		public ZString IssuingAuthorityCountry
		{
			get { return countryData.OV_RN_NKIssuingAuthorityCountry; }
		}

		#endregion
	}
}
