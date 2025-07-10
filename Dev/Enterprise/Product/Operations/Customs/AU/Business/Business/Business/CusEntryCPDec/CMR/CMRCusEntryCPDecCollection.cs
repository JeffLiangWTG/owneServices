using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCusEntryCPDecCollection : DependentBusinessObjectCollection<CMRCusEntryCPDec, BusinessObject>
	{
		public CMRCusEntryCPDecCollection(ICPQAAttachee parent)
			: base(parent as BusinessObject)
		{
			this.Parent = parent;
		}

		public CMRCusEntryCPDecCollection(ICPQAAttachee parent, OrgHeader orgParent)
			: base(orgParent)
		{
			this.Parent = parent;
		}

		public bool IsGoodsDeliveredQuestionAnsweredNo => Factory.GetValue(ref isGoodsDeliveredQuestionAnsweredNoCached, delegate
		{
			CMRCusEntryCPDec decQuestion = this.GetQuestionWithID(14);
			return decQuestion != null && decQuestion.IsAnswered && !decQuestion.IsYes;
		});
		CachedProperty<bool> isGoodsDeliveredQuestionAnsweredNoCached;

		public bool AreAllCPDecQuestionsAnswered => Factory.GetValue(ref cachedAreCPQuestionsAnswered, delegate
		{
			bool result = true;
			foreach (CMRCusEntryCPDec question in this)
			{
				if (!question.IsValidationSuspended)
				{
					question.Validation.ValidateON_AnswerCode();
				}
				if (question.IsOptionalQuestion)
				{
					result &= !question.HasNotifications();
				}
				else
				{
					result &= question.IsAnswered && !question.HasNotifications();
				}
				if (!result)
				{
					break;
				}
			}
			return result;
		});
		CachedProperty<bool> cachedAreCPQuestionsAnswered;

		public bool IsAnyCPQuestionAnswered => Factory.GetValue(ref cachedIsAnyCPQuestionAnswered, GetIsAnyCPQuestionAnswered);
		CachedProperty<bool> cachedIsAnyCPQuestionAnswered;

		bool GetIsAnyCPQuestionAnswered()
		{
			bool result = false;
			foreach (CMRCusEntryCPDec question in this)
			{
				if (question.IsAnswered)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public bool HaveDeclarationQuestionsBeenModified
		{
			get
			{
				ICollection<ZInt> questionsToBeCheckedForAmendment = new ZInt[] { 3, 6, 7, 8, 9, 375 };
				foreach (CMRCusEntryCPDec question in this)
				{
					if (questionsToBeCheckedForAmendment.Contains(question.ON_CPDecNum) && question.ON_AnswerCode != (ZString)question.ON_AnswerCodeInfo.OriginalValue)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool HasQuestionWithID(ZInt questionID)
		{
			foreach (CMRCusEntryCPDec cPDec in this)
			{
				if (cPDec.ON_CPDecNum == questionID)
				{
					return true;
				}
			}
			return false;
		}

		public CMRCusEntryCPDec GetQuestionWithID(ZInt questionID)
		{
			foreach (CMRCusEntryCPDec cPDec in this)
			{
				if (cPDec.ON_CPDecNum == questionID)
				{
					return cPDec;
				}
			}
			return null;
		}

		public bool HasQuestionWithID(ZInt questionID, ZDateTime startDate)
		{
			foreach (CMRCusEntryCPDec cPDec in this)
			{
				if (cPDec.ON_CPDecNum == questionID && cPDec.ON_CPDecStartDate.Date == startDate.Date)
				{
					return true;
				}
			}
			return false;
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return Parent.FKColumnInCusEntryCPDecTable; }
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			result.AddToFilter(JoinCondition.And, CusEntryCPDecSchema.ON_JE, SQLComparisonOperator.Equal, null);

			var declaration = (Parent as CusEntryHeader)?.Declaration;

			if (declaration != null && declaration.IsImportCMR && declaration.IsUPEDeclaration && !declaration.IsNonTransportDeclarationType)
			{
				result.AddToFilter(JoinCondition.And, CusEntryCPDecSchema.ON_CPDecNum, SQLComparisonOperator.NotEqual, 3);
				result.AddToFilter(JoinCondition.And, CusEntryCPDecSchema.ON_CPDecNum, SQLComparisonOperator.NotEqual, 375);
			}

			return result;
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			if (IsAttachedToConsolidatedDeclaration)
			{
				bizOAdded.SuspendValidation();
			}
		}

		protected bool IsAttachedToConsolidatedDeclaration
		{
			get
			{
				if (isAttachedToConsolidatedDeclaration == null)
				{
					isAttachedToConsolidatedDeclaration = Parent is ConsolidatedDeclaration
						|| (Parent is CusEntryHeader entryHeader && entryHeader.ConsolidatedDeclaration != null)
						|| (Parent is CusEntryLine entryLine && entryLine.Header?.ConsolidatedDeclaration != null);
				}
				return isAttachedToConsolidatedDeclaration.Value;
			}
		}
		bool? isAttachedToConsolidatedDeclaration;

		protected readonly ICPQAAttachee Parent;
	}
}
