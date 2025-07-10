using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class ChargeCodeInvoiceTaxMessageOverride : RegistryBusinessObjectTemplate
	{
		#region Constructors

		public ChargeCodeInvoiceTaxMessageOverride()
		{
		}

		public ChargeCodeInvoiceTaxMessageOverride(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#endregion

		#region Schema

		public abstract class Schema
		{
			public const string ChargeCode = "ChargeCode";
			public const string TaxMessage = "TaxMessage";
			public const string EnglishMessage = "EnglishMessage";
			public const string LocalMessage = "LocalMessage";
			public const string LocalOverrideMessage = "LocalOverrideMessage";
			public const string EnglishOverrideMessage = "EnglishOverrideMessage";
		}

		#endregion

		#region Fields

		#region Charge Code

		ZGuid fChargeCode;

		[List("ChargeCodes")]
		public ZGuid ChargeCode
		{
			get { return fChargeCode; }
			set
			{
				SetNonPersistentPropertyValue(ChargeCodeInfo, ref fChargeCode, value);
				if (!IsValidationSuspended)
				{
					ValidateChargeCode();
				}
				ChargeCodeInfo.RefreshBinding();
			}
		}

		void ValidateChargeCode()
		{
			ChargeCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ChargeCodeInfo);
			ListValidation.ErrorIfInvalidPK(ChargeCodeInfo);
			CheckRecordIsUnique();
		}

		public ZPropertyInfo ChargeCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ChargeCode); }
		}

		public BusinessObjectCollection ChargeCodes
		{
			get
			{
				return new AccChargeCodeCollection(CurrentFactory);
			}
		}

		#endregion

		#region TaxMessage

		ZGuid fTaxMessage;

		[List("TaxMessages")]
		public ZGuid TaxMessage
		{
			get { return fTaxMessage; }
			set
			{
				SetNonPersistentPropertyValue(TaxMessageInfo, ref fTaxMessage, value);
				if (!IsValidationSuspended)
				{
					ValidateTaxMessage();
				}
				TaxMessageInfo.RefreshBinding();
				EnglishMessageInfo.RefreshBinding();
				LocalMessageInfo.RefreshBinding();
			}
		}

		void ValidateTaxMessage()
		{
			TaxMessageInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(TaxMessageInfo);
			ListValidation.ErrorIfInvalidPK(TaxMessageInfo);
			CheckRecordIsUnique();
		}

		public ZPropertyInfo TaxMessageInfo
		{
			get { return GetZPropertyInfo(Schema.TaxMessage); }
		}

		public IBusinessObjectCollection TaxMessages
		{
			get
			{
				return new AccInvMsgCollection(CurrentFactory);
			}
		}

		void CheckRecordIsUnique()
		{
			foreach (BusinessObjectCollection collection in ParentCollections)
			{
				foreach (ChargeCodeInvoiceTaxMessageOverride item in collection)
				{
					item.ClearRowNotifications();
					if (item != this && item.TaxMessage == TaxMessage && item.ChargeCode == ChargeCode)
					{
						AddRowError(DuplicateMessage);
						item.AddRowError(DuplicateMessage);
						break;
					}
				}
			}
		}

		internal static string DuplicateMessage
		{
			get { return Res.GetString("c98372ec-ac81-4645-a15c-56e74a65ed6b", "Duplicate combination of Charge Code and Tax Message."); }
		}

		BusinessObject AccInvMsg
		{
			get
			{
				BusinessObject result = null;
				if (TaxMessage.IsValid && !TaxMessage.IsEmpty)
				{
					result = CurrentFactory.Load<AccInvMsg>(TaxMessage);
				}
				return result;
			}
		}

		#region Proxy Properties for Entered Message Values

		public ZString EnglishMessage
		{
			get
			{
				ZString result = ZString.Empty;
				if (AccInvMsg != null)
				{
					result = (ZString)AccInvMsg[AccInvMsgSchema.A9_EnglishMsg];
				}
				return result;
			}
		}

		public ZPropertyInfo EnglishMessageInfo
		{
			get { return GetZPropertyInfo(Schema.EnglishMessage); }
		}

		public ZString LocalMessage
		{
			get
			{
				ZString result = ZString.Empty;
				if (AccInvMsg != null)
				{
					result = (ZString)AccInvMsg[AccInvMsgSchema.A9_LocalMsg];
				}
				return result;
			}
		}

		public ZPropertyInfo LocalMessageInfo
		{
			get { return GetZPropertyInfo(Schema.LocalMessage); }
		}

		#endregion

		#endregion

		#region Local Override Message

		ZString fLocalOverrideMessage;
		[MaxLength(AutoAccInvMsg.Schema.A9_LocalMsgMaxLength)]
		public ZString LocalOverrideMessage
		{
			get { return fLocalOverrideMessage; }
			set
			{
				SetNonPersistentPropertyValue(LocalOverrideMessageInfo, ref fLocalOverrideMessage, value);
				if (!IsValidationSuspended)
				{
					ValidateLocalOverrideMessage();
				}
				TaxMessageInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo LocalOverrideMessageInfo
		{
			get { return GetZPropertyInfo(Schema.LocalOverrideMessage); }
		}

		void ValidateLocalOverrideMessage()
		{
			MandatoryValidation.CheckEntered(LocalOverrideMessageInfo);
		}

		#endregion

		#region English Override Message

		ZString fEnglishOverrideMessage;
		[MaxLength(AutoAccInvMsg.Schema.A9_EnglishMsgMaxLength)]
		public ZString EnglishOverrideMessage
		{
			get { return fEnglishOverrideMessage; }
			set
			{
				SetNonPersistentPropertyValue(EnglishOverrideMessageInfo, ref fEnglishOverrideMessage, value);
				if (!IsValidationSuspended)
				{
					ValidateEnglishOverrideMessage();
				}
				TaxMessageInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo EnglishOverrideMessageInfo
		{
			get { return GetZPropertyInfo(Schema.EnglishOverrideMessage); }
		}

		void ValidateEnglishOverrideMessage()
		{
			MandatoryValidation.CheckEntered(EnglishOverrideMessageInfo);
		}

		#endregion

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			ClearAllNotifications();
			base.RunPreSaveValidationCore();
			ValidateChargeCode();
			ValidateEnglishOverrideMessage();
			ValidateLocalOverrideMessage();
			ValidateTaxMessage();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ChargeCodeInvoiceTaxMessageOverride(fallbackLevel, factory);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ChargeCode = new ZGuid(reader.ReadElementString(Schema.ChargeCode));
			TaxMessage = new ZGuid(reader.ReadElementString(Schema.TaxMessage));
			LocalOverrideMessage = reader.ReadElementString(Schema.LocalOverrideMessage);
			EnglishOverrideMessage = reader.ReadElementString(Schema.EnglishOverrideMessage);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.ChargeCode, ChargeCode.ToString());
			writer.WriteElementString(Schema.TaxMessage, TaxMessage.ToString());
			writer.WriteElementString(Schema.LocalOverrideMessage, LocalOverrideMessage);
			writer.WriteElementString(Schema.EnglishOverrideMessage, EnglishOverrideMessage);
		}
	}
}