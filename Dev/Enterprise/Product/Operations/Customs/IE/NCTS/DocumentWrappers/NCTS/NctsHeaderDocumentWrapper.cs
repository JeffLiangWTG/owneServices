using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.NCTS.Business;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU.NCTS;
using static Enterprise.Integration.Customs.EU;
using NctsEuOfficeCode = Enterprise.Customs.EU.NCTS.Business.NctsEuOfficeCode;
using NctsSupportingDocument = Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocument;

namespace Enterprise.Customs.IE.NCTS.DocumentWrappers
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Used in documents DataContext")]
	public class NctsHeaderDocumentWrapper : Enterprise.DocumentWrappers.Customs.EU.NCTS.NctsHeaderDocumentWrapper
	{
		protected NctsHeaderDocumentWrapper(NctsHeader nctsHeader, BusinessObjectFactory factory) : base(nctsHeader, factory)
		{
		}

		public static DocBaseWrapper New(NctsHeader nctsHeader, BusinessObjectFactory factoryToWrap)
		{
			Argument.NotNull(nctsHeader, nameof(nctsHeader));
			DocBaseWrapper nctsHeaderDocumentWrapper;

			if (nctsHeader.IsInPhase5TransitionPeriod)
			{
				if (nctsHeader.IsSecurityDeclaration)
				{
					nctsHeaderDocumentWrapper = SecurityNctsHeaderDocumentWrapper.New(nctsHeader, factoryToWrap);
				}
				else
				{
					nctsHeaderDocumentWrapper = new NctsHeaderDocumentWrapper(nctsHeader, factoryToWrap);
				}
			}
			else
			{
				nctsHeaderDocumentWrapper = Phase5NctsHeaderDocumentWrapper.New(nctsHeader, factoryToWrap);
			}
			return nctsHeaderDocumentWrapper;
		}

		protected new NctsHeader NctsHeader => (NctsHeader)base.NctsHeader;

		protected override ZString ConvertDateToString(ZDateTime? date) => date?.ToString("dd-MM-yyyy") ?? ZString.Empty;

		public ZString BOX441DOCSANDCERTS => GetBox44_DocsAndCerts();

		protected override ZString GetGuarantee() => string.Join(";", NctsHeader.MovementHeader.Guarantees.Cast<NctsGuarantee>().Select(g => g.PW_BondType));
		protected override ZString GetOfficeOfDestination() => GetOfficeCode(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);

		protected ZString GetBox44_DocsAndCerts()
		{
			var result = new ZStringBuilder();
			var cusAuthorizationUsages = NctsHeader.IsPhase5Departure ? (IBusinessObjectCollection<CusAuthorizationUsage>)NctsHeader.MovementHeader.CusAuthorizationUsages : NctsHeader.CusAuthorizationUsages;
			var authorizationUsagesFormatted = GetAuthorizationUsageFormatted(cusAuthorizationUsages);
			var additionalInfosStringBuilder = new ZStringBuilder();
			var additionalReferencesStringBuilder = new ZStringBuilder();
			var additionalInformationStringBuilder = new ZStringBuilder();
			NctsHeader.AdditionalDocuments.Select(x => (x.CSI_SubType, x.CSI_Code, x.CSI_ReferenceNumber))
				.OrderBy(x => x.CSI_Code).ThenBy(x => x.CSI_ReferenceNumber)
				.ForEach(additionalDocument =>
				{
					switch (additionalDocument.CSI_SubType.ToUpperInvariant())
					{
						case AdditionalInfoSubTypeList.Codes.TransportDocument:
							additionalInfosStringBuilder.Append(GetAdditionalInfoFormatted(additionalDocument.CSI_Code, additionalDocument.CSI_ReferenceNumber));
							break;
						case AdditionalInfoSubTypeList.Codes.AdditionalReference:
							additionalReferencesStringBuilder.Append(GetAdditionalInfoFormatted(additionalDocument.CSI_Code, additionalDocument.CSI_ReferenceNumber));
							break;
						case AdditionalInfoSubTypeList.Codes.AdditionalInformation:
							additionalInformationStringBuilder.Append(GetAdditionalInfoFormatted(additionalDocument.CSI_Code, additionalDocument.CSI_ReferenceNumber));
							break;
					}
				});
			var supportingDocumentsFormatted = GetSupportingDocumentsFormatted(NctsHeader.MovementHeader.SupportingDocuments);
			result.AppendIfNotEmpty(authorizationUsagesFormatted);
			result.AppendIfNotEmpty(supportingDocumentsFormatted);
			result.AppendIfNotEmpty(additionalInfosStringBuilder.ToStringWithDelimiterBetweenAppends(";"));
			result.AppendIfNotEmpty(additionalReferencesStringBuilder.ToStringWithDelimiterBetweenAppends(";"));
			result.AppendIfNotEmpty(additionalInformationStringBuilder.ToStringWithDelimiterBetweenAppends(";"));
			return result.ToStringWithDelimiterBetweenAppends(";");
		}

		ZString GetAuthorizationUsageFormatted(IEnumerable<ICusAuthorizationUsage> authorizationUsage) => string.Join(";", authorizationUsage.Select(x => $"{x.AGC_Code} {x.AGC_Number}").OrderBy(x => x));

		ZString GetAdditionalInfoFormatted(ZString code, ZString referenceNumber) => $"{code} {referenceNumber}";

		ZString GetSupportingDocumentsFormatted(IEnumerable<NctsSupportingDocument> supportingDocument) => string.Join(";", supportingDocument.Select(x => $"{x.CSI_Code} {x.CSI_ReferenceNumber} {x.CSI_ItemNumber} {x.CSI_ReferenceNumber2}").OrderBy(x => x));

		protected override ZString GetBoxCOfficeOfDeparture() => GetOfficeCode(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);

		protected override ZString GetTransitCustomsOffices(ZInt rank)
		{
			var transitCustomsOfficeList = officeOfTransits ?? (officeOfTransits = NctsHeader.CommonMovementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().Where(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit).OrderBy(x => x.CY_Order).ThenBy(x => x.CY_SystemCreateTimeUtc).ToArray());
			var selectedTransitCustomsOffice = transitCustomsOfficeList.Length >= rank ? transitCustomsOfficeList[rank - 1] : null;
			return selectedTransitCustomsOffice?.CY_Data ?? ZString.Empty;
		}
		NctsEuOfficeCode[] officeOfTransits;

		public DocBaseWrapperCollection<NctsDepartureCargoDescWrapper> BoxS29Lines => boxS29Lines ?? (boxS29Lines = new NctsDepartureCargoDescWrapperCollection(NctsHeader.Bills.FirstOrDefault(), Factory));
		DocBaseWrapperCollection<NctsDepartureCargoDescWrapper> boxS29Lines;
	}
}
