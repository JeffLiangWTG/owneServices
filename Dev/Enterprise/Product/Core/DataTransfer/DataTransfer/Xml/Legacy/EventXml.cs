using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Xml;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Xml
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class EventXml : XmlGenerator
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public override string ToString()
		{
			TextWriter textWriter = new StringWriter();
			XmlWriter writer = new XmlTextWriter(textWriter);
			writer.WriteStartElement("Events");

			foreach (BaseStmALog log in StmALogs.Keys)
			{
				writer.WriteStartElement("Event");

				writer.WriteElementString("Source", log.SL_Table);

				if (!log.SL_SE_NKEvent.IsEmpty)
				{
					writer.WriteElementString("Code", log.SL_SE_NKEvent);
					if (log.Event != null)
					{
						writer.WriteElementString("CodeDescription", log.Event.SE_DescMultilingual.GetUnresolvedString());
					}
				}

				writer.WriteElementString("DateTime", log.SL_EventTime.ToString("s"));
				writer.WriteElementString("PostedDateTime", log.SL_PostedTimeUtc.ToString("s"));
				writer.WriteElementString("User", UserLoginID(log));
				writer.WriteElementString("IsEstimatedDate", log.SL_IsEstimate ? "true" : "false");

				ZString payload = StmALogs[log].ToString();
				if (!payload.IsEmpty)
				{
					writer.WriteStartElement("Payload");
					writer.WriteRaw(payload);
					writer.WriteEndElement();
				}

				ZString additionalXml = GetAdditionalXmlForLog(log);
				if (!additionalXml.IsEmpty)
				{
					writer.WriteRaw(additionalXml);
				}

				writer.WriteEndElement();
			}

			writer.WriteEndElement();

			return textWriter.ToString();
		}

		protected virtual ZString GetAdditionalXmlForLog(BaseStmALog log)
		{
			return "";
		}

		public void Add(BaseStmALog stmALog)
		{
			Add(stmALog, "");
		}

		public void Add(BaseStmALog stmALog, string payload)
		{
			StmALogs.Add(stmALog, payload);
		}

		protected Hashtable StmALogs
		{
			get
			{
				if (fStmALogs == null)
				{
					fStmALogs = new Hashtable();
				}

				return fStmALogs;
			}
		}
		Hashtable fStmALogs;

		protected virtual string UserLoginID(BaseStmALog log)
		{
			GlbStaff staff = (GlbStaff)log.Factory.LoadFromNaturalKey(typeof(GlbStaff), GlbStaffSchema.GS_Code, log.SL_GS_NKUser);
			return staff == null ? log.SL_GS_NKUser : staff.GS_LoginName;
		}
	}
}
