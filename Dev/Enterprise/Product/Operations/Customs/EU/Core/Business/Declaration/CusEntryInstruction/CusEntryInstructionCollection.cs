using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusEntryInstructionCollection<TCusEntryInstruction> : Customs.Business.CusEntryInstructionCollection<TCusEntryInstruction>
		where TCusEntryInstruction : CusEntryInstruction
	{
		public CusEntryInstructionCollection(JobDeclaration parentBO)
			: base(parentBO)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var currentCEI = ((TCusEntryInstruction)child);
			DefaultFiscalReferences(currentCEI);
			if (Count == 0)
			{
				SetDefaultForFirstCEI(currentCEI);
			}
		}

		protected virtual void SetDefaultForFirstCEI(TCusEntryInstruction cei)
		{
			if (cei.JobDeclaration?.Shipment is ForwardingShipment shipment)
			{
				cei.CEI_TotalInnerPackages = shipment.JS_TotalPackageCount;
				if (!(bool)cei.JobDeclaration.JE_OverrideFreightDefaults)
				{
					cei.SynchroniserReadOnlyMembers.Add(nameof(CusEntryInstruction.CEI_TotalInnerPackages));
				}
			}
		}

		protected virtual void DefaultFiscalReferences(TCusEntryInstruction cei)
		{
			cei.FiscalRepresentativeDefaulter.DefaultFiscalReferences(cei);
		}
	}
}
