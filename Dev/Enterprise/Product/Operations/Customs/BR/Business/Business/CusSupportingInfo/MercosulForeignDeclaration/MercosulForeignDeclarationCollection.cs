using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business
{
	public class MercosulForeignDeclarationCollection : Customs.Business.CusSupportingInfoCollection<MercosulForeignDeclaration>
	{
		public MercosulForeignDeclarationCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.MercosulForeignDeclaration)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var line = (MercosulForeignDeclaration)child;
			line.CSI_SubType = line.InvoiceLine?.MercosulForeignDeclarationType ?? ZString.Empty;
		}
	}
}
