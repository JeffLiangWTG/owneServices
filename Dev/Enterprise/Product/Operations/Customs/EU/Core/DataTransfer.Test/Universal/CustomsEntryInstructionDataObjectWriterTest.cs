using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;

namespace Enterprise.Customs.EU.DataTransfer.Universal.Testing
{
	sealed class CustomsEntryInstructionDataObjectWriterTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestGetEntryInstructionAddInfoCollection_ForTotalInnerPackages()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = "SEA";
			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_TotalInnerPackages = 10;

			var result = GetDataObject(instruction);

			AssertEquals("TotalInnerPackages value is expected to be same in AddInfoCollection with the value in CusEntryInstruction.", "10", result.AddInfoCollection.GetZStringValue(new ZString(Constants.AddInfoKeys.EntryInstruction.TotalInnerPackages)));
		}

		public void TestGetEntryInstructionAddInfoCollection_ForCustomsValueInformation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			var dv1Details1 = declaration.DV1Details.AddNew();
			var dv1Details2 = declaration.DV1Details.AddNew();

			var pivot2 = instruction.DV1DetailsPivots.Single(x => x.Sequence == 2);
			pivot2.IsForEntryInstruction = true;

			var result = GetDataObject(instruction);
			AssertEquals(
				"CustomsValueInformationLink should be the link of the selected DV1 Detail.",
				1,
				result.AddInfoCollection.GetZIntValue("CustomsValueInformationLink")
			);
		}

		public void TestExportPreviousDocuments()
		{
			SetupCusSupportingInfoCSI_TypeList(CusSupportingInfoTypeList.Codes.PreviousDocument);
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionPreviousDocumentsSupportConfigurationAndReturnMock(declaration, true), true))
			{
				AssertEquals("PreviousDocumentsSupport", true, declaration.Configuration.InstructionConfiguration.PreviousDocumentsSupport(declaration));
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var doc1Mock = Factory.NewMoq<PreviousDocument>();
				var doc1 = doc1Mock.Object;
				var doc1LookupsMock = new Mock<PreviousDocumentLookups>(doc1) { CallBase = true };
				doc1LookupsMock.Setup(x => x.CodeList).Returns(new MeansOfTransportList());
				doc1Mock.Protected().Setup<Customs.Business.CusSupportingInfoLookups>("GetNewLookups").Returns(doc1LookupsMock.Object);
				doc1.CSI_ParentID = entryInstruction.PK;
				doc1.CSI_ParentTableCode = entryInstruction.TablePrefix;
				entryInstruction.PreviousDocuments.Add(doc1);
				doc1.CSI_Code = MeansOfTransportList.Codes.WagonNumber;
				doc1.CSI_ReferenceNumber = "ABC123";
				doc1.CSI_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddDays(1);

				var doc2Mock = Factory.NewMoq<PreviousDocument>();
				var doc2 = doc2Mock.Object;
				var doc2LookupsMock = new Mock<PreviousDocumentLookups>(doc2) { CallBase = true };
				doc2LookupsMock.Setup(x => x.CodeList).Returns(new MeansOfTransportList());
				doc2Mock.Protected().Setup<Customs.Business.CusSupportingInfoLookups>("GetNewLookups").Returns(doc2LookupsMock.Object);
				doc2.CSI_ParentID = entryInstruction.PK;
				doc2.CSI_ParentTableCode = entryInstruction.TablePrefix;
				entryInstruction.PreviousDocuments.Add(doc2);
				doc2.CSI_Code = MeansOfTransportList.Codes.NameOfTheInlandWaterwaysVessel;
				doc2.CSI_ReferenceNumber = "DEF456";
				doc2.CSI_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;

				var dataObject = GetDataObject(entryInstruction);
				var previousDocDataObjects = dataObject.CustomsSupportingInformationCollection.Where(s => s.Category.Code.Value == CusSupportingInfoTypeList.Codes.PreviousDocument).ToArray();
				AssertEquals("Previous Document Count", 2, previousDocDataObjects.Length);
				AssertCustomsSupportingInformation("Previous Document 1",
					previousDocDataObjects[0],
					"PRE DESC",
					referenceNumber: "DEF456",
					type: new CodeDescriptionPair { Code = MeansOfTransportList.Codes.NameOfTheInlandWaterwaysVessel, Description = MeansOfTransportList.Descriptions.NameOfTheInlandWaterwaysVessel },
					subType: new CodeDescriptionPair { Code = ZString.Empty },
					dateOfExpiry: ZDateTime.Empty,
					dateOfIssue: ZDateTime.Empty,
					status: new CodeDescriptionPair { Code = ZString.Empty },
					packUnitOfQuantity: new CodeDescriptionPair { Code = ZString.Empty },
					unitOfQuantity3: new CodeDescriptionPair { Code = ZString.Empty },
					itemNumber: null);
				AssertCustomsSupportingInformation("Previous Document 2",
					previousDocDataObjects[1],
					"PRE DESC",
					referenceNumber: "ABC123",
					type: new CodeDescriptionPair { Code = MeansOfTransportList.Codes.WagonNumber, Description = MeansOfTransportList.Descriptions.WagonNumber },
					subType: new CodeDescriptionPair { Code = ZString.Empty },
					dateOfExpiry: ZDateTime.Empty,
					dateOfIssue: ZDateTime.Empty,
					status: new CodeDescriptionPair { Code = ZString.Empty },
					packUnitOfQuantity: new CodeDescriptionPair { Code = ZString.Empty },
					unitOfQuantity3: new CodeDescriptionPair { Code = ZString.Empty },
					itemNumber: null);
			}
		}

		public void TestExportPreviousDocuments_Empty()
		{
			SetupCusSupportingInfoCSI_TypeList(CusSupportingInfoTypeList.Codes.PreviousDocument);
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionPreviousDocumentsSupportConfigurationAndReturnMock(declaration, true), true))
			{
				AssertEquals("PreviousDocumentsSupport", true, declaration.Configuration.InstructionConfiguration.PreviousDocumentsSupport(declaration));
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var dataObject = GetDataObject(entryInstruction);
				var previousDocDataObjects = dataObject.CustomsSupportingInformationCollection.Where(s => s.Category.Code.Value == CusSupportingInfoTypeList.Codes.PreviousDocument).ToArray();
				AssertEquals("Previous Document Count", 1, previousDocDataObjects.Length);
				AssertCustomsSupportingInformation("Previous Document Empty", previousDocDataObjects[0], CusSupportingInfoTypeList.Descriptions.PreviousDocument, null, null, null, null, null, null, null, null, null, null, null, null, null);
			}
		}

		public void TestExportPreviousDocumentsUCC5()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				SetupCusSupportingInfoCSI_TypeList(CusSupportingInfoTypeList.Codes.PreviousDocument);
				var declaration = Factory.New<JobDeclaration>();
				using (ConfigurationTestHelper.TemporarySetupUCC5(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionPreviousDocumentsSupportConfigurationAndReturnMock(declaration, true), true))
				{
					AssertEquals("PreviousDocumentsSupport", true, declaration.Configuration.InstructionConfiguration.PreviousDocumentsSupport(declaration));
					var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

					var doc1 = entryInstruction.PreviousDocuments.AddNew();
					doc1.CSI_Code = MeansOfTransportList.Codes.WagonNumber;
					doc1.CSI_ReferenceNumber = "ABC123";
					doc1.CSI_AdditionalDescription = "Additional Desc:";

					var doc2 = entryInstruction.PreviousDocuments.AddNew();
					doc2.CSI_Code = MeansOfTransportList.Codes.NameOfTheInlandWaterwaysVessel;
					doc2.CSI_ReferenceNumber = "DEF456";

					var dataObject = GetDataObject(entryInstruction);
					var previousDocDataObjects = dataObject.CustomsSupportingInformationCollection.Where(s => s.Category.Code.Value == CusSupportingInfoTypeList.Codes.PreviousDocument).ToArray();
					CombineAssertions(() =>
					{
						AssertEquals("Previous Document Count", 2, previousDocDataObjects.Length);

						AssertEquals("Doc1 reference number", "ABC123", previousDocDataObjects[0].ReferenceNumber);
						AssertEquals("Doc1 type", MeansOfTransportList.Codes.WagonNumber, previousDocDataObjects[0].Type.Code);
						AssertEquals("Doc1 additional description", "Additional Desc:", previousDocDataObjects[0].AdditionalDescription);

						AssertEquals("Doc2 reference number", "DEF456", previousDocDataObjects[1].ReferenceNumber);
						AssertEquals("Doc2 type", MeansOfTransportList.Codes.NameOfTheInlandWaterwaysVessel, doc2.CSI_Code);
					});
				}
			}
		}

		public void TestDoNotExportPreviousDocumentsWhenNotSupported()
		{
			SetupCusSupportingInfoCSI_TypeList("ABC"); // should not contain PreviousDocument as it should be removed in DeclarationDataObjectWriter when not supported
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionPreviousDocumentsSupportConfigurationAndReturnMock(declaration, false), true))
			{
				AssertEquals("PreviousDocumentsSupport", false, declaration.Configuration.InstructionConfiguration.PreviousDocumentsSupport(declaration));
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var doc1 = entryInstruction.PreviousDocuments.AddNew();
				doc1.CSI_Code = "N380";
				doc1.CSI_ReferenceNumber = "ABC123";
				doc1.CSI_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
				var dataObject = GetDataObject(entryInstruction);
				var previousDocDataObjects = dataObject.CustomsSupportingInformationCollection.Where(s => s.Category.Code.Value == CusSupportingInfoTypeList.Codes.PreviousDocument).ToArray();
				AssertEquals("Previous Document Count", 0, previousDocDataObjects.Length);
			}
		}

		public void TestExportAdditionalInfos()
		{
			SetupCusSupportingInfoCSI_TypeList(CusSupportingInfoTypeList.Codes.AdditionalInfo);
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionAdditionalInfosSupportConfigurationAndReturnMock(declaration, true), true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				AssertEquals("AdditionalInfosSupport", true, declaration.Configuration.InstructionConfiguration.AdditionalInfosSupport(declaration, entryInstruction));

				var addInfo1Mock = Factory.NewMoq<AdditionalInfo>();
				var addInfo1 = addInfo1Mock.Object;
				var addInfo1LookupsMock = new Mock<AdditionalInfoLookups>(addInfo1) { CallBase = true };
				addInfo1LookupsMock.Setup(x => x.CodeList).Returns(new MeansOfTransportList());
				addInfo1LookupsMock.Setup(x => x.SubTypeList).Returns(new AdditionalInfoSubTypeList());
				addInfo1Mock.Protected().Setup<Customs.Business.CusSupportingInfoLookups>("GetNewLookups").Returns(addInfo1LookupsMock.Object);
				addInfo1.CSI_ParentID = entryInstruction.PK;
				addInfo1.CSI_ParentTableCode = entryInstruction.TablePrefix;
				entryInstruction.AdditionalInfos.Add(addInfo1);
				addInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				addInfo1.CSI_Code = MeansOfTransportList.Codes.ImoShipIdentificationNumber;
				addInfo1.CSI_ReferenceNumber = "202301060900";
				addInfo1.CSI_Description = "Test";
				addInfo1.CSI_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddDays(1);

				var addInfo2Mock = Factory.NewMoq<AdditionalInfo>();
				var addInfo2 = addInfo2Mock.Object;
				var addInfo2LookupsMock = new Mock<AdditionalInfoLookups>(addInfo2) { CallBase = true };
				addInfo2LookupsMock.Setup(x => x.CodeList).Returns(new MeansOfTransportList());
				addInfo2LookupsMock.Setup(x => x.SubTypeList).Returns(new AdditionalInfoSubTypeList());
				addInfo2Mock.Protected().Setup<Customs.Business.CusSupportingInfoLookups>("GetNewLookups").Returns(addInfo2LookupsMock.Object);
				addInfo2.CSI_ParentID = entryInstruction.PK;
				addInfo2.CSI_ParentTableCode = entryInstruction.TablePrefix;
				entryInstruction.AdditionalInfos.Add(addInfo2);
				addInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				addInfo2.CSI_Code = MeansOfTransportList.Codes.NameOfTheInlandWaterwaysVessel;
				addInfo2.CSI_ReferenceNumber = "202303291600";
				addInfo2.CSI_Description = "Another Test";
				addInfo2.CSI_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;

				var dataObject = GetDataObject(entryInstruction);
				var additionalDocumentDataObjects = dataObject.CustomsSupportingInformationCollection.Where(s => s.Category.Code.Value == CusSupportingInfoTypeList.Codes.AdditionalInfo).ToArray();
				AssertEquals("Additional Documents Count", 2, additionalDocumentDataObjects.Length);
				AssertCustomsSupportingInformation("Additional Document 1", additionalDocumentDataObjects[0], "OTH DESC",
					subType: new CodeDescriptionPair { Code = AdditionalInfoSubTypeList.Codes.AdditionalReference, Description = AdditionalInfoSubTypeList.Descriptions.AdditionalReference },
					type: new CodeDescriptionPair { Code = MeansOfTransportList.Codes.NameOfTheInlandWaterwaysVessel, Description = MeansOfTransportList.Descriptions.NameOfTheInlandWaterwaysVessel },
					referenceNumber: "202303291600",
					description: "Another Test",
					status: new CodeDescriptionPair { Code = AdditionalInfoIssuerList.Codes.Customs, Description = AdditionalInfoIssuerList.Descriptions.Customs },
					dateOfExpiry: ZDateTime.Empty,
					dateOfIssue: ZDateTime.Empty,
					packUnitOfQuantity: new CodeDescriptionPair { Code = ZString.Empty },
					unitOfQuantity3: new CodeDescriptionPair { Code = ZString.Empty },
					itemNumber: null);

				AssertCustomsSupportingInformation("Additional Document 2", additionalDocumentDataObjects[1], "OTH DESC",
					subType: new CodeDescriptionPair { Code = AdditionalInfoSubTypeList.Codes.TransportDocument, Description = AdditionalInfoSubTypeList.Descriptions.TransportDocument },
					type: new CodeDescriptionPair { Code = MeansOfTransportList.Codes.ImoShipIdentificationNumber, Description = MeansOfTransportList.Descriptions.ImoShipIdentificationNumber },
					referenceNumber: "202301060900",
					description: "Test",
					status: new CodeDescriptionPair { Code = AdditionalInfoIssuerList.Codes.Customs, Description = AdditionalInfoIssuerList.Descriptions.Customs },
					dateOfExpiry: ZDateTime.Empty,
					dateOfIssue: ZDateTime.Empty,
					packUnitOfQuantity: new CodeDescriptionPair { Code = ZString.Empty },
					unitOfQuantity3: new CodeDescriptionPair { Code = ZString.Empty },
					itemNumber: null);
			}
		}

		public void TestExportAdditionalInfos_Empty()
		{
			SetupCusSupportingInfoCSI_TypeList(CusSupportingInfoTypeList.Codes.AdditionalInfo);
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionAdditionalInfosSupportConfigurationAndReturnMock(declaration, true), true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				AssertEquals("AdditionalInfosSupport", true, declaration.Configuration.InstructionConfiguration.AdditionalInfosSupport(declaration, entryInstruction));

				var dataObject = GetDataObject(entryInstruction);
				var additionalDocumentDataObjects = dataObject.CustomsSupportingInformationCollection.Where(s => s.Category.Code.Value == CusSupportingInfoTypeList.Codes.AdditionalInfo).ToArray();
				AssertEquals("Additional Documents Count", 1, additionalDocumentDataObjects.Length);
				AssertCustomsSupportingInformation("Additional Document Empty", additionalDocumentDataObjects[0], CusSupportingInfoTypeList.Descriptions.AdditionalInfo, null, null, null, null, null, null, null, null, null, null, null, null, null);
			}
		}

		public void TestExportAdditionalInfosUCC5()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				SetupCusSupportingInfoCSI_TypeList(CusSupportingInfoTypeList.Codes.AdditionalInfo);
				var declaration = Factory.New<JobDeclaration>();
				using (ConfigurationTestHelper.TemporarySetupUCC5(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionAdditionalInfosSupportConfigurationAndReturnMock(declaration, true), true))
				{
					var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
					AssertEquals("AdditionalInfosSupport", true, declaration.Configuration.InstructionConfiguration.AdditionalInfosSupport(declaration, entryInstruction));

					var doc1 = entryInstruction.AdditionalInfos.AddNew();
					doc1.CSI_Code = MeansOfTransportList.Codes.ImoShipIdentificationNumber;
					doc1.CSI_ReferenceNumber = "ABC123";
					doc1.CSI_AdditionalDescription = "Additional Desc:";

					var doc2 = entryInstruction.AdditionalInfos.AddNew();
					doc2.CSI_Code = MeansOfTransportList.Codes.NameOfTheInlandWaterwaysVessel;
					doc2.CSI_ReferenceNumber = "DEF456";

					var dataObject = GetDataObject(entryInstruction);
					var additionalInfoDataObjects = dataObject.CustomsSupportingInformationCollection.Where(s => s.Category.Code.Value == CusSupportingInfoTypeList.Codes.AdditionalInfo).ToArray();
					CombineAssertions(() =>
					{
						AssertEquals("Previous Document Count", 2, additionalInfoDataObjects.Length);

						AssertEquals("Doc1 reference number", "ABC123", additionalInfoDataObjects[0].ReferenceNumber);
						AssertEquals("Doc1 type", MeansOfTransportList.Codes.ImoShipIdentificationNumber, additionalInfoDataObjects[0].Type.Code);
						AssertEquals("Doc1 additional description", "Additional Desc:", additionalInfoDataObjects[0].AdditionalDescription);

						AssertEquals("Doc2 reference number", "DEF456", additionalInfoDataObjects[1].ReferenceNumber);
						AssertEquals("Doc2 type", MeansOfTransportList.Codes.NameOfTheInlandWaterwaysVessel, additionalInfoDataObjects[1].Type.Code);
					});
				}
			}
		}

		public void TestDoNotExportAdditionalInfosWhenNotSupported()
		{
			SetupCusSupportingInfoCSI_TypeList(CusSupportingInfoTypeList.Codes.AdditionalInfo);
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionAdditionalInfosSupportConfigurationAndReturnMock(declaration, false), true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				AssertEquals("AdditionalInfosSupport", false, declaration.Configuration.InstructionConfiguration.AdditionalInfosSupport(declaration, entryInstruction));

				var addInfo1 = entryInstruction.AdditionalInfos.AddNew();
				addInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				addInfo1.CSI_Code = MeansOfTransportList.Codes.ImoShipIdentificationNumber;
				addInfo1.CSI_ReferenceNumber = "202301060900";
				addInfo1.CSI_Description = "Test";

				var dataObject = GetDataObject(entryInstruction);
				var additionalDocumentDataObjects = dataObject.CustomsSupportingInformationCollection.Where(s => s.Category.Code.Value == CusSupportingInfoTypeList.Codes.AdditionalInfo).ToArray();
				AssertEquals("Additional Documents Count", 0, additionalDocumentDataObjects.Length);
			}
		}

		public void TestExportSupportingDocuments()
		{
			SetupCusSupportingInfoCSI_TypeList(CusSupportingInfoTypeList.Codes.SupportingDocument);
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionSupportingDocumentsSupportConfigurationAndReturnMock(declaration, true), true))
			{
				AssertEquals("SupportingDocumentsSupport", true, declaration.Configuration.InstructionConfiguration.SupportingDocumentsSupport(declaration));
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

				var doc1Mock = Factory.NewMoq<SupportingDocument>();
				var doc1 = doc1Mock.Object;
				var doc1LookupsMock = new Mock<SupportingDocumentLookups>(doc1) { CallBase = true };
				doc1LookupsMock.Setup(x => x.CodeList).Returns(new MeansOfTransportList());
				doc1LookupsMock.Setup(x => x.PackTypeList).Returns(new QuantityUnitList());
				doc1LookupsMock.Setup(x => x.UnitOfQuantity3List).Returns(new QuantityUnitList());
				doc1Mock.Protected().Setup<Customs.Business.CusSupportingInfoLookups>("GetNewLookups").Returns(doc1LookupsMock.Object);
				doc1.CSI_ParentID = entryInstruction.PK;
				doc1.CSI_ParentTableCode = entryInstruction.TablePrefix;
				entryInstruction.SupportingDocuments.Add(doc1);
				doc1.CSI_AdditionalDescription = "Sample Issuing Authority 1";
				doc1.CSI_Code = MeansOfTransportList.Codes.IataFlightNumber;
				doc1.CSI_ItemNumber = 1;
				doc1.CSI_PackQty = 100;
				doc1.CSI_PackType = QuantityUnitList.Codes._30_Number;
				doc1.CSI_Quantity3 = 50;
				doc1.CSI_UnitOfQuantity3 = QuantityUnitList.Codes._9_NumberOfKits;
				doc1.CSI_ReferenceNumber = "TESTREF123";
				doc1.CSI_DateOfIssue = ZDateTime.BrettsBirthday;
				doc1.CSI_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddDays(1);

				var doc2Mock = Factory.NewMoq<SupportingDocument>();
				var doc2 = doc2Mock.Object;
				var doc2LookupsMock = new Mock<SupportingDocumentLookups>(doc2) { CallBase = true };
				doc2LookupsMock.Setup(x => x.CodeList).Returns(new MeansOfTransportList());
				doc2LookupsMock.Setup(x => x.PackTypeList).Returns(new QuantityUnitList());
				doc2LookupsMock.Setup(x => x.UnitOfQuantity3List).Returns(new QuantityUnitList());
				doc2Mock.Protected().Setup<Customs.Business.CusSupportingInfoLookups>("GetNewLookups").Returns(doc2LookupsMock.Object);
				doc2.CSI_ParentID = entryInstruction.PK;
				doc2.CSI_ParentTableCode = entryInstruction.TablePrefix;
				entryInstruction.SupportingDocuments.Add(doc2);
				doc2.CSI_AdditionalDescription = "Sample Issuing Authority 2";
				doc2.CSI_Code = MeansOfTransportList.Codes.RegistrationNumberOfTheAircraft;
				doc2.CSI_ItemNumber = 2;
				doc2.CSI_PackQty = 50;
				doc2.CSI_PackType = QuantityUnitList.Codes._97_KilowattHour;
				doc2.CSI_Quantity3 = 500;
				doc2.CSI_UnitOfQuantity3 = QuantityUnitList.Codes._83_CubicYard;
				doc2.CSI_ReferenceNumber = "TESTREF456";
				doc2.CSI_DateOfIssue = ZDateTime.BrettsBirthday.AddDays(4);
				doc2.CSI_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;

				var dataObject = GetDataObject(entryInstruction);
				var supportingDocDataObjects = dataObject.CustomsSupportingInformationCollection.Where(s => s.Category.Code.Value == CusSupportingInfoTypeList.Codes.SupportingDocument).ToArray();
				AssertEquals("Supporting document count", 2, supportingDocDataObjects.Length);

				AssertCustomsSupportingInformation("Doc 1", supportingDocDataObjects[0],
					"SUP DESC",
					additionalDescription: "Sample Issuing Authority 2",
					type: new CodeDescriptionPair { Code = MeansOfTransportList.Codes.RegistrationNumberOfTheAircraft, Description = MeansOfTransportList.Descriptions.RegistrationNumberOfTheAircraft },
					itemNumber: 2,
					packQuantity: 50,
					packUnitOfQuantity: new CodeDescriptionPair { Code = QuantityUnitList.Codes._97_KilowattHour, Description = QuantityUnitList.Descriptions._97_KilowattHour },
					quantity3: 500m,
					unitOfQuantity3: new CodeDescriptionPair { Code = QuantityUnitList.Codes._83_CubicYard, Description = QuantityUnitList.Descriptions._83_CubicYard },
					referenceNumber: "TESTREF456",
					dateOfIssue: ZDateTime.BrettsBirthday.AddDays(4),
					subType: new CodeDescriptionPair { Code = ZString.Empty },
					dateOfExpiry: ZDateTime.Empty,
					status: new CodeDescriptionPair { Code = ZString.Empty });

				AssertCustomsSupportingInformation("Doc 2", supportingDocDataObjects[1],
					"SUP DESC",
					additionalDescription: "Sample Issuing Authority 1",
					type: new CodeDescriptionPair { Code = MeansOfTransportList.Codes.IataFlightNumber, Description = MeansOfTransportList.Descriptions.IataFlightNumber },
					itemNumber: 1,
					packQuantity: 100,
					packUnitOfQuantity: new CodeDescriptionPair { Code = QuantityUnitList.Codes._30_Number, Description = QuantityUnitList.Descriptions._30_Number },
					quantity3: 50m,
					unitOfQuantity3: new CodeDescriptionPair { Code = QuantityUnitList.Codes._9_NumberOfKits, Description = QuantityUnitList.Descriptions._9_NumberOfKits },
					referenceNumber: "TESTREF123",
					dateOfIssue: ZDateTime.BrettsBirthday,
					subType: new CodeDescriptionPair { Code = ZString.Empty },
					dateOfExpiry: ZDateTime.Empty,
					status: new CodeDescriptionPair { Code = ZString.Empty });
			}
		}

		public void TestExportSupportingDocuments_Empty()
		{
			SetupCusSupportingInfoCSI_TypeList(CusSupportingInfoTypeList.Codes.SupportingDocument);
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionSupportingDocumentsSupportConfigurationAndReturnMock(declaration, true), true))
			{
				AssertEquals("SupportingDocumentsSupport", true, declaration.Configuration.InstructionConfiguration.SupportingDocumentsSupport(declaration));
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var dataObject = GetDataObject(entryInstruction);
				var supportingDocDataObjects = dataObject.CustomsSupportingInformationCollection.Where(s => s.Category.Code.Value == CusSupportingInfoTypeList.Codes.SupportingDocument).ToArray();
				AssertEquals("Supporting document count", 1, supportingDocDataObjects.Length);
				AssertCustomsSupportingInformation("Supporting Document Empty", supportingDocDataObjects[0], CusSupportingInfoTypeList.Descriptions.SupportingDocument, null, null, null, null, null, null, null, null, null, null, null, null, null);
			}
		}

		public void TestExportSupportingDocumentsUCC5()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				SetupCusSupportingInfoCSI_TypeList(CusSupportingInfoTypeList.Codes.SupportingDocument);
				var declaration = Factory.New<JobDeclaration>();
				using (ConfigurationTestHelper.TemporarySetupUCC5(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionSupportingDocumentsSupportConfigurationAndReturnMock(declaration, true), true))
				{
					AssertEquals("SupportingDocumentsSupport", true, declaration.Configuration.InstructionConfiguration.SupportingDocumentsSupport(declaration));
					var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

					var doc1 = entryInstruction.SupportingDocuments.AddNew();
					doc1.CSI_Code = MeansOfTransportList.Codes.IataFlightNumber;
					doc1.CSI_ReferenceNumber = "ABC123";
					doc1.CSI_AdditionalDescription = "Additional Desc:";

					var doc2 = entryInstruction.SupportingDocuments.AddNew();
					doc2.CSI_Code = MeansOfTransportList.Codes.RegistrationNumberOfTheAircraft;
					doc2.CSI_ReferenceNumber = "DEF456";

					var dataObject = GetDataObject(entryInstruction);
					var supportingDocDataObjects = dataObject.CustomsSupportingInformationCollection.Where(s => s.Category.Code.Value == CusSupportingInfoTypeList.Codes.SupportingDocument).ToArray();
					CombineAssertions(() =>
					{
						AssertEquals("Previous Document Count", 2, supportingDocDataObjects.Length);

						AssertEquals("Doc1 reference number", "ABC123", supportingDocDataObjects[0].ReferenceNumber);
						AssertEquals("Doc1 type", MeansOfTransportList.Codes.IataFlightNumber, supportingDocDataObjects[0].Type.Code);
						AssertEquals("Doc1 additional description", "Additional Desc:", supportingDocDataObjects[0].AdditionalDescription);

						AssertEquals("Doc2 reference number", "DEF456", supportingDocDataObjects[1].ReferenceNumber);
						AssertEquals("Doc2 type", MeansOfTransportList.Codes.RegistrationNumberOfTheAircraft, doc2.CSI_Code);
					});
				}
			}
		}

		public void TestDoNotExportSupportingDocumentsWhenNotSupported()
		{
			SetupCusSupportingInfoCSI_TypeList("ABC"); // should not contain SupportingDocument as it should be removed in DeclarationDataObjectWriter when not supported
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionSupportingDocumentsSupportConfigurationAndReturnMock(declaration, false), true))
			{
				AssertEquals("SupportingDocumentsSupport", false, declaration.Configuration.InstructionConfiguration.SupportingDocumentsSupport(declaration));
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

				var doc1 = entryInstruction.SupportingDocuments.AddNew();
				doc1.CSI_AdditionalDescription = "Sample Issuing Authority 1";
				doc1.CSI_Code = MeansOfTransportList.Codes.IataFlightNumber;
				doc1.CSI_ItemNumber = 1;
				doc1.CSI_PackQty = 100;
				doc1.CSI_UnitOfQuantity = QuantityUnitList.Codes._30_Number;
				doc1.CSI_Quantity3 = 50;
				doc1.CSI_UnitOfQuantity3 = QuantityUnitList.Codes._9_NumberOfKits;
				doc1.CSI_ReferenceNumber = "TESTREF123";
				doc1.CSI_DateOfExpiry = ZDateTime.BrettsBirthday;

				var dataObject = GetDataObject(entryInstruction);
				var supportingDocDataObjects = dataObject.CustomsSupportingInformationCollection.Where(s => s.Category.Code.Value == CusSupportingInfoTypeList.Codes.SupportingDocument).ToArray();
				AssertEquals("Supporting document count", 0, supportingDocDataObjects.Length);
			}
		}

		public void TestExportDocumentsRequested()
		{
			SetupCusSupportingInfoCSI_TypeList(CusSupportingInfoTypeList.Codes.InstructionRequestedDocument);
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionRequestedDocumentsSupportConfigurationAndReturnMock(declaration, true), true))
			{
				AssertEquals("RequestedDocumentsSupport", true, declaration.Configuration.InstructionConfiguration.RequestedDocumentsSupport(declaration));
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

				var requestedDoc1Mock = Factory.NewMoq<RequestedDocument>();
				var requestedDoc1 = requestedDoc1Mock.Object;
				var requestedDoc1LookupsMock = new Mock<RequestedDocumentLookups>(requestedDoc1) { CallBase = true };
				requestedDoc1LookupsMock.Setup(x => x.CodeList).Returns(new MeansOfTransportList());
				requestedDoc1LookupsMock.Setup(x => x.StatusList).Returns(new ExitItemStatusList());
				requestedDoc1Mock.Protected().Setup<Customs.Business.CusSupportingInfoLookups>("GetNewLookups").Returns(requestedDoc1LookupsMock.Object);
				requestedDoc1.CSI_ParentID = entryInstruction.PK;
				requestedDoc1.CSI_ParentTableCode = entryInstruction.TablePrefix;
				entryInstruction.RequestedDocuments.Add(requestedDoc1);
				requestedDoc1.CSI_Code = MeansOfTransportList.Codes.ImoShipIdentificationNumber;
				requestedDoc1.CSI_DateOfIssue = ZDateTime.BrettsBirthday;
				requestedDoc1.CSI_DateOfExpiry = ZDateTime.BrettsBirthday.AddDays(14);
				requestedDoc1.CSI_Status = ExitItemStatusList.Codes.COM;
				requestedDoc1.CSI_Description = "Test";
				requestedDoc1.CSI_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddDays(1);

				var requestedDoc2Mock = Factory.NewMoq<RequestedDocument>();
				var requestedDoc2 = requestedDoc2Mock.Object;
				var requestedDoc2LookupsMock = new Mock<RequestedDocumentLookups>(requestedDoc2) { CallBase = true };
				requestedDoc2LookupsMock.Setup(x => x.CodeList).Returns(new MeansOfTransportList());
				requestedDoc2LookupsMock.Setup(x => x.StatusList).Returns(new ExitItemStatusList());
				requestedDoc2Mock.Protected().Setup<Customs.Business.CusSupportingInfoLookups>("GetNewLookups").Returns(requestedDoc2LookupsMock.Object);
				requestedDoc2.CSI_ParentID = entryInstruction.PK;
				requestedDoc2.CSI_ParentTableCode = entryInstruction.TablePrefix;
				entryInstruction.RequestedDocuments.Add(requestedDoc2);
				requestedDoc2.CSI_Code = MeansOfTransportList.Codes.RegistrationNumberOfTheRoadVehicle;
				requestedDoc2.CSI_DateOfIssue = ZDateTime.BrettsBirthday.AddDays(31);
				requestedDoc2.CSI_DateOfExpiry = ZDateTime.BrettsBirthday.AddDays(40);
				requestedDoc2.CSI_Status = ExitItemStatusList.Codes.CAN;
				requestedDoc2.CSI_Description = "Test 2";
				requestedDoc2.CSI_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;

				var dataObject = GetDataObject(entryInstruction);
				var requestedDocDataObjects = dataObject.CustomsSupportingInformationCollection.Where(s => s.Category.Code.Value == CusSupportingInfoTypeList.Codes.InstructionRequestedDocument).ToArray();
				AssertEquals("Documents requested count", 2, requestedDocDataObjects.Length);

				AssertCustomsSupportingInformation("Requested Document 1", requestedDocDataObjects[0], "IRD DESC",
					type: new CodeDescriptionPair { Code = MeansOfTransportList.Codes.RegistrationNumberOfTheRoadVehicle, Description = MeansOfTransportList.Descriptions.RegistrationNumberOfTheRoadVehicle },
					dateOfIssue: ZDateTime.BrettsBirthday.AddDays(31),
					dateOfExpiry: ZDateTime.BrettsBirthday.AddDays(40),
					status: new CodeDescriptionPair { Code = ExitItemStatusList.Codes.CAN, Description = ExitItemStatusList.Descriptions.CAN },
					description: "Test 2",
					subType: new CodeDescriptionPair { Code = ZString.Empty },
					packUnitOfQuantity: new CodeDescriptionPair { Code = ZString.Empty },
					unitOfQuantity3: new CodeDescriptionPair { Code = ZString.Empty },
					itemNumber: null);

				AssertCustomsSupportingInformation("Requested Document 2", requestedDocDataObjects[1], "IRD DESC",
					type: new CodeDescriptionPair { Code = MeansOfTransportList.Codes.ImoShipIdentificationNumber, Description = MeansOfTransportList.Descriptions.ImoShipIdentificationNumber },
					dateOfIssue: ZDateTime.BrettsBirthday,
					dateOfExpiry: ZDateTime.BrettsBirthday.AddDays(14),
					status: new CodeDescriptionPair { Code = ExitItemStatusList.Codes.COM, Description = ExitItemStatusList.Descriptions.COM },
					description: "Test",
					subType: new CodeDescriptionPair { Code = ZString.Empty },
					packUnitOfQuantity: new CodeDescriptionPair { Code = ZString.Empty },
					unitOfQuantity3: new CodeDescriptionPair { Code = ZString.Empty },
					itemNumber: null);
			}
		}

		public void TestExportDocumentsRequested_Empty()
		{
			SetupCusSupportingInfoCSI_TypeList(CusSupportingInfoTypeList.Codes.InstructionRequestedDocument);
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionRequestedDocumentsSupportConfigurationAndReturnMock(declaration, true), true))
			{
				AssertEquals("RequestedDocumentsSupport", true, declaration.Configuration.InstructionConfiguration.RequestedDocumentsSupport(declaration));
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

				var dataObject = GetDataObject(entryInstruction);
				var requestedDocDataObjects = dataObject.CustomsSupportingInformationCollection.Where(s => s.Category.Code.Value == CusSupportingInfoTypeList.Codes.InstructionRequestedDocument).ToArray();
				AssertEquals("Documents requested count", 1, requestedDocDataObjects.Length);
				AssertCustomsSupportingInformation("Requested Document empty", requestedDocDataObjects[0], CusSupportingInfoTypeList.Descriptions.InstructionRequestedDocument, null, null, null, null, null, null, null, null, null, null, null, null, null);
			}
		}
		public void TestDoNotExportDocumentsRequestedWhenNotSupported()
		{
			SetupCusSupportingInfoCSI_TypeList("ABC"); // should not contain InstructionRequestedDocument as it should be removed in DeclarationDataObjectWriter when not supported
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionRequestedDocumentsSupportConfigurationAndReturnMock(declaration, false), true))
			{
				AssertEquals("RequestedDocumentsSupport", false, declaration.Configuration.InstructionConfiguration.RequestedDocumentsSupport(declaration));
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

				var requestedDoc1 = entryInstruction.RequestedDocuments.AddNew();
				requestedDoc1.CSI_Code = MeansOfTransportList.Codes.ImoShipIdentificationNumber;
				requestedDoc1.CSI_DateOfIssue = ZDateTime.BrettsBirthday;
				requestedDoc1.CSI_DateOfExpiry = ZDateTime.BrettsBirthday.AddDays(14);
				requestedDoc1.CSI_Status = ExitItemStatusList.Codes.COM;
				requestedDoc1.CSI_Description = "Test";

				var dataObject = GetDataObject(entryInstruction);
				var requestedDocDataObjects = dataObject.CustomsSupportingInformationCollection.Where(s => s.Category.Code.Value == CusSupportingInfoTypeList.Codes.InstructionRequestedDocument).ToArray();
				AssertEquals("Documents requested count", 0, requestedDocDataObjects.Length);
			}
		}

		public void TestExportCusSupplyChainActorReferences()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionAdditionalSupplyChainActorSupportConfiguration(declaration, true))
			{
				AssertEquals("AdditionalSupplyChainActorSupport", true, declaration.Configuration.InstructionConfiguration.AdditionalSupplyChainActorSupport(declaration));
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

				var actor1 = entryInstruction.CusSupplyChainActorReferences.AddNew();
				actor1.CFR_Code = Customs.Business.SupplyChainActorRoleList.Codes.CS;
				actor1.CFR_Reference = "REF123";
				var address1 = Factory.NewWithValidTestData<OrgAddress>();
				address1.CompanyName = "Test Company 1";
				address1.Address1 = "123 Test Street";
				address1.Address2 = "Town";
				address1.Postcode = "A12B3C4";
				actor1.CFR_OA_Owner = address1.PK;
				actor1.CFR_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddDays(1);

				var actor2 = entryInstruction.CusSupplyChainActorReferences.AddNew();
				actor2.CFR_Code = Customs.Business.SupplyChainActorRoleList.Codes.MF;
				actor2.CFR_Reference = "REF456";
				var address2 = Factory.NewWithValidTestData<OrgAddress>();
				address2.CompanyName = "Test Company 2";
				address2.Address1 = "456 Test Street";
				address2.Address2 = "City";
				address2.Postcode = "X98Y7Z6";
				actor2.CFR_OA_Owner = address2.PK;
				actor2.CFR_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;

				var dataObject = GetDataObject(entryInstruction);
				var actorDataObjects = dataObject.CustomsReferenceCollection.Where(c => c.Type.Code.Value == Customs.Business.CusReferenceTypeList.Codes.SupplyChainActor).ToArray();
				AssertEquals("CommercialInvoiceLine Supply Chain Actor count", 2, actorDataObjects.Length);

				CommercialInvoiceHeaderDataObjectWriterTest.AssertSupplyChainActor("Supply Chain Actor 1", actorDataObjects[0], new CodeDescriptionPair { Code = Customs.Business.SupplyChainActorRoleList.Codes.MF, Description = Customs.Business.SupplyChainActorRoleList.Descriptions.MF }, "REF456", address2);
				CommercialInvoiceHeaderDataObjectWriterTest.AssertSupplyChainActor("Supply Chain Actor 2", actorDataObjects[1], new CodeDescriptionPair { Code = Customs.Business.SupplyChainActorRoleList.Codes.CS, Description = Customs.Business.SupplyChainActorRoleList.Descriptions.CS }, "REF123", address1);
			}
		}

		public void TestExportCusSupplyChainActorReferencesEmpty()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionAdditionalSupplyChainActorSupportConfiguration(declaration, true))
			{
				AssertEquals("AdditionalSupplyChainActorSupport", true, declaration.Configuration.InstructionConfiguration.AdditionalSupplyChainActorSupport(declaration));
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

				var dataObject = GetDataObject(entryInstruction);
				var actorDataObjects = dataObject.CustomsReferenceCollection.Where(c => c.Type.Code.Value == Customs.Business.CusReferenceTypeList.Codes.SupplyChainActor).ToArray();
				AssertEquals("CommercialInvoiceLine Supply Chain Actor count", 1, actorDataObjects.Length);
				CommercialInvoiceHeaderDataObjectWriterTest.AssertSupplyChainActor("Supply Chain Actor Empty", actorDataObjects[0]);
			}
		}

		public void TestDoNotExportCusSupplyChainActorReferencesWhenNotSupported()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionAdditionalSupplyChainActorSupportConfiguration(declaration, false))
			{
				AssertEquals("AdditionalSupplyChainActorSupport", false, declaration.Configuration.InstructionConfiguration.AdditionalSupplyChainActorSupport(declaration));
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

				var actor1 = entryInstruction.CusSupplyChainActorReferences.AddNew();
				actor1.CFR_Code = Customs.Business.SupplyChainActorRoleList.Codes.CS;
				actor1.CFR_Reference = "REF123";
				var address1 = Factory.NewWithValidTestData<OrgAddress>();
				address1.CompanyName = "Test Company 1";
				address1.Address1 = "123 Test Street";
				address1.Address2 = "Town";
				address1.Postcode = "A12B3C4";
				actor1.CFR_OA_Owner = address1.PK;

				var dataObject = GetDataObject(entryInstruction);
				AssertNull(dataObject.CustomsReferenceCollection);
			}
		}

		public void TestPopulateGoodsLocationInfo()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));

				var declaration = Factory.New<JobDeclaration>();
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;

				entryInstruction.GoodsLocation.CGL_Qualifier = "Z";
				entryInstruction.GoodsLocation.CGL_Type = "B";
				entryInstruction.GoodsLocation.CGL_AdditionalIdentifier = "IT9843";
				entryInstruction.GoodsLocation.CGL_CustomsOffice = "IEDUB0001";
				entryInstruction.GoodsLocation.Address.AuthorisationNumber = "ABC112";
				entryInstruction.GoodsLocation.Address.E2_Contact = "Bob";
				entryInstruction.GoodsLocation.Address.E2_Phone = "9213";
				entryInstruction.GoodsLocation.Address.E2_Email = "reach@email.com";

				var dataObject = GetDataObject(entryInstruction);

				CombineAssertions("Location of Goods collection", () =>
				{
					var goodsLocation = dataObject.LocationOfGoodsCollection[0];
					AssertEquals("Qualifier", "Z", goodsLocation.Qualifier.Code);
					AssertEquals("Qualifier - Description", "Address", goodsLocation.Qualifier.Description);
					AssertEquals("LocationType", "B", goodsLocation.LocationType.Code);
					AssertEquals("LocationType - Description", "Authorized Place", goodsLocation.LocationType.Description);
					AssertEquals("AdditionalIdentifier", "IT9843", goodsLocation.AdditionalIdentifier);
					AssertEquals("CustomsOffice", "IEDUB0001", goodsLocation.CustomsOffice);
					AssertEquals("AuthorizationNumber", "ABC112", goodsLocation.AuthorizationNumber);
					AssertEquals("Contact Name", "Bob", goodsLocation.Contact.Name);
					AssertEquals("Contact PhoneNumber", "9213", goodsLocation.Contact.PhoneNumber);
					AssertEquals("Contact Email", "reach@email.com", goodsLocation.Contact.Email);
				});

				var orgHeader = Factory.New<OrgHeader>();
				var orgAddress = orgHeader.MainAddress;
				orgAddress.OA_Address1 = "Add-line1";
				orgAddress.OA_Address2 = "Add-line2";
				orgAddress.AddressCode = "ADD1";
				orgAddress.OA_City = "Dublin";
				orgAddress.CompanyName = "Comp Name";
				orgAddress.OA_RN_NKCountryCode = "IE";
				entryInstruction.GoodsLocation.Address.E2_OA_Address = orgAddress.PK;

				dataObject = GetDataObject(entryInstruction);

				CombineAssertions("Location of Goods address", () =>
				{
					var goodsLocationAddress = dataObject.OrganizationAddressCollection.FirstOrDefault(x => (x.AddressType ?? ZString.Empty) == "LocationOfGoods");
					AssertNotNull("Address of Location of Goods", goodsLocationAddress);
					AssertEquals("Address1", orgAddress.OA_Address1, goodsLocationAddress.Address1);
					AssertEquals("Address2", orgAddress.OA_Address2, goodsLocationAddress.Address2);
					AssertEquals("AddressOverride", false, goodsLocationAddress.AddressOverride);
					AssertEquals("AddressShortCode", orgAddress.AddressCode, goodsLocationAddress.AddressShortCode);
					AssertEquals("City", orgAddress.OA_City, goodsLocationAddress.City);
					AssertEquals("CompanyName", orgAddress.CompanyName, goodsLocationAddress.CompanyName);
					AssertEquals("Country Code", orgAddress.OA_RN_NKCountryCode, goodsLocationAddress.Country.Code);
					AssertEquals("Country Name", orgAddress.Country.RN_Desc, goodsLocationAddress.Country.Name);
				});
			}
		}

		public static void AssertCustomsSupportingInformation(string message, CustomsSupportingInformation supportingInformation, string categoryDescription, ICodeDescriptionDataObject type = null, ICodeDescriptionDataObject subType = null,
			string referenceNumber = "", ZDateTime? dateOfIssue = null, ZDateTime? dateOfExpiry = null, string description = "", ICodeDescriptionDataObject status = null, string additionalDescription = "",
			int? itemNumber = 0, int? packQuantity = 0, ICodeDescriptionDataObject packUnitOfQuantity = null, decimal? quantity3 = 0m, ICodeDescriptionDataObject unitOfQuantity3 = null)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("Category.Description", categoryDescription, supportingInformation.Category.Description);
				AssertEquals("Type.Code", type?.Code, supportingInformation.Type?.Code);
				AssertEquals("Type.Description", type?.Description, supportingInformation.Type?.Description);
				AssertEquals("SubType.Code", subType?.Code, supportingInformation.SubType?.Code);
				AssertEquals("SubType.Description", subType?.Description, supportingInformation.SubType?.Description);
				AssertEquals("ReferenceNumber", referenceNumber, supportingInformation.ReferenceNumber);
				AssertEquals("DateOfExpiry", dateOfExpiry, supportingInformation.DateOfExpiry);
				AssertEquals("DateOfIssue", dateOfIssue, supportingInformation.DateOfIssue);
				AssertEquals("Description", description, supportingInformation.Description);
				AssertEquals("Status.Code", status?.Code, supportingInformation.Status?.Code);
				AssertEquals("Status.Description", status?.Description, supportingInformation.Status?.Description);
				AssertEquals("AdditionalDescription", additionalDescription, supportingInformation.AdditionalDescription);
				AssertEquals("ItemNumber", itemNumber, supportingInformation.ItemNumber);
				AssertEquals("PackQuantity", packQuantity, supportingInformation.PackQuantity);
				AssertEquals("PackUnitOfQuantity.Code", packUnitOfQuantity?.Code, supportingInformation.PackUnitOfQuantity?.Code);
				AssertEquals("PackUnitOfQuantity.Description", packUnitOfQuantity?.Description, supportingInformation.PackUnitOfQuantity?.Description);
				AssertEquals("Quantity3", quantity3, supportingInformation.Quantity3);
				AssertEquals("UnitOfQuantity3.Code", unitOfQuantity3?.Code, supportingInformation.UnitOfQuantity3?.Code);
				AssertEquals("UnitOfQuantity3.Description", unitOfQuantity3?.Description, supportingInformation.UnitOfQuantity3?.Description);
			});
		}

		void SetupCusSupportingInfoCSI_TypeList(params string[] codes)
		{
			var cacheKey = UniversalCommonHelper.GetCusSupportingInfoCSI_TypeListCacheKey(Core.Constants.CountryCodes.Latvia, CusEntryInstructionSchema.Constants.Prefix, string.Empty);
			var list = new CodeDescriptionPairList();
			codes.ForEach(x => list.AddPair(x, x + " DESC"));
			Factory.BOFactory.ClearCachedValue<ICodeDescriptionPairList>(cacheKey);
			_ = Factory.BOFactory.GetCachedValue<ICodeDescriptionPairList>(cacheKey, () => list);
		}

		EntryInstruction GetDataObject(CusEntryInstruction entryInstruction)
		{
			var writer = new CustomsEntryInstructionDataObjectWriter(new DataWritingManager(new ActionInfo(null, entryInstruction)), new UniversalDataObjectWriterHelper(entryInstruction.Factory, Core.Constants.CountryCodes.Latvia));
			return writer.GetDataObject(entryInstruction);
		}
	}
}
