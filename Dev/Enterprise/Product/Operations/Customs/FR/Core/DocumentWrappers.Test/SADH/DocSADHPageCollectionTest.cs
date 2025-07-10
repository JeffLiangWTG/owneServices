using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using FRCusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.FR.Business.Declaration.CusEntryLine>;

namespace Enterprise.Customs.FR.DocumentWrappers.SADH.Testing;

[TestedType(typeof(DocSADHPageCollection))]
sealed class DocSADHPageCollectionTest : Enterprise.DocumentWrappers.Customs.EU.Testing.DocSADHPageCollectionTest<DocSADHPageCollection>
{
	public void TestPagesAreCorrectlyCreatedAndSortedInThisCollection()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
		{
			GenerateValueForBox44AddInfoAndDocumentsAggregated(out var entryHeader);
			AddEntryLine(in entryHeader, true);  //1
			AddEntryLine(in entryHeader, true);  //2
			AddEntryLine(in entryHeader, true);  //3
			AddEntryLine(in entryHeader, false); //4
			AddEntryLine(in entryHeader, false); //5
			AddEntryLine(in entryHeader, true);  //6
			AddEntryLine(in entryHeader, false); //7
			AddEntryLine(in entryHeader, false); //8
			AddEntryLine(in entryHeader, false); //9
			AddEntryLine(in entryHeader, false); //10
			AddEntryLine(in entryHeader, false); //11

			var pageCollection = new DocSADHPageCollection(entryHeader.MergedLines, Factory);
			AssertEquals("There should totally be 9 pages.", 9, pageCollection.Count);
			AssertEquals(true, pageCollection[0].Line1.NeedASecondPageForBox44);

			AssertHeaderPage(pageCollection[0]);
			AssertEnlargedBox44Page(pageCollection[1]);
			AssertTripleBox44Page(pageCollection[2]);
			AssertEnlargedBox44Page(pageCollection[3]);
			AssertEnlargedBox44Page(pageCollection[4]);
			AssertTripleBox44Page(pageCollection[5]);
			AssertEnlargedBox44Page(pageCollection[6]);
			AssertTripleBox44Page(pageCollection[7]);
			AssertTripleBox44Page(pageCollection[8], false, false);
		}

		void AssertHeaderPage(DocSADHPage page)
		{
			AssertNotNull("Line1 should not be null.", page.Line1);
			AssertNull("Line2 should be null.", page.Line2);
			AssertNull("Line3 should be null.", page.Line3);
			AssertEquals("Header page should not show enlarged box44.", false, page.ShowEnlargedBox44);
		}

		void AssertEnlargedBox44Page(DocSADHPage page)
		{
			AssertNotNull("Line1 should not be null.", page.Line1);
			AssertNull("Line2 should be null.", page.Line2);
			AssertNull("Line3 should be null.", page.Line3);
			AssertEquals("This page should show enlarged box44.", true, page.ShowEnlargedBox44);
		}

