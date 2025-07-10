using System;
using System.Drawing;
using System.Drawing.Imaging;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.GUI
{
	static class TicketBitmapHelper
	{
		internal static Image GetRenderedImage(TaskCardControl taskCard, ICardContent cardContent, BoardSectionViewModel sectionViewModel, bool isTemplateTaskCard)
		{
			return GetRenderedImage(() => TryRenderAsBitmap(taskCard, isTemplateTaskCard), cardContent, NormalBitmapDescription, sectionViewModel);
		}

		internal static Image GetDarkenedImage(Image sourceImage, ICardContent cardContent, BoardSectionViewModel sectionViewModel)
		{
			return GetRenderedImage(() =>
			{
				var bitmap = new Bitmap(sourceImage.Width, sourceImage.Height);

				bitmap.DisposeOnException(() =>
				{
					using (var graphics = Graphics.FromImage(bitmap))
					{
						const float colorDarkenFactor = 0.8f;

						var colorMatrix = new ColorMatrix
						{
							Matrix00 = colorDarkenFactor,
							Matrix11 = colorDarkenFactor,
							Matrix22 = colorDarkenFactor,
							Matrix44 = colorDarkenFactor,
						};

						var imageAttribs = new ImageAttributes();
						imageAttribs.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

						graphics.DrawImageWithExtendedErrorMessage(sourceImage, ControlDpiScalingHelper.NewScaledRectangle(0, 0, bitmap.Width, bitmap.Height, false), 0, 0, sourceImage.Width, sourceImage.Height, GraphicsUnit.Pixel, imageAttribs);
					}
				});

				return bitmap;
			}, cardContent, DarkenedBitmapDescription, sectionViewModel);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer diagnostic message")]
		const string NormalBitmapDescription = "normal";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer diagnostic message")]
		const string DarkenedBitmapDescription = "darkened";

		static Image GetRenderedImage(Func<Image> bitmapCreator, ICardContent cardContent, string bitmapBeingRendered, BoardSectionViewModel sectionViewModel)
		{
			Exception exceptionToReport = null;

			for (var attempt = 0; attempt < 3; attempt++)
			{
				try
				{
					return bitmapCreator();
				}
				catch (InvalidOperationException ex)
				{
					exceptionToReport = ex;
				}
			}

			ReportBitmapRenderingException(exceptionToReport, cardContent, bitmapBeingRendered, sectionViewModel);

			return null;
		}

		static Image TryRenderAsBitmap(TaskCardControl taskCard, bool isTemplateTaskCard)
		{
			var imageBuffer = new Bitmap(taskCard.Width, taskCard.Height, PixelFormat.Format32bppArgb);

			imageBuffer.DisposeOnException(() =>
			{
				if (isTemplateTaskCard)
				{
#if !WINZOR
					// When the TaskCardControl is used as a template, children components are already ordered in the reverse order (see CardRenderTemplate) to fix the known problem with rendering controls using DrawToBitmap() so there is no need to call DrawToBitmapFixed() instead
					// https://stackoverflow.com/questions/10096195/drawtobitmap-not-taking-screenshots-of-all-items
					taskCard.DrawToBitmap(imageBuffer, ControlDpiScalingHelper.NewScaledRectangle(0, 0, taskCard.Width, taskCard.Height, false));
#endif
				}
				else
				{
					taskCard.DrawToBitmapFixed(imageBuffer, ControlDpiScalingHelper.NewScaledRectangle(0, 0, taskCard.Width, taskCard.Height, false));
				}
			});

			return imageBuffer;
		}

		static void ReportBitmapRenderingException(Exception exceptionToReport, ICardContent cardContent, string bitmapBeingRendered, BoardSectionViewModel sectionViewModel)
		{
			try
			{
				var factory = sectionViewModel.FactoryProvider.GetNewBackgroundThreadLoaderFactory(nameof(GetRenderedImage));
				var task = cardContent.GetTask(factory);
				var workflow = cardContent.GetWorkflow(factory);

				var ticketInfo = FormattableString.Invariant($"Task: [{task?.GetDiagnosticLogInfo()}], Workflow: [{workflow?.Description}]"); // Developer diagnostic message
				var sectionInfo = FormattableString.Invariant($"Section: [{sectionViewModel.SectionName}], Board: [{sectionViewModel.BoardViewModel.BoardName}]"); // Developer diagnostic message

				ErrorReporter.ReportOnce(FormattableString.Invariant($"Exception when rendering {bitmapBeingRendered} bitmap for ticket {ticketInfo} on board section {sectionInfo}"), exceptionToReport);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				// We don't want to report exceptions from reporting exceptions.
			}
		}
	}
}
