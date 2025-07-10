using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AQISProducerCode))]
	sealed class AQISProducerCodeTest : AQISSingleValueBusinessObjectTest
	{
		AQISProducerCode aqisProducerCode;

		public override AQISSingleValueBusinessObject BusinessObjectToTest => aqisProducerCode ?? (aqisProducerCode = new AQISProducerCode(Factory));

		protected override BusinessObject GetNewBusinessObject() => new AQISProducerCode(Factory);
	}
}
