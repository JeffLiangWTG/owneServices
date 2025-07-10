using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// This will delete existing questions and generate new questions for CusEntryLine
	/// </summary>
	public class CMRCPDecQuestionGenerator
	{
		public CMRCPDecQuestionGenerator(ICPQAAttacheeHolder declaration)
		{
			this.declaration = declaration;
		}

		public void GenerateQuestions()
		{
			foreach (LineWithQuestions line in LineAndQuestions)
			{
				GenerateQuestions(line);
			}
		}

		#region LineAndQuestions

		IEnumerable<LineWithQuestions> LineAndQuestions
		{
			get
			{
				var result = new List<LineWithQuestions>();

				foreach (ICPQALineAttachee line in declaration.Lines)
				{
					var risks = CMRCommunityProtectionRisk.Load(line);

					var lineAndQuestions = new LineWithQuestions();
					lineAndQuestions.Questions = GetUniqueLodgementQuestionIDs(risks);
					lineAndQuestions.Line = line;
					result.Add(lineAndQuestions);

					line.Factory.AddFetchHint(CMRStatisticalClassificationPeriodCharacteristicSchema.SH_StatisticalClassificationPeriodSnapshotTariffClassificationNumber, line.CPQuestionKey.TariffNumber);
					line.Factory.AddFetchHint(CMRTariffRatePeriodSnapshotSchema.TT_TariffClassificationNumber, line.CPQuestionKey.TariffNumber);
					line.Factory.AddFetchHint(CMRLodgementQuestionSchema.Instance, CMRLodgementQuestion.GetLodgementQuestionFilter(lineAndQuestions.Line, lineAndQuestions.Questions));
				}

				return result;
			}
		}

		#endregion

		#region Implementation

		readonly ICPQAAttacheeHolder declaration;

		void GenerateQuestions(LineWithQuestions lineAndQuestions)
		{
			GenerateQuestionForLoadedRisks(lineAndQuestions.Line, lineAndQuestions.Questions);
		}

		IEnumerable<CMRCusEntryCPDec> AddEndDateToExistingQuestionsAndReturnInvalidQuestions(ICPQALineAttachee line, CMRLodgementQuestion[] lodgmentQuestions)
		{
			foreach (CMRCusEntryCPDec question in line.Questions.ToArray())
			{
				bool questionValid = false;
				foreach (CMRLodgementQuestion lodgementQuestion in lodgmentQuestions)
				{
					if (lodgementQuestion.CQ_LodgementQuestionIdentifier == question.ON_CPDecNum)
					{
						questionValid = AddEndDate(line, question);
						break;
					}
				}
				if (!questionValid)
				{
					yield return question;
				}
			}
		}

		bool AddEndDate(ICPQALineAttachee line, CMRCusEntryCPDec question)
		{
			var filter = new ZQuery(CMRLodgementQuestionSchema.CQ_LodgementQuestionIdentifier, question.ON_CPDecNum);
			filter.AddToFilter(CMRLodgementQuestionSchema.CQ_LodgementQuestionStartDate, SQLComparisonOperator.EqualToDatePartOnly, question.ON_CPDecStartDate);
			var lodgementQuestion = line.Factory.LoadTop1<CMRLodgementQuestion>(filter);
			if (lodgementQuestion != null)
			{
				question.ON_CPDecEndDate = lodgementQuestion.CQ_LodgementQuestionEndDate;
				return true;
			}
			return false;
		}

		internal ZInt[] GetUniqueLodgementQuestionIDs(CMRCommunityProtectionRisk[] risks)
		{
			ArrayList result = new ArrayList();
			foreach (CMRCommunityProtectionRisk risk in risks)
			{
				if (!result.Contains(risk.CK_LodgementQuestionIdentifier))
				{
					result.Add(risk.CK_LodgementQuestionIdentifier);
				}
			}
			return (ZInt[])result.ToArray(typeof(ZInt));
		}

		internal void GenerateQuestionForLoadedRisks(ICPQALineAttachee entryLine, ZInt[] questionIDs)
		{
			CMRLodgementQuestion[] lodgmentQuestions = CMRLodgementQuestion.Load(entryLine, questionIDs);

			var irrelevantQuestions = new List<CMRCusEntryCPDec>();

			if (entryLine.IsRiskHistorySupported)
			{
				irrelevantQuestions.AddRange(AddEndDateToExistingQuestionsAndReturnInvalidQuestions(entryLine, lodgmentQuestions));
			}
			else
			{
				irrelevantQuestions.AddRange(entryLine.Questions.Cast<CMRCusEntryCPDec>());
				irrelevantQuestions.ForEach(x => x.ClearAnswerAndPermit());
			}

			if (lodgmentQuestions.Length > 0)
			{
				var uniqueQuestions = new List<string>();
				foreach (CMRLodgementQuestion lodgementQ in lodgmentQuestions)
				{
					if (!uniqueQuestions.Contains(lodgementQ.Key))
					{
						CachedAnswer cachedAnswer = null;
						if (declaration.CachedQuestions != null)
						{
							var cusEntryLine = entryLine as CusEntryLine;
							if (cusEntryLine != null)
							{
								var key = new LineDefaultKey(lodgementQ.CQ_LodgementQuestionIdentifier, lodgementQ.CQ_LodgementQuestionStartDate);
								cachedAnswer = declaration.CachedQuestions.GetCachedQuestionWithKey(cusEntryLine, key);
							}
						}

						if (!entryLine.IsRiskHistorySupported || !entryLine.Questions.HasQuestionWithID(lodgementQ.CQ_LodgementQuestionIdentifier, lodgementQ.CQ_LodgementQuestionStartDate))
						{
							var cPDecQuestion = entryLine.Questions.Cast<CMRCusEntryCPDec>().FirstOrDefault(x => x.ON_CPDecNum == lodgementQ.CQ_LodgementQuestionIdentifier && x.ON_CPDecStartDate == lodgementQ.CQ_LodgementQuestionStartDate);

							if (cPDecQuestion != null)
							{
								irrelevantQuestions.Remove(cPDecQuestion);
							}
							else
							{
								cPDecQuestion = entryLine.Questions.AddNew();
								cPDecQuestion.ON_CPDecNum = lodgementQ.CQ_LodgementQuestionIdentifier;
								cPDecQuestion.ON_CPDecStartDate = lodgementQ.CQ_LodgementQuestionStartDate;
								cPDecQuestion.ON_CPDecEndDate = lodgementQ.CQ_LodgementQuestionEndDate;
							}

							if (cachedAnswer != null)
							{
								cPDecQuestion.ON_AnswerCode = cachedAnswer.Answer;
								cPDecQuestion.ON_Permit = cachedAnswer.Permit;
							}
							else
							{
								var defaultAnswer = entryLine.DefaultUniqueQuestions.GetDefaultAnswer(new LineDefaultKey(cPDecQuestion.ON_CPDecNum, cPDecQuestion.ON_CPDecStartDate));
								if (defaultAnswer.answer != null)
								{
									cPDecQuestion.ON_AnswerCode = defaultAnswer.answer.AnswerCode;
									cPDecQuestion.ON_Permit = defaultAnswer.answer.Permit;
									if (!string.IsNullOrEmpty(defaultAnswer.warning))
									{
										cPDecQuestion.AddRowWarning(defaultAnswer.warning);
									}
								}
							}

							uniqueQuestions.Add(lodgementQ.Key);
						}
					}
				}
			}

			irrelevantQuestions.ForEach(x => x.Delete());
		}

		struct LineWithQuestions
		{
			public ICPQALineAttachee Line;
			public ZInt[] Questions;
		}

		#endregion
	}
}
