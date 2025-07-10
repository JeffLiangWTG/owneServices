using System;
using System.Text;
using CargoWise.Common;
using Enterprise.Integration;
using PdfToTextNet;

namespace Enterprise.Client.EDI.ServiceTasks.ELearningDocument
{
	public interface IPdfTextExtractor
	{
		string GetText(byte[] data);
	}

	public class PdfTextExtractor : IPdfTextExtractor
	{
		readonly ILogger logger;
		public PdfTextExtractor(ILogger logger)
		{
			this.logger = logger;
		}
		public string GetText(byte[] data)
		{
			logger?.Log(LogType.Information, $"GetText started, data length={data?.Length}");
			var res = new StringBuilder();
			try
			{
				using (var document = PdfDocument.Open(data))
				{
					document.GetPages().ForEach(page =>
					{
						logger?.Log(LogType.Information, $"GetText get pages");
						page.GetTextLines().ForEach(line => res.Append(line.ToSingleLineOfText()));
						page.Operations.Clear();
						page.Letters.Clear();
					});
				}
				return res.ToString();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var message = $"something went wrong:{ex}, class:{nameof(PdfTextExtractor)}, method:{nameof(GetText)}";
				logger?.Log(LogType.Error, message);
				return string.Empty;
			}
		}
	}
}
