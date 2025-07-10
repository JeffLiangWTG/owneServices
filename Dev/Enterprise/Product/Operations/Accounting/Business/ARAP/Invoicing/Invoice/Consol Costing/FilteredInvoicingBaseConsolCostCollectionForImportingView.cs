using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class FilteredInvoicingBaseConsolCostCollectionForImportingView : BusinessObjectCollectionView<InvoicingBaseConsolCostForImporting>
	{
		public FilteredInvoicingBaseConsolCostCollectionForImportingView(InvoicingBaseConsolCostCollectionForImporting collectionToFilter)
			: base(collectionToFilter)
		{
		}

		#region IsThisPartOfTheCollection

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var consolcost = element as InvoicingBaseConsolCostForImporting;
			if (consolcost != null && consolcost.ApportionmentCharges.Cast<BaseCharge>().Any(x => !IsAllowedtoViewCostOutsideLoginPermission(x)))
			{
				return false;
			}
			return true;
		}

		protected bool AllowedToLogin(GlbBranch branch, GlbDepartment department)
		{
			return Factory.GetCachedValue($"Login BRN:{branch.GB_Code} DEP:{department.GE_Code}", delegate
			{
				var security = new UserLoginController().GetSecurityForUser(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), department.PK.ToGuid());
				return security.Login.IsAllowed;
			});
		}

		protected bool IsAllowedtoViewCostOutsideLoginPermission(BaseCharge charge)
		{
			var result = true;
			if (charge != null && !Env.Security.PayablesViewingFinancialOutsideLoginPermission.IsAllowed && charge.Branch != null && charge.Department != null)
			{
				if (charge.Branch != GlbBranch.CurrentBranch || charge.Department != GlbDepartment.CurrentDepartment)
				{
					result = AllowedToLogin(charge.Branch, charge.Department);
				}
			}
			return result;
		}

		#endregion

		#region Implementation

		protected override bool AllowNewCore
		{
			get { return collectionToFilter.AllowNew; }
		}

		protected override bool AllowRemoveCore
		{
			get { return collectionToFilter.AllowRemove; }
		}

		#endregion
	}
}
