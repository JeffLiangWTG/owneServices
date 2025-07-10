using System;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public partial class LastDbRestoreInfo : RegistryBusinessObjectTemplate
	{
		public LastDbRestoreInfo() { }

		public LastDbRestoreInfo(string operation, string toolVersion, DateTime completionDate, string dbSchemaVersionBefore, string dbSchemaVersionAfter)
		{
			Operation = operation;
			ToolVersion = toolVersion;
			CompletionDate = completionDate;
			DbSchemaVersionBefore = dbSchemaVersionBefore;
			DbSchemaVersionAfter = dbSchemaVersionAfter;
		}

		#region SuppressResourceStringsCheckRegion

		public static class Schema
		{
			public const string Operation = "Operation";
			public const string ToolVersion = "ToolVersion";
			public const string CompletionDate = "CompletionDate";
			public const string DbSchemaVersionBefore = "DbSchemaVersionBefore";
			public const string DbSchemaVersionAfter = "DbSchemaVersionAfter";
		}

		#endregion

		#region Properties

		#region Operation

		[MaxLength(20)]
		public ZString Operation
		{
			get { return operation; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(OperationInfo, ref operation, value);
			}
		}

		ZString operation;

		public ZPropertyInfo OperationInfo
		{
			get { return GetZPropertyInfo(Schema.Operation); }
		}

		#endregion

		#region ToolVersion

		public ZString ToolVersion
		{
			get { return toolVersion; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(ToolVersionInfo, ref toolVersion, value);
			}
		}

		ZString toolVersion;

		public ZPropertyInfo ToolVersionInfo
		{
			get { return GetZPropertyInfo(Schema.ToolVersion); }
		}

		#endregion

		#region CompletionDate

		[MaxLength(20)]
		public ZDateTime CompletionDate
		{
			get { return completionDate; }
			set
			{
				SetNonPersistentPropertyValue<ZDateTime>(CompletionDateInfo, ref completionDate, value);
			}
		}

		ZDateTime completionDate;

		public ZPropertyInfo CompletionDateInfo
		{
			get { return GetZPropertyInfo(Schema.CompletionDate); }
		}

		#endregion

		#region DbSchemaVersionBefore

		public ZString DbSchemaVersionBefore
		{
			get { return dbSchemaVersionBefore; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(DbSchemaVersionBeforeInfo, ref dbSchemaVersionBefore, value);
			}
		}

		ZString dbSchemaVersionBefore;

		public ZPropertyInfo DbSchemaVersionBeforeInfo
		{
			get { return GetZPropertyInfo(Schema.DbSchemaVersionBefore); }
		}

		#endregion

		#region DbSchemaVersionAfter

		public ZString DbSchemaVersionAfter
		{
			get { return dbSchemaVersionAfter; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(DbSchemaVersionAfterInfo, ref dbSchemaVersionAfter, value);
			}
		}

		ZString dbSchemaVersionAfter;

		public ZPropertyInfo DbSchemaVersionAfterInfo
		{
			get { return GetZPropertyInfo(Schema.DbSchemaVersionAfter); }
		}

		#endregion

		#endregion //Properties

		#region XMLSerialisation

		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			Operation = reader.ReadElementString(Schema.Operation);
			ToolVersion = reader.ReadElementString(Schema.ToolVersion);
			using (Culture.SetTemporarily(Culture.Invariant))
			{
				CompletionDate = reader.ReadElementStringAsZDateTime(Schema.CompletionDate, Core.Constants.LastDatabaseRestoreRegistryConstants.CompletionDateFormat);
			}
			DbSchemaVersionBefore = reader.ReadElementString(Schema.DbSchemaVersionBefore);
			DbSchemaVersionAfter = reader.ReadElementString(Schema.DbSchemaVersionAfter);
		}

		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Operation, Operation);
			writer.WriteElementString(Schema.ToolVersion, ToolVersion);
			writer.WriteElementString(Schema.CompletionDate, CompletionDate.ToString(Core.Constants.LastDatabaseRestoreRegistryConstants.CompletionDateFormat, CultureInfo.InvariantCulture));
			writer.WriteElementString(Schema.DbSchemaVersionBefore, DbSchemaVersionBefore);
			writer.WriteElementString(Schema.DbSchemaVersionAfter, DbSchemaVersionAfter);
		}

		#endregion //XMLSerialisation

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) // Glorious boilerplate.
		{
			return new LastDbRestoreInfo();
		}
	}
}
