using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Integration
{
	[Immutable]
	public interface IPropagationSettings
	{
		bool PropagateOnParameterChange { get; }
	}
}
