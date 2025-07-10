using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class HouseConsignmentGoodItemsSupportingDocumentsLayoutBuilder<T> : ColumnLayoutBuilder<T, HouseConsignmentGoodItemsSupportingDocumentsControlBag> where T : NctsSupportingDocument
	{
		public override HouseConsignmentGoodItemsSupportingDocumentsControlBag CommonBag => HouseConsignmentGoodItemsSupportingDocumentsControlBag.Instance;

		protected override int MaxColumns => 1;
	}
}
