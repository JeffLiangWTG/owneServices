using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DeclarationAESGoodsShipmentWrapperTest : WrapperHelperTest<DeclarationAESGoodsShipmentWrapper>
	{
		public void TestNatureOfTransaction()
		{
			CombineAssertions(() =>
			{
				invoiceHeader.JZ_ValuationCode = "21";
				AssertEquals("Expected filled NatureOfTransaction when entryInstruction is not B nor C", "21", wrapper.NatureOfTransaction);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
				AssertEquals("Expected empty NatureOfTransaction when entryInstruction is B", ZString.Empty, wrapper.NatureOfTransaction);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
				wrapper = GetWrapper(entryHeader, isComplementaryCWithMRN: true);
				AssertEquals("Expected filled NatureOfTransaction when EntryInstruction is C but isComplementaryCWithMRN is true", "21", wrapper.NatureOfTransaction);

				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected empty NatureOfTransaction when EntryInstruction is C and isComplementaryCWithMRN is false", ZString.Empty, wrapper.NatureOfTransaction);
			});
		}

		public void TestCountryOfExport()
		{
			declaration.JE_GoodsOrigin = "ES";
			AssertEquals("Expected filled CountryOfExport", "ES", wrapper.CountryOfExport);
		}

		public void TestCountryOfDestination()
		{
			declaration.JE_GoodsDestination = "FR";
			AssertEquals("Expected filled CountryOfDestination", "FR", wrapper.CountryOfDestination);
		}

		public void TestAdditionalSupplyActors()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty AdditionalSupplyActors when no data declared", 0, wrapper.AdditionalSupplyActors.Count);

				entryInstruction.CusSupplyChainActorReferences.AddNew();
				wrapper = GetWrapper(entryHeader);
				var additionalSupplyActors = wrapper.AdditionalSupplyActors;
				AssertEquals("Expected filled AdditionalSupplyActors", 1, additionalSupplyActors.Count);
				AssertSame("Cached AdditionalSupplyActors", wrapper.AdditionalSupplyActors, additionalSupplyActors);
			});
		}

		public void TestDeliveryTerms()
		{
			CombineAssertions(() =>
			{
				var deliveryTerms = wrapper.DeliveryTerms;
				AssertNotNull("Expected filled DeliveryTerms when entryInstruction is not B nor C", deliveryTerms);
				AssertSame("Cached DeliveryTerms", wrapper.DeliveryTerms, deliveryTerms);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
				wrapper = GetWrapper(entryHeader);
				AssertNull("Expected empty DeliveryTerms when entryInstruction is B", wrapper.DeliveryTerms);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
				wrapper = GetWrapper(entryHeader, isComplementaryCWithMRN: true);
				AssertNotNull("Expected filled DeliveryTerms when EntryInstruction is C but isComplementaryCWithMRN is true", wrapper.DeliveryTerms);

				wrapper = GetWrapper(entryHeader);
				AssertNull("Expected empty DeliveryTerms when EntryInstruction is C but isComplementaryCWithMRN is false", wrapper.DeliveryTerms);
			});
		}

		public void TestSupportingDocuments()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty SupportingDocuments list", 0, wrapper.SupportingDocuments.Count);

				var supdoc1 = declaration.SupportingDocuments.AddNew();
				supdoc1.CSI_Code = "9001";

				var supdoc2 = declaration.SupportingDocuments.AddNew();
				supdoc2.CSI_Code = "Y001";

				var supdoc3 = entryInstruction.SupportingDocuments.AddNew();
				supdoc3.CSI_Code = "A002";

				var supdoc4 = entryInstruction.SupportingDocuments.AddNew();
				supdoc4.CSI_Code = "Y002";

				var supdoc5 = invoiceHeader.SupportingDocuments.AddNew();
				supdoc5.CSI_Code = "9003";

				var supdoc6 = invoiceLine.SupportingDocuments.AddNew();
				supdoc6.CSI_Code = "Y003";

				using (TemporarilyClearDeclarationConfigurationAndThenSetIsTransitionPeriodAES30(declaration, true))
				{
					wrapper = GetWrapper(entryHeader);
					AssertEquals("Expected empty SupportingDocuments list when declared but transitionPeriod is true", 0, wrapper.SupportingDocuments.Count);
				}

				using (TemporarilyClearDeclarationConfigurationAndThenSetIsTransitionPeriodAES30(declaration, false))
				{
					wrapper = GetWrapper(entryHeader);
					var documents = wrapper.SupportingDocuments;
					AssertEquals("Expected filled SupportingDocuments when transition period is false (only included those that don't start with Y and are in declaration and entryInstruction)", 2, documents.Count);
					AssertSame("Cached SupportingDocuments", wrapper.SupportingDocuments, documents);
					AssertContainsExactElementsInExactOrder("Expected filled SupportingDocuments when transition period is false (only included those that don't start with Y and are in declaration and entryInstruction), ordered Name", new ZString[] { "A002", "9001" }, documents.Select(x => x.Name));
					AssertContainsExactElementsInExactOrder("Expected filled SupportingDocuments when transition period is false (only included those that don't start with Y and are in declaration and entryInstruction), ordered SequenceNumber", new ZString[] { "1", "2" }, documents.Select(x => x.SequenceNumber));
				}
			});

			AssertNoExceptionThrown("When any Supporting Document has null or empty CSI_Code no exception should be thrown", () =>
			{
				declaration.SupportingDocuments.RemoveAndDeleteAll();
				var supdoc7 = declaration.SupportingDocuments.AddNew();
				supdoc7.CSI_Code = ZString.Empty;

				entryInstruction.SupportingDocuments.RemoveAndDeleteAll();
				var supdoc8 = entryInstruction.SupportingDocuments.AddNew();
				supdoc8.CSI_Code = ZString.Empty;

				invoiceHeader.SupportingDocuments.RemoveAndDeleteAll();
				var supDoc9 = invoiceHeader.SupportingDocuments.AddNew();
				supDoc9.CSI_Code = ZString.Empty;

				invoiceLine.SupportingDocuments.RemoveAndDeleteAll();
				var supDoc10 = invoiceLine.SupportingDocuments.AddNew();
				supDoc10.CSI_Code = ZString.Empty;

				wrapper = GetWrapper(entryHeader);
				var documents = wrapper.SupportingDocuments;
			});
		}

		public void TestAdditionalReferences()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty AdditionalReferences list", 0, wrapper.AdditionalReferences.Count);

				var supdoc1 = declaration.SupportingDocuments.AddNew();
				supdoc1.CSI_Code = "9001";

				var supdoc2 = declaration.SupportingDocuments.AddNew();
				supdoc2.CSI_Code = "Y001";

				var supdoc3 = entryInstruction.SupportingDocuments.AddNew();
				supdoc3.CSI_Code = "9002";

				var supdoc4 = entryInstruction.SupportingDocuments.AddNew();
				supdoc4.CSI_Code = "Y002";

				var supdoc5 = invoiceHeader.SupportingDocuments.AddNew();
				supdoc5.CSI_Code = "9003";

				var supdoc6 = invoiceLine.SupportingDocuments.AddNew();
				supdoc6.CSI_Code = "Y003";

				var addRef1 = entryInstruction.AdditionalInfos.AddNew();
				addRef1.CSI_SubType = "REF";

				var addRef2 = declaration.AdditionalInfos.AddNew();
				addRef2.CSI_SubType = "REF";

				var addRef3 = invoiceHeader.AdditionalInfos.AddNew();
				addRef3.CSI_SubType = "REF";

				var addRef4 = invoiceLine.AdditionalInfos.AddNew();
				addRef4.CSI_SubType = "REF";

				var addRef5 = declaration.AdditionalInfos.AddNew();
				addRef5.CSI_SubType = "TRA";

				var addRef6 = entryInstruction.AdditionalInfos.AddNew();
				addRef6.CSI_SubType = "INF";

				using (TemporarilyClearDeclarationConfigurationAndThenSetIsTransitionPeriodAES30(declaration, true))
				{
					wrapper = GetWrapper(entryHeader);
					AssertEquals("Expected empty AdditionalReferences list when declared but transitionPeriod is true", 0, wrapper.AdditionalReferences.Count);
				}

				using (TemporarilyClearDeclarationConfigurationAndThenSetIsTransitionPeriodAES30(declaration, false))
				{
					wrapper = GetWrapper(entryHeader);
					var documents = wrapper.AdditionalReferences;
					AssertEquals("Expected filled AdditionalReferences when transition period is false (only included those that start with Y and are in declaration and entryInstruction), (also include AdditionalInfos where CSI_SubType = 'REF' for EntryInstruction and Declaration.)", 4, documents.Count);
					AssertSame("Cached AdditionalReferences", wrapper.AdditionalReferences, documents);
				}
			});
		}

		public void TestAdditionalInfos()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty AdditionalInfos list", 0, wrapper.AdditionalInfos.Count);

				var supdoc1 = declaration.AdditionalInfos.AddNew();
				supdoc1.CSI_Code = "9001";
				supdoc1.CSI_SubType = "TRA";

				var supdoc1INF = declaration.AdditionalInfos.AddNew();
				supdoc1INF.CSI_Code = "Y001";
				supdoc1INF.CSI_SubType = "INF";

				var supdoc2 = entryInstruction.AdditionalInfos.AddNew();
				supdoc2.CSI_Code = "9002";
				supdoc2.CSI_SubType = "TRA";

				var supdoc2INF = entryInstruction.AdditionalInfos.AddNew();
				supdoc2INF.CSI_Code = "Y002";
				supdoc2INF.CSI_SubType = "INF";

				var supdoc3 = invoiceHeader.AdditionalInfos.AddNew();
				supdoc3.CSI_Code = "9003";
				supdoc3.CSI_SubType = "TRA";

				var supdoc3INF = invoiceHeader.AdditionalInfos.AddNew();
				supdoc3INF.CSI_Code = "Y003";
				supdoc3INF.CSI_SubType = "INF";

				var supdoc4 = invoiceLine.AdditionalInfos.AddNew();
				supdoc4.CSI_Code = "9004";
				supdoc4.CSI_SubType = "INF";

				using (TemporarilyClearDeclarationConfigurationAndThenSetIsTransitionPeriodAES30(declaration, true))
				{
					wrapper = GetWrapper(entryHeader);
					AssertEquals("Expected empty AdditionalInfos list when declared but transitionPeriod is true", 0, wrapper.AdditionalInfos.Count);
				}

				using (TemporarilyClearDeclarationConfigurationAndThenSetIsTransitionPeriodAES30(declaration, false))
				{
					wrapper = GetWrapper(entryHeader);
					var documents = wrapper.AdditionalInfos;
					AssertEquals("Expected filled AdditionalInfos when transition period is false (only included those that that have subType INF and are in declaration and entryInstruction)", 2, documents.Count);
					AssertSame("Cached AdditionalInfos", wrapper.AdditionalInfos, documents);
				}
			});
		}

		public void TestConsignment()
		{
			var consignment = wrapper.Consignment;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Consignment", consignment);
				AssertSame("Cached Consignment", wrapper.Consignment, consignment);
			});
		}

		public void TestLines()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected 1 Line (mandatory at least one)", 1, wrapper.Lines.Count);

				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_Tariff = "2203001011";

				var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine3.JI_Tariff = "2203001012";

				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

				entryHeader = declaration.CustomsEntryHeaders[0];
				wrapper = GetWrapper(entryHeader);

				var lines = wrapper.Lines;

				AssertEquals("Expected 3 Lines", 3, lines.Count);
				AssertSame("Cached Lines", wrapper.Lines, lines);
			});
		}

		public void TestUCRInConsignmentOrLines() 
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty UCRReferenceNumber in Consignment when declaration reference is empty", ZString.Empty, wrapper.Consignment.ReferenceNumberUCR);
				declaration.JE_OwnerRef = "reference1";
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected filled ReferenceNumberUCR in Consignment when declaration reference is not empty", "reference1", wrapper.Consignment.ReferenceNumberUCR);

				invoiceLine.ZG_CommercialReference = "reference2";
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected filled UCRReferenceNumber in Lines when Commercial Reference is filled", "reference2", wrapper.Lines.ToList()[0].UCRReferenceNumber);

				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_Tariff = "2203001011";
				invoiceLine2.ZG_CommercialReference = "reference3";

				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

				entryHeader = declaration.CustomsEntryHeaders[0];
				wrapper = GetWrapper(entryHeader);
				AssertContainsExactElementsInAnyOrder("Expected filled UCRReferenceNumber in Lines when there are multiple lines", new ZString[] { "reference2", "reference3" }, wrapper.Lines.Select(x => x.UCRReferenceNumber).ToArray());
			});
		}

		public void TestUCRInLinesWhenCommercialReferenceIsEmpty()
		{
			CombineAssertions(() =>
			{
				invoiceLine.ZG_CommercialReference = ZString.Empty;
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected empty UCRReferenceNumber in Lines when Commercial Reference is empty", ZString.Empty, wrapper.Lines.ToList()[0].UCRReferenceNumber);

				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_Tariff = "2203001011";
				invoiceLine2.ZG_CommercialReference = ZString.Empty;

				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

				entryHeader = declaration.CustomsEntryHeaders[0];
				wrapper = GetWrapper(entryHeader);
				AssertContainsExactElementsInAnyOrder("Expected empty UCRReferenceNumber in Lines when there are multiple lines with empty Commercial Reference", new ZString[] { ZString.Empty, ZString.Empty }, wrapper.Lines.Select(x => x.UCRReferenceNumber).ToArray());
			});
		}

		public void TestConsignorInConsignmentOrLines()
		{
			CombineAssertions(() =>
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				invoiceLine.JI_OA_ExporterAddress = orgHeader.MainAddress.PK;
				AssertNotNull("Expected filled Consignor in Consignment when there is only one line", wrapper.Consignment.Consignor);
				AssertNull("Expected empty Consignor in Lines when there is only one line", wrapper.Lines.ToList()[0].Consignor);

				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_Tariff = "2203001011";
				var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
				invoiceLine2.JI_OA_ExporterAddress = orgHeader2.MainAddress.PK;

				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

				entryHeader = declaration.CustomsEntryHeaders[0];
				wrapper = GetWrapper(entryHeader);
				var lines = wrapper.Lines.ToList();
				AssertNull("Expected empty Consignor in Consignment when there are multiple lines with different codes", wrapper.Consignment.Consignor);
				AssertNotNull("Expected filled Consignor in Lines when there are multiple lines with different codes", lines[0].Consignor);
				AssertNotNull("Expected filled Consignor in Lines when there are multiple lines with different codes", lines[1].Consignor);

				invoiceLine2.JI_OA_ExporterAddress = orgHeader.MainAddress.PK;
				wrapper = GetWrapper(entryHeader);
				lines = wrapper.Lines.ToList();
				AssertNotNull("Expected filled Consignor in Consignment when there are multiple lines with same codes", wrapper.Consignment.Consignor);
				AssertNull("Expected empty Consignor in Lines when there are multiple lines with same codes, line 1", lines[0].Consignor);
				AssertNull("Expected empty Consignor in Lines when there are multiple lines with same codes, line 2", lines[1].Consignor);
			});
		}

		public void TestConsigneeInConsignmentOrLines()
		{
			CombineAssertions(() =>
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				invoiceHeader.JZ_OH_Buyer = orgHeader.PK;
				AssertNotNull("Expected filled Consignee in Consignment when there is only one line", wrapper.Consignment.Consignee);
				AssertNull("Expected empty Consignee in Lines when there is only one line", wrapper.Lines.ToList()[0].Consignee);

				var invoiceHeader2 = declaration.Invoices.AddNew();
				var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
				invoiceLine2.JI_Tariff = "2203001011";
				var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
				invoiceHeader2.JZ_OH_Buyer = orgHeader2.PK;

				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

				entryHeader = declaration.CustomsEntryHeaders[0];
				wrapper = GetWrapper(entryHeader);
				var lines = wrapper.Lines.ToList();
				AssertNull("Expected empty Consignee in Consignment when there are multiple lines with different codes", wrapper.Consignment.Consignee);
				AssertNotNull("Expected filled Consignee in Lines when there are multiple lines with different codes", lines[0].Consignee);
				AssertNotNull("Expected filled Consignee in Lines when there are multiple lines with different codes", lines[1].Consignee);

				invoiceHeader2.JZ_OH_Buyer = orgHeader.PK;
				wrapper = GetWrapper(entryHeader);
				lines = wrapper.Lines.ToList();
				AssertNotNull("Expected filled Consignee in Consignment when there are multiple lines with same codes", wrapper.Consignment.Consignee);
				AssertNull("Expected empty Consignee in Lines when there are multiple lines with same codes, line 1", lines[0].Consignee);
				AssertNull("Expected empty Consignee in Lines when there are multiple lines with same codes, line 2", lines[1].Consignee);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "11";

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			entryHeader = declaration.CustomsEntryHeaders[0];

			wrapper = GetWrapper(entryHeader);
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		CusEntryHeader entryHeader;
		DeclarationAESGoodsShipmentWrapper wrapper;

		DeclarationAESGoodsShipmentWrapper GetWrapper(CusEntryHeader entryHeader, bool isComplementaryCWithMRN = false) => new DeclarationAESGoodsShipmentWrapper(entryHeader, isComplementaryCWithMRN);

		protected override DeclarationAESGoodsShipmentWrapper GetProvider() => wrapper;
	}
}
