using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAutoresponder
{
	public class UrlExtractor
	{
		readonly Regex extractPattern = new Regex(@"(http|ftp|https)://([\w_-]+(?:(?:\.[\w_-]+)+))([\w.,@?^=%&:/~+#-]*[\w@?^=%&/~+#-])?", RegexOptions.IgnoreCase | RegexOptions.Multiline);
		readonly Regex filterPattern = new Regex(@"http(s)?://myaccount(-portal)?.cargowise.com", RegexOptions.IgnoreCase);
		readonly Regex updateNotesPattern = new Regex(@"UpdateNote(s)?", RegexOptions.IgnoreCase);
		readonly Regex rewritePathPattern = new Regex(@"CargoWiseOneWiseLearning.aspx", RegexOptions.IgnoreCase);

		readonly ELearningUrlComparer eLearningUrlComparer = new ELearningUrlComparer();

		public IEnumerable<string> Extract(string text)
		{
			text = text ?? throw new ArgumentNullException(nameof(text));
			var mc = extractPattern.Matches(text);
			return mc.Cast<Match>().Select(m => m.ToString())
				.Where(x => IsEligibleUrl(x))
				.Select(x => NormalizeUrl(x))
				.Distinct(eLearningUrlComparer);
		}

		public IEnumerable<string> ExtractFromExistingMyAccountUrls(string text)
		{
			text = text ?? throw new ArgumentNullException(nameof(text));
			var mc = extractPattern.Matches(text);
			return mc.Cast<Match>().Select(m => m.ToString())
				.Where(x => IsMyAccountUrl(x))
				.Select(x => x)
				.Distinct(eLearningUrlComparer);
		}

		bool IsMyAccountUrl(string url)
		{
			if (string.IsNullOrWhiteSpace(url))
			{
				return false;
			}
			return filterPattern.IsMatch(url);
		}

		public IEnumerable<string> ExtractFromIncident(BusinessObjectFactory factory, ZGuid incidentPk)
		{
			factory = factory ?? throw new ArgumentNullException(nameof(factory));

			var incident = factory.Load<IncidentMainBase>(incidentPk);
			var jobConversation = factory.Load<JobConversation>(new ZQuery(JobConversationSchema.JCC_ParentID, incident.IM_INC_Request)).FirstOrDefault();

			if (jobConversation is null)
			{
				return new List<string>();
			}

			var jobConversationMessages = factory.Load<JobConversationMessage>(new ZQuery(JobConversationMessageSchema.JCM_JCC_Conversation, jobConversation.PK))
				.Where(jcm => IsEligibleMessageForExtraction(jcm));

			if (jobConversationMessages.IsNullOrEmpty())
			{
				return new List<string>();
			}

			var texts = String.Join("\n", jobConversationMessages.Select(jcm => jcm.JCM_Body.ToString()));
			return this.Extract(texts);
		}

		static bool IsEligibleMessageForExtraction(JobConversationMessage jcm)
			=> jcm.MessageType != MessageType.Remote && jcm.MessageSubType != MessageSubType.SystemLog;

		bool IsEligibleUrl(string url)
		{
			try
			{
				var uri = new Uri(url);
				if (filterPattern.IsMatch(url)
					&& uri.LocalPath != "/"
					&& uri.Fragment != "#"
					&& !url.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
					&& !url.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
				{
					if (updateNotesPattern.IsMatch(url))
					{
						if (EDIDataRegistry.Instance.IncludeUpdateNoteUrls.Value)
						{
							return true;
						}
						return false;
					}
					return true;
				}
				return false;
			}
			catch (UriFormatException)
			{
				return false;
			}
		}

		string NormalizeUrl(string url)
		{
			var uriBuilder = new UriBuilder(url);
			var newPath = rewritePathPattern.Replace(uriBuilder.Path, "CargoWiseLearning.aspx");
			uriBuilder.Path = newPath;
			uriBuilder.Port = -1;
			return uriBuilder.ToString();
		}

		[SuppressMessage("Microsoft.Design", "CA1054", Justification = "Parameters are not URIs")]
		public static bool IsSameELearningUrl(string url1, string url2)
		{
			if (object.ReferenceEquals(url1, url2))
			{
				return true;
			}

			if (String.IsNullOrWhiteSpace(url1) || String.IsNullOrWhiteSpace(url2))
			{
				return false;
			}

			try
			{
				var uri1 = new Uri(url1);
				var uri2 = new Uri(url2);
				return uri1.PathAndQuery.Equals(uri2.PathAndQuery, StringComparison.OrdinalIgnoreCase)
					&& uri1.Fragment.Equals(uri2.Fragment, StringComparison.OrdinalIgnoreCase);
			}
			catch (UriFormatException)
			{
				return false;
			}
		}
	}

	public class ELearningUrlComparer : IEqualityComparer<string>
	{
		public bool Equals(string url1, string url2)
		{
			return UrlExtractor.IsSameELearningUrl(url1, url2);
		}

		public int GetHashCode(string url)
		{
			if (url.IsNullOrEmpty())
			{
				return 0;
			}

			try
			{
				var uri = new Uri(url);
				return (uri.PathAndQuery + uri.Fragment).ToUpperInvariant().GetHashCode();
			} catch (UriFormatException)
			{
				return 0;
			}
		}
	}
}
