using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	public class InvoiceTermTypeConverter : EnumConverter<InvoiceTermType>
	{
		protected override ZString[] GetCodes()
		{
			return new ZString[]
			{
				"COD", "INV", "MTH", "PER", "SHP", "MIC", "PIA", "DPC", "CUS", "EWK", "MLI", "LSI", "DLP"
			};
		}

		protected override InvoiceTermType[] GetEnumValues()
		{
			return new InvoiceTermType[]
			{
				InvoiceTermType.COD,
				InvoiceTermType.INV,
				InvoiceTermType.MTH,
				InvoiceTermType.PER,
				InvoiceTermType.SHP,
				InvoiceTermType.MIC,
				InvoiceTermType.PIA,
				InvoiceTermType.DPC,
				InvoiceTermType.CUS,
				InvoiceTermType.EWK,
				InvoiceTermType.MLI,
				InvoiceTermType.LSI,
				InvoiceTermType.DLP
			};
		}
	}
}
