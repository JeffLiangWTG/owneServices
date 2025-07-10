using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHHouseJobDocAddressValidation : JobDocAddressValidation
	{
		public CusCAeMHHouseJobDocAddressValidation(AutoJobDocAddress parent) : base(parent)
		{
		}

		protected override void CheckE2_OA_Address()
		{
			base.CheckE2_OA_Address();
			if (!Parent.E2_AddressOverride && !Parent.OrganisationPK.IsEmpty && Parent.Organisation != null)
			{
				var houseBill = Parent.Parent as CusCAeMHHouse;
				if (!houseBill?.HasRowMessageErrors ?? true)
				{
					CAAddressValidator.ValidateMHHouseValidationMandatory(Parent, Parent.E2_OA_AddressInfo, GetAddressCaption(Parent.E2_AddressType));
				}
			}
		}

		string GetAddressCaption(string addressType)
		{
			var addressCaption = string.Empty;
			switch (addressType)
			{
				case DocAddressTypes.Codes.ConsigneeDocumentaryAddress:
					addressCaption = Res.GetString("73a2bb92-8181-4e6d-8dc6-2c6bd6b903db", "Consignee");
					break;
				case DocAddressTypes.Codes.ConsignorDocumentaryAddress:
					addressCaption = Res.GetString("0560874f-7871-4b55-8f16-964ce4623749", "Shipper");
					break;
				case DocAddressTypes.Codes.ConsigneePickupDeliveryAddress:
					addressCaption = Res.GetString("1eda18be-fec4-4be6-8a49-1145c81d2378", "Delivery Address");
					break;
				case DocAddressTypes.Codes.NotifyParty:
					addressCaption = Res.GetString("b1f1d14e-31e7-4a19-a6af-f833ffc83ac4", "Notify Party");
					break;
				case DocAddressTypes.Codes.ImportBroker:
					addressCaption = Res.GetString("8a2a6fc1-08a1-4513-9d7a-4b90e699f23e", "Broker");
					break;
				case DocAddressTypes.Codes.ReceivingForwarderAddress:
					addressCaption = Res.GetString("6d6b1278-f382-4b92-bacb-1f0ffa8e2097", "Forwarder");
					break;
				case DocAddressTypes.Codes.Carrier:
					addressCaption = Res.GetString("cbbbbbd6-8efa-4c9a-ab8b-33cd8d9e136a", "Carrier");
					break;
				case DocAddressTypes.Codes.Warehouse:
					addressCaption = Res.GetString("5d46996b-0a39-4fc1-a628-2ebe775c71fb", "Warehouse");
					break;
				case DocAddressTypes.Codes.Consolidator:
					addressCaption = Res.GetString("2e72ae9f-cdd8-4e40-b89c-c7379793699a", "Consolidator");
					break;
				case DocAddressTypes.Codes.PlaceOfConsolidation:
					addressCaption = Res.GetString("1740b430-79be-4201-98cd-d0b83904f9ac", "Place of Consolidation");
					break;
			}
			return addressCaption;
		}
	}
}
