using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Filters;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public partial class OrgLedgerFilterCollection : NonPersistentBusinessObjectCollection<OrgLedgerFilter>	{
		public OrgLedgerFilterCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			OrgLedgerFilter newLedgerFilter = new OrgLedgerFilter(Factory, this);
			return newLedgerFilter;
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			if (elementToDelete is OrgLedgerFilter)
			{
				if (((OrgLedgerFilter)elementToDelete).OrganizationInfo.ReadOnly)
				{
					return;
				}
			}
			base.RemoveAndDelete(elementToDelete);
		}

		#region ContainsOrgPK

		public ZBool ContainsOrgPK(ZGuid orgPK)
		{
			ZBool containsOrg = false;
			foreach (OrgLedgerFilter orgInfo in this)
			{
				if (orgInfo.Organization == orgPK)
				{
					return true;
				}
			}

			return false;
		}

		#endregion

		#region GetLedgerFilterForOrg

		OrgLedgerFilter GetLedgerFilterForOrg(ZGuid orgPK)
		{
			OrgLedgerFilter ledgerFilter = null;
			foreach (OrgLedgerFilter orgInfo in this)
			{
				if (orgInfo.Organization == orgPK)
				{
					ledgerFilter = orgInfo;
				}
			}

			return ledgerFilter;
		}

		#endregion

		#region GuidOccursTwice

		// used in OrganisationAccInfo validation
		public ZBool GuidOccursTwice(ZGuid orgPK)
		{
			if (orgPK.IsValid)
			{
				ZInt noOfOccurrences = 0;
				foreach (OrgLedgerFilter orgInfo in this)
				{
					if (orgInfo.Organization == orgPK)
					{
						noOfOccurrences++;
						if (noOfOccurrences >= 2)
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		#endregion

		#region ArePKCollectionsEqual

		public ZBool ArePKCollectionsEqual(OrgLedgerFilterCollection oldOrgLedgers)
		{
			if (this.Count != oldOrgLedgers.Count)
			{
				return false;	// for efficiency
			}

			ZBool equalResult = true;

			foreach (OrgLedgerFilter oldOrgInfo in oldOrgLedgers)
			{
				OrgLedgerFilter newOrgInfo = this.GetLedgerFilterForOrg(oldOrgInfo.Organization);
				if (newOrgInfo != null)
				{
					if (newOrgInfo.ARLedger != oldOrgInfo.ARLedger ||
						newOrgInfo.APLedger != oldOrgInfo.APLedger)
					{
						equalResult = false;
					}
				}
				else
				{
					equalResult = false;
				}
			}

			return equalResult;
		}

		#endregion

		public OrgLedgerFilterCollection Clone()
		{
			OrgLedgerFilterCollection clonedCollection = new OrgLedgerFilterCollection(Factory);
			foreach (OrgLedgerFilter orgLedger in this)
			{
				if (orgLedger.OrganisationBizO != null)
				{
					OrgLedgerFilter ledgerFilter = clonedCollection.AddNew();
					ledgerFilter.OrganisationBizO = orgLedger.OrganisationBizO;
					ledgerFilter.ARLedger = orgLedger.ARLedger;
					ledgerFilter.APLedger = orgLedger.APLedger;
				}
			}
			return clonedCollection;
		}

		/// <summary>
		/// This method must only be used with Filters that search
		/// OrgHeader DB table
		/// </summary>
		public virtual void LoadOrgs(ZQuery filter, OrgHeader primaryOrg)
		{
			this.RemoveAll();
			if (primaryOrg != null && filter != null)
			{
				OrgHeaderCollection orgHeaders = new OrgHeaderCollection(Factory, filter);
				orgHeaders.Load();

				OrgLedgerFilter primaryOrgAccInfo = this.AddNew();
				primaryOrgAccInfo.OrganisationBizO = primaryOrg;
				//PrimaryOrgAccInfo.Organization = PrimaryOrg.PK;
				primaryOrgAccInfo.Organization_ReadOnly = true;

				foreach (OrgHeader org in orgHeaders)
				{
					if (org.PK != primaryOrg.PK)
					{
						OrgLedgerFilter orgAccInfo = this.AddNew();
						//OrgAccInfo.Organization = Org.PK;
						orgAccInfo.OrganisationBizO = org;
					}
				}
			}
		}

		public void SetAPLedgerOfAllElements(ZBool select)
		{
			foreach (OrgLedgerFilter orgAccInfo in this)
			{
				orgAccInfo.APLedger = select;
			}
		}

		public void SetARLedgerOfAllElements(ZBool select)
		{
			foreach (OrgLedgerFilter orgAccInfo in this)
			{
				orgAccInfo.ARLedger = select;
			}
		}
	}
}
