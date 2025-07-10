using CargoWise.Types;
using Enterprise.Customs.FR.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class FRCustomsFallbackEntryNumberGeneratorTarget : NumberGeneratorTarget
	{
		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore()
		{
			return Context.AccessRegistry(FRCustomsDataRegistry.Instance.FRCustomsFallbackEntryNumberCustomisation);
		}

		protected override int GetMaxLengthCore() => CusEntryHeader.Schema.FRCustomsFallbackEntryNumberMaxLength;

		protected override ZString GetNameCore() => "FRCustomsFallbackEntryNumber";

		public override string NumberCustomisationLocation => ((IRegistryItemInternals)FRCustomsDataRegistry.Instance.FRCustomsFallbackEntryNumberCustomisation).Location;
	}
}
