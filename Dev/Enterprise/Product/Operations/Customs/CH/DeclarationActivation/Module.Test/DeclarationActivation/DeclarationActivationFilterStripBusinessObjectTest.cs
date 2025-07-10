using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.DeclarationActivation.Module.Testing;

[TestedType(typeof(DeclarationActivationFilterStripBusinessObject))]
sealed class DeclarationActivationFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
{
	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new DeclarationActivationFilterStripBusinessObject();
}
