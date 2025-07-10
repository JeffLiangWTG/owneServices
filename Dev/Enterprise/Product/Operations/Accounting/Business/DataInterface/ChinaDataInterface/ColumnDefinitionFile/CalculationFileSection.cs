using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Already in another language")]
	public class CalculationFileSection : FileSection
	{
		public CalculationFileSection(BusinessObjectFactory factory)
			: base(factory)
		{
			fSectionHeader = "[核算]";
			fFileName = "";
		}
	}
}
