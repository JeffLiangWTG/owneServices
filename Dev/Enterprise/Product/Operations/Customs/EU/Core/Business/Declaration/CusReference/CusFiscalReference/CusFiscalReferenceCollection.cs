using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public interface ICusFiscalReferenceCollection<out TCusFiscalReference> : IBusinessObjectCollection<TCusFiscalReference>
		where TCusFiscalReference : CusFiscalReference
	{
		new TCusFiscalReference this[int index] { get; }
	}

	public class CusFiscalReferenceCollection<TCusFiscalReference> : CommonCusReferenceCollection<TCusFiscalReference>, ICusFiscalReferenceCollection<TCusFiscalReference>
		where TCusFiscalReference : CusFiscalReference
	{
		public CusFiscalReferenceCollection(BusinessObject parent) : base(parent, CusReferenceTypeList.Codes.FiscalReference)
		{
			MaxCountValidationEnable(MaxCountForValidation);
		}

		protected virtual int MaxCountForValidation => 99;
	}
}
