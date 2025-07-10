using System;
using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSPED;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.MasterFiles.Business;
using IDocAddress = Enterprise.MasterFiles.Integration.IDocAddress;

namespace Enterprise.Customs.CA.Business
{
	public class TradeChainPartnerCuspedMessageBuilder : D99BMessageBuilder<ITradeChainPartner, CUSPEDMessage, TCPMessage>
	{
		public TradeChainPartnerCuspedMessageBuilder(ITradeChainPartner org, MessageSubTypes messageSubType)
			: base(org, messageSubType)
		{
		}

		protected override void PopulateEdifactMessage()
		{
			var unh = CreateUNH();
			CreateBGM();
			CreateCST();
			CreateDTM();
			CreateGroup7();
			CreateUNT(unh);
		}

		UNHSegment CreateUNH()
		{
			var unh = edifactMessage.UNH[0];
			D99BMessageUtilities.PopulateUNH(unh, EDIMessage.MessageNumberPlaceHolder, Enterprise.Edifact.D99B.Elements.MessageTypeList.PeriodicCustomsDeclarationMessage, "S", "99B", ControllingAgencyList.UnCefact);
			return unh;
		}

		void CreateBGM()
		{
			var bgm = edifactMessage.BGM[0];
			D99BMessageUtilities.PopoulateBGMWithDocumentNameCode(bgm, DocumentNameCodeList.RegistrationDocument, data.MessageNumber, MessageFunctionCode);
		}

		void CreateCST()
		{
			var cst = edifactMessage.CST[0];
			D99BMessageUtilities.PouplateCST(cst, "", data.IsVendor ? "570" : "588", CodeListIdentificationCodeList.CustomsDeclarationType, data.ApplicationImporterNumber, CodeListIdentificationCodeList.BusinessAccountNumber,
				data.DivisionImporterNumber, CodeListIdentificationCodeList.BusinessAccountNumber);
		}

		void CreateDTM()
		{
			var dtm = edifactMessage.DTM[0];
			D99BMessageUtilities.PopulateDTM(dtm, DateTimePeriodFunctionCodeQualifierList.ProcessingDateTime, data.TransactionDate, "yyyyMMddHHmm", DateTimePeriodFormatCodeList.Ccyymmddhhmm);
		}

		void CreateGroup7()
		{
			var group7 = edifactMessage.Group7[0];
			CreateDMS(group7);
			CreateRFF(group7);
			CreateGroup10(group7);
		}

		void CreateDMS(SegmentGroup7 group7)
		{
			var dms = group7.DMS[0];
			D99BMessageUtilities.PopulateDMSWithNameCode(dms, DocumentNameCodeList.PartyInformation);
		}

		void CreateRFF(SegmentGroup7 group7)
		{
			var rff = group7.RFF[0];
			var referenceFunctionCodeQualifierList = GetReferenceFunctionCodeQualifierList();
			D99BMessageUtilities.PopulateRFF(rff, referenceFunctionCodeQualifierList, data.ReferenceIdentifier);
		}

		ReferenceFunctionCodeQualifierList GetReferenceFunctionCodeQualifierList()
		{
			switch (data.OrgCodeType)
			{
				case OrgCusCode.CodeTypes.DataUniversalNumberingSystem:
					return data.IsVendor ? ReferenceFunctionCodeQualifierList.DunAndBradstreetUs8DigitStandardIndustrialClassificationSicCode : ReferenceFunctionCodeQualifierList.DunAndBradstreetCanadas8DigitStandardIndustrialClassificationSicCode;
				case OrgCusCode.USACodeTypes.EmployerIdentificationNumber:
				case OrgCusCode.USACodeTypes.SocialSecurityNumber:
					return ReferenceFunctionCodeQualifierList.UsGovernmentAgencyNumber;
				case OrgCusCode.CodeTypes.CarrierCode:
					return ReferenceFunctionCodeQualifierList.StandardCarrierAlphaCodeScacNumber;
				case OrgCusCode.CACodeTypes.CSAReferenceID:
					return data.IsVendor ? ReferenceFunctionCodeQualifierList.InternalVendorNumber : ReferenceFunctionCodeQualifierList.ConsigneesReference;
				case OrgCusCode.CACodeTypes.BusinessNumberForImportExport:
					return ReferenceFunctionCodeQualifierList.TraderAccountNumber;
				default:
					return ReferenceFunctionCodeQualifierList.MutuallyDefinedReferenceNumber;
			}
		}

