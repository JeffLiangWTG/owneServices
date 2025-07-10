using CargoWise.Types;
using Enterprise.Customs.FR.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.FR.Business
{
	public class CorrelationIDNumberGeneratorTarget : NumberGeneratorTarget
	{
		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore()
		{
			return Context.AccessRegistry(FRCustomsDataRegistry.Instance.CorrelationIDCustomisation);
		}

		protected override int GetMaxLengthCore() => CorrelationIDMaxLength;

		protected override ZString GetNameCore() => "CorellationID";

		public override string NumberCustomisationLocation => ((IRegistryItemInternals)FRCustomsDataRegistry.Instance.CorrelationIDCustomisation).Location;

		const int CorrelationIDMaxLength = 10;
	}
}
