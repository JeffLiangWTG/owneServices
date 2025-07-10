using CargoWise.Types;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.BR.Business
{
	public class LPCOJobNumberGeneratorTarget : NumberGeneratorTarget
	{
		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore() => Context.AccessRegistry(BRCustomsDataRegistry.Instance.LPCOJobNumberCustomization);

		protected override int GetMaxLengthCore() => AutoCusPermitHeader.Schema.CPH_JobNumberMaxLength;

		protected override ZString GetNameCore() => Res.GetString("9E2356B7-092F-455B-A303-0521ADFD5567", "LPCO Job Number");

		public override string NumberCustomisationLocation => ((IRegistryItemInternals)BRCustomsDataRegistry.Instance.LPCOJobNumberCustomization).Location;
	}
}
