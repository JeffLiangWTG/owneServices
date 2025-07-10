using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Mexico.Testing
{
	public class MexicoEquivalentAgreedPaymentMethodTest : TestCaseWithFactory
	{
		public void TestEquivalentCodeForAgreedPaymentMethod()
		{
			var wrapperFormaPago = new List<Tuple<string, string, CodeDescriptionPair>>
			{
				new Tuple<string, string, CodeDescriptionPair>(OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck , string.Empty, DocWrapperMappingFormaPago.Efectivo ),
				new Tuple<string, string, CodeDescriptionPair>(OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck, string.Empty, DocWrapperMappingFormaPago.ChequeNominativo),
				new Tuple<string, string, CodeDescriptionPair>(OrgConstants.CreditAgreedPaymentMethods.Code.CollectionRequest, string.Empty, DocWrapperMappingFormaPago.TransferenciaElectronica),
				new Tuple<string, string, CodeDescriptionPair>(OrgConstants.CreditAgreedPaymentMethods.Code.BankTransfer, string.Empty, DocWrapperMappingFormaPago.TransferenciaElectronica),
				new Tuple<string, string, CodeDescriptionPair>(OrgConstants.CreditAgreedPaymentMethods.Code.CreditCard, string.Empty, DocWrapperMappingFormaPago.TarjetaCredito),
				new Tuple<string, string, CodeDescriptionPair>(OrgConstants.CreditAgreedPaymentMethods.Code.DebitCard, string.Empty, DocWrapperMappingFormaPago.TarjetaDebito),
				new Tuple<string, string, CodeDescriptionPair>(string.Empty, string.Empty, DocWrapperMappingFormaPago.PorDefinir),
				new Tuple<string, string, CodeDescriptionPair>("23", string.Empty, DocWrapperMappingFormaPago.Novacion),
				new Tuple<string, string, CodeDescriptionPair>("XXX", string.Empty, new CodeDescriptionPair("", "")),
				new Tuple<string, string, CodeDescriptionPair>(string.Empty, DocWrapperMappingMetodoPago.PPD.Code, DocWrapperMappingFormaPago.PorDefinir),
				new Tuple<string, string, CodeDescriptionPair>(OrgConstants.CreditAgreedPaymentMethods.Code.DebitCard, DocWrapperMappingMetodoPago.PPD.Code, DocWrapperMappingFormaPago.PorDefinir),
				new Tuple<string, string, CodeDescriptionPair>(OrgConstants.CreditAgreedPaymentMethods.Code.DebitCard, DocWrapperMappingMetodoPago.PUE.Code, DocWrapperMappingFormaPago.TarjetaDebito),
			};

			var result = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Core.Constants.CountryCodes.Mexico) as IEquivalentAgreedPaymentMethodProvider).GetEquivalentAgreedPaymentMethodProvider();

			foreach (var formapago in wrapperFormaPago)
			{
				AssertEquals($"Equivalent Code for the Agreed Payment Method {formapago.Item1} and {formapago.Item2}", formapago.Item3, result.GetEquivalentAgreedPaymentMethod(formapago.Item1, formapago.Item2));
			}
		}

		public void TestAgreedPaymentValidForMexico()
		{
			var wrapperFormaPago = new CodeDescriptionPairList();

			wrapperFormaPago.AddPair("01", "Efectivo");
			wrapperFormaPago.AddPair("02", "Cheque nominativo");
			wrapperFormaPago.AddPair("03", "Transferencia electrónica de fondos");
			wrapperFormaPago.AddPair("04", "Tarjeta de crédito");
			wrapperFormaPago.AddPair("28", "Tarjeta de débito");
			wrapperFormaPago.AddPair("05", "Monedero electrónico");
			wrapperFormaPago.AddPair("06", "Dinero electrónico");
			wrapperFormaPago.AddPair("08", "Vales de despensa");
			wrapperFormaPago.AddPair("12", "Dación en pago");
			wrapperFormaPago.AddPair("13", "Pago por subrogación");
			wrapperFormaPago.AddPair("14", "Pago por consignación");
			wrapperFormaPago.AddPair("15", "Condonación");
			wrapperFormaPago.AddPair("17", "Compensación");
			wrapperFormaPago.AddPair("23", "Novación");
			wrapperFormaPago.AddPair("24", "Confusión");
			wrapperFormaPago.AddPair("25", "Remisión de deuda");
			wrapperFormaPago.AddPair("26", "Prescripción o caducidad");
			wrapperFormaPago.AddPair("27", "A satisfacción del acreedor");
			wrapperFormaPago.AddPair("29", "Tarjeta de servicios");
			wrapperFormaPago.AddPair("30", "Aplicación de anticipos");
			wrapperFormaPago.AddPair("31", "Intermediario pagos");
			wrapperFormaPago.AddPair("99", "Por definir");

			foreach (CodeDescriptionPair formaPago in wrapperFormaPago)
			{
				AssertEquals($"the Agreed Payment Method {formaPago.Code} not exist in DocWrapperMappingFormaPago", formaPago, DocWrapperMappingFormaPago.GetCodeFromList(formaPago.Code));
			}
		}

		public void TestAgreedPaymentMethodIsNotValid()
		{
			AssertNull(DocWrapperMappingFormaPago.GetCodeFromList("CBC"));
		}

		public void TestAgreedPaymentMethodIsNull()
		{
			AssertNull(DocWrapperMappingFormaPago.GetCodeFromList(null));
		}

		public void TestAgreedPaymentMethodIsEmpty()
		{
			AssertNull(DocWrapperMappingFormaPago.GetCodeFromList(ZString.Empty));
		}
	}
}
