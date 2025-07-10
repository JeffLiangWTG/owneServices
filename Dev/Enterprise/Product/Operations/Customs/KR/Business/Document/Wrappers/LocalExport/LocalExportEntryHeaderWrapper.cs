using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class LocalExportEntryHeaderWrapper : NonPersistentBusinessObject,
		IVisualizerNoteSupporter
	{
		public LocalExportEntryHeaderWrapper(ZGuid pk, ZString headerType, LocalExportEntryHeader header, BusinessObjectFactory factory)
			: base(factory)
		{
			Header = header;
			HeaderType = headerType;
			EntryPK = pk;
		}
		ZGuid EntryPK { get; }

		#region IVisualizerNoteSupporter members
		ZGuid IVisualizerNoteSupporter.PK => EntryPK;
		ZGuid IVisualizerNoteSupporter.ChildBusinessObjectPK => ZGuid.Empty;
		string IVisualizerNoteSupporter.TableCode => CusEntryHeaderSchema.Constants.Prefix;
		#endregion

		public ILocalExportEntryHeader Header { get; }
		public ZString EntryStatus { get; set; }

		public ZString FormattedEntryNumber => MessageFunctions.DeclarationNumberFormat(Header.DeclarationNumber);

		public LocalExportEntryLineCollectionWrapper EntryLineItems
		{
			get
			{
				if (entryLineItems == null)
				{
					entryLineItems = new LocalExportEntryLineCollectionWrapper(Header.EntryLines, Factory);
				}
				return entryLineItems;
			}
		}
		LocalExportEntryLineCollectionWrapper entryLineItems;

		public LocalExportStevedoreCollectionWrapper StevedoreItems
		{
			get
			{
				if (stevedoreItems == null)
				{
					stevedoreItems = new LocalExportStevedoreCollectionWrapper(Header.Stevedores, Factory);
				}
				return stevedoreItems;
			}
		}
		LocalExportStevedoreCollectionWrapper stevedoreItems;

		public ZString HeaderType { get; }

		IGOVCBRR38MessageData MessageDataR38
		{
			get
			{
				if (messageDataR38 == null && R38Message != null)
				{
					using (var reader = R38Message.GetEM_MessageTextReader())
					{
						messageDataR38 = new GOVCBRR38DataProvider().GetMessageData(reader);
					}
				}
				return messageDataR38;
			}
		}
		IGOVCBRR38MessageData messageDataR38;

		IGOVCBRRR3MessageData MessageDataRR3
		{
			get
			{
				if (messageDataRR3 == null && RR3Message != null)
				{
					using (var reader = RR3Message.GetEM_MessageTextReader())
					{
						messageDataRR3 = new GOVCBRRR3DataProvider().GetMessageData(reader);
					}
				}
				return messageDataRR3;
			}
		}
		IGOVCBRRR3MessageData messageDataRR3;

		public ZString CustomsReferenceNumber => MessageDataR38?.ConfirmNumber ?? ZString.Empty;
		public ZString FormattedCustomsReferenceNumber => MessageFunctions.GetFormattedNumber(CustomsReferenceNumber, new int[] { 0, 3, 5, 7, 13 });

		public ZDateTime AcceptanceDate => MessageDataR38?.NoticeDateTime ?? ZDateTime.Empty;
		public ZString CustomsRemarks => MessageDataR38?.ContentDescription ?? ZString.Empty;
		public ZDateTime EntryReleaseDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (HeaderType == ElectronicDocumentTypeList.Codes._5DQ)
				{
					result = MessageDataRR3?.CustomsDateTime ?? ZDateTime.Empty;
				}
				return result;
			}
		}
		public ZString CustomsReviewOfficer => MessageDataRR3?.CustomsPersonName ?? ZString.Empty;
		public ZDateTime CustomsReviewDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (HeaderType == ElectronicDocumentTypeList.Codes._5DP)
				{
					result = MessageDataRR3?.CustomsDateTime ?? ZDateTime.Empty;
				}
				return result;
			}
		}

		public EDIMessage RR3Message { get; set; }
		public EDIMessage R38Message { get; set; }
		public ZString DeclarationTypeName => Factory.GetCachedValue<LocalExportTransactionNatureCodeList>().GetDescriptionFromCode(Header.DeclarationType);

		public OrganizationDocWrapper Supplier => supplier ?? (supplier = new OrganizationDocWrapper(Header.Supplier));
		OrganizationDocWrapper supplier;

		public OrganizationDocWrapper Exporter => exporter ?? (exporter = new OrganizationDocWrapper(Header.Exporter));
		OrganizationDocWrapper exporter;

		public OrganizationDocWrapper Manufacturer => manufacturer ?? (manufacturer = new OrganizationDocWrapper(Header.Manufacturer));
		OrganizationDocWrapper manufacturer;

		public OrganizationDocWrapper Importer => importer ?? (importer = new OrganizationDocWrapper(Header.Importer));
		OrganizationDocWrapper importer;

		public ZString WorkingVesselLloydsNumber => FirstOtherTransportMean?.WorkingVesselLloydsNumber ?? ZString.Empty;

		public ZString TransportVehicleRegNo => FirstOtherTransportMean?.TransportVehicleRegNo ?? ZString.Empty;

		public ZString BondedAreaAndTransportMeans
		{
			get
			{
				var transportMeans = WorkingVesselLloydsNumber;
				if (!transportMeans.IsEmpty && !TransportVehicleRegNo.IsEmpty)
				{
					transportMeans += ",";
				}
				transportMeans += TransportVehicleRegNo;
				var bondedArea = Header.BondedAreaCode;
				if (!bondedArea.IsEmpty && !transportMeans.IsEmpty)
				{
					bondedArea += " / ";
				}
				return bondedArea + transportMeans;
			}
		}

		public ILocalExportOtherTransportMeans FirstOtherTransportMean
		{
			get
			{
				if (firstOtherTransportMean == null)
				{
					firstOtherTransportMean = Header.OtherTransportMeans?.OrderBy(x => x.SequenceNo)?.FirstOrDefault();
				}
				return firstOtherTransportMean;
			}
		}
		ILocalExportOtherTransportMeans firstOtherTransportMean;

		public ZInt TotalEntryLineCount => EntryLineItems.Count;
		public ZDateTime MostRecentCESLogSLEventTime { get; set; }
		public ZString CustomsOfficeName { get; set; }
		public OrganizationDocWrapper Broker { get; set; }
	}
}
