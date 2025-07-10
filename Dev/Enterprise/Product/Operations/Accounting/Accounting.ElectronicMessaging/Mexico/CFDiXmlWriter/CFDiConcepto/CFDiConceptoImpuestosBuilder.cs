using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico
{
	public interface ICFDiConceptoImpuestosBuilder
	{
		ComprobanteConceptoImpuestos BuildComprobanteConceptoImpuestosInfo(PostingJournal line, TransactionType transactionType, int decimalsOfInvoiceCurrency);
	}

	class CFDiConceptoImpuestosBuilder : ICFDiConceptoImpuestosBuilder
	{
		ComprobanteConceptoImpuestos ICFDiConceptoImpuestosBuilder.BuildComprobanteConceptoImpuestosInfo(PostingJournal line, TransactionType transactionType, int decimalsOfInvoiceCurrency)
		{
			ComprobanteConceptoImpuestos comprobanteConceptoImpuestos = null;

			var itemsTraslados = Traslados(line, transactionType, decimalsOfInvoiceCurrency).ToArray();
			var itemsRetenciones = Retenciones(line, transactionType, decimalsOfInvoiceCurrency).ToArray();

			if (itemsTraslados.Any() || itemsRetenciones.Any())
			{
				comprobanteConceptoImpuestos = new ComprobanteConceptoImpuestos()
				{
					Traslados = itemsTraslados.Any() ? itemsTraslados : null,
					Retenciones = itemsRetenciones.Any() ? itemsRetenciones : null,
				};
			}

			return comprobanteConceptoImpuestos;
		}

		IEnumerable<ComprobanteConceptoImpuestosTraslado> Traslados(PostingJournal line, TransactionType transactionType, int decimalsOfInvoiceCurrency)
		{
			var returnValue = new List<ComprobanteConceptoImpuestosTraslado>();
			var taxType = line.VATTaxID?.TaxType?.Code.ToString() ?? "";
			var totalTaxes = 0.000m;
			var oSAmount = GetValueToReport(line.OSAmount ?? 0, transactionType == TransactionType.CRD);

			if (transactionType == TransactionType.INV || transactionType == TransactionType.CRD)
			{
				var taxRate = line.VATTaxID?.TaxRate;
				var extraTaxType = line.VATTaxID?.ExtraTaxType?.Code.ToString() ?? "";
				var oSGSTVATAmount = GetValueToReport(line.OSGSTVATAmount ?? 0, transactionType == TransactionType.CRD);
				var defTasaOCuota = taxRate.HasValue ? taxRate.Value / 100 : 0;
				var itemImpuestosTraslado = new ComprobanteConceptoImpuestosTraslado()
				{
					ImporteSpecified = true,
					Base = oSAmount,
					Importe = oSGSTVATAmount,
					TipoFactor = c_TipoFactor.Tasa,
					Impuesto = c_Impuesto.Item002,
					TasaOCuotaSpecified = true,
					TasaOCuota = defTasaOCuota
				};

				if ((taxType == "CAP" || taxType == "RAT") && string.IsNullOrEmpty(extraTaxType))
				{
					if (line.VATTaxID.TaxCode.Value == MexicoConstants.TaxCodes.IVA4)
					{
						AddComprobanteConceptoImpuestosTraslado();
					}
				}
				else if (taxType == "EXT" && string.IsNullOrEmpty(extraTaxType))
				{
					itemImpuestosTraslado.TipoFactor = c_TipoFactor.Exento;
					itemImpuestosTraslado.ImporteSpecified = false;
					itemImpuestosTraslado.TasaOCuotaSpecified = false;
					itemImpuestosTraslado.Importe = 0;
					itemImpuestosTraslado.TasaOCuota = 0;
				}
				else if (taxType == "RAT" && (extraTaxType == "RET" || extraTaxType == "REF"))
				{
					if (line.VATTaxID.TaxCode.Value == MexicoConstants.TaxCodes.IVAREC)
					{
						AddComprobanteConceptoImpuestosTraslado();
					}

					itemImpuestosTraslado.Importe = oSGSTVATAmount + GetValueToReport(line.OSExtraVATAmount ?? 0, transactionType == TransactionType.INV);
				}
				else
				{
					return Enumerable.Empty<ComprobanteConceptoImpuestosTraslado>();
				}

				returnValue.Add(itemImpuestosTraslado);

				void AddComprobanteConceptoImpuestosTraslado()
				{
					itemImpuestosTraslado.Base = FixDecimalPlaces((oSAmount * MexicoConstants.PercentageTax.Perc025), decimalsOfInvoiceCurrency);
					totalTaxes += itemImpuestosTraslado.Base;
					itemImpuestosTraslado.TasaOCuota = MexicoConstants.PercentageTax.Perc016;
					returnValue.Add(new ComprobanteConceptoImpuestosTraslado()
					{
						ImporteSpecified = true,
						TasaOCuotaSpecified = true,
						Impuesto = c_Impuesto.Item002,
						Base = oSAmount - itemImpuestosTraslado.Base,
						TipoFactor = c_TipoFactor.Tasa
					});
				}
			}

			if (totalTaxes > 0)
			{
				foreach (var x in returnValue)
				{
					if (x.TipoFactor == c_TipoFactor.Exento)
					{
						x.Base = oSAmount - totalTaxes;
					}
				}
			}

			return returnValue;
		}

		IEnumerable<ComprobanteConceptoImpuestosRetencion> Retenciones(PostingJournal line, TransactionType transactionType, int decimalsOfInvoiceCurrency)
		{
			var returnValue = new List<ComprobanteConceptoImpuestosRetencion>();
			var taxType = line.VATTaxID?.TaxType?.Code.ToString() ?? "";
			var extraTaxType = line.VATTaxID?.ExtraTaxType?.Code.ToString() ?? "";
			var taxCode = line.VATTaxID?.TaxCode ?? ZString.Empty;
			var tasaOCuota = (line.VATTaxID?.ExtraTaxRate ?? 0) / 100;
			var percentageBase = 1m;

			if (taxCode == MexicoConstants.TaxCodes.IVAREC)
			{
				tasaOCuota = MexicoConstants.PercentageTax.Perc0060;
				percentageBase = MexicoConstants.PercentageTax.Perc025;
			}

			if (taxType == "RAT" && (extraTaxType == "REF" || extraTaxType == "RET"))
			{
					returnValue.Add(new ComprobanteConceptoImpuestosRetencion()
					{
						Base = FixDecimalPlaces((GetValueToReport(line.OSAmount ?? 0, transactionType == TransactionType.CRD) * percentageBase), decimalsOfInvoiceCurrency),
						Importe = GetValueToReport(line.OSExtraVATAmount ?? 0, transactionType == TransactionType.INV),
						Impuesto = c_Impuesto.Item002,
						TasaOCuota = tasaOCuota,
						TipoFactor = c_TipoFactor.Tasa,
					});
			}

			return returnValue;
		}

		decimal GetValueToReport(decimal value, bool changeSign)
		{
			return (changeSign) ? value * -1 : value;
		}

		decimal FixDecimalPlaces(decimal value, int decimalsOfInvoiceCurrency)
		{
			return Utilities.Round(value, decimalsOfInvoiceCurrency);
		}
	}
}
