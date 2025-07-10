using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.JP.Common;

public sealed class ImportClearancePermitItemDocumentWrapper(ImportClearanceItemProvider item) : DocumentEngineCore.DocWrappers.DocumentWrapper
{
	readonly ImportClearanceItemProvider item = Argument.NotNull(item, nameof(item));

	#region Item Fields
	public ZInt I_170 => item.ColumnNumber;

	public ZInt I_171 => item.OriginalColumnNumber;

	public ZString I_172 => item.TariffCode;

	public ZString I_173 => item.NACCSCode;

	public ZString I_174 => item.PriceReconfirmationType;

	public ZString I_175 => item.GoodsDescription;

	public ZString I_176 => item.Quantity1?.Quantity.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_177 => item.Quantity1?.Unit ?? ZString.Empty;

	public ZString I_178 => item.OriginalTariffCode;

	public ZString I_179 => item.Quantity2?.Quantity.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_180 => item.Quantity2?.Unit ?? ZString.Empty;

	public ZString I_181 => item.DutiableValue.FormatNumberInDocument();

	public ZString I_182 => item.DutyQuantity?.Quantity.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_183 => item.DutyQuantity?.Unit ?? ZString.Empty;

	public ZString I_184 => item.DutyAmountSummary.FormatNumberInDocument();

	public ZString I_185 => item.DutyQuantitySummary?.Quantity.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_186 => item.DutyQuantitySummary?.Unit ?? ZString.Empty;

	public ZString I_187 => item.DutyRateCode;

	public ZString I_188 => item.DutyRate;

	public ZString I_189 => item.SpecialUrgentDutyType;

	public ZString I_190 => item.ImportTradeControlOrdinanceAppendixCode;

	public ZString I_191 => item.PreferentialRateApplicabilityType;

	public ZString I_192 => item.DutyAmount.FormatNumberInDocument();

	public ZString I_193 => item.CustomsValueApportionmentCoefficient.FormatNumberInDocument();

	public ZString I_194 => item.DutyExemptionReductionAmount.FormatNumberInDocument();

	public ZString I_195 => item.FOBCurrency;

	public ZString I_196 => item.CustomsValue.FormatNumberInDocument();

	public ZString I_197 => item.DutyReductionAmountSummary.FormatNumberInDocument();

	public ZString I_198 => item.StorageType;

	public ZString I_199 => item.FreightApportionmentType;

	public ZString I_200 => item.GoodsOriginCode;

	public ZString I_201 => item.GoodsOriginName;

	public ZString I_202 => item.CertificateOfOriginType;

	public ZString I_203 => item.DutyReductionExemptionCode;

	public ZString I_204 => item.DutyReductionExemptionClauseLaw;

	public ZString I_205 => item.DutyReductionExemptionClauseLawArticleNumber;

	public ZString I_206 => item.DeductionQuantity?.Quantity.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_207 => item.DeductionQuantity?.Unit ?? ZString.Empty;

	public ZString I_208 => item.DutyReductionExemptionClauseOrderArticleNumber;

	public ZString I_209 => item.DeductionQuantitySummary?.Quantity.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_210 => item.DeductionQuantitySummary?.Unit ?? ZString.Empty;

	public ZString I_211 => item.DutyReductionExemptionClausesAppendixNumber;

	public ZString I_212 => item.AdvancedRulingOnClassification;

	public ZString I_213 => item.AdvancedRulingOnOrigin;

	public ZString I_214_1 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(0)?.Name ?? ZString.Empty;

	public ZString I_215_1 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(0)?.TypeCode ?? ZString.Empty;

	public ZString I_216_1 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(0)?.DutiableValue.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_217_1 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(0)?.Quantity?.Quantity.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_218_1 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(0)?.Quantity?.Unit ?? ZString.Empty;

	public ZString I_219_1 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(0)?.AmountSummary.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_220_1 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(0)?.QuantitySummary?.Quantity.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_221_1 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(0)?.QuantitySummary?.Unit ?? ZString.Empty;

	public ZString I_222_1 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(0)?.Rate ?? ZString.Empty;

	public ZString I_223_1 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(0)?.Amount.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_224_1 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(0)?.ReductionCode ?? ZString.Empty;

	public ZString I_225_1 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(0)?.ReductionAmount.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_226_1 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(0)?.ReductionClause ?? ZString.Empty;

	public ZString I_227_1 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(0)?.ReductionAmountSummary.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_228_1 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(0)?.ReductionApplicableTerms ?? ZString.Empty;

	public ZString I_214_2 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(1)?.Name ?? ZString.Empty;

	public ZString I_215_2 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(1)?.TypeCode ?? ZString.Empty;

	public ZString I_216_2 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(1)?.DutiableValue.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_217_2 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(1)?.Quantity?.Quantity.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_218_2 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(1)?.Quantity?.Unit ?? ZString.Empty;

	public ZString I_219_2 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(1)?.AmountSummary.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_220_2 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(1)?.QuantitySummary?.Quantity.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_221_2 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(1)?.QuantitySummary?.Unit ?? ZString.Empty;

