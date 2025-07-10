using CargoWise.Types;
using Enterprise.Customs.DE.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business
{
	public class CusReconDeclarationJobNumberGeneratorTarget : NumberGeneratorTarget
	{
		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore() => Context.AccessRegistry(DECustomsDataRegistry.Instance.MonthlyClosingJobNumberCustomization);

		protected override int GetMaxLengthCore() => CusReconDeclarationSchema.CRD_JobReferenceNumber.MaxLength;

		protected override ZString GetNameCore() => Res.GetString("158E8C6F-164A-4028-8A30-028B317CC569", "monthly closing job number");

		public override string NumberCustomisationLocation => ((IRegistryItemInternals)DECustomsDataRegistry.Instance.MonthlyClosingJobNumberCustomization).Location;
	}
}
