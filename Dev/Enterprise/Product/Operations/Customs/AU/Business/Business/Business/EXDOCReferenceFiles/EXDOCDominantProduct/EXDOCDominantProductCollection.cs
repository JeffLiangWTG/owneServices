namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EXDOCDominantProductCollection : EXDOCRefCodeCollection
	{
		public EXDOCDominantProductCollection(IEXDOCRefCodeTypeProvider typeProvider)
			: base(typeProvider, "DOMP")
		{
		}
	}
}
