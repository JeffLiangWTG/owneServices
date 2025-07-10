using System.Windows.Forms;
using Enterprise.Client.EDI.ApplicationLogging.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.ApplicationLogging.GUI.Testing
{
	[TestedType(typeof(ApplicationLoggerForm))]
	internal class ApplicationLoggerFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var logger = Factory.New<ApplicationLogger>();
			logger.ALG_Product = "CargoWise";
			Factory.Save();
			return new ApplicationLoggerForm(logger);
		}
	}
}
