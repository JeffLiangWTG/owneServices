using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ImportLicenseLoadingObject))]
	class ImportLicenseLoadingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestReadOnly()
		{
			var loadingObject = GetNewBusinessObject() as ImportLicenseLoadingObject;
			CombineAssertions(() =>
			{
				Assert("ImportLicenseNo must be ReadOnly", loadingObject.ImportLicenseNoInfo.ReadOnly);
				Assert("RegistrationDate must be ReadOnly", loadingObject.RegistrationDateInfo.ReadOnly);
				Assert("InvoiceNo must NOT be ReadOnly", !loadingObject.InvoiceHeaderPKInfo.ReadOnly);
				Assert("Incoterm must be ReadOnly", loadingObject.IncotermInfo.ReadOnly);
				Assert("Currency must be ReadOnly", loadingObject.CurrencyInfo.ReadOnly);
				Assert("VMLE must be ReadOnly", loadingObject.VMLEInfo.ReadOnly);
				Assert("VMCV must be ReadOnly", loadingObject.VMCVInfo.ReadOnly);
				Assert("NetWeight must be ReadOnly", loadingObject.NetWeightInfo.ReadOnly);
				Assert("UQ must be ReadOnly", loadingObject.UQInfo.ReadOnly);
				Assert("ImportLicenseType must NOT be ReadOnly", !loadingObject.ImportLicenseTypeInfo.ReadOnly);
				Assert("ImportLicenseAuthorizationDate must NOT be ReadOnly", !loadingObject.ImportLicenseAuthorizationDateInfo.ReadOnly);
				Assert("ImportLicenseFeeType must NOT be ReadOnly", !loadingObject.ImportLicenseFeeTypeInfo.ReadOnly);
			});
		}

		#region Implemantation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ImportLicenseLoadingObject(Parent);
		}

		ImportLicenseLoadingObjectParent Parent => fParent ?? (fParent = new ImportLicenseLoadingObjectParent(Factory.New<JobDeclaration>()));
		ImportLicenseLoadingObjectParent fParent;

		#endregion
	}
}
