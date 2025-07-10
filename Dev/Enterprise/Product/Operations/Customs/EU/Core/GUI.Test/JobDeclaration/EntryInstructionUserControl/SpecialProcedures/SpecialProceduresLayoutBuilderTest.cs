using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(SpecialProceduresLayoutBuilder<JobDeclaration>))]
	sealed class SpecialProceduresLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<SpecialProceduresLayoutBuilder<JobDeclaration>, JobDeclaration, SpecialProceduresControlBag>
	{
		protected override SpecialProceduresLayoutBuilder<JobDeclaration> GetColumnLayoutBuilderForTesting()
		{
			var builder = new SpecialProceduresLayoutBuilder<JobDeclaration>();
			builder.AddControlBag(SpecialProceduresControlBag.Instance);
			return builder;
		}

		protected override int ExpectedMaxColumns => 3;
	}
}
