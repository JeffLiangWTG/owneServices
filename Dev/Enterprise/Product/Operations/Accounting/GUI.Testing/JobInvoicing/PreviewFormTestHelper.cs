using System.Linq;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.GUI;

namespace Enterprise.Accounting.GUI.Testing.JobInvoicing
{
	public static class PreviewFormTestHelper
	{
		public static bool CloseOpenedForms()
		{
			bool result = false;
			foreach (var previewForm in ZApplication.GetOpenForms().OfType<XLSPreviewForm>())
			{
				result = true;
				previewForm.Close();
			}

			return result;
		}
	}
}
