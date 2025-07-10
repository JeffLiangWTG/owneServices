using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Invoices.Testing
{
	[TestedType(typeof(UnapprovedTransactionFinancialInvoiceDataAdapterTestClass))]
	class UnapprovedTransactionFinancialInvoiceDataAdapterTest : FinancialInvoiceXmlDataAdapterTest
	{
		protected override ValueObjectDataAdapter<InvoicingBase, TxnHeader> GetNewBizObjXmlDataAdapter()
		{
			return new UnapprovedTransactionFinancialInvoiceDataAdapterTestClass();
		}

		public void TestDoesNotAllowResetChargeCodeAndJobOnError()
		{
			ARInvoice aRInvoice = Factory.NewWithValidTestData<ARInvoice>();
			ARInvoiceLine line = (ARInvoiceLine)aRInvoice.Lines.AddNew();

			AccGLHeader clearingAccount = Factory.LoadTop1<AccGLHeader>(new ZQuery());
			AccountingConfigurationRegistry.Instance.GLJournalClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, clearingAccount.PK.ToGuid());
			AccChargeCode postingCharge = Factory.NewWithValidTestData<AccChargeCode>();
			postingCharge.AC_Code = "PJW";

			GlbBranch branchInOtherCompany = GetBranchInAnotherCompany();
			OrgHeader debtor = GetOrgARInThisCompanyAPInOtherCompany(branchInOtherCompany.GB_GC.ToGuid());

			aRInvoice.AH_ExchangeRate = 1;
			aRInvoice.AH_OH = debtor.PK;
			aRInvoice.AH_InvoiceAmount = 100m;
			aRInvoice.AH_OutstandingAmount = 100m;
			aRInvoice.AH_OSTotal = 100m;

			line.AL_OSAmount = 100m;
			line.AL_OSExTaxAmount = 100m;
			line.AL_LineAmount = 100m;
			line.AL_AC = postingCharge.PK;
			Factory.Save();

			UnapprovedTransactionFinancialInvoiceDataAdapter adapter = new UnapprovedTransactionFinancialInvoiceDataAdapter();
			NotificationBuffer exportNotifications = new NotificationBuffer();
			var txnHeader = adapter.ExportToValueObject(aRInvoice, new ValueObjectExportContext(exportNotifications));
			Assert("Should export OK", !exportNotifications.HasErrors);
			AssertEquals(1, txnHeader.TxnLines.Count);
			AssertEquals(100m, txnHeader.TxnLines[0].OsInvoiceAmtExclTax.Value);

			using (branchInOtherCompany.SetAsTemporaryContext())
			{
				UnapprovedTransactionFinancialInvoiceDataAdapter newAdapter = new UnapprovedTransactionFinancialInvoiceDataAdapter();
				APInvoice aPInvoice = Factory.New<APInvoice>();

				NotificationBuffer notify = new NotificationBuffer();
				ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
				newAdapter.RunExtraValidation = false;
				newAdapter.ImportFromValueObject(aPInvoice, txnHeader, context);
				AssertEquals(1, aPInvoice.Lines.Count);
				AssertNotEquals(aPInvoice.Lines[0].AL_AG, clearingAccount.PK);
			}
		}

		GlbBranch GetBranchInAnotherCompany()
		{
			GlbCompany[] otherCompany = Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			Assert(otherCompany.Length > 0);
			GlbBranch branchInOtherCompany = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, otherCompany[0].PK));
			return branchInOtherCompany;
		}

		OrgHeader GetOrgARInThisCompanyAPInOtherCompany(Guid otherCompanyPK)
		{
			ZSqlParameterCollection parameters = new ZSqlParameterCollection();
			OrgHeader result = Factory.Load<OrgHeader>(PKofOrgARARInThisCompanyAPInOtherCompany(otherCompanyPK));
			AssertNotNull("Should be able to find an org that is AP in one company and AR in another", result);
			return result;
		}

		Guid PKofOrgARARInThisCompanyAPInOtherCompany(Guid otherCompanyPK)
		{
			DbCommand cmd = Db.Connection.Command(SQLForOrgARInThisCompanyAPInOtherCompany);
			cmd.AddParameter("@ARCompany", System.Data.SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK.ToGuid());
			cmd.AddParameter("@APCompany", System.Data.SqlDbType.UniqueIdentifier, otherCompanyPK);
			return (Guid)cmd.ExecuteScalar();
		}

		readonly string SQLForOrgARInThisCompanyAPInOtherCompany = @"
							SELECT
								OH_PK	 
							FROM 
								dbo.OrgHeader
								INNER JOIN dbo.OrgCompanyData OrgCompanyDataAR 
									ON 
									OrgCompanyDataAR.OB_OH = OH_PK 
									AND OrgCompanyDataAR.OB_GC = @ARCompany 
								INNER JOIN dbo.OrgCompanyData OrgCompanyDataAP 
									ON 
									OrgCompanyDataAP.OB_OH = OH_PK 
									AND OrgCompanyDataAP.OB_GC = @APCompany 
							WHERE
								OrgCompanyDataAR.OB_IsDebtor = 1
								AND OrgCompanyDataAP.OB_IsCreditor = 1";

		class UnapprovedTransactionFinancialInvoiceDataAdapterTestClass : UnapprovedTransactionFinancialInvoiceDataAdapter
		{
			protected override void SetTxnLineGuid(TxnLine xmlInvoiceLine, InvoicingLineBase invoiceLine)
			{
				xmlInvoiceLine.TxnLineGUID = "lineGUID";
			}
		}
	}
}
