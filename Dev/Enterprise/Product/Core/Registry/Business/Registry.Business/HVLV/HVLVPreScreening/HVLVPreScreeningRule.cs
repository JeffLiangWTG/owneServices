using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class HVLVPreScreeningRule : RegistryBusinessObjectTemplate, ICanDelete
	{
		#region Schema

		protected abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string TransportMode = nameof(TransportMode);
			public const string OriginCountryCode = nameof(OriginCountryCode);
			public const string DestinationCountryCode = nameof(DestinationCountryCode);
			public const string ETailer = nameof(ETailer);
			public const string ModuleType = nameof(ModuleType);
			public const string EmailNotificationType = nameof(EmailNotificationType);
			public const string EmailNotificationGroup = nameof(EmailNotificationGroup);
		}

		#endregion

		#region Constants

		public static class ModuleTypeCodes
		{
			public const string HVLVBookingHeader = "HVH";
			public const string HVLVShipment = "SHP";
		}

		#endregion

		#region TransportMode

		[List(nameof(TransportModeList))]
		public ZString TransportMode
		{
			get { return transportMode; }
			set
			{
				SetNonPersistentPropertyValue(TransportModeInfo, ref transportMode, value);

				if (!IsValidationSuspended)
				{
					ValidateAll();
				}
			}
		}

		public ZPropertyInfo TransportModeInfo => GetZPropertyInfo(Schema.TransportMode);

		public bool TransportMode_ReadOnly => ModuleType == ModuleTypeCodes.HVLVBookingHeader;

		ZString transportMode;

		void ValidateTransportMode()
		{
			TransportModeInfo.ClearAllNotifications();
			if (ETailer.IsEmpty && !ModuleType.IsEmpty && ModuleType != ModuleTypeCodes.HVLVBookingHeader)
			{
				MandatoryValidation.CheckEntered(TransportModeInfo);
			}
			ListValidation.ErrorIfInvalidCode(TransportModeInfo, TransportModeList);
		}

		#endregion

		#region eTailer

		[List(nameof(ConsignorsList))]
		[MaxLength(12)]
		public ZString ETailer
		{
			get { return eTailer; }
			set
			{
				SetNonPersistentPropertyValue(ETailerInfo, ref eTailer, value);

				if (!IsValidationSuspended)
				{
					ValidateAll();
				}
			}
		}

		public ZPropertyInfo ETailerInfo => GetZPropertyInfo(Schema.ETailer);

		ZString eTailer;

		void ValidateETailer()
		{
			ETailerInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(ETailerInfo, ConsignorsList);
		}

		#endregion

		#region Origin Country

		[List("CountryCodes")]
		[MaxLength(2)]
		public ZString OriginCountryCode
		{
			get { return originCountryCode; }
			set
			{
				SetNonPersistentPropertyValue(OriginCountryCodeInfo, ref originCountryCode, value);

				if (!IsValidationSuspended)
				{
					ValidateAll();
				}
			}
		}

		public ZPropertyInfo OriginCountryCodeInfo => GetZPropertyInfo(Schema.OriginCountryCode);

		ZString originCountryCode;

		void ValidateOriginCountry()
		{
			OriginCountryCodeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(OriginCountryCodeInfo, CountryCodes);
		}

		#endregion

		#region Destination Country

		[List("CountryCodes")]
		[MaxLength(2)]
		public ZString DestinationCountryCode
		{
			get { return destinationCountryCode; }
			set
			{
				SetNonPersistentPropertyValue(DestinationCountryCodeInfo, ref destinationCountryCode, value);

				if (!IsValidationSuspended)
				{
					ValidateAll();
				}

				SetDeminimus();
			}
		}

		public ZPropertyInfo DestinationCountryCodeInfo => GetZPropertyInfo(Schema.DestinationCountryCode);

		ZString destinationCountryCode;

		internal void ValidateDestinationCountry()
		{
			DestinationCountryCodeInfo.ClearAllNotifications();

			if (Fields.Cast<HVLVPreScreeningField>().Any(n => n.IsDeminimusField))
			{
				MandatoryValidation.CheckEntered(DestinationCountryCodeInfo);
			}

			ListValidation.ErrorIfInvalidCode(DestinationCountryCodeInfo, CountryCodes);
		}

		void SetDeminimus()
		{
			var goodsValueField = Fields.Cast<HVLVPreScreeningField>().Where(n => n.IsDeminimusField)?.FirstOrDefault();

			if (goodsValueField != null)
			{
				goodsValueField.ClearDeminimusCurrency();
				if (!DestinationCountryCodeInfo.HasErrors())
				{
					goodsValueField.SetDeminimusCore();
				}
			}
		}

		protected bool DestinationCountryCode_ReadOnly => !DestinationCountryCodeInfo.HasErrors() && Fields.Cast<HVLVPreScreeningField>().Any(n => n.IsDeminimusValueOverride);

		#endregion

		#region Module Type

		[List(nameof(ModuleTypeList))]
		public ZString ModuleType
		{
			get { return moduleType; }
			set
			{
				SetNonPersistentPropertyValue(ModuleTypeInfo, ref moduleType, value);

				if (!IsValidationSuspended)
				{
					ValidateAll();
				}

				if (moduleType == ModuleTypeCodes.HVLVBookingHeader)
				{
					TransportMode = ZString.Empty;
				}
			}
		}

		public ZPropertyInfo ModuleTypeInfo => GetZPropertyInfo(Schema.ModuleType);

		ZString moduleType;

		void ValidateModuleType()
		{
			ModuleTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ModuleTypeInfo);
			ListValidation.ErrorIfInvalidCode(ModuleTypeInfo, ModuleTypeList);
		}

		public bool isApplyBookingHeader => ModuleType == ModuleTypeCodes.HVLVBookingHeader;

		public bool isApplyShipment => ModuleType == ModuleTypeCodes.HVLVShipment;

		internal void SetDefaultModuleType()
		{
			moduleType = ModuleTypeCodes.HVLVShipment;
		}

		#endregion

		#region Email Notification Type

		[List(nameof(EmailNotificationTypesList))]
		public ZString EmailNotificationType
		{
			get { return emailNotificationType; }
			set
			{
				SetNonPersistentPropertyValue(EmailNotificationTypeInfo, ref emailNotificationType, value);

				if (!IsValidationSuspended)
				{
					ValidateAll();
				}

				if (emailNotificationType != Core.Constants.EmailTo.NominatedGroup
					&& emailNotificationType != Core.Constants.EmailTo.StaffMemberAndNominatedGroup)
				{
					EmailNotificationGroup = ZString.Empty;
				}
			}
		}

		ZString emailNotificationType;

		public ZPropertyInfo EmailNotificationTypeInfo => GetZPropertyInfo(Schema.EmailNotificationType);

		void ValidateEmailNotificationType()
		{
			EmailNotificationTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(EmailNotificationTypeInfo);
			ListValidation.ErrorIfInvalidCode(EmailNotificationTypeInfo, EmailNotificationTypesList);
		}

		internal void SetDefaultEmailNotificationType()
		{
			emailNotificationType = Core.Constants.EmailTo.NoEmails;
		}

		#endregion

		#region Email Notification Group

		[List(nameof(EmailNotificationGroupsList))]
		public ZString EmailNotificationGroup
		{
			get { return emailNotificationGroup; }
			set
			{
				SetNonPersistentPropertyValue(EmailNotificationGroupInfo, ref emailNotificationGroup, value);

				if (!IsValidationSuspended)
				{
					ValidateAll();
				}
			}
		}

		ZString emailNotificationGroup;

		public ZPropertyInfo EmailNotificationGroupInfo => GetZPropertyInfo(Schema.EmailNotificationGroup);

		public bool EmailNotificationGroup_ReadOnly => EmailNotificationType != Core.Constants.EmailTo.NominatedGroup && EmailNotificationType != Core.Constants.EmailTo.StaffMemberAndNominatedGroup;

		void ValidateEmailNotificationGroup()
		{
			EmailNotificationGroupInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(EmailNotificationGroupInfo, EmailNotificationGroupsList);
		}

		public IGlbGroup EmailNotificationGlbGroup
		{
			get
			{
				if (EmailNotificationType == Core.Constants.EmailTo.NominatedGroup || EmailNotificationType == Core.Constants.EmailTo.StaffMemberAndNominatedGroup)
				{
					return CurrentFactory.LoadFromNaturalKey<IGlbGroup>(GlbGroupSchema.GG_Code, EmailNotificationGroup);
				}

				return null;
			}
		}

		public bool NotifyStaffMember
		{
			get
			{
				return emailNotificationType == Core.Constants.EmailTo.StaffMember || emailNotificationType == Core.Constants.EmailTo.StaffMemberAndNominatedGroup;
			}
		}

		#endregion

		#region Fields

		[BusinessObjectTestExclude]
		public HVLVPreScreeningFieldCollection Fields
		{
			get { return fields ?? (Fields = new HVLVPreScreeningFieldCollection(this)); }
			private set
			{
				fields = value;
				fields.HVLVPreScreeningRule = this;
				RegisterEditableChildObject(fields);
			}
		}
		HVLVPreScreeningFieldCollection fields;

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new HVLVPreScreeningRule();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			var castedClone = (HVLVPreScreeningRule)clone;

			if (fields != null)
			{
				castedClone.Fields = (HVLVPreScreeningFieldCollection)Fields.Clone(castedClone.CurrentFallbackLevel, castedClone.Factory);
				castedClone.RegisterEditableChildObject(castedClone.Fields);
			}
		}

		#endregion

		#region XML Serialisation

		ZXmlSerializer FieldsSerialiser
		{
			get { return fieldsSerialiser ?? (fieldsSerialiser = ZXmlSerializer.New(typeof(HVLVPreScreeningFieldCollection))); }
		}
		ZXmlSerializer fieldsSerialiser;

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.TransportMode, TransportMode);
			writer.WriteElementString(Schema.ETailer, ETailer);
			writer.WriteElementString(Schema.OriginCountryCode, OriginCountryCode);
			writer.WriteElementString(Schema.DestinationCountryCode, DestinationCountryCode);
			writer.WriteElementString(Schema.ModuleType, ModuleType);
			writer.WriteElementString(Schema.EmailNotificationType, EmailNotificationType);
			writer.WriteElementString(Schema.EmailNotificationGroup, EmailNotificationGroup);
			FieldsSerialiser.Serialize(writer, Fields);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			TransportMode = reader.ReadElementString(Schema.TransportMode);
			ETailer = reader.ReadElementString(Schema.ETailer);
			OriginCountryCode = reader.ReadElementString(Schema.OriginCountryCode);
			DestinationCountryCode = reader.ReadElementString(Schema.DestinationCountryCode);
			ModuleType = reader.ReadElementString(Schema.ModuleType);
			EmailNotificationType = reader.ReadElementString(Schema.EmailNotificationType);
			EmailNotificationGroup = reader.ReadElementString(Schema.EmailNotificationGroup);
			Fields = (HVLVPreScreeningFieldCollection)FieldsSerialiser.Deserialize(reader);
			RegisterEditableChildObject(Fields);
		}

		#endregion

		#region Lookups

		public IBusinessObjectCollection CountryCodes
		{
			get
			{
				if (fCountryCodes == null)
				{
					fCountryCodes = (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IRefCountryCollection>(), RegistryFactory.Instance);
				}
				return fCountryCodes;
			}
		}
		IBusinessObjectCollection fCountryCodes;

		public CodeDescriptionPairList TransportModeList
		{
			get
			{
				return CurrentFactory.GetCachedValue("HVLVPreScreening.TransportModeList", () => new CodeDescriptionPairList(OLookUpEditType.TransportType));
			}
		}

		public BusinessObjectCollection ConsignorsList
		{
			get
			{
				if (fConsignors == null)
				{
					fConsignors = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IOrgHeaderCollection>(), RegistryFactory.Instance);
					fConsignors.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property3", ZBool.True, false));
				}
				return fConsignors;
			}
		}
		BusinessObjectCollection fConsignors;

		public CodeDescriptionPairList ModuleTypeList
		{
			get
			{
				return CurrentFactory.GetCachedValue("HVLVPreScreening.ModuleTypeList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(ModuleTypeCodes.HVLVBookingHeader, ResString.GetMultilingualString("729cff4f-d567-4a15-8923-163e3bd872f9", "HVLV Booking Header"));
					result.AddPair(ModuleTypeCodes.HVLVShipment, ResString.GetMultilingualString("a69064ba-b70c-4061-b53f-e5793641f2a8", "HVLV Shipment"));
					return result;
				});
			}
		}

		public CodeDescriptionPairList EmailNotificationTypesList
		{
			get
			{
				return CurrentFactory.GetCachedValue("HVLVPreScreening.EmailNotificationTypesList", () => new CodeDescriptionPairList(OLookUpEditType.EmailTo));
			}
		}

		public BusinessObjectCollection EmailNotificationGroupsList
		{
			get
			{
				if (fEmailNotificationGroups == null)
				{
					fEmailNotificationGroups = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IGlbGroupCollection>(), RegistryFactory.Instance);
				}
				return fEmailNotificationGroups;
			}
		}
		BusinessObjectCollection fEmailNotificationGroups;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateAll();
		}

		void ValidateAll()
		{
			ValidateEmailNotificationType();
			ValidateEmailNotificationGroup();
			ValidateModuleType();
			ValidateTransportMode();
			ValidateOriginCountry();
			ValidateDestinationCountry();
			ValidateETailer();
			CheckDuplicateRules();
		}

		void CheckDuplicateRules()
		{
			ClearRowNotifications();

			if (!TransportModeInfo.HasErrors()
				&& !OriginCountryCodeInfo.HasErrors()
				&& !DestinationCountryCodeInfo.HasErrors()
				&& !ETailerInfo.HasErrors()
				&& !EmailNotificationTypeInfo.HasErrors()
				&& !EmailNotificationGroupInfo.HasErrors()
				&& ParentCollections.Count > 0
				&& ((HVLVPreScreeningRuleCollection)ParentCollections.First()).IsDuplicateRule(this))
			{
				AddRowError(UniquenessVerificationMessage);
			}
		}

		public static string UniquenessVerificationMessage =>
			Res.GetString("c9077e76-847e-42c4-9ec7-8e0fea7c26f9", "Duplicate value combination (Transport Mode,eTailer,Origin Country/Region,Destination Country/Region,Module Type,Email Notification Type,Email Notification Group) are entered.");

		#endregion
	}
}
