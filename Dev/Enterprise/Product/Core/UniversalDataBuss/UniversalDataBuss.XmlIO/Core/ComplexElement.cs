using System;
using System.Reflection;

namespace Enterprise.UniversalDataBuss.XmlIO
{
	abstract class ComplexElement : Element
	{
		protected ComplexElement(PropertyInfo propertyInfo, ElementProcessor processor)
			: base(propertyInfo, processor)
		{
		}

		protected ComplexElement(string elementName, ElementProcessor processor)
			: base(elementName, processor)
		{
		}

		protected Element GetNewObjectElement(PropertyInfo propertyInfo, Type propertyType)
		{
			if (Processor.ShouldFlattenToAttributes(propertyType, out string basePropertyName))
			{
				return GetNewFlattenedElement(propertyInfo, propertyType, basePropertyName);
			}
			else
			{
				return GetNewComplexElement(propertyInfo, propertyType);
			}
		}

		protected abstract Element GetNewComplexElement(PropertyInfo propertyInfo, Type calculatedPropertyType);

		protected abstract Element GetNewFlattenedElement(PropertyInfo propertyInfo, Type calculatedPropertyType, string baseElementPropertyName);
	}
}
