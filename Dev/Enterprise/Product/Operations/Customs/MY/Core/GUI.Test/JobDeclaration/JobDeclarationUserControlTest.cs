using Enterprise.Customs.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MY.GUI.Testing
{
	[TestedType(typeof(MYJobDeclarationUserControl))]
	class JobDeclarationUserControlTest : BaseCustomsDeclarationUserControlAbstractTest<MYJobDeclarationUserControl, Business.JobDeclaration>
	{
	}
}
