using System;
using System.Reflection;
using Enterprise.DocumentEngine.ReflectiveFieldMap;

namespace Enterprise.DocumentVisualizer.Business.Reflection
{
	public sealed class VisualizerDocDataProviderReflector : DocDataProviderReflector
	{
		public VisualizerDocDataProviderReflector(Type type)
			: base(type, new VisualizerDataReflectorFilter(), MemberDescription.MacroTagTypes.Document)
		{
		}

		protected override PropertyDescription CreatePropertyDescription(PropertyInfo property, MemberBelongsTo memberBelongsTo)
		{
			return new VisualizerPropertyDescription(property, parentMember, GetHelpText(property), macroTags, filter, ShowIndex, DefaultIndex) { MemberBelongsTo = memberBelongsTo };
		}
	}
}
