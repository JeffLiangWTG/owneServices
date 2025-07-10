using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Warehouse
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ApplicationIdentifierCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new ApplicationIdentifier AddNew()
		{
			return (ApplicationIdentifier)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ApplicationIdentifierCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ApplicationIdentifier();
		}

		protected override bool AllowNewCore
		{
			get { return true; }
		}

		public new ApplicationIdentifier this[int i]
		{
			get { return (ApplicationIdentifier)Elements[i]; }
		}

		public ApplicationIdentifier this[string key]
		{
			get
			{
				foreach (ApplicationIdentifier applicationID in this)
				{
					if (applicationID.ApplicationID == key)
					{
						return applicationID;
					}
				}
				return null;
			}
		}

		public static ApplicationIdentifierCollection GetDefault()
		{
			#region SuppressResourceStringsCheckRegion
			// These values are not multilingual in the registry and so should not be localized

			ApplicationIdentifierCollection result = new ApplicationIdentifierCollection();

			SetupDefaultFields(result.AddNew(), "00", "Serial Shipping Container Code", "SSCC", DataTypeCodeList.Codes.Digits, 18, 18);
			SetupDefaultFields(result.AddNew(), "01", "Global Trade Item Number", "GTIN", DataTypeCodeList.Codes.Digits, 14, 14);
			SetupDefaultFields(result.AddNew(), "02", "GTIN of Trade Items Contained in a Logistic Unit", "CONTENT", DataTypeCodeList.Codes.Digits, 14, 14);
			SetupDefaultFields(result.AddNew(), "10", "Batch or Lot Number", "BATCH/LOT", DataTypeCodeList.Codes.Alphanumeric, 0, 20);
			SetupDefaultFields(result.AddNew(), "11", "Production Date (YYMMDD)", "PROD DATE", DataTypeCodeList.Codes.Digits, 6, 6);
			SetupDefaultFields(result.AddNew(), "12", "Date Due (YYMMDD)", "DUE DATE", DataTypeCodeList.Codes.Digits, 6, 6);
			SetupDefaultFields(result.AddNew(), "13", "Packaging Date (YYMMDD)", "PACK DATE", DataTypeCodeList.Codes.Digits, 6, 6);
			SetupDefaultFields(result.AddNew(), "15", "Best Before Date (YYMMDD)", "BEST BEFORE or SELL BY", DataTypeCodeList.Codes.Digits, 6, 6);
			SetupDefaultFields(result.AddNew(), "17", "Expiration Date (YYMMDD)", "USE BY or EXPIRY", DataTypeCodeList.Codes.Digits, 6, 6);
			SetupDefaultFields(result.AddNew(), "20", "Product Variant", "VARIANT", DataTypeCodeList.Codes.Digits, 2, 2);
			SetupDefaultFields(result.AddNew(), "21", "Serial Number", "SERIAL NUMBER", DataTypeCodeList.Codes.Alphanumeric, 0, 20);
			SetupDefaultFields(result.AddNew(), "22", "Secondary Data for Specific Health Industry Products", "QTY/DATE/BATCH", DataTypeCodeList.Codes.Alphanumeric, 0, 29);
			SetupDefaultFields(result.AddNew(), "240", "Additional Product Identification Assigned by the Manufacturer", "ADDITIONAL ID", DataTypeCodeList.Codes.Alphanumeric, 0, 30);
			SetupDefaultFields(result.AddNew(), "241", "Customer Part Number", "CUST. PART NO.", DataTypeCodeList.Codes.Alphanumeric, 0, 30);
			SetupDefaultFields(result.AddNew(), "242", "Made-to-Order Variation", "VARIATION NUMBER", DataTypeCodeList.Codes.Digits, 0, 6);
			SetupDefaultFields(result.AddNew(), "250", "Secondary Serial Number", "SECONDARY SERIAL", DataTypeCodeList.Codes.Alphanumeric, 0, 30);
			SetupDefaultFields(result.AddNew(), "251", "Reference to Source Entity", "REF. TO SOURCE", DataTypeCodeList.Codes.Alphanumeric, 0, 30);
			SetupDefaultFields(result.AddNew(), "253", "Global Document Type Identifier", "DOC. ID", DataTypeCodeList.Codes.Digits, 13, 30);
			SetupDefaultFields(result.AddNew(), "30", "Variable Count", "VAR. COUNT", DataTypeCodeList.Codes.Digits, 0, 8);
			SetupDefaultFields(result.AddNew(), "310n", "Net Weight - Kilograms - Trade", "NET WEIGHT (kg)", DataTypeCodeList.Codes.Digits, 6, 6);
			SetupDefaultFields(result.AddNew(), "311n", "Length or First Dimension - Meters - Trade", "LENGTH (m)", DataTypeCodeList.Codes.Digits, 6, 6);
			SetupDefaultFields(result.AddNew(), "312n", "Width, Diameter or Second Dimension - Meters - Trade", "WIDTH (m)", DataTypeCodeList.Codes.Digits, 6, 6);
			SetupDefaultFields(result.AddNew(), "313n", "Depth, Thickness, Height or Third Dimension - Meters - Trade", "HEIGHT (m)", DataTypeCodeList.Codes.Digits, 6, 6);
			SetupDefaultFields(result.AddNew(), "314n", "Area - Square Meters - Trade", "AREA (M2)", DataTypeCodeList.Codes.Digits, 6, 6);
			SetupDefaultFields(result.AddNew(), "315n", "Net Volume - Liters - Trade", "NET VOLUME (l)", DataTypeCodeList.Codes.Digits, 6, 6);
			SetupDefaultFields(result.AddNew(), "316n", "Net Volume - Cubic Meters - Trade", "NET VOLUME (M3)", DataTypeCodeList.Codes.Digits, 6, 6);
			SetupDefaultFields(result.AddNew(), "330n", "Gross Weight - Kilograms - Logistic", "GROSS WEIGHT (kg)", DataTypeCodeList.Codes.Digits, 6, 6);
			SetupDefaultFields(result.AddNew(), "331n", "Length of First Dimension - Meters - Logistic", "LENGTH (m), logistic", DataTypeCodeList.Codes.Digits, 6, 6);
			SetupDefaultFields(result.AddNew(), "332n", "Width, Diameter or Second Dimension - Meters - Logistic", "WIDTH (m), logistic", DataTypeCodeList.Codes.Digits, 6, 6);
			SetupDefaultFields(result.AddNew(), "333n", "Depth, Thickness, Height or Third Dimension - Meters - Logistic", "HEIGHT (m), logistic", DataTypeCodeList.Codes.Digits, 6, 6);
			SetupDefaultFields(result.AddNew(), "334n", "Area - Square Meters - Logistic", "AREA (M2), logistic", DataTypeCodeList.Codes.Digits, 6, 6);
			SetupDefaultFields(result.AddNew(), "335n", "Gross Volume - Liters - Logistic", "VOLUME (l), logistic", DataTypeCodeList.Codes.Digits, 6, 6);
			SetupDefaultFields(result.AddNew(), "336n", "Gross Volume - Cubic Meters - Logistic", "VOLUME (M3), logistic", DataTypeCodeList.Codes.Digits, 6, 6);
			SetupDefaultFields(result.AddNew(), "337n", "Kilograms Per Square Meter", "KG PER M2", DataTypeCodeList.Codes.Digits, 6, 6);
			SetupDefaultFields(result.AddNew(), "37", "Count of Trade Items Contained in a Logistic Unit", "COUNT", DataTypeCodeList.Codes.Digits, 0, 8);
			SetupDefaultFields(result.AddNew(), "390n", "Amount Payable - Single Monetary Area", "AMOUNT", DataTypeCodeList.Codes.Digits, 0, 15);
			SetupDefaultFields(result.AddNew(), "391n", "Amount Payable - With ISO Currency", "AMOUNT", DataTypeCodeList.Codes.Digits, 3, 18);
			SetupDefaultFields(result.AddNew(), "392n", "Amount Payable for a Variable Measure Trade Item - Single Monetary Unit", "PRICE", DataTypeCodeList.Codes.Digits, 0, 15);
			SetupDefaultFields(result.AddNew(), "393n", "Amount Payable for a Variable Measure Trade Item - With ISO Currency Code", "PRICE", DataTypeCodeList.Codes.Digits, 3, 18);
			SetupDefaultFields(result.AddNew(), "400", "Customer's Purchase Order Number", "ORDER NO.", DataTypeCodeList.Codes.Alphanumeric, 0, 30);
			SetupDefaultFields(result.AddNew(), "401", "Consignment Number", "CONSIGNMENT", DataTypeCodeList.Codes.Alphanumeric, 0, 30);
			SetupDefaultFields(result.AddNew(), "402", "Shipment Identification Number", "SHIPMENT NO.", DataTypeCodeList.Codes.Digits, 17, 17);
			SetupDefaultFields(result.AddNew(), "403", "Routing Code", "ROUTE", DataTypeCodeList.Codes.Alphanumeric, 0, 30);
			SetupDefaultFields(result.AddNew(), "410", "Ship To - Deliver to GS1 Global Location Number", "SHIP TO LOC", DataTypeCodeList.Codes.Digits, 13, 13);
			SetupDefaultFields(result.AddNew(), "411", "Bill To - Invoice to GS1 Global Location Number", "BILL TO", DataTypeCodeList.Codes.Digits, 13, 13);
			SetupDefaultFields(result.AddNew(), "412", "Purchased From GS1 Global Location Number", "PURCHASE FROM", DataTypeCodeList.Codes.Digits, 13, 13);
			SetupDefaultFields(result.AddNew(), "413", "Ship For - Deliver For - Forward To GS1 Global Location Number", "SHIP FOR LOC.", DataTypeCodeList.Codes.Digits, 13, 13);
			SetupDefaultFields(result.AddNew(), "414", "Identification of a Physical Location GS1 Global Location Number", "LOC NO.", DataTypeCodeList.Codes.Digits, 13, 13);
			SetupDefaultFields(result.AddNew(), "415", "GS1 Global Location Number of the Invoicing Party", "PAY TO", DataTypeCodeList.Codes.Digits, 13, 13);
			SetupDefaultFields(result.AddNew(), "420", "Ship To - Deliver To Postal Code Within a Single Postal Authority", "SHIP TO POST", DataTypeCodeList.Codes.Alphanumeric, 0, 20);
			SetupDefaultFields(result.AddNew(), "421", "Ship To - Deliver To Postal Code With Three-Digit ISO Country Code", "SHIP TO POST", DataTypeCodeList.Codes.Alphanumeric, 3, 12);
			SetupDefaultFields(result.AddNew(), "422", "Country of Origin of a Trade Item", "ORIGIN", DataTypeCodeList.Codes.Digits, 3, 3);
			SetupDefaultFields(result.AddNew(), "423", "Country of Initial Processing", "COUNTRY - INITIAL PROCESS", DataTypeCodeList.Codes.Digits, 3, 15);
			SetupDefaultFields(result.AddNew(), "424", "Country of Processing", "COUNTRY - PROCESS", DataTypeCodeList.Codes.Digits, 3, 3);
			SetupDefaultFields(result.AddNew(), "425", "Country of Disassembly", "COUNTRY - DISASSEMBLY", DataTypeCodeList.Codes.Digits, 3, 3);
			SetupDefaultFields(result.AddNew(), "426", "Country Covering Full Process Chain", "COUNTRY - FULL PROCESS", DataTypeCodeList.Codes.Digits, 3, 3);
			SetupDefaultFields(result.AddNew(), "7001", "NATO Stock Number", "NSN", DataTypeCodeList.Codes.Digits, 13, 13);
			SetupDefaultFields(result.AddNew(), "7002", "UN/ECE Meat Carcasses and Cuts Classification", "MEAT CUT", DataTypeCodeList.Codes.Alphanumeric, 0, 30);
			SetupDefaultFields(result.AddNew(), "7003", "Expiration Date and Time", "EXPIRY DATE/TIME", DataTypeCodeList.Codes.Digits, 10, 10);
			SetupDefaultFields(result.AddNew(), "703s", "Approval Number of Processor with ISO Country Code", "PROCESSOR # S4", DataTypeCodeList.Codes.Alphanumeric, 30, 30);
			SetupDefaultFields(result.AddNew(), "8001", "Roll Products - Width, Length, Core Diameter, Direction and Splices", "DIMENSIONS", DataTypeCodeList.Codes.Digits, 14, 14);
			SetupDefaultFields(result.AddNew(), "8002", "Electronic Serial Identifier for Cellular Mobile Telephones", "CMT NO.", DataTypeCodeList.Codes.Alphanumeric, 20, 20);
			SetupDefaultFields(result.AddNew(), "8003", "GS1 Global Returnable Asset Identifier", "GRAI", DataTypeCodeList.Codes.Alphanumeric, 14, 30);
			SetupDefaultFields(result.AddNew(), "8004", "GS1 Global Individual Asset Identifier", "GIAI", DataTypeCodeList.Codes.Alphanumeric, 0, 30);
			SetupDefaultFields(result.AddNew(), "8005", "Price Per Unit of Measure", "PRICE PER UNIT", DataTypeCodeList.Codes.Digits, 6, 6);
			SetupDefaultFields(result.AddNew(), "8006", "Identification of the Components of a Trade Item", "GCTIN", DataTypeCodeList.Codes.Alphanumeric, 18, 18);
			SetupDefaultFields(result.AddNew(), "8007", "International Bank Account Number", "IBAN", DataTypeCodeList.Codes.Alphanumeric, 0, 30);
			SetupDefaultFields(result.AddNew(), "8008", "Date and Time of Production", "PROD. TIME", DataTypeCodeList.Codes.Digits, 8, 12);
			SetupDefaultFields(result.AddNew(), "8018", "GS1 Global Service Relation Number", "GSRN", DataTypeCodeList.Codes.Digits, 18, 18);
			SetupDefaultFields(result.AddNew(), "8020", "Payment Slip Reference", "REF. NO", DataTypeCodeList.Codes.Alphanumeric, 0, 25);
			SetupDefaultFields(result.AddNew(), "8100", "GS1-128 Coupon Extender Code - U.P.C Prefix + Offer Code", "-", DataTypeCodeList.Codes.Digits, 6, 6);
			SetupDefaultFields(result.AddNew(), "8101", "GS1-128 Coupon Extender Code - U.P.C Prefix + Offer Code + End of Offer Code", "-", DataTypeCodeList.Codes.Digits, 10, 10);
			SetupDefaultFields(result.AddNew(), "8102", "GS1-128 Coupon Extended Code - U.P.C Prefix", "-", DataTypeCodeList.Codes.Digits, 2, 2);
			SetupDefaultFields(result.AddNew(), "90", "Information Mutually Agreed Between Trading Partners (Including FACT DIs)", "INTERNAL", DataTypeCodeList.Codes.Alphanumeric, 0, 30);
			SetupDefaultFields(result.AddNew(), "91", "Company Internal Information", "INTERNAL", DataTypeCodeList.Codes.Alphanumeric, 0, 30);
			SetupDefaultFields(result.AddNew(), "92", "Company Internal Information", "INTERNAL", DataTypeCodeList.Codes.Alphanumeric, 0, 30);
			SetupDefaultFields(result.AddNew(), "93", "Company Internal Information", "INTERNAL", DataTypeCodeList.Codes.Alphanumeric, 0, 30);
			SetupDefaultFields(result.AddNew(), "94", "Company Internal Information", "INTERNAL", DataTypeCodeList.Codes.Alphanumeric, 0, 30);
			SetupDefaultFields(result.AddNew(), "95", "Company Internal Information", "INTERNAL", DataTypeCodeList.Codes.Alphanumeric, 0, 30);
			SetupDefaultFields(result.AddNew(), "96", "Company Internal Information", "INTERNAL", DataTypeCodeList.Codes.Alphanumeric, 0, 30);
			SetupDefaultFields(result.AddNew(), "97", "Company Internal Information", "INTERNAL", DataTypeCodeList.Codes.Alphanumeric, 0, 30);
			SetupDefaultFields(result.AddNew(), "98", "Company Internal Information", "INTERNAL", DataTypeCodeList.Codes.Alphanumeric, 0, 30);
			SetupDefaultFields(result.AddNew(), "99", "Company Internal Information", "INTERNAL", DataTypeCodeList.Codes.Alphanumeric, 0, 30);
			return result;

			#endregion
		}

		static void SetupDefaultFields(ApplicationIdentifier identifier, ZString applicationID, ZString fullTitle, ZString dataTitle, ZString dataType, ZInt minimumFieldLength, ZInt maxFieldLength)
		{
			identifier.ApplicationID = applicationID;
			identifier.EnglishFullTitle = fullTitle;
			identifier.EnglishDataTitle = dataTitle;
			identifier.DataType = dataType;
			identifier.MinFieldLength = minimumFieldLength;
			identifier.MaxFieldLength = maxFieldLength;
		}
	}
}
