using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	public class JobConsolCostAttrib : AutoJobConsolCostAttrib
	{
		public new class Schema : AutoJobConsolCostAttrib.Schema
		{
			public const int E6A_AmountScale = 4;
		}

		public JobConsolCostAttrib(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject(nameof(Cost))]
		public override ZGuid E6A_E6_JobConsolCost
		{
			get => base.E6A_E6_JobConsolCost;
			set => base.E6A_E6_JobConsolCost = value;
		}

		[DecimalPlaces(Schema.E6A_AmountScale)]
		public override ZDecimal E6A_Amount
		{
			get => base.E6A_Amount;
			set => base.E6A_Amount = value;
		}

		public virtual JobConsolCost Cost => Factory.Load<JobConsolCost>(E6A_E6_JobConsolCost);

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			if (E6A_Name.IsEmpty)
			{
				E6A_Name = JobChargeAttribTypeList.Codes.UnroundedItemsToRate;
			}

			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif

	}
}