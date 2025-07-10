using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR5ASAmendmentDetailsManager : AmendmentDetailsManager<ExportEntryHeader, IExportEntryHeader>
	{
		public GOVCBR5ASAmendmentDetailsManager(CusEntryHeader entry, ZString messageType)
			: base(entry, messageType)
		{
		}
		public override ZString AmendmentType => _5ASAmendmentType.Codes.Amendment;

		protected override CodeDescriptionPairList GetDataItemIDList(BusinessObjectFactory factory) => factory.GetCachedValue<ExportAmendmentDataItemIDList>();

		protected override ExportEntryHeader GetCurrentDataProvider() => new ExportEntryHeaderCreator().Create(Entry);

		protected override IEnumerable<AmendedItem> GetAmendedItems()
		{
			var result = new List<AmendedItem>();
			IEnumerable<AmendedItem> amendedItems = base.GetAmendedItems();
			foreach (AmendedItem item in amendedItems)
			{
				item.AmendType = TransformAmendType(item);
				if (item.AmendType != EntityAmendType.NoChange)
				{
					item.DataItemID = TransformDataItemID(item);
					item.BeforeValue = TransformDescription(item, item.BeforeValue);
					item.AfterValue = TransformDescription(item, item.AfterValue);

					result.Add(item);
				}
			}
			return result;
		}

		readonly List<string> mandatoryItems = new List<string>();

		protected override IEnumerable<string> GetMandatoryItems()
		{
			return new string[]
			{
				ExportAmendmentDataItemIDList.Codes.B201
			};
		}

		static EntityAmendType TransformAmendType(AmendedItem item)
		{
			var result = item.AmendType;

			if (ExportAmendmentDataItemIDList.IsHeaderDataItem(item.DataItemID) && (item.AmendType == EntityAmendType.Delete || item.AmendType == EntityAmendType.Add))
			{
				result = EntityAmendType.Update;
			}

			if (item.AmendType == EntityAmendType.Update)
			{
				switch (item.DataItemID)
				{
					case ExportAmendmentDataItemIDList.Codes.F101:
					case ExportAmendmentDataItemIDList.Codes.H102:
						if (item.BeforeValue.IsEmpty)
						{
							result = EntityAmendType.Add;
						}
						else if (item.AfterValue.IsEmpty)
						{
							result = EntityAmendType.Delete;
						}
						break;
					case ExportAmendmentDataItemIDList.Codes.H101:
						if (item.BeforeValue.IsEmpty)
						{
							result = EntityAmendType.Add;
						}
						else if (item.AfterValue.IsEmpty)
						{
							result = EntityAmendType.NoChange;
						}
						break;
					case ExportAmendmentDataItemIDList.Codes.H103:
					case ExportAmendmentDataItemIDList.Codes.H104:
						var beforeValueTransformed = TransformDescription(item, item.BeforeValue);
						var afterValueTransformed = TransformDescription(item, item.AfterValue);
						if (beforeValueTransformed.IsEmpty)
						{
							result = EntityAmendType.Add;
						}
						else if (afterValueTransformed.IsEmpty)
						{
							result = EntityAmendType.NoChange;
						}
						break;
				}
			}

			return result;
		}

		static ZString TransformDataItemID(AmendedItem item)
		{
			var result = item.DataItemID;

			if (item.AmendType == EntityAmendType.Delete)
			{
				switch (item.DataItemID)
				{
					case ExportAmendmentDataItemIDList.Codes.F101:
						result = ExportAmendmentDataItemIDList.Codes.F001;
						break;
					case ExportAmendmentDataItemIDList.Codes.H102:
						result = ExportAmendmentDataItemIDList.Codes.H001;
						break;
					default:
						switch (item.EntityType)
						{
							case nameof(IExportEntryLine):
								result = ExportAmendmentDataItemIDList.Codes.B001;
								break;
							case nameof(IExportInvoiceLine):
								result = ExportAmendmentDataItemIDList.Codes.C001;
								break;
							case nameof(IExportContainer):
								result = ExportAmendmentDataItemIDList.Codes.D001;
								break;
							case nameof(IExportVehicleNo):
								result = ExportAmendmentDataItemIDList.Codes.E001;
								break;
							case nameof(IExportGAApprovalDocument):
								result = ExportAmendmentDataItemIDList.Codes.G001;
								break;
						}
						break;
				}
			}
			return result;
		}

		static ZString TransformDescription(AmendedItem amendedItem, ZString value)
		{
			var result = value;

			if (ExportAmendmentDataItemIDList.IsDateTimeField(amendedItem.DataItemID))
			{
				if (!value.IsEmpty)
				{
					if (DateTime.TryParseExact(value, ZDateTime.LongTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
					{
						result = dt.ToString(DateFormatType.Date);
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
