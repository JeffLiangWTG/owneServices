namespace Enterprise.BusinessObjectGenerator.ModelView
{
	static class CSharpHelper
	{
		internal static string GetViewClassName(ModelViewContext context)
		{
			return context.ModelName;
		}

		internal static string GetFullViewClassName(ModelViewContext context)
			=> GetFullClassName(context, GetViewClassName(context));

		internal static string GetIndexedViewClassName(ModelViewContext context)
		{
			return $"{context.ModelName}_Idx";
		}

		internal static string GetFullIndexedViewClassName(ModelViewContext context)
			=> GetFullClassName(context, GetIndexedViewClassName(context));

		internal static string GetFullClassName(ModelViewContext context, string className)
		{
			var strippedNSpace = StripNamespace(ModelViewConstants.NamespacePrefix, context.DefinitionNamespace);

			if (string.IsNullOrWhiteSpace(strippedNSpace))
			{
				return className;
			}

			return string.Join(NamespaceSeparator, strippedNSpace, className);
		}

		internal const string NamespaceSeparator = ".";

		static string StripNamespace(string nSpacePrefix, string nSpace)
		{
			if (!nSpace.StartsWith(nSpacePrefix))
			{
				return nSpace;
			}

			if (nSpace.Equals(nSpacePrefix))
			{
				return null;
			}

			return nSpace.Substring(nSpacePrefix.Length).Trim(NamespaceSeparator[0]);
		}
	}
}
