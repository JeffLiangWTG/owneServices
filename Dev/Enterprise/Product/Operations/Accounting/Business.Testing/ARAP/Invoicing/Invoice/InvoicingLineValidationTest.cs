using System;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public abstract class InvoicingLineValidationTest : InvoicingLineBaseValidationTest
	{
		protected abstract InvoiceLineValidation GetValidation(InvoiceLine parent);

		public void TestValidateGSTTrue()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = ZBool.True;
			InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);
			SetAllowUserToModifyGST(invoice, ZBool.True);

			invoice.Lines.AddNew();
			invoice.AH_OH = GSTRegisteredOrg.PK;

			InvoiceLineValidation testValidation = GetValidation((InvoiceLine)invoice.Lines[0]);
			testValidation.ValidateAL_AT();
			AssertHasErrors(invoice.Lines[0].AL_ATInfo);
		}

		public void TestValidateGSTFalse()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = ZBool.True;
			InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);
			SetAllowUserToModifyGST(invoice, ZBool.False);

			invoice.Lines.AddNew();
			invoice.AH_OH = NonGSTRegisteredOrg.PK;

			InvoiceLineValidation testValidation = GetValidation((InvoiceLine)invoice.Lines[0]);
			testValidation.ValidateAL_AT();
			AssertNoErrors(invoice.Lines[0].AL_ATInfo);
		}

		void SetAllowUserToModifyGST(InvoicingBase invoice, ZBool modify)
		{
			if (invoice.AH_Ledger == LedgerTypes.AccountsPayable || invoice.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
			{
				AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, modify);
			}
			else if (invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, modify);
			}
		}

		#region Implementation

		OrgHeader fNonWHTRegisteredOrg;
		protected OrgHeader NonWHTRegisteredOrg
		{
			get
			{
				if (fNonWHTRegisteredOrg == null)
				{
					fNonWHTRegisteredOrg = TestObjectCreator.CreateOrgHeader("TSTNONWHT", true, true, true, false, true, false);
				}
				return fNonWHTRegisteredOrg;
			}
		}

		#endregion
	}
}
