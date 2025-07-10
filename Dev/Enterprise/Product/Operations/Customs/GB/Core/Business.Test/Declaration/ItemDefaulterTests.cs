using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.Declaration.ItemDefaulter.Testing
{
	sealed class ItemDefaulterTests : TestCaseWithFactory
	{
		public void TestEndToEndThreeWayTestSetCpcToDocToIprNumberAndSpoff_CompanyRegistryFallback()
		{
			RunTest(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty);
		}

		public void TestEndToEndThreeWayTestSetCpcToDocToIprNumberAndSpoff_RegistryFallback()
		{
			RunTest(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid());
		}

		void RunTest(Guid companyPkToSetForRego, Guid branchPkToSetForRego)
		{
			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>();

			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				new string[] { importCodeType, exportCodeType }, "C601", "C601 DES", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "Additional Information");
			var cusCode2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "NCGDS", "NCGDS DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Direction", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Level", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			cusCode2.Attributes.AddNew("Direction", "IMPORT");
			cusCode2.Attributes.AddNew("Direction", "EXPORT");
			cusCode2.Attributes.AddNew("Level", "ITEM");

			Factory.Save();

			var iprClient = Factory.New<OrgHeader>();
			var itemCpcToC601 = CreateNewItemDefaulterSetting_CpcToC601(Factory);
			var itemCpcToSpoffIPR = CreateNewItemDefaulterSetting_CpcToSpoff(Factory);
			var itemC601ToIpr = CreateNewItemDefaulterSetting_C601ToIpr(iprClient);
			var itemCpc06ClientEoriDucr = CreateNewItemDefaulterSetting_Cpc061ToNcgds(Factory);
			var itemCpc061ToNcgds = CreateNewItemDefaulterSetting_Cpc06ToUseCLientEoriForDucr(Factory);
			var hmrcIprSpoffAddress = Factory.Load<OrgAddress>(itemCpcToSpoffIPR.TargetOrgAddress);
			var itemCpcToSpoffWarehouse = CreateNewItemDefaulterSetting_CpcToSpoffWarehouse(hmrcIprSpoffAddress);
			var coll = new ItemDefaulterSettingCollection();
			coll.Add(itemCpcToC601);
			coll.Add(itemCpcToSpoffIPR);
			coll.Add(itemC601ToIpr);
			coll.Add(itemCpcToSpoffWarehouse);
			coll.Add(itemCpc061ToNcgds);
			coll.Add(itemCpc06ClientEoriDucr);
			GBCustomsDataRegistry.Instance.ItemDefaults.SetValue(companyPkToSetForRego, branchPkToSetForRego, Guid.Empty, coll);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = iprClient.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "4100000";
			AssertEquals(hmrcIprSpoffAddress, invoiceLine.SupervisingOffice);
			AssertEquals(1, invoiceLine.SupportingDocuments.Count);
			var sd = invoiceLine.SupportingDocuments[0];
			AssertEquals("C601", sd.CSI_Code);
			AssertEquals("IP/1234/567/00", sd.CSI_ReferenceNumber);
			AssertEquals(ZString.Empty, sd.CSI_Availability);
			AssertEquals(ZString.Empty, sd.CSI_Actions);

			invoiceLine.JI_Procedure = "0612345";
			AssertEquals(1, invoiceLine.AdditionalInfos.Count);
			var ai = invoiceLine.AdditionalInfos[0];
			AssertEquals("NCGDS", ai.CSI_Code);

			var invoiceLine2Warehouse = (EU.Business.Declaration.JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2Warehouse.JI_Procedure = "71XXXX0";
			AssertEquals(itemCpcToSpoffWarehouse.TargetOrgAddress, invoiceLine2Warehouse.SupervisingOffice.PK);

			AssertEquals(false, invoiceLine.Declaration.UseClientEoriForDucr);
			invoiceLine.JI_Procedure = "069xxxx";
			AssertEquals(true, invoiceLine.Declaration.UseClientEoriForDucr);
			invoiceLine.Declaration.JE_UCR = "ABC";
			invoiceLine.Declaration.DeclarationNumber = "123";
			AssertEquals("Pre-req - need to put the dec in a state that doesn't allow the Client EORI checkbox to be changed", true, invoiceLine.Declaration.DucrGenerationOptionsReadOnly);
			invoiceLine.Declaration.UseClientEoriForDucr = false;
			invoiceLine.JI_Procedure = "069yyyy";
			AssertEquals("When the target checkbox field is read only, do not update it", false, invoiceLine.Declaration.UseClientEoriForDucr);
		}

		// Copy-pasted from GB/Core/Registry.Test/Business/ItemDefaulterBusinessTests.cs to remove reference to Registry.Test
		static ItemDefaulterSetting CreateNewItemDefaulterSetting_CpcToC601(BusinessObjectFactory factory)
		{
			var item = new ItemDefaulterSetting(factory);
			item.SourceType = SourceTypesList.Codes.CustomsProcedureCodeBox37;
			item.SourceValue = "4100000";
			item.TargetType = TargetTypesList.Codes.SupportingDocumentBox44;
			item.TargetCode = "C601";
			return item;
		}

		static ItemDefaulterSetting CreateNewItemDefaulterSetting_Cpc061ToNcgds(BusinessObjectFactory factory)
		{
			var item = new ItemDefaulterSetting(factory);
			item.SourceType = SourceTypesList.Codes.CustomsProcedureCodeBox37;
			item.SourceValue = "061.*";
			item.TargetType = TargetTypesList.Codes.AdditionalInformationStatementBox44;
			item.TargetCode = "NCGDS";
			return item;
		}

		static ItemDefaulterSetting CreateNewItemDefaulterSetting_CpcToSpoff(BusinessObjectFactory factory)
		{
			var hmrcAddress = factory.Load<OrgAddress>(new ZGuid("3A6474C2-B99E-4338-9C9F-ACB95C715B06"));
			if (hmrcAddress == null)
			{
				Assert("Pre-Req failed - OrgAddress with PK C3F842EF-3BE5-448C-BED3-0017B232C624 was not found in test database.  There's nothing special about this address, it was picked at random, but it still needs to exist.", false);
			}
			var item = new ItemDefaulterSetting(factory);
			item.SourceType = SourceTypesList.Codes.CustomsProcedureCodeBox37;
			item.SourceValue = "4100000";
			item.TargetType = TargetTypesList.Codes.SupervisingOfficeBox44;
			item.TargetCode = "";
			item.TargetOrgAddress = hmrcAddress.PK;
			return item;
		}

		static ItemDefaulterSetting CreateNewItemDefaulterSetting_C601ToIpr(OrgHeader iprClient)
		{
			iprClient.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.InwardProcessingReliefNumber, "IP/1234/567/00");
			var item = new ItemDefaulterSetting(iprClient.Factory);
			item.SourceType = SourceTypesList.Codes.SupportingDocumentBox44;
			item.SourceValue = "C601";
			item.TargetType = TargetTypesList.Codes.RegistrationNumberFromImporter;
			item.TargetCode = OrgCusCode.EuropeanUnionSharedCodeTypes.InwardProcessingReliefNumber;
			return item;
		}

		static ItemDefaulterSetting CreateNewItemDefaulterSetting_Cpc06ToUseCLientEoriForDucr(BusinessObjectFactory factory)
		{
			var item = new ItemDefaulterSetting(factory);
			item.SourceType = SourceTypesList.Codes.CustomsProcedureCodeBox37;
			item.SourceValue = "069.*";  // anythign is ok really
			item.TargetType = TargetTypesList.Codes.ClientEoriForDucrTickbox;
			item.TargetCode = "";
			return item;
		}

		public void TestSourceTypePREF_CompanyFallback()
		{
			RunTestSourceTypePREF(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty);
		}

		public void TestSourceTypePREF_BranchFallback()
		{
			RunTestSourceTypePREF(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid());
		}

		void RunTestSourceTypePREF(Guid companyPk, Guid branchPk)
		{
			_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "C100", "C100 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var itemDefaults = new ItemDefaulterSettingCollection();
			var item = CreateNewItemDefaultSetting(SourceTypesList.Codes.Preference, "100", TargetTypesList.Codes.SupportingDocumentBox44, "C100");
			itemDefaults.Add(item);
			GBCustomsDataRegistry.Instance.ItemDefaults.SetValue(companyPk, branchPk, Guid.Empty, itemDefaults);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertEquals(0, invoiceLine.SupportingDocuments.Count);
			invoiceLine.JI_PrimaryPreference = "100";
			AssertEquals(1, invoiceLine.SupportingDocuments.Count);
			var sDoc = invoiceLine.SupportingDocuments[0];
			AssertEquals("C100", sDoc.CSI_Code);
		}

		public void TestTargetTypeINVNO_CompanyFallback()
		{
			RunTestTargetTypeINVNO(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty);
		}

		public void TestTargetTypeINVNO_BranchFallback()
		{
			RunTestTargetTypeINVNO(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid());
		}

		void RunTestTargetTypeINVNO(Guid companyPk, Guid branchPk)
		{
			_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "U100", "U100 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var itemDefaults = new ItemDefaulterSettingCollection();
			var item = CreateNewItemDefaultSetting(SourceTypesList.Codes.Preference, "U100", TargetTypesList.Codes.SetReferenceFromInvoiceNumber, "1234");
			AssertHasErrorContaining(item.TargetTypeInfo, "only allowed with source type SUPPD");
			AssertHasErrorContaining(item.TargetCodeInfo, "should be blank");
			item.ClearAllNotifications();
			item.SourceType = SourceTypesList.Codes.SupportingDocumentBox44;
			item.TargetCode = ZString.Empty;
			AssertNoErrorContaining(item.TargetTypeInfo, "only allowed with source type SUPPD");
			AssertNoErrorContaining(item.TargetCodeInfo, "should be blank");
			itemDefaults.Add(item);
			GBCustomsDataRegistry.Instance.ItemDefaults.SetValue(companyPk, branchPk, Guid.Empty, itemDefaults);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV001AR1";
			var sDoc = invoice.SupportingDocuments.AddNew();
			sDoc.CSI_Code = "U100";
			AssertEquals("INV001AR1", sDoc.CSI_ReferenceNumber);

			var invoiceLine = invoice.InvoiceLines.AddNew();
			sDoc = invoiceLine.SupportingDocuments.AddNew();
			sDoc.CSI_Code = "U100";
			AssertEquals("INV001AR1", sDoc.CSI_ReferenceNumber);
		}

		public void TestAllowCascadeOfItemDefaultsForSupportingDocuments_CompanyFallback()
		{
			RunTestAllowCascadeOfItemDefaultsForSupportingDocuments(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty);
		}

		public void TestAllowCascadeOfItemDefaultsForSupportingDocuments_BranchFallback()
		{
			RunTestAllowCascadeOfItemDefaultsForSupportingDocuments(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), hasInvoiceParent: true);
		}

		void RunTestAllowCascadeOfItemDefaultsForSupportingDocuments(Guid companyPk, Guid branchPk, bool hasInvoiceParent = false)
		{
			_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "U110", "U110 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "U111", "U111 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "U112", "U112 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var itemDefaults = new ItemDefaulterSettingCollection();
			var item1 = CreateNewItemDefaultSetting(SourceTypesList.Codes.SupportingDocumentBox44, "U110", TargetTypesList.Codes.SupportingDocumentBox44, "U111");
			var item2 = CreateNewItemDefaultSetting(SourceTypesList.Codes.SupportingDocumentBox44, "U111", TargetTypesList.Codes.SupportingDocumentBox44, "U112");
			var item3 = CreateNewItemDefaultSetting(SourceTypesList.Codes.SupportingDocumentBox44, "U112", TargetTypesList.Codes.SupportingDocumentBox44, "U110");
			itemDefaults.AddRange(new[] { item1, item2, item3 });
			GBCustomsDataRegistry.Instance.ItemDefaults.SetValue(companyPk, branchPk, Guid.Empty, itemDefaults);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var supportingDocuments = hasInvoiceParent ? invoice.SupportingDocuments : invoiceLine.SupportingDocuments;

			var sDoc = supportingDocuments.AddNew();
			sDoc.CSI_Code = "U110";
			AssertEquals(2, supportingDocuments.Count);
			AssertEquals("U110", supportingDocuments[0].CSI_Code);
			AssertEquals("U111", supportingDocuments[1].CSI_Code);

			sDoc = supportingDocuments.AddNew();
			sDoc.CSI_Code = "U112";
			AssertEquals(4, supportingDocuments.Count);
			AssertEquals("U112", supportingDocuments[2].CSI_Code);
			AssertEquals("U110", supportingDocuments[3].CSI_Code);
			if (!hasInvoiceParent)
			{
				AssertHasMessageErrorContaining(supportingDocuments[3].CSI_CodeInfo, "already exists on this invoice line.");
			}
		}

		ItemDefaulterSetting CreateNewItemDefaulterSetting_CpcToSpoffWarehouse(OrgAddress hmrcIprAddress)
		{
			var hmrcOrg = hmrcIprAddress.Header;
			var hmrcWarehouseSpoffAddress = hmrcOrg.Addresses.AddNew();
			var item = new ItemDefaulterSetting();
			item.SourceType = SourceTypesList.Codes.CustomsProcedureCodeBox37;
			item.SourceValue = "^71.*0";  //warehouse
			item.TargetType = TargetTypesList.Codes.SupervisingOfficeBox44;
			item.TargetCode = ZString.Empty;
			item.TargetOrgAddress = hmrcWarehouseSpoffAddress.PK;
			return item;
		}

		ItemDefaulterSetting CreateNewItemDefaultSetting(string sourceType, string sourceCode, string targetType, string targetCode)
		{
			return new ItemDefaulterSetting(Factory) { SourceType = sourceType, SourceValue = sourceCode, TargetType = targetType, TargetCode = targetCode };
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
			eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			_ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom", eun);
		}

		UniversalReferenceTestDataHelper helper;
		RefDataGrouping eun;
	}
}
