using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ImportClassificationCollection : BaseClassificationCollection<Classification>
	{
		public ImportClassificationCollection(BusinessObjectFactory factory)
			: base(factory, Core.Constants.CountryCodes.Australia)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((Classification)child).CC_ClassificationType = Classification.ClassificationType.IMP;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			result.AddToFilter(CusClassificationSchema.CC_ClassificationType, Classification.ClassificationType.IMP);
			return result;
		}
	}
}
