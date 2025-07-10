
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1
{
	public sealed class DefinedDataItemValue : NonPersistentBusinessObject, IObsoleteValidation
	{
		public const string LocID = "T112";
		public ZString DefinedDataCode { get; set; }
		public ZString DefinedDataItemValueCode { get; set; }
		public ZString DefinedDataItemValueName { get; set; }
		public ZString DefinedDataItemValueDescription { get; set; }
		public ZString ParentDefinedDataItemValueCode { get; set; }
		public ZString DefinedDataItemValueLevel { get; set; }
	}

	public sealed class DefinedDataItemValueCollection : NonPersistentBusinessObjectCollection<DefinedDataItemValue>	{
		public DefinedDataItemValueCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DefinedDataItemValue();
		}
	}
}

