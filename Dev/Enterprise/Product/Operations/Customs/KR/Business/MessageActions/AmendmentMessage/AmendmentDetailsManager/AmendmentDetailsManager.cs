using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public abstract class AmendmentDetailsManager
	{
		public static AmendmentDetailsManager New(CusEntryHeader entry, string messageType)
		{
			switch (messageType)
			{
				case ElectronicDocumentTypeList.Codes._5BB:
					return new GOVCBR5BBAmendmentDetailsManager(entry, messageType);
				case ElectronicDocumentTypeList.Codes._5AS:
					return new GOVCBR5ASAmendmentDetailsManager(entry, messageType);
				case ElectronicDocumentTypeList.Codes._5DR:
				case ElectronicDocumentTypeList.Codes._5DS:
					return new LocalExportAmendmentDetailsManager(entry, messageType);
				case ElectronicDocumentTypeList.Codes._105:
					return new GOVCBR105AmendmentDetailsManager(entry, messageType);
				case ElectronicDocumentTypeList.Codes._5FE:
					return new GOVCBR5FEAmendmentDetailsManager(entry, messageType);
				case ElectronicDocumentTypeList.Codes._DHS:
					return new GOVCBRDHSAmendmentDetailsManager(entry, messageType);
				default:
					return null;
			}
		}
		protected AmendmentDetailsManager(CusEntryHeader entry, string messageType)
		{
			Entry = entry;
			OriginalMessageType = ElectronicDocumentTypeList.GetOriginalType(messageType);
		}
		protected readonly CusEntryHeader Entry;
		public readonly string OriginalMessageType;
		public IEnumerable<AmendedItem> AmendedItems => amendedItems ?? (amendedItems = GetAmendedItems());
		IEnumerable<AmendedItem> amendedItems;
		protected abstract IEnumerable<AmendedItem> GetAmendedItems();
		public abstract ZString AmendmentType { get; }
		public CodeDescriptionPairList DataItemIDList => GetDataItemIDList(Entry.Factory);
		protected abstract CodeDescriptionPairList GetDataItemIDList(BusinessObjectFactory factory);
	}
	public abstract class AmendmentDetailsManager<TCurrentDataProvider, TCurrentDataHeaderInterface> : AmendmentDetailsManager
		where TCurrentDataProvider : IMessageDataProvider, TCurrentDataHeaderInterface
		where TCurrentDataHeaderInterface : IMessageDataProvider
	{
		protected AmendmentDetailsManager(CusEntryHeader entry, string messageType) : base(entry, messageType)
		{
			CurrentDataProvider = GetCurrentDataProvider();
			CurrentDataProvider.RoundDecimalValueRoundedWithDecimalPlaces();
		}
		public readonly TCurrentDataProvider CurrentDataProvider;

		protected override IEnumerable<AmendedItem> GetAmendedItems()
		{
			IEnumerable<AmendedItem> result = Enumerable.Empty<AmendedItem>();
			if (LatestLodgedSnapshot != null)
			{
				TCurrentDataProvider latestLodgedDataProvider = default(TCurrentDataProvider);
				using (var textReader = LatestLodgedSnapshot.GetCES_SnapshotXmlReader())
				{
					latestLodgedDataProvider = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<TCurrentDataProvider>(textReader);
				}
				result = new DataProviderComparer<TCurrentDataHeaderInterface>().Compare(OriginalMessageType, latestLodgedDataProvider, CurrentDataProvider, GetMandatoryItems());
			}
			return result;
		}
		protected abstract TCurrentDataProvider GetCurrentDataProvider();
		CusEntrySnapshot LatestLodgedSnapshot => latestLodgedSnapshot ?? (latestLodgedSnapshot = Entry.Snapshots.GetLatestSnapshotIn(OriginalMessageType, EntrySnapshotStatus.Lodged));
		CusEntrySnapshot latestLodgedSnapshot;

		protected abstract IEnumerable<string> GetMandatoryItems();
	}
}
