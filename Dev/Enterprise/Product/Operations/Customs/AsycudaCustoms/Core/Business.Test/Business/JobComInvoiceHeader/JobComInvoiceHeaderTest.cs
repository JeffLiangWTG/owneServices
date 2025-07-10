using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(JobComInvoiceHeader))]
	sealed class JobComInvoiceHeaderTest : Customs.Business.Testing.BaseJobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
	{
		protected override List<string> CountryCodesForIsReciprocalRatesTest => new List<string> { Core.Constants.CountryCodes.Botswana, Core.Constants.CountryCodes.Lesotho, Core.Constants.CountryCodes.Namibia, Core.Constants.CountryCodes.Swaziland };

		public void TestTypeDecider()
		{
			Assert("Update BaseJobComInvoiceHeaderTypeDecider to include a decider for this class", Factory.New(typeof(BaseJobComInvoiceHeader)).GetType() == GetExpectedBusinessObjectType());
		}

		public void TestCusSupportingInfo()
		{
			var supportingDocumentsProvider = (ISupportingDocumentsProvider)Factory.NewWithValidTestData<JobDeclaration>().Invoices.AddNew();
			var supportingDocuments = supportingDocumentsProvider.SupportingDocuments;
			var supportingDocument1 = supportingDocuments.AddNew();
			Assert("CSI_Type=SUP & CSI_ParentID=inv.PK", supportingDocument1.CSI_Type == "SUP" && supportingDocument1.CSI_ParentID == supportingDocumentsProvider.PK);
			supportingDocument1.CSI_Code = "CD1";
			supportingDocument1.CSI_ReferenceNumber = "REF001";
			supportingDocument1.CSI_AdditionalDescription = "Additional Description";
			Factory.Save();
			var reloadedObject = new BusinessObjectFactory().LoadTop1<CusSupportingInfo>(new ZQuery(CusSupportingInfoSchema.PK, supportingDocument1.PK));
			AssertEquals("CSI_Type", "SUP", reloadedObject.CSI_Type);
			AssertEquals("CSI_ParentID", supportingDocumentsProvider.PK, reloadedObject.CSI_ParentID);
			AssertEquals("CSI_ReferenceNumber", "REF001", reloadedObject.CSI_ReferenceNumber);
			AssertEquals("CSI_AdditionalDescription", "Additional Description", reloadedObject.CSI_AdditionalDescription);
		}

		protected override Type ExpectedTypeOfGroupCharges => typeof(JobComInvApportionedChargeCollection<InvoiceApportionCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceCharge>);
	}

	class JobComInvoiceHeaderTestForDocumentWrapper : Customs.Business.Testing.BaseJobComInvoiceHeaderTestForDocumentWrapper
	{
		protected override BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();
	}
}
