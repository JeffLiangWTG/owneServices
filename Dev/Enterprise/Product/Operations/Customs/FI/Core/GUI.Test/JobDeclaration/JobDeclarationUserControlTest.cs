using Enterprise.Customs.FI.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FI.GUI.Testing;

[TestedType(typeof(JobDeclarationUserControl))]
sealed class JobDeclarationUserControlTest : Customs.GUI.Testing.BaseCustomsDeclarationUserControlAbstractTest<JobDeclarationUserControl, JobDeclaration>
{
}
