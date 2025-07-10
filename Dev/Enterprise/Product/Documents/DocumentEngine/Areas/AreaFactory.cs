using System;
using System.Reflection;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.Areas
{
	static class AreaFactory
	{
		public static Area InstantiateArea(int start, int end, Report report, string text)
		{
			Area result = null;

			if (text.Trim().Equals((NoResString)"#DOCUMENTFOOTER:STICKTOBOTTOM", StringComparison.OrdinalIgnoreCase))
			{
				text = (NoResString)"#LastPageFooter";
			}
			char[] delimiters = new char[] { '#', ':', ';', ',' };
			string sectionType = text.Split(delimiters, 3)[1];
			Type areaType = Type.GetType("Enterprise.DocumentEngine.Areas." + sectionType + "Area", false, true);

			if (areaType != null && !areaType.IsAbstract)
			{
				try
				{
					result = (Area)Activator.CreateInstance(areaType, new object[] { start, end, report, text });
				}
				catch (TemplateDefinitionException ex)
				{
					var message = Res.GetString("d3464cf0-b76f-4687-ac55-e45fbc9d5730", "Error in {0}: {1}", "InstantiateArea()", ex.Message);
					report.ErrorManager.Add(new ReportProcessingError(message, ex.CellReference, ReportProcessingErrorSeverity.Error, ex));
				}
				catch (TargetInvocationException ex)
				{
					if (ex.InnerException is DocumentEngineException)
					{
						throw ex.InnerException;
					}
					else
					{
						throw;
					}
				}
			}
			else
			{
				string reportName = (report != null) ? report.Name.ToString() : (NoResString)"[null]";
				throw new DocumentEngineException("Area type " + text + " is not defined! Error in report " + reportName);
			}

			return result;
		}
	}
}
