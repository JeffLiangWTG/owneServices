using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(JobDeclaration.B2bDeclarationInvoicingSupporter))]
	sealed class B2DeclarationInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			var result = Factory.NewWithValidTestData<JobDeclaration>();
			result.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			return result;
		}

		protected override ZString TestingCountry
		{
			get { return Core.Constants.CountryCodes.Canada; }
		}
	}
}
