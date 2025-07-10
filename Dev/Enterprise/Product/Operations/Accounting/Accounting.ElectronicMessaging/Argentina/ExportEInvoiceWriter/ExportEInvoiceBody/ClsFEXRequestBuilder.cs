using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.EInvoicingDependency;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.CountryCompliance.Argentina;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina
{
	public interface IClsFEXRequestBuilder
	{
		XStreamingElement BuildXML(TransactionInfo transaction, XNamespace fexv1, string registrationNumber, BusinessObjectFactory factory);
	}

	class ClsFEXRequestBuilder : IClsFEXRequestBuilder
	{
		public ClsFEXRequestBuilder()
		{
			complianceSequenceRetriever_constructorInitalizedOnly = new ComplianceSequenceRetriever();
			transactionInfoHelper_constructorInitializedOnly = new TransactionInfoHelper();
			EInvoicingExtension = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetArgentinaEInvoicingExtension();
			EInvoicingDependecies = ObjectFactory.Get<IEInvoicingDependencyFactory>().GetArgentinaEInvoicingDependencyFactory();
			ArgentinaEInvoiceHelper = EInvoicingDependecies.GetArgentinaEInvoiceHelper();
		}

		IComplianceSequenceRetriever ComplianceSequenceRetriever => complianceSequenceRetriever_constructorInitalizedOnly;
		IComplianceSequenceRetriever complianceSequenceRetriever_constructorInitalizedOnly;

		ITransactionInfoHelper TransactionInfoHelper => transactionInfoHelper_constructorInitializedOnly;
		ITransactionInfoHelper transactionInfoHelper_constructorInitializedOnly;

		readonly IArgentinaEInvoicingExtension EInvoicingExtension;
		readonly IArgentinaEInvoicingDependencyFactory EInvoicingDependecies;
		readonly IArgentinaEInvoiceHelper ArgentinaEInvoiceHelper;

#if DEBUG
		public void SubstituteComplianceSequenceRetriever_ForTestOnly(IComplianceSequenceRetriever replacement) => complianceSequenceRetriever_constructorInitalizedOnly = replacement;
		public IComplianceSequenceRetriever ComplianceSequenceRetriever_ExposedForTestOnly => ComplianceSequenceRetriever;
		public void SubstituteTransactionInfoHelper_ForTestOnly(ITransactionInfoHelper replacement) => transactionInfoHelper_constructorInitializedOnly = replacement;
		public ITransactionInfoHelper TransactionInfoHelper_ExposedForTestOnly => TransactionInfoHelper;
#endif

		XStreamingElement IClsFEXRequestBuilder.BuildXML(TransactionInfo transaction, XNamespace fexv1, string registrationNumber, BusinessObjectFactory factory)
		{
			(ZGuid branchPK, ZGuid companyPK, ZGuid departmentPK) = ArgentinaEInvoiceHelper.GetBranchCompanyAndDepartamentPKFromTransactionInfo(transaction, factory);
			AccComplianceSequence transactionComplianceSequenceFromSubType = null;
			try
			{
				transactionComplianceSequenceFromSubType = ComplianceSequenceRetriever.GetComplianceSequenceFromSubType(transaction.ComplianceSubType.GetValueOrDefault(), companyPK, branchPK, departmentPK, ZDateTime.Today);
			}
			catch (MultipleComplianceSequenceFoundException)
			{
				transactionComplianceSequenceFromSubType = null;
			}

			var compliancePrefix = ArgentinaEInvoiceHelper.GetComplianceSequencePrefixInfo(transactionComplianceSequenceFromSubType);
			var originalReferenceComplianceSubtype = ArgentinaEInvoiceHelper.GetOriginalTransactionComplianceSubType(transaction.OriginalReference);

			var monedaIdCtzInfo = EInvoicingExtension.GetCurrencyAndExchangeRateTransactionInfo(transaction);
			int multiplier = transaction.TransactionType == TransactionType.CRD ? -1 : 1;
			ZDecimal total = transaction.OSTotal.GetValueOrDefault() * multiplier;
			var isOriginalReferenceCreditOrDebitNoteTransaction = ArgentinaEInvoiceHelper.IsOriginalReferenceCreditOrDebitNoteTransaction(transaction);

			#region SuppressResourceStringsCheckRegion

			return new XStreamingElement(fexv1 + "Cmp",
				new XElement(fexv1 + "Id", GetId()),
				new XElement(fexv1 + "Fecha_cbte", transaction.TransactionDate.FormatNullableDate("yyyyMMdd")),
				new XElement(fexv1 + "Cbte_Tipo", EInvoicingExtension.GetDocumentType(transaction.ComplianceSubType.GetValueOrDefault(ZString.Empty))),
				new XElement(fexv1 + "Punto_vta", compliancePrefix),
				new XElement(fexv1 + "Cbte_nro", ZString.Empty),
				new XElement(fexv1 + "Tipo_expo", ArgentinaConstants.TypeOfExpo),
				new XElement(fexv1 + "Permiso_existente", ""),
				AfipCodeCountry(transaction, fexv1),
				BuildBuyerElementInfo(transaction, fexv1),
				new XElement(fexv1 + "Moneda_Id", monedaIdCtzInfo.MonendaId),
				new XElement(fexv1 + "Moneda_ctz", monedaIdCtzInfo.MonedaCtz),
				new XElement(fexv1 + "Imp_total", total.FormatDecimals("F2")),
				isOriginalReferenceCreditOrDebitNoteTransaction ? CompAsocBuildXML(transaction, fexv1, originalReferenceComplianceSubtype, registrationNumber) : null,
				GetPaymentMethod(transaction, fexv1),
				new XElement(fexv1 + "Idioma_cbte", "1"),
				transaction.ComplianceSubType.GetValueOrDefault() == ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE ? new XElement(fexv1 + "Fecha_pago", transaction.DueDate.FormatNullableDate("yyyyMMdd")) : null,
				ItemsBuildXML(transaction, fexv1)
				);

			#endregion
		}

		#region Implementation

		XElement GetPaymentMethod(TransactionInfo transactionInfo, XNamespace fexv1)
		{
			XElement paymentMethod = null;
			var paymentMethodDescription = ZString.Empty;
			var agreedPaymentMethod = transactionInfo.AgreedPaymentMethod.GetValueOrDefault(ZString.Empty);

			if (transactionInfo.ComplianceSubType.GetValueOrDefault() == ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE)
			{
				if (!agreedPaymentMethod.IsEmpty)
				{
					paymentMethodDescription = OrganisationsDataRegistry.Instance.ReceivablesCreditAgreedPaymentMethodsList.Value.GetCodeDescriptionPairList().GetDescriptionFromCode(agreedPaymentMethod);
				}

				paymentMethod = new XElement(fexv1 + "Forma_pago", paymentMethodDescription); // Hard-coded xml node name
			}

			return paymentMethod;
		}

		XElement AfipCodeCountry(TransactionInfo transaction, XNamespace fexv1)
		{
			var afipCodeId = ZString.Empty;
			var afipEquivalentCodes = ArgentinaConstants.AfipCodesCountries.AfipCodes;

			var afipCodeCountryCode = transaction.OrganizationAddress?.Country?.Code ?? ZString.Empty;

			if (!afipCodeCountryCode.IsEmpty)
			{
				afipCodeId = afipEquivalentCodes.ContainsKey(afipCodeCountryCode) ?
				afipEquivalentCodes[afipCodeCountryCode] :
				string.Empty;
			}

			return new XElement(fexv1 + "Dst_cmp", afipCodeId);
		}

		IEnumerable<XElement> BuildBuyerElementInfo(TransactionInfo transaction, XNamespace fexv1)
		{
			var organizationAddress = transaction.OrganizationAddress;
			var companyName = ZString.Empty;
			var companyAddress = ZString.Empty;
			var buyerXElementList = new List<XElement>();

			if (organizationAddress != null)
			{
				companyName = organizationAddress.CompanyName.GetValueOrDefault(ZString.Empty).SubstringSafe(0, 200);
				companyAddress = organizationAddress.Address1.GetValueOrDefault(ZString.Empty).SubstringSafe(0, 300);
			}

			#region SuppressResourceStringsCheckRegion

			buyerXElementList.Add(new XElement(fexv1 + "Cliente", companyName));
			buyerXElementList.Add(new XElement(fexv1 + "Domicilio_cliente", companyAddress));

			if (organizationAddress?.RegistrationNumberCollection != null && organizationAddress.RegistrationNumberCollection.Any())
			{
				var clientCountryTaxNumber = organizationAddress.RegistrationNumberCollection.FirstOrDefault(number =>
							   (number.Type?.Code ?? ZString.Empty) == ArgentinaOrgCusCodeInfo.OrgCusCodes.CUF)?.Value.GetValueOrDefault();

				if (clientCountryTaxNumber.HasValue)
				{
					buyerXElementList.Add(new XElement(fexv1 + "Cuit_pais_cliente", clientCountryTaxNumber.Value.RemoveNonNumericCharacters()));
				}
				else
				{
					var taxIdNumber = organizationAddress.RegistrationNumberCollection.FirstOrDefault()?.Value.GetValueOrDefault().ToString();
					buyerXElementList.Add(!taxIdNumber.IsNullOrEmpty() ? new XElement(fexv1 + "Id_impositivo", taxIdNumber) : null);
				}
			}

			#endregion

			return buyerXElementList;
		}

		XStreamingElement ItemsBuildXML(TransactionInfo transaction, XNamespace fexv1)
		{
			#region SuppressResourceStringsCheckRegion

			var itemsList = new List<XStreamingElement>();

			if (transaction.TransactionType.HasValue)
			{
				var sign = (transaction.TransactionType.Value == TransactionType.CRD ? -1 : 1);

				if (transaction.PostingJournalCollection != null)
				{
					foreach (var item in transaction.PostingJournalCollection)
					{
						var chargeCode = item.ChargeCode?.Code;
						if (!chargeCode.HasValue)
						{
							chargeCode = item.GLAccount?.AccountCode;
						}

						var osTotalAmount = (ZDecimal)(item.OSTotalAmount.GetValueOrDefault() * sign);

						itemsList.Add(new XStreamingElement(fexv1 + "Item",
							new XElement(fexv1 + "Pro_codigo", chargeCode.GetValueOrDefault(ZString.Empty)), // Hard - coded xml node name
							new XElement(fexv1 + "Pro_ds", item.Description.GetValueOrDefault(ZString.Empty)), // Hard - coded xml node name
							new XElement(fexv1 + "Pro_qty", ArgentinaConstants.Quantity), // Hard - coded xml node name
							new XElement(fexv1 + "Pro_umed", ArgentinaConstants.UnitOfMeasure), // Hard - coded xml node name
							new XElement(fexv1 + "Pro_precio_uni", osTotalAmount.FormatDecimals("F6")), // Hard - coded xml node name
							new XElement(fexv1 + "Pro_total_item", osTotalAmount.FormatDecimals("F2")))); // Hard - coded xml node name
					}
				}
			}

			return itemsList.Any() ? new XStreamingElement(fexv1 + "Items", itemsList) : null;

			#endregion
		}

		XStreamingElement CompAsocBuildXML(TransactionInfo transaction, XNamespace fexv1, string originalReferenceComplianceSubtype, string registrationNumber)
		{
			var complianceSubtype = EInvoicingExtension.GetDocumentType(originalReferenceComplianceSubtype);
			var originalTransactionCompliancePrefix = ArgentinaEInvoiceHelper.GetComplianceNumberPrefixFromTransaction(transaction.OriginalReference);
			var originalTransactionComplianceNumber = ArgentinaEInvoiceHelper.GetComplianceNumberFromTransaction(transaction.OriginalReference);

			var cmpsAsocList = new List<XStreamingElement>();

			#region SuppressResourceStringsCheckRegion

			var cmpsAsocItem = new XStreamingElement(fexv1 + "Cmp_asoc"); // Hard - coded xml node name

			cmpsAsocItem.Add(new XElement(fexv1 + "Cbte_tipo", complianceSubtype),
				new XElement(fexv1 + "Cbte_punto_vta", originalTransactionCompliancePrefix),
				new XElement(fexv1 + "Cbte_nro", originalTransactionComplianceNumber),
				new XElement(fexv1 + "Cbte_cuit", ((ZString)registrationNumber).RemoveNonNumericCharacters()));
			cmpsAsocList.Add(cmpsAsocItem);

			#endregion

			return cmpsAsocList.Any() ? new XStreamingElement(fexv1 + "Cmps_asoc", cmpsAsocList) : null;
		}

		long GetId()
		{
			int seed = Guid.NewGuid().GetHashCode();
			return TransactionInfoHelper.GetRandomLog(seed, 100000000000000, 999999999999999);
		}

		#endregion
	}
}
