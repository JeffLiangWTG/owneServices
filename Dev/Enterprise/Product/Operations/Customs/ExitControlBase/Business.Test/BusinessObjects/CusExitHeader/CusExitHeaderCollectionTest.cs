using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ExitControlBase.Business.Testing;

[TestedType(typeof(CusExitHeaderCollection<CusExitHeader>))]
sealed class CusExitHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<CusExitHeaderCollection<CusExitHeader>>
{
}
