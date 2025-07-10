using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class CusEntryInstructionJobDocAddressValidation : JobDocAddressValidation
	{
		public CusEntryInstructionJobDocAddressValidation(JobDocAddress address, CusEntryInstruction entryInstruction)
			: base(address)
		{
			this.entryInstruction = entryInstruction;
		}
		readonly CusEntryInstruction entryInstruction;

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();
			var parent = Parent;
			var info = parent.OrganisationPKInfo;
			if (parent.E2_AddressType == DocAddressTypes.Codes.MainAccountingAddress && entryInstruction.IsMainAccountingAddressAvailable)
			{
				MandatoryValidation.MessageErrorIfNotEntered(info, Res.GetString("fcc672f6-905a-4946-8a9b-63acd27dcb85", "Main Accounting Address"));
			}
		}
	}
}
