using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class MexicoNotificationRemainingFolioConfiguration : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string FoliosQuantity = "FoliosQuantity";
			public const string Interval = "Interval";
		}

		#endregion

		public MexicoNotificationRemainingFolioConfiguration()
		{
		}

		public MexicoNotificationRemainingFolioConfiguration(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		MexicoNotificationRemainingFolioConfigurationValidation Validation => new MexicoNotificationRemainingFolioConfigurationValidation(this);

		#region Xml Serialization

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Interval = reader.ReadElementStringAsZInt(Schema.Interval);
			FoliosQuantity = reader.ReadElementStringAsZInt(Schema.FoliosQuantity);
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Interval, Interval.ToString());
			writer.WriteElementString(Schema.FoliosQuantity, FoliosQuantity.ToString());
		}

		#endregion

		#region GetClone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new MexicoNotificationRemainingFolioConfiguration(fallbackLevel);
		}

		#endregion

		#region FoliosQuantity

		[ResourceStringData("MexicoNotificationRemainingFolioConfigurationControl|FoliosQuantity", Caption = "Quantity of remaining 'Folios/Timbres'")]
		public ZInt FoliosQuantity
		{
			get { return foliosQuantity; }
			set
			{
				SetNonPersistentPropertyValue(FoliosQuantityInfo, ref foliosQuantity, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateFoliosQuantityAndInterval();
				}
			}
		}

		ZInt foliosQuantity;

		public ZPropertyInfo FoliosQuantityInfo
		{
			get { return GetZPropertyInfo(Schema.FoliosQuantity); }
		}

		#endregion

		#region Interval

		[ResourceStringData("MexicoNotificationRemainingFolioConfigurationControl|Interval", Caption = "Interval")]
		public ZInt Interval
		{
			get { return interval; }
			set
			{
				SetNonPersistentPropertyValue(IntervalInfo, ref interval, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateFoliosQuantityAndInterval();
				}
			}
		}

		ZInt interval;

		public ZPropertyInfo IntervalInfo
		{
			get { return GetZPropertyInfo(Schema.Interval); }
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateFoliosQuantityAndInterval();
		}
	}
}
