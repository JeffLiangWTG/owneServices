using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Mail.Business
{
	public static class CustomerServiceEmailUniqueIdUtil
	{
		static byte[] GetIncidentHashingIdentifier(SupportIncident incident)
		{
			return System.Text.Encoding.UTF8.GetBytes($"{incident.Number}&{incident.PK}");
		}

		public static string GetUniqueID(MailItem mailItem)
		{
			try
			{
				var mimeMessage = mailItem.BuildMimeMessage();
				foreach (var body in mimeMessage.BodyParts.Where(x => x.ContentType.MediaType.Equals("text")))
				{
					var text = ((MimeKit.TextPart)body).Text;
					var uniqueID = GetMarkId(text);
					if (!string.IsNullOrEmpty(uniqueID))
					{
						return uniqueID;
					}
				}
			}
			catch (Exception)
			{
			}

			return "";
		}

		public static string GetUniqueIDFromBody(SupportIncident incident, MailItem mailItem)
		{
			try
			{
				var mimeMessage = mailItem.BuildMimeMessage();
				var tokenInEmail = string.Empty;
				var incidentToken = GenerateUniqueEmailID(incident);
				foreach (var body in mimeMessage.BodyParts.Where(x => x.ContentType.MediaType.Equals("text", StringComparison.OrdinalIgnoreCase)))
				{
					var text = ((MimeKit.TextPart)body).Text;
					var mediaSubtype = body.ContentType.MediaSubtype;
					if (mediaSubtype.Equals("html", StringComparison.OrdinalIgnoreCase))
					{
						text = WebUtility.HtmlDecode(text);
					}

					if (text.Contains(incidentToken))
					{
						tokenInEmail = incidentToken;
						break;
					}
				}

				if (!string.IsNullOrEmpty(tokenInEmail))
				{
					return tokenInEmail;
				}
			}
			catch (Exception)
			{
			}

			return "";
		}

		public static string GenerateUniqueEmailID(SupportIncident incident)
		{
			if (incident != null)
			{
				using var sha = SHA512.Create();
				var hash = sha.ComputeHash(GetIncidentHashingIdentifier(incident));
				return Convert.ToBase64String(hash);
			}

			return "";
		}

		static string GetMarkId(string html)
		{
			var re = new Regex(@"(<div[\w\W].+?>)");
			var divReq = re.Matches(html);
			var attrReg = new Regex(@"([a-zA-Z1-9_-]+)\s*=\s*(\x27|\x22)([^\x27\x22]*)(\x27|\x22)",
				RegexOptions.IgnoreCase);
			foreach (Match item in divReq)
			{
				if (item.ToString().Contains(IncidentConstants.MarkName))
				{
					var matches = attrReg.Matches(item.ToString());
					{
						for (var j = 0; j < matches.Count; j++)
						{
							var groups = matches[j].Groups;
							if (groups.Count >= 4)
							{
								if (groups[1].Value.ToUpper() == "ID")
								{
									return WebUtility.HtmlDecode(groups[3]?.Value ?? string.Empty);
								}
							}
						}
					}
				}
			}

			return "";
		}

		public static void AppendMark(EmailDef email, SupportIncident incident)
		{
			if (!email?.Body?.Contains(IncidentConstants.MarkName) ?? false)
			{
				var builder = new ZStringBuilder(email.Body);
				email.Body = builder.Append(ZString.Format(IncidentConstants.MarkContent,
					GenerateUniqueEmailID(incident))).ToString();
			}
		}
	}
}
