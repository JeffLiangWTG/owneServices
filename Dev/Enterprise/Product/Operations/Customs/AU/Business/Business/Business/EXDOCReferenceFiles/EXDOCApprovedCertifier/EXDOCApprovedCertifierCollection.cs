namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EXDOCApprovedCertifierCollection : EXDOCRefCodeCollection
	{
		public EXDOCApprovedCertifierCollection(IEXDOCRefCodeTypeProvider typeProvider)
			: base(typeProvider, "ACERT")
		{
		}
	}
}
