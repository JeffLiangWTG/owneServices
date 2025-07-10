using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Enterprise.DocumentEngine.ReflectiveFieldMap;

namespace Enterprise.DocumentVisualizer.Business.Reflection
{
	sealed class VisualizerPropertyDescription : PropertyDescription
	{
		public VisualizerPropertyDescription(PropertyInfo property, MemberDescription parentMemberDescription, string helpText, MacroTagTypes macroTagType, IDataReflectorFilter filter, bool showIndex = true, int defaultIndex = 1)
			: base(property, parentMemberDescription, helpText, macroTagType, filter, showIndex, defaultIndex)
		{
		}

		public override (Type ChildType, Type PossibleCollectionType) GetChildTypes()
		{
			var propertyType = Property.PropertyType;

			var targetInterface = propertyType.GetInterfaces().Concat(new Type[] { propertyType }).FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>));
			if (targetInterface != null)
			{
				return (targetInterface.GetGenericArguments()[0], Filter.IsRelatedObject(propertyType) ? propertyType : null);
			}

			return base.GetChildTypes();
		}

		public override DocDataReflector DocDataReflector
		{
			get
			{
				if (docDataReflector == null)
				{
					docDataReflector = new VisualizerDocDataReflector(this);
				}
				return docDataReflector;
			}
		}
		DocDataReflector docDataReflector;
	}
}
