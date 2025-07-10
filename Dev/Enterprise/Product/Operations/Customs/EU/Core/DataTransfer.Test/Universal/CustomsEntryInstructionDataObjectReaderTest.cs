using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.EU.DataTransfer.Universal.Testing
{
	sealed class CustomsEntryInstructionDataObjectReaderTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestPopulateTotalInnerPackages()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;

			var input = new EntryInstruction
			{
				AddInfoCollection = new List<UniversalDataBuss.DataObjects.Universal.AddInfo>(),
				AddInfoGroupCollection = new List<AddInfoGroup>()
			};

			input.AddInfoCollection.Add(new UniversalDataBuss.DataObjects.Universal.AddInfo
			{
				Key = Constants.AddInfoKeys.EntryInstruction.TotalInnerPackages,
				Value = "10"
			});

			var instruction = GetEntryInstructionFromData(input, declaration);
			AssertEquals("CEI_TotalInnerPackages value is expected to be updated with the value present in AddInfoCollection list.", 10, instruction.CEI_TotalInnerPackages);
			AssertEquals("TotalInnerPackages in AddInfoCollection was not loaded into CEI_AddInfo.", ZString.Empty, instruction.CEI_AddInfo);
		}

		public void TestMarkDv1DetailAsSelected()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			var euHelper = new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.CountryCodes.UnitedKingdom);

			var dv1Details1 = declaration.DV1Details.AddNew();
			var dv1Details2 = declaration.DV1Details.AddNew();
			var dv1Details3 = declaration.DV1Details.AddNew();

			euHelper.RegisterDv1DetailsPK(1, dv1Details1.PK);
			euHelper.RegisterDv1DetailsPK(2, dv1Details2.PK);
			euHelper.RegisterDv1DetailsPK(3, dv1Details3.PK);

			var input = new EntryInstruction
			{
				AddInfoCollection = new List<UniversalDataBuss.DataObjects.Universal.AddInfo>(),
				AddInfoGroupCollection = new List<AddInfoGroup>()
			};

			input.AddInfoCollection.Add(new UniversalDataBuss.DataObjects.Universal.AddInfo
			{
				Key = Constants.AddInfoKeys.EntryInstruction.CustomsValueInformationLink,
				Value = "2"
			});

			var instruction = GetEntryInstructionFromData(input, declaration, euHelper);
			AssertEquals("Number of DV1 Details", 3, instruction.DV1DetailsPivots.Count);
			var dv1DetailsPivots = instruction.DV1DetailsPivots;
			CombineAssertions(() =>
			{
				AssertEquals("The DV1 detail with link 1 should not be marked as selected.", ZBool.False, dv1DetailsPivots[0].IsForEntryInstruction);
				AssertEquals("The DV1 detail with link 2 should be marked as selected.", ZBool.True, dv1DetailsPivots[1].IsForEntryInstruction);
				AssertEquals("The DV1 detail with link 3 should not be marked as selected.", ZBool.False, dv1DetailsPivots[2].IsForEntryInstruction);
			});
		}

		public void TestImportSupplyChainActors()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionAdditionalSupplyChainActorSupportConfiguration(declaration, true))
			{
				AssertEquals("AdditionalSupplyChainActorSupport", true, declaration.Configuration.InstructionConfiguration.AdditionalSupplyChainActorSupport(declaration));
				var existingEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
				existingEntryInstruction.CEI_Style = "A";
				existingEntryInstruction.CEI_Description = "GREETING";
				var existingActor1 = existingEntryInstruction.CusSupplyChainActorReferences.AddNew();
				existingActor1.CFR_Code = "CS";
				existingActor1.CFR_Reference = "REF123";
				existingActor1.CFR_OA_Owner = Org3.MainAddress.PK;

				var existingActor2 = existingEntryInstruction.CusSupplyChainActorReferences.AddNew();
				existingActor2.CFR_Code = "MF";
				existingActor2.CFR_Reference = "REF657";
				existingActor2.CFR_OA_Owner = Org2.MainAddress.PK;

				var existingActor3 = existingEntryInstruction.CusSupplyChainActorReferences.AddNew();
				existingActor3.CFR_Code = "WH";
				existingActor3.CFR_Reference = "ZZZ999";
				existingActor3.CFR_OA_Owner = Org1.MainAddress.PK;

				Factory.SaveForTesting();

				CombineAssertions(() =>
				{
					var entryInstructionDataObject = new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance)
					{
						Style = "A",
						Description = "GREETING",
						CustomsReferenceCollection = CreateSupplyChainActors(),
					};

					var entryInstruction = GetEntryInstructionFromData(entryInstructionDataObject, declaration);
					AssertSame(existingEntryInstruction, entryInstruction);
					AssertEquals("Supply chain actor count", 2, entryInstruction.CusSupplyChainActorReferences.Count);
					var actor1 = entryInstruction.CusSupplyChainActorReferences[0];
					var actor2 = entryInstruction.CusSupplyChainActorReferences[1];
					if (existingActor1.CFR_Reference != "REF123")
					{
						actor1 = entryInstruction.CusSupplyChainActorReferences[1];
						actor2 = entryInstruction.CusSupplyChainActorReferences[0];
					}
					AssertSame(existingActor1, actor1);
					AssertSame(existingActor2, actor2);
					AssertEquals("existingActor3.IsDeleted - not matched based on CFR_Code", true, existingActor3.IsDeleted);

					AssertEquals("Actor 1 Role", "CS", actor1.CFR_Code);
					AssertEquals("Actor 1 Reference", "REF123", actor1.CFR_Reference);
					AssertEquals("Actor 1 Owner", Org1.MainAddress.PK, actor1.CFR_OA_Owner);

					AssertEquals("Actor 2 Role", "MF", actor2.CFR_Code);
					AssertEquals("Actor 2 Reference", "REF987", actor2.CFR_Reference);
					AssertEquals("Actor 2 Owner", Org2.MainAddress.PK, actor2.CFR_OA_Owner);
				});
			}
		}

		public void TestDoNotImportSupplyChainActorsWhenNotSupported()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionAdditionalSupplyChainActorSupportConfiguration(declaration, false))
			{
				AssertEquals("AdditionalSupplyChainActorSupport", false, declaration.Configuration.InstructionConfiguration.AdditionalSupplyChainActorSupport(declaration));
				var existingEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
				existingEntryInstruction.CEI_Style = "A";
				existingEntryInstruction.CEI_Description = "GREETING";
				var existingActor3 = existingEntryInstruction.CusSupplyChainActorReferences.AddNew();
				existingActor3.CFR_Code = "WH";
				existingActor3.CFR_Reference = "ZZZ999";
				existingActor3.CFR_OA_Owner = Org1.MainAddress.PK;

				Factory.SaveForTesting();

				CombineAssertions(() =>
				{
					var entryInstructionDataObject = new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance)
					{
						Style = "A",
						Description = "GREETING",
						CustomsReferenceCollection = CreateSupplyChainActors(),
					};

					var entryInstruction = GetEntryInstructionFromData(entryInstructionDataObject, declaration);
					AssertSame(existingEntryInstruction, entryInstruction);
					AssertEquals("Supply chain actor count", 1, entryInstruction.CusSupplyChainActorReferences.Count);
					AssertSame(existingActor3, entryInstruction.CusSupplyChainActorReferences[0]);
					AssertEquals("should not be deleted when not supported", false, existingActor3.IsDeleted);
				});
			}
		}

		public void TestImportSupplyChainActors_CustomsReferenceCollectionWithEmptyData()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionAdditionalSupplyChainActorSupportConfiguration(declaration, true))
			{
				AssertEquals("AdditionalSupplyChainActorSupport", true, declaration.Configuration.InstructionConfiguration.AdditionalSupplyChainActorSupport(declaration));
				var existingEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
				existingEntryInstruction.CEI_Style = "A";
				existingEntryInstruction.CEI_Description = "GREETING";
				var existingActor1 = existingEntryInstruction.CusSupplyChainActorReferences.AddNew();
				existingActor1.CFR_Code = "CS";
				existingActor1.CFR_Reference = "REF123";
				existingActor1.CFR_OA_Owner = Org3.MainAddress.PK;

				Factory.SaveForTesting();

				CombineAssertions(() =>
				{
					var entryInstructionDataObject = new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance)
					{
						Style = "A",
						Description = "GREETING",
						CustomsReferenceCollection = new List<CustomsReference>(new[] { new CustomsReference { Type = new CodeDescriptionPair() { Code = Customs.Business.CusReferenceTypeList.Codes.SupplyChainActor } } }),
					};

					var entryInstruction = GetEntryInstructionFromData(entryInstructionDataObject, declaration);
					AssertSame(existingEntryInstruction, entryInstruction);
					AssertEquals("Supply chain actor count", 0, entryInstruction.CusSupplyChainActorReferences.Count);
					AssertEquals("existingActor1.IsDeleted - not matched based on CFR_Code", true, existingActor1.IsDeleted);
				});
			}
		}

		public void TestImportSupplyChainActors_CustomsReferenceCollectionWithNoSupplyChainActor()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionAdditionalSupplyChainActorSupportConfiguration(declaration, true))
			{
				AssertEquals("AdditionalSupplyChainActorSupport", true, declaration.Configuration.InstructionConfiguration.AdditionalSupplyChainActorSupport(declaration));
				var existingEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
				existingEntryInstruction.CEI_Style = "A";
				existingEntryInstruction.CEI_Description = "GREETING";
				var existingActor1 = existingEntryInstruction.CusSupplyChainActorReferences.AddNew();
				existingActor1.CFR_Code = "CS";
				existingActor1.CFR_Reference = "REF123";
				existingActor1.CFR_OA_Owner = Org3.MainAddress.PK;

				Factory.SaveForTesting();

				CombineAssertions(() =>
				{
					var entryInstructionDataObject = new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance)
					{
						Style = "A",
						Description = "GREETING",
						CustomsReferenceCollection = new List<CustomsReference>(new[] { new CustomsReference { Type = new CodeDescriptionPair() { Code = Customs.Business.CusReferenceTypeList.Codes.FiscalReference }, Reference = "REF12" } }),
					};

					var entryInstruction = GetEntryInstructionFromData(entryInstructionDataObject, declaration);
					AssertSame(existingEntryInstruction, entryInstruction);
					AssertEquals("Supply chain actor count", 1, entryInstruction.CusSupplyChainActorReferences.Count);
					AssertSame(existingActor1, entryInstruction.CusSupplyChainActorReferences[0]);
					AssertEquals("existingActor1.IsDeleted - should not touch since xml doesn't contain data", false, existingActor1.IsDeleted);

					AssertEquals("Actor 1 Role", "CS", existingActor1.CFR_Code);
					AssertEquals("Actor 1 Reference", "REF123", existingActor1.CFR_Reference);
					AssertEquals("Actor 1 Owner", Org3.MainAddress.PK, existingActor1.CFR_OA_Owner);
				});
			}
		}

		CusEntryInstruction GetEntryInstructionFromData(EntryInstruction entryInstruction, JobDeclaration declaration, UniversalDataObjectReaderHelper readerHelper = null)
		{
			var helper = readerHelper ?? new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Latvia, Core.Constants.CountryCodes.Latvia);
			var reader = new CustomsEntryInstructionDataObjectReader(entryInstruction, new TestErrorLogger(), helper, Factory, declaration);
			return (CusEntryInstruction)reader.ReadIntoBusinessObject();
		}

		List<CustomsReference> CreateSupplyChainActors()
		{
			var result = new List<CustomsReference>();
			var actor1Address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			actor1Address.OrganizationCode = Org1.OH_Code;

			var actor1 = new CustomsReference();
			actor1.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			actor1.Type = new CodeDescriptionPair() { Code = Customs.Business.CusReferenceTypeList.Codes.SupplyChainActor };
			actor1.SubType = new CodeDescriptionPair35Char() { Code = "CS" };
			actor1.Reference = "REF123";
			actor1.Owner = actor1Address;
			result.Add(actor1);

			var actor2Address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			actor2Address.OrganizationCode = Org2.OH_Code;

			var actor2 = new CustomsReference();
			actor2.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			actor2.Type = new CodeDescriptionPair() { Code = Customs.Business.CusReferenceTypeList.Codes.SupplyChainActor };
			actor2.SubType = new CodeDescriptionPair35Char() { Code = "MF" };
			actor2.Reference = "REF987";
			actor2.Owner = actor2Address;
			result.Add(actor2);

			return result;
		}

		OrgHeader Org1
		{
			get
			{
				if (org1 == null)
				{
					org1 = Factory.NewWithValidTestData<OrgHeader>();
					org1.OH_FullName = "Test Company 1";
					org1.OH_Code = "TESTCO1";
					var org1Address = org1.MainAddress;
					org1Address.FillWithValidTestData();
					org1Address.Address1 = "123 Test Street";
					org1Address.Postcode = "A12B3C4";
				}
				return org1;
			}
		}
		OrgHeader org1;

		OrgHeader Org2
		{
			get
			{
				if (org2 == null)
				{
					org2 = Factory.NewWithValidTestData<OrgHeader>();
					org2.OH_FullName = "Test Company 2";
					org2.OH_Code = "TESTCO2";
					var org2Address = org2.MainAddress;
					org2Address.FillWithValidTestData();
					org2Address.Address1 = "456 Test Street";
					org2Address.Postcode = "X98Y7Z6";
				}
				return org2;
			}
		}
		OrgHeader org2;

		OrgHeader Org3
		{
			get
			{
				if (org3 == null)
				{
					org3 = Factory.NewWithValidTestData<OrgHeader>();
					org3.OH_FullName = "Test Company 3";
					org3.OH_Code = "TESTCO3";
					var org3Address = org3.MainAddress;
					org3Address.FillWithValidTestData();
					org3Address.Address1 = "789 Test Street";
					org3Address.Postcode = "C4DE56";
				}
				return org3;
			}
		}
		OrgHeader org3;

		public void TestImportSupportingDocuments()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionSupportingDocumentsSupportConfigurationAndReturnMock(declaration, true), true))
			{
				AssertEquals("SupportingDocumentsSupport", true, declaration.Configuration.InstructionConfiguration.SupportingDocumentsSupport(declaration));
				var existingEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
				existingEntryInstruction.CEI_Style = "A";
				existingEntryInstruction.CEI_Description = "GREETING";
				var existingDoc1 = existingEntryInstruction.SupportingDocuments.AddNew();
				existingDoc1.CSI_AdditionalDescription = "Sample Issuing Authority 1";
				existingDoc1.CSI_Code = "A004";
				existingDoc1.CSI_ItemNumber = 1;
				existingDoc1.CSI_ReferenceNumber = "TESTREF123";
				existingDoc1.CSI_DateOfExpiry = ZDateTime.BrettsBirthday;

				var existingDoc2 = existingEntryInstruction.SupportingDocuments.AddNew();
				existingDoc2.CSI_AdditionalDescription = "Sample Issuing Authority 2";
				existingDoc2.CSI_Code = "C013";
				existingDoc2.CSI_ItemNumber = 2;
				existingDoc2.CSI_ReferenceNumber = "TESTREF456";
				existingDoc2.CSI_DateOfExpiry = ZDateTime.BrettsBirthday.AddDays(14);
				existingDoc2.CSI_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;

				var existingDoc3 = existingEntryInstruction.SupportingDocuments.AddNew();
				existingDoc2.CSI_AdditionalDescription = "Sample Issuing Authority 2";
				existingDoc2.CSI_Code = "C013";
				existingDoc2.CSI_ItemNumber = 2;
				existingDoc2.CSI_ReferenceNumber = "TESTREF456";
				existingDoc2.CSI_DateOfExpiry = ZDateTime.BrettsBirthday.AddDays(14);
				existingDoc3.CSI_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddDays(1);

				Factory.SaveForTesting();

				CombineAssertions(() =>
				{
					var entryInstructionDataObject = new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance)
					{
						Style = "A",
						Description = "GREETING",
					};
					entryInstructionDataObject.SetCustomsSupportingInformationCollection(CreateSupportingDocumentDataObjects);

					var entryInstruction = GetEntryInstructionFromData(entryInstructionDataObject, declaration);
					AssertSame(existingEntryInstruction, entryInstruction);
					entryInstruction.SupportingDocuments.Load();
					var supportingDocuments = entryInstruction.SupportingDocuments.Cast<SupportingDocument>().ToArray();
					AssertEquals("Supporting Documents count", 3, supportingDocuments.Length);
					SupportingDocument doc1 = null;
					SupportingDocument doc2 = null;
					SupportingDocument doc3 = null;
					foreach (var supportingDocument in supportingDocuments)
					{
						switch (supportingDocument.CSI_Code)
						{
							case "A004":
								doc1 = supportingDocument;
								break;
							case "C013":
								doc2 = supportingDocument;
								break;
							default:
								doc3 = supportingDocument;
								break;
						}
					}

					AssertSame("existingDoc1 - Matched A004", existingDoc1, doc1);
					AssertEquals("Doc1 authority", "Sample Issuing Authority 1", doc1.CSI_AdditionalDescription);
					AssertEquals("Doc1 type", "A004", doc1.CSI_Code);
					AssertEquals("Doc1 item number", (ZShort)1, doc1.CSI_ItemNumber);
					AssertEquals("Doc1 Reference", "TESTREF123", doc1.CSI_ReferenceNumber);
					AssertEquals("Doc1 Date of validity", ZDateTime.BrettsBirthday, doc1.CSI_DateOfExpiry.Date);

					AssertSame("existingDoc2 - Matched C013", existingDoc2, doc2);
					AssertEquals("Doc2 authority", "Sample Issuing Authority 2", doc2.CSI_AdditionalDescription);
					AssertEquals("Doc2 type", "C013", doc2.CSI_Code);
					AssertEquals("Doc2 item number", (ZShort)2, doc2.CSI_ItemNumber);
					AssertEquals("Doc2 Reference", "TESTREF456", doc2.CSI_ReferenceNumber);
					AssertEquals("Doc2 Date of validity", ZDateTime.BrettsBirthday.AddDays(14), doc2.CSI_DateOfExpiry.Date);

					AssertEquals("Doc3 authority", "Sample Issuing Authority 3", doc3.CSI_AdditionalDescription);
					AssertEquals("Doc3 type", "C014", doc3.CSI_Code);
					AssertEquals("Doc3 item number", (ZShort)3, doc3.CSI_ItemNumber);
					AssertEquals("Doc3 Reference", "TESTREF536", doc3.CSI_ReferenceNumber);
					AssertEquals("Doc3 Date of validity", ZDateTime.BrettsBirthday.AddDays(13), doc3.CSI_DateOfExpiry.Date);

					AssertEquals("existingDoc3.IsDeleted - older was picked", true, existingDoc3.IsDeleted);
				});
			}
		}

		public void TestDoNotImportSupportingDocumentsWhenNotSupported()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionSupportingDocumentsSupportConfigurationAndReturnMock(declaration, false), true))
			{
				AssertEquals("SupportingDocumentsSupport", false, declaration.Configuration.InstructionConfiguration.SupportingDocumentsSupport(declaration));
				var existingEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
				existingEntryInstruction.CEI_Style = "A";
				existingEntryInstruction.CEI_Description = "GREETING";
				var existingDoc1 = existingEntryInstruction.SupportingDocuments.AddNew();
				existingDoc1.CSI_AdditionalDescription = "Sample Issuing Authority 2";
				existingDoc1.CSI_Code = "A004";
				existingDoc1.CSI_ItemNumber = 4;
				existingDoc1.CSI_PackQty = 55;
				existingDoc1.CSI_UnitOfQuantity = "E2";
				existingDoc1.CSI_Quantity3 = 800m;
				existingDoc1.CSI_UnitOfQuantity3 = "BO1";
				existingDoc1.CSI_ReferenceNumber = "TESTREF124";
				existingDoc1.CSI_DateOfExpiry = ZDateTime.BrettsBirthday.AddDays(-1);

				Factory.SaveForTesting();

				CombineAssertions(() =>
				{
					var entryInstructionDataObject = new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance)
					{
						Style = "A",
						Description = "GREETING",
					};
					entryInstructionDataObject.SetCustomsSupportingInformationCollection(CreateSupportingDocumentDataObjects);

					var entryInstruction = GetEntryInstructionFromData(entryInstructionDataObject, declaration);
					AssertSame(existingEntryInstruction, entryInstruction);
					AssertEquals("Supporting Documents count", 1, entryInstruction.SupportingDocuments.Count);
					var doc1 = entryInstruction.SupportingDocuments[0];
					AssertSame("existingDoc1", existingDoc1, doc1);
					AssertEquals("Doc1 authority", "Sample Issuing Authority 2", doc1.CSI_AdditionalDescription);
					AssertEquals("Doc1 type", "A004", doc1.CSI_Code);
					AssertEquals("Doc1 line no", (ZShort)4, doc1.CSI_ItemNumber);
					AssertEquals("Doc1 pack qty", 55, doc1.CSI_PackQty);
					AssertEquals("Doc1 pack UOQ", "E2", doc1.CSI_UnitOfQuantity);
					AssertEquals("Doc1 qty", 800m, doc1.CSI_Quantity3);
					AssertEquals("Doc1 UOQ", "BO1", doc1.CSI_UnitOfQuantity3);
					AssertEquals("Doc1 Reference", "TESTREF124", doc1.CSI_ReferenceNumber);
					AssertEquals("Doc1 Date of validity", ZDateTime.BrettsBirthday.AddDays(-1), doc1.CSI_DateOfExpiry.Date);
				});
			}
		}

		public void TestImportSupportingDocuments_CustomsSupportingInformationCollectionNoSupportingDocumentData()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionSupportingDocumentsSupportConfigurationAndReturnMock(declaration, true), true))
			{
				AssertEquals("SupportingDocumentsSupport", true, declaration.Configuration.InstructionConfiguration.SupportingDocumentsSupport(declaration));
				var existingEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
				existingEntryInstruction.CEI_Style = "A";
				existingEntryInstruction.CEI_Description = "GREETING";
				var existingDoc1 = existingEntryInstruction.SupportingDocuments.AddNew();
				existingDoc1.CSI_AdditionalDescription = "Sample Issuing Authority 2";
				existingDoc1.CSI_Code = "A004";
				existingDoc1.CSI_ItemNumber = 4;
				existingDoc1.CSI_PackQty = 55;
				existingDoc1.CSI_UnitOfQuantity = "E2";
				existingDoc1.CSI_Quantity3 = 800m;
				existingDoc1.CSI_UnitOfQuantity3 = "BO1";
				existingDoc1.CSI_ReferenceNumber = "TESTREF124";
				existingDoc1.CSI_DateOfExpiry = ZDateTime.BrettsBirthday.AddDays(-1);

				Factory.SaveForTesting();

				CombineAssertions(() =>
				{
					var entryInstructionDataObject = new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance)
					{
						Style = "A",
						Description = "GREETING",
					};
					entryInstructionDataObject.SetCustomsSupportingInformationCollection(() => new List<CustomsSupportingInformation>(new[]
					{
						new CustomsSupportingInformation
						{
							Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument },
							Type = new CodeDescriptionPair6Char() { Code = "C013" },
						}
					}));

					var entryInstruction = GetEntryInstructionFromData(entryInstructionDataObject, declaration);
					AssertSame(existingEntryInstruction, entryInstruction);
					AssertEquals("Supporting Documents count", 1, entryInstruction.SupportingDocuments.Count);
					var doc1 = entryInstruction.SupportingDocuments[0];
					AssertSame("existingDoc1", existingDoc1, doc1);
					AssertEquals("Doc1 authority", "Sample Issuing Authority 2", doc1.CSI_AdditionalDescription);
					AssertEquals("Doc1 type", "A004", doc1.CSI_Code);
					AssertEquals("Doc1 line no", (ZShort)4, doc1.CSI_ItemNumber);
					AssertEquals("Doc1 pack qty", 55, doc1.CSI_PackQty);
					AssertEquals("Doc1 pack UOQ", "E2", doc1.CSI_UnitOfQuantity);
					AssertEquals("Doc1 qty", 800m, doc1.CSI_Quantity3);
					AssertEquals("Doc1 UOQ", "BO1", doc1.CSI_UnitOfQuantity3);
					AssertEquals("Doc1 Reference", "TESTREF124", doc1.CSI_ReferenceNumber);
					AssertEquals("Doc1 Date of validity", ZDateTime.BrettsBirthday.AddDays(-1), doc1.CSI_DateOfExpiry.Date);
				});
			}
		}

		public void TestImportSupportingDocuments_CustomsSupportingInformationCollectionEmptySupportingDocumentData()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionSupportingDocumentsSupportConfigurationAndReturnMock(declaration, true), true))
			{
				AssertEquals("SupportingDocumentsSupport", true, declaration.Configuration.InstructionConfiguration.SupportingDocumentsSupport(declaration));
				var existingEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
				existingEntryInstruction.CEI_Style = "A";
				existingEntryInstruction.CEI_Description = "GREETING";
				var existingDoc1 = existingEntryInstruction.SupportingDocuments.AddNew();
				existingDoc1.CSI_AdditionalDescription = "Sample Issuing Authority 2";
				existingDoc1.CSI_Code = "A004";
				existingDoc1.CSI_ItemNumber = 4;
				existingDoc1.CSI_PackQty = 55;
				existingDoc1.CSI_UnitOfQuantity = "E2";
				existingDoc1.CSI_Quantity3 = 800m;
				existingDoc1.CSI_UnitOfQuantity3 = "BO1";
				existingDoc1.CSI_ReferenceNumber = "TESTREF124";
				existingDoc1.CSI_DateOfExpiry = ZDateTime.BrettsBirthday.AddDays(-1);

				Factory.SaveForTesting();

				CombineAssertions(() =>
				{
					var entryInstructionDataObject = new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance)
					{
						Style = "A",
						Description = "GREETING",
					};
					entryInstructionDataObject.SetCustomsSupportingInformationCollection(() => new List<CustomsSupportingInformation>(new[]
					{
						new CustomsSupportingInformation { Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument } }
					}));

					var entryInstruction = GetEntryInstructionFromData(entryInstructionDataObject, declaration);
					AssertSame(existingEntryInstruction, entryInstruction);
					AssertEquals("Supporting Documents count", 0, entryInstruction.SupportingDocuments.Count);
					AssertEquals("existingDoc1.IsDeleted - no matched", true, existingDoc1.IsDeleted);
				});
			}
		}

		public void TestImportSupportingDocumentsUCC5()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				var declaration = Factory.New<JobDeclaration>();
				using (ConfigurationTestHelper.TemporarySetupUCC5(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionSupportingDocumentsSupportConfigurationAndReturnMock(declaration, true), true))
				{
					AssertEquals("SupportingDocumentsSupport", true, declaration.Configuration.InstructionConfiguration.SupportingDocumentsSupport(declaration));
					var existingEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
					existingEntryInstruction.CEI_Style = "A";
					existingEntryInstruction.CEI_Description = "GREETING";
					var existingDoc1 = existingEntryInstruction.SupportingDocuments.AddNew();
					existingDoc1.CSI_AdditionalDescription = "Sample Issuing Authority 1";
					existingDoc1.CSI_Code = "A004";
					existingDoc1.CSI_ItemNumber = 1;
					existingDoc1.CSI_ReferenceNumber = "TESTREF123";
					existingDoc1.CSI_DateOfExpiry = ZDateTime.BrettsBirthday;

					var existingDoc2 = existingEntryInstruction.SupportingDocuments.AddNew();
					existingDoc2.CSI_AdditionalDescription = "Sample Issuing Authority 2";
					existingDoc2.CSI_Code = "C013";
					existingDoc2.CSI_ItemNumber = 2;
					existingDoc2.CSI_ReferenceNumber = "TESTREF456";
					existingDoc2.CSI_DateOfExpiry = ZDateTime.BrettsBirthday.AddDays(14);
					existingDoc2.CSI_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;

					var existingDoc3 = existingEntryInstruction.SupportingDocuments.AddNew();
					existingDoc2.CSI_AdditionalDescription = "Sample Issuing Authority 2";
					existingDoc2.CSI_Code = "C013";
					existingDoc2.CSI_ItemNumber = 2;
					existingDoc2.CSI_ReferenceNumber = "TESTREF456";
					existingDoc2.CSI_DateOfExpiry = ZDateTime.BrettsBirthday.AddDays(14);
					existingDoc3.CSI_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddDays(1);

					Factory.SaveForTesting();

					CombineAssertions(() =>
					{
						var entryInstructionDataObject = new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance)
						{
							Style = "A",
							Description = "GREETING",
						};
						entryInstructionDataObject.SetCustomsSupportingInformationCollection(CreateSupportingDocumentDataObjects);

						var entryInstruction = GetEntryInstructionFromData(entryInstructionDataObject, declaration);
						AssertSame(existingEntryInstruction, entryInstruction);
						entryInstruction.SupportingDocuments.Load();
						var supportingDocuments = entryInstruction.SupportingDocuments.Cast<SupportingDocument>().ToArray();
						AssertEquals("Supporting Documents count", 3, supportingDocuments.Length);
						SupportingDocument doc1 = null;
						SupportingDocument doc2 = null;
						SupportingDocument doc3 = null;
						foreach (var supportingDocument in supportingDocuments)
						{
							switch (supportingDocument.CSI_Code)
							{
								case "A004":
									doc1 = supportingDocument;
									break;
								case "C013":
									doc2 = supportingDocument;
									break;
								default:
									doc3 = supportingDocument;
									break;
							}
						}

						AssertSame("existingDoc1 - Matched A004", existingDoc1, doc1);
						AssertEquals("Doc1 authority", "Sample Issuing Authority 1", doc1.CSI_AdditionalDescription);
						AssertEquals("Doc1 type", "A004", doc1.CSI_Code);
						AssertEquals("Doc1 item number", (ZShort)1, doc1.CSI_ItemNumber);
						AssertEquals("Doc1 Reference", "TESTREF123", doc1.CSI_ReferenceNumber);
						AssertEquals("Doc1 Date of validity", ZDateTime.BrettsBirthday, doc1.CSI_DateOfExpiry.Date);

						AssertSame("existingDoc2 - Matched C013", existingDoc2, doc2);
						AssertEquals("Doc2 authority", "Sample Issuing Authority 2", doc2.CSI_AdditionalDescription);
						AssertEquals("Doc2 type", "C013", doc2.CSI_Code);
						AssertEquals("Doc2 item number", (ZShort)2, doc2.CSI_ItemNumber);
						AssertEquals("Doc2 Reference", "TESTREF456", doc2.CSI_ReferenceNumber);
						AssertEquals("Doc2 Date of validity", ZDateTime.BrettsBirthday.AddDays(14), doc2.CSI_DateOfExpiry.Date);

						AssertEquals("Doc3 authority", "Sample Issuing Authority 3", doc3.CSI_AdditionalDescription);
						AssertEquals("Doc3 type", "C014", doc3.CSI_Code);
						AssertEquals("Doc3 item number", (ZShort)3, doc3.CSI_ItemNumber);
						AssertEquals("Doc3 Reference", "TESTREF536", doc3.CSI_ReferenceNumber);
						AssertEquals("Doc3 Date of validity", ZDateTime.BrettsBirthday.AddDays(13), doc3.CSI_DateOfExpiry.Date);

						AssertEquals("existingDoc3.IsDeleted - older was picked", true, existingDoc3.IsDeleted);
					});
				}
			}
		}

		static List<CustomsSupportingInformation> CreateSupportingDocumentDataObjects()
		{
			return new List<CustomsSupportingInformation>(new[]
									{
							new CustomsSupportingInformation
							{
								Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument },
								Type = new CodeDescriptionPair6Char() { Code = "A004" },
								AdditionalDescription = "Sample Issuing Authority 1",
								ItemNumber = 1,
								DateOfExpiry = ZDateTime.BrettsBirthday,
								ReferenceNumber = "TESTREF123"
							},
							new CustomsSupportingInformation()
							{
								Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument },
								Type = new CodeDescriptionPair6Char() { Code = "C014" },
								AdditionalDescription = "Sample Issuing Authority 3",
								ItemNumber = 3,
								DateOfExpiry = ZDateTime.BrettsBirthday.AddDays(13),
								ReferenceNumber = "TESTREF536"
							},
							new CustomsSupportingInformation()
							{
								Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument },
								Type = new CodeDescriptionPair6Char() { Code = "C013" },
								AdditionalDescription = "Sample Issuing Authority 2",
								ItemNumber = 2,
								DateOfExpiry = ZDateTime.BrettsBirthday.AddDays(14),
								ReferenceNumber = "TESTREF456"
							}
						});
		}

		public void TestImportPreviousDocuments()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionPreviousDocumentsSupportConfigurationAndReturnMock(declaration, true), true))
			{
				AssertEquals("PreviousDocumentsSupport", true, declaration.Configuration.InstructionConfiguration.PreviousDocumentsSupport(declaration));
				var existingEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
				existingEntryInstruction.CEI_Style = "A";
				existingEntryInstruction.CEI_Description = "GREETING";
				var existingDoc1 = existingEntryInstruction.PreviousDocuments.AddNew();
				existingDoc1.CSI_ReferenceNumber = "REF12345";
				existingDoc1.CSI_Code = "N380";

				var existingDoc2 = existingEntryInstruction.PreviousDocuments.AddNew();
				existingDoc2.CSI_ReferenceNumber = "REF987";
				existingDoc2.CSI_Code = "N785";
				existingDoc2.CSI_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;

				var existingDoc3 = existingEntryInstruction.PreviousDocuments.AddNew();
				existingDoc3.CSI_ReferenceNumber = "REF987";
				existingDoc3.CSI_Code = "N785";
				existingDoc3.CSI_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddDays(1);

				Factory.SaveForTesting();

				CombineAssertions(() =>
				{
					var entryInstructionDataObject = new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance)
					{
						Style = "A",
						Description = "GREETING",
					};
					entryInstructionDataObject.SetCustomsSupportingInformationCollection(CreatePreviousDocumentDataObjects);

					var entryInstruction = GetEntryInstructionFromData(entryInstructionDataObject, declaration);
					AssertSame(existingEntryInstruction, entryInstruction);
					entryInstruction.PreviousDocuments.Load();
					var previousDocuments = entryInstruction.PreviousDocuments.Cast<PreviousDocument>().ToArray();
					AssertEquals("Previous Documents count", 3, previousDocuments.Length);
					PreviousDocument doc1 = null;
					PreviousDocument doc2 = null;
					PreviousDocument doc3 = null;
					foreach (var previousDocument in previousDocuments)
					{
						switch (previousDocument.CSI_ReferenceNumber)
						{
							case "REF12345":
								doc1 = previousDocument;
								break;
							case "REF987":
								doc2 = previousDocument;
								break;
							default:
								doc3 = previousDocument;
								break;
						}
					}
					AssertSame("existingDoc1 - Matched N380", existingDoc1, doc1);
					AssertEquals("Doc1 reference number", "REF12345", doc1.CSI_ReferenceNumber);
					AssertEquals("Doc1 type", "N380", doc1.CSI_Code);
					AssertSame("existingDoc2 - Matched N785", existingDoc2, doc2);
					AssertEquals("Doc2 reference number", "REF987", doc2.CSI_ReferenceNumber);
					AssertEquals("Doc2 type", "N785", doc2.CSI_Code);
					AssertEquals("Doc3 reference number", "REF567", doc3.CSI_ReferenceNumber);
					AssertEquals("Doc3 type", "N786", doc3.CSI_Code);

					AssertEquals("existingDoc3.IsDeleted - older got matched", true, existingDoc3.IsDeleted);
				});
			}
		}

		public void TestDoNotImportPreviousDocumentsWhenNoSupported()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionPreviousDocumentsSupportConfigurationAndReturnMock(declaration, false), true))
			{
				AssertEquals("PreviousDocumentsSupport", false, declaration.Configuration.InstructionConfiguration.PreviousDocumentsSupport(declaration));
				var existingEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
				existingEntryInstruction.CEI_Style = "A";
				existingEntryInstruction.CEI_Description = "GREETING";
				var existingDoc1 = existingEntryInstruction.PreviousDocuments.AddNew();
				existingDoc1.CSI_ReferenceNumber = "REF12346";
				existingDoc1.CSI_Code = "N380";

				Factory.SaveForTesting();

				CombineAssertions(() =>
				{
					var entryInstructionDataObject = new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance)
					{
						Style = "A",
						Description = "GREETING",
					};
					entryInstructionDataObject.SetCustomsSupportingInformationCollection(CreatePreviousDocumentDataObjects);

					var entryInstruction = GetEntryInstructionFromData(entryInstructionDataObject, declaration);
					AssertSame(existingEntryInstruction, entryInstruction);
					AssertEquals("Previous Documents count", 1, entryInstruction.PreviousDocuments.Count);
					var doc1 = entryInstruction.PreviousDocuments[0];
					AssertSame("existingDoc1", existingDoc1, doc1);
					AssertEquals("Doc1 reference number", "REF12346", doc1.CSI_ReferenceNumber);
					AssertEquals("Doc1 type", "N380", doc1.CSI_Code);
				});
			}
		}

		public void TestImportPreviousDocuments_CustomsSupportingInformationCollectionWithNoPreviousDocumentData()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionPreviousDocumentsSupportConfigurationAndReturnMock(declaration, true), true))
			{
				AssertEquals("PreviousDocumentsSupport", true, declaration.Configuration.InstructionConfiguration.PreviousDocumentsSupport(declaration));
				var existingEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
				existingEntryInstruction.CEI_Style = "A";
				existingEntryInstruction.CEI_Description = "GREETING";
				var existingDoc1 = existingEntryInstruction.PreviousDocuments.AddNew();
				existingDoc1.CSI_ReferenceNumber = "REF12346";
				existingDoc1.CSI_Code = "N380";

				Factory.SaveForTesting();

				CombineAssertions(() =>
				{
					var entryInstructionDataObject = new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance)
					{
						Style = "A",
						Description = "GREETING",
					};
					entryInstructionDataObject.SetCustomsSupportingInformationCollection(() => new List<CustomsSupportingInformation>(new[]
					{
						new CustomsSupportingInformation { Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument } }
					}));

					var entryInstruction = GetEntryInstructionFromData(entryInstructionDataObject, declaration);
					AssertSame(existingEntryInstruction, entryInstruction);
					AssertEquals("Previous Documents count", 1, entryInstruction.PreviousDocuments.Count);
					var doc1 = entryInstruction.PreviousDocuments[0];
					AssertSame("existingDoc1", existingDoc1, doc1);
					AssertEquals("Doc1 reference number", "REF12346", doc1.CSI_ReferenceNumber);
					AssertEquals("Doc1 type", "N380", doc1.CSI_Code);
				});
			}
		}

		public void TestImportPreviousDocuments_CustomsSupportingInformationCollectionWithEmptyData()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionPreviousDocumentsSupportConfigurationAndReturnMock(declaration, true), true))
			{
				AssertEquals("PreviousDocumentsSupport", true, declaration.Configuration.InstructionConfiguration.PreviousDocumentsSupport(declaration));
				var existingEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
				existingEntryInstruction.CEI_Style = "A";
				existingEntryInstruction.CEI_Description = "GREETING";
				var existingDoc1 = existingEntryInstruction.PreviousDocuments.AddNew();
				existingDoc1.CSI_ReferenceNumber = "REF12346";
				existingDoc1.CSI_Code = "N380";

				Factory.SaveForTesting();

				CombineAssertions(() =>
				{
					var entryInstructionDataObject = new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance)
					{
						Style = "A",
						Description = "GREETING",
					};
					entryInstructionDataObject.SetCustomsSupportingInformationCollection(() => new List<CustomsSupportingInformation>(new[]
					{
						new CustomsSupportingInformation { Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument } }
					}));

					var entryInstruction = GetEntryInstructionFromData(entryInstructionDataObject, declaration);
					AssertSame(existingEntryInstruction, entryInstruction);
					AssertEquals("Previous Documents count", 0, entryInstruction.PreviousDocuments.Count);
					AssertEquals("existingDoc1.IsDeleted", true, existingDoc1.IsDeleted);
				});
			}
		}

		public void TestImportPreviousDocumentsUCC5()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				var declaration = Factory.New<JobDeclaration>();
				using (ConfigurationTestHelper.TemporarySetupUCC5(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionPreviousDocumentsSupportConfigurationAndReturnMock(declaration, true), true))
				{
					AssertEquals("PreviousDocumentsSupport", true, declaration.Configuration.InstructionConfiguration.PreviousDocumentsSupport(declaration));
					var existingEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
					existingEntryInstruction.CEI_Style = "A";
					existingEntryInstruction.CEI_Description = "GREETING";
					var existingDoc1 = existingEntryInstruction.PreviousDocuments.AddNew();
					existingDoc1.CSI_ReferenceNumber = "REF12345";
					existingDoc1.CSI_Code = "N380";

					var existingDoc2 = existingEntryInstruction.PreviousDocuments.AddNew();
					existingDoc2.CSI_ReferenceNumber = "REF987";
					existingDoc2.CSI_Code = "N785";
					existingDoc2.CSI_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;

					var existingDoc3 = existingEntryInstruction.PreviousDocuments.AddNew();
					existingDoc3.CSI_ReferenceNumber = "REF987";
					existingDoc3.CSI_Code = "N785";
					existingDoc3.CSI_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddDays(1);

					Factory.SaveForTesting();

					CombineAssertions(() =>
					{
						var entryInstructionDataObject = new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance)
						{
							Style = "A",
							Description = "GREETING",
						};
						entryInstructionDataObject.SetCustomsSupportingInformationCollection(CreatePreviousDocumentDataObjects);

						var entryInstruction = GetEntryInstructionFromData(entryInstructionDataObject, declaration);
						AssertSame(existingEntryInstruction, entryInstruction);
						entryInstruction.PreviousDocuments.Load();
						var previousDocuments = entryInstruction.PreviousDocuments.Cast<PreviousDocument>().ToArray();
						AssertEquals("Previous Documents count", 3, previousDocuments.Length);
						PreviousDocument doc1 = null;
						PreviousDocument doc2 = null;
						PreviousDocument doc3 = null;
						foreach (var previousDocument in previousDocuments)
						{
							switch (previousDocument.CSI_ReferenceNumber)
							{
								case "REF12345":
									doc1 = previousDocument;
									break;
								case "REF987":
									doc2 = previousDocument;
									break;
								default:
									doc3 = previousDocument;
									break;
							}
						}
						AssertSame("existingDoc1 - Matched N380", existingDoc1, doc1);
						AssertEquals("Doc1 reference number", "REF12345", doc1.CSI_ReferenceNumber);
						AssertEquals("Doc1 type", "N380", doc1.CSI_Code);
						AssertSame("existingDoc2 - Matched N785", existingDoc2, doc2);
						AssertEquals("Doc2 reference number", "REF987", doc2.CSI_ReferenceNumber);
						AssertEquals("Doc2 type", "N785", doc2.CSI_Code);
						AssertEquals("Doc3 reference number", "REF567", doc3.CSI_ReferenceNumber);
						AssertEquals("Doc3 type", "N786", doc3.CSI_Code);

						AssertEquals("existingDoc3.IsDeleted - older got matched", true, existingDoc3.IsDeleted);
					});
				}
			}
		}

		static List<CustomsSupportingInformation> CreatePreviousDocumentDataObjects()
		{
			return new List<CustomsSupportingInformation>(new[]
			{
				new CustomsSupportingInformation()
				{
					Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument },
					Type = new CodeDescriptionPair6Char() { Code = "N380" },
					ReferenceNumber = "REF12345",
				},
				new CustomsSupportingInformation()
				{
					Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument },
					Type = new CodeDescriptionPair6Char() { Code = "N786" },
					ReferenceNumber = "REF567",
				},
				new CustomsSupportingInformation()
				{
					Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument },
					Type = new CodeDescriptionPair6Char() { Code = "N785" },
					ReferenceNumber = "REF987",
				}
			});
		}

		public void TestImportAdditionalInfos()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionAdditionalInfosSupportConfigurationAndReturnMock(declaration, true), true))
			{
				var existingEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
				AssertEquals("AdditionalInfosSupport", true, declaration.Configuration.InstructionConfiguration.AdditionalInfosSupport(declaration, existingEntryInstruction));
				existingEntryInstruction.CEI_Style = "A";
				existingEntryInstruction.CEI_Description = "GREETING";

				var existingAddInfo1 = existingEntryInstruction.AdditionalInfos.AddNew();
				existingAddInfo1.CSI_SubType = "TRA";
				existingAddInfo1.CSI_Code = "1D24";
				existingAddInfo1.CSI_ReferenceNumber = "202301060900";
				existingAddInfo1.CSI_Description = "Test";

				var existingAddInfo2 = existingEntryInstruction.AdditionalInfos.AddNew();
				existingAddInfo2.CSI_SubType = "REF";
				existingAddInfo2.CSI_Code = "4444";
				existingAddInfo2.CSI_ReferenceNumber = "202303291600";
				existingAddInfo2.CSI_Description = "Another Test";
				existingAddInfo2.CSI_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;

				var existingAddInfo3 = existingEntryInstruction.AdditionalInfos.AddNew();
				existingAddInfo3.CSI_SubType = "REF";
				existingAddInfo3.CSI_Code = "4444";
				existingAddInfo3.CSI_ReferenceNumber = "202303291600";
				existingAddInfo3.CSI_Description = "Another Test";
				existingAddInfo3.CSI_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddDays(1);

				Factory.SaveForTesting();

				CombineAssertions(() =>
				{
					var entryInstructionDataObject = new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance)
					{
						Style = "A",
						Description = "GREETING",
					};
					entryInstructionDataObject.SetCustomsSupportingInformationCollection(CreateAdditionalReferenceDataObjects);

					var entryInstruction = GetEntryInstructionFromData(entryInstructionDataObject, declaration);
					AssertSame(existingEntryInstruction, entryInstruction);
					entryInstruction.AdditionalInfos.Load();
					var additionalInfos = entryInstruction.AdditionalInfos.Cast<AdditionalInfo>().ToArray();
					AssertEquals("Additional Infos count", 3, additionalInfos.Length);

					AdditionalInfo ref1 = null;
					AdditionalInfo ref2 = null;
					AdditionalInfo ref3 = null;
					foreach (var additionalInfo in additionalInfos)
					{
						switch (additionalInfo.CSI_ReferenceNumber)
						{
							case "202301060900":
								ref1 = additionalInfo;
								break;
							case "202303291600":
								ref2 = additionalInfo;
								break;
							default:
								ref3 = additionalInfo;
								break;
						}
					}

					AssertSame("existingAddInfo1 - Matched N380", existingAddInfo1, ref1);
					AssertEquals("Reference 1 Kind", "TRA", ref1.CSI_SubType);
					AssertEquals("Reference 1 Full Type", "1D24", ref1.CSI_Code);
					AssertEquals("Reference 1 Reference Number", "202301060900", ref1.CSI_ReferenceNumber);
					AssertEquals("Reference 1 Description", "Test", ref1.CSI_Description);

					AssertSame("existingAddInfo2 - Matched 4444", existingAddInfo2, ref2);
					AssertEquals("Reference 2 Kind", "REF", ref2.CSI_SubType);
					AssertEquals("Reference 2 Full Type", "4444", ref2.CSI_Code);
					AssertEquals("Reference 2 Reference Number", "202303291600", ref2.CSI_ReferenceNumber);
					AssertEquals("Reference 2 Description", "Another Test", ref2.CSI_Description);

					AssertEquals("Reference 3 Kind", "REF", ref3.CSI_SubType);
					AssertEquals("Reference 3 Full Type", "5555", ref3.CSI_Code);
					AssertEquals("Reference 3 Reference Number", "202303291601", ref3.CSI_ReferenceNumber);
					AssertEquals("Reference 3 Description", "Another Test 2", ref3.CSI_Description);

					AssertEquals("existingAddInfo3.IsDeleted - older was matched", true, existingAddInfo3.IsDeleted);
				});
			}
		}

		public void TestDoNotImportAdditionalInfosWhenNotSupported()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionAdditionalInfosSupportConfigurationAndReturnMock(declaration, false), true))
			{
				var existingEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
				AssertEquals("AdditionalInfosSupport", false, declaration.Configuration.InstructionConfiguration.AdditionalInfosSupport(declaration, existingEntryInstruction));
				existingEntryInstruction.CEI_Style = "A";
				existingEntryInstruction.CEI_Description = "GREETING";

				var existingAddInfo1 = existingEntryInstruction.AdditionalInfos.AddNew();
				existingAddInfo1.CSI_SubType = "TR1";
				existingAddInfo1.CSI_Code = "1D24";
				existingAddInfo1.CSI_ReferenceNumber = "202301060901";
				existingAddInfo1.CSI_Description = "Test 1";

				Factory.SaveForTesting();

				CombineAssertions(() =>
				{
					var entryInstructionDataObject = new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance)
					{
						Style = "A",
						Description = "GREETING",
					};
					entryInstructionDataObject.SetCustomsSupportingInformationCollection(CreateAdditionalReferenceDataObjects);

					var entryInstruction = GetEntryInstructionFromData(entryInstructionDataObject, declaration);
					AssertSame(existingEntryInstruction, entryInstruction);
					AssertEquals("Additional Infos count", 1, entryInstruction.AdditionalInfos.Count);
					var ref1 = entryInstruction.AdditionalInfos[0];
					AssertSame("existingAddInfo1", existingAddInfo1, ref1);
					AssertEquals("Reference 1 Kind", "TR1", ref1.CSI_SubType);
					AssertEquals("Reference 1 Full Type", "1D24", ref1.CSI_Code);
					AssertEquals("Reference 1 Reference Number", "202301060901", ref1.CSI_ReferenceNumber);
					AssertEquals("Reference 1 Description", "Test 1", ref1.CSI_Description);
				});
			}
		}

		public void TestImportAdditionalInfos_CustomsSupportingInformationCollectionWithNoAdditionalInfoData()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionAdditionalInfosSupportConfigurationAndReturnMock(declaration, true), true))
			{
				var existingEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
				AssertEquals("AdditionalInfosSupport", true, declaration.Configuration.InstructionConfiguration.AdditionalInfosSupport(declaration, existingEntryInstruction));
				existingEntryInstruction.CEI_Style = "A";
				existingEntryInstruction.CEI_Description = "GREETING";

				var existingAddInfo1 = existingEntryInstruction.AdditionalInfos.AddNew();
				existingAddInfo1.CSI_SubType = "TR1";
				existingAddInfo1.CSI_Code = "1D24";
				existingAddInfo1.CSI_ReferenceNumber = "202301060901";
				existingAddInfo1.CSI_Description = "Test 1";

				Factory.SaveForTesting();

				CombineAssertions(() =>
				{
					var entryInstructionDataObject = new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance)
					{
						Style = "A",
						Description = "GREETING",
					};
					entryInstructionDataObject.SetCustomsSupportingInformationCollection(() => new List<CustomsSupportingInformation>(new[]
					{
						new CustomsSupportingInformation { Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument } }
					}));

					var entryInstruction = GetEntryInstructionFromData(entryInstructionDataObject, declaration);
					AssertSame(existingEntryInstruction, entryInstruction);
					AssertEquals("Additional Infos count", 1, entryInstruction.AdditionalInfos.Count);
					var ref1 = entryInstruction.AdditionalInfos[0];
					AssertSame("existingAddInfo1", existingAddInfo1, ref1);
					AssertEquals("Reference 1 Kind", "TR1", ref1.CSI_SubType);
					AssertEquals("Reference 1 Full Type", "1D24", ref1.CSI_Code);
					AssertEquals("Reference 1 Reference Number", "202301060901", ref1.CSI_ReferenceNumber);
					AssertEquals("Reference 1 Description", "Test 1", ref1.CSI_Description);
				});
			}
		}

		public void TestImportAdditionalInfos_CustomsSupportingInformationCollectionWithEmptyAdditionalInfoData()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionAdditionalInfosSupportConfigurationAndReturnMock(declaration, true), true))
			{
				var existingEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
				AssertEquals("AdditionalInfosSupport", true, declaration.Configuration.InstructionConfiguration.AdditionalInfosSupport(declaration, existingEntryInstruction));
				existingEntryInstruction.CEI_Style = "A";
				existingEntryInstruction.CEI_Description = "GREETING";

				var existingAddInfo1 = existingEntryInstruction.AdditionalInfos.AddNew();
				existingAddInfo1.CSI_SubType = "TR1";
				existingAddInfo1.CSI_Code = "1D24";
				existingAddInfo1.CSI_ReferenceNumber = "202301060901";
				existingAddInfo1.CSI_Description = "Test 1";

				Factory.SaveForTesting();

				CombineAssertions(() =>
				{
					var entryInstructionDataObject = new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance)
					{
						Style = "A",
						Description = "GREETING",
					};
					entryInstructionDataObject.SetCustomsSupportingInformationCollection(() => new List<CustomsSupportingInformation>(new[]
					{
						new CustomsSupportingInformation { Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo } }
					}));

					var entryInstruction = GetEntryInstructionFromData(entryInstructionDataObject, declaration);
					AssertSame(existingEntryInstruction, entryInstruction);
					AssertEquals("Additional Infos count", 0, entryInstruction.AdditionalInfos.Count);
					AssertEquals("existingAddInfo1.IsDeleted", true, existingAddInfo1.IsDeleted);
				});
			}
		}

		public void TestImportAdditionalInfosUCC5()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				var declaration = Factory.New<JobDeclaration>();
				using (ConfigurationTestHelper.TemporarySetupUCC5(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionAdditionalInfosSupportConfigurationAndReturnMock(declaration, true), true))
				{
					var existingEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
					AssertEquals("AdditionalInfosSupport", true, declaration.Configuration.InstructionConfiguration.AdditionalInfosSupport(declaration, existingEntryInstruction));
					existingEntryInstruction.CEI_Style = "A";
					existingEntryInstruction.CEI_Description = "GREETING";

					var existingAddInfo1 = existingEntryInstruction.AdditionalInfos.AddNew();
					existingAddInfo1.CSI_SubType = "TRA";
					existingAddInfo1.CSI_Code = "1D24";
					existingAddInfo1.CSI_ReferenceNumber = "202301060900";
					existingAddInfo1.CSI_Description = "Test";

					var existingAddInfo2 = existingEntryInstruction.AdditionalInfos.AddNew();
					existingAddInfo2.CSI_SubType = "REF";
					existingAddInfo2.CSI_Code = "4444";
					existingAddInfo2.CSI_ReferenceNumber = "202303291600";
					existingAddInfo2.CSI_Description = "Another Test";
					existingAddInfo2.CSI_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;

					var existingAddInfo3 = existingEntryInstruction.AdditionalInfos.AddNew();
					existingAddInfo3.CSI_SubType = "REF";
					existingAddInfo3.CSI_Code = "4444";
					existingAddInfo3.CSI_ReferenceNumber = "202303291600";
					existingAddInfo3.CSI_Description = "Another Test";
					existingAddInfo3.CSI_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddDays(1);

					Factory.SaveForTesting();

					CombineAssertions(() =>
					{
						var entryInstructionDataObject = new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance)
						{
							Style = "A",
							Description = "GREETING",
						};
						entryInstructionDataObject.SetCustomsSupportingInformationCollection(CreateAdditionalReferenceDataObjects);

						var entryInstruction = GetEntryInstructionFromData(entryInstructionDataObject, declaration);
						AssertSame(existingEntryInstruction, entryInstruction);
						entryInstruction.AdditionalInfos.Load();
						var additionalInfos = entryInstruction.AdditionalInfos.Cast<AdditionalInfo>().ToArray();
						AssertEquals("Additional Infos count", 3, additionalInfos.Length);

						AdditionalInfo ref1 = null;
						AdditionalInfo ref2 = null;
						AdditionalInfo ref3 = null;
						foreach (var additionalInfo in additionalInfos)
						{
							switch (additionalInfo.CSI_ReferenceNumber)
							{
								case "202301060900":
									ref1 = additionalInfo;
									break;
								case "202303291600":
									ref2 = additionalInfo;
									break;
								default:
									ref3 = additionalInfo;
									break;
							}
						}

						AssertSame("existingAddInfo1 - Matched N380", existingAddInfo1, ref1);
						AssertEquals("Reference 1 Kind", "TRA", ref1.CSI_SubType);
						AssertEquals("Reference 1 Full Type", "1D24", ref1.CSI_Code);
						AssertEquals("Reference 1 Reference Number", "202301060900", ref1.CSI_ReferenceNumber);
						AssertEquals("Reference 1 Description", "Test", ref1.CSI_Description);

						AssertSame("existingAddInfo2 - Matched 4444", existingAddInfo2, ref2);
						AssertEquals("Reference 2 Kind", "REF", ref2.CSI_SubType);
						AssertEquals("Reference 2 Full Type", "4444", ref2.CSI_Code);
						AssertEquals("Reference 2 Reference Number", "202303291600", ref2.CSI_ReferenceNumber);
						AssertEquals("Reference 2 Description", "Another Test", ref2.CSI_Description);

						AssertEquals("Reference 3 Kind", "REF", ref3.CSI_SubType);
						AssertEquals("Reference 3 Full Type", "5555", ref3.CSI_Code);
						AssertEquals("Reference 3 Reference Number", "202303291601", ref3.CSI_ReferenceNumber);
						AssertEquals("Reference 3 Description", "Another Test 2", ref3.CSI_Description);

						AssertEquals("existingAddInfo3.IsDeleted - older was matched", true, existingAddInfo3.IsDeleted);
					});
				}
			}
		}

		static List<CustomsSupportingInformation> CreateAdditionalReferenceDataObjects()
		{
			return new List<CustomsSupportingInformation>(new[]
			{
				new CustomsSupportingInformation()
				{
					Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo },
					SubType = new CodeDescriptionPair5Char() { Code = "TRA" },
					Type = new CodeDescriptionPair6Char() { Code = "1D24" },
					ReferenceNumber = "202301060900",
					Description = "Test",
				},
				new CustomsSupportingInformation()
				{
					Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo },
					SubType = new CodeDescriptionPair5Char() { Code = "REF" },
					Type = new CodeDescriptionPair6Char() { Code = "5555" },
					ReferenceNumber = "202303291601",
					Description = "Another Test 2",
				},
				new CustomsSupportingInformation()
				{
					Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo },
					SubType = new CodeDescriptionPair5Char() { Code = "REF" },
					Type = new CodeDescriptionPair6Char() { Code = "4444" },
					ReferenceNumber = "202303291600",
					Description = "Another Test",
				}
			});
		}

		public void TestImportRequestedDocuments()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionRequestedDocumentsSupportConfigurationAndReturnMock(declaration, true), true))
			{
				var existingEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
				AssertEquals("RequestedDocumentsSupport", true, declaration.Configuration.InstructionConfiguration.RequestedDocumentsSupport(declaration));
				existingEntryInstruction.CEI_Style = "A";
				existingEntryInstruction.CEI_Description = "GREETING";

				var existingDoc1 = existingEntryInstruction.RequestedDocuments.AddNew();
				existingDoc1.CSI_DateOfIssue = ZDateTime.BrettsBirthday;
				existingDoc1.CSI_DateOfExpiry = ZDateTime.BrettsBirthday.AddDays(7);
				existingDoc1.CSI_Code = "5555";
				existingDoc1.CSI_Status = "OPE";
				existingDoc1.CSI_Description = "Test";

				var existingDoc2 = existingEntryInstruction.RequestedDocuments.AddNew();
				existingDoc2.CSI_DateOfIssue = ZDateTime.BrettsBirthday.AddDays(20);
				existingDoc2.CSI_DateOfExpiry = ZDateTime.BrettsBirthday.AddDays(30);
				existingDoc2.CSI_Code = "222";
				existingDoc2.CSI_Status = "CAN";
				existingDoc2.CSI_Description = "Test 3";
				existingDoc2.CSI_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddDays(1);

				var existingDoc3 = existingEntryInstruction.RequestedDocuments.AddNew();
				existingDoc3.CSI_DateOfIssue = ZDateTime.BrettsBirthday.AddDays(20);
				existingDoc3.CSI_DateOfExpiry = ZDateTime.BrettsBirthday.AddDays(30);
				existingDoc3.CSI_Code = "222";
				existingDoc3.CSI_Status = "CAN";
				existingDoc3.CSI_Description = "Test 3";
				existingDoc3.CSI_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddDays(1);

				Factory.SaveForTesting();

				CombineAssertions(() =>
				{
					var entryInstructionDataObject = new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance)
					{
						Style = "A",
						Description = "GREETING",
					};
					entryInstructionDataObject.SetCustomsSupportingInformationCollection(CreateRequestedDocumentDataObjects);

					var entryInstruction = GetEntryInstructionFromData(entryInstructionDataObject, declaration);
					AssertSame(existingEntryInstruction, entryInstruction);
					entryInstruction.RequestedDocuments.Load();
					var requestedDocuments = entryInstruction.RequestedDocuments.Cast<RequestedDocument>().ToArray();
					AssertEquals("Requested Documents count", 3, requestedDocuments.Length);

					RequestedDocument doc1 = null;
					RequestedDocument doc2 = null;
					RequestedDocument doc3 = null;
					foreach (var document in requestedDocuments)
					{
						switch (document.CSI_Code)
						{
							case "5555":
								doc1 = document;
								break;
							case "222":
								doc2 = document;
								break;
							default:
								doc3 = document;
								break;
						}
					}
					if (doc1.CSI_Status != "OPE")
					{
						doc1 = entryInstruction.RequestedDocuments[0];
						doc2 = entryInstruction.RequestedDocuments[1];
					}

					AssertSame("existingDoc1 - Matched 5555", existingDoc1, doc1);
					AssertEquals("Doc 1 Date of Request", ZDateTime.BrettsBirthday, doc1.CSI_DateOfIssue);
					AssertEquals("Doc 1 Provide by Date", ZDateTime.BrettsBirthday.AddDays(7), doc1.CSI_DateOfExpiry);
					AssertEquals("Doc 1 Type", "5555", doc1.CSI_Code);
					AssertEquals("Doc 1 Status", "OPE", doc1.CSI_Status);
					AssertEquals("Doc 1 Description", "Test", doc1.CSI_Description);

					AssertSame("existingDoc2 - Matched 222", existingDoc2, doc2);
					AssertEquals("Doc 2 Date of Request", ZDateTime.BrettsBirthday.AddDays(20), doc2.CSI_DateOfIssue);
					AssertEquals("Doc 2 Provide by Date", ZDateTime.BrettsBirthday.AddDays(30), doc2.CSI_DateOfExpiry);
					AssertEquals("Doc 2 Type", "222", doc2.CSI_Code);
					AssertEquals("Doc 2 Status", "CAN", doc2.CSI_Status);
					AssertEquals("Doc 2 Description", "Test 3", doc2.CSI_Description);

					AssertEquals("Doc 3 Date of Request", ZDateTime.BrettsBirthday.AddDays(19), doc3.CSI_DateOfIssue);
					AssertEquals("Doc 3 Provide by Date", ZDateTime.BrettsBirthday.AddDays(29), doc3.CSI_DateOfExpiry);
					AssertEquals("Doc 3 Type", "223", doc3.CSI_Code);
					AssertEquals("Doc 3 Status", "ADJ", doc3.CSI_Status);
					AssertEquals("Doc 3 Description", "Test 4", doc3.CSI_Description);

					AssertEquals("existingDoc3.IsDeleted - older was used", true, existingDoc3.IsDeleted);
				});
			}
		}

		public void TestDoNotImportRequestedDocumentsWhenNoSupported()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionRequestedDocumentsSupportConfigurationAndReturnMock(declaration, false), true))
			{
				var existingEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
				AssertEquals("RequestedDocumentsSupport", false, declaration.Configuration.InstructionConfiguration.RequestedDocumentsSupport(declaration));
				existingEntryInstruction.CEI_Style = "A";
				existingEntryInstruction.CEI_Description = "GREETING";

				var existingDoc1 = existingEntryInstruction.RequestedDocuments.AddNew();
				existingDoc1.CSI_DateOfIssue = ZDateTime.BrettsBirthday.AddDays(6);
				existingDoc1.CSI_DateOfExpiry = ZDateTime.BrettsBirthday.AddDays(8);
				existingDoc1.CSI_Code = "5555";
				existingDoc1.CSI_Status = "OP1";
				existingDoc1.CSI_Description = "Test 1";

				Factory.SaveForTesting();

				CombineAssertions(() =>
				{
					var entryInstructionDataObject = new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance)
					{
						Style = "A",
						Description = "GREETING",
					};
					entryInstructionDataObject.SetCustomsSupportingInformationCollection(CreateRequestedDocumentDataObjects);

					var entryInstruction = GetEntryInstructionFromData(entryInstructionDataObject, declaration);
					AssertSame(existingEntryInstruction, entryInstruction);
					AssertEquals("Requested Documents count", 1, entryInstruction.RequestedDocuments.Count);

					var doc1 = entryInstruction.RequestedDocuments[0];
					AssertSame("existingDoc1 - Matched 5555", existingDoc1, doc1);
					AssertEquals("Doc 1 Date of Request", ZDateTime.BrettsBirthday.AddDays(6), doc1.CSI_DateOfIssue);
					AssertEquals("Doc 1 Provide by Date", ZDateTime.BrettsBirthday.AddDays(8), doc1.CSI_DateOfExpiry);
					AssertEquals("Doc 1 Type", "5555", doc1.CSI_Code);
					AssertEquals("Doc 1 Status", "OP1", doc1.CSI_Status);
					AssertEquals("Doc 1 Description", "Test 1", doc1.CSI_Description);
				});
			}
		}

		public void TestImportRequestedDocuments_CustomsSupportingInformationCollectionWithNoRequestedDocumentData()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionRequestedDocumentsSupportConfigurationAndReturnMock(declaration, true), true))
			{
				var existingEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
				AssertEquals("RequestedDocumentsSupport", true, declaration.Configuration.InstructionConfiguration.RequestedDocumentsSupport(declaration));
				existingEntryInstruction.CEI_Style = "A";
				existingEntryInstruction.CEI_Description = "GREETING";

				var existingDoc1 = existingEntryInstruction.RequestedDocuments.AddNew();
				existingDoc1.CSI_DateOfIssue = ZDateTime.BrettsBirthday.AddDays(6);
				existingDoc1.CSI_DateOfExpiry = ZDateTime.BrettsBirthday.AddDays(8);
				existingDoc1.CSI_Code = "5555";
				existingDoc1.CSI_Status = "OP1";
				existingDoc1.CSI_Description = "Test 1";

				Factory.SaveForTesting();

				CombineAssertions(() =>
				{
					var entryInstructionDataObject = new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance)
					{
						Style = "A",
						Description = "GREETING",
					};
					entryInstructionDataObject.SetCustomsSupportingInformationCollection(() => new List<CustomsSupportingInformation>(new[]
					{
						new CustomsSupportingInformation { Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo } }
					}));

					var entryInstruction = GetEntryInstructionFromData(entryInstructionDataObject, declaration);
					AssertSame(existingEntryInstruction, entryInstruction);
					AssertEquals("Requested Documents count", 1, entryInstruction.RequestedDocuments.Count);

					var doc1 = entryInstruction.RequestedDocuments[0];
					AssertSame("existingDoc1 - Matched 5555", existingDoc1, doc1);
					AssertEquals("Doc 1 Date of Request", ZDateTime.BrettsBirthday.AddDays(6), doc1.CSI_DateOfIssue);
					AssertEquals("Doc 1 Provide by Date", ZDateTime.BrettsBirthday.AddDays(8), doc1.CSI_DateOfExpiry);
					AssertEquals("Doc 1 Type", "5555", doc1.CSI_Code);
					AssertEquals("Doc 1 Status", "OP1", doc1.CSI_Status);
					AssertEquals("Doc 1 Description", "Test 1", doc1.CSI_Description);
				});
			}
		}

		public void TestImportRequestedDocuments_CustomsSupportingInformationCollectionWithEmptyRequestedDocumentData()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionRequestedDocumentsSupportConfigurationAndReturnMock(declaration, true), true))
			{
				var existingEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
				AssertEquals("RequestedDocumentsSupport", true, declaration.Configuration.InstructionConfiguration.RequestedDocumentsSupport(declaration));
				existingEntryInstruction.CEI_Style = "A";
				existingEntryInstruction.CEI_Description = "GREETING";

				var existingDoc1 = existingEntryInstruction.RequestedDocuments.AddNew();
				existingDoc1.CSI_DateOfIssue = ZDateTime.BrettsBirthday.AddDays(6);
				existingDoc1.CSI_DateOfExpiry = ZDateTime.BrettsBirthday.AddDays(8);
				existingDoc1.CSI_Code = "5555";
				existingDoc1.CSI_Status = "OP1";
				existingDoc1.CSI_Description = "Test 1";

				Factory.SaveForTesting();

				CombineAssertions(() =>
				{
					var entryInstructionDataObject = new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance)
					{
						Style = "A",
						Description = "GREETING",
					};
					entryInstructionDataObject.SetCustomsSupportingInformationCollection(() => new List<CustomsSupportingInformation>(new[]
					{
						new CustomsSupportingInformation { Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.InstructionRequestedDocument } }
					}));

					var entryInstruction = GetEntryInstructionFromData(entryInstructionDataObject, declaration);
					AssertSame(existingEntryInstruction, entryInstruction);
					AssertEquals("Requested Documents count", 0, entryInstruction.RequestedDocuments.Count);
					AssertEquals("existingDoc1.IsDeleted", true, existingDoc1.IsDeleted);
				});
			}
		}

		static List<CustomsSupportingInformation> CreateRequestedDocumentDataObjects()
		{
			return new List<CustomsSupportingInformation>(new[]
			{
				new CustomsSupportingInformation()
				{
					Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.InstructionRequestedDocument },
					DateOfIssue = ZDateTime.BrettsBirthday,
					DateOfExpiry = ZDateTime.BrettsBirthday.AddDays(7),
					Type = new CodeDescriptionPair6Char() { Code = "5555" },
					Status = new CodeDescriptionPair() { Code = "OPE" },
					Description = "Test",
				},
				new CustomsSupportingInformation()
				{
					Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.InstructionRequestedDocument },
					DateOfIssue = ZDateTime.BrettsBirthday.AddDays(19),
					DateOfExpiry = ZDateTime.BrettsBirthday.AddDays(29),
					Type = new CodeDescriptionPair6Char() { Code = "223" },
					Status = new CodeDescriptionPair() { Code = "ADJ" },
					Description = "Test 4",
				},
				new CustomsSupportingInformation()
				{
					Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.InstructionRequestedDocument },
					DateOfIssue = ZDateTime.BrettsBirthday.AddDays(20),
					DateOfExpiry = ZDateTime.BrettsBirthday.AddDays(30),
					Type = new CodeDescriptionPair6Char() { Code = "222" },
					Status = new CodeDescriptionPair() { Code = "CAN" },
					Description = "Test 3",
				}
			});
		}

		public void TestImportGoodsLocationInfo()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				var declaration = Factory.New<JobDeclaration>();
				var existingEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
				existingEntryInstruction.CEI_Style = "A";
				existingEntryInstruction.CEI_Description = "GREETING";
				Factory.SaveForTesting();

				var entryInstructionDataObject = new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Style = "A",
					Description = "GREETING",
				};
				entryInstructionDataObject.SetLocationOfGoodsCollection(() => CreateLocationOfGoodsDataObjects("Z"));
				var entryInstruction = GetEntryInstructionFromData(entryInstructionDataObject, declaration);
				AssertSame(existingEntryInstruction, entryInstruction);

				CombineAssertions("Goods location in EntryInstruction", () =>
				{
					var goodsLocation = existingEntryInstruction.GoodsLocation;

					AssertEquals("Qualifier", "Z", goodsLocation.CGL_Qualifier);
					AssertEquals("Type", "B", goodsLocation.CGL_Type);
					AssertEquals("AuthorisationNumber", "A123", goodsLocation.Address.AuthorisationNumber);
					AssertEquals("AdditionalIdentifier", "Z99", goodsLocation.CGL_AdditionalIdentifier);
					AssertEquals("CGL_CustomsOffice", "IEDUB0001", goodsLocation.CGL_CustomsOffice);
					AssertEquals("Contact Name", "Bob", goodsLocation.Address.E2_Contact);
					AssertEquals("Contact Phone", "923", goodsLocation.Address.E2_Phone);
					AssertEquals("Contact Email", "bob@gmail.in", goodsLocation.Address.E2_Email);
				});
			}
		}

		public void TestImportGoodsLocationAddress()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				var declaration = Factory.New<JobDeclaration>();

				var existingEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
				existingEntryInstruction.CEI_Style = "A";
				existingEntryInstruction.CEI_Description = "GREETING";
				Factory.SaveForTesting();

				var entryInstructionDataObject = new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Style = "A",
					Description = "GREETING",
				};
				entryInstructionDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress()
					{
						AddressType = "LocationOfGoods",
						OrganizationCode = Org1.OH_Code,
						AddressShortCode = Org1.MainAddress.AddressCode,
					}
				});
				entryInstructionDataObject.SetLocationOfGoodsCollection(() => CreateLocationOfGoodsDataObjects("Y"));

				var entryInstruction = GetEntryInstructionFromData(entryInstructionDataObject, declaration);
				AssertSame(existingEntryInstruction, entryInstruction);
				var goodsLocation = existingEntryInstruction.GoodsLocation;
				CombineAssertions("When Qualifier = Y", () =>
				{
					AssertEquals("OrganisationPK", ZGuid.Empty, goodsLocation.Address.OrganisationPK);
					AssertEquals("IdentificationHolderPK", Org1.PK, goodsLocation.Address.IdentificationHolderPK);
				});

				entryInstructionDataObject.SetLocationOfGoodsCollection(() => CreateLocationOfGoodsDataObjects("Z"));
				var entryInstruction2 = GetEntryInstructionFromData(entryInstructionDataObject, declaration);
				AssertSame(existingEntryInstruction, entryInstruction2);
				goodsLocation = existingEntryInstruction.GoodsLocation;
				CombineAssertions("When Qualifier = Z", () =>
				{
					AssertEquals("OrganisationPK", Org1.PK, goodsLocation.Address.OrganisationPK);
					AssertEquals("E2_OA_Address", Org1.MainAddress.PK, goodsLocation.Address.E2_OA_Address);
					AssertEquals("IdentificationHolderPK", ZGuid.Empty, goodsLocation.Address.IdentificationHolderPK);
				});
			}
		}

		static List<LocationOfGoods> CreateLocationOfGoodsDataObjects(string qualifier)
		{
			return new List<LocationOfGoods>
			{
				new LocationOfGoods
				{
					Qualifier = new CodeDescriptionPair1Char { Code = qualifier },
					LocationType = new CodeDescriptionPair1Char { Code = "B", Description = "Authorized Place" },
					AuthorizationNumber = "A123",
					AdditionalIdentifier = "Z99",
					CustomsOffice = "IEDUB0001",
					Contact = new Contact
					{
						Name = "Bob",
						PhoneNumber = "923",
						Email = "bob@gmail.in",
					},
				}
			};
		}
	}
}
