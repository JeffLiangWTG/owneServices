using CargoWise.Types;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Generators
{
	class G2gDeclarationFromShipmentExternalUcr : IG2gDeclaration
	{
		// If we know of an external DUCR, include it in the list of what is good to go.... but omit all but DUCR and friends
		public G2gDeclarationFromShipmentExternalUcr(Common.CusEntryNumber externalUcr, ZString shipmentNumber, IG2gHeader consolHeader)
		{
			this.ducrAndPart = externalUcr.CE_EntryNum;
			this.ducrLevelCar = externalUcr.CE_EntryLineReference;
			this.shipmentNumber = shipmentNumber;
			this.consolHeader = consolHeader;
		}

		ZString IG2gDeclaration.DeclarationUcr
		{
			get { return (ducrAndPart + "/").Split('/')[0]; }
		}

		ZString IG2gDeclaration.DeclarationUcrPart
		{
			get { return (ducrAndPart + "/").Split('/')[1]; }
		}

		ZString IG2gDeclaration.DeclarationEpu
		{
			get { return ""; }
		}

		ZString IG2gDeclaration.DeclarationENo
		{
			get { return ""; }
		}

		ZDateTime IG2gDeclaration.DeclarationDoe
		{
			get { return ZDateTime.Empty; }
		}

		ZString IG2gDeclaration.DeclarationSoe
		{
			get { return ""; }
		}

		ZString IG2gDeclaration.CustomsAuthorisationReference
		{
			get { return ducrLevelCar; }
		}

		ZString IG2gDeclaration.AirportCode
		{
			get { return consolHeader.Airport; }
		}

		ZString IG2gDeclaration.ShedOpId
		{
			get { return consolHeader.Shed; }
		}

		ZString IG2gDeclaration.CargoWiseNumber
		{
			get { return string.Format("External declaration {0} on shipment {1}", ducrAndPart, shipmentNumber); }
		}

		ZString IG2gDeclaration.DeclarationRoute
		{
			get { return ZString.Empty; }
		}

		readonly ZString ducrAndPart;
		readonly ZString shipmentNumber;
		readonly ZString ducrLevelCar;
		readonly IG2gHeader consolHeader;
	}
}
