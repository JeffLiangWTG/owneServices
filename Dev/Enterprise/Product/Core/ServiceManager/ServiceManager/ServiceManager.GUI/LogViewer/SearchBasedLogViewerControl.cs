using System;
using System.ComponentModel;
using System.Drawing;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ServiceManager.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class SearchBasedLogViewerControl : ZUserControl, ILogViewerControl
	{
		public SearchBasedLogViewerControl()
		{
			InitializeComponent();
			InitialiseSearchFilterApplyButtonImage();
		}

		SearchBasedLogViewer SearchBasedLogViewer
		{
			get { return (SearchBasedLogViewer)BindingSource.Current; }
		}

		public void RefreshLogs()
		{
			SearchBasedLogViewer.RunPreSaveValidation();

			if (SearchBasedLogViewer.HasErrors)
			{
				return;
			}
			if (SearchBasedLogViewer.InvalidElasticSearchConfiguration)
			{
				Globals.Message.ShowError("Elasticsearch logging configuration is invalid, check the registry.");
				return;
			}
			if (SearchBasedLogViewer.InvalidKafkaConfiguration)
			{
				Globals.Message.ShowError("Kafka logging configuration is invalid, check the registry.");
				return;
			}
			SearchBasedLogViewer.ReloadEvents();
		}

		void eventGrid_ColourDeciding(object sender, ZArchitecture.ColourDecidingEventArgs e)
		{
			var eventRecord = (EventRecord)e.ObjectAtRow;
			if (eventRecord.Type == "Error")
			{
				e.Colour = Color.Tomato;
			}
			else if (eventRecord.Type == "Warning")
			{
				e.Colour = Color.Yellow;
			}
		}

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<SearchBasedLogViewerControl>().Result;
		}

		void searchFilterApplyButton_Click(object sender, EventArgs e) => RefreshLogs();

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			if (SearchBasedLogViewer.InvalidElasticSearchConfiguration)
			{
				Globals.Message.ShowError("Elasticsearch logging configuration is invalid, check the registry.");
			}
			else if (SearchBasedLogViewer.InvalidKafkaConfiguration)
			{
				Globals.Message.ShowError("Kafka logging configuration is invalid, check the registry.");
			}
		}

		void InitialiseSearchFilterApplyButtonImage()
		{
			var originalImage = Icons.GetImage(IconTypes.FindButtonActive);
			var targetSize = searchFilterApplyButton.Size;
			var scaleFactor = Math.Min((double)targetSize.Width / originalImage.Width,
										(double)targetSize.Height / originalImage.Height);
			var newWidth = (int)(originalImage.Width * scaleFactor * 0.5);
			var newHeight = (int)(originalImage.Height * scaleFactor * 0.5);
			var thumbnail = originalImage.GetThumbnailImage(newWidth, newHeight, () => false, IntPtr.Zero);
			var paddedImage = new Bitmap(newWidth + 5, newHeight);
			using (var graphics = Graphics.FromImage(paddedImage))
			{
				graphics.Clear(Color.Transparent);
				graphics.DrawImage(thumbnail, 5, 0, newWidth, newHeight);
			}
			searchFilterApplyButton.Image = paddedImage;
		}
	}
}
