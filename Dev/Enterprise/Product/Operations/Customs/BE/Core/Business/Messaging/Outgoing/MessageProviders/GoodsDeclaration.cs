using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public class GoodsDeclaration : ITGoodsDeclarationImport
{
	public GoodsDeclaration(CusEntryHeader entry)
	{
		this.entry = entry;
		this.declaration = entry.Declaration;
		Totals = new Totals(entry);
		Declarant = new Declarant(entry.Declaration.Declarant) { DeclarantType = declaration.JE_DeclarantType };
		Consignee = new Operator(entry.Importer);
		Consignor = new Operator(entry.Supplier);
		PaymentTaxes = new PaymentTaxes(entry);
		PaymentVat = new PaymentVAT(entry);
		Customs = new CustomsImport(declaration);
		TransportMeans = new TransportMeansImport(entry.InvoiceHeaders.First() as JobComInvoiceHeader);
	}

	readonly CusEntryHeader entry;
	readonly JobDeclaration declaration;

	public ITOperator Carrier { get; }
	public ITChargesImport ChargesImport { get; }
	public ITOperator Consignee { get; set; }
	public ITOperator Consignor { get; set; }
	public ITCustomsImport Customs { get; set; }
	public ITDV1DeclarationHeader DV1DeclarationHeader { get; }
	public ZString InternalCurrencyUnit { get => ZString.Empty; }
	public ITOperator Intracom { get; }
	public ITPaymentTaxes PaymentTaxes { get; set; }
	public ITPaymentVat PaymentVat { get; set; }
	public ITOperator Representative { get; }
	public ITTransportMeansImport TransportMeans { get; set; }
	public ZString TypePartOne { get => declaration.JE_EntryStyle; }
	public ZString TypePartTwo { get => entry.EntryInstruction.CEI_SubStyle; }
	public ZString ValueDetails { get => ZString.Empty; }
	public DateTime AcceptanceDate { get => ZDateTime.Today.ToDateTime(); }
	public ZString CommercialReference { get => declaration.JE_DeclarationReference; }
	public ITDeclarant Declarant { get; set; }
	public ZString IssuePlace { get => ZString.Join(" ", new[] { Declarant.OperatorAddress.PostalCode, Declarant.OperatorAddress.City }); }
	// TODO confirm whether to implement and where to retrieve
	public ZDecimal LoadingList { get => ZDecimal.Zero; }
	public ZBool LoadingListSpecified { get => ZBool.False; }
	public ZString LocalReferenceNumber { get => entry.CH_BGMReference; }
	// TODO pending Johannes's rule
	public ZString RegistrationNumber { get => ZString.Empty; }
	public ZString Signature { get => ZString.Empty; }
	public ITtotals Totals { get; set; }
	public ITTransactionNature TransactionNature { get => new TransactionNature(entry.InvoiceHeaders.First() as JobComInvoiceHeader); }
}
