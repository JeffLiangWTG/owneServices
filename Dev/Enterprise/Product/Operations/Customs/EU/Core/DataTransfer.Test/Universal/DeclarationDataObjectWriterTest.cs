using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.AirManifest;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.DataTransfer.Universal.Testing
{
	sealed class DeclarationDataObjectWriterTest : DeclarationDataObjectWriterAbstractTest<JobDeclaration, DeclarationDataObjectWriter>
	{
		public void TestUniversalDataObjectWriterHelper()
		{
			var declaration = Factory.BOFactory.New<JobDeclaration>();
			AssertType<UniversalDataObjectWriterHelper>(new DeclarationDataObjectWriterForTest(new DataWritingManager(new ActionInfo(null, declaration))).CreateNewUniversalDataObjectWriterHelper(declaration));
		}

		public void TestDontEditCachedList()
		{
			var cusSupportingInfoTypeListProviders = new CargoWise.Application.KeyObjectHandleDictionaryObject
			{
				{ Core.Constants.CountryCodes.Latvia, new CargoWise.Application.Testing.TestObjectHandle(new CusSupportingInfoTypeListProviderForTest()) }
			};
			using (CargoWise.Application.ObjectFactory.Substitute("UniversalCustomsDataObjectProviders", cusSupportingInfoTypeListProviders))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				var declaration = Factory.BOFactory.New<JobDeclaration>();
				var writer = new DeclarationDataObjectWriterForTest(new DataWritingManager(new ActionInfo(null, declaration)));
				var helper = writer.CreateNewUniversalDataObjectWriterHelper(declaration);
				AssertNotNull(helper);
			}
		}

		public void TestPopulateCustomsReferenceFromCusReferenceOnEntryInstructionLevel()
		{
			var mainAddressPK = CustomsReferenceDataObjectReaderTest.CreateTestOrgAddress(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var fiscalReference = entryInstruction.FiscalReferences.AddNew();
				fiscalReference.CFR_Code = "FR3";
				fiscalReference.CFR_OA_Owner = mainAddressPK;
				fiscalReference.CFR_Reference = "REF3232";
				Factory.SaveForTesting();

				var manager = declaration.GetUniversalDataContextManager() as IShipmentDataContextManager;
				var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
				using (((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator())
				{
					var shipmentData = (Shipment)writer.GetDataObject(declaration);
					CombineAssertions(() =>
					{
						var additionalCustomsReference = shipmentData.EntryInstructionCollection.SingleOrDefault().CustomsReferenceCollection.SingleOrDefault(cr => cr.Type.Code.GetValueOrDefault() == "FIS");
						AssertNotNull("AdditionalCustomsReference", additionalCustomsReference);
						AssertEquals("AdditionalCustomsReference.Code", "FR3", additionalCustomsReference.SubType.Code);
						AssertEquals("AdditionalCustomsReference.Reference", "REF3232", additionalCustomsReference.Reference);
						AssertEquals("AdditionalCustomsReference.Owner", "TESTORG", additionalCustomsReference.Owner.OrganizationCode);
					});
				}
			}
		}

		public void TestPopulateDocumentsRequested()
		{
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
				requestedDoc1Mock.Protected().Setup<CusSupportingInfoLookups>("GetNewLookups").Returns(requestedDoc1LookupsMock.Object);
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
				requestedDoc2Mock.Protected().Setup<CusSupportingInfoLookups>("GetNewLookups").Returns(requestedDoc2LookupsMock.Object);
				requestedDoc2.CSI_ParentID = entryInstruction.PK;
				requestedDoc2.CSI_ParentTableCode = entryInstruction.TablePrefix;
				entryInstruction.RequestedDocuments.Add(requestedDoc2);
				requestedDoc2.CSI_Code = MeansOfTransportList.Codes.RegistrationNumberOfTheRoadVehicle;
				requestedDoc2.CSI_DateOfIssue = ZDateTime.BrettsBirthday.AddDays(31);
				requestedDoc2.CSI_DateOfExpiry = ZDateTime.BrettsBirthday.AddDays(40);
				requestedDoc2.CSI_Status = ExitItemStatusList.Codes.CAN;
				requestedDoc2.CSI_Description = "Test 2";
				requestedDoc2.CSI_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;

				Factory.SaveForTesting();

				var manager = declaration.GetUniversalDataContextManager() as IShipmentDataContextManager;
				var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
				using (((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator())
				{
					var shipmentData = (Shipment)writer.GetDataObject(declaration);
					var dntryInstructionDataObject = shipmentData.EntryInstructionCollection.SingleOrDefault();
					var requestedDocDataObjects = dntryInstructionDataObject.CustomsSupportingInformationCollection.Where(s => s.Category.Code.Value == CusSupportingInfoTypeList.Codes.InstructionRequestedDocument).ToArray();
					AssertEquals("Documents requested count", 2, requestedDocDataObjects.Length);

					AssertCustomsSupportingInformation_RequestedDocument("Requested Document 1", requestedDocDataObjects[0], CusSupportingInfoTypeList.Descriptions.InstructionRequestedDocument,
						type: new CodeDescriptionPair { Code = MeansOfTransportList.Codes.RegistrationNumberOfTheRoadVehicle, Description = MeansOfTransportList.Descriptions.RegistrationNumberOfTheRoadVehicle },
						dateOfIssue: ZDate.BrettsBirthday.AddDays(31),
						dateOfExpiry: ZDate.BrettsBirthday.AddDays(40),
						status: new CodeDescriptionPair { Code = ExitItemStatusList.Codes.CAN, Description = ExitItemStatusList.Descriptions.CAN },
						description: "Test 2");

					AssertCustomsSupportingInformation_RequestedDocument("Requested Document 2", requestedDocDataObjects[1], CusSupportingInfoTypeList.Descriptions.InstructionRequestedDocument,
						type: new CodeDescriptionPair { Code = MeansOfTransportList.Codes.ImoShipIdentificationNumber, Description = MeansOfTransportList.Descriptions.ImoShipIdentificationNumber },
						dateOfIssue: ZDate.BrettsBirthday,
						dateOfExpiry: ZDate.BrettsBirthday.AddDays(14),
						status: new CodeDescriptionPair { Code = ExitItemStatusList.Codes.COM, Description = ExitItemStatusList.Descriptions.COM },
						description: "Test");
				}
			}
		}

		void AssertCustomsSupportingInformation_RequestedDocument(string message, CustomsSupportingInformation supportingInformation, string categoryDescription, ICodeDescriptionDataObject type, ZDateTime? dateOfIssue, ZDateTime? dateOfExpiry, ZString? description, ICodeDescriptionDataObject status)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("Category.Description", categoryDescription, supportingInformation.Category.Description);

				if (type == null)
				{
					AssertNull("Type", supportingInformation.Type);
				}
				else
				{
					AssertEquals("Type.Code", type.Code, supportingInformation.Type.Code);
					AssertEquals("Type.Description", type.Description, supportingInformation.Type.Description);
				}
				AssertEquals("DateOfExpiry", dateOfExpiry, supportingInformation.DateOfExpiry);
				AssertEquals("DateOfIssue", dateOfIssue, supportingInformation.DateOfIssue);
				AssertEquals("Description", description, supportingInformation.Description);
				if (status == null)
				{
					AssertNull("Status", supportingInformation.Status);
				}
				else
				{
					AssertEquals("Status.Code", status.Code, supportingInformation.Status.Code);
					AssertEquals("Status.Description", status.Description, supportingInformation.Status.Description);
				}
			});
		}

		public void TestPopulateCustomsReferenceFromCusAuthorisationUsageOnEntryInstructionLevel()
		{
			var mainAddressPK = CustomsReferenceDataObjectReaderTest.CreateTestOrg(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var cusAuthorisationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
				cusAuthorisationUsage.AGC_Code = "ABC";
				cusAuthorisationUsage.AGC_OH_Owner = mainAddressPK;
				cusAuthorisationUsage.AGC_Number = "12345";
				Factory.SaveForTesting();

				var manager = declaration.GetUniversalDataContextManager() as IShipmentDataContextManager;
				var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
				using (((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator())
				{
					var shipmentData = (Shipment)writer.GetDataObject(declaration);
					CombineAssertions(() =>
					{
						var additionalCustomsReference = shipmentData.EntryInstructionCollection.SingleOrDefault().CustomsReferenceCollection.SingleOrDefault(cr => cr.Type.Code.GetValueOrDefault() == "AUT");
						AssertNotNull("AdditionalCustomsReference", additionalCustomsReference);
						AssertEquals("Code", "ABC", additionalCustomsReference.SubType.Code);
						AssertEquals("12345", additionalCustomsReference.Reference);
						AssertEquals("TESTORG", additionalCustomsReference.Owner.OrganizationCode);
					});
				}
			}
		}

		public void TestPopulateCustomsReferenceWithNullOwnerFromCusAuthorisationUsageOnEntryInstructionLevel()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var cusAuthorisationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
				cusAuthorisationUsage.AGC_Code = "ABC";
				cusAuthorisationUsage.AGC_Number = "12345";
				Factory.SaveForTesting();

				var manager = declaration.GetUniversalDataContextManager() as IShipmentDataContextManager;
				var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
				using (((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator())
				{
					AssertNoExceptionThrown(() =>
					{
						var shipmentData = (Shipment)writer.GetDataObject(declaration);
						var additionalCustomsReference = shipmentData.EntryInstructionCollection.SingleOrDefault().CustomsReferenceCollection.SingleOrDefault();
						AssertNull("additionalCustomsReference.Owner", additionalCustomsReference.Owner);
					});
				}
			}
		}

		public void TestPopulateDV1Data()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
				var declaration = Factory.BOFactory.New<JobDeclaration>();
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				var dv1Detail1 = declaration.DV1Details.AddNew();
				dv1Detail1.DV1_Relationship = "N";
				dv1Detail1.DV1_PriceInfluence = "Y";
				dv1Detail1.DV1_RelationDetails = "Relation details";
				dv1Detail1.DV1_Restrictions = "N";
				dv1Detail1.DV1_Consideration = "N";
				dv1Detail1.DV1_RestrictionConsiderationDetails = "Consideration details";
				dv1Detail1.DV1_RoyaltiesLicence = "Y";
				dv1Detail1.DV1_RoyaltiesLicenceDetails = "Royalties details";
				dv1Detail1.DV1_Resale = "Y";
				dv1Detail1.DV1_ResaleDetails = "Resale details";
				dv1Detail1.DV1_CustomsDecisionNumber = "DES123";

				var dv1Detail2 = declaration.DV1Details.AddNew();
				dv1Detail2.DV1_Relationship = "Y";
				dv1Detail2.DV1_PriceInfluence = "N";
				dv1Detail2.DV1_RelationDetails = "Relation details 2";
				dv1Detail2.DV1_Restrictions = "N";
				dv1Detail2.DV1_Consideration = "Y";
				dv1Detail2.DV1_RestrictionConsiderationDetails = "Consideration details 2";
				dv1Detail2.DV1_RoyaltiesLicence = "Y";
				dv1Detail2.DV1_RoyaltiesLicenceDetails = "Royalties details 2";
				dv1Detail2.DV1_Resale = "N";
				dv1Detail2.DV1_ResaleDetails = "Resale details 2";
				dv1Detail2.DV1_CustomsDecisionNumber = "DES456";

				var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(null, declaration)));
				var shipment = writer.GetDataObject(declaration);

				CombineAssertions(() =>
				{
					AssertEquals("Number of Customs Value Information", 2, shipment.CustomsValueInformationCollection.Count);

					var customsValueInformation1 = shipment.CustomsValueInformationCollection[0];
					AssertEquals("LinkID", 1, customsValueInformation1.Link);
					AssertEquals("Number of details in first Customs Value Information", 9, customsValueInformation1.CustomsValueDetailCollection.Count);

					var relationshipInfo = customsValueInformation1.CustomsValueDetailCollection.Single(x => x.Type.GetValueOrDefault() == Constants.CustomsValueTypes.Relationship);
					AssertEquals("Relationship Code", "N", relationshipInfo.Code);

					var priceInfluenceInfo = customsValueInformation1.CustomsValueDetailCollection.Single(x => x.Type.GetValueOrDefault() == Constants.CustomsValueTypes.PriceInfluence);
					AssertEquals("Price Influence Code", "Y", priceInfluenceInfo.Code);

					var relationDetailsInfo = customsValueInformation1.CustomsValueDetailCollection.Single(x => x.Type.GetValueOrDefault() == Constants.CustomsValueTypes.RelationDetails);
					AssertEquals("RelationDetails Details", "Relation details", relationDetailsInfo.Details);

					var restrictionsInfo = customsValueInformation1.CustomsValueDetailCollection.Single(x => x.Type.GetValueOrDefault() == Constants.CustomsValueTypes.Restrictions);
					AssertEquals("Restrictions Code", "N", restrictionsInfo.Code);

					var considerationInfo = customsValueInformation1.CustomsValueDetailCollection.Single(x => x.Type.GetValueOrDefault() == Constants.CustomsValueTypes.Consideration);
					AssertEquals("Consideration Code", "N", considerationInfo.Code);

					var restrictionConsiderationDetailsInfo = customsValueInformation1.CustomsValueDetailCollection.Single(x => x.Type.GetValueOrDefault() == Constants.CustomsValueTypes.RestrictionConsiderationDetails);
					AssertEquals("Restriction Consideration Details Details", "Consideration details", restrictionConsiderationDetailsInfo.Details);

					var royaltiesLicenseInfo = customsValueInformation1.CustomsValueDetailCollection.Single(x => x.Type.GetValueOrDefault() == Constants.CustomsValueTypes.RoyaltiesLicence);
					AssertEquals("RoyaltiesLicence Code", "Y", royaltiesLicenseInfo.Code);
					AssertEquals("RoyaltiesLicence Details", "Royalties details", royaltiesLicenseInfo.Details);

					var resaleInfo = customsValueInformation1.CustomsValueDetailCollection.Single(x => x.Type.GetValueOrDefault() == Constants.CustomsValueTypes.Resale);
					AssertEquals("Resale Code", "Y", resaleInfo.Code);
					AssertEquals("Resale Details", "Resale details", resaleInfo.Details);

					var decisionNumberInfo = customsValueInformation1.CustomsValueDetailCollection.Single(x => x.Type.GetValueOrDefault() == Constants.CustomsValueTypes.DecisionNumber);
					AssertEquals("Decision Number Details", "DES123", decisionNumberInfo.Details);

					var customsValueInformation2 = shipment.CustomsValueInformationCollection[1];
					AssertEquals("LinkID", 2, customsValueInformation2.Link);
					AssertEquals("Number of details in second Customs Value Information", 9, customsValueInformation2.CustomsValueDetailCollection.Count);

					relationshipInfo = customsValueInformation2.CustomsValueDetailCollection.Single(x => x.Type.GetValueOrDefault() == Constants.CustomsValueTypes.Relationship);
					AssertEquals("Relationship Code 2", "Y", relationshipInfo.Code);

					priceInfluenceInfo = customsValueInformation2.CustomsValueDetailCollection.Single(x => x.Type.GetValueOrDefault() == Constants.CustomsValueTypes.PriceInfluence);
					AssertEquals("Price Influence Code 2", "N", priceInfluenceInfo.Code);

					relationDetailsInfo = customsValueInformation2.CustomsValueDetailCollection.Single(x => x.Type.GetValueOrDefault() == Constants.CustomsValueTypes.RelationDetails);
					AssertEquals("RelationDetails Details 2", "Relation details 2", relationDetailsInfo.Details);

					restrictionsInfo = customsValueInformation2.CustomsValueDetailCollection.Single(x => x.Type.GetValueOrDefault() == Constants.CustomsValueTypes.Restrictions);
					AssertEquals("Restrictions Code 2", "N", restrictionsInfo.Code);

					considerationInfo = customsValueInformation2.CustomsValueDetailCollection.Single(x => x.Type.GetValueOrDefault() == Constants.CustomsValueTypes.Consideration);
					AssertEquals("Consideration Code 2", "Y", considerationInfo.Code);

					restrictionConsiderationDetailsInfo = customsValueInformation2.CustomsValueDetailCollection.Single(x => x.Type.GetValueOrDefault() == Constants.CustomsValueTypes.RestrictionConsiderationDetails);
					AssertEquals("RestrictionConsiderationDetails Details 2", "Consideration details 2", restrictionConsiderationDetailsInfo.Details);

					royaltiesLicenseInfo = customsValueInformation2.CustomsValueDetailCollection.Single(x => x.Type.GetValueOrDefault() == Constants.CustomsValueTypes.RoyaltiesLicence);
					AssertEquals("RoyaltiesLicence Code 2", "Y", royaltiesLicenseInfo.Code);
					AssertEquals("RoyaltiesLicence Details 2", "Royalties details 2", royaltiesLicenseInfo.Details);

					resaleInfo = customsValueInformation2.CustomsValueDetailCollection.Single(x => x.Type.GetValueOrDefault() == Constants.CustomsValueTypes.Resale);
					AssertEquals("Resale Code 2", "N", resaleInfo.Code);
					AssertEquals("Resale Details 2", "Resale details 2", resaleInfo.Details);

					decisionNumberInfo = customsValueInformation2.CustomsValueDetailCollection.Single(x => x.Type.GetValueOrDefault() == Constants.CustomsValueTypes.DecisionNumber);
					AssertEquals("Decision Number Details 2", "DES456", decisionNumberInfo.Details);
				});
			}
		}

		public void TestPopulateCountrySpecificContainerValue()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
				var declaration = Factory.BOFactory.New<JobDeclaration>();
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				declaration.JE_TransportModeInland = "ROA";
				declaration.JE_TransportIDInland = "12345";
				declaration.JE_RN_NKTransportNationalityInland = "IE";
				declaration.JE_Trailer1RegNo = "24680";
				declaration.JE_RN_NKTrailer1Nationality = "GB";
				declaration.JE_Trailer2RegNo = "98765";
				declaration.JE_RN_NKTrailer2Nationality = "FR";
				var container = declaration.CusContainers.AddNew();
				var seal1 = container.AdditionalSeals.AddNew();
				seal1.BK_SealNumber = "44444";
				var seal2 = container.AdditionalSeals.AddNew();
				seal2.BK_SealNumber = "55555";
				var equipment = declaration.Equipments.AddNew();
				equipment.CEQ_IdentificationNumber = "ABC123";
				var equipmentSeal1 = equipment.Seals.AddNew();
				equipmentSeal1.BK_SealNumber = "99999";
				var equipmentSeal2 = equipment.Seals.AddNew();
				equipmentSeal2.BK_SealNumber = "88888";
				var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(null, declaration)));
				var shipment = writer.GetDataObject(declaration);

				AssertEquals("Number of containers", 1, shipment.ContainerCollection.Count);
				var testContainer = shipment.ContainerCollection[0];
				var sealNumbers = new[] { "44444", "55555" };
				AssertContainsExactElementsInExactOrder("Additional Seal Numbers", sealNumbers, testContainer.AdditionalSealNumberCollection.Select(s => s.Number.ToString()));
			}
		}

		public void TestPopulateEquipments()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
				var declaration = Factory.BOFactory.New<JobDeclaration>();
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				declaration.JE_TransportModeInland = "ROA";
				declaration.JE_TransportIDInland = "12345";
				declaration.JE_RN_NKTransportNationalityInland = "IE";
				declaration.JE_Trailer1RegNo = "24680";
				declaration.JE_RN_NKTrailer1Nationality = "GB";
				declaration.JE_Trailer2RegNo = "98765";
				declaration.JE_RN_NKTrailer2Nationality = "FR";
				var container = declaration.CusContainers.AddNew();
				var seal1 = container.AdditionalSeals.AddNew();
				seal1.BK_SealNumber = "44444";
				var seal2 = container.AdditionalSeals.AddNew();
				seal2.BK_SealNumber = "55555";
				var equipment = declaration.Equipments.AddNew();
				equipment.CEQ_IdentificationNumber = "ABC123";
				var equipmentSeal1 = equipment.Seals.AddNew();
				equipmentSeal1.BK_SealNumber = "99999";
				var equipmentSeal2 = equipment.Seals.AddNew();
				equipmentSeal2.BK_SealNumber = "88888";
				var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(null, declaration)));
				var shipment = writer.GetDataObject(declaration);

				AssertEquals("Equipments Required", true, declaration.EquipmentsRequired);
				AssertEquals("Number of equipments", 1, shipment.TransportEquipmentCollection.Count);
				var testEquipment = shipment.TransportEquipmentCollection[0];
				AssertEquals("Equipment Identification Number", "ABC123", testEquipment.IdentificationNumber.ToString());
				var sealNumbers = new[] { "99999", "88888" };
				AssertContainsExactElementsInAnyOrder("Additional Seal Numbers", sealNumbers, testEquipment.SealNumberCollection.Select(s => s.Number.ToString()));
			}
		}

		public void TestPopulateDeclarationTransportMeansCollection_Road()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				declaration.JE_TransportModeInland = Core.Constants.TransportModes.Road;
				declaration.JE_TransportIDInland = "12345";
				declaration.JE_RN_NKTransportNationalityInland = "IE";
				declaration.JE_Trailer1RegNo = "24680";
				declaration.JE_RN_NKTrailer1Nationality = "AU";
				declaration.JE_Trailer2RegNo = "98765";
				declaration.JE_RN_NKTrailer2Nationality = "FR";
				var manager = declaration.GetUniversalDataContextManager() as IShipmentDataContextManager;
				var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
				using (((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator())
				{
					var shipmentData = (Shipment)writer.GetDataObject(declaration);
					CombineAssertions(() =>
					{
						var transportMeansCollection = shipmentData.TransportMeansCollection.Where(x => x.TransportType == TransportTypeCode.Inland).ToArray();
						AssertEquals("3 Transport means elements", 3, transportMeansCollection.Length);
						var transportMeans = transportMeansCollection[0];
						AssertEquals("TransportMeans[0] Order", ZInt.Zero, transportMeans.Order);
						AssertEquals("TransportMeans[0] Identification", "12345", transportMeans.IdentificationNumber);
						AssertEquals("TransportMeans[0] Nationality", "IE", transportMeans.Nationality.Code);
						AssertEquals("TransportMeans[0] Nationality - Description", "Ireland", transportMeans.Nationality.Description);
						AssertEquals("TransportMeans[0] Type of Identification", TransportMeansList.Codes.RegistrationNumberOfTheRoadVehicle, transportMeans.TypeOfIdentification.Code);
						AssertEquals("TransportMeans[0] Type of Identification - Description", TransportMeansList.Descriptions.RegistrationNumberOfTheRoadVehicle, transportMeans.TypeOfIdentification.Description);

						transportMeans = transportMeansCollection[1];
						AssertEquals("TransportMeans[1] Order", 1, transportMeans.Order);
						AssertEquals("TransportMeans[1] Identification", "24680", transportMeans.IdentificationNumber);
						AssertEquals("TransportMeans[1] Nationality", "AU", transportMeans.Nationality.Code);
						AssertEquals("TransportMeans[1] Nationality - Description", "Australia", transportMeans.Nationality.Description);
						AssertEquals("TransportMeans[1] Type of Identification", TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer, transportMeans.TypeOfIdentification.Code);
						AssertEquals("TransportMeans[1] Type of Identification - Description", TransportMeansList.Descriptions.RegistrationNumberOfTheRoadTrailer, transportMeans.TypeOfIdentification.Description);

						transportMeans = transportMeansCollection[2];
						AssertEquals("TransportMeans[2] Order", 2, transportMeans.Order);
						AssertEquals("TransportMeans[2] Identification", "98765", transportMeans.IdentificationNumber);
						AssertEquals("TransportMeans[2] Nationality", "FR", transportMeans.Nationality.Code);
						AssertEquals("TransportMeans[2] Nationality - Description", "France", transportMeans.Nationality.Description);
						AssertEquals("TransportMeans[2] Type of Identification", TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer, transportMeans.TypeOfIdentification.Code);
						AssertEquals("TransportMeans[2] Type of Identification - Description", TransportMeansList.Descriptions.RegistrationNumberOfTheRoadTrailer, transportMeans.TypeOfIdentification.Description);
					});
				}
			}
		}

		public void TestPopulateDeclarationTransportMeansCollection_Air()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				declaration.JE_TransportModeInland = Core.Constants.TransportModes.Air;
				declaration.JE_TransportIDInland = "2468";
				declaration.JE_RN_NKTransportNationalityInland = "AU";
				declaration.JE_Trailer1RegNo = "24680";
				declaration.JE_RN_NKTrailer1Nationality = "GB";
				declaration.JE_Trailer2RegNo = "98765";
				declaration.JE_RN_NKTrailer2Nationality = "FR";
				var manager = declaration.GetUniversalDataContextManager() as IShipmentDataContextManager;
				var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
				using (((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator())
				{
					var shipmentData = (Shipment)writer.GetDataObject(declaration);
					CombineAssertions(() =>
					{
						var transportMeansCollection = shipmentData.TransportMeansCollection.Where(x => x.TransportType == TransportTypeCode.Inland).ToArray();
						AssertEquals("Transport means elements", 1, transportMeansCollection.Length);
						var transport = transportMeansCollection[0];
						AssertEquals("Air : Order", ZInt.Zero, transport.Order);
						AssertEquals("Air : Identification", "2468", transport.IdentificationNumber);
						AssertEquals("Air : Nationality", "AU", transport.Nationality.Code);
						AssertEquals("Air : Nationality - Description", "Australia", transport.Nationality.Description);
						AssertEquals("Air : Type of Identification", TransportMeansList.Codes.RegistrationNumberOfTheAircraft, transport.TypeOfIdentification.Code);
						AssertEquals("Air : Type of Identification - Description", TransportMeansList.Descriptions.RegistrationNumberOfTheAircraft, transport.TypeOfIdentification.Description);
					});
				}
			}
		}

		public void TestPopulateDeclarationTransportMeansCollection_Sea()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				declaration.JE_TransportModeInland = Core.Constants.TransportModes.Sea;
				declaration.JE_TransportMeans = TransportMeansList.Codes.ImoShipIdentificationNumber;
				declaration.JE_TransportIDInland = "55555";
				declaration.JE_RN_NKTransportNationalityInland = "AU";
				declaration.JE_Trailer1RegNo = "24680";
				declaration.JE_RN_NKTrailer1Nationality = "GB";
				declaration.JE_Trailer2RegNo = "98765";
				declaration.JE_RN_NKTrailer2Nationality = "FR";
				var manager = declaration.GetUniversalDataContextManager() as IShipmentDataContextManager;
				var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
				using (((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator())
				{
					var shipmentData = (Shipment)writer.GetDataObject(declaration);
					CombineAssertions(() =>
					{
						var transportMeansCollection = shipmentData.TransportMeansCollection.Where(x => x.TransportType == TransportTypeCode.Inland).ToArray();
						AssertEquals("Transport means elements", 1, transportMeansCollection.Length);
						var transport = transportMeansCollection[0];
						AssertEquals("Sea : Order", ZInt.Zero, transport.Order);
						AssertEquals("Sea : Identification", "55555", transport.IdentificationNumber);
						AssertEquals("Sea : Nationality", "AU", transport.Nationality.Code);
						AssertEquals("Sea : Nationality - Description", "Australia", transport.Nationality.Description);
						AssertEquals("Sea : Type of Identification", TransportMeansList.Codes.ImoShipIdentificationNumber, transport.TypeOfIdentification.Code);
						AssertEquals("Sea : Type of Identification - Description", TransportMeansList.Descriptions.ImoShipIdentificationNumber, transport.TypeOfIdentification.Description);
					});
				}
			}
		}

		public void TestPopulateDeclarationTransportMeansCollection_Other()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				var manager = declaration.GetUniversalDataContextManager() as IShipmentDataContextManager;
				var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
				using (((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator())
				{
					foreach ((string transporMode, string transportMeanCode, string transportMeanDesc) in new[]
					{
						(Core.Constants.TransportModes.FixedTransportInstallations, TransportMeansList.Codes.ImoShipIdentificationNumber, TransportMeansList.Descriptions.ImoShipIdentificationNumber),
						(Core.Constants.TransportModes.InlandWaterwayTransport, TransportMeansList.Codes.ImoShipIdentificationNumber, TransportMeansList.Descriptions.ImoShipIdentificationNumber),
						(Core.Constants.TransportModes.OwnPropulsion, TransportMeansList.Codes.ImoShipIdentificationNumber, TransportMeansList.Descriptions.ImoShipIdentificationNumber),
						(Core.Constants.TransportModes.Mail, TransportMeansList.Codes.ImoShipIdentificationNumber, TransportMeansList.Descriptions.ImoShipIdentificationNumber),
						(Core.Constants.TransportModes.OwnPropulsion, TransportMeansList.Codes.ImoShipIdentificationNumber, TransportMeansList.Descriptions.ImoShipIdentificationNumber),
						(Core.Constants.TransportModes.OwnPropulsion, TransportMeansList.Codes.NameOfTheSeaGoingVessel, TransportMeansList.Descriptions.NameOfTheSeaGoingVessel),
						(Core.Constants.TransportModes.OwnPropulsion, TransportMeansList.Codes.WagonNumber, TransportMeansList.Descriptions.WagonNumber),
						(Core.Constants.TransportModes.OwnPropulsion, TransportMeansList.Codes.TrainNumber, TransportMeansList.Descriptions.TrainNumber),
						(Core.Constants.TransportModes.OwnPropulsion, TransportMeansList.Codes.RegistrationNumberOfTheRoadVehicle, TransportMeansList.Descriptions.RegistrationNumberOfTheRoadVehicle),
						(Core.Constants.TransportModes.OwnPropulsion, TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer, TransportMeansList.Descriptions.RegistrationNumberOfTheRoadTrailer),
						(Core.Constants.TransportModes.OwnPropulsion, TransportMeansList.Codes.IataFlightNumber, TransportMeansList.Descriptions.IataFlightNumber),
						(Core.Constants.TransportModes.OwnPropulsion, TransportMeansList.Codes.RegistrationNumberOfTheAircraft, TransportMeansList.Descriptions.RegistrationNumberOfTheAircraft),
						(Core.Constants.TransportModes.OwnPropulsion, TransportMeansList.Codes.EuropeanVesselIdentificationNumberEniCode, TransportMeansList.Descriptions.EuropeanVesselIdentificationNumberEniCode),
						(Core.Constants.TransportModes.OwnPropulsion, TransportMeansList.Codes.NameOfTheInlandWaterwaysVessel, TransportMeansList.Descriptions.NameOfTheInlandWaterwaysVessel),
					})
					{
						CombineAssertions(transporMode + " - " + transportMeanCode, () =>
						{
							SetUpGenericTransportInland(declaration, transporMode, transportMeanCode);
							var shipmentData = (Shipment)writer.GetDataObject(declaration);
							var transportMeansCollection = shipmentData.TransportMeansCollection.Where(x => x.TransportType == TransportTypeCode.Inland).ToArray();
							AssertEquals("Transport means elements", 1, transportMeansCollection.Length);
							var transport = transportMeansCollection[0];
							AssertEquals("Order", ZInt.Zero, transport.Order);
							AssertEquals("Identification", "55555", transport.IdentificationNumber);
							AssertEquals("Nationality", "AU", transport.Nationality.Code);
							AssertEquals("Nationality - Description", "Australia", transport.Nationality.Description);
							AssertEquals("Type of Identification", transportMeanCode, transport.TypeOfIdentification.Code);
							AssertEquals("Type of Identification - Description", transportMeanDesc, transport.TypeOfIdentification.Description);
						});
					}
					CombineAssertions("Unknown", () =>
					{
						SetUpGenericTransportInland(declaration, "#@", TransportMeansList.Codes.NameOfTheInlandWaterwaysVessel);
						var shipmentData = (Shipment)writer.GetDataObject(declaration);
						var transportMeansCollection = shipmentData.TransportMeansCollection.Where(x => x.TransportType == TransportTypeCode.Inland).ToArray();
						AssertEquals("Transport means elements", 1, transportMeansCollection.Length);
						var transport = transportMeansCollection[0];
						AssertEquals("Order", ZInt.Zero, transport.Order);
						AssertEquals("Identification", "55555", transport.IdentificationNumber);
						AssertEquals("Nationality", "AU", transport.Nationality.Code);
						AssertEquals("Nationality - Description", "Australia", transport.Nationality.Description);
						AssertNull("Type of Identification", transport.TypeOfIdentification);
					});
				}
			}
		}

		public void TestPopulateDeclarationLocationOfGoods()
		{
			var declaration = Factory.New<JobDeclaration>();
			var manager = declaration.GetUniversalDataContextManager() as IShipmentDataContextManager;
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			using (((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator())
			{
				declaration.GoodsLocation.CGL_Qualifier = "Z";
				declaration.GoodsLocation.CGL_Type = "B";
				declaration.GoodsLocation.Address.AuthorisationNumber = "ABC112";
				declaration.GoodsLocation.CGL_AdditionalIdentifier = "IT9843";
				declaration.GoodsLocation.Address.E2_Contact = "Bob";
				declaration.GoodsLocation.Address.E2_Phone = "9213";
				declaration.GoodsLocation.Address.E2_Email = "reach@email.com";

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
				{
					var shipmentData = (Shipment)writer.GetDataObject(declaration);

					CombineAssertions("When Ucc6 true", () =>
					{
						var goodsLocation = shipmentData.LocationOfGoodsCollection[0];
						AssertEquals("Qualifier", "Z", goodsLocation.Qualifier.Code);
						AssertEquals("Qualifier - Description", "Address", goodsLocation.Qualifier.Description);
						AssertEquals("LocationType", "B", goodsLocation.LocationType.Code);
						AssertEquals("LocationType - Description", "Authorized Place", goodsLocation.LocationType.Description);
						AssertEquals("AuthorizationNumber", "ABC112", goodsLocation.AuthorizationNumber);
						AssertEquals("AdditionalIdentifier", "IT9843", goodsLocation.AdditionalIdentifier);
						AssertEquals("Contact Name", "Bob", goodsLocation.Contact.Name);
						AssertEquals("Contact PhoneNumber", "9213", goodsLocation.Contact.PhoneNumber);
						AssertEquals("Contact Email", "reach@email.com", goodsLocation.Contact.Email);
					});
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: false))
				{
					var shipmentData = (Shipment)writer.GetDataObject(declaration);
					AssertNull("When Ucc6 false, LocationOfGoodsCollection", shipmentData.LocationOfGoodsCollection);
				}
			}
		}

		public void TestPopulateGoodsLocationAddress()
		{
			var orgHeader1 = Factory.New<OrgHeader>();
			var orgAddress1 = orgHeader1.MainAddress;
			orgAddress1.OA_Address1 = "Lane1";
			orgAddress1.OA_Address2 = "Cross1";
			orgAddress1.AddressCode = "XYZ1";
			orgAddress1.OA_City = "Milan";
			orgAddress1.CompanyName = "XYZ1 Ltd";
			orgAddress1.OA_RN_NKCountryCode = "IT";

			var orgHeader2 = Factory.New<OrgHeader>();
			var orgAddress2 = orgHeader2.MainAddress;
			orgAddress2.OA_Address1 = "Lane2";
			orgAddress2.OA_Address2 = "Cross2";
			orgAddress2.AddressCode = "XYZ2";
			orgAddress2.OA_City = "Padvoa";
			orgAddress2.CompanyName = "XYZ2 Ltd";
			orgAddress2.OA_RN_NKCountryCode = "IT";

			var declaration = Factory.New<JobDeclaration>();
			var manager = declaration.GetUniversalDataContextManager() as IShipmentDataContextManager;
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			using (((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator())
			{
				var goodsLocation = declaration.GoodsLocation;
				goodsLocation.Address.E2_OA_Address = orgAddress1.PK;
				var shipmentData = (Shipment)writer.GetDataObject(declaration);

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
				{
					var goodsLocationAddressFromShipment = GetGoodsLocationAddressFromShipment(shipmentData);
					AssertNull("When Qualifier empty", goodsLocationAddressFromShipment);

					goodsLocation.CGL_Qualifier = "K";
					shipmentData = (Shipment)writer.GetDataObject(declaration);
					goodsLocationAddressFromShipment = GetGoodsLocationAddressFromShipment(shipmentData);
					AssertNull("When Qualifier = K, goodsLocationAddress", goodsLocationAddressFromShipment);

					goodsLocation.CGL_Qualifier = "Z";
					goodsLocation.Address.E2_OA_Address = orgAddress1.PK;
					shipmentData = (Shipment)writer.GetDataObject(declaration);
					goodsLocationAddressFromShipment = GetGoodsLocationAddressFromShipment(shipmentData);
					AssertNotNull("When Qualifier = Z, goodsLocationAddress", goodsLocationAddressFromShipment);
					AssertAddressSame("When Qualifier = Z", orgAddress1, goodsLocationAddressFromShipment);

					goodsLocation.Address.E2_AddressOverride = true;
					shipmentData = (Shipment)writer.GetDataObject(declaration);
					goodsLocationAddressFromShipment = GetGoodsLocationAddressFromShipment(shipmentData);
					AssertEquals("When Qualifier = Z and override ticked", true, goodsLocationAddressFromShipment.AddressOverride);

					goodsLocation.CGL_Qualifier = "Y";
					goodsLocation.Address.IdentificationHolderPK = orgHeader2.PK;
					shipmentData = (Shipment)writer.GetDataObject(declaration);
					goodsLocationAddressFromShipment = GetGoodsLocationAddressFromShipment(shipmentData);
					AssertNotNull("When Qualifier = Y, goodsLocationAddress", goodsLocationAddressFromShipment);
					AssertAddressSame("When Qualifier = Y", orgAddress2, goodsLocationAddressFromShipment);
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: false))
				{
					shipmentData = (Shipment)writer.GetDataObject(declaration);
					var goodsLocationAddressFromShipment = GetGoodsLocationAddressFromShipment(shipmentData);
					AssertNull("When Ucc6 false, goodsLocationAddress", goodsLocationAddressFromShipment);
				}
			}

			OrganizationAddress GetGoodsLocationAddressFromShipment(Shipment shipment)
			{
				return shipment.OrganizationAddressCollection.FirstOrDefault(x => (x.AddressType ?? ZString.Empty) == "LocationOfGoods");
			}

			void AssertAddressSame(string message, OrgAddress orgAddress, OrganizationAddress goodsLocationAddress)
			{
				CombineAssertions(message, () =>
				{
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

		public void TestAgreedPlaceCode()
		{
			using (((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator())
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.EUD_AgreedPlaceCode = "ZA11";
				Factory.SaveForTesting();
				var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
				var result = writer.GetDataObject(declaration);
				AssertEquals("EUD_AgreedPlaceCode", "ZA11", result.AgreedPlaceCode);
			}
		}

		public void TestEntryHeaderDataObjectWriterType()
		{
			using (((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator())
			{
				var declaration = Factory.BOFactory.New<JobDeclaration>();
				var writer = new DeclarationDataObjectWriterForTest(new DataWritingManager(new ActionInfo(null, declaration)));
				writer.GetDataObject(declaration);
				AssertType<CustomsEntryHeaderDataObjectWriter>("EntryHeader writer type", writer.GetNewCustomsEntryHeaderDataObjectWriterExposed());
			}
		}

		void SetUpGenericTransportInland(JobDeclaration declaration, ZString transportModeInland, ZString transportMeans)
		{
			declaration.JE_TransportModeInland = transportModeInland;
			declaration.JE_TransportMeans = transportMeans;
			declaration.JE_TransportIDInland = "55555";
			declaration.JE_RN_NKTransportNationalityInland = "AU";
			declaration.JE_Trailer1RegNo = "24680";
			declaration.JE_RN_NKTrailer1Nationality = "GB";
			declaration.JE_Trailer2RegNo = "98765";
			declaration.JE_RN_NKTrailer2Nationality = "FR";
		}

		public void TestPopulateDeclarationDutyPayers()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				var declaration = Factory.New<JobDeclaration>();
				((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();

				var orgHeader1 = Factory.New<OrgHeader>();
				orgHeader1.OH_Code = "Org1";
				var orgAddress1 = orgHeader1.MainAddress;
				orgAddress1.OA_Address1 = "Address line 1:";
				orgAddress1.OA_Address2 = "Address line 2:";
				orgAddress1.AddressCode = "Add1";
				orgAddress1.OA_City = "City1";
				orgAddress1.CompanyName = "XYZ1 Ltd";
				orgAddress1.OA_RN_NKCountryCode = "IE";

				declaration.JE_OH_DutyPayer = orgAddress1.OA_OH;
				var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(null, declaration)));
				var shipment = writer.GetDataObject(declaration);
				var dutyPayerAddressCollection = shipment.OrganizationAddressCollection.FirstOrDefault(x => (x.AddressType ?? ZString.Empty) == "DutyPayer");
				CombineAssertions("Write Duty payer value to shipment", () =>
				{
					AssertEquals("Address code", orgAddress1.AddressCode, dutyPayerAddressCollection.AddressShortCode);
					AssertEquals("Address1", orgAddress1.OA_Address1, dutyPayerAddressCollection.Address1);
					AssertEquals("City", orgAddress1.OA_City, dutyPayerAddressCollection.City);
					AssertEquals("Org", orgHeader1.OH_Code, dutyPayerAddressCollection.OrganizationCode);
				});
			}
		}

		public void TestPopulateDeclarationDefermentParty()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				var declaration = Factory.New<JobDeclaration>();
				((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
				var orgHeader1 = Factory.New<OrgHeader>();
				var orgAddress1 = orgHeader1.MainAddress;
				orgAddress1.OA_Address1 = "Address line 1:";
				orgAddress1.OA_Address2 = "Address line 2:";
				orgAddress1.AddressCode = "Add1";
				orgAddress1.OA_City = "City1";
				orgAddress1.CompanyName = "XYZ1 Ltd";
				orgAddress1.OA_RN_NKCountryCode = "IE";
				declaration.DefermentPartyDocAddress.E2_OA_Address = orgAddress1.PK;

				var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(null, declaration)));
				var shipment = writer.GetDataObject(declaration);
				var defermentPartyDocAdd = shipment.OrganizationAddressCollection.FirstOrDefault(x => (x.AddressType ?? ZString.Empty) == "DefermentParty");
				CombineAssertions("Write Deferment Party value to shipment", () =>
				{
					AssertEquals("Address code", orgAddress1.AddressCode, defermentPartyDocAdd.AddressShortCode);
					AssertEquals("Address1", orgAddress1.OA_Address1, defermentPartyDocAdd.Address1);
					AssertEquals("City", orgAddress1.OA_City, defermentPartyDocAdd.City);
					AssertEquals("Org", orgHeader1.OH_Code, defermentPartyDocAdd.OrganizationCode);
				});
			}
		}

		public void TestPopulateDeclarationInlandTransportUCC5()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				var declaration = Factory.New<JobDeclaration>();
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC5Core", true))
				{
					((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
					SetUpGenericTransportInland(declaration, Core.Constants.TransportModes.Air, TransportMeansList.Codes.RegistrationNumberOfTheAircraft);
					var writer = new DeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(null, declaration)));
					var shipment = writer.GetDataObject(declaration);
					var transportMeansFromShipment = shipment.TransportMeansCollection.First();
					var transportModeInland = shipment.AddInfoCollection.First(x => (x.Key ?? ZString.Empty) == "InlandModeOfTransport");
					CombineAssertions("Inland Transport UCC5", () =>
					{
						AssertEquals("Transport Type", "Inland", transportMeansFromShipment.TransportType.ToString());
						AssertEquals("Identification number", "55555", transportMeansFromShipment.IdentificationNumber);
						AssertEquals("Transport Type", TransportMeansList.Codes.RegistrationNumberOfTheAircraft, transportMeansFromShipment.TypeOfIdentification.Code);
						AssertEquals("Transport Means", Core.Constants.TransportModes.Air, transportModeInland.Value);
					});
				}
			}
		}
	}

	class DeclarationDataObjectWriterForTest : DeclarationDataObjectWriter
	{
		public DeclarationDataObjectWriterForTest(IDataWritingManager manager) : base(manager)
		{
		}

		public new Customs.DataTransfer.Universal.UniversalDataObjectWriterHelper CreateNewUniversalDataObjectWriterHelper(BaseJobDeclaration declarationBO)
		{
			return base.CreateNewUniversalDataObjectWriterHelper(declarationBO);
		}

		public Customs.DataTransfer.Universal.CustomsEntryHeaderDataObjectWriter GetNewCustomsEntryHeaderDataObjectWriterExposed() => GetNewCustomsEntryHeaderDataObjectWriter();
	}

	class CusSupportingInfoTypeListProviderForTest : CusSupportingInfoTypeListProvider, IUniversalCustomsDataObjectProvider
	{
		protected override ICodeDescriptionPairList TableSpecificCusSupportingInfoTypeListCore(ZString tableCode, string dataContext)
		{
			var result = base.TableSpecificCusSupportingInfoTypeListCore(tableCode, dataContext);
			if (result == null)
			{
				switch (tableCode)
				{
					case CusEntryInstructionSchema.Constants.Prefix:
						result = GetListForList();
						break;
				}
			}
			return result;
		}

		public IEnumerable<ITopLevelDataObjectReader> GetNewAirManifestDataObjectReaders(Shipment mawbDataObject, Shipment subShipment, IXmlImportLogger logger, UniversalObjectFactory factory, bool singleHAWBCheck) => null;
		public ITopLevelDataObjectWriter GetNewAirManifestDataObjectWriter(IDataWritingManager manager) => null;
		public ITopLevelDataObjectWriter GetNewAirManifestLineDataObjectWriter(IDataWritingManager manager, AirManifestDataObjectWriterHelper helper) => null;
		public IEnumerable<ITopLevelDataObjectReader> GetNewCusSCAOceanBillDataObjectReaders(Shipment dataObject, Shipment subShipment, IXmlImportLogger logger, UniversalObjectFactory factory) => null;
		public ITopLevelDataObjectWriter GetNewCusSCAOceanBillDataObjectWriter(IDataWritingManager manager) => null;
		public ITopLevelDataObjectWriter GetNewDeclarationDataObjectWriter(IDataWritingManager manager) => null;
		public ITopLevelDataObjectReader GetNewJobDeclarationDataObjectReader(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment) => null;
		public ITopLevelDataObjectReader GetNewStandaloneCommercialInvoiceDataObjectReader(Shipment shipmentDataObject, CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, UniversalObjectFactory factory) => null;
		public StandaloneCommercialInvoiceDataObjectWriter GetNewStandaloneCommercialInvoiceDataObjectWriter(IDataWritingManager manager) => null;
		public Customs.DataTransfer.Universal.UniversalDataObjectReaderHelper GetNewUniversalDataObjectReaderHelper(UniversalObjectFactory factory, string sourceCountryCode, string dataProviderForCodeMapping = null) => null;
		public ICodeDescriptionPairList TableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList(ZString tableCode, string dataContext) => null;
		public ICodeDescriptionPairList TableSpecificCusAddInfoTypeList(ZString tableCode, string dataContext) => null;
		public ICodeDescriptionPairList TableSpecificCusCodeDataCodeList(ZString tableCode, string dataContext) => null;
		public ICodeDescriptionPairList TableSpecificCusCodeDataTypeList(ZString tableCode, string dataContext) => null;
		public ICodeDescriptionPairList TableSpecificCusReferenceTypeList(ZString tableCode, string dataContext) => null;
	}
}
