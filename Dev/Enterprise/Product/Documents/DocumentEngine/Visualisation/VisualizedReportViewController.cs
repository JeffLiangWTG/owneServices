using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.Visualisation
{
	public sealed class VisualizedReportViewController
	{
		public VisualizedReportViewController(
			Report report,
			VisualiserDataSet boundDataSet)
		{
			this.report = report;
			this.boundDataSet = boundDataSet;
		}

		IVisualizedReportView view;
		public IVisualizedReportView View
		{
			get => view;
			set
			{
				view = value;
				if (view != null)
				{
					view.Text = Report.GetLocalizedSheetName(report.Name);
					view.FirstShown += new EventHandler(HandleViewFirstShown);
				}
			}
		}
		readonly Report report;
		readonly VisualiserDataSet boundDataSet;

		TemplateToVisualiserComponentsConverter converter;
		public TemplateToVisualiserComponentsConverter Converter
		{
			get { return converter ?? (converter = new TemplateToVisualiserComponentsConverter(report, boundDataSet)); }
		}

		readonly List<Type> exceptionTypesToHandle = new List<Type>(new Type[]
		{
			typeof(DataProviderException),
			typeof(InvalidGroupByColumnException),
			typeof(FormulaProviderException)
		});

		public bool AllowVisualizeReportRegardlessOfWarnings()
		{
			var allowVisualize = true;
			HandleLoadComponentsError(() =>
			{
				var components = Converter.Components;
				if (report.ErrorManager.HasErrors)
				{
					using (ObjectFactory.New<INeedToShowMessage>().SuppressNewFormInTransactionWarning())
					{
						allowVisualize = report.PrintTaskUIProvider.ShowErrors(report);
					}
				}
			});
			return allowVisualize;
		}

		void HandleViewFirstShown(object sender, EventArgs e)
		{
			view.ClientSize = GetClientSize();

			HandleLoadComponentsError(() =>
			{
				var components = Converter.Components;

				view.ControlDrawer.Draw(components);
				view.BorderDrawer.Draw(components);
			});
		}

		void HandleLoadComponentsError(Action action)
		{
			try
			{
				action();
			}
			catch (Exception ex)
			{
				if (IsExceptionTypeHandled(ex))
				{
					var errorMessage = new StringBuilder(Res.GetString("16f031b7-acbc-43ad-8d1c-5e80183b27a3", "Could not render the document due to an error in the template:"));

					if (ex is InvalidGroupByColumnException)
					{
						errorMessage.Append(Res.GetString("ad1f88de-e348-41b1-8ba6-594848ca7e08", "{0}{1} column '{2}' is invalid. Please verify the {1} areas and/or {3} macros in your template.", System.Environment.NewLine, "GroupBy", (ex as InvalidGroupByColumnException).InvalidColumnName, "GroupCount"));
					}
					else if (!report.ContainsAnyCustomisation && !report.Parent.IsRunFromMenusCustomisationForm)
					{
						ErrorReporter.ReportOnce("VisualizedReportViewController.HandleViewFirstShown:" + report.ToString(), ex.Message, ex);
					}

					errorMessage.Append(string.Format("{0}{1}", System.Environment.NewLine, ex.Message));
					Globals.Message.Show(errorMessage.ToString());
				}
				else
				{
					throw;
				}
			}
		}

		bool IsExceptionTypeHandled(Exception exception)
		{
			Type exceptionType = exception.GetType();
			foreach (Type typeToBeHandled in exceptionTypesToHandle)
			{
				if (typeToBeHandled.IsAssignableFrom(exceptionType))
				{
					return true;
				}
			}
			return false;
		}

		Size GetClientSize()
		{
			return report.PageStyle == PageStyles.Portrait ?
					ControlDpiScalingHelper.NewScaledSize(VisualizerPageSizes.PortraitPageWidth, VisualizerPageSizes.PortraitPageHeight) :
					ControlDpiScalingHelper.NewScaledSize(VisualizerPageSizes.LandscapePageWidth, VisualizerPageSizes.LandscapePageHeight);
		}
	}
}
