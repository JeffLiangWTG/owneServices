using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusEntryHeaderCollection : CusEntryHeaderCollection<CusEntryHeader>
	{
		public CusEntryHeaderCollection(JobDeclaration parentDeclaration, BusinessObjectFactory factory)
			: base(parentDeclaration, factory)
		{
		}

		#region AreWithdrawalLodgementQuestionsGeneratedAndAnswered
		public override bool AreWithdrawalLodgementQuestionsGeneratedAndAnswered => Factory.GetValue(ref cachedAreWithdrawalLodgementQuestions, GetAreWithdrawalLodgementQuestionsGeneratedAndAnswered);
		CachedProperty<ZBool> cachedAreWithdrawalLodgementQuestions;

		ZBool GetAreWithdrawalLodgementQuestionsGeneratedAndAnswered()
		{
			bool result = true;
			if (!Declaration.IsSAC)
			{
				foreach (CusEntryHeader entryHeader in this)
				{
					if (!entryHeader.Questions.HasQuestionWithID(12) && !entryHeader.Questions.HasQuestionWithID(13) || !entryHeader.Questions.AreAllCPDecQuestionsAnswered)
					{
						result = false;
						break;
					}
				}
			}
			return result;
		}
		#endregion

		#region AreAmendmentLodgementQuestionsGeneratedAndAnswered

		public override bool AreAmendmentLodgementQuestionsGeneratedAndAnswered => Factory.GetValue(ref cachedAreAmendmentLodgementGeneratedAndAnswered, GetAreAmendmentLodgementQuestionsGeneratedAndAnswered);

		CachedProperty<ZBool> cachedAreAmendmentLodgementGeneratedAndAnswered;

		ZBool GetAreAmendmentLodgementQuestionsGeneratedAndAnswered()
		{
			bool result = true;
			if (!Declaration.IsSAC)
			{
				foreach (CusEntryHeader entryHeader in this)
				{
					if (!entryHeader.Questions.HasQuestionWithID(11) || !entryHeader.Questions.AreAllCPDecQuestionsAnswered)
					{
						result = false;
						break;
					}
				}
			}
			return result;
		}

		#endregion

		#region IsAnyLodgementQuestion7AnsweredNo

		public override bool IsAnyLodgementQuestion7AnsweredNo => Factory.GetValue(ref cachedIsAnyLodgementQuestion7AnsweredNo, GetIsAnyLodgementQuestion7AnsweredNo);
		CachedProperty<ZBool> cachedIsAnyLodgementQuestion7AnsweredNo;

		ZBool GetIsAnyLodgementQuestion7AnsweredNo()
		{
			bool result = false;
			if (!Declaration.IsSAC)
			{
				foreach (CusEntryHeader entryHeader in this)
				{
					CMRCusEntryCPDec question = entryHeader.Questions.GetQuestionWithID(7);
					if (question != null && question.IsAnswered && !question.IsYes)
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		#endregion

		#region DoAllEntriesHaveEntryNumber
		public override bool DoAllEntriesHaveEntryNumber => Factory.GetValue(ref cachedDoAllEntriesHaveEntryNumber, delegate
		{
			foreach (CusEntryHeader entryHeader in this)
			{
				if (entryHeader.EntryNumber.IsEmpty)
				{
					return false;
				}
			}
			return true;
		});

		CachedProperty<ZBool> cachedDoAllEntriesHaveEntryNumber;

		#endregion

		#region AreAllCPQuestionsAnswered
		public override bool AreAllCPQuestionsAnswered => Factory.GetValue(ref cachedAreCPQuestionsAnswered, GetAreAllCPQuestionsAnswered);

		CachedProperty<ZBool> cachedAreCPQuestionsAnswered;

		ZBool GetAreAllCPQuestionsAnswered()
		{
			bool result = true;
			foreach (CusEntryHeader entryHeader in this)
			{
				result &= entryHeader.Questions.AreAllCPDecQuestionsAnswered && entryHeader.MergedLines.AreAllCPQuestionsAnswered;
				if (!result)
				{
					break;
				}
			}
			return result;
		}

		#endregion

		#region Money Amount

		/// <summary>
		/// CMR, total payable for amendment should be the additional amount
		/// </summary>
		public override ZDecimal TotalAmountPayableForThisSession
		{
			get
			{
				bool isImportCMR = Declaration.IsImportCMR;
				ZDecimal result = 0;
				foreach (CusEntryHeader entryHeader in this)
				{
					if (isImportCMR)
					{
						result += entryHeader.TotalAmountPayableForThisSession;
					}
					else
					{
						result += entryHeader.TotalAmountPayable;
					}
				}
				return result;
			}
		}

		public ZDecimal TotalAmountPayable
		{
			get
			{
				ZDecimal result = 0;
				foreach (CusEntryHeader entryHeader in this)
				{
					result += entryHeader.TotalAmountPayable;
				}
				return result;
			}
		}

		public ZDecimal DutyAmount
		{
			get
			{
				ZDecimal result = 0;
				foreach (CusEntryHeader entryHeader in this)
				{
					result += entryHeader.DutyAmount;
				}
				return result;
			}
		}

		public ZDecimal LCTAmount
		{
			get
			{
				ZDecimal result = 0;
				foreach (CusEntryHeader entryHeader in this)
				{
					result += entryHeader.LCTAmount;
				}
				return result;
			}
		}

		public ZDecimal WETAmount
		{
			get
			{
				ZDecimal result = 0;
				foreach (CusEntryHeader entryHeader in this)
				{
					result += entryHeader.WETAmount;
				}
				return result;
			}
		}

		public ZDecimal WoodLevyAmount
		{
			get
			{
				ZDecimal result = 0;
				foreach (CusEntryHeader entryHeader in this)
				{
					result += entryHeader.WoodLevy;
				}
				return result;
			}
		}

		public ZDecimal EntryFeeAmount
		{
			get
			{
				ZDecimal result = 0;
				foreach (CusEntryHeader entryHeader in this)
				{
					result += entryHeader.EntryFee;
				}
				return result;
			}
		}

		public ZDecimal MessageFeeAmount
		{
			get
			{
				ZDecimal result = 0;
				foreach (CusEntryHeader entryHeader in this)
				{
					result += entryHeader.MessageFee;
				}
				return result;
			}
		}

		public ZDecimal TradegateGSTAmount
		{
			get
			{
				ZDecimal result = 0;
				foreach (CusEntryHeader entryHeader in this)
				{
					result += entryHeader.TradegateGST;
				}
				return result;
			}
		}

		public ZDecimal ScreenFreeAmount
		{
			get
			{
				ZDecimal result = 0;
				foreach (CusEntryHeader entryHeader in this)
				{
					result += entryHeader.ScreenFreeCharge;
				}
				return result;
			}
		}

		public ZDecimal OtherChargesAmount
		{
			get
			{
				ZDecimal result = 0;
				foreach (CusEntryHeader entryHeader in this)
				{
					result += entryHeader.OtherEntryCharge;
				}
				return result;
			}
		}

		#endregion

		public override bool HasEntryWithPostLodgeStatus => Factory.GetValue(ref hasEntryWithPostLodgeStatusCached, delegate
		{
			foreach (CusEntryHeader entry in this)
			{
				if (entry.IsStatusPostLodge)
				{
					return true;
				}
			}
			return false;
		});

		CachedProperty<bool> hasEntryWithPostLodgeStatusCached;

		public override bool StatusNeedsRecalculation
		{
			get
			{
				foreach (CusEntryHeader entryHeader in this)
				{
					if (((IStatusNeedsRecalculationProvider)entryHeader).StatusNeedsRecalculation)
					{
						return true;
					}
				}
				return false;
			}
		}

		protected override System.Collections.IComparer GetCustomsEntryHeaderComparer()
		{
			return new CusEntryHeader.CusEntryComparer();
		}

		new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}
	}
}
