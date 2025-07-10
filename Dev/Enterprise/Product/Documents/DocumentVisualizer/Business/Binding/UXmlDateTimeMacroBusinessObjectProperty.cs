using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.DocumentVisualizer.Business.Binding
{
	sealed class UXmlDateTimeMacroBusinessObjectProperty : IMacroBusinessObjectProperty
	{
		public UXmlDateTimeMacroBusinessObjectProperty(IMacroBusinessObjectProperty macroBusinessObjectProperty)
		{
			this.innerMacroBusinessObjectProperty = macroBusinessObjectProperty;
		}

		readonly IMacroBusinessObjectProperty innerMacroBusinessObjectProperty;

		public string PropertyName => innerMacroBusinessObjectProperty.PropertyName;

		public Type PropertyType
		{
			get
			{
				return Value == null ? typeof(ZDateTime) : Value.GetType();
			}
		}

		public string DisplayName => innerMacroBusinessObjectProperty.DisplayName;

		public object Value
		{
			get
			{
				var baseValue = innerMacroBusinessObjectProperty.Value;
				return ((UXmlDateTime?)baseValue)?.ZDate;
			}
			set
			{
				innerMacroBusinessObjectProperty.Value = value switch
				{
					ZDateTime time => new UXmlDateTime(time),
					ZDateTimeOffset offset => new UXmlDateTime(offset),
					_ => innerMacroBusinessObjectProperty.Value
				};
			}
		}

		public IReadOnlyCollection<DynamicMetaData> MetaData => innerMacroBusinessObjectProperty.MetaData;

		public void Validate(ZPropertyInfo propertyInfo)
		{
			innerMacroBusinessObjectProperty.Validate(propertyInfo);
		}
	}
}