		void CreateGroup10(SegmentGroup7 group7)
		{
			var group10 = group7.Group10[0];
			CreateDOC(group10);
			CreateNAD(group10);
		}

		void CreateDOC(SegmentGroup10 group10)
		{
			var doc = group10.DOC[0];
			D99BMessageUtilities.PopulateDOC(doc, DocumentNameCodeList.PartyInformation, string.Empty);
		}

		void CreateNAD(SegmentGroup10 group10)
		{
			var nad = group10.NAD[0];

			nad.PartyFunctionCodeQualifier = data.IsVendor ? PartyFunctionCodeQualifierList.Vendor : PartyFunctionCodeQualifierList.UltimateConsignee;

			var address = data.VendorOrConsigneeAddress;
			if (address != null)
			{
				var orgName = address.E2_CompanyName;
				if (!string.IsNullOrEmpty(orgName))
				{
					SetPartyName(nad, orgName);
				}
				SetPartyAddress(nad, address);

				nad.CityName = address.E2_City;

				nad.CountrySubEntityDetails.CountrySubEntityNameCode = address.E2_State;
				nad.CountrySubEntityDetails.CodeListIdentificationCode = CodeListIdentificationCodeList.CountrySubEntity;
				nad.CountrySubEntityDetails.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.IsoInternationalOrganizationForStandardization;

				nad.PostalIdentificationCode = address.E2_Postcode;
				nad.CountryNameCode = address.CountryCode;
			}
		}

		void SetPartyName(NADSegment nad, ZString orgName)
		{
			var orgNameAndPartyName = GetPartyNameAndOrgName(orgName);
			nad.PartyName.PartyName1 = orgNameAndPartyName.Item2;
			orgNameAndPartyName = GetPartyNameAndOrgName(orgNameAndPartyName.Item1);
			nad.PartyName.PartyName2 = orgNameAndPartyName.Item2;
			orgNameAndPartyName = GetPartyNameAndOrgName(orgNameAndPartyName.Item1);
			nad.PartyName.PartyName3 = orgNameAndPartyName.Item2;
			orgNameAndPartyName = GetPartyNameAndOrgName(orgNameAndPartyName.Item1);
			nad.PartyName.PartyName4 = orgNameAndPartyName.Item2;
			orgNameAndPartyName = GetPartyNameAndOrgName(orgNameAndPartyName.Item1);
			nad.PartyName.PartyName5 = orgNameAndPartyName.Item2;
			nad.PartyName.PartyNameFormatCode = PartyNameFormatCodeList.NameComponentsInSequenceAsDefinedInDescriptionBelow;
		}

		Tuple<ZString, string> GetPartyNameAndOrgName(ZString orgName)
		{
			var length = 35;
			var partyName = string.Empty;
			orgName = orgName.Trim();
			if (orgName.Length > length)
			{
				partyName = orgName.Left(length);
				orgName = orgName.Substring(length);
			}
			else
			{
				partyName = orgName;
				orgName = ZString.Empty;
			}
			return Tuple.Create(orgName, partyName);
		}

		void SetPartyAddress(NADSegment nad, IDocAddress docAddress)
		{
			var length = 30;
			var address1 = docAddress.E2_Address1.Trim();
			var address2 = docAddress.E2_Address2.Trim();

			if (address1.Length > length)
			{
				nad.Street.StreetAndNumberPOBox1 = address1.Left(length);
				var exceedingAddress1 = address1.Substring(length).Trim();
				if (exceedingAddress1.Length > 0)
				{
					nad.Street.StreetAndNumberPOBox2 = exceedingAddress1.Length < length ? (exceedingAddress1 + ' ' + address2.Left(length - 1 - exceedingAddress1.Length)).TrimEnd() : exceedingAddress1.Left(length).ToString();
				}
				else
				{
					nad.Street.StreetAndNumberPOBox2 = address2.Left(length);
				}
			}
			else
			{
				nad.Street.StreetAndNumberPOBox1 = address1;
				nad.Street.StreetAndNumberPOBox2 = address2.Left(length);
			}
		}

		void CreateUNT(UNHSegment unh)
		{
			var unt = edifactMessage.UNT[0];
			D99BMessageUtilities.PopulateUNT(unt, edifactMessage.CountIncludingUNT.ToString(CultureInfo.InvariantCulture), unh.MessageReferenceNumber);
		}
	}
}
