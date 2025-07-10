using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	#region Rounding Modes

	public static class RoundingModes
	{
		public const string Up = "RUP";
		public const string Down = "RDN";
		public const string BankersRounding = "BNK";
	}

	public class RoundingModeList : CodeDescriptionPairList
	{
		public RoundingModeList()
		{
			AddPair(RoundingModes.Up, ResString.GetMultilingualString("4ebd9c1c-7d4c-4975-a3b4-be67f4c60eb2", "Round Up"));
			AddPair(RoundingModes.Down, ResString.GetMultilingualString("6ac151f6-3972-4399-bff1-aaf9bbb7a36e", "Round Down"));
			AddPair(RoundingModes.BankersRounding, ResString.GetMultilingualString("7077dc7a-5bd3-4f30-b87c-d2f341ebe661", "Banker's Rounding"));
		}
	}

	#endregion

	#region Unit of Measure Types

	public class UnitOfMeasureTypeList : CodeDescriptionPairList
	{
		public UnitOfMeasureTypeList()
		{
			AddRange(new CodeDescriptionPairList(OLookUpEditType.Weight));
			AddRange(new CodeDescriptionPairList(OLookUpEditType.Volume));
			AddRange(new CodeDescriptionPairList(OLookUpEditType.Length));
		}
	}

	#endregion

	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class DefaultNumberOfDecimals : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string UnitOfMeasure = "UnitOfMeasure";
			public const string TransportMode = "TransportMode";
			public const string NumberOfDecimals = "NumberOfDecimals";
			public const string RoundingMode = "RoundingMode";

			public const int UnitOfMeasureMaxLength = 2;
			public const int TransportModeMaxLength = 3;
			public const int RoundingModeMaxLength = 3;
			public const int DefaultNumberOfDecimalsForWeightAndVolumeUnits = 3;
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DefaultNumberOfDecimals();
		}

		#endregion

		#region Properties

		#region UnitOfMeasure

		[CargoWise.ComponentModel.MaxLength(DefaultNumberOfDecimals.Schema.UnitOfMeasureMaxLength)]
		public ZString UnitOfMeasure
		{
			get { return unitOfMeasure; }
			set
			{
				CheckMaximumLength(UnitOfMeasureInfo, value);
				unitOfMeasure = value;
				UnitOfMeasureInfo.RefreshBinding();
				ValidateUnitOfMeasure();
			}
		}
		ZString unitOfMeasure;

		public ZPropertyInfo UnitOfMeasureInfo
		{
			get { return GetZPropertyInfo(DefaultNumberOfDecimals.Schema.UnitOfMeasure); }
		}

		public void ValidateUnitOfMeasure()
		{
			if (!IsValidationSuspended)
			{
				UnitOfMeasureInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(UnitOfMeasureInfo);
				ListValidation.ErrorIfInvalidCode(UnitOfMeasureInfo, UnitOfMeasureList);
				CheckForDuplicateItems(UnitOfMeasureInfo);
			}
		}

		#endregion

		#region TransportMode

		[CargoWise.ComponentModel.MaxLength(DefaultNumberOfDecimals.Schema.TransportModeMaxLength)]
		public ZString TransportMode
		{
			get { return transportMode; }
			set
			{
				CheckMaximumLength(TransportModeInfo, value);
				transportMode = value;
				TransportModeInfo.RefreshBinding();
				ValidateTransportMode();
			}
		}
		ZString transportMode;

		public ZPropertyInfo TransportModeInfo
		{
			get { return GetZPropertyInfo(DefaultNumberOfDecimals.Schema.TransportMode); }
		}

		public void ValidateTransportMode()
		{
			if (!IsValidationSuspended)
			{
				TransportModeInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(TransportModeInfo);
				ListValidation.ErrorIfInvalidCode(TransportModeInfo, TransportModeList);
				CheckForDuplicateItems(TransportModeInfo);
			}
		}

		#endregion

		#region NumberOfDecimals

		public ZInt NumberOfDecimals
		{
			get { return numberOfDecimals; }
			set
			{
				numberOfDecimals = value;
				NumberOfDecimalsInfo.RefreshBinding();
				ValidateNumberOfDecimals();
			}
		}
		ZInt numberOfDecimals;

		public ZPropertyInfo NumberOfDecimalsInfo
		{
			get { return GetZPropertyInfo(DefaultNumberOfDecimals.Schema.NumberOfDecimals); }
		}

		public void ValidateNumberOfDecimals()
		{
			if (!IsValidationSuspended)
			{
				NumberOfDecimalsInfo.ClearAllNotifications();

				if (NumberOfDecimals < 0 || NumberOfDecimals > DefaultNumberOfDecimals.Schema.DefaultNumberOfDecimalsForWeightAndVolumeUnits)
				{
					NumberOfDecimalsInfo.AddError(Res.GetString("290d6dfb-18ba-4eda-a547-170b9aeca3ba", "Please enter a numeric value between 0 and 3 for weight/volume units."));
				}
			}
		}

		#endregion

		#region RoundingMode

		[CargoWise.ComponentModel.MaxLength(DefaultNumberOfDecimals.Schema.RoundingModeMaxLength)]
		public ZString RoundingMode
		{
			get { return roundingMode; }
			set
			{
				CheckMaximumLength(RoundingModeInfo, value);
				roundingMode = value;
				RoundingModeInfo.RefreshBinding();
				ValidateRoundingMode();
			}
		}
		ZString roundingMode;

		public ZPropertyInfo RoundingModeInfo
		{
			get { return GetZPropertyInfo(DefaultNumberOfDecimals.Schema.RoundingMode); }
		}

		public void ValidateRoundingMode()
		{
			if (!IsValidationSuspended)
			{
				RoundingModeInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(RoundingModeInfo);
				ListValidation.ErrorIfInvalidCode(RoundingModeInfo, RoundingModeList);
			}
		}

		#endregion

		#endregion

		#region Lookups

		public RoundingModeList RoundingModeList
		{
			get { return new RoundingModeList(); }
		}

		public CodeDescriptionPairList UnitOfMeasureList
		{
			get { return new UnitOfMeasureTypeList(); }
		}

		public CodeDescriptionPairList TransportModeList
		{
			get
			{
				if (transportModeList == null)
				{
					transportModeList = new CodeDescriptionPairList();
					transportModeList.AddPair(Constants.TransportModes.Air, ResString.GetMultilingualString("b36693ec-2156-4bfc-93ce-ec0d6e37522d", "Air Freight"));
					transportModeList.AddPair(Constants.TransportModes.Sea, ResString.GetMultilingualString("35161a27-519e-41ab-84c8-5d55ddc9cd2e", "Sea Freight"));
					transportModeList.AddPair(Constants.TransportModes.Road, ResString.GetMultilingualString("27a06169-e986-49fb-ae88-fac71ba6db56", "Road Freight"));
					transportModeList.AddPair(Constants.TransportModes.Rail, ResString.GetMultilingualString("8edf0c8b-4d13-4327-bacc-5a8e03953f2b", "Rail Freight"));
				}

				return transportModeList;
			}
		}
		CodeDescriptionPairList transportModeList;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateUnitOfMeasure();
			ValidateTransportMode();
			ValidateNumberOfDecimals();
			ValidateRoundingMode();
		}

		void CheckForDuplicateItems(ZPropertyInfo propertyInfo)
		{
			if (ParentCollections.Count > 0 && ((DefaultNumberOfDecimalsCollection)ParentCollections.First()).IsDuplicateItem(this))
			{
				propertyInfo.AddError(Res.GetString("566709b7-50e9-47e3-8352-eb00bcc51cfb", "Default Number of Decimal Places for this combination of Unit Of Measure and Transport Mode has already been entered. No duplicate combinations are allowed."));
			}
		}

		#endregion

		#region XML Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.UnitOfMeasure, UnitOfMeasure);
			writer.WriteElementString(Schema.TransportMode, TransportMode);
			writer.WriteElementString(Schema.NumberOfDecimals, NumberOfDecimals.ToString());
			writer.WriteElementString(Schema.RoundingMode, RoundingMode);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			UnitOfMeasure = reader.ReadElementString(Schema.UnitOfMeasure);
			TransportMode = reader.ReadElementString(Schema.TransportMode);
			NumberOfDecimals = reader.ReadElementStringAsZInt(Schema.NumberOfDecimals);
			RoundingMode = reader.ReadElementString(Schema.RoundingMode);
		}

		#endregion

		#region Implementation

		public static ZDecimal GetRoundedValue(ZDecimal value, ZString roundingMode, ZInt numberOfDecimals)
		{
			switch (roundingMode)
			{
				case RoundingModes.Up:
					return RoundUp(value, numberOfDecimals);

				case RoundingModes.Down:
					return RoundDown(value, numberOfDecimals);

				case RoundingModes.BankersRounding:
				default:
					return RoundUsingBankersRounding(value, numberOfDecimals);
			}
		}

		static ZDecimal RoundUp(ZDecimal value, ZInt numberOfDecimals)
		{
			var integral = Math.Truncate(value);
			var @decimal = value - integral;

			var multiplier = Convert.ToDecimal(Math.Pow(10, Convert.ToDouble(numberOfDecimals)));
			return integral + Math.Ceiling(@decimal * multiplier) / multiplier;
		}

		static ZDecimal RoundDown(ZDecimal value, ZInt numberOfDecimals)
		{
			var integral = Math.Truncate(value);
			var @decimal = value - integral;

			var multiplier = Convert.ToDecimal(Math.Pow(10, Convert.ToDouble(numberOfDecimals)));
			return integral + Math.Floor(@decimal * multiplier) / multiplier;
		}

		static ZDecimal RoundUsingBankersRounding(ZDecimal value, ZInt numberOfDecimals)
		{
			return Enterprise.ZArchitecture.Core.Utilities.Round(value, numberOfDecimals);
		}

		public static ZString GetBoundPropertyName(string propertyPath)
		{
			int separatorIndex = 0;

			if (propertyPath.Contains('.'))
			{
				separatorIndex = propertyPath.LastIndexOf('.');
			}
			if (propertyPath.Contains('+'))
			{
				separatorIndex = propertyPath.LastIndexOf('+');
			}

			return (separatorIndex > 0) ? propertyPath.Substring(separatorIndex + 1) : propertyPath;
		}

		#endregion
	}
}
