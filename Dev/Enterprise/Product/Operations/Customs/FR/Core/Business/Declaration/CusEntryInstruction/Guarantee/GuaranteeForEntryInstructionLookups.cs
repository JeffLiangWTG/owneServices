using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class GuaranteeForEntryInstructionLookups : EU.Business.Declaration.GuaranteeForEntryInstructionLookups
	{
		public GuaranteeForEntryInstructionLookups(GuaranteeForEntryInstruction guarantee) : base(guarantee)
		{
		}

		protected override CustomsOfficeCodeCollection OfficeCodeListCore => EUCustomsOfficeCodeCollection.AllEuropeanUnionCustomsOfficesWithRequiredRoles(Factory, EuOfficeCodesTypes.Codes.OfficeOfGuarantee);
	}
}
