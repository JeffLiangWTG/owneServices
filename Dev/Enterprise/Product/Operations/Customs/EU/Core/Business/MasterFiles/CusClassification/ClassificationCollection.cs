using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.MasterFiles
{
	public class ClassificationCollection<T> : Customs.Business.ClassificationCollection<T> where T : CusClassification
	{
		public ClassificationCollection(OrgSupplierPart part, ZString countryCode)
			: base(part, countryCode)
		{
		}
	}
}
