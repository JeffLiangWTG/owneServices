using System.Xml.Linq;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DataTransformation;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class XsltDataTransformationForTest : XsltDataTransformation
	{
		public override string Description
		{
			get { return "test"; }
		}

		public override string DataStoreName
		{
			get { return "name"; }
		}

		protected override string GetXslt()
		{
			return
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<xsl:stylesheet version=""1.0""
xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"">
<xsl:template match=""Entity"">
  <Entity Type=""{@Type}"">
    <xsl:for-each select=""Property"">
     <Property Name=""{@Name}"">
       <xsl:value-of select=""Value/text()"" />
     </Property>
    </xsl:for-each>
  </Entity>
</xsl:template>
</xsl:stylesheet>";
		}

		protected override void OnAfterRun(XDocument input, XDocument xslt, XDocument result, INotificationsHandler handler)
		{
			base.OnAfterRun(input, xslt, result, handler);

			var source = new NotificationSource("test");
			var notification = new Notification(source, NotificationType.Information, "test message");

			handler.Add(notification);
		}
	}
}