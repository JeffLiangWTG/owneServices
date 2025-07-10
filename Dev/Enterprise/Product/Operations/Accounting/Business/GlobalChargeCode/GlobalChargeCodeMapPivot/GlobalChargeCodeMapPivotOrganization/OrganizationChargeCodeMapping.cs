using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GlobalChargeCode
{
	public class OrganizationChargeCodeMapping : NonPersistentBusinessObject, IObsoleteValidation
	{
		public OrganizationChargeCodeMapping(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Properties

		#region Organization_PK

		[List("Headers")]
		public ZGuid Organization_PK
		{
			get { return fOrganization_PK; }
			set
			{
				SetNonPersistentPropertyValue(Organization_PKInfo, ref fOrganization_PK, value);
				ChargeCodePivotCollection.RefreshBindingIncludingChildren();
			}
		}
		ZGuid fOrganization_PK;

		public ZPropertyInfo Organization_PKInfo
		{
			get { return GetZPropertyInfo(nameof(Organization_PK)); }
		}

		#endregion

		#region ChargeCodePivotCollection

		public GlobalChargeCodeMapPivotOrganizationCollection ChargeCodePivotCollection
		{
			get
			{
				if (fChargeCodePivotCollection == null)
				{
					fChargeCodePivotCollection = new GlobalChargeCodeMapPivotOrganizationCollection(Factory);
					fChargeCodePivotCollection.SetReadOnlyIncludingChildren(true);
					RegisterEditableChildObject(fChargeCodePivotCollection);
				}
				if (CurrentOrganization_PK != Organization_PK)
				{
					CurrentOrganization_PK = Organization_PK;
					fChargeCodePivotCollection.OrganisationPK = CurrentOrganization_PK;
					fChargeCodePivotCollection.Load(GetOrganizationFilter(CurrentOrganization_PK));
					fChargeCodePivotCollection.SetReadOnlyIncludingChildren(!CurrentOrganization_PK.IsValid);
				}

				return fChargeCodePivotCollection;
			}
		}
		GlobalChargeCodeMapPivotOrganizationCollection fChargeCodePivotCollection;
		ZGuid CurrentOrganization_PK;

		protected bool ChargeCodePivotCollection_ReadOnly
		{
			get { return Organization_PK.IsEmpty; }
		}

		ZQuery GetOrganizationFilter(ZGuid organisationPK)
		{
			ZDBOnlyQuery organizationFilter = new ZDBOnlyQuery(typeof(GlobalChargeCodeMapPivot));
			ZDBOnlySubQuery organizationFilterSubQuery = new ZDBOnlySubQuery(typeof(GlobalChargeCodeMap), AccGlobalChargeCodeMapSchema.PK);
			organizationFilterSubQuery.AddToFilter(AccGlobalChargeCodeMapSchema.YG_OH, organisationPK);
			organizationFilter.AddSubQuery(AccGlobalChargeCodeMapPivotSchema.YP_YG, organizationFilterSubQuery, JoinCondition.And);
			return organizationFilter;
		}

		#endregion

		#endregion

		#region Lookups

		public OrgHeaderCollection Headers
		{
			get
			{
				return new OrgHeaderCollection(Factory);
			}
		}

		#endregion

	}
}

