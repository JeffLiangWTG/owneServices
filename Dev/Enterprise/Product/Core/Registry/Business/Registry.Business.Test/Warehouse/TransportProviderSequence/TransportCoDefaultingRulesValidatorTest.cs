using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Warehouse;

namespace Enterprise.Registry.Business.Testing
{
	public class TransportCoDefaultingRulesValidatorTest : SequenceValidatorTest
	{
		protected override ZPropertyInfo[] GetPropertyInfos()
		{
			var bizo = new TransportCoDefaultingRules();
			return new ZPropertyInfo[]
			{
				bizo.ConsigneeInfo,
				bizo.ClientInfo,
				bizo.WarehouseInfo
			};
		}

		protected override bool MustHaveAtLeastOneNonZeroValue => false;
	}
}
