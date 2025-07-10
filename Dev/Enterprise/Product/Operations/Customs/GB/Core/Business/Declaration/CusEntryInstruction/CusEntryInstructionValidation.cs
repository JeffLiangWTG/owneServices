using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class CusEntryInstructionValidation : EU.Business.Declaration.CusEntryInstructionValidation
	{
		public CusEntryInstructionValidation(CusEntryInstruction parent) : base(parent)
		{
		}

		protected override void CheckCEI_SubStyle()
		{
			base.CheckCEI_SubStyle();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CEI_SubStyleInfo);
		}

		protected override void ValidateWarehouseAddressIsInValidCountry(ZPropertyInfo wareHouseInfo, OrgAddress address, BaseJobDeclaration declaration, Customs.Business.CusEntryHeader entryHeader)
		{
		}
	}
}
