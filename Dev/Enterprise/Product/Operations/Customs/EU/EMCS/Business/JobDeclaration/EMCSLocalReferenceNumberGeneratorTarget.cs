using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.EMCS.Business
{
	sealed class EMCSLocalReferenceNumberGeneratorTarget : NumberGeneratorTarget
	{
		public override string NumberCustomisationLocation => Res.GetString("B73AE5E5-C104-4513-BA3B-4003393040C0", "Customs -> EMCS Job Number Customization");

		protected override int GetMaxLengthCore() => JobDeclarationSchema.JE_DeclarationReference.MaxLength;

		protected override ZString GetNameCore() => Res.GetString("E3563384-CAD7-42C3-AC05-A0CD48878E11", "EMCS Job Number");

		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore() => Context.AccessRegistry(CustomsDataRegistry.Instance.EMCSLocalReferenceNumberCustomisation);
	}
}
