using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class FilteredAPInvoicingBaseCollectionViewForModule : BusinessObjectCollectionView<InvoicingBase>
	{
		public FilteredAPInvoicingBaseCollectionViewForModule(InvoicingBaseCollectionForModule collectionToFilter)
			: base(collectionToFilter)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
			=> ((ILegacyBusinessObjectCollectionInternals)collectionToFilter).RelationshipFilter.ShallowClone();

		#region IsThisPartOfTheCollection

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var result = true;
			var invoice = element as InvoicingBase;
			var isAllowedtoViewTransaction = Env.Security.PayablesViewingFinancialOutsideLoginPermission.IsAllowed;
			if (!isAllowedtoViewTransaction && invoice != null && invoice.Branch != null && invoice.Department != null)
			{
				if (invoice.Branch != GlbBranch.CurrentBranch || invoice.Department != GlbDepartment.CurrentDepartment)
				{
					result = AllowedToLogin(invoice.Branch, invoice.Department);
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Combining Cache key, not related to GUI")]
		protected bool AllowedToLogin(GlbBranch branch, GlbDepartment department)
		{
			return Factory.GetCachedValue("Login BRN:" + branch.GB_Code + " DEP:" + department.GE_Code, delegate
			{
				var security = new UserLoginController().GetSecurityForUser(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), department.PK.ToGuid());
				return security.Login.IsAllowed;
			});
		}

		#endregion

		#region Implementation

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException("This collection contains abstract type entity.");
		}

		protected override BusinessObject AddNewCore(Type bizOType)
		{
			throw new NotSupportedException("This collection contains abstract type entity.");
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public override void Load()
		{
			collectionToFilter.Load();
		}

		#endregion
	}
}
