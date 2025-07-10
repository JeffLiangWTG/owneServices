using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ImportLicenseInfo))]
	public class ImportLicenseInfoTest : Customs.Business.Testing.CusSupportingInfoTest<ImportLicenseInfo>
	{
		public void TestSetDefaultValues()
		{
			var supporting = Factory.New<ImportLicenseInfo>();
			AssertEquals(CusSupportingInfoTypeList.Codes.ImportLicense, supporting.CSI_Type);
			AssertEquals(JobComInvoiceLineSchema.Constants.Prefix, supporting.CSI_ParentTableCode);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			return declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().ImportLicenseSupportingInfo;
		}

		protected override IEnumerable<ImportLicenseInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var importLicense = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().ImportLicenseSupportingInfo;
			importLicense.CSI_Code = "1";
			importLicense.CSI_ReferenceNumber = "123456";
			importLicense.CSI_DateOfIssue = ZDateTime.Now;

			yield return importLicense;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetBizObjsForCorrectlyTypeDecideTest(factory).FirstOrDefault();
		}
	}
}
