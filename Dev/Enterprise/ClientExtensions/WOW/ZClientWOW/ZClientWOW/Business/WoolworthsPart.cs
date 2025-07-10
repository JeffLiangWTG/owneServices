using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Client.Wow
{
	public class WoolworthsProduct : AUOrgSupplierPart
	{
		public WoolworthsProduct(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			if (OP_Wow_ImportedFromMI)
			{
				((IZPropertyInfoObsolete)OP_PartNumInfo).ReadOnly = true;
			}
		}

		public bool OP_Wow_ImportedFromMI
		{
			get { return OP_CustomFlag5; }
			set { OP_CustomFlag5 = value; }
		}
	}
}
