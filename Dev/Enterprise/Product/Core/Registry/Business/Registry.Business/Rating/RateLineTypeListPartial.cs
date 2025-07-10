namespace Enterprise.Registry.Business
{
	partial class RateLineTypeList
	{
		public static string GetTypeOfLine(string lineType)
		{
			return GetTypeOfLine(lineType, false);
		}

		public static string GetTypeOfLine(string lineType, bool plural)
		{
			switch (lineType)
			{
				case RateLineTypeList.Codes.PerInvoiceLinePerEntry:
				case RateLineTypeList.Codes.PerInvoiceLinePerInvoice:
				case RateLineTypeList.Codes.PerInvoiceLinePerShipment:
					return plural ? Res.GetString("85e6f303-4587-48e6-92c8-f428ec1f84ab", "Invoice Lines") : Res.GetString("295ff871-dc69-4e6c-9d44-7446bac503b6", "Invoice Line");

				case RateLineTypeList.Codes.PerTariffLinePerEntry:
				case RateLineTypeList.Codes.PerTariffLinePerInvoice:
				case RateLineTypeList.Codes.PerTariffLinePerShipment:
				case RateLineTypeList.Codes.PerHTSCodePerShipment:
				case RateLineTypeList.Codes.PerHTSCodePerInvoice:
				case RateLineTypeList.Codes.TotalHTSCountPerDeclaration:
					return plural ? Res.GetString("a37ed194-72c0-49ff-a91b-13ec4cc11cc5", "Tariff Lines") : Res.GetString("f14fe651-9fcb-4068-a99b-26add9091ec8", "Tariff Line");
			}

			return null;
		}

		public static string GetDescriptiveHeaderText(string lineType)
		{
			switch (lineType)
			{
				case RateLineTypeList.Codes.PerInvoiceLinePerEntry:
				case RateLineTypeList.Codes.PerTariffLinePerEntry:
					return Res.GetString("38847745-be76-4e6a-a6e7-528756f9272g", "for each entry");

				case RateLineTypeList.Codes.PerInvoiceLinePerInvoice:
				case RateLineTypeList.Codes.PerTariffLinePerInvoice:
				case RateLineTypeList.Codes.PerHTSCodePerInvoice:
					return Res.GetString("2251f85e-d023-44b2-a378-55b900ffcbbe", "for each invoice");

				case RateLineTypeList.Codes.PerInvoiceLinePerShipment:
				case RateLineTypeList.Codes.PerTariffLinePerShipment:
				case RateLineTypeList.Codes.PerHTSCodePerShipment:
					return Res.GetString("2b8afa51-c864-4a73-a860-aeb7a5b90bf0", "for shipment");

				case RateLineTypeList.Codes.TotalHTSCountPerDeclaration:
					return Res.GetString("0ea5bf21-6874-4ba6-8254-13588aa86fb9", "for declaration");
			}

			return null;
		}
	}
}
