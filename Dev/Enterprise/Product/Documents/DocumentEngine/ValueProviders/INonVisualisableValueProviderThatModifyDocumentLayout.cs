using CargoWise.Types;

namespace Enterprise.DocumentEngine
{
	internal interface INonVisualisableValueProviderThatModifyDocumentLayout : INonVisualisableValueProvider
	{
		ZString DocumentationForFormatting { get; }
	}
}