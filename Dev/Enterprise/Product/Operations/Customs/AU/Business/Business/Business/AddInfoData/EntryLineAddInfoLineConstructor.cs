using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EntryLineAddInfoLineConstructor
	{
		public EntryLineAddInfoLineConstructor(CusEntryLine entryLine)
		{
			this.entryLine = entryLine;
		}

		public ZString AddInfoLine
		{
			get
			{
				if (!isCalculationUpToDate)
				{
					CalculateAddInfoLine();
					isCalculationUpToDate = true;
				}
				return fAddInfoLine;
			}
		}

		public void RefreshCalculation()
		{
			isCalculationUpToDate = false;
		}

		#region Implmentation

		void CalculateAddInfoLine()
		{
			if (entryLine.RandomLine != null)
			{
				Dictionary<string, ZString> addInfoPairs = new Dictionary<string, ZString>();

				foreach (ZString addInfoPair in entryLine.RandomLine.AggregatedAddInfoLine.Split('*'))
				{
					string key = addInfoPair.Split('=')[0];
					string fieldName = SerialisableAUAddInfo.Schema.TablePrefix + key;

					if (!ShouldIgnore(fieldName) && !FieldNamesToAggregate.Contains(fieldName))
					{
						addInfoPairs.Add(key, addInfoPair);
					}
				}

				foreach (string fieldName in FieldNamesToAggregate)
				{
					Money aggregated = GetAggregatedMoney(fieldName);

					string amountString = "";

					if (!aggregated.IsEmpty || fieldName == AUAddInfoSchema.ZA_TILV.Name && aggregated.IsValid)
					{
						amountString = aggregated.Amount.ToString(2).Replace(".00", "") + (aggregated.Currency != null ? aggregated.Currency.Code : "");
					}
					else
					{
						var aggregatedValue = GetAggregatedValue(fieldName);
						if (aggregatedValue > 0m)
						{
							amountString = aggregatedValue.ToString(2).Replace(".00", "");
						}
					}

					if (!string.IsNullOrEmpty(amountString))
					{
						string key1 = fieldName.Substring(3);
						addInfoPairs.Add(key1, key1 + "=" + amountString);
					}
				}

				ArrayList orderedKeys = new ArrayList(addInfoPairs.Keys);
				orderedKeys.Sort(new CaseInsensitiveComparer());

				var result = new ZStringBuilder();
				foreach (string key in orderedKeys)
				{
					ZString addInfoPair;
					if (addInfoPairs.TryGetValue(key, out addInfoPair))
					{
						result.Append(addInfoPair);
					}
				}
				fAddInfoLine = result.ToStringWithDelimiterBetweenAppends("*");
			}
		}

		ZString fAddInfoLine;
		bool isCalculationUpToDate;
		readonly CusEntryLine entryLine;

		static IEnumerable<string> FieldNamesToAggregate
		{
			get
			{
				fieldsToAggregate = fieldsToAggregate ?? new string[]
			   {
					AUAddInfoSchema.ZA_DTY.Name,
					AUAddInfoSchema.ZA_ADJ.Name,
					AUAddInfoSchema.ZA_DXP.Name,
					AUAddInfoSchema.ZA_TILV.Name,
					AUAddInfoSchema.ZA_STD.Name,
					AUAddInfoSchema.ZA_WET.Name,
					AUAddInfoSchema.ZA_QT2.Name,
					AUAddInfoSchema.ZA_DMP.Name,
					AUAddInfoSchema.ZA_WRQ.Name,
					AUAddInfoSchema.ZA_ODF.Name
				};
				return fieldsToAggregate;
			}
		}

		[ThreadStatic]
		static string[] fieldsToAggregate;

		bool ShouldIgnore(string fieldName)
		{
			return ((entryLine.RandomLine.AddInfo.ZA_PST == AUAddInfo.GeneralPreferenceRate)
				&& ((fieldName == AUAddInfoSchema.ZA_POC.Name) || (fieldName == AUAddInfoSchema.ZA_PRT.Name)));
		}

		Money GetAggregatedMoney(string fieldName)
		{
			if (fieldName == AUAddInfoSchema.ZA_ADJ.Name)
			{
				return entryLine.PriceAdjustment;
			}

			if (fieldName == AUAddInfoSchema.ZA_DXP.Name)
			{
				return entryLine.DumpingExportPrice;
			}

			if (fieldName == AUAddInfoSchema.ZA_TILV.Name)
			{
				return HasInvoiceLineWithOverriddenTILV() ? (!entryLine.EntryLineAddInfo.ZA_TILV.IsEmpty ? entryLine.EntryLineAddInfo.TILVMoney : entryLine.TransportAndInsurance) : Money.Invalid;
			}

			return Money.Empty;
		}

		bool HasInvoiceLineWithOverriddenTILV()
		{
			foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
			{
				if (!invoiceLine.AddInfo.ZA_TILV.IsEmpty)
				{
					return true;
				}
			}
			return false;
		}

		ZDecimal GetAggregatedValue(string fieldName)
		{
			if (fieldName == AUAddInfoSchema.ZA_DTY.Name)
			{
				return entryLine.DTY.Amount;
			}

			if (fieldName == AUAddInfoSchema.ZA_STD.Name)
			{
				return entryLine.StandardDuty.Amount;
			}

			if (fieldName == AUAddInfoSchema.ZA_WET.Name)
			{
				return entryLine.WET.Amount;
			}

			if (fieldName == AUAddInfoSchema.ZA_ODF.Name)
			{
				return entryLine.OtherDutyFactor.Amount;
			}

			if (fieldName == AUAddInfoSchema.ZA_QT2.Name)
			{
				return entryLine.SecondCustomsQuantity;
			}

			if (fieldName == AUAddInfoSchema.ZA_DMP.Name)
			{
				return entryLine.DumpingDuty;
			}

			if (fieldName == AUAddInfoSchema.ZA_WRQ.Name)
			{
				return entryLine.WRQ;
			}

			return 0m;
		}

		#endregion
	}
}
