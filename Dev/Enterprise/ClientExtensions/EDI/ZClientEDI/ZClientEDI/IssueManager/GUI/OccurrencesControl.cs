using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
#if !WINZOR
using System.Text;
#endif
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
#if !WINZOR
using CargoWise.Windows.UI;
#endif
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using HtmlAgilityPack;
using WTG.RtfConverter;
using WTG.StaticAnalysis.Annotation;
#if WINZOR
using WTG.RtfConverter.Dom;
#endif
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.IssueManager.GUI
{
	public partial class OccurrencesControl : ZUserControl
	{
		public OccurrencesControl()
		{
			InitializeComponent();

			CreateContextMenuItems();
		}

		EdiHelpErrorLog HelpErrorLog => ((ZForm)FindForm()).BusinessEntity as EdiHelpErrorLog;

		HelpErrorLogOccurrence Occurrence => OccurrencesBindingManager == null || OccurrencesBindingManager.Position < 0 ? null : OccurrencesBindingManager.GetCurrent() as HelpErrorLogOccurrence;

		#region CreateContextMenuItems

		void CreateContextMenuItems()
		{
			OccurrencesGrid.ContextMenu.MenuItems.Add("Split").Click += splitMenuItem_Click;
			OccurrencesGrid.ContextMenu.MenuItems.Add("Export Error Report To XML File").Click += exportReportToXmlFileMenuItem_Click;
		}

		#endregion

		#region ExportReportToXmlFile

		void exportReportToXmlFileMenuItem_Click(object sender, EventArgs e)
		{
			if (HelpErrorLog != null && Occurrence != null)
			{
				var file = CreateErrorReportXmlFile(Occurrence);

				try
				{
					OpenFile(file);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Globals.Message.ShowError($"Failed to open exported error report xml file {Path.GetFileName(file.Filename)}");
				}
			}
		}

		TempFile CreateErrorReportXmlFile(HelpErrorLogOccurrence occurrence)
		{
			var file = TempFile.NewWithExtension("xml");
			File.WriteAllText(file.Filename, occurrence.HO_XMLData);
			return file;
		}

		protected virtual object OpenFile(TempFile tempFile) => ObjectFactory.Get<IUserFileAccess>().OpenFile(tempFile.Filename);

		#endregion

		#region Splitting

		void splitMenuItem_Click(object sender, EventArgs e)
		{
			if (HelpErrorLog != null && Occurrence != null)
			{
				var form = new ErrorLogProcessorForm(new LogSplitter(Occurrence), "Split Occurrence");
				form.Show();
				form.RunProcessor();
			}
		}

		#endregion

		#region View Locally

		internal void ViewLocallyButton_Click(object sender, EventArgs e)
		{
			if (Occurrence == null)
			{
				return;
			}

			var tempFile = (TempFile)reportHash[Occurrence];
			if (tempFile == null)
			{
				Globals.Message.ShowError("Can't find the source file");
				return;
			}

			try
			{
				OpenFile(tempFile);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError($"Failed to open exported file {Path.GetFileName(tempFile.Filename)} to view locally");
			}
		}

		#endregion

		#region More Details

		internal void MoreDetailsButton_Click(object sender, EventArgs e)
		{
			var outputFile = CalculateAndCacheMoreDetailsFile();
			if (outputFile == null)
			{
				return;
			}
			OpenFile(outputFile);
		}

		protected virtual bool IsContinueCalculateMoreDetails()
		{
			var message = Res.GetString("534f8277-b482-4e00-8b82-95629a120691", "This operation will calculate source line and IL information, take few minutes or more. Still continue?");
			return Globals.Message.Show(message, "More Details", MessageBoxButtons.OKCancel, DialogResult.Cancel) == DialogResult.OK;
		}

		TempFile CalculateAndCacheMoreDetailsFile()
		{
			try
			{
				if (Occurrence == null)
				{
					return null;
				}
				var cachedFile = (TempFile)reportHash[Occurrence];

				if (ExistSourceLineAndILInfo(cachedFile))
				{
					return cachedFile;
				}

				if (!IsContinueCalculateMoreDetails() || !CalculateSourceAndILForXmlData(Occurrence))
				{
					return null;
				}
				cachedFile?.Dispose();

				var report = new ExceptionReport(Occurrence.HO_XMLData);
				cachedFile = new ExceptionReportRenderer(report).CreateHtmlFile();
				RewriteHtmlForCache(cachedFile.Filename);
				reportHash[Occurrence] = cachedFile;
				return cachedFile;
			}
			catch (Exception e)
			{
				ErrorReporter.ReportOnce("OccurrenceControlMoreDetails", e.GetFullMessage(), e);
				return null;
			}
		}

		static bool CalculateSourceAndILForXmlData(HelpErrorLogOccurrence occurrence)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				try
				{
					CalculateLineNo.Calculate(occurrence);
					return true;
				}
				finally
				{
					Cursor.Current = Cursors.Default;
				}
			}
			catch (CalcLineNoException ex)
			{
				Globals.Message.ShowError(ex.ToString());
			}
			catch (ReleaseBuildException ex)
			{
				Globals.Message.ShowError(ex.ToString());
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("Error when calculating line number.", ex);
			}

			return false;
		}

		bool ExistSourceLineAndILInfo(TempFile outputFile)
		{
			if (outputFile == null)
			{
				return false;
			}
			var html = string.Empty;
			using (var sr = new StreamReader(outputFile.Filename))
			{
				html = sr.ReadToEnd();
			}
			var hasSourceLineNoLink = html.Contains("sourceLineNoHyperLink");
			var hasAdvanceLineNoLink = html.Contains("advancedLineNoHyperLink");

			return hasAdvanceLineNoLink && hasSourceLineNoLink;
		}

		#endregion

		#region Open Licence

		internal void OpenLicence()
		{
			if (OccurrencesGrid.ListManager.Position > -1)
			{
				var occurrence = (HelpErrorLogOccurrence)OccurrencesGrid.ListManager.GetCurrent();
				if (occurrence != null)
				{
					var factory = new BusinessObjectFactory() { RefreshEnabled = false };
					LicenceHeader licence = null;

					var clientCompany = factory.Load<ClientCompany>(occurrence.HO_LCC);
					if (clientCompany != null)
					{
						var org = clientCompany.Org;
						if (org != null && org.LicCompany != null)
						{
							licence = org.LicCompany.LicHeadersForAllDatabases.Cast<LicenceHeader>().FirstOrDefault(x => x.Database.PK == clientCompany.LCC_LD);
						}

						if (licence == null)
						{
							licence = clientCompany.Database.UsageOwnerOrFirstLicence;
						}
					}

					if (licence == null)
					{
						var db = factory.Load<LicenceDatabase>(occurrence.HO_LD);
						if (db != null)
						{
							licence = db.LicHeadersForAllCompanies
								.Cast<LicenceHeader>()
								.OrderBy(x => x.LA_IsActive ? 0 : 1)
								.FirstOrDefault();
						}
					}

					if (licence != null)
					{
						var licenceController = ZControllerFactory.Create(ClientControllerRegistration.LicenseKey);
						licenceController.ShowEditForm(licence);
						LastController = licenceController;
					}
				}
			}
		}

		void OccurrencesGrid_DoubleClick(object sender, EventArgs e)
		{
			OpenLicence();
		}

		#endregion

		internal ZController LastController;

		#region Binding

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			// The following line is moved from function SetDataBinding because OccurrencesGrid.ListManager is always null in that function for WebBrowser control
			OccurrencesBindingManager = CurrentDataItem == null ? null : OccurrencesGrid.ListManager;
		}

		internal BindingManagerBase OccurrencesBindingManager
		{
			get { return occurrencesBindingManager; }
			set
			{
				if (occurrencesBindingManager != null)
				{
					occurrencesBindingManager.CurrentChanged -= new EventHandler(OccurrencesBindingManager_CurrentChanged);
				}
				occurrencesBindingManager = value;
				if (occurrencesBindingManager != null)
				{
					occurrencesBindingManager.CurrentChanged += new EventHandler(OccurrencesBindingManager_CurrentChanged);
				}
				ShowCurrentOccurrence();
			}
		}
		BindingManagerBase occurrencesBindingManager;

		void OccurrencesBindingManager_CurrentChanged(object sender, EventArgs e)
		{
			ShowCurrentOccurrence();
		}

		#endregion

		#region Implementation

		void ShowCurrentOccurrence()
		{
			var html = string.Empty;

			if (Occurrence == null || Occurrence.HO_XMLData.IsEmpty)
			{
				return;
			}

			var outputFile = (TempFile)reportHash[Occurrence];

			if (outputFile == null)
			{
				var report = new ExceptionReport(Occurrence.HO_XMLData);
				outputFile = new ExceptionReportRenderer(report).CreateHtmlFile();
				RewriteHtmlForCache(outputFile.Filename);
				reportHash[Occurrence] = outputFile;
			}

			html = File.ReadAllText(outputFile.Filename);

			var handledHtmlDocument = RemoveUnusedHtmlElesInRtf(html);
#if WINZOR
			var removeStyleContent = RemoveStyleContent(handledHtmlDocument);
			var phrases = ManualCssStyleHtmlTags
				.Select(kv => KeyValuePair.Create(kv.Key, new Phrase { Bold = true, Font = "Arial", Size = new Unit(kv.Value, UnitType.Point) }))
				.ToDictionary();

			var wtgdom = HtmlParser.Parse(removeStyleContent)
				.Reduce(HtmlDecoder.CreateFactory(phrases))
				.Collect();
			var markupContent = wtgdom.Reduce(HtmlEncoder.CreateFactory()).Markup();

			richTextBox.Html = markupContent;
#else
			var handledRtfText = ConvertHtmlToRtf(handledHtmlDocument);
			richTextBox.Rtf = handledRtfText;
			if (!string.IsNullOrEmpty(handledRtfText))
			{
				BeautifulRtfContentStyle();
			}
#endif
		}

