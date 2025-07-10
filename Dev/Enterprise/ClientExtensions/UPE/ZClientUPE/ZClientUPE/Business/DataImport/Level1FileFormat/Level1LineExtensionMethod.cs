using System.Collections.Generic;

namespace Enterprise.Client.UPE.Business.DataImport.Level1FileFormat
{
	public static class Level1LineExtensionMethod
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static void AddOrAppend<TKey, TValue>(this IDictionary<TKey, ISet<TValue>> dictionary, TKey key, TValue value)
		{
			if (dictionary.TryGetValue(key, out var valueCollection))
			{
				valueCollection.Add(value);
			}
			else
			{
				valueCollection = new HashSet<TValue> { value };
				dictionary.Add(key, valueCollection);
			}
		}

		public static bool IsGCCChild(this Level1Record record) => record?._200000?.IsGCCChild ?? false;

		public static bool IsGCCLead(this Level1Record record) => record?._200000?.IsGCCLead ?? false;
	}
}
