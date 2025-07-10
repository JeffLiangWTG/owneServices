using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Client.EDI.ApplicationLogging.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.ApplicationLogging.GUI.Testing
{
	[TestedType(typeof(ApplicationActiveLoggerForm))]
	internal class ApplicationActiveLoggerFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var logger = Factory.NewWithValidTestData<ApplicationLogger>();
			logger.ALG_Product = "CargoWise";
			logger.ALG_Name = "Name";

			var activeLogger = Factory.New<ApplicationActiveLogger>();
			activeLogger.AAL_Environment = Guid.NewGuid().ToString();
			activeLogger.AAL_ActiveUntil = ZDateTimeOffset.Now;
			activeLogger.AAL_ALG_ApplicationLogger = logger.PK;
			Factory.Save();
			return new ApplicationActiveLoggerForm(activeLogger);
		}
	}
}
