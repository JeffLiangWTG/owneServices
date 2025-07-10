using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Enterprise.ExcelTemplates;

namespace Enterprise.DocumentEngine
{
	public sealed class ExcelTemplateForBi : ExcelTemplate
	{
		public ExcelTemplateForBi(string templateName)
			: base(templateName, GetTemplateSourceLocation())
		{
		}

		static string GetTemplateSourceLocation()
		{
			return "Enterprise.DocumentEngine.BI.ReportTemplates.BIReportTemplates.xls";
		}

		[SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		protected override Stream GetAsTemplateStreamInternal()
		{
			Stream result = GetType().Assembly.GetManifestResourceStream(TemplateSourceLocation) ?? throw new ApplicationException(TemplateSourceLocation + " is not a valid resource embedded in DocumentEngine.dll.");

			if (result.Length < 1)
			{
				throw new ApplicationException(TemplateSourceLocation + " could not be read as a resource embedded in DocumentEngine.dll.");
			}

			return result;
		}

		protected override byte[] GetAsByteArrayInternal()
		{
			return GetTemplateStreamAsByteArray();
		}
	}
}
