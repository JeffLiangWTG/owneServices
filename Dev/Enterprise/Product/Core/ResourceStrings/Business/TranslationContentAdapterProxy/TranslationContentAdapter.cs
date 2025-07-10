using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CargoWise.Types;
using Newtonsoft.Json;

namespace Enterprise.ResourceStrings.Business
{
	public class TranslationContentAdapter
	{
		const string ServiceUrl = "https://myaccount-portal.cargowise.com/myaccount/api/ContentTranslation/";

		public TranslationContentAdapter(string contentModule)
		{
			ContentModule = contentModule;
		}

		readonly string ContentModule;

		public void Export(string targetDirectory, string language = "")
		{
			Dictionary<string, string> contents = GetContents(language);

			if (contents != null)
			{
				foreach (var entry in contents)
				{
					var filename = SanitizeFilename(entry.Key);
					string filePath = GetFilePath(targetDirectory, filename);
					int index = 0;
					while (File.Exists(filePath))
					{
						var newFilename = string.Format(CultureInfo.InvariantCulture, "{0} ({1})", filename, index++);
						filePath = GetFilePath(targetDirectory, newFilename);
					}

					File.WriteAllText(filePath, entry.Value);
				}
			}
		}

		string GetFilePath(string targetDirectory, string filename)
		{
			return Path.Combine(targetDirectory, string.Format(CultureInfo.InvariantCulture, "{0}.xml", filename));
		}

		string SanitizeFilename(string filename)
		{
			var reservedWords = new[]
			{
				"CON", "PRN", "AUX", "CLOCK$", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4",
				"COM5", "COM6", "COM7", "COM8", "COM9", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4",
				"LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
			};

			foreach (var word in reservedWords)
			{
				if (filename.Equals(word, StringComparison.OrdinalIgnoreCase))
				{
					filename = "_reservedword_";
					break;
				}
			}

			var invalid = Path.GetInvalidFileNameChars();
			foreach (var c in invalid)
			{
				filename = filename.Replace(c, '_');
			}

			filename = filename.TrimEnd('.');

			if (string.IsNullOrEmpty(filename))
			{
				filename = ZDateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);
			}

			return filename;
		}

		const int maxRetries = 1;
		protected Dictionary<string, string> GetContents(string language)
		{
			int retry = 0;
			do
			{
				try
				{
					return GetContentsCore(language);
				}
				catch (TaskCanceledException ex)
				{
					if (ex.CancellationToken.IsCancellationRequested)
					{
						throw new TaskCanceledException("Cancellation requested", ex);
					}
					else
					{
						if (retry >= maxRetries)
						{
							throw;
						}
					}
				}
			}
			while (retry++ < maxRetries);

			return null;
		}

		protected virtual Dictionary<string, string> GetContentsCore(string language)
		{
			Dictionary<string, string> contents;
			using (var client = new HttpClient())
			{
				var json = JsonConvert.SerializeObject(new { Language = language, ContentModule = ContentModule });
				using (var requestContent = new StringContent(json, Encoding.UTF8, "application/json"))
				{
					client.Timeout = TimeSpan.FromMinutes(5);
					client.DefaultRequestHeaders.Accept.Clear();
					client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
					var uri = new Uri(ServiceUrl + "Export");
					using (var response = client.PostAsync(uri, requestContent).Result)
					{
						var responseContentTask = response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
						responseContentTask.Wait();
						contents = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContentTask.Result);
					}
				}
			}
			return contents;
		}

		public void Import(string language, string contentDirectory)
		{
			using (var client = new HttpClient())
			{
				client.Timeout = TimeSpan.FromMinutes(5);

				foreach (var file in Directory.EnumerateFiles(contentDirectory, "*.xml"))
				{
					var contents = File.ReadAllText(file);
					var json = JsonConvert.SerializeObject(new { Contents = contents, Language = language });

					using (var requestContent = new StringContent(json, Encoding.UTF8, "application/json"))
					{
						client.DefaultRequestHeaders.Accept.Clear();
						client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
						ImportCore(client, requestContent);
					}
				}
			}
		}

		protected virtual void ImportCore(HttpClient client, StringContent requestContent)
		{
			var uri = new Uri(ServiceUrl + "Import");
			using (var response = client.PostAsync(uri, requestContent).Result)
			{
				var responseContentTask = response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
				responseContentTask.Wait();
			}
		}
	}
}
