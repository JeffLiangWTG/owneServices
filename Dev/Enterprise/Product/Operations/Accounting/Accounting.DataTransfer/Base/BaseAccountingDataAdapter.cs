using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer
{
	public abstract class BaseAccountingDataAdapter<TBusinessObject, TValueObject> : ValueObjectDataAdapter<TBusinessObject, TValueObject>
		where TBusinessObject : BusinessObject
		where TValueObject : IValueObject
	{
		protected bool IsShipmentJob(JobHeader job)
		{
			return (job != null && job.JH_ParentTableCode == JobShipmentSchema.Constants.Prefix);
		}

		protected bool IsDeclarationJob(JobHeader job)
		{
			return (job != null && job.JH_ParentTableCode == JobDeclarationSchema.Constants.Prefix);
		}

		protected ZQuery CurrentCompanyFilter
		{
			get
			{
				return new ZQuery(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			}
		}

		public override bool OnlySaveDataWhenNoRecordsHaveErrorsCheckBoxChecked
		{
			get { return AccountingConfigurationRegistry.Instance.DataImportShouldSaveOnlyWhenThereAreNoErrors.Value; }
		}

		public override bool OnlySaveDataWhenNoRecordsHaveErrorsCheckBoxVisible
		{
			get { return true; }
		}

		protected void InvertSigns(Xsd.TxnHeader txnHeader)
		{
			txnHeader.OsInvoiceAmtExclTax.Value = -txnHeader.OsInvoiceAmtExclTax.Value;
			txnHeader.OsInvoiceAmtInclTax.Value = -txnHeader.OsInvoiceAmtInclTax.Value;
			txnHeader.OsTaxAmount.Value = -txnHeader.OsTaxAmount.Value;
			txnHeader.OsWHTAmount.Value = -txnHeader.OsWHTAmount.Value;

			txnHeader.LocalInvoiceAmtExclTax.Value = -txnHeader.LocalInvoiceAmtExclTax.Value;
			txnHeader.LocalInvoiceAmtInclTax.Value = -txnHeader.LocalInvoiceAmtInclTax.Value;
			txnHeader.LocalTaxAmount.Value = -txnHeader.LocalTaxAmount.Value;
			txnHeader.LocalWHTAmount.Value = -txnHeader.LocalWHTAmount.Value;

			foreach (Xsd.TxnLine txnLine in txnHeader.TxnLines)
			{
				txnLine.OsInvoiceAmtExclTax.Value = -txnLine.OsInvoiceAmtExclTax.Value;
				txnLine.OsInvoiceAmtInclTax.Value = -txnLine.OsInvoiceAmtInclTax.Value;
				txnLine.OsTaxAmount.Value = -txnLine.OsTaxAmount.Value;
				txnLine.OsWHTAmount.Value = -txnLine.OsWHTAmount.Value;

				txnLine.LocalInvoiceAmtExclTax.Value = -txnLine.LocalInvoiceAmtExclTax.Value;
				txnLine.LocalInvoiceAmtInclTax.Value = -txnLine.LocalInvoiceAmtInclTax.Value;
				txnLine.LocalTaxAmount.Value = -txnLine.LocalTaxAmount.Value;
				txnLine.LocalWHTAmount.Value = -txnLine.LocalWHTAmount.Value;
			}
		}

		#region OrganisationAdapter

		protected OrganisationValueObjectDataAdapter OrganisationAdapter
		{
			get { return fOrganisationAdapter ?? (fOrganisationAdapter = new OrganisationValueObjectDataAdapter()); }
		}

		OrganisationValueObjectDataAdapter fOrganisationAdapter;

		#endregion
	}
}
