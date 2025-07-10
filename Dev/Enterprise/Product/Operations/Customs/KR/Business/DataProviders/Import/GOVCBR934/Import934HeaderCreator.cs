using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Import934HeaderCreator : CommonValuationHeaderCreator<Import934Header>
	{		
		public Import934HeaderCreator() : base()
		{
		}

		protected override void PopulateAdditionalFields(Import934Header entryHeaderData, CusEntryHeader entry)
		{
			entryHeaderData.ImportDeclarationNumber = entry.CusEntryNumber?.CE_EntryNum ?? ZString.Empty;
			var randomHeader = entry.RandomHeader;
			entryHeaderData.InvoiceNo = randomHeader.JZ_InvoiceNumber;
			if (randomHeader.JZ_InvoiceDate.IsValid)
			{
				entryHeaderData.InvoiceDate = randomHeader.JZ_InvoiceDate.ToDateTime();
			}

			entryHeaderData.ContractNo = randomHeader.ContractNumber;
			if (randomHeader.ContractDate.IsValid)
			{
				entryHeaderData.ContractDate = randomHeader.ContractDate.ToDateTime();
			}

			entryHeaderData.TotalCustomsValueKRW += entry.CustomsValue;
			entryHeaderData.ProvisionalPricingYN = randomHeader.IsProvPricing;
			entryHeaderData.ProvisionalAdditionRate = randomHeader.JZ_ProvAdditionalRate;

			if (randomHeader.JZ_EstimatedDateOfFinalPrice != ZDate.Empty)
			{
				entryHeaderData.EstimatedDateOfFinalPrice = randomHeader.JZ_EstimatedDateOfFinalPrice.ToDateTime();
			}
			if (randomHeader.JZ_ImpContractExpiryDate.IsValid)
			{
				entryHeaderData.ContractExpirationDate = randomHeader.JZ_ImpContractExpiryDate.ToDateTime();
			}
			entryHeaderData.ProvisionalAdditionalAmount = randomHeader.JZ_ProvAdditionalAmount;

			var useCodes = new List<ValueDeclarationCode>();
			var goodsPricingBasises = new List<ValueDeclarationCode>();
			var provisionalPricingReasons = new List<ValueDeclarationCode>();
			foreach (var valuationDecCode in randomHeader.ValuationDeclarationCodes)
			{
				switch (valuationDecCode.CY_Code.Left(1))
				{
					case ProvisionalPricingReasonsType:
						provisionalPricingReasons.Add(new ValueDeclarationCode { Code = valuationDecCode.CY_Code, CodeOtherDescription = valuationDecCode.CY_Data });
						break;
					case UseCodesType:
						useCodes.Add(new ValueDeclarationCode { Code = valuationDecCode.CY_Code, CodeOtherDescription = valuationDecCode.CY_Data });
						break;
					case GoodsPricingBasisType:
						goodsPricingBasises.Add(new ValueDeclarationCode { Code = valuationDecCode.CY_Code, CodeOtherDescription = valuationDecCode.CY_Data });
						break;
				}
			}
			entryHeaderData.ProvisionalPricingReasons = provisionalPricingReasons.ToArray();

			if (randomHeader.JZ_ValuationCode == ValuationCodeList.Codes.MethodOne)
			{
				var formAData = new Import934FormA();
				formAData.Questions = new ValuationQuestionsProvider().PopulateQuestionAndAnswer(randomHeader, PriceQuestionCodeList.Codes._7EA, PriceQuestionCodeList.Codes._7EB, PriceQuestionCodeList.Codes._7E);
				formAData.Method1ValuationData = new Valuation1_MethodDataCreator().Create(randomHeader, entry.CurrencyConverter);
				entryHeaderData.FormAData = formAData;
			}
			else
			{
				var formBData = new Import934FormB();
				formBData.ExpectedCustomsValue = entry.CustomsValue;
				formBData.ValuationMethod = randomHeader.JZ_ValuationCode;
				formBData.ValuationSupportingDocument1 = randomHeader.ValuationSupportingDocument1;
				formBData.ValuationSupportingDocument2 = randomHeader.ValuationSupportingDocument2;
				formBData.UseCodes = useCodes.ToArray();
				formBData.GoodsPricingBasis = goodsPricingBasises.ToArray();
				switch (randomHeader.JZ_ValuationCode)
				{
					case ValuationCodeList.Codes.MethodTwo:
					case ValuationCodeList.Codes.MethodThree:
						formBData.Method2_3ValuationData = new ValuationMethodDataCreator().Create(randomHeader, entry.CurrencyConverter);
						break;
					case ValuationCodeList.Codes.MethodFourA:
					case ValuationCodeList.Codes.MethodFourB:
						formBData.Method4ValuationData = new Valuation4_MethodDataCreator().Create(randomHeader, entry.CurrencyConverter);
						break;
					case ValuationCodeList.Codes.MethodFive:
					case ValuationCodeList.Codes.MethodSix:
						formBData.Method5_6ValuationData = new ValuationMethodDataCreator().Create(randomHeader, entry.CurrencyConverter);
						break;
				}
				entryHeaderData.FormBData = formBData;
			}
		}
		const string ProvisionalPricingReasonsType = "1";
		const string UseCodesType = "3";
		const string GoodsPricingBasisType = "4";
	}
}
