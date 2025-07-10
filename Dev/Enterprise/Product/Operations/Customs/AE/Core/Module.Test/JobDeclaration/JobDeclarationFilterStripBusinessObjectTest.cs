using Enterprise.Customs.Module;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Module.Testing;

[TestedType(typeof(JobDeclarationFilterStripBusinessObject))]
sealed class JobDeclarationFilterStripBusinessObjectTest : Customs.Module.Testing.JobDeclarationFilterBusinessObjectTest
{
	public void TestShipmentSubTypeFilterIsNotExist()
	{
		var filter = new JobDeclarationFilterStripBusinessObject();
		var subTypeFilter = (ModuleTextFilter)filter[DeclarationFilterConstants.ShipmentSubType];
		AssertNull(subTypeFilter);
	}
}
