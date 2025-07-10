using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageDefinitions;
using CargoWise.Customs.GB.MessageDefinitions.CDS.CDSInventoryLinking;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Customs.Shared.WorldCustomsOrganisation.MessageDefinitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Interfaces;
using Enterprise.Customs.GB.CDS.CDSResponse;
using Enterprise.Customs.GB.Registry;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using CusEntryHeader = Enterprise.Customs.GB.Business.Declaration.CusEntryHeader;
using CusEntryLine = Enterprise.Customs.GB.Business.Declaration.CusEntryLine;
using FriendlyCodeWithPointers = Enterprise.Customs.GB.CDS.Messaging.FriendlyCodeWithPointers;

namespace Enterprise.Customs.GB.CDS
{
	public static class CDSExtensions
	{
		public static ZString GetCredentialsKey(this ForwardingConsol consol, ZString badgeCode)
		{
			var enterpriseCode = GBExtensions.GetEnterpriseCode();
			var eori = consol?.GetEori() ?? ZString.Empty;
			return (badgeCode.Length == 3 && !eori.IsEmpty) || (badgeCode.Length > 3 && !eori.IsEmpty && !badgeCode.Contains(eori))
				? FormattableString.Invariant($"{enterpriseCode}.{eori}.{badgeCode}") : FormattableString.Invariant($"{enterpriseCode}.{badgeCode}");
		}

		public static bool IsQueryMessageFunctionCode(this ZString messageFunction)
		{
			return messageFunction == CDSEDIMessageTypeList.Codes.InventoryLinkingQueryRequest || messageFunction == CDSEDIMessageTypeList.Codes.MasterQueryDeclaration;
		}

		public static ZBool IsApplicableToAmendment(this CusEntryHeader entry, ZString amendmentReasonCode)
		{
			return entry != null
					&& !entry.MovementReferenceNumber.IsEmpty
					&& amendmentReasonCode.EqualsAnyIgnoringCase(AmendmentCodes);
		}

		static IEnumerable<ZString> AmendmentCodes
		{
			get
			{
				yield return AmendmentCancellationReasonCode.Codes.A_CommodityCode;
				yield return AmendmentCancellationReasonCode.Codes.A_Value;
				yield return AmendmentCancellationReasonCode.Codes.A_TaxLines;
				yield return AmendmentCancellationReasonCode.Codes.A_Preference;
				yield return AmendmentCancellationReasonCode.Codes.A_Quota;
				yield return AmendmentCancellationReasonCode.Codes.A_License;
				yield return AmendmentCancellationReasonCode.Codes.A_Currency;
				yield return AmendmentCancellationReasonCode.Codes.A_CPC;
				yield return AmendmentCancellationReasonCode.Codes.A_WeightQuantity;
				yield return AmendmentCancellationReasonCode.Codes.A_ConsigneeConsignor;
				yield return AmendmentCancellationReasonCode.Codes.A_Delete;
				yield return AmendmentCancellationReasonCode.Codes.A_Nil;
				yield return AmendmentCancellationReasonCode.Codes.A_Other;
			}
		}

		public static ZBool IsApplicableToCancellation(this CusEntryHeader entry, ZString amendmentReasonCode)
		{
			return entry != null
					&& !entry.MovementReferenceNumber.IsEmpty
					&& amendmentReasonCode.EqualsAnyIgnoringCase(CancellationCodes);
		}

		static IEnumerable<ZString> CancellationCodes
		{
			get
			{
				yield return AmendmentCancellationReasonCode.Codes.C_NotRequired;
				yield return AmendmentCancellationReasonCode.Codes.C_Duplicate;
				yield return AmendmentCancellationReasonCode.Codes.C_Other;
			}
		}

		static OrgAddress GetMatchedOrgAddressOrNullIfUnmatched(OrgAddress orgAddress) => (orgAddress?.Header == null || orgAddress.Header.PK == Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation) ? null : orgAddress;

		public static ZBool IsUnmatchedOrgHeader(this OrgHeader orgHeader) => orgHeader != null && orgHeader.PK == Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation;

		public static ZBool IsUnmatchedOrgAddress(this OrgAddress orgAddress) => orgAddress != null && orgAddress.OA_OH == Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation;

		public static ZBool IsUnmatchedJobDocAddress(this JobDocAddress jobDocAddress) => jobDocAddress != null && jobDocAddress.OrganisationPK == Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation;

		public static bool HasMultipleExportersViaLines(this CusEntryHeader entryHeader)
		{
			return entryHeader.MergedLines.Cast<CusEntryLine>()
				.Select(x => x.Consignor)
				.Where(x => GetMatchedOrgAddressOrNullIfUnmatched(x) != null)
				.Distinct()
				.IsCountMoreThan(1);
		}

