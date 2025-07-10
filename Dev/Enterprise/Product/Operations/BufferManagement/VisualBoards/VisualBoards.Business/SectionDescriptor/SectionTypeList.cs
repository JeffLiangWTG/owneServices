using Enterprise.ZArchitecture.Core;

namespace Enterprise.VisualBoards.Business
{
	public class SectionTypeList : UntranslatableCodeDescriptionPairList
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "No point translating the untranslatable reason")]
		public SectionTypeList()
			: base("Descriptions come from classes accessed through Spring")
		{
			foreach (var descriptor in SectionDescriptorProvider.GetAllDescriptors())
			{
				AddPair(descriptor.Type, descriptor.Description);
			}
		}
	}
}
