using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class MessageTypeObj : RegistryBusinessObjectTemplate
	{
		#region Schema

		protected abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string MessageType = "MessageType";
			public const string MessageTypeDescription = "MessageTypeDescription";
			public const string MessageSubType = "MessageSubType";
			public const string MessageSubTypeDescription = "MessageSubTypeDescription";
			public const string Selected = "Selected";
			public const string MinPurgeTime = "MinPurgeTime";
			public const string MinPurgeTimeUnit = "MinPurgeTimeUnit";
			public const string PurgeTime = "PurgeTime";
			public const string PurgeTimeUnit = "PurgeTimeUnit";
			public const string LatestPurgedMessageTimeUtc = "LatestPurgedMessageTimeUtc";
		}

		#endregion

		#region Properties

		#region PurgeType

		internal ICodeDescription PurgeType { private get; set; }

		#endregion

		#region MessageType

		public ZString MessageType_ForBinding
		{
			get
			{
				if (PurgeType.Equals(PurgeTypeList.MessageType) || PurgeType.Equals(PurgeTypeList.MessageTypeAndSubType))
				{
					return MessageType.IsEmpty ? (NoResString)"(Empty)" : MessageType;
				}

				return ZString.Empty;
			}
		}

		[CargoWise.ComponentModel.MaxLength(3)]
		public ZString MessageType
		{
			get { return messageType; }
			set { SetNonPersistentPropertyValue(MessageTypeInfo, ref messageType, value); }
		}
		ZString messageType;

		public ZPropertyInfo MessageTypeInfo
		{
			get { return GetZPropertyInfo(Schema.MessageType); }
		}

		#endregion

		#region MessageTypeDescription

		[CargoWise.ComponentModel.MaxLength(100)]
		public ZString MessageTypeDescription
		{
			get { return messageTypeDescription; }
			set { SetNonPersistentPropertyValue(MessageTypeDescriptionInfo, ref messageTypeDescription, value); }
		}
		ZString messageTypeDescription;

		public ZPropertyInfo MessageTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.MessageTypeDescription); }
		}

		#endregion

		#region MessageSubType

		public ZString MessageSubType_ForBinding
		{
			get
			{
				if (PurgeType.Equals(PurgeTypeList.MessageSubType) || PurgeType.Equals(PurgeTypeList.MessageTypeAndSubType))
				{
					return MessageSubType.IsEmpty ? (NoResString)"(Empty)" : MessageSubType;
				}

				return ZString.Empty;
			}
		}

		[CargoWise.ComponentModel.MaxLength(3)]
		public ZString MessageSubType
		{
			get { return messageSubType; }
			set { SetNonPersistentPropertyValue(MessageSubTypeInfo, ref messageSubType, value); }
		}
		ZString messageSubType;

		public ZPropertyInfo MessageSubTypeInfo
		{
			get { return GetZPropertyInfo(Schema.MessageSubType); }
		}

		#endregion

		#region MessageSubTypeDescription

		[CargoWise.ComponentModel.MaxLength(100)]
		public ZString MessageSubTypeDescription
		{
			get { return messageSubTypeDescription; }
			set { SetNonPersistentPropertyValue(MessageSubTypeDescriptionInfo, ref messageSubTypeDescription, value); }
		}
		ZString messageSubTypeDescription;

		public ZPropertyInfo MessageSubTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.MessageSubTypeDescription); }
		}

		#endregion

		#region Selected

		public ZBool Selected
		{
			get { return selected; }
			set { SetNonPersistentPropertyValue(SelectedInfo, ref selected, value); }
		}
		ZBool selected;

		public ZPropertyInfo SelectedInfo
		{
			get { return GetZPropertyInfo(Schema.Selected); }
		}

		#endregion

		#region PurgeTime

		public ZShort PurgeTime
		{
			get { return purgeTime; }
			set
			{
				SetNonPersistentPropertyValue(PurgeTimeInfo, ref purgeTime, value);

				if (!IsValidationSuspended)
				{
					ValidatePurgeTime();
				}
			}
		}
		ZShort purgeTime;

		public ZPropertyInfo PurgeTimeInfo
		{
			get { return GetZPropertyInfo(Schema.PurgeTime); }
		}

		public void ValidatePurgeTime()
		{
			PurgeTimeInfo.ClearAllNotifications();
			if (PurgeTime < 1)
			{
				PurgeTimeInfo.AddError(Res.GetString("322afde2-70aa-4acb-af90-c5462be59119", "1 week is the minimum allowed."));
			}
		}

		#endregion

		#region PurgeTimeUnit

		[List("PurgeTimeUnits")]
		public ZGuid PurgeTimeUnit
		{
			get { return purgeTimeUnit; }
			set
			{
				SetNonPersistentPropertyValue(PurgeTimeUnitInfo, ref purgeTimeUnit, value);
				if (!IsValidationSuspended)
				{
					ValidatePurgeTimeUnit();
				}
			}
		}
		ZGuid purgeTimeUnit;

		public ZPropertyInfo PurgeTimeUnitInfo
		{
			get { return GetZPropertyInfo(Schema.PurgeTimeUnit); }
		}

		public void ValidatePurgeTimeUnit()
		{
			PurgeTimeUnitInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(PurgeTimeUnitInfo);
			ListValidation.ErrorIfInvalidPK(PurgeTimeUnitInfo, PurgeTimeUnits);
		}

		public CodeDescriptionPairList PurgeTimeUnits => new PurgeTimeUnitsList();

		#endregion

		#region LatestPurgedMessageTime

		public ZDateTime LatestPurgedMessageTimeUtc
		{
			get { return latestPurgedMessageTimeUtc; }
			set { SetNonPersistentPropertyValue(LatestPurgedMessageTimeUtcInfo, ref latestPurgedMessageTimeUtc, value); }
		}
		ZDateTime latestPurgedMessageTimeUtc;

		public ZPropertyInfo LatestPurgedMessageTimeUtcInfo
		{
			get { return GetZPropertyInfo(Schema.LatestPurgedMessageTimeUtc); }
		}

		#endregion // LatestPurgedMessageTime

		#endregion

		#region Override

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			Selected = true;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new MessageTypeObj();
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.MessageType, MessageType);
			writer.WriteElementString(Schema.MessageTypeDescription, MessageTypeDescription);
			writer.WriteElementString(Schema.MessageSubType, MessageSubType);
			writer.WriteElementString(Schema.MessageSubTypeDescription, MessageSubTypeDescription);
			writer.WriteElementString(Schema.Selected, Selected.ToString());
			writer.WriteElementString(Schema.PurgeTime, PurgeTime.ToString());
			writer.WriteElementString(Schema.PurgeTimeUnit, PurgeTimeUnit.ToString());
			writer.WriteElementString(Schema.LatestPurgedMessageTimeUtc, LatestPurgedMessageTimeUtc.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			MessageType = reader.ReadElementString(Schema.MessageType);
			MessageTypeDescription = reader.ReadElementString(Schema.MessageTypeDescription);
			MessageSubType = reader.ReadElementString(Schema.MessageSubType);
			MessageSubTypeDescription = reader.ReadElementString(Schema.MessageSubTypeDescription);
			Selected = new ZBool(reader.ReadElementString(Schema.Selected));
			PurgeTime = new ZShort(reader.ReadElementString(Schema.PurgeTime));
			PurgeTimeUnit = new ZGuid(reader.ReadElementString(Schema.PurgeTimeUnit));
			LatestPurgedMessageTimeUtc = new ZDateTime(reader.ReadElementString(Schema.LatestPurgedMessageTimeUtc));
		}

		#endregion
	}
}
