using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class CellsValues
	{
		public CellsValues(List<ZGuid> relevantKeys, string humanReadableName, List<string> values)
		{
			this.RelevantKeys = relevantKeys;
			this.HumanReadableName = humanReadableName;
			this.Values = values;
		}

		public List<ZGuid> RelevantKeys;
		public string HumanReadableName;
		public List<string> Values;
	}
}