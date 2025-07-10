namespace Enterprise.AuditDataServices.Business
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;

	public class AuditChange : NonPersistentBusinessObject, IObsoleteValidation
	{
		public AuditChange(BusinessObjectFactory factory) : base(factory)
		{
		}

		public ZString ColumnName
		{ get; set; }

		public ZString RealColumnName
		{ get; set; }

		public ZString ValueBefore
		{ get; set; }

		public ZString ValueAfter
		{ get; set; }

		public class AuditChangeWithBinaryValue : AuditChange
		{
			public AuditChangeWithBinaryValue(BusinessObjectFactory factory) : base(factory)
			{
			}

			public ZBlob BinaryValue { get; set; }
		}
	}
}