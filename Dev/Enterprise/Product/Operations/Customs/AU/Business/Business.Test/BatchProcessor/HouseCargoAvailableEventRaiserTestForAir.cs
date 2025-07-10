using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(HouseCargoAvailableEventRaiser))]
	sealed class HouseCargoAvailableEventRaiserTestForAir : HouseCargoAvailableEventRaiserTest<CusHAWB>
	{
		protected override CusHAWB CreateParentCore() => Factory.New<CusHAWB>();

		protected override SchemaColumn StatusSchemaColumn => CusHAWBSchema.CS_CustomsStatus;
	}
}
