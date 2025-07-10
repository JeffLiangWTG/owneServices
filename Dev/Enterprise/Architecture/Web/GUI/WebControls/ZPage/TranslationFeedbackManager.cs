using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class TranslationFeedbackManager
	{
		public TranslationFeedbackManager()
		{
		}

		public void Initialize(ZPage page)
		{
			if (GetEnableTranslationFeedback(page))
			{
				this.rid = Guid.NewGuid();
				Res.NotifyChange(); // force rebuild of cache
				Res.ResourceDemanded += Res_ResourceDemanded;
			}
		}

		bool GetEnableTranslationFeedback(ZPage page) => !Res.IsEnglish(Res.CurrentLanguage) && GetUseHasTranslationFeedbackPermission(page);

		bool GetUseHasTranslationFeedbackPermission(ZPage page) => page?.SiteUser?.AreSecurityRightsGranted(WebSecurityRightsList.WebTranslationFeedback) ?? false;

		void Res_ResourceDemanded(object sender, ResourceStringDemandedEventArgs e)
		{
			if (!usedKeys.Contains(e.Key))
			{
				usedKeys.Add(e.Key);
			}
		}

		public void RenderTranslationFeedbackScript(ZPage page)
		{
			if (GetEnableTranslationFeedback(page))
			{
				var absoluteUrl = page.Request.Url.GetLeftPart(UriPartial.Path);
				absoluteUrl = absoluteUrl.Substring(0, absoluteUrl.Length - page.PageRelativePath.Length);
				absoluteUrl = new Uri(new Uri(absoluteUrl), ResourceStringUsageRequesHelper.Url).ToString();

				var trustedDomains = Environment.EnvProxy.Instance.Registry.ResourceStringUsageTrustedDomains?.ToList() ?? new List<string>();
				var host = page.Request.Url.GetLeftPart(UriPartial.Authority);

				if (!trustedDomains.Contains(host))
				{
					trustedDomains.Add(host);
					Environment.EnvProxy.Instance.Registry.ResourceStringUsageTrustedDomains = trustedDomains.ToArray();
				}

				var script = string.Format(@"
<script type='text/javascript'>
	var TranslationFeedbackConfiguration = {{
		LicenceCode: '{0}',
		Language: '{1}',
		UsageCallback: '{2}?s={3}&Data={4}'
	}};
</script>", GlbCompany.CurrentCompany.LicenceKeyIdentifier, Res.CurrentLanguage, absoluteUrl, page.Session.SessionID, rid.ToString("D")); // Javascript code should not be translated
				page.ZClientScript.RegisterClientScriptBlock(GetType(), "TranslationFeedbackConfiguration", script);
				var translationFeedbackScript = new ZWebResource(typeof(ZPage), "TranslationFeedback.js", page);
				translationFeedbackScript.Extract();
				page.ZClientScript.RegisterClientScriptInclude("TranslationFeedback", translationFeedbackScript.FileName);
			}
		}

		public void OnPageUnload(ZPage page)
		{
			if (GetEnableTranslationFeedback(page))
			{
				string usageFile = GetResourceStringUsageFile(page.Session.SessionID, rid);
				Directory.CreateDirectory(Path.GetDirectoryName(usageFile));
				File.WriteAllBytes(usageFile, Compressor.Compress(Encoding.UTF8.GetBytes(string.Join("\n", usedKeys.ToArray()))));
				Res.ResourceDemanded -= Res_ResourceDemanded;
			}
		}

		public static void CleanupResourceStringUsageFiles(string sessionId)
		{
			string directory = Path.Combine(ResourceStringUsageDataBasePath, sessionId);
			if (Directory.Exists(directory))
			{
				Directory.Delete(directory, true);
			}
		}

		public static void CleanupAllResourceStringUsageFiles()
		{
			if (Directory.Exists(ResourceStringUsageDataBasePath))
			{
				foreach (var directory in Directory.GetDirectories(ResourceStringUsageDataBasePath))
				{
					Directory.Delete(directory, true);
				}
			}
		}

		public static string GetResourceStringUsageFile(string sessionId, Guid rid)
		{
			return Path.Combine(ResourceStringUsageDataBasePath, sessionId, rid.ToString("D"));
		}

		static string ResourceStringUsageDataBasePath
		{
			get
			{
				return resourceStringUsageDataBasePath ?? (resourceStringUsageDataBasePath = CommonProgramData.GetCargoWiseDirectory("ResourceStringUsageData", Db.ServerName, Db.DatabaseName));
			}
		}
		static string resourceStringUsageDataBasePath;

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		readonly static HashSet<string> usedKeys = new HashSet<string>();
#if DEBUG
		public HashSet<string> GetUsedKeysForTest() => usedKeys;
#endif
		Guid rid;

		class ResourceStringUsageRequestHandler : DataRequestHandler<ResourceStringUsageRequesHelper>
		{
			public override ZBlob GetBinaryData()
			{
				string file = GetResourceStringUsageFile(QueryString["s"], Guid.Parse(QueryString["Data"]));
				if (!File.Exists(file))
				{
					throw new InvalidQueryStringException("Requested data not found");
				}
				return File.ReadAllBytes(file);
			}

			protected override BusinessObject[] GetNewBusinessObjects()
			{
				return null;
			}

			public override string FileName
			{
				get { return null; }
			}
		}

		class ResourceStringUsageRequesHelper : DataRequestHelper
		{
			public const string Url = "ResourceStringUsageData.axd";

			public override string BaseUrl
			{
				get { return Url; }
			}

			public override bool EnableCache
			{
				get { return false; }
			}

			public override bool UseSecureQueryString
			{
				get { return false; }
			}
		}
	}
}
