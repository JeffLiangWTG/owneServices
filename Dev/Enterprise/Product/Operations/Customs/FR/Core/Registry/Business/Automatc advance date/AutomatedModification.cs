using System;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.FR.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.FR.Registry.XmlSerializers")]
	public class AutomatedModification : RegistryBusinessObjectTemplate
	{
		#region Schema

		abstract class Schema
		{
			public const string EnableAutomatedModification = "EnableAutomatedModification";
			public const string TimeByDefault = "TimeByDefault";
		}

		#endregion

		#region Constructions and cloning

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AutomatedModification(fallbackLevel, factory);
		}

		public AutomatedModification()
			: base()
		{
		}
		public AutomatedModification(BusinessObjectFactory factory)
		: base(factory)
		{
		}
		public AutomatedModification(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#endregion

		#region Read / Write Elements

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			EnableAutomatedModification = reader.ReadElementStringAsZBool(Schema.EnableAutomatedModification);
			TimeByDefault = reader.ReadElementStringAsZDateTime(Schema.TimeByDefault, ZDateTime.BestReadableDateTimeFormat);
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.EnableAutomatedModification, EnableAutomatedModification.ToString());
			writer.WriteElementString(Schema.TimeByDefault, TimeByDefault.ToBestReadableDateTimeString());
		}

		#endregion

		#region Enable automated Modification
		public ZBool EnableAutomatedModification
		{
			get { return enableAutomatedModification; }
			set
			{
				var hasChanged = value != EnableAutomatedModification;
				SetNonPersistentPropertyValue(EnableAutomatedModificationInfo, ref enableAutomatedModification, value);

				if (hasChanged)
				{
					if (!EnableAutomatedModification)
					{
						TimeByDefault = ZDateTime.Empty;
					}
					else if (TimeByDefault.IsEmpty)
					{
						TimeByDefault = new ZDateTime(2023, 1, 1, 11, 30, 00);
					}
				}
			}
		}
		ZBool enableAutomatedModification;

		public ZPropertyInfo EnableAutomatedModificationInfo
		{
			get { return GetZPropertyInfo(Schema.EnableAutomatedModification); }
		}
		#endregion

		#region TimeByDefault

		[ReadOnlyMember(nameof(TimeByDefaultReadOnly))]
		public ZDateTime TimeByDefault
		{
			get { return new ZDateTime(timeByDefault, DateTimeKind.Unspecified); }
			set
			{
				SetNonPersistentPropertyValue(TimeByDefaultInfo, ref timeByDefault, value);
				if (!IsValidationSuspended)
				{
					ValidateTimeByDefault();
				}
			}
		}
		ZDateTime timeByDefault;

		public ZPropertyInfo TimeByDefaultInfo
		{
			get { return GetZPropertyInfo(Schema.TimeByDefault); }
		}

		public void ValidateTimeByDefault()
		{
			TimeByDefaultInfo.ClearAllNotifications();
			if (EnableAutomatedModification)
			{
				MandatoryValidation.CheckEntered(TimeByDefaultInfo);
			}
		}

		#endregion

		public bool TimeByDefaultReadOnly => !EnableAutomatedModification;
	}
}
