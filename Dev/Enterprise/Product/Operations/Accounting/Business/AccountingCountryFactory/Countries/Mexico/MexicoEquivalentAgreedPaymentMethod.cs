using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Mexico
{
	public class MexicoEquivalentAgreedPaymentMethod : IEquivalentAgreedPaymentMethod
	{
		CodeDescriptionPair IEquivalentAgreedPaymentMethod.GetEquivalentAgreedPaymentMethod(ZString agreedPaymentMethod, ZString paymentMethod)
		{
			if (paymentMethod == DocWrapperMappingMetodoPago.PPD.Code || agreedPaymentMethod.IsEmpty)
			{
				return DocWrapperMappingFormaPago.PorDefinir;
			}

			switch (agreedPaymentMethod)
			{
				case OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck:
					return DocWrapperMappingFormaPago.Efectivo;

				case OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck:
					return DocWrapperMappingFormaPago.ChequeNominativo;

				case OrgConstants.CreditAgreedPaymentMethods.Code.CollectionRequest:
				case OrgConstants.CreditAgreedPaymentMethods.Code.BankTransfer:
					return DocWrapperMappingFormaPago.TransferenciaElectronica;

				case OrgConstants.CreditAgreedPaymentMethods.Code.CreditCard:
					return DocWrapperMappingFormaPago.TarjetaCredito;

				case OrgConstants.CreditAgreedPaymentMethods.Code.DebitCard:
					return DocWrapperMappingFormaPago.TarjetaDebito;

				default:
					return DocWrapperMappingFormaPago.GetCodeFromList(agreedPaymentMethod) ?? new CodeDescriptionPair("", "");
			}
		}
	}

	static class DocWrapperMappingFormaPago
	{
		public static CodeDescriptionPair GetCodeFromList(ZString agreedPaymentMethodCode) => (CodeDescriptionPair)FormaPagoList[agreedPaymentMethodCode];

		static CodeDescriptionPairList FormaPagoList
		{
			get
			{
				CodeDescriptionPairList formaPagoList = new CodeDescriptionPairList();
				formaPagoList.Add(Efectivo);
				formaPagoList.Add(ChequeNominativo);
				formaPagoList.Add(TransferenciaElectronica);
				formaPagoList.Add(TarjetaCredito);
				formaPagoList.Add(TarjetaDebito);
				formaPagoList.Add(MonederoElectronico);
				formaPagoList.Add(DineroElectronico);
				formaPagoList.Add(ValesDeDespensa);
				formaPagoList.Add(DacionEnPago);
				formaPagoList.Add(PagoPorSubrogacion);
				formaPagoList.Add(PagoPorConsignacion);
				formaPagoList.Add(Condonacion);
				formaPagoList.Add(Compensacion);
				formaPagoList.Add(Novacion);
				formaPagoList.Add(Confusion);
				formaPagoList.Add(RemisionDeDeuda);
				formaPagoList.Add(PrescipcionOCaducidad);
				formaPagoList.Add(ASatisfaccionDelAcreedor);
				formaPagoList.Add(TarjetaDeServicios);
				formaPagoList.Add(AmpliacionDeAnticipos);
				formaPagoList.Add(IntermediarioDePagos);
				formaPagoList.Add(PorDefinir);
				return formaPagoList;
			}
		}

		public static CodeDescriptionPair Efectivo => new CodeDescriptionPair("01", (NoResString)"Efectivo");
		public static CodeDescriptionPair ChequeNominativo => new CodeDescriptionPair("02", (NoResString)"Cheque nominativo");
		public static CodeDescriptionPair TransferenciaElectronica => new CodeDescriptionPair("03", (NoResString)"Transferencia electrónica de fondos");
		public static CodeDescriptionPair TarjetaCredito => new CodeDescriptionPair("04", (NoResString)"Tarjeta de crédito");
		public static CodeDescriptionPair TarjetaDebito => new CodeDescriptionPair("28", (NoResString)"Tarjeta de débito");
		public static CodeDescriptionPair MonederoElectronico => new CodeDescriptionPair("05", (NoResString)"Monedero electrónico");
		public static CodeDescriptionPair DineroElectronico => new CodeDescriptionPair("06", (NoResString)"Dinero electrónico");
		public static CodeDescriptionPair ValesDeDespensa => new CodeDescriptionPair("08", (NoResString)"Vales de despensa");
		public static CodeDescriptionPair DacionEnPago => new CodeDescriptionPair("12", (NoResString)"Dación en pago");
		public static CodeDescriptionPair PagoPorSubrogacion => new CodeDescriptionPair("13", (NoResString)"Pago por subrogación");
		public static CodeDescriptionPair PagoPorConsignacion => new CodeDescriptionPair("14", (NoResString)"Pago por consignación");
		public static CodeDescriptionPair Condonacion => new CodeDescriptionPair("15", (NoResString)"Condonación");
		public static CodeDescriptionPair Compensacion => new CodeDescriptionPair("17", (NoResString)"Compensación");
		public static CodeDescriptionPair Novacion => new CodeDescriptionPair("23", (NoResString)"Novación");
		public static CodeDescriptionPair Confusion => new CodeDescriptionPair("24", (NoResString)"Confusión");
		public static CodeDescriptionPair RemisionDeDeuda => new CodeDescriptionPair("25", (NoResString)"Remisión de deuda");
		public static CodeDescriptionPair PrescipcionOCaducidad => new CodeDescriptionPair("26", (NoResString)"Prescripción o caducidad");
		public static CodeDescriptionPair ASatisfaccionDelAcreedor => new CodeDescriptionPair("27", (NoResString)"A satisfacción del acreedor");
		public static CodeDescriptionPair TarjetaDeServicios => new CodeDescriptionPair("29", (NoResString)"Tarjeta de servicios");
		public static CodeDescriptionPair AmpliacionDeAnticipos => new CodeDescriptionPair("30", (NoResString)"Aplicación de anticipos");
		public static CodeDescriptionPair IntermediarioDePagos => new CodeDescriptionPair("31", (NoResString)"Intermediario pagos");
		public static CodeDescriptionPair PorDefinir => new CodeDescriptionPair("99", (NoResString)"Por definir");
	}
}
