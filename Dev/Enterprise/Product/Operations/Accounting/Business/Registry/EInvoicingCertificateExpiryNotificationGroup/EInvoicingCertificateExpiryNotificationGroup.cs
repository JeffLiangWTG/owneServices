using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class EInvoicingCertificateExpiryNotificationGroup : RegistryBusinessObjectTemplate
	{
		abstract class Schema
		{
			public const string AlertDays = "AlertDays";
			public const string NotificationGroup = "NotificationGroup";
		}

		public EInvoicingCertificateExpiryNotificationGroup(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public EInvoicingCertificateExpiryNotificationGroup()
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EInvoicingCertificateExpiryNotificationGroup(fallbackLevel, factory)
			{
				AlertDays = AlertDays,
				NotificationGroup = NotificationGroup
			};
		}

		public ZInt AlertDays
		{
			get { return alertDays; }
			set
			{
				SetNonPersistentPropertyValue(AlertDaysInfo, ref alertDays, value);

				if (!IsValidationSuspended)
				{
					ValidateAlertDays();
				}
			}
		}

		ZInt alertDays;

		public ZPropertyInfo AlertDaysInfo
		{
			get { return GetZPropertyInfo(Schema.AlertDays); }
		}

		[List(nameof(NotificationGroupList))]
		[MaxLength(3)]
		public ZGuid NotificationGroup
		{
			get { return notificationGroup; }
			set
			{
				SetNonPersistentPropertyValue(NotificationGroupInfo, ref notificationGroup, value);

				if (!IsValidationSuspended)
				{
					ValidateNotificationGroup();
				}
			}
		}

		ZGuid notificationGroup;

		public ZPropertyInfo NotificationGroupInfo
		{
			get { return GetZPropertyInfo(Schema.NotificationGroup); }
		}

		public GlbGroupCollection NotificationGroupList => notificationGroupList ?? (notificationGroupList = new GlbGroupCollection(CurrentFactory));
		GlbGroupCollection notificationGroupList;

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateNotificationGroup();
			ValidateAlertDays();
		}

		void ValidateNotificationGroup()
		{
			NotificationGroupInfo.ClearAllNotifications();

			if (AlertDays > 0)
			{
				MandatoryValidation.CheckEntered(NotificationGroupInfo);
			}

			ListValidation.ErrorIfInvalidPK(NotificationGroupInfo);
		}

		void ValidateAlertDays()
		{
			AlertDaysInfo.ClearAllNotifications();

			if (AlertDays > 365)
			{
				AlertDaysInfo.AddError(Res.GetString("90665D4D-C118-4892-9FF4-D721FF194473", "The day value must be less than or equal to the maximum 365."));
			}
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			AlertDays = reader.ReadElementStringAsZInt(Schema.AlertDays);
			NotificationGroup = new ZGuid(reader.ReadElementString(Schema.NotificationGroup));
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.AlertDays, AlertDays.ToString());
			writer.WriteElementString(Schema.NotificationGroup, NotificationGroup.ToString());
		}
	}
}