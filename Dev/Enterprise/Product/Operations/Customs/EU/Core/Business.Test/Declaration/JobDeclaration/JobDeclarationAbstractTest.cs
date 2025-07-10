using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.Business.UniversalReferenceConstants;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public abstract class JobDeclarationAbstractTest<TEUJobDeclaration> : BaseJobDeclarationTest<TEUJobDeclaration>
		where TEUJobDeclaration : JobDeclaration
	{
		public void TestTransportMeansDependency()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("TransportMeansDependency should be JE_TransportModeInland for Import.", ExpectedTransportMeansDependencyForImport, declaration.TransportMeansDependency);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("TransportMeansDependency should be None for Export.", ExpectedTransportMeansDependencyForExport, declaration.TransportMeansDependency);

			declaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("TransportMeansDependency should be None for Miscellaneous Customs.", ExpectedTransportMeansDependencyForMiscellaneousCustoms, declaration.TransportMeansDependency);
		}

		protected virtual EUCommonConstants.TransportModeSource ExpectedTransportMeansDependencyForImport => EUCommonConstants.TransportModeSource.None;
		protected virtual EUCommonConstants.TransportModeSource ExpectedTransportMeansDependencyForExport => EUCommonConstants.TransportModeSource.None;
		protected virtual EUCommonConstants.TransportModeSource ExpectedTransportMeansDependencyForMiscellaneousCustoms => EUCommonConstants.TransportModeSource.None;

		[ExpectNoExceptions]
		public virtual void TestAllAddInfoColumnsAreInModelView()
		{
			var jobDeclaration = Factory.New<TEUJobDeclaration>();
			var modelViewSchema = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(jobDeclaration.CountryCode + JobDeclarationSchema.Constants.TableName);
			ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(jobDeclaration, modelViewSchema?.TableName ?? "EUJobDeclaration", (fieldName) => !fieldName.StartsWith(JobEUDeclarationSchema.Constants.Prefix));
		}

		public void TestBothModelViewAndBaseEUAddInfoCanSave()
		{
			var declaration = Factory.NewWithValidTestData<TEUJobDeclaration>();
			if (declaration is IAddInfoSchemaProvider schemaProvider)
			{
				var schema = schemaProvider.AddInfoTableSchema;
				var countryAddInfo = schema.All.First(c => !Schema.IsSystemColumn(c.ObjectName));
				var countryAddInfoValue = BusinessObjectHelper.GetNonDefaultValueForZType(countryAddInfo.GetEquivalentZType());
				if (countryAddInfoValue is ZString zString)
				{
					countryAddInfoValue = zString.Left(countryAddInfo.MaxLength);
				}

				AssertBothModelViewAndBaseEUAddInfoCanSave(declaration,
								nameof(JobDeclaration.ZG_VATDeferType), (ZString)DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14, (ZString)DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority,
								countryAddInfo.Name, countryAddInfoValue);
			}
			else
			{
				Assert("This test is only valid for declarations that implement IAddInfoSchemaProvider", true);
			}
		}

		void AssertBothModelViewAndBaseEUAddInfoCanSave(TEUJobDeclaration declaration, string euAddInfoName, IZType euAddInfoValue, IZType euAddInfoAltValue, string countryAddInfoName, IZType countryAddInfoValue)
		{
			var euAddInfoNameString = euAddInfoName.Substring(euAddInfoName.IndexOf('_') + 1);
			var countryAddInfoNameString = countryAddInfoName.Substring(countryAddInfoName.IndexOf('_') + 1);
			var euAddInfoValueString = euAddInfoValue.GetStringRepresentation();
			var euAddInfoAltValueString = euAddInfoAltValue.GetStringRepresentation();
			var countryAddInfoValueString = countryAddInfoValue.GetStringRepresentation();

			declaration[euAddInfoName] = euAddInfoValue;
			declaration[countryAddInfoName] = countryAddInfoValue;
			Factory.Save();

			AssertContains($"{euAddInfoNameString}={euAddInfoValueString}", declaration.JE_AddInfo);
			AssertContains($"{countryAddInfoNameString}={countryAddInfoValueString}", declaration.JE_AddInfo);

			var newFactory = new BusinessObjectFactory();
			var declarationInNewFactory = newFactory.Load<JobDeclaration>(declaration.PK);
			AssertContains($"{euAddInfoNameString}={euAddInfoValueString}", declarationInNewFactory.JE_AddInfo);
			AssertContains($"{countryAddInfoNameString}={countryAddInfoValueString}", declarationInNewFactory.JE_AddInfo);
			AssertEquals(euAddInfoValue, declarationInNewFactory[euAddInfoName]);
			AssertEquals(countryAddInfoValue, declarationInNewFactory[countryAddInfoName]);

			declarationInNewFactory[euAddInfoName] = euAddInfoAltValue;
			newFactory.Save();

			AssertContains($"{euAddInfoNameString}={euAddInfoAltValueString}", declarationInNewFactory.JE_AddInfo);

			declaration.Reload();
			AssertContains($"{euAddInfoNameString}={euAddInfoAltValueString}", declaration.JE_AddInfo);
			AssertEquals(euAddInfoAltValue, declarationInNewFactory[euAddInfoName]);
		}

		public void TestResetImpQuantitiesOnDeclarationTypeChange()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CustomsFourthQuantity = -10;
			invoiceLine.JI_CustomsFifthQuantity = -10;

			AssertHasErrorContaining("Import, Negative Fourth Quantity", invoiceLine.JI_CustomsFourthQuantityInfo, MandatoryValidation.ValueCannotBeNegative);
			AssertHasErrorContaining("Import, Negative Fifth Quantity", invoiceLine.JI_CustomsFifthQuantityInfo, MandatoryValidation.ValueCannotBeNegative);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertNoErrorContaining("Export, Negative Fourth Quantity", invoiceLine.JI_CustomsFourthQuantityInfo, MandatoryValidation.ValueCannotBeNegative);
			AssertNoErrorContaining("Export, Negative Fifth Quantity", invoiceLine.JI_CustomsFifthQuantityInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestExitPresentationStatus()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ClusterKey = 555;
			AssertEquals("No exit report", ZString.Empty, declaration.ExitPresentationStatus);

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			Factory.Save();
			ExitControlTestHelper.CreateCusExitReportWithStatus(declaration, new ZString[] { "EXR" });
			AssertEquals("One exit report", "EXR", declaration.ExitPresentationStatus);

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			Factory.Save();
			ExitControlTestHelper.CreateCusExitReportWithStatus(declaration, new ZString[] { "COX", "COX" });
			AssertEquals("Multiple exit reports with same status", "COX", declaration.ExitPresentationStatus);

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			Factory.Save();
			ExitControlTestHelper.CreateCusExitReportWithStatus(declaration, new ZString[] { "COX", "REQ" });
			AssertEquals("Multiple exit reports with different statuses", "MLT", declaration.ExitPresentationStatus);
		}

		public override void TestGetNewCusEquipmentCollection()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertType<CusEquipmentCollection<CusEquipment>>(dec.Equipments);
		}

		public override void TestIsDeclarationWithEntryInstruction()
		{
			Assert(true);  // Duly overridden, m'lud.
		}

		public override void TestContainersRequiredOnNonTransportDeclarationType()
		{
			Assert("TestContainersRequired() handles this", true);
		}

		public override void TestContainersRequiredOnSea()
		{
			Assert("TestContainersRequired() handles this", true);
		}

		public void TestCustomsDocStatus()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();

			AssertEquals("Should be empty with no RequestedDocuments", ZString.Empty, declaration.CustomsDocStatus);

			var requestedDocument1 = instruction1.RequestedDocuments.AddNew();
			var requestedDocument2 = instruction1.RequestedDocuments.AddNew();
			var requestedDocument3 = instruction2.RequestedDocuments.AddNew();

			requestedDocument1.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
			requestedDocument2.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
			requestedDocument3.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
			AssertEquals(RequestedDocumentStatusList.Codes.RequestOpened, declaration.CustomsDocStatus);
			AssertEquals(RequestedDocumentStatusList.Descriptions.RequestOpened, declaration.CustomsDocStatusDesc);

			requestedDocument1.CSI_Status = RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;
			requestedDocument2.CSI_Status = RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;
			requestedDocument3.CSI_Status = RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;
			AssertEquals(RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived, declaration.CustomsDocStatus);
			AssertEquals(RequestedDocumentStatusList.Descriptions.DocumentsConfirmedReceived, declaration.CustomsDocStatusDesc);

			requestedDocument1.CSI_Status = RequestedDocumentStatusList.Codes.PhysicallyPresentDocument;
			requestedDocument2.CSI_Status = RequestedDocumentStatusList.Codes.PhysicallyPresentDocument;
			requestedDocument3.CSI_Status = RequestedDocumentStatusList.Codes.PhysicallyPresentDocument;
			AssertEquals(RequestedDocumentStatusList.Codes.PhysicallyPresentDocument, declaration.CustomsDocStatus);
			AssertEquals(RequestedDocumentStatusList.Descriptions.PhysicallyPresentDocument, declaration.CustomsDocStatusDesc);

			requestedDocument1.CSI_Status = RequestedDocumentStatusList.Codes.RequestCancelled;
			requestedDocument2.CSI_Status = RequestedDocumentStatusList.Codes.RequestCancelled;
			requestedDocument3.CSI_Status = RequestedDocumentStatusList.Codes.RequestCancelled;
			AssertEquals(RequestedDocumentStatusList.Codes.RequestCancelled, declaration.CustomsDocStatus);
			AssertEquals(RequestedDocumentStatusList.Descriptions.RequestCancelled, declaration.CustomsDocStatusDesc);

			requestedDocument1.CSI_Status = RequestedDocumentStatusList.Codes.RequestCancelled;
			requestedDocument2.CSI_Status = RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;
			requestedDocument3.CSI_Status = RequestedDocumentStatusList.Codes.RequestCancelled;
			requestedDocument3.CSI_Status = RequestedDocumentStatusList.Codes.RequestCancelled;
			AssertEquals(RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived, declaration.CustomsDocStatus);
			AssertEquals(RequestedDocumentStatusList.Descriptions.DocumentsConfirmedReceived, declaration.CustomsDocStatusDesc);

			requestedDocument1.CSI_Status = RequestedDocumentStatusList.Codes.RequestCancelled;
			requestedDocument2.CSI_Status = RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;
			requestedDocument3.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
			requestedDocument3.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
			AssertEquals(RequestedDocumentStatusList.Codes.RequestOpened, declaration.CustomsDocStatus);
			AssertEquals(RequestedDocumentStatusList.Descriptions.RequestOpened, declaration.CustomsDocStatusDesc);
		}

		public override void TestBondedWarehouseEditable()
		{
			var declarationMock = Factory.NewMoq<JobDeclaration>();
			declarationMock.Setup(m => m.AreMultipleEntryInstructionsAllowed).Returns(false);
			var declaration = declarationMock.Object;
			CombineAssertions("!AreMultipleEntryInstructionsAllowed", () =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("Import", true, declaration.BondedWarehouseEditable);

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("Export", true, declaration.BondedWarehouseEditable);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
				AssertEquals("Other", false, declaration.BondedWarehouseEditable);
			});

			declarationMock.Setup(m => m.AreMultipleEntryInstructionsAllowed).Returns(true);
			CombineAssertions("AreMultipleEntryInstructionsAllowed", () =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("Import", false, declaration.BondedWarehouseEditable);

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("Export", false, declaration.BondedWarehouseEditable);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
				AssertEquals("Other", false, declaration.BondedWarehouseEditable);
			});
		}

		public virtual void TestJE_TransportMode_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("[25] Transport", DataBoundResourceStrings.GetDataForProperty(declaration.JE_TransportModeInfo).Caption);
		}

		public virtual void TestZG_RegionOfDestination_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("Region of Destination", DataBoundResourceStrings.GetDataForProperty(declaration.ZG_RegionOfDestinationInfo).Caption);
			AssertEquals("Dest. Region", DataBoundResourceStrings.GetDataForProperty(declaration.ZG_RegionOfDestinationInfo).ShortCaption);
		}

		public override void TestGetContainerModeForDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals(Core.Constants.ContainerModes.FCL, declaration.GetContainerModeForDeclaration(ZString.Empty, Core.Constants.ContainerModes.FCL));
			AssertEquals(Core.Constants.ContainerModes.LCL, declaration.GetContainerModeForDeclaration(ZString.Empty, Core.Constants.ContainerModes.LCL));
			AssertEquals(Core.Constants.ContainerModes.Containerised, declaration.GetContainerModeForDeclaration(ZString.Empty, Core.Constants.ContainerModes.Containerised));
			AssertEquals(Core.Constants.ContainerModes.Containerised, declaration.GetContainerModeForDeclaration(ZString.Empty, Core.Constants.ContainerModes.BuyersConsol));
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals(Core.Constants.ContainerModes.NonContainerised, declaration.GetContainerModeForDeclaration(ZString.Empty, Core.Constants.ContainerModes.BuyersConsol));
		}

		public virtual void TestMaxSupportingDocuments()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertEquals("MaxSupportingDocuments default value", -1, dec.MaxSupportingDocuments);
		}

		public virtual void TestDefermentPartyDocAddressRequirement_ValidateOrganisationPK()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();

			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			if (countryCode == Core.Constants.CountryCodes.Germany)
			{
				var cusAccountCollection = new OrgCusAccountCollection(orgHeader2, countryCode);
				cusAccountCollection.AddNew();
			}
			else
			{
				orgHeader2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "2345", countryCode);
			}
			orgHeader2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "1234", Core.Constants.CountryCodes.Greece);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.DefermentPartyDocAddress.OrganisationPK = orgHeader1.PK;
			AssertHasMessageError(dec.DefermentPartyDocAddress.OrganisationPKInfo, "The Deferment Party must have a Deferment Account Number");
			AssertHasMessageError(dec.DefermentPartyDocAddress.OrganisationPKInfo, "The Deferment Party must have a Registration Number / Code of Type 'EOR'");

			dec.DefermentPartyDocAddress.OrganisationPK = orgHeader2.PK;
			AssertNoNotifications(dec.DefermentPartyDocAddress.OrganisationPKInfo);
		}

		public virtual void TestZG_GatewayVisible()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertEquals(false, dec.ZG_GatewayVisible);
		}

		public virtual void TestZG_SpecificCircumstanceIndicator()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertEquals(3, dec.ZG_SpecificCircumstanceIndicatorInfo.MaxLength);
		}

		public virtual void TestGetCusCodeDataType()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertEquals(typeof(EuOfficeCode), ((ICusCodeDataTypeSupporter)dec).GetCusCodeDataTypes()[CusCodeDataTypeList.Codes.OfficeCode]);
		}

		public virtual void TestGetCustomsEntryInstructionProviderCore()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertType(typeof(EntryInstructionProvider), dec.CustomsEntryInstructionProvider);
		}

		public virtual void TestAutoRating()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountry = Env.CurrentCompany.Country.Code;
			var eunId = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(currentCountry, parent: eunId);
			Factory.Save();

			var dut = helper.CreateNewOrGetExistingRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, RefCusRateTypes.Duty, "Duty");
			var add = helper.CreateNewOrGetExistingRateType(currentCountry, RefCusRateTypes.AntiDumpingDuty, "Anti-dumping Duty");
			var interest = helper.CreateNewOrGetExistingRateType(currentCountry, RefCusRateTypes.Interest, "Interest");
			dut.ZZR_IsPayable = true;
			add.ZZR_IsPayable = true;
			interest.ZZR_IsPayable = true;
			helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, dut.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty, add.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.CompensatoryInterestVat, interest.PK);
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = dec.ActiveEntryHeaders.AddNew();

			var line = entry.MergedLines.AddNew();
			line.CL_CustomsPostedStatus = "ACT";
			line.Fees.AddOrUpdate(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, 57.25);
			line.Fees.AddOrUpdate(UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty, 22.44);
			line.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 79.69);
			line.Fees.AddOrUpdate(UniversalReferenceConstants.RefCusRateCodes.CompensatoryInterestVat, 5.86);
			line.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 16.13);

			entry.Charges.AddNew(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, 57.25);

			Factory.Save();
			AssertAutoRateResult(dec, entry, TestAutoRatingExpectedAmount, 1);
		}

		public void TestSetDefaultBorderTransportToIDForTransportMode()
		{
			var dec = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(dec, true))
			{
				CombineAssertions(() =>
				{
					AssertEquals("", dec.ZG_BorderTransportMeans);

					dec.JE_TransportMode = Core.Constants.TransportModes.Air;
					AssertEquals(DefaultBorderTransportModeForAir, dec.ZG_BorderTransportMeans);

					dec.JE_TransportMode = Core.Constants.TransportModes.Sea;
					AssertEquals(DefaultBorderTransportModeForSea, dec.ZG_BorderTransportMeans);

					dec.JE_TransportMode = ZString.Empty;
					AssertEquals("", dec.ZG_BorderTransportMeans);

					dec.JE_TransportMode = Core.Constants.TransportModes.Mail;
					AssertEquals("", dec.ZG_BorderTransportMeans);
				});
			}
		}

		[TestDate(2010, 08, 09)]
		public virtual void TestDateOfValuation()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "EXP";
			AssertEquals("Without ATD and ATA, ValuationDate is today", new ZDate(2010, 08, 09), dec.DateOfValuation);
			dec.JE_MessageType = "IMP";
			AssertEquals("Without ATD and ATA, ValuationDate is today", new ZDate(2010, 08, 09), dec.DateOfValuation);

			dec.JE_ExportDate = new ZDate(2010, 06, 06);
			dec.JE_DateOfArrival = new ZDate(2010, 07, 07);
			dec.JE_MessageType = "EXP";
			AssertEquals("With ATD and ATA, ValuationDate is still today (EXP)", new ZDate(2010, 08, 09), dec.DateOfValuation);
			dec.JE_MessageType = "IMP";
			AssertEquals("With ATD and ATA, ValuationDate is still today (IMP)", new ZDate(2010, 08, 09), dec.DateOfValuation);

			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "120-123456A";
			var entryNumber = entryHeader.CusEntryNumber;
			entryNumber.CE_IssueDate = ZDateTime.BrettsBirthday;
			AssertEquals("When entry date is known, always use that", ZDateTime.BrettsBirthday, dec.DateOfValuation);
		}

		public virtual void TestMergeManagerType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				var jobDec = Factory.New<JobDeclaration>();
				AssertEquals("Should be a Customs.EU.Business.MergeManager", ExpectedMergeManagerType, jobDec.MergeManager.GetType());
			}
		}

		public void TestTypeDecider()
		{
			Assert("Update BaseJobDeclaration to include a decider for this class", Factory.New(typeof(BaseJobDeclaration)).GetType() == GetExpectedBusinessObjectType());
		}

		public virtual void TestDefaultValuesForExport()
		{
			GlbDepartment.CurrentDepartment.GE_Import = false;
			GlbDepartment.CurrentDepartment.GE_Export = true;

			var declaration = Factory.New<JobDeclaration>();

			AssertEquals(ExpectedDefaultDeclarantTypeForExport, declaration.JE_DeclarantType);
			AssertEquals(ZString.Empty, declaration.ZG_CTStatusID);
		}

		protected virtual ZString ExpectedDefaultDeclarantTypeForExport => RepresentationTypeList.Codes._2Direct;

		public virtual void TestDefaultCTStatusIDSwitchingBetweenExportAndImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals(ZString.Empty, declaration.ZG_CTStatusID);
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals(ZString.Empty, declaration.ZG_CTStatusID);
		}

		public virtual void TestCustomsOfficeOfExit()
		{
			var decEcs = Factory.New<JobDeclaration>();
			decEcs.JE_MessageType = "EXP";
			AssertEquals("", decEcs.OfficeOfExit);
			decEcs.CustomsOffices.RemoveAndDeleteAll();
			Factory.Save();
			var cusOffice = decEcs.CustomsOffices.AddNew();
			cusOffice.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfExit;
			cusOffice.CY_Data = "GB000001";
			AssertEquals("GB000001", decEcs.OfficeOfExit);

			decEcs.JE_MessageType = "1";
			decEcs.JE_ApplicationCode = "EMC";
			Assert(decEcs.IsEMCS);

			AssertExceptionThrown<NotImplementedException>("Setting OfficeOfExit should cause NotImplementedException thrown. ", () => decEcs.OfficeOfEntry = "XXX");
		}

		public virtual void TestCustomsOfficeOfEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.CustomsOffices.RemoveAndDeleteAll();
			declaration.JE_CustomsOffice = "LV001000";
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, "LV002000");
			AssertEquals("", declaration.OfficeOfEntry);

			AssertExceptionThrown<NotImplementedException>("Setting OfficeOfEntry should cause NotImplementedException thrown. ", () => declaration.OfficeOfEntry = "XXX");
		}

		public void TestPackTypes()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("If there is no package on the declaration, PackTypes is empty.", ZString.Empty, declaration.PackTypes);
			var package1 = declaration.Packages.AddNew();
			var package2 = declaration.Packages.AddNew();
			package1.CW_PackType = "A";
			package2.CW_PackType = "A";
			AssertEquals("If all Packages.CW_PackType are the same, PackTypes is the same as the CW_PackType of the first package.", "A", declaration.PackTypes);
			var package3 = declaration.Packages.AddNew();
			package3.CW_PackType = "B";
			AssertEquals("If all packages don't have exactly the same CW_PackType, PackTypes is MLT.", MessageStatusList.Codes.MultipleStatus, declaration.PackTypes);
		}

		public void TestDeclarationPropertiesAfterAssigningImporter()
		{
			var localPortCode = GetLocalPortCode();
			var countryCode = localPortCode.Substring(0, 2);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var dec = Factory.New<JobDeclaration>();
				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				dec.ClientReferenceForDucr = null;
				dec.JE_OwnerRef = null;

				var importer = Factory.NewWithValidTestData<OrgHeader>();
				var addr = importer.Addresses.AddNewMainAddress();

				importer.OH_RL_NKClosestPort = localPortCode;
				addr.OA_RL_NKRelatedPortCode = localPortCode;

				AssertEquals("Pre-Req: Importer country is same as declaration country.", dec.CountryCode, importer.CountryCode);
				AssertEquals("Pre-Req: Importer address country is same as declaration country.", dec.CountryCode, addr.Country.RN_Code);

				var addInfo = EUOrgImpAddInfo.Get(importer, dec.CountryCode);
				addInfo.Deserialise();

				addInfo.ZO_OtherDeferType = "B";
				addInfo.ZO_Box14UseIndirectRepresentation = true;

				dec.DocsAndCartage.JP_CustomAttrib1 = "A1";
				dec.DocsAndCartage.JP_CustomAttrib2 = "A2";

				Factory.Save();

				dec.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;
				var expectedDeclarantType = IsDeclarantTypeExpectedToChangeWhenAssigningImporter ? (ZString)RepresentationTypeList.Codes._3Indirect : ZString.Empty;
				AssertEquals("B", dec.JE_PaymentMethod);
				AssertEquals(expectedDeclarantType, dec.JE_DeclarantType);

				AssertCountrySpecificBehaviourAfterAssigningImporter(addInfo, dec);

				dec.ImporterDocumentaryAddress.E2_OA_Address = new ZGuid();
				dec.JE_OH_Importer = new ZGuid();

				addInfo.ZO_OtherDeferType = "C";
				addInfo.ZO_Box14UseIndirectRepresentation = false;

				dec.DocsAndCartage.JP_CustomAttrib1 = "T1";
				dec.DocsAndCartage.JP_CustomAttrib2 = "T2";

				dec.ClientReferenceForDucr = null;
				dec.JE_OwnerRef = null;
				dec.JE_DeclarantType = "X";

				Factory.Save();

				dec.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;

				AssertEquals("C", dec.JE_PaymentMethod);
				expectedDeclarantType = IsDeclarantTypeExpectedToChangeWhenAssigningImporter ? RepresentationTypeList.Codes._2Direct : "X";
				AssertEquals(expectedDeclarantType, dec.JE_DeclarantType);

				AssertCountrySpecificBehaviourAfterAssigningImporter(addInfo, dec);
			}
		}

		public void TestBox30LocationOfGoodsForDocumentsAndMessaging()
		{
			var localPortCode = GetLocalPortCode();
			var countryCode = localPortCode.Substring(0, 2);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_LocationOfGoods = "";
				declaration.SubLocation = "";
				AssertEquals("", declaration.Box30LocationOfGoodsForDocumentsAndMessaging);

				declaration.JE_LocationOfGoods = "LHR";
				declaration.SubLocation = "";
				AssertEquals(GetExpectedBox30LocationOfGoods(countryCode), declaration.Box30LocationOfGoodsForDocumentsAndMessaging);

				declaration.JE_LocationOfGoods = "LHR";
				declaration.SubLocation = "BAC";
				AssertEquals(GetExpectedBox30LocationOfGoodsWithSubLocation(countryCode), declaration.Box30LocationOfGoodsForDocumentsAndMessaging);
			}
		}

		public void TestBox18IdentityOfTransportAtDepartureForDocumentsAndMessaging()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";

			Assert("Should be empty when Import", declaration.Box18IdentityOfTransportAtDepartureForDocumentsAndMessaging.IsEmpty);

			declaration.JE_MessageType = "EXP";
			declaration.ZG_Box18TransportID = "123";
			AssertEquals("Box18IdentityOfTransportAtDepartureForDocumentsAndMessaging", "123", declaration.Box18IdentityOfTransportAtDepartureForDocumentsAndMessaging);
		}

		public virtual void TestCustomsOfficeRequirementHelper()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<JobDeclarationCustomsOfficeRequirementHelper>(declaration.CustomsOfficeRequirementHelper);
		}

		public virtual void TestGetJobComInvoiceLineCalculatorType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var invoiceLineCalculator = declaration.GetJobComInvoiceLineCalculator(invoiceLine);
			AssertNotNull(invoiceLineCalculator);
			AssertType<JobComInvoiceLineValueCalculator>(invoiceLineCalculator);
		}

		public virtual void TestMergeOperation()
		{
			var countryCode = GetCountryCode_ToTestMergeOperation();
			PrepareUniversalData_ToTestMergeOperation(countryCode);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				CreateBadges_ToTestMergeOperation();
				var mergeScenarios = PrepareMergeScenarios_ToTestMergeOperation(countryCode);
				var shutterUpperer = new SendsMessagesToCustomsShutterUpperer(false);

				foreach (var scenario in mergeScenarios)
				{
					var mergeResult = scenario.JobDeclaration.DoMerge(shutterUpperer);
					Assert(scenario.AssertMessage + ": Merge Result", mergeResult);

					Produce_EDI_Message_ToTestMergeOperation(scenario);
					CheckEntries_ToTestMergeOperation(scenario);
				}
			}
		}

		public virtual void TestClassTypesBeingUsed()
		{
			TestClassTypesBeingUsed_Core("");
		}

		public override void TestICusEntryNumFilterProviderImplementation()
		{
			// EU implementation of ValidCusEntryNumFilter is different from implementation in shared.
			// CusEntryNumber cannot be attached to EU declaration directly.

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			var cusEntryNumFilterProvider = (ICusEntryNumFilterProvider)declaration;
			var entryNumbers = new CusEntryNumCollection(Factory);

			entryNumbers.Load(cusEntryNumFilterProvider.ValidCusEntryNumFilter);
			AssertEquals("pre-condition", 0, entryNumbers.Count);

			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.EntryNumber = "12121212";
			entryNumbers.Load(cusEntryNumFilterProvider.ValidCusEntryNumFilter);
			AssertArrayEqualsByElements(new string[]
			{
				"12121212"
			}, entryNumbers.Cast<CusEntryNumber>().OrderBy(e => e.CE_EntryNum).Select(e => (string)e.CE_EntryNum).ToArray());

			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.EntryNumber = "21212121";
			entryNumbers.Load(cusEntryNumFilterProvider.ValidCusEntryNumFilter);
			AssertArrayEqualsByElements(new string[]
			{
				"12121212",
				"21212121"
			}, entryNumbers.Cast<CusEntryNumber>().OrderBy(e => e.CE_EntryNum).Select(e => (string)e.CE_EntryNum).ToArray());

			// test with cancelled entry
			entryHeader1.CH_EntryStatus = EntryStatusList.Codes.Cancelled;
			entryNumbers.Load(cusEntryNumFilterProvider.ValidCusEntryNumFilter);
			AssertArrayEqualsByElements(new string[]
			{
				"21212121"
			}, entryNumbers.Cast<CusEntryNumber>().OrderBy(e => e.CE_EntryNum).Select(e => (string)e.CE_EntryNum).ToArray());

			var otherDeclaration = Factory.New<BaseJobDeclaration>();
			otherDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			var otherEntryHeader = otherDeclaration.CustomsEntryHeaders.AddNew();
			otherEntryHeader.EntryNumber = "66666667";
			entryNumbers.Load(((ICusEntryNumFilterProvider)otherDeclaration).ValidCusEntryNumFilter);
			AssertArrayEqualsByElements(new string[]
			{
				"66666667"
			}, entryNumbers.Cast<CusEntryNumber>().OrderBy(e => e.CE_EntryNum).Select(e => (string)e.CE_EntryNum).ToArray());

			var lrn = CusEntryNumber.New(otherEntryHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, otherDeclaration.CountryCode);
			lrn.CE_EntryNum = "LRN";
			entryNumbers.Load(((ICusEntryNumFilterProvider)otherDeclaration).ValidCusEntryNumFilter);
			AssertArrayEqualsByElements(new string[]
			{
				"66666667"
			}, entryNumbers.Cast<CusEntryNumber>().OrderBy(e => e.CE_EntryNum).Select(e => (string)e.CE_EntryNum).ToArray());

			var declaration2 = Factory.New<JobDeclaration>();

			var entryHeader3 = declaration2.CustomsEntryHeaders.AddNew();
			var mrn = CusEntryNumber.New(entryHeader3, CusEntryNumberTypes.Standard.MovementReferenceNumber, declaration2.CountryCode);
			mrn.CE_EntryNum = "MRN";

			var filteredCusEntryNums = declaration2.ShouldMrnBeHiddenFromShipmentEntryNumbersListViaGetValidCusEntryNumFilter ? Array.Empty<string>() : new string[] { "MRN" };

			entryNumbers.Load(((ICusEntryNumFilterProvider)declaration2).ValidCusEntryNumFilter);
			AssertArrayEqualsByElements(filteredCusEntryNums, entryNumbers.Cast<CusEntryNumber>().OrderBy(e => e.CE_EntryNum).Select(e => (string)e.CE_EntryNum).ToArray());
		}

		public void TestPackages()
		{
			var dec = GetJobDeclaration();
			AssertEquals(ExpectedDeclarationLevelPackageCollectionType, dec.Packages.GetType());
		}

		public void TestAdditionalSealsRequired()
		{
			var exportDeclaration = Factory.New<JobDeclaration>();
			exportDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;

			var importDeclaration = Factory.New<JobDeclaration>();
			importDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;

			var miscellaneousDeclaration = Factory.New<JobDeclaration>();
			miscellaneousDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;

			var emptyTypeDeclaration = Factory.New<JobDeclaration>();
			emptyTypeDeclaration.JE_MessageType = ZString.Empty;

			CombineAssertions(() =>
			{
				AssertEquals("Export declatration: AdditionalSealsRequired.", true, exportDeclaration.AdditionalSealsRequired);
				AssertEquals("Import declaration: AdditionalSealsRequired.", false, importDeclaration.AdditionalSealsRequired);
				AssertEquals("MiscellaneousCustoms declaration: AdditionalSealsRequired.", false, miscellaneousDeclaration.AdditionalSealsRequired);
				AssertEquals("Declaration without type: AdditionalSealsRequired.", false, miscellaneousDeclaration.AdditionalSealsRequired);
			});
		}

		public override void TestShouldCopyProcedureFromPreviousInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (CustomsDataRegistry.Instance.CopyPreviousLineCustomsProcedureCode.SetTemporaryValue(Guid.Empty, declaration.Branch.EntityPK.ToGuid(), Guid.Empty, true))
			{
				AssertEquals("ShouldCopyProcedureFromPreviousInvoiceLine should be true if registry is true", true, declaration.ShouldCopyProcedureFromPreviousInvoiceLine);
			}
			var declaration2 = Factory.New<JobDeclaration>();
			using (CustomsDataRegistry.Instance.CopyPreviousLineCustomsProcedureCode.SetTemporaryValue(Guid.Empty, declaration2.Branch.EntityPK.ToGuid(), Guid.Empty, false))
			{
				AssertEquals("ShouldCopyProcedureFromPreviousInvoiceLine should be false if registry is false", false, declaration2.ShouldCopyProcedureFromPreviousInvoiceLine);
			}
		}

		public void TestAllowUCC6PropertiesWithUCC5()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				AssertEquals("Allow UCC6 properties when UCC6 is set", true, declaration.AllowUCC6PropertiesWithUCC5);
			}
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC5Core", true))
			{
				AssertEquals("Allow UCC5 properties when UCC6 is set", false, declaration.AllowUCC6PropertiesWithUCC5);
			}
		}

		public void TestUseDutyPayerAndDefermentPartyInUXML()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("Allow use of Duty Payer and Deferment Party fields in UXML export", false, declaration.UseDutyPayerAndDefermentPartyInUXML);
		}

		public void TestAllowGoodsLocationFromImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			AssertEquals("Allow use of Goods Location in UXML export when declaration type is import", false, declaration.AllowGoodsLocationFromImport);
		}

		protected override ZString GetForeignCountryCodeForTestDefaultPorts() => Core.Constants.CountryCodes.Singapore;

		protected override void CreatePartAndClassification(string partNum, string tariff, OrgHeader importer)
		{
			var part = Factory.New<Customs.Business.OrgSupplierPart>();
			part.OP_PartNum = partNum;
			part.RelatedOrganisations.AddOwner(importer);

			var classification = Factory.New<BaseCusClassification>();
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.EXP;
			classification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			classification.CC_TariffNum = tariff;

			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_ChildType = ClassificationType.Both;
			pivot.CI_CC = classification.PK;
			pivot.CI_OP = part.PK;
		}

		protected override void PrepareCharge(JobComInvCharge charge)
		{
			base.PrepareCharge(charge);
			charge.J7_DistributeBy = Common.ChargeDistributeByList.Codes.Value;
		}

		protected virtual Type AddInfoChildType => typeof(JobEUDeclaration);

		protected virtual ZDecimal TestAutoRatingExpectedAmount => 85.55M;

		protected void AssertAutoRateResult(JobDeclaration declaration, Customs.Business.CusEntryHeader entryHeader, ZDecimal expectedAmount, ZInt expectedCount)
		{
			var customsChargesManager = new CustomsChargesManager(declaration);
			customsChargesManager.PopulateChargeCodesForEmptyCustomsCharges(((IAccInvoiceDataProvider)entryHeader).CustomsCharges);

			var autoRateInfoCollection = customsChargesManager.RateCustomsCharges(((IAccInvoiceDataProvider)entryHeader).CustomsCharges);
			AssertEquals(expectedCount, autoRateInfoCollection.Count);
			AssertEquals(expectedAmount, autoRateInfoCollection[0].Amount);
		}

		protected virtual ZString DefaultBorderTransportModeForAir => "40";

		protected virtual ZString DefaultBorderTransportModeForSea => "10";

		protected virtual string GetLocalPortCode() => "LVLPX";

		protected virtual bool IsDeclarantTypeExpectedToChangeWhenAssigningImporter => true;

		protected virtual void AssertCountrySpecificBehaviourAfterAssigningImporter(EUOrgImpAddInfo addInfo, JobDeclaration dec)
		{
		}

		protected virtual string GetExpectedBox30LocationOfGoods(string countryCode) => countryCode + "LHRLHR";

		protected virtual string GetExpectedBox30LocationOfGoodsWithSubLocation(string countryCode) => countryCode + "LHRLHRBAC";

		protected virtual string GetCountryCode_ToTestMergeOperation() => Core.Constants.CountryCodes.Latvia;

		protected virtual void CreateBadges_ToTestMergeOperation()
		{
		}

		protected virtual void PrepareUniversalData_ToTestMergeOperation(string countryCode)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(countryCode, "To test Merge operation");

			const string ADDIN = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation;
			const string IMPORT = "IMPORT";
			const string EXPORT = "EXPORT";
			const string HEADER = "HEADER";
			const string ITEM = "ITEM";

			helper.CreateNewOrGetExistingCusCodeType(ADDIN, "AdditionalInformation", countryCode);

			CreateAdditionalInfo(helper, countryCode, "IMH01", IMPORT, HEADER);
			CreateAdditionalInfo(helper, countryCode, "IMH02", IMPORT, HEADER);
			CreateAdditionalInfo(helper, countryCode, "IMH03", IMPORT, HEADER);
			CreateAdditionalInfo(helper, countryCode, "IMI01", IMPORT, ITEM);
			CreateAdditionalInfo(helper, countryCode, "IMI02", IMPORT, ITEM);
			CreateAdditionalInfo(helper, countryCode, "IMI03", IMPORT, ITEM);

			CreateAdditionalInfo(helper, countryCode, "EXH01", EXPORT, HEADER);
			CreateAdditionalInfo(helper, countryCode, "EXH02", EXPORT, HEADER);
			CreateAdditionalInfo(helper, countryCode, "EXH03", EXPORT, HEADER);
			CreateAdditionalInfo(helper, countryCode, "EXI01", EXPORT, ITEM);
			CreateAdditionalInfo(helper, countryCode, "EXI02", EXPORT, ITEM);
			CreateAdditionalInfo(helper, countryCode, "EXI03", EXPORT, ITEM);

			CreateAdditionalInfo(helper, countryCode, "00500", IMPORT, ITEM);
			CreateAdditionalInfo(helper, countryCode, "05000", IMPORT, ITEM);

			CreateAdditionalInfo(helper, "CDS", "00500", IMPORT, ITEM);
			CreateAdditionalInfo(helper, "CDS", "05000", IMPORT, ITEM);

			Factory.Save();
		}

		protected virtual List<MergeScenario> PrepareMergeScenarios_ToTestMergeOperation(string countryCode)
		{
			const string EXP = MessageTypeList.Codes.Export;
			const string IMP = MessageTypeList.Codes.Import;

			var list = new List<MergeScenario>();

			#region Scenario_01_EU

			var dec = CreateJobDeclaration_ToTestMergeOperation(IMP, "", 2);

			var invoice = dec.Invoices[0];
			invoice.AdditionalInfos.Add(CreateAdditionalInfo("IMH01"));

			var invLine = invoice.InvoiceLines[0];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("IMI01"));

			var scenario = new MergeScenario(dec);
			scenario.AssertMessage = "EU test scenario 01";

			var entry = scenario.AddEntry("-11-12");
			entry.AddAdditionalInfo("IMH01");

			var entryLine = entry.AddEntryLine("11");
			entryLine.AddAdditionalInfo("IMI01");

			entryLine = entry.AddEntryLine("12");

			list.Add(scenario);

			#endregion  //End: "Scenario_01_EU"

			#region Scenario_02_EU

			dec = CreateJobDeclaration_ToTestMergeOperation(IMP, "", 2, 3);
			dec.AdditionalInfos.Add(CreateAdditionalInfo("IMH01"));
			dec.AdditionalInfos.Add(CreateAdditionalInfo("IMI01"));

			invoice = dec.Invoices[0];
			invoice.AdditionalInfos.Add(CreateAdditionalInfo("IMH02"));

			invLine = invoice.InvoiceLines[0];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("IMI02"));

			invLine = invoice.InvoiceLines[1];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("IMI03"));

			invoice = dec.Invoices[1];
			invoice.AdditionalInfos.Add(CreateAdditionalInfo("IMH03"));

			invLine = invoice.InvoiceLines[2];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("IMI01"));
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("IMI02"));

			scenario = new MergeScenario(dec);
			scenario.AssertMessage = "EU test scenario 02";

			entry = scenario.AddEntry("-11-12");
			entry.AddAdditionalInfo("IMH01");
			entry.AddAdditionalInfo("IMI01");
			entry.AddAdditionalInfo("IMH02");

			entryLine = entry.AddEntryLine("11");
			entryLine.AddAdditionalInfo("IMI02");

			entryLine = entry.AddEntryLine("12");
			entryLine.AddAdditionalInfo("IMI03");

			entry = scenario.AddEntry("-24-25-26");
			entry.AddAdditionalInfo("IMH01");
			entry.AddAdditionalInfo("IMI01");
			entry.AddAdditionalInfo("IMH03");

			entryLine = entry.AddEntryLine("24");
			entryLine = entry.AddEntryLine("25");

			entryLine = entry.AddEntryLine("26");
			entryLine.AddAdditionalInfo("IMI01");
			entryLine.AddAdditionalInfo("IMI02");

			list.Add(scenario);

			#endregion  //End: "Scenario_02_EU"

			#region Scenario_03_EU

			dec = CreateJobDeclaration_ToTestMergeOperation(EXP, "", 1, 2, 3);
			dec.AdditionalInfos.Add(CreateAdditionalInfo("EXH03"));

			invoice = dec.Invoices[0];
			invoice.AdditionalInfos.Add(CreateAdditionalInfo("EXH01"));

			invLine = invoice.InvoiceLines[0];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("EXI02"));

			invoice = dec.Invoices[1];
			invoice.AdditionalInfos.Add(CreateAdditionalInfo("EXH02"));

			invLine = invoice.InvoiceLines[0];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("EXI01"));
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("EXI03"));

			invLine = invoice.InvoiceLines[1];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("EXI02"));

			invoice = dec.Invoices[2];
			invoice.AdditionalInfos.Add(CreateAdditionalInfo("EXH03"));

			invLine = invoice.InvoiceLines[0];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("EXI02"));

			invLine = invoice.InvoiceLines[1];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("EXI03"));

			invLine = invoice.InvoiceLines[2];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("EXI02"));
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("EXI01"));

			scenario = new MergeScenario(dec);
			scenario.AssertMessage = "EU test scenario 03";

			entry = scenario.AddEntry("-11");
			entry.AddAdditionalInfo("EXH03");
			entry.AddAdditionalInfo("EXH01");

			entryLine = entry.AddEntryLine("11");
			entryLine.AddAdditionalInfo("EXI02");

			entry = scenario.AddEntry("-24-25");
			entry.AddAdditionalInfo("EXH03");
			entry.AddAdditionalInfo("EXH02");

			entryLine = entry.AddEntryLine("24");
			entryLine.AddAdditionalInfo("EXI01");
			entryLine.AddAdditionalInfo("EXI03");

			entryLine = entry.AddEntryLine("25");
			entryLine.AddAdditionalInfo("EXI02");

			entry = scenario.AddEntry("-37-38-39");
			entry.AddAdditionalInfo("EXH03");

			entryLine = entry.AddEntryLine("37");
			entryLine.AddAdditionalInfo("EXI02");

			entryLine = entry.AddEntryLine("38");
			entryLine.AddAdditionalInfo("EXI03");

			entryLine = entry.AddEntryLine("39");
			entryLine.AddAdditionalInfo("EXI02");
			entryLine.AddAdditionalInfo("EXI01");

			list.Add(scenario);

			#endregion  //End: "Scenario_03_EU"

			return list;
		}

		protected virtual JobDeclaration CreateJobDeclaration_ToTestMergeOperation(string messageType, string applicationCode, int invLinesOnInvoice1, int invLinesOnInvoice2 = 0, int invLinesOnInvoice3 = 0)
		{
			Assert("From 0 to 3 invoice lines are allowed.", (invLinesOnInvoice1 >= 0) && (invLinesOnInvoice1 <= 3));
			Assert("From 0 to 3 invoice lines are allowed.", (invLinesOnInvoice2 >= 0) && (invLinesOnInvoice2 <= 3));
			Assert("From 0 to 3 invoice lines are allowed.", (invLinesOnInvoice3 >= 0) && (invLinesOnInvoice3 <= 3));

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = messageType;
			if (!string.IsNullOrEmpty(applicationCode))
			{
				dec.JE_ApplicationCode = applicationCode;
			}
			else
			{
				dec.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			}
			dec.JE_TotalNoOfPacks = 5;
			dec.JE_MasterBill = "MB111111112";
			dec.JE_VesselName = "TITANIC";

			JobComInvoiceHeader invoice = null;
			JobComInvoiceLine invLine;
			PreviousDocument prevDoc;

			if (invLinesOnInvoice1 > 0)
			{
				invoice = dec.Invoices.AddNew();

				invLine = invoice.InvoiceLines.AddNew();
				invLine.JI_Procedure = "P11";
				invLine.JI_Description = "Invoice line desc 1-1";
				invLine.JI_LinePrice = 11;
				invLine.JI_CustomsQuantity = 11;
				invLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;

				prevDoc = invLine.PreviousDocuments.AddNew();
				prevDoc.CSI_Code = "110";
				prevDoc.CSI_SubType = "Z";
				prevDoc.CSI_ReferenceNumber = "PrevDoc";

				if (invLinesOnInvoice1 > 1)
				{
					invLine = invoice.InvoiceLines.AddNew();
					invLine.JI_Procedure = "P12";
					invLine.JI_Description = "Invoice line desc 1-2";
					invLine.JI_LinePrice = 12;
					invLine.JI_CustomsQuantity = 12;
					invLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;

					prevDoc = invLine.PreviousDocuments.AddNew();
					prevDoc.CSI_Code = "120";
					prevDoc.CSI_SubType = "Z";
					prevDoc.CSI_ReferenceNumber = "PrevDoc";

					if (invLinesOnInvoice1 > 2)
					{
						invLine = invoice.InvoiceLines.AddNew();
						invLine.JI_Procedure = "P13";
						invLine.JI_Description = "Invoice line desc 1-3";
						invLine.JI_LinePrice = 13;
						invLine.JI_CustomsQuantity = 13;
						invLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;

						prevDoc = invLine.PreviousDocuments.AddNew();
						prevDoc.CSI_Code = "130";
						prevDoc.CSI_SubType = "Z";
						prevDoc.CSI_ReferenceNumber = "PrevDoc";
					}
				}
			}

			if (invLinesOnInvoice2 > 0)
			{
				invoice = dec.Invoices.AddNew();

				invLine = invoice.InvoiceLines.AddNew();
				invLine.JI_Procedure = "P21";
				invLine.JI_Description = "Invoice line desc 2-1";
				invLine.JI_LinePrice = 24;
				invLine.JI_CustomsQuantity = 24;
				invLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;

				prevDoc = invLine.PreviousDocuments.AddNew();
				prevDoc.CSI_Code = "210";
				prevDoc.CSI_SubType = "Z";
				prevDoc.CSI_ReferenceNumber = "PrevDoc";

				if (invLinesOnInvoice2 > 1)
				{
					invLine = invoice.InvoiceLines.AddNew();
					invLine.JI_Procedure = "P22";
					invLine.JI_Description = "Invoice line desc 2-2";
					invLine.JI_LinePrice = 25;
					invLine.JI_CustomsQuantity = 25;
					invLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;

					prevDoc = invLine.PreviousDocuments.AddNew();
					prevDoc.CSI_Code = "220";
					prevDoc.CSI_SubType = "Z";
					prevDoc.CSI_ReferenceNumber = "PrevDoc";

					if (invLinesOnInvoice2 > 2)
					{
						invLine = invoice.InvoiceLines.AddNew();
						invLine.JI_Procedure = "P23";
						invLine.JI_Description = "Invoice line desc 2-3";
						invLine.JI_LinePrice = 26;
						invLine.JI_CustomsQuantity = 26;
						invLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;

						prevDoc = invLine.PreviousDocuments.AddNew();
						prevDoc.CSI_Code = "230";
						prevDoc.CSI_SubType = "Z";
						prevDoc.CSI_ReferenceNumber = "PrevDoc";
					}
				}
			}

			if (invLinesOnInvoice3 > 0)
			{
				invoice = dec.Invoices.AddNew();

				invLine = invoice.InvoiceLines.AddNew();
				invLine.JI_Procedure = "P31";
				invLine.JI_Description = "Invoice line desc 3-1";
				invLine.JI_LinePrice = 37;
				invLine.JI_CustomsQuantity = 37;
				invLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;

				prevDoc = invLine.PreviousDocuments.AddNew();
				prevDoc.CSI_Code = "310";
				prevDoc.CSI_SubType = "Z";
				prevDoc.CSI_ReferenceNumber = "PrevDoc";

				if (invLinesOnInvoice3 > 1)
				{
					invLine = invoice.InvoiceLines.AddNew();
					invLine.JI_Procedure = "P32";
					invLine.JI_Description = "Invoice line desc 3-2";
					invLine.JI_LinePrice = 38;
					invLine.JI_CustomsQuantity = 38;
					invLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;

					prevDoc = invLine.PreviousDocuments.AddNew();
					prevDoc.CSI_Code = "320";
					prevDoc.CSI_SubType = "Z";
					prevDoc.CSI_ReferenceNumber = "PrevDoc";

					if (invLinesOnInvoice3 > 2)
					{
						invLine = invoice.InvoiceLines.AddNew();
						invLine.JI_Procedure = "P33";
						invLine.JI_Description = "Invoice line desc 3-3";
						invLine.JI_LinePrice = 39;
						invLine.JI_CustomsQuantity = 39;
						invLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;

						prevDoc = invLine.PreviousDocuments.AddNew();
						prevDoc.CSI_Code = "330";
						prevDoc.CSI_SubType = "Z";
						prevDoc.CSI_ReferenceNumber = "PrevDoc";
					}
				}
			}

			return dec;
		}

		protected virtual AdditionalInfo CreateAdditionalInfo(string code)
		{
			var addInfo = Factory.New<AdditionalInfo>();
			addInfo.CSI_Code = code;
			return addInfo;
		}

		protected virtual void Produce_EDI_Message_ToTestMergeOperation(MergeScenario scenario)
		{
		}

		protected virtual List<AdditionalInfo> GetAddInfoListFromEntryHeader(CusEntryHeader entryHeader) => entryHeader.AdditionalInfos.ToList();

		protected virtual List<AdditionalInfo> GetAddInfoListFromEntryLine(CusEntryLine entryLine) => entryLine.AdditionalInfos.ToList();

		protected virtual void Check_EDI_Message_ToTestMergeOperation(MergeScenario scenario, Entry_ToTestMergeOperation expectedEntry, CusEntryHeader entryHeader)
		{
		}

		protected void TestClassTypesBeingUsed_Core(string applicationCode)
		{
			var countryCode = GetCountryCode_ToTestClassTypes();
			PrepareUniversalData_ToTestClassTypes(countryCode);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var dec = CreateJobDeclaration_ToTestClassTypes(applicationCode);
				var shutterUpperer = new SendsMessagesToCustomsShutterUpperer(false);

				var mergeResult = dec.DoMerge(shutterUpperer);
				Assert("Merge must be successful.", mergeResult);

				InspectVariousObjects_ToTestClassTypes(dec);
			}
		}

		protected virtual string GetCountryCode_ToTestClassTypes() => Core.Constants.CountryCodes.Latvia;

		protected virtual void InspectVariousObjects_ToTestClassTypes(JobDeclaration dec)
		{
			const string EU = "EU";

			var invoice = dec.Invoices[0];
			var invLine = invoice.InvoiceLines[0];

			CheckClassType(EU, typeof(JobDeclaration), dec);
			CheckClassType(EU, typeof(JobComInvoiceHeader), invoice);
			CheckClassType(EU, typeof(JobComInvoiceLine), invLine);

			var entry = dec.CustomsEntryHeaders[0];
			var entryLine = entry.AllEntryLines[0];

			CheckClassType(EU, typeof(CusEntryHeader), entry);
			CheckClassType(EU, typeof(CusEntryLine), entryLine);

			var euEntry = entry;
			var euEntryLine = entryLine;

			var list = new List<object>();

			list.AddRange(dec.AdditionalInfos);
			list.AddRange(invoice.AdditionalInfos);
			list.AddRange(invLine.AdditionalInfos);
			list.AddRange(euEntry.AdditionalInfos);
			list.AddRange(euEntryLine.AdditionalInfos);

			foreach (var obj in list)
			{
				CheckClassType(EU, typeof(AdditionalInfo), obj);
			}

			list.Clear();
			list.AddRange(dec.SupportingDocuments);
			list.AddRange(invoice.SupportingDocuments);
			list.AddRange(invLine.SupportingDocuments);
			list.AddRange(euEntry.SupportingDocuments);
			list.AddRange(euEntryLine.SupportingDocuments);

			foreach (var obj in list)
			{
				CheckClassType(EU, typeof(SupportingDocument), obj);
			}

			list.Clear();
			list.AddRange(dec.PreviousDocuments);
			list.AddRange(invoice.PreviousDocuments);
			list.AddRange(invLine.PreviousDocuments);
			list.AddRange(euEntry.PreviousDocuments);
			list.AddRange(euEntryLine.PreviousDocuments);

			foreach (var obj in list)
			{
				CheckClassType(EU, typeof(PreviousDocument), obj);
			}
		}

		protected void CompareAdditionalInfos_ToTestMergeOperation(string msg, List<AddInfo_ToTestMergeOperation> expectedList, Dictionary<string, AdditionalInfo> lookup)
		{
			AssertEquals(msg + ": Total number of AdditionalInfo objects.", expectedList.Count, lookup.Count);

			foreach (var expectedAddInfo in expectedList)
			{
				var code = expectedAddInfo.Code;
				var addInfo = lookup[code];
				Assert(msg + ": AdditionalInfo not found: " + code, addInfo != null);
			}
		}

		protected void CheckClassType(string domain, Type theType, object obj)
		{
			var msg = $"For {domain}, the object instance to be tested, cannot be null.";
			Assert(msg, obj != null);

			msg = $"For {domain}, the expected class type cannot be null, when testing object instance: {obj.GetType().FullName}.";
			Assert(msg, theType != null);

			msg = $"For {domain}, class type must be: {theType.FullName}, when testing object instance: {obj.GetType().FullName}.";
			AssertEquals(msg, theType, obj.GetType());
		}

		protected virtual Type ExpectedMergeManagerType => typeof(MergeManager);

		protected abstract Type ExpectedDeclarationLevelPackageCollectionType { get; }

		protected override System.Collections.Hashtable ExpectedDocAddressTypes
		{
			get
			{
				var result = base.ExpectedDocAddressTypes;
				result[DocAddressTypes.Codes.CustomsSupervisingOffice] = DocAddressType.CustomsSupervisingOffice;
				result[DocAddressTypes.Codes.GovernmentContractor] = DocAddressType.GovernmentContractor;
				result[DocAddressTypes.Codes.CustomsPlaceOfLoading] = DocAddressType.CustomsPlaceOfLoading;
				result[DocAddressTypes.Codes.SellingParty] = DocAddressType.SellingParty;
				result[DocAddressTypes.Codes.Exporter] = DocAddressType.Exporter;
				result[DocAddressTypes.Codes.Representative] = DocAddressType.Representative;
				result[DocAddressTypes.Codes.DefermentParty] = DocAddressType.DefermentParty;
				result[DocAddressTypes.Codes.ContractualPartner] = DocAddressType.ContractualPartner;
				result[DocAddressTypes.Codes.Carrier] = DocAddressType.Carrier;
				return result;
			}
		}

		protected override bool ExpectedSupportsJobComInvoiceLineTax => true;

		void CreateAdditionalInfo(UniversalReferenceTestDataHelper helper, string countryCode, string code, string directionValue, string levelValue)
		{
			const string ADDIN = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation;
			const string Direction = "Direction";
			const string Level = "Level";

			var dtMIN = ZDateTime.MinSmallDateTimeValue;
			var dtMAX = ZDateTime.MaxSmallDateTimeValue;

			var refCusCodeList = helper.CreateCusCodeList(countryCode, ADDIN, code, code + " - Description", dtMIN, dtMAX);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Direction, "Desc.", ADDIN, countryCode, ADDIN);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Level, "Desc.", ADDIN, countryCode, ADDIN);
			refCusCodeList.Attributes.AddNew(Direction, directionValue);
			refCusCodeList.Attributes.AddNew(Level, levelValue);
		}

		void CheckEntries_ToTestMergeOperation(MergeScenario scenario)
		{
			var msg = scenario.AssertMessage;
			var dec = scenario.JobDeclaration;

			AssertEquals(msg + ": No of Entries", scenario.TotalNoOfEntriesExpected, dec.ActiveEntryHeaders.Count);
			var entryLookup = CreateEntryLookup(dec.ActiveEntryHeaders);

			foreach (var expectedEntry in scenario.ExpectedEntryList)
			{
				var entryHeader = FindCorrespondingEntryHeader(expectedEntry, entryLookup);
				Assert(msg + ": Entry header not found.", entryHeader != null);

				var addInfoLookup = CreateAddInfoLookup_ToTestMergeOperation(GetAddInfoListFromEntryHeader(entryHeader));
				CompareAdditionalInfos_ToTestMergeOperation(msg, expectedEntry.AdditionalInfoList, addInfoLookup);

				var entryLineLookup = CreateEntryLineLookup(entryHeader);
				AssertEquals(msg + ": No of entry lines", expectedEntry.EntryLines.Count, entryLineLookup.Count);

				foreach (var expectedLine in expectedEntry.EntryLines)
				{
					var entryLine = FindCorrespondingEntryLine(expectedLine, entryLineLookup);
					Assert(msg + ": Entry line not found.", entryLine != null);

					addInfoLookup = CreateAddInfoLookup_ToTestMergeOperation(GetAddInfoListFromEntryLine(entryLine));
					CompareAdditionalInfos_ToTestMergeOperation(msg, expectedLine.AdditionalInfoList, addInfoLookup);
				}

				Check_EDI_Message_ToTestMergeOperation(scenario, expectedEntry, entryHeader);
			}
		}

		Dictionary<string, CusEntryHeader> CreateEntryLookup(ActiveCusEntryHeaderCollection col)
		{
			var lookup = new Dictionary<string, CusEntryHeader>();
			var list = col.ToList();

			foreach (var obj in list)
			{
				var entryHeader = obj as CusEntryHeader;
				if (entryHeader != null)
				{
					var key = DetermineEntryKey(entryHeader);
					lookup.Add(key, entryHeader);
				}
			}

			return lookup;
		}

		string DetermineEntryKey(CusEntryHeader entryHeader)
		{
			var sb = new StringBuilder();
			var list = entryHeader.MergedLines.ToArray();

			foreach (var obj in list)
			{
				var entryLine = obj as CusEntryLine;
				if (entryLine != null)
				{
					sb.Append("-");
					sb.Append(entryLine.CustomsQuantity);
				}
			}

			var key = sb.ToString();
			return key;
		}

		CusEntryHeader FindCorrespondingEntryHeader(Entry_ToTestMergeOperation expectedEntry, Dictionary<string, CusEntryHeader> entryLookup)
		{
			entryLookup.TryGetValue(expectedEntry.lookupKey, out var entryHeader);
			return entryHeader;
		}

		CusEntryLine FindCorrespondingEntryLine(EntryLine_ToTestMergeOperation expectedLine, Dictionary<string, CusEntryLine> entryLineLookup)
		{
			entryLineLookup.TryGetValue(expectedLine.lookupKey, out var entryLine);
			return entryLine;
		}

		Dictionary<string, AdditionalInfo> CreateAddInfoLookup_ToTestMergeOperation(List<AdditionalInfo> list)
		{
			var lookup = new Dictionary<string, AdditionalInfo>();

			foreach (var obj in list)
			{
				if (obj != null)
				{
					lookup.Add(obj.CSI_Code, obj);
				}
			}

			return lookup;
		}

		Dictionary<string, CusEntryLine> CreateEntryLineLookup(CusEntryHeader entryHeader)
		{
			var lookup = new Dictionary<string, CusEntryLine>();
			var list = entryHeader.MergedLines.ToList();

			foreach (var obj in list)
			{
				var entryLine = obj;
				if (entryLine != null)
				{
					lookup.Add(entryLine.CustomsQuantity.ToString(), entryLine);
				}
			}

			return lookup;
		}

		void PrepareUniversalData_ToTestClassTypes(string countryCode)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(countryCode, "To test Class types");

			const string ADDIN = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation;

			helper.CreateNewOrGetExistingCusCodeType(ADDIN, "AdditionalInformation");

			Create_CusCodeList_RefData(helper, countryCode, ADDIN, "ADD01");
			Create_CusCodeList_RefData(helper, countryCode, ADDIN, "ADD02");
			Create_CusCodeList_RefData(helper, countryCode, ADDIN, "ADD03");

			Create_CusCodeList_RefData(helper, countryCode, ADDIN, "00500");
			Create_CusCodeList_RefData(helper, countryCode, ADDIN, "05000");

			Create_CusCodeList_RefData(helper, "CDS", ADDIN, "00500");
			Create_CusCodeList_RefData(helper, "CDS", ADDIN, "05000");

			const string DOC44 = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocument;
			const string DC44I = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;

			helper.CreateNewOrGetExistingCusCodeType(DOC44, "SupportingDocument", countryCode);
			helper.CreateNewOrGetExistingCusCodeType(DC44I, "SupportingDocImports", countryCode);

			Create_CusCodeList_RefData(helper, countryCode, DOC44, "SUP01");
			Create_CusCodeList_RefData(helper, countryCode, DOC44, "SUP02");
			Create_CusCodeList_RefData(helper, countryCode, DOC44, "SUP03");

			Create_CusCodeList_RefData(helper, countryCode, DC44I, "SIM01");
			Create_CusCodeList_RefData(helper, countryCode, DC44I, "SIM02");
			Create_CusCodeList_RefData(helper, countryCode, DC44I, "SIM03");

			Factory.Save();
		}

		void Create_CusCodeList_RefData(UniversalReferenceTestDataHelper helper, string countryCode, string refDataType, string code)
		{
			const string Direction = "Direction";
			const string Level = "Level";

			const string IMPORT = "IMPORT";
			const string HEADER = "HEADER";
			const string ITEM = "ITEM";

			var dtMIN = ZDateTime.MinSmallDateTimeValue;
			var dtMAX = ZDateTime.MaxSmallDateTimeValue;

			var refData = helper.CreateCusCodeList(countryCode, refDataType, code, code + " - Description", dtMIN, dtMAX);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Direction, "Desc.", refDataType, countryCode, refDataType);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Level, "Desc.", refDataType, countryCode, refDataType);

			refData.Attributes.AddNew(Direction, IMPORT);
			refData.Attributes.AddNew(Level, HEADER);
			refData.Attributes.AddNew(Level, ITEM);
		}

		public void TestExitControlTabVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("True for export", true, declaration.ExitControlTabVisible);
			declaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("True for Misc", true, declaration.ExitControlTabVisible);
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("False for Import", false, declaration.ExitControlTabVisible);
		}

		public void TestEmptyIsSecurityDeclarationIfNecessary()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				declaration.JE_MessageType = "EXP";
				declaration.ZG_IsSecurityDeclaration = true;

				declaration.JE_MessageType = "IMP";
				AssertEquals("Empty IsSecurityDeclaration switching from UCC6 EXP to IMP, IsSecurityDeclaration", false, declaration.ZG_IsSecurityDeclaration);
			}
		}

		public virtual void TestSupportsCalculateInsurance()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("The default value should be false for export", false, declaration.SupportsCalculateInsurance);
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				AssertEquals("The default value should be true for import ucc6", true, declaration.SupportsCalculateInsurance);
			}
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC5Core", true))
			{
				AssertEquals("The default value should be true for import ucc5", true, declaration.SupportsCalculateInsurance);
			}
		}

		public virtual void TestChangeMessageTypeFromSupplierOrImporter()
		{
			var uNLOCOAU = Factory.New<RefUNLOCO>();
			uNLOCOAU.RL_Code = "AU";
			uNLOCOAU.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var uNLOCOFR = Factory.New<RefUNLOCO>();
			uNLOCOFR.RL_Code = "FR";
			uNLOCOFR.RL_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var orgHeaderSupplier1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderSupplier1.OH_RL_NKClosestPort = "AU";
			var supplieradr1 = orgHeaderSupplier1.Addresses.AddNewMainAddress();
			var orgHeaderSupplier2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderSupplier2.OH_RL_NKClosestPort = "AU";
			var supplieradr2 = orgHeaderSupplier1.Addresses.AddNewMainAddress();
			var orgHeaderSupplier3 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderSupplier3.OH_RL_NKClosestPort = "FR";
			var supplieradr3 = orgHeaderSupplier1.Addresses.AddNewMainAddress();

			var orgHeaderImporter1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderImporter1.OH_RL_NKClosestPort = "AU";
			var importeradr1 = orgHeaderImporter1.Addresses.AddNewMainAddress();
			var orgHeaderImporter2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderImporter2.OH_RL_NKClosestPort = "AU";
			var importeradr2 = orgHeaderImporter2.Addresses.AddNewMainAddress();
			var orgHeaderImporter3 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderImporter3.OH_RL_NKClosestPort = "FR";
			var importeradr3 = orgHeaderImporter2.Addresses.AddNewMainAddress();

			CombineAssertions(() =>
			{
				using (EUCustomsDataRegistry.Instance.CheckChangeMessageTypeFromSupplierOrImporter.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.France))
					{
						declaration.SupplierDocumentaryAddress.E2_OA_Address = orgHeaderSupplier1.MainAddress.PK;
						declaration.ImporterDocumentaryAddress.E2_OA_Address = orgHeaderImporter1.MainAddress.PK;
						declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
						declaration.SupplierDocumentaryAddress.E2_OA_Address = orgHeaderSupplier2.MainAddress.PK;
						AssertEquals("Registry = true, Answer = Yes, Supplier AU", JobMessageTypeList.Codes.Export, declaration.JE_MessageType);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
						declaration.SupplierDocumentaryAddress.E2_OA_Address = orgHeaderSupplier3.MainAddress.PK;
						AssertEquals("Registry = true, Answer = Yes, Supplier FR", JobMessageTypeList.Codes.Export, declaration.JE_MessageType);

						declaration.SupplierDocumentaryAddress.E2_OA_Address = orgHeaderSupplier1.MainAddress.PK;
						declaration.ImporterDocumentaryAddress.E2_OA_Address = orgHeaderImporter1.MainAddress.PK;
						declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
						declaration.SupplierDocumentaryAddress.E2_OA_Address = orgHeaderSupplier2.MainAddress.PK;
						AssertEquals("Registry = true, Answer = No, Supplier AU", JobMessageTypeList.Codes.WarehousedByExternalAgent, declaration.JE_MessageType);

						declaration.SupplierDocumentaryAddress.E2_OA_Address = orgHeaderSupplier1.MainAddress.PK;
						declaration.ImporterDocumentaryAddress.E2_OA_Address = orgHeaderImporter1.MainAddress.PK;
						declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
						declaration.ImporterDocumentaryAddress.E2_OA_Address = orgHeaderImporter2.MainAddress.PK;
						AssertEquals("Registry = true, Answer = Yes, Importer AU", JobMessageTypeList.Codes.Export, declaration.JE_MessageType);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
						declaration.ImporterDocumentaryAddress.E2_OA_Address = orgHeaderImporter3.MainAddress.PK;
						AssertEquals("Registry = true, Answer = Yes, Importer FR", JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
					}
				}

				using (EUCustomsDataRegistry.Instance.CheckChangeMessageTypeFromSupplierOrImporter.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.France;
					declaration.ImporterDocumentaryAddress.E2_OA_Address = orgHeaderImporter1.MainAddress.PK;
					declaration.SupplierDocumentaryAddress.E2_OA_Address = orgHeaderSupplier1.MainAddress.PK;
					declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
					declaration.SupplierDocumentaryAddress.E2_OA_Address = orgHeaderSupplier2.MainAddress.PK;
					AssertEquals("Registry = false, Supplier", JobMessageTypeList.Codes.Export, declaration.JE_MessageType);

					GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
					declaration.ImporterDocumentaryAddress.E2_OA_Address = orgHeaderImporter1.MainAddress.PK;
					declaration.SupplierDocumentaryAddress.E2_OA_Address = orgHeaderSupplier1.MainAddress.PK;
					declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
					declaration.ImporterDocumentaryAddress.E2_OA_Address = orgHeaderImporter2.MainAddress.PK;
					AssertEquals("Registry = false, Importer", JobMessageTypeList.Codes.Export, declaration.JE_MessageType);
				}
			});
		}

		JobDeclaration CreateJobDeclaration_ToTestClassTypes(string applicationCode)
		{
			var dec = CreateJobDeclaration_ToTestMergeOperation(MessageTypeList.Codes.Import, applicationCode, 1);

			var addInfo = dec.AdditionalInfos.AddNew();
			addInfo.CSI_Code = "ADD01";

			var supDoc = dec.SupportingDocuments.AddNew();
			supDoc.CSI_Code = "SUP01";

			var prevDoc = dec.PreviousDocuments.AddNew();
			prevDoc.CSI_Code = "SIM01";

			var invoice = dec.Invoices[0];

			addInfo = invoice.AdditionalInfos.AddNew();
			addInfo.CSI_Code = "ADD02";

			supDoc = invoice.SupportingDocuments.AddNew();
			supDoc.CSI_Code = "SUP02";

			prevDoc = invoice.PreviousDocuments.AddNew();
			prevDoc.CSI_Code = "SIM02";

			var invLine = invoice.InvoiceLines[0];

			addInfo = invLine.AdditionalInfos.AddNew();
			addInfo.CSI_Code = "ADD03";

			supDoc = invLine.SupportingDocuments.AddNew();
			supDoc.CSI_Code = "SUP03";

			prevDoc = invLine.PreviousDocuments.AddNew();
			prevDoc.CSI_Code = "SIM03";

			return dec;
		}

		protected class MergeScenario
		{
			public MergeScenario(JobDeclaration jobDeclaration)
			{
				ExpectedEntryList = new List<Entry_ToTestMergeOperation>();
				JobDeclaration = jobDeclaration;
			}

			public Entry_ToTestMergeOperation AddEntry(string lookupKey)
			{
				var entry = new Entry_ToTestMergeOperation(lookupKey);
				ExpectedEntryList.Add(entry);
				return entry;
			}

			public JobDeclaration JobDeclaration { get; }

			public string AssertMessage { get; set; }

			public List<Entry_ToTestMergeOperation> ExpectedEntryList { get; }

			public int TotalNoOfEntriesExpected => ExpectedEntryList.Count;
		}

		protected class Entry_ToTestMergeOperation
		{
			public Entry_ToTestMergeOperation(string lookupKey)
			{
				AdditionalInfoList = new List<AddInfo_ToTestMergeOperation>();
				EntryLines = new List<EntryLine_ToTestMergeOperation>();

				this.lookupKey = lookupKey;
			}

			public void AddAdditionalInfo(AddInfo_ToTestMergeOperation addInfo)
			{
				AdditionalInfoList.Add(addInfo);
			}

			public void AddAdditionalInfo(string code)
			{
				var addInfo = new AddInfo_ToTestMergeOperation(code, code);
				AdditionalInfoList.Add(addInfo);
			}

			public void AddEntryLine(EntryLine_ToTestMergeOperation entryLine)
			{
				EntryLines.Add(entryLine);
			}

			public EntryLine_ToTestMergeOperation AddEntryLine(string lookupKey)
			{
				var entryLine = new EntryLine_ToTestMergeOperation(lookupKey);
				EntryLines.Add(entryLine);
				return entryLine;
			}

			public List<AddInfo_ToTestMergeOperation> AdditionalInfoList { get; }

			public List<EntryLine_ToTestMergeOperation> EntryLines { get; }

			public string lookupKey { get; }
		}

		protected class AddInfo_ToTestMergeOperation
		{
			public AddInfo_ToTestMergeOperation(string code, string text)
			{
				Code = code;
				Text = text;
			}

			public ZString Code { get; set; }

			public ZString Text { get; set; }
		}

		protected class EntryLine_ToTestMergeOperation
		{
			public EntryLine_ToTestMergeOperation(string lookupKey)
			{
				AdditionalInfoList = new List<AddInfo_ToTestMergeOperation>();

				this.lookupKey = lookupKey;
			}

			public void AddAdditionalInfo(AddInfo_ToTestMergeOperation addInfo)
			{
				AdditionalInfoList.Add(addInfo);
			}

			public void AddAdditionalInfo(string code)
			{
				var addInfo = new AddInfo_ToTestMergeOperation(code, code);
				AdditionalInfoList.Add(addInfo);
			}

			public List<AddInfo_ToTestMergeOperation> AdditionalInfoList { get; }
			public string lookupKey { get; }
		}

		#region IUcc6ValueProvider

		public void TestIUcc6ValueProvider_IsUCC6()
		{
			var declaration = Factory.New<JobDeclaration>();
			IUcc6ValueProvider ucc6ValueProvider = declaration;

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				AssertEquals("Non-UCC6", false, ucc6ValueProvider.IsUCC6);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				AssertEquals("UCC6", true, ucc6ValueProvider.IsUCC6);
			}
		}

		public void TestIUcc6ValueProvider_IsExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			IUcc6ValueProvider ucc6ValueProvider = declaration;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals(nameof(IUcc6ValueProviderExtensions.IsUCC6AndIsExport), true, ucc6ValueProvider.IsUCC6AndIsExport());
				AssertEquals(nameof(IUcc6ValueProvider.IsExport), true, ucc6ValueProvider.IsExport);

				declaration.JE_MessageType = "COM";
				AssertEquals(nameof(IUcc6ValueProviderExtensions.IsUCC6AndIsExport), false, ucc6ValueProvider.IsUCC6AndIsExport());
				AssertEquals(nameof(IUcc6ValueProvider.IsExport), false, ucc6ValueProvider.IsExport);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: false))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals(nameof(IUcc6ValueProviderExtensions.IsUCC6AndIsExport), false, ucc6ValueProvider.IsUCC6AndIsExport());
				AssertEquals(nameof(IUcc6ValueProvider.IsExport), true, ucc6ValueProvider.IsExport);
			}
		}

		public void TestIUcc6ValueProvider_IsImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			IUcc6ValueProvider ucc6ValueProvider = declaration;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals(nameof(IUcc6ValueProviderExtensions.IsUCC6AndIsImport), true, ucc6ValueProvider.IsUCC6AndIsImport());
				AssertEquals(nameof(IUcc6ValueProvider.IsImport), true, ucc6ValueProvider.IsImport);

				declaration.JE_MessageType = "COM";
				AssertEquals(nameof(IUcc6ValueProviderExtensions.IsUCC6AndIsImport), false, ucc6ValueProvider.IsUCC6AndIsImport());
				AssertEquals(nameof(IUcc6ValueProvider.IsImport), false, ucc6ValueProvider.IsImport);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: false))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals(nameof(IUcc6ValueProviderExtensions.IsUCC6AndIsImport), false, ucc6ValueProvider.IsUCC6AndIsImport());
				AssertEquals(nameof(IUcc6ValueProvider.IsImport), true, ucc6ValueProvider.IsImport);
			}
		}
		#endregion

		[CodeAlive("Test Code")]
		class JobDeclarationForTest : JobDeclaration
		{
			public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
		}
	}

	sealed class JobDeclarationUniversalCopyTest : BaseAddInfoUniversalCopyTest
	{
		protected override void AssertHasOtherNodes(string[] allNodeNames)
		{
			AssertCollectionContains("CustomFields", "CustomFields", allNodeNames);
		}

		protected override IAddInfoManager GetManager()
		{
			return Factory.New<JobDeclaration>();
		}
	}

	class JobDeclarationForEntryStyleCalculationTest : JobDeclaration
	{
		public JobDeclarationForEntryStyleCalculationTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public IEntryStyleCalculationStrategy GetEntryStyleCalculationStrategyExposed() => GetEntryStyleCalculationStrategy();

		public ZString GetEntryStyleByEntryTypeExposed() => GetEntryStyleByEntryType();
	}

	class JobDeclarationForDefermentPartyDocAddressTest : JobDeclaration
	{
		public JobDeclarationForDefermentPartyDocAddressTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void DefermentPartyDocAddressChanged(object sender, EventArgs e)
		{
			base.DefermentPartyDocAddressChanged(sender, e);
			EventHandlerTriggered = true;
		}

		public bool EventHandlerTriggered { get; set; }
	}
}
