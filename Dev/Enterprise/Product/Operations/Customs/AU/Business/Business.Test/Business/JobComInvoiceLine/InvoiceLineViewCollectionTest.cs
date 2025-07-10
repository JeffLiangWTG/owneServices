using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class InvoiceLineViewCollectionTest : TestCaseWithFactory
	{
		public void TestCancelNonCommittedLine()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;

			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV 1";
			Factory.Save();

			var invoiceLineViewCollection = declaration.FilteredInvoiceLines;
			var tempLine = (invoiceLineViewCollection as IBindingList).AddNew() as BusinessObject;

			var tempExDocLineQuery = new ZQuery(QuarantineExDocLineSchema.QL_JI, tempLine.PK);
			AssertEquals("Temporary Quarantine Line created", 1, Factory.Load<QuarantineExDocLine>(tempExDocLineQuery).Length);

			(invoiceLineViewCollection as ICancelAddNew).CancelNew(0);

			AssertEquals("Temporary Invoice is deleted", true, tempLine.IsDeleted);
			AssertEquals("Temporary Quarantine Line is cleaned up", 0, Factory.Load<QuarantineExDocLine>(tempExDocLineQuery).Length);

			AssertNoExceptionThrown("Declaration can be saved.", () => Factory.Save());
		}

		public void TestCopyLastLineDetailsToNewLinesIfEnabled_CopyRFPDetails()
		{
			var testEstablishment = Factory.New<OrgHeader>();
			testEstablishment.OH_Code = "TESTEST";
			var estCode = testEstablishment.CustomsCodes.AddNew();
			estCode.OK_CodeType = OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber;
			estCode.OK_RN_NKCodeCountry = "AU";
			estCode.OK_CustomsRegNo = "12345";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			declaration.IsAQISCertificateRequest = true;

			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV 1";

			var invoiceLineViewCollection = declaration.FilteredInvoiceLines;
			var invoiceLine1 = invoiceLineViewCollection.AddNew();
			invoiceLine1.JI_Calc_Invoice = invoiceHeader.JZ_InvoiceNumber;
			var quarantineExDocLine1 = invoiceLine1.QuarantineExDocLine;

			// QuarantineExDocLine1
			{
				//RFP Packages
				{
					//Weights
					{
						quarantineExDocLine1.QL_NetQuantity = 1m;
						quarantineExDocLine1.QL_NetQuantityUnit = "BIL";
						quarantineExDocLine1.QL_ImperialNetWeight = 2m;
						quarantineExDocLine1.QL_ImperialNetWeightUnit = "CWI";
						quarantineExDocLine1.QL_GrossMetricWeight = 3m;
						quarantineExDocLine1.QL_GrossMetricWeightUnit = "CU";
					}

					//Shipping Marks and Batch Code
					{
						quarantineExDocLine1.QL_ShippingMarks = "ShippingMarks";
						quarantineExDocLine1.QL_BatchCode = "BatchCode";
					}

					//Packages
					{
						//Outer Pack
						{
							quarantineExDocLine1.QL_OuterPackCount = 4;
							quarantineExDocLine1.QL_OuterPackType = "BG";
							quarantineExDocLine1.QL_OuterPackAccuracy = "4";
							quarantineExDocLine1.QL_OuterPackWeight = 5m;
							quarantineExDocLine1.QL_OuterPackWeightUnit = "GRM";
						}

						//Intermediate Pack
						{
							quarantineExDocLine1.QL_IntermediatePackCount = 6;
							quarantineExDocLine1.QL_IntermediatePackType = "BL";
							quarantineExDocLine1.QL_IntermediatePackAccuracy = "3";
							quarantineExDocLine1.QL_IntermediatePackWeight = 7m;
							quarantineExDocLine1.QL_IntermediatePackWeightUnit = "MTN";
						}

						//Inner Pack
						{
							quarantineExDocLine1.QL_InnerPackCount = 8;
							quarantineExDocLine1.QL_InnerPackType = "BO";
							quarantineExDocLine1.QL_InnerPackAccuracy = "4";
							quarantineExDocLine1.QL_InnerPackWeight = 9m;
							quarantineExDocLine1.QL_InnerPackWeightUnit = "TN";
						}
					}
				}

				//RFP Details
				{
					//Product Details
					{
						quarantineExDocLine1.QL_ProductType = "AA";
						quarantineExDocLine1.QL_SupplimentaryCode = "A";
						quarantineExDocLine1.QL_PackType = "BG";
						quarantineExDocLine1.QL_PreservationType = "C";
						quarantineExDocLine1.QL_CutCode = "4320";
						quarantineExDocLine1.QL_ProductDescriptionLocationQualifier = "AUSTRALIAN";
						quarantineExDocLine1.QL_ProductDescriptionQualityQualifier = "2";
						quarantineExDocLine1.QL_NatureOfCommodity = "BP";
						quarantineExDocLine1.QL_AddtionalDeclarationComments = "AddtionalDeclarationComments";
						invoiceLine1.JI_RelatedExportPermitNumber = "2";
						invoiceLine1.JI_RelatedExportPermitAuthority = "AFM";
						invoiceLine1.JI_RelatedExportPermitDate = new ZDateTime(2019, 1, 14);
						invoiceLine1.JI_Drawback = ZBool.True;
						quarantineExDocLine1.QL_ClientLineItemID = "11";
						quarantineExDocLine1.QL_TreatmentType = "BI";
					}
				}

				//RFP Process
				{
					//Processing Details
					{
						var process1 = quarantineExDocLine1.Processes.AddNew();
						process1.EE_ProcessingType = "AQ";
						process1.EE_TreatmentInfo = "TreatmentInfo";
						process1.EE_AuthorisationEstablishmentID = "12345";
						process1.EE_StartDate = new ZDateTime(2019, 1, 14);
						process1.EE_EndDate = new ZDateTime(2019, 1, 15);
						process1.EE_Depuration = new ZDateTime(2019, 1, 14);
						process1.EE_HarvestArea = "T";
						process1.EE_InspectionRequestedDate = new ZDateTime(2019, 1, 14);
						process1.EE_LeaseNumber = "L";
						process1.EE_TreatmentCode = "ACID";

						quarantineExDocLine1.QL_UseByStart = new ZDateTime(2019, 1, 14);
						quarantineExDocLine1.QL_UseByEnd = new ZDateTime(2019, 1, 15);
					}
				}

				//RFP Certificates
				{
					//Product Description
					{
						quarantineExDocLine1.QL_MeatInspectionDescription = "MeatInspectionDescription";
						quarantineExDocLine1.QL_AddtionalProductDescription = "AddtionalProductDescription";
						quarantineExDocLine1.QL_CommercialProductDescription = "CommercialProductDescription";
						quarantineExDocLine1.QL_HealthCertificateDescription = "HealthCertificateDescription";
					}

					//Certificate
					{
						quarantineExDocLine1.QL_HCFormatRequested = "HCFormat";
						quarantineExDocLine1.QL_ExtraCertificate = "ExtraCertificate";
						quarantineExDocLine1.QL_HCFormatAllocated = "H12";
						quarantineExDocLine1.QL_HCNumber = "925783";
						quarantineExDocLine1.QL_SendHCDesc = ZBool.False;
						quarantineExDocLine1.QL_ImportAuthorityCode = "ImportAuthorityCode";
					}
				}

				//RFP Analysis
				{
					//Fish
					{
						quarantineExDocLine1.QL_DrainedWeight = 1m;
						quarantineExDocLine1.QL_DrainedWeightUnit = "CGM";
						quarantineExDocLine1.QL_CatchStartDate = new ZDateTime(2019, 1, 14);
						quarantineExDocLine1.QL_CatchEndDate = new ZDateTime(2019, 1, 15);
					}

					//Dairy
					{
						quarantineExDocLine1.QL_PercentOfMilkProtein = 1m;
						quarantineExDocLine1.QL_PercentOfMilkFat = 2m;
						quarantineExDocLine1.QL_TotalWeightOfMilkProteinInMixtures = 3m;
						quarantineExDocLine1.QL_TotalWeightOfMilkFatInMixtures = 4m;
						quarantineExDocLine1.QL_IMA1SerialNumber = "1";
						quarantineExDocLine1.QL_IMA1QuotaYear = "2";
						quarantineExDocLine1.QL_IMA1ProductDesciption = "3";
					}

					//Horticulture
					{
						quarantineExDocLine1.QL_GrowerNumber = "4";
					}

					//Skins And Hides
					{
						quarantineExDocLine1.QL_SaltingDate = new ZDateTime(2019, 1, 15);
					}
				}

				//RFP Meat
				{
					//Meat
					{
						quarantineExDocLine1.QL_ChemicalLeanPercentage = 1;
						quarantineExDocLine1.QL_BeefVealWeightAmount = 2m;
						quarantineExDocLine1.QL_LabelApprovalNumber = "123";
						quarantineExDocLine1.QL_LabelApprovalIndicator = ZBool.True;
						quarantineExDocLine1.QL_UngradedProductIndicator = ZBool.True;
						quarantineExDocLine1.QL_HalalProductIndicator = ZBool.True;
						quarantineExDocLine1.QL_DominantProduct = "BEEF";
						quarantineExDocLine1.QL_AdditionalProducts = "BEER";
					}
				}

				//RFP Statements
				{
					//Statement Numbers
					{
						quarantineExDocLine1.QL_StatementNumber1 = new ZShort(1);
						quarantineExDocLine1.QL_StatementNumber2 = new ZShort(2);
						quarantineExDocLine1.QL_StatementNumber3 = new ZShort(3);
						quarantineExDocLine1.QL_StatementNumber4 = new ZShort(4);
						quarantineExDocLine1.QL_StatementNumber5 = new ZShort(5);
					}

					//Statement Text
					{
						quarantineExDocLine1.QL_StatementText = "StatementText";
					}
				}

				//RFP Numbers
				{
					var rfpNumber = invoiceLine1.RFPNumbers.AddNew();
					rfpNumber.ZA_RFPNumber = "TEST1";
					rfpNumber.ZA_RFPLine = 1;
					rfpNumber.ZA_RFPNetQuantity = 1m;
					rfpNumber.ZA_RFPQtyUM = "TN";
					rfpNumber.ZA_RFPPackCount = 2m;
					rfpNumber.ZA_RFPPackType = "BG";
				}
			}

			Factory.Save();

			invoiceLineViewCollection.CopyLastLineDetailsToNewLines = true;
			var clonedInvoiceLine = invoiceLineViewCollection.AddNew();
			var clonedQuarantineExDocLine = clonedInvoiceLine.QuarantineExDocLine;
			CombineAssertions(() =>
			{
				//Weights
				AssertEquals("Net Quantity", 1m, clonedQuarantineExDocLine.QL_NetQuantity);
				AssertEquals("Net Quantity UQ", "BIL", clonedQuarantineExDocLine.QL_NetQuantityUnit);
				AssertEquals("Net Imperial Weight", 2m, clonedQuarantineExDocLine.QL_ImperialNetWeight);
				AssertEquals("Net Imperial Weight UQ", "CWI", clonedQuarantineExDocLine.QL_ImperialNetWeightUnit);
				AssertEquals("Gross Metric Weight", 3m, clonedQuarantineExDocLine.QL_GrossMetricWeight);
				AssertEquals("Gross Metric Weight UQ", "CU", clonedQuarantineExDocLine.QL_GrossMetricWeightUnit);

				//Shipping Marks and Batch Code
				AssertEquals("Marks", "ShippingMarks", clonedQuarantineExDocLine.QL_ShippingMarks);
				AssertEquals("Batch", "BatchCode", clonedQuarantineExDocLine.QL_BatchCode);

				//Outer Pack
				AssertEquals("Outer Pack|Pack Count", 4, clonedQuarantineExDocLine.QL_OuterPackCount);
				AssertEquals("Outer Pack|Pack Type", "BG", clonedQuarantineExDocLine.QL_OuterPackType);
				AssertEquals("Outer Pack|Accuracy", "4", clonedQuarantineExDocLine.QL_OuterPackAccuracy);
				AssertEquals("Outer Pack|Pack Weight", 5m, clonedQuarantineExDocLine.QL_OuterPackWeight);
				AssertEquals("Outer Pack|Pack Weight UQ", "GRM", clonedQuarantineExDocLine.QL_OuterPackWeightUnit);

				//Intermediate Pack
				AssertEquals("Intermediate Pack|Pack Count", 6, clonedQuarantineExDocLine.QL_IntermediatePackCount);
				AssertEquals("Intermediate Pack|Pack Type", "BL", clonedQuarantineExDocLine.QL_IntermediatePackType);
				AssertEquals("Intermediate Pack|Accuracy", "3", clonedQuarantineExDocLine.QL_IntermediatePackAccuracy);
				AssertEquals("Intermediate Pack|Pack Weight", 7M, clonedQuarantineExDocLine.QL_IntermediatePackWeight);
				AssertEquals("Intermediate Pack|Pack Weight UQ", "MTN", clonedQuarantineExDocLine.QL_IntermediatePackWeightUnit);

				//Inner Pack
				AssertEquals("Inner Pack|Pack Count", 8, clonedQuarantineExDocLine.QL_InnerPackCount);
				AssertEquals("Inner Pack|Pack Type", "BO", clonedQuarantineExDocLine.QL_InnerPackType);
				AssertEquals("Inner Pack|Accuracy", "4", clonedQuarantineExDocLine.QL_InnerPackAccuracy);
				AssertEquals("Inner Pack|Pack Weight", 9m, clonedQuarantineExDocLine.QL_InnerPackWeight);
				AssertEquals("Inner Pack|Pack Weight UQ", "TN", clonedQuarantineExDocLine.QL_InnerPackWeightUnit);

				//Product Details
				AssertEquals("Product", "AA", clonedQuarantineExDocLine.QL_ProductType);
				AssertEquals("Supplementary Code", "A", clonedQuarantineExDocLine.QL_SupplimentaryCode);
				AssertEquals("Pack Type", "BG", clonedQuarantineExDocLine.QL_PackType);
				AssertEquals("Preservation", "C", clonedQuarantineExDocLine.QL_PreservationType);
				AssertEquals("Cut Code", "4320", clonedQuarantineExDocLine.QL_CutCode);
				AssertEquals("Product Location", "AUSTRALIAN", clonedQuarantineExDocLine.QL_ProductDescriptionLocationQualifier);
				AssertEquals("Product Quality", "2", clonedQuarantineExDocLine.QL_ProductDescriptionQualityQualifier);
				AssertEquals("Nature of Commodity", "BP", clonedQuarantineExDocLine.QL_NatureOfCommodity);
				AssertEquals("Additional Declaration", "AddtionalDeclarationComments", clonedQuarantineExDocLine.QL_AddtionalDeclarationComments);
				AssertEquals("Related Export Permit Number", "2", clonedInvoiceLine.JI_RelatedExportPermitNumber);
				AssertEquals("Related Export Permit Authority", "AFM", clonedInvoiceLine.JI_RelatedExportPermitAuthority);
				AssertEquals("Related Export Permit Date", new ZDateTime(2019, 1, 14), clonedInvoiceLine.JI_RelatedExportPermitDate);
				AssertEquals("Drawback Ind.", ZBool.True, clonedInvoiceLine.JI_Drawback);
				AssertEquals("Client Line Item ID", "11", clonedQuarantineExDocLine.QL_ClientLineItemID);
				AssertEquals("Treatment Type", "BI", clonedQuarantineExDocLine.QL_TreatmentType);

				//Processing Details
				AssertEquals("Process Count", 1, clonedQuarantineExDocLine.Processes.Count);
				var clonedProcess = clonedQuarantineExDocLine.Processes[0];
				AssertEquals("Processing Type", "AQ", clonedProcess.EE_ProcessingType);
				AssertEquals("Processing Establishment", quarantineExDocLine1.Processes[0].Address.PK, clonedProcess.EE_E2_Address);
				AssertEquals("ID", "12345", clonedProcess.EE_AuthorisationEstablishmentID);
				AssertEquals("Start Date", new ZDateTime(2019, 1, 14), clonedProcess.EE_StartDate);
				AssertEquals("End Date", new ZDateTime(2019, 1, 15), clonedProcess.EE_EndDate);
				AssertEquals("Depuration", new ZDateTime(2019, 1, 14), clonedProcess.EE_Depuration);
				AssertEquals("Harvest Area", "T", clonedProcess.EE_HarvestArea);
				AssertEquals("Inspection Requested Date", new ZDateTime(2019, 1, 14), clonedProcess.EE_InspectionRequestedDate);
				AssertEquals("Lease Number", "L", clonedProcess.EE_LeaseNumber);
				AssertEquals("Treatment Code", "ACID", clonedProcess.EE_TreatmentCode);
				AssertEquals("Treatment Info", "TreatmentInfo", clonedProcess.EE_TreatmentInfo);
				AssertEquals("Use By Start", new ZDateTime(2019, 1, 14), clonedQuarantineExDocLine.QL_UseByStart);
				AssertEquals("Use By End", new ZDateTime(2019, 1, 15), clonedQuarantineExDocLine.QL_UseByEnd);

				//Product Description
				AssertEquals("Line Item (Inspection Description)", "MeatInspectionDescription", clonedQuarantineExDocLine.QL_MeatInspectionDescription);
				AssertEquals("Addtional", "AddtionalProductDescription", clonedQuarantineExDocLine.QL_AddtionalProductDescription);
				AssertEquals("Commercial", "CommercialProductDescription", clonedQuarantineExDocLine.QL_CommercialProductDescription);
				AssertEquals("Health Certificate", "HealthCertificateDescription", clonedQuarantineExDocLine.QL_EffectiveHealthCertificateDescription);

				//Certificate
				AssertEquals("Format Requested", "HCFormat", clonedQuarantineExDocLine.QL_HCFormatRequested);
				AssertEquals("Extra Format Requested", "ExtraCertificate", clonedQuarantineExDocLine.QL_ExtraCertificate);
				AssertEquals("Format Allocated", ZString.Empty, clonedQuarantineExDocLine.QL_HCFormatAllocated);
				AssertEquals("Certificate Number", ZString.Empty, clonedQuarantineExDocLine.QL_HCNumber);
				AssertEquals("Send Override HC Description", ZBool.False, clonedQuarantineExDocLine.QL_SendHCDesc);
				AssertEquals("Import Authority Code", "ImportAuthorityCode", clonedQuarantineExDocLine.QL_ImportAuthorityCode);

				//Fish
				AssertEquals("Drained Weight", 1m, clonedQuarantineExDocLine.QL_DrainedWeight);
				AssertEquals("Drained Weight UQ", "CGM", clonedQuarantineExDocLine.QL_DrainedWeightUnit);
				AssertEquals("Catch start date", new ZDateTime(2019, 1, 14), clonedQuarantineExDocLine.QL_CatchStartDate);
				AssertEquals("Catch end date", new ZDateTime(2019, 1, 15), clonedQuarantineExDocLine.QL_CatchEndDate);

				//Dairy
				AssertEquals("Milk Protein %", 1m, clonedQuarantineExDocLine.QL_PercentOfMilkProtein);
				AssertEquals("Milk Fat %", 2m, clonedQuarantineExDocLine.QL_PercentOfMilkFat);
				AssertEquals("Total Weight Milk Protein", 3m, clonedQuarantineExDocLine.QL_TotalWeightOfMilkProteinInMixtures);
				AssertEquals("Total Weight Milk Fat", 4m, clonedQuarantineExDocLine.QL_TotalWeightOfMilkFatInMixtures);
				AssertEquals("IMA1 Serial Number", "1", clonedQuarantineExDocLine.QL_IMA1SerialNumber);
				AssertEquals("IMA1 Quota Year", "2", clonedQuarantineExDocLine.QL_IMA1QuotaYear);
				AssertEquals("IMA1 Product Description", "3", clonedQuarantineExDocLine.QL_IMA1ProductDesciption);

				//Horticulture
				AssertEquals("Grower Number", "4", clonedQuarantineExDocLine.QL_GrowerNumber);

				//Skins And Hides
				AssertEquals("Salting Date", new ZDateTime(2019, 1, 15), clonedQuarantineExDocLine.QL_SaltingDate);

				//Meat
				AssertEquals("Chemical/Lean", 1, clonedQuarantineExDocLine.QL_ChemicalLeanPercentage);
				AssertEquals("Beef/Veal Weight", 2m, clonedQuarantineExDocLine.QL_BeefVealWeightAmount);
				AssertEquals("Label Approval No", "123", clonedQuarantineExDocLine.QL_LabelApprovalNumber);
				AssertEquals("Label Approval Indicator", ZBool.True, clonedQuarantineExDocLine.QL_LabelApprovalIndicator);
				AssertEquals("Ungraded Product", ZBool.True, clonedQuarantineExDocLine.QL_UngradedProductIndicator);
				AssertEquals("Halal Product Indicator", ZBool.True, clonedQuarantineExDocLine.QL_HalalProductIndicator);
				AssertEquals("Dominant Product", "BEEF", clonedQuarantineExDocLine.QL_DominantProduct);
				AssertEquals("Additional Products", "BEER", clonedQuarantineExDocLine.QL_AdditionalProducts);

				//Statement Numbers
				AssertEquals("Number 1", new ZShort(1), clonedQuarantineExDocLine.QL_StatementNumber1);
				AssertEquals("Number 2", new ZShort(2), clonedQuarantineExDocLine.QL_StatementNumber2);
				AssertEquals("Number 3", new ZShort(3), clonedQuarantineExDocLine.QL_StatementNumber3);
				AssertEquals("Number 4", new ZShort(4), clonedQuarantineExDocLine.QL_StatementNumber4);

				//Statement Text
				AssertEquals("Statement Text", "StatementText", clonedQuarantineExDocLine.QL_StatementText);

				//RFP Numbers
				AssertEquals("RFP Numbers Count", 1, clonedInvoiceLine.RFPNumbers.Count);
				var clonedRFPNumber = clonedInvoiceLine.RFPNumbers[0];
				AssertEquals("RFP Number", "TEST1", clonedRFPNumber.ZA_RFPNumber);
				AssertEquals("RFP Line", 1, clonedRFPNumber.ZA_RFPLine);
				AssertEquals("Net Quantity", 1m, clonedRFPNumber.ZA_RFPNetQuantity);
				AssertEquals("UQ", "TN", clonedRFPNumber.ZA_RFPQtyUM);
				AssertEquals("Pack Count", 2m, clonedRFPNumber.ZA_RFPPackCount);
				AssertEquals("Pack Type", "BG", clonedRFPNumber.ZA_RFPPackType);
			});
		}

		public void TestCopyLastLineDetailsIncludesCustomsUQ()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV 1";

			var invoiceLineViewCollection = declaration.FilteredInvoiceLines;
			var invoiceLine = invoiceLineViewCollection.AddNew();
			invoiceLine.JI_Calc_Invoice = invoiceHeader.JZ_InvoiceNumber;
			invoiceLine.JI_Tariff = "9015.90.01";
			invoiceLine.JI_Description = "Line Tariff/Product Description";
			invoiceLine.JI_CustomsQuantity = 230;
			invoiceLine.JI_CustomsUnitQty = "NR";

			Factory.Save();

			invoiceLineViewCollection.CopyLastLineDetailsToNewLines = true;
			var clonedInvoiceLine = invoiceLineViewCollection.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("Invoice Number", "INV 1", clonedInvoiceLine.JI_Calc_Invoice);
				AssertEquals("Tariff", "9015.90.01", clonedInvoiceLine.JI_Tariff);
				AssertEquals("Tariff Description", "Line Tariff/Product Description", clonedInvoiceLine.JI_Description);
				AssertEquals("Customs Quantity", 230m, clonedInvoiceLine.JI_CustomsQuantity);
				AssertEquals("Customs UQ on new line should have the copied value from the previous line", "NR", clonedInvoiceLine.JI_CustomsUnitQty);
			});
		}

		public void TestLineNoDoesntGetOverwrittenWhenLoaded()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV 1";

			JobComInvoiceLine line1 = declaration.FilteredInvoiceLines.AddNew();
			line1.JI_Calc_Invoice = invoiceHeader.JZ_InvoiceNumber;
			line1.JI_LinePrice = 100m;

			JobComInvoiceLine line2 = declaration.FilteredInvoiceLines.AddNew();
			line2.JI_Calc_Invoice = invoiceHeader.JZ_InvoiceNumber;
			line2.JI_LinePrice = 200m;

			AssertEquals("Line1 Line number", (short)1, line1.JI_LineNo);
			AssertEquals("Line2 Line number", (short)2, line2.JI_LineNo);

			declaration.FilteredInvoiceLines.Sort(JobComInvoiceLine.Schema.JI_LinePrice, ListSortDirection.Descending);
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobDeclaration loadedDeclaration = newFactory.New<JobDeclaration>();
			foreach (JobComInvoiceLine invoiceLine in loadedDeclaration.FilteredInvoiceLines)
			{
				if (invoiceLine.JI_LinePrice == 100m)
				{
					AssertEquals("Line1 LineNumber", (short)1, invoiceLine.JI_LineNo);
				}
				else if (invoiceLine.JI_LinePrice == 200m)
				{
					AssertEquals("Line2 LineNumber", (short)2, invoiceLine.JI_LineNo);
				}
			}
		}

		public void TestHasUnclassified()
		{
			line1.JI_Tariff = "111";
			line2.JI_Tariff = "222";
			//Line3.JI_Tariff = "";

			Assert("One unclassified line expected", declaration.FilteredInvoiceLines.HasUnclassifiedLines);

			line3.JI_Tariff = "333";
			Assert("All classified", !header.JobComInvoiceLines.HasUnclassifiedLines());
		}

		public void TestSortNotAffectLineNO()
		{
			line3.JI_InvoiceQuantity = 3;
			line2.JI_InvoiceQuantity = 2;
			line1.JI_InvoiceQuantity = 1;

			header.JobComInvoiceLines.Sort(JobComInvoiceLine.Schema.JI_InvoiceQuantity, ListSortDirection.Descending);

			AssertEquals("Line No", (short)1, line1.JI_LineNo);
			AssertEquals("Line No", (short)2, line2.JI_LineNo);
			AssertEquals("Line No", (short)3, line3.JI_LineNo);
		}

		public void TestUpdateLineNumbersNewAdded()
		{
			JobComInvoiceLine line4 = header.JobComInvoiceLines.AddNew();
			AssertEquals("Line No", (short)4, line4.JI_LineNo);
		}

		public void TestUpdateLineNumbersDeleted()
		{
			line2.Delete();
			AssertEquals("Line No", (short)1, line1.JI_LineNo);
			AssertEquals("Line No", (short)2, line3.JI_LineNo);
		}

		public void TestUpdateLineNumbersDeletedAfterSorting()
		{
			using (line1.InvoiceHeader.GetLineNumberRenumberingSuspender())
			{
				line3.JI_InvoiceQuantity = 3;
				line2.JI_InvoiceQuantity = 2;
				line1.JI_InvoiceQuantity = 1;
			}
			header.JobComInvoiceLines.Sort(JobComInvoiceLine.Schema.JI_InvoiceQuantity, ListSortDirection.Descending);

			line2.Delete();
			AssertEquals("Line No", (short)1, line1.JI_LineNo);
			AssertEquals("Line No", (short)2, line3.JI_LineNo);
		}

		public void TestRecordWithInvoiceChangeStaysInList()
		{
			header.JZ_InvoiceNumber = "INV 1";
			JobComInvoiceLine newLine = declaration.FilteredInvoiceLines.AddNew();
			AssertEquals("Total Invoice Line Count", 4, declaration.FilteredInvoiceLines.Count);
			JobComInvoiceHeader newHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			newHeader.JZ_InvoiceNumber = "INV 2";
			newLine.JI_Calc_Invoice = newHeader.JZ_InvoiceNumber;
			AssertEquals("Total Invoice Line Count", 4, declaration.FilteredInvoiceLines.Count);
		}

		public void TestBalance()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "1000";
			invoiceHeader.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			invoiceHeader.JZ_InvoiceAmount = 1000m;

			JobComInvoiceLine line1 = declaration.FilteredInvoiceLines.AddNew();
			line1.JI_Calc_Invoice = invoiceHeader.JZ_InvoiceNumber;
			line1.JI_LinePrice = 1000m;
			AssertEquals("Balance of invoice header should be 0", 0m, invoiceHeader.JZ_Calc_Balance);
		}

		public void TestHasUnclassifiedLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;

			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "1";

			JobComInvoiceLine line1 = declaration.FilteredInvoiceLines.AddNew();
			JobComInvoiceLine line2 = declaration.FilteredInvoiceLines.AddNew();
			line1.JI_Calc_Invoice = "1";
			line1.JI_Tariff = "0000.00.00";

			line2.JI_Calc_Invoice = "1";

			AssertEquals("There is one unclassified line", true, declaration.FilteredInvoiceLines.HasUnclassifiedLines);
		}

		public void TestTrailerLineNonZeroPriceMessageError()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			JobComInvoiceLine line1 = declaration.FilteredInvoiceLines.AddNew();
			line1.JI_LinePrefix = "P";
			JobComInvoiceLine line2 = declaration.FilteredInvoiceLines.AddNew();
			line2.JI_LinePrice = 1;
			line2.JI_LinePrefix = JobComInvoiceLine.LinePrefixString.Trailer;
			line2.RunPreSaveValidation();
			AssertEquals("Line2 should be a trailer", JobComInvoiceLine.LinePrefixString.Trailer, line2.JI_LinePrefix);
			AssertEquals("Line Price should be 0", true, line2.JI_LinePriceInfo.HasMessageErrors());
		}

		public void TestDontDefaultDrawbackForImports()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceLine line1 = declaration.FilteredInvoiceLines.AddNew();
			JobComInvoiceLine line2 = declaration.FilteredInvoiceLines.AddNew();

			AssertEquals("Drawback", false, line1.JI_Drawback);
			AssertEquals("Drawback", false, line2.JI_Drawback);
		}

		public void TestDefaultDrawbackForExports()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			JobComInvoiceLine line1 = declaration.FilteredInvoiceLines.AddNew();
			JobComInvoiceLine line2 = declaration.FilteredInvoiceLines.AddNew();

			AssertEquals("Drawback", true, line1.JI_Drawback);
			AssertEquals("Drawback", true, line2.JI_Drawback);
		}

		//P, T1, T2: T2 should have a message error
		public void TestValidateParentTrailerPair1()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = declaration.FilteredInvoiceLines.AddNew();
			line1.JI_Calc_Invoice = header.JZ_InvoiceNumber;
			JobComInvoiceLine line2 = declaration.FilteredInvoiceLines.AddNew();
			line2.JI_Calc_Invoice = header.JZ_InvoiceNumber;
			JobComInvoiceLine line3 = declaration.FilteredInvoiceLines.AddNew();
			line3.JI_Calc_Invoice = header.JZ_InvoiceNumber;

			line2.JI_LinePrefix = JobComInvoiceLine.LinePrefixString.Parent;
			AssertEquals("PreCondition:Line 3 trailer", JobComInvoiceLine.LinePrefixString.Trailer, line3.JI_LinePrefix);

			line1.JI_LinePrefix = JobComInvoiceLine.LinePrefixString.Parent;
			AssertEquals("PreCondition:Line 2 becomes a trailer", JobComInvoiceLine.LinePrefixString.Trailer, line2.JI_LinePrefix);
			AssertEquals("PreCondition:Line 3 is still a trailer", JobComInvoiceLine.LinePrefixString.Trailer, line3.JI_LinePrefix);
			line3.RunPreSaveValidation();
			AssertEquals("Line 3 should have a message error", true, line3.JI_LinePrefixInfo.HasMessageErrors());
			AssertEquals("Line 3 Line prefix should be editible", false, line3.JI_LinePrefixInfo.ReadOnly);
		}

		//T1, T2: T1 should have a message error
		public void TestValidateParentTrailerPair2()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = declaration.FilteredInvoiceLines.AddNew();
			line1.JI_Calc_Invoice = header.JZ_InvoiceNumber;
			JobComInvoiceLine line2 = declaration.FilteredInvoiceLines.AddNew();
			line2.JI_Calc_Invoice = header.JZ_InvoiceNumber;

			line1.JI_LinePrefix = JobComInvoiceLine.LinePrefixString.Parent;
			AssertEquals("PreCondition:Line 2 trailer", JobComInvoiceLine.LinePrefixString.Trailer, line2.JI_LinePrefix);

			line1.JI_LinePrefix = JobComInvoiceLine.LinePrefixString.Trailer;
			AssertEquals("Line1 should have a message error", true, line1.JI_LinePrefixInfo.HasMessageErrors());
			AssertEquals("Line1 line prefix should be editible", false, line1.JI_LinePrefixInfo.ReadOnly);

			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobComInvoiceLine loadedLine1 = newFactory.Load<JobComInvoiceLine>(line1.PK);
			AssertEquals("Line1 Line prefix should be editible", false, loadedLine1.JI_LinePrefixInfo.ReadOnly);
		}

		public void TestSortByLineNo()
		{
			JobDeclaration declaration = SetUpRecordsForParentTrailerSort();
			JobComInvoiceLine line1 = declaration.FilteredInvoiceLines[0];
			JobComInvoiceLine line2 = declaration.FilteredInvoiceLines[1];
			JobComInvoiceLine line3 = declaration.FilteredInvoiceLines[2];
			JobComInvoiceLine line4 = declaration.FilteredInvoiceLines[3];
			JobComInvoiceLine line5 = declaration.FilteredInvoiceLines[4];
			JobComInvoiceLine line6 = declaration.FilteredInvoiceLines[5];
			JobComInvoiceLine line7 = declaration.FilteredInvoiceLines[6];

			declaration.FilteredInvoiceLines.Sort(JobComInvoiceLine.Schema.JI_LineNo, ListSortDirection.Ascending);
			AssertEquals(line1, declaration.FilteredInvoiceLines[0]);
			AssertEquals(line2, declaration.FilteredInvoiceLines[1]);
			AssertEquals(line3, declaration.FilteredInvoiceLines[2]);
			AssertEquals(line6, declaration.FilteredInvoiceLines[3]);
			AssertEquals(line7, declaration.FilteredInvoiceLines[4]);
			AssertEquals(line4, declaration.FilteredInvoiceLines[5]);
			AssertEquals(line5, declaration.FilteredInvoiceLines[6]);

			declaration.FilteredInvoiceLines.Sort(JobComInvoiceLine.Schema.JI_LineNo, ListSortDirection.Descending);
			AssertEquals(line4, declaration.FilteredInvoiceLines[0]);//Parent
			AssertEquals(line5, declaration.FilteredInvoiceLines[1]);//Trailer
			AssertEquals(line7, declaration.FilteredInvoiceLines[2]);
			AssertEquals(line6, declaration.FilteredInvoiceLines[3]);
			AssertEquals(line3, declaration.FilteredInvoiceLines[4]);
			AssertEquals(line1, declaration.FilteredInvoiceLines[5]);//Parent
			AssertEquals(line2, declaration.FilteredInvoiceLines[6]);//Trailer
		}

		public void TestSortByInvoiceNumber()
		{
			JobDeclaration declaration = SetUpRecordsForParentTrailerSort();
			JobComInvoiceLine line1 = declaration.FilteredInvoiceLines[0];
			JobComInvoiceLine line2 = declaration.FilteredInvoiceLines[1];
			JobComInvoiceLine line3 = declaration.FilteredInvoiceLines[2];
			JobComInvoiceLine line4 = declaration.FilteredInvoiceLines[3];
			JobComInvoiceLine line5 = declaration.FilteredInvoiceLines[4];
			JobComInvoiceLine line6 = declaration.FilteredInvoiceLines[5];
			JobComInvoiceLine line7 = declaration.FilteredInvoiceLines[6];

			declaration.FilteredInvoiceLines.Sort(JobComInvoiceLine.Schema.JI_Calc_Invoice, ListSortDirection.Ascending);
			AssertEquals(line1, declaration.FilteredInvoiceLines[0]);
			AssertEquals(line2, declaration.FilteredInvoiceLines[1]);
			AssertEquals(line3, declaration.FilteredInvoiceLines[2]);
			AssertEquals(line6, declaration.FilteredInvoiceLines[3]);
			AssertEquals(line7, declaration.FilteredInvoiceLines[4]);
			AssertEquals(line4, declaration.FilteredInvoiceLines[5]);
			AssertEquals(line5, declaration.FilteredInvoiceLines[6]);

			declaration.FilteredInvoiceLines.Sort(JobComInvoiceLine.Schema.JI_Calc_Invoice, ListSortDirection.Descending);
			AssertEquals(line4, declaration.FilteredInvoiceLines[0]);//Parent
			AssertEquals(line5, declaration.FilteredInvoiceLines[1]);//Trailer
			AssertEquals(line7, declaration.FilteredInvoiceLines[2]);
			AssertEquals(line6, declaration.FilteredInvoiceLines[3]);
			AssertEquals(line3, declaration.FilteredInvoiceLines[4]);
			AssertEquals(line1, declaration.FilteredInvoiceLines[5]);//Parent
			AssertEquals(line2, declaration.FilteredInvoiceLines[6]);//Trailer
		}

		public void TestSortByLinePrice()
		{
			JobDeclaration declaration = SetUpRecordsForParentTrailerSort();
			JobComInvoiceLine line1 = declaration.FilteredInvoiceLines[0];
			JobComInvoiceLine line2 = declaration.FilteredInvoiceLines[1];
			JobComInvoiceLine line3 = declaration.FilteredInvoiceLines[2];
			JobComInvoiceLine line4 = declaration.FilteredInvoiceLines[3];
			JobComInvoiceLine line5 = declaration.FilteredInvoiceLines[4];
			JobComInvoiceLine line6 = declaration.FilteredInvoiceLines[5];
			JobComInvoiceLine line7 = declaration.FilteredInvoiceLines[6];

			declaration.FilteredInvoiceLines.Sort(JobComInvoiceLine.Schema.JI_LinePrice, ListSortDirection.Ascending);
			AssertEquals(line1, declaration.FilteredInvoiceLines[0]);//Parent
			AssertEquals(line2, declaration.FilteredInvoiceLines[1]);//Trailer
			AssertEquals(line3, declaration.FilteredInvoiceLines[2]);
			AssertEquals(line4, declaration.FilteredInvoiceLines[3]);//Parent
			AssertEquals(line5, declaration.FilteredInvoiceLines[4]);//Trailer
			AssertEquals(line6, declaration.FilteredInvoiceLines[5]);
			AssertEquals(line7, declaration.FilteredInvoiceLines[6]);

			declaration.FilteredInvoiceLines.Sort(JobComInvoiceLine.Schema.JI_LinePrice, ListSortDirection.Descending);
			AssertEquals(line7, declaration.FilteredInvoiceLines[0]);
			AssertEquals(line6, declaration.FilteredInvoiceLines[1]);
			AssertEquals(line4, declaration.FilteredInvoiceLines[2]);
			AssertEquals(line5, declaration.FilteredInvoiceLines[3]);
			AssertEquals(line3, declaration.FilteredInvoiceLines[4]);
			AssertEquals(line1, declaration.FilteredInvoiceLines[5]);//Parent
			AssertEquals(line2, declaration.FilteredInvoiceLines[6]);//Trailer
		}

		public void TestSortByLinePrefix()
		{
			JobDeclaration declaration = SetUpRecordsForParentTrailerSort();
			JobComInvoiceLine line1 = declaration.FilteredInvoiceLines[0];
			JobComInvoiceLine line2 = declaration.FilteredInvoiceLines[1];
			JobComInvoiceLine line3 = declaration.FilteredInvoiceLines[2];
			JobComInvoiceLine line4 = declaration.FilteredInvoiceLines[3];
			JobComInvoiceLine line5 = declaration.FilteredInvoiceLines[4];
			JobComInvoiceLine line6 = declaration.FilteredInvoiceLines[5];
			JobComInvoiceLine line7 = declaration.FilteredInvoiceLines[6];

			declaration.FilteredInvoiceLines.Sort(JobComInvoiceLine.Schema.JI_LinePrefix, ListSortDirection.Ascending);
			AssertEquals(line3, declaration.FilteredInvoiceLines[0]);
			AssertEquals(line6, declaration.FilteredInvoiceLines[1]);
			AssertEquals(line7, declaration.FilteredInvoiceLines[2]);
			AssertEquals(line1, declaration.FilteredInvoiceLines[3]);//Parent
			AssertEquals(line2, declaration.FilteredInvoiceLines[4]);//Trailer
			AssertEquals(line4, declaration.FilteredInvoiceLines[5]);//Parent
			AssertEquals(line5, declaration.FilteredInvoiceLines[6]);//Trailer

			declaration.FilteredInvoiceLines.Sort(JobComInvoiceLine.Schema.JI_LinePrefix, ListSortDirection.Descending);
			AssertEquals(line1, declaration.FilteredInvoiceLines[0]);
			AssertEquals(line2, declaration.FilteredInvoiceLines[1]);
			AssertEquals(line4, declaration.FilteredInvoiceLines[2]);
			AssertEquals(line5, declaration.FilteredInvoiceLines[3]);
			AssertEquals(line3, declaration.FilteredInvoiceLines[4]);
			AssertEquals(line6, declaration.FilteredInvoiceLines[5]);
			AssertEquals(line7, declaration.FilteredInvoiceLines[6]);
		}

		/// <summary>
		/// LineNo		Invoice	 LinePrice	LinePrefix	ParentLine
		/// ------		-------	 ---------	----------	----------
		/// 1/Line1		"INV1"		1000	P			-
		/// 2/Line2		"INV1"		0		T			1/INV2
		/// 3/Line3		"INV1"		2000	-			-
		/// 1/Line4		"INV2"		3000	P			-
		/// 2/Line5		"INV2"		0		T			1/INV1
		/// 4/Line6		"INV1"		3000	-			-
		/// 5/Line7		"INV1"		5000	-			-
		/// </summary>
		JobDeclaration SetUpRecordsForParentTrailerSort()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader header1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header1.JZ_InvoiceNumber = "INV1";
			JobComInvoiceHeader header2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header2.JZ_InvoiceNumber = "INV2";

			JobComInvoiceLine line1 = declaration.FilteredInvoiceLines.AddNew();
			line1.JI_Calc_Invoice = header1.JZ_InvoiceNumber;
			line1.JI_LinePrefix = JobComInvoiceLine.LinePrefixString.Parent;
			line1.JI_LinePrice = 1000;

			JobComInvoiceLine line2 = declaration.FilteredInvoiceLines.AddNew();
			line2.JI_Calc_Invoice = header1.JZ_InvoiceNumber;
			line2.JI_LinePrefix = JobComInvoiceLine.LinePrefixString.Trailer;

			JobComInvoiceLine line3 = declaration.FilteredInvoiceLines.AddNew();
			line3.JI_Calc_Invoice = header1.JZ_InvoiceNumber;
			line3.JI_LinePrice = 2000;

			JobComInvoiceLine line4 = declaration.FilteredInvoiceLines.AddNew();
			line4.JI_Calc_Invoice = header2.JZ_InvoiceNumber;
			line4.JI_LinePrefix = JobComInvoiceLine.LinePrefixString.Parent;
			line4.JI_LinePrice = 3000;
			JobComInvoiceLine line5 = declaration.FilteredInvoiceLines.AddNew();
			line5.JI_Calc_Invoice = header2.JZ_InvoiceNumber;
			line5.JI_LinePrefix = JobComInvoiceLine.LinePrefixString.Trailer;

			JobComInvoiceLine line6 = declaration.FilteredInvoiceLines.AddNew();
			line6.JI_Calc_Invoice = header1.JZ_InvoiceNumber;
			line6.JI_LinePrice = 4000;

			JobComInvoiceLine line7 = declaration.FilteredInvoiceLines.AddNew();
			line7.JI_Calc_Invoice = header1.JZ_InvoiceNumber;
			line7.JI_LinePrice = 5000;
			return declaration;
		}

		#region Implementation

		protected JobDeclaration declaration;
		protected JobComInvoiceHeader header;
		protected JobComInvoiceLine line1;
		protected JobComInvoiceLine line2;
		protected JobComInvoiceLine line3;
		protected RefCurrency aUD;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header.JZ_InvoiceNumber = "INV1";
			line1 = header.JobComInvoiceLines.AddNew();
			line2 = header.JobComInvoiceLines.AddNew();
			line3 = header.JobComInvoiceLines.AddNew();
			aUD = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
		}

		#endregion
	}
}
