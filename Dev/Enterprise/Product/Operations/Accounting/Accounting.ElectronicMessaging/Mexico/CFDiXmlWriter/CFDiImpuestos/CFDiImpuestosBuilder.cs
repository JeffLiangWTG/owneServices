using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico
{
	public interface ICFDiImpuestosBuilder
	{
		ComprobanteImpuestos BuildComprobanteImpuestosInfo(ComprobanteConcepto[] comprobanteConcepto);
	}

	class CFDiImpuestosBuilder : ICFDiImpuestosBuilder
	{
		ComprobanteImpuestos ICFDiImpuestosBuilder.BuildComprobanteImpuestosInfo(ComprobanteConcepto[] comprobanteConcepto)
		{
			ComprobanteImpuestos comprobanteImpuestos = null;

			if (comprobanteConcepto != null)
			{
				var traslados = Traslados(comprobanteConcepto).ToArray();
				var retenciones = Retenciones(comprobanteConcepto).ToArray();

				if (traslados.Any() || retenciones.Any())
				{
					comprobanteImpuestos = new ComprobanteImpuestos()
					{
						Traslados = traslados.Any() ? traslados : null,
						Retenciones = retenciones.Any() ? retenciones : null,
						TotalImpuestosTrasladadosSpecified = traslados.Any(x => x.TipoFactor == c_TipoFactor.Tasa),
						TotalImpuestosTrasladados = traslados.Sum(x => x.Importe),
						TotalImpuestosRetenidosSpecified = retenciones.Any(),
						TotalImpuestosRetenidos = retenciones.Sum(x => x.Importe),
					};
				}
			}
			return comprobanteImpuestos;
		}

		IEnumerable<ComprobanteImpuestosTraslado> Traslados(ComprobanteConcepto[] comprobanteConcepto)
		{
			var comprobanteConceptoImpuestosTrasladoList = comprobanteConcepto.Where(x => x != null && x.Impuestos != null && x.Impuestos.Traslados != null).SelectMany(x => x.Impuestos.Traslados).ToList();

			return comprobanteConceptoImpuestosTrasladoList
				.Where(x => x != null)
				.GroupBy(x => new { x.TasaOCuota, x.TipoFactor })
				.Select(y => new ComprobanteImpuestosTraslado
				{
					Base = y.Sum(x => x.Base),
					Impuesto = c_Impuesto.Item002,
					TipoFactor = y.Key.TipoFactor,
					TasaOCuota = y.Key.TasaOCuota,
					Importe = y.Sum(x => x.Importe),
					ImporteSpecified = (y.Key.TipoFactor == c_TipoFactor.Tasa),
					TasaOCuotaSpecified = (y.Key.TipoFactor == c_TipoFactor.Tasa)
				});
		}

		IEnumerable<ComprobanteImpuestosRetencion> Retenciones(ComprobanteConcepto[] comprobanteConcepto)
		{
			var comprobanteConceptoImpuestosRetencionList = comprobanteConcepto.Where(x =>	x.Impuestos != null && x.Impuestos.Retenciones != null).SelectMany(x => x.Impuestos.Retenciones).ToList();

			if (comprobanteConceptoImpuestosRetencionList.Count == 0)
			{
				return Enumerable.Empty<ComprobanteImpuestosRetencion>();
			}

			return new List<ComprobanteImpuestosRetencion>() {
			new ComprobanteImpuestosRetencion
			{
				Impuesto = c_Impuesto.Item002,
				Importe = comprobanteConceptoImpuestosRetencionList.Sum(x => x.Importe)
			}
			};
		}
	}
}
