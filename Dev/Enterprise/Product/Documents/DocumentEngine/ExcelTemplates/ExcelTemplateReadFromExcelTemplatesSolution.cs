using System;
using System.IO;
using CargoWise.Common;
using Enterprise.Core;

namespace Enterprise.ExcelTemplates
{
	public class ExcelTemplateReadFromExcelTemplatesSolution : ExcelTemplate
	{
		public ExcelTemplateReadFromExcelTemplatesSolution(string templateName)
			: this(templateName, "ExcelTemplates." + templateName + Extension)
		{
		}

		protected ExcelTemplateReadFromExcelTemplatesSolution(string templateName, string templateResourceName)
			: base(templateName, templateResourceName)
		{
		}

		internal const string Extension = ".xls";

		protected override Stream GetAsTemplateStreamInternal()
		{
			Stream result = AssemblyLoader.LoadAssembly("ExcelTemplates").GetManifestResourceStream(TemplateSourceLocation) ?? throw new ApplicationException(TemplateSourceLocation + " is not a valid resource embedded in ExcelTemplates.dll.");

			if (result.Length < 1)
			{
				throw new ApplicationException(TemplateSourceLocation + " could not be read as a resource embedded in ExcelTemplates.dll.");
			}

			return result;
		}

		protected override byte[] GetAsByteArrayInternal()
		{
			return GetTemplateStreamAsByteArray();
		}

		#region Template Names

		public static class TemplateNames
		{
			[DataContext(Constants.DataContext.DocumentCoverSheet)]
			public const string FaxCoverSheet = "FaxCoverSheet";

			[DataContext(Constants.DataContext.DocumentCoverSheet)]
			public const string EmailCoverSheet = "EmailCoverSheet";

			[DataContext(Constants.DataContext.DocumentCoverSheet)]
			public const string PrintCoverSheet = "PrintCoverSheet";

			[DataContext(Constants.DataContext.Sailing)]
			public const string BookingSummary = "BookingSummary";

			[DataContext(Constants.DataContext.None)]
			public const string OverflowNotesTemplate = "OverflowNotesTemplate";

			[DataContext(Constants.DataContext.None)]
			public const string DocManagerCopyDocs = "DocManagerCopyDocs";

			[DataContext(Constants.DataContext.None)]
			public const string PrintTest = "PrintTest";
		}

		#endregion
	}
}
