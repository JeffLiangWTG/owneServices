using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using SystemImage = System.Drawing.Image;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class Image : ValueProvider, IControlSizeProvider
	{
		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "This function has supported out of screen processing")]
		protected override ValueProviderDocumenter GetDocumentation()
		{
			var message = ResString.GetMultilingualString("2d884b09-c170-461c-be1d-a5668acfa1e3",
				@"Will cause an image of the type specified to be inserted from the {0} registry scaled to fill the number of rows and columns specified in the second and third parameters. 
If {1} is 'Y' then the image will maintain it's original aspect ratio within the bounds of the rows/columns set.
Available image types: {2}, {3}, {4}, {5}, {6}.",
				Core.Constants.ProductName, "isAspectRatioLocked", "CompanyLogo", "ChequeLogoWithBankDetail", "HBLLogo", "HAWBLogo", "ChequeLogo");
			return new ValueProviderDocumenter("<Image({imagetype}, {heightinrows}, {widthincolumns}, {isAspectRatioLocked})>", message,
				new List<(string example, object expectedResult)> { ((NoResString)"<Image(COMPANYLOGO, 2, 4, Y)>", new System.Drawing.Size(5, 5)) });
		}

		public Image()
		{
			AddImageValueProviders();
		}

		void AddImageValueProviders()
		{
			ImageValueProviders.Add("COMPANYLOGO", new ImageValueProvider(CompanyLogoValueProvider));
			ImageValueProviders.Add("HBLLOGO", new ImageValueProvider(HBLLogoValueProvider));
			ImageValueProviders.Add("HAWBLOGO", new ImageValueProvider(HAWBLogoValueProvider));
			ImageValueProviders.Add("CHEQUELOGO", new ImageValueProvider(ChequeLogoValueProvider));
			ImageValueProviders.Add("CHEQUELOGOWITHBANKDETAIL", new ImageValueProvider(ChequeLogoWithBankDetailValueProvider));
			ImageValueProviders.Add("USERSIGNATURE", new ImageValueProvider(UserSignatureValueProvider));
		}

		readonly Dictionary<string, ImageValueProvider> ImageValueProviders = new Dictionary<string, ImageValueProvider>(StringComparer.OrdinalIgnoreCase);
		delegate SystemImage ImageValueProvider(Report report);

		protected override void ResetCore()
		{
			ImageValueProviders.Clear();
			AddImageValueProviders();
		}

		#region GetReplacement

		protected override object GetReplacementCore(string macro, Report report)
		{
			object result = null;

			try
			{
				result = DoReplacement(macro, report);
			}
			catch (DocumentEngineException exception)
			{
				ReportMacroError(report, exception.Message);
			}

			return result;
		}

		object DoReplacement(string macro, Report report)
		{
			var matchedGroups = Regex.Match(macro).Groups;
			var imageSource = matchedGroups["ImageSource"].Value;
			var columnsHeight = GetIntFromString(matchedGroups["ColumnsHeight"].Value);
			var columnsWidth = GetIntFromString(matchedGroups["ColumnsWidth"].Value);

			if (columnsHeight < 1 || columnsWidth < 1)
			{
				if (columnsHeight < 1)
				{
					ReportMacroError(report, Res.GetString("9065c7b6-9db5-46f4-a447-e4e0fad93cec", "Parameter '{0}' must be a positive integer but its value is '{1}'", "heightinrows", matchedGroups["ColumnsHeight"].Value));
				}
				if (columnsWidth < 1)
				{
					ReportMacroError(report, Res.GetString("9065c7b6-9db5-46f4-a447-e4e0fad93cec", "Parameter '{0}' must be a positive integer but its value is '{1}'", "widthincolumns", matchedGroups["ColumnsWidth"].Value));
				}

				return null;
			}

			SystemImage image;
			if (ImageValueProviders.ContainsKey(imageSource))
			{
				var imageValueDelegate = ImageValueProviders[imageSource];
				image = imageValueDelegate(report);
			}
			else
			{
				image = ImageFromColumnValue(imageSource, report);
			}

			if (image != null)
			{
				if (image.IsDisposed())
				{
					throw new DocumentEngineException("The image can't be disposed.");
				}
				else
				{
					var totalWidthInXls = GetTotalWidthInXls(report, columnsWidth);
					var imageName = imageSource.Replace(".", "");
					var isAspectRatioLocked = string.Equals(matchedGroups["IsAspectRatioLocked"].Value.Trim(), "Y", StringComparison.OrdinalIgnoreCase);

					return new ExcelImage((SystemImage)image.Clone(), columnsHeight, totalWidthInXls, imageName, isAspectRatioLocked);
				}
			}

			return null;
		}

		int GetTotalWidthInXls(Report report, int columnsWide)
		{
			var aggregatedWidth =
				report.Renderer.OriginalColumnWidths
					.Skip(report.Renderer.CurrentColumn)
					.Take(columnsWide)
					.Aggregate(new { TotalWidth = 0, Count = 0 }, (seed, columnWidth) => new { TotalWidth = seed.TotalWidth + columnWidth, Count = seed.Count + 1 });

			if (aggregatedWidth.Count < columnsWide && !report.ContainsAnyCustomisation)
			{
				ErrorReporter.ReportOnce("ImageMacroSpecifiesWidthBeyondAvailableColumns",
					string.Format(
@"<Image> macro specifies image width beyond available columns.
Report: {0}
Template: {1}
Available columns: {2}
Current column: {3}
Image width (in columns): {4}
Used width (in column): {5}",
					report.Name,
					report.Template != null ? report.Template.TemplateName : ZString.Empty,
					report.Renderer.OriginalColumnWidths.Count(),
					report.Renderer.CurrentColumn,
					columnsWide,
					aggregatedWidth.Count));
			}

			return aggregatedWidth.TotalWidth;
		}

		SystemImage CompanyLogoValueProvider(Report report)
		{
			SystemImage result = GetImageFromDataProvider("CompanyLogo", report) ?? SystemDataRegistry.Instance.CompanyLogo.Value;

			return result;
		}

		SystemImage HBLLogoValueProvider(Report report)
		{
			SystemImage result = GetImageFromDataProvider("HBLLogo", report) ?? Env.Registry.HouseBillOfLadingLogo;

			return result;
		}

		SystemImage HAWBLogoValueProvider(Report report)
		{
			return GetImageFromDataProvider("HAWBLogo", report) ?? Env.Registry.Freight.AirWaybill.HAWBLogo;
		}

		SystemImage ChequeLogoValueProvider(Report report)
		{
			return ObjectFactory.Get<IAccounting>().PrintLogoOnCheque ? SystemDataRegistry.Instance.CompanyLogo.Value : null;
		}

		SystemImage ChequeLogoWithBankDetailValueProvider(Report report)
		{
			return ObjectFactory.Get<IAccounting>().PrintLogoOnCheque ? SystemDataRegistry.Instance.CompanyCheckLogo.Value : null;
		}

		SystemImage UserSignatureValueProvider(Report report)
		{
			SystemImage result = GetImageFromDataProvider("UserSignature", report);

			if (result == null && GlbStaff.CurrentUser != null)
			{
				if (GlbStaff.CurrentUser.GS_UserSignature.Length > 0)
				{
					MemoryStream stream = new MemoryStream(GlbStaff.CurrentUser.GS_UserSignature);
					result = SystemImage.FromStream(stream);
				}
			}

			return result;
		}

		protected SystemImage GetImageFromDataProvider(string imageSource, Report report)
		{
			SystemImage result = null;
			try
			{
				using (report.ErrorManager.GetErrorCheckingSuspender())
				{
					object value = report.Renderer.CurrentAreaToProcess.GetColumnValue(report.Renderer.CurrentDataRow, imageSource);
					result = value as SystemImage;
				}
			}
			catch (DataProviderException) { }
			catch (DocumentEngineException) { }
			catch (FieldNotFoundException) { }

			return result;
		}

		protected SystemImage ImageFromColumnValue(string imageSource, Report report)
		{
			SystemImage result = null;
			object value = report.Renderer.CurrentAreaToProcess.GetColumnValue(report.Renderer.CurrentDataRow, imageSource);
			result = value as SystemImage;

			if (result == null && value != null)
			{
				throw new DocumentEngineException(string.Format("Field {0} is not of type System.Drawing.Image. It is of type {1}", imageSource, value.GetType().FullName));
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1017", Justification = "Not working in pixels")]
		Size IControlSizeProvider.GetCellRange(string macro)
		{
			var matchedGroups = Regex.Match(macro).Groups;
			var height = GetIntFromString(matchedGroups["ColumnsHeight"].Value);
			var width = GetIntFromString(matchedGroups["ColumnsWidth"].Value);

			return new Size(width, height);
		}

		int GetIntFromString(string valueFromMacro)
		{
			int result;
			if (!int.TryParse(valueFromMacro, out result))
			{
				result = -1;
			}

			return result;
		}

		#endregion

		public override VisualiserComponentTypes ComponentType
		{
			get { return VisualiserComponentTypes.Image; }
		}

		public override Passes PassToStartReplacingOn
		{
			get { return Passes.SecondPass; }
		}

		public override Regex Regex
		{
			get { return regex; }
		}

		static readonly Regex regex = new Regex(@"^<(?:[\s]*)Image(?:[\s]*)\((?:[\s]*)(?<ImageSource>[^,\s]+('.*?'[^,\s'""]+|"".*?""[^,\s'""]+)*)(?:[\s]*),(?:[\s]*)(?<ColumnsHeight>[^,\s]+)(?:[\s]*),(?:[\s]*)(?<ColumnsWidth>[^,\s]+)(?:[\s]*),?(?:[\s]*)(?<IsAspectRatioLocked>[^\s]?)(?:[\s]*)\)(?:[\s]*)>$",
			RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
