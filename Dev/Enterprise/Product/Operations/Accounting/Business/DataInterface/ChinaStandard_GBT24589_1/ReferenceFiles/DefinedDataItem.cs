
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1
{
	public sealed class DefinedDataItem : NonPersistentBusinessObject, IObsoleteValidation
	{
		public const string LocID = "T111";
		public ZString DataCode { get; set; }
		public ZString DataName { get; set; }
		public ZString DataDescription { get; set; }
		public ZString HasLevel { get; set; }
		public ZString DefinedDataCodeRule { get; set; }
	}

	public sealed class DefinedDataItemCollection : NonPersistentBusinessObjectCollection<DefinedDataItem>	{
		public DefinedDataItemCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DefinedDataItem();
		}
	}
}

