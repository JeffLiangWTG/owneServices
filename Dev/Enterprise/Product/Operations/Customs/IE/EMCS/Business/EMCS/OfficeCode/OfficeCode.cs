using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class OfficeCode : EU.EMCS.Business.OfficeCode
	{
		public OfficeCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(EMCSJobDeclaration));

		public override bool OfficeCodesUseDesInsteadOfCaa => true;
	}
}
