using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Customs.GB.Business.Wizards.CFSP;
using Enterprise.Customs.GB.CDS.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs;
using DeclarationApplicationCodeList = Enterprise.Customs.GB.Registry.Business.DeclarationApplicationCodeList;
using UniversalReferenceConstants = Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(JobDeclaration))]
	public class JobDeclarationTests : EU.Business.Declaration.Testing.JobDeclarationAbstractTest<JobDeclaration>
	{
		public override void TestGetCreditCheckMessage()
		{
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				base.TestGetCreditCheckMessage();

				var importer = Factory.New<OrgHeader>();
				importer.OH_Code = "IMPORTER";
				var declaration = GetNewBusinessObject() as JobDeclaration;
				declaration.JE_OH_Importer = importer.PK;
				Factory.Save();

				var transaction = Factory.New<AccTransactionHeader>();
				transaction.AH_TransactionNum = "ACC001";
				transaction.AH_JobNumber = declaration.JobNumber;
				transaction.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
				transaction.AH_InvoiceTerm = Core.Constants.InvoiceTerms.PaymentInAdvance;
				transaction.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
				transaction.AH_OH = declaration.PK;
				transaction.AH_GB = GlbBranch.CurrentBranch.PK;
				transaction.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference;
				transaction.AH_InvoiceDate = ZDateTime.Now;
				transaction.AH_OH = importer.PK;
				transaction.AH_GB = GlbBranch.CurrentBranch.PK;
				transaction.AH_GE = GlbDepartment.CurrentDepartment.PK;
				Factory.Save();

				using (GBCustomsDataRegistry.Instance.ChiefValidationCreditCheck_AlwaysCheck.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, true))
				using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					AssertContains("Delivery of this message is restricted because:\r\n       The Importer, Supplier, Local Client for Billing or any Debtors in associated Shipment\r\n\t  a) Has unpaid PIA (Payment In Advance) invoices posted on this shipment, OR\r\n\t  b) Has no invoices posted on this shipment and has credit terms PIA (Payment In Advance).", ((IBaseJobDeclaration)declaration).GetCreditCheckMessage());
				}

				using (GBCustomsDataRegistry.Instance.ChiefValidationCreditCheck_AlwaysCheck.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, false))
				using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					AssertContains(ZString.Empty, ((IBaseJobDeclaration)declaration).GetCreditCheckMessage());
				}

				using (GBCustomsDataRegistry.Instance.ChiefValidationCreditCheck_AlwaysCheck.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, true))
				using (CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					AssertContains(ZString.Empty, ((IBaseJobDeclaration)declaration).GetCreditCheckMessage());
				}
			}
		}

		public void TestJE_VoyageFlightNoCaption()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			dec.JE_TransportMode = Core.Constants.TransportModes.Road;
			var info = dec.JE_VoyageFlightNoInfo;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Road", "Registration", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Registration", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			dec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Sea", "Voyage", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Voyage", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			dec.JE_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Rail", "Voyage / Flight No", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Voyage / Flight No", shortCaption: "Voyage/Flight", fullDescription: "A unique reference assigned by a carrier to identify a specific journey of an aircraft or vessel.", dataBoundBusinessObject: new DataBoundBusinessObject(dec));

			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Air - CDS - Import", "[UCC 7/9] Identity of the means of transport at departure - Flight Number", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: JobDeclaration.MultipleKeyCdsImport, dataBoundBusinessObject: new DataBoundBusinessObject(dec),
				shortCaption: "[UCC 7/9] Flight No",
				caption: "[UCC 7/9] Identity of the means of transport at departure - Flight Number",
				fullDescription: "Enter the Flight Number for when the goods are presented and the customs formalities for their release are to be completed.");

			dec.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Air - CDS - Export", "[UCC 7/7] Identity of the means of transport at departure - Flight Number", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: JobDeclaration.MultipleKeyCdsExport,dataBoundBusinessObject: new DataBoundBusinessObject(dec),
				shortCaption: "[UCC 7/7] Flight No",
				caption: "[UCC 7/7] Identity of the means of transport at departure - Flight Number",
				fullDescription: "Enter the Flight Number for which the goods are directly loaded at the time of export.");

			dec.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Air - CDS - MiscellaneousCustoms", "[UCC 7/9] Flight No", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: JobDeclaration.MultipleKeyCdsMisc, "[UCC 7/9] Flight No", dataBoundBusinessObject: new DataBoundBusinessObject(dec));

			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Air - Chief", "Flight", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: JobDeclaration.MultipleKeyChief, "Flight", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
		}

		public void TestValidationOfMucrReuse()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MasterUCR = "Alright me ole Mucker";
			AssertNoWarningContaining(dec1.JE_MasterUCRInfo, "has already been used within your company");
			dec1.Factory.Save();

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MasterUCR = "Alright me ole Mucker";
			AssertHasWarningContaining(dec2.JE_MasterUCRInfo, "has already been used within your company");
			dec2.JE_MasterUCR = "New mucker, innit";
			AssertNoWarningContaining(dec2.JE_MasterUCRInfo, "has already been used within your company");
		}

		public void TestValidationOfMucrReuseWithDate()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MasterUCR = "Run to the hills";
			dec1.Factory.Save();

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MasterUCR = "Run to the hills";
			AssertHasWarningContaining(dec2.JE_MasterUCRInfo, "has already been used within your company");

			ZQuery q = new ZQuery(CusEntryNumSchema.CE_ParentID, dec1.PK);
			CusEntryNumber ce = Factory.LoadTop1<CusEntryNumber>(q);
			AssertNotNull(ce);
			ce.CE_ExpiryDate = ZDateTime.Now.AddDays(-20);
			ce.Factory.Save();
			dec2.JE_MasterUCR = "Run to the hills"; // Jig it to fire the validation again
			AssertNoWarningContaining(dec2.JE_MasterUCRInfo, "has already been used within your company");
		}

		public void TestDunsForBranchOrgProxy()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertEquals(ZString.Empty, dec.DunsForBranchOrgProxy);
			var duns = dec.Branch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, Core.Constants.CountryCodes.UnitedKingdom);
			duns.OK_CustomsRegNo = "123456";
			AssertEquals("123456", dec.DunsForBranchOrgProxy);

			duns.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			AssertEquals("No GB DUNS record", "", dec.DunsForBranchOrgProxy);

			duns.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedKingdom;
			AssertEquals("123456", dec.DunsForBranchOrgProxy);

			dec.Branch.GB_OH_OrgProxy = ZGuid.Empty;
			AssertEquals("No branch org proxy", "", dec.DunsForBranchOrgProxy);
		}

		public void TestValidationOfMucrReuseWithCountry()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MasterUCR = "Shamone";
			dec1.Factory.Save();

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MasterUCR = "Shamone";
			AssertHasWarningContaining(dec2.JE_MasterUCRInfo, "has already been used within your company");

			ZQuery q = new ZQuery(CusEntryNumSchema.CE_ParentID, dec1.PK);
			CusEntryNumber ce = Factory.LoadTop1<CusEntryNumber>(q);
			AssertNotNull(ce);
			ce.CE_RN_NKCountryCode = Core.Constants.CountryCodes.VietNam; // anythign but Australia, under which DAT runs
			ce.Factory.Save();
			dec2.JE_MasterUCR = "Shamone"; // Jig it to fire the validation again
			AssertNoWarningContaining(dec2.JE_MasterUCRInfo, "has already been used within your company");
		}

		public void TestJE_MasterUCR()
		{
			var originalCount = Factory.GetDatabaseCount(typeof(CusEntryNumber), new ZQuery());

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			Factory.Save();
			AssertEquals(originalCount, Factory.GetDatabaseCount(typeof(CusEntryNumber), new ZQuery()));
			declaration.JE_MasterUCR = "HELLO";
			AssertEquals("HELLO", declaration.JE_MasterUCR);
			Factory.Save();
			AssertEquals(originalCount + 1, Factory.GetDatabaseCount(typeof(CusEntryNumber), new ZQuery()));
			declaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals("HELLO", declaration.JE_MasterUCR);
			declaration.JE_MasterUCR = "";
			declaration.Factory.Save();
			AssertEquals("Deleted upon save of blank", originalCount, declaration.Factory.GetDatabaseCount(typeof(CusEntryNumber), new ZQuery()));
			declaration.JE_MasterUCR = "HELLO";
			AssertEquals("Should not explode with some 'accessing property on deleted business object' message", "HELLO", declaration.JE_MasterUCR);

			var anotherDecWithSameMucr = declaration.Factory.New<JobDeclaration>();
			anotherDecWithSameMucr.JE_MasterUCR = declaration.JE_MasterUCR;
			declaration.Factory.Save();
			declaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertNoWarningContaining(declaration.JE_MasterUCRInfo, "Master UCR");
			declaration.Validation.ValidateAll();
			AssertHasWarningContaining(declaration.JE_MasterUCRInfo, "Master UCR"); // File>Validate All shows blue error on this field even without touching the field
		}

		public void TestChangedMucrIsRecordedAsPreviousDocument()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_TransportMode = "AIR";
			dec.JE_MessageType = "EXP";

			dec.JE_MasterUCR = "ONE";
			dec.JE_MasterUCR = "TWO";
			Factory.Save();
			AssertEquals(0, dec.PreviousDocuments.Count);
			dec.JE_MasterUCR = "THREE";
			dec.JE_MasterUCR = "FOUR";
			Factory.Save();
			AssertEquals(1, dec.PreviousDocuments.Count);
			AssertEquals("Last persistent MUCR is saved as PD", "TWO", dec.PreviousDocuments[0].CSI_ReferenceNumber);

			var secondFactory = new BusinessObjectFactory();
			var decReloaded = secondFactory.Load<JobDeclaration>(dec.PK);
			AssertEquals(1, dec.PreviousDocuments.Count);
			decReloaded.JE_MasterUCR = "FIVE";
			decReloaded.JE_MasterUCR = "SIX";
			secondFactory.Save();
			AssertEquals(2, decReloaded.PreviousDocuments.Count);
			AssertEquals("Old PD still there", "TWO", decReloaded.PreviousDocuments[0].CSI_ReferenceNumber);
			AssertEquals("Last persistent MUCR is saved as new PD", "FOUR", decReloaded.PreviousDocuments[1].CSI_ReferenceNumber);
			decReloaded.JE_MasterUCR = "";
			secondFactory.Save();
			AssertEquals(3, decReloaded.PreviousDocuments.Count);
			AssertEquals("Last persistent MUCR is saved as new PD, even when MUCR set to blank", "SIX", decReloaded.PreviousDocuments[2].CSI_ReferenceNumber);

			var decWrongMode = Factory.New<JobDeclaration>();
			decWrongMode.JE_TransportMode = "SEA";
			decWrongMode.JE_MasterUCR = "ONE";
			decWrongMode.JE_MasterUCR = "TWO";
			Factory.Save();
			AssertEquals(0, decWrongMode.PreviousDocuments.Count);
			decWrongMode.JE_MasterUCR = "THREE";
			decWrongMode.JE_MasterUCR = "FOUR";
			Factory.Save();
			AssertEquals(0, decWrongMode.PreviousDocuments.Count);
		}

		public void TestGetNewRelatedDeclarationCore_MasterUCR()
		{
			var source = Factory.New<JobDeclaration>();
			source.JE_MessageType = "EXP";
			source.JE_TransportMode = "AIR";
			source.JE_MasterUCR = "MASTERME";
			var result = (JobDeclaration)source.GetNewRelatedDeclaration(Factory);
			AssertEquals("Mucr is copied during DEEP clone for air exports", "MASTERME", result.JE_MasterUCR);
			source.JE_MessageType = "IMP";
			source.JE_TransportMode = "AIR";
			result = (JobDeclaration)source.GetNewRelatedDeclaration(Factory);
			AssertEquals("Mucr is NOT copied during DEEP clone for imports", "", result.JE_MasterUCR);
			source.JE_MessageType = "EXP";
			source.JE_TransportMode = "SEA";
			result = (JobDeclaration)source.GetNewRelatedDeclaration(Factory);
			AssertEquals("Mucr is NOT copied during DEEP clone for sea exports", "", result.JE_MasterUCR);

			source.JE_MessageType = "EXP";
			source.JE_TransportMode = "AIR";
			result = (JobDeclaration)source.TemplateCopy();
			AssertEquals("Mucr is NOT copied during SHALLOW clone for sea exports", "", result.JE_MasterUCR);
		}

		public void TestIsNorthernIrelandDomestic()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ZG_NorthernIrelandMode = NIModeList.Codes.MovementFromGreatBritainToNi;
			Assert(declaration.IsNorthernIrelandDomestic);

			declaration.ZG_NorthernIrelandMode = NIModeList.Codes.ExportFromNiToRestOfWorld;
			Assert(!declaration.IsNorthernIrelandDomestic);
		}

		public void TestIsNorthernIrelandImportFromRow()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ZG_NorthernIrelandMode = NIModeList.Codes.ImportIntoNiFromRestOfWorld;
			Assert(declaration.IsNorthernIrelandImportFromRow);

			declaration.ZG_NorthernIrelandMode = NIModeList.Codes.ExportFromNiToRestOfWorld;
			Assert(!declaration.IsNorthernIrelandDomestic);
		}

		public void TestIsEuTariffToBeUsedForNorthernIreland()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ZG_NorthernIrelandMode = NIModeList.Codes.MovementFromGreatBritainToNi;
			Assert(declaration.IsEuTariffToBeUsedForNorthernIreland);

			declaration.ZG_NorthernIrelandMode = NIModeList.Codes.ImportIntoNiFromRestOfWorld;
			Assert(!declaration.IsEuTariffToBeUsedForNorthernIreland);

			declaration.ZG_NiGoodsAtRiskOfMovingToROI = true;
			Assert(declaration.IsEuTariffToBeUsedForNorthernIreland);
		}

		public virtual void TestAddFiscalReferenceWhenSettingZG_UsePostPonedVatAccounting()
		{
			var relatedPartyHeader = Factory.NewWithValidTestData<OrgHeader>();
			relatedPartyHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "TST");
			relatedPartyHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "123456789", GlbBranch.CurrentBranch.Country);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var euObj = EUOrgImpAddInfo.Get(orgHeader, Core.Constants.CountryCodes.UnitedKingdom);
			euObj.ZO_UseFr3FiscalRepresentation = true;
			var relatedParty = orgHeader.AllRelatedParties.AddNew();
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.CustomsPostponedVatAccounting;
			relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			relatedParty.PR_OH_RelatedParty = relatedPartyHeader.PK;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
			declaration.JE_OH_Importer = orgHeader.PK;
			declaration.ZG_UsePostponedVatAccounting = true;
			declaration.CustomsEntryInstructions.AddNew();

			// Verify scenario where the flag is the trigger
			AssertEquals(2, declaration.CustomsEntryInstructions.Count);
			foreach (var entryInstruction in declaration.CustomsEntryInstructions)
			{
				AssertEquals(1, entryInstruction.FiscalReferences.Count);
				AssertEquals(FiscalReferenceCodeList.Codes.FR3_TaxRepresentative, entryInstruction.FiscalReferences[0].CFR_Code);
				AssertEquals("GBTST", entryInstruction.FiscalReferences[0].CFR_Reference);
			}

			// Reset and invert triggers
			declaration.ZG_UsePostponedVatAccounting = false;
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.CustomsEntryInstructions.AddNew();
			declaration.ZG_UsePostponedVatAccounting = true;
			declaration.JE_OH_Importer = orgHeader.PK;
			declaration.CustomsEntryInstructions.AddNew();

			// Verify scenario where the importer is the trigger
			AssertEquals(4, declaration.CustomsEntryInstructions.Count);
			foreach (var entryInstruction in declaration.CustomsEntryInstructions)
			{
				AssertEquals(1, entryInstruction.FiscalReferences.Count);
				AssertEquals(FiscalReferenceCodeList.Codes.FR3_TaxRepresentative, entryInstruction.FiscalReferences[0].CFR_Code);
				AssertEquals("GBTST", entryInstruction.FiscalReferences[0].CFR_Reference);
			}
		}

		public virtual void TestAddFiscalReferenceWhenSettingZG_UsePostPonedVatAccountingWhenNoRelatedParty()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var euObj = EUOrgImpAddInfo.Get(importer, Core.Constants.CountryCodes.UnitedKingdom);
			euObj.ZO_UseFr3FiscalRepresentation = true;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
			declaration.JE_OH_Importer = importer.PK;
			declaration.ZG_UsePostponedVatAccounting = true;

			AssertEquals(1, declaration.CustomsEntryInstructions.Count);
			AssertEquals(0, declaration.CustomsEntryInstructions.FirstOrDefault()?.FiscalReferences.Count);
		}

		public void TestLoadCorrectCusContainerTypeAfterGBIsRemovedFormEUN_WI00377628()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CusContainers.AddNew();
			Factory.Save();

			var mock = new Mock<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>();
			mock.Setup(m => m.IsInEuropeanCustomsUnion(It.IsAny<string>()))
				.Returns<string>(arguments => arguments[0].ToString() != CountryCodes.UnitedKingdom);
			mock.Setup(m => m.IsInEuropeanCustomsUnionOrInheritsFromEU(It.IsAny<string>()))
				.Returns((string arg) => arg == CountryCodes.UnitedKingdom);

			using (ObjectFactory.Substitute("Shared.IEuropeanUnionCustomsMembersProvider", mock.Object))
			{
				AssertNoExceptionThrown(() =>
				{
					_ = NewFactory().Load<JobDeclaration>(declaration.PK).CusContainers;
				});
			}
			mock.Verify(m => m.IsInEuropeanCustomsUnion(It.IsAny<string>()), Times.Never);
			mock.Verify(m => m.IsInEuropeanCustomsUnionOrInheritsFromEU(It.IsAny<string>()));
		}

		public void TestJE_LocationOtherInformation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			declaration.JE_Calc_LocationOtherInformationCountry = "";
			AssertEquals("", declaration.JE_LocationOtherInformation);

			declaration.JE_GoodsLocation = "ABDABDABM";
			AssertEquals("      ABDABDABM", declaration.JE_LocationOtherInformation);

			declaration.JE_LocationQualifier = "CW";
			AssertEquals("    CWABDABDABM", declaration.JE_LocationOtherInformation);

			declaration.JE_Calc_LocationOtherInformationCountry = "GB";
			AssertEquals("GB  CWABDABDABM", declaration.JE_LocationOtherInformation);

			declaration.JE_Calc_LocationOtherInformationType = "AU";
			AssertEquals("GBAUCWABDABDABM", declaration.JE_LocationOtherInformation);

			declaration.JE_GoodsLocation = "";
			declaration.JE_LocationQualifier = "";
			declaration.JE_Calc_LocationOtherInformationCountry = "";
			declaration.JE_Calc_LocationOtherInformationType = "";
			declaration.JE_LocationOtherInformation = "";
			AssertEquals("", declaration.JE_GoodsLocation);
			AssertEquals("", declaration.JE_LocationQualifier);
			AssertEquals("", declaration.JE_Calc_LocationOtherInformationCountry);
			AssertEquals("", declaration.JE_Calc_LocationOtherInformationType);

			declaration.JE_LocationOtherInformation = "GBAUCWABDABDABM";
			AssertEquals("ABDABDABM", declaration.JE_GoodsLocation);
			AssertEquals("", declaration.JE_LocationQualifier);
			AssertEquals("GB", declaration.JE_Calc_LocationOtherInformationCountry);
			AssertEquals("AU", declaration.JE_Calc_LocationOtherInformationType);

			declaration.JE_LocationOtherInformation = "GB    BELBELVLQ1";
			AssertEquals("", declaration.JE_Calc_LocationOtherInformationType);

			declaration.JE_LocationOtherInformation = "  AUBELBELVLQ1";
			AssertEquals("", declaration.JE_Calc_LocationOtherInformationCountry);
		}

		public void TestZG_ImportClearanceStatusICSList()
		{
			AssertEquals("AddInfoLookups.ImportClearanceStatusICSList", Factory.New<JobDeclaration>().ZG_ImportClearanceStatusICSInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
		}

		public void TestZG_VATDeferNumberMaxLength()
		{
			AssertEquals(7, Factory.New<JobDeclaration>().ZG_VATDeferNumberInfo.MaxLength);
		}

		public void TestEntryStyle_GetEntrySubStyleForCommonTransit_GB()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);

			//A country eligible to a common transit procedure
			var euctp = helper.CreateTradeGroup("EUN", Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.EUCommonTransitProcedure, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(euctp, "NO", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.AddCountry(euctp, "IS", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.AddCountry(euctp, "CH", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.AddCountry(euctp, "LI", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.AddCountry(euctp, "TR", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			//EFTA: NO, IS, CH, LI
			var efta = helper.CreateTradeGroup("EUN", UniversalReferenceConstants.RefCusTradeGroups.Groups.EFTA, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(efta, "CH", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.AddCountry(efta, "IS", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.AddCountry(efta, "LI", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.AddCountry(efta, "NO", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			var orgHeaderNO = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderNO.OH_RL_NKClosestPort = "NO";

			var orgHeaderIS = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderIS.OH_RL_NKClosestPort = "IS";

			var orgHeaderCH = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderCH.OH_RL_NKClosestPort = "CH";

			var orgHeaderLI = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderLI.OH_RL_NKClosestPort = "LI";

			var orgHeaderTR = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderTR.OH_RL_NKClosestPort = "TR";

			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = MessageTypeList.Codes.Import;

			dec.SupplierDocumentaryAddress.OrganisationPK = orgHeaderNO.PK;
			AssertEquals("EU", dec.JE_EntryStyle);

			dec.SupplierDocumentaryAddress.OrganisationPK = orgHeaderIS.PK;
			AssertEquals("EU", dec.JE_EntryStyle);

			dec.SupplierDocumentaryAddress.OrganisationPK = orgHeaderCH.PK;
			AssertEquals("EU", dec.JE_EntryStyle);

			dec.SupplierDocumentaryAddress.OrganisationPK = orgHeaderLI.PK;
			AssertEquals("EU", dec.JE_EntryStyle);

			dec.SupplierDocumentaryAddress.OrganisationPK = orgHeaderTR.PK;
			AssertEquals("IM", dec.JE_EntryStyle);

			dec.JE_MessageType = MessageTypeList.Codes.Export;
			dec.ImporterDocumentaryAddress.OrganisationPK = orgHeaderNO.PK;
			AssertEquals("EU", dec.JE_EntryStyle);

			dec.ImporterDocumentaryAddress.OrganisationPK = orgHeaderIS.PK;
			AssertEquals("EU", dec.JE_EntryStyle);

			dec.ImporterDocumentaryAddress.OrganisationPK = orgHeaderCH.PK;
			AssertEquals("EU", dec.JE_EntryStyle);

			dec.ImporterDocumentaryAddress.OrganisationPK = orgHeaderLI.PK;
			AssertEquals("EU", dec.JE_EntryStyle);

			dec.ImporterDocumentaryAddress.OrganisationPK = orgHeaderTR.PK;
			AssertEquals("EX", dec.JE_EntryStyle);
		}

		public void TestIEntryStyleCalculatorFallbackInfoProviderOverrideMembers()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var efta = helper.CreateTradeGroup("EUN", UniversalReferenceConstants.RefCusTradeGroups.Groups.EFTA, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(efta, CountryCodes.Norway, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			var declaration = Factory.New<JobDeclaration>();
			var provider = (IEntryStyleCalculatorFallbackInfoProvider)declaration;
			AssertEquals("For EFTA countries, GetEntrySubStyleForCommonTransit", EntryStyleListImport.Codes.ImportFromEFTAMember, provider.GetEntrySubStyleForCommonTransit(RefCountry.LoadFromCountryCode(Factory, CountryCodes.Norway)));

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("For Import declarations, GetEntrySubStyleForCommonTransit", EntryStyleListImport.Codes.ImportNormal, provider.GetEntrySubStyleForCommonTransit(RefCountry.LoadFromCountryCode(Factory, CountryCodes.Latvia)));

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("For Export declarations, GetEntrySubStyleForCommonTransit", EntryStyleListExport.Codes.ExportNormal, provider.GetEntrySubStyleForCommonTransit(RefCountry.LoadFromCountryCode(Factory, CountryCodes.Latvia)));
		}

		public override void TestDefaultValuesForExport()
		{
			GlbDepartment.CurrentDepartment.GE_Import = false;
			GlbDepartment.CurrentDepartment.GE_Export = true;

			var declaration = Factory.New<JobDeclaration>();

			AssertEquals("DIR", declaration.JE_DeclarantType);
			AssertEquals("X", declaration.ZG_CTStatusID);
		}

		public override void TestDefaultCTStatusIDSwitchingBetweenExportAndImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("X", declaration.ZG_CTStatusID);
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("", declaration.ZG_CTStatusID);
		}

		public void TestShouldMrnBeHiddenFromShipmentEntryNumbersListViaGetValidCusEntryNumFilter()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entry = (CusEntryHeader)dec.ActiveEntryHeaders.AddNew();
			entry.EntryNumber = "66666667";
			entry.MovementReferenceNumberSetter("12341234", ZDateTime.UtcNow);

			var lrn = CusEntryNumber.New(entry, CusEntryNumberTypes.Standard.LocalReferenceNumber, dec.CountryCode);
			lrn.CE_EntryNum = "LRN";

			var entryNumbers = new CusEntryNumCollection(Factory);
			entryNumbers.Load(((ICusEntryNumFilterProvider)dec).ValidCusEntryNumFilter);
			AssertArrayEqualsByElements(new string[]
			{
				"12341234"
			}, entryNumbers.Cast<CusEntryNumber>().OrderBy(e => e.CE_EntryNum).Select(e => (string)e.CE_EntryNum).ToArray());
		}

		public void TestIValidateForCustomsMessagingSupporter_SupportValidateCustomsMessaging()
		{
			var dec = Factory.New<JobDeclaration>();
			Assert(((IValidateForCustomsMessagingSupporter)dec).SupportValidateCustomsMessaging);
		}

		public void TestZG_MethodOfPayment_Defaulting()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			dec.JE_MessageType = "IMP";
			dec.JE_PaymentMethod = "A";
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
			AssertEquals(ZString.Empty, invoiceLine.ZG_MethodOfPayment);

			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			AssertEquals("E", invoiceLine.ZG_MethodOfPayment);
		}

		public void TestJE_EntryStatusDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(ZString.Empty, declaration.JE_EntryStatusDescription);

			declaration.JE_ApplicationCode = "CHF";
			var header0 = declaration.ActiveEntryHeaders.AddNew();
			header0.CH_EntryStatus = "CLR";
			AssertEquals("Clear", declaration.JE_EntryStatusDescription);
			declaration.JE_ApplicationCode = "CDS";
			AssertEquals("Clear", declaration.JE_EntryStatusDescription);

			declaration.JE_ApplicationCode = "CHF";
			header0.CH_EntryStatus = "XXX";
			AssertEquals("XXX", declaration.JE_EntryStatusDescription);
			declaration.JE_ApplicationCode = "CDS";
			AssertEquals("XXX", declaration.JE_EntryStatusDescription);

			declaration.JE_ApplicationCode = "CHF";
			var header1 = declaration.ActiveEntryHeaders.AddNew();
			header1.CH_EntryStatus = "2";
			AssertEquals("Multiple Statuses exists - See Entries Tab", declaration.JE_EntryStatusDescription);
			declaration.JE_ApplicationCode = "CDS";
			AssertEquals("Multiple Statuses exists - See Entries Tab", declaration.JE_EntryStatusDescription);

			declaration.JE_ApplicationCode = "CHF";
			header1.CH_EntryStatus = "XXX";
			AssertEquals("XXX", declaration.JE_EntryStatusDescription);
			declaration.JE_ApplicationCode = "CDS";
			AssertEquals("XXX", declaration.JE_EntryStatusDescription);

			declaration.JE_ApplicationCode = "CHF";
			header1.CH_EntryStatus = ZString.Empty;
			AssertEquals("XXX", declaration.JE_EntryStatusDescription);
			declaration.JE_ApplicationCode = "CDS";
			AssertEquals("XXX", declaration.JE_EntryStatusDescription);

			declaration.JE_ApplicationCode = "CHF";
			header0.CH_EntryStatus = ZString.Empty;
			AssertEquals(ZString.Empty, declaration.JE_EntryStatusDescription);
			declaration.JE_ApplicationCode = "CDS";
			AssertEquals(ZString.Empty, declaration.JE_EntryStatusDescription);

			declaration.JE_ApplicationCode = "CHF";
			header0.CH_EntryStatus = "ACC";
			AssertEquals("ACC", declaration.JE_EntryStatusDescription);
			declaration.JE_ApplicationCode = "CDS";
			AssertEquals("Declaration has been legally accepted", declaration.JE_EntryStatusDescription);

			declaration.JE_ApplicationCode = "CHF";
			header1.CH_EntryStatus = "RCV";
			AssertEquals("Multiple Statuses exists - See Entries Tab", declaration.JE_EntryStatusDescription);
			declaration.JE_ApplicationCode = "CDS";
			AssertEquals("Multiple Statuses exists - See Entries Tab", declaration.JE_EntryStatusDescription);

			declaration.JE_ApplicationCode = "CHF";
			header1.CH_EntryStatus = ZString.Empty;
			AssertEquals("ACC", declaration.JE_EntryStatusDescription);
			declaration.JE_ApplicationCode = "CDS";
			AssertEquals("Declaration has been legally accepted", declaration.JE_EntryStatusDescription);

			declaration.JE_ApplicationCode = "CHF";
			header0.CH_EntryStatus = "RCV";
			AssertEquals("RCV", declaration.JE_EntryStatusDescription);
			declaration.JE_ApplicationCode = "CDS";
			AssertEquals("Message has been registered", declaration.JE_EntryStatusDescription);
		}

		public override void TestMergeManagerType()
		{
			var jobDec = Factory.New<JobDeclaration>();
			AssertEquals("Should be a Customs.GB.CDS.Declaration.CDSMergeManager", ExpectedMergeManagerType, jobDec.MergeManager.GetType());
		}

		public void TestModeOfTransportAtTheBorderGB()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = "";
			AssertEquals("declaration.ModeOfTransport", "", declaration.ModeOfTransportAtTheBorder);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("declaration.ModeOfTransport", "4", declaration.ModeOfTransportAtTheBorder);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("declaration.ModeOfTransport", "1", declaration.ModeOfTransportAtTheBorder);
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			AssertEquals("declaration.ModeOfTransport", "3", declaration.ModeOfTransportAtTheBorder);
			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			AssertEquals("declaration.ModeOfTransport", "2", declaration.ModeOfTransportAtTheBorder);
			declaration.JE_TransportMode = GBTransportTypeList.Codes.ROR;
			AssertEquals("declaration.ModeOfTransport", "6", declaration.ModeOfTransportAtTheBorder);
		}

		public void TestIsRoRoOnlyLocation()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			var eunGroup = refHelper.CreateNewOrGetExistingDataGrouping("EUN");
			var gbGroup = refHelper.CreateNewOrGetExistingDataGrouping("GB", parent: eunGroup);

			var portCodeType = refHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Port");
			var code1 = refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PtRoRo", "RoRo Location for Goods", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.Save();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals("False before adding some RoRo Attribute", false, declaration.IsRoRoOnlyLocation);

			var roroAttr = refHelper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK.ToGuid(), RefCusCodeListAttributeTypes.Codes.RoRoLocation, "");
			Factory.Save();
			declaration.JE_LocationOfGoods = "PtRoRo";
			AssertEquals("We have RoRo Location Attribute", true, declaration.IsRoRoOnlyLocation);
		}

		public void TestIsRoRoLocation()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			var eunGroup = refHelper.CreateNewOrGetExistingDataGrouping("EUN");
			var gbGroup = refHelper.CreateNewOrGetExistingDataGrouping("GB", parent: eunGroup);

			var portCodeType = refHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Port");
			var code1 = refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PtRoRo", "RoRo Location for Goods", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.Save();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals("False before adding some RoRo Attribute", false, declaration.IsRoRoLocation);

			var roroAttr = refHelper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK.ToGuid(), RefCusCodeListAttributeTypes.Codes.RoRoLocation, "");
			Factory.Save();

			declaration.JE_LocationOfGoods = "PtRoRo";
			AssertEquals("We have RoRo Location Attribute", true, declaration.IsRoRoLocation);

			var code2 = refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PrRoRo", "RoRo Location for Goods", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			JobDeclaration declaration1 = Factory.New<JobDeclaration>();
			AssertEquals("False before adding some RoRo Attribute", false, declaration1.IsRoRoLocation);

			var roroAttr1 = refHelper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK.ToGuid(), RefCusCodeListAttributeTypes.Codes.RoRoLocation, "MIXED");
			Factory.Save();
			declaration1.JE_LocationOfGoods = "PrRoRo";
			AssertEquals("We MIXED RoRo Location Attribute", true, declaration1.IsRoRoLocation);
		}

		public void TestIsRoRoMixedLocation()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			var eunGroup = refHelper.CreateNewOrGetExistingDataGrouping("EUN");
			var gbGroup = refHelper.CreateNewOrGetExistingDataGrouping("GB", parent: eunGroup);

			var portCodeType = refHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Port");
			var code1 = refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PtRoRo", "RoRo Location for Goods", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.Save();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			AssertEquals("False before adding some RoRo Attribute", false, declaration.IsRoRoLocation);

			var roroAttr = refHelper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK.ToGuid(), RefCusCodeListAttributeTypes.Codes.RoRoLocation, "");
			Factory.Save();

			declaration.JE_LocationOfGoods = "PtRoRo";
			AssertEquals("We have pure RoRo Location Attribute", false, declaration.IsRoRoMixedLocation);

			JobDeclaration declaration1 = Factory.New<JobDeclaration>();
			var code2 = refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PrRoRo", "RoRo Location for Goods", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			var roroAttr1 = refHelper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK.ToGuid(), RefCusCodeListAttributeTypes.Codes.RoRoLocation, "MIXED");
			Factory.Save();
			declaration1.JE_LocationOfGoods = "PrRoRo";
			AssertEquals("We MIXED RoRo Location Attribute", true, declaration1.IsRoRoMixedLocation);
		}

		public void TestFullLocationOfGoods()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_Calc_LocationOtherInformationCountry = "AA";
			dec.JE_Calc_LocationOtherInformationType = "BB";
			dec.JE_LocationQualifier = "CC";
			dec.JE_GoodsLocation = "DD";
			AssertEquals("AABBCCDD", dec.FullLocationOfGoods);
		}

		public void TestShowApportionmentMenuItem()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "EXP";
			dec.ZG_ManualCalc = false;
			Assert(dec.ShowApportionmentMenuItem);
			dec.JE_MessageType = "IMP";
			dec.ZG_ManualCalc = false;
			Assert(dec.ShowApportionmentMenuItem);
			dec.ZG_ManualCalc = true;
			Assert(!dec.ShowApportionmentMenuItem);
		}

		public void TestUCRReadOnly()
		{
			var jobDec = Factory.New<JobDeclaration>();
			Assert(jobDec.JE_UCRInfo.ReadOnly);
			Assert(!jobDec.DucrGenerationOptionsReadOnly);
			Factory.Save();
			Assert(!jobDec.JE_UCRInfo.ReadOnly);
			Assert(!jobDec.DucrGenerationOptionsReadOnly);
			jobDec.JE_UCR = "FOO";
			var ceh = jobDec.CustomsEntryHeaders.AddNew();
			ceh.EntryNumber = "BAR";
			jobDec.DeclarationNumber = "BAR";
			AssertEquals("Pre-Req, declaration number is stored", "BAR", jobDec.DeclarationNumber);
			jobDec.UseClientEoriForDucrInfo.RefreshBinding();
			jobDec.ClientReferenceForDucrInfo.RefreshBinding();
			Assert(jobDec.JE_UCRInfo.ReadOnly);
			Assert(jobDec.DucrGenerationOptionsReadOnly);
		}

		[TestDate(2019, 07, 31)]
		public void TestUCRWithDeclarantAndClientDucr()
		{
			var jobDec = Factory.New<JobDeclaration>();
			jobDec.JE_MessageType = JobMessageTypeList.Codes.Export;
			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "declarant2";
			jobDec.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			var trn = declarant.CustomsCodes.AddNew();
			trn.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			trn.OK_CustomsRegNo = "987654321999";

			var party = Factory.NewWithValidTestData<OrgHeader>();
			party.OH_Code = "Test123";
			var partyEori = party.CustomsCodes.AddNew();
			partyEori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			partyEori.OK_CustomsRegNo = "555666444222";
			partyEori.OK_RN_NKCodeCountry = jobDec.Country.Code;

			Factory.Save();
			var declarationReference = jobDec.JE_DeclarationReference;

			jobDec.JE_UCR = "FOO";
			var ceh = jobDec.CustomsEntryHeaders.AddNew();
			ceh.EntryNumber = "BAR";
			jobDec.DeclarationNumber = "BAR";

			AssertEquals("Pre-Req", "BAR", jobDec.DeclarationNumber);
			AssertEquals("Pre-req: JE_UCR must be readonly to proceed", true, jobDec.JE_UCRInfo.ReadOnly);

			jobDec.UseClientEoriForDucr = true;
			AssertEquals("FOO", jobDec.JE_UCR);

			jobDec.UseClientEoriForDucr = false;
			AssertEquals("FOO", jobDec.JE_UCR);
			jobDec.ClientReferenceForDucr = "ABC";
			AssertEquals("FOO", jobDec.JE_UCR);
			jobDec.ClientReferenceForDucr = ZString.Empty;
			AssertEquals("FOO", jobDec.JE_UCR);
		}

		protected override Type ExpectedMergeManagerType => typeof(CDSMergeManager);

		public void TestIrcInventoryReturnCodeAndRouteOfEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_AddInfo = "RouteOfEntry=RR*IrcInventoryReturnCode=II*ImportClearanceStatusICS=6*StyleOfEntrySOE=2";

			AssertEquals("RR", entry.CH_RouteOfEntry);
			AssertEquals("II", entry.CH_IrcInventoryReturnCode);
			Assert(entry.CH_AddInfo.Contains("RouteOfEntry=RR"));
			Assert(entry.CH_AddInfo.Contains("IrcInventoryReturnCode=II"));
			AssertEquals("6", entry.CH_ImportClearanceStatusICS);
			AssertEquals("2", entry.CH_StyleOfEntrySOE);

			AssertEquals(declaration.JE_GBIrcInventoryReturnCode, entry.CH_IrcInventoryReturnCode);
			AssertEquals(declaration.JE_GBRouteOfEntry, entry.CH_RouteOfEntry);
			AssertEquals(declaration.ZG_StyleOfEntrySOE, entry.CH_StyleOfEntrySOE);
			AssertEquals(declaration.ZG_ImportClearanceStatusICS, entry.CH_ImportClearanceStatusICS);

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(string.Empty, declaration.JE_GBIrcInventoryReturnCode);
			AssertEquals(string.Empty, declaration.JE_GBRouteOfEntry);
			AssertEquals(string.Empty, declaration.ZG_StyleOfEntrySOE);
			AssertEquals(string.Empty, declaration.ZG_ImportClearanceStatusICS);

			entry2.CH_AddInfo = "RouteOfEntry=RR*IrcInventoryReturnCode=II*ImportClearanceStatusICS=6*StyleOfEntrySOE=2";
			AssertEquals("", declaration.JE_GBIrcInventoryReturnCode);
			AssertEquals("RR", declaration.JE_GBRouteOfEntry);
			AssertEquals("2", declaration.ZG_StyleOfEntrySOE);
			AssertEquals("6", declaration.ZG_ImportClearanceStatusICS);
		}

		public new void TestAllEntriesCleared()
		{
			BaseJobDeclaration declaration = (BaseJobDeclaration)GetNewBusinessObject();
			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			AssertEquals("Empty collection", 0, declaration.CustomsEntryHeaders.Count);
			AssertEquals(false, declaration.HaveAllEntriesCleared);

			var mock1 = Factory.NewMoq<CusEntryHeader>();
			mock1.Setup(m => m.ClearanceDate).Returns(new ZDateTime(2005, 2, 1));
			declaration.CustomsEntryHeaders.Add(mock1.Object);
			AssertEquals(true, declaration.HaveAllEntriesCleared);

			var mock2 = Factory.NewMoq<CusEntryHeader>();
			mock2.Setup(m => m.ClearanceDate).Returns(ZDateTime.Empty);
			declaration.CustomsEntryHeaders.Add(mock2.Object);
			AssertEquals(false, declaration.HaveAllEntriesCleared);
			mock1.VerifyAll();
			mock2.VerifyAll();
		}

		public void TestGetEoriFor()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "Importer";
			importer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "1234", Core.Constants.CountryCodes.UnitedKingdom);
			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "Declarant";
			declarant.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "5678", Core.Constants.CountryCodes.UnitedKingdom);
			var declarantAddress = declarant.Addresses.AddNew();
			declarantAddress.Address1 = "Address";
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_OH_Importer = importer.PK;
			dec.JE_OA_DeclarantAddress = declarantAddress.PK;

			AssertEquals("GB1234", dec.GetEoriFor(EU.Business.DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority));
			AssertEquals("GB1234", dec.GetEoriFor(EU.Business.DefermentMethodList.Codes.ConsigneesAccountStandingAuthority));
			AssertEquals("GB1234", dec.GetEoriFor(EU.Business.DefermentMethodList.Codes.ConsigneesAccountConsigneeCompletingTheDeclaration));
			AssertEquals("GB5678", dec.GetEoriFor(EU.Business.DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14));
			AssertEquals(ZString.Empty, dec.GetEoriFor(ZString.Empty));
		}

		public void TestGBDeclarationSchemaMaxLength()
		{
			AssertEquals(3, JobDeclaration.Schema.JE_DeclarantTypeMaxLength);
			AssertEquals(1, JobDeclaration.Schema.JE_EntrySubStyleMaxLength);
			AssertEquals(3, JobDeclaration.Schema.SubLocationaxLength);
			AssertEquals(8, JobDeclaration.Schema.JE_CustomsOfficeMaxLength);
			AssertEquals(3, JobDeclaration.Schema.JE_CustomsProfileMaxLength);
			AssertEquals(1, JobDeclaration.Schema.JE_PaymentMethodMaxLength);

			AssertEquals(9, JobDeclaration.Schema.JE_ACAReferenceMaxLength);
			AssertEquals(2, JobDeclaration.Schema.JE_LocationQualifierMaxLength);
			AssertEquals(2, JobDeclaration.Schema.JE_Calc_LocationOtherInformationCountryMaxLength);
			AssertEquals(2, JobDeclaration.Schema.JE_Calc_LocationOtherInformationTypeMaxLength);
		}

		public void TestJE_EntrySubStyleField()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();

			declaration.JE_EntrySubStyle = "A";
			AssertNotNull(declaration.CusEntryInstruction);
			AssertEquals("Should be ChildEditable", true, declaration.IsRegisteredEditableChildObject(declaration.CustomsEntryInstructions));
			AssertEquals("declaration.CusEntryInstruction.CEI_SubStyle", "A", declaration.CusEntryInstruction.CEI_SubStyle);

			declaration.JE_EntrySubStyle = "D";
			AssertEquals("declaration.CusEntryInstruction.CEI_SubStyle", "D", declaration.CusEntryInstruction.CEI_SubStyle);

			declaration.JE_EntrySubStyle = "G";
			AssertEquals("declaration.CusEntryInstruction.CEI_SubStyle", "G", declaration.CusEntryInstruction.CEI_SubStyle);

			declaration.JE_EntryStyle = "RU";
			declaration.JE_EntrySubStyle = "G";
			AssertEquals("declaration.JE_MessageSubType", "RU", declaration.JE_MessageSubType);
			AssertEquals("declaration.JE_EntryStyle", "RU", declaration.JE_EntryStyle);
			AssertEquals("declaration.CusEntryInstruction.CEI_SubStyle", "G", declaration.CusEntryInstruction.CEI_SubStyle);

			declaration.JE_EntryStyle = "";
			declaration.JE_EntrySubStyle = "G";
			AssertEquals("declaration.JE_EntryStyle", "", declaration.JE_EntryStyle);
			AssertEquals("declaration.CusEntryInstruction.CEI_SubStyle", "G", declaration.CusEntryInstruction.CEI_SubStyle);

			AssertEquals("declaration.JE_EntryStyleInfo.MaxLength", 2, declaration.JE_EntryStyleInfo.MaxLength);
			AssertEquals("declaration.JE_EntrySubStyleInfo.MaxLength", 1, declaration.JE_EntrySubStyleInfo.MaxLength);
			AssertEquals("declaration.CusEntryInstruction.CEI_SubStyleInfo.MaxLength", 1, declaration.CusEntryInstruction.CEI_SubStyleInfo.MaxLength);
		}

		protected override void SetupInvoice(BaseJobDeclaration declaration)
		{
			base.SetupInvoice(declaration);
			((JobDeclaration)declaration).JE_DeclarationType = "ABC";
		}

		public void TestDefaultValueOnCreation()
		{
			var invoiceline = Factory.New<JobComInvoiceLine>();
			AssertEquals(0m, invoiceline.JI_CustomsThirdQuantity);
			AssertEquals("", invoiceline.JI_PrimaryPreference);
			AssertEquals("", invoiceline.JI_PrimaryPreference);
			AssertEquals(0m, invoiceline.ZG_StatisticalValue);
			AssertEquals("", invoiceline.JI_ValuationCode);
			AssertEquals("", invoiceline.JI_ConcessionOrder);
			AssertEquals(0m, invoiceline.JI_ValuationMarkup);
			AssertEquals("", invoiceline.JI_ValuationCode);
			AssertEquals("", invoiceline.JI_ConcessionOrder);
			AssertEquals(ZGuid.Empty, invoiceline.JI_OA_SupervisingOffice);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			dec.CusEntryInstruction.CEI_Style = "ABC";
			AssertEquals("CHIEF Declaration", Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF, dec.JE_ApplicationCode);

			var invLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			invLine.JI_CEI = dec.CusEntryInstruction.PK;
			AssertEquals(dec.CustomsEntryInstructionProvider.CustomsEntryInstructions[0].PK, invLine.JI_CEI);
		}

		public void TestUCCHelper()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = ZString.Empty;
			AssertNotNull(dec.UCCHelper);
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			AssertNotNull(dec.UCCHelper);
			AssertType<UCCHelper>(dec.UCCHelper);
		}

		public void TestCustomsEntryInstructions()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarantType = "IMP";
			AssertType<CusEntryInstructionCollection>(dec.CustomsEntryInstructions);
			AssertEquals(false, dec.JE_EntrySubStyle_ReadOnly);

			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = dec.PK;
			instruction.CEI_SubStyle = "X";
			AssertEquals(false, dec.JE_EntrySubStyle_ReadOnly);

			var newInstruction1 = dec.CustomsEntryInstructions.AddNew();
			newInstruction1.CEI_SubStyle = "Y";
			var newInstruction2 = dec.CustomsEntryInstructions.AddNew();
			newInstruction2.CEI_SubStyle = "Z";
			AssertEquals(true, dec.JE_EntrySubStyle_ReadOnly);

			dec.CustomsEntryInstructions.DeleteAll();
			dec.JE_EntrySubStyle = "J";
			Assert(dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.Any());
			var oneCusEntryInstruction = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions[0].CEI_SubStyle;
			AssertEquals("Should be J for one CusEntryInstruction", dec.JE_EntrySubStyle, oneCusEntryInstruction);
		}

		public new void TestGetCustomsEntryInstructionProviderCore()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertType(typeof(EntryInstructionProvider), dec.CustomsEntryInstructionProvider);
		}

		public void TestCreationOf_SelfRep_AdditionalInfo_IsUCCCompliant_Import()
		{
			CreationOf_SelfRep_AdditionalInfo_IsUCCCompliant(isImport: true);
		}
		public void TestCreationOf_SelfRep_AdditionalInfo_IsUCCCompliant_Export()
		{
			CreationOf_SelfRep_AdditionalInfo_IsUCCCompliant(isImport: false);
		}

		public void CreationOf_SelfRep_AdditionalInfo_IsUCCCompliant(bool isImport)
		{
			const string countryCodeUK = Core.Constants.CountryCodes.UnitedKingdom;
			const string EORI = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			const string eoriCode = "X123";

			var gbClient = Factory.New<OrgHeader>();
			gbClient.OH_Code = "ImporterA";
			gbClient.OH_RL_NKClosestPort = "GBLHR";
			gbClient.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(EORI, eoriCode, countryCodeUK);

			var orgForDeclarant = Factory.New<OrgHeader>();
			orgForDeclarant.OH_Code = "ImporterB";
			orgForDeclarant.OH_RL_NKClosestPort = "GBLHR";
			orgForDeclarant.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(EORI, eoriCode, countryCodeUK);
			var addressForDeclarant = orgForDeclarant.Addresses.AddNew();
			addressForDeclarant.Address1 = "Address for Declarant";
			addressForDeclarant.OA_RN_NKCountryCode = countryCodeUK;

			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec.JE_MessageType = isImport ? JobMessageTypeList.Codes.Import : JobMessageTypeList.Codes.Export;
			if (isImport)
			{
				dec.JE_OH_Importer = gbClient.PK;
			}
			else
			{
				dec.JE_OH_Supplier = gbClient.PK;
			}

			AssertEquals(false, dec.AdditionalInfos.Cast<AdditionalInfo>().Any(x =>
																			x.CSI_Code == "00500"
																			&& x.CSI_Description == "IMPORTER"));

			AssertEquals(false, dec.AdditionalInfos.Cast<AdditionalInfo>().Any(x =>
																			x.CSI_Code == "00400"
																			&& x.CSI_Description == "EXPORTER"));

			dec.JE_OA_DeclarantAddress = addressForDeclarant.PK;

			AssertEquals(isImport, dec.AdditionalInfos.Cast<AdditionalInfo>().Any(x =>
																x.CSI_Code == "00500"
																&& x.CSI_Description == "IMPORTER"));

			AssertEquals(!isImport, dec.AdditionalInfos.Cast<AdditionalInfo>().Any(x =>
																x.CSI_Code == "00400"
																&& x.CSI_Description == "EXPORTER"));
		}

		public override void TestAreMultipleEntryInstructionsAllowed()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			AssertEquals(true, dec.AreMultipleEntryInstructionsAllowed);
		}

		public override void TestGetSupportingDocSendingObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var sendingObject = declaration.GetSupportingDocSendingObject();
			AssertType(typeof(DocumentSending.SupportingDocSendingObject), sendingObject);
		}

		#region cns courier
		public void TestIsCNSandIsCNSAirImport()
		{
			var dec = Factory.New<JobDeclaration>();
			Assert(!dec.IsCNS);
			Assert(!dec.IsCNSAirImport);
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = TransportModes.Air;
			dec.ZG_Gateway = GatewayList.Codes.CNS_CUSDECOnly;
			Assert(dec.IsCNSAirImport);
			Assert(dec.IsCNS);
		}

		public void TestCourierConsignmentType()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.CourierConsignmentType = "xxx";
			AssertHasMessageErrorContaining(dec.CourierConsignmentTypeInfo, ListValidation.InvalidCodeMessageError);
			dec.CourierConsignmentType = CourierConsignmentTypes.Codes.Documents;
			AssertNoMessageErrorContaining(dec.CourierConsignmentTypeInfo, ListValidation.InvalidCodeMessageError);
			dec.CourierConsignmentType = "";
			AssertNoMessageErrorContaining(dec.CourierConsignmentTypeInfo, ListValidation.InvalidCodeMessageError);
			dec.CourierConsignmentType = CourierConsignmentTypes.Codes.Documents;
			AssertEquals(CourierConsignmentTypes.Codes.Documents, dec.CourierConsignmentType);
			Factory.Save();
			var reloaded = new BusinessObjectFactory().Load<JobDeclaration>(dec.PK);
			AssertEquals(CourierConsignmentTypes.Codes.Documents, reloaded.CourierConsignmentType);
		}

		public void TestCnsReferences()
		{
			var dec = Factory.New<JobDeclaration>();
			var b = dec.AdditionalReferenceNumbers.AddNew();
			b.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BagReference;
			b.CE_EntryNum = "BAG1";
			AssertEquals("BAG1", dec.CourierBagReference);

			var c = dec.AdditionalReferenceNumbers.AddNew();
			c.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CourierConsignmentReference;
			c.CE_EntryNum = "COU1";
			AssertEquals("COU1", dec.CourierConsignmentReference);

			Factory.Save();
			var reloaded = new BusinessObjectFactory().Load<JobDeclaration>(dec.PK);
			AssertEquals("COU1", reloaded.CourierConsignmentReference);
			AssertEquals("BAG1", reloaded.CourierBagReference);
		}

		public void TestCourierSiteId()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Test Facility Code");
			var shed = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "XXXABC", "Some Description1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			shed.Attributes.Add(helper.CreateNewOrGetExistingCusCodeListAttribute(shed.PK, EU.Business.UniversalReferenceConstants.ShedAttributes.SITECODE, "SITE1"));
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.ZG_Gateway = GatewayList.Codes.CNS_CUSDECOnly;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = TransportModeCodeList.Codes.Air;
			dec.JE_LocationOfGoods = shed.ZZD_Code;
			AssertEquals("SITE1", dec.CourierSiteId);
		}
		public void TestCourierCarrierCode()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.ZG_Gateway = GatewayList.Codes.CNS_CUSDECOnly;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = TransportModeCodeList.Codes.Air;
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "C123", Core.Constants.CountryCodes.UnitedKingdom);
			dec.JE_OH_ShippingLine = carrier.PK;
			AssertEquals("C123", dec.CourierCarrierCode);
		}
		#endregion

		public override void TestAutoRating()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountry = Env.CurrentCompany.Country.Code;
			var eunId = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(currentCountry, parent: eunId);
			Factory.Save();

			var dut = helper.CreateNewOrGetExistingRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Duty, "Duty");
			var add = helper.CreateNewOrGetExistingRateType(currentCountry, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.AntiDumpingDuty, "Anti-dumping Duty");
			var interest = helper.CreateNewOrGetExistingRateType(currentCountry, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Interest, "Interest");
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
			line.Fees.AddOrUpdate(UniversalReferenceConstants.RefCusRateCodes.Vat, 10.27);
			entry.Charges.AddNew(UniversalReferenceConstants.RefCusRateCodes.Vat, 10.27);

			Factory.Save();
			AssertAutoRateResult(dec, entry, 95.82m, 1);
		}

		public void TestResetCDSdata()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_Calc_LocationOtherInformationCountry = "GE";
			declaration.JE_Calc_LocationOtherInformationType = "BY";
			declaration.JE_LocationQualifier = "CW";
			declaration.JE_GoodsLocation = "12345";
			declaration.JE_SubLocationOfGoods = "XXX";

			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			AssertEquals("GE", declaration.JE_Calc_LocationOtherInformationCountry);
			AssertEquals("BY", declaration.JE_Calc_LocationOtherInformationType);
			AssertEquals("CW", declaration.JE_LocationQualifier);
			AssertEquals("12345", declaration.JE_GoodsLocation);
			AssertEquals("XXX", declaration.JE_SubLocationOfGoods);
			AssertEquals(ZString.Empty, declaration.JE_CHIEF_GoodsLocation);

			declaration.JE_CHIEF_GoodsLocation = "ABC";
			declaration.JE_Calc_LocationOtherInformationCountry = "";
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			AssertEquals("ABC", declaration.JE_CHIEF_GoodsLocation);
			AssertEquals("GB", declaration.JE_Calc_LocationOtherInformationCountry);
		}

		public void TestIATALoadPortIsNotDefaulted()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_RL_NKPortOfLoading = ZString.Empty;
			declaration.JE_IATALoadPort = ZString.Empty;
			declaration.JE_RL_NKPortOfLoading = "AUSYD";
			AssertEquals("declaration.JE_IATALoadPort", ZString.Empty, declaration.JE_IATALoadPort);
		}

		public void TestSupportingDocuments()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertType<SupportingDocumentCollection>(dec.SupportingDocuments);
		}

		public void TestAdditionalInfoCorrectType()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<AdditionalInfoCollection>(declaration.AdditionalInfos);
		}

		public void TestPreviousDocuments()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertType<PreviousDocumentCollection>(dec.PreviousDocuments);
		}

		public override void TestZG_GatewayVisible()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertEquals(true, dec.ZG_GatewayVisible);
		}

		public void TestJE_GBRouteOfEntryDescription()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.CustomsEntryHeaders.AddNew();
			dec.SingleEntry.CH_RouteOfEntry = "1X";
			AssertEquals(EntryStatusList.Descriptions.Route1X, dec.JE_GBRouteOfEntryDescription);
		}

		public void TestZG_ImportClearanceStatusICSDescription()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec.CustomsEntryHeaders.AddNew();
			dec.SingleEntry.CH_ImportClearanceStatusICS = DeclarationStatusICSList.Codes.DeclarationGoodsRelease;
			AssertEquals(DeclarationStatusICSList.Descriptions.DeclarationGoodsRelease, dec.ZG_ImportClearanceStatusICSDescription);
		}

		public void TestFilteredInvoiceLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<EU.Business.Declaration.InvoiceLineViewCollection<JobComInvoiceLine>>(declaration.FilteredInvoiceLines);
		}

		public void TestInvoices()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertType<InvoiceHeaderActiveCollection>(dec.Invoices);
		}

		public void TestInvoiceLines()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertType<InvoiceLineCompleteCollection>(dec.InvoiceLines);
		}

		public void TestReciprocalRates()
		{
			Assert(!Factory.New<JobDeclaration>().IsReciprocalRates);
		}

		public void TestGetCusAddInfoType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var iCusAddInfoTypeSupporter = (ICusAddInfoTypeSupporter)declaration;
			iCusAddInfoTypeSupporter.AssertType(typeof(CusAddInfo<GBCusAddInfo>), CusAddInfoTypeAttribute.Codes.GBAllSimpleProperties);
		}

		public override void TestLocalCurrencyCoreOverride()
		{
			AssertEquals(Enterprise.Core.Constants.CurrencyCodes.UnitedKingdom, ((JobDeclarationForTest)GetJobDeclarationForTesting()).LocalCurrencyCodeCoreExposed);
		}

		public void TestIsPentant()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(false, declaration.IsPentant);
			declaration.ZG_Gateway = GatewayList.Codes.Pentant;
			AssertEquals(true, declaration.IsPentant);
		}

		public void TestIsMCP()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(false, declaration.IsMCP);
			declaration.ZG_Gateway = GatewayList.Codes.MCP_CUSDECOnly;
			AssertEquals(true, declaration.IsMCP);
		}

		public void TestACA()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("", declaration.JE_ACAReference);
			declaration.JE_ACAReference = "Daniel";
			AssertEquals("Daniel", declaration.JE_ACAReference);
			Factory.Save();
			var decReloaded = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals("Daniel", decReloaded.JE_ACAReference);
			AssertEquals(1, decReloaded.AdditionalReferenceNumbers.Count);
			decReloaded.JE_ACAReference = "";
			AssertEquals("", decReloaded.JE_ACAReference);
			AssertEquals(0, decReloaded.AdditionalReferenceNumbers.Count);
		}

		public void TestACAGeneration()
		{
			var badge = new BadgeCodeSetting();
			badge.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
			badge.BadgeCode = "DJC";
			var badge2 = new BadgeCodeSetting();
			badge2.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
			badge2.BadgeCode = "LSM";
			var credential = new CredentialsSetting();
			credential.BadgeCode = badge.BadgeCode;
			credential.PIMA = "CUKFFW98000DAN";
			credential.Company = "ABC";
			var credential2 = new CredentialsSetting();
			credential2.BadgeCode = badge2.BadgeCode;
			credential2.PIMA = "CUKFFW98000DAN";
			credential2.Company = "xxxDEF";
			var badges = new BadgeCodeSettingCollection();
			badges.Add(badge);
			badges.Add(badge2);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);
			var credentials = new CredentialsSettingCollection();
			credentials.Add(credential);
			credentials.Add(credential2);
			GBCustomsDataRegistry.Instance.Credentials.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, credentials);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_CustomsProfile = "DJC";
			declaration.ZG_Gateway = GatewayList.Codes.Pentant;
			Factory.Save();
			AssertEquals("ABC00001M", declaration.JE_ACAReference);
			declaration.JE_GoodsDescription = "any small change";
			Factory.Save();
			AssertEquals("ABC00001M", declaration.JE_ACAReference);
			declaration.HasChanges = true;
			declaration.ForceNewAcaReferenceOnSaving = true;
			Factory.Save();
			AssertEquals("ABC00002M", declaration.JE_ACAReference);

			var exportDeclaration = Factory.New<JobDeclaration>();
			exportDeclaration.JE_CustomsProfile = "LSM";
			exportDeclaration.ZG_Gateway = GatewayList.Codes.Pentant;
			Factory.Save();
			AssertEquals("", exportDeclaration.JE_ACAReference);
			exportDeclaration.JE_GoodsDescription = "any small change";
			Factory.Save();
			AssertEquals("", exportDeclaration.JE_ACAReference);
			exportDeclaration.ForceNewAcaReferenceOnSaving = true;
			Factory.Save();
			AssertEquals("DEF00001X", exportDeclaration.JE_ACAReference);
		}

		public override void TestDescription()
		{
			var importdeclaration = Factory.New<JobDeclaration>();
			var descriptionImportCustomizationCollection = CustomsDataRegistry.Instance.DeclarationImportDescriptionCustomization.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
			((CodeDescriptionBoolDisallowNew)descriptionImportCustomizationCollection.FindByCode("JNO")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionImportCustomizationCollection.FindByCode("MBL")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionImportCustomizationCollection.FindByCode("HBL")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionImportCustomizationCollection.FindByCode("ENT")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionImportCustomizationCollection.FindByCode("IMP")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionImportCustomizationCollection.FindByCode("EXP")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionImportCustomizationCollection.FindByCode("ORF")).Bool = ZBool.True;

			CustomsDataRegistry.Instance.DeclarationImportDescriptionCustomization.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, descriptionImportCustomizationCollection);
			importdeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			importdeclaration.JE_DeclarationReference = "ABC";
			AssertEquals("Default Description of import job", "ABC", importdeclaration.Description);
			importdeclaration.JE_MasterUCR = "DUCR";
			AssertEquals("UCR: DUCR", importdeclaration.Description);

			var exportdeclaration = Factory.New<JobDeclaration>();
			exportdeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var descriptionExportCustomizationCollection = CustomsDataRegistry.Instance.DeclarationExportDescriptionCustomization.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
			((CodeDescriptionBoolDisallowNew)descriptionExportCustomizationCollection.FindByCode("JNO")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionExportCustomizationCollection.FindByCode("MBL")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionExportCustomizationCollection.FindByCode("HBL")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionExportCustomizationCollection.FindByCode("ENT")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionExportCustomizationCollection.FindByCode("IMP")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionExportCustomizationCollection.FindByCode("EXP")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionExportCustomizationCollection.FindByCode("ORF")).Bool = ZBool.True;

			CustomsDataRegistry.Instance.DeclarationExportDescriptionCustomization.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, descriptionExportCustomizationCollection);
			exportdeclaration.JE_DeclarationReference = "DEF";
			AssertEquals("Default Description of export job", "DEF", exportdeclaration.Description);
			exportdeclaration.JE_MasterUCR = "DUCR";
			AssertEquals("UCR: DUCR", exportdeclaration.Description);
		}

		public void TestGetSupportingDocumentValueSetStrategy()
		{
			var declaration = Factory.New<JobDeclarationForTest>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (EU.Business.Declaration.JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var supportingDocument = invoiceLine.SupportingDocuments.AddNew();
			var valueSetStrategy = declaration.GetSupportingDocumentValueSetStrategyExposed(supportingDocument);
			AssertNotNull(valueSetStrategy);
			AssertType(typeof(SupportingDocumentValueSetStrategy), valueSetStrategy);
		}

		public void TestICusAddInfoTypeSupporterAndDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			ICusAddInfoTypeSupporter supporter = declaration;
			supporter.AssertType(typeof(CusAddInfo<GBCusAddInfo>), CusAddInfoTypeAttribute.Codes.GBAllSimpleProperties);
			supporter.AssertType(null, "ZZ!");

			declaration.JE_GBRouteOfEntry = "6";
			var addInfos = Factory.Load<CusAddInfo<GBCusAddInfo>>(new ZQuery(CusAddInfoSchema.B7_ParentID, declaration.PK));
			AssertEquals(1, addInfos.Length);
			AssertEquals(declaration, addInfos[0].Data.Declaration);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var addInfo = newFactory.Load<CusAddInfo>(addInfos[0].PK);
			AssertEquals(typeof(CusAddInfo<GBCusAddInfo>), addInfo.GetType());
		}

		public void TestJE_DeclarationTypeGetterShouldNotCreateCusEntryInstruciton()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.CustomsEntryInstructions.RemoveAndDeleteAll();
			AssertEquals(ZString.Empty, dec.JE_DeclarationType);
			AssertEquals(1, dec.CustomsEntryInstructions.Count);
		}

		public void TestIsSDIAndIsFSD()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";
			dec.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportSupplementaryDeclaration; //ISD
			dec.JE_EntrySubStyle = GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.SdpSupplementaryDeclarationOrSdpAndOrLcpEidrFinalSupplementaryDeclaration; //Y
			Assert("Should be IsSDI", dec.IsSDI);

			var line1 = dec.InvoiceLines.AddNew();
			line1.JI_Procedure = JobComInvoiceLine.CfspFsdCPCCode;
			AssertEquals("Line has right CPC, must be FSD", true, dec.IsFSD);
		}

		public void TestRefLocoMapForFolkestone()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Port, "port");
			var codelist = helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, UniversalReferenceConstants.RefCusCodeListType.Port, "DOG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(codelist.PK, "Type", "SEA");
			Factory.Save();

			var countryPk = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedKingdom)).PK;
			var refLocoMapFolkestone = Factory.New<RefLocoMap>();
			refLocoMapFolkestone.RY_LocalPortCode = "DOG";
			refLocoMapFolkestone.RY_RL_NKLocoPort = "GBFOL";
			refLocoMapFolkestone.RY_SystemUsage = "SEA";
			refLocoMapFolkestone.RY_RN = countryPk;

			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			dec.JE_MessageType = "IMP";
			dec.JE_EntrySubStyle = GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.NormalFullAndWrdDeclarationGoodsArrived;
			dec.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("Pre-req", "", dec.JE_LocationOfGoods);
			dec.JE_RL_NKPortOfArrival = "GBFOL";
			AssertEquals("Folkestone GBFOL should map to DOG in box 30", "DOG", dec.JE_LocationOfGoods);
		}

		public void TestIsGoodsArrivedSubStyleAndIsGoodsNotArrivedSubStyle()
		{
			JobDeclaration dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			dec.JE_EntrySubStyle = "J";
			Assert(dec.IsGoodsArrivedSubStyle);
			dec.JE_EntrySubStyle = "K";
			Assert(!dec.IsGoodsArrivedSubStyle);
			dec.JE_EntrySubStyle = "A";
			Assert(dec.IsGoodsArrivedSubStyle);
			dec.JE_EntrySubStyle = "D";
			Assert(!dec.IsGoodsArrivedSubStyle);
			dec.JE_EntrySubStyle = "B";
			Assert(dec.IsGoodsArrivedSubStyle);
			dec.JE_EntrySubStyle = "E";
			Assert(!dec.IsGoodsArrivedSubStyle);
			dec.JE_EntrySubStyle = "C";
			Assert(dec.IsGoodsArrivedSubStyle);
			dec.JE_EntrySubStyle = "F";
			Assert(!dec.IsGoodsArrivedSubStyle);
			dec.JE_EntrySubStyle = "G";
			Assert(dec.IsGoodsArrivedSubStyle);
			dec.JE_EntrySubStyle = "H";
			Assert(!dec.IsGoodsArrivedSubStyle);

			dec.JE_EntrySubStyle = "J";
			Assert(!dec.IsGoodsNotArrivedSubStyle);
			dec.JE_EntrySubStyle = "K";
			Assert(dec.IsGoodsNotArrivedSubStyle);
			dec.JE_EntrySubStyle = "A";
			Assert(!dec.IsGoodsNotArrivedSubStyle);
			dec.JE_EntrySubStyle = "D";
			Assert(dec.IsGoodsNotArrivedSubStyle);
			dec.JE_EntrySubStyle = "B";
			Assert(!dec.IsGoodsNotArrivedSubStyle);
			dec.JE_EntrySubStyle = "E";
			Assert(dec.IsGoodsNotArrivedSubStyle);
			dec.JE_EntrySubStyle = "C";
			Assert(!dec.IsGoodsNotArrivedSubStyle);
			dec.JE_EntrySubStyle = "F";
			Assert(dec.IsGoodsNotArrivedSubStyle);
			dec.JE_EntrySubStyle = "G";
			Assert(!dec.IsGoodsNotArrivedSubStyle);
			dec.JE_EntrySubStyle = "H";
			Assert(dec.IsGoodsNotArrivedSubStyle);
		}

		public void TestGoodsArrivedSubStyles()
		{
			JobDeclaration dec = Factory.New<JobDeclaration>();
			dec.JE_EntrySubStyle = "J";
			Assert(JobDeclaration.GoodsArrivedSubStyles.Contains(dec.JE_EntrySubStyle));
			dec.JE_EntrySubStyle = "K";
			Assert(!JobDeclaration.GoodsArrivedSubStyles.Contains(dec.JE_EntrySubStyle));
			dec.JE_EntrySubStyle = "A";
			Assert(JobDeclaration.GoodsArrivedSubStyles.Contains(dec.JE_EntrySubStyle));
			dec.JE_EntrySubStyle = "D";
			Assert(!JobDeclaration.GoodsArrivedSubStyles.Contains(dec.JE_EntrySubStyle));
			dec.JE_EntrySubStyle = "B";
			Assert(JobDeclaration.GoodsArrivedSubStyles.Contains(dec.JE_EntrySubStyle));
			dec.JE_EntrySubStyle = "E";
			Assert(!JobDeclaration.GoodsArrivedSubStyles.Contains(dec.JE_EntrySubStyle));
			dec.JE_EntrySubStyle = "C";
			Assert(JobDeclaration.GoodsArrivedSubStyles.Contains(dec.JE_EntrySubStyle));
			dec.JE_EntrySubStyle = "F";
			Assert(!JobDeclaration.GoodsArrivedSubStyles.Contains(dec.JE_EntrySubStyle));
			dec.JE_EntrySubStyle = "G";
			Assert(JobDeclaration.GoodsArrivedSubStyles.Contains(dec.JE_EntrySubStyle));
			dec.JE_EntrySubStyle = "H";
			Assert(!JobDeclaration.GoodsArrivedSubStyles.Contains(dec.JE_EntrySubStyle));
		}

		public void TestIsSubStyleGoodsArrived()
		{
			foreach (var letter in ZString.AlphabeticCharacters)
			{
				var style = new ZString(letter);
				AssertEquals(style, JobDeclaration.GoodsArrivedSubStyles.Contains(style.ToUpper()), JobDeclaration.IsSubStyleGoodsArrived(style));
			}
		}

		public void TestEntryStatusDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_EntryStatus = "CLR";
			AssertContains("clear", declaration.JE_EntryStatusDescription.ToLower());

			declaration.JE_EntryStatus = "RH6";
			AssertContains("probable route 6", declaration.JE_EntryStatusDescription.ToLower());

			declaration.JE_EntryStatus = "RT6";
			AssertContains("route 6", declaration.JE_EntryStatusDescription.ToLower());

			declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_EntryStatus = "XXX";
			AssertContains("xxx", declaration.JE_EntryStatusDescription.ToLower());

			declaration.JE_EntryStatus = "YYY";
			declaration.SingleEntry.CH_RouteOfEntry = "ZZZ";
			AssertContains("route zzz", declaration.JE_EntryStatusDescription.ToLower());

			declaration.JE_EntryStatus = Customs.Common.EU.MessageStatusList.Codes.FailedFromTransmission;
			AssertContains(Customs.Common.EU.MessageStatusList.Descriptions.FailedFromTransmission, declaration.JE_EntryStatusDescription);
		}

		public void TestValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertContains("CDS", declaration.Validation.GetType().FullName);
		}

		public void TestIsSFD()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";
			AssertEquals(false, dec.IsSFD);
			dec.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportFullDeclaration;
			dec.JE_EntrySubStyle = "X";
			AssertEquals(false, dec.IsSFD);
			dec.JE_EntrySubStyle = GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.SdpSfdGoodsArrived;
			AssertEquals(true, dec.IsSFD);
			dec.JE_EntrySubStyle = GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.SdpSfdGoodsNotArrived;
			AssertEquals(true, dec.IsSFD);
			dec.JE_EntrySubStyle = GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.TransitSfdGoodsArrived;
			AssertEquals(true, dec.IsSFD);
			dec.JE_EntrySubStyle = GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.TransitSfdGoodsNotArrived;
			AssertEquals(true, dec.IsSFD);
		}

		public void TestJE_JS()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = "AGT";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "GBLHR";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "GBLHR";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			AssertEquals(MessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertNotNull(declaration.RelevantConsol);
		}

		public void TestGetNewRelatedDeclaration()
		{
			var sourceDeclaration = Factory.New<JobDeclaration>();
			sourceDeclaration.CustomsEntryHeaders.AddNew();
			sourceDeclaration.SingleEntry.CH_RouteOfEntry = "6";
			sourceDeclaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			AssertEquals(1, sourceDeclaration.CustomsEntryHeaders.Count);
			SetupFecFields(sourceDeclaration);
			AssertEquals(1, sourceDeclaration.CustomsEntryHeaders.Count);
			AssertEquals(9, sourceDeclaration.CustomsEntryHeaders[0].FECChallenges.Count);

			var relatedDeclaration = (JobDeclaration)sourceDeclaration.GetNewRelatedDeclaration(Factory);
			AssertEquals(ZString.Empty, relatedDeclaration.JE_GBRouteOfEntry);
			AssertAllFecFieldEmpty(relatedDeclaration);
		}

		public void TestParentRelatedDeclaration_IncludeRelationType_SUP()
		{
			var parentDeclaration = Factory.New<JobDeclaration>();
			Factory.Save();

			var manager = new SuppDecWizardManager(parentDeclaration);
			var presenterForTest = new Customs.Business.Testing.RelatedDeclarationControllerTest.ZFormPresenterForTest();
			var relatedDeclarationHelper = new RelatedDeclarationHelper(presenterForTest);
			var declaration = manager.CreateAndShowSupplemenaryDeclaration(relatedDeclarationHelper);

			var factory = declaration.Factory;
			factory.ResetDatabaseLoadCount();
			using (factory.EnableTableHitQueryCollection(new[] { JobDeclarationSchema.Constants.TableName }))
			{
				_ = declaration.ParentRelatedDeclaration;
			}
			var tableSelect = factory.TableSelects.Single(x => x.TableName.Equals(JobDeclarationSchema.Constants.TableName));
			var tableSelectQuery = tableSelect.Queries.Single();
			AssertContains(nameof(tableSelectQuery.Query), $"{GenPivotSchema.Constants.XX_RelationType} in ('', 'SUP')", tableSelectQuery.Query);
		}

		void SetupFecFields(JobDeclaration sourceDeclaration)
		{
			var entryHeader = sourceDeclaration.SingleEntry;
			var je_DSP = entryHeader.FECChallenges.AddNew().CY_Code = FECChallengeFields.Codes.JE_DSP;
			var je_DST = entryHeader.FECChallenges.AddNew().CY_Code = FECChallengeFields.Codes.JE_DST;
			var je_FLG = entryHeader.FECChallenges.AddNew().CY_Code = FECChallengeFields.Codes.JE_FLG;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var ji_ORG = entryHeader.FECChallenges.AddNew().CY_Code = FECChallengeFields.Codes.JI_ORG;
			var ji_NettMass = entryHeader.FECChallenges.AddNew().CY_Code = FECChallengeFields.Codes.JI_NettMass;
			var ji_NettMassUQ = entryHeader.FECChallenges.AddNew().CY_Code = FECChallengeFields.Codes.JI_NettMassUQ;
			var ji_Supp = entryHeader.FECChallenges.AddNew().CY_Code = FECChallengeFields.Codes.JI_Supp;
			var ji_SuppUQ = entryHeader.FECChallenges.AddNew().CY_Code = FECChallengeFields.Codes.JI_SuppUQ;
			var ji_price = entryHeader.FECChallenges.AddNew().CY_Code = FECChallengeFields.Codes.JI_Price;

			sourceDeclaration.InvoiceLines.AddNew().JI_CL = entryLine.PK;
			sourceDeclaration.InvoiceLines.AddNew().JI_CL = entryLine.PK;
			foreach (EU.Business.Declaration.JobComInvoiceLine line in sourceDeclaration.InvoiceLines)
			{
				line.ZG_FecDST = true;
			}
		}

		void AssertAllFecFieldEmpty(JobDeclaration relatedDeclaration)
		{
			AssertEquals(0, relatedDeclaration.CustomsEntryHeaders.Count);
			foreach (EU.Business.Declaration.JobComInvoiceLine line in relatedDeclaration.InvoiceLines)
			{
				Assert(!line.ZG_FecDST);
			}
		}

		public void TestSetDefaultValuesGB()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertEquals(false, dec.ZG_ManualCalc);
			AssertEquals(RepresentationTypeList.Codes._2Direct, dec.JE_DeclarantType);
			AssertEquals("BAS", dec.ZG_ShipmentType);
		}

		public void TestJE_GBRouteOfEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("", declaration.SingleEntry.CH_RouteOfEntry);
			declaration.SingleEntry.CH_RouteOfEntry = "XY";
			AssertEquals("XY", declaration.SingleEntry.CH_RouteOfEntry);
			Factory.Save();
			var declarationReloaded = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals("XY", declarationReloaded.SingleEntry.CH_RouteOfEntry);
		}

		public void TestJE_GbInventoryReturnCodeIRC()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("", declaration.JE_GBIrcInventoryReturnCode);
			declaration.JE_GBIrcInventoryReturnCode = "XY";
			AssertEquals("XY", declaration.JE_GBIrcInventoryReturnCode);
			Factory.Save();
			var declarationReloaded = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals("XY", declarationReloaded.JE_GBIrcInventoryReturnCode);
		}

		public void TestCanDeactivateAndIsEntryOnHold_Export()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "EXP";
			dec.CustomsEntryHeaders.AddNew();
			AssertEquals(true, dec.CanDelete);
			Assert(dec.CanCancel().IsNullOrEmpty());
			AssertEquals(false, dec.IsEntryOnHold);
			AssertEquals("", dec.ReasonForNotAbleToDelete);

			dec.JE_EntryStatus = EntryStatusList.Codes.CA;
			AssertEquals(false, dec.IsEntryOnHold);
			AssertEquals(true, dec.CanDelete);
			Assert(dec.CanCancel().IsNullOrEmpty());
			AssertEquals("", dec.ReasonForNotAbleToDelete);
			AssertEquals(false, dec.JE_RL_NKFinalDestinationInfo.ReadOnly);
			AssertEquals(false, dec.JE_RL_NKOriginInfo.ReadOnly);
			AssertEquals(false, dec.JE_RL_NKPortOfLoadingInfo.ReadOnly);
			AssertEquals(false, dec.JE_RL_NKPortOfArrivalInfo.ReadOnly);

			dec.JE_EntryStatus = EntryStatusList.Codes.XA;
			AssertEquals(true, dec.IsEntryOnHold);
			AssertEquals(true, dec.CanDelete);
			AssertContains("This declaration must not be deactivated, due to the Customs Entry Status or it is awaiting a reply", dec.CanCancel());
			AssertContains("The entry is on hold", dec.ReasonForNotAbleToDelete);

			dec.JE_EntryStatus = "";
			AssertEquals(true, dec.CanDelete);
			Assert(dec.CanCancel().IsNullOrEmpty());
			AssertEquals(false, dec.IsEntryOnHold);
			AssertEquals("", dec.ReasonForNotAbleToDelete);

			dec.JE_GBRouteOfEntry = "6";
			AssertEquals(true, dec.CanDelete);
			Assert(dec.CanCancel().IsNullOrEmpty());
			AssertEquals(false, dec.IsEntryOnHold);
			AssertEquals("", dec.ReasonForNotAbleToDelete);

			dec.SingleEntry.CH_RouteOfEntry = "1";
			AssertEquals(true, dec.CanDelete);
			AssertEquals(true, dec.IsEntryOnHold);
			AssertContains("This declaration must not be deactivated, due to the Customs Entry Status or it is awaiting a reply", dec.CanCancel());
			AssertContains("The entry is on hold", dec.ReasonForNotAbleToDelete);
			AssertEquals(false, dec.JE_RL_NKFinalDestinationInfo.ReadOnly);
			AssertEquals(false, dec.JE_RL_NKOriginInfo.ReadOnly);
			AssertEquals(false, dec.JE_RL_NKPortOfLoadingInfo.ReadOnly);
			AssertEquals(false, dec.JE_RL_NKPortOfArrivalInfo.ReadOnly);

			dec.JE_EntryStatus = EntryStatusList.Codes.Cancelled;
			AssertEquals(true, dec.IsEntryOnHold);
			Assert(dec.CanCancel().IsNullOrEmpty());
			AssertContains("", dec.ReasonForNotAbleToDelete);

			dec.JE_EntryStatus = EntryStatusList.Codes.CA;
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = dec.PK;
			var jobCharge1 = Factory.NewWithValidTestData<JobCharge>();
			jobCharge1.JR_JH = jobHeader.PK;
			var testCreditor = Factory.NewWithValidTestData<OrgHeader>();
			testCreditor.OH_IsCreditor = true;

			jobCharge1.JR_OH_CostAccount = testCreditor.PK;
			jobCharge1.JR_GB = GlbBranch.CurrentBranch.PK.ToGuid();
			jobCharge1.JR_OSCostAmt = 10.0;
			var entry = dec.ActiveEntryHeaders.AddNew();

			Factory.Save();
			Assert(!dec.CanCancel().IsNullOrEmpty());
			AssertContains("Job Invoicing Charge(s) have been saved against this Invoicing Job Header", dec.CanCancel());
		}

		public void TestCanDeactivateAndIsEntryOnHold_Import()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";
			dec.CustomsEntryHeaders.AddNew();
			dec.SingleEntry.CH_RouteOfEntry = "6";
			AssertEquals(false, dec.IsEntryOnHold);

			dec.SingleEntry.CH_RouteOfEntry = "1";
			AssertEquals(true, dec.CanDelete);
			AssertEquals(true, dec.IsEntryOnHold);
			AssertEquals(false, dec.JE_RL_NKFinalDestinationInfo.ReadOnly);
			AssertEquals(false, dec.JE_RL_NKOriginInfo.ReadOnly);
			AssertEquals(false, dec.JE_RL_NKPortOfLoadingInfo.ReadOnly);
			AssertEquals(false, dec.JE_RL_NKPortOfArrivalInfo.ReadOnly);

			AssertEquals(true, dec.IsEntryOnHold);
			Assert(!dec.CanCancel().IsNullOrEmpty());
			AssertContains("The entry is on hold", dec.ReasonForNotAbleToDelete);

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = dec.PK;
			var jobCharge1 = Factory.NewWithValidTestData<JobCharge>();
			jobCharge1.JR_JH = jobHeader.PK;
			var testCreditor = Factory.NewWithValidTestData<OrgHeader>();
			testCreditor.OH_IsCreditor = true;

			jobCharge1.JR_OH_CostAccount = testCreditor.PK;
			jobCharge1.JR_GB = GlbBranch.CurrentBranch.PK.ToGuid();
			jobCharge1.JR_OSCostAmt = 10.0;
			var entry = dec.ActiveEntryHeaders.AddNew();

			Factory.Save();
			Assert(!dec.CanCancel().IsNullOrEmpty());
			AssertContains("Job Invoicing Charge(s) have been saved against this Invoicing Job Header", dec.CanCancel());
		}

		public override void TestCanCancel_JobInCurrentCompany()
		{
			Assert(true);  // see above
		}

		public override void TestCanCancel_JobInForeignCompany()
		{
			Assert(true);  // see above
		}

		public override void TestCanCancel_ShipmentJob()
		{
			Assert(true);  // see above
		}

		public override void TestCancelCustomsCommenced_ForExport()
		{
			Assert(true);
		}

		public void TestDefaultingOnOrgChange()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "oldDec");

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "oldImp");
			declaration.JE_OH_Importer = importer.PK;

			var newImporter = Factory.NewWithValidTestData<OrgHeader>();
			newImporter.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "newImp");

			var gbOrgImpAddInfo = GBOrgImpAddInfo.Get(newImporter);
			gbOrgImpAddInfo.Deserialise();
			gbOrgImpAddInfo.ZO_VATDeferType = "B";

			var newDeclarant = Factory.NewWithValidTestData<OrgHeader>();
			newDeclarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "newDec");

			Factory.Save();

			declaration.JE_PaymentMethod = "A";
			declaration.ZG_VATDeferType = "A";
			AssertEquals(declaration.JE_DefermentAccountNumber, "oldDec");
			AssertEquals(declaration.ZG_VATDeferNumber, "oldDec");

			declaration.JE_OA_DeclarantAddress = newDeclarant.Addresses[0].PK;
			AssertEquals(declaration.JE_DefermentAccountNumber, "newDec");
			AssertEquals(declaration.ZG_VATDeferNumber, "newDec");

			declaration.JE_PaymentMethod = "B";
			declaration.ZG_VATDeferType = "B";
			AssertEquals(declaration.JE_DefermentAccountNumber, "oldImp");
			AssertEquals(declaration.ZG_VATDeferNumber, "oldImp");

			declaration.JE_OH_Importer = newImporter.PK;
			AssertEquals(declaration.JE_DefermentAccountNumber, "newImp");
			AssertEquals(declaration.ZG_VATDeferNumber, "newImp");
		}

		public void TestIsPrelodgedAndNotCancelled()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.CustomsEntryHeaders.AddNew();
			dec.JE_MessageType = "EXP";
			AssertEquals(false, dec.IsPrelodgedAndNotCancelled);
			AssertEquals(false, dec.ZG_CTStatusIDInfo.ReadOnly);
			dec.SingleEntry.CH_RouteOfEntry = "6";
			AssertEquals(false, dec.IsPrelodgedAndNotCancelled);
			AssertEquals(false, dec.ZG_CTStatusIDInfo.ReadOnly);
			dec.SingleEntry.CH_RouteOfEntry = "H";
			AssertEquals(true, dec.IsPrelodgedAndNotCancelled);
			AssertEquals(true, dec.ZG_CTStatusIDInfo.ReadOnly);
			dec.JE_EntryStatus = EntryStatusList.Codes.Cancelled;
			AssertEquals(false, dec.IsPrelodgedAndNotCancelled);
			AssertEquals(false, dec.ZG_CTStatusIDInfo.ReadOnly);
			dec.JE_EntryStatus = "";
			dec.JE_MessageType = "IMP";
			dec.SingleEntry.CH_RouteOfEntry = "H";
			AssertEquals(true, dec.IsPrelodgedAndNotCancelled);
			AssertEquals(false, dec.ZG_CTStatusIDInfo.ReadOnly);
		}

		public void TestBranchSuffixListForBox44()
		{
			var dec = Factory.New<JobDeclaration>();
			var supplier = Factory.New<OrgHeader>();
			var importer = Factory.New<OrgHeader>();
			var declarant = Factory.New<OrgHeader>();

			dec.JE_OH_Importer = importer.PK;
			dec.JE_OH_Supplier = supplier.PK;
			dec.JE_OA_DeclarantAddress = declarant.Addresses.AddNew().PK;

			var entryHeader = dec.CustomsEntryHeaders.AddNew();

			importer.CustomsCodes.RemoveAll();
			importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "987654321001");
			importer.CustomsCodes.AddNew(OrgCusCode.UnitedKingdomCodeTypes.EoriBranchSuffix, "333");
			AssertEquals("BR333", EuEoriProviderAndValidator.GetBranchSuffixesForBox44(entryHeader));
			supplier.CustomsCodes.AddNew(OrgCusCode.UnitedKingdomCodeTypes.EoriBranchSuffix, "666");
			AssertEquals("BR666 BR333", EuEoriProviderAndValidator.GetBranchSuffixesForBox44(entryHeader));
			declarant.CustomsCodes.AddNew(OrgCusCode.UnitedKingdomCodeTypes.EoriBranchSuffix, "999");
			AssertEquals("BR666 BR333 AG999", EuEoriProviderAndValidator.GetBranchSuffixesForBox44(entryHeader));
		}

		protected void PrepNITestLocations()
		{
			var chester = new RefUNLOCO.Loader(Factory).Load("GBCEG");
			if (chester.CountryStates == null || string.Compare(chester.CountryStates.RW_RegionName, "ENGLAND", true) != 0)
			{
				var eng = Factory.New<RefCountryStates>();
				chester.RL_RW = eng.PK;
				eng.RW_RegionName = "EngLanD";
			}

			var belfast = new RefUNLOCO.Loader(Factory).Load("GBBEL");
			if (belfast.CountryStates == null || string.Compare(belfast.CountryStates.RW_RegionName, "NORTHERN IRELAND", true) != 0)
			{
				var ni = Factory.New<RefCountryStates>();
				belfast.RL_RW = ni.PK;
				ni.RW_RegionName = "nORtHeRn IRelAnd";
			}
		}

		public void TestNILocations()
		{
			PrepNITestLocations();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_RL_NKOrigin = "ESBCN";
			dec.JE_RL_NKFinalDestination = "GBBEL";
			Assert(!dec.IsOriginNorthernIreland);
			Assert(dec.IsDestinationNorthernIreland);
			dec.JE_RL_NKFinalDestination = "GBCEG";
			Assert(!dec.IsDestinationNorthernIreland);
			dec.JE_RL_NKOrigin = "GBBEL";
			dec.JE_RL_NKFinalDestination = "GBCEG";
			Assert(dec.IsOriginNorthernIreland);
		}

		public void TestSpringLoad()
		{
			var gbDec = Factory.New<Integration.Customs.GB.IJobDeclaration>();
			AssertType(typeof(JobDeclaration), gbDec);
		}

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			return dec;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			declaration.CustomsEntryHeaders.AddNew();
			return declaration;
		}

		protected BaseJobDeclaration GetJobDeclarationForTesting()
		{
			return Factory.New<JobDeclarationForTest>();
		}

		public void TestTypesForLoad()
		{
			var gbDec = Factory.New<JobDeclaration>();
			AssertType(typeof(JobDeclaration), gbDec);

			var baseDecForGb = Factory.New<BaseJobDeclaration>();
			AssertType(typeof(JobDeclaration), gbDec);
		}

		public void TestGetNewValidation()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertContains("Enterprise.Customs.GB.CDS.Declaration.CDSJobDeclarationValidation", dec.Validation.GetType().FullName);
		}

		public void TestIsPackingInformationRelevantCore()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertEquals(true, dec.IsPackingInformationRelevant);
		}

		public void TestGetMergeManager()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertType(typeof(CDSMergeManager), dec.MergeManager);
		}

		public void TestCreateNewDocumentSupporter()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertType(typeof(JobDeclarationDocumentSupporter), dec.DocumentSupporter);
		}

		protected override System.Collections.Hashtable ExpectedDocAddressTypes
		{
			get
			{
				var result = base.ExpectedDocAddressTypes;
				result[DocAddressTypes.Codes.CustomsSupervisingOffice] = DocAddressType.CustomsSupervisingOffice;
				result[DocAddressTypes.Codes.GovernmentContractor] = DocAddressType.GovernmentContractor;
				result[DocAddressTypes.Codes.CustomsPlaceOfLoading] = DocAddressType.CustomsPlaceOfLoading;
				return result;
			}
		}

		public void TestMucrValidationInventorySystem()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = MessageTypeList.Codes.Export;

			dec.JE_MasterUCR = "TINY";
			AssertHasMessageErrorContaining(dec.JE_MasterUCRInfo, "MUCR should match one of the Customs-defined formats");
			dec.JE_MasterUCR = "GB/CUK1-X";
			AssertHasMessageErrorContaining(dec.JE_MasterUCRInfo, "MUCR should match one of the Customs-defined formats");
			dec.JE_MasterUCR = "GB/CUK1-X2345";
			AssertNoMessageErrorContaining(dec.JE_MasterUCRInfo, "MUCR should match one of the Customs-defined formats");
			dec.JE_MasterUCR = "GB/BAC-X2345";
			AssertNoMessageErrorContaining(dec.JE_MasterUCRInfo, "MUCR should match one of the Customs-defined formats");
			dec.JE_MasterUCR = "GB/BAC-X";
			AssertHasMessageErrorContaining(dec.JE_MasterUCRInfo, "MUCR should match one of the Customs-defined formats");

			dec.JE_MasterUCR = "A:";
			AssertHasMessageErrorContaining(dec.JE_MasterUCRInfo, "MUCR should match one of the Customs-defined formats");
			dec.JE_MasterUCR = "A:12512345678";
			AssertHasMessageErrorContaining(dec.JE_MasterUCRInfo, "MUCR should match one of the Customs-defined formats");
			dec.JE_TransportMode = TransportTypeList.Codes.Air;
			dec.JE_MasterUCR = "";
			dec.JE_MasterUCR = "A:12512345678";
			AssertNoMessageErrorContaining(dec.JE_MasterUCRInfo, "MUCR should match one of the Customs-defined formats");
			dec.JE_MessageType = "IMP";
			dec.JE_MasterUCR = "A:00012345678";
			AssertHasMessageErrorContaining(dec.JE_MasterUCRInfo, "MUCR should match one of the Customs-defined formats");
			dec.JE_MessageType = "EXP";
			dec.JE_MasterUCR = "A:BAX12345678";
			AssertNoMessageErrorContaining(dec.JE_MasterUCRInfo, "MUCR should match one of the Customs-defined formats");
			dec.JE_MasterUCR = "A:1251234567800000000";
			AssertNoMessageErrorContaining(dec.JE_MasterUCRInfo, "MUCR should match one of the Customs-defined formats");
			dec.JE_MasterUCR = "A:BAX1234567800000000";
			AssertNoMessageErrorContaining(dec.JE_MasterUCRInfo, "MUCR should match one of the Customs-defined formats");
			dec.JE_MasterUCR = "A:BAX123456780000000";
			AssertHasMessageErrorContaining(dec.JE_MasterUCRInfo, "MUCR should match one of the Customs-defined formats");
			dec.JE_MasterUCR = "A:BAX123456780000000X";
			AssertHasMessageErrorContaining(dec.JE_MasterUCRInfo, "MUCR should match one of the Customs-defined formats");

			dec.JE_MasterUCR = "GB/123456789-";
			AssertHasMessageErrorContaining(dec.JE_MasterUCRInfo, "MUCR should match one of the Customs-defined formats");
			dec.JE_MasterUCR = "GB/123456789-1";
			AssertNoMessageErrorContaining(dec.JE_MasterUCRInfo, "MUCR should match one of the Customs-defined formats");
			dec.JE_MessageType = "IMP";
			dec.JE_MasterUCR = "GB/123456789-2";
			AssertHasMessageErrorContaining(dec.JE_MasterUCRInfo, "MUCR should match one of the Customs-defined formats");
			dec.JE_MessageType = "EXP";
			dec.JE_MasterUCR = "GB/123456789-X";
			AssertNoMessageErrorContaining(dec.JE_MasterUCRInfo, "MUCR should match one of the Customs-defined formats");
			dec.JE_MasterUCR = "YBAC1112222222233333333";
			AssertNoMessageErrorContaining(dec.JE_MasterUCRInfo, "MUCR should match one of the Customs-defined formats");
			dec.JE_MasterUCR = "YBAC11122222222";
			AssertNoMessageErrorContaining(dec.JE_MasterUCRInfo, "MUCR should match one of the Customs-defined formats");
		}

		public void TestPortsAndShedLookups()
		{
			EU.Business.Testing.ShedTest.CreateShed(Factory, "GB", "LBAELX", "LBAELX  TORQUE LOGISTICS LIMITED at Leeds / Bradford", portName: "Leeds / Bradford");
			EU.Business.Testing.ShedTest.CreateShed(Factory, "GB", "ABCDJC", "Daniel Road Shed", transportModes: new[] { "ROA", "SEA" });
			EU.Business.Testing.PortTest.CreatePort(Factory, "GB", "DOV", "Dover", portType: "SEA");
			EU.Business.Testing.PortTest.CreatePort(Factory, "GB", "DOG", "Dover", portType: "ROA");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";

			declaration.JE_TransportMode = "AIR";
			AssertContains("We should see shed ELX. If not, check that the FAC row has an attribute for the airport name and that you're setting this attribute during the unit test", "ELX", declaration.Lookups.ShedsList.CodesAsString);
			var locationCodes = ((CodeDescriptionPairList)declaration.Lookups.Locations).CodesAsString;
			AssertContains("LBA", locationCodes);
			AssertNotContains("DJC", locationCodes);
			AssertNotContains("DOG", locationCodes);
			AssertNotContains("DOV", locationCodes);
			AssertNotContains("ABC", locationCodes);

			declaration.JE_TransportMode = "ROA";
			locationCodes = ((CodeDescriptionPairList)declaration.Lookups.Locations).CodesAsString;
			AssertNotContains("ELX", declaration.Lookups.ShedsList.CodesAsString);
			AssertNotContains("LBA", locationCodes);
			AssertContains("DJC", declaration.Lookups.ShedsList.CodesAsString);
			AssertContains("DOG", locationCodes);
			AssertNotContains("DOV", locationCodes);

			declaration.JE_TransportMode = "SEA";
			locationCodes = ((CodeDescriptionPairList)declaration.Lookups.Locations).CodesAsString;
			AssertNotContains("ELX", declaration.Lookups.ShedsList.CodesAsString);
			AssertNotContains("LBA", locationCodes);
			AssertContains("DJC", declaration.Lookups.ShedsList.CodesAsString);
			AssertNotContains("DOG", locationCodes);
			AssertContains("DOV", locationCodes);
		}

		public void TestMUCRValidationType1()
		{
			var declaration = Factory.New<JobDeclarationForTest>();
			AssertContains("Standard validation type", "GbMUCR", declaration.CusEntryNumValidationForMUCRForTest.FullName);

			var ccsukImportAirDeclaration = Factory.New<JobDeclarationForTest>();
			ccsukImportAirDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			ccsukImportAirDeclaration.ZG_Gateway = GatewayList.Codes.CCSUKviaNTMsgGW;
			ccsukImportAirDeclaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertContains("CCSUK validation type", "GbCcsukMUCR", ccsukImportAirDeclaration.CusEntryNumValidationForMUCRForTest.FullName);
			ccsukImportAirDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertContains("Not import air, back to standard type", "GbMUCR", ccsukImportAirDeclaration.CusEntryNumValidationForMUCRForTest.FullName);
		}

		public void TestMUCRValidationType2()
		{
			DeclarationTestHelper.CreateAgentAndShedBadgesAndCreds();
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = "AIR";
			declaration.JE_CustomsProfile = "FEY";
			AssertEquals("Pre Req, right gateway selected", GatewayList.Codes.MCP_CUSDECOnly, declaration.ZG_Gateway);
			declaration.JE_MasterUCR = "123456789";
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, "CCSUK");
			declaration.JE_CustomsProfile = "LXA";
			AssertEquals("Pre Req, right gateway selected", GatewayList.Codes.CCSUKviaNTMsgGW, declaration.ZG_Gateway);
			declaration.JE_MasterUCR = "1234567890";
			AssertHasMessageErrorContaining(declaration.JE_MasterUCRInfo, "CCSUK");
			declaration.JE_CustomsProfile = "FEY";
			declaration.JE_MasterUCR = "123456789";
			AssertNoMessageErrorContaining(declaration.JE_MasterUCRInfo, "CCSUK");
		}

		public void TestGbInventoryReturnCodeIRCList()
		{
			// Meaning of IRC varies by CSP
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.ZG_Gateway = GatewayList.Codes.CNS_CUSDECOnly;
			AssertContains("not arrived", declaration.GbInventoryReturnCodeIRCList.GetDescriptionFromCode("007"));
			declaration.ZG_Gateway = GatewayList.Codes.CCSUKviaNTMsgGW;
			AssertContains("nominated to another agent", declaration.GbInventoryReturnCodeIRCList.GetDescriptionFromCode("007"));
			var cachedList = declaration.GbInventoryReturnCodeIRCList;

			declaration.ZG_Gateway = GatewayList.Codes.MCP_CUSDECOnly;
			Assert(!declaration.GbInventoryReturnCodeIRCList.ContainsCode("007"));
			AssertContains("not authorised", declaration.GbInventoryReturnCodeIRCList.GetDescriptionFromCode("008"), true);

			AssertSame("Caching", declaration.GbInventoryReturnCodeIRCList, declaration.GbInventoryReturnCodeIRCList);
			declaration.ZG_Gateway = GatewayList.Codes.CCSUKviaNTMsgGW;
			AssertSame("Caching per gateway type", cachedList, declaration.GbInventoryReturnCodeIRCList);
		}

		public void TestJE_ApplicationCode_NeedToGetNewIncoTermAndChargeFactory()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			var invoice = declaration.Invoices.AddNew();

			AssertNotNull(declaration.IncoTermAndChargeFactory);
			AssertNotNull(invoice.IncoTermAndChargeFactory);
			Assert(!declaration.NeedToGetNewIncoTermAndChargeFactory);
			Assert(!invoice.NeedToGetNewIncoTermAndChargeFactory);

			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			Assert(declaration.NeedToGetNewIncoTermAndChargeFactory);
			Assert(invoice.NeedToGetNewIncoTermAndChargeFactory);
		}

		public void TestJobDeclarationMessageCollectionApplicationCodeList()
		{
			var declaration = Factory.New<JobDeclarationForTest>();
			AssertContainsExactElementsInAnyOrder(new ZString[]
			{
				ApplicationCodeList.Codes.GbCcsuk,
				ApplicationCodeList.Codes.GbCnsEdifactOutboundOnly,
				ApplicationCodeList.Codes.GbCnsCompass,
				ApplicationCodeList.Codes.GbEdifactShared,
				ApplicationCodeList.Codes.GbMcpEdifactOutboundOnly,
				ApplicationCodeList.Codes.GbMcpRra01AndRra11,
				ApplicationCodeList.Codes.GbMcpRra12,
				ApplicationCodeList.Codes.GbNesAllMessageTypes,
				ApplicationCodeList.Codes.GbMcpClaimUcn
			}
			, declaration.JobDeclarationMessageCollectionApplicationCodeListExposed);
		}

		public void TestZG_IsTrainingDeclarationReadonly()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertEquals(false, dec.ZG_IsTrainingDeclarationInfo.ReadOnly);
			dec.CustomsEntryHeaders.AddNew().EntryNumber = "Lodged";
			AssertEquals(true, dec.ZG_IsTrainingDeclarationInfo.ReadOnly);
		}

		public void TestTradersOwnReferenceFullForBox7WithTruncation()
		{
			var declarationBox7 = Factory.New<JobDeclaration>();
			declarationBox7.JE_MessageType = "IMP";
			declarationBox7.ZG_Gateway = GatewayList.Codes.CCSUKviaNTMsgGW;

			RunBox7TruncationTest("1234567", "1234567", declarationBox7, CcsukChiefBox7BehaviourList.Codes.DefaultSendFullReferenceWithoutManipulation);
			RunBox7TruncationTest("12345678", "12345678", declarationBox7, CcsukChiefBox7BehaviourList.Codes.DefaultSendFullReferenceWithoutManipulation);
			RunBox7TruncationTest("123456789", "123456789", declarationBox7, CcsukChiefBox7BehaviourList.Codes.DefaultSendFullReferenceWithoutManipulation);
			RunBox7TruncationTest("1234567890ABC", "1234567890ABC", declarationBox7, CcsukChiefBox7BehaviourList.Codes.DefaultSendFullReferenceWithoutManipulation);

			RunBox7TruncationTest("1234567", "1234567", declarationBox7, CcsukChiefBox7BehaviourList.Codes.TruncateWithIntelliganceIfSpaceAllowsSendRightmost8CharactersAndThenFullReferenceOtherwiseSameAsDef);
			RunBox7TruncationTest("12345678", "12345678", declarationBox7, CcsukChiefBox7BehaviourList.Codes.TruncateWithIntelliganceIfSpaceAllowsSendRightmost8CharactersAndThenFullReferenceOtherwiseSameAsDef);
			RunBox7TruncationTest("123456789", "23456789 123456789", declarationBox7, CcsukChiefBox7BehaviourList.Codes.TruncateWithIntelliganceIfSpaceAllowsSendRightmost8CharactersAndThenFullReferenceOtherwiseSameAsDef);
			RunBox7TruncationTest("1234567890ABC", "1234567890ABC", declarationBox7, CcsukChiefBox7BehaviourList.Codes.TruncateWithIntelliganceIfSpaceAllowsSendRightmost8CharactersAndThenFullReferenceOtherwiseSameAsDef);

			RunBox7TruncationTest("1234567", "1234567", declarationBox7, CcsukChiefBox7BehaviourList.Codes.TruncateWithCompromiseSendRightmost8CharactersAndThenLeftmost12Characters);
			RunBox7TruncationTest("12345678", "12345678", declarationBox7, CcsukChiefBox7BehaviourList.Codes.TruncateWithCompromiseSendRightmost8CharactersAndThenLeftmost12Characters);
			RunBox7TruncationTest("123456789", "23456789 123456789", declarationBox7, CcsukChiefBox7BehaviourList.Codes.TruncateWithCompromiseSendRightmost8CharactersAndThenLeftmost12Characters);
			RunBox7TruncationTest("1234567890ABC", "67890ABC 1234567890AB", declarationBox7, CcsukChiefBox7BehaviourList.Codes.TruncateWithCompromiseSendRightmost8CharactersAndThenLeftmost12Characters);

			RunBox7TruncationTest("1234567", "1234567", declarationBox7, CcsukChiefBox7BehaviourList.Codes.TruncateSimplySendRightmost8CharactersOnly);
			RunBox7TruncationTest("12345678", "12345678", declarationBox7, CcsukChiefBox7BehaviourList.Codes.TruncateSimplySendRightmost8CharactersOnly);
			RunBox7TruncationTest("123456789", "23456789", declarationBox7, CcsukChiefBox7BehaviourList.Codes.TruncateSimplySendRightmost8CharactersOnly);
			RunBox7TruncationTest("1234567890ABC", "67890ABC", declarationBox7, CcsukChiefBox7BehaviourList.Codes.TruncateSimplySendRightmost8CharactersOnly);

			RunBox7TruncationTest("1234567890ABC", "1234567890ABC", declarationBox7, CcsukChiefBox7BehaviourList.Codes.TruncateWithCompromiseSendRightmost8CharactersAndThenLeftmost12Characters, "SEA");

			declarationBox7.JE_OwnerRef = "X";
			AssertEquals("X", declarationBox7.TradersOwnReferenceFullForBox7);
		}

		public void TestSupplementaryDeclarationsStatistics()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_TotalNoOfPacks = 6;
			Factory.Save();
			AssertEquals(0, dec.NumberOfLinkedSupplementaryDeclarations);
			AssertEquals("", dec.NumberOfLinkedSupplementaryDeclarationsString);
			AssertEquals(0, dec.NumberOfPackagesDeclaredOnLinkedSupplementaryDeclarations);
			AssertEquals("", dec.NumberOfPackagesDeclaredOnLinkedSupplementaryDeclarationsString);

			dec.JE_DeclarationType = "ESP";
			AssertEquals(0, dec.NumberOfLinkedSupplementaryDeclarations);
			AssertEquals("0", dec.NumberOfLinkedSupplementaryDeclarationsString);

			var presenterForTest = new Customs.Business.Testing.RelatedDeclarationControllerTest.ZFormPresenterForTest();
			var relatedDeclarationHelper = new RelatedDeclarationHelper(presenterForTest);
			var relatedDec = (JobDeclaration)relatedDeclarationHelper.CreateNewRelated(dec, "SUP");
			relatedDec.JE_TotalNoOfPacks = 0;
			relatedDec.Factory.Save();

			AssertEquals(1, dec.NumberOfLinkedSupplementaryDeclarations);
			AssertEquals("1", dec.NumberOfLinkedSupplementaryDeclarationsString);

			dec.JE_DeclarationType = "EFD";
			AssertEquals("Count is correct", 1, dec.NumberOfLinkedSupplementaryDeclarations);
			AssertEquals("Count is suppressed when dec type is not relevant", "", dec.NumberOfLinkedSupplementaryDeclarationsString);

			dec.JE_DeclarationType = "ELP";
			AssertEquals(1, dec.NumberOfLinkedSupplementaryDeclarations);
			AssertEquals("1", dec.NumberOfLinkedSupplementaryDeclarationsString);
			AssertEquals(0, dec.NumberOfPackagesDeclaredOnLinkedSupplementaryDeclarations);
			AssertEquals("0", dec.NumberOfPackagesDeclaredOnLinkedSupplementaryDeclarationsString);
			AssertEquals(6, dec.NumberOfPackagesRemainingOnChildSupplementaryDeclarations);
			AssertEquals("6", dec.NumberOfPackagesRemainingOnChildSupplementaryDeclarationsString);

			relatedDec.JE_TotalNoOfPacks = 1;
			relatedDec.Factory.Save();
			AssertEquals(1, dec.NumberOfPackagesDeclaredOnLinkedSupplementaryDeclarations);
			AssertEquals("1", dec.NumberOfPackagesDeclaredOnLinkedSupplementaryDeclarationsString);
			AssertEquals(5, dec.NumberOfPackagesRemainingOnChildSupplementaryDeclarations);
			AssertEquals("5", dec.NumberOfPackagesRemainingOnChildSupplementaryDeclarationsString);

			var anotherRelatedDec = (JobDeclaration)relatedDeclarationHelper.CreateNewRelated(dec, "SUP");
			anotherRelatedDec.Factory.Save();
			AssertEquals(2, dec.NumberOfLinkedSupplementaryDeclarations);

			var yetAnotherRelatedDecButWrongRelationship = (JobDeclaration)relatedDeclarationHelper.CreateNewRelated(dec);
			yetAnotherRelatedDecButWrongRelationship.Factory.Save();
			AssertEquals("Does not show up in SuppDecs list", 2, dec.NumberOfLinkedSupplementaryDeclarations);
		}

		public void TestIsDeclarationTypeImpliesRequiresSupplementaryDeclaration()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationType = "EFD";
			Assert(!dec.IsDeclarationTypeImpliesRequiresSupplementaryDeclaration);
			dec.JE_DeclarationType = "ESD";
			Assert(!dec.IsDeclarationTypeImpliesRequiresSupplementaryDeclaration);
			dec.JE_DeclarationType = "ECR";
			Assert(!dec.IsDeclarationTypeImpliesRequiresSupplementaryDeclaration);
			dec.JE_DeclarationType = "EXS";
			Assert(!dec.IsDeclarationTypeImpliesRequiresSupplementaryDeclaration);
			dec.JE_DeclarationType = "ELP";
			Assert(dec.IsDeclarationTypeImpliesRequiresSupplementaryDeclaration);
			dec.JE_DeclarationType = "ESP";
			Assert(dec.IsDeclarationTypeImpliesRequiresSupplementaryDeclaration);
			dec.JE_MessageType = "IMP";
			dec.JE_DeclarationType = "IFD";
			Assert(!dec.IsSFD);
			Assert(!dec.IsDeclarationTypeImpliesRequiresSupplementaryDeclaration);
			dec.JE_EntrySubStyle = GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.SdpSfdGoodsArrived;
			Assert(dec.IsSFD);
			Assert(dec.IsDeclarationTypeImpliesRequiresSupplementaryDeclaration);
		}

		public void TestIsSupplementaryDeclarationType()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationType = "EFD";
			Assert(!dec.IsSupplementaryDeclarationType);
			dec.JE_DeclarationType = "ESD";
			Assert(dec.IsSupplementaryDeclarationType);
			dec.JE_MessageType = "IMP";
			dec.JE_DeclarationType = "IFD";
			Assert(!dec.IsSupplementaryDeclarationType);
			dec.JE_DeclarationType = "ISD";
			Assert(dec.IsSupplementaryDeclarationType);
			dec.JE_DeclarationType = "IFW";
			Assert(!dec.IsSupplementaryDeclarationType);
			dec.JE_DeclarationType = "ISW";
			Assert(dec.IsSupplementaryDeclarationType);
			dec.JE_DeclarationType = "ICR";
			Assert(!dec.IsSupplementaryDeclarationType);
		}

		public void TestJE_ApplicationCode_ReadOnly_ConfigureLocalCountryCustomsInterface()
		{
			CombineAssertions(() =>
			{
				var customsInterface = new LocalCountryCustomsInterface();
				customsInterface.RecipientID = "RecipientID";
				customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					AssertEquals("Has LocalCountryCustomsInterface with SubmissionType BLT", true, Factory.New<JobDeclaration>().JE_ApplicationCodeInfo.ReadOnly);
				}

				customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					AssertEquals("Has LocalCountryCustomsInterface with SubmissionType ITF", true, Factory.New<JobDeclaration>().JE_ApplicationCodeInfo.ReadOnly);
				}

				customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothInterfaceDefaulted;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					AssertEquals("Has LocalCountryCustomsInterface with SubmissionType BIT", false, Factory.New<JobDeclaration>().JE_ApplicationCodeInfo.ReadOnly);
				}

				customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					AssertEquals("Has LocalCountryCustomsInterface with SubmissionType BTH", false, Factory.New<JobDeclaration>().JE_ApplicationCodeInfo.ReadOnly);
				}
			});
		}

		public void TestJE_ApplicationCodeCalculatedWhenBadgeCodeSelected()
		{
			var badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var badgeCodeSettingCDS = badgeCodeSettings.AddNew();
			badgeCodeSettingCDS.BadgeCode = "ABC";
			badgeCodeSettingCDS.RL_PortCode = "GBLBA";
			badgeCodeSettingCDS.Direction = "IMP";
			badgeCodeSettingCDS.CSPCode = "CCSUK";
			badgeCodeSettingCDS.MasterUcrCalculationMode = MucrGenerationStyles.Codes.Ccsuk;
			badgeCodeSettingCDS.ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			var badgeCodeSettingChief = badgeCodeSettings.AddNew();
			badgeCodeSettingChief.BadgeCode = "ABC";
			badgeCodeSettingChief.RL_PortCode = "GBLBA";
			badgeCodeSettingChief.Direction = "EXP";
			badgeCodeSettingChief.CSPCode = "CCSUK";
			badgeCodeSettingChief.MasterUcrCalculationMode = MucrGenerationStyles.Codes.Air;
			badgeCodeSettingChief.ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;

			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);
			GlbStaff.CurrentUser.GS_IsDeveloper = false;
			Factory.Save();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.DeclarationApplicationCode, CountryCodes.UnitedKingdom, ZDateTime.Now, true))
			{
				var dec = Factory.New<JobDeclaration>();
				AssertNotEquals("Pre-requisite: defaulted application code", Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced, dec.JE_ApplicationCode);
				dec.JE_MessageType = "IMP";
				dec.JE_CustomsProfile = "ABC";
				AssertEquals("Application Code should default to CDS", Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, dec.JE_ApplicationCode);

				dec = Factory.New<JobDeclaration>();
				dec.JE_MessageType = "EXP";
				dec.JE_CustomsProfile = "ABC";
				AssertEquals("Application Code should default to CHIEF", Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF, dec.JE_ApplicationCode);
			}
		}

		public void TestJE_ApplicationCodeSetToChiefIfBadgeApplicationCodeIsEmpty()
		{
			var badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var badgeCodeSettingCDS = badgeCodeSettings.AddNew();
			badgeCodeSettingCDS.BadgeCode = "ABC";
			badgeCodeSettingCDS.RL_PortCode = "GBLBA";
			badgeCodeSettingCDS.Direction = "IMP";
			badgeCodeSettingCDS.CSPCode = "CCSUK";
			badgeCodeSettingCDS.MasterUcrCalculationMode = MucrGenerationStyles.Codes.Ccsuk;
			badgeCodeSettingCDS.ApplicationCode = ZString.Empty;

			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);
			GlbStaff.CurrentUser.GS_IsDeveloper = false;
			Factory.Save();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.DeclarationApplicationCode, CountryCodes.UnitedKingdom, ZDateTime.Now, true))
			{
				var dec = Factory.New<JobDeclaration>();
				dec.JE_MessageType = "IMP";
				dec.JE_CustomsProfile = "ABC";
				AssertEquals("Should default to CHIEF if badge application code is empty", Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF, dec.JE_ApplicationCode);
			}
		}

		void RunBox7TruncationTest(string source, ZString expectedOutput, JobDeclaration declarationBox7, string registryMode, string transportModeToMakeItRelevantToCcsuk = "AIR")
		{
			GBCustomsDataRegistry.Instance.CcsukTruncateChiefBox7ToRight8Characters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryMode);
			declarationBox7.JE_TransportMode = transportModeToMakeItRelevantToCcsuk;
			declarationBox7.JE_DeclarationReference = source;
			AssertEquals(registryMode + " should give...", expectedOutput, declarationBox7.TradersOwnReferenceFullForBox7);
		}

		public void TestCheckBox18ChangesByTransportMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = "RAI";
			declaration.ZG_Box18TransportID = "TR123";
			declaration.ZG_Box18TransportNationality = "KR";
			AssertEquals("TR123", declaration.ZG_Box18TransportID);
			AssertEquals("KR", declaration.ZG_Box18TransportNationality);

			declaration.JE_TransportMode = "SEA";
			AssertEquals("", declaration.ZG_Box18TransportID);
			AssertEquals("", declaration.ZG_Box18TransportNationality);

			declaration.JE_TransportMode = "RAI";
			declaration.ZG_Box18TransportID = "TR123";
			declaration.ZG_Box18TransportNationality = "KR";
			AssertEquals("TR123", declaration.ZG_Box18TransportID);
			AssertEquals("KR", declaration.ZG_Box18TransportNationality);

			declaration.JE_TransportMode = "MAI";
			AssertEquals("TR123", declaration.ZG_Box18TransportID);
			AssertEquals("KR", declaration.ZG_Box18TransportNationality);
		}

		public void TestIsInventoryControlledAirImport()
		{
			JobDeclaration dec = Factory.New<JobDeclaration>();
			dec.JE_TransportMode = TransportTypeList.Codes.Air;
			dec.JE_MessageType = "IMP";
			dec.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportFullDeclaration;
			AssertEquals("IMP/Air/IFD IsInventoryControlledImport", true, dec.IsInventoryControlledImport);
			AssertEquals("IMP/Air/IFD IsInventoryControlledAirImport", true, dec.IsInventoryControlledAirImport);

			dec.JE_MessageType = "EXP";
			AssertEquals("EXP/Air/IFD IsInventoryControlledImport", false, dec.IsInventoryControlledImport);
			AssertEquals("EXP/Air/IFD IsInventoryControlledAirImport", false, dec.IsInventoryControlledAirImport);

			dec.JE_TransportMode = TransportTypeList.Codes.Sea;
			dec.JE_MessageType = "IMP";
			dec.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportFullDeclaration;
			AssertEquals("IMP/Sea/IFD IsInventoryControlledImport", true, dec.IsInventoryControlledImport);
			AssertEquals("IMP/Sea/IFD IsInventoryControlledAirImport", false, dec.IsInventoryControlledAirImport);

			dec.JE_TransportMode = TransportTypeList.Codes.Air;
			dec.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportSupplementaryDeclaration;
			AssertEquals("IMP/Air/ISD IsInventoryControlledImport", false, dec.IsInventoryControlledImport);
			AssertEquals("IMP/Air/ISD IsInventoryControlledAirImport", false, dec.IsInventoryControlledAirImport);

			dec.JE_TransportMode = TransportTypeList.Codes.Air;
			dec.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportSupplementaryWarehouse;
			AssertEquals("IMP/Air/ISW IsInventoryControlledAirImport", false, dec.IsInventoryControlledAirImport);

			dec.JE_TransportMode = TransportTypeList.Codes.Air;
			dec.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportFullWarehouse;
			AssertEquals("IMP/Air/IFW IsInventoryControlledAirImport", false, dec.IsInventoryControlledAirImport);
		}

		public void TestIsXXXDeclarationType()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";
			dec.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportClearanceRequest;
			RunMultiAssertDeclarationType(dec, dec.IsICR, dec.IsIFD, dec.IsIFW, dec.IsISD, dec.IsISW, dec.IsESD, dec.IsELP, dec.IsEFD, dec.IsECR, dec.IsESP, dec.IsEXS);
			dec.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportFullDeclaration;
			RunMultiAssertDeclarationType(dec, dec.IsIFD, dec.IsIFW, dec.IsISD, dec.IsISW, dec.IsESD, dec.IsELP, dec.IsEFD, dec.IsECR, dec.IsESP, dec.IsEXS, dec.IsICR);
			dec.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportFullWarehouse;
			RunMultiAssertDeclarationType(dec, dec.IsIFW, dec.IsISD, dec.IsISW, dec.IsESD, dec.IsELP, dec.IsEFD, dec.IsECR, dec.IsESP, dec.IsEXS, dec.IsICR, dec.IsIFD);
			dec.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportSupplementaryDeclaration;
			RunMultiAssertDeclarationType(dec, dec.IsISD, dec.IsISW, dec.IsESD, dec.IsELP, dec.IsEFD, dec.IsECR, dec.IsESP, dec.IsEXS, dec.IsICR, dec.IsIFD, dec.IsIFW);
			dec.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportSupplementaryWarehouse;
			RunMultiAssertDeclarationType(dec, dec.IsISW, dec.IsESD, dec.IsELP, dec.IsEFD, dec.IsECR, dec.IsESP, dec.IsEXS, dec.IsICR, dec.IsIFD, dec.IsIFW, dec.IsISD);

			dec.JE_MessageType = "EXP";
			dec.JE_DeclarationType = ExportSADDeclarationTypeList.Codes.ExportSupplementaryDeclaration;
			RunMultiAssertDeclarationType(dec, dec.IsESD, dec.IsELP, dec.IsEFD, dec.IsECR, dec.IsESP, dec.IsEXS, dec.IsICR, dec.IsIFD, dec.IsIFW, dec.IsISD, dec.IsISW);
			dec.JE_DeclarationType = ExportSADDeclarationTypeList.Codes.ExportLCPPreShipment;
			RunMultiAssertDeclarationType(dec, dec.IsELP, dec.IsEFD, dec.IsECR, dec.IsESP, dec.IsEXS, dec.IsICR, dec.IsIFD, dec.IsIFW, dec.IsISD, dec.IsISW, dec.IsESD);
			dec.JE_DeclarationType = ExportSADDeclarationTypeList.Codes.ExportFullDeclaration;
			RunMultiAssertDeclarationType(dec, dec.IsEFD, dec.IsECR, dec.IsESP, dec.IsEXS, dec.IsICR, dec.IsIFD, dec.IsIFW, dec.IsISD, dec.IsISW, dec.IsESD, dec.IsELP);
			dec.JE_DeclarationType = ExportSADDeclarationTypeList.Codes.ExportClearanceRequest;
			RunMultiAssertDeclarationType(dec, dec.IsECR, dec.IsESP, dec.IsEXS, dec.IsICR, dec.IsIFD, dec.IsIFW, dec.IsISD, dec.IsISW, dec.IsESD, dec.IsELP, dec.IsEFD);
			dec.JE_DeclarationType = ExportSADDeclarationTypeList.Codes.ExportSDPPreShipment;
			RunMultiAssertDeclarationType(dec, dec.IsESP, dec.IsEXS, dec.IsICR, dec.IsIFD, dec.IsIFW, dec.IsISD, dec.IsISW, dec.IsESD, dec.IsELP, dec.IsEFD, dec.IsECR);
			dec.JE_DeclarationType = ExportSADDeclarationTypeList.Codes.ExitSummaryDeclaration;
			RunMultiAssertDeclarationType(dec, dec.IsEXS, dec.IsICR, dec.IsIFD, dec.IsIFW, dec.IsISD, dec.IsISW, dec.IsESD, dec.IsELP, dec.IsEFD, dec.IsECR, dec.IsESP);
			//dec.IsICR, dec.IsIFD, dec.IsIFW , dec.IsISD, dec.IsISW , dec.IsESD , dec.IsELP , dec.IsEFD , dec.IsECR, dec.IsESP, dec.IsEXS
		}

		public override void TestDefaultDataGroupingCode()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			AssertEquals("DefaultDataGroupingCode should be CDS", (ZString)Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, dec.GetDefaultDataGroupingCode());

			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			AssertEquals("DefaultDataGroupingCode should be GB", CountryCodes.UnitedKingdom, dec.GetDefaultDataGroupingCode());
		}

		public override void TestDefaultDataGroupingCodeForTariffs()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			AssertEquals("DefaultDataGroupingCode should be CDS", (ZString)Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, dec.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff));
			dec.JE_NorthernIrelandMode = NIModeList.Codes.MovementFromGreatBritainToNi;
			dec.JE_NiGoodsAtRiskOfMovingToROI = true;
			AssertEquals("DefaultDataGroupingCode should be XI", (ZString)Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, dec.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff));
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			AssertEquals("DefaultDataGroupingCode should be GB", CountryCodes.UnitedKingdom, dec.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff));
		}

		public override void TestDefaultDataGroupingCodeForDutyRateCodes()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			AssertEquals("DefaultDataGroupingCode should be CDS", (ZString)Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, dec.GetDefaultDataGroupingCode(DefaultDataGroupingType.DutyRateCodes));

			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			AssertEquals("DefaultDataGroupingCode should be GB", CountryCodes.UnitedKingdom, dec.GetDefaultDataGroupingCode(DefaultDataGroupingType.DutyRateCodes));
		}

		public void TestGuarantees()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertType<GBGuaranteeCollection>(dec.Guarantees);
			var guarantee = dec.Guarantees.AddNew();
			AssertType<GBGuarantee>(dec.Guarantees[0]);
		}

		public void TestShowSubmitMenuItem()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
				AssertEquals("CHF", false, declaration.ShowSubmitMenuItem);
				declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				AssertEquals("CDS", false, declaration.ShowSubmitMenuItem);
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
				AssertEquals("ITF", true, declaration.ShowSubmitMenuItem);
				declaration.JE_ApplicationCode = ZString.Empty;
				AssertEquals("Empty", true, declaration.ShowSubmitMenuItem);
			});
		}

		public void TestCloneJobDeclarationWithSupportingDocuments()
		{
			#region SetupReferenceData

			var helper = new UniversalReferenceTestDataHelper(Factory);
			string grouping = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			helper.CreateNewOrGetExistingDataGrouping(grouping, "CDS Grouping");
			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			helper.CreateNewOrGetExistingCusCodeType(importCodeType, "Document Type (EU Box 44 Imports)");
			helper.CreateNewOrGetExistingCusCodeType(exportCodeType, "Document Type (EU Box 44 Exports)");

			var attributeNameValuePairs = new Dictionary<string, string[]>();
			attributeNameValuePairs.Add("Level", new string[] { "HEADER" });
			attributeNameValuePairs.Add("ACTAV", new string[] { "XY", "EF" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(grouping, new string[] { importCodeType, exportCodeType }, "9MCR", "9MCR - Description",
				attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			attributeNameValuePairs.Clear();
			attributeNameValuePairs.Add("Level", new string[] { "HEADER" });
			attributeNameValuePairs.Add("ACTAV", new string[] { "XY", "AB" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(grouping, new string[] { importCodeType, exportCodeType }, "9DCR", "9DCR - Description",
				attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			attributeNameValuePairs.Clear();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			attributeNameValuePairs.Add("ACTAV", new string[] { "XY", "IJ" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(grouping, new string[] { importCodeType, exportCodeType }, "C040", "C040 - Description",
				attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			attributeNameValuePairs.Clear();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			attributeNameValuePairs.Add("ACTAV", new string[] { "XY", "CD" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(grouping, new string[] { importCodeType, exportCodeType }, "Y019", "Y019 - Description",
				attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			attributeNameValuePairs.Clear();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			attributeNameValuePairs.Add("ACTAV", new string[] { "XY", "GH" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(grouping, new string[] { importCodeType, exportCodeType }, "9100", "9100 - Description",
				attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			attributeNameValuePairs.Clear();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			attributeNameValuePairs.Add("ACTAV", new string[] { "XY", "KL" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(grouping, new string[] { importCodeType, exportCodeType }, "D015", "D015 - Description",
				attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			#endregion

			#region ConstructBusinessObjects

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";
			dec.JE_ApplicationCode = "CDS";

			var invoice = dec.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV-AA";

			var invLine = invoice.InvoiceLines.AddNew();
			invLine.JI_LineNo = 1;

			var supDocDec1 = dec.SupportingDocuments.AddNew();
			PrepareSupDocForCloneTest(supDocDec1, "9DCR", "A", "B");

			var supDocDec2 = dec.SupportingDocuments.AddNew();
			PrepareSupDocForCloneTest(supDocDec2, "Y019", "C", "D");

			var supDocHdr1 = invoice.SupportingDocuments.AddNew();
			PrepareSupDocForCloneTest(supDocHdr1, "9MCR", "E", "F");

			var supDocHdr2 = invoice.SupportingDocuments.AddNew();
			PrepareSupDocForCloneTest(supDocHdr2, "9100", "G", "H");

			var supDocLn1 = invLine.SupportingDocuments.AddNew();
			PrepareSupDocForCloneTest(supDocLn1, "C040", "I", "J");

			var supDocLn2 = invLine.SupportingDocuments.AddNew();
			PrepareSupDocForCloneTest(supDocLn2, "D015", "K", "L");

			#endregion

			var clone = (JobDeclaration)dec.TemplateCopy();

			string relationshipType = null;
			var relatedDeclaration = (JobDeclaration)dec.GetNewRelatedDeclaration(new BusinessObjectFactory(), relationshipType);

			ConfirmSuppDocExpectedSettingsForCloneTest(supDocDec1, "9DCR", false, false, true, "A", "B");
			ConfirmSuppDocExpectedSettingsForCloneTest(supDocDec2, "Y019", true, true, false, "C", "D");
			ConfirmSuppDocExpectedSettingsForCloneTest(supDocHdr1, "9MCR", false, false, true, "E", "F");
			ConfirmSuppDocExpectedSettingsForCloneTest(supDocHdr2, "9100", true, true, false, "G", "H");
			ConfirmSuppDocExpectedSettingsForCloneTest(supDocLn1, "C040", true, true, false, "I", "J");
			ConfirmSuppDocExpectedSettingsForCloneTest(supDocLn2, "D015", true, true, false, "K", "L");

			CompareClonedSupportingDocuments(dec, clone);
			CompareClonedSupportingDocuments(dec, relatedDeclaration);

			Check_HasChanges_Property_during_Save_Operation(clone);
			Check_HasChanges_Property_during_Save_Operation(relatedDeclaration);
		}

		void CompareClonedSupportingDocuments(JobDeclaration dec, JobDeclaration clone)
		{
			var invoice = dec.Invoices[0];
			var invLine = invoice.InvoiceLines[0];

			var cloneInvoice = clone.Invoices[0];
			var cloneInvLine = cloneInvoice.InvoiceLines[0];

			AssertNotEquals(dec, clone);
			AssertNotEquals(invoice, cloneInvoice);
			AssertNotEquals(invLine, cloneInvLine);

			AssertNotEquals(dec.SupportingDocuments, clone.SupportingDocuments);
			AssertNotEquals(invoice.SupportingDocuments, cloneInvoice.SupportingDocuments);
			AssertNotEquals(invLine.SupportingDocuments, cloneInvLine.SupportingDocuments);

			AssertEquals(dec.SupportingDocuments.Count, clone.SupportingDocuments.Count);
			AssertEquals(invoice.SupportingDocuments.Count, cloneInvoice.SupportingDocuments.Count);
			AssertEquals(invLine.SupportingDocuments.Count, cloneInvLine.SupportingDocuments.Count);

			for (int a = 0; a < dec.SupportingDocuments.Count; a++)
			{
				var oriSupDoc = dec.SupportingDocuments[a];
				var clonedDoc = clone.SupportingDocuments[a];
				CompareClonedSupportingDocumentToOriginal(oriSupDoc, clonedDoc, "declaration");
			}

			for (int a = 0; a < invoice.SupportingDocuments.Count; a++)
			{
				var oriSupDoc = invoice.SupportingDocuments[a];
				var clonedDoc = cloneInvoice.SupportingDocuments[a];
				CompareClonedSupportingDocumentToOriginal(oriSupDoc, clonedDoc, "commercial invoice header");
			}

			for (int a = 0; a < invLine.SupportingDocuments.Count; a++)
			{
				var oriSupDoc = invLine.SupportingDocuments[a];
				var clonedDoc = cloneInvLine.SupportingDocuments[a];
				CompareClonedSupportingDocumentToOriginal(oriSupDoc, clonedDoc, "commercial invoice line");
			}
		}

		void PrepareSupDocForCloneTest(SupportingDocument supDoc,
										string typeCode,
										string availability,
										string actions)
		{
			supDoc.CSI_Code = typeCode;
			supDoc.CSI_Availability = availability;
			supDoc.CSI_Actions = actions;
		}

		void ConfirmSuppDocExpectedSettingsForCloneTest(SupportingDocument supDoc,
														string typeCode,
														bool isLine,
														bool isLineOnly,
														bool isHeaderOnly,
														string availability,
														string actions)
		{
			string msg = "Supporting Document type code = " + typeCode;

			AssertEquals(msg, typeCode, supDoc.CSI_Code);
			AssertEquals(msg, isLine, supDoc.IsLine);
			AssertEquals(msg, isLineOnly, supDoc.IsLineOnly);
			AssertEquals(msg, isHeaderOnly, supDoc.IsHeaderOnly);

			AssertEquals(msg, availability, supDoc.CSI_Availability);
			AssertEquals(msg, actions, supDoc.CSI_Actions);
		}

		void CompareClonedSupportingDocumentToOriginal(SupportingDocument oriSupDoc, SupportingDocument clonedDoc, string parentType)
		{
			string msg = $"Supporting Document type code = {oriSupDoc.CSI_Code}. Parent is a {parentType}.";

			AssertEquals(msg, oriSupDoc.CSI_Code, clonedDoc.CSI_Code);
			AssertEquals(msg, oriSupDoc.IsLine, clonedDoc.IsLine);
			AssertEquals(msg, oriSupDoc.IsLineOnly, clonedDoc.IsLineOnly);
			AssertEquals(msg, oriSupDoc.IsHeaderOnly, clonedDoc.IsHeaderOnly);

			AssertEquals(msg, oriSupDoc.CSI_Availability, clonedDoc.CSI_Availability);
			AssertEquals(msg, oriSupDoc.CSI_Actions, clonedDoc.CSI_Actions);
		}

		void Check_HasChanges_Property_during_Save_Operation(JobDeclaration clone)
		{
			var list = new List<BusinessObject>();

			AssertEquals("Two supporting documents expected under the declaration.", 2, clone.SupportingDocuments.Count);
			AssertEquals("One invoice header expected under the declaration.", 1, clone.Invoices.Count);

			list.Add(clone);
			list.Add(clone.SupportingDocuments[0]);
			list.Add(clone.SupportingDocuments[1]);

			var invoice = clone.Invoices[0];

			AssertEquals("Two supporting documents expected under the invoice header.", 2, invoice.SupportingDocuments.Count);
			AssertEquals("One invoice line expected under the invoice header.", 1, invoice.InvoiceLines.Count);

			list.Add(invoice);
			list.Add(invoice.SupportingDocuments[0]);
			list.Add(invoice.SupportingDocuments[1]);

			var invLine = invoice.InvoiceLines[0];

			AssertEquals("Two supporting documents expected under the invoice line.", 2, invLine.SupportingDocuments.Count);

			list.Add(invLine);
			list.Add(invLine.SupportingDocuments[0]);
			list.Add(invLine.SupportingDocuments[1]);

			Check_HasChanges_Property_Status(true, list, "Before saving, the value of the HasChanges property must be TRUE");

			var factories = new BusinessObjectFactory[] { clone.Factory };
			BusinessObjectFactory.SaveTogether(factories);

			Check_HasChanges_Property_Status(false, list, "After saving, the value of the HasChanges property must be FALSE");
		}

		void Check_HasChanges_Property_Status(bool expectedValue, List<BusinessObject> list, string assertMessage)
		{
			foreach (BusinessObject bo in list)
			{
				string msg = assertMessage + $", for object type {bo.GetType().Name}.";
				AssertEquals(msg, expectedValue, bo.HasChanges);
			}
		}

		protected override string GetLocalPortCode()
		{
			return "GBLON";
		}

		public void TestDeclarationPropertiesAfterAssigningImport_GB()
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

				var euAddInfo = EUOrgImpAddInfo.Get(importer, dec.CountryCode);
				var addInfo = GBOrgImpAddInfo.Get(importer);
				euAddInfo.Deserialise();

				// Don't use A for test here, as we default the values from declarant when ZO_OtherDeferType is A, from importer otherwise.
				euAddInfo.ZO_OtherDeferType = "B";
				addInfo.ZO_VATDeferType = "D";
				euAddInfo.ZO_Box14UseIndirectRepresentation = true;
				addInfo.ZO_Box44UseClientsEoriForDucrs = false;
				addInfo.ZO_Box44ClientsDucrSourceAttributeField = "CA1";
				addInfo.ZO_Box7DeclarantsReferenceSourceAttributeField = "CA2";

				dec.DocsAndCartage.JP_CustomAttrib1 = "A1";
				dec.DocsAndCartage.JP_CustomAttrib2 = "A2";

				Factory.Save();

				dec.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;

				AssertEquals("B", dec.JE_PaymentMethod);
				AssertEquals(RepresentationTypeList.Codes._3Indirect, dec.JE_DeclarantType);

				AssertCountrySpecificBehaviourAfterAssigningImporter(addInfo, dec);

				dec.ImporterDocumentaryAddress.E2_OA_Address = new ZGuid();
				dec.JE_OH_Importer = new ZGuid();

				euAddInfo.ZO_OtherDeferType = "C";
				addInfo.ZO_VATDeferType = "V";
				euAddInfo.ZO_Box14UseIndirectRepresentation = false;
				addInfo.ZO_Box44UseClientsEoriForDucrs = true;
				addInfo.ZO_Box44ClientsDucrSourceAttributeField = "CA2";
				addInfo.ZO_Box7DeclarantsReferenceSourceAttributeField = "CA1";

				dec.DocsAndCartage.JP_CustomAttrib1 = "T1";
				dec.DocsAndCartage.JP_CustomAttrib2 = "T2";

				dec.ClientReferenceForDucr = null;
				dec.JE_OwnerRef = null;
				dec.JE_DeclarantType = "X";

				Factory.Save();

				dec.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;

				AssertEquals("C", dec.JE_PaymentMethod);
				AssertEquals(RepresentationTypeList.Codes._2Direct, dec.JE_DeclarantType);

				AssertCountrySpecificBehaviourAfterAssigningImporter(addInfo, dec);
			}
		}

		protected void AssertCountrySpecificBehaviourAfterAssigningImporter(GBOrgImpAddInfo addInfo, EU.Business.Declaration.JobDeclaration dec)
		{
			AssertEquals(addInfo.ZO_VATDeferType, dec.ZG_VATDeferType);
			AssertEquals(addInfo.ZO_Box44UseClientsEoriForDucrs, dec.UseClientEoriForDucr);

			string expectedClientRefForDucr = null;
			var ducrGenerationAttributeName = addInfo.ZO_Box44ClientsDucrSourceAttributeField;
			ZPropertyInfo sourceInfo = null;

			if (null != dec.DocsAndCartage)
			{
				if ("CA1".Equals(ducrGenerationAttributeName))
				{
					sourceInfo = dec.DocsAndCartage.JP_CustomAttrib1Info;
				}
				else if ("CA2".Equals(ducrGenerationAttributeName))
				{
					sourceInfo = dec.DocsAndCartage.JP_CustomAttrib2Info;
				}
			}

			if (sourceInfo != null && !sourceInfo.Value.IsEmpty)
			{
				expectedClientRefForDucr = (ZString)sourceInfo.Value;
			}

			AssertEquals(expectedClientRefForDucr, dec.ClientReferenceForDucr);

			string expectedOwnerRef = null;
			var declarantsRefName = addInfo.ZO_Box7DeclarantsReferenceSourceAttributeField;
			sourceInfo = null;

			if (null != dec.DocsAndCartage)
			{
				if ("CA1".Equals(declarantsRefName))
				{
					sourceInfo = dec.DocsAndCartage.JP_CustomAttrib1Info;
				}
				else if ("CA2".Equals(declarantsRefName))
				{
					sourceInfo = dec.DocsAndCartage.JP_CustomAttrib2Info;
				}

				if (sourceInfo != null && !sourceInfo.Value.IsEmpty)
				{
					expectedOwnerRef = (ZString)sourceInfo.Value;
				}
			}

			AssertEquals(expectedOwnerRef, dec.JE_OwnerRef);
		}

		public new void TestBox30LocationOfGoodsForDocumentsAndMessaging()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_LocationOfGoods = "";
			declaration.SubLocation = "";
			AssertEquals("", declaration.Box30LocationOfGoodsForDocumentsAndMessaging);

			declaration.JE_LocationOfGoods = "LHR";
			declaration.SubLocation = "";
			AssertEquals("GBLHR", declaration.Box30LocationOfGoodsForDocumentsAndMessaging);

			declaration.JE_LocationOfGoods = "LHR";
			declaration.SubLocation = "BAC";
			AssertEquals("GBLHRBAC", declaration.Box30LocationOfGoodsForDocumentsAndMessaging);
		}

		public void TestLocationOfGoodsForDocumentsAndMessaging()
		{
			EU.Business.Testing.ShedTest.CreateShed(Factory, "GB", "LHRABC", "BRITISH AIRWAYS at Heathrow", chiefPort: "CCC");
			EU.Business.Testing.ShedTest.CreateShed(Factory, "GB", "LHRDEF", "BRITISH AIRWAYS at Heathrow");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_LocationOfGoods = "LHR";
			declaration.JE_SubLocationOfGoods = "LHRABC";
			AssertEquals("CCC", declaration.LocationOfGoodsWithoutGbPrefix);
			AssertEquals("GBCCC", declaration.LocationOfGoodsForDocumentsAndMessaging);

			declaration.JE_SubLocationOfGoods = "LHRDEF";
			AssertEquals("LHR", declaration.LocationOfGoodsWithoutGbPrefix);
			AssertEquals("GBLHR", declaration.LocationOfGoodsForDocumentsAndMessaging);
		}

		public void TestShedPhysicalCodeFromDatabaseOrHeathrowERT()
		{
			EU.Business.Testing.ShedTest.CreateShed(Factory, "GB", "LHRABC", "BRITISH AIRWAYS at Heathrow", chiefShed: "CCC");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_LocationOfGoods = "LHR";
			declaration.SubLocation = "ABC";
			AssertEquals("CCC", declaration.ShedPhysicalCodeFromDatabaseOrHeathrowERT);

			declaration.SubLocation = "DEF";
			AssertEquals("DEF", declaration.ShedPhysicalCodeFromDatabaseOrHeathrowERT);
		}

		public void TestZG_VATDeferNumberNumberMaxLength()
		{
			var gbDeclaration = Factory.New<EU.Business.Declaration.JobDeclaration>();
			AssertEquals(7, gbDeclaration.GetPossiblyCustomPropertyMaxLength(EU.Business.Declaration.JobDeclaration.Schema.ZG_VATDeferNumber));

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				var lvDeclaration = Factory.New<EU.Business.Declaration.JobDeclaration>();
				AssertEquals(20, lvDeclaration.GetPossiblyCustomPropertyMaxLength(EU.Business.Declaration.JobDeclaration.Schema.ZG_VATDeferNumber));
			}
		}

		public void TestJE_Nch1RequestTypeIsReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_GBRouteOfEntry = "1";
			Assert(!declaration.JE_Nch1RequestTypeInfo.ReadOnly);
			declaration.JE_GBRouteOfEntry = "4";
			Assert(declaration.JE_Nch1RequestTypeInfo.ReadOnly);
			declaration.JE_GBRouteOfEntry = "2";
			Assert(!declaration.JE_Nch1RequestTypeInfo.ReadOnly);
			declaration.JE_GBRouteOfEntry = "3";
			Assert(!declaration.JE_Nch1RequestTypeInfo.ReadOnly);
			declaration.JE_GBRouteOfEntry = "4";
			Assert(declaration.JE_Nch1RequestTypeInfo.ReadOnly);
		}

		public void TestJE_Nch1PriorityIsReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_GBRouteOfEntry = "1";
			Assert(!declaration.JE_Nch1PriorityInfo.ReadOnly);
			declaration.JE_GBRouteOfEntry = "4";
			Assert(declaration.JE_Nch1PriorityInfo.ReadOnly);
			declaration.JE_GBRouteOfEntry = "2";
			Assert(!declaration.JE_Nch1PriorityInfo.ReadOnly);
			declaration.JE_GBRouteOfEntry = "3";
			Assert(!declaration.JE_Nch1PriorityInfo.ReadOnly);
			declaration.JE_GBRouteOfEntry = "4";
			Assert(declaration.JE_Nch1PriorityInfo.ReadOnly);
		}

		#region Implementation

		void RunMultiAssertDeclarationType(JobDeclaration dec, bool shouldBeTrue, params bool[] shouldBeFalses)
		{
			Assert("Declaration of type " + dec.JE_DeclarationType + " needs to have IsXXX set", shouldBeTrue);
			foreach (var oneFalseMove in shouldBeFalses)
			{
				Assert("Declaration of type " + dec.JE_DeclarationType + " needs to have all other IsXXX set to false", !oneFalseMove);
			}
		}

		public void TestZG_RX_NKVATAdj()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ZG_ManualCalc = true;
			AssertEquals(ZString.Empty, declaration.ZG_RX_NKVATAdj);
			declaration.ZG_VATAdjAmt = 123m;
			AssertEquals(CurrencyCodes.UnitedKingdom, declaration.ZG_RX_NKVATAdj);
			declaration.ZG_RX_NKVATAdj = CurrencyCodes.EuropeanUnion;
			declaration.ZG_VATAdjAmt = 456m;
			AssertEquals(CurrencyCodes.EuropeanUnion, declaration.ZG_RX_NKVATAdj);
		}

		public void TestChangeSubStyleFromNotArrivedToArrived()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_EntrySubStyle = "0";
			AssertEquals("Check '0' is not 'GoodsNotArrived'", false, declaration.IsGoodsNotArrivedSubStyle);

			declaration.ChangeSubStyleFromNotArrivedToArrived();
			AssertEquals("No Change expected", "0", declaration.JE_EntrySubStyle);

			declaration.JE_EntrySubStyle = EntrySubStyleListExport.Codes.C21_GoodsNotArrived_IECR;
			AssertEquals("Check 'K' is 'GoodsNotArrived'", true, declaration.IsGoodsNotArrivedSubStyle);

			declaration.ChangeSubStyleFromNotArrivedToArrived();
			AssertEquals("Change expected", EntrySubStyleListExport.Codes.C21_GoodsArrived_IECR, declaration.JE_EntrySubStyle);
		}

		public void TestAddInfoFields()
		{
			var dec = Factory.New<JobDeclaration>();
			var infoOriginalValue = (ZString)dec.JE_NorthernIrelandModeInfo.OriginalValue;
			AssertEquals("Should be the same", dec.JE_NorthernIrelandModeInfo.Value, infoOriginalValue);
			dec.JE_NorthernIrelandMode = "NII";
			Assert(!dec.JE_NorthernIrelandModeInfo.Value.Equals(infoOriginalValue));
			AssertEquals("NII", dec.JE_NorthernIrelandMode);

			var originalValue = (ZBool)dec.JE_ClaimEuSubsidyInfo.OriginalValue;
			AssertEquals("Should be the same", dec.JE_ClaimEuSubsidyInfo.Value, originalValue);
			dec.JE_ClaimEuSubsidy = true;
			Assert(!dec.JE_ClaimEuSubsidyInfo.Value.Equals(originalValue));
			Assert(dec.JE_ClaimEuSubsidy);

			dec = Factory.New<JobDeclaration>();
			dec.JE_NiGoodsAtRiskOfMovingToROI = true;
			Assert(dec.JE_NiGoodsAtRiskOfMovingToROI);

			dec = Factory.New<JobDeclaration>();
			dec.JE_AddInfo = "IsGvmsPort=true";
			Assert(dec.JE_IsGvmsPort);
		}

		class JobDeclarationForTest : JobDeclaration
		{
			public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			public ZString LocalCurrencyCodeCoreExposed
			{
				get { return LocalCurrencyCodeCore; }
			}

			internal Type CusEntryNumValidationForMUCRForTest
			{
				get { return base.CusEntryNumValidationForMUCRCore; }
			}

			public IValueSetStrategy GetSupportingDocumentValueSetStrategyExposed(EU.Business.Declaration.MultiLineAddInfos.SupportingDocument sd)
			{
				return base.GetSupportingDocumentValueSetStrategyCore(sd);
			}

			public IEnumerable<ZString> JobDeclarationMessageCollectionApplicationCodeListExposed
			{
				get { return JobDeclarationMessageCollectionApplicationCodeListCore; }
			}
		}

		#endregion

		#region TestMergeOperation_GB

		public override void TestMergeOperation()
		{
			Assert("Merge doesn't work when application code is empty. This will be tested in Chief.JobDeclarationTest and CDS.JobDeclarationTest.", true);
		}

		protected override string GetCountryCode_ToTestMergeOperation()
		{
			return Core.Constants.CountryCodes.UnitedKingdom;
		}

		protected override void CreateBadges_ToTestMergeOperation()
		{
			var badge1 = new BadgeCodeSetting();
			badge1.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
			badge1.BadgeCode = "BC1";

			var cred1 = new CredentialsSetting();
			cred1.BadgeCode = badge1.BadgeCode;
			cred1.PIMA = "CUKFFW98000MDK";
			cred1.Company = "BC1";

			var badge2 = new BadgeCodeSetting();
			badge2.CSPCode = GatewayList.Codes.CDS;
			badge2.BadgeCode = "BC2";

			var cred2 = new CredentialsSetting();
			cred2.BadgeCode = badge2.BadgeCode;
			cred2.PIMA = "AA";
			cred2.Company = GlbCompany.CurrentCompany.GC_Code;

			var badges = new BadgeCodeSettingCollection();
			badges.Add(badge1);
			badges.Add(badge2);

			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty,
															   GlbBranch.CurrentBranch.PK.ToGuid(),
															   Guid.Empty,
															   badges);

			var credentials = new CredentialsSettingCollection();
			credentials.Add(cred1);
			credentials.Add(cred2);

			GBCustomsDataRegistry.Instance.Credentials.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(),
																Guid.Empty,
																Guid.Empty,
																credentials);

			GBCustomsDataRegistry.Instance.CcsukLocalHostMnemonic.SetValue(Guid.Empty,
																		   Guid.Empty,
																		   Guid.Empty,
																		   "LOCALHOST");

			var extPwd = Factory.New<GlbExternalPassword_GB>();
			extPwd.GP_GC = GlbCompany.CurrentCompany.PK.ToGuid();
			extPwd.GP_UserID = "GBAA.BC2";
			extPwd.GP_PasswordType = "CDS";
			extPwd.GP_CurrentPassword = "NOWPWD";
			extPwd.GP_NextPassword = "NEXTPWD";
			extPwd.GP_IssueDate = ZDateTime.Today.AddDays(-3);
			extPwd.GP_ExpiryDate = ZDateTime.Today.AddDays(3);
			extPwd.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			Factory.Save();
		}

		protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo CreateAdditionalInfo(string code)
		{
			var addInfo = Factory.New<AdditionalInfo>();
			addInfo.CSI_Code = code;
			return addInfo;
		}

		protected override List<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo> GetAddInfoListFromEntryHeader(EU.Business.Declaration.CusEntryHeader entryHeader)
		{
			var list = new List<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>();
			var gbEntryHeader = entryHeader as CusEntryHeader;

			if (gbEntryHeader != null)
			{
				list.AddRange(gbEntryHeader.AdditionalInfos);
			}

			return list;
		}

		protected override List<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo> GetAddInfoListFromEntryLine(EU.Business.Declaration.CusEntryLine entryLine)
		{
			var list = new List<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>();
			var gbEntryLine = entryLine as CusEntryLine;

			if (gbEntryLine != null)
			{
				list.AddRange(gbEntryLine.AdditionalInfos);
			}

			return list;
		}

		#endregion  //End: "TestMergeOperation_GB"

		#region TestClassTypes_GB

		public override void TestClassTypesBeingUsed()
		{
			Assert("Merge doesn't work when application code is empty. This will be tested in Chief.JobDeclarationTest and CDS.JobDeclarationTest.", true);
		}

		protected override void InspectVariousObjects_ToTestClassTypes(EU.Business.Declaration.JobDeclaration dec)
		{
			const string GB = "GB";

			var invoice = dec.Invoices[0];
			var invLine = invoice.InvoiceLines[0];

			CheckClassType(GB, typeof(JobDeclaration), dec);
			CheckClassType(GB, typeof(JobComInvoiceHeader), invoice);
			CheckClassType(GB, typeof(JobComInvoiceLine), invLine);

			var entry = dec.CustomsEntryHeaders[0];
			var entryLine = entry.AllEntryLines[0];

			CheckClassType(GB, typeof(CusEntryHeader), entry);
			CheckClassType(GB, typeof(CusEntryLine), entryLine);

			var gbEntry = entry as CusEntryHeader;
			var gbEntryLine = entryLine as CusEntryLine;

			var list = new List<object>();

			list.AddRange(dec.AdditionalInfos);
			list.AddRange(invoice.AdditionalInfos);
			list.AddRange(invLine.AdditionalInfos);
			list.AddRange(gbEntry.AdditionalInfos);
			list.AddRange(gbEntryLine.AdditionalInfos);

			foreach (var obj in list)
			{
				CheckClassType(GB, typeof(AdditionalInfo), obj);
			}

			list.Clear();
			list.AddRange(dec.SupportingDocuments);
			list.AddRange(invoice.SupportingDocuments);
			list.AddRange(invLine.SupportingDocuments);
			list.AddRange(gbEntry.SupportingDocuments);
			list.AddRange(gbEntryLine.SupportingDocuments);

			foreach (var obj in list)
			{
				CheckClassType(GB, typeof(SupportingDocument), obj);
			}

			list.Clear();
			list.AddRange(dec.PreviousDocuments);
			list.AddRange(invoice.PreviousDocuments);
			list.AddRange(invLine.PreviousDocuments);
			list.AddRange(gbEntry.PreviousDocuments);
			list.AddRange(gbEntryLine.PreviousDocuments);

			foreach (var obj in list)
			{
				CheckClassType(GB, typeof(PreviousDocument), obj);
			}
		}

		protected override string GetCountryCode_ToTestClassTypes()
		{
			return Core.Constants.CountryCodes.UnitedKingdom;
		}

		#endregion  //End: "TestClassTypes_GB"

		public override void TestCustomsOfficeRequirementHelper()
		{
			Assert("This will be tested in Chief.JobDeclarationTest and CDS.JobDeclarationTest.", true);
		}

		public void TestMaxEntryLines()
		{
			var factory1 = new BusinessObjectFactory();
			var dec1 = factory1.New<JobDeclaration>();
			dec1.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;

			AssertEquals("Default: 99", 99, dec1.MaximumEntryLineCount);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionalityAttribute(Universal.Constants.FunctionalityTypes.MaxEntryLines, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MaxEntryLines, "555"))
			{
				var factory2 = new BusinessObjectFactory();
				var dec2 = factory2.New<JobDeclaration>();
				dec2.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
				AssertEquals("Step-up to 555", 555, dec2.MaximumEntryLineCount);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionalityAttribute(Universal.Constants.FunctionalityTypes.MaxEntryLines, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MaxEntryLines, "999"))
			{
				var factory3 = new BusinessObjectFactory();
				var dec3 = factory3.New<JobDeclaration>();
				dec3.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
				AssertEquals("Step-up to 999", 999, dec3.MaximumEntryLineCount);
			}
		}

		public override void TestGetJobComInvoiceLineCalculatorType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var invoiceLineCalculator = declaration.GetJobComInvoiceLineCalculator(invoiceLine);
			AssertNotNull(invoiceLineCalculator);
			AssertType<JobComInvoiceLineValueCalculator>(invoiceLineCalculator);
		}

		public void TestGetReasonForNotAbleToUpdateCore()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			var entry1 = dec.CustomsEntryHeaders.AddNew();
			entry1.CH_BGMReference = "1";
			var entry2 = dec.CustomsEntryHeaders.AddNew();
			entry2.CH_BGMReference = "2";
			entry2.CH_EntryStatus = EntryStatusList.Codes.NotSent;

			entry1.CH_EntryStatus = EntryStatusList.Codes.NotSent;
			AssertEquals(string.Empty, dec.GetReasonForNotAbleToUpdate());

			entry1.CH_EntryStatus = EntryStatusList.Codes.Cancelled;
			AssertEquals(string.Empty, dec.GetReasonForNotAbleToUpdate());

			entry1.CH_EntryStatus = EDIMessageStatusList.Codes.Rejected;
			AssertEquals(string.Empty, dec.GetReasonForNotAbleToUpdate());

			entry1.CH_EntryStatus = EntryStatusList.Codes.Clear;
			AssertContains("Entry 1 is in status CLR and the import of USXML is disallowed", dec.GetReasonForNotAbleToUpdate());

			entry1.CH_EntryStatus = EntryStatusList.Codes.AwaitingResponse;
			AssertContains("Entry 1 is in status AWR and the import of USXML is disallowed", dec.GetReasonForNotAbleToUpdate());
			AssertContains("Declaration has at least one entry which is awaiting a response from customs", dec.GetReasonForNotAbleToUpdate());
		}

		public void TestSupportsBondedWarehousing()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "123";
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_OH_Importer = importer.PK;

			CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Assert(!declaration.SupportsBondedWarehousing);
			CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, false);
			Assert(!declaration.SupportsBondedWarehousing);
		}

		public void TestSupportMultipleWarehouseEntry()
		{
			var declaration = Factory.New<JobDeclaration>();

			CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Assert(declaration.SupportMultipleWarehouseEntry);
			CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, false);
			Assert(!declaration.SupportMultipleWarehouseEntry);
		}

		public void TestNoErrorWhenValueBuildUp()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.ZG_ManualCalc = false;
			declaration.ZG_OSAirTransportAmount = 1;
			declaration.ZG_RX_NKFrtChg = "1";
			declaration.ZG_RX_NKDisc = "1";
			declaration.ZG_DiscPerc = 1;
			declaration.ZG_InsAmt = 1;
			declaration.ZG_RX_NKIns = "1";
			declaration.ZG_OthChgAmt = 1;
			declaration.ZG_RX_NKOthChg = "1";
			declaration.ZG_VATAdjAmt = 1;
			declaration.ZG_RX_NKVATAdj = "1";
			declaration.ZG_ManualCalc = true;

			AssertNotContains("This declaration is meant to use the standard valuation method. This setter should not be invoked", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestGetCountryCodeForSupplementaryCodeProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "";
			AssertEquals("When ApplicationCode is not set", "GB", declaration.GetCountryCodeForSupplementaryCodeProvider());

			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			AssertEquals("When ApplicationCode is CHF", "GBCHF", declaration.GetCountryCodeForSupplementaryCodeProvider());

			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			AssertEquals("When ApplicationCode is CHF", "GBCDS", declaration.GetCountryCodeForSupplementaryCodeProvider());
		}

		public override void TestCustomsOfficeOfExit()
		{
			JobDeclaration decEcs = Factory.New<JobDeclaration>();
			decEcs.JE_MessageType = "EXP";
			AssertEquals("", decEcs.OfficeOfExit);
			decEcs.JE_CustomsOffice = "GB000001";
			AssertEquals("GB000001", decEcs.OfficeOfExit);

			decEcs.JE_MessageType = "1";
			decEcs.JE_ApplicationCode = "EMC";
			Assert(decEcs.IsEMCS);
		}

		[TestDate(2010, 08, 09)]
		public override void TestDateOfValuation()
		{
			JobDeclaration dec = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceline = invoice.InvoiceLines.AddNew();
			invoiceline.FillWithValidTestData();

			dec.JE_MessageType = "EXP";
			Assert(dec.EarliestCustomsEntryIssueDate.IsEmpty);
			Assert(dec.JE_EntryAuthorisationDate.IsEmpty);
			AssertEquals("ValuationDate is empty", dec.CachedTodaysDate, dec.DateOfValuation);

			dec.JE_EntryAuthorisationDate = new ZDate(2010, 06, 06);

			Assert(dec.EarliestCustomsEntryIssueDate.IsEmpty);
			AssertEquals(new ZDate(2010, 06, 06), dec.JE_EntryAuthorisationDate);
			AssertEquals("With ATD and ATA, ValuationDate is still today (EXP)", dec.JE_EntryAuthorisationDate, dec.DateOfValuation);

			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "120-123456A";
			var entryNumber = entryHeader.CusEntryNumber;
			entryNumber.CE_IssueDate = ZDateTime.BrettsBirthday;

			Assert(!dec.EarliestCustomsEntryIssueDate.IsEmpty);
			Assert(!dec.JE_EntryAuthorisationDate.IsEmpty);

			AssertEquals("With ATD and ATA, ValuationDate is still today (EXP)", dec.EarliestCustomsEntryIssueDate, dec.DateOfValuation);
		}

		public void TestGBTransportModeConverter()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<TransportModeTranslator>(declaration.TransportModeTranslator);
		}

		public void TestIsInventorySelectionEnabled()
		{
			CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			var warehouse = Factory.New<OrgHeader>();
			warehouse.CompanyData.OB_IMUsedBondedWhs = false;

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			entryInstruction.CEI_OA_Warehouse = warehouse.MainAddress.PK;
			AssertEquals("Inventory selection is disabled when there is no automated from warehouse.", false, declaration.IsInventorySelectionEnabled);

			warehouse.CompanyData.OB_IMUsedBondedWhs = true;
			AssertEquals("Inventory selection is disabled when there is any automated from warehouse.", true, declaration.IsInventorySelectionEnabled);

			CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);
			AssertEquals("Inventory selection is disabled when registry setting IsWarehouseEnabledForGB is false", false, declaration.IsInventorySelectionEnabled);
		}

		public void TestAfterUniversalCopy()
		{
			var declaration = Factory.New<JobDeclaration>();
			SetupUniversalCopyTestForChief(declaration);

			RunAndAssertAfterUniversalCopy(declaration);

			AssertEquals("Should only contain 1 entry instruction (IFD).", 1, declaration.CustomsEntryInstructions.Count);
			AssertEquals("Should only contain 1 entry instruction (IFD).", "IFD", declaration.CustomsEntryInstructions[0].CEI_Style);

			declaration = Factory.New<JobDeclaration>();
			SetupUniversalCopyTestForCDS(declaration);

			RunAndAssertAfterUniversalCopy(declaration);

			AssertEquals("Should contain 2 entry instructions.", 2, declaration.CustomsEntryInstructions.Count);
		}

		public void TestDiscPercentageWhilePerformingUniversalCopyOnNewDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JobComInvoiceGroupHeaders.RemoveAndDeleteAll();
			var copyTemplateTree = new CopyTemplateTree(typeof(JobDeclaration));
			var discPercNode = (PropertyCopyTemplateNode)((EntityCopyTemplateNode)copyTemplateTree.InnerNode).Nodes.FirstOrDefault(n => n.Name == JobDeclaration.Schema.ZG_DiscPerc);
			discPercNode.CopyMethod = CopyMethod.Copy;
			var copyManager = new BusinessObjectCopyManager(Factory);
			AssertNoExceptionThrown(() => copyManager.Copy(declaration, copyTemplateTree));
		}

		public void TestIsSendForeignEoriToCds()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.SendForeignEoriToCds, CountryCodes.UnitedKingdom, ZDate.Today, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				AssertEquals("SendForeignEoriToCds turned on", true, declaration.IsSendForeignEoriToCds);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.SendForeignEoriToCds, CountryCodes.UnitedKingdom, ZDate.Today, false))
			{
				var declaration = Factory.New<JobDeclaration>();
				AssertEquals("SendForeignEoriToCds turned off", false, declaration.IsSendForeignEoriToCds);
			}
		}

		public void TestDeclarationMessagesHaveBeenSent()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("Pre-requisite: IsDeclarationIntegrated must return false so DeclarationMessagesHaveBeenSentCore is called", false, declaration.IsDeclarationIntegrated);

			AssertEquals(false, declaration.DeclarationMessagesHaveBeenSent());

			var message1 = Factory.New<EDIMessage>();
			declaration.Messages.Add(message1);
			AssertEquals(true, declaration.DeclarationMessagesHaveBeenSent());

			message1.EM_Status = "DCD";
			AssertEquals(false, declaration.DeclarationMessagesHaveBeenSent());

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var message2 = Factory.New<EDIMessage>();
			entry.Messages.Add(message2);

			AssertEquals(true, declaration.DeclarationMessagesHaveBeenSent());

			message2.EM_Status = "DCD";
			AssertEquals(false, declaration.DeclarationMessagesHaveBeenSent());

			AssertEquals(false, declaration.DeclarationMessagesHaveBeenSent(true));
			AssertEquals(1, declaration.Messages.Count);
			AssertEquals(1, declaration.CustomsEntryHeaders[0].Messages.Count);
		}

		public void TestIsWritingOffQuantityForbiddenForH4()
		{
			AssertEquals("No FUNCS added so should be false", false, JobDeclaration.IsWritingOffQuantityForbiddenForH4);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.H4ForbidWritingOff, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, true))
			{
				AssertEquals("No FUNCS added so should be true", true, JobDeclaration.IsWritingOffQuantityForbiddenForH4);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.H4ForbidWritingOff, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, false))
			{
				AssertEquals("No FUNCS added so should be false", false, JobDeclaration.IsWritingOffQuantityForbiddenForH4);
			}
		}

		protected override Type ExpectedDeclarationLevelPackageCollectionType => typeof(BaseDeclarationLevelPackageCollection<Package>);

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			return declaration;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			return declaration;
		}

		void RunAndAssertAfterUniversalCopy(JobDeclaration declaration)
		{
			var type = declaration.GetType();

			var extendedEntitiesAttribute = type
				.GetCustomAttributes<UniversalCopyWithExtendedEntitiesAttribute>(true)
				.First();

			var method = extendedEntitiesAttribute.FinishCopyMethod;
			AssertEquals("Precondition.", "AfterUniversalCopy", method);
			AssertNoExceptionThrown(() => type.InvokeMember(method, BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, declaration, null));
		}

		void SetupUniversalCopyTestForChief(JobDeclaration declaration)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "A", "11", "11", "111", "One", "IMP", group: "IFD");
			helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "B", "11", "11", "111", "One", "EXP", group: "EFD");

			declaration.JE_ApplicationCode = "CHF";
			declaration.JE_MessageType = "IMP";
			//declaration already has one "IFD" entry instruction by default
			var cei2 = declaration.CustomsEntryInstructions.AddNew();
			cei2.CEI_Style = "EFD";
		}

		void SetupUniversalCopyTestForCDS(JobDeclaration declaration)
		{
			declaration.JE_ApplicationCode = "CDS";
			declaration.JE_MessageType = "IMP";
			// declaration already has one "H1" entry instruction by default
			var cei1 = declaration.CustomsEntryInstructions.AddNew();
			cei1.CEI_Style = "B1";
		}

		#region Base tests
		public void TestJE_DeclarantType_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			AssertEquals("[14] Rep. Type", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_DeclarantTypeInfo, JobDeclaration.MultipleKeyChief).Caption);
			AssertEquals("[UCC 3/21] Rep. Type", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_DeclarantTypeInfo, JobDeclaration.MultipleKeyCdsImport).Caption);
		}

		public void TestJE_LocationOfGoods_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			AssertEquals("[30] Goods Location", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_LocationOfGoodsInfo, JobDeclaration.MultipleKeyChief).Caption);
			AssertEquals("[UCC 5/23] Location of Goods", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_LocationOfGoodsInfo, JobDeclaration.MultipleKeyCdsImport).Caption);
		}

		public void TestJE_OA_DeclarantAddress_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_OA_DeclarantAddressInfo, JobDeclaration.MultipleKeyChief);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "[14] Declarant", resourceStringDataAttribute.Caption);
				AssertEquals("ShortCaption", "[14] Declarant", resourceStringDataAttribute.ShortCaption);
				AssertEquals("MediumCaption", "[14] Declarant", resourceStringDataAttribute.MediumCaption);
				AssertEquals("FullDescription", "[14] Declarant. Name of the declarant controlling this declaration.", resourceStringDataAttribute.FullDescription);
			});
			resourceStringDataAttribute = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_OA_DeclarantAddressInfo, JobDeclaration.MultipleKeyCdsImport);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "[UCC 3/18] Declarant", resourceStringDataAttribute.Caption);
				AssertEquals("ShortCaption", "[UCC 3/18] Declarant", resourceStringDataAttribute.ShortCaption);
				AssertEquals("MediumCaption", "[UCC 3/18] Declarant", resourceStringDataAttribute.MediumCaption);
				AssertEquals("FullDescription", "[UCC 3/18] Declarant. Name of the declarant controlling this declaration.", resourceStringDataAttribute.FullDescription);
			});
		}

		public void TestJE_OA_Representative_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("Representative", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_OA_RepresentativeInfo, JobDeclaration.MultipleKeyChief).Caption);
			AssertEquals("[UCC 3/20] Representative", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_OA_RepresentativeInfo, JobDeclaration.MultipleKeyCdsImport).Caption);
		}

		public void TestJE_OA_ManufacturerAddress_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			AssertEquals("Manufacturer", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_OA_ManufacturerAddressInfo, JobDeclaration.MultipleKeyChief).Caption);
			AssertEquals("[UCC 3/37] Manufacturer", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_OA_ManufacturerAddressInfo, JobDeclaration.MultipleKeyCdsImport).Caption);
		}

		public void TestJE_OA_ShipperAddress_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			AssertEquals("[UCC 3/7] Consignor", DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_OA_ShipperAddressInfo).Caption);
		}

		public void TestJE_OwnerRef_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_OwnerRefInfo, JobDeclaration.MultipleKeyChief);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "[7] Declarant's Ref", resourceStringDataAttribute.Caption);
				AssertEquals("ShortCaption", "[7] Dec. Ref", resourceStringDataAttribute.ShortCaption);
			});
			resourceStringDataAttribute = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_OwnerRefInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Declarant\'s Reference", resourceStringDataAttribute.Caption);
				AssertEquals("ShortCaption", "Dec. Ref", resourceStringDataAttribute.ShortCaption);
				AssertEquals("FullDescription", "Please enter the Declarant\'s own reference for this consignment.", resourceStringDataAttribute.FullDescription);
			});
		}

		public void TestJE_RL_NKFinalDestination_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			AssertEquals("[17] Destination", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_RL_NKFinalDestinationInfo, JobDeclaration.MultipleKeyChief).Caption);
			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_RL_NKFinalDestinationInfo);
			AssertEquals("[UCC 5/8] Destination Port", resourceStringForCds.Caption);
			AssertEquals("Please enter the UNLOCO for the final Destination port where the goods will arrive.", resourceStringForCds.FullDescription);
		}

		public void TestJE_RN_NKTransportNationality_Caption()
		{
			var jobDeclaration = base.Factory.New<JobDeclaration>();
			AssertEquals("[21] Nationality", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_RN_NKTransportNationalityInfo, JobDeclaration.MultipleKeyChief).Caption);

			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_RN_NKTransportNationalityInfo);
			AssertEquals("[UCC 7/15] Nationality", resourceStringForCds.Caption);
			AssertEquals("[UCC 7/15] Nationality of active means of transport crossing the border", resourceStringForCds.FullDescription);
		}

		public void TestJE_IATALoadPort_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var chfResourceStringDataAttribute = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_IATALoadPortInfo, JobDeclaration.MultipleKeyChief);
			AssertEquals("IATA", chfResourceStringDataAttribute.Caption);
			CombineAssertions(() =>
			{
				var cdsResourceStringDataAttribute = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.JE_IATALoadPortInfo, JobDeclaration.MultipleKeyCdsImport);
				AssertEquals("[UCC 5/21] IATA", cdsResourceStringDataAttribute.Caption);
				AssertEquals("Caption", "[UCC 5/21] IATA", cdsResourceStringDataAttribute.Caption);
				AssertEquals("ShortCaption", "5/21", cdsResourceStringDataAttribute.ShortCaption);
				AssertEquals("MediumCaption", "[UCC 5/21]", cdsResourceStringDataAttribute.MediumCaption);
				AssertEquals("FullDescription", "[UCC 5/21] IATA Loading Location", cdsResourceStringDataAttribute.FullDescription);
			});
		}

		public void TestJE_ShipmentIncoTerm_Caption()
		{
			var jobDeclaration = base.Factory.New<JobDeclaration>();
			AssertEquals("[20.1] Incoterm", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_ShipmentIncoTermInfo, JobDeclaration.MultipleKeyChief).Caption);

			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_ShipmentIncoTermInfo);
			AssertEquals("[UCC 4/1] Delivery Terms - Incoterm", resourceStringForCds.Caption);
			AssertEquals("[UCC 4/1] Incoterm", resourceStringForCds.ShortCaption);
			AssertEquals("Incoterms are agreed between the supplier and buyer of the goods being moved. They set out which party is responsible for each part of the shipment.", resourceStringForCds.FullDescription);
		}

		public void TestJE_ShipmentIncoTermPlace_Caption()
		{
			var jobDeclaration = base.Factory.New<JobDeclaration>();
			AssertEquals("[20.2] Place", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_ShipmentIncoTermPlaceInfo, JobDeclaration.MultipleKeyChief).Caption);

			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_ShipmentIncoTermPlaceInfo);
			AssertEquals("[UCC 4/1] Delivery Terms - Place", resourceStringForCds.Caption);
			AssertEquals("[UCC 4/1] Place", resourceStringForCds.ShortCaption);
			AssertEquals("The Place up to which the Incoterms apply forms part of the Delivery Term. Please enter a UNLOCO or Country Code & Location Name", resourceStringForCds.FullDescription);
		}

		public override void TestJE_TransportMode_Caption()
		{
			var jobDeclaration = base.Factory.New<JobDeclaration>();
			AssertEquals("[25] Transport", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_TransportModeInfo, JobDeclaration.MultipleKeyChief).Caption);

			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_TransportModeInfo);

			AssertEquals("[UCC 7/4] Transp.", resourceStringForCds.ShortCaption);
			AssertEquals("[UCC 7/4] Transp.", resourceStringForCds.MediumCaption);
			AssertEquals("[UCC 7/4] Transport", resourceStringForCds.Caption);
			AssertEquals("The Mode of Transport at the Border describes how the goods will cross the border.", resourceStringForCds.FullDescription);
		}

		public void TestSupplierDocumentaryAddress_Caption()
		{
			var jobDeclaration = base.Factory.New<JobDeclaration>();
			AssertEquals("[2] Supplier", DataBoundResourceStrings.GetDataForProperty(typeof(JobDeclaration), nameof(jobDeclaration.SupplierDocumentaryAddress), [JobDeclaration.MultipleKeyChief]).Caption);

			var resourceStringForCdsExport = DataBoundResourceStrings.GetDataForProperty(typeof(JobDeclaration), nameof(jobDeclaration.SupplierDocumentaryAddress), [JobDeclaration.MultipleKeyCdsExport]);
			var resourceStringForCdsImport = DataBoundResourceStrings.GetDataForProperty(typeof(JobDeclaration), nameof(jobDeclaration.SupplierDocumentaryAddress), [JobDeclaration.MultipleKeyCdsImport]);
			var resourceStringForCdsMisc = DataBoundResourceStrings.GetDataForProperty(typeof(JobDeclaration), nameof(jobDeclaration.SupplierDocumentaryAddress), [JobDeclaration.MultipleKeyCdsMisc]);

			AssertEquals("CDS EXP", "[UCC 3/1] Exporter", resourceStringForCdsExport.Caption);
			AssertEquals("CDS EXP", "Enter details of the Exporter. Typically, this is the entity who has the power to determine and has determined that the goods are to be taken out of that customs territory.", resourceStringForCdsExport.FullDescription);
			AssertEquals("CDS IMP", "[UCC 3/1] Exporter", resourceStringForCdsImport.Caption);
			AssertEquals("CDS IMP", "Enter details of the Exporter. Typically, this is the entity that is the last seller of the goods prior to crossing the border.", resourceStringForCdsImport.FullDescription);
			AssertEquals("CDS MSC", "[UCC 3/1] Exporter", resourceStringForCdsMisc.Caption);
			AssertEquals("CDS MSC", "Enter details of the Exporter. Typically, this is the entity that is the last seller of the goods prior to crossing the border.", resourceStringForCdsMisc.FullDescription);
		}

		public void TestImporterDocumentaryAddress_Caption()
		{
			var jobDeclaration = base.Factory.New<JobDeclaration>();
			AssertEquals("[8] Importer", DataBoundResourceStrings.GetDataForProperty(typeof(JobDeclaration), nameof(jobDeclaration.ImporterDocumentaryAddress), [JobDeclaration.MultipleKeyChief]).Caption);

			var resourceStringForCdsExport = DataBoundResourceStrings.GetDataForProperty(typeof(JobDeclaration), nameof(jobDeclaration.ImporterDocumentaryAddress), [JobDeclaration.MultipleKeyCdsExport]);
			var resourceStringForCdsImport = DataBoundResourceStrings.GetDataForProperty(typeof(JobDeclaration), nameof(jobDeclaration.ImporterDocumentaryAddress), [JobDeclaration.MultipleKeyCdsImport]);
			var resourceStringForCdsMisc = DataBoundResourceStrings.GetDataForProperty(typeof(JobDeclaration), nameof(jobDeclaration.ImporterDocumentaryAddress), [JobDeclaration.MultipleKeyCdsMisc]);

			AssertEquals("CDS EXP", "[UCC 3/9] Consignee", resourceStringForCdsExport.Caption);
			AssertEquals("CDS EXP", "The Consignee is the party to whom the goods are consigned / shipped in the third country.", resourceStringForCdsExport.FullDescription);
			AssertEquals("CDS IMP", "[UCC 3/15] Importer", resourceStringForCdsImport.Caption);
			AssertEquals("CDS IMP", "Enter details of the Importer. Typically, this is the first buyer of the goods in the union, but please refer to HMRC guidance when declaring the Importer.", resourceStringForCdsImport.FullDescription);
			AssertEquals("CDS MSC", "Importer / Consignee", resourceStringForCdsMisc.Caption);
			AssertEquals("CDS MSC", "This is the party to whom the goods are consigned / shipped to.", resourceStringForCdsMisc.FullDescription);
		}

		public void TestJE_MessageType_Caption()
		{
			var jobDeclaration = base.Factory.New<JobDeclaration>();
			AssertEquals("Entry Type", DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_MessageTypeInfo).Caption);

			var resourceStringForCdsExport = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_MessageTypeInfo, JobDeclaration.MultipleKeyCdsExport);
			var resourceStringForCdsImport = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_MessageTypeInfo, JobDeclaration.MultipleKeyCdsImport);
			var resourceStringForCdsMisc = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_MessageTypeInfo, JobDeclaration.MultipleKeyCdsMisc);

			AssertEquals("CDS Export", "Entry Type", resourceStringForCdsExport.Caption);
			AssertEquals("CDS Export", "The Entry Type describes the flux/direction of the goods concerned.", resourceStringForCdsExport.FullDescription);
			AssertEquals("CDS Import", "Entry Type", resourceStringForCdsImport.Caption);
			AssertEquals("CDS Import", "The Entry Type describes the flux/direction of the goods concerned.", resourceStringForCdsImport.FullDescription);
			AssertEquals("CDS Misc", "Entry Type", resourceStringForCdsMisc.Caption);
			AssertEquals("CDS Misc", "The Entry Type describes the flux/direction of the goods concerned.", resourceStringForCdsMisc.FullDescription);
		}

		public void TestJE_EntryStyle_Caption()
		{
			var jobDeclaration = base.Factory.New<JobDeclaration>();
			AssertEquals("[1a] Entry Style", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_EntryStyleInfo, JobDeclaration.MultipleKeyChief).Caption);

			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_EntryStyleInfo);
			AssertEquals("[UCC 1/1] Dec. Type", resourceStringForCds.Caption);
			AssertEquals("The Declaration Type determines if goods are for Rest of World or for/from a special territory of the UK.", resourceStringForCds.FullDescription);
		}

		public void TestJE_RS_NKServiceLevel_Caption()
		{
			var jobDeclaration = base.Factory.New<JobDeclaration>();

			var resourceStringForChief = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_RS_NKServiceLevelInfo, JobDeclaration.MultipleKeyChief);
			AssertEquals("Service", resourceStringForChief.Caption);
			AssertEquals("Select the Service Level for this Declaration.", resourceStringForChief.FullDescription);

			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_RS_NKServiceLevelInfo);
			AssertEquals("Service Level", resourceStringForCds.Caption);
			AssertEquals("The Service Level for this declaration describes the quality/speed of service agreed to with the customer.", resourceStringForCds.FullDescription);
		}

		public void TestZG_SpecificCircumstanceIndicator_Caption()
		{
			var jobDeclaration = base.Factory.New<JobDeclaration>();

			AssertEquals("Circumstance", DataBoundResourceStrings.GetDataForProperty(jobDeclaration.ZG_SpecificCircumstanceIndicatorInfo).Caption);

			var resourceStringForCdsExport = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.ZG_SpecificCircumstanceIndicatorInfo, JobDeclaration.MultipleKeyCdsExport);
			AssertEquals("[UCC 1/7] Circumstance", resourceStringForCdsExport.Caption);
			AssertEquals("[UCC 1/7] Specific Circumstance Indicator allows you to indicate if this is an Express Consignment.", resourceStringForCdsExport.FullDescription);
		}

		public void TestDeclarationNumber_Caption()
		{
			var jobDeclaration = base.Factory.New<JobDeclaration>();
			AssertEquals("Entry Number", DataBoundResourceStrings.GetDataForProperty(typeof(JobDeclaration), nameof(jobDeclaration.DeclarationNumber), [JobDeclaration.MultipleKeyChief]).Caption);

			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(typeof(JobDeclaration), nameof(jobDeclaration.DeclarationNumber), []);
			AssertEquals("MRN", resourceStringForCds.Caption);
			AssertEquals("The MRN (Movement Reference Number) is a unique reference assigned by customs to this declaration.", resourceStringForCds.FullDescription);
		}

		public void TestEarliestCustomsEntryIssueDate_Caption()
		{
			var jobDeclaration = base.Factory.New<JobDeclaration>();
			AssertEquals("", DataBoundResourceStrings.GetDataForProperty(typeof(JobDeclaration), nameof(jobDeclaration.EarliestCustomsEntryIssueDate), [JobDeclaration.MultipleKeyChief])?.Caption ?? "");

			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(typeof(JobDeclaration), nameof(jobDeclaration.EarliestCustomsEntryIssueDate), []);
			AssertEquals("Earliest Customs Entry Issue Date", resourceStringForCds.Caption);
			AssertEquals("This box shows the issue date for this declaration. i.e. when the MRN was assigned by customs.", resourceStringForCds.FullDescription);
		}

		public void TestZG_StyleOfEntrySOE_Caption()
		{
			var jobDeclaration = base.Factory.New<JobDeclaration>();

			AssertEquals("SOE", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.ZG_StyleOfEntrySOEInfo, JobDeclaration.MultipleKeyChief).Caption);
			AssertEquals("Topic", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.ZG_StyleOfEntrySOEInfo, JobDeclaration.MultipleKeyCdsImport).Caption);

			var resourceStringForCdsExportMisc = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.ZG_StyleOfEntrySOEInfo);
			AssertEquals("Top.", resourceStringForCdsExportMisc.ShortCaption);
			AssertEquals("Topic", resourceStringForCdsExportMisc.Caption);
			AssertEquals("The Topic gives the Status of Entry (SOE) code for the export declaration.", resourceStringForCdsExportMisc.FullDescription);
		}

		public void TestJE_EntryStatusDescription_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			AssertEquals("Status", DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_EntryStatusDescriptionInfo).Caption);

			var resourceStringForCdsExport = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_EntryStatusDescriptionInfo, JobDeclaration.MultipleKeyCdsExport);
			var resourceStringForCdsImport = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_EntryStatusDescriptionInfo, JobDeclaration.MultipleKeyCdsImport);
			var resourceStringForCdsMisc = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_EntryStatusDescriptionInfo, JobDeclaration.MultipleKeyCdsMisc);

			AssertEquals("CDS Export", "Status", resourceStringForCdsExport.Caption);
			AssertEquals("CDS Export", "The Status field shows the most recent status message from customs as seen on the Entries tab.", resourceStringForCdsExport.FullDescription);
			AssertEquals("CDS Import", "Status", resourceStringForCdsImport.Caption);
			AssertEquals("CDS Import", "The Status field shows the most recent status message from customs as seen on the Entries tab.", resourceStringForCdsImport.FullDescription);
			AssertEquals("CDS Misc", "Status", resourceStringForCdsMisc.Caption);
			AssertEquals("CDS Misc", "The Status field shows the most recent status message from customs as seen on the Entries tab.", resourceStringForCdsMisc.FullDescription);
		}

		public void TestZG_ImportClearanceStatusICS_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			AssertEquals("ICS", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.ZG_ImportClearanceStatusICSInfo, JobDeclaration.MultipleKeyChief).Caption);

			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.ZG_ImportClearanceStatusICSInfo);
			AssertEquals("Customs Position Reason", resourceStringForCds.Caption);
			AssertEquals("The Customs Position Reason gives the current status code for the declaration or the reason for rejection.  It will only be set when a query message is sent to CDS and the response processed.", resourceStringForCds.FullDescription);
		}

		public void TestJE_GBRouteOfEntry_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			AssertEquals("Route", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_GBRouteOfEntryInfo, JobDeclaration.MultipleKeyChief).Caption);

			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_GBRouteOfEntryInfo);
			AssertEquals("Route of Examination", resourceStringForCds.Caption);
			AssertEquals("The Route of Entry code determines how the goods will be cleared through customs and what checks will need to be performed. It will only be set when a query message is sent to CDS and the response processed.", resourceStringForCds.FullDescription);
		}

		public void TestJE_MasterBill_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();

			var resourceStringForOthers = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_MasterBillInfo);
			AssertEquals("Master Bill", resourceStringForOthers.Caption);
			AssertEquals("Master Bill of the consignment.", resourceStringForOthers.FullDescription);

			jobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			jobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(jobDeclaration.JE_MasterBillInfo, JobDeclaration.MultipleKeyCdsExport, dataBoundBusinessObject: new DataBoundBusinessObject(jobDeclaration),
				caption: "Master Bill of Lading",
				shortCaption: "Master Bill",
				fullDescription: "The Master Bill of Lading is usually issued by the carrier. It serves as a contract of carriage between shipper and carrier.");

			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(jobDeclaration.JE_MasterBillInfo, JobDeclaration.MultipleKeyCdsImport, dataBoundBusinessObject: new DataBoundBusinessObject(jobDeclaration),
				caption: "Master Bill of Lading",
				shortCaption: "Master Bill",
				fullDescription: "The Master Bill of Lading is usually issued by the carrier. It serves as a contract of carriage between shipper and carrier.");

			jobDeclaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(jobDeclaration.JE_MasterBillInfo, JobDeclaration.MultipleKeyCdsMisc, dataBoundBusinessObject: new DataBoundBusinessObject(jobDeclaration),
				caption: "Master Bill of Lading",
				shortCaption: "Master Bill",
				fullDescription: "The Master Bill of Lading is usually issued by the carrier. It serves as a contract of carriage between shipper and carrier.");

			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			jobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(jobDeclaration.JE_MasterBillInfo, JobDeclaration.MultipleKeyCdsExport, dataBoundBusinessObject: new DataBoundBusinessObject(jobDeclaration),
				caption: "Master Bill of Lading",
				shortCaption: "Ocean Bill",
				fullDescription: "The Master Bill of Lading is usually issued by the carrier. It serves as a contract of carriage between shipper and carrier.");

			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(jobDeclaration.JE_MasterBillInfo, JobDeclaration.MultipleKeyCdsExport, dataBoundBusinessObject: new DataBoundBusinessObject(jobDeclaration),
				caption: "Master Bill of Lading",
				shortCaption: "Ocean Bill",
				fullDescription: "The Master Bill of Lading is usually issued by the carrier. It serves as a contract of carriage between shipper and carrier.");

			jobDeclaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(jobDeclaration.JE_MasterBillInfo, JobDeclaration.MultipleKeyCdsExport, dataBoundBusinessObject: new DataBoundBusinessObject(jobDeclaration),
				caption: "Master Bill of Lading",
				shortCaption: "Ocean Bill",
				fullDescription: "The Master Bill of Lading is usually issued by the carrier. It serves as a contract of carriage between shipper and carrier.");
		}

		public override void TestHouseBillLabel()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var resourceStringForChief = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_HouseBillInfo, JobDeclaration.MultipleKeyChief);
			AssertEquals("House Bill", resourceStringForChief.Caption);
			AssertEquals("House", resourceStringForChief.ShortCaption);
			AssertEquals("House Bill of shipment", resourceStringForChief.FullDescription);

			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_HouseBillInfo);
			AssertEquals("House Bill of Lading", resourceStringForCds.Caption);
			AssertEquals("House Bill", resourceStringForCds.ShortCaption);
			AssertEquals("The House Bill of Lading is usually issued by the Freight Forwarder. It serves as a contract of carriage between them and the shipper or consignee.", resourceStringForCds.FullDescription);
		}

		public void TestZG_Box18TransportID_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			AssertEquals("[18] Transport ID (inland)", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.ZG_Box18TransportIDInfo, JobDeclaration.MultipleKeyChief).Caption);
			AssertEquals("[UCC 7/9] Transport ID", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.ZG_Box18TransportIDInfo, JobDeclaration.MultipleKeyCdsMisc).Caption);

			var reurceStringForCdsExport = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.ZG_Box18TransportIDInfo, JobDeclaration.MultipleKeyCdsExport);
			AssertEquals("[UCC 7/7] Identity of the means of transport at departure", reurceStringForCdsExport.Caption);
			AssertEquals("[UCC 7/7] Transport ID", reurceStringForCdsExport.ShortCaption);
			AssertEquals("Enter the identity of the means of transport on which the goods are directly loaded at the time of export.", reurceStringForCdsExport.FullDescription);

			var resourceStringForCdsImport = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.ZG_Box18TransportIDInfo, JobDeclaration.MultipleKeyCdsImport);
			AssertEquals("[UCC 7/9] Identity of Means of Transport on Arrival", resourceStringForCdsImport.Caption);
			AssertEquals("[UCC 7/9] Transport ID", resourceStringForCdsImport.ShortCaption);
			AssertEquals("Enter the identity of the means of transport at the point when the goods are presented and the customs formalities for their release are to be completed.", resourceStringForCdsImport.FullDescription);
		}

		public void TestJE_VesselName_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			AssertEquals("[21] Vessel", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_VesselNameInfo, JobDeclaration.MultipleKeyChief).Caption);
			AssertEquals("[UCC 7/9] Vessel", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_VesselNameInfo, JobDeclaration.MultipleKeyCdsMisc).Caption);

			var reurceStringForCdsExport = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_VesselNameInfo, JobDeclaration.MultipleKeyCdsExport);
			AssertEquals("[UCC 7/7] Identity of the means of transport at departure - Vessel", reurceStringForCdsExport.Caption);
			AssertEquals("[UCC 7/7] Vessel", reurceStringForCdsExport.ShortCaption);
			AssertEquals("Enter the identity of the vessel which the goods are directly loaded at the time of export.", reurceStringForCdsExport.FullDescription);

			var resourceStringForCdsImport = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_VesselNameInfo, JobDeclaration.MultipleKeyCdsImport);
			AssertEquals("[UCC 7/9] Identity of Means of Transport on Arrival", resourceStringForCdsImport.Caption);
			AssertEquals("[UCC 7/9] Vessel", resourceStringForCdsImport.ShortCaption);
			AssertEquals("Enter the identity of the vessel at the point when the goods are presented and the customs formalities for their release are to be completed.", resourceStringForCdsImport.FullDescription);
		}

		public void TestJE_TransportModeInland_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			AssertEquals("[26] Inland M.O.T", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_TransportModeInlandInfo, JobDeclaration.MultipleKeyChief).Caption);

			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_TransportModeInlandInfo);
			AssertEquals("[UCC 7/5] Inland Mode of Transport", resourceStringForCds.Caption);
			AssertEquals("[UCC 7/5] Inland M.O.T", resourceStringForCds.ShortCaption);
			AssertEquals("", resourceStringForCds.FullDescription);
		}

		public void TestJE_RL_NKPortOfLoading_Caption()
		{
			var jobDeclaration = base.Factory.New<JobDeclaration>();
			AssertEquals("[27] Load Port", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_RL_NKPortOfLoadingInfo, JobDeclaration.MultipleKeyChief).Caption);

			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_RL_NKPortOfLoadingInfo);
			AssertEquals("Load Port", resourceStringForCds.Caption);
			AssertEquals("Please enter the UNLOCO for the port of the goods are dispatched from.", resourceStringForCds.FullDescription);
		}

		public void TestJE_ExportDate_Caption()
		{
			var jobDeclaration = base.Factory.New<JobDeclaration>();
			AssertEquals("Dep.", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_ExportDateInfo, JobDeclaration.MultipleKeyChief).Caption);

			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_ExportDateInfo);
			AssertEquals("Departure Date", resourceStringForCds.Caption);
			AssertEquals("Dep.", resourceStringForCds.ShortCaption);
			AssertEquals("The Departure Date is the date the goods are due to leave the Load Port.", resourceStringForCds.FullDescription);
		}

		public void TestJE_RL_NKPortOfArrival_Caption()
		{
			var jobDeclaration = base.Factory.New<JobDeclaration>();
			var resourceStringForChief = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_RL_NKPortOfArrivalInfo, JobDeclaration.MultipleKeyChief);
			AssertEquals("Discharge Port", resourceStringForChief.Caption);
			AssertEquals("Discharge", resourceStringForChief.ShortCaption);

			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_RL_NKPortOfArrivalInfo);
			AssertEquals("Discharge Port", resourceStringForCds.Caption);
			AssertEquals("Discharge", resourceStringForCds.ShortCaption);
			AssertEquals("Please enter the UNLOCO for the Destination port where the goods will arrive into.", resourceStringForCds.FullDescription);
		}

		public override void TestJE_DateOfArrivalCaption()
		{
			var jobDeclaration = base.Factory.New<JobDeclaration>();
			var resourceStringForChief = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_DateOfArrivalInfo, JobDeclaration.MultipleKeyChief);
			AssertEquals("Date of Arrival", resourceStringForChief.Caption);
			AssertEquals("Arr.", resourceStringForChief.ShortCaption);

			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_DateOfArrivalInfo);
			AssertEquals("Arrival Date", resourceStringForCds.Caption);
			AssertEquals("Arr.", resourceStringForCds.ShortCaption);
			AssertEquals("The Arrival Date is the date the goods are due to arrive at the Discharge Port.", resourceStringForCds.FullDescription);
		}

		public void TestJE_RL_NKOrigin_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			AssertEquals("[15] Origin", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_RL_NKOriginInfo, JobDeclaration.MultipleKeyChief).Caption);

			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_RL_NKOriginInfo);
			AssertEquals("[UCC 5/14] Origin Port", resourceStringForCds.Caption);
			AssertEquals("Please enter the UNLOCO for the port of the goods are dispatched from.", resourceStringForCds.FullDescription);
		}

		public void TestJE_GoodsOrigin_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var resourceStringForChief = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_GoodsOriginInfo, JobDeclaration.MultipleKeyChief);
			AssertEquals("[15] Country/Region of Dispatch", resourceStringForChief.Caption);
			AssertEquals("[15] Dispatch", resourceStringForChief.ShortCaption);
			AssertEquals("[15] Country/Region of Dispatch of the goods", resourceStringForChief.FullDescription);

			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_GoodsOriginInfo);
			AssertEquals("[UCC 5/14] Country of Dispatch/Export Code", resourceStringForCds.Caption);
			AssertEquals("This field shows the country code for where the goods are shipping from.", resourceStringForCds.FullDescription);
		}

		public void TestJE_DateAtOrigin_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var resourceStringForChief = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_DateAtOriginInfo, JobDeclaration.MultipleKeyChief);
			AssertEquals("Estimated Departure Date At Origin Port", resourceStringForChief.Caption);
			AssertEquals("ETD", resourceStringForChief.ShortCaption);
			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_DateAtOriginInfo);
			AssertEquals("Estimated Departure Date from Origin Port", resourceStringForCds.Caption);
			AssertEquals("Estimated Departure Date from Origin Port", resourceStringForCds.FullDescription);
		}

		public void TestJE_GoodsDestination_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			AssertEquals("Goods Destination", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_GoodsDestinationInfo, JobDeclaration.MultipleKeyChief).Caption);

			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_GoodsDestinationInfo);
			AssertEquals("[UCC 5/8] Country of Destination Code", resourceStringForCds.Caption);
			AssertEquals("This field shows the final destination country code where the goods will arrive.", resourceStringForCds.FullDescription);
		}

		public void TestJE_DateAtFinalDestination_Caption()
		{
			var jobDeclaration = base.Factory.New<JobDeclaration>();
			var resourceStringForChief = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_DateAtFinalDestinationInfo, JobDeclaration.MultipleKeyChief);
			AssertEquals("Estimated Time / Date of Arrival Date at the Final Destination Port", resourceStringForChief.Caption);
			AssertEquals("ETA", resourceStringForChief.ShortCaption);
			AssertEquals("Estimated Time / Date of Arrival at the Final Destination Port.", resourceStringForChief.FullDescription);

			AssertEquals("Estimated Date of Arrival at the Final Destination Port", DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_DateAtFinalDestinationInfo).Caption);
		}

		public void TestJE_Calc_LocationOtherInformationCountry_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_Calc_LocationOtherInformationCountryInfo);
			AssertEquals("Country/Region", resourceStringForCds.Caption);
			AssertEquals("C/R", resourceStringForCds.ShortCaption);
			AssertEquals("Ctry/Rgn.", resourceStringForCds.MediumCaption);
			AssertEquals("Country/Region of goods location for [UCC 5/23] Location of Goods.\nThis will allow customs to determine where a control will take place if required.", resourceStringForCds.FullDescription);
		}

		public void TestJE_Calc_LocationOtherInformationType_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_Calc_LocationOtherInformationTypeInfo);
			AssertEquals("Location Type", resourceStringForCds.Caption);
			AssertEquals("Type", resourceStringForCds.ShortCaption);
			AssertEquals("Location Type Code for [UCC 5/23] Location of Goods.\nThis will allow customs to determine where a control will take place if required.", resourceStringForCds.FullDescription);
		}

		public void TestJE_LocationQualifier_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_LocationQualifierInfo);
			AssertEquals("Qualifier", resourceStringForCds.Caption);
			AssertEquals("Qua.", resourceStringForCds.ShortCaption);
			AssertEquals("Qualifier for [UCC 5/23] Location of Goods.\nThis will allow customs to determine where a control will take place if required.", resourceStringForCds.FullDescription);
		}

		public void TestJE_GoodsLocation_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_GoodsLocationInfo);
			AssertEquals("Location", resourceStringForCds.Caption);
			AssertEquals("Loc.", resourceStringForCds.ShortCaption);
			AssertEquals("Goods Location for [UCC 5/23] Location of Goods.\nThis will allow customs to determine where a control will take place if required.", resourceStringForCds.FullDescription);
		}

		public void TestJE_SubLocationOfGoods_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_SubLocationOfGoodsInfo);
			AssertEquals("Sub-Location for [UCC 5/23] Location of Goods.", resourceStringForCds.Caption);
			AssertEquals("Sub.", resourceStringForCds.ShortCaption);
			AssertEquals("Sub-Location for [UCC 5/23] Location of Goods. For CCS-UK jobs, this will usually be the airport and shed code.\nThis will allow customs to determine where a control will take place if required.", resourceStringForCds.FullDescription);
		}

		public void TestJE_TotalNoOfPacks_Caption()	
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			AssertEquals("[6] No. Pkgs.", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_TotalNoOfPacksInfo, JobDeclaration.MultipleKeyChief).Caption);

			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_TotalNoOfPacksInfo);
			AssertEquals("Total number of Packages and Unit", resourceStringForCds.Caption);
			AssertEquals("No. Pkgs.", resourceStringForCds.ShortCaption);
			AssertEquals("Enter the number of packages and [UCC 6/9] Type of Packages making up the consignment covered by the declaration, based on the smallest external packing unit.", resourceStringForCds.FullDescription);
		}

		public void TestJE_TotalWeight_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var resourceStringForChief = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_TotalWeightInfo, JobDeclaration.MultipleKeyChief);
			AssertEquals("Total Weight", resourceStringForChief.Caption);
			AssertEquals("Weight", resourceStringForChief.ShortCaption);
			AssertEquals("Enter the total weight of this shipment", resourceStringForChief.FullDescription);

			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_TotalWeightInfo);
			AssertEquals("Total Weight and Unit", resourceStringForCds.Caption);
			AssertEquals("Total Weight", resourceStringForCds.ShortCaption);
			AssertEquals("Please enter the total weight and unit of measurement.", resourceStringForCds.FullDescription);
		}

		public void TestJE_TotalVolume_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var resourceStringForChief = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_TotalVolumeInfo, JobDeclaration.MultipleKeyChief);
			AssertEquals("Total Volume", resourceStringForChief.Caption);
			AssertEquals("Vol", resourceStringForChief.ShortCaption);
			AssertEquals("Volume", resourceStringForChief.MediumCaption);
			AssertEquals("Enter the total volume of this shipment", resourceStringForChief.FullDescription);
			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_TotalVolumeInfo);
			AssertEquals("Total Volume and Unit", resourceStringForCds.Caption);
			AssertEquals("Total Volume", resourceStringForCds.ShortCaption);
			AssertEquals("Please enter the total volume and unit of measurement.", resourceStringForCds.FullDescription);
		}

		public void TestZG_AgreedPlaceCode_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var resourceStringForChief = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.ZG_AgreedPlaceCodeInfo, JobDeclaration.MultipleKeyChief);
			AssertEquals("Incoterm Place Code", resourceStringForChief.Caption);
			AssertEquals("Inco. Place Code", resourceStringForChief.ShortCaption);
			AssertEquals("Inco. Place Code", resourceStringForChief.MediumCaption);
			AssertEquals("Incoterm Place Code: insert a Country (2 chars) or an UNLOCO (5 chars)", resourceStringForChief.FullDescription);
			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.ZG_AgreedPlaceCodeInfo);
			AssertEquals("Agreed Place Code", resourceStringForCds.Caption);
			AssertEquals("This field confirms the status of the goods when the responsibility is transferred from the buyer to the seller according to the Incoterm. i.e. whether the goods will remain in the country of departure, reach another union country or leave the union.", resourceStringForCds.FullDescription);
		}

		public void TestJE_AgentsReference_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var resourceStringForChief = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_AgentsReferenceInfo, JobDeclaration.MultipleKeyChief);
			AssertEquals("Agents Reference", resourceStringForChief.Caption);
			AssertEquals("Agents Ref.", resourceStringForChief.ShortCaption);
			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_AgentsReferenceInfo);
			AssertEquals("Agent's Reference", resourceStringForCds.Caption);
			AssertEquals("Agent's Ref.", resourceStringForCds.ShortCaption);
			AssertEquals("Please enter the Agent's own reference for this consignment.", resourceStringForCds.FullDescription);
		}

		public void TestJE_UCR_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var resourceStringForChief = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_UCRInfo, JobDeclaration.MultipleKeyChief);
			AssertEquals("DUCR", resourceStringForChief.Caption);
			AssertEquals("Declaration UCR", resourceStringForChief.FullDescription);
			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_UCRInfo);
			AssertEquals("[UCC 2/4] DUCR (Declaration Unique Consignment Reference)", resourceStringForCds.Caption);
			AssertEquals("[UCC 2/4] DUCR", resourceStringForCds.ShortCaption);
			AssertEquals("The DUCR (Declaration Unique Consignment Reference) is a unique identifier that customs use to track a consignment at different stages of a shipment.", resourceStringForCds.FullDescription);
		}

		public void TestJE_MasterUCR_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			AssertEquals("Master UCR", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_MasterUCRInfo, JobDeclaration.MultipleKeyChief).Caption);
			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_MasterUCRInfo);
			AssertEquals("Master UCR", resourceStringForCds.Caption);
			AssertEquals("A MUCR (Master Unique Consignment Reference) is normally used to associate or link several Declaration UCRs for export or to provide an inventory consignment reference (ICR) for imports.", resourceStringForCds.FullDescription);
		}

		public void TestJE_CustomsProfile_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			AssertEquals("Badge", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_CustomsProfileInfo, JobDeclaration.MultipleKeyChief).Caption);
			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_CustomsProfileInfo);
			AssertEquals("Profile", resourceStringForCds.Caption);
			AssertEquals("Select a profile to determine the appropriate service or badge for this declaration.", resourceStringForCds.FullDescription);
		}

		public void TestZG_Gateway_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			AssertEquals("Gateway", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.ZG_GatewayInfo, JobDeclaration.MultipleKeyChief).Caption);
			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.ZG_GatewayInfo);
			AssertEquals("Gateway", resourceStringForCds.Caption);
			AssertEquals("This field shows which Gateway or CSP (Community System Provider) will receive this declaration.", resourceStringForCds.FullDescription);
		}

		public void TestJE_ScreeningStatus_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var resourceStringForChief = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(jobDeclaration.JE_ScreeningStatusInfo, JobDeclaration.MultipleKeyChief);
			AssertEquals("Screening Status", resourceStringForChief.Caption);
			AssertEquals("Scrn.", resourceStringForChief.ShortCaption);
			AssertEquals("Screening", resourceStringForChief.MediumCaption);
			var resourceStringForCds = DataBoundResourceStrings.GetDataForProperty(jobDeclaration.JE_ScreeningStatusInfo);
			AssertEquals("Screening Status", resourceStringForCds.Caption);
			AssertEquals("Scrn.", resourceStringForCds.ShortCaption);
			AssertEquals("Screening", resourceStringForCds.MediumCaption);
			AssertEquals("CargoWise DPS (Denied Party Screening) Service can be used to prevent the submission of Standalone Customs declarations if any organizations used on the job have an Unknown or Matched status.", resourceStringForCds.FullDescription);
		}

		public void TestJE_CustomsOffice_Caption()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();

			jobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(jobDeclaration.JE_CustomsOfficeInfo, JobDeclaration.MultipleKeyChief, dataBoundBusinessObject: new DataBoundBusinessObject(jobDeclaration),
				caption: "[29] Office of Exit",
				fullDescription: "Customs Office");

			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(jobDeclaration.JE_CustomsOfficeInfo, JobDeclaration.MultipleKeyChief, dataBoundBusinessObject: new DataBoundBusinessObject(jobDeclaration),
				caption: "Office of Presentation",
				fullDescription: "Customs Office");

			jobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(jobDeclaration.JE_CustomsOfficeInfo, JobDeclaration.MultipleKeyCdsExport, dataBoundBusinessObject: new DataBoundBusinessObject(jobDeclaration),
				caption: "[UCC 5/12] Customs Office of Exit",
				shortCaption: "[UCC 5/12] Office of Exit",
				fullDescription: "This is the Customs Office of Exit responsible for dealing with this customs declaration.");

			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(jobDeclaration.JE_CustomsOfficeInfo, JobDeclaration.MultipleKeyCdsImport, dataBoundBusinessObject: new DataBoundBusinessObject(jobDeclaration),
				caption: "[UCC 5/26] Customs Office of Presentation",
				shortCaption: "[UCC 5/26] Office of Pres.",
				fullDescription: "This is the Customs Office of Presentation responsible for dealing with this customs declaration.");

			jobDeclaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(jobDeclaration.JE_CustomsOfficeInfo, JobDeclaration.MultipleKeyCdsMisc	, dataBoundBusinessObject: new DataBoundBusinessObject(jobDeclaration),
				caption: "Customs Office",
				fullDescription: "This is the Customs Office responsible for dealing with this customs declaration.");
		}
		#endregion
	}

	sealed class JobDeclarationBaseOnlyTest : TestCaseWithFactory
	{
		public void TestJE_ApplicationCodeIsReadOnlyWhenBuiltin_Import()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				var dec = Factory.New<JobDeclaration>();
				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				Assert("Application Code should be read only when builtin import", dec.JE_ApplicationCodeInfo.ReadOnly);
			}
		}

		public void TestJE_ApplicationCodeIsReadOnlyWhenBuiltin_Export()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				var dec = Factory.New<JobDeclaration>();
				dec.JE_MessageType = JobMessageTypeList.Codes.Export;
				Assert("Application Code should be read only when builtin Export", dec.JE_ApplicationCodeInfo.ReadOnly);
			}
		}
	}

	sealed class JobDeclarationSynchroniserTests : EU.Business.Declaration.Testing.JobDeclarationSynchroniserTest
	{
		public void TestHookConsolToDeclarationSynchronisers()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "GBLHR";
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			var houseShipment = consol.Shipments.AddNew();
			var dec = GetSynchronisedDec(houseShipment, MessageTypeList.Codes.Import);
			AssertEquals(ShipmentTypeList.Codes.HouseConsignment, dec.ZG_ShipmentType);

			var basic = Factory.New<ForwardingConsol>();
			basic.JK_AgentType = Core.Constants.AgentType.Direct;
			basic.JK_RL_NKLoadPort = "AUSYD";
			basic.JK_RL_NKDischargePort = "GBLHR";
			var directShipment = basic.Shipments.AddNew();
			var dec2 = GetSynchronisedDec(directShipment, MessageTypeList.Codes.Import);
			AssertEquals(ShipmentTypeList.Codes.BasicDirect, dec2.ZG_ShipmentType);
		}

		public void TestJE_LocationOtherInformationSynchroniser()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			var codelist = helper.CreateNewOrGetExistingCusCodeList(Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "MNCMANDHXCUK", "MNCMANDHXCUK", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeListAttribute(codelist.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CCSUK, "MANDHX");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codelist.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Facility, "AU");
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "GBLHR";
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			var houseShipment = consol.Shipments.AddNew();

			var mawb = Factory.New<Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB>();
			mawb.CM_ArrivalDate = new ZDateTime(1986, 3, 12);
			mawb.CM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_WarehouseLocation = "MANDHX";
			hawb.CS_JS = houseShipment.PK;
			hawb.CS_CustomsStatus = "CX";
			var dec = GetSynchronisedDec(houseShipment, MessageTypeList.Codes.Import);
			AssertEquals("GBAU  MNCMANDHXCUK", dec.JE_LocationOtherInformation);
		}

		public void TestHasPortInventoryAttribute()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "AR1", "MYPORT", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), GBCommonConstants.RefCusCodeListAttributeCodes.Inventory, "Inventory");
			Factory.Save();
			var dec = Factory.New<JobDeclaration>();
			dec.JE_CHIEF_GoodsLocation = "AR1";
			AssertEquals("Has Port Inventory Attribute", true, dec.HasPortInventoryAttribute);
		}

		protected override EU.Business.Declaration.JobDeclaration GetDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}
	}

	sealed class JobDeclarationAutoSendingMessageSupporterTest : Customs.Business.Testing.JobDeclarationMessageSupporterTest<JobDeclaration>
	{
		public override void TestIJobDeclarationMessageSupporterMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supporter = (IJobDeclarationAutoSendingMessageSupporter)declaration;
			Assert(supporter.SupportEntryDeclarationMessage);
			Assert(!supporter.SupportReleaseMessage);
			AssertNotNull(supporter.CreateEntryDeclarationMessageProcessor());
			Assert(supporter.CreateEntryDeclarationMessageProcessor() is GBAutoSendCustomsMessageProcessor);
		}
	}
}
