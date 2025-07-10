using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Generators
{
	class G2gConsignmentFromShipment : IG2gConsignment
	{
		public G2gConsignmentFromShipment(ForwardingShipment shipment, ZBool isDirect, IG2gHeader consolHeader)
		{
			this.shipment = shipment;
			this.isDirectBasic = isDirect;
			this.consolHeader = consolHeader;
		}

		ZString IG2gConsignment.HouseAWBNumber
		{
			get { return isDirectBasic ? ZString.Empty : shipment.JS_HouseBill; }
		}

		ZString IG2gConsignment.CustomsAuthorisationReference
		{
			get { return G2gUtilities.GetFirstNumber(G2gUtilities.CodeType.CAR, shipment.Numbers); }
		}

		IEnumerable<IG2gDeclaration> IG2gConsignment.Declarations
		{
			get
			{
				foreach (var bizO in shipment.Declarations)
				{
					var declaration = bizO as JobDeclaration;
					if (declaration != null)
					{
						if (declaration.CustomsEntryHeaders.Count > 0)
						{
							foreach (CusEntryHeader ceh in declaration.CustomsEntryHeaders)
							{
								yield return new G2gDeclarationFromCusEntryHeader(declaration, ceh);
							}
						}
						else
						{
							yield return new G2gDeclarationFromDeclarationNoEntry(declaration);
						}
					}
				}
				foreach (var externalUcr in G2gUtilities.GetNumbers(G2gUtilities.CodeType.UCR, shipment.Numbers))
				{
					yield return new G2gDeclarationFromShipmentExternalUcr(externalUcr, shipment.JS_UniqueConsignRef, consolHeader);
				}
			}
		}

		ZString IG2gConsignment.CargoWiseNumber
		{
			get { return (isDirectBasic ? "Direct Shipment " : "Shipment ") + shipment.JS_UniqueConsignRef; }
		}

		readonly IG2gHeader consolHeader;
		readonly ForwardingShipment shipment;
		readonly ZBool isDirectBasic;
	}
}
