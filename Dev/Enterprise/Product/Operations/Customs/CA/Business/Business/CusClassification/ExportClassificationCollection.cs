using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	[ModuleID(ModuleId.CAExportClassification)]
	public class ExportClassificationCollection : BaseClassificationCollection<CusClassification>
	{
		public ExportClassificationCollection(BusinessObjectFactory factory)
			: base(factory, Core.Constants.CountryCodes.Canada)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((CusClassification)child).CC_ClassificationType = CusClassification.ClassificationType.EXP;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			result.AddToFilter(CusClassificationSchema.CC_ClassificationType, CusClassification.ClassificationType.EXP);
			return result;
		}
	}
}
