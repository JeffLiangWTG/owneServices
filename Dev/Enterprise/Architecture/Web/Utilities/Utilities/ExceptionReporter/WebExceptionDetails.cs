using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.SessionState;
using System.Xml;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Web.Utilities.Exceptions
{
	public class WebExceptionDetails : ExceptionDetails
	{
		public WebExceptionDetails(Exception ex)
			: base(ex)
		{
		}

		public override void WriteWebInfo(XmlTextWriter xtw)
		{
			if (HttpContext.Current != null && HttpContext.Current.Request != null)
			{
				HttpServerUtility server = HttpContext.Current.Server;
				HttpRequest request = HttpContext.Current.Request;

				xtw.WriteStartElement("WebInfo");

				xtw.WriteElementString("RequestedURL", HttpContext.Current.Request.Url.AbsolutePath);
				xtw.WriteElementString("ReferrerURL", HttpContext.Current.Request.UrlReferrer == null ? string.Empty : HttpContext.Current.Request.UrlReferrer.AbsolutePath);
				xtw.WriteElementString("UserAgent", HttpContext.Current.Request.UserAgent);
				xtw.WriteElementString("UserHostAddress", HttpContext.Current.Request.UserHostAddress);

				try
				{
					xtw.WriteElementString("WebApplicationPath", HttpContext.Current.Request.PhysicalApplicationPath);
				}
				catch (ArgumentNullException)
				{
					xtw.WriteElementString("WebApplicationPath", "has thrown System.ArgumentNullException");
				}

				xtw.WriteElementString("ScriptTimeout", HttpContext.Current.Server.ScriptTimeout.ToString());

				HttpSessionState session = HttpContext.Current.Session;

				if (session != null)
				{
					xtw.WriteElementString("SessionID", session.SessionID);
					xtw.WriteElementString("SessionTimeout", session.Timeout.ToString());
					xtw.WriteElementString("SessionIsNew", session.IsNewSession.ToString());
				}
				else
				{
					xtw.WriteElementString("SessionIsNull", "true");
				}

				if (request != null)
				{
					WriteNameValueCollection(xtw, "Request_Parameters", request.Params);
				}

				if (server != null)
				{
					Exception lastError = null;
					Exception baseException = null;

					try
					{
						lastError = server.GetLastError();
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
					}

					if (lastError != null)
					{
						baseException = lastError.GetBaseException();
						if (baseException != null && baseException != lastError)
						{
							xtw.WriteStartElement("WebTopLevelException");

							xtw.WriteElementString("Message", baseException.Message);
							xtw.WriteElementString("Type", baseException.GetType().ToString());
							xtw.WriteElementString("Source", baseException.Source);
							xtw.WriteElementString("Method", baseException.TargetSite.Name);
							xtw.WriteElementString("StackTrace", baseException.StackTrace);

							xtw.WriteEndElement();
						}
					}
				}

				xtw.WriteStartElement("WebUserActivityLogs");

				int logNo = 0;
				List<UserActivityLog> logs = UserActivityLogger.ActivityLogs;
				foreach (UserActivityLog activityLog in logs)
				{
					logNo++;
					xtw.WriteStartElement("Log_" + logNo);
					xtw.WriteElementString("Created", activityLog.CreatedDateTime.ToString(CultureInfo.CurrentCulture));
					foreach (string actionKey in activityLog.ActionTimes.Keys)
					{
						xtw.WriteElementString(actionKey + "_LastOccurrence", activityLog.ActionTimes[actionKey].ToString(CultureInfo.CurrentCulture));
						if (activityLog.ActionOccurrences.ContainsKey(actionKey))
						{
							xtw.WriteElementString(actionKey + "_Occurrences", activityLog.ActionOccurrences[actionKey].ToString());
						}
					}

					WriteNameValueCollection(xtw, "Request_Parameters", activityLog.RequestParameters);

					xtw.WriteEndElement();
				}

				xtw.WriteEndElement();

				xtw.WriteEndElement();
			}
		}

		void WriteNameValueCollection(XmlTextWriter xtw, string sectionTag, NameValueCollection @params)
		{
			xtw.WriteStartElement(sectionTag);

			foreach (string variableKey in @params.AllKeys)
			{
				if (!string.IsNullOrEmpty(variableKey))
				{
					if (variableKey != "ALL_HTTP" && variableKey != "ALL_RAW")
					{
						bool mustCloseElement = false;
						try
						{
							string tag = CleanXmlTag(variableKey);
							string value = @params[variableKey];
							if (!string.IsNullOrEmpty(tag) && !string.IsNullOrEmpty(value))
							{
								xtw.WriteStartElement(tag);
								mustCloseElement = true;
								xtw.WriteCData(value);
								xtw.WriteEndElement();
								mustCloseElement = false;
							}
						}
						catch (Exception e) when (!e.IsCriticalException()) { }
						if (mustCloseElement)
						{
							xtw.WriteEndElement();
						}
					}
				}
			}

			xtw.WriteEndElement();
		}

		internal static string CleanXmlTag(string xmlTag)
		{
			xmlTag = xmlTag.Replace("$", "_");
			xmlTag = xmlTag.Replace(".", "_");
			Regex regex = new Regex(@"#x((10?|[2-F])FFF[EF]|FDD[0-9A-F]|7F|8[0-46-9A-F]9[0-9A-F])", RegexOptions.IgnoreCase);
			if (regex.IsMatch(xmlTag))
			{
				xmlTag = regex.Replace(xmlTag, "_");
			}
			if (xmlTag.Length > 0 && Char.IsDigit(xmlTag[0]))
			{
				xmlTag = "_" + xmlTag;
			}
			return xmlTag;
		}
	}
}