		void AssertTripleBox44Page(DocSADHPage page, bool hasLine2 = true, bool hasLine3 = true)
		{
			AssertNotNull("Line1 should not be null.", page.Line1);
			if (hasLine2)
			{
				AssertNotNull("Line2 should not be null.", page.Line2);
			}
			else
			{
				AssertNull("Line2 should be null.", page.Line2);
			}
			if (hasLine3)
			{
				AssertNotNull("Line3 should not be null.", page.Line3);
			}
			else
			{
				AssertNull("Line3 should be null.", page.Line3);
			}
			AssertEquals("This page should show enlarged box44.", false, page.ShowEnlargedBox44);
		}
	}
	void AddEntryLine(in CusEntryHeader entryHeader, bool shouldUseEnlargedBox44)
	{
		var declaration = entryHeader.Declaration;
		var invoiceHeader = declaration.Invoices[0];
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		if (shouldUseEnlargedBox44)
		{
			for (var i = 0; i < 8; i++)
			{
				var addInfo = invoiceLine.AdditionalInfos.AddNew();
				addInfo.CSI_Code = i.ToString();
				addInfo.CSI_Description = new string('A', addInfo.CSI_DescriptionInfo.MaxLength);
			}
		}
	}

	void GenerateValueForBox44AddInfoAndDocumentsAggregated(out CusEntryHeader entryHeader)
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var frGroup = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);

		var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation;
		var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
		var addInfCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation;
		var attributeNameValuePairs = new Dictionary<string, string[]>();

		var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "EX", "10", "71", "F61", "", "EXP", "10P");
		procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.No;
		procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;

		attributeNameValuePairs.Add("Level", new string[] { "ITEM", "HEADER" });
		helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.France, new string[] { importCodeType, exportCodeType }, "HDR1", "COZ WE WANT TO", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

		attributeNameValuePairs.Clear();
		attributeNameValuePairs.Add("Level", new string[] { "HEADER" });
		helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.France, new string[] { importCodeType, exportCodeType }, "HNY1", "HONEY TO THE BEE", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
		helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.France, new string[] { addInfCodeType }, "HIT99", "RANDOM SONG TITLE", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

		attributeNameValuePairs.Clear();
		attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
		helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.France, new string[] { importCodeType, exportCodeType }, "9100", "I HAVE NO CLUE", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
		helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.France, new string[] { importCodeType, exportCodeType }, "9120", "9120 Test", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
		declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		var importer = Factory.New<OrgHeader>();
		importer.OH_Code = "IMPORTER";
		declaration.JE_OH_Importer = importer.PK;

		var euAddInfo = EU.Business.EUOrgImpAddInfo.Get(importer, Core.Constants.CountryCodes.France);
		euAddInfo.ZO_UseFr3FiscalRepresentation = true;

		var fiscalReferenceOrganisation = Factory.New<OrgHeader>();
		fiscalReferenceOrganisation.OH_Code = "FISCALREP";

		var fiscalReferenceOrganisationAddress = fiscalReferenceOrganisation.Addresses.AddNew();
		fiscalReferenceOrganisationAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
		fiscalReferenceOrganisationAddress.OA_Address1 = "fiscal address1";
		fiscalReferenceOrganisationAddress.OA_Address2 = "fiscal address2";
		fiscalReferenceOrganisationAddress.OA_City = "fiscal city";
		fiscalReferenceOrganisationAddress.OA_PostCode = "333";
		fiscalReferenceOrganisationAddress.OA_RN_NKCountryCode = "FR";

		var fiscalReference = entryInstruction.FiscalReferences.AddNew();
		fiscalReference.CFR_Code = "FR3";
		fiscalReference.CFR_Reference = "FR33562024100133";
		fiscalReference.CFR_OA_Owner = fiscalReferenceOrganisationAddress.PK;
	}

	internal static Customs.Business.CusAuthorisationRule CreateCusAuthorisationRule(Customs.Business.CusAuthorisationHeader authHeader, string code, string value)
	{
		var rule = authHeader.CusAuthorisationRules.AddNew();
		rule.CPR_RuleCode = code;
		rule.CPR_ValueFrom = value;
		return rule;
	}

	protected override DocSADHPageCollection GetNewDocumentWrapperCollection()
	{
		return new DocSADHPageCollection(EntryLineCollection, Factory);
	}

	protected override object GetNewObjectToWrap()
	{
		return null;
	}
	protected override DocumentEngineCore.DocWrappers.DocumentWrapper AddNewDocumentWrapperToCollection(DocumentEngineCore.DocWrappers.DocumentWrapperCollection collection)
	{
		var result = DocSADHPage.New(Factory, EntryLineCollection.AddNew());
		collection.Add(result);
		return result;
	}
	new FRCusEntryLineCollection EntryLineCollection
	{
		get
		{
			if (entryHeaderCollection == null)
			{
				var declaration = Factory.New<JobDeclaration>();
				var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				invoiceHeader.JobComInvoiceLines.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.MergedLines.AddNew();

				entryHeaderCollection = entryHeader.MergedLines;
			}
			return entryHeaderCollection;
		}
	}
	FRCusEntryLineCollection entryHeaderCollection;

	protected override string CountryToUseForTesting => Core.Constants.CountryCodes.France;
}
