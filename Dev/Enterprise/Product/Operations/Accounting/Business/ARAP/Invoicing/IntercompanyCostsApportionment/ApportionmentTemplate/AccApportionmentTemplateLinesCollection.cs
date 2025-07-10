using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class AccApportionmentTemplateLinesCollection : DependentBusinessObjectCollection<AccApportionmentTemplateLines, AccApportionmentTemplate>
	{
		public AccApportionmentTemplateLinesCollection(AccApportionmentTemplate master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			AccApportionmentTemplateLines line = child as AccApportionmentTemplateLines;
			if (line != null)
			{
				line.Y0_A0 = Master.PK;
				line.Y0_Description = Master.DefaultDescription;
			}
		}
	}
}
