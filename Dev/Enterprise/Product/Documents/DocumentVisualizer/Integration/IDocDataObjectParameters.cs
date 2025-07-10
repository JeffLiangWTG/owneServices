using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IDocDataObjectParameters
	{
		string DocumentTitle { get; }
		string DataStoreName { get; }
		object Data { get; }
		IStmALogProvider LogProvider { get; }
	}
}
