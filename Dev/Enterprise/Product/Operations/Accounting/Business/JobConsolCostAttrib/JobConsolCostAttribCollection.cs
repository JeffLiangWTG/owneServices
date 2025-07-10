using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	[CodeAlive("Will be used in the next workflow")]
	public class JobConsolCostAttribCollection : DependentBusinessObjectCollection<JobConsolCostAttrib, JobConsolCost>
	{
		public JobConsolCostAttribCollection(JobConsolCost master)
			: base(master)
		{
		}

		public JobConsolCostAttribCollection(JobConsolCost master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		public JobConsolCostAttribCollection(JobConsolCost master, BusinessObjectFactory factory, bool allowMasterFactoryToBeDifferent)
			: base(master, factory, allowMasterFactoryToBeDifferent)
		{
		}

		public JobConsolCostAttribCollection(JobConsolCost master, ZQuery additionalFilter)
			: base(master, additionalFilter)
		{
		}

		public JobConsolCostAttribCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public void Add(RateAttributeSet attributeSet)
		{
			foreach (var attribute in attributeSet.Attributes)
			{
				Add(attribute.Code, attribute.Value, attribute.Amount);
			}
		}

		public void Add(string code, string value, decimal? amount = null)
		{
			var attr = AddNew();
			attr.E6A_Name = code;
			attr.E6A_Value = value;

			if (amount != null)
			{
				attr.E6A_Amount = amount.Value;
			}
		}

		protected override string FkColumnName => JobConsolCostAttribSchema.E6A_E6_JobConsolCost.Name;
	}
}