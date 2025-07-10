using System.Reflection;
using System.Xml.Linq;
using WTG.ErrorReporting;

namespace Enterprise.RemotePrinting.Client
{
	public static class ReportBuilderHelper
	{
		public static void SetCustomValue(this EnterpriseErrorReportBuilder reportBuilder, string key, string value)
		{
			var setRootElementValueMethod = typeof(EnterpriseErrorReportBuilder).GetMethod("SetRootElementValue", BindingFlags.Instance | BindingFlags.NonPublic);
			setRootElementValueMethod?.Invoke(reportBuilder, new object[] { key, value });
		}

		public static void SetCustomValueWithSection(this EnterpriseErrorReportBuilder reportBuilder, string section, string key, string value)
		{
			var getRootElementValueMethod = typeof(EnterpriseErrorReportBuilder).GetMethod("GetOrAddRootElement", BindingFlags.Instance | BindingFlags.NonPublic);
			var sectionElement = getRootElementValueMethod?.Invoke(reportBuilder, new object[] { section }) as XElement;
			if (sectionElement != null)
			{
				sectionElement.Add(new XElement(key, value));
			}
		}
	}
}
