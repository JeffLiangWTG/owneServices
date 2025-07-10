using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Module
{
	internal class IndexJobOrWorkflowFilterOptions : CodeDescriptionPairList
	{
		public class Codes
		{
			public const string All = "ALL";
			public const string Job = "JOB";
			public const string Workflow = "WFL";
		}

		public class Descriptions
		{
			public static MultilingualString All => ResString.GetMultilingualString("JobAndWorkflowFilterOptions|All", "Show both Jobs and Workflows");
			public static MultilingualString Job => ResString.GetMultilingualString("JobAndWorkflowFilterOptions|Job", "Show Jobs Only");
			public static MultilingualString Workflow => ResString.GetMultilingualString("JobAndWorkflowFilterOptions|Workflow", "Show Workflows Only");
		}

		public IndexJobOrWorkflowFilterOptions()
		{
			AddPair(Codes.All, Descriptions.All);
			AddPair(Codes.Job, Descriptions.Job);
			AddPair(Codes.Workflow, Descriptions.Workflow);
		}
	}
}
