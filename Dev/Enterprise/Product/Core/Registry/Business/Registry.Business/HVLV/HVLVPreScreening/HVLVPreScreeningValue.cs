using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Freight.Shipment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class HVLVPreScreeningValue : RegistryBusinessObjectTemplate, ICanDelete
	{
		#region Schema

		protected abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string ScreeningValue = "ScreeningValue";
			public const string ScreeningComparisonOperatorCode = "ScreeningComparisonOperatorCode";
			public const string FromHSCode = "FromHSCode";
			public const string ToHSCode = "ToHSCode";
			public const string MessageTextPerValue = "MessageTextPerValue";
		}

		#endregion

		public HVLVPreScreeningValue(HVLVPreScreeningField screeningField)
		{
			base.SetCustomDefaultValuesCore();

			HVLVPreScreeningField = screeningField;
		}

		public HVLVPreScreeningValue()
		{
			base.SetCustomDefaultValuesCore();
		}

		#region Screening Value

		public ZString ScreeningValue
		{
			get
			{
				if (RequireTariffFormatting)
				{
					return FormatTariff(screeningValue);
				}
				return screeningValue;
			}
			set
			{
				SetNonPersistentPropertyValue(ScreeningValueInfo, ref screeningValue, value);

				if (!IsValidationSuspended)
				{
					ValidateScreeningValue();
				}
			}
		}

		public ZPropertyInfo ScreeningValueInfo => GetZPropertyInfo(Schema.ScreeningValue);

		ZString screeningValue;

		protected bool ScreeningValue_ReadOnly
		{
			get
			{
				var fieldDescriptionInfo = HVLVPreScreeningField?.FieldDescriptionInfo;
				return fieldDescriptionInfo != null && (fieldDescriptionInfo.HasErrors() || fieldDescriptionInfo.Value.IsEmpty);
			}
		}
		void ValidateScreeningValue()
		{
			if (!(HVLVPreScreeningField?.IsScreeningValuesVisible ?? false))
			{
				return;
			}

			ScreeningValueInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ScreeningValueInfo);

			if (!ScreeningValueInfo.HasErrors())
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(ScreeningValueInfo);
			}
		}

		#endregion

		#region MessageTextPerValue

		public ZString MessageTextPerValue
		{
			get { return messageTextPerValue; }
			set
			{
				SetNonPersistentPropertyValue(MessageTextPerValueInfo, ref messageTextPerValue, value);
			}
		}
		ZString messageTextPerValue;

		public ZPropertyInfo MessageTextPerValueInfo => GetZPropertyInfo(Schema.MessageTextPerValue);

		#endregion

		#region Tariff Formatting

		bool RequireTariffFormatting => HVLVPreScreeningField != null && (HVLVPreScreeningField.FieldName == HVLVItemLineSchema.Constants.HVS_OriginTariff || HVLVPreScreeningField.FieldName == HVLVItemLineSchema.Constants.HVS_DestinationTariff);

		ZString FormatTariff(ZString unformattedTariff)
		{
			var formatHelper = ObjectFactory.Get<ICommonTariffFormatter>();
			var countryCodeForFormatting = ZString.Empty;

			if (HVLVPreScreeningField != null && HVLVPreScreeningField.HVLVPreScreeningRule != null)
			{
				if (HVLVPreScreeningField.FieldName == HVLVItemLineSchema.Constants.HVS_OriginTariff)
				{
					countryCodeForFormatting = HVLVPreScreeningField.HVLVPreScreeningRule.OriginCountryCode;
				}
				else if (HVLVPreScreeningField.FieldName == HVLVItemLineSchema.Constants.HVS_DestinationTariff)
				{
					countryCodeForFormatting = HVLVPreScreeningField.HVLVPreScreeningRule.DestinationCountryCode;
				}
			}
			return formatHelper.DisplayFormat(countryCodeForFormatting, unformattedTariff);
		}

		#endregion

		#region ScreeningComparisonOperator

		[List(nameof(ScreeningComparisonOperatorsList))]
		public ZString ScreeningComparisonOperatorDescription
		{
			get { return ScreeningComparisonOperatorsList.GetDescriptionFromCode(ScreeningComparisonOperatorCode) ?? ScreeningComparisonOperatorCode; }
			set { ScreeningComparisonOperatorCode = ScreeningComparisonOperatorsList.GetCodeFromDescription(value) ?? value; }
		}

		public ZString ScreeningComparisonOperatorCode
		{
			get { return screeningComparisonOperatorCode; }
			set
			{
				screeningComparisonOperatorCode = value;
				SetNonPersistentPropertyValue(ScreeningComparisonOperatorCodeInfo, ref screeningComparisonOperatorCode, ScreeningComparisonOperatorCode);

				if (!IsValidationSuspended)
				{
					ValidateScreeningComparisonCode();
				}

				ScreeningComparisonOperatorCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ScreeningComparisonOperatorCodeInfo => GetZPropertyInfo(Schema.ScreeningComparisonOperatorCode);

		public ZWrappedPropertyInfo ScreeningComparisonOperatorDescriptionInfo => GetWrappedZPropertyInfo(Schema.ScreeningComparisonOperatorCode, v => ScreeningComparisonOperatorCodeInfo);

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			screeningComparisonOperatorCode = HVLVPreScreeningComparisonOperators.Codes.Contains;
		}

		ZString screeningComparisonOperatorCode;

		void ValidateScreeningComparisonCode()
		{
			if (!(HVLVPreScreeningField?.IsScreeningValuesVisible ?? false))
			{
				return;
			}

			ScreeningComparisonOperatorCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ScreeningValueInfo);
			ListValidation.ErrorIfInvalidCode(ScreeningComparisonOperatorCodeInfo, ScreeningComparisonOperatorsList);
		}

		#endregion

		#region From HS Code

		public ZString FromHSCode
		{
			get
			{
				if (RequireTariffFormatting)
				{
					return FormatTariff(fromHSCode);
				}
				return fromHSCode;
			}
			set
			{
				SetNonPersistentPropertyValue(FromHSCodeInfo, ref fromHSCode, value);

				if (!IsValidationSuspended)
				{
					ValidateFromToHSCode();
				}
			}
		}

		public ZPropertyInfo FromHSCodeInfo => GetZPropertyInfo(Schema.FromHSCode);

		ZString fromHSCode;

		protected bool FromHSCode_ReadOnly
		{
			get
			{
				var fieldDescriptionInfo = HVLVPreScreeningField?.FieldDescriptionInfo;
				return fieldDescriptionInfo != null && (fieldDescriptionInfo.HasErrors() || fieldDescriptionInfo.Value.IsEmpty);
			}
		}

		#endregion

		#region To HS Code

		public ZString ToHSCode
		{
			get
			{
				if (RequireTariffFormatting)
				{
					return FormatTariff(toHSCode);
				}
				return toHSCode;
			}
			set
			{
				SetNonPersistentPropertyValue(ToHSCodeInfo, ref toHSCode, value);

				if (!IsValidationSuspended)
				{
					ValidateFromToHSCode();
				}
			}
		}

		public ZPropertyInfo ToHSCodeInfo => GetZPropertyInfo(Schema.ToHSCode);

		ZString toHSCode;

		protected bool ToHSCode_ReadOnly
		{
			get
			{
				var fieldDescriptionInfo = HVLVPreScreeningField?.FieldDescriptionInfo;
				return fieldDescriptionInfo != null && (fieldDescriptionInfo.HasErrors() || fieldDescriptionInfo.Value.IsEmpty);
			}
		}

		void ValidateFromToHSCode()
		{
			if (!(HVLVPreScreeningField?.IsHSCodeVisible ?? false))
			{
				return;
			}

			FromHSCodeInfo.ClearAllNotifications();
			ToHSCodeInfo.ClearAllNotifications();
			if (FromHSCode.IsEmpty && !ToHSCode.IsEmpty)
			{
				FromHSCodeInfo.AddError(Res.GetString(
					"5449a22b-738d-4859-a330-848a4ab29f19",
					"'To HS Code' can only be entered if 'From HS Code' is also entered."));
			}
			else if (!FromHSCode.IsEmpty && !ToHSCode.IsEmpty)
			{
				if (HSCodeToLong(FromHSCode) > HSCodeToLong(ToHSCode))
				{
					FromHSCodeInfo.AddError(Res.GetString(
						"50852232-2ab2-454f-90c2-ef417feee635",
						"'From HS Code' must be less than 'To HS Code'."));
				}
			}
			else
			{
				MandatoryValidation.CheckEntered(FromHSCodeInfo);
			}

			if (!FromHSCodeInfo.HasErrors())
			{
				if (HVLVPreScreeningField?.HasDuplicateToAndFromHSCodeCombination() ?? false)
				{
					FromHSCodeInfo.AddError(Res.GetString(
					"8abad503-43b0-48d0-8d04-3e24640e44bd",
					"Same 'To HS Code' and 'From HS Code' shared across multiple validations."));
				}
			}
		}

		#endregion

		public CodeDescriptionPairList ScreeningComparisonOperatorsList
		{
			get => CurrentFactory.GetCachedValue("HVLVPreScreening.ScreeningComparisonOperatorsList", () => new HVLVPreScreeningComparisonOperators());
		}

		public bool IsMatched(ZString fieldValue)
		{
			var result = false;
			if (!fieldValue.IsEmpty)
			{
				switch (ScreeningComparisonOperatorCode)
				{
					case HVLVPreScreeningComparisonOperators.Codes.Contains:
						result = fieldValue.Contains(ScreeningValue, StringComparison.OrdinalIgnoreCase);
						break;
					case HVLVPreScreeningComparisonOperators.Codes.ExactMatch:
						result = fieldValue.EqualsIgnoringCase(ScreeningValue);
						break;
					case HVLVPreScreeningComparisonOperators.Codes.StartsWith:
						result = fieldValue.StartsWith(ScreeningValue, StringComparison.OrdinalIgnoreCase);
						break;
					case HVLVPreScreeningComparisonOperators.Codes.EndsWith:
						result = fieldValue.EndsWith(ScreeningValue, StringComparison.OrdinalIgnoreCase);
						break;
				}
			}

			return result;
		}

		public bool IsMatchedForHSCode(ZString fieldValue)
		{
			var result = false;

			if (!fieldValue.IsEmpty)
			{
				if (ToHSCode.IsEmpty)
				{
					result = fieldValue.EqualsIgnoringCase(FromHSCode);
				}
				else if (HSCodeToLong(FromHSCode) <= HSCodeToLong(fieldValue) && HSCodeToLong(fieldValue) <= HSCodeToLong(ToHSCode))
				{
					result = true;
				}
			}
			return result;
		}

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new HVLVPreScreeningValue(HVLVPreScreeningField);
		}

		#endregion

		#region XML Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.ScreeningValue, ScreeningValue);
			writer.WriteElementString(Schema.FromHSCode, FromHSCode);
			writer.WriteElementString(Schema.ToHSCode, ToHSCode);
			writer.WriteElementString(Schema.ScreeningComparisonOperatorCode, ScreeningComparisonOperatorCode);
			writer.WriteElementString(Schema.MessageTextPerValue, MessageTextPerValue);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			while (FindNextElement(reader.Reader))
			{
				switch (reader.Reader.Name)
				{
					case Schema.ScreeningValue:
						ScreeningValue = new ZString(reader.ReadElementString(Schema.ScreeningValue));
						break;
					case Schema.FromHSCode:
						FromHSCode = new ZString(reader.ReadElementString(Schema.FromHSCode));
						break;
					case Schema.ToHSCode:
						ToHSCode = new ZString(reader.ReadElementString(Schema.ToHSCode));
						break;
					case Schema.ScreeningComparisonOperatorCode:
						ScreeningComparisonOperatorCode = new ZString(reader.ReadElementString(Schema.ScreeningComparisonOperatorCode));
						break;
					case Schema.MessageTextPerValue:
						MessageTextPerValue = new ZString(reader.ReadElementString(Schema.MessageTextPerValue));
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

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateScreeningValue();
			ValidateScreeningComparisonCode();
			ValidateFromToHSCode();
		}

		#endregion

		readonly Func<ZString, long> HSCodeToLong = x => long.Parse(new string(x.ToString().Trim().Where(c => char.IsDigit(c)).ToArray()));

		internal HVLVPreScreeningField HVLVPreScreeningField;
	}
}
