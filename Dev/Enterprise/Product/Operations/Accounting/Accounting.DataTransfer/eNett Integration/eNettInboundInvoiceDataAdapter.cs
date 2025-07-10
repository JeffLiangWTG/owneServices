using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.eNett_Integration
{
	public class eNettInboundInvoiceDataAdapter : FinancialInvoiceDataAdapter, Accounting.Integration.IeNettInboundInvoiceDataAdapter
	{
		public eNettInboundInvoiceDataAdapter()
			: base(false)
		{ }

		protected override void ExportToValueObjectCore(InvoicingBase bizObj, TxnHeader constructedValueObject, IValueObjectExportContext context)
		{
			throw new NotSupportedException();
		}

		protected override bool ShouldImportFromARtoAP(TxnHeader xmlInvoiceHeader, IValueObjectImportContext context)
		{
			return true;
		}

		protected override Organisation GetCreditorForCrossLedgerImport(InvoicingBase invoiceHeader, IValueObjectImportContext context)
		{
			Organisation result = null;
			Organisation ediOrganisation = ((XmlInterchange)context.Interchange).InterchangeInfo.EDIOrganisation;
			Enterprise.DataTransfer.Xml.XsdVersion1.RegistrationNumber registrationNumber = ediOrganisation.OrganisationDetails.RegistrationNumbers.FindRegistrationNumber(RegistrationNumberTypes.ENE, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			if (registrationNumber != null)
			{
				string eNettRegistrationNumber = registrationNumber.Number;
				ZQuery query = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.eNettRegistrationNumber);
				query.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, registrationNumber.Number);
				OrgCusCode code = invoiceHeader.Factory.LoadTop1<OrgCusCode>(query);
				if (code != null)
				{
					OrgHeader orgBizO = invoiceHeader.Factory.Load<OrgHeader>(code.OK_OH);
					if (orgBizO != null)
					{
						result = new OrganisationValueObjectDataAdapter().ExportToValueObject(orgBizO, new ValueObjectExportContext(context));
					}
				}
			}
			if (result == null)
			{
				context.Add(ErrorType.Error,
					Res.GetString("a37d0c74-54c8-409e-b276-9e8436bff372", "Error: Attempt to process ComPay inbound transaction failed because not organization match could be found for ComPay Debtor {0} '{1}', Code '{2}'.",
					ediOrganisation.EDICode.IsEmpty ? Res.GetString("4d3f7fcb-6f9a-4cbf-a6ef-0747a367d353", "Name") : Res.GetString("86f40356-fcf4-42df-81a6-291744176f85", "ID"),
					ediOrganisation.EDICode.IsEmpty ? ediOrganisation.OrganisationDetails.Name : ediOrganisation.EDICode,
					registrationNumber != null ? registrationNumber.Number.ToString() : Res.GetString("06746cd6-74d3-4967-942f-695c5b6400fc", "Not specified")));
			}
			return result;
		}

		protected override TransactionHeaderBuilder GetTransactionHeaderBuilder(bool isCrossLedgerImport)
		{
			return new eNettTransactionHeaderBuilder(Notifier, GetTransactionBuilderConfig(isCrossLedgerImport));
		}

		protected override TransactionBuilderConfig GetTransactionBuilderConfig(bool isCrossLedgerImport)
		{
			TransactionBuilderConfig result = base.GetTransactionBuilderConfig(isCrossLedgerImport);
			result.SetBranch = false;
			result.SetDepartment = false;
			result.AllowResetChargeCodeAndJobOnError = false;
			result.UseConsolOrJobNumberToMatchJob = false;
			result.UseForeignChargeAmountWhenPostingLocalCurrencyInvoices = true;
			result.PreserveExchangeRateFromSourceTransaction = true;
			return result;
		}
	}
}
