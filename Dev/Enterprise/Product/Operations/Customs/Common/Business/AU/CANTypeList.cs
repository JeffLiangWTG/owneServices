using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.AU
{
	public class CANTypeList : UntranslatableCodeDescriptionPairList
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Untranslatable CodeDescription")]
		public CANTypeList()
			: base("Country-Specific Customs values")
		{
			Add(CANType.CustomsAuthorityNumber);
			Add(CANType.ContingencyCustomsAuthorityNumber);
			AddRange(new CMR.CMRExportExemptionCodesList());
		}
	}
}