#if WINZOR
		static readonly ImmutableDictionary<string, int> ManualCssStyleHtmlTags =
			ImmutableDictionary.CreateRange([
				KeyValuePair.Create("h1", 14),
				KeyValuePair.Create("h2", 12),
			]);

		string RemoveStyleContent(string html)
		{
			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(html);

			ManualCssStyleHtmlTags.Keys.ForEach(titleTag =>
			{
				var titleNodes = htmlDoc.DocumentNode.SelectNodes($"//{titleTag}");
				if (titleNodes != null)
				{
					foreach (var titleNode in titleNodes)
					{
						titleNode.SetAttributeValue("class", titleTag);
					}
				}
			});

			htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'overlay')]")?.Remove();
			return htmlDoc.DocumentNode.SelectSingleNode("//body").InnerHtml;
		}
#endif

		internal static string RemoveUnusedHtmlElesInRtf(string html)
		{
			if (string.IsNullOrEmpty(html))
			{
				return string.Empty;
			}

			html = Regex.Replace(html, @"\(<a href=""#Top"">Top</a>\)", string.Empty);
			html = EscapeUncloseHtmlTag(html);
			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(html);
			var bodyNode = htmlDoc.DocumentNode.SelectSingleNode("//body");
			if (bodyNode != null)
			{
				var headTitleLinks = bodyNode.ChildNodes.TakeUntil(e => e.Name == "h1" && e.InnerText == "Summary").ToList();
				if (headTitleLinks != null && headTitleLinks.Count > 0)
				{
					headTitleLinks.RemoveAt(headTitleLinks.Count - 1);
					foreach (var item in headTitleLinks)
					{
						item.Remove();
					}
				}

				bodyNode.SelectNodes("//a")
					?.Where(e => e.InnerHtml.Contains("<h1>") || e.InnerHtml.Contains("<h2>"))
					.ForEach(e =>
					{
						var innerNode = HtmlNode.CreateNode(e.InnerHtml);
						e.ParentNode.ReplaceChild(innerNode, e);
					});

				bodyNode.SelectNodes("//a[starts-with(@name,'sourceLineNoHyperLink')] | //button | //a[starts-with(@name, 'advancedLineNoHyperLink')]")
					?.ForEach(e => e.Remove());

				return htmlDoc.DocumentNode.OuterHtml;
			}

			return html;
		}

		static readonly string OpenTagRegex = @"<(\w+)[^>]*>";
		static readonly string CloseTagRegex = @"</(\w+)[^>]*>";

		static string EscapeUncloseHtmlTag(string html)
		{
			var openTagSet = TagExtract(OpenTagRegex);
			var closeTagSet = TagExtract(CloseTagRegex);

			openTagSet.ExceptWith(closeTagSet);
			openTagSet.ExceptWith(ExceptionReportRenderer.SelfCloseTags);

			if (openTagSet.IsNullOrEmpty())
			{
				return html;
			}

			var unclosedTagPattern = string.Join("|", openTagSet.Select(tag => $@"<({tag}[^>]*)>"));
			var unclosedTagRegex = new Regex(unclosedTagPattern, RegexOptions.IgnoreCase);

			var groupIndex = 1;
			return unclosedTagRegex.Replace(html, match =>
			{
				var tagName = match.Groups[groupIndex].Value;
				groupIndex++;
				return $"&lt;{tagName}&gt;";
			});

			HashSet<string> TagExtract(string regex)
			{
				var openTagCollections = Regex.Matches(html, regex);
				var openTags = openTagCollections.Cast<Match>().Select(m => m.Groups[1].Value).ToHashSet();
				return openTags;
			}
		}

		internal static void RewriteHtmlForCache(string filename)
		{
			var html = File.ReadAllText(filename);
			if (string.IsNullOrEmpty(html))
			{
				return;
			}

			html = RemoveEmptyTable(html);

			File.WriteAllText(filename, html);
		}

		internal static string RemoveEmptyTable(string html)
		{
			if (string.IsNullOrEmpty(html))
			{
				return string.Empty;
			}

			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(html);
			var bodyNode = htmlDoc.DocumentNode.SelectSingleNode("//body");
			if (bodyNode == null)
			{
				return html;
			}
			var emptyTables = bodyNode.SelectNodes("//table[not(.//*[normalize-space() != ''])]");
			if (emptyTables != null)
			{
				foreach (var emptyTable in emptyTables)
				{
					emptyTable.Remove();
				}
			}

			return htmlDoc.DocumentNode.OuterHtml;
		}

		protected readonly Hashtable reportHash = new Hashtable();

		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			DeleteTemporaryFiles();
			base.Dispose(isNotFinalizing);
		}

		void DeleteTemporaryFiles()
		{
			foreach (TempFile tempFile in reportHash.Values)
			{
				tempFile.Dispose();
			}
		}
		#endregion

