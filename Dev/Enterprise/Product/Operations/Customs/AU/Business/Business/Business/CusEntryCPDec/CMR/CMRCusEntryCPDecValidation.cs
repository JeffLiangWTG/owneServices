using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCusEntryCPDecValidation : CusEntryCPDecValidation
	{
		public CMRCusEntryCPDecValidation(CMRCusEntryCPDec cPDecQuestion)
			: base(cPDecQuestion)
		{
			jobDeclaration = cPDecQuestion.Declaration;
		}

		#region Implementation

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateQuestionID();
		}

		protected override void CheckON_AnswerCode()
		{
			base.CheckON_AnswerCode();

			if (jobDeclaration == null || (jobDeclaration != null && !jobDeclaration.IsWarehousedByExternalAgent && !jobDeclaration.IsImportByExternalBroker))
			{
				ListValidation.ErrorIfInvalidCode(CPDecQuestion.ON_AnswerCodeInfo, CPDecQuestion.Lookups.ON_AnswerCode_List);
				if (CPDecQuestion.IsAcknowledge && !CPDecQuestion.IsOptionalQuestion && !CPDecQuestion.IsYes)
				{
					CPDecQuestion.ON_AnswerCodeInfo.AddMessageError("This is a required lodgement declaration (not a question), you must acknowledge this declaration by entering YES, otherwise your entry/amendment will be rejected.");
				}
				else if (!CPDecQuestion.IsAnswered && !CPDecQuestion.IsOptionalQuestion)
				{
					CPDecQuestion.ON_AnswerCodeInfo.AddMessageError("You have not answered this Declaration or CP Dec question.");
				}
				else if (CPDecQuestion.IsAnswered && CPDecQuestion.IsOptionalQuestion && CPDecQuestion.IsDependentQuestion)
				{
					CPDecQuestion.ON_AnswerCodeInfo.AddMessageError("You have answered this Declaration or CP Dec question but the current state of the entry indicates that it is not required, this will probably be rejected by Customs.");
				}

				if (jobDeclaration != null)
				{
					if (CPDecQuestion.ON_CPDecNum == 19)
					{
						if (CPDecQuestion.IsYes && jobDeclaration.AddInfo.ZA_AQISInspectLocation_Hidden.IsEmpty)
						{
							CPDecQuestion.ON_AnswerCodeInfo.AddMessageError("You want to refer these goods to Quarantine, but you have not entered Quarantine Inspection location in Misc Option tab.");
						}
					}
					else if (CPDecQuestion.ON_CPDecNum == 18)
					{
						if (!CPDecQuestion.IsYes && jobDeclaration.IsSACWithLines)
						{
							CPDecQuestion.ON_AnswerCodeInfo.AddMessageError("You should answer YES to this question as you already selected 'SAC With Lines(Tobacco or Alcohol)' on the front screen of the declaration.");
						}
					}
					else if (CPDecQuestion.ON_CPDecNum == 14)
					{
						if (CPDecQuestion.IsYes)
						{
							CPDecQuestion.ON_AnswerCodeInfo.AddWarning("You cannot withdraw this entry if you answer YES to this question.");
						}
					}
					else if (CPDecQuestion.ON_CPDecNum == 7 && !CPDecQuestion.IsOptionalQuestion)
					{
						if (!CPDecQuestion.IsYes && jobDeclaration.AQISConcernTypes.Count == 0)
						{
							CPDecQuestion.ON_AnswerCodeInfo.AddMessageError("As you have answered 'No' to this question, at least one Quarantine Concern Type is required.");
						}
					}
					else if (CPDecQuestion.ON_CPDecNum == 15)
					{
						if (CPDecQuestion.ON_AnswerCode == Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec.Answers.NO)
						{
							CPDecQuestion.ON_AnswerCodeInfo.AddWarning("If you answer 'No' to this declaration question, Australian Customs will not let you change the answer to 'Yes' on any subsequent amendment.");
						}
					}
					if (CPDecQuestion.AQISContainerQuestions.Contains(CPDecQuestion.ON_CPDecNum) && !CPDecQuestion.ON_AnswerCode.IsEmpty && CPDecQuestion.IsOptionalQuestion)
					{
						CPDecQuestion.ON_AnswerCodeInfo.AddMessageError("Answering this question, with Question 14 = Y, will result in this amendment being rejected by Customs.");
					}
				}
			}
		}

		protected override void CheckON_Permit()
		{
			base.CheckON_Permit();

			if (CPDecQuestion.LineAttachee != null && !CPDecQuestion.ON_Permit.IsEmpty)
			{
				if (CPDecQuestion.LineAttachee.IsRiskCalculatedFromTariff)
				{
					if (!CPDecQuestion.IsPermitRelevant)
					{
						CPDecQuestion.ON_PermitInfo.AddMessageError(PermitNotRelevant);
					}
				}
				else
				{
					if (CPDecQuestion.LodgementQuestion != null)
					{
						CMRCommunityProtectionRisk[] risks = CMRCommunityProtectionRisk.Load(CPDecQuestion.LodgementQuestion);
						if (risks.Length == 0)
						{
							CPDecQuestion.ON_PermitInfo.AddWarning(PermitNotRelevant);
						}
						else
						{
							bool isSameIndicatorForAllRisks = true;
							ZBool permitIndicator = risks[0].CK_PermitApplicationIndicator;
							foreach (CMRCommunityProtectionRisk risk in risks)
							{
								if (risk.CK_PermitApplicationIndicator != permitIndicator)
								{
									isSameIndicatorForAllRisks = false;
									break;
								}
							}
							if (isSameIndicatorForAllRisks && !permitIndicator)
							{
								CPDecQuestion.ON_PermitInfo.AddWarning(PermitNotRelevant);
							}
						}
					}
				}
			}
		}

		public void ValidateQuestionID()
		{
			ValidateCalculatedProperty(CPDecQuestion.QuestionIDInfo);
		}

		protected void CheckQuestionID()
		{
			if (CPDecQuestion.LineAttachee != null && !CPDecQuestion.LineAttachee.IsRiskCalculatedFromTariff)
			{
				if (CPDecQuestion.LodgementQuestion == null || CPDecQuestion.LodgementQuestion.CQ_LodgementQuestionType == CMRCusEntryCPDec.LodgementQuestionTypes.GeneralLodgementQuestion)
				{
					CPDecQuestion.QuestionIDInfo.AddError(InvalidLodgementQuestion);
				}
				else
				{
					var key = new LineDefaultKey(CPDecQuestion.ON_CPDecNum, CPDecQuestion.ON_CPDecStartDate);
					foreach (CMRCusEntryCPDec cPDec in CPDecQuestion.LineAttachee.Questions)
					{
						if (cPDec.PK != CPDecQuestion.PK && cPDec.ON_CPDecNum == key.QuestionID && cPDec.ON_CPDecStartDate == key.StartDate)
						{
							CPDecQuestion.QuestionIDInfo.AddError(DuplicateLodgementQuestion);
						}
					}
				}
			}
		}

		protected override void CheckON_CPDecNum()
		{
			base.CheckON_CPDecNum();
			if (CPDecQuestion.EntryLine != null && CPDecQuestion.RiskId == 0)
			{
				CPDecQuestion.ON_CPDecNumInfo.AddMessageError(NoRiskIDForCPDecQuestion);
			}
		}

		public const string NoRiskIDForCPDecQuestion = "There is no Risk ID associated with this question number and this question will be disregarded in the message as there is no Risk ID.\r\nPlease regenerate questions by clicking Brokerage > Regenerate Declaration Questions before sending a message.";
		public const string PermitNotRelevant = "According to reference files, this question does not require Permit/Licence to be entered.";
		public const string InvalidLodgementQuestion = "Invalid Lodgement question.";
		public const string DuplicateLodgementQuestion = "Duplicate lodgement question.";

		readonly JobDeclaration jobDeclaration;

		CMRCusEntryCPDec CPDecQuestion
		{
			get { return (CMRCusEntryCPDec)base.Parent; }
		}

		#endregion
	}
}
