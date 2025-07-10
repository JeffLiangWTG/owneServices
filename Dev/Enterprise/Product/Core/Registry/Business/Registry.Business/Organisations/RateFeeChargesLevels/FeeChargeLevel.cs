using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class FeeChargeLevel : RegistryBusinessObjectTemplate
	{
		#region Schema

		protected abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string LevelCode = "Code";
			public const string LevelDescription = "Description";
			public const string EnglishDescription = "EnglishDescription";
			public const string Amount1Type = "Amount1Type";
			public const string Amount1 = "Amount1";
			public const string Amount1Currency = "Amount1Currency";
			public const string Amount2Type = "Amount2Type";
			public const string Amount2 = "Amount2";
			public const string Amount2Currency = "Amount2Currency";
		}

		#endregion

		#region Properties

		#region Fee Charge Level Code

		[MaxLength(3)]
		public ZString Code
		{
			get { return code; }
			set
			{
				CheckMaximumLength(CodeInfo, value);
				SetNonPersistentPropertyValue(CodeInfo, ref code, value);

				if (!IsValidationSuspended)
				{
					ValidateCodeCore();
				}
			}
		}
		ZString code;

		public ZPropertyInfo CodeInfo
		{
			get { return GetZPropertyInfo(Schema.LevelCode); }
		}

		protected void ValidateCodeCore()
		{
			CodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CodeInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(CodeInfo);
		}

		#endregion

		#region Fee Charge Level Description

		[MaxLength(256)]
		public MultilingualString Description
		{
			get { return description ?? (NoResString)""; }
			set
			{
				if (value == null)
				{
					value = (NoResString)"";
				}

				CheckMaximumLength(DescriptionInfo, value.GetUnresolvedString());
				SetNonPersistentPropertyValue(DescriptionInfo, ref description, value, false);
				EnglishDescriptionInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					ValidateDescriptionCore();
				}
			}
		}
		MultilingualString description;

		[MaxLength(256)]
		public virtual ZString EnglishDescription
		{
			get { return Description.GetUnresolvedString(); }
			set { Description = (NoResString)value; }
		}

		public ZPropertyInfo EnglishDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.EnglishDescription); }
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.LevelDescription); }
		}

		protected void ValidateDescriptionCore()
		{
			DescriptionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(DescriptionInfo);
		}

		#endregion

		#region Amount 1 Type

		[List("Amount1TypeList")]
		[MaxLength(3)]
		public ZString Amount1Type
		{
			get { return amount1Type; }
			set
			{
				CheckMaximumLength(Amount1TypeInfo, value);
				SetNonPersistentPropertyValue(Amount1TypeInfo, ref amount1Type, value);

				if (!IsValidationSuspended)
				{
					ValidateAmount1TypeCore();
				}

				if (amount1Type == OrgConstants.ServiceLevelAmountTypes.Code.None)
				{
					Amount1 = 0;
					Amount1Currency = ZString.Empty;
					IsAmount1TypeNone = true;
				}
				else
				{
					IsAmount1TypeNone = false;
				}
			}
		}
		ZString amount1Type;

		public ZPropertyInfo Amount1TypeInfo
		{
			get { return GetZPropertyInfo(Schema.Amount1Type); }
		}

		protected void ValidateAmount1TypeCore()
		{
			ValidateAmountTypes(Amount1TypeInfo, Amount1TypeList);
		}

		#endregion

		#region Amount 1 Value

		[DecimalPlaces(2)]
		[ReadOnlyMember(nameof(IsAmount1TypeNone))]
		public ZDecimal Amount1
		{
			get { return amount1; }
			set
			{
				SetNonPersistentPropertyValue<ZDecimal>(Amount1Info, ref amount1, value);

				if (!IsValidationSuspended)
				{
					ValidateAmount1Core();
				}
			}
		}
		ZDecimal amount1;

		public ZPropertyInfo Amount1Info
		{
			get { return GetZPropertyInfo(Schema.Amount1); }
		}

		protected void ValidateAmount1Core()
		{
			Amount1Info.ClearAllNotifications();
			TypeValidation.CheckValidDecimal(Amount1Info, 9, 2);
		}

		#endregion

		#region Amount 1 Currency

		[List("CurrencyList")]
		[ReadOnlyMember(nameof(IsAmount1TypeNone))]
		public ZString Amount1Currency
		{
			get { return amount1Currency; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(Amount1CurrencyInfo, ref amount1Currency, value);

				if (!IsValidationSuspended)
				{
					ValidateAmount1CurrencyCore();
				}
			}
		}
		ZString amount1Currency;

		public ZPropertyInfo Amount1CurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.Amount1Currency); }
		}

		protected void ValidateAmount1CurrencyCore()
		{
			Amount1CurrencyInfo.ClearAllNotifications();

			if (Amount1Type != OrgConstants.ServiceLevelAmountTypes.Code.None)
			{
				ValidateCurrency(Amount1CurrencyInfo);
			}
		}

		#endregion

		#region Amount 2 Type

		[MaxLength(3)]
		[List("Amount2TypeList")]
		public ZString Amount2Type
		{
			get { return amount2Type; }
			set
			{
				CheckMaximumLength(Amount2TypeInfo, value);
				SetNonPersistentPropertyValue(Amount2TypeInfo, ref amount2Type, value);

				if (!IsValidationSuspended)
				{
					ValidateAmount2TypeCore();
				}

				if (amount2Type == OrgConstants.ServiceLevelAmountTypes.Code.None)
				{
					IsAmount2TypeNone = true;
				}
				else
				{
					IsAmount2TypeNone = false;
				}
			}
		}
		ZString amount2Type;

		public ZPropertyInfo Amount2TypeInfo
		{
			get { return GetZPropertyInfo(Schema.Amount2Type); }
		}

		protected void ValidateAmount2TypeCore()
		{
			ValidateAmountTypes(Amount2TypeInfo, Amount2TypeList);
		}

		#endregion

		#region Amount 2 Value

		[DecimalPlaces(2)]
		[ReadOnlyMember(nameof(IsAmount2TypeNone))]
		public ZDecimal Amount2
		{
			get { return amount2; }
			set
			{
				SetNonPersistentPropertyValue<ZDecimal>(Amount2Info, ref amount2, value);

				if (!IsValidationSuspended)
				{
					ValidateAmount2Core();
				}
			}
		}
		ZDecimal amount2;

		public ZPropertyInfo Amount2Info
		{
			get { return GetZPropertyInfo(Schema.Amount2); }
		}

		protected void ValidateAmount2Core()
		{
			Amount2Info.ClearAllNotifications();
			TypeValidation.CheckValidDecimal(Amount2Info, 9, 2);
		}

		#endregion

		#region Amount 2 Currency

		[List("CurrencyList")]
		[ReadOnlyMember(nameof(IsAmount2TypeNone))]
		public ZString Amount2Currency
		{
			get { return amount2Currency; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(Amount2CurrencyInfo, ref amount2Currency, value);

				if (!IsValidationSuspended)
				{
					ValidateAmount2CurrencyCore();
				}
			}
		}

		public ZPropertyInfo Amount2CurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.Amount2Currency); }
		}
		ZString amount2Currency;

		protected void ValidateAmount2CurrencyCore()
		{
			Amount2CurrencyInfo.ClearAllNotifications();

			if (Amount2Type != OrgConstants.ServiceLevelAmountTypes.Code.None)
			{
				ValidateCurrency(Amount2CurrencyInfo);
			}
		}

		#endregion

		#endregion

		#region Read Only Memebers

		public bool IsAmount1TypeNone
		{
			get
			{
				return isAmount1TypeNone;
			}
			set
			{
				isAmount1TypeNone = value;
			}
		}
		bool isAmount1TypeNone;

		public bool IsAmount2TypeNone
		{
			get
			{
				return isAmount2TypeNone;
			}
			set
			{
				isAmount2TypeNone = value;

				if (isAmount2TypeNone)
				{
					Amount2 = 0;
					Amount2Currency = ZString.Empty;
				}
			}
		}
		bool isAmount2TypeNone;

		#endregion

		#region Lookups

		public CodeDescriptionPairList Amount1TypeList
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.ServiceLevelAmount1Type); }
		}

		public CodeDescriptionPairList Amount2TypeList
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.ServiceLevelAmount2Type); }
		}

		public IActiveBusinessObjectCollection CurrencyList
		{
			get { return fCurrencyList ?? (fCurrencyList = (IActiveBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IRefCurrencyCollection>(), new object[] { CurrentFactory })); }
		}
		IActiveBusinessObjectCollection fCurrencyList;

		#endregion

		#region General Validation

		void ValidateCurrency(ZPropertyInfo currencyInfo)
		{
			currencyInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(currencyInfo);
			ListValidation.ErrorIfInvalidCode(currencyInfo, CurrencyList);
		}

		void ValidateAmountTypes(ZPropertyInfo typeInfo, CodeDescriptionPairList list)
		{
			typeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(typeInfo);
			ListValidation.ErrorIfInvalidCode(typeInfo, list);
		}

		#endregion

		#region overriden

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateCodeCore();
			ValidateDescriptionCore();
			ValidateAmount1Core();
			ValidateAmount1TypeCore();
			ValidateAmount1CurrencyCore();
			ValidateAmount2TypeCore();
			ValidateAmount2Core();
			ValidateAmount2CurrencyCore();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new FeeChargeLevel();
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.LevelCode, Code);
			writer.WriteElementString(Schema.LevelDescription, EnglishDescription);
			writer.WriteElementString(Schema.Amount1Type, Amount1Type);
			writer.WriteElementString(Schema.Amount1, Amount1.ToString());
			writer.WriteElementString(Schema.Amount1Currency, Amount1Currency.ToString());
			writer.WriteElementString(Schema.Amount2Type, Amount2Type);
			writer.WriteElementString(Schema.Amount2, Amount2.ToString());
			writer.WriteElementString(Schema.Amount2Currency, Amount2Currency.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Code = reader.ReadElementString(Schema.LevelCode);
			EnglishDescription = reader.ReadElementString(Schema.LevelDescription);
			Amount1Type = reader.ReadElementString(Schema.Amount1Type);
			Amount1 = reader.ReadElementStringAsZDecimal(Schema.Amount1);
			Amount1Currency = reader.ReadElementString(Schema.Amount1Currency);
			Amount2Type = reader.ReadElementString(Schema.Amount2Type);
			Amount2 = reader.ReadElementStringAsZDecimal(Schema.Amount2);
			Amount2Currency = reader.ReadElementString(Schema.Amount2Currency);
		}

		#endregion
	}
}
