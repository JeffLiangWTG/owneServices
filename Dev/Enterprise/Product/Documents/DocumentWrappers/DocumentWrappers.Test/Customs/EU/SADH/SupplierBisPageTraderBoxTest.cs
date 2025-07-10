using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Customs.EU.Testing
{
	[Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Core.Constants.CountryCodes.Latvia)]
	sealed class SupplierBisPageTraderBoxTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("When docSADH is null", () => new SupplierBisPageTraderBox(null));
		}

		public void TestCaption()
		{
			AssertEquals("Caption", "2 Consignor/Exporter", bisPageTraderBox.Caption);
		}

		public void TestID()
		{
			AssertEquals("ID", "", bisPageTraderBox.ID);

			var supplier = Factory.New<OrgHeader>();
			supplier.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "LVXYZ", RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Latvia));
			entryHeader.Declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("ID", "LVXYZ", bisPageTraderBox.ID);
		}

		public void TestContent()
		{
			AssertEquals("Content", "", bisPageTraderBox.Content);

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_RL_NKClosestPort = "ITMIL";
			supplier.OH_FullName = "FULL NAME";
			entryHeader.Declaration.JE_OH_Supplier = supplier.PK;
			docSADHForTest.ShowBox2SupplierCountryCodeExposed = false;
			AssertEquals("Content", "FULL NAME", bisPageTraderBox.Content);

			docSADHForTest.ShowBox2SupplierCountryCodeExposed = true;
			AssertEquals("Content", "FULL NAME\r\nItaly", bisPageTraderBox.Content);
		}

		protected override void SetUp()
		{
			base.SetUp();
			entryHeader = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			docSADHForTest = DocSADHForTest.New(entryHeader, Factory);
			bisPageTraderBox = new SupplierBisPageTraderBox(docSADHForTest);
		}

		CusEntryHeader entryHeader;
		IBisPageTraderBox bisPageTraderBox;
		DocSADHForTest docSADHForTest;

		class DocSADHForTest : DocSADH
		{
			public static new DocSADHForTest New(CusEntryHeader entryHeader, BusinessObjectFactory factoryToWrap) => new DocSADHForTest(entryHeader, factoryToWrap);

			protected DocSADHForTest(CusEntryHeader entryHeader, BusinessObjectFactory factoryToWrap) : base(entryHeader, factoryToWrap)
			{
			}

			public ZBool ShowBox2SupplierCountryCodeExposed { get; set; }

			protected override ZBool ShowBox2SupplierCountryCodeCore => ShowBox2SupplierCountryCodeExposed;
		}
	}
}
