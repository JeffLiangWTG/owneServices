using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.FlexCelIntegration;
using FlexCel.Core;
using FlexCel.XlsAdapter;

namespace Enterprise.DocumentVisualizer.Build
{
	public sealed class ResourceStringAnalyzer
	{
		public ushort Asmid => ResourceStringExtensions.Asmid;

		public IReadOnlyCollection<ResourceStringData> ExtractResStringData(IReadOnlyCollection<byte[]> templateData)
		{
			var res = new Dictionary<string, ResourceStringData>();

			foreach (var data in templateData)
			{
				var resStrings = ExtractResStringData(data);

				foreach (var resString in resStrings)
				{
					res[resString.Key] = resString;
				}
			}

			return res.Values.ToArray();
		}

		IReadOnlyCollection<ResourceStringData> ExtractResStringData(byte[] templateData)
		{
			if (templateData == null
				|| templateData.Length == 0)
			{
				return Array.Empty<ResourceStringData>();
			}

			var template = CreateTemplate(templateData);

			if (template == null)
			{
				return Array.Empty<ResourceStringData>();
			}

			var res = new HashSet<ResourceStringData>();

			foreach (var resString in ExtractResStringData(template))
			{
				res.Add(resString);
			}

			return res;
		}

		IReadOnlyCollection<ResourceStringData> ExtractResStringData(ITemplate template)
		{
			if (!(template is IStandardTemplate standardTemplate))
			{
				return Array.Empty<ResourceStringData>();
			}
			var res = InitializeResStringDataCollection(standardTemplate);

			if (!template.EnableTranslation)
			{
				return res;
			}

			var cells = standardTemplate
				.SelectContentCells();

			foreach (var cell in cells)
			{
				var unquotedMacro = Convert.ToString(cell.Value);
				var caption = unquotedMacro.GetResStringCaption();

				if (!string.IsNullOrWhiteSpace(caption))
				{
					var key = unquotedMacro.GetResStringKey();
					var resStringData = new ResourceStringData(key, caption);
					res.Add(resStringData);
				}
			}

			return res;
		}

		HashSet<ResourceStringData> InitializeResStringDataCollection(IStandardTemplate standardTemplate)
		{
			var res = new HashSet<ResourceStringData>();

			var unquotedMacros = new string[]
			{
				standardTemplate.Config?.MessageRecipient?.Macro?.Trim('\"'),
				standardTemplate.Config?.DocumentName?.Macro?.Trim('\"')
			};

			foreach (var macro in unquotedMacros)
			{
				var caption = macro.GetResStringCaption();

				if (!string.IsNullOrWhiteSpace(caption))
				{
					var key = macro.GetResStringKey();
					var resStringData = new ResourceStringData(key, caption);
					res.Add(resStringData);
				}
			}

			return res;
		}

		ITemplate CreateTemplate(byte[] templateData)
		{
			var worksheet = CreateWorksheet(templateData);

			if (worksheet == null)
			{
				return null;
			}

			try
			{
				var res = worksheet.CreateTemplate();

				return res.IsRight
					? res.Right
					: null;
			}
			catch (FlexCelCoreException)
			{
				// eat
				return null;
			}
		}

		IWorksheet CreateWorksheet(byte[] templateData)
		{
			using (var templateStream = new MemoryStream(templateData))
			{
				try
				{
					return FlexCelWorksheet.FromStream(templateStream);
				}
				catch (FlexCelXlsAdapterException)
				{
					//eat
					return null;
				}
			}
		}
	}
}
