using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRLodgementQuestionGenerator
	{
		public CMRLodgementQuestionGenerator(ICPQAAttacheeHolder declaration)
		{
			this.declaration = declaration;
		}

		public void GenerateQuestions(bool isWithdrawal)
		{
			foreach (ICPQAHeaderAttachee entryHeader in declaration.Headers)
			{
				MessageTypesForCPQAGenerator messageType;
				if (isWithdrawal)
				{
					messageType = MessageTypesForCPQAGenerator.Withdraw;
				}
				else
				{
					messageType = entryHeader.IsStatusPostLodge ? MessageTypesForCPQAGenerator.Amend : MessageTypesForCPQAGenerator.Original;
				}

				GenerateQuestions(entryHeader, messageType);
			}
		}

		#region Implementation

		readonly ICPQAAttacheeHolder declaration;

		ZDecimal Deminimus
		{
			get
			{
				var declaration = this.declaration as JobDeclaration;
				var factory = declaration == null ? new BusinessObjectFactory() : declaration.Factory;
				return UniversalReferenceHelper.GetDeminimus(factory);
			}
		}

		internal void GenerateQuestions(ICPQAHeaderAttachee entryHeader, MessageTypesForCPQAGenerator messageType)
		{
			CMRLodgementQuestion[] lodgementQuesions = GetLodgementQuestions(entryHeader, messageType);
			GenerateQuestions(entryHeader, lodgementQuesions, messageType);
		}

		internal void GenerateQuestions(ICPQAHeaderAttachee entryHeader, CMRLodgementQuestion[] lodgementQuesions, MessageTypesForCPQAGenerator messageType)
		{
			var uniqueQuestions = new ArrayList();
			var irrelevantQuestions = new List<CMRCusEntryCPDec>(entryHeader.Questions.Cast<CMRCusEntryCPDec>());

			var declaration = (entryHeader as CusEntryHeader)?.Declaration;

			var excludeQuestionNums = declaration != null && declaration.IsImportCMR && declaration.IsUPEDeclaration && !declaration.IsNonTransportDeclarationType
				? new ZInt[] { 3, 375 }
				: Array.Empty<ZInt>();

			foreach (CMRLodgementQuestion lodgementQ in lodgementQuesions)
			{
				if (!uniqueQuestions.Contains(lodgementQ.Key) && excludeQuestionNums.All(c => c != lodgementQ.CQ_LodgementQuestionIdentifier))
				{
					var question = irrelevantQuestions.Find(x => x.ON_CPDecNum == lodgementQ.CQ_LodgementQuestionIdentifier && x.ON_CPDecStartDate == lodgementQ.CQ_LodgementQuestionStartDate);
					if (question != null)
					{
						question.ResetCachedValues();
						irrelevantQuestions.Remove(question);
					}
					else
					{
						question = entryHeader.Questions.AddNew();
						question.ON_CPDecNum = lodgementQ.CQ_LodgementQuestionIdentifier;
						question.ON_CPDecStartDate = lodgementQ.CQ_LodgementQuestionStartDate;
						question.ON_CPDecEndDate = lodgementQ.CQ_LodgementQuestionEndDate;
						if (question.ON_CPDecNum == 18 && entryHeader.LodgementQuestionKey.IsSACWithLine)
						{
							question.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;
						}
					}

					question.GeneratorMessageType = messageType;
					uniqueQuestions.Add(lodgementQ.Key);
				}
			}

			irrelevantQuestions.Where(x => !IsQuestionToRemain(x)).ToList().ForEach(x => x.Delete());
		}

		bool IsQuestionToRemain(CMRCusEntryCPDec question)
		{
			if (question.ON_CPDecNum == AUCustomsDataRegistry.Instance.LowValueSecurityDeclarationQuestion.Value)
			{
				return true;
			}
			return false;
		}

		CMRLodgementQuestion[] GetLodgementQuestions(ICPQAHeaderAttachee entryHeader, MessageTypesForCPQAGenerator messageType)
		{
			ArrayList decQuestions = null;
			LodgementQuestionKeys lodgementQuestionKey = entryHeader.LodgementQuestionKey;
			if (lodgementQuestionKey.IsSAC)
			{
				decQuestions = GetSACQuestionIDs(lodgementQuestionKey, messageType);
			}
			else
			{
				decQuestions = GetPreLodgeOrLodgeQuestionIDs(lodgementQuestionKey, messageType);
			}

			return CMRLodgementQuestion.Load(entryHeader, (ZInt[])decQuestions.ToArray(typeof(ZInt)));
		}

		internal ArrayList GetPreLodgeOrLodgeQuestionIDs(LodgementQuestionKeys keys, MessageTypesForCPQAGenerator messageType)
		{
			ArrayList questionIDs = new ArrayList();
			if (messageType != MessageTypesForCPQAGenerator.Withdraw)
			{
				if (messageType == MessageTypesForCPQAGenerator.Amend)
				{
					questionIDs.Add(new ZInt(11));
					questionIDs.Add(new ZInt(4));
					questionIDs.Add(new ZInt(14));
					if (!keys.IsNature20)
					{
						questionIDs.Add(new ZInt(15));
					}

					questionIDs.Add(new ZInt(10));
					questionIDs.Add(new ZInt(282));
					questionIDs.Add(new ZInt(326));
				}
				else
				{
					questionIDs.Add(new ZInt(1));
				}

				if (keys.IsPaidUnderProtest)
				{
					questionIDs.Add(new ZInt(2));
				}

				if (keys.IsABNQuotedForLCTAndWET)
				{
					questionIDs.Add(new ZInt(5));
				}

				if (!keys.IsNature20 && !keys.IsNature30 && keys.TotalCustomsValue <= Deminimus)
				{
					questionIDs.Add(new ZInt(3));
					questionIDs.Add(new ZInt(375));
				}

				if (keys.IsUPEDeclaration)
				{
					if (Env.Registry.CMRTestMode)
					{
						questionIDs.Add(new ZInt(394));
						questionIDs.Add(new ZInt(395));
						questionIDs.Add(new ZInt(396));
						questionIDs.Add(new ZInt(397));
						questionIDs.Add(new ZInt(398));
						questionIDs.Add(new ZInt(399));
						questionIDs.Add(new ZInt(400));
					}
					else
					{
						questionIDs.Add(new ZInt(502));
						questionIDs.Add(new ZInt(503));
						questionIDs.Add(new ZInt(504));
						questionIDs.Add(new ZInt(505));
						questionIDs.Add(new ZInt(506));
						questionIDs.Add(new ZInt(507));
						questionIDs.Add(new ZInt(508));
					}
				}

				if (keys.IsSOFADeclaration)
				{
					if (Env.Registry.CMRTestMode)
					{
						questionIDs.Add(new ZInt(402));
					}
					else
					{
						questionIDs.Add(new ZInt(533));
					}
				}

				if (!keys.IsNature30)
				{
					if (keys.IsSea)
					{
						if (keys.HasFCLOrFCXLines)
						{
							questionIDs.Add(new ZInt(6));
							questionIDs.Add(new ZInt(7));
						}
						if (keys.HasLCLLines)
						{
							questionIDs.Add(new ZInt(8));
							questionIDs.Add(new ZInt(9));
						}
					}
				}

				if (AUCustomsDataRegistry.Instance.LowValueSecurityImplementationDate.Value <= ZDateTime.Today.ToDateTime()
					&& keys.HasSecurityTreatment)
				{
					questionIDs.Add(new ZInt(AUCustomsDataRegistry.Instance.LowValueSecurityDeclarationQuestion.Value));
				}

				if (keys.HasRemissionOnBunkerFuels)
				{
					questionIDs.Add(new ZInt(797));
					questionIDs.Add(new ZInt(798));
					questionIDs.Add(new ZInt(799));
					questionIDs.Add(new ZInt(800));
				}
			}
			else
			{
				questionIDs.Add(new ZInt(12));
				questionIDs.Add(new ZInt(14));
				if (!keys.IsNature20)
				{
					questionIDs.Add(new ZInt(15));
				}

				if (!keys.IsPaid)
				{
					questionIDs.Add(new ZInt(13));
				}

				if (keys.TotalCustomsValue > Deminimus)
				{
					questionIDs.Add(new ZInt(10));
				}
			}

			return questionIDs;
		}

		internal ArrayList GetSACQuestionIDs(LodgementQuestionKeys keys, MessageTypesForCPQAGenerator messageType)
		{
			ArrayList result = new ArrayList();
			if (keys.IsSAC)
			{
				if (messageType != MessageTypesForCPQAGenerator.Withdraw)
				{
					result.Add(new ZInt(16));
					result.Add(new ZInt(17));
					result.Add(new ZInt(18));
					result.Add(new ZInt(19));
				}
				else
				{
					result.Add(new ZInt(12));
					if (!keys.IsPaid)
					{
						result.Add(new ZInt(13));
					}

					result.Add(new ZInt(14));
					if (keys.IsSACWithLine)
					{
						if (!keys.IsNature20)
						{
							result.Add(new ZInt(15));
						}

						result.Add(new ZInt(10));
					}
				}
			}

			return result;
		}
		#endregion
	}
}
