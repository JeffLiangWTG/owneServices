using Enterprise.Customs.DK.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.DK.GUI.Testing
{
	[TestedType(typeof(JobDeclarationUserControl))]
	sealed class JobDeclarationUserControlTest : Customs.GUI.Testing.BaseCustomsDeclarationUserControlAbstractTest<JobDeclarationUserControl, JobDeclaration>
	{
	}
}
