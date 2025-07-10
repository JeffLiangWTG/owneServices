using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI
{
	class UnitConversionHelper
	{
		internal UnitConversionHelper(ZCalcEditCore calcEditCore)
		{
			this.calcEditCore = Argument.NotNull(calcEditCore, "calcEditCore");
		}

		readonly ZCalcEditCore calcEditCore;
		ZPropertyInfo bizPropertyInfo;

		internal void ShowMeasureUnitConversion(BusinessObject dataSource, string propertyName)
		{
			if (dataSource != null && !string.IsNullOrEmpty(propertyName))
			{
				if (propertyName.Contains('+'))
				{
					var firstPlusIndex = propertyName.IndexOf('+');
					var linkedTableName = propertyName.Substring(0, firstPlusIndex);
					var linkedProperty = propertyName.Substring(firstPlusIndex + 1);

					if (ZReflection.GetPropertyIncludingNew(dataSource, linkedTableName) != null)
					{
						var linkedBizObj = dataSource[linkedTableName] as BusinessObject;
						if (bizPropertyInfo == null)
						{
							bizPropertyInfo = dataSource.FindPropertyInfo(propertyName);
						}

						if (linkedBizObj != null)
						{
							linkedBizObj.PropertyValueChanged -= PropertyValueChanged;
							linkedBizObj.PropertyValueChanged += PropertyValueChanged;
							ShowMeasureUnitConversion(linkedBizObj, linkedProperty);
						}
					}
				}
				else
				{
					var boundPropertyDescriptor = TypeDescriptor.GetProperties(dataSource)[propertyName];

					if (boundPropertyDescriptor != null)
					{
						ShowMeasureUnitConversion(dataSource, boundPropertyDescriptor);
					}
				}
			}
		}

		void ShowMeasureUnitConversion(BusinessObject dataSource, PropertyDescriptor boundPropertyDescriptor)
		{
			var measureUnitAttribute = (MeasureUnitAttribute)boundPropertyDescriptor.Attributes[typeof(MeasureUnitAttribute)];

			if (measureUnitAttribute != null)
			{
				var unitPropertyDescriptor = TypeDescriptor.GetProperties(dataSource)[measureUnitAttribute.UnitProperty];

				if (unitPropertyDescriptor != null)
				{
					var allProperties = TypeDescriptor.GetProperties(dataSource, true)
						.Cast<PropertyDescriptor>()
						.Where(x => PointsToSameUnit(x, measureUnitAttribute.UnitProperty, dataSource))
						.ToArray();

					if (allProperties.Any())
					{
						var unitConversion = UnitConversion.Create(dataSource, allProperties, unitPropertyDescriptor, measureUnitAttribute.UnitType);
						ZFormModaliser.ShowDialogAndDispose(new UnitConversionForm(unitConversion, calcEditCore));
					}
				}
			}
		}

		bool PointsToSameUnit(PropertyDescriptor propertyDescriptor, string unitProperty, object dataSource)
		{
			var attribute = (MeasureUnitAttribute)propertyDescriptor.Attributes[typeof(MeasureUnitAttribute)];
			return attribute != null && attribute.UnitProperty == unitProperty && AllowsExposure(attribute, dataSource);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception Message")]
		bool AllowsExposure(MeasureUnitAttribute attribute, object dataSource)
		{
			var exposureMember = attribute.ExposureMember;

			if (exposureMember == null)
			{
				return true;
			}

			var expression = exposureMember.With<StandardLibrary>().CreateExpression();
			var result = (bool)expression.Evaluate(dataSource);

			if (expression.Errors.Any())
			{
				var message = string.Format(@"Error evaluating exposure member {0} on data source {1}.
Errors:
{2}", exposureMember, dataSource.GetType().FullName, string.Join(System.Environment.NewLine, expression.Errors));
				throw new NotSupportedException(message);
			}

			return result;
		}

		void PropertyValueChanged(object sender, ZPropertyValueChangedEventArgs e)
		{
			if (bizPropertyInfo != null)
			{
				bizPropertyInfo.RefreshBindingForParentRelationFK();
			}
		}
	}
}
