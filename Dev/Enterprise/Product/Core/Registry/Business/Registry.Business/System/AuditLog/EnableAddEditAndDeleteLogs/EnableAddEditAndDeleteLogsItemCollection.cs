using System.Linq;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class EnableAddEditAndDeleteLogsItemCollection : RegistryBusinessObjectCollectionTemplate<EnableAddEditAndDeleteLogsItem>
	{
		public EnableAddEditAndDeleteLogsItemCollection() : base(null, null)
		{
		}

		public EnableAddEditAndDeleteLogsItemCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowSort => false;

		protected override bool AllowRemoveCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new EnableAddEditAndDeleteLogsItem();
		}

		public EnableAddEditAndDeleteLogsItem FindByTableName(string tableName)
		{
			return this.Cast<EnableAddEditAndDeleteLogsItem>().FirstOrDefault(logItem => string.Compare((logItem).Table, tableName) == 0);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EnableAddEditAndDeleteLogsItemCollection();
		}

		public static EnableAddEditAndDeleteLogsItemCollection DefaultValue
		{
			get
			{
				var defaultValues = new EnableAddEditAndDeleteLogsItemCollection();
				var auditStmALogDecider = ObjectFactory.Get<IAuditStmALogDecider>();
				foreach (var table in auditStmALogDecider.AutoLoggedTablesDefaultConfigurationValues)
				{
					defaultValues.Add(new EnableAddEditAndDeleteLogsItem()
					{
						Table = table.Key,
						EnableADDLogs = table.Value.IsEnabledForADD,
						EnableEDTLogs = table.Value.IsEnabledForEDT,
						EnableDELLogs = table.Value.IsEnabledForDEL,
					});
				}
				return defaultValues;
			}
		}
	}
}
