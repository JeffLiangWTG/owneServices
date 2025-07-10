using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.IO;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	[TestedType(typeof(XLSPreviewForm))]
	sealed class XLSPreviewFormBasher : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var resourceRetriever = new EmbeddedResourceRetriever(typeof(PrintTaskTest).Assembly);
			var resourceStream = resourceRetriever.GetStream("Enterprise.DocumentEngine.Test.FlexCelInterface.Testing.OnePageSlowPreview.xls");
			var form = new XLSPreviewForm(resourceStream, new DeliveryInfo[] { new DeliveryInfo(DeliveryInfo.DeliveryFormats.Report) }, null);

			TypeDescriptor.AddAttributes(form.ZoomPresetButton, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(form.GoToFirstPageButton, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(form.GoToLastPageButton, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(form.GoToNextPageButton, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(form.GoToPrevPageButton, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(form.HidThumbsButton, new SuppressFormsLocalizedTestAttribute());

			return form;
		}
	}
}
