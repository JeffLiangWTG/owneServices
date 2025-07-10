using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(JobDeclaration.JobDeclarationInvoicingSupporter))]
	class JobDeclarationInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		public void TestOverridenDepartmentImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var expectedDepartmentPK = CargoWise.Application.ObjectFactory.Get<Integration.Accounting.IAccounting>().CustomsImportOther;
			CombineAssertions(() =>
			{
				foreach (var transportMode in new TransportTypeList().GetAllCodes())
				{
					declaration.JE_TransportMode = transportMode;
					switch (transportMode)
					{
						case TransportTypeList.Codes.PassengerCarried:
							AssertEquals("Handled by Enterprise.Accounting.Business.JobInvoicing.DepartmentChooser", expectedDepartmentPK, ((IJobInvoicingPlugIn)declaration).InvoicingSupporter.OverriddenDepartmentPK);
							break;
						default:
							AssertEquals($"TransportMode: {transportMode}", ZGuid.Empty, ((IJobInvoicingPlugIn)declaration).InvoicingSupporter.OverriddenDepartmentPK);
							break;
					}
				}
			}

			);
		}

		public void TestOverridenDepartmentExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var expectedDepartmentPK = CargoWise.Application.ObjectFactory.Get<Integration.Accounting.IAccounting>().CustomsOther;
			CombineAssertions(() =>
			{
				foreach (var transportMode in new TransportTypeList().GetAllCodes())
				{
					declaration.JE_TransportMode = transportMode;
					switch (transportMode)
					{
						case TransportTypeList.Codes.PassengerCarried:
							AssertEquals("Handled by Enterprise.Accounting.Business.JobInvoicing.DepartmentChooser", expectedDepartmentPK, ((IJobInvoicingPlugIn)declaration).InvoicingSupporter.OverriddenDepartmentPK);
							break;
						default:
							AssertEquals($"TransportMode: {transportMode}", ZGuid.Empty, ((IJobInvoicingPlugIn)declaration).InvoicingSupporter.OverriddenDepartmentPK);
							break;
					}
				}
			}

			);
		}

		protected override IJobInvoicingPlugIn GetNewBusinessObject() => Factory.NewWithValidTestData<JobDeclaration>();

		protected override ZString TestingCountry => Core.Constants.CountryCodes.China;
	}
}
