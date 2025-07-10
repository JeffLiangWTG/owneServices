using System;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Mexico
{
	public class MexicoInvoicePaymentMethod : IInvoicePaymentMethod
	{
		CodeDescriptionPair IInvoicePaymentMethod.GetInvoicePaymentMethod(CargoWise.Types.ZString invoiceTermType, CargoWise.Types.ZDateTime? invoiceDate, CargoWise.Types.ZDateTime? dueDate)
		{
			if (!invoiceTermType.IsEmpty)
			{
				var value = (InvoiceTermType)Enum.Parse(typeof(InvoiceTermType), invoiceTermType);

				switch (value)
				{
					case InvoiceTermType.COD:
					case InvoiceTermType.PIA:
						return DocWrapperMappingMetodoPago.PUE;
					default:
						return HaveTheSameMonthYear() ? DocWrapperMappingMetodoPago.PUE : DocWrapperMappingMetodoPago.PPD;
				}
			}

			bool HaveTheSameMonthYear()
			{
				if (invoiceDate.HasValue && dueDate.HasValue)
				{
					return invoiceDate.Value.Month == dueDate.Value.Month && invoiceDate.Value.Year == dueDate.Value.Year;
				}
				return false;
			}

			return new CodeDescriptionPair("", "");
		}
	}

	static class DocWrapperMappingMetodoPago
	{
		public static CodeDescriptionPair PUE => new CodeDescriptionPair("PUE", (NoResString)"Pago en una sola exhibición"); // Mexico's Payment Method
		public static CodeDescriptionPair PPD => new CodeDescriptionPair("PPD", (NoResString)"Pago en Parcialidades o Diferido"); // Mexico's Payment Method
	}
}
