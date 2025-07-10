using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.Customs.Common.Shared
{
	public interface ICustomsDocDataObjectProvider
	{
		object GetDocDataObject(object parent, string dataContext, IDocDataObjectParameters parameters);
	}
}
