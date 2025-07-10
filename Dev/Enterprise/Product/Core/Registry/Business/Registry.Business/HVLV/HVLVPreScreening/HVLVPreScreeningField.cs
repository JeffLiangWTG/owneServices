using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Macros;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;
using static Enterprise.Integration.Customs.Shared;
using static Enterprise.Registry.Business.HVLVPreScreeningRule;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class HVLVPreScreeningField : RegistryBusinessObjectTemplate
	{
		#region Schema

		protected abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string FieldDescription = "FieldDescription";
			public const string ValidationRule = "ValidationRule";
			public const string DeminimusValue = "DeminimusValue";
			public const string DeminimusCurrency = "DeminimusCurrency";
			public const string IsDeminimusValueOverride = "IsDeminimusValueOverride";
			public const string CheckSameConsignee = "CheckSameConsignee";
			public const string MacrosScript = "MacrosScript";
			public const string MessageText = "MessageText";
			public const string IsMandatory = "IsMandatory";
			public const string ArrayOfHVLVPreScreeningValue = "ArrayOfHVLVPreScreeningValue";
			public const string ArrayOfHVLVPreScreeningSpecialCharacterValue = "ArrayOfHVLVPreScreeningSpecialCharacterValue";
		}

		#endregion

		#region Constants

		public abstract class Constants
		{
			public const string HVS_FormattedOriginTariff = "HVS_FormattedOriginTariff";
			public const string HVS_FormattedDestinationTariff = "HVS_FormattedDestinationTariff";
			public const string UserDefined = "UserDefined";
			public const string SpecialCharacters = "SpecialCharacters";
			public const string TotalLineCustomsValues = "TotalLineCustomsValues";
			public const string TotalLineIntrinsicValues = "TotalLineIntrinsicValues";
		}

		#endregion

		public HVLVPreScreeningField(HVLVPreScreeningRule validationRule)
		{
			HVLVPreScreeningRule = validationRule;
		}

		public HVLVPreScreeningField()
		{
		}

		#region Field Description

		[List("FieldDescriptionList")]
		public ZString FieldDescription
		{
			get { return fieldDescription; }
			set
			{
				fieldName = ZString.Empty;
				SetNonPersistentPropertyValue(FieldDescriptionInfo, ref fieldDescription, value);

				if ((IsDeminimusField || IsMacrosField || IsSpecialCharactersField) && ScreeningValues.Count > 0)
				{
					ScreeningValues.RemoveAll();
				}

				if (!IsValidationSuspended)
				{
					ValidateAll();
				}

				SetDeminimus();
			}
		}

		public ZPropertyInfo FieldDescriptionInfo => GetZPropertyInfo(Schema.FieldDescription);

		ZString fieldDescription;

		void ValidateFieldDescription()
		{
			FieldDescriptionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(FieldDescriptionInfo);
			ListValidation.ErrorIfInvalidCode(FieldDescriptionInfo, FieldDescriptionList);

			if (!FieldDescriptionInfo.HasErrors())
			{
				HVLVPreScreeningRule.ValidateDestinationCountry();

				if (!IsMacrosField)
				{
					PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(FieldDescriptionInfo);
				}
			}
		}

		protected bool FieldDescription_ReadOnly => ScreeningValueEdit();

		bool ScreeningValueEdit()
		{
			var result = false;
			if (IsDeminimusField)
			{
				result = IsDeminimusValueOverride;
			}
			else if (IsMacrosField)
			{
				result = !MacrosScript.IsEmpty;
			}
			else if (IsSpecialCharactersField)
			{
				result = !SpecialCharacters.IsEmpty;
			}
			else
			{
				result = !ScreeningValues.IsEmpty;
			}

			return result;
		}

		public ZString FieldName
		{
			get { return fieldName.IsEmpty ? fieldName = FieldDescriptionList.GetDescriptionFromCode(FieldDescription) : fieldName; }
		}

		ZString fieldName;

		#endregion

		#region Notification Type

		[List("ValidationRuleList")]
		public ZString ValidationRule
		{
			get { return validationRule; }
			set
			{
				SetNonPersistentPropertyValue(ValidationRuleInfo, ref validationRule, value);

				if (!IsValidationSuspended)
				{
					ValidateAll();
				}
			}
		}

		public ZPropertyInfo ValidationRuleInfo => GetZPropertyInfo(Schema.ValidationRule);

		ZString validationRule;

		void ValidateValidationRule()
		{
			ValidationRuleInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ValidationRuleInfo);
			ListValidation.ErrorIfInvalidCode(ValidationRuleInfo, ValidationRuleList);
		}

		#endregion

		#region Screening Value

		[BusinessObjectTestExclude]
		public HVLVPreScreeningValueCollection ScreeningValues
		{
			get { return screeningValues ?? (ScreeningValues = new HVLVPreScreeningValueCollection(this)); }
			private set
			{
				screeningValues = value;
				screeningValues.HVLVPreScreeningField = this;
				RegisterEditableChildObject(screeningValues);
			}
		}
		HVLVPreScreeningValueCollection screeningValues;

		public ZBool IsDeminimusVisible => IsDeminimusField;

		public ZBool IsHSCodeField => FieldName == HVLVItemLineSchema.Constants.HVS_OriginTariff || FieldName == HVLVItemLineSchema.Constants.HVS_DestinationTariff;

		public ZBool IsHSCodeVisible => IsHSCodeField;

		public ZBool IsScreeningValuesVisible => !IsDeminimusField && !IsMacrosField && !IsSpecialCharactersField && !IsHSCodeVisible;

		public ZBool IsSameConsigneeCheckBoxVisible => HVLVPreScreeningRule.ModuleType == ModuleTypeCodes.HVLVShipment || HVLVPreScreeningRule.ModuleType == ModuleTypeCodes.HVLVBookingHeader;

		#endregion

		#region Special Characters

		[BusinessObjectTestExclude]
		public HVLVPreScreeningSpecialCharacterValueCollection SpecialCharacters
		{
			get { return specialCharacters ?? (SpecialCharacters = new HVLVPreScreeningSpecialCharacterValueCollection(this)); }
			private set
			{
				specialCharacters = value;
				specialCharacters.HVLVPreScreeningField = this;
				RegisterEditableChildObject(specialCharacters);
			}
		}
		HVLVPreScreeningSpecialCharacterValueCollection specialCharacters;

		public ZBool IsSpecialCharactersField => FieldName == Constants.SpecialCharacters;

		public ZBool IsSpecialCharactersVisible => IsSpecialCharactersField;

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new HVLVPreScreeningField(HVLVPreScreeningRule);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			var castedClone = (HVLVPreScreeningField)clone;

			if (screeningValues != null)
			{
				castedClone.ScreeningValues = (HVLVPreScreeningValueCollection)ScreeningValues.Clone(castedClone.CurrentFallbackLevel, castedClone.Factory);
				castedClone.RegisterEditableChildObject(castedClone.ScreeningValues);
			}

			if (specialCharacters != null)
			{
				castedClone.SpecialCharacters = (HVLVPreScreeningSpecialCharacterValueCollection)SpecialCharacters.Clone(castedClone.CurrentFallbackLevel, castedClone.Factory);
				castedClone.RegisterEditableChildObject(castedClone.SpecialCharacters);
			}
		}

		#endregion

		#region XML Serialisation

		ZXmlSerializer ScreeningValuesSerialiser
		{
			get { return screeningValuesSerialiser ?? (screeningValuesSerialiser = ZXmlSerializer.New(typeof(HVLVPreScreeningValueCollection))); }
		}
		ZXmlSerializer screeningValuesSerialiser;

		ZXmlSerializer SpecialCharactersSerialiser
		{
			get { return specialCharactersSerialiser ?? (specialCharactersSerialiser = ZXmlSerializer.New(typeof(HVLVPreScreeningSpecialCharacterValueCollection))); }
		}
		ZXmlSerializer specialCharactersSerialiser;

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.FieldDescription, FieldDescription);
			writer.WriteElementString(Schema.ValidationRule, ValidationRule);
			writer.WriteElementString(Schema.DeminimusValue, DeminimusValue.ToString());
			writer.WriteElementString(Schema.IsDeminimusValueOverride, IsDeminimusValueOverride.ToString());
			writer.WriteElementString(Schema.CheckSameConsignee, CheckSameConsignee.ToString());
			writer.WriteElementString(Schema.MacrosScript, MacrosScript);
			writer.WriteElementString(Schema.MessageText, MessageText);
			writer.WriteElementString(Schema.IsMandatory, IsMandatory.ToString());
			writer.WriteElementString(Schema.DeminimusCurrency, DeminimusCurrency);
			ScreeningValuesSerialiser.Serialize(writer, ScreeningValues);
			SpecialCharactersSerialiser.Serialize(writer, SpecialCharacters);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			while (FindNextElement(reader.Reader))
			{
				switch (reader.Reader.Name)
				{
					case Schema.FieldDescription:
						FieldDescription = new ZString(reader.ReadElementString(Schema.FieldDescription));
						break;
					case Schema.ValidationRule:
						ValidationRule = new ZString(reader.ReadElementString(Schema.ValidationRule));
						break;
					case Schema.DeminimusValue:
						DeminimusValue = ZDecimal.Parse(reader.ReadElementString(Schema.DeminimusValue));
						break;
					case Schema.IsDeminimusValueOverride:
						IsDeminimusValueOverride = new ZBool(reader.ReadElementString(Schema.IsDeminimusValueOverride));
						break;
					case Schema.CheckSameConsignee:
						CheckSameConsignee = new ZBool(reader.ReadElementString(Schema.CheckSameConsignee));
						break;
					case Schema.MacrosScript:
						MacrosScript = new ZString(reader.ReadElementString(Schema.MacrosScript));
						break;
					case Schema.MessageText:
						MessageText = new ZString(reader.ReadElementString(Schema.MessageText));
						break;
					case Schema.IsMandatory:
						IsMandatory = new ZBool(reader.ReadElementString(Schema.IsMandatory));
						break;
					case Schema.DeminimusCurrency:
						DeminimusCurrency = new ZString(reader.ReadElementString(Schema.DeminimusCurrency));
						break;
					case Schema.ArrayOfHVLVPreScreeningValue:
						var screeningValueCollection = (HVLVPreScreeningValueCollection)ScreeningValuesSerialiser.Deserialize(reader);
						if (screeningValueCollection != null)
						{
							ScreeningValues = screeningValueCollection;
							RegisterEditableChildObject(ScreeningValues);
						}
						break;
					case Schema.ArrayOfHVLVPreScreeningSpecialCharacterValue:
						var specialCharacterCollection = (HVLVPreScreeningSpecialCharacterValueCollection)SpecialCharactersSerialiser.Deserialize(reader);
						if (specialCharacterCollection != null)
						{
							SpecialCharacters = specialCharacterCollection;
							RegisterEditableChildObject(SpecialCharacters);
						}
						break;
					default:
						SkipCurrentElementAsItIsNotInTheSchemaAnymore(reader);
						break;
				}
			}

			void SkipCurrentElementAsItIsNotInTheSchemaAnymore(XmlReader reader)
			{
				reader.ReadElementContentAsObject();
			}

			bool FindNextElement(XmlReader reader)
			{
				while (reader.Name.IsNullOrEmpty())
				{
					reader.Read();
				}

				return reader.NodeType == XmlNodeType.Element;
			}
		}

		#endregion

		#region Lookup

		public CodeDescriptionPairList FieldDescriptionList
		{
			get
			{
				return CurrentFactory.GetCachedValue<CodeDescriptionPairList>("HVLVPreScreening.FieldList", () =>
				{
					var consignmentType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(HVLVConsignmentSchema.Constants.Prefix);
					var itemLineType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(HVLVItemLineSchema.Constants.Prefix);

					var result = new CodeDescriptionPairList();
					result.AddPair(GetCaptionForProperty(consignmentType, HVLVConsignmentSchema.Constants.HVC_ConsigneeName), HVLVConsignmentSchema.Constants.HVC_ConsigneeName);
					result.AddPair(GetCaptionForProperty(consignmentType, HVLVConsignmentSchema.Constants.HVC_ConsigneeContact), HVLVConsignmentSchema.Constants.HVC_ConsigneeContact);
					result.AddPair(GetCaptionForProperty(consignmentType, HVLVConsignmentSchema.Constants.HVC_ConsigneeAddress1), HVLVConsignmentSchema.Constants.HVC_ConsigneeAddress1);
					result.AddPair(GetCaptionForProperty(consignmentType, HVLVConsignmentSchema.Constants.HVC_ConsigneeAddress2), HVLVConsignmentSchema.Constants.HVC_ConsigneeAddress2);
					result.AddPair(GetCaptionForProperty(consignmentType, HVLVConsignmentSchema.Constants.HVC_ConsigneeCity), HVLVConsignmentSchema.Constants.HVC_ConsigneeCity);
					result.AddPair(GetCaptionForProperty(consignmentType, HVLVConsignmentSchema.Constants.HVC_ConsigneePostcode), HVLVConsignmentSchema.Constants.HVC_ConsigneePostcode);
					result.AddPair(GetCaptionForProperty(consignmentType, HVLVConsignmentSchema.Constants.HVC_ConsigneeState), HVLVConsignmentSchema.Constants.HVC_ConsigneeState);
					result.AddPair(GetCaptionForProperty(consignmentType, HVLVConsignmentSchema.Constants.HVC_ConsigneeEmail), HVLVConsignmentSchema.Constants.HVC_ConsigneeEmail);
					result.AddPair(GetCaptionForProperty(consignmentType, HVLVConsignmentSchema.Constants.HVC_ConsigneeMobile), HVLVConsignmentSchema.Constants.HVC_ConsigneeMobile);
					result.AddPair(GetCaptionForProperty(consignmentType, HVLVConsignmentSchema.Constants.HVC_ConsigneePhone), HVLVConsignmentSchema.Constants.HVC_ConsigneePhone);
					result.AddPair(GetCaptionForProperty(consignmentType, HVLVConsignmentSchema.Constants.HVC_ConsigneeFax), HVLVConsignmentSchema.Constants.HVC_ConsigneeFax);
					result.AddPair(GetCaptionForProperty(consignmentType, HVLVConsignmentSchema.Constants.HVC_GoodsDescription), HVLVConsignmentSchema.Constants.HVC_GoodsDescription);
					result.AddPair(GetCaptionForProperty(consignmentType, HVLVConsignmentSchema.Constants.HVC_GoodsValue), HVLVConsignmentSchema.Constants.HVC_GoodsValue);
					result.AddPair(GetCaptionForProperty(consignmentType, HVLVConsignmentSchema.Constants.HVC_ShipperName), HVLVConsignmentSchema.Constants.HVC_ShipperName);
					result.AddPair(GetCaptionForProperty(consignmentType, HVLVConsignmentSchema.Constants.HVC_ShipperAddress1), HVLVConsignmentSchema.Constants.HVC_ShipperAddress1);
					result.AddPair(GetCaptionForProperty(consignmentType, HVLVConsignmentSchema.Constants.HVC_ShipperAddress2), HVLVConsignmentSchema.Constants.HVC_ShipperAddress2);
					result.AddPair(GetCaptionForProperty(consignmentType, HVLVConsignmentSchema.Constants.HVC_ShipperCity), HVLVConsignmentSchema.Constants.HVC_ShipperCity);
					result.AddPair(GetCaptionForProperty(consignmentType, HVLVConsignmentSchema.Constants.HVC_ShipperContact), HVLVConsignmentSchema.Constants.HVC_ShipperContact);
					result.AddPair(GetCaptionForProperty(consignmentType, HVLVConsignmentSchema.Constants.HVC_ShipperEmail), HVLVConsignmentSchema.Constants.HVC_ShipperEmail);
					result.AddPair(GetCaptionForProperty(consignmentType, HVLVConsignmentSchema.Constants.HVC_ShipperFax), HVLVConsignmentSchema.Constants.HVC_ShipperFax);
					result.AddPair(GetCaptionForProperty(consignmentType, HVLVConsignmentSchema.Constants.HVC_ShipperMobile), HVLVConsignmentSchema.Constants.HVC_ShipperMobile);
					result.AddPair(GetCaptionForProperty(consignmentType, HVLVConsignmentSchema.Constants.HVC_ShipperPhone), HVLVConsignmentSchema.Constants.HVC_ShipperPhone);
					result.AddPair(GetCaptionForProperty(consignmentType, HVLVConsignmentSchema.Constants.HVC_ShipperPostcode), HVLVConsignmentSchema.Constants.HVC_ShipperPostcode);
					result.AddPair(GetCaptionForProperty(consignmentType, HVLVConsignmentSchema.Constants.HVC_ShipperState), HVLVConsignmentSchema.Constants.HVC_ShipperState);
					result.AddPair(GetCaptionForProperty(consignmentType, HVLVConsignmentSchema.Constants.HVC_ConsigneeInstructions), HVLVConsignmentSchema.Constants.HVC_ConsigneeInstructions);
					result.AddPair(GetCaptionForProperty(itemLineType, Constants.HVS_FormattedOriginTariff), HVLVItemLineSchema.Constants.HVS_OriginTariff);
					result.AddPair(GetCaptionForProperty(itemLineType, Constants.HVS_FormattedDestinationTariff), HVLVItemLineSchema.Constants.HVS_DestinationTariff);
					result.AddPair(Res.GetString("8e45192f-409f-4a9e-ac05-15e27e7b414e", "User Defined"), Constants.UserDefined);
					result.AddPair(Res.GetString("1adb60c8-93ec-476e-8ff0-15c221b682fa", "Special Characters"), Constants.SpecialCharacters);
					result.AddPair(Res.GetString("6397cf7d-1fe3-45ff-9a66-f63a4d78e772", "Total Lines Customs Value"), Constants.TotalLineCustomsValues);
					result.AddPair(Res.GetString("06d07a9d-6b7d-4f03-9de5-57fa7c9498ce", "Total Lines Intrinsic Value"), Constants.TotalLineIntrinsicValues);

					string GetCaptionForProperty(Type type, ZString propertyName)
					{
						return DataBoundResourceStrings.GetDataForProperty(type, propertyName).Caption;
					}

					return result;
				});
			}
		}

		public CodeDescriptionPairList ValidationRuleList
		{
			get
			{
				return CurrentFactory.GetCachedValue<CodeDescriptionPairList>("HVLVPreScreening.ValidationRuleList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(ValidationRuleCodes.Non, ResString.GetMultilingualString("9021682c-887f-4e77-8651-7c02a6fd22d7", "No action"));
					result.AddPair(ValidationRuleCodes.Warning, ResString.GetMultilingualString("fa1afe8c-ec28-47d1-8988-5a074ea7eff8", "Add Warning validation"));
					result.AddPair(ValidationRuleCodes.Error, ResString.GetMultilingualString("a9d662ca-a46f-4d7a-91a4-a08b4af3fe51", "Add message error validation"));
					return result;
				});
			}
		}

		public static class ValidationRuleCodes
		{
			public const string Error = "ERR";
			public const string Warning = "WRN";
			public const string Non = "NON";
		}

		public IBusinessObjectCollection GoodsValueCurrencies => goodsValueCurrencies
			?? (goodsValueCurrencies = (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<MasterFiles.Integration.IRefCurrencyCollection>(), new object[] { CurrentFactory }));
		IBusinessObjectCollection goodsValueCurrencies;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateAll();
		}

		void ValidateAll()
		{
			ValidateFieldDescription();
			ValidateValidationRule();
			ValidateMessageText();
			ValidateMacroScripts();
		}

		#endregion

		#region Deminimus

		public ZDecimal DeminimusValue
		{
			get { return deminimusValue; }
			set
			{
				SetNonPersistentPropertyValue(DeminimusValueInfo, ref deminimusValue, value);

				if (!IsValidationSuspended)
				{
					ValidateAll();
				}
			}
		}
		ZDecimal deminimusValue;

		public ZPropertyInfo DeminimusValueInfo => GetZPropertyInfo(Schema.DeminimusValue);

		[List("GoodsValueCurrencies")]
		[ReadOnlyMember(nameof(IsNotDeminimusValueOverride))]
		public ZString DeminimusCurrency
		{
			get
			{
				return deminimusCurrency.IsEmpty ? deminimusCurrency = GetDeminimusCurrency() : deminimusCurrency;
			}
			set
			{
				SetNonPersistentPropertyValue(DeminimusCurrencyInfo, ref deminimusCurrency, value);
			}
		}
		ZString deminimusCurrency;

		ZString GetDeminimusCurrency()
		{
			var result = ZString.Empty;
			if (HVLVPreScreeningRule != null && !HVLVPreScreeningRule.DestinationCountryCode.IsEmpty && !HVLVPreScreeningRule.DestinationCountryCodeInfo.HasErrors())
			{
				var destinationCountry = HVLVPreScreeningRule.CountryCodes.Find(new ZQuery(RefCountrySchema.RN_Code, HVLVPreScreeningRule.DestinationCountryCode))[0];
				result = destinationCountry[RefCountrySchema.RN_RX_NKLocalCurrency.Name].ToString();
			}
			return result;
		}

		public ZPropertyInfo DeminimusCurrencyInfo => GetZPropertyInfo(Schema.DeminimusCurrency);

		public ZBool IsDeminimusValueOverride
		{
			get { return isDeminimusValueOverride; }
			set
			{
				SetNonPersistentPropertyValue(IsDeminimusValueOverrideInfo, ref isDeminimusValueOverride, value);

				if (!IsDeminimusValueOverride)
				{
					SetDeminimus();
				}
			}
		}

		public ZPropertyInfo IsDeminimusValueOverrideInfo => GetZPropertyInfo(Schema.IsDeminimusValueOverride);

		ZBool isDeminimusValueOverride;

		ZBool IsNotDeminimusValueOverride => !IsDeminimusValueOverride;

		public ZBool CheckSameConsignee
		{
			get { return checkSameConsignee; }
			set
			{
				SetNonPersistentPropertyValue(CheckSameConsigneeInfo, ref checkSameConsignee, value);
			}
		}

		public ZPropertyInfo CheckSameConsigneeInfo => GetZPropertyInfo(Schema.CheckSameConsignee);

		ZBool checkSameConsignee;

		public ZBool IsDeminimusField => FieldName == HVLVConsignmentSchema.Constants.HVC_GoodsValue
										 || FieldName == Constants.TotalLineCustomsValues
										 || FieldName == Constants.TotalLineIntrinsicValues;

		IRefCusTaxOrFeeProvider RrefCusTaxOrFee
		{
			get { return refCusTaxOrFee ?? (refCusTaxOrFee = ObjectFactory.Get<IRefCusTaxOrFeeProvider>("IRefCusTaxOrFeeProvider", new object[] { CurrentFactory })); }
		}

		IRefCusTaxOrFeeProvider refCusTaxOrFee;

		void SetDeminimus()
		{
			if (HVLVPreScreeningRule != null
			&& !HVLVPreScreeningRule.DestinationCountryCodeInfo.HasErrors()
			&& IsDeminimusField)
			{
				SetDeminimusCore();
			}
		}

		internal void SetDeminimusCore()
		{
			var countryCode = HVLVPreScreeningRule.DestinationCountryCode;
			DeminimusValue = (countryCode.IsEmpty ? ZDecimal.Zero : RrefCusTaxOrFee.LoadMostRecentEffectiveDeminimusOfCountry(countryCode).Round(2));
			DeminimusCurrencyInfo.RefreshBinding();
		}

		internal void ClearDeminimusCurrency()
		{
			deminimusCurrency = ZString.Empty;
		}

		#endregion

		#region Macros

		public ZString MacrosScript
		{
			get { return macrosScript; }
			set
			{
				SetNonPersistentPropertyValue(MacrosScriptInfo, ref macrosScript, value);

				if (!IsValidationSuspended)
				{
					ValidateMacroScripts();
				}
			}
		}
		ZString macrosScript;

		void ValidateMacroScripts()
		{
			MacrosScriptInfo.ClearAllNotifications();

			if (IsMacrosField)
			{
				MandatoryValidation.CheckEntered(MacrosScriptInfo, Res.GetString("b21bb494-2168-4d6b-bf46-adc41e92a464", "Macro"));
			}

			if (!macrosScript.IsEmpty)
			{
				try
				{
					var expression = ObjectFactory.Get<ITextMacroProcessor>().Replace(macrosScript, new BusinessObject[] { new PreScreeningMacroSupportingBusinessObject() });
					expression.EvaluateDocEngineExpression(RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.Value);
				}
				catch (InvalidOperationException e)
				{
					MacrosScriptInfo.AddError(e.Message);
				}
			}
		}

		public ZPropertyInfo MacrosScriptInfo => GetZPropertyInfo(Schema.MacrosScript);

		public ZBool IsMacrosVisible => IsMacrosField;

		public ZBool IsMacrosField => FieldName == Constants.UserDefined;

		internal class PreScreeningMacroSupportingBusinessObject : NonPersistentBusinessObject
		{
		}

		#endregion

		#region MessageText

		public ZString MessageText
		{
			get { return messageText; }
			set
			{
				SetNonPersistentPropertyValue(MessageTextInfo, ref messageText, value);

				if (!IsValidationSuspended)
				{
					ValidateAll();
				}
			}
		}
		ZString messageText;

		void ValidateMessageText()
		{
			MessageTextInfo.ClearAllNotifications();
			if (IsMacrosField)
			{
				MandatoryValidation.CheckEntered(MessageTextInfo);
			}
		}

		public ZPropertyInfo MessageTextInfo => GetZPropertyInfo(Schema.MessageText);

		#endregion

		#region Mandatory

		public ZBool IsMandatory
		{
			get { return isMandatory; }
			set
			{
				SetNonPersistentPropertyValue(IsMandatoryInfo, ref isMandatory, value);
			}
		}

		ZBool isMandatory;

		public ZPropertyInfo IsMandatoryInfo => GetZPropertyInfo(Schema.IsMandatory);

		#endregion

		#region Email Notification Type & Group

		public IGlbGroup EmailNotificationGroup => HVLVPreScreeningRule.EmailNotificationGlbGroup;

		public bool NotifyStaffMember => HVLVPreScreeningRule.NotifyStaffMember;

		#endregion

		#region HS Code Hash Set

		public bool HasDuplicateToAndFromHSCodeCombination()
		{
			var combinations = new HashSet<(ZString, ZString)>();
			if (IsHSCodeVisible)
			{
				foreach (HVLVPreScreeningValue screeningValue in ScreeningValues)
				{
					var combination = (screeningValue.FromHSCode, screeningValue.ToHSCode);
					if (combinations.Contains(combination))
					{
						return true;
					}

					combinations.Add(combination);
				}
			}

			return false;
		}

		#endregion

		internal HVLVPreScreeningRule HVLVPreScreeningRule;
	}
}