#if !WINZOR
		[ThreadSafe]
		static readonly HtmlToRtfConverter HtmlToRtfConverter = new();

		void BeautifulRtfContentStyle()
		{
			richTextBox.SelectAll();
			richTextBox.SelectionIndent += ControlDpiScalingHelper.NewScaledPadding(15).Left;
			richTextBox.SelectionLength = 0;
		}
		internal static string ConvertHtmlToRtf(string htmlStr)
		{
			if (string.IsNullOrEmpty(htmlStr))
			{
				return string.Empty;
			}
			var handledHtmlDocument = new HtmlDocument();
			handledHtmlDocument.LoadHtml(htmlStr);
			var bodyNode = handledHtmlDocument.DocumentNode.SelectSingleNode("//body");
			return ExceptionReportRtfConvertHelper.ConvertHtmlToRtf(bodyNode);
		}

		internal static class ExceptionReportRtfConvertHelper
		{
			class CellData
			{
				// 0 - normal, 1 - merged, 2 - be merged
				internal int CellState { get; }
				internal HtmlNode Data { get; }

				internal CellData(int cellState, HtmlNode data)
				{
					CellState = cellState;
					Data = data;
				}
			}

			internal static string ConvertFactoryStatisticsNestedTableToRtf(HtmlNode tableNode)
			{
				var rows = tableNode.SelectNodes("./tbody/tr | tr");
				if (rows.IsNullOrEmpty())
				{
					return string.Empty;
				}

				try
				{
					var sb = new StringBuilder();
					for (var i = 0; i < rows.Count; i++)
					{
						var rowItem = rows[i];
						var isTitle = i == 0;
						var rowDatas = BuildVirtualTable(isTitle, rowItem);

						var rtfForRowDatas = GenerateRtfForRowDataList(isTitle, rowDatas);
						sb.Append(rtfForRowDatas);
					}
					sb.Append(@"\pard\par");
					return sb.ToString();
				}
				catch (Exception)
				{
					var placeholderText = "This section is not available in RTF, please click 'view locally' button to check details in HTML.";
					return $@"\pard\f0\fs20 {placeholderText} \par\line";
				}
			}

			static List<List<CellData>> BuildVirtualTable(bool isTitle, HtmlNode rowItem)
			{
				var rowDatas = new List<List<CellData>>();
				if (isTitle)
				{
					var rowData = rowItem.ChildNodes.Where(e => e.Name == "td")
						.Select(e => new CellData(0, e))
						.ToList();
					rowDatas.Add(rowData);
				}
				else
				{
					var cellItems = rowItem.ChildNodes.Where(e => e.Name == "td").ToList();
					var nestedTableItem = cellItems[cellItems.Count - 1].SelectSingleNode("table");
					var nestedRows = nestedTableItem.SelectNodes("./tbody/tr | tr");
					for (var nestedRowIdx = 0; nestedRowIdx < nestedRows.Count; nestedRowIdx++)
					{
						var nestedRow = nestedRows[nestedRowIdx];
						var nestRowData = new List<CellData>();
						// merged item
						for (var m = 0; m < cellItems.Count - 1; m++)
						{
							if (nestedRowIdx == 0)
							{
								nestRowData.Add(new CellData(1, cellItems[m]));
							}
							else
							{
								nestRowData.Add(new CellData(2, HtmlNode.CreateNode(string.Empty)));
							}
						}
						var nestedRowEles = nestedRow.ChildNodes.Where(e => e.Name == "td")
							.Select(e => new CellData(0, e)).ToList();
						nestRowData.AddRange(nestedRowEles);
						rowDatas.Add(nestRowData);
					}
				}

				return rowDatas;
			}

			static string GenerateRtfForRowDataList(bool isTitle, List<List<CellData>> rowDatas)
			{
				// 15 twips == 1px
				var width = 30000;
				var titleCellRadio = new List<double> { 0.1, 0.5, 0.55, 1 };
				var dataCellRadio = new List<double> { 0.1, 0.5, 0.55, 0.6, 0.65, 0.7, 0.75, 0.8, 0.85, 0.9, 0.95, 1 };
				var radio = isTitle ? titleCellRadio : dataCellRadio;
				var sb = new StringBuilder();
				foreach (var rowData in rowDatas)
				{
					var cellRtf = string.Empty;
					var colRtf = string.Empty;
					for (var i = 0; i < radio.Count; i++)
					{
						var radioVal = radio[i];
						var cellData = rowData[i];
						if (cellData.CellState == 0)
						{
							cellRtf += $@"\cellx{radioVal * width}";
						}
						if (cellData.CellState == 1)
						{
							cellRtf += $@"\clvmgf\cellx{radioVal * width}";
						}
						if (cellData.CellState == 2)
						{
							cellRtf += $@"\clvmrg\cellx{radioVal * width}";
						}

						var text = EscapeTextForRTF(cellData.Data);
						colRtf += $@"\intbl {text} \cell";
					}
					var rowRtf = $@"\trowd {cellRtf} \pard {colRtf} \row
";
					sb.Append(rowRtf);
				}
				return sb.ToString();
			}

			internal static string ConvertHtmlToRtf(HtmlNode bodyNode)
			{
				var rtfContent = bodyNode.ChildNodes.Select(e =>
				{
					if (e.Name == "h1")
					{
						return $@"\pard\f0\b\fs48 {e.InnerText} \b0\par\fs20\line
";
					}
					if (e.Name == "h2")
					{
						return $@"\pard\f0\b\fs36 {e.InnerText} \b0\par\fs20\line
";
					}
					if (e.Name == "h3")
					{
						return $@"\pard\f0\b\fs27 {e.InnerText} \b0\par\fs20\line
";
					}
					if (e.Name == "h4")
					{
						return $@"\pard\f0\b\fs24 {e.InnerText} \b0\par\fs20\line
";
					}
					if (e.Name == "p")
					{
						var text = EscapeTextForRTF(e);
						return $@"\pard\f0\fs20 {text} \par\line";
					}
					if (e.Name == "b")
					{
						var text = EscapeTextForRTF(e);
						return $@"\pard\f0\b\fs20 {text} \b0\par\line";
					}
					var hasSubTable = e.InnerHtml.Contains("<table>") && e.InnerHtml.Contains("</table>");
					if (e.Name == "table" && !hasSubTable)
					{
						return ConvertSimpleTableToRtf(e);
					}
					if (e.Name == "table" && hasSubTable)
					{
						return ConvertFactoryStatisticsNestedTableToRtf(e);
					}
					return string.Empty;
				}).Aggregate(string.Concat);

				return $@"{{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang3081{{\fonttbl{{\f0\fnil\fcharset0 Microsoft Sans Serif;}}}}
{{\*\generator Riched20 10.0.19041}}\viewkind4\uc1
{rtfContent}}}";
			}

			static readonly string RtfContentPrefix = @"\{\\\*\\generator RTFConverter W\.X\.Y\.Z\}";

			static readonly string RtfContentExtractPattern = @$"{RtfContentPrefix}{{{{(.*?)}}\\par}}}}$";

			static string EscapeTextForRTF(HtmlNode htmlNode)
			{
				var text = PreHandleForContentEscaping(htmlNode);
				var convertContent = HtmlToRtfConverter.Convert(text);

				var match = Regex.Match(convertContent, RtfContentExtractPattern);
				if (match.Success)
				{
					return match.Groups[1].Value;
				}

				return text;
			}

			static string PreHandleForContentEscaping(HtmlNode htmlNode)
			{
				if (htmlNode == null || string.IsNullOrEmpty(htmlNode.InnerHtml))
				{
					return string.Empty;
				}
				return ContainsHtmlTags(htmlNode.InnerHtml) ? RemoveUnusedHtmlTags(htmlNode.InnerHtml) : htmlNode.InnerText;
			}

			static readonly Regex HtmlTagRegex = new("<.*?>", RegexOptions.Compiled);
			static bool ContainsHtmlTags(string text)
			{
				if (string.IsNullOrEmpty(text))
				{
					return false;
				}

				return HtmlTagRegex.IsMatch(text);
			}

			static string ConvertSimpleTableToRtf(HtmlNode tableNode)
			{
				var tableNodeOuterHtml = tableNode.OuterHtml;
				var htmlAfterRemoveUnusedHtmlTags = RemoveUnusedHtmlTags(tableNodeOuterHtml);
				var content = HtmlToRtfConverter.Convert(htmlAfterRemoveUnusedHtmlTags);
				var rtfTableContentExtractRegexExpression = @$"{RtfContentPrefix}{{(.*?)\\pard}}}}";
				var match = Regex.Match(content, rtfTableContentExtractRegexExpression);
				if (match.Success)
				{
					var rtfTableContent = match.Groups[1].Value;
					return adjustTableWidth(rtfTableContent);
				}

				return string.Empty;
			}

			static string RemoveUnusedHtmlTags(string tableNodeOuterHtml)
			{
				return tableNodeOuterHtml.Replace(@"<pre>", string.Empty).Replace(@"</pre>", string.Empty);
			}

			static readonly string cellWidthPattern = @"cellx(\d+)";
			static readonly string unusedRtfSymbolPattern = @"\\intbl(.*?)\\cell";
			static readonly int tableWidthInTwips = 25000;
			static readonly List<double> cellWidthRadioForTwoColumnsTable = new List<double> { 0.2, 1 };
			static string adjustTableWidth(string rtfTableContent)
			{
				var matchCollection = Regex.Matches(rtfTableContent, cellWidthPattern, RegexOptions.IgnoreCase);
				var originalCellWidthSets = matchCollection.Cast<Match>()
					.Select(e => int.Parse(e.Groups[1].Value)).ToImmutableSortedSet();
				var columnCount = originalCellWidthSets.Count;
				if (columnCount == 0)
				{
					return rtfTableContent;
				}

				var newRtfTableContent = rtfTableContent;
				var newCellWidthSets = calculateCellWidth(columnCount);
				for (var i = 0; i < originalCellWidthSets.Count; i++)
				{
					var originalCellWidth = originalCellWidthSets[i];
					var newCellWidth = newCellWidthSets[i];
					newRtfTableContent = newRtfTableContent.Replace("cellx" + originalCellWidth, "cellx" + newCellWidth);
				}

				newRtfTableContent = Regex.Replace(newRtfTableContent, unusedRtfSymbolPattern, "$1\\cell");
				return newRtfTableContent;
			}

			static ImmutableSortedSet<int> calculateCellWidth(int columnCount)
			{
				var cellRadio = columnCount == 2 ? cellWidthRadioForTwoColumnsTable : Enumerable.Range(1, columnCount).Select(e => (double)e / columnCount).ToList();
				return cellRadio.Select(radio => (int)(radio * tableWidthInTwips)).ToImmutableSortedSet();
			}
		}
#endif
	}
}
