using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Import934Header : Import934_5SMHeader, IImport934Header
	{
		public string ImportDeclarationNumber { get; set; }
		public Import934FormA FormAData { get; set; }
		public Import934FormB FormBData { get; set; }
		public string InvoiceNo { get; set; }
		public DateTime InvoiceDate { get; set; }
		public string ContractNo { get; set; }
		public DateTime ContractDate { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.CustomsValue)]
		public decimal TotalCustomsValueKRW { get; set; }
		public bool ProvisionalPricingYN { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.TaxRate)]
		public decimal ProvisionalAdditionRate { get; set; }
		public DateTime EstimatedDateOfFinalPrice { get; set; }
		public DateTime ContractExpirationDate { get; set; }
		public ValueDeclarationCode[] ProvisionalPricingReasons { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.CustomsValue)]
		public decimal ProvisionalAdditionalAmount { get; set; }

		ZString IImport934Header.ImportDeclarationNumber => ImportDeclarationNumber;
		IImport934FormA IImport934Header.FormAData => FormAData;
		IImport934FormB IImport934Header.FormBData => FormBData;
		ZString IImport934Header.InvoiceNo => InvoiceNo;
		ZDate IImport934Header.InvoiceDate => (ZDate)InvoiceDate;
		ZString IImport934Header.ContractNo => ContractNo;
		ZDate IImport934Header.ContractDate => (ZDate)ContractDate;
		ZDecimal IImport934Header.TotalCustomsValueKRW => TotalCustomsValueKRW;
		ZBool IImport934Header.ProvisionalPricingYN => ProvisionalPricingYN;
		ZDecimal IImport934Header.ProvisionalAdditionRate => ProvisionalAdditionRate;
		ZDate IImport934Header.EstimatedDateOfFinalPrice => (ZDate)EstimatedDateOfFinalPrice;
		ZDate IImport934Header.ContractExpirationDate => (ZDate)ContractExpirationDate;
		IEnumerable<IValueDeclarationCode> IImport934Header.ProvisionalPricingReasons => ProvisionalPricingReasons;
		ZDecimal IImport934Header.ProvisionalAdditionalAmount => ProvisionalAdditionalAmount;
	}
}
