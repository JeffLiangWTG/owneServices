using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AQISPackageValidationTest : TestCaseWithFactory
	{
		public void TestCheckNumberAndType()
		{
			AQISPackage package = new AQISPackage(Factory);
			AssertNoErrors("Type", package.TypeInfo);
			AssertNoErrors("Number", package.NumberInfo);

			package.Number = 1;
			package.Validation.ValidateType();
			AssertHasErrors("Type", package.TypeInfo);
			AssertNoErrors("Number", package.NumberInfo);

			package.Number = ZInt.Zero;
			package.Validation.ValidateType();
			AssertNoErrors("Type", package.TypeInfo);
			AssertNoErrors("Number", package.NumberInfo);

			package.Number = -1;
			package.Validation.ValidateType();
			AssertNoErrors("Type", package.TypeInfo);
			AssertHasErrors("Number", package.NumberInfo);

			package.Number = ZInt.Zero;
			package.Validation.ValidateType();
			AssertNoErrors("Type", package.TypeInfo);
			AssertNoErrors("Number", package.NumberInfo);

			package.Type = "CTN";
			package.Validation.ValidateNumber();
			AssertNoErrors("Type", package.TypeInfo);
			AssertHasErrors("Number", package.NumberInfo);

			package.Type = ZString.Empty;
			package.Validation.ValidateNumber();
			AssertNoErrors("Type", package.TypeInfo);
			AssertNoErrors("Number", package.NumberInfo);

			package.Number = 1;
			package.Type = "CTN";
			AssertNoErrors("Type", package.TypeInfo);
			AssertNoErrors("Number", package.NumberInfo);
		}

		public void TestNoPackageTypeIsRepeated()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AQISPackage package1 = invoiceLine.AQISPackages.AddNew();
			AssertNoErrors("No errors", package1.TypeInfo);

			package1.Number = 1;
			package1.Type = "KG";

			AQISPackage package2 = invoiceLine.AQISPackages.AddNew();
			package2.Number = 1;
			package2.Type = "KG";
			AssertHasErrors("Errors", package2.TypeInfo);

			package2.Number = ZInt.Zero;
			package2.Type = ZString.Empty;
			AssertNoErrors("No errors", package2.TypeInfo);
		}

		public void TestNumberOfPackagesEntered()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AQISPackage package1 = invoiceLine.AQISPackages.AddNew();
			package1.Number = 123;
			package1.Type = "One";

			AQISPackage package2 = invoiceLine.AQISPackages.AddNew();
			package2.Number = 456;
			package2.Type = "Two";

			AQISPackage package3 = invoiceLine.AQISPackages.AddNew();
			package3.Number = 789;
			package3.Type = "Thr";

			AQISPackage package4 = invoiceLine.AQISPackages.AddNew();
			package2.Number = 321;
			package2.Type = "Four";

			AQISPackage package5 = invoiceLine.AQISPackages.AddNew();
			package5.Number = 654;
			package5.Type = "Five";

			AQISPackage package6 = invoiceLine.AQISPackages.AddNew();
			package6.Number = 987;
			package6.Type = "Six";

			AQISPackage package7 = invoiceLine.AQISPackages.AddNew();
			package7.Number = 987;
			package7.Type = "Sev";

			AQISPackage package8 = invoiceLine.AQISPackages.AddNew();
			package8.Number = 987;
			package8.Type = "Eig";

			AQISPackage package9 = invoiceLine.AQISPackages.AddNew();
			package9.Number = 987;
			package9.Type = "Nine";

			AQISPackage package10 = invoiceLine.AQISPackages.AddNew();
			package10.Number = 987;
			package10.Type = "Ten";

			package10.Validation.ValidateAll();

			AssertEquals("Package 1 has no errors", false, package1.HasRowErrors);
			AssertEquals("Package 2 has no errors", false, package2.HasRowErrors);
			AssertEquals("Package 3 has no errors", false, package3.HasRowErrors);
			AssertEquals("Package 4 has no errors", false, package4.HasRowErrors);
			AssertEquals("Package 5 has no errors", false, package5.HasRowErrors);
			AssertEquals("Package 6 has no errors", false, package6.HasRowErrors);
			AssertEquals("Package 7 has no errors", false, package7.HasRowErrors);
			AssertEquals("Package 8 has no errors", false, package8.HasRowErrors);
			AssertEquals("Package 9 has no errors", false, package9.HasRowErrors);
			AssertEquals("Package 10 has no errors", false, package10.HasRowErrors);

			AQISPackage package11 = invoiceLine.AQISPackages.AddNew();
			package11.Number = 987;
			package11.Type = "Ele";
			Factory.Save();

			package11.Validation.ValidateAll();
			AssertEquals("Package 1 has no errors", false, package1.HasRowErrors);
			AssertEquals("Package 2 has no errors", false, package2.HasRowErrors);
			AssertEquals("Package 3 has no errors", false, package3.HasRowErrors);
			AssertEquals("Package 4 has no errors", false, package4.HasRowErrors);
			AssertEquals("Package 5 has no errors", false, package5.HasRowErrors);
			AssertEquals("Package 6 has no errors", false, package6.HasRowErrors);
			AssertEquals("Package 7 has no errors", false, package7.HasRowErrors);
			AssertEquals("Package 8 has no errors", false, package8.HasRowErrors);
			AssertEquals("Package 9 has no errors", false, package9.HasRowErrors);
			AssertEquals("Package 10 has no errors", false, package10.HasRowErrors);
			AssertEquals("Package 11 has errors", true, package11.HasRowErrors);

			invoiceLine.AQISPackages.RemoveAndDelete(package2);
			package11.Validation.ValidateAll();
			AssertEquals("Package 1 has no errors", false, package1.HasRowErrors);
			AssertEquals("Package 3 has no errors", false, package3.HasRowErrors);
			AssertEquals("Package 4 has no errors", false, package4.HasRowErrors);
			AssertEquals("Package 5 has no errors", false, package5.HasRowErrors);
			AssertEquals("Package 6 has no errors", false, package6.HasRowErrors);
			AssertEquals("Package 7 has no errors", false, package7.HasRowErrors);
			AssertEquals("Package 8 has no errors", false, package8.HasRowErrors);
			AssertEquals("Package 9 has no errors", false, package9.HasRowErrors);
			AssertEquals("Package 10 has no errors", false, package10.HasRowErrors);
			AssertEquals("Package 11 has no errors", false, package11.HasRowErrors);
		}
	}
}
