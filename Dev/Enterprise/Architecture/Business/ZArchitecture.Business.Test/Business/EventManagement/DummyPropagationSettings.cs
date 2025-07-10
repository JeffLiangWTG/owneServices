using Enterprise.Integration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[Immutable]
	sealed class DummyPropagationSettings : IPropagationSettings
	{
		public DummyPropagationSettings(bool propagatedOnParamChange)
		{
			PropagateOnParameterChange = propagatedOnParamChange;
		}

		public bool PropagateOnParameterChange { get; }
	}
}
