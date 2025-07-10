using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class EntryCreationStrategyBaseOnlyTest : EntryCreationStrategyTest
	{
		public void TestLineIsValidForMerge()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.EntryNumber = "123456";
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var invoiceLine2 = declaration.Invoices[0].InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			var strategy = declaration.CreateEntryCreationStrategy();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetLockNumberOfEntryLinesForRegisteredEntryConfiguration(declaration, configurationValue: true))
			{
				CombineAssertions(() =>
				{
					AssertEquals("Line1: valid", true, strategy.LineIsValidForMerge(invoiceLine));
					AssertEquals("Line2: valid", true, strategy.LineIsValidForMerge(invoiceLine2));

					invoiceLine2.JI_CL = ZGuid.Empty;
					AssertEquals("Line2: invalid", false, strategy.LineIsValidForMerge(invoiceLine2));
					AssertHasRowError("Line2 has error", invoiceLine2, invoiceLine.Validation.GetNotAllowCreateNewEntryLineErrorMessage((CusEntryHeader)entryHeader));
				});
			}
		}

		public void TestMergeKeyForLineDoesNotContainJI_RN_NKCountryOfExport()
		{
			var dec = GetJobDeclarationForTest();
			var invLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			invLine.JI_RN_NKCountryOfExport = "IE";
			var entryCreationStrategy = dec.CreateEntryCreationStrategy();
			Assert(!entryCreationStrategy.GetKeyForLine(invLine).Contains(invLine.JI_RN_NKCountryOfExport));
		}

		public void TestMergeKeyForLineContainsJI_RN_NKCountryOfExport()
		{
			var mockDec = Factory.NewMoq<JobDeclaration>();
			var dec = mockDec.Object;
			var invLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			invLine.JI_RN_NKCountryOfExport = "IE";

			var mockEntryLineConfiguration = new Mock<EntryLineConfiguration>();
			mockEntryLineConfiguration.Protected().Setup<ZBool>("MergeJI_RN_NKCountryOfExportCore", ItExpr.IsAny<JobDeclaration>()).Returns(ZBool.True);
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(dec, "GetNewEntryLineConfiguration", mockEntryLineConfiguration.Object))
			{
				var entryCreationStrategy = dec.CreateEntryCreationStrategy();
				Assert(entryCreationStrategy.GetKeyForLine(invLine).Contains(invLine.JI_RN_NKCountryOfExport));
			}
		}

		public void TestMergeKeyForLineContainsZG_CountryOfDispatch()
		{
			var dec = GetJobDeclarationForTest();
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(dec, true))
			{
				var invLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
				invLine.ZG_CountryOfDispatch = "IE";
				var entryCreationStrategy = dec.CreateEntryCreationStrategy();
				Assert(entryCreationStrategy.GetKeyForLine(invLine).Contains(invLine.ZG_CountryOfDispatch));
			}
		}

		public void TestMergeKeyForLineContainsZG_RegionOfDestination()
		{
			var dec = GetJobDeclarationForTest();
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(dec, true))
			{
				var invLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
				invLine.ZG_RegionOfDestination = "IE";
				var entryCreationStrategy = dec.CreateEntryCreationStrategy();
				Assert(entryCreationStrategy.GetKeyForLine(invLine).Contains(invLine.ZG_RegionOfDestination));
			}
		}

		public void TestMergeKeyContainsCountryOfSupplyAndDestination()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var dec = GetJobDeclarationForTest();
				var cei = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				var inv1 = dec.Invoices.AddNew();
				var invLine = inv1.JobComInvoiceLines.AddNew();
				invLine.ZG_CountryOfDestination = CountryCodes.UnitedKingdom;
				invLine.ZG_CountryOfSupply = CountryCodes.Germany;

				var mergeStrategy = dec.CreateEntryCreationStrategy();
				Assert(mergeStrategy.GetKeyForLine(invLine).Contains(invLine.ZG_CountryOfDestination));
				Assert(mergeStrategy.GetKeyForLine(invLine).Contains(invLine.ZG_CountryOfSupply));
			}
		}

		public void TestMergeKeyContainsRelatedIndicatorWhenNotUcc6AndExport()
		{
			AssertRelatedIndicatorsArePartOfTheMergeKey(isUcc6: false, isExport: true, expectedInTheHeaderMergeKey: true, expectedInTheLineMergeKey: true);
		}

		public void TestMergeKeyDoesNotContainRelatedIndicatorWhenUcc6AndExport()
		{
			AssertRelatedIndicatorsArePartOfTheMergeKey(isUcc6: true, isExport: true, expectedInTheHeaderMergeKey: false, expectedInTheLineMergeKey: false);
		}

		public void TestMergeKeyDoesNotContainRelatedIndicatorWhenUcc6AndImport()
		{
			AssertRelatedIndicatorsArePartOfTheMergeKey(isUcc6: true, isExport: false, expectedInTheHeaderMergeKey: false, expectedInTheLineMergeKey: true);
		}

		public void AssertRelatedIndicatorsArePartOfTheMergeKey(bool isUcc6, bool isExport, bool expectedInTheHeaderMergeKey, bool expectedInTheLineMergeKey)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedKingdom))
			{
				var dec = GetJobDeclarationForTest();
				dec.JE_MessageType = isExport ? MessageTypeList.Codes.Export : MessageTypeList.Codes.Import;
				var inv1 = dec.Invoices.AddNew();
				var invLine = inv1.JobComInvoiceLines.AddNew();

				var expectedRelatedIndicatorMergeKeysForLine = new IZType[]
				{
					invLine.RelatedIndicator,
					invLine.ZG_RelatedIndicator2,
					invLine.ZG_RelatedIndicator3,
					invLine.ZG_RelatedIndicator4,
				};

				var expectedRelatedIndicatorMergeKeysForHeader = new IZType[]
				{
					inv1.RelatedIndicator,
					inv1.ZG_RelatedIndicator2,
					inv1.ZG_RelatedIndicator3,
					inv1.ZG_RelatedIndicator4,
				};

				CombineAssertions(() =>
				{
					using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(dec, isUcc6))
					{
						var mergeStrategy = dec.CreateEntryCreationStrategy();
						AssertMergeKeyContainsExpectedKeys("Line Merge Key", expectedInTheLineMergeKey, expectedRelatedIndicatorMergeKeysForLine, mergeStrategy.GetKeyForLine(invLine));
						AssertMergeKeyContainsExpectedKeys("Header Merge Key", expectedInTheHeaderMergeKey, expectedRelatedIndicatorMergeKeysForHeader, mergeStrategy.GetKeyForHeader(invLine));
					}
				});
			}

			string JoinKeys(IEnumerable<IZType> keys)
			{
				return string.Join(" ,", keys.Select(x => x.ToString()));
			}

			void AssertMergeKeyContainsExpectedKeys(string assertionMessage, bool shouldBeInTheMergeKey, IEnumerable<IZType> expectedKeys, MergeKey mergeKey)
			{
				var actualValue = JoinKeys(mergeKey.Keys);
				var expectedValue = JoinKeys(expectedKeys);

				if (shouldBeInTheMergeKey)
				{
					AssertContains(assertionMessage, expectedValue, actualValue);
				}
				else
				{
					AssertNotContains(assertionMessage, expectedValue, actualValue);
				}
			}
		}

		public void TestNationalCodesCreatesTwoMergeLines()
		{
			var mockDec = Factory.NewMoq<JobDeclaration>();
			var dec = mockDec.Object;
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var inv1 = dec.Invoices.AddNew();
			var invLine1 = inv1.JobComInvoiceLines.AddNew();
			invLine1.JI_Tariff = "2203001010";
			var inv2 = dec.Invoices.AddNew();
			var invLine2 = inv2.JobComInvoiceLines.AddNew();
			invLine2.JI_Tariff = "2203001010";
			Action<bool> doMerge = (bool value) =>
			{
				var mockEntryCreationStrategy = new Mock<EntryCreationStrategy>(dec);
				mockEntryCreationStrategy.CallBase = true;
				var entryCreationStrategy = mockEntryCreationStrategy.Object;
				mockDec.Setup(m => m.CreateEntryCreationStrategy()).Returns(mockEntryCreationStrategy.Object);
				mockEntryCreationStrategy.Protected().Setup<EntryManager>("GetEntryManager").Returns(new EntryManager(dec, entryCreationStrategy));
				mockEntryCreationStrategy.Protected().Setup<bool>("ShouldProcessNationalCodesForLineMergeKey").Returns(value);
				dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			};
			CombineAssertions("ShouldProcessNationalCodesForLineMergeKey - false", () =>
			{
				invLine1.JI_NationalAdditionalCode1 = "ABCD";
				invLine2.JI_NationalAdditionalCode1 = "EFGH";
				doMerge(false);
				AssertEquals("One entry should be created because National Codes is only applicable to UCC6", 1, dec.CustomsEntryHeaders[0].AllEntryLines.Count);
			});
			CombineAssertions("ShouldProcessNationalCodesForLineMergeKey - true", () =>
			{
				invLine1.JI_NationalAdditionalCode1 = ZString.Empty;
				invLine2.JI_NationalAdditionalCode1 = ZString.Empty;
				doMerge(true);
				AssertEquals(1, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

				invLine1.JI_NationalAdditionalCode1 = "ABCD";
				invLine2.JI_NationalAdditionalCode1 = "ABCD";

				doMerge(true);
				AssertEquals(1, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

				invLine2.JI_NationalAdditionalCode1 = "EFGH";

				doMerge(true);
				AssertEquals("Two entries should be created because they have different National Codes 1", 2, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

				invLine1.JI_NationalAdditionalCode1 = "ABCD";
				invLine2.JI_NationalAdditionalCode1 = "ABCD";
				invLine1.JI_NationalAdditionalCode2 = "EFGH";
				invLine2.JI_NationalAdditionalCode2 = "EFGH";

				doMerge(true);
				AssertEquals(1, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

				invLine2.JI_NationalAdditionalCode2 = "IJKL";

				doMerge(true);
				AssertEquals("Two entries should be created because they have different National Codes 2", 2, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

				invLine2.JI_NationalAdditionalCode2 = "EFGH";
				invLine1.NationalAdditionalCodes.AsString = "IJKL,MNOP";
				invLine2.NationalAdditionalCodes.AsString = "IJKL,MNOP";

				doMerge(true);
				AssertEquals(1, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

				invLine2.NationalAdditionalCodes.AsString = "MNOP,IJKL";

				doMerge(true);
				AssertEquals("Changing order of National Additional Codes", 1, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

				invLine2.NationalAdditionalCodes.AsString = "MNO1,IJKL";

				doMerge(true);
				AssertEquals("Two entries should be created because they have different Additional National Codes", 2, dec.CustomsEntryHeaders[0].AllEntryLines.Count);
			});
		}

		public void TestMergeKeyDoesNotContainCusSupplyChainActorsWhenNotUcc6()
		{
			AssertInvoiceLineAdditionalSupplyChainActorsArePartOfTheMergeKey(isUcc6: false, expectedInTheMergeKey: false);
		}

		public void TestMergeKeyContainsCusSupplyChainActorsWhenUcc6()
		{
			AssertInvoiceLineAdditionalSupplyChainActorsArePartOfTheMergeKey(isUcc6: true, expectedInTheMergeKey: true);
		}

		public void TestAdditionalProcedureCodesCreatesTwoMergeLines()
		{
			var dec = GetJobDeclarationForTest();
			var inv1 = dec.Invoices.AddNew();
			var invLine1 = inv1.JobComInvoiceLines.AddNew();
			var invLine2 = inv1.JobComInvoiceLines.AddNew();
			var add1 = invLine1.AdditionalProcedureCodes.AddNew();
			add1.CY_Code = "AAA";
			var add2 = invLine1.AdditionalProcedureCodes.AddNew();
			add2.CY_Code = "BBB";
			var add3 = invLine2.AdditionalProcedureCodes.AddNew();
			add3.CY_Code = "BBB";
			var add4 = invLine2.AdditionalProcedureCodes.AddNew();
			add4.CY_Code = "AAA";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, dec.CustomsEntryHeaders.Count);
			AssertEquals(1, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

			var add5 = invLine2.AdditionalProcedureCodes.AddNew();
			add5.CY_Code = "CCC";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, dec.CustomsEntryHeaders.Count);
			AssertEquals(2, dec.CustomsEntryHeaders[0].AllEntryLines.Count);
		}

		public void TestDifferentCPCCodeCreatesTwoMergeLines()
		{
			var dec = GetJobDeclarationForTest();
			dec.Invoices.AddNew();
			dec.InvoiceLines.AddNew();
			var line2 = dec.InvoiceLines.AddNew();
			line2.JI_Procedure = "400000";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, dec.CustomsEntryHeaders.Count);
			AssertEquals(2, dec.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestHeaderAdditionalInfoOnInvoiceHeadersCreatesTwoEntries()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "Additional Information");
			var cusCode = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "RPTID", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Direction", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Level", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			cusCode.Attributes.AddNew("Direction", "IMPORT");
			cusCode.Attributes.AddNew("Direction", "EXPORT");
			cusCode.Attributes.AddNew("Level", "HEADER");
			Factory.Save();
			var dec = GetJobDeclarationForTest();
			var inv1 = dec.Invoices.AddNew();
			var invLine1 = inv1.JobComInvoiceLines.AddNew();
			invLine1.JI_Tariff = "2203001010";
			var inv2 = dec.Invoices.AddNew();
			var invLine2 = inv2.JobComInvoiceLines.AddNew();
			invLine2.JI_Tariff = "2203001010";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, dec.CustomsEntryHeaders.Count);
			AdditionalInfo inv1AddInfo = inv1.AdditionalInfos.AddNew();
			inv1AddInfo.CSI_Code = "RPTID";
			inv1AddInfo.CSI_Description = "123";

			AdditionalInfo inv2AddInfo = inv2.AdditionalInfos.AddNew();
			inv2AddInfo.CSI_Code = "RPTID";
			inv2AddInfo.CSI_Description = "123";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, dec.CustomsEntryHeaders.Count);

			inv2AddInfo.CSI_Description = "124";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			AssertEquals("Two entries should be created because description is different for each RPTID", 2, dec.CustomsEntryHeaders.Count);
			Factory.Save(); // Triggers save and calculation of BGM Reference
			ZString year = ZDateTime.Now.Year.ToString();
			string[] expected = { year.Right(1) + "-B00001000", year.Right(1) + "-B00001000/1" };
			string[] actual = { dec.CustomsEntryHeaders[0].CH_BGMReference, dec.CustomsEntryHeaders[1].CH_BGMReference };
			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		public void TestMergeKeyByLineForInvoiceHeaderTransportChargesMOP()
		{
			var dec = GetJobDeclarationForTest();
			var inv1 = dec.Invoices.AddNew();
			inv1.JobComInvoiceLines.AddNew();
			var inv2 = dec.Invoices.AddNew();
			inv2.JobComInvoiceLines.AddNew();

			dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			dec.DoMerge();
			AssertEquals(1, dec.CustomsEntryHeaders.Count);
			AssertEquals(1, dec.CustomsEntryHeaders[0].MergedLines.Count);
			inv1.ZG_TransportChargesMethodOfPayment = "X";
			inv2.ZG_TransportChargesMethodOfPayment = "X";
			dec.DoMerge();
			AssertEquals(1, dec.CustomsEntryHeaders.Count);
			AssertEquals(1, dec.CustomsEntryHeaders[0].MergedLines.Count);
			inv2.ZG_TransportChargesMethodOfPayment = "Y";
			dec.DoMerge();
			AssertEquals(1, dec.CustomsEntryHeaders.Count);
			AssertEquals(2, dec.CustomsEntryHeaders[0].MergedLines.Count);
			inv2.ZG_TransportChargesMethodOfPayment = "X";
			dec.DoMerge();
			AssertEquals(1, dec.CustomsEntryHeaders.Count);
			AssertEquals(1, dec.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestMergeSame()
		{
			var dec = GetJobDeclarationForTest();
			dec.Invoices.AddNew();
			dec.InvoiceLines.AddNew();
			dec.InvoiceLines.AddNew();

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, dec.CustomsEntryHeaders.Count);
			AssertEquals(1, dec.CustomsEntryHeaders[0].MergedLines.Count);
		}

		public void TestSupplementaryCodesCreatesTwoMergeLines()
		{
			var dec = GetJobDeclarationForTest();
			var inv1 = dec.Invoices.AddNew();
			var invLine1 = inv1.JobComInvoiceLines.AddNew();
			invLine1.JI_Tariff = "2203001010";
			var inv2 = dec.Invoices.AddNew();
			var invLine2 = inv2.JobComInvoiceLines.AddNew();
			invLine2.JI_Tariff = "2203001010";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

			invLine1.JI_SupplementaryCode1 = "ABCD";
			invLine2.JI_SupplementaryCode1 = "ABCD";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

			invLine2.JI_SupplementaryCode1 = "EFGH";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			AssertEquals("Two entries should be created because they have different Supplementary Codes", 2, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

			invLine1.JI_SupplementaryCode1 = "ABCD";
			invLine2.JI_SupplementaryCode1 = "ABCD";
			invLine1.JI_SupplementaryCode2 = "EFGH";
			invLine2.JI_SupplementaryCode2 = "EFGH";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

			invLine2.JI_SupplementaryCode2 = "IJKL";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			AssertEquals("Two entries should be created because they have different Supplementary Codes", 2, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

			invLine2.JI_SupplementaryCode2 = "EFGH";
			invLine1.AdditionalSupplementaryCodes.AsString = "IJKL,MNOP";
			invLine2.AdditionalSupplementaryCodes.AsString = "IJKL,MNOP";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

			invLine2.AdditionalSupplementaryCodes.AsString = "MNOP,IJKL";

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			AssertEquals("Two entries should be created because they have different Supplementary Codes", 2, dec.CustomsEntryHeaders[0].AllEntryLines.Count);
		}

		public void TestMergeKeyDoesNotContainCusAuthorizationUsagesWhenNotUcc6()
		{
			AssertInvoiceLineAuthorizationUsagesArePartOfTheMergeKey(isUcc6: false, expectedInTheMergeKey: false);
		}

		public void TestMergeKeyContainsCusAuthorizationUsagesWhenUcc6()
		{
			AssertInvoiceLineAuthorizationUsagesArePartOfTheMergeKey(isUcc6: true, expectedInTheMergeKey: true);
		}

		public void TestMergeKeyDoesNotContainFiscalReferencesWhenNotUcc6()
		{
			AssertInvoiceLineFiscalReferencesArePartOfTheMergeKey(isUcc6: false, expectedInTheMergeKey: false);
		}

		public void TestMergeKeyContainsFiscalReferencesWhenUcc6()
		{
			AssertInvoiceLineFiscalReferencesArePartOfTheMergeKey(isUcc6: true, expectedInTheMergeKey: true);
		}

		public void TestMergeKeyDoesNotContainOrganisationsWhenNotUcc6()
		{
			AssertInvoiceLineOrganisationsArePartOfTheMergeKey(isUcc6: false, expectedInTheMergeKey: false);
		}

		public void TestMergeKeyContainsOrganisationsWhenUcc6()
		{
			AssertInvoiceLineOrganisationsArePartOfTheMergeKey(isUcc6: true, expectedInTheMergeKey: true);
		}

		public void TestMergeKeyContainsTransportChargesMethodOfPayment()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var dec = GetJobDeclarationForTest();
				dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				var inv1 = dec.Invoices.AddNew();
				inv1.ZG_TransportChargesMethodOfPayment = "Z";
				var invLine = inv1.JobComInvoiceLines.AddNew();

				var mergeStrategy = dec.CreateEntryCreationStrategy();

				Assert(mergeStrategy.GetKeyForLine(invLine).Contains(invLine.InvoiceHeader.ZG_TransportChargesMethodOfPayment));
				Assert(!mergeStrategy.GetKeyForHeader(invLine).Contains(invLine.InvoiceHeader.ZG_TransportChargesMethodOfPayment));
				Assert(mergeStrategy.GetKeyForHeader(invLine).Contains(invLine.JI_CEI));
			}
		}

		public void TestMergeKeyContainsAdditionalProcedureCodes()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var dec = GetJobDeclarationForTest();
				var cei = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				var inv1 = dec.Invoices.AddNew();
				var invLine = inv1.JobComInvoiceLines.AddNew();
				var mergeStrategy = dec.CreateEntryCreationStrategy();
				Assert(mergeStrategy.GetKeyForLine(invLine).Contains(invLine.AdditionalProcedureCodesAsString));
			}
		}

		public void TestGetPreviousDocumentHeaderKeys()
		{
			var dec = GetJobDeclarationForTest();
			var mergeStrategy = dec.CreateEntryCreationStrategy();
			AssertArrayEqualsByElements(new[]
			{
				  PreviousDocument.Schema.CSI_SubType,
				  PreviousDocument.Schema.CSI_Description,
				  PreviousDocument.Schema.CSI_ReferenceNumber,
				  PreviousDocument.Schema.CSI_Code
			}, mergeStrategy.GetPreviousDocumentHeaderKeys());
		}

		public void TestGetPreviousDocumentKeys()
		{
			var dec = GetJobDeclarationForTest();
			var mergeStrategy = dec.CreateEntryCreationStrategy();
			AssertArrayEqualsByElements(new[]
			{
				  PreviousDocument.Schema.CSI_SubType,
				  PreviousDocument.Schema.CSI_Description,
				  PreviousDocument.Schema.CSI_ReferenceNumber,
				  PreviousDocument.Schema.CSI_Code
			}, mergeStrategy.GetPreviousDocumentKeys());
		}

		public void TestMergeKeyForSupportingDocs()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var dec = GetJobDeclarationForTest();
				dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
				var inv1 = dec.Invoices.AddNew();
				var invLine1 = inv1.JobComInvoiceLines.AddNew();

				var supDoc1 = invLine1.SupportingDocuments.AddNew();
				supDoc1.CSI_Code = "AAA";
				supDoc1.CSI_Quantity = 1;

				dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals(1, dec.CustomsEntryHeaders.Count);
				AssertEquals(1, dec.ActiveEntryHeaders[0].MergedLines.Count);

				var supDoc2 = invLine1.SupportingDocuments.AddNew();
				supDoc2.CSI_Code = "BBB";
				supDoc2.CSI_Quantity = 2;

				dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals(1, dec.CustomsEntryHeaders.Count);
				AssertEquals(1, dec.ActiveEntryHeaders[0].MergedLines.Count);

				var inv2 = dec.Invoices.AddNew();
				var invLine2 = inv2.JobComInvoiceLines.AddNew();

				var supDoc3 = invLine2.SupportingDocuments.AddNew();
				supDoc3.CSI_Code = "BBB";
				supDoc3.CSI_Quantity = 3;

				dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals(1, dec.CustomsEntryHeaders.Count);
				AssertEquals(2, dec.ActiveEntryHeaders[0].MergedLines.Count);

				var supDoc4 = invLine2.SupportingDocuments.AddNew();
				supDoc4.CSI_Code = "AAA";
				supDoc4.CSI_Quantity = 4;

				dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals(1, dec.CustomsEntryHeaders.Count);
				AssertEquals(1, dec.ActiveEntryHeaders[0].MergedLines.Count);

				var supDoc5 = invLine2.SupportingDocuments.AddNew();
				supDoc5.CSI_Code = "CCC";
				supDoc5.CSI_Quantity = 5;

				dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals(1, dec.CustomsEntryHeaders.Count);
				AssertEquals(2, dec.ActiveEntryHeaders[0].MergedLines.Count);
			}
		}

		public void TestMergeKeyForCustomsUnitQuantityWithPercentageValue()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusCode = helper.CreateNewOrGetExistingCusCodeList(CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "ASV", "%vol", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode.PK, RefCusCodeListAttributeTypes.Codes.NoMerge, "Y");
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedKingdom))
			{
				var dec = GetJobDeclarationForTest();
				dec.JE_ApplicationCode = "CHF";
				var inv = dec.Invoices.AddNew();
				var invLine1 = inv.JobComInvoiceLines.AddNew();
				var invLine2 = inv.JobComInvoiceLines.AddNew();

				ResetInvoiceLinesCustomsUnitsAndQuantities(inv);
				invLine1.JI_CustomsSecondUnitQty = "KGM";
				invLine1.JI_CustomsSecondQuantity = 12.3;
				invLine2.JI_CustomsSecondUnitQty = "KGM";
				invLine2.JI_CustomsSecondQuantity = 12.3;

				dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals(1, dec.ActiveEntryHeaders[0].MergedLines.Count);

				ResetInvoiceLinesCustomsUnitsAndQuantities(inv);
				invLine1.JI_CustomsThirdUnitQty = "KGM";
				invLine1.JI_CustomsThirdQuantity = 23.4;
				invLine2.JI_CustomsThirdUnitQty = "KGM";
				invLine2.JI_CustomsThirdQuantity = 23.4;

				dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals(1, dec.ActiveEntryHeaders[0].MergedLines.Count);

				ResetInvoiceLinesCustomsUnitsAndQuantities(inv);
				invLine1.JI_CustomsFourthUnitQty = "KGM";
				invLine1.JI_CustomsFourthQuantity = 34.5;
				invLine2.JI_CustomsFourthUnitQty = "KGM";
				invLine2.JI_CustomsFourthQuantity = 34.5;

				dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals(1, dec.ActiveEntryHeaders[0].MergedLines.Count);

				ResetInvoiceLinesCustomsUnitsAndQuantities(inv);
				invLine1.JI_CustomsFifthUnitQty = "KGM";
				invLine1.JI_CustomsFifthQuantity = 34.5;
				invLine2.JI_CustomsFifthUnitQty = "KGM";
				invLine2.JI_CustomsFifthQuantity = 34.5;

				dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals(1, dec.ActiveEntryHeaders[0].MergedLines.Count);

				ResetInvoiceLinesCustomsUnitsAndQuantities(inv);
				invLine1.JI_CustomsSecondUnitQty = "ASV";
				invLine1.JI_CustomsSecondQuantity = 12.3;
				invLine2.JI_CustomsSecondUnitQty = "ASV";
				invLine2.JI_CustomsSecondQuantity = 12.3;

				dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals(2, dec.ActiveEntryHeaders[0].MergedLines.Count);

				ResetInvoiceLinesCustomsUnitsAndQuantities(inv);
				invLine1.JI_CustomsThirdUnitQty = "ASV";
				invLine1.JI_CustomsThirdQuantity = 23.4;
				invLine2.JI_CustomsThirdUnitQty = "ASV";
				invLine2.JI_CustomsThirdQuantity = 23.4;

				dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals(2, dec.ActiveEntryHeaders[0].MergedLines.Count);

				ResetInvoiceLinesCustomsUnitsAndQuantities(inv);
				invLine1.JI_CustomsFourthUnitQty = "ASV";
				invLine1.JI_CustomsFourthQuantity = 34.5;
				invLine2.JI_CustomsFourthUnitQty = "ASV";
				invLine2.JI_CustomsFourthQuantity = 34.5;

				dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals(2, dec.ActiveEntryHeaders[0].MergedLines.Count);

				ResetInvoiceLinesCustomsUnitsAndQuantities(inv);
				invLine1.JI_CustomsFifthUnitQty = "ASV";
				invLine1.JI_CustomsFifthQuantity = 34.5;
				invLine2.JI_CustomsFifthUnitQty = "ASV";
				invLine2.JI_CustomsFifthQuantity = 34.5;

				dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals(2, dec.ActiveEntryHeaders[0].MergedLines.Count);
			}
		}

		public void TestMergeKeyContainsCusNumber()
		{
			var declaration = GetJobDeclarationForTest();
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.ZG_CusNumber = "912132-6";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.ZG_CusNumber = "012324-1";
			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.ZG_CusNumber = "912132-6";

			CombineAssertions(() =>
			{
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("EntryHeaders Count", 1, declaration.CustomsEntryHeaders.Count);
				AssertEquals("Entry Lines Count", 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
				AssertArrayEqualsByElements("CusNumbers", new ZString[] { "912132-6", "012324-1" }, declaration.CustomsEntryHeaders[0].MergedLines.Select(x => x.CusNumber).ToArray());
			});
		}

		public void TestCreateANewEntryIfMaximumEntryLineExceeded()
		{
			var declaration = GetJobDeclarationForTest();
			var entryCreationStrategy = new EntryCreationStrategyForTest(declaration);
			AssertEquals("For EU, CreateANewEntryIfMaximumEntryLineExceeded", false, entryCreationStrategy.CreateANewEntryIfMaximumEntryLineExceededExposed);
		}

		public void TestMergeKeyContainsJI_PK()
		{
			var dataGrouping = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			var classType = helper.CreateOrGetExistingRefCusConditionType(dataGrouping, Enterprise.Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Class, "CAS2", "Test Class Condition Type");
			var preference = helper.CreatePreferenceForCountry("P1", "TestPreference", dataGrouping);
			var tradeGroupStandard = helper.CreateTradeGroup(dataGrouping, "STANDARD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			_ = helper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Portugal, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			var tariff = helper.CreateTariff(dataGrouping, tariffType.PK, "1122334455667", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var conditionValueType = helper.CreateOrGetExistingRefCusConditionValueType(dataGrouping, Enterprise.Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber, "SupportingDocumentNoReferenceNumber");
			var condition = helper.CreateOrGetExistingRefCusCondition(dataGrouping, classType.PK, tariff.PK, "C3", isImport: true, isExport: false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, c => c.ZX1_ZZS_Preference = preference.PK);
			_ = helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition.PK, "C333");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1122334455667";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Portugal;
			invoiceLine.JI_PrimaryPreference = "P1";

			var entryCreationStrategy = declaration.CreateEntryCreationStrategy();
			Assert(entryCreationStrategy.GetKeyForLine(invoiceLine).Contains(invoiceLine.PK));

			using (EUCustomsDataRegistry.Instance.ForbidMergingInvoiceLinesWithRatioBasedTariff.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				entryCreationStrategy = declaration.CreateEntryCreationStrategy();
				Assert(!entryCreationStrategy.GetKeyForLine(invoiceLine).Contains(invoiceLine.PK));
			}
		}

		#region Implementation

		void ResetInvoiceLinesCustomsUnitsAndQuantities(JobComInvoiceHeader invoice)
		{
			foreach (JobComInvoiceLine line in invoice.JobComInvoiceLines)
			{
				line.JI_CustomsSecondUnitQty = ZString.Empty;
				line.JI_CustomsSecondQuantity = 0;
				line.JI_CustomsThirdUnitQty = ZString.Empty;
				line.JI_CustomsThirdQuantity = 0;
				line.JI_CustomsFourthUnitQty = ZString.Empty;
				line.JI_CustomsFourthQuantity = 0;
				line.JI_CustomsFifthUnitQty = ZString.Empty;
				line.JI_CustomsFifthQuantity = 0;
			}
		}

		void AssertInvoiceLineAdditionalSupplyChainActorsArePartOfTheMergeKey(bool isUcc6, bool expectedInTheMergeKey)
		{
			var org = Factory.New<OrgHeader>();
			var dec = GetJobDeclarationForTest();
			var cei = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var inv = dec.Invoices.AddNew();
			var invLine = inv.JobComInvoiceLines.AddNew();
			invLine.JI_CEI = cei.PK;
			var reference = invLine.CusSupplyChainActorReferences.AddNew();
			reference.CFR_Code = "FR1";
			reference.CFR_Reference = "REF1";
			reference.CFR_OA_Owner = org.MainAddress.PK;
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(dec, isUcc6))
				{
					var mergeStrategy = dec.CreateEntryCreationStrategy();
					var mergeKey = mergeStrategy.GetKeyForLine(invLine);
					AssertEquals("reference.CFR_Code", expectedInTheMergeKey, mergeKey.Contains(reference.CFR_Code));
					AssertEquals("reference.CFR_Reference", expectedInTheMergeKey, mergeKey.Contains(reference.CFR_Reference));
					AssertEquals("reference.CFR_OA_Owner", expectedInTheMergeKey, mergeKey.Contains(reference.CFR_OA_Owner));
				}
			});
		}

		void AssertInvoiceLineAuthorizationUsagesArePartOfTheMergeKey(bool isUcc6, bool expectedInTheMergeKey)
		{
			var org = Factory.New<OrgHeader>();
			var dec = GetJobDeclarationForTest();
			var cei = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var inv = dec.Invoices.AddNew();
			var invLine = inv.JobComInvoiceLines.AddNew();
			invLine.JI_CEI = cei.PK;
			var authorisation = invLine.CusAuthorizationUsages.AddNew();
			authorisation.AGC_Code = "FR1";
			authorisation.AGC_Number = "REF1";
			authorisation.AGC_OH_Owner = org.PK;
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(dec, isUcc6))
				{
					var mergeStrategy = dec.CreateEntryCreationStrategy();
					var mergeKey = mergeStrategy.GetKeyForLine(invLine);
					AssertEquals("authorisation.AGC_Code", expectedInTheMergeKey, mergeKey.Contains(authorisation.AGC_Code));
					AssertEquals("authorisation.AGC_Number", expectedInTheMergeKey, mergeKey.Contains(authorisation.AGC_Number));
					AssertEquals("authorisation.AGC_OH_Owner", expectedInTheMergeKey, mergeKey.Contains(authorisation.AGC_OH_Owner));
				}
			});
		}

		void AssertInvoiceLineOrganisationsArePartOfTheMergeKey(bool isUcc6, bool expectedInTheMergeKey)
		{
			var consignor = Factory.New<OrgHeader>();
			var consignee = Factory.New<OrgHeader>();
			var buyer = Factory.New<OrgHeader>();
			var seller = Factory.New<OrgHeader>();
			var declaration = GetJobDeclarationForTest();
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			invoiceLine.JI_OA_ExporterAddress = consignor.MainAddress.PK;
			invoiceLine.JI_OA_ConsigneeAddress = consignee.MainAddress.PK;
			invoiceLine.BuyerDocAddress.OrganisationPK = buyer.PK;
			invoiceLine.SellerDocAddress.OrganisationPK = seller.PK;

			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUcc6))
				{
					var mergeStrategy = declaration.CreateEntryCreationStrategy();
					var mergeKey = mergeStrategy.GetKeyForLine(invoiceLine);
					AssertEquals("Contains Consignor?", expectedInTheMergeKey, mergeKey.Contains(consignor.PK));
					AssertEquals("Contains Consignee?", expectedInTheMergeKey, mergeKey.Contains(consignee.PK));
					AssertEquals("Contains Buyer?", expectedInTheMergeKey, mergeKey.Contains(buyer.PK));
					AssertEquals("Contains Seller?", expectedInTheMergeKey, mergeKey.Contains(seller.PK));
				}
			});
		}

		void AssertInvoiceLineFiscalReferencesArePartOfTheMergeKey(bool isUcc6, bool expectedInTheMergeKey)
		{
			var declaration = GetJobDeclarationForTest();
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var fiscalReference = invoiceLine.FiscalReferences.AddNew();
			fiscalReference.CFR_Code = "FR1";
			fiscalReference.CFR_Reference = "REF1";

			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUcc6))
				{
					var mergeStrategy = declaration.CreateEntryCreationStrategy();
					var mergeKey = mergeStrategy.GetKeyForLine(invoiceLine);
					AssertEquals("Contains FiscalReference.CFR_Code?", expectedInTheMergeKey, mergeKey.Contains(fiscalReference.CFR_Code));
					AssertEquals("Contains FiscalReference.CFR_Reference?", expectedInTheMergeKey, mergeKey.Contains(fiscalReference.CFR_Reference));
				}
			});
		}

		sealed class EntryCreationStrategyForTest : EntryCreationStrategy
		{
			public EntryCreationStrategyForTest(JobDeclaration declaration) : base(declaration)
			{
			}

			public bool CreateANewEntryIfMaximumEntryLineExceededExposed => CreateANewEntryIfMaximumEntryLineExceeded;
		}

		#endregion
	}
}
