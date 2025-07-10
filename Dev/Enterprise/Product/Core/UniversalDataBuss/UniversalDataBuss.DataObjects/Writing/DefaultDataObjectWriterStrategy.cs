using Enterprise.UniversalDataBuss.Integration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.UniversalDataBuss.DataObjects
{
	[Immutable]
	public class DefaultDataObjectWriterStrategy : IDataObjectWriterStrategy
	{
		public static DefaultDataObjectWriterStrategy Instance { get; } = new DefaultDataObjectWriterStrategy();
		#if DEBUG
		public static DefaultDataObjectWriterStrategy TestInstance { get; } = new DefaultDataObjectWriterStrategy();
		#endif

		public DefaultDataObjectWriterStrategy()
		{
		}

		public bool IsAllowSet(string fieldName) => true;
	}
}
