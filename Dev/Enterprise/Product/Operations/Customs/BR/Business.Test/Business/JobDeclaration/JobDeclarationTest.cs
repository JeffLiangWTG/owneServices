using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.CustomsLists;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(JobDeclaration))]
	class JobDeclarationTest : Customs.Business.Testing.BaseJobDeclarationTest<JobDeclaration>
	{
		public void TestIsTransportByWater()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			Assert(declaration.IsSea);
			Assert(!declaration.IsRiver);
			Assert(!declaration.IsLake);
			Assert(declaration.IsTransportByWater);
			declaration.JE_TransportMode = TransportTypeList.Codes.River;
			Assert(!declaration.IsSea);
			Assert(declaration.IsRiver);
			Assert(!declaration.IsLake);
			Assert(declaration.IsTransportByWater);
			declaration.JE_TransportMode = TransportTypeList.Codes.Lake;
			Assert(!declaration.IsSea);
			Assert(!declaration.IsRiver);
			Assert(declaration.IsLake);
			Assert(declaration.IsTransportByWater);
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			Assert(!declaration.IsSea);
			Assert(!declaration.IsRiver);
			Assert(!declaration.IsLake);
			Assert(!declaration.IsTransportByWater);
		}

		public void TestIsMantraApplicable()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			Assert(declaration.IsMantraApplicable);

			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			Assert(!declaration.IsMantraApplicable);

			declaration.JE_TransportMode = TransportTypeList.Codes.Lake;
			Assert(!declaration.IsMantraApplicable);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			Assert(!declaration.IsMantraApplicable);

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			Assert(!declaration.IsMantraApplicable);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			Assert(!declaration.IsMantraApplicable);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			Assert(!declaration.IsMantraApplicable);
		}

		public void TestShouldCalculateAfrmm()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_TransportMode = TransportTypeList.Codes.River;
			Assert(!declaration.ShouldCalculateAfrmm);

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			Assert(!declaration.ShouldCalculateAfrmm);

			entryInstruction.IsAFRMMRateOverridden = true;
			Assert(declaration.ShouldCalculateAfrmm);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			Assert(declaration.ShouldCalculateAfrmm);

			entryInstruction.IsAFRMMRateOverridden = false;
			entryInstruction.CEI_AFRMMMethodOfCalculation = "FMM1";
			Assert(declaration.ShouldCalculateAfrmm);

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			Assert(!declaration.ShouldCalculateAfrmm);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.River;
			Assert(!declaration.ShouldCalculateAfrmm);
		}

		public void TestJE_VoyageFlightNoCaption()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_TransportMode = Core.Constants.TransportModes.Road;
			var info = dec.JE_VoyageFlightNoInfo;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Road", "Registration", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Registration", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			dec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Sea", "Voyage", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Voyage", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			dec.JE_TransportMode = TransportTypeList.Codes.Lake;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Lake", "Voyage", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Voyage", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			dec.JE_TransportMode = TransportTypeList.Codes.River;
			AssertEquals("JE_VoyageFlightNoInfo.Description - River", "Voyage", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Voyage", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Air", "Flight/Folio", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Flight/Folio", mediumCaption: "Flight No.", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			dec.JE_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Rail", "Voyage / Flight No", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Voyage / Flight No", shortCaption: "Voyage/Flight", fullDescription: "A unique reference assigned by a carrier to identify a specific journey of an aircraft or vessel.", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
		}

		public void TestLookups()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertEquals("Type of Lookups", typeof(JobDeclarationLookups), dec.Lookups.GetType());
		}

		public void TestLoadedProcessRelatedCollection()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertEquals("Type of ProcessRelatedCollection", typeof(ProcessRelatedNumberCollection), dec.ProcessRelatedNumbers.GetType());
			dec.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var num = dec.ProcessRelatedNumbers.AddNew();
			AssertEquals("Have 1 row", 1, dec.ProcessRelatedNumbers.Count);
			dec.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertEquals("Have 0 row", 0, dec.ProcessRelatedNumbers.Count);
		}

		public void TestLoadedDispatchInstructionCollection()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertEquals("Type of ProcessRelatedCollection", typeof(DispatchInstructionNumberCollection), dec.DispatchInstructionNumbers.GetType());
			dec.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var num = dec.DispatchInstructionNumbers.AddNew();
			AssertEquals("Have 1 row", 1, dec.DispatchInstructionNumbers.Count);
			dec.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertEquals("Have 0 row", 0, dec.DispatchInstructionNumbers.Count);
		}

		public override void TestAreMultipleEntryInstructionsAllowed()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertEquals(false, dec.AreMultipleEntryInstructionsAllowed);
		}

		public override void TestLocalCurrencyCoreOverride()
		{
			AssertEquals("Replace this with the correct currency code when implemented in a real country", Core.Constants.CurrencyCodes.Brazil, GetJobDeclarationForTesting().LocalCurrencyCode);
		}

		public void TestHouseBillsCollectionIsOfRightType()
		{
			var declaration = (JobDeclaration)GetNewBusinessObject();
			AssertEquals(typeof(BillCollection<Bill, JobDeclaration>), declaration.Bills.GetType());
		}

		public void TestLookupObjectIsCached()
		{
			var bizO = (JobDeclaration)GetNewBusinessObject();
			var firstLookup = bizO.Lookups;
			var secondLookup = bizO.Lookups;
			AssertEquals(secondLookup, firstLookup);
		}

		public void TestTypeDecider()
		{
			Assert("Update dbo.JobDeclaration to include a decider for this class", Factory.New<JobDeclaration>().GetType() == GetExpectedBusinessObjectType());
		}

		public override void TestIsDeclarationWithEntryInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			Assert("BR Declaration should support EntryInstructions", !declaration.CustomsEntryInstructionProvider.IsNoEntryInstruction);
		}

		public void TestDefaultJE_ApplicationCode()
		{
			var customsInterface = new LocalCountryCustomsInterface { RecipientID = "RecipientID", SubmissionType = DeclarationApplicationCodeList.Codes.Builtin };
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				AssertEquals("Builtin JE_ApplicationCode", DeclarationApplicationCodeList.Codes.Builtin, Factory.New<JobDeclaration>().JE_ApplicationCode);
			}
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				AssertEquals("BothBuiltInDefaulted JE_ApplicationCode", DeclarationApplicationCodeList.Codes.Builtin, Factory.New<JobDeclaration>().JE_ApplicationCode);
			}
			customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				AssertEquals("Interfaced JE_ApplicationCode", DeclarationApplicationCodeList.Codes.Interfaced, Factory.New<JobDeclaration>().JE_ApplicationCode);
			}
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothInterfaceDefaulted;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				AssertEquals("BothInterfaceDefaulted JE_ApplicationCode", DeclarationApplicationCodeList.Codes.Interfaced, Factory.New<JobDeclaration>().JE_ApplicationCode);
			}
		}

		public void TestJE_ApplicationCode_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var customsInterface = new LocalCountryCustomsInterface { RecipientID = "RecipientID", SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted };
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				Assert("BothBuiltInDefaulted JE_ApplicationCode_ReadOnly", !declaration.JE_ApplicationCodeInfo.ReadOnly);
				declaration.CustomsEntryHeaders.AddNew();
				Assert("BothBuiltInDefaulted Has Entry JE_ApplicationCode_ReadOnly", declaration.JE_ApplicationCodeInfo.ReadOnly);
			}
			customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Builtin;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
				Assert("Builtin JE_ApplicationCode_ReadOnly", declaration.JE_ApplicationCodeInfo.ReadOnly);
			}
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothInterfaceDefaulted;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				Factory.InvalidateCachedProperties();
				Assert("BothInterfaceDefaulted JE_ApplicationCode_ReadOnly", !declaration.JE_ApplicationCodeInfo.ReadOnly);
				declaration.CustomsEntryHeaders.AddNew();
				Assert("BothInterfaceDefaulted Has Entry JE_ApplicationCode_ReadOnly", declaration.JE_ApplicationCodeInfo.ReadOnly);
			}
			customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
				Assert("Interfaced JE_ApplicationCode_ReadOnly", declaration.JE_ApplicationCodeInfo.ReadOnly);
			}
		}

		public void TestSaveDeclarationAndDeleteUCRNumberWhenIsNotExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = "SEA";
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.UCRNumber = "9CN91330302765207767NTINVGWAB190311";
			Factory.Save();
			var query = new ZQuery(CusEntryNumSchema.CE_ParentID, instruction.PK);
			query.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.UniqueConsignementReference);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Brazil);
			var cusEntryNum1 = Factory.LoadTop1<CusEntryNumber>(query);
			AssertEquals("UCR Number saved", "9CN91330302765207767NTINVGWAB190311", cusEntryNum1.CE_EntryNum);
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			Factory.Save();
			var cusEntryNum2 = Factory.LoadTop1<CusEntryNumber>(query);
			AssertEquals("UCR Number deleted", null, cusEntryNum2);
		}

		public void TestDefaultDeclarantAddressOnJE_DeclarantTypeChanged()
		{
			var declarant = Factory.New<OrgHeader>();
			var orgProxy = Factory.New<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_DeclarantType = ZString.Empty;

			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declaration.JE_DeclarantType = TypeOfOperationExportList.Codes._1001;
			CombineAssertions("Operation Type changed to 1", () =>
			{
				AssertEquals("JE_OA_DeclarantAddress should be cleared", declaration.JE_OA_DeclarantAddress, ZGuid.Empty);
				AssertEquals("JE_OA_DeclarantAddress_ZAddress.OrgPK should be Empty", ZGuid.Empty, declaration.JE_OA_DeclarantAddress_ZAddress.OrgPK);
			});

			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declaration.JE_DeclarantType = TypeOfOperationExportList.Codes._1002;
			CombineAssertions("Operation Type changed to 2", () =>
			{
				AssertEquals("JE_OA_DeclarantAddress should be cleared", declaration.JE_OA_DeclarantAddress, ZGuid.Empty);
				AssertEquals("JE_OA_DeclarantAddress_ZAddress.OrgPK should be Empty", ZGuid.Empty, declaration.JE_OA_DeclarantAddress_ZAddress.OrgPK);
			});

			declaration.Branch.GB_OH_OrgProxy = ZGuid.Empty;
			declaration.JE_DeclarantType = TypeOfOperationExportList.Codes._1003;
			CombineAssertions("Operation Type changed to 3", () =>
			{
				AssertEquals("Branch does not have OrgProxy", ZGuid.Empty, declaration.JE_OA_DeclarantAddress);
				AssertEquals("JE_OA_DeclarantAddress_ZAddress.OrgPK should be Empty", ZGuid.Empty, declaration.JE_OA_DeclarantAddress_ZAddress.OrgPK);
			});

			declaration.Branch.GB_OH_OrgProxy = orgProxy.PK;
			declaration.JE_DeclarantType = TypeOfOperationExportList.Codes._1001;
			declaration.JE_DeclarantType = TypeOfOperationExportList.Codes._1003;
			CombineAssertions("Operation Type changed to 3, JE_OA_DeclarantAddress is empty", () =>
			{
				AssertEquals("JE_OA_DeclarantAddress should default to Branch's OrgProxy", orgProxy.MainAddress.PK, declaration.JE_OA_DeclarantAddress);
				AssertEquals("JE_OA_DeclarantAddress_ZAddress.OrgPK should be set", orgProxy.PK, declaration.JE_OA_DeclarantAddress_ZAddress.OrgPK);
			});

			declaration.JE_DeclarantType = TypeOfOperationExportList.Codes._1001;
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declaration.JE_DeclarantType = TypeOfOperationExportList.Codes._1003;
			CombineAssertions("Operation Type changed to 3, JE_OA_DeclarantAddress is NOT empty", () =>
			{
				AssertEquals("JE_OA_DeclarantAddress should be changed", declarant.MainAddress.PK, declaration.JE_OA_DeclarantAddress);
				AssertEquals("JE_OA_DeclarantAddress_ZAddress.OrgPK should be set", declarant.PK, declaration.JE_OA_DeclarantAddress_ZAddress.OrgPK);
			});

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarantType = TypeOfOperationExportList.Codes._1001;
			AssertEquals("JE_OA_DeclarantAddress should be default when the job is not Export", declarant.MainAddress.PK, declaration.JE_OA_DeclarantAddress);
		}

		public void TestJE_OA_DeclarantAddressReadOnly()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.Addresses.AddNewMainAddress();

			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_DeclarantType = TypeOfOperationExportList.Codes._1001;
			declaration.JE_OA_DeclarantAddress = supplier.PK;
			Assert("JE_OA_DeclarantAddress should NOT be ReadOnly", !declaration.JE_OA_DeclarantAddressInfo.ReadOnly);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			declaration.JE_OA_DeclarantAddress = supplier.PK;
			declaration.JE_DeclarantType = TypeOfOperationExportList.Codes._1001;
			Assert("JE_OA_DeclarantAddress should be ReadOnly", declaration.JE_OA_DeclarantAddressInfo.ReadOnly);

			declaration.JE_OA_DeclarantAddress = supplier.MainAddress.PK;
			declaration.JE_DeclarantType = TypeOfOperationExportList.Codes._1002;
			Assert("JE_OA_DeclarantAddress should NOT be ReadOnly", !declaration.JE_OA_DeclarantAddressInfo.ReadOnly);

			declaration.JE_OA_DeclarantAddress = supplier.MainAddress.PK;
			declaration.JE_DeclarantType = TypeOfOperationExportList.Codes._1003;
			Assert("JE_OA_DeclarantAddress should NOT be ReadOnly", !declaration.JE_OA_DeclarantAddressInfo.ReadOnly);
		}

		public void TestCreateNewDocumentSupporter()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertType(typeof(JobDeclarationDocumentSupporter), dec.DocumentSupporter);
		}

		public void TestContainerModeVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			AssertEquals("Mail", true, declaration.ContainerModeVisible);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("Sea", true, declaration.ContainerModeVisible);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("Air", false, declaration.ContainerModeVisible);
			declaration.JE_TransportMode = TransportTypeList.Codes.River;
			AssertEquals("River", true, declaration.ContainerModeVisible);
			declaration.JE_TransportMode = TransportTypeList.Codes.Lake;
			AssertEquals("Lake", true, declaration.ContainerModeVisible);
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			AssertEquals("When MessageType ImportLicense", false, declaration.ContainerModeVisible);
		}

		public void TestIMessageManageableBizObj()
		{
			var messageTypes = new string[] { BRJobMessageTypeList.Codes.ImportLicense, BRJobMessageTypeList.Codes.Export, BRJobMessageTypeList.Codes.Import };

			foreach (var messageType in messageTypes)
			{
				var declaration = Factory.New<JobDeclaration>();
				Factory.Save();
				SendsMessagesToCustomsShutterUpperer sender = new SendsMessagesToCustomsShutterUpperer(false);
				declaration.MessageInitiator = sender;
				declaration.JE_MessageType = messageType;
				IBackDoorSavingSupportableBizObj bizObj = declaration;
				AssertType<AmendmentWithdrawalReason>(bizObj.GetAmendmentWithdrawalReason());
				AssertType<DeclarationMultiMessageManager>(bizObj.GetMessageManagerForAmendmentDetection());
				Assert(bizObj.SupportBackDoorForSavingWhenAmendmentDetected);
				Assert($"ShouldCheckAmendment for {messageType} MessageStatus not right", !bizObj.IsInAStatusAmendmentSendable);
				AssertEquals(ContinueWithDetection.Yes, bizObj.ProcessBeforeDetectingAmendmentAndContinue());
				declaration.JE_MessageStatus = "AAA";
				declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
				var cusHeader = declaration.CustomsEntryHeaders.AddNew();
				Assert($"ShouldCheckAmendment for {messageType} CustomsStatus not right", !bizObj.IsInAStatusAmendmentSendable);

				declaration.JE_EntryStatus = ZString.Empty;
				cusHeader.EntryNumber = "99999999";
				Assert("ShouldCheckAmendment for import", bizObj.IsInAStatusAmendmentSendable);
				Assert($"ShouldCheckAmendment for {messageType}", bizObj.IsInAStatusAmendmentSendable);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
				bizObj = declaration;
				AssertNull("GetMessageManagerForAmendmentDetection should be null for Import Siscomex", bizObj.GetMessageManagerForAmendmentDetection());
			}
		}

		public void TestResetTaxDetailsDataOnInvoiceLines()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Brazil, "HSN");
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Brazil, hsnTariffType.PK, "03024100", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
			Factory.Save();

			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCMTE, "ATT_2557", Universal.Constants.ProfileQuestion.AnswerDataTypes.List);
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NVE, "AA", Universal.Constants.ProfileQuestion.AnswerDataTypes.List).ZB1_IsMandatory = true;

			var profileType = helper.CreateRefCusProfileType(Constants.Profile.Types.NCMTE, "HSN", Core.Constants.CountryCodes.Brazil);
			var profile1 = helper.CreateRefCusProfile(profileType, "03024100", "ATT1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, new[] { (Constants.Profile.AttributeNames.Modality, "IMP") });
			helper.CreateRefCusProfileQuestion(profileType, "Test 1", "ATT1", "Text 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, note: "Oriatention01",
				attributes: new[] { (Constants.ProfileQuestion.AttributeNames.Target, Constants.ProfileQuestion.AttributeValues.Duimp) });
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1111";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "03024100";
			invoiceLine.JI_CountryOfOrigin = "CA";

			AssertEquals("AttributeCusCodeDataCollection count should be", 1, invoiceLine.Attributes.Count);
			AssertEquals("NVECusCodeDataCollection count should be", 0, invoiceLine.NVECusCodeDataCollection.Count);

			var att1 = invoiceLine.Attributes.GetFirstElementHaving("ATT_2557");
			AssertEquals("CY_DataFieldType", "ATT_2557", att1.CY_Code);
			AssertEquals("Forma Preenchimento = LISTA_ESTATICA", Universal.Constants.ProfileQuestion.AnswerDataTypes.List, att1.TariffProfileQuestion.AnswerDataType);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertEquals("AttributeCusCodeDataCollection count should be", 0, invoiceLine.Attributes.Count);
			AssertEquals("NVECusCodeDataCollection count should be", 1, invoiceLine.NVECusCodeDataCollection.Count);

			invoiceLine.JI_PrimaryPreference = Constants.RatePreferenceType.ExTariff;
			var nveAA = invoiceLine.NVECusCodeDataCollection.GetFirstElementHaving("AA");
			AssertEquals("CY_DataFieldType", "AA", nveAA.CY_Code);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			AssertEquals("AttributeCusCodeDataCollection should be", 1, invoiceLine.Attributes.Count);
			AssertEquals("NVECusCodeDataCollection count should be", 0, invoiceLine.NVECusCodeDataCollection.Count);

			var att2 = invoiceLine.Attributes.GetFirstElementHaving("ATT1");
			AssertEquals("CY_DataFieldType", "ATT1", att2.CY_Code);
			AssertEquals("Forma Preenchimento = LISTA_ESTATICA", Universal.Constants.ProfileQuestion.AnswerDataTypes.Boolean, att2.TariffProfileQuestion.AnswerDataType);

			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "01234567", 50m);
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			invoiceLine.JI_Tariff = "01234567";
			invoiceLine.JI_CountryOfOrigin = "CA";
			AssertDutyIpiPisCofinsRates(Constants.RatePreferenceType.ExTariff, ipiTaxRegimeShouldBeOverriden: true, pisTaxRegimeShouldBeOverriden: true, cofinsTaxRegimeShouldBeOverriden: true);
			AssertDutyIpiPisCofinsRates(Constants.RatePreferenceType.ReducedRate, ipiTaxRegimeShouldBeOverriden: false, pisTaxRegimeShouldBeOverriden: true, cofinsTaxRegimeShouldBeOverriden: true);
			AssertDutyIpiPisCofinsRates(Constants.RatePreferenceType.ReductionMargin, ipiTaxRegimeShouldBeOverriden: true, pisTaxRegimeShouldBeOverriden: false, cofinsTaxRegimeShouldBeOverriden: true);
			AssertDutyIpiPisCofinsRates(Constants.RatePreferenceType.FreeTradeAgreement, ipiTaxRegimeShouldBeOverriden: true, pisTaxRegimeShouldBeOverriden: true, cofinsTaxRegimeShouldBeOverriden: false);
			AssertDutyIpiPisCofinsRates(Constants.RatePreferenceType.FreeTradeAgreement, ipiTaxRegimeShouldBeOverriden: true, pisTaxRegimeShouldBeOverriden: false, cofinsTaxRegimeShouldBeOverriden: false);

			void AssertDutyIpiPisCofinsRates(string expectedPrimaryReference, bool ipiTaxRegimeShouldBeOverriden, bool pisTaxRegimeShouldBeOverriden, bool cofinsTaxRegimeShouldBeOverriden)
			{
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

				var specialDuty = invoiceLine.SpecialCaseTaxes.AddNew();
				specialDuty.TaxGroup = Constants.RateCodes.ImportDuty;
				specialDuty.TaxType = GetTaxtype(expectedPrimaryReference);
				specialDuty.RateOrUnitValue = 9m;

				var specialIPI = invoiceLine.SpecialCaseTaxes.AddNew();
				specialIPI.TaxGroup = Constants.RateCodes.IPI;
				specialIPI.TaxType = ipiTaxRegimeShouldBeOverriden ? SpecialCaseTaxTypeList.Codes.AdValoremRate : SpecialCaseTaxTypeList.Codes.Reduced;
				specialIPI.RateOrUnitValue = 2m;

				var specialPIS = invoiceLine.SpecialCaseTaxes.AddNew();
				specialPIS.TaxGroup = Constants.RateCodes.PIS;
				specialPIS.TaxType = pisTaxRegimeShouldBeOverriden ? SpecialCaseTaxTypeList.Codes.AdValoremRate : SpecialCaseTaxTypeList.Codes.Reduced;
				specialPIS.RateOrUnitValue = 3m;

				var specialCofins = invoiceLine.SpecialCaseTaxes.AddNew();
				specialCofins.TaxGroup = Constants.RateCodes.Cofins;
				specialCofins.TaxType = cofinsTaxRegimeShouldBeOverriden ? SpecialCaseTaxTypeList.Codes.AdValoremRate : SpecialCaseTaxTypeList.Codes.Reduced;
				specialCofins.RateOrUnitValue = 4m;

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
				CombineAssertions(() =>
				{
					AssertEquals("JI_PrimaryPreference", expectedPrimaryReference, invoiceLine.JI_PrimaryPreference);
					AssertEquals("DutyVigentRateValue", expectedPrimaryReference == Constants.RatePreferenceType.ExTariff ? 9m : 50m, invoiceLine.DutyVigentRateValue);
					AssertEquals("FTADutyRateValue", expectedPrimaryReference == Constants.RatePreferenceType.FreeTradeAgreement ? 45.5m : ZDecimal.Zero, invoiceLine.FTADutyRateValue);
					AssertEquals("ReductionDutyRateValue", expectedPrimaryReference == Constants.RatePreferenceType.ReductionMargin ? 45.5m : ZDecimal.Zero, invoiceLine.ReductionDutyRateValue);
					AssertEquals("ReducedDutyRateValue", expectedPrimaryReference == Constants.RatePreferenceType.ReducedRate ? 9m : ZDecimal.Zero, invoiceLine.ReducedDutyRateValue);
					AssertEquals("DutyRateIsOverridden should be Checked", expected: true, invoiceLine.DutyRateIsOverridden);

					AssertEquals("IPITaxRegime", ipiTaxRegimeShouldBeOverriden ? ZString.Empty : IPITaxRegimeList.Codes.Reduction, invoiceLine.IPITaxRegime);
					AssertEquals("IPIVigentRateValue", ipiTaxRegimeShouldBeOverriden ? 2m : ZDecimal.Zero, invoiceLine.IPIVigentRateValue);
					AssertEquals("IPIRateIsOverridden should be Checked", expected: ipiTaxRegimeShouldBeOverriden, invoiceLine.IPIRateIsOverridden);

					AssertEquals("PisCofinsTaxRegime", !pisTaxRegimeShouldBeOverriden && !cofinsTaxRegimeShouldBeOverriden ? TaxRegimeList.Codes.Reduction : ZString.Empty, invoiceLine.PisCofinsTaxRegime);

					AssertEquals("PisVigentRateValue", pisTaxRegimeShouldBeOverriden ? 3m : ZDecimal.Zero, invoiceLine.PisVigentRateValue);
					AssertEquals("PisRateIsOverridden should be Checked", expected: pisTaxRegimeShouldBeOverriden, invoiceLine.PisRateIsOverridden);

					AssertEquals("CofinsVigentRateValue", cofinsTaxRegimeShouldBeOverriden ? 4m : ZDecimal.Zero, invoiceLine.CofinsVigentRateValue);
					AssertEquals("CofinsRateIsOverridden should be Checked", expected: cofinsTaxRegimeShouldBeOverriden, invoiceLine.CofinsRateIsOverridden);

					AssertEquals("SpecialCase should NOT Contain Duty", expected: false,
						invoiceLine.SpecialCaseTaxes.Cast<SpecialCaseTax>().Any(a => a.TaxGroup == Constants.RateCodes.ImportDuty));

					AssertEquals($"SpecialCase should {(ipiTaxRegimeShouldBeOverriden ? "NOT " : "")}Contain IPI", expected: !ipiTaxRegimeShouldBeOverriden,
						invoiceLine.SpecialCaseTaxes.Cast<SpecialCaseTax>().Any(a => a.TaxGroup == Constants.RateCodes.IPI));

					AssertEquals($"SpecialCase should {(pisTaxRegimeShouldBeOverriden ? "NOT " : "")} Contain PIS", expected: !pisTaxRegimeShouldBeOverriden,
						invoiceLine.SpecialCaseTaxes.Cast<SpecialCaseTax>().Any(a => a.TaxGroup == Constants.RateCodes.PIS));

					AssertEquals($"SpecialCase should {(cofinsTaxRegimeShouldBeOverriden ? "NOT " : "")} Contain Cofins", expected: !cofinsTaxRegimeShouldBeOverriden,
						invoiceLine.SpecialCaseTaxes.Cast<SpecialCaseTax>().Any(a => a.TaxGroup == Constants.RateCodes.Cofins));
				});

				invoiceLine.DutyVigentRateValue = ZDecimal.Zero;
				invoiceLine.FTAMarginRateValue = ZDecimal.Zero;
				invoiceLine.ReductionMarginRateValue = ZDecimal.Zero;
				invoiceLine.ReducedDutyRateValue = ZDecimal.Zero;
				invoiceLine.DutyRateIsOverridden = false;
				invoiceLine.IPITaxRegime = ZString.Empty;
				invoiceLine.IPIVigentRateValue = ZDecimal.Zero;
				invoiceLine.IPIRateIsOverridden = false;
				invoiceLine.PisCofinsTaxRegime = ZString.Empty;
				invoiceLine.PisVigentRateValue = ZDecimal.Zero;
				invoiceLine.PisRateIsOverridden = false;
				invoiceLine.CofinsVigentRateValue = ZDecimal.Zero;
				invoiceLine.CofinsRateIsOverridden = false;
				invoiceLine.SpecialCaseTaxes.RemoveAndDeleteAll();
				invoiceLine.Taxes.RemoveAndDeleteAll();

				string GetTaxtype(string taxType)
				{
					return taxType switch
					{
						Constants.RatePreferenceType.FreeTradeAgreement => SpecialCaseTaxTypeList.Codes.TariffAgreement,
						Constants.RatePreferenceType.ReductionMargin => SpecialCaseTaxTypeList.Codes.Reduction,
						Constants.RatePreferenceType.ReducedRate => SpecialCaseTaxTypeList.Codes.Reduced,
						Constants.RatePreferenceType.ExTariff => SpecialCaseTaxTypeList.Codes.AdValoremRate,
						_ => string.Empty,
					};
				}
			}
		}

		public void TestMakeNonPersistentOnBRDeclaration()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			Assert("JobDeclaration.IsPersistent", declaration.IsPersistent);

			declaration.MakeNonPersistent();
			Assert("JobDeclaration.IsPersistent", !declaration.IsPersistent);
		}

		public void TestCountryContext()
		{
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				var holder = (IApportionInvoiceHolder)declaration;
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				AssertEquals("Export", "BREXP", holder.CountryContext);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				AssertEquals("Import", "BRIMP", holder.CountryContext);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
				AssertEquals("Import License", "BRLIC", holder.CountryContext);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Drawback;
				AssertEquals("Drawback", "BR", holder.CountryContext);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
				AssertEquals("LPCO", "BR", holder.CountryContext);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ExWarehouse;
				AssertEquals("ExWarehouse", "BR", holder.CountryContext);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.MiscellaneousCustoms;
				AssertEquals("MiscellaneousCustoms", "BR", holder.CountryContext);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Refund;
				AssertEquals("Refund", "BR", holder.CountryContext);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.WarehousedByExternalAgent;
				AssertEquals("WarehousedByExternalAgent", "BRIMP", holder.CountryContext);
			});
		}

		public void TestCountryVessel()
		{
			RefVessel refVessel = Factory.New<RefVessel>();
			refVessel.RV_Code = "VESSEL";
			refVessel.RV_RN_NKCountryOfReg = "AU";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_VesselName = refVessel.RV_Code;
			AssertEquals("CountryVessel should be", "AU", declaration.VesselCountry);

			declaration.JE_VesselName = ZString.Empty;
			AssertEquals("CountryVessel should be", ZString.Empty, declaration.VesselCountry);
		}

		public void TestJE_MessageTypeChanged()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("JE_MergeBy default to TRF for Export", OrgConstants.MergeInvoiceLines.Tariff, declaration.JE_MergeBy);

			declaration.JE_ContainerMode = "BLK";
			declaration.JE_UCR = "1236545";
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			AssertEquals("JE_MergeBy should be changed to TRF", OrgConstants.MergeInvoiceLines.Tariff, declaration.JE_MergeBy);
			AssertEquals("JE_ContainerMode should NOT be changed to empty", ZString.Empty, declaration.JE_ContainerMode);
			AssertEquals("JE_UCR should NOT be changed to empty", ZString.Empty, declaration.JE_UCR);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertEquals("JE_MergeBy should NOT be changed", OrgConstants.MergeInvoiceLines.Tariff, declaration.JE_MergeBy);

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			AssertEquals("JE_MergeBy should NOT be changed", OrgConstants.MergeInvoiceLines.Classification, declaration.JE_MergeBy);
		}

		public void TestDefaultingMergeBy()
		{
			GlbDepartment.CurrentDepartment.GE_Import = true;
			AssertEquals("JE_MergeBy defaults to TRF for Import", OrgConstants.MergeInvoiceLines.Tariff, Factory.New<JobDeclaration>().JE_MergeBy);

			GlbDepartment.CurrentDepartment.GE_Import = false;
			AssertEquals("JE_MergeBy defaults to TRF for Export", OrgConstants.MergeInvoiceLines.Tariff, Factory.New<JobDeclaration>().JE_MergeBy);
		}

		public void TestIsCargoArrivalDocumentApplicable()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertEquals("IsCargoArrivalDocumentApplicable should be", false, declaration.IsCargoArrivalDocumentApplicable);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertEquals("IsCargoArrivalDocumentApplicable should be", false, declaration.IsCargoArrivalDocumentApplicable);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertEquals("IsCargoArrivalDocumentApplicable should be", false, declaration.IsCargoArrivalDocumentApplicable);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("IsCargoArrivalDocumentApplicable should be", true, declaration.IsCargoArrivalDocumentApplicable);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("IsCargoArrivalDocumentApplicable should be", true, declaration.IsCargoArrivalDocumentApplicable);

			declaration.JE_TransportMode = TransportTypeList.Codes.River;
			AssertEquals("IsCargoArrivalDocumentApplicable should be", true, declaration.IsCargoArrivalDocumentApplicable);

			declaration.JE_TransportMode = TransportTypeList.Codes.Lake;
			AssertEquals("IsCargoArrivalDocumentApplicable should be", true, declaration.IsCargoArrivalDocumentApplicable);

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			AssertEquals("IsCargoArrivalDocumentApplicable should be", true, declaration.IsCargoArrivalDocumentApplicable);

			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			AssertEquals("IsCargoArrivalDocumentApplicable should be", true, declaration.IsCargoArrivalDocumentApplicable);

			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			AssertEquals("IsCargoArrivalDocumentApplicable should be", false, declaration.IsCargoArrivalDocumentApplicable);

			declaration.JE_TransportMode = TransportTypeList.Codes.Fixed;
			AssertEquals("IsCargoArrivalDocumentApplicable should be", false, declaration.IsCargoArrivalDocumentApplicable);

			declaration.JE_TransportMode = TransportTypeList.Codes.Own;
			AssertEquals("IsCargoArrivalDocumentApplicable should be", false, declaration.IsCargoArrivalDocumentApplicable);
		}

		public void TestIsMultimodalAvailable()
		{
			var declaration = Factory.New<JobDeclaration>();

			var transportModesList = declaration.Lookups.TransportTypeList.GetAllCodes();
			var transportModesMultimodalAvailable = new string[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.River, TransportTypeList.Codes.Lake };

			var subTypesList = declaration.Lookups.MessageSubTypeList.GetAllCodes();
			var subTypesMultimodalNotAvailable = new string[] { MessageSubTypeList.Codes._13, MessageSubTypeList.Codes._14, MessageSubTypeList.Codes._15, MessageSubTypeList.Codes._16, MessageSubTypeList.Codes._17, MessageSubTypeList.Codes._18, MessageSubTypeList.Codes._20, MessageSubTypeList.Codes._21 };

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			AssertEquals("IsCargoArrivalDocumentApplicable should be", false, declaration.IsCargoArrivalDocumentApplicable);

			CombineAssertions(() =>
			{
				foreach (var transportMode in transportModesList)
				{
					declaration.JE_TransportMode = transportMode;
					foreach (var subType in subTypesList)
					{
						declaration.JE_MessageSubType = subType;
						if (transportModesMultimodalAvailable.Contains(transportMode) && !subTypesMultimodalNotAvailable.Contains(subType))
						{
							AssertEquals($"IsMultimodalAvailable when JE_TransportMode = {transportMode} and JE_MessageSubType = {subType} should be", true, declaration.IsMultimodalAvailable);
						}
						else
						{
							AssertEquals($"IsMultimodalAvailable when JE_TransportMode = {transportMode} and JE_MessageSubType = {subType} should be", false, declaration.IsMultimodalAvailable);
						}
					}
				}
			});
		}

		public void TestAdminstrativeStatusConcatenated()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_AdministrativeStatus = BRAdministrativeStatusList.Codes.Deferred;

			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_AdministrativeStatus = BRAdministrativeStatusList.Codes.Deferred;

			AssertEquals("AdminstrativeStatus should be", BRAdministrativeStatusList.Codes.Deferred, declaration.AdminstrativeStatus);

			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_AdministrativeStatus = BRAdministrativeStatusList.Codes.InProcess;

			AssertEquals("AdminstrativeStatus should be", CommonMessageStatusList.Codes.MultipleMessageStatus, declaration.AdminstrativeStatus);
		}

		public void TestAdminstrativeStatusDescriptionConcatenated()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_AdministrativeStatus = BRAdministrativeStatusList.Codes.Deferred;

			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_AdministrativeStatus = BRAdministrativeStatusList.Codes.Deferred;

			AssertEquals("AdminstrativeStatusDescription should be", BRAdministrativeStatusList.Descriptions.Deferred, declaration.AdminstrativeStatusDescription);

			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_AdministrativeStatus = BRAdministrativeStatusList.Codes.InProcess;

			AssertEquals("AdminstrativeStatusDescription should be", CommonMessageStatusList.Descriptions.MultipleMessageStatus, declaration.AdminstrativeStatusDescription);
		}

		public void TestCargoStatusConcatenated()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CargoStatus = BRCargoStatusList.Codes.Stored;

			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CargoStatus = BRCargoStatusList.Codes.Stored;

			AssertEquals("CargoStatus should be", BRCargoStatusList.Codes.Stored, declaration.CargoStatus);

			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CargoStatus = BRCargoStatusList.Codes.InTransit;

			AssertEquals("CargoStatus should be", CommonMessageStatusList.Codes.MultipleMessageStatus, declaration.CargoStatus);
		}

		public void TestCargoStatusDescriptionConcatenated()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CargoStatus = BRCargoStatusList.Codes.Stored;

			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CargoStatus = BRCargoStatusList.Codes.Stored;

			AssertEquals("CargoStatusDescription should be", BRCargoStatusList.Descriptions.Stored, declaration.CargoStatusDescription);

			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CargoStatus = BRCargoStatusList.Codes.InTransit;

			AssertEquals("CargoStatusDescription should be", CommonMessageStatusList.Descriptions.MultipleMessageStatus, declaration.CargoStatusDescription);
		}

		public void TestClearanceDateConcatenated()
		{
			var date1 = ZDateTime.Today;
			var date2 = ZDateTime.Today.AddDays(-1);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryReleaseDate = date1;

			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryReleaseDate = date1;

			AssertEquals("ClearanceDate should be", date1.ToString(), declaration.ClearanceDateAsString);

			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryReleaseDate = date2;

			AssertEquals("ClearanceDate should be", CommonMessageStatusList.Descriptions.MultipleMessageStatus, declaration.ClearanceDateAsString);
		}

		public void TestSetDefaultValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(true, declaration.ClearanceOfficeIsCustomsEnclosure);
			AssertEquals(ZString.Empty, declaration.JE_PaymentMethod);
		}

		public void TestCustomsOffices()
		{
			var declaration = Factory.New<JobDeclaration>();
			var office = Factory.New<CustomsOffice>();
			office.CY_ParentTableCode = declaration.TablePrefix;
			office.CY_ParentID = declaration.PK;
			AssertEquals("CustomsOffices", 1, declaration.CustomsOffices.Count);
		}

		public void TestCustomsEnclosures()
		{
			var declaration = Factory.New<JobDeclaration>();
			var office = Factory.New<CustomsEnclosure>();
			office.CY_ParentTableCode = declaration.TablePrefix;
			office.CY_ParentID = declaration.PK;
			AssertEquals("CustomsEnclosures", 1, declaration.CustomsEnclosures.Count);
		}

		public void TestWarehouseAreas()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var area = Factory.New<WarehouseArea>();
			area.CY_ParentTableCode = declaration.TablePrefix;
			area.CY_ParentID = declaration.PK;
			AssertEquals("WarehouseIdCollection", 1, declaration.WarehouseAreas.Count);
		}

		public void TestWarehouseAreasConcatenated()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var warehouseID = declaration.WarehouseAreas.AddNew();
			warehouseID.CY_Code = "00001";

			var warehouseID2 = declaration.WarehouseAreas.AddNew();
			warehouseID2.CY_Code = "00002";

			var warehouseID3 = declaration.WarehouseAreas.AddNew();
			warehouseID3.CY_Code = "00003";

			AssertEquals("WarehouseAreasConcatenated should be '00001,00002,00003' ", "00001,00002,00003", declaration.WarehouseAreasConcatenated);
		}

		public void TestDeleteWarehouseAreasOnSaving()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var warehouseID = declaration.WarehouseAreas.AddNew("000001");
			Factory.Save();
			Assert("WarehouseArea should NOT be deleted when Message Type is Import", !warehouseID.IsDeleted);
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			Factory.Save();
			Assert("WarehouseArea should be deleted when Message Type differs from Import", warehouseID.IsDeleted);
		}

		public void TestJE_CustomsOffice()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertEquals("MaxLength", 7, declaration.JE_CustomsOfficeInfo.MaxLength);
		}

		public void TestJE_LocationOfGoods()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("MaxLength", 7, declaration.JE_LocationOfGoodsInfo.MaxLength);

			declaration.ClearanceOfficeIsCustomsEnclosure = true;
			AssertEquals("ReadOnly", false, declaration.JE_LocationOfGoodsInfo.ReadOnly);
			declaration.ClearanceOfficeIsCustomsEnclosure = false;
			AssertEquals("ReadOnly", true, declaration.JE_LocationOfGoodsInfo.ReadOnly);
		}

		public void TestClearanceOfficeIsCustomsEnclosure()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertEquals("Default to Y", true, declaration.ClearanceOfficeIsCustomsEnclosure);

			declaration.JE_LocationQualifier = "";
			AssertEquals("ClearanceOfficeIsCustomsEnclosure", false, declaration.ClearanceOfficeIsCustomsEnclosure);

			declaration.ClearanceOfficeIsCustomsEnclosure = ZBool.False;
			AssertEquals("ClearanceOfficeIsCustomsEnclosure", false, declaration.ClearanceOfficeIsCustomsEnclosure);
			AssertEquals("JE_LocationQualifier", "N", declaration.JE_LocationQualifier);

			declaration.ClearanceOfficeIsCustomsEnclosure = ZBool.True;
			AssertEquals("ClearanceOfficeIsCustomsEnclosure", true, declaration.ClearanceOfficeIsCustomsEnclosure);
			AssertEquals("JE_LocationQualifier", "YN", declaration.JE_LocationQualifier);

			declaration.JE_LocationQualifier = "NN";
			AssertEquals("ClearanceOfficeIsCustomsEnclosure", false, declaration.ClearanceOfficeIsCustomsEnclosure);

			declaration.ClearanceOfficeIsCustomsEnclosure = ZBool.False;
			AssertEquals("ClearanceOfficeIsCustomsEnclosure", false, declaration.ClearanceOfficeIsCustomsEnclosure);
			AssertEquals("JE_LocationQualifier", "NN", declaration.JE_LocationQualifier);

			declaration.ClearanceOfficeIsCustomsEnclosure = ZBool.True;
			AssertEquals("ClearanceOfficeIsCustomsEnclosure", true, declaration.ClearanceOfficeIsCustomsEnclosure);
			AssertEquals("JE_LocationQualifier", "YN", declaration.JE_LocationQualifier);
		}

		public void TestClearanceOfficeIsHomeDispatch()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertEquals("ClearanceOfficeIsHomeDispatch", false, declaration.ClearanceOfficeIsHomeDispatch);

			declaration.ClearanceOfficeIsHomeDispatch = ZBool.False;
			AssertEquals("ClearanceOfficeIsHomeDispatch", false, declaration.ClearanceOfficeIsHomeDispatch);
			AssertEquals("JE_LocationQualifier", "YN", declaration.JE_LocationQualifier);

			declaration.ClearanceOfficeIsHomeDispatch = ZBool.True;
			AssertEquals("ClearanceOfficeIsHomeDispatch", true, declaration.ClearanceOfficeIsHomeDispatch);
			AssertEquals("JE_LocationQualifier", "YY", declaration.JE_LocationQualifier);

			declaration.JE_LocationQualifier = "NN";
			AssertEquals("ClearanceOfficeIsHomeDispatch", false, declaration.ClearanceOfficeIsHomeDispatch);

			declaration.ClearanceOfficeIsHomeDispatch = ZBool.False;
			AssertEquals("ClearanceOfficeIsHomeDispatch", false, declaration.ClearanceOfficeIsHomeDispatch);
			AssertEquals("JE_LocationQualifier", "NN", declaration.JE_LocationQualifier);

			declaration.ClearanceOfficeIsHomeDispatch = ZBool.True;
			AssertEquals("ClearanceOfficeIsHomeDispatch", true, declaration.ClearanceOfficeIsHomeDispatch);
			AssertEquals("JE_LocationQualifier", "NY", declaration.JE_LocationQualifier);
		}

		public void TestBoardingOfficeCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertEquals("A new CustomsOffice should be added", 1, declaration.CustomsOffices.Count);

			declaration.BoardingOfficeCode = "1234567";
			var boardingOffice = declaration.CustomsOffices.First();
			AssertEquals("CY_Code", Constants.CustomsOfficeCodes.BoardingOffice, boardingOffice.CY_Code);
			AssertEquals("CY_Type", CusCodeDataTypeList.Codes.CustomsOffice, boardingOffice.CY_Type);
			AssertEquals("CY_Data", "1234567", boardingOffice.CY_Data);

			declaration.BoardingOfficeCode = "7654321";
			AssertEquals("CY_Data", "7654321", boardingOffice.CY_Data);
		}

		public void TestBoardingOfficeIsCustomsEnclosure()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;

			AssertEquals("BoardingOfficeIsCustomsEnclosure", true, declaration.BoardingOfficeIsCustomsEnclosure);
			AssertEquals("A new CustomsOffice should be added", 1, declaration.CustomsOffices.Count);

			var boardingOffice = declaration.CustomsOffices.First();
			AssertEquals("CY_Code", Constants.CustomsOfficeCodes.BoardingOffice, boardingOffice.CY_Code);
			AssertEquals("CY_Type", CusCodeDataTypeList.Codes.CustomsOffice, boardingOffice.CY_Type);
			AssertEquals("CY_IsOverridden", true, boardingOffice.CY_IsOverridden);

			declaration.BoardingOfficeIsCustomsEnclosure = false;
			AssertEquals("CY_IsOverridden", false, boardingOffice.CY_IsOverridden);
		}

		public void TestBoardingOfficeIsCustomsEnclosureChange()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;

			declaration.BoardingOfficeIsCustomsEnclosure = ZBool.False;

			var docAddress = declaration.BoardingLocalAddress;
			docAddress.DocAddressType = DocAddressType.BoardingLocalDocumentaryAddress;
			docAddress.E2_AddressOverride = ZBool.True;
			docAddress.E2_CompanyName = "CompanyTest";
			docAddress.E2_Address1 = "Address Test 1";
			docAddress.E2_Address2 = "";

			AssertEquals("BoardingLocalAddressCompanyName", "CompanyTest", declaration.BoardingLocalAddress.E2_CompanyName);
			AssertEquals("BoardingLocalAddress1", "Address Test 1", declaration.BoardingLocalAddress.E2_Address1);
			AssertEquals("BoardingLocalAddress2", ZString.Empty, declaration.BoardingLocalAddress.E2_Address2);

			declaration.BoardingOfficeIsCustomsEnclosure = ZBool.True;

			AssertEquals("BoardingLocalAddressCompanyName", ZString.Empty, declaration.BoardingLocalAddress.E2_CompanyName);
			AssertEquals("BoardingLocalAddress1", ZString.Empty, declaration.BoardingLocalAddress.E2_Address1);
			AssertEquals("BoardingLocalAddress2", ZString.Empty, declaration.BoardingLocalAddress.E2_Address2);
		}

		public void TestClearanceOfficeIsCustomsEnclosureChange()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;

			declaration.ClearanceOfficeIsCustomsEnclosure = ZBool.False;
			declaration.ClearanceOfficeIsHomeDispatch = ZBool.True;

			var docAddress = declaration.ClearanceLocalInvolvedParty;
			docAddress.DocAddressType = DocAddressType.ClearanceLocalInvolvedParty;
			docAddress.E2_AddressOverride = ZBool.True;
			docAddress.E2_Address1 = "Address Test 1";
			docAddress.E2_Address2 = "Address Test 2";
			docAddress.E2_Longitude = 12.5;
			docAddress.E2_Latitude = 10.1;
			docAddress.E2_GovRegNum = "97442770000126";

			AssertEquals("ClearanceOfficeIsHomeDispatch", ZBool.True, declaration.ClearanceOfficeIsHomeDispatch);
			AssertEquals("ClearanceLocalInvolvedPartyAddress1", "Address Test 1", declaration.ClearanceLocalInvolvedParty.E2_Address1);
			AssertEquals("ClearanceLocalInvolvedPartyAddress2", "Address Test 2", declaration.ClearanceLocalInvolvedParty.E2_Address2);
			AssertEquals("ClearanceLocalInvolvedPartyLongitude", 12.5m, declaration.ClearanceLocalInvolvedParty.E2_Longitude);
			AssertEquals("ClearanceLocalInvolvedPartyLatitude", 10.1m, declaration.ClearanceLocalInvolvedParty.E2_Latitude);
			AssertEquals("ClearanceLocalInvolvedPartyGovRegNum", "97442770000126", declaration.ClearanceLocalInvolvedParty.E2_GovRegNum);

			declaration.ClearanceOfficeIsCustomsEnclosure = ZBool.True;

			AssertEquals("ClearanceOfficeIsHomeDispatch", ZBool.False, declaration.ClearanceOfficeIsHomeDispatch);
			AssertEquals("ClearanceLocalInvolvedPartyAddress1", ZString.Empty, declaration.ClearanceLocalInvolvedParty.E2_Address1);
			AssertEquals("ClearanceLocalInvolvedPartyAddress2", ZString.Empty, declaration.ClearanceLocalInvolvedParty.E2_Address2);
			AssertEquals("ClearanceLocalInvolvedPartyLongitude", ZDecimal.Zero, declaration.ClearanceLocalInvolvedParty.E2_Longitude);
			AssertEquals("ClearanceLocalInvolvedPartyLatitude", ZDecimal.Zero, declaration.ClearanceLocalInvolvedParty.E2_Latitude);
			AssertEquals("ClearanceLocalInvolvedPartyGovRegNum", ZString.Empty, declaration.ClearanceLocalInvolvedParty.E2_GovRegNum);
		}

		public void TestBoardingEnclosureCode_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			declaration.BoardingOfficeIsCustomsEnclosure = ZBool.True;
			AssertEquals("BoardingEnclosureCode_ReadOnly", false, declaration.BoardingEnclosureCodeInfo.ReadOnly);
			declaration.BoardingOfficeIsCustomsEnclosure = ZBool.False;
			AssertEquals("BoardingEnclosureCode_ReadOnly", true, declaration.BoardingEnclosureCodeInfo.ReadOnly);
		}

		public void TestBoardingEnclosureCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertEquals("Precondition:", 0, declaration.CustomsEnclosures.Count);

			declaration.BoardingEnclosureCode = "1234567";
			AssertEquals("A new CustomsEnclosure should be added", 1, declaration.CustomsEnclosures.Count);

			var boardingEnclosure = declaration.CustomsEnclosures.First();
			AssertEquals("CY_Code", Constants.CustomsOfficeCodes.BoardingOffice, boardingEnclosure.CY_Code);
			AssertEquals("CY_Type", CusCodeDataTypeList.Codes.CustomsEnclosure, boardingEnclosure.CY_Type);
			AssertEquals("CY_Data", "1234567", boardingEnclosure.CY_Data);

			declaration.BoardingEnclosureCode = "7654321";
			AssertEquals("CY_Data", "7654321", boardingEnclosure.CY_Data);
		}

		public void TestEntranceOfficeCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.CustomsOffices.RemoveAndDeleteAll();

			AssertEquals("Precondition:", 0, declaration.CustomsOffices.Count);

			declaration.EntranceOfficeCode = "1234567";
			AssertEquals("A new Entrance Office should be added", 1, declaration.CustomsOffices.Count);

			var entranceOffice = declaration.CustomsOffices.First();
			AssertEquals("CY_Code", Constants.CustomsOfficeCodes.EntranceOffice, entranceOffice.CY_Code);
			AssertEquals("CY_Type", CusCodeDataTypeList.Codes.CustomsOffice, entranceOffice.CY_Type);
			AssertEquals("CY_Data", "1234567", entranceOffice.CY_Data);

			declaration.EntranceOfficeCode = "7654321";
			AssertEquals("CY_Data", "7654321", entranceOffice.CY_Data);
		}

		public void TestDeleteBoardingOfficeWhenJobIsNotExport()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;

			declaration.BoardingOfficeCode = "1234567";
			declaration.BoardingEnclosureCode = "7654321";
			AssertEquals("A new CustomsOffices should be added", 1, declaration.CustomsOffices.Count);
			AssertEquals("A new CustomsEnclosure should be added", 1, declaration.CustomsEnclosures.Count);

			var boardingOffice = declaration.CustomsOffices.First();
			var boardingEnclosure = declaration.CustomsEnclosures.First();

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			Factory.Save();

			Assert("Boarding Office should be deleted", boardingOffice.IsDeleted);
			Assert("Boarding Enclosure should be deleted", boardingEnclosure.IsDeleted);

			AssertEquals("BoardingOfficeCode should be cleared", "", declaration.BoardingOfficeCode);
			AssertEquals("BoardingEnclosureCode should be cleared", "", declaration.BoardingEnclosureCode);
		}

		public void TestClearanceLocalInvolvedPartyAddress()
		{
			var brazil = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Brazil);
			brazil.RN_PostcodeValidationRule = CountryAddressValidationRuleList.Codes.NoValidationRule;
			brazil.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.NoValidationRule;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;

			var docAddress = declaration.ClearanceLocalInvolvedParty;
			AssertEquals(DocAddressType.ClearanceLocalInvolvedParty, docAddress.DocAddressType);
			docAddress.E2_AddressOverride = true;
			docAddress.E2_CompanyName = "CompanyTest";
			docAddress.E2_Address1 = "Address1 Test";
			docAddress.E2_Address2 = "";
			docAddress.E2_City = "City";
			docAddress.Validation.ValidateAll();
			AssertNoErrors(docAddress.E2_CompanyNameInfo);
			AssertNoErrors(docAddress.E2_Address1Info);
			AssertNoErrors(docAddress.E2_Address2Info);
			AssertNoErrors(docAddress.E2_CityInfo);
			AssertNoErrors(docAddress.E2_PostcodeInfo);
			AssertNoErrors(docAddress.E2_StateInfo);

			docAddress.E2_CompanyName = "";
			docAddress.E2_Address1 = "";
			docAddress.E2_Address2 = "";
			docAddress.E2_City = "";
			docAddress.E2_Postcode = "";
			docAddress.Validation.ValidateAll();
			AssertHasErrors(docAddress.E2_Address1Info);
			AssertNoErrors(docAddress.E2_Address2Info);
			AssertHasErrors(docAddress.E2_CompanyNameInfo);
			AssertHasErrors(docAddress.E2_CityInfo);
			AssertNoErrors(docAddress.E2_PostcodeInfo);
			AssertNoErrors(docAddress.E2_StateInfo);

			docAddress.E2_AddressOverride = false;
			docAddress.Validation.ValidateAll();
			AssertNoErrors(docAddress.E2_Address1Info);
			AssertNoErrors(docAddress.E2_Address2Info);
			AssertNoErrors(docAddress.E2_CompanyNameInfo);
			AssertNoErrors(docAddress.E2_CityInfo);
			AssertNoErrors(docAddress.E2_PostcodeInfo);
			AssertNoErrors(docAddress.E2_StateInfo);

			docAddress.E2_AddressOverride = true;
			docAddress.E2_Address1 = "TestOrg1";
			docAddress.E2_Address2 = "TestOrg2";

			Factory.Save();
			var query = new ZQuery(JobDeclarationSchema.PK, declaration.PK);
			var declarationLoaded = Factory.LoadTop1<JobDeclaration>(query);
			AssertEquals("TestOrg1", declarationLoaded.ClearanceLocalInvolvedParty.E2_Address1);
			AssertEquals("TestOrg2", declarationLoaded.ClearanceLocalInvolvedParty.E2_Address2);
		}

		public void TestSupportedAddressTypes_ClearanceLocalInvolvedParty()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.ClearanceOfficeIsCustomsEnclosure = true;
			var supportedAddressTypes = ((IDocAddresses)declaration).SupportedAddressTypes;
			var expectedDocAddressTypes = base.ExpectedDocAddressTypes;

			AssertEquals("Count of Address Types", expectedDocAddressTypes.Count, supportedAddressTypes.Count);
			declaration.ClearanceOfficeIsCustomsEnclosure = false;

			expectedDocAddressTypes.Add(DocAddressTypes.Codes.ClearanceLocalInvolvedParty, DocAddressType.ClearanceLocalInvolvedParty);
			supportedAddressTypes = ((IDocAddresses)declaration).SupportedAddressTypes;

			AssertEquals("Count of Address Types", expectedDocAddressTypes.Count, supportedAddressTypes.Count);
			foreach (var addressType in expectedDocAddressTypes.Values)
			{
				AssertCollectionContains(addressType, supportedAddressTypes);
			}

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			supportedAddressTypes = ((IDocAddresses)declaration).SupportedAddressTypes;

			AssertCollectionNotContains(DocAddressType.ClearanceLocalInvolvedParty, supportedAddressTypes);
		}

		public void TestSupportedAddressTypes_BoardingLocalDocumentaryAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.BoardingOfficeIsCustomsEnclosure = true;
			var supportedAddressTypes = ((IDocAddresses)declaration).SupportedAddressTypes;
			var expectedDocAddressTypes = base.ExpectedDocAddressTypes;

			AssertEquals("Count of Address Types", expectedDocAddressTypes.Count, supportedAddressTypes.Count);
			declaration.BoardingOfficeIsCustomsEnclosure = false;

			expectedDocAddressTypes.Add(DocAddressTypes.Codes.BoardingLocalDocumentaryAddress, DocAddressType.BoardingLocalDocumentaryAddress);
			supportedAddressTypes = ((IDocAddresses)declaration).SupportedAddressTypes;

			AssertEquals("Count of Address Types", expectedDocAddressTypes.Count, supportedAddressTypes.Count);
			foreach (var addressType in expectedDocAddressTypes.Values)
			{
				AssertCollectionContains(addressType, supportedAddressTypes);
			}

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			supportedAddressTypes = ((IDocAddresses)declaration).SupportedAddressTypes;

			AssertCollectionNotContains(DocAddressType.BoardingLocalDocumentaryAddress, supportedAddressTypes);
		}

		public void TestBoardingLocalAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;

			var docAddress = declaration.BoardingLocalAddress;
			AssertEquals("DocAddressType", DocAddressType.BoardingLocalDocumentaryAddress, docAddress.DocAddressType);
			docAddress.E2_AddressOverride = true;

			docAddress.E2_CompanyName = "CompanyTest";
			docAddress.E2_Address1 = "";
			docAddress.E2_Address2 = "";
			docAddress.Validation.ValidateAll();
			AssertNoErrors(docAddress.E2_CompanyNameInfo);
			AssertHasErrors(docAddress.E2_Address1Info);
			AssertNoErrors(docAddress.E2_Address2Info);
			AssertNoErrors(docAddress.E2_CityInfo);
			AssertNoErrors(docAddress.E2_PostcodeInfo);
			AssertNoErrors(docAddress.E2_StateInfo);

			docAddress.E2_AddressOverride = false;
			docAddress.Validation.ValidateAll();
			AssertNoErrors(docAddress.E2_Address1Info);
			AssertNoErrors(docAddress.E2_Address2Info);
			AssertNoErrors(docAddress.E2_CompanyNameInfo);
			AssertNoErrors(docAddress.E2_CityInfo);
			AssertNoErrors(docAddress.E2_PostcodeInfo);
			AssertNoErrors(docAddress.E2_StateInfo);

			docAddress.E2_AddressOverride = true;
			docAddress.E2_Address1 = "TestOrg1";
			docAddress.E2_Address2 = "TestOrg2";
			docAddress.Validation.ValidateAll();
			AssertNoErrors(docAddress.E2_Address1Info);
			AssertNoErrors(docAddress.E2_Address2Info);
			AssertNoErrors(docAddress.E2_CompanyNameInfo);
			AssertNoErrors(docAddress.E2_CityInfo);
			AssertNoErrors(docAddress.E2_PostcodeInfo);
			AssertNoErrors(docAddress.E2_StateInfo);

			Factory.Save();
			var query = new ZQuery(JobDeclarationSchema.PK, declaration.PK);
			var declarationLoaded = Factory.LoadTop1<JobDeclaration>(query);
			AssertEquals("TestOrg1", declarationLoaded.BoardingLocalAddress.E2_Address1);
		}

		public void TestGetDocAddressRequirement()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;

			var clearanceInvlovedParty = declaration.DocAddresses.AddNew(DocAddressType.ClearanceLocalInvolvedParty);
			AssertType<ClearanceLocalInvolvedPartyRequirement>(clearanceInvlovedParty.Requirement);

			var boardingLocalAddress = declaration.DocAddresses.AddNew(DocAddressType.BoardingLocalDocumentaryAddress);
			AssertType<JobDocAddressRequirementWithLightValidation>(boardingLocalAddress.Requirement);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertType<ClearanceLocalInvolvedPartyRequirement>(declaration.ClearanceLocalInvolvedParty.Requirement);
			AssertType<JobDocAddressRequirementWithLightValidation>(declaration.BoardingLocalAddress.Requirement);
		}

		public void TestIsImportLicense()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			AssertEquals("IsImportLicense", false, dec.IsImportLicense);
			dec.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			AssertEquals("IsImportLicense", true, dec.IsImportLicense);
		}

		public void TestIsImportSiscomex()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			Assert("IsImportSiscomex must be False", !dec.IsImportSiscomex);
			dec.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			Assert("IsImportSiscomex must be True", dec.IsImportSiscomex);
		}

		public void TestIsImportExcludingLicense()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			Assert("IsImport must be True", dec.IsImport);
			Assert("IsImportExcludingLicense must be True", dec.IsImportExcludingLicense);
			dec.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			Assert("IsImport must be True", dec.IsImport);
			Assert("IsImportExcludingLicense must be True", dec.IsImportExcludingLicense);
			dec.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			Assert("IsImport must be True", dec.IsImport);
			Assert("IsImportExcludingLicense must be False", !dec.IsImportExcludingLicense);
			dec.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			Assert("IsImport must be True", !dec.IsImport);
			Assert("IsImportExcludingLicense must be False", !dec.IsImportExcludingLicense);
		}

		public void TestIsImportOnly()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			Assert("IsImportOnly must be True", dec.IsImportOnly);
			dec.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			Assert("IsImportOnly must be False", !dec.IsImportOnly);
			dec.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			Assert("IsImportOnly must be False", !dec.IsImportOnly);
			dec.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			Assert("IsImportOnly must be False", !dec.IsImportOnly);
		}

		public void TestIncoTermAndChargeFactory()
		{
			var declaration = Factory.New<JobDeclaration>();

			CombineAssertions(() =>
			{
				foreach (var messageType in new BRJobMessageTypeList().GetAllCodesZString())
				{
					declaration.JE_MessageType = messageType;
					switch (messageType)
					{
						case BRJobMessageTypeList.Codes.Import:
						case BRJobMessageTypeList.Codes.ImportSiscomex:
						case BRJobMessageTypeList.Codes.WarehousedByExternalAgent:
							AssertType<ImportIncoTermAndCustomsChargeFactory>(messageType, declaration.IncoTermAndChargeFactory);
							break;
						case BRJobMessageTypeList.Codes.ImportLicense:
							AssertType<ImportLicenseIncoTermAndCustomsChargeFactory>(messageType, declaration.IncoTermAndChargeFactory);
							break;
						case BRJobMessageTypeList.Codes.Export:
							AssertType<ExportIncoTermAndCustomsChargeFactory>(messageType, declaration.IncoTermAndChargeFactory);
							break;
						default:
							AssertType<IncoTermAndCustomsChargeFactory>(messageType, declaration.IncoTermAndChargeFactory);
							break;
					}
				}
			});
		}

		public void TestBrokerCertificate()
		{
			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "BR1";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;

			AssertEquals("BrokerCertificate should be", null, declaration.BrokerCertificate);

			declaration.JE_GS_NKCusAgent = broker.GS_Code;
			AssertEquals("BrokerCertificate should be", null, declaration.BrokerCertificate);

			var password = BRGlbStaffWrapper.Get(broker).CCTPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(10);
			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;

			AssertEquals("BrokerCertificate should be", password, declaration.BrokerCertificate);
		}

		public void TestPaymentBankAccount()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();

			var registry = new BRCustomsDataRegistry();
			registry.TaxFeeCustomsPaymentBankAccount.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, bankAccount.PK.ToGuid());

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Default;
			AssertEquals("PaymentBankAccountPK should be", Guid.Empty, declaration.PaymentBankAccountPK);

			declaration.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Broker;
			AssertEquals("PaymentBankAccountPK should be", bankAccount.PK, declaration.PaymentBankAccountPK);
			AssertEquals("declaration.PaymentBankAccount.PK should be", bankAccount.PK, declaration.PaymentBankAccount.PK);

			declaration.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Importer;
			AssertEquals("PaymentBankAccountPK should be", Guid.Empty, declaration.PaymentBankAccountPK);
		}

		public void TestEntrySubmitDateAsStringSameValue()
		{
			var date = ZDateTime.Today;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("TST1");
			entryHeader.PopulateEntrySubmittedDateIfRequired(date);

			AssertEquals("EntrySubmittedDate should be", date.ToString(), declaration.EntrySubmitDateAsString);

			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("TST2");
			entryHeader.PopulateEntrySubmittedDateIfRequired(date);

			AssertEquals("EntrySubmittedDate should be", date.ToString(), declaration.EntrySubmitDateAsString);
		}

		public void TestEntrySubmitDateAsStringMultipleValue()
		{
			var date1 = ZDateTime.Today;
			var date2 = ZDateTime.Today.AddDays(-1);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("TST1");
			entryHeader.PopulateEntrySubmittedDateIfRequired(date1);

			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("TST2");
			entryHeader.PopulateEntrySubmittedDateIfRequired(date1);

			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("TST3");
			entryHeader.PopulateEntrySubmittedDateIfRequired(date2);

			AssertEquals("EntrySubmittedDate should be", CommonMessageStatusList.Descriptions.MultipleMessageStatus, declaration.EntrySubmitDateAsString);
		}

		public void TestEntryIssueDateAsStringSameValue()
		{
			var date = ZDateTime.Today;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("TST1", date);
			AssertEquals("EntryIssueDateAsString should be", date.ToString(), declaration.EntryIssueDateAsString);

			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("TST2", date);

			AssertEquals("EntryIssueDateAsString should be", date.ToString(), declaration.EntryIssueDateAsString);
		}

		public void TestEntryIssueDateAsStringMultipleValue()
		{
			var date1 = ZDateTime.Today;
			var date2 = ZDateTime.Today.AddDays(-1);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("TST1", date1);

			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("TST2", date1);

			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("TST3", date2);

			AssertEquals("EntryIssueDateAsString should be", CommonMessageStatusList.Descriptions.MultipleMessageStatus, declaration.EntryIssueDateAsString);
		}

		public void TestIfPaymentPartyIsSetting()
		{
			var oOrgHeader1 = Factory.New<OrgHeader>();
			oOrgHeader1.OH_Code = "C1";
			var oOrgHeader2 = Factory.New<OrgHeader>();
			oOrgHeader2.OH_Code = "C2";
			var oBRAddinfo = BROrgImpAddInfo.Get(oOrgHeader2);
			oBRAddinfo.ZO_BSBNumber = "111";
			oBRAddinfo.ZO_BankCode = "XXX";
			oBRAddinfo.ZO_AccountNumber = "777";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = oOrgHeader1.PK;
			AssertEquals("Payment method should be equal to Broker because importer do not have bank account", PaymentPartyCodeDescriptionList.Codes.Broker, declaration.JE_PaymentMethod);
			declaration.JE_OH_Importer = oOrgHeader2.PK;
			AssertEquals("Payment method should be equal to Importer because importer do have bank account", PaymentPartyCodeDescriptionList.Codes.Importer, declaration.JE_PaymentMethod);
		}

		public void TestClearCustomsOfficeFields()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			declaration.JE_SubLocationOfGoods = "001";
			declaration.JE_LocationOfGoods = "9999999";
			declaration.JE_CustomsOffice = "XXXX";
			declaration.EntranceOfficeCode = "8655439";

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;

			AssertEquals("JE_SubLocationOfGoods should be", ZString.Empty, declaration.JE_SubLocationOfGoods);
			AssertEquals("JE_LocationOfGoods should be", ZString.Empty, declaration.JE_LocationOfGoods);
			AssertEquals("JE_CustomsOffice should be", ZString.Empty, declaration.JE_CustomsOffice);
			AssertEquals("EntranceOfficeCode should be", ZString.Empty, declaration.EntranceOfficeCode);
		}

		public void TestPossibleImportLicenseDeclarationForAttachment_List()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<AttachJobDeclarationModuleCollection>(declaration.PossibleImportLicenseDeclarationForAttachment_List);
		}

		public void TestDeclarationNumber()
		{
			var date = ZDateTime.Today;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("TST1", date);
			AssertEquals("DeclarationNumber should be", "TST1", declaration.DeclarationNumber);

			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("", date);

			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("TST1", date);
			AssertEquals("DeclarationNumber should be", "TST1", declaration.DeclarationNumber);

			entryHeader.MovementReferenceNumberSetter("TST2", date);
			AssertEquals("DeclarationNumber should be", CommonMessageStatusList.Descriptions.MultipleMessageStatus, declaration.DeclarationNumber);
		}

		public void TestGetTemplateCopyStrategy()
		{
			var declaration = Factory.New<JobDeclarationForTest>();
			var stratety = declaration.GetTemplateCopyStrategyExposed(declaration.Factory, CloneType.TemplateCopy);
			AssertType<JobDeclarationDeepCloneStrategy>(stratety);
		}

		public void TestResetClearanceLocalInvolvedParty()
		{
			var testOrg = Factory.New<OrgHeader>();
			testOrg.OH_FullName = "TestOrg1";
			testOrg.OH_Code = "TS1";
			testOrg.PrimaryRegistrationNumber.Number = "97442770000126";

			var orgAddress = testOrg.MainAddress;
			orgAddress.OA_Longitude = 5.5;
			orgAddress.OA_Latitude = -20.85;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;

			declaration.ClearanceOfficeIsCustomsEnclosure = ZBool.False;
			declaration.ClearanceOfficeIsHomeDispatch = ZBool.True;

			var docAddress = declaration.ClearanceLocalInvolvedParty;
			docAddress.OrganisationPK = testOrg.PK;

			AssertEquals("ClearanceOfficeIsHomeDispatch", ZBool.True, declaration.ClearanceOfficeIsHomeDispatch);
			AssertEquals("ClearanceLocalInvolvedParty.E2_Longitude", 5.5m, declaration.ClearanceLocalInvolvedParty.E2_Longitude);
			AssertEquals("ClearanceLocalInvolvedParty.E2_Latitude", -20.85m, declaration.ClearanceLocalInvolvedParty.E2_Latitude);
			AssertEquals("ClearanceLocalInvolvedParty.E2_GovRegNum", "97442770000126", declaration.ClearanceLocalInvolvedParty.E2_GovRegNum);
			AssertEquals("OrganisationPK", testOrg.PK, declaration.ClearanceLocalInvolvedParty.OrganisationPK);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			AssertEquals("ClearanceOfficeIsHomeDispatch", ZBool.False, declaration.ClearanceOfficeIsHomeDispatch);
			AssertEquals("ClearanceLocalInvolvedParty.E2_Longitude", ZDecimal.Zero, declaration.ClearanceLocalInvolvedParty.E2_Longitude);
			AssertEquals("ClearanceLocalInvolvedParty.E2_Latitude", ZDecimal.Zero, declaration.ClearanceLocalInvolvedParty.E2_Latitude);
			AssertEquals("ClearanceLocalInvolvedParty.E2_GovRegNum", ZString.Empty, declaration.ClearanceLocalInvolvedParty.E2_GovRegNum);
			AssertEquals("OrganisationPK", ZGuid.Empty, declaration.ClearanceLocalInvolvedParty.OrganisationPK);
		}

		public void TestResetBoardingLocalAddress()
		{
			var testOrg = Factory.New<OrgHeader>();
			testOrg.OH_FullName = "TestOrg1";
			testOrg.OH_Code = "TS1";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;

			declaration.BoardingOfficeIsCustomsEnclosure = ZBool.False;

			var docAddress = declaration.BoardingLocalAddress;
			docAddress.DocAddressType = DocAddressType.BoardingLocalDocumentaryAddress;
			docAddress.OrganisationPK = testOrg.PK;

			AssertEquals("BoardingLocalAddress.OrganisationPK", testOrg.PK, docAddress.OrganisationPK);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			AssertEquals("BoardingLocalAddress.E2_AddressOverride", ZBool.False, docAddress.E2_AddressOverride);
			AssertEquals("BoardingLocalAddress.OrganisationPK", ZGuid.Empty, docAddress.OrganisationPK);
		}

		public void TestSetterOfTransportModeClearsContainerMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = "CNT";
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("Setting JE_TransportMode should clear JE_ContainerMode", ZString.Empty, declaration.JE_ContainerMode);
		}

		public void TestUpdateJI_ProcedureOnJE_MessageTypeChanged()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;

			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.DutyTaxRegime = "1";
			invoiceLine.DutyLegalBase = "01";
			AssertEquals("JI_Procedure Should be 011", "01101", invoiceLine.JI_Procedure);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			Assert("JI_Procedure must be empty", invoiceLine.JI_Procedure.IsEmpty);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_MessageSubType = MessageSubTypeList.Codes._02;
			invoiceLine.DutyTaxRegime = "1";
			invoiceLine.DutyLegalBase = "01";
			AssertEquals("JI_Procedure", "02101", invoiceLine.JI_Procedure);
		}

		public void TestUpdateJI_ProcedureOnJE_MessageSubTypeChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_MessageSubType = MessageSubTypeList.Codes._02;
			var invoice1 = declaration.Invoices.AddNew();
			var line1 = invoice1.InvoiceLines.AddNew();
			AddDefaultDataForJE_MessageSubTypeTest(line1);
			var line2 = invoice1.InvoiceLines.AddNew();
			AddDefaultDataForJE_MessageSubTypeTest(line2);

			var invoice2 = declaration.Invoices.AddNew();
			var line3 = invoice2.InvoiceLines.AddNew();
			AddDefaultDataForJE_MessageSubTypeTest(line3);
			var line4 = invoice2.InvoiceLines.AddNew();
			AddDefaultDataForJE_MessageSubTypeTest(line4);

			declaration.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => AssertJI_Procedure(x, "02203"));

			declaration.JE_MessageSubType = MessageSubTypeList.Codes._03;
			declaration.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => AssertJI_Procedure(x, "03203"));

			declaration.JE_MessageSubType = ZString.Empty;
			declaration.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => AssertJI_Procedure(x, "  203"));
		}

		void AssertJI_Procedure(JobComInvoiceLine line, ZString expectedProcedure)
		{
			AssertEquals("JI_Procedure", expectedProcedure, line.JI_Procedure);
		}

		void AddDefaultDataForJE_MessageSubTypeTest(JobComInvoiceLine line)
		{
			line.DutyTaxRegime = "2";
			line.DutyLegalBase = "03";
		}

		public void TestClearSubLocationOfGoods()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			declaration.JE_CustomsOffice = "XXXX";
			declaration.JE_LocationOfGoods = "9999999";
			declaration.JE_SubLocationOfGoods = "001";
			AssertEquals("JE_SubLocationOfGoods must be equal to 001", "001", declaration.JE_SubLocationOfGoods);

			declaration.JE_CustomsOffice = "0000";
			Assert("JE_SubLocationOfGoods must be Empty", declaration.JE_SubLocationOfGoods.IsEmpty);

			declaration.JE_SubLocationOfGoods = "001";
			declaration.JE_LocationOfGoods = "0000000";
			Assert("JE_SubLocationOfGoods must be Empty", declaration.JE_SubLocationOfGoods.IsEmpty);

			declaration.JE_SubLocationOfGoods = "001";
			AssertEquals("JE_SubLocationOfGoods must be equal to 001", "001", declaration.JE_SubLocationOfGoods);
		}

		public void TestNFeExportObjectCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var entryheader1 = declaration.ActiveEntryHeaders.AddNew();
			entryheader1.MovementReferenceNumberSetter("2000010001");
			entryheader1.CH_Status = BRMessageStatusList.Codes.Accepted;

			var entryheader2 = declaration.ActiveEntryHeaders.AddNew();
			entryheader2.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;

			AssertEquals("NFeExportObjectCollection Count should be", 2, declaration.NFeExportObject.Entries.Count);
		}

		public void TestJE_OH_Consignee()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			Assert("JE_OH_Consignee must be readonly", declaration.JE_OH_ConsigneeInfo.ReadOnly);
			Assert("JE_OH_Consignee must be empty", declaration.JE_OH_Consignee.IsEmpty);

			declaration.OperationType = TypeOfOperationImportList.Codes.AccountAndOrder;
			declaration.JE_OH_Consignee = Factory.New<OrgHeader>().PK;
			Assert("JE_OH_Consignee must NOT be readonly", !declaration.JE_OH_ConsigneeInfo.ReadOnly);
			Assert("JE_OH_Consignee must NOT be empty", !declaration.JE_OH_Consignee.IsEmpty);

			declaration.OperationType = TypeOfOperationImportList.Codes.OnItsOwn;
			Assert("JE_OH_Consignee must be readonly", declaration.JE_OH_ConsigneeInfo.ReadOnly);
			Assert("JE_OH_Consignee must be empty", declaration.JE_OH_Consignee.IsEmpty);

			declaration.DeclarantType = DeclarantTypeList.Codes.DoorToDoor;
			declaration.JE_OH_Consignee = Factory.New<OrgHeader>().PK;
			Assert("JE_OH_Consignee must NOT be readonly", !declaration.JE_OH_ConsigneeInfo.ReadOnly);
			Assert("JE_OH_Consignee must NOT be empty", !declaration.JE_OH_Consignee.IsEmpty);

			declaration.DeclarantType = DeclarantTypeList.Codes.NaturalPerson;
			declaration.OperationType = TypeOfOperationImportList.Codes.AccountAndOrder;
			declaration.JE_OH_Consignee = Factory.New<OrgHeader>().PK;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			Assert("JE_OH_Consignee must be empty", declaration.JE_OH_Consignee.IsEmpty);
		}

		public void TestOperationTypeAndDeclarantType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			Assert("JE_DeclarantType must be empty", declaration.JE_DeclarantType.IsEmpty);

			declaration.OperationType = "1";
			AssertEquals("JE_DeclarantType must be", "1", declaration.JE_DeclarantType);

			declaration.DeclarantType = "2";
			AssertEquals("JE_DeclarantType must be", "12", declaration.JE_DeclarantType);

			declaration.OperationType = ZString.Empty;
			declaration.DeclarantType = ZString.Empty;
			Assert("JE_DeclarantType must be empty", declaration.JE_DeclarantType.IsEmpty);

			declaration.DeclarantType = "4";
			AssertEquals("JE_DeclarantType must be", " 4", declaration.JE_DeclarantType);

			declaration.OperationType = "2";
			AssertEquals("JE_DeclarantType must be", "24", declaration.JE_DeclarantType);

			declaration.DeclarantType = "6";
			AssertEquals("JE_DeclarantType must be", " 6", declaration.JE_DeclarantType);

			declaration.DeclarantType = ZString.Empty;
			declaration.OperationType = "2";
			AssertEquals("JE_DeclarantType must be ", "2", declaration.JE_DeclarantType);

			declaration.JE_DeclarantType = " 6";
			AssertEquals("DeclarantType must be ", "6", declaration.DeclarantType);
			AssertEquals("OperationType must be ", "", declaration.OperationType);

			declaration.JE_DeclarantType = "2";
			AssertEquals("DeclarantType must be ", "", declaration.DeclarantType);
			AssertEquals("OperationType must be ", "2", declaration.OperationType);

			declaration.JE_DeclarantType = "31";
			AssertEquals("DeclarantType must be ", "1", declaration.DeclarantType);
			AssertEquals("OperationType must be ", "3", declaration.OperationType);
		}

		public void TestIsOperationTypeApplicable()
		{
			var declaration = Factory.New<JobDeclaration>();
			Assert("IsOperationTypeApplicable must be FALSE", !declaration.IsOperationTypeApplicable);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			Assert("IsOperationTypeApplicable must be TRUE", declaration.IsOperationTypeApplicable);

			declaration.DeclarantType = DeclarantTypeList.Codes.DoorToDoor;
			Assert("IsOperationTypeApplicable must be FALSE", !declaration.IsOperationTypeApplicable);

			declaration.DeclarantType = DeclarantTypeList.Codes.LegalPerson;
			Assert("IsOperationTypeApplicable must be TRUE", declaration.IsOperationTypeApplicable);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			Assert("IsOperationTypeApplicable must be FALSE", !declaration.IsOperationTypeApplicable);

			declaration.JE_MessageType = DeclarantTypeList.Codes.DiplomaticMission;
			Assert("IsOperationTypeApplicable must be FALSE", !declaration.IsOperationTypeApplicable);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			Assert("IsOperationTypeApplicable must be TRUE", declaration.IsOperationTypeApplicable);
		}

		public void TestOceanBillCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertEquals("IsCargoArrivalDocumentApplicable Caption should be", "Ocean Bill", declaration.OceanBillCaption.Caption);
			AssertEquals("IsCargoArrivalDocumentApplicable Full Description should be", "Ocean Bill of the consignment.", declaration.OceanBillCaption.FullDescription);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("IsCargoArrivalDocumentApplicable Caption should be", "Ocean Bill", declaration.OceanBillCaption.Caption);
			AssertEquals("IsCargoArrivalDocumentApplicable Full Description should be", "Ocean Bill of the consignment.", declaration.OceanBillCaption.FullDescription);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("IsCargoArrivalDocumentApplicable Caption should be", "Rail Bill", declaration.OceanBillCaption.Caption);
			AssertEquals("IsCargoArrivalDocumentApplicable Full Description should be", "Rail Bill of the consignment.", declaration.OceanBillCaption.FullDescription);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("IsCargoArrivalDocumentApplicable Caption should be", "Road Bill", declaration.OceanBillCaption.Caption);
			AssertEquals("IsCargoArrivalDocumentApplicable Full Description should be", "Road Bill of the consignment.", declaration.OceanBillCaption.FullDescription);
		}

		public void TestConsigneeCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertConsigneeCaption(BRJobMessageTypeList.Codes.ImportSiscomex);
			AssertConsigneeCaption(BRJobMessageTypeList.Codes.Import);

			void AssertConsigneeCaption(ZString messageType)
			{
				declaration.JE_MessageType = messageType;
				CombineAssertions($"JE_MessageType = {messageType}", () =>
				{
					declaration.DeclarantType = DeclarantTypeList.Codes.DoorToDoor;
					AssertEquals("ConsigneeCaption should be Consignee when DeclarantType = DoorToDoor", "Consignee", declaration.ConsigneeCaption.Caption);

					declaration.DeclarantType = DeclarantTypeList.Codes.LegalPerson;
					AssertEquals("ConsigneeCaption should be Acquirer when DeclarantType = LegalPerson", "Acquirer", declaration.ConsigneeCaption.Caption);

					declaration.DeclarantType = DeclarantTypeList.Codes.NaturalPerson;
					AssertEquals("ConsigneeCaption should be Acquirer when DeclarantType = NaturalPerson", "Acquirer", declaration.ConsigneeCaption.Caption);

					declaration.DeclarantType = DeclarantTypeList.Codes.DiplomaticMission;
					AssertEquals("ConsigneeCaption should be Consignee when DeclarantType = DiplomaticMission", "Acquirer", declaration.ConsigneeCaption.Caption);
				});
			}
		}

		public void TestDispatchModalityCaptionCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertEquals("DispatchModality Caption should be ", "Dispatch Modality", declaration.DispatchModalityCaption.Caption);
			AssertEquals("DispatchModality FullDescription should be ", "Modality Adopted for Customs Clearance.", declaration.DispatchModalityCaption.FullDescription);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			AssertEquals("DispatchModality Caption should be ", "Dispatch Modality", declaration.DispatchModalityCaption.Caption);
			AssertEquals("DispatchModality FullDescription should be ", "The Special Dispatch Modality.", declaration.DispatchModalityCaption.FullDescription);
		}

		public void TestJE_UCRLabel()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("UCRCaption Caption should be", "DSIC", declaration.UCRCaption.Caption);
			AssertEquals("UCRCaption FullDescription should be", "The Cargo Information Subsidiary Document.", declaration.UCRCaption.FullDescription);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("UCRCaption Caption should be", "TIF/DTA", declaration.UCRCaption.Caption);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
			AssertEquals("UCRCaption Caption should be", "Barcode", declaration.UCRCaption.Caption);
			AssertEquals("UCRCaption FullDescription should be", "The Barcode.", declaration.UCRCaption.FullDescription);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("UCRCaption Caption should be", "e-Bill", declaration.UCRCaption.Caption);
			AssertEquals("UCRCaption FullDescription should be", "Electronic Bill number generated by Merchant system.", declaration.UCRCaption.FullDescription);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("UCRCaption Caption should be", "e-Bill", declaration.UCRCaption.Caption);
			AssertEquals("UCRCaption FullDescription should be", "Road Transport Bill Number.", declaration.UCRCaption.FullDescription);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Other;
			AssertEquals("UCRCaption Caption should be", "e-Bill", declaration.UCRCaption.Caption);
		}

		public void TestBillType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			CombineBillType(declaration, "55555", "111111", ZString.Empty, BillTypeList.Codes.HBL);
			CombineBillType(declaration, ZString.Empty, "111111", ZString.Empty, BillTypeList.Codes.HBL);

			declaration.JE_TransportMode = TransportTypeList.Codes.River;
			CombineBillType(declaration, "55555", "111111", ZString.Empty, BillTypeList.Codes.HBL);
			CombineBillType(declaration, ZString.Empty, "111111", ZString.Empty, BillTypeList.Codes.HBL);

			declaration.JE_TransportMode = TransportTypeList.Codes.Lake;
			CombineBillType(declaration, "55555", "111111", ZString.Empty, BillTypeList.Codes.HBL);
			CombineBillType(declaration, ZString.Empty, "111111", ZString.Empty, BillTypeList.Codes.HBL);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			CombineBillType(declaration, "55555", "111111", "22222", BillTypeList.Codes.DSIC);
			CombineBillType(declaration, "55555", "111111", ZString.Empty, BillTypeList.Codes.HAWB);
			CombineBillType(declaration, ZString.Empty, "111111", ZString.Empty, BillTypeList.Codes.HAWB);
			CombineBillType(declaration, "55555", ZString.Empty, ZString.Empty, BillTypeList.Codes.AWB);

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			CombineBillType(declaration, "55555", "111111", ZString.Empty, BillTypeList.Codes.CRT);

			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			CombineBillType(declaration, "55555", "111111", "22222", BillTypeList.Codes.TIFDTA);
			CombineBillType(declaration, "55555", "111111", ZString.Empty, BillTypeList.Codes.HRWB);
			CombineBillType(declaration, ZString.Empty, "111111", ZString.Empty, BillTypeList.Codes.HRWB);
			CombineBillType(declaration, "55555", ZString.Empty, ZString.Empty, BillTypeList.Codes.RWB);

			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			CombineBillType(declaration, "55555", "111111", ZString.Empty, BillTypeList.Codes.Barcode);
		}

		public void TestEntriesNumberConcatenated()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			Assert("EntryNumbersConcatenated should be Empty", declaration.EntryNumbersConcatenated.IsEmpty);

			var entryheader1 = declaration.ActiveEntryHeaders.AddNew();
			var entryheader2 = declaration.ActiveEntryHeaders.AddNew();
			Assert("EntryNumbersConcatenated should be Empty", declaration.EntryNumbersConcatenated.IsEmpty);

			entryheader1.EntryNumber = "0001";
			AssertEquals("EntryNumbersConcatenated should be 0001", "0001", declaration.EntryNumbersConcatenated);

			entryheader2.EntryNumber = "0002";
			AssertEquals("EntryNumbersConcatenated should be 0001;0002", "0001;0002", declaration.EntryNumbersConcatenated);
		}

		public void TestEntriesStatusConcatenated()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			Assert("EntryStatusesConcatenated should be Empty", declaration.EntryStatusesConcatenated.IsEmpty);

			var entryheader1 = declaration.ActiveEntryHeaders.AddNew();
			var entryheader2 = declaration.ActiveEntryHeaders.AddNew();
			Assert("EntryStatusesConcatenated should be Empty", declaration.EntryStatusesConcatenated.IsEmpty);

			entryheader1.CH_EntryStatus = "CUS";
			AssertEquals("EntryStatusesConcatenated should be CUS", "CUS", declaration.EntryStatusesConcatenated);

			entryheader2.CH_EntryStatus = "CUS";
			AssertEquals("EntryStatusesConcatenated should be CUS;CUS", "CUS;CUS", declaration.EntryStatusesConcatenated);
		}

		public void TestEntriesStatusDescriptionConcatenated()
		{
			var countryCode = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");
			helper.CreateCusCodeList(countryCode, RefCusCodeListTypes.Codes.CustomsStatus, "LT1", "LTA1Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(countryCode, RefCusCodeListTypes.Codes.CustomsStatus, "LT2", "LTA2Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			Assert("EntryStatusDescriptionsConcatenated should be Empty", declaration.EntryStatusDescriptionsConcatenated.IsEmpty);

			var entryheader1 = declaration.ActiveEntryHeaders.AddNew();
			var entryheader2 = declaration.ActiveEntryHeaders.AddNew();
			AssertEquals("EntryStatusDescriptionsConcatenated should be", ZString.Empty, declaration.EntryStatusDescriptionsConcatenated);

			entryheader1.CH_EntryStatus = "LT1";
			AssertEquals("EntryStatusDescriptionsConcatenated should be", "LTA1Desc", declaration.EntryStatusDescriptionsConcatenated);

			entryheader2.CH_EntryStatus = "LT2";
			AssertEquals("EntryStatusDescriptionsConcatenated should be", "LTA1Desc;LTA2Desc", declaration.EntryStatusDescriptionsConcatenated);
		}

		void CombineBillType(JobDeclaration dec, ZString master, ZString house, ZString ucr, ZString correctBillType)
		{
			dec.JE_MasterBill = master;
			dec.JE_HouseBill = house;
			dec.JE_UCR = ucr;

			AssertEquals("BillType should be", correctBillType, dec.BillType);
		}

		public void TestEnableAttachCommercialInvoice()
		{
			var declaration = Factory.New<JobDeclaration>();

			foreach (var messageType in new BRJobMessageTypeList().GetAllCodes())
			{
				declaration.JE_MessageType = messageType;
				AssertEquals(!declaration.IsImportLicense, declaration.EnableAttachCommercialInvoice);
			}
		}

		public override void TestEnableCopyCommercialInvoice()
		{
			var declaration = Factory.New<JobDeclaration>();
			foreach (var messageType in new BRJobMessageTypeList().GetAllCodes())
			{
				declaration.JE_MessageType = messageType;
				AssertEquals(!declaration.IsImportLicense, declaration.EnableCopyCommercialInvoice);
			}
		}

		public override void TestEnableCommercialInvoiceMenuItem()
		{
			var declaration = Factory.New<JobDeclaration>();
			foreach (var messageType in new BRJobMessageTypeList().GetAllCodes())
			{
				declaration.JE_MessageType = messageType;
				AssertEquals(!declaration.IsImportLicense, declaration.EnableCommercialInvoiceMenuItem);
			}
		}

		public override void TestMessageTypeForDocumentFilter()
		{
			base.TestMessageTypeForDocumentFilter();

			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			AssertEquals("Import License type", BRJobMessageTypeList.Codes.ImportLicense, declaration.MessageTypeForDocumentFilter);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
			AssertEquals("LPCO type", BRJobMessageTypeList.Codes.LPCO, declaration.MessageTypeForDocumentFilter);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertEquals("ImportSiscomex type", BRJobMessageTypeList.Codes.ImportSiscomex, declaration.MessageTypeForDocumentFilter);
		}

		public void TestUpdateAFRMMOnEntryInstructionAndInvoiceLines_ISW()
		{
			AssertUpdateAFRMMOnEntryInstructionAndInvoiceLines(BRJobMessageTypeList.Codes.ImportSiscomex);
		}

		public void TestUpdateAFRMMOnEntryInstructionAndInvoiceLines_IMP()
		{
			AssertUpdateAFRMMOnEntryInstructionAndInvoiceLines(BRJobMessageTypeList.Codes.Import);
		}

		void AssertUpdateAFRMMOnEntryInstructionAndInvoiceLines(ZString messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction1.PK;
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction2.PK;

			instruction1.CEI_AFRMMMethodOfCalculation = "FMM1";
			invoiceLine1.FMMBenefit = FMMBenefitTypeList.Codes.Exemption;
			CombineAssertions("When JE_TransportMode = SEA", () =>
			{
				AssertEquals("instruction1.CEI_AFRMMMethodOfCalculation should be", "FMM1", instruction1.CEI_AFRMMMethodOfCalculation);
				AssertEquals("invoiceLine1.FMMBenefit should be", FMMBenefitTypeList.Codes.Exemption, invoiceLine1.FMMBenefit);
				AssertEquals("instruction2.CEI_AFRMMMethodOfCalculation should be", ZString.Empty, instruction2.CEI_AFRMMMethodOfCalculation);
				AssertEquals("invoiceLine2.FMMBenefit should be", ZString.Empty, invoiceLine2.FMMBenefit);
			});

			declaration.JE_TransportMode = TransportTypeList.Codes.Lake;
			instruction2.CEI_AFRMMMethodOfCalculation = "FMM1";
			invoiceLine2.FMMBenefit = FMMBenefitTypeList.Codes.Exemption;
			CombineAssertions("When JE_TransportMode = LAK", () =>
			{
				AssertEquals("instruction1.CEI_AFRMMMethodOfCalculation should be", "FMM1", instruction1.CEI_AFRMMMethodOfCalculation);
				AssertEquals("invoiceLine1.FMMBenefit should be", FMMBenefitTypeList.Codes.Exemption, invoiceLine1.FMMBenefit);
				AssertEquals("instruction2.CEI_AFRMMMethodOfCalculation should be", "FMM1", instruction2.CEI_AFRMMMethodOfCalculation);
				AssertEquals("invoiceLine2.FMMBenefit should be", FMMBenefitTypeList.Codes.Exemption, invoiceLine2.FMMBenefit);
			});

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			CombineAssertions("When JE_TransportMode = AIR", () =>
			{
				AssertEquals("instruction1.CEI_AFRMMMethodOfCalculation should be", ZString.Empty, instruction1.CEI_AFRMMMethodOfCalculation);
				AssertEquals("invoiceLine1.FMMBenefit should be", ZString.Empty, invoiceLine1.FMMBenefit);
				AssertEquals("instruction2.CEI_AFRMMMethodOfCalculation should be", ZString.Empty, instruction2.CEI_AFRMMMethodOfCalculation);
				AssertEquals("invoiceLine2.FMMBenefit should be", ZString.Empty, invoiceLine2.FMMBenefit);
			});

			instruction1.CEI_AFRMMMethodOfCalculation = "FMM1";
			invoiceLine1.FMMBenefit = FMMBenefitTypeList.Codes.Exemption;
			instruction2.CEI_AFRMMMethodOfCalculation = "FMM1";
			invoiceLine2.FMMBenefit = FMMBenefitTypeList.Codes.Exemption;
			declaration.JE_TransportMode = TransportTypeList.Codes.River;
			CombineAssertions("When JE_TransportMode = RIV", () =>
			{
				AssertEquals("instruction1.CEI_AFRMMMethodOfCalculation should be", "FMM1", instruction1.CEI_AFRMMMethodOfCalculation);
				AssertEquals("invoiceLine1.FMMBenefit should be", FMMBenefitTypeList.Codes.Exemption, invoiceLine1.FMMBenefit);
				AssertEquals("instruction2.CEI_AFRMMMethodOfCalculation should be", "FMM1", instruction2.CEI_AFRMMMethodOfCalculation);
				AssertEquals("invoiceLine2.FMMBenefit should be", FMMBenefitTypeList.Codes.Exemption, invoiceLine2.FMMBenefit);
			});

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			CombineAssertions("When JE_MessageType = EXP", () =>
			{
				AssertEquals("instruction1.CEI_AFRMMMethodOfCalculation should be", ZString.Empty, instruction1.CEI_AFRMMMethodOfCalculation);
				AssertEquals("instruction1.CEI_AFRMMRateOverride should be", ZDecimal.Zero, instruction1.CEI_AFRMMRateOverride);
				AssertEquals("instruction1.CEI_UtilizationFeeOverride should be", ZDecimal.Zero, instruction1.CEI_UtilizationFeeOverride);
				AssertEquals("invoiceLine1.FMMBenefit should be", ZString.Empty, invoiceLine1.FMMBenefit);
				AssertEquals("instruction2.CEI_AFRMMMethodOfCalculation should be", ZString.Empty, instruction2.CEI_AFRMMMethodOfCalculation);
				AssertEquals("instruction2.CEI_AFRMMRateOverride should be", ZDecimal.Zero, instruction2.CEI_AFRMMRateOverride);
				AssertEquals("instruction2.CEI_UtilizationFeeOverride should be", ZDecimal.Zero, instruction2.CEI_UtilizationFeeOverride);
				AssertEquals("invoiceLine2.FMMBenefit should be", ZString.Empty, invoiceLine2.FMMBenefit);
			});
		}

		public void TestContainersRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals(true, declaration.ContainersRequired);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals(false, declaration.ContainersRequired);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals(false, declaration.ContainersRequired);
		}

		public void TestIsLCSupported()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertEquals("ILandedCostHeader.IsLCSupported", true, ((ILandedCostHeader)declaration).IsLCSupported);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			AssertEquals("ILandedCostHeader.IsLCSupported", false, ((ILandedCostHeader)declaration).IsLCSupported);
		}

		public void TestIsPackingInformationRelevant()
		{
			var declaration = Factory.New<JobDeclarationForTest>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertEquals("IsPackingInformationRelevantCoreExposed", true, declaration.IsPackingInformationRelevant);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			AssertEquals("IsPackingInformationRelevantCoreExposed", false, declaration.IsPackingInformationRelevant);
		}

		public void TestIsNonTransportDeclarationType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertEquals(false, declaration.IsNonTransportDeclarationType);
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			AssertEquals(true, declaration.IsNonTransportDeclarationType);
		}

		public void TestRequiresOrderNumbersOnDocs()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_FullName = "Consignee";
			consignee.MiscServ.OM_IMImporterRequiresOrderNumbersOnDocs = true;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_OH_Importer = consignee.PK;
			AssertEquals("RequiresOrderNumbersOnDocs should be", true, ((IShipmentWithDocsAndCartage)declaration).RequiresOrderNumbersOnDocs());

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			AssertEquals("RequiresOrderNumbersOnDocs should be", false, ((IShipmentWithDocsAndCartage)declaration).RequiresOrderNumbersOnDocs());
		}

		public void TestRequiresOrderTrackLinkCore()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_FullName = "Consignee";
			consignee.MiscServ.OM_IMJobRequireOrderTrackLink = true;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_OH_Importer = consignee.PK;
			AssertEquals("RequiresOrderNumbersOnDocs should be", true, ((IShipmentWithDocsAndCartage)declaration).RequiresOrderTrackLink());
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			AssertEquals("RequiresOrderNumbersOnDocs should be", false, ((IShipmentWithDocsAndCartage)declaration).RequiresOrderTrackLink());
		}

		public void TestRequiresTransportDetails()
		{
			var messageSubTypeList = new[] {
				MessageSubTypeList.Codes._01, MessageSubTypeList.Codes._02, MessageSubTypeList.Codes._03, MessageSubTypeList.Codes._04, MessageSubTypeList.Codes._05,
				MessageSubTypeList.Codes._06, MessageSubTypeList.Codes._07, MessageSubTypeList.Codes._08, MessageSubTypeList.Codes._09, MessageSubTypeList.Codes._10,
				MessageSubTypeList.Codes._12, MessageSubTypeList.Codes._13, MessageSubTypeList.Codes._14, MessageSubTypeList.Codes._15 };

			var declaration = Factory.New<JobDeclarationForTest>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			foreach (var messageSubType in declaration.Lookups.MessageSubTypeList.GetAllCodes())
			{
				declaration.JE_MessageSubType = messageSubType;
				AssertEquals($"JE_MessageSubType={declaration.JE_MessageSubType}", messageSubTypeList.Contains(messageSubType), declaration.RequiresTransportDetails);
			}
		}

		public void TestRequiresDepartureDetails()
		{
			var messageSubTypeList = new[] {
				MessageSubTypeList.Codes._01, MessageSubTypeList.Codes._02, MessageSubTypeList.Codes._03, MessageSubTypeList.Codes._04, MessageSubTypeList.Codes._05,
				MessageSubTypeList.Codes._06, MessageSubTypeList.Codes._07, MessageSubTypeList.Codes._08, MessageSubTypeList.Codes._09, MessageSubTypeList.Codes._10,
				MessageSubTypeList.Codes._12 };

			var declaration = Factory.New<JobDeclarationForTest>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			foreach (var messageSubType in declaration.Lookups.MessageSubTypeList.GetAllCodes())
			{
				declaration.JE_MessageSubType = messageSubType;
				AssertEquals($"JE_MessageSubType={declaration.JE_MessageSubType}", messageSubTypeList.Contains(messageSubType), declaration.RequiresDepartureDetails);
			}
		}

		public void TestRequiresShippingLine()
		{
			var messageSubTypeList = new[] {
				MessageSubTypeList.Codes._01, MessageSubTypeList.Codes._02, MessageSubTypeList.Codes._03, MessageSubTypeList.Codes._04, MessageSubTypeList.Codes._05,
				MessageSubTypeList.Codes._06, MessageSubTypeList.Codes._07, MessageSubTypeList.Codes._08, MessageSubTypeList.Codes._09, MessageSubTypeList.Codes._10,
				MessageSubTypeList.Codes._12, MessageSubTypeList.Codes._13, MessageSubTypeList.Codes._14, MessageSubTypeList.Codes._15 };

			var transportModeList = new[] {
				TransportTypeList.Codes.Sea, TransportTypeList.Codes.River, TransportTypeList.Codes.Lake,
				TransportTypeList.Codes.Air, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Road
			};

			var declaration = Factory.New<JobDeclarationForTest>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			foreach (var messageSubType in declaration.Lookups.MessageSubTypeList.GetAllCodes())
			{
				declaration.JE_MessageSubType = messageSubType;
				foreach (var transportMode in declaration.Lookups.TransportTypeList.GetAllCodes())
				{
					declaration.JE_TransportMode = transportMode;
					AssertEquals($"JE_MessageSubType={declaration.JE_MessageSubType}, JE_TransportMode={declaration.JE_TransportMode}", messageSubTypeList.Contains(messageSubType) && transportModeList.Contains(transportMode), declaration.RequiresShippingLine);
				}
			}
		}

		public void TestJE_MessageType_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			AssertEquals("JE_MessageType_ReadOnly should be false", false, declaration.JE_MessageType_ReadOnly);

			declaration.FixedJobMessageType = BRJobMessageTypeList.Codes.ImportLicense;
			AssertEquals("JE_MessageType_ReadOnly should be true", true, declaration.JE_MessageType_ReadOnly);
		}

		public void TestJE_VesselNameMaxLength()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			AssertEquals("JE_VesselNameInfo.MaxLength should be equal to", JobDeclaration.Schema.JE_VesselNameMaxLength, declaration.JE_VesselNameInfo.MaxLength);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("JE_VesselNameInfo.MaxLength should be equal to", JobDeclaration.Schema.JE_VesselNameMaxLength, declaration.JE_VesselNameInfo.MaxLength);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			AssertEquals("JE_VesselNameInfo.MaxLength should be equal to", 15, declaration.JE_VesselNameInfo.MaxLength);
		}

		public void TestAttachedOrdersVisible()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.JE_JS = ZGuid.Invalid;
			Assert("AttachedOrdersVisible should be TRUE when JE_JS INVALID and NOT LPCO", declaration.AttachedOrdersVisible);

			declaration.JE_JS = ZGuid.NewZGuid();
			Assert("AttachedOrdersVisible should be FALSE when JE_JS VALID and NOT LPCO", !declaration.AttachedOrdersVisible);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
			declaration.JE_JS = ZGuid.Invalid;
			Assert("AttachedOrdersVisible should be FALSE when JE_JS INVALID and LPCO", !declaration.AttachedOrdersVisible);

			declaration.JE_JS = ZGuid.NewZGuid();
			Assert("AttachedOrdersVisible should be FALSE when JE_JS VALID and LPCO", !declaration.AttachedOrdersVisible);
		}

		public void TestIsAFRMMApplicable_ISW()
		{
			AssertIsAFRMMApplicable(BRJobMessageTypeList.Codes.ImportSiscomex);
		}

		public void TestIsAFRMMApplicable_IMP()
		{
			AssertIsAFRMMApplicable(BRJobMessageTypeList.Codes.Import);
		}

		void AssertIsAFRMMApplicable(ZString messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;

			CombineAssertions(() =>
			{
				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				Assert("IsAFRMMApplicable should be TRUE when TransportMode is Sea and MessageType = ISW", declaration.IsAFRMMApplicable);

				declaration.JE_TransportMode = TransportTypeList.Codes.Lake;
				Assert("IsAFRMMApplicable should be TRUE when TransportMode is Lake and MessageType = ISW", declaration.IsAFRMMApplicable);

				declaration.JE_TransportMode = TransportTypeList.Codes.River;
				Assert("IsAFRMMApplicable should be TRUE when TransportMode is River and MessageType = ISW", declaration.IsAFRMMApplicable);

				declaration.JE_TransportMode = TransportTypeList.Codes.Road;
				Assert("IsAFRMMApplicable should be FALSE when TransportMode is Road and MessageType = ISW", !declaration.IsAFRMMApplicable);

				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				Assert("IsAFRMMApplicable should be FALSE when TransportMode is Air and MessageType = ISW", !declaration.IsAFRMMApplicable);

				declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
				Assert("IsAFRMMApplicable should be FALSE when TransportMode is Rail and MessageType = ISW", !declaration.IsAFRMMApplicable);

				declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
				Assert("IsAFRMMApplicable should be FALSE when TransportMode is Mail and MessageType = ISW", !declaration.IsAFRMMApplicable);

				declaration.JE_TransportMode = TransportTypeList.Codes.Own;
				Assert("IsAFRMMApplicable should be FALSE when TransportMode is Own and MessageType = ISW", !declaration.IsAFRMMApplicable);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				Assert("IsAFRMMApplicable should be FALSE when TransportMode is Sea and MessageType = EXP", !declaration.IsAFRMMApplicable);
			});
		}

		public void TestIsBillNumberOnEntryInstructionApplicable()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			Assert("IsBillNumberApplicable should be TRUE when TransportMode is Sea and MessageType = IMP", declaration.IsBillNumberOnEntryInstructionApplicable);

			declaration.JE_TransportMode = TransportTypeList.Codes.Lake;
			Assert("IsBillNumberApplicable should be TRUE when TransportMode is Lake and MessageType = IMP", declaration.IsBillNumberOnEntryInstructionApplicable);

			declaration.JE_TransportMode = TransportTypeList.Codes.River;
			Assert("IsBillNumberApplicable should be TRUE when TransportMode is River and MessageType = IMP", declaration.IsBillNumberOnEntryInstructionApplicable);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			Assert("IsBillNumberApplicable should be TRUE when TransportMode is Air and MessageType = IMP", declaration.IsBillNumberOnEntryInstructionApplicable);

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			Assert("IsBillNumberApplicable should be TRUE when TransportMode is Road and MessageType = IMP", declaration.IsBillNumberOnEntryInstructionApplicable);

			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			Assert("IsBillNumberApplicable should be TRUE when TransportMode is Rail and MessageType = IMP", declaration.IsBillNumberOnEntryInstructionApplicable);

			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			Assert("IsBillNumberApplicable should be FALSE when TransportMode is Mail and MessageType = IMP", !declaration.IsBillNumberOnEntryInstructionApplicable);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			Assert("IsBillNumberApplicable should be FALSE when TransportMode is Air and MessageType = ISW", !declaration.IsBillNumberOnEntryInstructionApplicable);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_DispatchModality = DispatchModalityCodes.Codes.AnticipatedFractionalDelivery;
			Assert("IsBillNumberApplicable should be FALSE when TransportMode is Sea and MessageType = IMP and there is a value on Dispatch Modality", !declaration.IsBillNumberOnEntryInstructionApplicable);
		}

		public void TestAllOverseasFreightChargesHaveTheSameCurrency()
		{
			AssertAllChargesHaveTheSameCurrency(nameof(JobDeclaration.AllOverseasFreightChargesHaveTheSameCurrency), ImportCustomsChargeTypeList.OverseasFreightChargeTypes);
		}

		public void TestAllOverseasInsuranceChargesHaveTheSameCurrency()
		{
			AssertAllChargesHaveTheSameCurrency(nameof(JobDeclaration.AllOverseasInsuranceChargesHaveTheSameCurrency), Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance);
		}

		void AssertAllChargesHaveTheSameCurrency(string allChargesHaveTheSameCurrencyPropertyName, params ZString[] chargeTypes)
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var chargeType = chargeTypes[0];

			declaration.TopGroupInvoice.Charges.AddNew(chargeType);
			AssertEquals(allChargesHaveTheSameCurrencyPropertyName, true, declaration[allChargesHaveTheSameCurrencyPropertyName]);

			declaration.TopGroupInvoice.Charges.AddNew(chargeType, 50m, Core.Constants.CurrencyCodes.UnitedStates);
			AssertEquals(allChargesHaveTheSameCurrencyPropertyName, true, declaration[allChargesHaveTheSameCurrencyPropertyName]);

			var groupCharge = declaration.TopGroupInvoice.Charges.AddNew(chargeType, 50m, Core.Constants.CurrencyCodes.EuropeanUnion);
			AssertEquals(allChargesHaveTheSameCurrencyPropertyName, false, declaration[allChargesHaveTheSameCurrencyPropertyName]);
			groupCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			AssertEquals(allChargesHaveTheSameCurrencyPropertyName, true, declaration[allChargesHaveTheSameCurrencyPropertyName]);

			AssertAllChargesHaveTheSameCurrency(invoice.Charges, chargeType);
			AssertAllChargesHaveTheSameCurrency(invoiceLine.Charges, chargeType);

			foreach (var anotherType in chargeTypes.Skip(1))
			{
				AssertAllChargesHaveTheSameCurrency(invoice.Charges, anotherType);
				AssertAllChargesHaveTheSameCurrency(invoiceLine.Charges, anotherType);
			}

			void AssertAllChargesHaveTheSameCurrency<T>(JobComInvChargeCollection<T> collection, string type) where T : JobComInvCharge
			{
				collection.AddNew(type, 50m, Core.Constants.CurrencyCodes.UnitedStates);
				AssertEquals(allChargesHaveTheSameCurrencyPropertyName, true, declaration[allChargesHaveTheSameCurrencyPropertyName]);
				var charge = collection.AddNew(type, 50m, Core.Constants.CurrencyCodes.EuropeanUnion);
				AssertEquals(allChargesHaveTheSameCurrencyPropertyName, false, declaration[allChargesHaveTheSameCurrencyPropertyName]);
				charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				AssertEquals(allChargesHaveTheSameCurrencyPropertyName, true, declaration[allChargesHaveTheSameCurrencyPropertyName]);
			}
		}

		public void TestRiskChannel()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_RiskChannel = RiskChannelList.Codes.Red;

			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_RiskChannel = RiskChannelList.Codes.Red;

			AssertEquals("RiskChannel should be", RiskChannelList.Codes.Red, declaration.RiskChannel);
			AssertEquals("RiskChannelDescription should be", RiskChannelList.Descriptions.Red, declaration.RiskChannelDescription);

			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_RiskChannel = RiskChannelList.Codes.Green;

			AssertEquals("RiskChannel should be", CommonMessageStatusList.Codes.MultipleMessageStatus, declaration.RiskChannel);
			AssertEquals("RiskChannelDescription should be", CommonMessageStatusList.Descriptions.MultipleMessageStatus, declaration.RiskChannelDescription);
		}

		public void TestICurrencyConverterDataProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var decAsProvider = declaration as ICurrencyConverterDataProvider;
			AssertEquals("JE_MessageType = IMP, Rate Type should be", ZArchitecture.Core.ExchangeRateType.Customs, decAsProvider.RateType);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			AssertEquals("JE_MessageType = LIC, Rate Type should be", ZArchitecture.Core.ExchangeRateType.Customs, decAsProvider.RateType);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertEquals("JE_MessageType = ISW, Rate Type should be", ZArchitecture.Core.ExchangeRateType.Customs, decAsProvider.RateType);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
			AssertEquals("JE_MessageType = LPC, Rate Type should be", ZArchitecture.Core.ExchangeRateType.Customs, decAsProvider.RateType);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertEquals("JE_MessageType = EXP, Rate Type should be", ZArchitecture.Core.ExchangeRateType.CustomsSecondary, decAsProvider.RateType);
		}

		public void TestIsCargoProvenanceAvailable()
		{
			var declaration = Factory.New<JobDeclarationForTest>();

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertEquals("IsCargoProvenanceAvailable should be true", true, declaration.IsCargoProvenanceAvailable);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			AssertEquals("IsCargoProvenanceAvailable should be true", true, declaration.IsCargoProvenanceAvailable);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			AssertEquals("IsCargoProvenanceAvailable should be false", false, declaration.IsCargoProvenanceAvailable);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("IsCargoProvenanceAvailable should be true", true, declaration.IsCargoProvenanceAvailable);

			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			AssertEquals("IsCargoProvenanceAvailable should be true", true, declaration.IsCargoProvenanceAvailable);

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			AssertEquals("IsCargoProvenanceAvailable should be true", true, declaration.IsCargoProvenanceAvailable);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("IsCargoProvenanceAvailable should be false", false, declaration.IsCargoProvenanceAvailable);

			declaration.JE_DispatchModality = DispatchModalityCodes.Codes.Normal;
			AssertEquals("IsCargoProvenanceAvailable should be false", false, declaration.IsCargoProvenanceAvailable);
		}

		public void TestClearJE_UCR()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_UCR = "1233654";
			AssertEquals("JE_UCR should be", "1233654", declaration.JE_UCR);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			Assert("JE_UCR should be Empty", declaration.JE_UCR.IsEmpty);
		}

		public void TestShouldKeepDeletedLinesOnAmendment()
		{
			var declaration = Factory.New<JobDeclarationForTest>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			Assert("ShouldKeepDeletedLinesOnAmendment", declaration.ShouldKeepDeletedLinesOnAmendment);
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			Assert("ShouldKeepDeletedLinesOnAmendment", !declaration.ShouldKeepDeletedLinesOnAmendment);
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			Assert("ShouldKeepDeletedLinesOnAmendment", !declaration.ShouldKeepDeletedLinesOnAmendment);
		}

		public void TestFormalEntryHeaders()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_MessageType = MessageTypeList.Codes.CDI;
			AssertEquals(1, declaration.FormalEntryHeaders.Count);

			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_MessageType = MessageTypeList.Codes.SUF;
			AssertEquals(1, declaration.FormalEntryHeaders.Count);

			entryHeader1.AllEntryLines.AddNew();
			AssertEquals(1, declaration.FormalEntryHeaders.Count);

			var entryHeader3 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader3.CH_MessageType = MessageTypeList.Codes.CDE;
			AssertEquals(2, declaration.FormalEntryHeaders.Count);
		}

		public void TestMultipleKeysToUse()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var multipleKeySupport = (ISupportMultipleResourceStringData)declaration;
			AssertSequencesEqual($"JE_MessageType={declaration.JE_MessageType}", new[] { BRJobMessageTypeList.Codes.Import }, multipleKeySupport.MultipleKeysToUse);
		}

		public void TestRequiresDispatchModality()
		{
			var messageSubTypeList = new[] {
				MessageSubTypeList.Codes._01, MessageSubTypeList.Codes._02, MessageSubTypeList.Codes._03, MessageSubTypeList.Codes._04, MessageSubTypeList.Codes._05,
				MessageSubTypeList.Codes._06, MessageSubTypeList.Codes._07, MessageSubTypeList.Codes._08, MessageSubTypeList.Codes._09, MessageSubTypeList.Codes._10,
				MessageSubTypeList.Codes._11, MessageSubTypeList.Codes._12, MessageSubTypeList.Codes._22, MessageSubTypeList.Codes._23, MessageSubTypeList.Codes._24,
				MessageSubTypeList.Codes._25, MessageSubTypeList.Codes._26, MessageSubTypeList.Codes._27, MessageSubTypeList.Codes._28 };

			var declaration = Factory.New<JobDeclarationForTest>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			foreach (var messageSubType in declaration.Lookups.MessageSubTypeList.GetAllCodes())
			{
				declaration.JE_MessageSubType = messageSubType;
				AssertEquals($"JE_MessageType={declaration.JE_MessageType}, JE_MessageSubType={declaration.JE_MessageSubType}", messageSubTypeList.Contains(messageSubType), declaration.RequiresDispatchModality);
			}

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			foreach (var messageSubType in declaration.Lookups.MessageSubTypeList.GetAllCodes())
			{
				declaration.JE_MessageSubType = messageSubType;
				Assert($"JE_MessageType={declaration.JE_MessageType}, JE_MessageSubType={declaration.JE_MessageSubType}", declaration.RequiresDispatchModality);
			}
		}

		public void TestJE_DispatchModalityOnJE_MessageSubTypeChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.JE_DispatchModality = DispatchModalityCodes.Codes.FractionalDelivery;

			declaration.JE_MessageSubType = MessageSubTypeList.Codes._12;
			Assert("JE_DispatchModality should not be cleared", !declaration.JE_DispatchModality.IsEmpty);

			declaration.JE_MessageSubType = MessageSubTypeList.Codes._13;
			Assert("JE_DispatchModality should be cleared", declaration.JE_DispatchModality.IsEmpty);
		}

		public void TestGetAdditionalReferenceNumberTypeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportModeList.Codes.Airfreight;
			var additionalReferenceNumber = declaration.AdditionalReferenceNumbers.AddNew();
			var types = additionalReferenceNumber.Lookups.AdditionalReferenceNumberTypes;
			AssertList(types, BrazilAdditionalReferenceNumberTypes.Codes.RUC, shouldContain: true);
			AssertList(types, BrazilAdditionalReferenceNumberTypes.Codes.MBL, shouldContain: false);
			AssertSame(types, additionalReferenceNumber.Lookups.AdditionalReferenceNumberTypes);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			types = additionalReferenceNumber.Lookups.AdditionalReferenceNumberTypes;
			AssertList(types, BrazilAdditionalReferenceNumberTypes.Codes.RUC, shouldContain: true);
			AssertList(types, BrazilAdditionalReferenceNumberTypes.Codes.MBL, shouldContain: true);
			AssertSame(types, additionalReferenceNumber.Lookups.AdditionalReferenceNumberTypes);

			declaration.JE_TransportMode = TransportModeList.Codes.Seafreight;
			types = additionalReferenceNumber.Lookups.AdditionalReferenceNumberTypes;
			AssertList(types, BrazilAdditionalReferenceNumberTypes.Codes.RUC, shouldContain: true);
			AssertList(types, BrazilAdditionalReferenceNumberTypes.Codes.MBL, shouldContain: false);
			AssertSame(types, additionalReferenceNumber.Lookups.AdditionalReferenceNumberTypes);

			static void AssertList(CodeDescriptionPairList list, string code, bool shouldContain)
			{
				if (shouldContain)
				{
					Assert(list.ToArray().Any(pair => pair.Code == code));
					return;
				}
				Assert(!list.ToArray().Any(pair => pair.Code == code));
			}
		}

		public void TestBRTransportMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			foreach (var transportMode in new BRTransportModeList().GetAllCodes())
			{
				declaration.BRTransportMode = transportMode;

				var (expectedTransportMode, expectedTransportMeans) = transportModesMapping[transportMode];

				CombineAssertions($"BRTransportMode={transportMode}", () =>
				{
					AssertEquals("BRTransportMode", transportMode, declaration.BRTransportMode);
					AssertEquals("JE_TransportMode", expectedTransportMode, declaration.JE_TransportMode);
					AssertEquals("JE_TransportMeans", expectedTransportMeans, declaration.JE_TransportMeans);
					AssertEquals("BRTransportMode should not have error", false, declaration.BRTransportModeInfo.HasErrors());
					AssertEquals("JE_TransportMode should not have error", false, declaration.JE_TransportModeInfo.HasErrors());
					AssertEquals("JE_TransportMeans should not have error", false, declaration.JE_TransportMeansInfo.HasErrors());
				});
			}

			declaration.BRTransportMode = ZString.Empty;
			AssertEquals("BRTransportMode", ZString.Empty, declaration.BRTransportMode);
			AssertEquals("JE_TransportMode must be", ZString.Empty, declaration.JE_TransportMode);
			AssertEquals("JE_TransportMeans must be", ZString.Empty, declaration.JE_TransportMeans);

			declaration.BRTransportMode = "ZZZ";
			AssertEquals("BRTransportMode", "ZZZ", declaration.BRTransportMode);
			AssertEquals("JE_TransportMode must be", "ZZZ", declaration.JE_TransportMode);
			AssertEquals("JE_TransportMeans must be", ZString.Empty, declaration.JE_TransportMeans);
		}

		readonly Dictionary<string, (string TransportMode, string TransportMeans)> transportModesMapping = new()
		{
			{ BRTransportModeList.Codes.FIC, (TransportTypeGenericList.Codes.Other, BRTransportModeList.TransportMeansList.FIC) },
			{ BRTransportModeList.Codes.OTH, (TransportTypeGenericList.Codes.Other, BRTransportModeList.TransportMeansList.OTH) },
			{ BRTransportModeList.Codes.OWN, (TransportTypeList.Codes.Own, ZString.Empty) },
			{ BRTransportModeList.Codes.AIR, (TransportTypeList.Codes.Air, ZString.Empty) },
			{ BRTransportModeList.Codes.FIX, (TransportTypeList.Codes.Fixed, ZString.Empty) },
			{ BRTransportModeList.Codes.MAI, (TransportTypeList.Codes.Mail, ZString.Empty) },
			{ BRTransportModeList.Codes.LAK, (TransportTypeList.Codes.Lake, ZString.Empty) },
			{ BRTransportModeList.Codes.RAI, (TransportTypeList.Codes.Rail, ZString.Empty) },
			{ BRTransportModeList.Codes.ROA, (TransportTypeList.Codes.Road, ZString.Empty) },
			{ BRTransportModeList.Codes.RIV, (TransportTypeList.Codes.River, ZString.Empty) },
			{ BRTransportModeList.Codes.SEA, (TransportTypeList.Codes.Sea, ZString.Empty) },
		};

		protected override ZString[] TransportModesNotToTestForGenericTranslation => new ZString[] { TransportTypeGenericList.Codes.Other };

		#region Implementation

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			return declaration;
		}

		JobDeclaration GetJobDeclarationForTesting()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.DisableDefaultPackingInformation = true;
			dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			return dec;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var dec = factory.New<JobDeclaration>();
			dec.CustomsEnclosures.AddNew();
			dec.CustomsOffices.AddNew();
			return dec;
		}

		protected override bool ExpectedSupportsJobComInvoiceLineTax => true;

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest) => new LightValidationTesterExcludingJobDocAddress(bizObjToTest);

		#endregion

		class JobDeclarationForTest : JobDeclaration
		{
			public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public Customs.Business.JobDeclarationDeepCloneStrategy GetTemplateCopyStrategyExposed(BusinessObjectFactory alternateFactory, CloneType cloneType) => base.GetTemplateCopyStrategy(alternateFactory, cloneType);
		}

		class LightValidationTesterExcludingJobDocAddress : LightValidationTester
		{
			public LightValidationTesterExcludingJobDocAddress(BusinessObject bo) : base(bo)
			{
			}

			protected override bool ShouldTestProperty(ZPropertyInfo info)
			{
				return info.BizObj is not JobDocAddress && base.ShouldTestProperty(info);
			}
		}
	}
}
