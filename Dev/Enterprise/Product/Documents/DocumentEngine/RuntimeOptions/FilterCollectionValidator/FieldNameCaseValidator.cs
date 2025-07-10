using System;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class FieldNameCaseValidator : FilterCollectionValidator
	{
		public FieldNameCaseValidator(DocumentNote documentNote)
		{
			DocumentNote = documentNote;
		}

		internal DocumentNote DocumentNote { get; }

		public override bool IsValid(FilterField filterToValidate)
		{
			return !GetDuplicates(filterToValidate).Any();
		}

		List<FilterField> GetDuplicates(FilterField filterToValidate)
		{
			return DocumentNote.UserDefinedFieldList.OfType<FilterField>().Where(f =>
				string.Equals(f.DisplayName, filterToValidate.DisplayName, StringComparison.OrdinalIgnoreCase) &&
				f.DisplayName != filterToValidate.DisplayName).ToList();
		}

		public override string GetErrorMessage(FilterField filterToValidate)
		{
			var duplicates = GetDuplicates(filterToValidate);

			if (duplicates.Any())
			{
				return Res.GetString("ABF173E3-7C6D-4BEF-B357-5E2EF481790E",
					@"{0} is duplicate with field(s) below (same name in different case):
{1}", filterToValidate.DisplayName, string.Join("\r\n", duplicates.Select(d => $"'{d.DisplayName}' in template '{d.TemplateName}'")));
			}

			return "";
		}
	}
}
