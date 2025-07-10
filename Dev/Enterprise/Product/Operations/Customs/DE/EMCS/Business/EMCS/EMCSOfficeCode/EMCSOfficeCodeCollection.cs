using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class EMCSOfficeCodeCollection : OfficeCodeCollection<EMCSOfficeCode>
	{
		public EMCSOfficeCodeCollection(EMCSJobDeclaration master)
			: base(master)
		{
		}
	}
}
