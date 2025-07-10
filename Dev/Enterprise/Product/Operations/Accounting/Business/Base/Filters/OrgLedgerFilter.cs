using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Base.Filters
{
	public class OrgLedgerFilter : NonPersistentBusinessObject, IObsoleteValidation
	{
		public OrgLedgerFilter(BusinessObjectFactory factory, OrgLedgerFilterCollection settlementOrgs)
			: base(factory)
		{
			fSettlementOrgs = settlementOrgs;
		}

		protected OrgLedgerFilterCollection fSettlementOrgs;

		#region Organization

		[List("OrgHeaders")]
		public ZGuid Organization
		{
			get { return fOrganization; }
			set
			{
				fOrganization = value;
				OrganizationInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateOrganization();
				}
				if (!OrganizationInfo.HasErrors())
				{
					OrgHeader orgBizO = Factory.Load<OrgHeader>(Organization);
					if (orgBizO != null)
					{
						OrganisationBizO = orgBizO;
					}
				}
			}
		}
		protected ZGuid fOrganization;

		public ZPropertyInfo OrganizationInfo
		{
			get { return GetZPropertyInfo(nameof(Organization)); }
		}

		public bool Organization_ReadOnly { get; set; }

		#endregion

		#region OrganisationBizO

		public OrgHeader OrganisationBizO
		{
			get { return fOrganisationBizO; }
			set
			{
				fOrganization = value.PK;
				APLedger = value.OH_IsCreditor;
				ARLedger = value.OH_IsDebtor;
				fOrganisationBizO = value;
			}
		}
		OrgHeader fOrganisationBizO;

		#endregion

		#region APLedger

		public ZBool APLedger
		{
			get { return fAPLedger; }
			set
			{
				fAPLedger = value;
				APLedgerInfo.RefreshBinding();
			}
		}
		protected ZBool fAPLedger;

		public ZPropertyInfo APLedgerInfo
		{
			get { return GetZPropertyInfo(nameof(APLedger)); }
		}

		#endregion

		#region ARLedger

		public ZBool ARLedger
		{
			get { return fARLedger; }
			set
			{
				fARLedger = value;
				ARLedgerInfo.RefreshBinding();
			}
		}
		protected ZBool fARLedger;

		public ZPropertyInfo ARLedgerInfo
		{
			get { return GetZPropertyInfo(nameof(ARLedger)); }
		}

		#endregion

		#region OrgHeaders

		public OrgHeaderCollection OrgHeaders
		{
			get
			{
				if (fOrgHeaders == null)
				{
					fOrgHeaders = new DebtorOrCreditorCollection(new BusinessObjectFactory());
					fOrgHeaders.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property0", ZBool.True));
					fOrgHeaders.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property1", ZBool.True));
					fOrgHeaders.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "OrJoinCondition", ZBool.True));
					fOrgHeaders.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "OrJoinCondition", ZBool.False));
				}
				return fOrgHeaders;
			}
		}
		protected OrgHeaderCollection fOrgHeaders;

		#endregion

		#region Validation

		public virtual void ValidateOrganization()
		{
			OrganizationInfo.ClearAllNotifications();
			if (fSettlementOrgs.GuidOccursTwice(Organization))
			{
				OrganizationInfo.AddError(Res.GetString("e3a9c883-622c-4cea-842f-42b78be88020", "This organization has already been chosen, please choose another one"));
			}
			if (!Organization.IsValid)
			{
				OrganizationInfo.AddError(Res.GetString("7944cc6f-9b96-438a-880d-688e3a1669db", "Please enter a valid creditor/debtor account"));
			}
		}

		#endregion
	}
}
