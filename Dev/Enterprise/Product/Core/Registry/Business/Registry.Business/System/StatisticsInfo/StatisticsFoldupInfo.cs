using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public partial class StatisticsFoldupInfo : RegistryBusinessObjectTemplate
	{
		public StatisticsFoldupInfo() { }

		public StatisticsFoldupInfo(FallbackLevel fallbackLevel, BusinessObjectFactory factory, StatisticsFoldupInfoCollection parentCollection)
			: base(fallbackLevel, factory)
		{
			this.ParentCollection = parentCollection;
		}

		internal StatisticsFoldupInfoCollection ParentCollection;

		#region SuppressResourceStringsCheckRegion

		public static class Schema
		{
			public const string WaitScale = "WaitScale";
			public const string WaitAmount = "WaitAmount";
			public const string AggregateScale = "AggregateScale";
			public const string AggregateAmount = "AggregateAmount";
		}

		#endregion

		#region Properties

		#region WaitScale

		[List("WaitScaleList")]
		[ResourceStringData("StatisticsFoldupInfo|WaitScale", Caption = "Wait scale")]
		[MaxLength(20)]
		public ZString WaitScale
		{
			get { return waitScale; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(WaitScaleInfo, ref waitScale, value);
				this.waitAmountRange = null;
				if (!IsValidationSuspended)
				{
					ValidateEverything();
				}
			}
		}

		ZString waitScale;

		public ZPropertyInfo WaitScaleInfo
		{
			get { return GetZPropertyInfo(Schema.WaitScale); }
		}

		#endregion

		#region WaitAmount

		[ResourceStringData("StatisticsFoldupInfo|WaitAmount", Caption = "Wait amount")]
		public ZInt WaitAmount
		{
			get { return waitAmount; }
			set
			{
				SetNonPersistentPropertyValue<ZInt>(WaitAmountInfo, ref waitAmount, value);
				this.waitAmountRange = null;
				if (!IsValidationSuspended)
				{
					ValidateEverything();
				}
			}
		}

		ZInt waitAmount;

		public ZPropertyInfo WaitAmountInfo
		{
			get { return GetZPropertyInfo(Schema.WaitAmount); }
		}

		#endregion

		#region AggregateScale

		[List("AggregateScaleList")]
		[ResourceStringData("StatisticsFoldupInfo|AggregateScale", Caption = "Aggregate scale")]
		[MaxLength(20)]
		public ZString AggregateScale
		{
			get { return aggregateScale; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(AggregateScaleInfo, ref aggregateScale, value);
				if (!IsValidationSuspended)
				{
					ValidateEverything();
				}
			}
		}

		ZString aggregateScale;

		public ZPropertyInfo AggregateScaleInfo
		{
			get { return GetZPropertyInfo(Schema.AggregateScale); }
		}

		#endregion

		#region AggregateAmount

		[ResourceStringData("StatisticsFoldupInfo|AggregateAmount", Caption = "Aggregate amount")]
		public ZInt AggregateAmount
		{
			get { return aggregateAmount; }
			set
			{
				SetNonPersistentPropertyValue<ZInt>(AggregateAmountInfo, ref aggregateAmount, value);
				if (!IsValidationSuspended)
				{
					ValidateEverything();
				}
			}
		}

		ZInt aggregateAmount;

		public ZPropertyInfo AggregateAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AggregateAmount); }
		}

		#endregion

		#region Lookups

		public CodeDescriptionPairList WaitScaleList
		{
			get { return new TimeFrameList(); }
		}

		public CodeDescriptionPairList AggregateScaleList
		{
			get
			{
				return new CodeDescriptionPairList()
				{
					new CodeDescriptionPair(TimeFrameList.Codes.Minute, TimeFrameList.Descriptions.Minute),
					new CodeDescriptionPair(TimeFrameList.Codes.Hour, TimeFrameList.Descriptions.Hour),
					new CodeDescriptionPair(TimeFrameList.Codes.Day, TimeFrameList.Descriptions.Day),
					new CodeDescriptionPair(TimeFrameList.Codes.Month, TimeFrameList.Descriptions.Month),
					new CodeDescriptionPair(TimeFrameList.Codes.Quarter, TimeFrameList.Descriptions.Quarter),
					new CodeDescriptionPair(TimeFrameList.Codes.Year, TimeFrameList.Descriptions.Year),
				};
			}
		}

		#endregion //Lookups

		#endregion //Properties

		#region XMLSerialisation

		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			AggregateScale = reader.ReadElementString(Schema.AggregateScale);
			AggregateAmount = reader.ReadElementStringAsZInt(Schema.AggregateAmount);
			WaitScale = reader.ReadElementString(Schema.WaitScale);
			WaitAmount = reader.ReadElementStringAsZInt(Schema.WaitAmount);
		}

		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.AggregateScale, AggregateScale);
			writer.WriteElementString(Schema.AggregateAmount, AggregateAmount.ToString());
			writer.WriteElementString(Schema.WaitScale, WaitScale);
			writer.WriteElementString(Schema.WaitAmount, WaitAmount.ToString());
		}

		#endregion //XMLSerialisation

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) // Glorious boilerplate.
		{
			return new StatisticsFoldupInfo(fallbackLevel, factory, null);
		}

		public ZDateTime GetWaitDate(ZDateTime now)
		{
			return
				(WaitScale == TimeFrameList.Codes.Year) ? now.AddYears(-WaitAmount)
				: (WaitScale == TimeFrameList.Codes.Quarter) ? now.AddMonths(-3 * WaitAmount)
				: (WaitScale == TimeFrameList.Codes.Month) ? now.AddMonths(-WaitAmount)
				: (WaitScale == TimeFrameList.Codes.Week) ? now.AddDays(-7 * WaitAmount)
				: (WaitScale == TimeFrameList.Codes.Day) ? now.AddDays(-WaitAmount)
				: (WaitScale == TimeFrameList.Codes.Hour) ? now.AddHours(-WaitAmount)
				: (WaitScale == TimeFrameList.Codes.Minute) ? now.AddMinutes(-WaitAmount)
				: ZDateTime.Empty;
		}

		#region Validation

		public void CheckWaitScale(StatisticsFoldupInfo rule)
		{
			rule.WaitScaleInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(rule.WaitScaleInfo);
			ListValidation.ErrorIfInvalidCode(rule.WaitScaleInfo);
		}

		public void CheckWaitAmount(StatisticsFoldupInfo rule)
		{
			rule.WaitAmountInfo.ClearAllNotifications();

			switch (rule.WaitScale)
			{
				case TimeFrameList.Codes.Minute:
					CompareValidation.CheckWithinRange(rule.WaitAmountInfo, 1, 1440); // up to 1 day
					break;
				case TimeFrameList.Codes.Hour:
					CompareValidation.CheckWithinRange(rule.WaitAmountInfo, 1, 168); // up to 1 week
					break;
				case TimeFrameList.Codes.Day:
					CompareValidation.CheckWithinRange(rule.WaitAmountInfo, 1, 365); // up to ~1 year
					break;
				case TimeFrameList.Codes.Week:
					CompareValidation.CheckWithinRange(rule.WaitAmountInfo, 1, 156); // up to ~3 years
					break;
				case TimeFrameList.Codes.Month:
					CompareValidation.CheckWithinRange(rule.WaitAmountInfo, 1, 36); // up to 3 years
					break;
				case TimeFrameList.Codes.Quarter:
					CompareValidation.CheckWithinRange(rule.WaitAmountInfo, 1, 12); // up to 3 years
					break;
				case TimeFrameList.Codes.Year:
					CompareValidation.CheckWithinRange(rule.WaitAmountInfo, 1, 100); // up to 100 years
					break;
				default:
					CompareValidation.CheckGreaterThanOrEqualTo(rule.WaitAmountInfo, 1);
					break;
			}
		}

		public void CheckFoldScale(StatisticsFoldupInfo rule)
		{
			rule.AggregateScaleInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(rule.AggregateScaleInfo);
			ListValidation.ErrorIfInvalidCode(rule.AggregateScaleInfo);
		}

		public void CheckFoldAmount(StatisticsFoldupInfo rule)
		{
			rule.AggregateAmountInfo.ClearAllNotifications();

			switch (rule.AggregateScale)
			{
				case TimeFrameList.Codes.Minute:
					CompareValidation.CheckWithinSet(rule.AggregateAmountInfo, 1, 2, 3, 4, 5, 6, 10, 12, 15, 20, 30);
					break;
				case TimeFrameList.Codes.Hour:
					CompareValidation.CheckWithinSet(rule.AggregateAmountInfo, 1, 2, 3, 4, 6, 8, 12);
					break;
				case TimeFrameList.Codes.Day:
					CompareValidation.CheckWithinSet(rule.AggregateAmountInfo, 1);
					break;
				case TimeFrameList.Codes.Month:
					CompareValidation.CheckWithinSet(rule.AggregateAmountInfo, 1, 2, 3, 4, 6);
					break;
				case TimeFrameList.Codes.Quarter:
					CompareValidation.CheckWithinSet(rule.AggregateAmountInfo, 1, 2);
					break;
				case TimeFrameList.Codes.Year:
					CompareValidation.CheckWithinRange(rule.AggregateAmountInfo, 1, 100);
					break;
				default:
					CompareValidation.CheckGreaterThanOrEqualTo(rule.AggregateAmountInfo, 1);
					break;
			}
		}

		#region WaitAmountRange

		sealed class AmountRange
		{
			public int From;
			public int To;
		}

		Dictionary<int, AmountRange> MonthAmountRange
		{
			get
			{
				if (monthAmountRange == null)
				{
					monthAmountRange = new Dictionary<int, AmountRange>();
					monthAmountRange.Add(01, new AmountRange { From = 24 * 60 * 0028, To = 24 * 60 * 0031 });
					monthAmountRange.Add(02, new AmountRange { From = 24 * 60 * 0059, To = 24 * 60 * 0062 });
					monthAmountRange.Add(03, new AmountRange { From = 24 * 60 * 0089, To = 24 * 60 * 0092 });
					monthAmountRange.Add(04, new AmountRange { From = 24 * 60 * 0120, To = 24 * 60 * 0123 });
					monthAmountRange.Add(05, new AmountRange { From = 24 * 60 * 0150, To = 24 * 60 * 0153 });
					monthAmountRange.Add(06, new AmountRange { From = 24 * 60 * 0181, To = 24 * 60 * 0184 });
					monthAmountRange.Add(07, new AmountRange { From = 24 * 60 * 0212, To = 24 * 60 * 0215 });
					monthAmountRange.Add(08, new AmountRange { From = 24 * 60 * 0242, To = 24 * 60 * 0245 });
					monthAmountRange.Add(09, new AmountRange { From = 24 * 60 * 0273, To = 24 * 60 * 0276 });
					monthAmountRange.Add(10, new AmountRange { From = 24 * 60 * 0303, To = 24 * 60 * 0306 });
					monthAmountRange.Add(11, new AmountRange { From = 24 * 60 * 0334, To = 24 * 60 * 0337 });
					monthAmountRange.Add(12, new AmountRange { From = 24 * 60 * 0365, To = 24 * 60 * 0366 });
					monthAmountRange.Add(13, new AmountRange { From = 24 * 60 * 0393, To = 24 * 60 * 0397 });
					monthAmountRange.Add(14, new AmountRange { From = 24 * 60 * 0424, To = 24 * 60 * 0428 });
					monthAmountRange.Add(15, new AmountRange { From = 24 * 60 * 0454, To = 24 * 60 * 0458 });
					monthAmountRange.Add(16, new AmountRange { From = 24 * 60 * 0485, To = 24 * 60 * 0489 });
					monthAmountRange.Add(17, new AmountRange { From = 24 * 60 * 0515, To = 24 * 60 * 0519 });
					monthAmountRange.Add(18, new AmountRange { From = 24 * 60 * 0546, To = 24 * 60 * 0550 });
					monthAmountRange.Add(19, new AmountRange { From = 24 * 60 * 0577, To = 24 * 60 * 0581 });
					monthAmountRange.Add(20, new AmountRange { From = 24 * 60 * 0607, To = 24 * 60 * 0611 });
					monthAmountRange.Add(21, new AmountRange { From = 24 * 60 * 0638, To = 24 * 60 * 0642 });
					monthAmountRange.Add(22, new AmountRange { From = 24 * 60 * 0668, To = 24 * 60 * 0672 });
					monthAmountRange.Add(23, new AmountRange { From = 24 * 60 * 0699, To = 24 * 60 * 0703 });
					monthAmountRange.Add(24, new AmountRange { From = 24 * 60 * 0730, To = 24 * 60 * 0731 });
					monthAmountRange.Add(25, new AmountRange { From = 24 * 60 * 0758, To = 24 * 60 * 0762 });
					monthAmountRange.Add(26, new AmountRange { From = 24 * 60 * 0789, To = 24 * 60 * 0793 });
					monthAmountRange.Add(27, new AmountRange { From = 24 * 60 * 0819, To = 24 * 60 * 0823 });
					monthAmountRange.Add(28, new AmountRange { From = 24 * 60 * 0850, To = 24 * 60 * 0854 });
					monthAmountRange.Add(29, new AmountRange { From = 24 * 60 * 0880, To = 24 * 60 * 0884 });
					monthAmountRange.Add(30, new AmountRange { From = 24 * 60 * 0911, To = 24 * 60 * 0915 });
					monthAmountRange.Add(31, new AmountRange { From = 24 * 60 * 0942, To = 24 * 60 * 0946 });
					monthAmountRange.Add(32, new AmountRange { From = 24 * 60 * 0972, To = 24 * 60 * 0976 });
					monthAmountRange.Add(33, new AmountRange { From = 24 * 60 * 1003, To = 24 * 60 * 1007 });
					monthAmountRange.Add(34, new AmountRange { From = 24 * 60 * 1033, To = 24 * 60 * 1037 });
					monthAmountRange.Add(35, new AmountRange { From = 24 * 60 * 1064, To = 24 * 60 * 1068 });
					monthAmountRange.Add(36, new AmountRange { From = 24 * 60 * 1095, To = 24 * 60 * 1096 });
				}

				return monthAmountRange;
			}
		}

		Dictionary<int, AmountRange> monthAmountRange;

		AmountRange WaitAmountRange
		{
			get
			{
				if (waitAmountRange == null)
				{
					int value = 0;
					switch (WaitScale)
					{
						case TimeFrameList.Codes.Year:
							if (WaitAmount <= 3)
							{
								waitAmountRange = MonthAmountRange[12 * WaitAmount];
							}
							else
							{
								value = 365 * 24 * 60 * WaitAmount;
								waitAmountRange = new AmountRange { From = value, To = value };
							}
							break;
						case TimeFrameList.Codes.Quarter:
							waitAmountRange = MonthAmountRange[3 * WaitAmount];
							break;
						case TimeFrameList.Codes.Month:
							waitAmountRange = MonthAmountRange[WaitAmount];
							break;
						case TimeFrameList.Codes.Week:
							value = 7 * 24 * 60 * WaitAmount;
							waitAmountRange = new AmountRange { From = value, To = value };
							break;
						case TimeFrameList.Codes.Day:
							value = 24 * 60 * WaitAmount;
							waitAmountRange = new AmountRange { From = value, To = value };
							break;
						case TimeFrameList.Codes.Hour:
							value = 60 * WaitAmount;
							waitAmountRange = new AmountRange { From = value, To = value };
							break;
						case TimeFrameList.Codes.Minute:
							waitAmountRange = new AmountRange { From = WaitAmount, To = WaitAmount };
							break;
						default:
							throw new Exception();
					}
				}

				return waitAmountRange;
			}
		}

		AmountRange waitAmountRange;

		#endregion //WaitAmountRange

		int GetFoldAmount(StatisticsFoldupInfo rule)
		{
			switch (rule.AggregateScale)
			{
				case TimeFrameList.Codes.Year:
					return 7000 + rule.AggregateAmount;
				case TimeFrameList.Codes.Quarter:
					return 6000 + rule.AggregateAmount;
				case TimeFrameList.Codes.Month:
					return 5000 + rule.AggregateAmount;
				case TimeFrameList.Codes.Week:
					return 4000 + rule.AggregateAmount;
				case TimeFrameList.Codes.Day:
					return 3000 + rule.AggregateAmount;
				case TimeFrameList.Codes.Hour:
					return 2000 + rule.AggregateAmount;
				case TimeFrameList.Codes.Minute:
					return 1000 + rule.AggregateAmount;
				default:
					throw new Exception();
			}
		}

		bool HasBaseNotifications()
		{
			bool hasNotifications = false;
			if (ParentCollection != null)
			{
				foreach (StatisticsFoldupInfo rule in ParentCollection)
				{
					CheckWaitScale(rule);
					CheckWaitAmount(rule);
					CheckFoldScale(rule);
					CheckFoldAmount(rule);

					if (rule.WaitScaleInfo.HasNotifications()
						|| rule.WaitAmountInfo.HasNotifications()
						|| rule.AggregateScaleInfo.HasNotifications()
						|| rule.AggregateAmountInfo.HasNotifications())
					{
						hasNotifications = true;
					}
				}
			}

			return hasNotifications;
		}

		void ValidateEverything()
		{
			if (!HasBaseNotifications() && ParentCollection != null)
			{
				ParentCollection.Sort<StatisticsFoldupInfo>((x, y) => x.WaitAmountRange.To.CompareTo(y.WaitAmountRange.To));

				var collection = ParentCollection
					.Cast<StatisticsFoldupInfo>()
					.OrderBy(rule => rule.WaitAmountRange.To);

				StatisticsFoldupInfo prev = null;
				var rules = new List<StatisticsFoldupInfo>();
				foreach (StatisticsFoldupInfo curr in collection)
				{
					if (prev != null)
					{
						//Wait duplicates and overlaps
						if (curr.WaitAmountRange.From <= prev.WaitAmountRange.To && prev.WaitAmountRange.From <= curr.WaitAmountRange.To)
						{
							SetErrorWaitOverlap(prev, curr);
							return;
						}

						//Fold in order
						if (GetFoldAmount(curr) < GetFoldAmount(prev))
						{
							SetErrorFoldInOrder(prev, curr);
							return;
						}

						//Fold ranges
						if (ValidForCheckFoldRange(prev, curr))
						{
							if (!rules.Contains(prev))
							{
								rules.Add(prev);
							}

							rules.Add(curr);
						}
					}

					prev = curr;
				}

				//Fold ranges
				foreach (var group in rules.GroupBy(rule => rule.AggregateScale))
				{
					ValidateFoldRanges(group.ToList());
				}
			}
		}

		bool ValidForCheckFoldRange(StatisticsFoldupInfo prev, StatisticsFoldupInfo curr)
		{
			return
				prev.AggregateScale == curr.AggregateScale
				&&
				(
					prev.AggregateScale == TimeFrameList.Codes.Minute
					|| prev.AggregateScale == TimeFrameList.Codes.Hour
					|| prev.AggregateScale == TimeFrameList.Codes.Month
				)
				&& prev.AggregateAmount > 1
				&& prev.AggregateAmount < curr.AggregateAmount;
		}

		void ValidateFoldRanges(List<StatisticsFoldupInfo> rules)
		{
			for (int i = 0; i < rules.Count - 1; i++)
			{
				var prev = rules[i];
				for (int j = i + 1; j < rules.Count; j++)
				{
					var curr = rules[j];
					if (curr.AggregateAmount % prev.AggregateAmount > 0)
					{
						SetErrorFoldRanges(prev, curr);
					}
				}
			}
		}

		void SetErrorWaitOverlap(StatisticsFoldupInfo prev, StatisticsFoldupInfo curr)
		{
			var errorMessageWaitOverlap = ResString.GetMultilingualString("30201ce6-9d1a-4dd8-8e54-f499cf1ba885",
				"The wait period {0}({1}) causes overlapping with the wait period {2}({3}).",
				prev.WaitScale, prev.WaitAmount,
				curr.WaitScale, curr.WaitAmount);

			prev.WaitAmountInfo.AddError(errorMessageWaitOverlap);
			curr.WaitAmountInfo.AddError(errorMessageWaitOverlap);
		}

		void SetErrorFoldInOrder(StatisticsFoldupInfo prev, StatisticsFoldupInfo curr)
		{
			var errorMessageFoldInOrder = ResString.GetMultilingualString("2707f08d-35fb-4985-a2d3-f2827ee68157",
				"The aggregate period {0}({1}) is less then previous aggregate period {2}({3}).\r\nEach next aggregate period MUST be greater or equal to previous one.",
				curr.AggregateScale, curr.AggregateAmount,
				prev.AggregateScale, prev.AggregateAmount);

			prev.AggregateAmountInfo.AddError(errorMessageFoldInOrder);
			curr.AggregateAmountInfo.AddError(errorMessageFoldInOrder);
		}

		void SetErrorFoldRanges(StatisticsFoldupInfo prev, StatisticsFoldupInfo curr)
		{
			var errorMessageFoldRanges = ResString.GetMultilingualString("a81afde2-ee4e-48e8-a9d5-1eee2b72cda2",
				"The aggregate period {0}({1}) causes overlapping with the aggregate period {2}({3}).",
				prev.AggregateScale, prev.AggregateAmount,
				curr.AggregateScale, curr.AggregateAmount);

			prev.AggregateAmountInfo.AddError(errorMessageFoldRanges);
			curr.AggregateAmountInfo.AddError(errorMessageFoldRanges);
		}

#endregion //Validation
	}
}
