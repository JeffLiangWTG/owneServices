using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Business
{
	public class PermitCountrySpecificInstruction : Customs.Business.PermitCountrySpecificInstruction
	{
		public PermitCountrySpecificInstruction(BusinessObjectFactory factory) : base(factory)
		{
		}

		public virtual ZZRefCusCodeListCombinedCollection GetFullTypeList(ZString country)
		{
			var supportingDocumentListWithPermitAttribute = Factory.GetSupportingDocumentListWithPermitAttribute(country);
			supportingDocumentListWithPermitAttribute.Load();
			return supportingDocumentListWithPermitAttribute.Any() ? supportingDocumentListWithPermitAttribute : Factory.GetSupportingDocumentList(country);
		}

		public override bool IsQtyValIndicatorMandatory => false;
	}
}
