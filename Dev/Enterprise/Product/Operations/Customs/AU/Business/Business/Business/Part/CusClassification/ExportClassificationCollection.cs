using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[CodeProperty(Classification.Schema.CC_Description)]
	[DescriptionProperty(Classification.Schema.CC_LookupCode)]
	public class ExportClassificationCollection : BaseClassificationCollection<Classification>
	{
		public ExportClassificationCollection(BusinessObjectFactory factory)
			: base(factory, Core.Constants.CountryCodes.Australia)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((Classification)child).CC_ClassificationType = Classification.ClassificationType.EXP;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			result.AddToFilter(CusClassificationSchema.CC_ClassificationType, Classification.ClassificationType.EXP);
			return result;
		}
	}
}
