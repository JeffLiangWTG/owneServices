using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Warehouse;

namespace Enterprise.Registry.Business.Testing
{
	public class CartonGroupSequenceValidatorTest : SequenceValidatorTest
	{
		protected override ZPropertyInfo[] GetPropertyInfos()
		{
			var bizo = new CartonGroupSequence();
			return new ZPropertyInfo[]
			{
				bizo.ProductInfo,
				bizo.CarrierInfo,
				bizo.ConsigneeInfo,
				bizo.ClientInfo,
				bizo.WarehouseInfo
			};
		}

		protected override bool MustHaveAtLeastOneNonZeroValue => true;
	}
}
