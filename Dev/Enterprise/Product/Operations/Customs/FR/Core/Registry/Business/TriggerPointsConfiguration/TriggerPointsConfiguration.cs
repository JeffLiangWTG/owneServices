using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.FR.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.FR.Registry.XmlSerializers")]
	public class TriggerPointsConfiguration : RegistryBusinessObjectTemplate
	{
		#region Schema

		abstract class Schema
		{
			public const string EnableAutomatedValidation = "EnableAutomatedValidation";
			public const string ImportTriggerPoint = "ImportTriggerPoint";
			public const string ExportTriggerPoint = "ExportTriggerPoint";
		}

		#endregion

		#region Constructions and cloning

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TriggerPointsConfiguration(fallbackLevel, factory);
		}

		public TriggerPointsConfiguration()
			: base()
		{
		}
		public TriggerPointsConfiguration(BusinessObjectFactory factory)
		: base(factory)
		{
		}
		public TriggerPointsConfiguration(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}
		
		#endregion

		#region Read / Write Elements

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			EnableAutomatedValidation = reader.ReadElementStringAsZBool(Schema.EnableAutomatedValidation);
			ImportTriggerPoint = reader.ReadElementString(Schema.ImportTriggerPoint);
			ExportTriggerPoint = reader.ReadElementString(Schema.ExportTriggerPoint);
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.EnableAutomatedValidation, EnableAutomatedValidation.ToString());
			writer.WriteElementString(Schema.ImportTriggerPoint, ImportTriggerPoint);
			writer.WriteElementString(Schema.ExportTriggerPoint, ExportTriggerPoint);
		}

		#endregion

		#region Enable automated validation
		public ZBool EnableAutomatedValidation
		{
			get { return enableAutomatedValidation; }
			set
			{
				SetNonPersistentPropertyValue(EnableAutomatedValidationInfo, ref enableAutomatedValidation, value);

				if (!EnableAutomatedValidation)
				{
					ImportTriggerPoint = TriggerPointsCodeList.Codes.NUL;
					ExportTriggerPoint = TriggerPointsCodeList.Codes.NUL;
				}
			}
		}
		ZBool enableAutomatedValidation;

		public ZPropertyInfo EnableAutomatedValidationInfo
		{
			get { return GetZPropertyInfo(Schema.EnableAutomatedValidation); }
		}
		#endregion

		#region ImportTriggerPoint

		[List(nameof(ImportTriggerPointsCodeList))]
		[ReadOnlyMember(nameof(TriggerPointReadOnly))]
		public ZString ImportTriggerPoint
		{
			get { return importTriggerPoint; }
			set
			{
				SetNonPersistentPropertyValue(ImportTriggerPointInfo, ref importTriggerPoint, value);

				if (!IsValidationSuspended)
				{
					ValidateImportTriggerPoint();
				}
			}
		}
		ZString importTriggerPoint;

		public ZPropertyInfo ImportTriggerPointInfo
		{
			get { return GetZPropertyInfo(Schema.ImportTriggerPoint); }
		}

		public void ValidateImportTriggerPoint()
		{
			ImportTriggerPointInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(ImportTriggerPointInfo, ImportTriggerPointsCodeList);
		}

		#endregion

		#region ExportTriggerPoint

		[List(nameof(ExportTriggerPointsCodeList))]
		[ReadOnlyMember(nameof(TriggerPointReadOnly))]
		public ZString ExportTriggerPoint
		{
			get { return exportTriggerPoint; }
			set
			{
				SetNonPersistentPropertyValue(ExportTriggerPointInfo, ref exportTriggerPoint, value);

				if (!IsValidationSuspended)
				{
					ValidateExportTriggerPoint();
				}
			}
		}
		ZString exportTriggerPoint;

		public ZPropertyInfo ExportTriggerPointInfo
		{
			get { return GetZPropertyInfo(Schema.ExportTriggerPoint); }
		}

		public void ValidateExportTriggerPoint()
		{
			ExportTriggerPointInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(ExportTriggerPointInfo, ExportTriggerPointsCodeList);
		}

		#endregion

		public bool TriggerPointReadOnly => !EnableAutomatedValidation;

		#region Lists

		public CodeDescriptionPairList ImportTriggerPointsCodeList
		{
			get
			{
				if (importTriggerPointsCodeList == null)
				{
					importTriggerPointsCodeList = new CodeDescriptionPairList();
					importTriggerPointsCodeList.AddPair(TriggerPointsCodeList.Codes.PAB, TriggerPointsCodeList.Descriptions.PAB);
					importTriggerPointsCodeList.AddPair(TriggerPointsCodeList.Codes.VAQ, TriggerPointsCodeList.Descriptions.VAQ);
					importTriggerPointsCodeList.AddPair(TriggerPointsCodeList.Codes.NUL, TriggerPointsCodeList.Descriptions.NUL);
				}

				return importTriggerPointsCodeList;
			}
		}
		CodeDescriptionPairList importTriggerPointsCodeList;

		public CodeDescriptionPairList ExportTriggerPointsCodeList
		{
			get
			{
				if (exportTriggerPointsCodeList == null)
				{
					exportTriggerPointsCodeList = new CodeDescriptionPairList();
					exportTriggerPointsCodeList.AddPair(TriggerPointsCodeList.Codes.REC, TriggerPointsCodeList.Descriptions.REC);
					exportTriggerPointsCodeList.AddPair(TriggerPointsCodeList.Codes.NUL, TriggerPointsCodeList.Descriptions.NUL);
				}

				return exportTriggerPointsCodeList;
			}
		}
		CodeDescriptionPairList exportTriggerPointsCodeList;

		#endregion
	}
}
