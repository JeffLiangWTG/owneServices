using System;
using Enterprise.DocumentEngine.FlexCelInterface;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class FilterFieldDuplicatedException : TemplateDefinitionException
	{
#if NETFRAMEWORK
		protected FilterFieldDuplicatedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public FilterFieldDuplicatedException(string duplicatedFieldDisplayName)
			: base(Res.GetString("45A05F4B-91B1-44D8-A898-70D8D66B049D"
					, "There are filters using the same name [{0}], which should be fixed by updating the template."
					, duplicatedFieldDisplayName),
				new CellReference(Res.GetString("03F4FC6B-3EF9-4C0A-82F7-37AF8A14BC52", "Filter"), String.Empty))
		{
		}
	}
}
