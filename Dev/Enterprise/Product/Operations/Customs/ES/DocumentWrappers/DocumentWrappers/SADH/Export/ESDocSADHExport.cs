using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using ESCusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.DocumentWrappers.SADH
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Used in documents DataContext")]
	public class ESDocSADHExport : ESDocSADH
	{
		ESDocSADHExport(ESCusEntryHeader entryHeader, BusinessObjectFactory factoryToWrap)
			: base(entryHeader, factoryToWrap)
		{ }

		public static ESDocSADHExport New(ESCusEntryHeader entryHeader, BusinessObjectFactory factoryToWrap) => new ESDocSADHExport(entryHeader, factoryToWrap);

		protected override DocSADHLineCollection GetLinesCore() => new ESDocSADHLineCollectionExport(EntryHeader.MergedLines, Factory);

		protected override DocSADHPageCollection GetPagesCore() => new ESDocSADHPageCollectionExport(EntryHeader.MergedLines, Factory);

		protected override ZString Box1bSubStyleCore => EntryHeader.CH_EntryStatus == EntryStatusCodes.PreDeclarationAccepted ? EntryHeader.GetMappedSubTypeForExportPreDeclaration() : base.Box1bSubStyleCore;

		protected override ZString Box18IdentityOfTransportAtDepartureCore => Declaration.IsRoadInland
																					? (Declaration.JE_TransportIDInland.IsEmpty
																								? Declaration.JE_Trailer1RegNo
																								: Declaration.JE_TransportIDInland)
																					: (Declaration.IsRailInland && Declaration.ZG_Box18TransportID.IsEmpty)
																							? Declaration.JE_Trailer1RegNo
																							: Declaration.ZG_Box18TransportID;

		protected override ZString Box18TransportNationalityAtDepartureCore => Declaration.IsRoadInland
																					? (Declaration.JE_TransportIDInland.IsEmpty
																								? Declaration.JE_RN_NKTrailer1Nationality
																								: Declaration.JE_RN_NKTransportNationalityInland)
																					: (Declaration.IsRailInland && Declaration.ZG_Box18TransportID.IsEmpty)
																							? Declaration.JE_RN_NKTrailer1Nationality
																							: Declaration.ZG_Box18TransportNationality;

		protected override ZString Box20AgreedPlaceCode2Core => ZString.Empty;

		protected override ZString Box20AgreedPlaceCore
		{
			get
			{
				var code = EntryHeader.IsExportUCC6 ? CommonWrappersHelper.GetIncotermPlaceCode(InvoiceHeader, Declaration) : ZString.Empty;

				return code.IsEmpty ? base.Box20AgreedPlaceCore : code;
			}
		}

		protected override ZString Box20AgreedPlaceCodeCore => EntryHeader.IsExportUCC6
														? CommonWrappersHelper.GetIncotermPlaceCodeCountry(InvoiceHeader, Declaration)
														: Declaration.ZG_AgreedPlaceCode;

		public ZString Box14DeclarantRepresentativeName
		{
			get
			{
				if (Declaration.JE_DeclarantType != ESRepresentationTypeList.Codes._1Auto && Declaration.Declarant != null)
				{
					return DocAddress.New(Declaration.Declarant, Factory).CompanyName;
				}
				else
				{
					return "EXPEDIDOR";
				}
			}
		}

		public ZString Box14Authorization => Declaration.ZG_AuthPerDeclaration ? (NoResString)"Autorización: O" : string.Empty;

		public ZString BoxDReleaseDate => IsEntryAcceptedByCustoms && !EntryHeader.CH_EntryReleaseDate.IsEmpty ? string.Format(CultureInfo.InvariantCulture, (NoResString)"Levante: {0}", EntryHeader.CH_EntryReleaseDate.ToString("dd-MM-yyyy", CultureInfo.CurrentCulture)) : string.Empty;

		public ZString BoxDAcceptanceDate => IsEntryAcceptedByCustoms && !EntryHeader.MovementReferenceNumberIssueDate.IsEmpty ? string.Format(CultureInfo.InvariantCulture, (NoResString)"Admitido: {0}", EntryHeader.MovementReferenceNumberIssueDate.ToString("dd-MM-yyyy", CultureInfo.CurrentCulture)) : string.Empty;

		protected override ZString BoxDContainerSealsAffixedCore
		{
			get
			{
				if (IsEntryAcceptedByCustoms)
				{
					ZStringBuilder result = new ZStringBuilder();
					ZString[] seals;
					ZString sealsCount = ZString.Empty;
					if (!EntryHeader.IsUCC6)
					{
						seals = EntryHeader.SealCodes.ToArray();
						sealsCount = seals.Length > 0 ? string.Format(CultureInfo.InvariantCulture, (NoResString)"Con precinto: {0}", seals.Length.ToString(CultureInfo.InvariantCulture)) : string.Empty;
						result.AppendIfNotEmpty(sealsCount);
					}
					else
					{
						seals = EntryHeader.SealCodesEquipments.ToArray();
					}
					foreach (var seal in seals)
					{
						result.AppendIfNotEmpty(seal);
					}
					return result.ToStringWithDelimiterBetweenAppends("; ");
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZString BoxDClearance => IsEntryAcceptedByCustoms && !EntryHeader.CSVClearance.IsEmpty ? string.Format(CultureInfo.InvariantCulture, "C.S.V.: {0}", EntryHeader.CSVClearance) : string.Empty;

		protected override OrgHeader GetBox54OrgHeader => Declaration.Supplier;
	}
}