		public static int CountSellersViaInvoiceHeaders(this CusEntryHeader entryHeader)
		{
			return entryHeader.InvoiceHeaders.Cast<JobComInvoiceHeader>()
				.Select(x => x.SellerAddress)
				.Where(x => GetMatchedOrgAddressOrNullIfUnmatched(x) != null)
				.Distinct()
				.Count();
		}

		public static int CountBuyersViaInvoiceHeaders(this CusEntryHeader entryHeader)
		{
			return entryHeader.InvoiceHeaders.Cast<JobComInvoiceHeader>()
				.Select(x => x.BuyerAddress)
				.Where(x => GetMatchedOrgAddressOrNullIfUnmatched(x) != null)
				.Distinct()
				.Count();
		}

		public static MetaData SetDeclaration(this MetaData metaData, CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData.Declaration declarationNode)
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(declarationNode.Serialize());

			metaData.Item = new[] { xmlDoc.DocumentElement };
			return metaData;
		}

		public static CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData.Declaration GetDeclaration(this MetaData metaData)
		{
			var items = metaData.Item;
			return items.Length == 1 ? XmlObjectSerializer.Deserialize<CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData.Declaration>(items[0].OuterXml) : null;
		}

		public static IEnumerable<Response> GetResponses(this MetaData metaData)
		{
			return metaData.Item.Select(responseXmlElement => XmlObjectSerializer.Deserialize<Response>(responseXmlElement.OuterXml)).WhereNotNull();
		}

		public static ZString Serialize(this inventoryLinkingConsolidationRequest request) => SerializationHelper.Serialize(request);

		public static ZString Serialize(this inventoryLinkingMovementRequest request) => SerializationHelper.Serialize(request);

		public static ZString Serialize(this inventoryLinkingQueryRequest request) => SerializationHelper.Serialize(request);

		public static ZString Serialize(this MetaData metaData) => SerializationHelper.Serialize(metaData);

		public static ZString Serialize(this CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData.Declaration declaration) => SerializationHelper.Serialize(declaration);

		public static ZString Serialize(this Response responseData) => SerializationHelper.Serialize(responseData);

