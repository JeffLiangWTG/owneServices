
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.AUS.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.AUS
{
	public class DataConverter : FlatFileConverter
	{
		public DataConverter(INotifications notification, BusinessObjectFactory factory, ZGuid importer, ZGuid supplier) : base(notification, factory)
		{
			ImporterPK = importer;
			SupplierPK = supplier;
		}

		protected override void MapImport(IValueObject valueObject, FlatFileDataRowCollection fileLines)
		{
			Xsd.InvoiceHeaderCollection invoices = valueObject as Xsd.InvoiceHeaderCollection;

			string currentInvoice = "";
			Xsd.InvoiceHeader invoiceHeader = null;

			foreach (FlatFileDataRow fileLine in fileLines)
			{
				if (fileLine[AUSConstants.PartNumber] != "" && fileLine[AUSConstants.PartDescription] != "")
				{
					if (fileLine[AUSConstants.InvoiceNumber] != currentInvoice)
					{
						invoiceHeader = new Xsd.InvoiceHeader();
						invoices.Add(invoiceHeader);
						currentInvoice = invoiceHeader.InvoiceNumber = fileLine[AUSConstants.InvoiceNumber];
					}

					Xsd.InvoiceLine invoiceLine = new Xsd.InvoiceLine();

					invoiceLine.ProductNumber = fileLine[AUSConstants.PartNumber];
					invoiceLine.ProductDescription = fileLine[AUSConstants.PartDescription];

					const string DefaultCountryCode = "";
					ZDecimal calcUnitPrice = (fileLine.GetFieldAsZDecimal(AUSConstants.LinePrice) / 100);
					ZDecimal shippedQuantity = fileLine.GetFieldAsZDecimal(AUSConstants.Quantity);
					ZDecimal calcLinePrice = calcUnitPrice * shippedQuantity;
					invoiceLine.InvoiceQty = Xsd.DimensionValue.FromAmountAndUnit(shippedQuantity, DefaultCountryCode);
					invoiceLine.LinePrice = Xsd.FinancialValue.FromAmountAndCurrencyCode(calcLinePrice, DefaultCountryCode);

					invoiceLine.LineClassification.TariffLookup = fileLine[AUSConstants.Classification].Right(5);
					LineOrigin = fileLine[AUSConstants.Origin];
					if (LineOrigin == "LU")
					{
						LineOrigin = "BE";
					}

					invoiceLine.LineClassification.OriginOfGoods = LineOrigin;

					if (!string.IsNullOrEmpty(LineOrigin))
					{
						ClientAUSOriginPreferenceMapping thisMappingData = GetOriginPrefMapping();
						if (thisMappingData != null)
						{
							if (thisMappingData.T7_RN_NKPreferenceOrigin != "")
							{
								Xsd.AdditionalCustomsInformation addInfo = new Xsd.AdditionalCustomsInformation();
								addInfo.CustomsDetailType = "POC";
								addInfo.CustomsDetailValue = thisMappingData.T7_RN_NKPreferenceOrigin;
								invoiceLine.LineClassification.AddCustomsDetails.Add(addInfo);
							}

							if (thisMappingData.T7_PreferenceRuleType != "")
							{
								Xsd.AdditionalCustomsInformation addInfo = new Xsd.AdditionalCustomsInformation();
								addInfo.CustomsDetailType = "PRT";
								addInfo.CustomsDetailValue = thisMappingData.T7_PreferenceRuleType;
								invoiceLine.LineClassification.AddCustomsDetails.Add(addInfo);
							}

							if (thisMappingData.T7_PreferenceSchemeType != "")
							{
								Xsd.AdditionalCustomsInformation addInfo = new Xsd.AdditionalCustomsInformation();
								addInfo.CustomsDetailType = "PST";
								addInfo.CustomsDetailValue = thisMappingData.T7_PreferenceSchemeType;
								invoiceLine.LineClassification.AddCustomsDetails.Add(addInfo);
							}
						}
					}

					invoiceHeader.InvoiceLines.Add(invoiceLine);
					invoiceHeader.InvoiceAmount.Value += invoiceLine.LinePrice.Value;
				}
			}
		}

		#region Origin-Preference Mapping

		ClientAUSOriginPreferenceMapping GetOriginPrefMapping()
		{
			ZQuery mappingFilter = new ZQuery(ClientAUSOriginPreferenceMappingSchema.T7_OH_Importer, ImporterPK);
			mappingFilter.AddToFilter(ClientAUSOriginPreferenceMappingSchema.T7_OH_Supplier, SupplierPK);
			mappingFilter.AddToFilter(ClientAUSOriginPreferenceMappingSchema.T7_RN_NKOrigin, LineOrigin);
			ClientAUSOriginPreferenceMapping mappingResult = (ClientAUSOriginPreferenceMapping)Factory.LoadTop1(typeof(ClientAUSOriginPreferenceMapping), mappingFilter);

			return mappingResult;
		}

		#endregion

		string LineOrigin;
		readonly ZGuid ImporterPK;
		readonly ZGuid SupplierPK;
	}
}
