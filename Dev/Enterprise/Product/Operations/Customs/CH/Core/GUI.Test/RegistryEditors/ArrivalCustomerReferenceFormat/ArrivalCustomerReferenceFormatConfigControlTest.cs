using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(ArrivalCustomerReferenceFormatConfigControl))]
class ArrivalCustomerReferenceFormatConfigControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
{
	protected override IBusiness GetNewBusinessEntity()
	{
		return new ArrivalCustomerReferenceFormat();
	}
}
