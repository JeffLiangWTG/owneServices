using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers
{
	public class G5PartyInfoWrapper : PartyNameWrapper, IG5PartyInfo
	{
		public new static G5PartyInfoWrapper New(OrgAddress address) => address?.Header == null ? null : new G5PartyInfoWrapper(address);

		protected G5PartyInfoWrapper(OrgAddress orgA) : base(orgA.Header)
		{
			orgAddress = orgA;
			isConsignorOrConsignee = false;
			type = (string)orgHeader.OH_Category switch
			{
				OrgConstants.Category.Business => TypeCodeForCompanyBUS,
				OrgConstants.Category.NaturalPersonIndividual => TypeCodeForPersonalNAT,
				OrgConstants.Category.Government or OrgConstants.Category.NonGovernmentOrganisation => TypeCodeForAssociationGOVorNGO,
				_ => ZString.Empty
			};
			(street, number) = DivideStreetAndNumber(orgAddress.OA_Address1);
			(streetAddLine, number) = SetStreetAddLineAndNumberIfNeeded(orgAddress.OA_Address2, number);
			communicationId = GetCommunicationIdFromAddress(orgAddress);
			communicationType = GetCommunicationType(communicationId);
		}

		public G5PartyInfoWrapper(OrgAddress orgA, ZString id, ZString name, ZString type, ZString street, ZString streetAddLine, ZString state, ZString country, ZString postCode, ZString city, ZString communicationId) : base(orgA?.Header)
		{
			isConsignorOrConsignee = true;
			this.id = id;
			this.name = name;
			this.type = type;
			(this.street, number) = DivideStreetAndNumber(street);
			(this.streetAddLine, number) = SetStreetAddLineAndNumberIfNeeded(streetAddLine, number);
			this.state = state;
			this.country = country;
			this.postCode = postCode;
			this.city = city;
			var commId = GetCommunicationIdFromAddress(orgA, communicationId);
			communicationType = GetCommunicationType(commId);
			this.communicationId = commId;
		}

		readonly OrgAddress orgAddress;
		readonly bool isConsignorOrConsignee;
		readonly ZString id;
		readonly ZString name;
		readonly ZString type;
		readonly ZString street;
		readonly ZString streetAddLine;
		readonly ZString number;
		readonly ZString state;
		readonly ZString country;
		readonly ZString postCode;
		readonly ZString city;
		readonly ZString communicationType;
		readonly ZString communicationId;

		readonly static ZString TypeCodeForPersonalNAT = "1";
		readonly static ZString TypeCodeForCompanyBUS = "2";
		readonly static ZString TypeCodeForAssociationGOVorNGO = "3";
		const string mailChar = "@";
		const string communicationTypeMail = "EM";
		const string communicationTypePhone = "TE";

		protected override ZString IdCore => isConsignorOrConsignee && orgHeader == null ? id : base.IdCore;

		protected override ZString NameCore => isConsignorOrConsignee ? name : base.NameCore;

		public ZString Type => type;

		public ZString Street => street;

		public ZString StreetAddLine => streetAddLine;

		public ZString Number => number;

		public ZString POBox => ZString.Empty;

		public ZString State => isConsignorOrConsignee ? state : orgAddress.State;

		public ZString Country => isConsignorOrConsignee ? country : orgAddress.OA_RN_NKCountryCode;

		public ZString PostCode => isConsignorOrConsignee ? postCode : orgAddress.OA_PostCode;

		public ZString City => isConsignorOrConsignee ? city : orgAddress.OA_City;

		public ZString CommunicationType => communicationType;

		public ZString CommunicationId => communicationId;

		(ZString street, ZString number) DivideStreetAndNumber(ZString streetAndNumber)
		{
			var number = Regex.Match(streetAndNumber, @"\d+$", RegexOptions.RightToLeft).Value;
			var street = number.IsNullOrEmpty() ? streetAndNumber : streetAndNumber.Replace(number, "");

			return (street, number);
		}

		(ZString street, ZString number) SetStreetAddLineAndNumberIfNeeded(ZString streetAddLine, ZString numberFromStreet)
		{
			var street = streetAddLine;
			var number = numberFromStreet;
			if (numberFromStreet.IsEmpty)
			{
				(street, number) = DivideStreetAndNumber(streetAddLine);
			}

			return (street, number);
		}

		ZString GetCommunicationIdFromAddress(OrgAddress orgA, string commId = "") => orgA != null ? orgA.OA_Email.IsEmpty ? orgA.OA_Phone : orgA.OA_Email : commId;

		ZString GetCommunicationType(ZString communicationId) => communicationId.IsEmpty ? ZString.Empty : communicationId.Contains(mailChar) ? communicationTypeMail : communicationTypePhone;
	}
}
