using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineIntegration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CA.Business
{
	class D4NoticeDocumentWrapper : NonPersistentBusinessObject, IDocumentWrapper, IVisualizerNoteSupporter, ISourceIdentifierProvider
	{
		internal D4NoticeDocumentWrapper(UniversalEventMessage message, string showRawMessage) : base(message.Factory)
		{
			Argument.NotNull(message, nameof(message));
			if (message.EM_MessageSubType != UniversalEventMessageTypes.Codes.D4Notices)
			{
				throw new ArgumentException("D4NoticeDocumentWrapper is for D4 message only but was " + message.EM_MessageSubType);
			}

			universalEvent = message.GetEM_MessageTextReader()?.Parse<UniversalEvent>();
			this.message = message;

			carrier = new Lazy<ZZRefCarrierCombined>(() =>
								new ZZRefCarrierCombined.Loader(Factory).LoadFromCode(Enterprise.Core.Constants.CountryCodes.Canada, CarrierCode));
			this.ShowRawMessage = showRawMessage;
		}

		readonly UniversalEvent universalEvent;
		readonly UniversalEventMessage message;

		D4MessageInterpretationGenerator Generator => generator ?? (generator = new D4MessageInterpretationGenerator(Factory, universalEvent));

		D4MessageInterpretationGenerator generator;
		readonly Lazy<ZZRefCarrierCombined> carrier;

		#region Properties

		public ZString ShowRawMessage { get; }

		public ZString EventType => Generator.EventType;

		public ZString DocumentType => Generator.DocumentType;

		public ZString ProcessingDate => Generator.ProcessingDate.ToString();

		public ZString SendersReference => Generator.SendersReference;

		public ZString ReferenceNumber => Generator.ReferenceNumber;

		public BusinessObjectCollectionWrapper<D4MessageInterpretationGenerator.RelatedDocument> RelatedDocuments => Generator.RelatedDocuments;

		BusinessObjectCollectionWrapper<D4MessageInterpretationGenerator.D4PGADetail> pgaDetails;

		public BusinessObjectCollectionWrapper<D4MessageInterpretationGenerator.D4PGADetail> PGADetails
		{
			get
			{
				return pgaDetails ??
						 (pgaDetails =
							 new BusinessObjectCollectionWrapper<D4MessageInterpretationGenerator.D4PGADetail>(Generator.PGADetails));
			}
		}

		BusinessObjectCollectionWrapper<UniversalEventMessageInterpretationGenerator.Status> statuses;

		public BusinessObjectCollectionWrapper<UniversalEventMessageInterpretationGenerator.Status> Statuses
		{
			get
			{
				return statuses ??
						 (statuses =
							 new BusinessObjectCollectionWrapper<UniversalEventMessageInterpretationGenerator.Status>(Generator.Statuses));
			}
		}

		BusinessObjectCollectionWrapper<UniversalEventMessageInterpretationGenerator.CloseMessageHouseBills> closeMessageHouseBillses;

		public ZString NoticeRecipientType => Generator.NoticeRecipientType;

		public ZString NoticeRecipientReferenceNumber => Generator.NoticeRecipientReferenceNumber;

		public BusinessObjectCollectionWrapper<UniversalEventMessageInterpretationGenerator.CloseMessageHouseBills> CloseMessageHouseBillses
		{
			get
			{
				return closeMessageHouseBillses ??
						 (closeMessageHouseBillses =
							 new BusinessObjectCollectionWrapper<UniversalEventMessageInterpretationGenerator.CloseMessageHouseBills>(
								 Generator.CloseMessageHouseBillses));
			}
		}

		BusinessObjectCollectionWrapper<UniversalEventMessageInterpretationGenerator.RequestingPGA> requestingPGAs;

		public BusinessObjectCollectionWrapper<UniversalEventMessageInterpretationGenerator.RequestingPGA> RequestingPGAs
		{
			get
			{
				return requestingPGAs ??
						 (requestingPGAs =
							 new BusinessObjectCollectionWrapper<UniversalEventMessageInterpretationGenerator.RequestingPGA>(Generator.RequestingPGAs));
			}
		}

		BusinessObjectCollectionWrapper<UniversalEventMessageInterpretationGenerator.ErrorDetail> errorDetails;

		public BusinessObjectCollectionWrapper<UniversalEventMessageInterpretationGenerator.ErrorDetail> ErrorDetails
		{
			get
			{
				return errorDetails ??
						 (errorDetails =
							 new BusinessObjectCollectionWrapper<UniversalEventMessageInterpretationGenerator.ErrorDetail>(Generator.ErrorDetails));
			}
		}

		public ZString ContainerString => Generator.ContainerString;

		public ZString RawMessage => Generator.RawMessage;

		public ZString PreviousCCN => Generator.RelatedDocuments.OfType<D4MessageInterpretationGenerator.RelatedDocument>().Select(d => d.DocumentNumber).FirstOrDefault();

		public ZString CarrierCode => CarrierCodeFromReferenceNumber(Generator.ReferenceNumber);

		public ZZRefCarrierCombined Carrier => carrier.Value;

		public ZString CarrierName => Carrier?.ZZ4_Description ?? "";

		#endregion

		ZString CarrierCodeFromReferenceNumber(ZString referenceNumber)
		{
			return referenceNumber.SubstringSafe(0, 4);
		}

		#region IVisualizerNoteSupporter members

		ZGuid IVisualizerNoteSupporter.PK => message.PK;

		ZGuid IVisualizerNoteSupporter.ChildBusinessObjectPK => ZGuid.Empty;

		string IVisualizerNoteSupporter.TableCode => EDIMessageSchema.Constants.Prefix;

		#endregion

		ZGuid ISourceIdentifierProvider.SourceIdentifier => this.message.PK;
	}
}
