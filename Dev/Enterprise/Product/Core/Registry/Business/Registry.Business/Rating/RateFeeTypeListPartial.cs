namespace Enterprise.Registry.Business
{
	partial class RateFeeTypeList
	{
		public static string GetHeaderType(string code, bool plural)
		{
			string result = null;

			switch (code)
			{
				case Codes.PerEntry:
					result = plural ? Res.GetString("bda85466-79b5-4fb3-b88a-1d4331a769e0", "Entries") : Res.GetString("ee43093a-c31f-483c-a6b4-55fd542b227f", "Entry");
					break;

				case Codes.PerEntryPage:
					result = plural ? Res.GetString("14f11d4f-fc1f-4596-88a9-f18be164ef21", "Entry Pages Per Entry") : Res.GetString("f36ffeb9-5a57-4e47-9233-312709303817", "Entry Page Per Entry");
					break;

				case Codes.PerInvoice:
					result = plural ? Res.GetString("41c79042-060a-42b9-b3c9-1641b3ea91d6", "Invoices") : Res.GetString("3654b327-e62f-4fcb-9afe-9dd7d4672784", "Invoice");
					break;

				case Codes.PerSupplier:
					result = plural ? Res.GetString("90e3b55e-acf7-4f91-9c22-ae37cf49ce02", "Suppliers") : Res.GetString("7439cd32-fafb-4844-b01d-31bf4dadeba9", "Supplier");
					break;

				case Codes.PerShipment:
					result = plural ? Res.GetString("dd6e3a7f-a11c-4a7f-b5e0-1a7216e75b0d", "Shipments") : Res.GetString("6239a31e-0d0c-465a-86da-f3a4acf035a7", "Shipment");
					break;

				case Codes.PerSubHeader:
					result = plural ? Res.GetString("47784190-A4BD-4195-BE19-EF28F7C5A9E2", "Sub Header") : Res.GetString("E91283A2-F592-4403-9DB2-34F4B12F6884", "Sub Header");
					break;
			}

			return result;
		}
	}
}
