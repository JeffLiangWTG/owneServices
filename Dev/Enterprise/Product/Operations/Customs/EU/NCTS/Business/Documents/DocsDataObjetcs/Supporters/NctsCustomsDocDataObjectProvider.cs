using Enterprise.Customs.Common.Shared;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.Customs.EU.NCTS.Business.Documents.DocDataObjects
{
	public class NctsCustomsDocDataObjectProvider : ICustomsDocDataObjectProvider
	{
		public object GetDocDataObject(object parent, string dataContext, IDocDataObjectParameters parameters)
		{
			return GetDocDataObjectCore(parent, dataContext, parameters);
		}

		protected virtual object GetDocDataObjectCore(object nctsHeader, string dataContext, IDocDataObjectParameters parameters)
		{
			return null;
		}
	}
}
