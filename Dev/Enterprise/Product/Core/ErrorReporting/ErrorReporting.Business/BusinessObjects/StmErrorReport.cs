using System.Data;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.ErrorReporting.Business
{
	[CodeProperty("ErrorReportID")]
	public class StmErrorReport : AutoStmErrorReport
	{
		public StmErrorReport(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("StmErrorReport|ErrorReportID", Caption = "Error Report ID", ShortCaption = "ID")]
		public ZString ErrorReportID
		{
			get
			{
				if (errorReportID == null)
				{
					try
					{
						var document = XDocument.Parse(QER_ReportXml);
						var reportIDNode = document.XPathSelectElement("//ErrorReportID");
						errorReportID = (reportIDNode == null ? string.Empty : reportIDNode.Value);
					}
					catch (XmlException)
					{
						errorReportID = string.Empty;
					}
				}
				return errorReportID;
			}
		}
		string errorReportID;

		protected override ZString HumanReadableNameCore
		{
			get { return ErrorReportID; }
		}

		public override ZString QER_ReportXml
		{
			get { return base.QER_ReportXml; }
			set
			{
				base.QER_ReportXml = value;
				errorReportID = null;
			}
		}

		[ResourceStringData("StmErrorReport|QER_ReportType", Caption = "Error Report Type", ShortCaption = "Type")]
		public override ZInt QER_ReportType
		{
			get => base.QER_ReportType;
			set => base.QER_ReportType = value;
		}
	}
}
