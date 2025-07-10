using System;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.ElectronicMessaging.EInvoicingDependency;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico
{
	public interface ICFDiComprobanteBuilder
	{
		Comprobante BuildXml(TransactionInfo transaction);
	}

	class CFDiComprobanteBuilder : ICFDiComprobanteBuilder
	{
		readonly IMexicoEInvoicingDependencyFactory MexicoEInvocingDependencies;

		public CFDiComprobanteBuilder()
		{
			MexicoEInvocingDependencies = ObjectFactory.Get<IEInvoicingDependencyFactory>().GetMexicoEInvoicingDependencyFactory();

			#region Obsolete, use MexicoEInvocingDependencies for new dependencies

			cfdiEmisorBuilder_constructorInitializedOnly = new CFDiEmisorBuilder();
			cfdiReceptorBuilder_constructorInitializedOnly = new CFDiReceptorBuilder();
			cfdiConceptoBuilder_constructorInitializedOnly = new CFDiConceptoBuilder();
			cfdiImpuestosBuilder_constructorInitializedOnly = new CFDiImpuestosBuilder();
			factory_InitializedOnly = new BusinessObjectFactory();

			#endregion
		}

		Comprobante ICFDiComprobanteBuilder.BuildXml(TransactionInfo transaction)
		{
			var oComprobante = new Comprobante();

			var oEmisor = CFDiEmisorBuilder.BuildEmisorInfo(transaction);
			var oReceptor = CFDiReceptorBuilder.BuildReceptorInfo(transaction);
			var oConceptos = CFDiConceptoBuilder.BuildComprobanteConceptoInfo(transaction, factory_InitializedOnly);
			var oImpuestos = CFDiImpuestosBuilder.BuildComprobanteImpuestosInfo(oConceptos);
			var oRelacionados = MexicoEInvocingDependencies.GetCFDiRelacionadosBuilder().BuildRelacionadosInfo(transaction);

			var getCurrencyRelatedValues = GetCurrencyRelatedValues(transaction);

			oComprobante.Version = "4.0";
			oComprobante.Moneda = getCurrencyRelatedValues.Moneda;
			oComprobante.TipoDeComprobante = GetTipoComprobante(transaction);
			oComprobante.Exportacion = c_Exportacion.NoAplica;

			if (transaction.Number.HasValue)
			{
				oComprobante.Folio = transaction.Number.Value;
			}

			if (oConceptos != null)
			{
				var localSubTotal = oConceptos.Length > 0 ? oConceptos.Sum(x => x.Importe) : 0M;
				oComprobante.SubTotal = localSubTotal;
				oComprobante.Total = localSubTotal;

				if (oImpuestos != null)
				{
					var totalRetenciones = oImpuestos.TotalImpuestosRetenidosSpecified ? oImpuestos.TotalImpuestosRetenidos : 0M;
					var totalTraslados = oImpuestos.TotalImpuestosTrasladadosSpecified ? oImpuestos.TotalImpuestosTrasladados : 0M;
					oComprobante.Total = (localSubTotal + totalTraslados) - totalRetenciones;
				}
			}

			if (transaction.TransactionDate.HasValue)
			{
				oComprobante.Fecha = transaction.TransactionDate.Value.ToDateTime();
			}

			if (transaction.BranchAddress != null && transaction.BranchAddress.Postcode.HasValue)
			{
				oComprobante.LugarExpedicion = transaction.BranchAddress.Postcode.Value;
			}

			oComprobante.TipoCambioSpecified = false;
			if (getCurrencyRelatedValues.TipoCambio.HasValue)
			{
				oComprobante.TipoCambioSpecified = true;
				oComprobante.TipoCambio = getCurrencyRelatedValues.TipoCambio.Value;
			}

			oComprobante.MetodoPagoSpecified = false;
			if (transaction.InvoiceTerm.HasValue)
			{
				var codeFromInvoiceTerm = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.Mexico) as IInvoicePaymentMethodProvider)
					?.GetInvoicePaymentMethodProvider().GetInvoicePaymentMethod(transaction.InvoiceTerm.ToString(), transaction.TransactionDate, transaction.DueDate);

				if (codeFromInvoiceTerm != null && !codeFromInvoiceTerm.Code.IsNullOrEmpty())
				{
					oComprobante.MetodoPago = (c_MetodoPago)Enum.Parse(typeof(c_MetodoPago), codeFromInvoiceTerm.Code);
					oComprobante.MetodoPagoSpecified = true;
				}
			}

			oComprobante.FormaPagoSpecified = false;
			var formaPagoEnum = SetFormaPago(transaction.AgreedPaymentMethod, oComprobante.MetodoPago.ToString());
			if (formaPagoEnum.HasValue)
			{
				oComprobante.FormaPago = formaPagoEnum.Value;
				oComprobante.FormaPagoSpecified = true;
			}

			if (oComprobante != null)
			{
				oComprobante.Emisor = oEmisor;
				oComprobante.Receptor = oReceptor;
				oComprobante.Conceptos = oConceptos;
				oComprobante.Impuestos = oImpuestos;
				oComprobante.CfdiRelacionados = oRelacionados;
			}

			return oComprobante;
		}

		static c_TipoDeComprobante GetTipoComprobante(TransactionInfo transaction)
		{
			if (transaction.ComplianceSubType.HasValue)
			{
				switch (transaction.ComplianceSubType.Value)
				{
					case MexicoComplianceInfo.ComplianceSubTypeCodes.TXI:
					case MexicoComplianceInfo.ComplianceSubTypeCodes.TDR:
						return c_TipoDeComprobante.I;
					case MexicoComplianceInfo.ComplianceSubTypeCodes.TCR:
						return c_TipoDeComprobante.E;
					default:
						throw new ArgumentException("Invalid Compliance Sub Type.");
				}
			}
			throw new ArgumentException("Compliance Sub Type must have a value.");
		}

		static (decimal? TipoCambio, c_Moneda Moneda) GetCurrencyRelatedValues(TransactionInfo transaction)
		{
			if (transaction?.OSCurrency != null
				&& Enum.TryParse(transaction.OSCurrency.Code, out c_Moneda monedaEnumValue)
				&& Enum.IsDefined(typeof(c_Moneda), monedaEnumValue))
			{
				if (monedaEnumValue != c_Moneda.MXN && transaction.ExchangeRate.HasValue)
				{
					return (transaction.ExchangeRate.Value, monedaEnumValue);
				}
				else
				{
					return (null, monedaEnumValue);
				}
			}

			return (null, c_Moneda.XXX);
		}

		static c_FormaPago? SetFormaPago(ZString? agreedPaymentMethodFromTransaction, ZString paymentMethod)
		{
			var codeDescriptionPair = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.Mexico) as IEquivalentAgreedPaymentMethodProvider)
				?.GetEquivalentAgreedPaymentMethodProvider().GetEquivalentAgreedPaymentMethod(agreedPaymentMethodFromTransaction.GetValueOrDefault(), paymentMethod);

			var agreedPaymentHasACorrectEnumValue = GetEnumFromString(typeof(c_FormaPago), codeDescriptionPair?.Code ?? ZString.Empty);

			return agreedPaymentHasACorrectEnumValue != null
				? (c_FormaPago)agreedPaymentHasACorrectEnumValue
				: null;
		}

		static object GetEnumFromString(Type enumTypeParam, string enumValueParam)
		{
			foreach (var currentEnum in Enum.GetValues(enumTypeParam))
			{
				var member = enumTypeParam.GetMember(currentEnum.ToString()).FirstOrDefault();
				var attribute = member.GetCustomAttributes(false).OfType<XmlEnumAttribute>().FirstOrDefault();
				if (enumValueParam == attribute.Name)
				{
					return currentEnum;
				}
			}
			return null;
		}

		ICFDiEmisorBuilder CFDiEmisorBuilder => cfdiEmisorBuilder_constructorInitializedOnly;
		ICFDiEmisorBuilder cfdiEmisorBuilder_constructorInitializedOnly;
		ICFDiReceptorBuilder CFDiReceptorBuilder => cfdiReceptorBuilder_constructorInitializedOnly;
		ICFDiReceptorBuilder cfdiReceptorBuilder_constructorInitializedOnly;
		ICFDiConceptoBuilder CFDiConceptoBuilder => cfdiConceptoBuilder_constructorInitializedOnly;
		ICFDiConceptoBuilder cfdiConceptoBuilder_constructorInitializedOnly;
		ICFDiImpuestosBuilder CFDiImpuestosBuilder => cfdiImpuestosBuilder_constructorInitializedOnly;
		ICFDiImpuestosBuilder cfdiImpuestosBuilder_constructorInitializedOnly;
		BusinessObjectFactory factory_InitializedOnly;
