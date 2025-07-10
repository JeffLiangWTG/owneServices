using CargoWise.Types;

namespace Enterprise.ResourceStrings.Business
{
	public interface IDocBuilderUsage
	{
		ZString Macro { get; }
		ZString TemplateName { get; }
		ZString AllDocumentNames { get; }
	}
}