		public static CusEntryNumber GetCusEntryNumberFromLRN(this Response response, BusinessObjectFactory factory)
		{
			var lrn = response.GetDeclarationFunctionalReferenceID();

			var cusEntryNumQuery = new ZQuery(CusEntryNumSchema.CE_EntryNum, lrn);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedKingdom);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryIsSystemGenerated, ZBool.True);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.LocalReferenceNumber);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);

			var cusEntryNumber = factory.LoadTop1<CusEntryNumber>(cusEntryNumQuery);

			return cusEntryNumber;
		}

		public static CusEntryNumber GetCusEntryNumberFromMRN(this Response response, BusinessObjectFactory factory)
		{
			var mrn = response.Declaration?.ID?.Value;

			if (!string.IsNullOrEmpty(mrn))
			{
				var cusEntryNumQuery = new ZQuery(CusEntryNumSchema.CE_EntryNum, mrn);
				cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
				cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedKingdom);
				cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_IssueDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDate.Today.AddYears(-10));
				var entryNum = factory.LoadTop1<CusEntryNumber>(cusEntryNumQuery);

				var cusEntryNumber = factory.LoadTop1<CusEntryNumber>(cusEntryNumQuery);

				return cusEntryNumber;
			}

			return null;
		}

		public static IMessageAttachee GetMessageAttacheeFromMRN(this DeclarationStatusResponseDeclarationStatusDetails detail, BusinessObjectFactory factory)
		{
			var mrn = detail.Declaration?.ID?.Value;

			if (!string.IsNullOrEmpty(mrn))
			{
				var cusEntryNumQuery = new ZQuery(CusEntryNumSchema.CE_EntryNum, mrn);
				cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
				cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedKingdom);
				cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_IssueDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDate.Today.AddYears(-10));
				var entryNum = factory.LoadTop1<CusEntryNumber>(cusEntryNumQuery);

				var cusEntryNumber = factory.LoadTop1<CusEntryNumber>(cusEntryNumQuery);

				if (cusEntryNumber != null)
				{
					return cusEntryNumber.Parent as IMessageAttachee;
				}
			}

			return null;
		}

		public static ZString Serialize(this GBCustomsRequest requestData) => SerializationHelper.Serialize(requestData);

		public static ZString GetApplicationCodeForMessage(this CusEntryHeader entry)
		{
			var gateWay = (entry?.Declaration)?.ZG_Gateway ?? ZString.Empty;
			return gateWay == GatewayList.Codes.CCSUKviaNTMsgGW
				? EDIMessage.ApplicationCodes.GbCDSViaCCSUK
				: EDIInterchange.ApplicationCodes.GbCustomsDeclarationServices;
		}

		public static ZString StripNewlineCharacters(this ZString value, int maxLength = int.MinValue)
		{
			var maxLengthToTrim = maxLength == int.MinValue ? value.Length : maxLength;
			return value.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ").Trim().SubstringSafe(0, maxLengthToTrim);
		}

		public static ZString StripIllegalCharactersT1(this ZString input)
		{
			//Text Format 1: EDIFACT Level B character set. This consists of upper and lower case alphas, numeric digits, spaces and certain special characters. 
			//'A' - 'Z', 'a' - 'z', '0' - '9, ‘ ‘ . , - ( ) / ' + : = ? ! " % & * ; < > 
			return Regex.Replace(input, @"[^A-Za-z0-9 \.\,\-\(\)\/\'\+\:\=\?\!""\%\&\*\;\<\>]", "");
		}

		public static ZDateTime ToZDateTime(this DateTimeType dateTimeString)
		{
			if (dateTimeString.formatCode == FormatCodeType.Item304)
			{
				return ZDateTime.TryParseExact(((ZString)dateTimeString.Value).SubstringSafe(0, 14), out var date, "yyyyMMddHHmmss") ? date : ZDateTime.Empty;
			}
			else
			{
				return ZDateTime.TryParseISO8601Date(dateTimeString.Value, out var date) ? date : ZDateTime.Empty;
			}
		}

		public static ZString InnerXML(this XElement el)
		{
			var reader = el.CreateReader();
			reader.MoveToContent();
			return reader.ReadInnerXml();
		}

		public static ZString FormatXml(this ZString xml)
		{
			try
			{
				var doc = XDocument.Parse(xml);
				return doc.ToString();
			}
			catch (XmlException)
			{
				return xml;
			}
		}

		public static IEnumerable<FriendlyCodeWithPointers> GetAllFriendlyErrorsForRequestAndRejection(this Response response, XElement requestXml)
		{
			return response.GetAllFriendlyCodePointers(res => res?.Error ?? Array.Empty<ICodeWithPointers>(), requestXml);
		}

		public static IEnumerable<FriendlyCodeWithPointers> GetAllFriendlyAmendments(this Response response, XElement newDeclarationXml)
		{
			return response.GetAllFriendlyCodePointers(res => res?.Amendment ?? Array.Empty<ICodeWithPointers>(), newDeclarationXml);
		}

		static IEnumerable<FriendlyCodeWithPointers> GetAllFriendlyCodePointers(this Response response, Func<Response, IEnumerable<ICodeWithPointers>> codeWithPointersGetter, XElement requestXml)
		{
			return response?.GetCodeWithPointers(codeWithPointersGetter).Select(x => new FriendlyCodeWithPointers(new CargoWise.Customs.GB.MessageDefinitions.CDS.PointerParser(), x, requestXml)) ?? Enumerable.Empty<FriendlyCodeWithPointers>();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static IEnumerable<ICodeWithPointers> GetCodeWithPointers(this Response response, Func<Response, IEnumerable<ICodeWithPointers>> codeWithPointersGetter)
		{
			var codeWithPointerses = response == null
				? Enumerable.Empty<ICodeWithPointers>()
				: codeWithPointersGetter?.Invoke(response) ?? Enumerable.Empty<ICodeWithPointers>();

			return codeWithPointerses
				.Where(x => !string.IsNullOrEmpty(x.Code));
		}

		public static IPointer Find(this ICodeWithPointers provider, ZString documentSectionCode)
		{
			return provider?.Pointers?.FirstOrDefault(c => c.DocumentSectionCode == documentSectionCode);
		}

		public static ZString GetPointers(this ICodeWithPointers provider)
		{
			var pointers = provider?.Pointers ?? Enumerable.Empty<IPointer>();

			return pointers.Select(x => x.ToFriendlyString()).Where(x => !x.IsEmpty).JoinAsString("/");
		}

		static ZString ToFriendlyString(this IPointer pointer)
		{
			return pointer == null
				? string.Empty
				: string.IsNullOrEmpty(pointer.TagId)
					? pointer.SequenceNumeric > ZDecimal.Zero
						? string.Format("{0}[{1}]", pointer.DocumentSectionCode, pointer.SequenceNumeric)
						: pointer.DocumentSectionCode
					: pointer.SequenceNumeric > ZDecimal.Zero
						? string.Format("{0}[{1}]/{2}[1]", pointer.DocumentSectionCode, pointer.SequenceNumeric, pointer.TagId)
						: string.Format("{0}/{1}[1]", pointer.DocumentSectionCode, pointer.TagId);
		}

		public static ZString ConvertFromUnToChiefCountry(this CusEntryHeader entryHeader, ZString unCountry)
		{
			if (!unCountry.IsEmpty)
			{
				var convertedCode = Universal.ZZRefCusMapCombined.MapCW1CodeToCustomsCode(entryHeader.Factory,
					entryHeader.Declaration.JE_ApplicationCode,
					Universal.RefCusMapTypeList.Codes.EUCTY, unCountry, ZDateTime.Today);
				if (!convertedCode.IsEmpty)
				{
					unCountry = convertedCode;
				}
			}
			return unCountry;
		}
	}
}
