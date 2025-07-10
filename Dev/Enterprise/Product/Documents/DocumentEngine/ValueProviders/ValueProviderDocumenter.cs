using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine
{
	public class ValueProviderDocumenter
	{
		/// <summary>
		/// Provides information to the DocumentEngine so that it can generate a manual for the useage of our DocumentEngine macros on Reports and Documents.
		/// </summary>
		/// <param name="useage">A syntax definition including the outer angle brackets. Put field names in as {fieldname} and leave out unnecessary spaces.</param>
		/// <param name="explanation">A clear description of what this macro is used for, and how to use it. </param>
		/// <param name="examples">Include examples at the bottom if necessary.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public ValueProviderDocumenter(ZString useage, MultilingualString explanation, List<(string example, object expectedResult)> examplesAndResults = null)
		{
			this.useage = useage;
			this.explanation = explanation;
			this.examplesAndResults = examplesAndResults;
		}

		readonly ZString useage;
		readonly MultilingualString explanation;
		readonly List<(string example, object expectedResult)> examplesAndResults;

		public ZString Useage => useage;

		public MultilingualString Explanation
		{
			get
			{
				var explanationIncludingExamples = new List<MultilingualString>() { explanation };
				if (ExamplesAndResults != null)
				{
					foreach (var example in ExamplesAndResults)
					{
						explanationIncludingExamples.Add(
							ResString.GetMultilingualString("6439b810-9d40-4b59-9a54-e213e97e4056", "E.g: {0}", example.example));
					}
				}
				return MultilingualString.Join(System.Environment.NewLine, explanationIncludingExamples.ToArray());
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public List<(string example, object expectedResult)> ExamplesAndResults => examplesAndResults;
	}
}
