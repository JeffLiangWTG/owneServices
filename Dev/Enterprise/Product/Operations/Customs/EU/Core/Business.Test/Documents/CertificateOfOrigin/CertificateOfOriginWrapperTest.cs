using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin.Testing
{
	public abstract class CertificateOfOriginWrapperTest : TestCaseWithFactory
	{
		public void TestJobDeclarationConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("When declaration is null", () => new EURCertificateOfOriginWrapper(declaration: null));
			AssertNoExceptionThrown("When declaration is not null", () => new EURCertificateOfOriginWrapper(Factory.New<JobDeclaration>()));
		}

		public void TestCusEntryHeaderConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("When entryHeader is null", () => new EURCertificateOfOriginWrapper(entryHeader: null));
			AssertExceptionThrown<ArgumentNullException>("When entryHeader parent declaration is null", () => new EURCertificateOfOriginWrapper(Factory.New<CusEntryHeader>()));
			AssertNoExceptionThrown("When entryHeader parent declaration is not null", () => new EURCertificateOfOriginWrapper(Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew()));
		}

		[ExpectNoExceptions]
		public void TestRemarks()
		{
			var declaration = Factory.New<JobDeclaration>();
			var wrapper = GetNewWrapperFromDeclaration(declaration);
			NUnit.Framework.Assert.That(wrapper.Remarks, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Remarks always empty");
		}

		[ExpectNoExceptions]
		public void TestDeclarationReference()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "XYZ";
			var wrapper = GetNewWrapperFromDeclaration(declaration);
			NUnit.Framework.Assert.That(wrapper.DeclarationReference, NUnit.Framework.Is.EqualTo("XYZ").Using(CustomComparers.TypeComparison), "DeclarationReference");
		}

		[ExpectNoExceptions]
		public void TestReferenceDateFormat()
		{
			var declaration = Factory.New<JobDeclaration>();
			var wrapper = GetNewWrapperFromDeclaration(declaration);
			NUnit.Framework.Assert.That(wrapper.ReferenceDateFormat, NUnit.Framework.Is.EqualTo("dd/MM/yyyy").Using(CustomComparers.TypeComparison), "ReferenceDateFormat");
		}

		[ExpectNoExceptions]
		public void TestSupplierAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			var wrapper = GetNewWrapperFromDeclaration(declaration);
			NUnit.Framework.Assert.That(wrapper.SupplierAddress, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "SupplierAddress");

			var supplier = Factory.New<OrgHeader>();
			declaration.JE_OH_Supplier = supplier.PK;
			supplier.OH_FullName = "Freds Supply Co";
			supplier.MainAddress.OA_Address1 = "367 George St";
			supplier.MainAddress.OA_City = "Sydney";
			supplier.MainAddress.OA_State = "NSW";
			supplier.MainAddress.OA_PostCode = "2000";
			supplier.MainAddress.OA_RN_NKCountryCode = "AU";
			wrapper = GetNewWrapperFromDeclaration(declaration);
			NUnit.Framework.Assert.That(wrapper.SupplierAddress, NUnit.Framework.Is.EqualTo("FREDS SUPPLY CO\n367 GEORGE ST\nSYDNEY NSW 2000\nAUSTRALIA").Using(CustomComparers.TypeComparison), "SupplierAddress");
		}

		[ExpectNoExceptions]
		public void TestImporterAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			var wrapper = GetNewWrapperFromDeclaration(declaration);
			NUnit.Framework.Assert.That(wrapper.SupplierAddress, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "ImporterAddress");

			var importer = Factory.New<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			importer.OH_FullName = "Freds Supply Co";
			importer.MainAddress.OA_Address1 = "367 George St";
			importer.MainAddress.OA_City = "Sydney";
			importer.MainAddress.OA_State = "NSW";
			importer.MainAddress.OA_PostCode = "2000";
			importer.MainAddress.OA_RN_NKCountryCode = "AU";
			wrapper = GetNewWrapperFromDeclaration(declaration);
			NUnit.Framework.Assert.That(wrapper.ImporterAddress, NUnit.Framework.Is.EqualTo("FREDS SUPPLY CO\n367 GEORGE ST\nSYDNEY NSW 2000\nAUSTRALIA").Using(CustomComparers.TypeComparison), "ImporterAddress");
		}

		protected IEURCertificateOfOrigin GetNewWrapperFromDeclaration(JobDeclaration declaration) => new EURCertificateOfOriginWrapper(declaration);
	}
}
