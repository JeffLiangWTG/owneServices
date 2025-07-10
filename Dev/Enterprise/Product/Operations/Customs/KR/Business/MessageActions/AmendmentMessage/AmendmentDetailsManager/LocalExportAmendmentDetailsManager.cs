using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class LocalExportAmendmentDetailsManager : AmendmentDetailsManager<LocalExportEntryHeader, ILocalExportEntryHeader>
	{
		public LocalExportAmendmentDetailsManager(CusEntryHeader entry, ZString messageType)
			: base(entry, messageType)
		{
		}

		public override ZString AmendmentType => LocalExportAmendmentTypeList.Codes.Amendment;

		protected override LocalExportEntryHeader GetCurrentDataProvider() => OriginalMessageType == ElectronicDocumentTypeList.Codes._5DP ? new LocalExport5DPEntryHeaderCreator().Create(Entry)
			: new LocalExport5DQEntryHeaderCreator().Create(Entry);

		protected override CodeDescriptionPairList GetDataItemIDList(BusinessObjectFactory factory) => factory.GetCachedValue<LocalExportAmendmentDataItemIDList>();

		protected override IEnumerable<AmendedItem> GetAmendedItems()
		{
			var result = new List<AmendedItem>();
			IEnumerable<AmendedItem> amendedItems = base.GetAmendedItems();
			foreach (AmendedItem item in amendedItems)
			{
				if (item.AmendType != EntityAmendType.NoChange)
				{
					if (item.AmendType == EntityAmendType.Delete)
					{
						item.DataItemID = TransformDataItemID(item);
					}
					item.BeforeValue = TransformDescription(item, item.BeforeValue);
					item.AfterValue = TransformDescription(item, item.AfterValue);
				}

				result.Add(item);
			}
			return result;
		}

		protected override IEnumerable<string> GetMandatoryItems() => Enumerable.Empty<string>();

		static ZString TransformDataItemID(AmendedItem amendedItem)
		{
			var result = ZString.Empty;
			if (amendedItem.EntityType == nameof(ILocalExportOtherTransportMeans))
			{
				result = LocalExportAmendmentDataItemIDList.Codes._11B;
			}

			return result;
		}

		static ZString TransformDescription(AmendedItem amendedItem, ZString value)
		{
			var result = value;

			if (amendedItem.DataItemID == LocalExportAmendmentDataItemIDList.Codes._31)
			{
				if (!value.IsEmpty)
				{
					if (DateTime.TryParse(value, out DateTime dt1))
					{
						result = dt1.ToString(DateFormatType.Date);
					}
					else
					{
						result = ZString.Empty;
					}
				}
			}
			return result;
		}
	}
}
