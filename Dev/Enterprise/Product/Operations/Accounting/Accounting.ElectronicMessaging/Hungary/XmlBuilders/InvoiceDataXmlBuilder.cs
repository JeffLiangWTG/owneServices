using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Common;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Messaging.Integration;

namespace Enterprise.Accounting.ElectronicMessaging.Hungary
{
	/// <summary>
	/// Builds a Hungary GEN ManageInvoiceRequest XML document.
	/// IMPORTANT: no CW1 database access is permitted; all data must be in ManageInvoiceRequestModel.
	/// </summary>
	public class InvoiceDataXmlBuilder
	{
		public InvoiceDataXmlBuilder(InvoiceDataModel model)
		{
			Model = Argument.NotNull(model, nameof(model));
		}

		InvoiceDataModel Model { get; }

		public string GetInvoiceDataAsBase64()
		{
			var result = string.Empty;
			IXmlElement xmlElement = BuildInvoiceData();
			if (xmlElement != null)
			{
				var settings = new XmlWriterSettings
				{
					ConformanceLevel = ConformanceLevel.Document,
					OmitXmlDeclaration = false,
					NamespaceHandling = NamespaceHandling.OmitDuplicates,
					Encoding = MessageEncoding.UTF8WithoutBOM,
					NewLineHandling = NewLineHandling.Replace,
					Indent = false
				};

				using (var stream = new MemoryStream())
				{
					xmlElement.WriteToXmlStream(stream, settings);
					result = Convert.ToBase64String(stream.ToArray());
				}
			}
			return result;
		}

		public ComplexXmlElement BuildInvoiceData()
		{
			var invoiceDataXml = new ComplexXmlElement(DocumentXmlns
											, Tag_InvoiceData
											, new IXmlElementWithSequence(new SimpleXmlElement(DocumentXmlns, Tag_InvoiceNumber, Model.InvoiceNumber), sequence: 1)
											, new IXmlElementWithSequence(new SimpleXmlElement(DocumentXmlns, Tag_InvoiceIssueDate, Model.InvoiceIssueDateFormatted), sequence: 2)
											, new IXmlElementWithSequence(new ComplexXmlElement(DocumentXmlns
																							, Tag_InvoiceMain
																							, BuildInvoiceMain()), sequence: 4));
			invoiceDataXml.Attributes.AddRange(BuildNameSpaceAttributes());

			invoiceDataXml.Children.Add(new IXmlElementWithSequence(new SimpleXmlElement(DocumentXmlns, Tag_CompletenessIndicator, Model.InvoiceCompletenessIndicator), sequence: 3));

			return invoiceDataXml;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "xml namespace for HU EInvoice")]
		IEnumerable<XmlElementAttribute> BuildNameSpaceAttributes() =>
			new[]
			{
				new XmlElementAttribute("xmlns", DocumentXmlns),
				new XmlElementAttribute(XNamespace.Xmlns + "ns2", baseNS)
			};

