using CargoWise.Types;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using Bill = Enterprise.Customs.NZ.Business.Declaration.Bill;

namespace Enterprise.DocumentWrappers.Customs.NZ.Testing
{
	[TestedType(typeof(DocDeclaration))]
	sealed class DocDeclarationECIWriteoffTest : DocDeclarationTest
	{
		public override void TestTransportModeDescription()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("TransportModeDescription", "Air", DeclarationWrapper.TransportModeDescription);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("TransportModeDescription", "Sea", DeclarationWrapper.TransportModeDescription);
		}

		public void TestDeclarationInternalIsOfRightType()
		{
			AssertEquals(typeof(JobDeclaration), Declaration.GetType());
		}

		public void TestDeclarationEqualsDeclarationInternal()
		{
			AssertEquals(Declaration.PK, Declaration.PK);
		}

		public override void TestTotalInvoiceLineCustomAttrib2()
		{
			Assert("No InvoiceLines available. Test excluded from this declaration", true);
		}

		public override void TestSupplierInvoiceNumbers()
		{
			AssertEquals("ECI Writeoff has no SupplierInvoiceNumbers", ZString.Empty, DeclarationWrapper.SupplierInvoiceNumbers);
		}

		public override void TestCountryOfOrigin()
		{
			Assert("ECI-WriteOff doesnt have invoice lines at all. InvoiceLine.JobDeclaration is a different type", true);
		}

		public override void TestTotalInvoiceAmount()
		{
			Declaration.JE_ECI_InvoiceAmount = 2000m;
			Declaration.JE_ECI_InvoiceCurrency = (RefCurrency.LoadFromCurrencyCode(Factory, GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency)).PK;
			AssertEquals("ECI Writeoff Total Invoice Amount", 2000M, DeclarationWrapper.TotalInvoiceAmount);
			AssertEquals("ECI Writeoff Total Invoice currency", GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency, DeclarationWrapper.TotalInvoiceCurrency.Code);
		}

		public override void TestInvoiceLinesInternal()
		{
			Assert("ECI-WriteOff doesnt have invoice lines at all. InvoiceLine.JobDeclaration is a different type", true);
		}

		public override void TestInvoiceLinesSortedByLineNoInternal()
		{
			Assert("ECI-WriteOff doesnt have invoice lines at all. InvoiceLine.JobDeclaration is a different type", true);
		}

		public override void TestPortOfFirstArrival()
		{
			Assert("Port of first arrival is not used in NZ", true);
		}

		#region Implementation
		protected override JobDeclaration GetNewJobDeclaration()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageSubType = Enterprise.Customs.NZ.Business.JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			return declaration;
		}

		protected override void SetupPackages()
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, new CodeDescriptionPair("BA", "Barrel"), new CodeDescriptionPair("BK", "Basket"));

			Declaration.JE_TotalNoOfPacks = 25;

			Bill houseBill = Declaration.Bills.AddNew();
			PackingGroup packingGroup = Declaration.PackingGroups.AddNew();
			packingGroup.CR_CU_HouseBill = houseBill.PK;

			Package package1 = packingGroup.Packages.AddNew();
			package1.CW_ContainerNoOrEquipmentNo = "ABCD0123456";
			package1.CW_PackQty = 10;
			package1.CW_PackType = "BA";

			Package package2 = packingGroup.Packages.AddNew();
			package2.CW_ContainerNoOrEquipmentNo = "ABCD0123456";
			package2.CW_PackQty = 15;
			package2.CW_PackType = "BK";
		}

		protected override ZString GetExpectedPackagesInfoString()
		{
			return new ZString("25 PCS");
		}

		#endregion
	}
}
