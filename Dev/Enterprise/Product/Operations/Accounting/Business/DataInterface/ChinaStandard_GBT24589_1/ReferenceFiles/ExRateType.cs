using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1
{
	public sealed class ExRateType : NonPersistentBusinessObject, IObsoleteValidation
	{
		public const string LocID = "T104";
		public ZString ExRateTypeNumber { get; set; }
		public ZString ExRateTypeName { get; set; }
	}

	public sealed class ExRateTypeCollection : NonPersistentBusinessObjectCollection<ExRateType>	{
		public ExRateTypeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			AddDefaultElements();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ExRateType();
		}

		void AddDefaultElements()
		{
			ExRateType exRateType = AddNew();
			exRateType.ExRateTypeNumber = "1";
			exRateType.ExRateTypeName = (NoResString)"买入汇率";

			exRateType = AddNew();
			exRateType.ExRateTypeNumber = "2";
			exRateType.ExRateTypeName = (NoResString)"卖出汇率";
		}
	}
}

