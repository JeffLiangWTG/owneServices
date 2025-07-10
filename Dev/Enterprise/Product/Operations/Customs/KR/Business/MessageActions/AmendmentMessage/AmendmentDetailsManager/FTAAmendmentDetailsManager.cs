using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public abstract class FTAAmendmentDetailsManager<TCurrentDataProvider, TCurrentDataHeaderInterface> : AmendmentDetailsManager<TCurrentDataProvider, TCurrentDataHeaderInterface>
		where TCurrentDataProvider : IMessageDataProvider, TCurrentDataHeaderInterface
		where TCurrentDataHeaderInterface : IMessageDataProvider
	{
		protected FTAAmendmentDetailsManager(CusEntryHeader entry, string messageType)
			: base(entry, messageType)
		{
		}

		public ZString LawCodeDescription => Entry.Factory.GetCachedValue<FTALawCodeList>().GetDescriptionFromCode(Entry.EntryInstruction?.CEI_FTARelationArticleCode ?? ZString.Empty);
		public override ZString AmendmentType
		{
			get
			{
				if (!amendmentType.HasValue)
				{
					amendmentType = FTAAmendmentType.Codes.UXX;
					var amendTypeSum = CalculateAmendmentType(nameof(IImportFTALine));
					switch (amendTypeSum)
					{
						case 1:
							amendmentType = FTAAmendmentType.Codes.XXI;
							break;
						case 2:
							amendmentType = FTAAmendmentType.Codes.XDX;
							break;
						case 3:
							amendmentType = FTAAmendmentType.Codes.XDI;
							break;
						case 4:
							amendmentType = FTAAmendmentType.Codes.UXX;
							break;
						case 5:
							amendmentType = FTAAmendmentType.Codes.UXI;
							break;
						case 6:
							amendmentType = FTAAmendmentType.Codes.UDX;
							break;
						case 7:
							amendmentType = FTAAmendmentType.Codes.UDI;
							break;
					}
				}
				return amendmentType.Value;
			}
		}
		ZString? amendmentType;

		protected override CodeDescriptionPairList GetDataItemIDList(BusinessObjectFactory factory) => factory.GetCachedValue<FTAAmendmentDataItemIDList>();

		protected override IEnumerable<string> GetMandatoryItems() => Enumerable.Empty<string>();

		[CodeAlive("Used in sum calculation part")]
		enum LineAmendType { Add = 1, Delete = 2, Update = 4 }

		protected int CalculateAmendmentType(string lineEntityType)
		{
			var amendTypes = AmendedItems.Where(x => x.EntityType == lineEntityType).Select(x => x.AmendType).Distinct();
			var amendmentTypeSum = 0;
			foreach (var amendType in amendTypes)
			{
				switch (amendType)
				{
					case EntityAmendType.Add:
						amendmentTypeSum += (int)LineAmendType.Add;
						break;
					case EntityAmendType.Delete:
						amendmentTypeSum += (int)LineAmendType.Delete;
						break;
					case EntityAmendType.Update:
						amendmentTypeSum += (int)LineAmendType.Update;
						break;
				}
			}
			return amendmentTypeSum;
		}

		protected override IEnumerable<AmendedItem> GetAmendedItems()
		{
			var result = new List<AmendedItem>();
			IEnumerable<AmendedItem> amendedItems = base.GetAmendedItems();
			foreach (AmendedItem item in amendedItems)
			{
				if (item.AmendType != EntityAmendType.NoChange)
				{
					item.BeforeValue = TransformDescription(item, item.BeforeValue);
					item.AfterValue = TransformDescription(item, item.AfterValue);

					result.Add(item);
				}
			}
			return result;
		}

		static ZString TransformDescription(AmendedItem amendedItem, ZString value)
		{
			var result = value;

			if (FTAAmendmentDataItemIDList.IsDateTimeField(amendedItem.DataItemID))
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
