using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.MX.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.MX.Manifest.Business
{
	internal class AN1408Wrappers : IAN1408
	{
		readonly AsycudaBill bill;
		readonly string partyType;

		public AN1408Wrappers(AsycudaBill bill, string partyType)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
			this.partyType = partyType;
		}

		IN1Name IAN1408.N1 => n1 ?? (n1 = new N1Wrapper(bill, partyType));
		protected IN1Name n1;

		IReadOnlyCollection<IN3AddressInformation> IAN1408.N3 => new IN3AddressInformation[1] { new N3Wrappers(bill, partyType) };

		IN4GeographicLocation408 IAN1408.N4 => n4 ?? (n4 = new N4Wrapper(bill, partyType));
		protected IN4GeographicLocation408 n4;

		IPERAdministrativeComunicationsContact IAN1408.PER => per ?? (per = new PERWrapper(bill, partyType));
		protected IPERAdministrativeComunicationsContact per;
	}

	internal class PERWrapper : IPERAdministrativeComunicationsContact
	{
		readonly AsycudaBill bill;
		readonly string partyType;

		public PERWrapper(AsycudaBill bill, string partyType)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
			this.partyType = partyType;
		}

		string IPERAdministrativeComunicationsContact.ContactFunction => Parties.ContactFunction;

		string IPERAdministrativeComunicationsContact.Name
		{
			get
			{
				switch (partyType)
				{
					case Parties.ShipperCode:
						return bill.ABL_ShipperName;
					case Parties.ConsigneeCode:
						return bill.ABL_ConsigneeName;
					default:
						return bill.ABL_NotifyPartyName;
				}
			}
		}

		string IPERAdministrativeComunicationsContact.Communication => Parties.Communication;

		string IPERAdministrativeComunicationsContact.CommunicationData
		{
			get
			{
				switch (partyType)
				{
					case Parties.ShipperCode:
						return bill.ABL_ShipperPhone;
					case Parties.ConsigneeCode:
						return bill.ABL_ConsigneePhone;
					default:
						return bill.ABL_NotifyPartyPhone;
				}
			}
		}

		string IPERAdministrativeComunicationsContact.Communication2 => ZString.Empty;

		string IPERAdministrativeComunicationsContact.CommunicationData2 => ZString.Empty;

		string IPERAdministrativeComunicationsContact.Communication3 => ZString.Empty;

		string IPERAdministrativeComunicationsContact.Communication4 => ZString.Empty;

		string IPERAdministrativeComunicationsContact.Communication5 => ZString.Empty;
	}

	internal class N4Wrapper : IN4GeographicLocation408
	{
		readonly AsycudaBill bill;
		readonly string partyType;

		public N4Wrapper(AsycudaBill bill, string partyType)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
			this.partyType = partyType;
		}

		string IN4GeographicLocation408.CityName
		{
			get
			{
				switch (partyType)
				{
					case Parties.ShipperCode:
						return bill.ABL_ShipperCity;
					case Parties.ConsigneeCode:
						return bill.ABL_ConsigneeCity;
					default:
						return bill.ABL_NotifyPartyCity;
				}
			}
		}

		string IN4GeographicLocation408.StateCode
		{
			get
			{
				switch (partyType)
				{
					case Parties.ShipperCode:
						return bill.ShipperCountry != null && SEA309Helper.MustStateCodeBeSent(bill.ShipperCountry.Code) ? bill.ABL_ShipperState : ZString.Empty;
					case Parties.ConsigneeCode:
						return bill.ConsigneeCountry != null && SEA309Helper.MustStateCodeBeSent(bill.ConsigneeCountry.Code) ? bill.ABL_ConsigneeState : ZString.Empty;
					default:
						return bill.NotifyPartyCountry != null && SEA309Helper.MustStateCodeBeSent(bill.NotifyPartyCountry.Code) ? bill.ABL_NotifyPartyState : ZString.Empty;
				}
			}
		}

		string IN4GeographicLocation408.PostalCode
		{
			get
			{
				switch (partyType)
				{
					case Parties.ShipperCode:
						return bill.ABL_ShipperPostcode;
					case Parties.ConsigneeCode:
						return bill.ABL_ConsigneePostcode;
					default:
						return bill.ABL_NotifyPartyPostcode;
				}
			}
		}

		string IN4GeographicLocation408.CountryCode
		{
			get
			{
				switch (partyType)
				{
					case Parties.ShipperCode:
						return bill.ABL_RN_NKShipperCountry;
					case Parties.ConsigneeCode:
						return bill.ABL_RN_NKConsigneeCountry;
					default:
						return bill.ABL_RN_NKNotifyPartyCountry;
				}
			}
		}

		string IN4GeographicLocation408.LocationQualifier => ZString.Empty;

		string IN4GeographicLocation408.LocationIdentifier => ZString.Empty;

		string IN4GeographicLocation408.CountrySubdivision => ZString.Empty;

		string IN4GeographicLocation408.PostalCodeFormatted => ZString.Empty;
	}

	internal class N3Wrappers : IN3AddressInformation
	{
		readonly AsycudaBill bill;
		readonly string partyType;

		public N3Wrappers(AsycudaBill bill, string partyType)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
			this.partyType = partyType;
		}

		string IN3AddressInformation.AddressInformation
		{
			get
			{
				switch (partyType)
				{
					case Parties.ShipperCode:
						return bill.ABL_ShipperStreet1;
					case Parties.ConsigneeCode:
						return bill.ABL_ConsigneeStreet1;
					default:
						return bill.ABL_NotifyPartyStreet1;
				}
			}
		}

		string IN3AddressInformation.AddressInformation2
		{
			get
			{
				switch (partyType)
				{
					case Parties.ShipperCode:
						return bill.ABL_ShipperStreet2;
					case Parties.ConsigneeCode:
						return bill.ABL_ConsigneeStreet2;
					default:
						return bill.ABL_NotifyPartyStreet2;
				}
			}
		}
	}

	internal class N1Wrapper : IN1Name
	{
		readonly AsycudaBill bill;
		readonly string partyType;

		public N1Wrapper(AsycudaBill bill, string partyType)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
			this.partyType = partyType;
		}

		string IN1Name.EntityIdentifierCode
		{
			get
			{
				switch (partyType)
				{
					case Parties.ShipperCode:
						return Parties.ShipperCode;
					case Parties.ConsigneeCode:
						return Parties.ConsigneeCode;
					default:
						return Parties.NotifyCode;
				}
			}
		}

		string IN1Name.Name
		{
			get
			{
				switch (partyType)
				{
					case Parties.ShipperCode:
						return bill.ABL_ShipperName;
					case Parties.ConsigneeCode:
						return bill.ABL_ConsigneeName;
					default:
						return bill.ABL_NotifyPartyName;
				}
			}
		}

		string IN1Name.IdentificationCodeQualifier => partyType == Parties.ConsigneeCode ? Parties.ConsigneeCode : ZString.Empty;

		string IN1Name.IdentificationCode
		{
			get
			{
				switch (partyType)
				{
					case Parties.ShipperCode:
						return bill.ABL_ShipperRegNo;
					case Parties.ConsigneeCode:
						return bill.ABL_ConsigneeRegNo;
					default:
						return bill.ABL_NotifyPartyRegNo;
				}
			}
		}

		string IN1Name.EntityRelationshipCode => ZString.Empty;

		string IN1Name.EntityIdentifierCode2 => ZString.Empty;
	}
}
