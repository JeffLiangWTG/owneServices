using System.Diagnostics;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class ProcessPriorityOptions : CodeDescriptionPairList
	{
		public static readonly string Normal = $"{ProcessPriorityClass.Normal}";
		public static readonly string BelowNormal = $"{ProcessPriorityClass.BelowNormal}";
		public static readonly string Idle = $"{ProcessPriorityClass.Idle}";

		public ProcessPriorityOptions()
		{
			AddPair(Normal, ResString.GetMultilingualString("30ce72b8-bd33-4eb9-a481-62d8082c60ff", "Normal Mode. Default setting of normal process."));
			AddPair(BelowNormal, ResString.GetMultilingualString("4eacec7a-b758-482d-b697-2f600795404e", "Non Blocking Mode. The threads of the Runner may let the threads of higher priority process run first."));
			AddPair(Idle, ResString.GetMultilingualString("5862bd5a-e611-4777-b5ff-fe19a9036786}", "Idle Mode. It makes runner run only when CPU is free."));

			DefaultCode = BelowNormal;
		}
	}
}
