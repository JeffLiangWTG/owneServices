using System.Collections.Generic;

namespace Enterprise.Customs.JP.Common;

public static class JPResultCodeMapping
{
	public static Dictionary<string, List<string>> Mapping = new Dictionary<string, List<string>>()
	{
		{ "1DA", new List<string> { "IDC" } },
		{ "1ID", new List<string> { "IDC" } },
		{ "1SW", new List<string> { "IDC" } },
		{ "3ID", new List<string> { "IDC" } },
		{ "1FA", new List<string> { "IFA" } },
		{ "1FC", new List<string> { "IFC" } },
		{ "1OL", new List<string> { "OLT01" } },
		{ "3MI", new List<string> { "1MI" } },
		{ "3IF", new List<string> { "IFC" } },
		{ "MIE", new List<string> { "MIC" } },
		{ "MWD", new List<string> { "MWB" } },
		{ "MWE", new List<string> { "MWC" } },
		{ "OTD", new List<string> { "OTB" } },
		{ "OTE", new List<string> { "OTC" } },
	};
}
