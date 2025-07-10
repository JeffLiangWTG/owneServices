using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// Collection of AU Customs Entry Lines.
	/// These lines are created by merging commercial invoice lines.
	/// The collection is dependent upon an CusEntryHeader business object.
	/// </summary>
	public class CusEntryLineCollection : Customs.Business.CusEntryLineCollection<CusEntryLine>
	{
		public CusEntryLineCollection(CusEntryHeader parentHeader, BusinessObjectFactory factory)
			: base(parentHeader)
		{
			this.entryHeader = parentHeader;
		}
		readonly CusEntryHeader entryHeader;

		public void UpdateDetailsNotToBeAmendedAsEntryIsCleared()
		{
			foreach (CusEntryLine entryLine in this)
			{
				entryLine.UpdateDetailsNotToBeAmendedAsEntryIsCleared();
			}
		}

		#region Boolean Properties
		public bool IsABNQuotedForLCTAndWET => Factory.GetValue(ref isABNQuotedForLCTAndWETCached, GetIsABNQuotedForLCTAndWET);

		CachedProperty<ZBool> isABNQuotedForLCTAndWETCached;

		ZBool GetIsABNQuotedForLCTAndWET()
		{
			bool result = false;
			foreach (CusEntryLine entryLine in this)
			{
				JobComInvoiceLine invoiceLine = entryLine.RandomLine;
				if (invoiceLine != null && (invoiceLine.AddInfo.ZA_WETQ == "Y" || invoiceLine.AddInfo.ZA_LCTQ == "Y"))
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public ZDecimal TotalLinePayable => Factory.GetValue(ref totalLinePayableCached, delegate
		{
			ZDecimal result = 0m;
			foreach (CusEntryLine entryLine in this)
			{
				result += entryLine.CurrentTotalDutyTax;
			}
			return result;
		});

		CachedProperty<ZDecimal> totalLinePayableCached;

		public bool IsRefundLikely => Factory.GetValue(ref isRefundLikelyCached, GetIsRefundLikely);

		CachedProperty<ZBool> isRefundLikelyCached;

		ZBool GetIsRefundLikely()
		{
			bool result = false;

			if (entryHeader.IsCustomsChargePaid)
			{
				foreach (CusEntryLine entryLine in this)
				{
					if (entryLine.IsLessDutyAndTax)
					{
						result = true;
						break;
					}
				}
			}

			return result;
		}

		public bool HasRefundReason => Factory.GetValue(ref hasRefundReasonCached, delegate
		{
			foreach (ICusEntryLine entryLine in this)
			{
				if (!entryLine.RefundReasonCode.IsEmpty)
				{
					return true;
				}
			}
			return false;
		});

		CachedProperty<ZBool> hasRefundReasonCached;

		public bool AreAllCPQuestionsAnswered => Factory.GetValue(ref cachedAreCPQuestionsAnswered, delegate
		{
			bool result = true;
			foreach (CusEntryLine entryLine in this)
			{
				result &= entryLine.Questions.AreAllCPDecQuestionsAnswered;
				if (!result)
				{
					break;
				}
			}
			return result;
		});

		CachedProperty<ZBool> cachedAreCPQuestionsAnswered;

		public virtual bool HasALineWithPUPIndicator => Factory.GetValue(ref hasALineWithPUPIndicatorCached, delegate
		{
			foreach (CusEntryLine entryLine in this)
			{
				if (entryLine.PUP == "Y")
				{
					return true;
				}
			}
			return false;
		});

		CachedProperty<ZBool> hasALineWithPUPIndicatorCached;
		#endregion

		#region Cloning Stuff
		public CusEntryLineCollection Clone(CusEntryHeader clonedHeader)
		{
			CusEntryLineCollection newCollection = new CusEntryLineCollection(clonedHeader, Factory);
			foreach (CusEntryLine line in this)
			{
				newCollection.Add(line.Clone());
			}
			return newCollection;
		}

		#endregion

		#region Implementation

		protected override IComparer GetComparer()
		{
			return new CusEntryLineComparer();
		}

		class CusEntryLineComparer : IComparer
		{
			#region IComparer Members
			public int Compare(object x, object y)
			{
				CusEntryLine lineX = (CusEntryLine)x;
				CusEntryLine lineY = (CusEntryLine)y;
				int result = lineX.CL_LineNumber.CompareTo(lineY.CL_LineNumber);
				if (result == 0)
				{
					result = lineX.Prefix.CompareTo(lineY.Prefix);
				}
				return result;
			}

			#endregion
		}

		#endregion
	}
}
