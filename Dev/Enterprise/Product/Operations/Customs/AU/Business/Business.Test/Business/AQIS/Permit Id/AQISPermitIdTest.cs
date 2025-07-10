using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AQISPermitId))]
	sealed class AQISPermitIdTest : AQISSingleValueBusinessObjectTest
	{
		AQISPermitId aqisPermitId;

		public override AQISSingleValueBusinessObject BusinessObjectToTest => aqisPermitId ?? (aqisPermitId = new AQISPermitId(Factory));

		protected override BusinessObject GetNewBusinessObject() => new AQISPermitId(Factory);
	}
}
