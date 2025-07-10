using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	class DraftInvoiceReadonlyHelper : Assertion
	{
		public void AssertReadOnlyPropertiesForAssociatedDraftInvoice(InvoicingBase invoicingBase, bool couldHaveAssociatedDraftInvoice, string[] alwaysReadOnlyProperties)
		{ 
			AssertEquals("PreCondtion, Transaction is not associated"
				, expected: false
				, invoicingBase.Factory.Exists(typeof(AccDraftInvoiceHeader), new ZQuery(AccDraftInvoiceHeaderSchema.AIH_AH_PostedTransactionHeader, invoicingBase.PK))
			);
			AssertReadOnlyProperties("PreCondtion, properties are not readonly", invoicingBase, alwaysReadOnlyProperties, expectedReadOnlyValue: false);

			var draftInvoice = invoicingBase.Factory.NewWithValidTestData<AccDraftInvoiceHeader>();
			draftInvoice.AIH_AH_PostedTransactionHeader = invoicingBase.PK;

			invoicingBase.ClearReadOnlyForAssociatedDraftInvoice_ForTestOnly();
			if (couldHaveAssociatedDraftInvoice)
			{
				Globals.IsUserInteractive = false;
				AssertReadOnlyProperties("Readonly logic is not applies when it is not user interactive.", invoicingBase, alwaysReadOnlyProperties, expectedReadOnlyValue: false);

				Globals.IsUserInteractive = true;
				AssertReadOnlyProperties("Draft invoice assocaited properties should be readonly", invoicingBase, alwaysReadOnlyProperties, expectedReadOnlyValue: true);

				invoicingBase.SaveAsIncomplete();
				invoicingBase.Factory.Save();

				var incompleteTransaction = (InvoicingBase)new BusinessObjectFactory().Load(invoicingBase.GetType(), invoicingBase.PK);

				AssertReadOnlyProperties("Incomplete invoice with draft invoice assocaited properties should be readonly", incompleteTransaction, alwaysReadOnlyProperties, expectedReadOnlyValue: true);
			}
			else
			{
				AssertReadOnlyProperties("Properties should not be readonly", invoicingBase, alwaysReadOnlyProperties, expectedReadOnlyValue: false);
			}
		}

		void AssertReadOnlyProperties(string comment, InvoicingBase invoicingBase, string[] alwaysReadOnlyProperties, bool expectedReadOnlyValue)
		{
			CombineAssertions(comment, () => {
				foreach (var propertyName in draftInvoiceProperties.Except(alwaysReadOnlyProperties))
				{ 
					AssertEquals(propertyName, expectedReadOnlyValue, invoicingBase.FindPropertyInfo(propertyName).ReadOnly);
				}

				var expectedReadOnlyForOriginalTransaction = !invoicingBase.ShouldShowOriginalInvoiceReferenceFields || expectedReadOnlyValue;

				foreach (var propertyName in OriginalTransactionProperties)
				{
					AssertEquals(propertyName, expectedReadOnlyForOriginalTransaction, invoicingBase.FindPropertyInfo(propertyName).ReadOnly);
				}

				foreach (var propertyName in alwaysReadOnlyProperties)
				{
					AssertEquals(propertyName, true, invoicingBase.FindPropertyInfo(propertyName).ReadOnly);
				}
			});
		}

		string[] draftInvoiceProperties { get; } = new[] {
			nameof(InvoicingBase.DisplayInvoiceContactOverride),
			nameof(InvoicingBase.DisplayInvoiceAddressOverrideForAddressControl),
			nameof(InvoicingBase.AH_InvoiceDate),
			nameof(InvoicingBase.AH_DocumentReceivedDate),
			nameof(InvoicingBase.AH_PostDate),
			nameof(InvoicingBase.AH_DueDate),
			nameof(InvoicingBase.AH_TransactionNum),
			nameof(InvoicingBase.IsSelfBillingInvoice),
			nameof(InvoicingBase.AH_Desc),
			nameof(InvoicingBase.AH_RX_NKTransactionCurrency),
			nameof(InvoicingBase.ValidateExpectedInvoiceTotal),
			nameof(InvoicingBase.ExpectedInvoiceTotal),
			nameof(InvoicingBase.ExpectedInvoiceTaxTotal),
			nameof(InvoicingBase.ExpectedInvoiceExclTaxTotal),
			nameof(InvoicingBase.AH_OriginalTransactionNum),
			nameof(InvoicingBase.AH_OriginalInvoiceDate),
			nameof(InvoicingBase.AH_OH),
		};

		string[] OriginalTransactionProperties { get; } = new[] {
			nameof(InvoicingBase.OriginalTransactionReference),
		};
	}
}
