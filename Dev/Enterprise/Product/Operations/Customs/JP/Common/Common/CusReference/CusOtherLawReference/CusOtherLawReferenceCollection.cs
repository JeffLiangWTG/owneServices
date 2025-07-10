using CargoWise.EntityFramework;

namespace Enterprise.Customs.JP.Common
{
	public class CusOtherLawReferenceCollection<T> : CusReferenceCollection<T> where T : CusOtherLawReference
	{
		public CusOtherLawReferenceCollection(BusinessObject parent) : base(parent, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanOtherLaws)
		{
			MaxCountValidationEnable(MaxRowCount);
		}

		public const int MaxRowCount = 5;

		protected override bool AllowNewCore => base.AllowNewCore && Count < MaxRowCount;
	}
}
