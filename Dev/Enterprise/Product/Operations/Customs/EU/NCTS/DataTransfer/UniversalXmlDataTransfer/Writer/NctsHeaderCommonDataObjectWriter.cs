using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalDataObjectWriterHelper = Enterprise.Customs.EU.DataTransfer.Universal.UniversalDataObjectWriterHelper;

namespace Enterprise.Customs.EU.NCTS.DataTransfer
{
	public abstract class NctsHeaderCommonDataObjectWriter : TopLevelDataObjectWriter<NctsHeader, Shipment>
	{
		protected NctsHeaderCommonDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}
		protected UniversalDataObjectWriterHelper helper { get; private set; }

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.NctsHeader;
		}

		protected sealed override void PopulateDataObject(NctsHeader headerBO, Shipment headerData)
		{
			helper = new UniversalDataObjectWriterHelper(headerBO.Factory, headerBO.CountryCode);
			headerData.Branch = Branch.New(headerBO.Branch);
			headerData.MessagingApplicationCode = GetMessagingApplicationCode(headerBO);
			PopulateMovementReferenceNumber(headerBO, headerData);
			PopulateJobDocAddresses(headerBO, headerData);
			PopulateNotes(headerBO, headerData);
			PopulateDataObjectCore(headerBO, headerData);
		}

		void PopulateJobDocAddresses(NctsHeader headerBO, Shipment headerData)
		{
			headerData.SetOrganizationAddressCollection(() => ProcessCollection(((IDocAddresses)headerBO).DocAddresses, new JobDocAddressDataObjectWriter(writeManager)));
		}

		void PopulateMovementReferenceNumber(NctsHeader headerBO, Shipment headerData)
		{
			headerData.SetEntryNumberCollection(() =>
			{
				var entryNumbers = new List<EntryNumber>();

				var mrn = headerBO.MovementReferenceNumber;
				if (!mrn.IsEmpty)
				{
					entryNumbers.Add(new EntryNumber()
					{
						Type = new EntryType() { Code = CusEntryNumberTypes.Standard.MovementReferenceNumber },
						Number = mrn
					});
				}

				return entryNumbers;
			});
		}

		void PopulateNotes(NctsHeader headerBO, Shipment headerData)
		{
			headerData.SetNoteCollection(() =>
			{
				var notes = headerBO.Notes.GetAllNotesVisibleToCurrentCompany().OrderBy(x => x.ST_Description);
				return ProcessCollection(notes, new NoteDataObjectWriter(writeManager), CollectionContent.Partial);
			});
		}

		protected virtual void PopulateDataObjectCore(NctsHeader headerBO, Shipment headerData) { }

		protected abstract CodeDescriptionPair GetMessagingApplicationCode(NctsHeader headerBO);
	}
}
