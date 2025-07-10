using CargoWise.Types;
using Enterprise.Customs.FR.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.FR.Business.CusStatement
{
	public class FRStatementNumberGeneratorTarget : NumberGeneratorTarget
	{
		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore() => Context.AccessRegistry(FRCustomsDataRegistry.Instance.FRStatementNumberCustomisation);

		protected override int GetMaxLengthCore() => CusStatementHeader.Schema.B2_StatementNumberMaxLength;

		protected override ZString GetNameCore() => Res.GetString("5F30685F-1A69-457A-9ABD-3B907044E906", "Statement Number");

		public override string NumberCustomisationLocation => ((IRegistryItemInternals)FRCustomsDataRegistry.Instance.FRStatementNumberCustomisation).Location;
	}
}
