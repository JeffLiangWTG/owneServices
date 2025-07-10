using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ImportD87Header : IImportD87Header
	{
		public bool HouseBillSplitDeclarationIndicator { get; set; }
		public string CarnetCertificateNumber { get; set; }
		public string RepresentativeProductName { get; set; }
		public Organisation Supplier { get; set; }
		public string BondedAreaCode { get; set; }
		public Organisation Importer { get; set; }
		public string CarnetUseCode { get; set; }
		public string DeclarationCustomsOffice { get; set; }
		public string DeclarationCustomsDivision { get; set; }
		public DateTime EffectiveToDate { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.ImportTotalInvoiceAmount)]
		public decimal TotalInvoiceAmount { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.Qty)]
		public decimal TotalQty { get; set; }
		public string InvoiceCurrency { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.Weight)]
		public decimal TotalGrossWeight { get; set; }
		public string TotalGrossWeighUnit { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.Qty)]
		public decimal TotalPackQty { get; set; }
		public string PackType { get; set; }
		public string CargoManagementNo { get; set; }
		public string UnipassDeclarantID { get; set; }

		ZBool IImportD87Header.HouseBillSplitDeclarationIndicator => HouseBillSplitDeclarationIndicator;
		ZString IImportD87Header.CarnetCertificateNumber => CarnetCertificateNumber;
		ZString IImportD87Header.RepresentativeProductName => RepresentativeProductName;
		IOrganization IImportD87Header.Supplier => Supplier;
		ZString IImportD87Header.BondedAreaCode => BondedAreaCode;
		IOrganization IImportD87Header.Importer => Importer;
		ZString IImportD87Header.CarnetUseCode => CarnetUseCode;
		ZString IImportD87Header.DeclarationCustomsOffice => DeclarationCustomsOffice;
		ZString IImportD87Header.DeclarationCustomsDivision => DeclarationCustomsDivision;
		ZDate IImportD87Header.EffectiveToDate => new ZDate(EffectiveToDate);
		ZDecimal IImportD87Header.TotalInvoiceAmount => TotalInvoiceAmount;
		ZDecimal IImportD87Header.TotalQty => TotalQty;
		ZString IImportD87Header.InvoiceCurrency => InvoiceCurrency;
		ZDecimal IImportD87Header.TotalGrossWeight => TotalGrossWeight;
		ZString IImportD87Header.TotalGrossWeighUnit => TotalGrossWeighUnit;
		ZDecimal IImportD87Header.TotalPackQty => TotalPackQty;
		ZString IImportD87Header.PackType => PackType;
		ZString IImportD87Header.CargoManagementNo => CargoManagementNo;
		ZString IImportD87Header.UnipassDeclarantID => UnipassDeclarantID;
	}
}
