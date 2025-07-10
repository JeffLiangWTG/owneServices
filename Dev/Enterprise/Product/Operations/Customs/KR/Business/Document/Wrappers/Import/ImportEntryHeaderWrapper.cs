using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class ImportEntryHeaderWrapper : NonPersistentBusinessObject,
		IVisualizerNoteSupporter, ISourceIdentifierProvider
	{
		public ImportEntryHeaderWrapper(ZGuid pk, ImportEntryHeader header, BusinessObjectFactory factory)
			: base(factory)
		{
			Header = header;
			EntryPK = pk;
		}
		ZGuid EntryPK { get; }

		#region IVisualizerNoteSupporter members
		ZGuid IVisualizerNoteSupporter.PK => EntryPK;
		ZGuid IVisualizerNoteSupporter.ChildBusinessObjectPK => ZGuid.Empty;
		string IVisualizerNoteSupporter.TableCode => CusEntryHeaderSchema.Constants.Prefix;
		#endregion

		public IImportEntryHeader Header { get; }
		public ZString MessageStatus { get; set; }
		public ZString FormattedImportDeclarationNumber => MessageFunctions.DeclarationNumberFormat(Header.ImportDeclarationNumber);
		public OrganizationDocWrapper Importer => importer ?? (importer = new OrganizationDocWrapper(Header.Importer));
		OrganizationDocWrapper importer;
		public OrganizationDocWrapper Declarant => declarant ?? (declarant = new OrganizationDocWrapper(Header.Declarant));
		OrganizationDocWrapper declarant;
		public OrganizationDocWrapper Payer => payer ?? (payer = new OrganizationDocWrapper(Header.Payer));
		OrganizationDocWrapper payer;
		public ZString PayerFormattedKoreanRegNoForResident => Payer.FormattedKoreanRegNoForResident;
		public ZString PayerFormattedBusinessRegNo => Payer.FormattedBusinessRegNo;
		public ZString PayerFormattedCorporationCode { get; set; }

		public ImportEntryLineWrapperCollection EntryLineItems
		{
			get
			{
				if (entryLineItems == null)
				{
					entryLineItems = new ImportEntryLineWrapperCollection(Header.EntryLines, Header.ExchangeRate, EntryLinePackTitle, Factory);
				}
				return entryLineItems;
			}
		}
		public ZString EntryLinePackTitle => !Header.PackType.IsEmpty ? Header.PackType + "(" + PackTypeName + ")" : ZString.Empty;

		ImportEntryLineWrapperCollection entryLineItems;
		public IImportEntryLine FirstEntryLine => Header.EntryLines.OrderBy(x => x.EntryLineNo).FirstOrDefault();

		public ImportPreviousExpDecLineWrapperCollection ImportPreviousExpDecLines
		{
			get
			{
				if (importPreviousExpDecLines == null)
				{
					importPreviousExpDecLines = new ImportPreviousExpDecLineWrapperCollection(Header.EntryLines, Factory);
				}
				return importPreviousExpDecLines;
			}
		}
		ImportPreviousExpDecLineWrapperCollection importPreviousExpDecLines;

		public EarlyReleaseMiscMessageSendingObject MessageSendingObject5BD { get; set; }
		public GOVCBR5GVMessageData MessageData5GV { get; set; }
		public GOVCBR5BEMessageData MessageData5BE { get; set; }
		public CusEntryNumber EntryNum5BD { get; set; }
		public ZDateTime DeclarationDate { get; set; }
		public ZString CustomsOfficerName5BD { get; set; }
		ZString PackTypeName => Factory.GetCachedValue<PackageKindCodeList>().GetDescriptionFromCode(Header.PackType);
		public ZString FormattedEntryNumber => MessageFunctions.DeclarationNumberFormat(Header.ImportDeclarationNumber);
		public ZString BondedAreaName => Header.BondedAreaCode.IsEmpty ? ZString.Empty : MessageFunctions.GetRefCusCodeListDescription(Factory, Header.BondedAreaCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode);
		public ZString VesselCountryCodeShort => MessageFunctions.GetCountryKRCCode(Factory, Header.VesselCountryCode);
		public ZString DepartureCountryCodeShort => MessageFunctions.GetCountryKRCCode(Factory, Header.DepartureCountryCode);

		public ZBool IsFullPage => ImportPreviousExpDecLines.Count % MaxRowsInOnePage == 0;
		const int MaxRowsInOnePage = 29;
		
		public CusEntryNumber EntryNumD72 { get; set; }

		public GOVCBR5TVMessageData MessageData5TV { get; set; }
		
		public GOVCBR5UOMessageData MessageData5UO { get; set; }
		
		public GOVCBR5WNMessageData MessageData5WN { get; set; }
		
		public GOVCBR5TWMessageData MessageData5TW { get; set; }

		public ZDateTime CancellationDecisionDate { get; set; }
		public ZDateTime EntryReleaseDate { get; set; }
		public ZBool IsImportCancellationDeclinedByCustoms { get; set; }
		public ZString FormattedCargoManagementNo
		{
			get
			{
				if (Header.CargoManagementNo.Length == 15)
				{
					return MessageFunctions.GetFormattedNumber(Header.CargoManagementNo, new int[] { 0, 11 });
				}
				else
				{
					return MessageFunctions.GetFormattedNumber(Header.CargoManagementNo, new int[] { 0, 11, 15 });
				}
			}
		}
		public CustomsOfficer CustomsOfficers5BF { get; set; }
		public CancellationMessageSendingObject MessageSendingObject5BF { get; set; }
		public CusEntryNumber EntryNum5GU { get; set; }
		public ZString FormattedTotalEntryLineCount => Header.EntryLines.Count().ToString("D3");
		public GOVCBR5GUMessageData MessageData5GU { get; set; }
		public CusEntryNumber EntryNum5UA { get; set; }
		public CusEntryNumber EntryNum5BA { get; set; }
		public CusEntryNumber EntryNumIMP { get; set; }
		public ZString CustomsOfficeName => MessageFunctions.GetCustomsOffice(Factory, Header.DeclarationCustomsOffice);
		public GOVCBR5UBMessageData MessageData5UB { get; set; }
		public CusEntryInstruction EntryInstruction5BA { get; set; }
		public ZString BrokerBusinessRegNo { get; set; }

		public ZGuid SourceIdentifier => EntryPK;

		public MessageSendingEntryLineObjectCollection EntryLineObjects5FN
		{
			get
			{
				if (entryLineObjects5FN == null)
				{
					entryLineObjects5FN = new MessageSendingEntryLineObjectCollection(Factory.Load<CusEntryHeader>(EntryPK));
					entryLineObjects5FN.PopulateElementsFrom5FNMessages((ImportEntryHeader)Header);
				}
				return entryLineObjects5FN;
			}
		}
		MessageSendingEntryLineObjectCollection entryLineObjects5FN;

		public bool HasAny5FNRejection { get; set; }
		public ZString ImporterTypeOfBusiness { get; set; }
		public CusEntryNumber EntryNum5TM { get; set; }
		public MessageSendingEntryLineObjectCollection GoldVATDeclarationEntryLineObjects
		{
			get
			{
				if (goldVATDeclarationEntryLineObjects == null)
				{
					goldVATDeclarationEntryLineObjects = new MessageSendingEntryLineObjectCollection(Factory.Load<CusEntryHeader>(EntryPK));
					goldVATDeclarationEntryLineObjects.PopulateElementsFromMergedLines(x => x.CL_IsGoldOrItsProduct, ElectronicDocumentTypeList.Codes._5TM);
				}
				return goldVATDeclarationEntryLineObjects;
			}
		}
		MessageSendingEntryLineObjectCollection goldVATDeclarationEntryLineObjects;
	}
}
