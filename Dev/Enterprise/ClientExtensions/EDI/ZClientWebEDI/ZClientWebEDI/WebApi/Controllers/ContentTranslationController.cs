using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using CargoWise.Application;
using CargoWise.Data;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZClientWebCargoWiseEDI.Translations;

namespace Enterprise.ZClientWebCargoWiseEDI.WebApi.Controllers
{
	[RoutePrefix("api/ContentTranslation")]
	public class ContentTranslationController : ControllerWithEnvironment
	{
		[HttpPost]
		[Route("Export")]
		public IHttpActionResult GetDataForExport([FromBody] ExportRequestData request)
		{
			if (!IsValidClientIp())
			{
				return StatusCode(HttpStatusCode.Forbidden);
			}

			if (request == null)
			{
				return Content(HttpStatusCode.NoContent, "Empty request");
			}

			var language = "";

			if (!string.IsNullOrEmpty(request.language))
			{
				try
				{
					language = Culture.GetLanguageForCulture(CultureInfo.GetCultureInfo(request.language));
				}
				catch (CultureNotFoundException ex)
				{
					return Content(HttpStatusCode.Forbidden, ex.Message);
				}
			}

			var dataSource = GetDataSource(request.contentModule);

			if (dataSource == null)
			{
				return Content(HttpStatusCode.NotFound, FormattableString.Invariant($"Content Module Not Found: {request.contentModule}"));
			}

			var response = new Dictionary<string, string>();

			foreach (var data in dataSource.GetTranslatableResources())
			{
				var resData = new List<ResourceStringData>();
				foreach (var entry in data.Resources)
				{
					if (string.IsNullOrEmpty(language) || ResourceStringsFactory.Lookup(language, entry.Key) == null)
					{
						resData.Add(new ResourceStringData(entry.Key, entry.Value));
					}
				}

				if (resData.Any())
				{
					using (var ms = new MemoryStream())
					{
						var xml = new ResourceStringXmSerializer(ms, Res.DefaultLanguage);
						xml.Serialize(resData);
						response.Add(data.Id, Encoding.UTF8.GetString(ms.ToArray()).Trim('\uFEFF', '\u200B'));
					}
				}
			}

			return Content(HttpStatusCode.OK, response);
		}

		protected virtual IContentTranslationDataSource GetDataSource(string contentModule)
		{
			var dataSourceName = "IContentTranslationDataSource." + contentModule;
			return ObjectFactory.Contains(dataSourceName) ? ObjectFactory.Get<IContentTranslationDataSource>(dataSourceName) : null;
		}

		[HttpPost]
		[Route("Import")]
		public IHttpActionResult Import([FromBody] ImportRequestData request)
		{
			var shouldSendNotificationEmail = false;
			var result = ImportCore(request, out shouldSendNotificationEmail);
			var readableResult = (result as NegotiatedContentResult<string>);
			if (readableResult.StatusCode != HttpStatusCode.OK || shouldSendNotificationEmail)
			{
				SendNotificationEmail(request, readableResult.Content);
			}

			return result;
		}

		IHttpActionResult ImportCore(ImportRequestData request, out bool shouldSendNotificationEmail)
		{
			shouldSendNotificationEmail = false;

			if (!IsValidClientIp())
			{
				return Content(HttpStatusCode.BadRequest, "Invalid Client IP address");
			}

			if (request == null)
			{
				return Content(HttpStatusCode.NoContent, "Empty request");
			}

			if (string.IsNullOrEmpty(request.contents))
			{
				return Content(HttpStatusCode.NoContent, "Empty request content");
			}

			if (string.IsNullOrEmpty(request.language))
			{
				return Content(HttpStatusCode.NoContent, "Empty request language");
			}

			var language = request.language;

			try
			{
				var culture = Culture.GetCultureForLanguage(language);
				if (culture != null)
				{
					if (!culture.EnglishName.StartsWith("Unknown locale", StringComparison.OrdinalIgnoreCase))
					{
						language = Culture.GetLanguageForCulture(CultureInfo.GetCultureInfo(request.language));
					}
					else
					{
						return Content(HttpStatusCode.BadRequest, culture.EnglishName);
					}
				}
			}
			catch (CultureNotFoundException ex)
			{
				return Content(HttpStatusCode.BadRequest, ex.Message);
			}

			var update = new List<HelpDataString>();

			using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(request.contents)))
			{
				var translated = XmlResourceStringSource.ReadAll(ms);
				if (!translated.Any())
				{
					shouldSendNotificationEmail = true;
					return Content(HttpStatusCode.OK, "Empty translated resources");
				}

				var dataStrings = translated.Select(r => HelpDataString.CreateFromResourceStringData(r, language));
				if (!dataStrings.Any())
				{
					return Content(HttpStatusCode.NoContent, "Empty data strings");
				}

				foreach (var str in dataStrings)
				{
					str.HD_Language = language;
					update.Add(str);

					var eng = ResourceStringsFactory.Lookup(Res.DefaultLanguage, str.HD_Code);
					if (eng == null)
					{
						eng = new HelpDataString();
						eng.HD_Language = Res.DefaultLanguage;
						eng.HD_Code = str.HD_Code;
						eng.HD_Caption = "";
						update.Add(eng);
					}
				}
			}

			if (update.Count > 0)
			{
				using (new UseDatabaseStrings())
				{
					ResourceStringsFactory.Save(EditReasons.Codes.CustomizableDataTranslation, Db.Connection, update.ToArray());

					return Content(HttpStatusCode.OK, string.Format(CultureInfo.InvariantCulture, "{0} records have been updated.", update.Count));
				}
			}

			return Content(HttpStatusCode.NoContent, "No records have been updated");
		}

		void SendNotificationEmail(ImportRequestData request, string error)
		{
			EmailDef email = new EmailDef();
			email.Body = string.Format(CultureInfo.InvariantCulture, "Could not process data. Language = {0}, Contents = {1}. Error: {2}", request?.language, request?.contents, error);
			email.Subject = "Could not import exam translation data";
			Env.OutgoingMailManager.CreateAndSave(email, EDIDataRegistry.Instance.InternalNotificationGroup.Value, GroupSourceLocator.GetFromRegistryItem(EDIDataRegistry.Instance.InternalNotificationGroup));
		}

		class UseDatabaseStrings : IDisposable
		{
			readonly bool initialDisabled;
			readonly bool initialUseDb = true;

			public UseDatabaseStrings()
			{
#if DEBUG
				initialDisabled = DatabaseResourceStringSource.disabled;
				initialUseDb = ResourcesDeltaSource.UseDatabaseStrings.Value;

				DatabaseResourceStringSource.disabled = false;
				ResourcesDeltaSource.UseDatabaseStrings.Value = true;
#endif
			}

			public void Dispose()
			{
				if (initialDisabled || !initialUseDb)
				{
#if DEBUG
					ResourcesDeltaSource.UseDatabaseStrings.Value = initialUseDb;
					DatabaseResourceStringSource.disabled = initialDisabled;
#endif
				}
			}
		}

		protected virtual bool IsValidClientIp()
		{
			return IPAddress.TryParse(HttpContext.Current.Request.UserHostAddress, out var address) && new[] { new IPAddressRange("10.61.0.0/16"), new IPAddressRange("10.2.0.0/16") }.IsInRange(address);
		}
	}
}
