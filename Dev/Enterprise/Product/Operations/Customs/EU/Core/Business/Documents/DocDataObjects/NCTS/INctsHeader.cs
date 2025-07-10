using CargoWise.Types;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.Customs.EU.Business
{
	public interface INctsHeader
	{
		ZString SourceType { get; }

		ZString SourceID { get; }

		object GetDocDataObject(string dataContext, IDocDataObjectParameters parameters);
	}
}
