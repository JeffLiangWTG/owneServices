using System.ComponentModel;

namespace Enterprise.DataTransfer.Xml
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class CommercialInvoiceXsd
	{
		public static class XPath
		{
			public static class Invoices
			{
				public const string InvoiceHeader = "InvoiceHeader";
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
			public static class InvoiceHeader
			{
				public const string InvoiceNumber = "InvoiceNumber";
				public const string InvoiceAmount = "InvoiceAmount";
				public const string InvoiceDate = "InvoiceDate";
				public const string ValuationDate = "ValuationDate";
				public const string Consignor = "Consignor";
				public const string IsGroupInvoice = "IsGroupInvoice";
				public const string RelatedGroupInvoiceNumber = "RelatedGroupInvoiceNumber";
				public const string IncoTerm = "Incoterm";
				public const string Volume = "Volume";
				public const string Weight = "Weight";

				public const string AddCustomsDetail = "AddCustomsDetails/AddCustomsDetail";
				public const string InvoiceCharge = "InvoiceCharges/InvoiceCharge";
				public const string InvoiceLine = "InvoiceLines/InvoiceLine";
			}

			public static class InvoiceCharge
			{
				public const string NodeName = "InvoiceCharge";
				public const string ChargeType = "ChargeType";
				public const string ChargeValue = "ChargeValue";
				public const string GSTApplies = "GstApplies";
				public const string DutyApplies = "DutyApplies";
				public const string IsIncludedInTotal = "IsIncludedInTotal";
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
			public static class InvoiceLine
			{
				public const string NodeName = "InvoiceLine";
				public const string InvoiceQty = "InvoiceQty";
				public const string LinePrice = "LinePrice";
				public const string ProductNumber = "ProductNumber";
				public const string ProductDescription = "ProductDescription";
				public const string CustomsInvoiceQty = "CustomsInvoiceQty";
				public const string OrderNumber = "OrderNumber";
				public const string TariffCode = "LineClassification/TariffCode";
				public const string TariffLookup = "LineClassification/TariffLookup";
				public const string OriginOfGoods = "LineClassification/OriginOfGoods";
				public const string TreatmentCode = "LineClassification/TreatmentCode";
				public const string Preference = "LineClassification/Preference";
				public const string Concession = "LineClassification/Concession";
				//				AdditionalCustomsDetails				
				public const string Volume = "Volume";
				public const string Weight = "Weight";
				public const string CustomText1 = "CustomText1";
				public const string CustomText2 = "CustomText2";
				public const string CustomText3 = "CustomText3";
			}
		}
	}
}
