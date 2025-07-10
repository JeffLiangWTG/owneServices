using System.Drawing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public static class BMConstants
	{
		public const int NumberOfZones = 3;
		public const int NumberOfZonesIncludingZoneZero = NumberOfZones + 1;

		public const int Zone0DefaultMultiplier = 1;
		public const int Zone1DefaultMultiplier = 1;
		public const int Zone2DefaultMultiplier = 1;
		public const int Zone3DefaultMultiplier = 1;

		public const int AgeHeadingPosition = 0;
		public const int ZoneHeadingPosition = 1;

		public const int DefaultBufferLoadLimitPercent = 50;
		public const decimal DefaultBufferTimeCapacityConstraintThresholdMultiple = 1.0m;
		public const decimal DefaultNonCCRTemporaryOverloadLimitMultiplier = 2.0m;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Not a duration")]
		public const int WorkingHoursPerDay = 8;
		public const int WorkingDaysPerWeek = 5;

		public const decimal TargetReleaseGapForMeetingAgreedDeliveryDate = 1.5m;
		public const decimal ResourceCapacityUtilisationOverloadFactor = 1.2m;
		public const decimal WeLetALittleBitMoreSqueezeThroughTheSystemMultiplier = 2.0m;
		public const int NudgeAmountForSharpFinishDelivery = 20;
		public const int NudgeAmountForGraduatedFinishDelivery = 10;

		public const int NumberOfCardsToShow = 100;

		public const int ChannelExpansionFactor = 3;

		public static readonly Color Zone3DefaultColor = Color.Blue;
		public static readonly Color Zone2DefaultColor = Color.LightBlue;
		public static readonly Color Zone1DefaultColor = Color.Pink;
		public static readonly Color Zone0DefaultColor = Color.Red;
		public static readonly Color BackgroundDefaultColor = Color.Beige;

		public static readonly Color HighRiskBoardColor = Color.FromArgb(255, 255, 93, 93);
		public static readonly Color CautionBoardColor = Color.FromArgb(255, 254, 255, 95);
		public static readonly Color GoodBoardColor = Color.FromArgb(255, 64, 126, 251);
		public static readonly Color ExcellentBoardColor = Color.FromArgb(255, 163, 255, 55);

		public static readonly string NecessityDependencyArrowColorName = Color.Black.Name;
		public static readonly string DependencyArrowWithoutLinkColorName = Color.DarkGray.Name;
		public static readonly string ResourceDependencyArrowColorName = Color.Orange.Name;
		public static readonly string DecoupledArrowColorName = Color.Gray.Name;

		public const string ChannelByTimeCode = "TIM";
		public static MultilingualString ChannelByTimeDescription
		{
			get { return ResString.GetMultilingualString("b6a90c8e-981e-4189-b4f5-27da200dc13b", "Show time units"); }
		}

		public static string UnchanneledDisplayName => Res.GetString("1670250a-9d55-4023-913e-09594b325e12", "Un-channeled");
		public static string NonConstrainedResourcesChannelDisplayName => Res.GetString("e84c4901-2828-4976-b689-09d9106fb7a9", "Non-Constrained");

		public const char BulletPointCharacter = '•';

		public const string BMSLeaveType = "BMS";

		public const string ComponentSectionType = "CMP";
		public const string ModuleGridSectionType = "MOD";
		public const string MENTSectionType = "MNT";
		public const string WEBSectionType = "WEB";
		public const string NetworkDiagramSectionType = "DIA";

		public const string WorkshopRunCachePropertyName = "WorkshopRun";

		public const string CCPMReleaseRulesTagGroupCode = "CRR";
		public const string NCNReleaseRulesTagGroupCode = "NRR";
		public const string ReadyToReleaseTagCode = "RTR";
		public const string ReleaseBlockedTagCode = "RBL";

		public const string WorkQueuesTagGroupCode = "QUE";

		public const string LastReleaseFailureNoteType = "RLF";
		public const string LastSuccessfulReleaseNoteType = "LSR";

		public const string LastTransferTypeDefaultCode = "NA";

		public const int ServiceTaskLogExpiryTimeMultiplier = 4;

		public const int SqlServerMaxValuesClausesCount = 1000;

		public const string WorkflowTypeNotFoundCode = "ZZZ";

		public const string JobLevelWorkflowCategoryCode = "JOB";

		public const string BufferPenetrationUpdaterServiceTaskCode = "BMB";
		public const string AgedScoresServiceTaskCode = "ASM";
		public const string MENTDataPurgeServiceTaskCode = "BMP";

		public const int MaximumReleaseSequenceParentWorkflowsDepth = 5;

		#region Filter Control Column Names

		public static class ProcessHeaderLinkFilterControlColumnNames
		{
			public const string FromJob = "HeaderFrom+ParentJobDescription";
			public const string FromJobDescription = "HeaderFrom+ProviderJobDescription";
			public const string ToJob = "HeaderTo+ParentJobDescription";
			public const string ToJobDescription = "HeaderTo+ProviderJobDescription";
		}

		#endregion

		#region Event Parameters

		public static class ComponentChangedEventParameters
		{
			public const string Mode = "MOD";
			public const string FromComponent = "FRM";
			public const string ToComponent = "TO";
			public const string CcrStatus = "CCR";
			public const string WorkflowStatus = "STS";
			public const string ComponentLinkPk = "LNK";
			public const string Reason = "RES";
		}

		public static class BufferParameters
		{
			public const string WorkflowBufferPenetration = "WPEN";
			public const string TaskBufferPenetration = "TPEN";
			public const string BufferPenetration = "PEN";
			public const string BufferZone = "ZON";
		}

		public static class TagEventParameters
		{
			public const string Action = "ACT";
			public const string Tag = "TAG";
			public const string TagGroup = "GRP";
			public const string TagRule = "RUL";
		}

		#endregion
	}
}