#if DEBUG
		public void SubstituteCFDiEmisorBuilder_ForTestOnly(ICFDiEmisorBuilder replacement) => cfdiEmisorBuilder_constructorInitializedOnly = replacement;
		public ICFDiEmisorBuilder CFDiEmisorBuilder_ExposedForTestOnly => CFDiEmisorBuilder;
		public void SubstituteCFDiConceptoBuilder_ForTestOnly(ICFDiConceptoBuilder replacement) => cfdiConceptoBuilder_constructorInitializedOnly = replacement;
		public ICFDiConceptoBuilder CFDiConceptoBuilder_ExposedForTestOnly => CFDiConceptoBuilder;
		public void SubstituteCFDiReceptorBuilder_ForTestOnly(ICFDiReceptorBuilder replacement) => cfdiReceptorBuilder_constructorInitializedOnly = replacement;
		public ICFDiReceptorBuilder CFDiReceptorBuilder_ExposedForTestOnly => CFDiReceptorBuilder;
		public void SubstituteCFDiImpuestosBuilder_ForTestOnly(ICFDiImpuestosBuilder replacement) => cfdiImpuestosBuilder_constructorInitializedOnly = replacement;
		public ICFDiImpuestosBuilder CFDiImpuestosBuilder_ExposedForTestOnly => CFDiImpuestosBuilder;
		public void SubstituteFactory_ForTestOnly(BusinessObjectFactory replacement) => factory_InitializedOnly = replacement;
#endif

	}
}
