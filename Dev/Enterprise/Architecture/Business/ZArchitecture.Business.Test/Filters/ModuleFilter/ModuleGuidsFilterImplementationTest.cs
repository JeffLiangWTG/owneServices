using Enterprise.UniversalDataBuss.DataObjects;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(ShipmentXQueryPaths))]

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ModuleGuidsFilter))]
	sealed class ModuleGuidsFilterImplementationTest : ModuleGuidsFilterTest
	{
	}
}