		public ComplexXmlElement BuildInvoiceMain()
		{
			return new ComplexXmlElement(DocumentXmlns
									, Tag_InvoiceMain_Invoice
									, BuildInvoiceReference()
									, new ComplexXmlElement(DocumentXmlns
															, Tag_InvoiceMain_Invoice_InvoiceHead
															, BuildSupplierInfo()
															, BuildCustomerInfo()
															, BuildInvoiceDetail())
									, BuildInvoiceLines()
									, BuildInvoiceSummary());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Lower case boolean string required by external XSD")]
		public ComplexXmlElement BuildInvoiceReference()
		{
			if (Model.InvoiceReference != null)
			{
				return new ComplexXmlElement(DocumentXmlns
										, Tag_InvoiceReference
										, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceReference_OriginalInvoiceNumber, Model.InvoiceReference.FirstOriginalInvoiceNumber)
										, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceReference_ModifyWithoutMaster, Model.InvoiceReference.WasModifiedWithoutMaster)
										, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceReference_ModificationIndex, Model.InvoiceReference.ModificationIndex));
			}
			return null;
		}

		public ComplexXmlElement BuildSupplierInfo()
		{
			var supplierInfo = Model.SupplierInfo;
			return new ComplexXmlElement(DocumentXmlns
								, Tag_SupplierInfo
								, new ComplexXmlElement(DocumentXmlns
													, Tag_SupplierInfo_SupplierTaxNumber
													, new SimpleXmlElement(BaseXmlns, Tag_TaxpayerId, supplierInfo.TaxNumber))
								, new ComplexXmlElement(DocumentXmlns
													, Tag_SupplierInfo_GroupTaxNumber
													, new SimpleXmlElement(BaseXmlns, Tag_TaxpayerId, supplierInfo.GroupMemberTaxNumber))
								, new SimpleXmlElement(DocumentXmlns, Tag_SupplierInfo_CommunityVatNumber, supplierInfo.CommunityMemberVatNumber)
								, new SimpleXmlElement(DocumentXmlns, Tag_SupplierInfo_SupplierName, supplierInfo.Name)
								, new ComplexXmlElement(DocumentXmlns
													, Tag_SupplierInfo_SupplierAddress
													, new ComplexXmlElement(BaseXmlns
																		, Tag_SupplierInfo_SupplierAddress_SimpleAddress
																		, new SimpleXmlElement(BaseXmlns, Tag_CountryCode, supplierInfo.CountryCode, allowNull: true)
																		, new SimpleXmlElement(BaseXmlns, Tag_PostalCode, supplierInfo.PostCode)
																		, new SimpleXmlElement(BaseXmlns, Tag_City, supplierInfo.City)
																		, new SimpleXmlElement(BaseXmlns, Tag_AdditionalAddressDetail, supplierInfo.AdditionalAddressDetail))));
		}

		public ComplexXmlElement BuildCustomerInfo()
		{
			if (Model.CustomerInfo.IsPrivatePerson)
			{
				return new ComplexXmlElement(DocumentXmlns
											, Tag_CustomerInfo
											, new SimpleXmlElement(DocumentXmlns, Tag_CustomerInfo_CustomerVatStatus, Model.CustomerInfo.VATStatus));
			}
			else
			{
				var customerInfo = Model.CustomerInfo;
				return new ComplexXmlElement(DocumentXmlns
											, Tag_CustomerInfo
											, new SimpleXmlElement(DocumentXmlns, Tag_CustomerInfo_CustomerVatStatus, customerInfo.VATStatus)
											, new ComplexXmlElement(DocumentXmlns
																	, Tag_CustomerInfo_CustomerVatData
																	, new ComplexXmlElement(DocumentXmlns
																							, Tag_CustomerInfo_CustomerVatData_CustomerTaxNumber
																							, new SimpleXmlElement(BaseXmlns, Tag_TaxpayerId, customerInfo.TaxNumber)
																							, new ComplexXmlElement(DocumentXmlns
																													, Tag_CustomerInfo_CustomerVatData_CustomerTaxNumber_GroupTaxNumber
																													, new SimpleXmlElement(BaseXmlns, Tag_TaxpayerId, customerInfo.GroupMemberTaxNumber)))
																	, new SimpleXmlElement(DocumentXmlns, Tag_CustomerInfo_CustomerVatData_CommunityVatNumber, customerInfo.CommunityMemberVatNumber)
																	, new SimpleXmlElement(DocumentXmlns, Tag_CustomerInfo_CustomerVatData_ThirdStateTaxId, customerInfo.ThirdStateTaxId))
											, new SimpleXmlElement(DocumentXmlns, Tag_CustomerInfo_CustomerName, customerInfo.Name)
											, new ComplexXmlElement(DocumentXmlns
																	, Tag_CustomerInfo_CustomerAddress
																	, new ComplexXmlElement(BaseXmlns
																							, Tag_CustomerInfo_CustomerAddress_SimpleAddress
																							, new SimpleXmlElement(BaseXmlns, Tag_CountryCode, customerInfo.CountryCode)
																							, new SimpleXmlElement(BaseXmlns, Tag_PostalCode, customerInfo.PostCode)
																							, new SimpleXmlElement(BaseXmlns, Tag_City, customerInfo.City)
																							, new SimpleXmlElement(BaseXmlns, Tag_AdditionalAddressDetail, customerInfo.AdditionalAddressDetail))));
			}
		}

		public ComplexXmlElement BuildInvoiceDetail()
		{
			return new ComplexXmlElement(DocumentXmlns
								, Tag_InvoiceDetail
								, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceDetail_InvoiceCategory, Model.InvoiceCategory)
								, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceDetail_InvoiceDeliveryDate, Model.InvoiceDeliveryDateFormatted)
								, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceDetail_CurrencyCode, Model.CurrencyCode)
								, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceDetail_ExchangeRate, Model.ExchangeRateFormatted)
								, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceDetail_PaymentDate, Model.PaymentDateFormatted)
								, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceDetail_InvoiceAppearance, Model.InvoiceAppearance));
		}

		public ComplexXmlElement BuildInvoiceLines()
		{
			var element = new ComplexXmlElement(DocumentXmlns
								, Tag_InvoiceLines
								, allowNull: true
								, new IXmlElementWithSequence(new CollectionXmlElement<InvoiceLineInfo>(Model.InvoiceLines
																								, (line) => new ComplexXmlElement(DocumentXmlns
																																, Tag_InvoiceLines_Line
																																, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceLines_Line_LineNumber, line.LineNumber)
																																, GetLineModificationReference(line)
																																, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceLines_Line_LineExpressionIndicator, line.LineExpressionIndicator)
																																, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceLines_Line_LineNatureIndicator, line.LineNatureIndicator)
																																, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceLines_Line_LineDescription, line.LineDescription)
																																, new ComplexXmlElement(DocumentXmlns
																																						, Tag_InvoiceLines_Line_LineAmountsNormal
																																						, new ComplexXmlElement(DocumentXmlns
																																											, Tag_InvoiceLines_Line_LineAmountsNormal_LineNetAmountData
																																											, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceLines_Line_LineAmountsNormal_LineNetAmountData_LineNetAmount, line.LineNetAmountFormatted)
																																											, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceLines_Line_LineAmountsNormal_LineNetAmountData_lineNetAmountHUF, line.lineNetAmountHUFFormatted))
																																						, GetVATRateElement(line))
																																, BuildInvoiceLinesAggregateInvoiceLineData(line)))
																, sequence: 2));
			element.Children.Add(new IXmlElementWithSequence(new SimpleXmlElement(DocumentXmlns, Tag_InvoiceOperations_InvoiceOperation_MergeItemIndicator, Model.MergeItemIndicator), sequence: 1));
			return element;
		}

		IXmlElement GetLineModificationReference(InvoiceLineInfo line)
		{
			return !line.LineModificationReference.HasValue ? null
					: new ComplexXmlElement(DocumentXmlns, Tag_InvoiceLines_Line_LineModificationReference,
						new SimpleXmlElement(DocumentXmlns, Tag_InvoiceLines_Line_LineModificationReference_LineNumberReference, line.LineModificationReference.Value),
						new SimpleXmlElement(DocumentXmlns, Tag_InvoiceLines_Line_LineModificationReference_LineOperation, "CREATE"));
		}

		public ComplexXmlElement BuildInvoiceSummary()
		{
			return new ComplexXmlElement(DocumentXmlns
										, Tag_InvoiceSummary
										, new ComplexXmlElement(DocumentXmlns
																, Tag_InvoiceSummary_SummaryNormal
																, BuildInvoiceSummaryGroupedByVatRate()			
																, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceSummary_SummaryNormal_InvoiceNetAmount, Model.InvoiceNetAmountFormatted)   // decimal format string as required by XSD
																, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceSummary_SummaryNormal_InvoiceNetAmountHUF, Model.InvoiceNetAmountHUFFormatted)  // decimal format string as required by XSD
																, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceSummary_SummaryNormal_InvoiceVatAmount, Model.InvoiceVatAmountFormatted)   // decimal format string as required by XSD
																, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceSummary_SummaryNormal_InvoiceVatAmountHUF, Model.InvoiceVatAmountHUFFormatted)) // decimal format string as required by XSD		
										, BuildInvoiceSummaryGrossData()
										); 													
		}

		CollectionXmlElement<VatRateAggregateWithTypeInfo> BuildInvoiceSummaryGroupedByVatRate()
		{
			return new CollectionXmlElement<VatRateAggregateWithTypeInfo>(Model.InvoiceSummaryByVATRate
																	, (vr) => new ComplexXmlElement(DocumentXmlns
																									, Tag_InvoiceSummary_SummaryNormal_SummaryByVatRate
																									, new ComplexXmlElement(DocumentXmlns
																															, Tag_InvoiceSummary_SummaryNormal_SummaryByVatRate_VatRate
																															, GetVATRateElement(vr.Key))
																									, new ComplexXmlElement(DocumentXmlns
																															, Tag_InvoiceSummary_SummaryNormal_SummaryByVatRate_VatRateNetData
																															, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceSummary_SummaryNormal_SummaryByVatRate_VatRateNetData_VatRateNetAmount, vr.NetAmountOSFormatted)
																															, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceSummary_SummaryNormal_SummaryByVatRate_VatRateNetData_VatRateNetAmountHUF, vr.NetAmountHUFFormatted))
																									, new ComplexXmlElement(DocumentXmlns
																															, Tag_InvoiceSummary_SummaryNormal_SummaryByVatRate_VatRateVatData
																															, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceSummary_SummaryNormal_SummaryByVatRate_VatRateVatData_VatRateVatAmount, vr.VatAmountOSFormatted)
																															, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceSummary_SummaryNormal_SummaryByVatRate_VatRateVatData_VatRateVatAmountHUF, vr.VatAmountHUFFormatted))
																									, new ComplexXmlElement(DocumentXmlns
																															, Tag_InvoiceSummary_SummaryNormal_SummaryByVatRate_VatRateGrossData
																															, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceSummary_SummaryNormal_SummaryByVatRate_VatRateGrossData_VatRateGrossAmount, vr.TotalAmountOSFormatted)
																															, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceSummary_SummaryNormal_SummaryByVatRate_VatRateGrossData_VatRateGrossAmountHUF, vr.TotalAmountHUFFormatted))));
		}

		ComplexXmlElement BuildInvoiceSummaryGrossData()
		{
			return new ComplexXmlElement(DocumentXmlns
					, Tag_InvoiceSummary_SummaryNormal_SummaryGrossData
					, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceSummary_SummaryNormal_SummaryGrossData_InvoiceGrossAmount, Model.InvoiceGrossAmountFormatted)
					, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceSummary_SummaryNormal_SummaryGrossData_InvoiceGrossAmountHUF, Model.InvoiceGrossAmountHUFFormatted));
		}

		ComplexXmlElement BuildInvoiceLinesAggregateInvoiceLineData(InvoiceLineInfo line)
		{
			if (Model.InvoiceCategory == "AGGREGATE")        // hard coded string as defined in XSD
			{
				return new ComplexXmlElement(DocumentXmlns
					, Tag_InvoiceLines_Line_AggregateInvoiceLineData
					, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceLines_Line_AggregateInvoiceLineData_LineExchangeRate, line.LineExchangeRateFormatted)
					, new SimpleXmlElement(DocumentXmlns, Tag_InvoiceLines_Line_AggregateInvoiceLineData_LineDeliveryDate, line.LineDeliveryDateFormatted));
			}

			return null;
		}

		IXmlElement GetVATRateElement(InvoiceLineInfo line)
		{
			var vatRateElement = GetVATRateElement(new VATRateKey()
			{
				VATExemption = line.VATExemption,
				VATRateValue = line.LineVatRate,
				VATRateType = line.VATType
			});

			var lineVatRateElement = new ComplexXmlElement(DocumentXmlns
														, Tag_InvoiceLines_Line_LineAmountsNormal_LineVatRate
														, allowNull: true
														, vatRateElement);
			return lineVatRateElement;
		}

		IXmlElement GetVATRateElement(VATRateKey key)
		{
			var tagName = GetVATRateTagName(key.VATRateType);
			switch (key.VATRateType)
			{
				case VATRateTagType.VatPercentage:
				case VATRateTagType.VatDomesticReverseCharge:
					return new SimpleXmlElement(DocumentXmlns, tagName, key.VATRateValue);
				case VATRateTagType.VatExemption:
				case VATRateTagType.VatOutOfScope:
					return new ComplexXmlElement(DocumentXmlns
						, tagName
						, new SimpleXmlElement(DocumentXmlns, Tag_Case, key.VATExemption?.Case)
						, new SimpleXmlElement(DocumentXmlns, Tag_Reason, key.VATExemption?.Reason));
				default:
					return null;
			}
		}

		string GetVATRateTagName(VATRateTagType rateTagType)
		{
			switch (rateTagType)
			{
				case VATRateTagType.VatDomesticReverseCharge:
					return Tag_VatDomesticReverseCharge;
				case VATRateTagType.VatExemption:
					return Tag_VatExemption;
				case VATRateTagType.VatOutOfScope:
					return Tag_VatOutOfScope;
				case VATRateTagType.VatPercentage:
					return Tag_VatPercentage;
				case VATRateTagType.None:
				default:
					return string.Empty;
			}
		}

		XNamespace DocumentXmlns => "http://schemas.nav.gov.hu/OSA/3.0/data";

		XNamespace BaseXmlns => baseNS;

		#region XML Constants
		#region SuppressResourceStringsCheckRegion

		const string baseNS = "http://schemas.nav.gov.hu/OSA/3.0/base";

		const string Tag_InvoiceData = "InvoiceData";
		const string Tag_InvoiceNumber = "invoiceNumber";
		const string Tag_InvoiceIssueDate = "invoiceIssueDate";

		#region <invoiceMain>

		const string Tag_InvoiceMain = "invoiceMain";
		const string Tag_InvoiceMain_Invoice = "invoice";
		const string Tag_InvoiceMain_Invoice_InvoiceHead = "invoiceHead";

		#endregion

		#region <invoiceData>

		const string Tag_CompletenessIndicator = "completenessIndicator";

		#endregion

		#region <invoiceReference>

		const string Tag_InvoiceReference = "invoiceReference";
		const string Tag_InvoiceReference_OriginalInvoiceNumber = "originalInvoiceNumber";
		const string Tag_InvoiceReference_ModifyWithoutMaster = "modifyWithoutMaster";
		const string Tag_InvoiceReference_ModificationIndex = "modificationIndex";

		#endregion

		#region <supplierInfo>

		const string Tag_SupplierInfo = "supplierInfo";
		const string Tag_SupplierInfo_SupplierTaxNumber = "supplierTaxNumber";
		const string Tag_SupplierInfo_GroupTaxNumber = "groupMemberTaxNumber";
		const string Tag_SupplierInfo_CommunityVatNumber = "communityVatNumber";
		const string Tag_SupplierInfo_SupplierName = "supplierName";
		const string Tag_SupplierInfo_SupplierAddress = "supplierAddress";
		const string Tag_SupplierInfo_SupplierAddress_SimpleAddress = "simpleAddress";

		#endregion

		#region <customerInfo>

		const string Tag_CustomerInfo = "customerInfo";
		const string Tag_CustomerInfo_CustomerVatData_CustomerTaxNumber = "customerTaxNumber";
		const string Tag_CustomerInfo_CustomerVatData_CustomerTaxNumber_GroupTaxNumber = "groupMemberTaxNumber";
		const string Tag_CustomerInfo_CustomerVatData_CommunityVatNumber = "communityVatNumber";
		const string Tag_CustomerInfo_CustomerName = "customerName";
		const string Tag_CustomerInfo_CustomerAddress = "customerAddress";
		const string Tag_CustomerInfo_CustomerAddress_SimpleAddress = "simpleAddress";
		const string Tag_CustomerInfo_CustomerVatStatus = "customerVatStatus";
		const string Tag_CustomerInfo_CustomerVatData = "customerVatData";
		const string Tag_CustomerInfo_CustomerVatData_ThirdStateTaxId = "thirdStateTaxId";

		#endregion

		#region <invoiceDetail>

		const string Tag_InvoiceDetail = "invoiceDetail";
		const string Tag_InvoiceDetail_InvoiceCategory = "invoiceCategory";
		const string Tag_InvoiceDetail_InvoiceDeliveryDate = "invoiceDeliveryDate";
		const string Tag_InvoiceDetail_CurrencyCode = "currencyCode";
		const string Tag_InvoiceDetail_ExchangeRate = "exchangeRate";
		const string Tag_InvoiceDetail_InvoiceAppearance = "invoiceAppearance";
		const string Tag_InvoiceDetail_PaymentDate = "paymentDate";

		#endregion

		#region <invoiceLines>

		const string Tag_InvoiceLines = "invoiceLines";
		const string Tag_InvoiceLines_Line = "line";
		const string Tag_InvoiceLines_Line_LineNumber = "lineNumber";
		const string Tag_InvoiceLines_Line_LineExpressionIndicator = "lineExpressionIndicator";
		const string Tag_InvoiceLines_Line_LineModificationReference = "lineModificationReference";
		const string Tag_InvoiceLines_Line_LineModificationReference_LineNumberReference = "lineNumberReference";
		const string Tag_InvoiceLines_Line_LineModificationReference_LineOperation = "lineOperation";
		const string Tag_InvoiceLines_Line_LineNatureIndicator = "lineNatureIndicator";
		const string Tag_InvoiceLines_Line_LineDescription = "lineDescription";
		const string Tag_InvoiceLines_Line_LineAmountsNormal = "lineAmountsNormal";
		const string Tag_InvoiceLines_Line_LineAmountsNormal_LineNetAmountData = "lineNetAmountData";
		const string Tag_InvoiceLines_Line_LineAmountsNormal_LineNetAmountData_LineNetAmount = "lineNetAmount";
		const string Tag_InvoiceLines_Line_LineAmountsNormal_LineNetAmountData_lineNetAmountHUF = "lineNetAmountHUF";
		const string Tag_InvoiceLines_Line_LineAmountsNormal_LineVatRate = "lineVatRate";
		const string Tag_InvoiceLines_Line_AggregateInvoiceLineData = "aggregateInvoiceLineData";
		const string Tag_InvoiceLines_Line_AggregateInvoiceLineData_LineExchangeRate = "lineExchangeRate";
		const string Tag_InvoiceLines_Line_AggregateInvoiceLineData_LineDeliveryDate = "lineDeliveryDate";

		const string Tag_InvoiceOperations_InvoiceOperation_MergeItemIndicator = "mergedItemIndicator";
		const string Tag_Case = "case";
		const string Tag_Reason = "reason";

		#endregion

		#region <invoiceSummary>

		const string Tag_InvoiceSummary = "invoiceSummary";
		const string Tag_InvoiceSummary_SummaryNormal = "summaryNormal";
		const string Tag_InvoiceSummary_SummaryNormal_SummaryByVatRate = "summaryByVatRate";
		const string Tag_InvoiceSummary_SummaryNormal_SummaryByVatRate_VatRate = "vatRate";
		const string Tag_InvoiceSummary_SummaryNormal_SummaryByVatRate_VatRateNetData = "vatRateNetData";
		const string Tag_InvoiceSummary_SummaryNormal_SummaryByVatRate_VatRateNetData_VatRateNetAmount = "vatRateNetAmount";
		const string Tag_InvoiceSummary_SummaryNormal_SummaryByVatRate_VatRateNetData_VatRateNetAmountHUF = "vatRateNetAmountHUF";
		const string Tag_InvoiceSummary_SummaryNormal_SummaryByVatRate_VatRateVatData = "vatRateVatData";
		const string Tag_InvoiceSummary_SummaryNormal_SummaryByVatRate_VatRateVatData_VatRateVatAmount = "vatRateVatAmount";
		const string Tag_InvoiceSummary_SummaryNormal_SummaryByVatRate_VatRateVatData_VatRateVatAmountHUF = "vatRateVatAmountHUF";
		const string Tag_InvoiceSummary_SummaryNormal_SummaryByVatRate_VatRateGrossData = "vatRateGrossData";
		const string Tag_InvoiceSummary_SummaryNormal_SummaryByVatRate_VatRateGrossData_VatRateGrossAmount = "vatRateGrossAmount";
		const string Tag_InvoiceSummary_SummaryNormal_SummaryByVatRate_VatRateGrossData_VatRateGrossAmountHUF = "vatRateGrossAmountHUF";
		const string Tag_InvoiceSummary_SummaryNormal_SummaryGrossData = "summaryGrossData";
		const string Tag_InvoiceSummary_SummaryNormal_SummaryGrossData_InvoiceGrossAmount = "invoiceGrossAmount";
		const string Tag_InvoiceSummary_SummaryNormal_SummaryGrossData_InvoiceGrossAmountHUF = "invoiceGrossAmountHUF";
		const string Tag_InvoiceSummary_SummaryNormal_InvoiceNetAmount = "invoiceNetAmount";
		const string Tag_InvoiceSummary_SummaryNormal_InvoiceNetAmountHUF = "invoiceNetAmountHUF";
		const string Tag_InvoiceSummary_SummaryNormal_InvoiceVatAmount = "invoiceVatAmount";
		const string Tag_InvoiceSummary_SummaryNormal_InvoiceVatAmountHUF = "invoiceVatAmountHUF";

		#endregion

		#region Shared Tags

		const string Tag_TaxpayerId = "taxpayerId";

		const string Tag_CountryCode = "countryCode";
		const string Tag_PostalCode = "postalCode";
		const string Tag_City = "city";
		const string Tag_AdditionalAddressDetail = "additionalAddressDetail";

		const string Tag_VatPercentage = "vatPercentage";
		const string Tag_VatExemption = "vatExemption";
		const string Tag_VatOutOfScope = "vatOutOfScope";
		const string Tag_VatDomesticReverseCharge = "vatDomesticReverseCharge";

		#endregion

		#endregion
		#endregion
	}
}
