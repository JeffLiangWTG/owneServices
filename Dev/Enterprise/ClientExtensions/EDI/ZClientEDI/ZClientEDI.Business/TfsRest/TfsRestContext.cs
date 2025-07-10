using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Newtonsoft.Json;

namespace Enterprise.Client.EDI.TfsRest
{
	public sealed class TfsRestContext : IDevOpsRestContext
	{
		public TfsRestContext()
			: this(new Uri("http://tfs.wtg.zone:8080/tfs/CargoWise", UriKind.Absolute))
		{
		}

		public TfsRestContext(Uri uri)
		{
			if (uri == null)
			{
				throw new ArgumentNullException(nameof(uri));
			}

			if (!uri.IsAbsoluteUri)
			{
				throw new ArgumentException("Uri must be an absolute Uri.", nameof(uri));
			}

			if (uri.AbsoluteUri.Length > 0 && uri.AbsoluteUri[uri.AbsoluteUri.Length - 1] != '/')
			{
				uri = new Uri(uri.AbsoluteUri + "/", UriKind.Absolute);
			}

			baseUri = uri;
		}

		readonly Uri baseUri;

		T Get<T>(Uri relativeUri)
		{
			DoNotHitProductionServerDuringUnitTests();

			string json;

#pragma warning disable SYSLIB0014 // WebRequest.Create(Uri) is obsolete: WebRequest, HttpWebRequest, ServicePoint, and WebClient are obsolete. Use HttpClient instead.
			using (var client = new WebClient())
#pragma warning restore SYSLIB0014
			{
				client.UseDefaultCredentials = true;
				client.BaseAddress = baseUri.AbsoluteUri;

				client.Encoding = Encoding.UTF8;
				client.Headers[HttpRequestHeader.Accept] = "application/json";

				json = client.DownloadString(relativeUri);
			}

			var value = JsonConvert.DeserializeObject<T>(json);
			return value;
		}

		public IEnumerable<PullRequestInfo> GetPullRequestsByOwner(string ownerId)
		{
			var uri = new Uri(string.Format(CultureInfo.InvariantCulture, "_apis/git/pullrequests?searchCriteria.creatorId={0}", Uri.EscapeDataString(ownerId)), UriKind.Relative);
			var response = Get<ListResult<PullRequestInfo>>(uri);
			if (response?.Value != null)
			{
				foreach (var item in response.Value)
				{
					item.pullRequestUrl = new Uri(baseUri.AbsoluteUri.TrimEnd('/') + "/" + item.repository.project.name + "/_git/" + item.repository.name + "/pullrequest/" + item.pullRequestId.ToString(CultureInfo.InvariantCulture));
				}
			}
			return response?.Value;
		}

		public string GetUserId(ZString loginName)
		{
			var uri = new Uri(string.Format(CultureInfo.InvariantCulture, "_apis/Identities?searchFilter=DirectoryAlias&filterValue={0}&api-version=1.0", Uri.EscapeDataString(loginName)), UriKind.Relative);
			var response = Get<ListResult<IdentityInfo>>(uri);
			return response?.Value?.FirstOrDefault(i => i.Properties?["Domain"]?.Value?.Equals(DomainName, StringComparison.OrdinalIgnoreCase) ?? false)?.Id;
		}

		[Conditional("DEBUG")]
		void DoNotHitProductionServerDuringUnitTests()
		{
			if (Globals.IsTest && !baseUri.ToString().Contains(".sand."))
			{
				throw new InvalidOperationException("Attempted to access " + baseUri.ToString() + " during a unit test");
			}
		}

		public const string DomainName = "CORP";
	}
}
