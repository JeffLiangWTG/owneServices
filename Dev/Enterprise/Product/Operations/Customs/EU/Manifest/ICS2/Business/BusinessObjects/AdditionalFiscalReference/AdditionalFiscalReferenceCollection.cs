using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AdditionalFiscalReferenceCollection : EU.Business.Declaration.CusFiscalReferenceCollection<AdditionalFiscalReference>
	{
		public AdditionalFiscalReferenceCollection(BusinessObject parent) : base(parent)
		{
		}

		protected override int MaxCountForValidation => 1;
	}
}
