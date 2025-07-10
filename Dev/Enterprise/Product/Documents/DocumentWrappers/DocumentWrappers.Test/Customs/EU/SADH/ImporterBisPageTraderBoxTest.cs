using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Customs.EU.Testing
{
	[Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Core.Constants.CountryCodes.Latvia)]
	sealed class ImporterBisPageTraderBoxTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("When docSADH is null", () => new ImporterBisPageTraderBox(null));
		}

		public void TestCaption()
		{
			AssertEquals("Caption", "8 Consignee", bisPageTraderBox.Caption);
		}

		public void TestID()
		{
			AssertEquals("ID", "", bisPageTraderBox.ID);

			var importer = Factory.New<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "LVXYZ", RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Latvia));
			entryHeader.Declaration.JE_OH_Importer = importer.PK;
			AssertEquals("ID", "LVXYZ", bisPageTraderBox.ID);
		}

		public void TestContent()
		{
			AssertEquals("Content", "", bisPageTraderBox.Content);

			var importer = Factory.New<OrgHeader>();
			importer.OH_RL_NKClosestPort = "ITMIL";
			importer.OH_FullName = "FULL NAME";
			entryHeader.Declaration.JE_OH_Importer = importer.PK;
			docSADHForTest.ShowBox8ImporterCountryCodeExposed = false;
			AssertEquals("Content", "FULL NAME", bisPageTraderBox.Content);

			docSADHForTest.ShowBox8ImporterCountryCodeExposed = true;
			AssertEquals("Content", "FULL NAME\r\nItaly", bisPageTraderBox.Content);
		}

		protected override void SetUp()
		{
			base.SetUp();
			entryHeader = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			docSADHForTest = DocSADHForTest.New(entryHeader, Factory);
			bisPageTraderBox = new ImporterBisPageTraderBox(docSADHForTest);
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

			public ZBool ShowBox8ImporterCountryCodeExposed { get; set; }

			protected override ZBool ShowBox8ImporterCountryCodeCore => ShowBox8ImporterCountryCodeExposed;
		}
	}
}
