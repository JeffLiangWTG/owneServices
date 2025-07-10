using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class GuaranteeForEntryInstruction : EU.Business.Declaration.GuaranteeForEntryInstruction
	{
		public GuaranteeForEntryInstruction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new GuaranteeForEntryInstructionLookups Lookups => (GuaranteeForEntryInstructionLookups)base.Lookups;

		protected override CusBondDetailLookups GetNewLookups() => new GuaranteeForEntryInstructionLookups(this);
	}
}
