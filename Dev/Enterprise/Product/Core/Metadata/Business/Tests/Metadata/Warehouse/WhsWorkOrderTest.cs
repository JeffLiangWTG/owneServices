#if DEBUG

using Enterprise.Metadata.Integration;

namespace Enterprise.Metadata.Business.Tests
{
	public class WhsWorkOrderTest : WhsDocketTest
	{
		protected override IMetadata NewMetadata => new WhsWorkOrder();
	}
}

#endif
