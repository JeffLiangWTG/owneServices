using System;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using FlexCel.Core;
using FlexCel.Winforms;
using FlexCel.XlsAdapter;

namespace Enterprise.DocumentEngine.GUI
{
	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis] // Has no Resx as the design work is not supposed to be done in this class.
	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	class FlexCelPreviewWithCulture : FlexCelPreview
	{
		protected override void OnLayout(LayoutEventArgs e)
		{
			AutoScaleMode = ControlDpiScalingHelper.DpiScaleMode;
			AutoScaleDimensions = ControlDpiScalingHelper.DpiScaleDimensions;
			base.OnLayout(e);
		}

		internal void InternalOnPaint(PaintEventArgs pe)
		{
			OnPaint(pe);
		}

		protected override void OnPaint(PaintEventArgs pe)
		{
			CultureInfo currentCulture = Culture.Current;
			if (IsLocalDocument)
			{
				Culture.Set(Culture.CurrentCompanyCountryCulture);
			}

			try
			{
#if DEBUG
				if (Globals.IsTest && GenerateFlexCelImageException)
				{
					throw new FlexCelCoreException("Error in Flexcel when creating an image", FlxErr.ErrCreatingImage);
				}
				else if (Globals.IsTest && GenerateFlexCelFontException)
				{
					throw new FlexCelCoreException("Error in Flexcel when creating an font", FlxErr.ErrFontNotSupported);
				}
#endif
				base.OnPaint(pe);
			}
			catch (FlexCel.Core.FlexCelCoreException exception)
			{
				if (exception.ErrorCode == FlxErr.ErrCreatingImage)
				{
					string errorMessage = Res.GetString("06431445-f0a7-4444-af2a-30526ecd1fcd", @"{0} caught when loading the Preview Control - 

Your system might be low on memory or system resources, please close all other tasks, exit {1}, come back in and try again.

Error message is: {2}",
						"FlexCelCoreException",
						Core.Constants.ProductName,
						exception.Message);
					Globals.Message.ShowError(errorMessage, Res.GetString("96d5f291-787e-4a9a-99af-7f95b0e42693", "Error"));
				}
				else if (exception.ErrorCode == FlxErr.ErrFontNotSupported)
				{
					string errorMessage = Res.GetString("A673DAFF-2171-48E0-88C1-3AF61217A7B2", @"{0} caught when loading the Preview Control - 

Your system may have some problems with a font needed for rendering the document, please fix or uninstall this font before trying to run this document again.

Error message is: {1}", "FlexCelCoreException", exception.Message);
					Globals.Message.ShowError(errorMessage, Res.GetString("6f11ff45-9384-455a-86fa-92646636f9b9", "Error Rendering Font"));
				}
				else
				{
					throw;
				}
			}
			catch (OutOfMemoryException) //GDI+ generic error
			{
				string errorMessage = Res.GetString("db13924c-9bf1-49cf-8c9f-5f84d2924477", @"Exception caught when loading the Preview Control - 

Either there is an image that GDI+ (windows framework for displaying image) does not know the format of (because it is malformed or corrupt) or your system is low on memory/resources.
If problems persist, please contact your system administrator and show them this information:

{0}", TopLevelExceptionHandler.ResourcesMessage());
				Globals.Message.ShowError(errorMessage, Res.GetString("96d5f291-787e-4a9a-99af-7f95b0e42693", "Error"));
			}
			catch (InvalidOperationException) //GDI+ generic error
			{
				string errorMessage = Res.GetString("c657d648-63c9-424a-9f0d-3c10d93e98d5", @"Exception caught when loading the Preview Control - 

A transient error has occurred in GDI+ (windows framework for displaying image) or your system is low on memory/resources.
If problems persist, please contact your system administrator and show them this information:

{0}", TopLevelExceptionHandler.ResourcesMessage());
				Globals.Message.ShowError(errorMessage, Res.GetString("96d5f291-787e-4a9a-99af-7f95b0e42693", "Error"));
			}
			catch (ArgumentException) //GDI+ error, happens in System.Drawing.Bitmap..ctor(Int32 width, Int32 height, PixelFormat format) 
			{
				string errorMessage = Res.GetString("44b94a30-8a88-4c90-a2f4-1bfa0490c3f5", @"Exception caught when loading the Preview Control - 

A likely cause is that an image was attempted to be made with a far too large size, or GDI+ (windows framework for displaying image) is in an invalid or corrupt state.
If problems persist, please contact your system administrator and show them this information:

{0}", TopLevelExceptionHandler.ResourcesMessage());
				Globals.Message.ShowError(errorMessage, Res.GetString("96d5f291-787e-4a9a-99af-7f95b0e42693", "Error"));
			}
			catch (ExternalException exception) when (exception.IsGdiPlusException())
			{
				exception.ReportWithInvalidCharacters(Document.Workbook as XlsFile);
				throw;
			}
			finally
			{
				if (IsLocalDocument)
				{
					Culture.Set(currentCulture);
				}
			}
		}

		internal bool IsLocalDocument { get; set; }

		#region Test
#if DEBUG
		internal bool GenerateFlexCelImageException;

		internal bool GenerateFlexCelFontException;

#endif
		#endregion
	}
}