	public ZString I_222_2 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(1)?.Rate ?? ZString.Empty;

	public ZString I_223_2 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(1)?.Amount.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_224_2 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(1)?.ReductionCode ?? ZString.Empty;

	public ZString I_225_2 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(1)?.ReductionAmount.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_226_2 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(1)?.ReductionClause ?? ZString.Empty;

	public ZString I_227_2 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(1)?.ReductionAmountSummary.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_228_2 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(1)?.ReductionApplicableTerms ?? ZString.Empty;

	public ZString I_214_3 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(2)?.Name ?? ZString.Empty;

	public ZString I_215_3 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(2)?.TypeCode ?? ZString.Empty;

	public ZString I_216_3 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(2)?.DutiableValue.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_217_3 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(2)?.Quantity?.Quantity.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_218_3 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(2)?.Quantity?.Unit ?? ZString.Empty;

	public ZString I_219_3 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(2)?.AmountSummary.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_220_3 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(2)?.QuantitySummary?.Quantity.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_221_3 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(2)?.QuantitySummary?.Unit ?? ZString.Empty;

	public ZString I_222_3 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(2)?.Rate ?? ZString.Empty;

	public ZString I_223_3 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(2)?.Amount.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_224_3 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(2)?.ReductionCode ?? ZString.Empty;

	public ZString I_225_3 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(2)?.ReductionAmount.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_226_3 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(2)?.ReductionClause ?? ZString.Empty;

	public ZString I_227_3 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(2)?.ReductionAmountSummary.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_228_3 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(2)?.ReductionApplicableTerms ?? ZString.Empty;

	public ZString I_214_4 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(3)?.Name ?? ZString.Empty;

	public ZString I_215_4 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(3)?.TypeCode ?? ZString.Empty;

	public ZString I_216_4 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(3)?.DutiableValue.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_217_4 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(3)?.Quantity?.Quantity.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_218_4 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(3)?.Quantity?.Unit ?? ZString.Empty;

	public ZString I_219_4 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(3)?.AmountSummary.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_220_4 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(3)?.QuantitySummary?.Quantity.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_221_4 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(3)?.QuantitySummary?.Unit ?? ZString.Empty;

	public ZString I_222_4 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(3)?.Rate ?? ZString.Empty;

	public ZString I_223_4 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(3)?.Amount.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_224_4 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(3)?.ReductionCode ?? ZString.Empty;

	public ZString I_225_4 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(3)?.ReductionAmount.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_226_4 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(3)?.ReductionClause ?? ZString.Empty;

	public ZString I_227_4 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(3)?.ReductionAmountSummary.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_228_4 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(3)?.ReductionApplicableTerms ?? ZString.Empty;

	public ZString I_214_5 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(4)?.Name ?? ZString.Empty;

	public ZString I_215_5 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(4)?.TypeCode ?? ZString.Empty;

	public ZString I_216_5 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(4)?.DutiableValue.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_217_5 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(4)?.Quantity?.Quantity.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_218_5 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(4)?.Quantity?.Unit ?? ZString.Empty;

	public ZString I_219_5 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(4)?.AmountSummary.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_220_5 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(4)?.QuantitySummary?.Quantity.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_221_5 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(4)?.QuantitySummary?.Unit ?? ZString.Empty;

	public ZString I_222_5 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(4)?.Rate ?? ZString.Empty;

	public ZString I_223_5 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(4)?.Amount.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_224_5 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(4)?.ReductionCode ?? ZString.Empty;

	public ZString I_225_5 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(4)?.ReductionAmount.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_226_5 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(4)?.ReductionClause ?? ZString.Empty;

	public ZString I_227_5 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(4)?.ReductionAmountSummary.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_228_5 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(4)?.ReductionApplicableTerms ?? ZString.Empty;

	public ZString I_214_6 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(5)?.Name ?? ZString.Empty;

	public ZString I_215_6 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(5)?.TypeCode ?? ZString.Empty;

	public ZString I_216_6 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(5)?.DutiableValue.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_217_6 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(5)?.Quantity?.Quantity.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_218_6 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(5)?.Quantity?.Unit ?? ZString.Empty;

	public ZString I_219_6 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(5)?.AmountSummary.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_220_6 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(5)?.QuantitySummary?.Quantity.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_221_6 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(5)?.QuantitySummary?.Unit ?? ZString.Empty;

	public ZString I_222_6 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(5)?.Rate ?? ZString.Empty;

	public ZString I_223_6 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(5)?.Amount.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_224_6 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(5)?.ReductionCode ?? ZString.Empty;

	public ZString I_225_6 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(5)?.ReductionAmount.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_226_6 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(5)?.ReductionClause ?? ZString.Empty;

	public ZString I_227_6 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(5)?.ReductionAmountSummary.FormatNumberInDocument() ?? ZString.Empty;

	public ZString I_228_6 => item.DomesticConsumptionTaxes?.ElementAtOrDefault(5)?.ReductionApplicableTerms ?? ZString.Empty;

	#endregion
}
