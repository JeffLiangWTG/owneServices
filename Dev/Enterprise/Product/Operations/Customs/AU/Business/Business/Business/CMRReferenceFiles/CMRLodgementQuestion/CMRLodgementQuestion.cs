using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public enum AnswerType { ACK, YesOrNo }

	[CodeProperty("QuestionIDAndStartDate"), DescriptionProperty("QuestionIDAndStartDate")]
	public class CMRLodgementQuestion : AutoCMRLodgementQuestion, ICodeDescription
	{
		public new class Schema : AutoCMRLodgementQuestion.Schema
		{
			public const string QuestionIDAndStartDate = "QuestionIDAndStartDate";
		}

		public CMRLodgementQuestion(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRLodgementQuestion New(BusinessObjectFactory factory)
		{
			return factory.New<CMRLodgementQuestion>();
		}

		public static CMRLodgementQuestion[] Load(ICPQAAttachee attachee, ZInt[] questionIDs)
		{
			if (attachee == null)
			{
				throw new ArgumentNullException(nameof(attachee));
			}

			return (CMRLodgementQuestion[])attachee.Factory.Load(typeof(CMRLodgementQuestion), GetLodgementQuestionFilter(attachee, questionIDs));
		}

		public static CMRLodgementQuestion Load(BusinessObjectFactory factory, ZInt questionID, ZDateTime startDate, SQLComparisonOperator startDateOperator)
		{
			CMRLodgementQuestion result = null;
			if (startDate.IsValid)
			{
				ZQuery filter = new ZQuery(CMRLodgementQuestionSchema.CQ_LodgementQuestionIdentifier, questionID);
				filter.AddToFilter(JoinCondition.And, CMRLodgementQuestionSchema.CQ_LodgementQuestionStartDate, startDateOperator, startDate);
				result = factory.LoadTop1<CMRLodgementQuestion>(filter);
			}

			return result;
		}

		public static ZQuery GetLodgementQuestionFilter(ICPQAAttachee attachee, ZInt[] questionIDs)
		{
			ZQuery result = new ZQuery();
			if (questionIDs.Length > 0 && attachee.SelectionDate.IsValid)
			{
				result.AddToFilter(CMRLodgementQuestionSchema.CQ_LodgementQuestionStartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, attachee.SelectionDate);
				ZQuery endDateFilter = new ZQuery(CMRLodgementQuestionSchema.CQ_LodgementQuestionEndDate, null);
				endDateFilter.AddToFilter(JoinCondition.Or, CMRLodgementQuestionSchema.CQ_LodgementQuestionEndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, attachee.SelectionDate);
				result.AddToFilter(endDateFilter, JoinCondition.And);
				ZQuery iDFilter = new ZQuery(CMRLodgementQuestionSchema.CQ_LodgementQuestionIdentifier, questionIDs);
				result.AddToFilter(iDFilter, JoinCondition.And);
			}
			else
			{
				result.AddToFilter(CMRLodgementQuestionSchema.PK, SQLComparisonOperator.Equal, ZGuid.Empty);
			}

			return result;
		}

		public string Key
		{
			get
			{
				return CQ_LodgementQuestionIdentifier
					+ CQ_LodgementQuestionStartDate.ToShortDateString();
			}
		}

		public ZInt GetRiskIndentifier(CMRCusEntryCPDec cPQuestion)
		{
			CMRCommunityProtectionRisk risk = GetRisk(cPQuestion.LineAttachee);
			return risk != null ? risk.CK_Identifier : ZInt.Zero;
		}

		public ZBool IsPermitRelevant(CMRCusEntryCPDec cPQuestion)
		{
			CMRCommunityProtectionRisk risk = GetRisk(cPQuestion.LineAttachee);
			return risk != null && risk.CK_PermitApplicationIndicator;
		}

		CMRCommunityProtectionRisk GetRisk(ICPQALineAttachee lineAttachee)
		{
			if (lineAttachee != null)
			{
				ZDateTime selectionDate = lineAttachee.SelectionDate;
				CMRCommunityProtectionRisk[] risks = CMRCommunityProtectionRisk.Load(lineAttachee);
				foreach (CMRCommunityProtectionRisk risk in risks)
				{
					if (risk.CK_LodgementQuestionIdentifier == CQ_LodgementQuestionIdentifier
						&& risk.CK_StartDate <= selectionDate && (risk.CK_EndDate.IsEmpty || risk.CK_EndDate >= selectionDate))
					{
						return risk;
					}
				}
			}

			return null;
		}

		#region ICodeDescription Members

		object ICodeDescription.PK
		{
			get { return PK; }
		}

		string ICodeDescription.Code
		{
			get { return QuestionIDAndStartDate; }
		}

		string ICodeDescription.Description
		{
			get { return "UNUSED"; }
		}

		#endregion

		#region QuestionIDAndStartDate

		public ZString QuestionIDAndStartDate
		{
			get
			{
				ZString result = CQ_LodgementQuestionIdentifier.ToString();
				if (!CQ_LodgementQuestionStartDate.IsEmpty)
				{
					result += "-" + CQ_LodgementQuestionStartDate.ToShortDateString();
				}

				return result;
			}
		}

		#endregion
	}
}
