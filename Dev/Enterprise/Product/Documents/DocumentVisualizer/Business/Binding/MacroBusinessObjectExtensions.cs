using System.Collections.Generic;
using System.Linq;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentVisualizer.Business
{
	public static class MacroBusinessObjectExtensions
	{
		public static CustomBusinessObject CreateBusinessObject(this IMacroBusinessObjectProperty property)
		{
			return property != null
				? CreateBusinessObject(new[] { property })
				: CreateBusinessObject(Enumerable.Empty<IMacroBusinessObjectProperty>());
		}

		public static CustomBusinessObject CreateBusinessObject(this IEnumerable<IMacroBusinessObjectProperty> properties)
		{
			var propertyCollection = new MacroBusinessObjectPropertyCollection(properties);

			return new CustomBusinessObject(null, propertyCollection);
		}
	}
}
