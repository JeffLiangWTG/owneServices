using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(ExceptionKeyStacktraceDepthCollection))]
	class ExceptionKeyStacktraceDepthCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ExceptionKeyStacktraceDepthCollection>
	{
		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override ExceptionKeyStacktraceDepthCollection GetCollectionToTest()
		{
			return new ExceptionKeyStacktraceDepthCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ExceptionKeyStacktraceDepth();
		}
	}
}
