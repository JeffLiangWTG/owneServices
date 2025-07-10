using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class EnableAddEditAndDeleteLogsItem : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string Table = "Table";
			public const string EnableADDLogs = "EnableADDLogs";
			public const string EnableEDTLogs = "EnableEDTLogs";
			public const string EnableDELLogs = "EnableDELLogs";
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EnableAddEditAndDeleteLogsItem();
		}

		#endregion

		#region Properties

		#region TableName
		[MaxLength(50)]
		[ReadOnly(true)]
		public ZString Table
		{
			get { return table; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(TableInfo, ref table, value);
			}
		}
		ZString table;

		public ZPropertyInfo TableInfo
		{
			get { return GetZPropertyInfo(Schema.Table); }
		}

		#endregion

		#region EnableADDLogs

		[ReadOnly(false)]
		public ZBool EnableADDLogs
		{
			get { return enableADDLogs; }
			set
			{
				SetNonPersistentPropertyValue<ZBool>(EnableADDLogsInfo, ref enableADDLogs, value);
			}
		}
		ZBool enableADDLogs;

		public ZPropertyInfo EnableADDLogsInfo
		{
			get { return GetZPropertyInfo(Schema.EnableADDLogs); }
		}

		#endregion

		#region EnableEDTLogs

		[ReadOnly(false)]
		public ZBool EnableEDTLogs
		{
			get { return enableEDTLogs; }
			set
			{
				SetNonPersistentPropertyValue<ZBool>(EnableEDTLogsInfo, ref enableEDTLogs, value);
			}
		}
		ZBool enableEDTLogs;

		public ZPropertyInfo EnableEDTLogsInfo
		{
			get { return GetZPropertyInfo(Schema.EnableEDTLogs); }
		}

		#endregion

		#region EnableDELLogs

		[ReadOnly(false)]
		public ZBool EnableDELLogs
		{
			get { return enableDELLogs; }
			set
			{
				SetNonPersistentPropertyValue<ZBool>(EnableDELLogsInfo, ref enableDELLogs, value);
			}
		}
		ZBool enableDELLogs;

		public ZPropertyInfo EnableDELLogsInfo
		{
			get { return GetZPropertyInfo(Schema.EnableDELLogs); }
		}

		#endregion

		public (ZBool IsEnabledForADD, ZBool IsEnabledForEDT, ZBool IsEnabledForDEL) GetAuditConfiguration() => (EnableADDLogs, EnableEDTLogs, EnableDELLogs);

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Table, Table);
			writer.WriteElementString(Schema.EnableADDLogs, EnableADDLogs.ToString());
			writer.WriteElementString(Schema.EnableEDTLogs, EnableEDTLogs.ToString());
			writer.WriteElementString(Schema.EnableDELLogs, EnableDELLogs.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Table = reader.ReadElementString(Schema.Table);
			EnableADDLogs = reader.ReadElementStringAsZBool(Schema.EnableADDLogs);
			EnableEDTLogs = reader.ReadElementStringAsZBool(Schema.EnableEDTLogs);
			EnableDELLogs = reader.ReadElementStringAsZBool(Schema.EnableDELLogs);
		}

		#endregion
	}
}
