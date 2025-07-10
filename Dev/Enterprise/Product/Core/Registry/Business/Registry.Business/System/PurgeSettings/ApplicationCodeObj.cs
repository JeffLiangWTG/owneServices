using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ApplicationCodeObj : RegistryBusinessObjectTemplate
	{
		#region Schema

		protected abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string ApplicationCode = "ApplicationCode";
			public const string PurgeTypeDescription = "PurgeTypeDescription";
			public const string MinPurgeTime = "MinPurgeTime";
			public const string MinPurgeTimeUnit = "MinPurgeTimeUnit";
			public const string PurgeTime = "PurgeTime";
			public const string PurgeTimeUnit = "PurgeTimeUnit";
			public const string LatestPurgedMessageTimeUtc = "LatestPurgedMessageTimeUtc";
			public const string Selected = "Selected";
		}

		#endregion

		#region Properties

		#region ApplicationCode

		[CargoWise.ComponentModel.MaxLength(3)]
		public ZString ApplicationCode
		{
			get { return applicationCode; }
			set { SetNonPersistentPropertyValue(ApplicationCodeInfo, ref applicationCode, value); }
		}
		ZString applicationCode;

		public ZPropertyInfo ApplicationCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ApplicationCode); }
		}

		#endregion

		#region Interchanges

		[ChildEditable]
		public InterchangeObjCollection Interchanges
		{
			get
			{
				if (interchanges == null)
				{
					interchanges = new InterchangeObjCollection();
					RegisterEditableChildObject(interchanges);
				}
				return interchanges;
			}
		}

		InterchangeObjCollection interchanges;

		public void SetInterchanges(InterchangeObjCollection varInterchanges)
		{
			UnRegisterEditableChildObject(Interchanges);
			this.interchanges = varInterchanges;
			RegisterEditableChildObject(Interchanges);
		}

		#endregion

		#region MessageTypes

		[ChildEditable]
		public MessageTypeObjCollection MessageTypes
		{
			get
			{
				if (messageTypes == null)
				{
					this.messageTypes = new MessageTypeObjCollection();
					RegisterEditableChildObject(messageTypes);
				}

				return messageTypes;
			}
		}

		MessageTypeObjCollection messageTypes;

		public void SetMessageTypes(MessageTypeObjCollection messageTypes)
		{
			UnRegisterEditableChildObject(MessageTypes);
			this.messageTypes = messageTypes;
			RegisterEditableChildObject(MessageTypes);
		}

		#endregion

		#region PurgeTime

		[ReadOnlyMemberAttribute(nameof(PurgeTime_ReadOnly))]
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

		protected bool PurgeTime_ReadOnly
		{
			get { return !object.Equals(PurgeType, PurgeTypeList.ApplicationCode); }
		}

		public ZPropertyInfo PurgeTimeInfo
		{
			get { return GetZPropertyInfo(Schema.PurgeTime); }
		}

		public void ValidatePurgeTime()
		{
			PurgeTimeInfo.ClearAllNotifications();
			if (!IsUnpurgable && PurgeTime < 1 && object.Equals(PurgeType, PurgeTypeList.ApplicationCode))
			{
				PurgeTimeInfo.AddError(Res.GetString("322afde2-70aa-4acb-af90-c5462be59119", "1 week is the minimum allowed."));
			}
		}

		#endregion

		#region PurgeTimeUnit

		[List("PurgeTimeUnits")]
		[ReadOnlyMember(nameof(PurgeTime_ReadOnly))]
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

			if (!isUnpurgable)
			{
				if (object.Equals(PurgeType, PurgeTypeList.ApplicationCode))
				{
					MandatoryValidation.CheckEntered(PurgeTimeUnitInfo);
				}

				ListValidation.ErrorIfInvalidPK(PurgeTimeUnitInfo, PurgeTimeUnits);
			}
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

		#region PurgeType

		public ZString PurgeTypeDescription
		{
			get { return purgeTypeDescription; }
			private set { SetNonPersistentPropertyValue(PurgeTypeDescriptionInfo, ref purgeTypeDescription, value); }
		}
		ZString purgeTypeDescription;

		public ZPropertyInfo PurgeTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.PurgeTypeDescription); }
		}

		public ICodeDescription PurgeType
		{
			get { return PurgeTypeList.GetPurgeType(purgeTypeDescription); }
			set
			{
				PurgeTypeDescription = value.Description;
			}
		}

		#endregion

		#region Selected

		[ReadOnlyMember(nameof(IsUnpurgable))]
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

		#region IsUnpurgable

		public ZBool IsUnpurgable
		{
			get { return isUnpurgable; }
			set { SetNonPersistentPropertyValue(IsUnpurgableInfo, ref isUnpurgable, value); }
		}
		ZBool isUnpurgable;

		public ZPropertyInfo IsUnpurgableInfo
		{
			get { return GetZPropertyInfo(nameof(IsUnpurgable)); }
		}

		#endregion

		#endregion

		#region Override

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			Selected = true;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ApplicationCodeObj();
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.ApplicationCode, ApplicationCode);
			writer.WriteElementString(Schema.PurgeTypeDescription, PurgeTypeDescription);
			writer.WriteElementString(Schema.PurgeTime, PurgeTime.ToString());
			writer.WriteElementString(Schema.PurgeTimeUnit, PurgeTimeUnit.ToString());
			writer.WriteElementString(Schema.Selected, Selected.ToString());
			writer.WriteElementString(Schema.LatestPurgedMessageTimeUtc, LatestPurgedMessageTimeUtc.ToString());
			InterchangesSerialiser.Serialize(writer, Interchanges);
			MessageTypesSerialiser.Serialize(writer, MessageTypes);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ApplicationCode = reader.ReadElementString(Schema.ApplicationCode);
			PurgeTypeDescription = reader.ReadElementString(Schema.PurgeTypeDescription);
			PurgeTime = new ZShort(reader.ReadElementString(Schema.PurgeTime));
			PurgeTimeUnit = new ZGuid(reader.ReadElementString(Schema.PurgeTimeUnit));
			var preSelect = reader.ReadElementString(Schema.Selected);
			Selected = preSelect.IsNullOrEmpty() ? true : new ZBool(preSelect);
			LatestPurgedMessageTimeUtc = new ZDateTime(reader.ReadElementString(Schema.LatestPurgedMessageTimeUtc));
			SetInterchanges((InterchangeObjCollection)InterchangesSerialiser.Deserialize(reader));
			SetMessageTypes((MessageTypeObjCollection)MessageTypesSerialiser.Deserialize(reader));
		}

		ZXmlSerializer InterchangesSerialiser
		{
			get { return interchangesSerialiser ?? (interchangesSerialiser = ZXmlSerializer.New(typeof(InterchangeObjCollection))); }
		}
		ZXmlSerializer interchangesSerialiser;

		ZXmlSerializer MessageTypesSerialiser
		{
			get { return messageTypesSerialiser ?? (messageTypesSerialiser = ZXmlSerializer.New(typeof(MessageTypeObjCollection))); }
		}
		ZXmlSerializer messageTypesSerialiser;

		public override bool ReadOnly
		{
			get { return IsUnpurgable; }
		}

		#endregion
	}
}
