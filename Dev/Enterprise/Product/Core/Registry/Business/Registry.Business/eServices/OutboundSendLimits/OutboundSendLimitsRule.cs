using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class OutboundSendLimitsRule : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string SendCountLimit = "SendCountLimit";
			public const string SendSizeLimit = "SendSizeLimit";
		}

		#endregion

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			SendCountLimit = 5;
			SendSizeLimit = 1024;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OutboundSendLimitsRule();
		}

		#region Send Count Limit

		public ZShort SendCountLimit
		{
			get { return sendCountLimit; }
			set
			{
				SetNonPersistentPropertyValue<ZShort>(SendCountLimitInfo, ref sendCountLimit, value);
				if (!IsValidationSuspended)
				{
					ValidateSendCountLimit();
				}
			}
		}

		public ZPropertyInfo SendCountLimitInfo
		{
			get { return GetZPropertyInfo(Schema.SendCountLimit, "Count Limit (Num. of EDI Interchanges)"); }
		}

		public void ValidateSendCountLimit()
		{
			SendCountLimitInfo.ClearAllNotifications();
			CompareValidation.CheckWithinRange(SendCountLimitInfo, 1, 9999);
		}

		ZShort sendCountLimit;

		#endregion

		#region Send Size Limit

		public ZInt SendSizeLimit
		{
			get { return sendSizeLimit; }
			set
			{
				SetNonPersistentPropertyValue<ZInt>(SendSizeLimitInfo, ref sendSizeLimit, value);
				if (!IsValidationSuspended)
				{
					ValidateSendSizeLimit();
				}
			}
		}

		public ZPropertyInfo SendSizeLimitInfo
		{
			get { return GetZPropertyInfo(Schema.SendSizeLimit, "Size Limit (KB)"); }
		}

		public void ValidateSendSizeLimit()
		{
			SendSizeLimitInfo.ClearAllNotifications();
			CompareValidation.CheckWithinRange(SendSizeLimitInfo, 1, 999999);
		}

		ZInt sendSizeLimit;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ClearRowNotifications();

			ValidateSendCountLimit();
			ValidateSendSizeLimit();
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.SendCountLimit, SendCountLimit.ToString());
			writer.WriteElementString(Schema.SendSizeLimit, SendSizeLimit.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			SendCountLimit = ZShort.Parse(reader.ReadElementString(Schema.SendCountLimit));
			SendSizeLimit = ZInt.Parse(reader.ReadElementString(Schema.SendSizeLimit));
		}

		#endregion

	}
}
