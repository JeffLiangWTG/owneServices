using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public sealed class DocDataObjectParameters : IDocDataObjectParameters
	{
		public DocDataObjectParameters(string documentTitle, string dataStoreName, object data = null, IStmALogProvider logProvider = null)
		{
			DocumentTitle = documentTitle;
			Data = data;
			DataStoreName = dataStoreName;
			LogProvider = logProvider;
		}

		public string DocumentTitle { get; }
		public string DataStoreName { get; }
		public object Data { get; }
		public IStmALogProvider LogProvider { get; }
	}
}
