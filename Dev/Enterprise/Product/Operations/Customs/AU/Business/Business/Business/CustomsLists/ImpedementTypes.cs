using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public partial class CMRImpedimentTypes : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string AlertProfileMatch = "APMATCH";
			public const string ConditionalReleaseCONDRELS = "COND RELS";
			public const string ConditionalReleaseCONDRELS2 = "CONDRELS";
			public const string EvaluationHoldAutoAndManualAssessmentEVAHOLD = "EVA HOLD";
			public const string EvaluationHoldAutoAndManualAssessmentEVAHOLD2 = "EVAHOLD";
			public const string ImportedFoodConditionalRelease = "IFCONDRELS";
			public const string ImportedFoodEvaluationHold = "IFEVAHOLD";
			public const string ImportsCargoRiskAssessmentHoldCRAIMPHLD = "CRA IMPHLD";
			public const string ImportsCargoRiskAssessmentHoldCRAIMPHLD2 = "CRAIMPHLD";
			public const string MatchedACommunityProtectionProfile = "CPMATCH";
			public const string OneOrMoreAqisConcernTypesHaveBeenSuppliedOnTheImportDeclaration = "AQISCONCRN";
			public const string QualityCheckOfLowRiskResponsesToCpLodgmentQuestions = "CPQUAL";
			public const string ReferToAqisREFERAQIS = "REFER AQIS";
			public const string ReferToAqisREFERAQIS2 = "REFERAQIS";
			public const string TaskAndAssessmentCreatedInAqisSystemOverTheAqisInterface = "AQISIFACE";
			public const string TheDeclarationOwnerIsNotAccreditedUnderTheAppropriateAqisBrokerAccreditationScheme = "AQISACCRED";
			public const string UserPlacementOfAnAssessmentOnAWorkItem = "USERSELECT";
			public const string UserReferralOfAWorkItemToAnotherEvaluationWorkgroup = "USERREFER";
			public const string UserReferralOfAWorkItemWithAssessmentToAnOtherEvaluationWorkgroup = "USERASSMT";
		}

		public static class Descriptions
		{
			public const string AlertProfileMatch = "Alert Profile Match";
			public const string ConditionalReleaseCONDRELS = "Conditional release";
			public const string ConditionalReleaseCONDRELS2 = "Conditional release";
			public const string EvaluationHoldAutoAndManualAssessmentEVAHOLD = "Evaluation hold - auto and manual assessment";
			public const string EvaluationHoldAutoAndManualAssessmentEVAHOLD2 = "Evaluation hold - auto and manual assessment";
			public const string ImportedFoodConditionalRelease = "Imported Food Conditional Release";
			public const string ImportedFoodEvaluationHold = "Imported Food Evaluation Hold";
			public const string ImportsCargoRiskAssessmentHoldCRAIMPHLD = "Imports Cargo Risk Assessment hold";
			public const string ImportsCargoRiskAssessmentHoldCRAIMPHLD2 = "Imports Cargo Risk Assessment hold";
			public const string MatchedACommunityProtectionProfile = "Matched a community protection profile";
			public const string OneOrMoreAqisConcernTypesHaveBeenSuppliedOnTheImportDeclaration = "One or more Quarantine concern types have been supplied on the import declaration";
			public const string QualityCheckOfLowRiskResponsesToCpLodgmentQuestions = "Quality check of low risk responses to CP lodgment questions";
			public const string ReferToAqisREFERAQIS = "Refer to Quarantine";
			public const string ReferToAqisREFERAQIS2 = "Refer to Quarantine";
			public const string TaskAndAssessmentCreatedInAqisSystemOverTheAqisInterface = "Task and assessment created in Quarantine system over the Quarantine interface";
			public const string TheDeclarationOwnerIsNotAccreditedUnderTheAppropriateAqisBrokerAccreditationScheme = "The declaration owner is not accredited under the appropriate Quarantine Broker Accreditation Scheme";
			public const string UserPlacementOfAnAssessmentOnAWorkItem = "User placement of an assessment on a work item";
			public const string UserReferralOfAWorkItemToAnotherEvaluationWorkgroup = "User referral of a work item to another evaluation workgroup";
			public const string UserReferralOfAWorkItemWithAssessmentToAnOtherEvaluationWorkgroup = "User referral of a work item with assessment to an other evaluation workgroup";
		}

		public CMRImpedimentTypes()
		{
			AddPair(Codes.AlertProfileMatch, Descriptions.AlertProfileMatch);
			AddPair(Codes.ConditionalReleaseCONDRELS, Descriptions.ConditionalReleaseCONDRELS);
			AddPair(Codes.ConditionalReleaseCONDRELS2, Descriptions.ConditionalReleaseCONDRELS2);
			AddPair(Codes.EvaluationHoldAutoAndManualAssessmentEVAHOLD, Descriptions.EvaluationHoldAutoAndManualAssessmentEVAHOLD);
			AddPair(Codes.EvaluationHoldAutoAndManualAssessmentEVAHOLD2, Descriptions.EvaluationHoldAutoAndManualAssessmentEVAHOLD2);
			AddPair(Codes.ImportedFoodConditionalRelease, Descriptions.ImportedFoodConditionalRelease);
			AddPair(Codes.ImportedFoodEvaluationHold, Descriptions.ImportedFoodEvaluationHold);
			AddPair(Codes.ImportsCargoRiskAssessmentHoldCRAIMPHLD, Descriptions.ImportsCargoRiskAssessmentHoldCRAIMPHLD);
			AddPair(Codes.ImportsCargoRiskAssessmentHoldCRAIMPHLD2, Descriptions.ImportsCargoRiskAssessmentHoldCRAIMPHLD2);
			AddPair(Codes.MatchedACommunityProtectionProfile, Descriptions.MatchedACommunityProtectionProfile);
			AddPair(Codes.OneOrMoreAqisConcernTypesHaveBeenSuppliedOnTheImportDeclaration, Descriptions.OneOrMoreAqisConcernTypesHaveBeenSuppliedOnTheImportDeclaration);
			AddPair(Codes.QualityCheckOfLowRiskResponsesToCpLodgmentQuestions, Descriptions.QualityCheckOfLowRiskResponsesToCpLodgmentQuestions);
			AddPair(Codes.ReferToAqisREFERAQIS, Descriptions.ReferToAqisREFERAQIS);
			AddPair(Codes.ReferToAqisREFERAQIS2, Descriptions.ReferToAqisREFERAQIS2);
			AddPair(Codes.TaskAndAssessmentCreatedInAqisSystemOverTheAqisInterface, Descriptions.TaskAndAssessmentCreatedInAqisSystemOverTheAqisInterface);
			AddPair(Codes.TheDeclarationOwnerIsNotAccreditedUnderTheAppropriateAqisBrokerAccreditationScheme, Descriptions.TheDeclarationOwnerIsNotAccreditedUnderTheAppropriateAqisBrokerAccreditationScheme);
			AddPair(Codes.UserPlacementOfAnAssessmentOnAWorkItem, Descriptions.UserPlacementOfAnAssessmentOnAWorkItem);
			AddPair(Codes.UserReferralOfAWorkItemToAnotherEvaluationWorkgroup, Descriptions.UserReferralOfAWorkItemToAnotherEvaluationWorkgroup);
			AddPair(Codes.UserReferralOfAWorkItemWithAssessmentToAnOtherEvaluationWorkgroup, Descriptions.UserReferralOfAWorkItemWithAssessmentToAnOtherEvaluationWorkgroup);
		}
	}
}
