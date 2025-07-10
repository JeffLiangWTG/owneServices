using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.EmailNotification
{
	public class ComplianceNumberAllocationFailureEmail : AccountingEmailDef
	{
		public ComplianceNumberAllocationFailureEmail(InvoicingBase invoice)
		{
			ContentType = EmailContentTypes.HTML;
			this.TransactionNum = invoice.AH_TransactionNum;
			this.SubType = invoice.AH_ComplianceSubType;
		}

		#region Implementation

		readonly string TransactionNum;
		readonly string SubType;

		#region Overrides

		protected override GuidRegistryItem Recipient
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.ComplianceInvoiceBookAllocaltionFailureNotificationGroup;
			}
		}

		protected override string GetBody()
		{
			return Res.GetString("f7993145-57fe-44fa-a4e3-7bc531e4055f", @"Please review your Compliance Invoice Book setups for Branch {0} and Sub Type {1} in the Maintain > Account > Compliance Sequences module.
Whilst logged in to Branch {0}  User {2} attempted to assign a Compliance Number against transaction {3}.
This assignment could not be made because an Active Compliance Book for Branch {0} and Sub Type {1} did not exist.
If required, please configure / activate a new Compliance Invoice Book.",
					GlbBranch.CurrentBranch.GB_Code,
					SubType,
					Env.CurrentUser.FullName,
					TransactionNum);
		}

		protected override string GetSubject()
		{
			return Res.GetString("236b1a13-62be-4f98-9f58-cfeddabb07d6", "Compliance Invoice Book Allocation Failure - Branch {0} / Sub Type {1}", GlbBranch.CurrentBranch.GB_Code, SubType);
		}

		#endregion

		#endregion
	}
}