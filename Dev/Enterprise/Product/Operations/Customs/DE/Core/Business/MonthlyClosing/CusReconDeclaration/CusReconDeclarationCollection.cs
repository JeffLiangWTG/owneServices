using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business
{
	public class CusReconDeclarationCollection : CusReconDeclarationCollection<CusReconDeclaration>
	{
		public CusReconDeclarationCollection(BusinessObjectFactory factory)
			: base(factory, CusReconDeclarationApplicationCodeList.Codes.CLS)
		{
		}
	}
}
