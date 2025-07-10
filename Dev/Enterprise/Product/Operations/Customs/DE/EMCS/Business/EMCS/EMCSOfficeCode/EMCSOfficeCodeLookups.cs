using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class EMCSOfficeCodeLookups : OfficeCodeLookups
	{
		public EMCSOfficeCodeLookups(EMCSOfficeCode officeCode)
			: base(officeCode)
		{
		}

		protected new EMCSJobDeclaration Declaration => (EMCSJobDeclaration)base.Declaration;
	}
}
