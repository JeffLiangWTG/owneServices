using CargoWise.Types;

namespace Enterprise.DocumentEngine.Business
{
	internal interface IDocumentConfigItem
	{
		ZString SectionName { get; }
		ZString SectionType { get; }
		ZString FilterList { get; }
		bool? EvaluatedValue { get; set; }
	}
}