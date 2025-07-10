using Enterprise.Integration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.Business
{
	[Immutable]
	public sealed class DefaultPropagationSettings : IPropagationSettings
	{
		public bool PropagateOnParameterChange => true;
	}
}
