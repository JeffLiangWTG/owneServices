using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Accounting.Business.Res;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class InvoiceCopy : RegistryBusinessObjectTemplate
	{
		public InvoiceCopy() : base()
		{
			SetDefaults();
		}

		public InvoiceCopy(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
			SetDefaults();
		}

		void SetDefaults()
		{
			Company = GetCompany();
		}

		GlbCompany GetCompany()
		{
			GlbCompany result = null;

			var fallbackLevelCompanyPK = CurrentFallbackLevel?.CompanyPK(false) ?? Guid.Empty;
			if (fallbackLevelCompanyPK != Guid.Empty)
			{
				result = fallbackLevelCompanyPK == GlbCompany.CurrentCompany.PK ? GlbCompany.CurrentCompany : CurrentFactory.Load<GlbCompany>(fallbackLevelCompanyPK);
			}

			return result;
		}

		protected InvoiceCopyCollection GetParentCollection()
		{
			return (InvoiceCopyCollection)GetParentCollection(this, typeof(InvoiceCopyCollection));
		}

		#region Schema

		public abstract class Schema
		{
			public const string Name = "Name";
			public const string EnglishName = "EnglishName";
			public const string DeliveryMethod = "DeliveryMethod";
			public const string IncludeTradingTerms = "IncludeTradingTerms";
			public const string IsOriginal = "IsOriginal";
			public const string Message = "Message";
			public const string Order = "Order";
			public const int NameMaxLength = 50;
			public const int MessageMaxLength = 256;
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = new InvoiceCopy(fallbackLevel, factory);
			result.IsOriginal = IsOriginal;

			return result;
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateName();
			ValidateDeliveryMethod();
		}

		public ZBool IsOriginal;
		GlbCompany Company;

		#region Bound Properties

		#region Name

		[MaxLength(Schema.NameMaxLength)]
		public MultilingualString Name
		{
			get { return name ?? (NoResString)""; }
			set
			{
				if (value == null)
				{
					value = (NoResString)"";
				}

				CheckMaximumLength(NameInfo, value.GetUnresolvedString());
				SetNonPersistentPropertyValue(NameInfo, ref name, value, false);
				EnglishNameInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					ValidateName();
				}
			}
		}

		public ZPropertyInfo NameInfo
		{
			get { return GetZPropertyInfo(Schema.Name); }
		}

		public void ValidateName()
		{
			NameInfo.ClearAllNotifications();

			if (!IsOriginal)
			{
				MandatoryValidation.CheckEntered(NameInfo, Res.GetString("529e3e2f-abf1-44e5-b3a7-d7df7ecc5e13", "Name"));
			}
		}

		[MaxLength(Schema.NameMaxLength)]
		[ResourceStringData("b4d0b642-4212-40ab-ae84-8c4e52bad1f0", ShortCaption = "Name", Caption = "Name")]
		public ZString EnglishName
		{
			get { return Name.GetUnresolvedString(); }
			set { Name = (NoResString)value; }
		}

		public ZPropertyInfo EnglishNameInfo
		{
			get { return GetZPropertyInfo(Schema.EnglishName); }
		}

		MultilingualString name;

		#endregion

		#region Delivery Method

		[MaxLength(3)]
		[List("DeliveryMethodList")]
		public ZString DeliveryMethod
		{
			get { return deliveryMethod; }
			set
			{
				SetNonPersistentPropertyValue(DeliveryMethodInfo, ref deliveryMethod, value);
				if (!IsValidationSuspended)
				{
					ValidateDeliveryMethod();
				}
			}
		}

		public ZPropertyInfo DeliveryMethodInfo
		{
			get { return GetZPropertyInfo(Schema.DeliveryMethod); }
		}

		protected bool DeliveryMethod_ReadOnly
		{
			get { return IsOriginal; }
		}

		public void ValidateDeliveryMethod()
		{
			DeliveryMethodInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(DeliveryMethodInfo, (IMultilingualString)ResString.GetMultilingualString("870e92b1-0a33-40ee-93c9-98aed045fba4", "Delivery Method"));
			ListValidation.ErrorIfInvalidCode(DeliveryMethodInfo, DeliveryMethodList);
		}

		ZString deliveryMethod;

		#endregion

		#region Include Trading Terms

		public ZBool IncludeTradingTerms
		{
			get { return includeTradingTerms; }
			set { SetNonPersistentPropertyValue(IncludeTradingTermsInfo, ref includeTradingTerms, value); }
		}

		public ZPropertyInfo IncludeTradingTermsInfo
		{
			get { return GetZPropertyInfo(Schema.IncludeTradingTerms); }
		}

		ZBool includeTradingTerms;

		#endregion

		#region Message

		[ResourceStringData("037c9600-681f-4b98-b5bb-ea8be8ccc6e4", Caption = "Message")]
		[MaxLength(Schema.MessageMaxLength)]
		public ZString Message
		{
			get
			{
				return NeedCompliancePortugalDefaultMessage ? (NoResString)"Cópia de documento não válida para os fins previstos no regime dos bens em circulação" : (string)message;
			}
			set
			{
				SetNonPersistentPropertyValue(MessageInfo, ref message, value);
				MessageInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo MessageInfo
		{
			get { return GetZPropertyInfo(Schema.Message); }
		}

		ZString message;

		protected bool Message_ReadOnly => NeedCompliancePortugalDefaultMessage;

		bool NeedCompliancePortugalDefaultMessage => Order > 3 && Company != null && Company.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Portugal;

		#endregion

		#region Order

		[ResourceStringData("5a39d93e-6938-49ab-8952-5cadaa817226", Caption = "Order")]
		public ZInt Order
		{
			get { return order; }
			set
			{
				SetNonPersistentPropertyValue(OrderInfo, ref order, value);
				MessageInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo OrderInfo
		{
			get { return GetZPropertyInfo(Schema.Order); }
		}

		protected bool Order_ReadOnly => true;

		ZInt order;

		#endregion

		#endregion

		#region Delivery Method List

		public CodeDescriptionPairList DeliveryMethodList
		{
			get
			{
				if (fDeliveryMethodList == null)
				{
					fDeliveryMethodList = new CodeDescriptionPairList(OLookUpEditType.PrintCopyType);
				}

				return fDeliveryMethodList;
			}
		}

		CodeDescriptionPairList fDeliveryMethodList;

		#endregion

		#region ICanDelete Members

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				return ResString.GetMultilingualString("c22a1d42-0e4f-440e-8c34-34f2ff68be0f", @"This row cannot be deleted because it relates to the existing invoice in the system.
If you would like to use default values, please leave the fields blank.");
			}
		}

		public override bool CanDelete
		{
			get { return !IsOriginal; }
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Name, Name);
			writer.WriteElementString(Schema.DeliveryMethod, DeliveryMethod);
			writer.WriteElementString(Schema.IncludeTradingTerms, IncludeTradingTerms.ToString());
			writer.WriteElementString(Schema.IsOriginal, IsOriginal.ToString());
			writer.WriteElementString(Schema.Order, Order.ToString());
			writer.WriteElementString(Schema.Message, Message);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Name = (NoResString)reader.ReadElementString(Schema.Name);
			DeliveryMethod = reader.ReadElementString(Schema.DeliveryMethod);
			IncludeTradingTerms = new ZBool(reader.ReadElementString(Schema.IncludeTradingTerms));
			IsOriginal = new ZBool(reader.ReadElementString(Schema.IsOriginal));
			Order = reader.ReadElementStringAsZInt(Schema.Order);
			Message = reader.ReadElementString(Schema.Message);
		}

		#endregion
	}
}