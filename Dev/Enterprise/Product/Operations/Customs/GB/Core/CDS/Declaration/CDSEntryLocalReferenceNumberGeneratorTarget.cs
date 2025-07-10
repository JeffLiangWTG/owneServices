using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.GB.CDS.Declaration
{
	public class CDSEntryLocalReferenceNumberGeneratorTarget : NumberGeneratorTarget
	{
		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore()
		{
			return Context.AccessRegistry(GBCustomsDataRegistry.Instance.CdsEntryLocalReferenceNumberCustomisationFromCW1);
		}

		protected override int GetMaxLengthCore() => CusEntryHeader.Schema.LRNMaxLength;

		protected override ZString GetNameCore() => "CDS Local Reference Number";

		public override string NumberCustomisationLocation => ((IRegistryItemInternals)GBCustomsDataRegistry.Instance.CdsEntryLocalReferenceNumberCustomisationFromCW1).Location;
	}
}
