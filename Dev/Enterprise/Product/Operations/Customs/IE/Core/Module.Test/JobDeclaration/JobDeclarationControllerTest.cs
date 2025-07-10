using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Module.Testing
{
	[TestedType(typeof(JobDeclarationController))]
	class JobDeclarationControllerTest : EU.Module.Testing.JobDeclarationControllerTest
	{
		public override Type ControllerToBashType => typeof(JobDeclarationController);

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var result = Factory.New<JobDeclaration>();
			result.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._30;
			Factory.Save();

			return result;
		}
	}
}
