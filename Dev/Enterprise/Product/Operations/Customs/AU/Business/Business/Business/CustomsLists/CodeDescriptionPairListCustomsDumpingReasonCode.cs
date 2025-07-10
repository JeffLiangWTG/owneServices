
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CodeDescriptionPairListCustomsDumpingReasonCode : CodeDescriptionPairList
	{
		public CodeDescriptionPairListCustomsDumpingReasonCode()
		{
			AddPair("11", "Not applicable - country/region of export not Country/Region of Origin");
			AddPair("12", "Not applicable to the exporter");
			AddPair("13", "Not applicable to goods of this type (model / size / specification, etc.)");
			AddPair("14", "Dumping and/or Countervaling Measures Imposed before 01/01/93, Duty is calculated but not payable");
			AddPair("15", "Dumping and/or Countervaling Measures Imposed before 01/01/93. Duty is payable");
			AddPair("16", "The goods are subject to an undertaking");
			AddPair("17", "Provisional Dumping and/or Countervaling Measures Apply");
			AddPair("18", "Interim Dumping and/or Countervaling Measures Apply");
		}
	}
}
