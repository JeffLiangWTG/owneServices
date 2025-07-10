using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCusEntryCPDec : BaseCusEntryCPDec
	{
		#pragma warning disable IDE0001 // Prevent simplification to base class
		public new class Schema : BaseCusEntryCPDec.Schema
		#pragma warning restore IDE0001 // Prevent simplification to base class
		{
			public const string QuestionID = "QuestionID";
		}

		public static class Answers
		{
			public const string YES = "Y";
			public const string NO = "N";
			public const string Ambiguous = "?";
		}

		public static class LodgementQuestionTypes
		{
			public const string GeneralLodgementQuestion = "GLQ";
			public const string CommunityProtectionQuestion = "CPQ";
		}

		public CMRCusEntryCPDec(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		#region Boolean Properties

		public override bool SupportsNotes
		{
			get { return false; }
		}

		public bool IsAcknowledge
		{
			get { return AcknowledgeQuestions.Contains(ON_CPDecNum); }
		}

		public bool IsAnswered
		{
			get { return !ON_AnswerCode.IsEmpty; }
		}

		public bool IsYes
		{
			get { return ON_AnswerCode == Answers.YES; }
		}

		public ZInt RiskId
		{
			get { return LodgementQuestion == null ? ZInt.Zero : LodgementQuestion.GetRiskIndentifier(this); }
		}

		public ZString QuestionType
		{
			get { return LodgementQuestion == null ? ZString.Empty : LodgementQuestion.CQ_LodgementQuestionName; }
		}

		public ZPropertyInfo QuestionTypeInfo
		{
			get { return GetZPropertyInfo(nameof(QuestionType)); }
		}

		public ZString EffectiveDefaultAnswer { get; set; }

		public
#if DEBUG
 virtual
#endif
 ZBool IsOptionalQuestion
		{
			get
			{
				if (!fIsOptionalQuestionSet)
				{
					if (EntryLine == null && ConditionalRules != null)
					{
						fIsOptionalQuestion = ConditionalRules.IsConditional;
					}
					else
					{
						fIsOptionalQuestion = false;
					}
					fIsOptionalQuestionSet = true;
				}
				return fIsOptionalQuestion;
			}
		}
		bool fIsOptionalQuestion;
		bool fIsOptionalQuestionSet;

		public ZPropertyInfo IsOptionalQuestionInfo
		{
			get { return GetZPropertyInfo(nameof(IsOptionalQuestion)); }
		}

		public void ResetCachedValues()
		{
			fIsOptionalQuestionSet = false;
			if (ConditionalRules != null)
			{
				ConditionalRules.ResetCachedProperties();
			}
		}

		public ZBool IsDependentQuestion
		{
			get
			{
				IList<int> dependentQuestions = new int[] { 6, 7, 8, 9, 10, 12, 13, 14, 15, 282 };
				return dependentQuestions.Contains(ON_CPDecNum);
			}
		}

		public MessageTypesForCPQAGenerator GeneratorMessageType
		{
			get
			{
				return fGeneratorMessageType;
			}
			set
			{
				fGeneratorMessageType = value;
			}
		}
		MessageTypesForCPQAGenerator fGeneratorMessageType;

		[ReadOnly(true)]
		public override ZInt ON_CPDecNum
		{
			get { return base.ON_CPDecNum; }
			set
			{
				base.ON_CPDecNum = value;
				fConditionalRules = null;
			}
		}

		CMRConditionalQuestionRules ConditionalRules
		{
			get
			{
				if (fConditionalRules == null && !ON_CPDecNum.IsEmpty)
				{
					fConditionalRules = new CMRConditionalQuestionRules(this, ON_CPDecNum);
				}
				return fConditionalRules;
			}
		}
		CMRConditionalQuestionRules fConditionalRules;

		public List<ZInt> AQISContainerQuestions = new List<ZInt>(new ZInt[] { 6, 7, 8, 9 });

		public override ZString ON_AnswerCode
		{
			get { return base.ON_AnswerCode; }
			set
			{
				bool hasChanged = base.ON_AnswerCode != value;
				base.ON_AnswerCode = value;
				if (hasChanged)
				{
					if (ON_Permit_ReadOnly)
					{
						ON_Permit = ZString.Empty;
					}

					if (ON_CPDecNum == 14)
					{
						AQISDecsNotRequiredAferDelivery(value);
					}
				}
			}
		}

		bool ON_Permit_ReadOnly
		{
			get { return !IsYes; }
		}

		public bool IsPermitRelevant
		{
			get { return LodgementQuestion != null && LodgementQuestion.IsPermitRelevant(this); }
		}

		#endregion

		#region HasKey

		internal void ClearAnswerAndPermit()
		{
			ON_AnswerCode = ZString.Empty;
			ON_Permit = ZString.Empty;
		}

		#endregion

		#region New Bindable Properties

		[BusinessObjectTestExclude]
		[CargoWise.ComponentModel.MaxLength(64)]
		public ZString QuestionID
		{
			get { return ON_CPDecNum.ToString(); }
			set
			{
				CMRLodgementQuestionParser parser = new CMRLodgementQuestionParser(value);
				if (parser.IsCompleteCode)
				{
					ON_CPDecNum = parser.QuestionID;
					ON_CPDecStartDate = parser.StartDate;
				}
				else
				{
					ZInt questionNumber = 0;
					if (ZInt.TryParse(value, out questionNumber))
					{
						ON_CPDecNum = questionNumber;
						ON_CPDecStartDate = GetTheMostAvailableDate();
					}
				}
				QuestionIDInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateQuestionID();
				}
			}
		}

		public ZPropertyInfo QuestionIDInfo
		{
			get { return GetZPropertyInfo(Schema.QuestionID); }
		}

		protected ZDateTime GetTheMostAvailableDate()
		{
			ZDateTime result = ZDateTime.Empty;
			ICPQAAttachee attachee = this.LineAttachee;
			if (attachee == null && Declaration != null)
			{
				attachee = Declaration;
			}
			if (attachee != null)
			{
				CMRLodgementQuestion[] sameNumberQuestions = CMRLodgementQuestion.Load(attachee, new ZInt[] { ON_CPDecNum });
				int questionsCount = sameNumberQuestions.Length;
				if (questionsCount > 0)
				{
					Array.Sort(sameNumberQuestions, new CMRLodgementQuestionComparer());
					CMRLodgementQuestion lastQuestion = sameNumberQuestions[questionsCount - 1];
					result = lastQuestion.CQ_LodgementQuestionStartDate;
				}
			}
			return result;
		}

		public ZString EntryLineDescription
		{
			get
			{
				StringBuilder result = new StringBuilder();
				var entryLine = EntryLine;
				if (entryLine != null)
				{
					result.Append("Entry Line No: ");
					result.Append(EntryLineDescriptionPrefix);
					result.Append(entryLine.CL_LineNumber + "/");
					result.Append(entryLine.TariffAndStatCodeFormatted + "/");
					result.Append(entryLine.Description);
				}
				return result.ToString();
			}
		}

		public ZPropertyInfo EntryLineDescriptionInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(EntryLineDescription));
			}
		}

		public ZString EntryLineDescriptionPrefix { get; set; }

		public ZString Question
		{
			get { return LodgementQuestion != null ? LodgementQuestion.CQ_LodgementQuestionText.ToString() : this.ON_CPDecNum == 999 ? DrawbackPayeeDec : ""; }
		}
		public const string DrawbackPayeeDec = "I declare that the payee was the legal owner of the goods at the time of export or the payee was assigned the right to claim drawback of import duty paid on the goods.";

		public ZPropertyInfo QuestionInfo
		{
			get { return GetZPropertyInfo(nameof(Question)); }
		}

		#endregion

		#region Related Objects

		public new CMRCusEntryCPDecLookups Lookups
		{
			get { return (CMRCusEntryCPDecLookups)base.Lookups; }
		}

		protected override CusEntryCPDecLookups GetNewLookups()
		{
			return new CMRCusEntryCPDecLookups(this);
		}

		public CusEntryHeader EntryHeader
		{
			get { return (CusEntryHeader)Factory.Load(typeof(CusEntryHeader), ON_CH); }
		}

		public CusEntryLine EntryLine
		{
			get { return (CusEntryLine)Factory.Load(typeof(CusEntryLine), ON_CL); }
		}

		public ICPQALineAttachee LineAttachee
		{
			get
			{
				ICPQALineAttachee result = null;
				if (EntryLine != null)
				{
					result = EntryLine;
				}
				else if (ON_ParentTableCode == CusClassificationSchema.Constants.Prefix)
				{
					result = (ICPQALineAttachee)Factory.Load(typeof(Classification), ON_ParentID);
				}
				else if (ON_ParentTableCode == CusClassPartPivotSchema.Constants.Prefix)
				{
					result = (ICPQALineAttachee)Factory.Load(typeof(CusClassPartPivot), ON_ParentID);
				}
				else if (ON_ParentTableCode == OrgHeaderSchema.Constants.Prefix)
				{
					var org = Factory.Load<OrgHeader>(ON_ParentID);
					if (org != null)
					{
						result = new OrganisationCPQA(org);
					}
				}
				return result;
			}
		}

		public JobDeclaration Declaration
		{
			get
			{
				JobDeclaration result = null;
				if (ON_JE.IsValid)
				{
					result = (JobDeclaration)Factory.Load(typeof(JobDeclaration), ON_JE);
				}
				else if (EntryHeader != null)
				{
					result = EntryHeader.Declaration;
				}
				else if (EntryLine != null)
				{
					result = EntryLine.Declaration;
				}
				return result;
			}
		}

		public CMRLodgementQuestion LodgementQuestion
		{
			get { return CMRLodgementQuestion.Load(Factory, ON_CPDecNum, ON_CPDecStartDate, SQLComparisonOperator.Equal); }
		}

		internal ArrayList AcknowledgeQuestions
		{
			get
			{
				if (fAcknowledgeQuestions == null)
				{
					fAcknowledgeQuestions = new ArrayList();
					fAcknowledgeQuestions.Add(new ZInt(1));
					fAcknowledgeQuestions.Add(new ZInt(2));
					fAcknowledgeQuestions.Add(new ZInt(3));
					fAcknowledgeQuestions.Add(new ZInt(4));
					fAcknowledgeQuestions.Add(new ZInt(5));
					fAcknowledgeQuestions.Add(new ZInt(10));
					fAcknowledgeQuestions.Add(new ZInt(11));
					fAcknowledgeQuestions.Add(new ZInt(12));
					fAcknowledgeQuestions.Add(new ZInt(13));
					fAcknowledgeQuestions.Add(new ZInt(16));
					fAcknowledgeQuestions.Add(new ZInt(326));
					fAcknowledgeQuestions.Add(new ZInt(375));
					if (Enterprise.Environment.Env.Registry.CMRTestMode)
					{
						fAcknowledgeQuestions.Add(new ZInt(400));
						fAcknowledgeQuestions.Add(new ZInt(402));
					}
					else
					{
						fAcknowledgeQuestions.Add(new ZInt(508));
						fAcknowledgeQuestions.Add(new ZInt(533));
					}
				}
				return fAcknowledgeQuestions;
			}
		}
		ArrayList fAcknowledgeQuestions;

		new CMRCusEntryCPDecValidation Validation
		{
			get { return (CMRCusEntryCPDecValidation)base.Validation; }
		}

		protected override CusEntryCPDecValidation GetNewValidation()
		{
			return new CMRCusEntryCPDecValidation(this);
		}

		protected void AQISDecsNotRequiredAferDelivery(ZString dec14Answer)
		{
			if (IsPostDeliveryAQISDecProcessingRequired)
			{
				foreach (CMRCusEntryCPDec cPDec in EntryHeader.Questions)
				{
					cPDec.ResetCachedValues();
					if (AQISContainerQuestions.Contains(cPDec.ON_CPDecNum))
					{
						if (dec14Answer == Answers.YES)
						{
							cPDec.ON_AnswerCode = ZString.Empty;
						}
						else
						{
							if (!IsValidationSuspended)
							{
								cPDec.Validation.ValidateON_AnswerCode();
							}
						}
					}
				}
			}
		}

		bool IsPostDeliveryAQISDecProcessingRequired
		{
			get { return (Declaration != null && EntryHeader != null && Declaration.JE_TransportMode == Core.Constants.TransportModes.Sea); }
		}

		#endregion
	}
}
