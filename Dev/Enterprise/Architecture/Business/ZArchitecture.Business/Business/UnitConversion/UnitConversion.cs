using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.ZArchitecture.Business
{
	public class UnitConversion : NonPersistentBusinessObject, IObsoleteValidation
	{
		public static UnitConversion Create(BusinessObject dataSource, IEnumerable<PropertyDescriptor> boundProperties, PropertyDescriptor unitProperty, MeasureUnitType unitType)
		{
			var result = new UnitConversion(dataSource, boundProperties, unitProperty, unitType);
			result.Setup();

			return result;
		}

		UnitConversion(BusinessObject dataSource, IEnumerable<PropertyDescriptor> boundProperties, PropertyDescriptor unitProperty, MeasureUnitType unitType)
		{
			this.dataSource = Argument.NotNull(dataSource, "dataSource");
			this.boundProperties = Argument.NotNull(boundProperties, "boundProperties");
			this.unitProperty = Argument.NotNull(unitProperty, "unitProperty");
			this.unitType = unitType;
		}

		readonly BusinessObject dataSource;
		readonly IEnumerable<PropertyDescriptor> boundProperties;
		readonly PropertyDescriptor unitProperty;
		readonly MeasureUnitType unitType;

		#region Schema

		public abstract class Schema
		{
			public const string FromUnit = "FromUnit";
			public const string ToUnit = "ToUnit";
		}

		#endregion

		#region Setup

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception Message")]
		void Setup()
		{
			var listAttribute = (ListAttribute)unitProperty.Attributes[typeof(ListAttribute)];

			var macro = listAttribute.ListDataSourceMember.Replace('+', '.');

			var expression = macro.With<StandardLibrary>().CreateExpression();

			unitList = (IList)expression.Evaluate(dataSource);

			if (expression.Errors.Any())
			{
				var message = string.Format(@"Error processing list member {0} on data source {1}.
Errors:
{2}", listAttribute.ListDataSourceMember, dataSource.GetType().FullName, string.Join(System.Environment.NewLine, expression.Errors));
				throw new NotSupportedException(message);
			}

			var startUnit = (ZString)unitProperty.GetValue(dataSource);
			FromUnit = startUnit;

			var unitAndType = new UnitAndType(FromUnit, unitType);

			if (UnitDefaults.ContainsKey(unitAndType))
			{
				ToUnit = UnitDefaults[unitAndType];
			}

			var unitAmountCollection = new CustomUnitAmountCollection(boundProperties, dataSource, this);
			CreateUnitAmountContainerFromCollection(unitAmountCollection);
			ConvertAll();
		}

		static IReadOnlyDictionary<UnitAndType, string> UnitDefaults
		{
			get { return unitDefaults.Value; }
		}

		static readonly Lazy<IReadOnlyDictionary<UnitAndType, string>> unitDefaults = new Lazy<IReadOnlyDictionary<UnitAndType, string>>(GetUnitDefaults);

		static IReadOnlyDictionary<UnitAndType, string> GetUnitDefaults()
		{
			var result = new Dictionary<UnitAndType, string>();

			Action<MeasureUnitType, string, string> addBidirectionalMapping = (unitType, unit1, unit2) =>
			{
				var unitAndType1 = new UnitAndType(unit1, unitType);
				if (!result.ContainsKey(unitAndType1))
				{
					result.Add(unitAndType1, unit2);
				}

				var unitAndType2 = new UnitAndType(unit2, unitType);
				if (!result.ContainsKey(unitAndType2))
				{
					result.Add(unitAndType2, unit1);
				}
			};

			addBidirectionalMapping(MeasureUnitType.Area, Constants.Area.SquareCentimetre, Constants.Area.SquareInch);
			addBidirectionalMapping(MeasureUnitType.Area, Constants.Area.SquareMetre, Constants.Area.SquareFoot);
			addBidirectionalMapping(MeasureUnitType.Area, Constants.Area.SquareKilometer, Constants.Area.SquareMile);

			addBidirectionalMapping(MeasureUnitType.Length, Constants.Length.Centimetres, Constants.Length.Inches);
			addBidirectionalMapping(MeasureUnitType.Length, Constants.Length.Metres, Constants.Length.Feet);
			addBidirectionalMapping(MeasureUnitType.Length, Constants.Length.Kilometres, Constants.Length.Miles);

			addBidirectionalMapping(MeasureUnitType.Volume, Constants.Volume.CubicCentimeters, Constants.Volume.CubicInches);
			addBidirectionalMapping(MeasureUnitType.Volume, Constants.Volume.CubicMetres, Constants.Volume.CubicFeet);

			addBidirectionalMapping(MeasureUnitType.Weight, Constants.Weight.Grams, Constants.Weight.Ounces);
			addBidirectionalMapping(MeasureUnitType.Weight, Constants.Weight.Kilograms, Constants.Weight.Pounds);
			addBidirectionalMapping(MeasureUnitType.Weight, Constants.Weight.Tonnes, Constants.Weight.LongTons);

			addBidirectionalMapping(MeasureUnitType.Temperature, Constants.Temperature.Centigrade, Constants.Temperature.Fahrenheit);
			addBidirectionalMapping(MeasureUnitType.Temperature, Constants.Temperature.Kelvin, Constants.Temperature.Centigrade);

			return result;
		}

		struct UnitAndType
		{
			public UnitAndType(string unit, MeasureUnitType type)
			{
				this.unit = unit;
				this.type = type;
			}

			readonly string unit;
			readonly MeasureUnitType type;

			public override bool Equals(object obj)
			{
				var other = (UnitAndType)obj;
				return unit == other.unit && type == other.type;
			}

			public override int GetHashCode()
			{
				return unit.GetHashCode() ^ type.GetHashCode();
			}
		}

		#endregion

		public IEnumerable<PropertyDescriptor> AmountDescriptors
		{
			get { return boundProperties; }
		}

		public BusinessObject DataSource
		{
			get { return dataSource; }
		}

		public CustomBusinessObject UnitAmountContainer
		{
			get { return unitAmountContainer; }
		}

		void CreateUnitAmountContainerFromCollection(CustomUnitAmountCollection collection)
		{
			unitAmountContainer = new CustomBusinessObject(null, collection);
			unitAmountContainer.Validation.ValidateAll();
		}

		CustomBusinessObject unitAmountContainer;

		public const string FromPrefix = "From_";
		public const string ToPrefix = "To_";

		public class CustomUnitAmountCollection : CustomPropertyCollection
		{
			public CustomUnitAmountCollection(IEnumerable<PropertyDescriptor> descriptors, BusinessObject dataSource, UnitConversion parent)
			{
				this.parent = parent;

				foreach (var descriptor in descriptors)
				{
					var fromName = FromPrefix + descriptor.Name;
					var toName = ToPrefix + descriptor.Name;
					var validator = GetValidator(descriptor);
					Add(typeof(ZDecimal), fromName, validator, Array.Empty<DynamicMetaData>());
					Add(typeof(ZDecimal), toName, validator, Array.Empty<DynamicMetaData>());

					var amount = ZDecimal.Zero;
					if (descriptor.Converter.GetType() == typeof(ZIntTypeConverter))
					{
						amount = (ZDecimal)(ZInt)descriptor.GetValue(dataSource);
					}
					else
					{
						amount = (ZDecimal)descriptor.GetValue(dataSource);
					}

					propertyMap.Add(fromName, amount);
					propertyMap.Add(toName, amount);
				}
			}

			readonly UnitConversion parent;
			readonly Dictionary<string, ZDecimal> propertyMap = new Dictionary<string, ZDecimal>();

			protected override object GetValueCore(BusinessObject cusObj, string propertyName)
			{
				return propertyMap[propertyName];
			}

			protected override bool TrySetValueCore(BusinessObject cusObj, string propertyName, object value)
			{
				var newValue = (ZDecimal)value;
				propertyMap[propertyName] = newValue;
				if (propertyName.StartsWith(FromPrefix, StringComparison.OrdinalIgnoreCase))
				{
					parent.UnitAmountContainer[propertyName.Replace(FromPrefix, ToPrefix)] = parent.ConvertSafe(newValue);
				}

				return true;
			}

			Action<ZPropertyInfo> GetValidator(PropertyDescriptor descriptor)
			{
				if (descriptor.IsReadOnly)
				{
					return AddReadOnlyWarning;
				}

				return null;
			}

			void AddReadOnlyWarning(ZPropertyInfo info)
			{
				info.AddWarning(Res.GetString("2d259be7-6723-492f-bfc6-2894304079f1", "Unit Conversion will not apply to this field as it is read only."));
			}
		}

		#region Properties

		[List("UnitList")]
		public ZString FromUnit
		{
			get { return fromUnit; }
			set
			{
				fromUnit = value;
				ValidateFromUnit();

				ConvertAll();
				FromUnitInfo.RefreshBinding();
			}
		}

		ZString fromUnit;

		public ZPropertyInfo FromUnitInfo
		{
			get { return GetZPropertyInfo(Schema.FromUnit); }
		}

		[List("UnitList")]
		public ZString ToUnit
		{
			get { return toUnit; }
			set
			{
				toUnit = value;
				ValidateToUnit();

				ConvertAll();
				ToUnitInfo.RefreshBinding();
			}
		}

		ZString toUnit;

		public ZPropertyInfo ToUnitInfo
		{
			get { return GetZPropertyInfo(Schema.ToUnit); }
		}

		#endregion

		#region Validation

		public void ValidateAll()
		{
			ValidateFromUnit();
			ValidateToUnit();
		}

		void ValidateFromUnit()
		{
			FromUnitInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(FromUnitInfo);
			ListValidation.ErrorIfInvalidCode(FromUnitInfo);
		}

		void ValidateToUnit()
		{
			ToUnitInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ToUnitInfo);
			ListValidation.ErrorIfInvalidCode(ToUnitInfo);
		}

		bool HasUnitErrors
		{
			get { return FromUnitInfo.HasErrors() || ToUnitInfo.HasErrors(); }
		}

		#endregion

		[SuppressWeaklyTypedCollectionMessage]
		public IList UnitList
		{
			get { return unitList; }
		}

		IList unitList;

		Func<decimal, string, string, decimal> Convert
		{
			get
			{
				if (convert == null)
				{
					switch (unitType)
					{
						case MeasureUnitType.Area:
							convert = Constants.Area.Convert;
							break;
						case MeasureUnitType.Length:
							convert = (val, sourceUnit, targetUnit) => Constants.Length.Convert(val, sourceUnit, targetUnit);
							break;
						case MeasureUnitType.Volume:
							convert = (val, sourceUnit, targetUnit) => Constants.Volume.Convert(val, sourceUnit, targetUnit);
							break;
						case MeasureUnitType.Weight:
							convert = (val, sourceUnit, targetUnit) => Constants.Weight.Convert(val, sourceUnit, targetUnit);
							break;
						case MeasureUnitType.Temperature:
							convert = Constants.Temperature.Convert;
							break;
					}
				}

				return convert;
			}
		}

		Func<decimal, string, string, decimal> convert;

		ZDecimal ConvertSafe(ZDecimal amount)
		{
			return !HasUnitErrors ? Convert(amount, FromUnit, ToUnit) : 0;
		}

		void ConvertAll()
		{
			if (UnitAmountContainer != null)
			{
				foreach (var descriptor in boundProperties.Where(x => !x.IsReadOnly))
				{
					if (!HasUnitErrors)
					{
						var fromAmount = (ZDecimal)UnitAmountContainer[FromPrefix + descriptor.Name];
						UnitAmountContainer[ToPrefix + descriptor.Name] = Convert(fromAmount, FromUnit, ToUnit);
					}
					else
					{
						UnitAmountContainer[ToPrefix + descriptor.Name] = 0;
					}
				}
			}
		}

		public void CommitConversion()
		{
			foreach (var descriptor in boundProperties)
			{
				var amount = (ZDecimal)UnitAmountContainer[ToPrefix + descriptor.Name];
				if (descriptor.Converter.GetType() == typeof(ZIntTypeConverter))
				{
					descriptor.SetValue(dataSource, amount.ToZInt());
				}
				else
				{
					descriptor.SetValue(dataSource, amount);
				}
			}

			unitProperty.SetValue(dataSource, ToUnit);
		}
	}
}
