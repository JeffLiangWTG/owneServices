using System.Reflection;
using Enterprise.DocumentEngine.ReflectiveFieldMap;

namespace Enterprise.DocumentVisualizer.Business.Reflection
{
	public sealed class VisualizerDocDataReflector : DocDataReflector
	{
		public VisualizerDocDataReflector(MemberDescription memberDescription)
		: base(memberDescription)
		{
		}

		protected override PropertyDescription CreatePropertyDescription(PropertyInfo property, MemberBelongsTo memberBelongsTo)
		{
			return new VisualizerPropertyDescription(property, parentMember, GetHelpText(property), macroTags, filter, ShowIndex, DefaultIndex) { MemberBelongsTo = memberBelongsTo };
		}
	}
}
