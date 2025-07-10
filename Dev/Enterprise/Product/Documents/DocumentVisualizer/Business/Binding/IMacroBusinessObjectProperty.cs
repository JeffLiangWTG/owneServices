using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentVisualizer.Business
{
	public interface IMacroBusinessObjectProperty
	{
		string PropertyName { get; }
		Type PropertyType { get; }
		string DisplayName { get; }
		object Value { get; set; }

		IReadOnlyCollection<DynamicMetaData> MetaData { get; }

		void Validate(ZPropertyInfo propertyInfo);
	}
}