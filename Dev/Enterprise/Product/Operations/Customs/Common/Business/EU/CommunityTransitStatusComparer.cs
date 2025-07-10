using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.Common.EU
{
	public class CommunityTransitStatusComparer : IComparer<ZString>
	{
		// Most severe to least severe:  TD  T1 T2 TF X F C
		public int Compare(ZString x, ZString y)
		{
			var sequence = new Dictionary<ZString, int>();
			sequence.Add(ExportCommunityTransitStatusList.Codes.TD, 1);
			sequence.Add(ExportCommunityTransitStatusList.Codes.T1, 2);
			sequence.Add(ExportCommunityTransitStatusList.Codes.T2, 3);
			sequence.Add(ExportCommunityTransitStatusList.Codes.TF, 4);
			sequence.Add(ExportCommunityTransitStatusList.Codes.X, 5);
			sequence.Add(ExportCommunityTransitStatusList.Codes.F, 6);
			sequence.Add(ExportCommunityTransitStatusList.Codes.C, 7);
			return (sequence.ContainsKey(x) && sequence.ContainsKey(y)) ? sequence[x].CompareTo(sequence[y]) : 0;
		}
	}
}
