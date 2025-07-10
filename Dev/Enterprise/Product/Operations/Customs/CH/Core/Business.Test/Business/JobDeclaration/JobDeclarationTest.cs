using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Business.Testing;
using Enterprise.Customs.Common.CH;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(JobDeclaration))]
sealed class JobDeclarationTest : Customs.Business.Testing.BaseJobDeclarationTest<JobDeclaration>
{
	[ExpectNoExceptions]
	public void TestAllAddInfoColumnsAreInModelView()
	{
		ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(
			Factory.New<JobDeclaration>(),
			"CHJobDeclaration");
	}

	public void TestJE_DispatchCountryConfirmation()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertEquals("JE_DispatchCountryConfirmation Caption", "Confirm", DataBoundResourceStrings.GetDataForProperty(declaration.JE_DispatchCountryConfirmationInfo).Caption);
	}

	public void TestJE_VATPaidBy_Default()
	{
		var paidTestHelper = new PaidTestHelper(Factory);
		CombineAssertions(() =>
		{
			paidTestHelper.TestPaidByDefault(OrgCusCode.SwissCodeTypes.CAV, Core.Constants.IncoTerms.DeliveredDutyPaid, false, false, d => d.JE_VATPaidBy, DeclarationPayerList.Codes.Declarant);
			paidTestHelper.TestPaidByDefault(OrgCusCode.SwissCodeTypes.CAV, Core.Constants.IncoTerms.DeliveredDutyUnpaid, false, false, d => d.JE_VATPaidBy, DeclarationPayerList.Codes.Declarant);
			paidTestHelper.TestPaidByDefault(OrgCusCode.SwissCodeTypes.CAV, Core.Constants.IncoTerms.DeliveredDutyPaid, true, false, d => d.JE_VATPaidBy, DeclarationPayerList.Codes.Consignor);
			paidTestHelper.TestPaidByDefault(OrgCusCode.SwissCodeTypes.CAV, Core.Constants.IncoTerms.DeliveredDutyPaid, true, true, d => d.JE_VATPaidBy, DeclarationPayerList.Codes.Consignor);
			paidTestHelper.TestPaidByDefault(OrgCusCode.SwissCodeTypes.CAV, Core.Constants.IncoTerms.DeliveredDutyUnpaid, false, true, d => d.JE_VATPaidBy, DeclarationPayerList.Codes.Importer);
			paidTestHelper.TestPaidByDefault(OrgCusCode.SwissCodeTypes.CAV, Core.Constants.IncoTerms.DeliveredDutyUnpaid, true, true, d => d.JE_VATPaidBy, DeclarationPayerList.Codes.Importer);
		});
	}

	public void TestGetValidation()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertType<ImportJobDeclarationValidation>(Declaration.Validation);
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertType<ExportJobDeclarationValidation>(Declaration.Validation);
		Declaration.JE_MessageType = ZString.Empty;
		AssertType<JobDeclarationValidation>(Declaration.Validation);
	}

	public override void TestLocalCurrencyCoreOverride()
	{
		AssertEquals(Core.Constants.CurrencyCodes.Switzerland, Factory.New<JobDeclaration>().LocalCurrencyCode);
	}

	public override void TestAreMultipleEntryInstructionsAllowed() => CombineAssertions(() =>
	{
		var declaration = GetJobDeclaration();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("import allowed", true, declaration.AreMultipleEntryInstructionsAllowed);
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals("export allowed", true, declaration.AreMultipleEntryInstructionsAllowed);
		declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		AssertEquals("export declaration activation not allowed", false, declaration.AreMultipleEntryInstructionsAllowed);
	});

	public void TestCustomsEntryInstrucions()
	{
		var declaration = GetJobDeclaration();
		AssertType<CusEntryInstructionCollection>(declaration.CustomsEntryInstructions);
	}

	public override void TestIsDeclarationWithEntryInstruction()
	{
		Assert("CH Declaration should support EntryInstructions", !Declaration.CustomsEntryInstructionProvider.IsNoEntryInstruction);
	}

	public void TestHouseBillsCollectionIsOfRightType()
	{
		AssertType<BillCollection<Bill, JobDeclaration>>(Declaration.Bills);
	}

	public void TestLookupObjectIsCached()
	{
		var bizO = (JobDeclaration)GetNewBusinessObject();
		var firstLookup = bizO.Lookups;
		var secondLookup = bizO.Lookups;
		AssertEquals(secondLookup, firstLookup);
	}

	public void TestSupportedAddressTypesCoreOverride() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("JobDeclaration Import Number of SupportedAddressTypes", base.ExpectedDocAddressTypes.Count, ((IDocAddresses)Declaration).SupportedAddressTypes.Count);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals("JobDeclaration Export Number of SupportedAddressTypes", ExpectedDocAddressTypes.Count, ((IDocAddresses)Declaration).SupportedAddressTypes.Count);
		AssertCollectionContains("JobDeclaration Export SupportedAddressTypes contains ConsignorDocumentaryAddress", DocAddressType.ConsignorDocumentaryAddress, ((IDocAddresses)Declaration).SupportedAddressTypes);
	});

	protected override Hashtable ExpectedDocAddressTypes
	{
		get
		{
			if (fExpectedDocAddressTypes == null)
			{
				fExpectedDocAddressTypes = base.ExpectedDocAddressTypes;
				fExpectedDocAddressTypes.Add(AutoDocAddressTypes.Codes.ConsignorDocumentaryAddress, DocAddressType.ConsignorDocumentaryAddress);
			}

			return fExpectedDocAddressTypes;
		}
	}
	Hashtable fExpectedDocAddressTypes;

	public void TestJE_VehicleTypeInfo()
	{
		AssertEquals("MaxLength", 2, Declaration.JE_VehicleTypeInfo.MaxLength);
	}

	public void TestJE_VehicleTypeDefault()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		Declaration.JE_TransportMode = TransportTypeList.Codes.Road;
		AssertEquals("default", UniversalReferenceConstants.TransportationTypeCodes.Truck, Declaration.JE_VehicleType);
	}

	public void TestMessageStatusDescription() => AssertEquals("Caption", "Messaging Status", Factory.New<JobDeclaration>().JE_MessageStatusDescriptionInfo.Description);

	public void TestJE_MessageStatus_Export() => AssertJE_MessageStatus(JobMessageTypeList.Codes.Export);
	public void TestJE_MessageStatus_Import() => AssertJE_MessageStatus(JobMessageTypeList.Codes.Import);

	void AssertJE_MessageStatus(string messageType) => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = messageType;

		var entryStatusList = declaration.Lookups.EntryStatusList;

		AssertEquals("No EntryHeader", ZString.Empty, declaration.JE_MessageStatus);
		AssertEquals("No EntryHeader", ZString.Empty, declaration.JE_MessageStatusDescription);

		var entry1 = declaration.ActiveEntryHeaders.AddNew();
		AssertEquals("Single EntryHeader empty JE_MessageStatus", ZString.Empty, declaration.JE_MessageStatus);
		AssertEquals("Single EntryHeader empty JE_MessageStatus", ZString.Empty, declaration.JE_MessageStatusDescription);

		entry1.CH_Status = "XYZ";
		AssertEquals("Single EntryHeader with unknown Status", "XYZ", declaration.JE_MessageStatus);
		AssertEquals("Single EntryHeader with unknown Status", ZString.Empty, declaration.JE_MessageStatusDescription);

		entry1.CH_Status = CHLogicalStatusList.Codes.Accepted;
		AssertEquals("Single EntryHeader with known Status", CHLogicalStatusList.Codes.Accepted, declaration.JE_MessageStatus);
		AssertEquals("Single EntryHeader with known Status", CHLogicalStatusList.Descriptions.Accepted, declaration.JE_MessageStatusDescription);

		var entry2 = declaration.ActiveEntryHeaders.AddNew();
		entry2.CH_Status = CHLogicalStatusList.Codes.Accepted;
		AssertEquals("Multiple EntryHeaders, same value", CHLogicalStatusList.Codes.Accepted, declaration.JE_MessageStatus);
		AssertEquals("Multiple EntryHeaders, same value", CHLogicalStatusList.Descriptions.Accepted, declaration.JE_MessageStatusDescription);

		entry2.CH_Status = ZString.Empty;
		AssertEquals("Multiple Entry Headers with empty value", CommonEntryStatusList.Codes.MultipleEntryStatus, declaration.JE_MessageStatus);
		AssertEquals("Multiple Entry Headers with empty value", CommonEntryStatusList.Descriptions.MultipleEntryStatus, declaration.JE_MessageStatusDescription);

		entry2.CH_Status = CHLogicalStatusList.Codes.Failed;
		AssertEquals("Multiple Entry Headers with different values (not empty)", CommonEntryStatusList.Codes.MultipleEntryStatus, declaration.JE_MessageStatus);
		AssertEquals("Multiple Entry Headers with different values (not empty)", CommonEntryStatusList.Descriptions.MultipleEntryStatus, declaration.JE_MessageStatusDescription);

		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
		AssertEquals("Multiple EntryHeaders", CommonEntryStatusList.Codes.MultipleEntryStatus, declaration.JE_MessageStatus);
		AssertEquals("Multiple EntryHeaders", CommonEntryStatusList.Descriptions.MultipleEntryStatus, declaration.JE_MessageStatusDescription);
	});

	public void TestSelectionResultDescription() => AssertEquals("Caption", "Selection Result", Factory.New<JobDeclaration>().SelectionResultDescriptionInfo.Description);

	public void TestSelectionResult_Export() => AssertSelectionResult(JobMessageTypeList.Codes.Export);
	public void TestSelectionResult_Import() => AssertSelectionResult(JobMessageTypeList.Codes.Import);

	void AssertSelectionResult(string messageType) => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateSelectionResultCodeListAndFrenchLanguage(Factory);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = messageType;

		AssertEquals("SelectionResult: No EntryHeader", ZString.Empty, declaration.SelectionResult);
		AssertEquals("SelectionResultDescription: No EntryHeader", ZString.Empty, declaration.SelectionResultDescription);

		var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
		AssertEquals("SelectionResult: Single EntryHeader no MRN", ZString.Empty, declaration.SelectionResult);
		AssertEquals("SelectionResultDescription: Single EntryHeader no MRN", ZString.Empty, declaration.SelectionResultDescription);

		var mrn1 = AddNewMrnToEntryHeader(entryHeader1);
		AssertEquals("SelectionResult: Single EntryHeader with MRN and empty CE_EntryStatus", ZString.Empty, declaration.SelectionResult);
		AssertEquals("SelectionResultDescription: Single EntryHeader MRN and empty CE_EntryStatus", ZString.Empty, declaration.SelectionResultDescription);

		mrn1.CE_EntryStatus = "XXX";
		AssertEquals("SelectionResult: Single EntryHeader with MRN and unknown CE_EntryStatus", "XXX", declaration.SelectionResult);
		AssertEquals("SelectionResultDescription: Single EntryHeader with MRN and unknown CE_EntryStatus", ZString.Empty, declaration.SelectionResultDescription);

		mrn1.CE_EntryStatus = RefCusCodeTestHelper.EntryStatusCode;
		AssertEquals("SelectionResult: Single EntryHeader with MRN and CE_EntryStatus", RefCusCodeTestHelper.EntryStatusCode, declaration.SelectionResult);
		AssertEquals("SelectionResultDescription: Single EntryHeader with MRN and CE_EntryStatus", RefCusCodeTestHelper.EntryStatusDescription, declaration.SelectionResultDescription);

		var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
		AssertEquals("SelectionResult: Multiple EntryHeaders with different MRN CE_EntryStatus (no MRN)", CommonEntryStatusList.Codes.MultipleEntryStatus, declaration.SelectionResult);
		AssertEquals("SelectionResultDescription: Multiple EntryHeaders with different MRN CE_EntryStatus (no MRN)", CommonEntryStatusList.Descriptions.MultipleEntryStatus, declaration.SelectionResultDescription);

		var mrn2 = AddNewMrnToEntryHeader(entryHeader2);
		AssertEquals("SelectionResult: Multiple EntryHeaders with different MRN CE_EntryStatus (empty)", CommonEntryStatusList.Codes.MultipleEntryStatus, declaration.SelectionResult);
		AssertEquals("SelectionResultDescription: Multiple EntryHeaders with different MRN CE_EntryStatus (empty)", CommonEntryStatusList.Descriptions.MultipleEntryStatus, declaration.SelectionResultDescription);

		mrn2.CE_EntryStatus = "XXX";
		AssertEquals("SelectionResult: Multiple EntryHeaders with different MRN CE_EntryStatus (unknown)", CommonEntryStatusList.Codes.MultipleEntryStatus, declaration.SelectionResult);
		AssertEquals("SelectionResultDescription: Multiple EntryHeaders with different MRN CE_EntryStatus (unknown)", CommonEntryStatusList.Descriptions.MultipleEntryStatus, declaration.SelectionResultDescription);

		mrn2.CE_EntryStatus = RefCusCodeTestHelper.EntryStatusCode;
		AssertEquals("SelectionResult: Multiple EntryHeaders with same MRN CE_EntryStatus", RefCusCodeTestHelper.EntryStatusCode, declaration.SelectionResult);
		AssertEquals("SelectionResultDescription: Multiple EntryHeaders with same MRN CE_EntryStatus", RefCusCodeTestHelper.EntryStatusDescription, declaration.SelectionResultDescription);

		CusEntryNumber AddNewMrnToEntryHeader(CusEntryHeader entryHeader)
		{
			var cusEntryNumber = Factory.New<CusEntryNumber>();
			cusEntryNumber.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			cusEntryNumber.CE_ParentID = entryHeader.PK;
			cusEntryNumber.CE_ParentTable = entryHeader.TableName;
			cusEntryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			cusEntryNumber.CE_EntryNum = "123456";
			cusEntryNumber.CE_IssueDate = ZDateTime.Today;
			return cusEntryNumber;
		}
	});

	public void TestJE_EntryStatus() => CombineAssertions(() =>
	{
		AssertEntryStatus(JobMessageTypeList.Codes.Export, DeclarationApplicationCodeList.Codes.Builtin);
		AssertEntryStatus(JobMessageTypeList.Codes.Export, DeclarationApplicationCodeList.Codes.Interfaced);
		AssertEntryStatus(JobMessageTypeList.Codes.Import, DeclarationApplicationCodeList.Codes.Builtin);
		AssertEntryStatus(JobMessageTypeList.Codes.Import, DeclarationApplicationCodeList.Codes.Interfaced);

		void AssertEntryStatus(string messageType, string submissionType)
		{
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new LocalCountryCustomsInterface { SubmissionType = submissionType }))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = messageType;

				var entryStatusList = declaration.Lookups.EntryStatusList;

				AssertEquals(AssertionMessage("No EntryHeader"), ZString.Empty, declaration.JE_EntryStatus);
				AssertEquals(AssertionMessage("No EntryHeader"), ZString.Empty, declaration.JE_EntryStatusDescription);

				var entry1 = declaration.ActiveEntryHeaders.AddNew();
				declaration.JE_EntryStatus = "ABC";
				AssertEquals(AssertionMessage("Single EntryHeader empty CH_Status - JE_EntryStatus"), "ABC", declaration.JE_EntryStatus);
				AssertEquals(AssertionMessage("Single EntryHeader empty CH_Status - JE_EntryStatusDescription"), ZString.Empty, declaration.JE_EntryStatusDescription);

				entry1.CH_EntryStatus = "XYZ";
				AssertEquals(AssertionMessage("Single EntryHeader with unknown Status - JE_EntryStatus"), "XYZ", declaration.JE_EntryStatus);
				AssertEquals(AssertionMessage("Single EntryHeader with unknown Status - JE_EntryStatusDescription"), ZString.Empty, declaration.JE_EntryStatusDescription);

				entry1.CH_EntryStatus = EntryStatusList.Codes.Cancelled;
				AssertEquals(AssertionMessage("Single EntryHeader with known Status - JE_EntryStatus"), EntryStatusList.Codes.Cancelled, declaration.JE_EntryStatus);
				AssertEquals(AssertionMessage("Single EntryHeader with known Status - JE_EntryStatusDescription"), EntryStatusList.Descriptions.Cancelled, declaration.JE_EntryStatusDescription);

				var entry2 = declaration.ActiveEntryHeaders.AddNew();
				entry2.CH_EntryStatus = ZString.Empty;
				AssertEquals(AssertionMessage("Multiple Entry Headers with empty value - JE_EntryStatus"), CommonEntryStatusList.Codes.MultipleEntryStatus, declaration.JE_EntryStatus);
				AssertEquals(AssertionMessage("Multiple Entry Headers with empty value - JE_EntryStatusDescription"), CommonEntryStatusList.Descriptions.MultipleEntryStatus, declaration.JE_EntryStatusDescription);

				entry2.CH_EntryStatus = EntryStatusList.Codes.Error;
				AssertEquals(AssertionMessage("Multiple Entry Headers with different values (not empty) - JE_EntryStatus"), CommonEntryStatusList.Codes.MultipleEntryStatus, declaration.JE_EntryStatus);
				AssertEquals(AssertionMessage("Multiple Entry Headers with different values (not empty) - JE_EntryStatusDescription"), CommonEntryStatusList.Descriptions.MultipleEntryStatus, declaration.JE_EntryStatusDescription);

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				AssertEquals(AssertionMessage("Multiple EntryHeaders - JE_EntryStatus"), CommonEntryStatusList.Codes.MultipleEntryStatus, declaration.JE_EntryStatus);
				AssertEquals(AssertionMessage("Multiple EntryHeaders - JE_EntryStatusDescription"), CommonEntryStatusList.Descriptions.MultipleEntryStatus, declaration.JE_EntryStatusDescription);
			}

			string AssertionMessage(string info) => $"MessageType={messageType} SubmissionType={submissionType} {info}";
		}
	});

	public void TestJE_CustomsOfficeMaxLength()
	{
		AssertHasCustomAttribute<MaxLengthAttribute>(Declaration.GetType(), "JE_CustomsOffice", false, attr => attr.MaxLength == 8);
	}

	public void TestJE_CustomsOffice_Default() => CombineAssertions(() =>
	{
		var authorizedConsignee = Factory.NewWithValidTestData<OrgHeader>();
		var authorisationHeader = Factory.New<CusAuthorisationHeader>();
		authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeLocationsForEDec;
		authorisationHeader.CPH_OH_PermitHolder = authorizedConsignee.PK;
		authorisationHeader.CPH_Number = "N1";
		var authorisationRule = authorisationHeader.CusAuthorisationRules.AddNew();
		authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
		authorisationRule.CPR_ValueFrom = "LOC1";
		var linkedRule = authorisationRule.LinkedCusAuthorisationRules.AddNew();
		linkedRule.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice;
		linkedRule.CPR_ValueFrom =  "CUS1";

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		Declaration.JE_OA_Representative = authorizedConsignee.MainAddress.PK;

		Declaration.JE_ClearanceLocation = ClearanceLocation.CustomsOffice;
		Declaration.JE_LocationOfGoods = ZString.Empty;
		Declaration.JE_LocationOfGoods = "LOC1";
		AssertEquals("CustomsOffice not set when ClearanceLocation<>2", ZString.Empty, Declaration.JE_CustomsOffice);

		Declaration.JE_ClearanceLocation = ClearanceLocation.Domicile;
		Declaration.JE_LocationOfGoods = ZString.Empty;
		Declaration.JE_LocationOfGoods = "LOC1";
		AssertEquals("CustomsOffice set when ClearanceLocation=1", "CUS1", Declaration.JE_CustomsOffice);

		Declaration.JE_CustomsOffice = "CUS9";
		Declaration.JE_LocationOfGoods = ZString.Empty;
		Declaration.JE_LocationOfGoods = "LOC1";
		AssertEquals("Existing CustomsOffice overwritten", "CUS1", Declaration.JE_CustomsOffice);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		Declaration.JE_CustomsOffice = ZString.Empty;
		Declaration.JE_LocationOfGoods = ZString.Empty;
		Declaration.JE_LocationOfGoods = "LOC1";
		AssertEquals("CustomsOffice not set when not Import", ZString.Empty, declaration.JE_CustomsOffice);
	});

	public void TestSupportsChcPivotBetweenInvoiceLineAndPackingCore()
	{
		var declaration = Factory.New<JobDeclarationForTesting>();
		AssertEquals(true, declaration.SupportsChcPivotBetweenInvoiceLineAndPackingCoreExposed);
	}

	public void TestContainerModeVisible() => CombineAssertions(() =>
	{
		foreach (var code in Declaration.Lookups.TransportTypeList.GetAllCodes())
		{
			Declaration.JE_TransportMode = code;
			AssertEquals(code, true, Declaration.ContainerModeVisible);
		}
	});

	public void TestJE_OH_Consignee_DefaultIsImporter() => CombineAssertions(() =>
	{
		var importer = Factory.New<OrgHeader>();

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		Declaration.JE_OH_Importer = importer.PK;
		AssertEquals("Consignee defaults to importer on import", importer.PK, Declaration.JE_OH_Consignee);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		Declaration.JE_OH_Importer = importer.PK;
		Declaration.JE_OH_Consignee = ZGuid.Empty;
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("Consignee defaults to importer on message type change to import", importer.PK, Declaration.JE_OH_Consignee);
	});

	public void TestDispatchCountryCode() => CombineAssertions(() =>
	{
		AssertEquals("DispatchCountryCode Caption", "Dispatch Country", DataBoundResourceStrings.GetDataForProperty(Declaration.DispatchCountryCodeInfo).Caption);
		AssertEquals("DispatchCountryCode ReadOnly", true, Declaration.DispatchCountryCodeInfo.ReadOnly);
	});

	public void TestDispatchCountryCode_Default() => CombineAssertions(() =>
	{
		Declaration.JE_RL_NKPortOfLoading = "DEBER";
		AssertEquals("DispatchCountryCode default", "DE", Declaration.DispatchCountryCode);
	});

	public void TestJE_PaymentMethod() => CombineAssertions(() =>
	{
		AssertEquals("JE_PaymentMethod Caption", "Duty paid by", DataBoundResourceStrings.GetDataForProperty(Declaration.JE_PaymentMethodInfo).Caption);
		AssertEquals("JE_PaymentMethod MaxLength", 3, Declaration.JE_PaymentMethodInfo.MaxLength);
	});

	public void TestJE_PaymentMethod_Default() => CombineAssertions(() =>
	{
		var paidTestHelper = new PaidTestHelper(Factory);

		paidTestHelper.TestPaidByDefault(OrgCusCode.SwissCodeTypes.CAD, Core.Constants.IncoTerms.DeliveredDutyPaid, false, false, d => d.JE_PaymentMethod, DeclarationPayerList.Codes.Declarant);
		paidTestHelper.TestPaidByDefault(OrgCusCode.SwissCodeTypes.CAD, Core.Constants.IncoTerms.DeliveredDutyUnpaid, false, false, d => d.JE_PaymentMethod, DeclarationPayerList.Codes.Declarant);
		paidTestHelper.TestPaidByDefault(OrgCusCode.SwissCodeTypes.CAD, Core.Constants.IncoTerms.DeliveredDutyPaid, true, false, d => d.JE_PaymentMethod, DeclarationPayerList.Codes.Consignor);
		paidTestHelper.TestPaidByDefault(OrgCusCode.SwissCodeTypes.CAD, Core.Constants.IncoTerms.DeliveredDutyPaid, true, true, d => d.JE_PaymentMethod, DeclarationPayerList.Codes.Consignor);
		paidTestHelper.TestPaidByDefault(OrgCusCode.SwissCodeTypes.CAD, Core.Constants.IncoTerms.DeliveredDutyUnpaid, false, true, d => d.JE_PaymentMethod, DeclarationPayerList.Codes.Importer);
		paidTestHelper.TestPaidByDefault(OrgCusCode.SwissCodeTypes.CAD, Core.Constants.IncoTerms.DeliveredDutyUnpaid, true, true, d => d.JE_PaymentMethod, DeclarationPayerList.Codes.Importer);
	});

	public void TestJE_VATPaidBy()
	{
		AssertEquals("JE_VATPaidBy Caption", "VAT paid by", DataBoundResourceStrings.GetDataForProperty(Declaration.JE_VATPaidByInfo).Caption);
	}

	public void TestJE_ClearanceLocation()
	{
		AssertEquals("JE_ClearanceLocation Caption", "Clearance Location", DataBoundResourceStrings.GetDataForProperty(Declaration.JE_ClearanceLocationInfo).Caption);
	}

	public void TestJE_LocationOfGoods() => CombineAssertions(() =>
	{
		AssertEquals("JE_LocationOfGoods Caption", "Goods Location", DataBoundResourceStrings.GetDataForProperty(Declaration.JE_LocationOfGoodsInfo).Caption);
		AssertEquals("JE_LocationOfGoods MaxLength", 100, Declaration.JE_LocationOfGoodsInfo.MaxLength);
	});

	public void TestDefaultJE_LocationOfGoods_Export() => CombineAssertions(() =>
	{
		var declarantWithNoAuthorisation = Factory.New<OrgHeader>();
		declarantWithNoAuthorisation.OH_Code = "CH001";

		var declarantWithOneAuthorisation = Factory.New<OrgHeader>();
		declarantWithOneAuthorisation.OH_Code = "CH002";

		var authorization1 = Factory.New<CusAuthorisationHeader>();
		authorization1.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar;
		authorization1.CPH_OH_PermitHolder = declarantWithOneAuthorisation.PK;
		authorization1.CPH_Number = "123";

		var declarantWithTwoAuthorisation = Factory.NewWithValidTestData<OrgHeader>();
		declarantWithTwoAuthorisation.OH_Code = "CH003";

		var authorization2 = Factory.New<CusAuthorisationHeader>();
		authorization2.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar;
		authorization2.CPH_OH_PermitHolder = declarantWithTwoAuthorisation.PK;
		authorization2.CPH_Number = "456";

		var authorization3 = Factory.New<CusAuthorisationHeader>();
		authorization3.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar;
		authorization3.CPH_OH_PermitHolder = declarantWithTwoAuthorisation.PK;
		authorization3.CPH_Number = "789";

		Declaration.JE_OA_DeclarantAddress = declarantWithNoAuthorisation.MainAddress.PK;
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;

		AssertEquals("LocationOfGoods empty / no Authorisation", ZString.Empty, Declaration.JE_LocationOfGoods);

		Declaration.JE_OA_DeclarantAddress = declarantWithOneAuthorisation.MainAddress.PK;
		AssertEquals("LocationOfGoods defaulted / 1 Authorisation matching", "123", Declaration.JE_LocationOfGoods);

		Declaration.JE_LocationOfGoods = "000";
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		AssertEquals("LocationOfGoods not changed / LocationOfGoods already has value", "000", Declaration.JE_LocationOfGoods);

		Declaration.JE_LocationOfGoods = ZString.Empty;
		Declaration.JE_OA_DeclarantAddress = declarantWithTwoAuthorisation.MainAddress.PK;
		AssertEquals("LocationOfGoods not defaulted / 2 Authorisations matching", ZString.Empty, Declaration.JE_LocationOfGoods);
	});

	public void TestDefaultJE_LocationOfGoods_Import() => CombineAssertions(() =>
	{
		var authorizedConsignee1 = Factory.NewWithValidTestData<OrgHeader>();
		var authorizedConsignee2 = Factory.NewWithValidTestData<OrgHeader>();
		CreateAuthorization(authorizedConsignee1, "R1");
		CreateAuthorization(authorizedConsignee2, "R2A", "R2B");

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;

		Declaration.JE_ClearanceLocation = ClearanceLocation.Domicile;

		Declaration.JE_OA_Representative = authorizedConsignee1.MainAddress.PK;
		AssertEquals("Set when single rule", "R1", Declaration.JE_LocationOfGoods);

		Declaration.JE_OA_Representative = authorizedConsignee2.MainAddress.PK;
		AssertEquals("Not set when multiple rules", ZString.Empty, Declaration.JE_LocationOfGoods);

		Declaration.JE_OA_Representative = authorizedConsignee1.MainAddress.PK;
		Declaration.JE_LocationOfGoods = "XXX";
		Declaration.JE_OA_Representative = authorizedConsignee2.MainAddress.PK;
		AssertEquals("Emptied on change to multiple", ZString.Empty, Declaration.JE_LocationOfGoods);

		Declaration.JE_OA_Representative = authorizedConsignee2.MainAddress.PK;
		Declaration.JE_LocationOfGoods = "XXX";
		Declaration.JE_OA_Representative = authorizedConsignee1.MainAddress.PK;
		AssertEquals("Overwritten on change to single", "R1", Declaration.JE_LocationOfGoods);

		Declaration.JE_ClearanceLocation = ClearanceLocation.CustomsOffice;
		Declaration.JE_OA_Representative = authorizedConsignee2.MainAddress.PK;
		Declaration.JE_LocationOfGoods = ZString.Empty;
		Declaration.JE_OA_Representative = authorizedConsignee1.MainAddress.PK;
		AssertEquals("Not set when not Domicile", ZString.Empty, Declaration.JE_LocationOfGoods);

		Declaration.JE_OA_Representative = authorizedConsignee1.MainAddress.PK;
		Declaration.JE_LocationOfGoods = "XXX";
		Declaration.JE_ClearanceLocation = ClearanceLocation.Domicile;
		AssertEquals("Set on change to Domicile", "R1", Declaration.JE_LocationOfGoods);

		void CreateAuthorization(OrgHeader holder, params string[] ruleValues)
		{
			var authorisationHeader = Factory.New<CusAuthorisationHeader>();
			authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeLocationsForEDec;
			authorisationHeader.CPH_Number = "N" + holder.OH_Code;
			authorisationHeader.CPH_OH_PermitHolder = holder.PK;
			foreach (var ruleValue in ruleValues)
			{
				var authorisationRule = authorisationHeader.CusAuthorisationRules.AddNew();
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
				authorisationRule.CPR_ValueFrom = ruleValue;
			}
		}
	});

	public void TestJE_DeclarationLanguage()
	{
		AssertEquals("JE_DeclarationLanguage Caption", "Language", DataBoundResourceStrings.GetDataForProperty(Declaration.JE_DeclarationLanguageInfo).Caption);
	}

	public void TestJE_DeclarationLanguage_Default() => CombineAssertions(() =>
	{
		string[] swissLanguages = { SharedConstants.Languages.German, SharedConstants.Languages.French, SharedConstants.Languages.Italian };
		const string nonSwissLanguage = SharedConstants.Languages.Swedish;

		foreach (var swissLanguage in swissLanguages)
		{
			var swissImporter = Factory.New<OrgHeader>();
			swissImporter.OH_Language = swissLanguage;

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = CHJobMessageTypeList.Codes.Import;
			declaration1.JE_DeclarationLanguage = ZString.Empty;
			declaration1.JE_OH_Importer = swissImporter.PK;
			AssertEquals($"IMP: Importer language {swissLanguage}", swissLanguage.Substring(0, 2), declaration1.JE_DeclarationLanguage);

			declaration1.JE_OH_Importer = ZGuid.Empty;
			declaration1.JE_MessageType = CHJobMessageTypeList.Codes.Export;
			declaration1.JE_OH_Supplier = swissImporter.PK;
			AssertEquals($"EXP: Supplier language {swissLanguage}", swissLanguage.Substring(0, 2), declaration1.JE_DeclarationLanguage);
		}

		var nonSwissImporter = Factory.New<OrgHeader>();
		nonSwissImporter.OH_Language = nonSwissLanguage;

		foreach (var swissLanguage in swissLanguages)
		{
			GlbCompany.CurrentCompany.OrgProxy.OH_Language = swissLanguage;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = CHJobMessageTypeList.Codes.Import;
			declaration2.JE_DeclarationLanguage = ZString.Empty;
			declaration2.JE_OH_Importer = nonSwissImporter.PK;
			AssertEquals($"IMP: Company language {swissLanguage}", swissLanguage.Substring(0, 2), declaration2.JE_DeclarationLanguage);

			declaration2.JE_OH_Importer = ZGuid.Empty;
			declaration2.JE_MessageType = CHJobMessageTypeList.Codes.Export;
			declaration2.JE_DeclarationLanguage = ZString.Empty;
			declaration2.JE_OH_Supplier = nonSwissImporter.PK;
			AssertEquals($"EXP: Company language {swissLanguage}", swissLanguage.Substring(0, 2), declaration2.JE_DeclarationLanguage);
		}

		var declaration3 = Factory.New<JobDeclaration>();
		declaration3.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		GlbCompany.CurrentCompany.OrgProxy.OH_Language = nonSwissLanguage;
		GlbStaff.CurrentUser.GS_WorkingLanguage = SharedConstants.Languages.French;
		declaration3.JE_DeclarationLanguage = ZString.Empty;
		declaration3.JE_OH_Importer = nonSwissImporter.PK;
		AssertEquals($"IMP: User Language", "FR", declaration3.JE_DeclarationLanguage);

		GlbStaff.CurrentUser.GS_WorkingLanguage = SharedConstants.Languages.Albanian;
		declaration3.JE_OH_Importer = ZGuid.Empty;
		declaration3.JE_DeclarationLanguage = ZString.Empty;
		declaration3.JE_OH_Importer = nonSwissImporter.PK;
		AssertEquals($"IMP: Fallback language", "DE", declaration3.JE_DeclarationLanguage);

		declaration3.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		declaration3.JE_OH_Importer = ZGuid.Empty;
		declaration3.JE_DeclarationLanguage = ZString.Empty;
		declaration3.JE_OH_Supplier = nonSwissImporter.PK;
		AssertEquals($"EXP: Fallback Language", "DE", declaration3.JE_DeclarationLanguage);

		GlbStaff.CurrentUser.GS_WorkingLanguage = SharedConstants.Languages.French;
		declaration3.JE_OH_Supplier = ZGuid.Empty;
		declaration3.JE_DeclarationLanguage = ZString.Empty;
		declaration3.JE_OH_Supplier = nonSwissImporter.PK;
		AssertEquals($"EXP: User Language", "FR", declaration3.JE_DeclarationLanguage);
	});

	public void TestJE_AdditionalDecisionInfo()
	{
		AssertEquals("JE_AdditionalDecisionInfo Caption", "Assessment decision receipt for EU", DataBoundResourceStrings.GetDataForProperty(Declaration.JE_AdditionalDecisionInfoInfo).Caption);
	}

	public void TestDutyByAccountNo() => CombineAssertions(() =>
	{
		var paidTestHelper = new PaidTestHelper(Factory);

		paidTestHelper.TestPaidByAccountNo(OrgCusCode.SwissCodeTypes.CAD, (d, v) => d.JE_PaymentMethod = v, d => d.DutyPaidByAccountNo, DeclarationPayerList.Codes.Consignor, PaidTestHelper.SupplierAccountNo);
		paidTestHelper.TestPaidByAccountNo(OrgCusCode.SwissCodeTypes.CAD, (d, v) => d.JE_PaymentMethod = v, d => d.DutyPaidByAccountNo, DeclarationPayerList.Codes.Importer, PaidTestHelper.ImporterAccountNo);
		paidTestHelper.TestPaidByAccountNo(OrgCusCode.SwissCodeTypes.CAD, (d, v) => d.JE_PaymentMethod = v, d => d.DutyPaidByAccountNo, DeclarationPayerList.Codes.Consignee, PaidTestHelper.ConsigneeAccountNo);
		paidTestHelper.TestPaidByAccountNo(OrgCusCode.SwissCodeTypes.CAD, (d, v) => d.JE_PaymentMethod = v, d => d.DutyPaidByAccountNo, DeclarationPayerList.Codes.Forwarder, PaidTestHelper.FreightForwarderAccountNo);
		paidTestHelper.TestPaidByAccountNo(OrgCusCode.SwissCodeTypes.CAD, (d, v) => d.JE_PaymentMethod = v, d => d.DutyPaidByAccountNo, DeclarationPayerList.Codes.Cash, ZString.Empty);
		paidTestHelper.TestPaidByAccountNo(OrgCusCode.SwissCodeTypes.CAD, (d, v) => d.JE_PaymentMethod = v, d => d.DutyPaidByAccountNo, DeclarationPayerList.Codes.Declarant, PaidTestHelper.DeclarantAccountNo);
		paidTestHelper.TestDutyByAccountNoForDeclarantFallback(OrgCusCode.SwissCodeTypes.CAD, (d, v) => d.JE_PaymentMethod = v, d => d.DutyPaidByAccountNo);
	});

	public void TestVATByAccountNo() => CombineAssertions(() =>
	{
		var paidTestHelper = new PaidTestHelper(Factory);

		paidTestHelper.TestPaidByAccountNo(OrgCusCode.SwissCodeTypes.CAV, (d, v) => d.JE_VATPaidBy = v, d => d.VATPaidByAccountNo, DeclarationPayerList.Codes.Consignor, PaidTestHelper.SupplierAccountNo);
		paidTestHelper.TestPaidByAccountNo(OrgCusCode.SwissCodeTypes.CAV, (d, v) => d.JE_VATPaidBy = v, d => d.VATPaidByAccountNo, DeclarationPayerList.Codes.Importer, PaidTestHelper.ImporterAccountNo);
		paidTestHelper.TestPaidByAccountNo(OrgCusCode.SwissCodeTypes.CAV, (d, v) => d.JE_VATPaidBy = v, d => d.VATPaidByAccountNo, DeclarationPayerList.Codes.Consignee, PaidTestHelper.ConsigneeAccountNo);
		paidTestHelper.TestPaidByAccountNo(OrgCusCode.SwissCodeTypes.CAV, (d, v) => d.JE_VATPaidBy = v, d => d.VATPaidByAccountNo, DeclarationPayerList.Codes.Forwarder, PaidTestHelper.FreightForwarderAccountNo);
		paidTestHelper.TestPaidByAccountNo(OrgCusCode.SwissCodeTypes.CAV, (d, v) => d.JE_VATPaidBy = v, d => d.VATPaidByAccountNo, DeclarationPayerList.Codes.Cash, ZString.Empty);
		paidTestHelper.TestPaidByAccountNo(OrgCusCode.SwissCodeTypes.CAV, (d, v) => d.JE_VATPaidBy = v, d => d.VATPaidByAccountNo, DeclarationPayerList.Codes.Declarant, PaidTestHelper.DeclarantAccountNo);
		paidTestHelper.TestDutyByAccountNoForDeclarantFallback(OrgCusCode.SwissCodeTypes.CAV, (d, v) => d.JE_VATPaidBy = v, d => d.VATPaidByAccountNo);
	});

	public void TestIsRelocationProcedureOrProcessingTraffic() => CombineAssertions(() =>
	{
		AssertEquals("No invoice", false, Declaration.IsRelocationProcedureOrProcessingTraffic);

		var invoice = Declaration.Invoices.AddNew();
		AssertEquals("No invoice line", false, Declaration.IsRelocationProcedureOrProcessingTraffic);

		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_ZZF_NKTaxType = UniversalReferenceConstants.TaxCodes.StandardRate;
		AssertEquals($"{nameof(UniversalReferenceConstants.TaxCodes.StandardRate)}", false, Declaration.IsRelocationProcedureOrProcessingTraffic);

		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_ZZF_NKTaxType = UniversalReferenceConstants.TaxCodes.RelocationProcedure;
		AssertEquals($"{nameof(UniversalReferenceConstants.TaxCodes.RelocationProcedure)}", true, Declaration.IsRelocationProcedureOrProcessingTraffic);
		invoiceLine2.JI_ZZF_NKTaxType = UniversalReferenceConstants.TaxCodes.ProcessingTraffic;
		AssertEquals($"{nameof(UniversalReferenceConstants.TaxCodes.ProcessingTraffic)}", true, Declaration.IsRelocationProcedureOrProcessingTraffic);
	});

	public void TestCaptions() => CombineAssertions(() =>
	{
		AssertEquals($"{nameof(Declaration.DutyPaidByAccountNo)}", "Duty Account", DataBoundResourceStrings.GetDataForProperty(Declaration.DutyPaidByAccountNoInfo).Caption);
		AssertEquals($"{nameof(Declaration.VATPaidByAccountNo)}", "VAT Account", DataBoundResourceStrings.GetDataForProperty(Declaration.VATPaidByAccountNoInfo).Caption);
		AssertEquals($"{nameof(Declaration.JE_OA_DeclarantAddress)}", "Declarant", DataBoundResourceStrings.GetDataForProperty(Declaration.JE_OA_DeclarantAddressInfo).Caption);
	});

	public void TestCHDPassword() => CombineAssertions(() =>
	{
		var agent1 = Factory.NewWithValidTestData<GlbStaff>();
		agent1.GS_Code = "AG1";
		var agent2 = Factory.NewWithValidTestData<GlbStaff>();
		agent2.GS_Code = "AG2";

		var externalPassword = Factory.New<GlbExternalPassword_CHD>();
		externalPassword.GP_GS = agent1.PK;
		externalPassword.GP_UserID = "901";

		AssertNull("No agent", Declaration.CHDPassword);

		Declaration.JE_GS_NKCusAgent = agent1.GS_Code;
		AssertEquals("Agent with password", "901", Declaration.CHDPassword.GP_UserID);

		Declaration.JE_GS_NKCusAgent = agent2.GS_Code;
		Assert("Agent without password", Declaration.CHDPassword.GP_UserID.IsEmpty);
	});

	public void TestIncoTermAndChargeFactory()
	{
		AssertType<IncoTermAndCustomsChargeFactory>(GetJobDeclaration().IncoTermAndChargeFactory);
	}

	public void TestDefaultEntryInstructionIsAddedOnSave_Import() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateEnstyCodeList(Factory);
		RefCusCodeTestHelper.CreateEnsubCodeList(Factory);

		var declaration = InitializeDeclarationWithHeaderAndLine(JobMessageTypeList.Codes.Import);

		Factory.Save();

		var newFactory = new BusinessObjectFactory();
		var loadDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);
		AssertEquals("CustomsEntryInstructions Count", 1, loadDeclaration.CustomsEntryInstructions.Count);
		var entryInstruction = loadDeclaration.CustomsEntryInstructions[0];

		AssertEquals("Default Entry Instruction CEI_Style", UniversalReferenceConstants.DeclarationTypeCodes.Definitive, entryInstruction.CEI_Style);
		AssertEquals("Default Entry Instruction CEI_SubStyle", UniversalReferenceConstants.DeclarationTimeCodes.PresentationToCustoms, entryInstruction.CEI_SubStyle);
		AssertEquals("Default Entry Instruction CEI_Description", "Definitiv - Gestellung", entryInstruction.CEI_Description);
	});

	public void TestConsignorDocAddress() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals("DocAddressType", DocAddressType.ConsignorDocumentaryAddress, Declaration.ConsignorDocAddress.DocAddressType);
		Assert("DocAddresses", Declaration.DocAddresses.Contains(Declaration.ConsignorDocAddress));

		IDocAddresses docAddresses = Declaration;
		var supportedAddressTypes = new List<DocAddressType>(docAddresses.SupportedAddressTypes);
		AssertEquals("supportedAddressTypes.Contains(DocAddressType.ConsignorDocumentaryAddress)", true, supportedAddressTypes.Contains(DocAddressType.ConsignorDocumentaryAddress));
		AssertEquals("docAddresses.GetDocAddress(DocAddressType.ConsignorDocumentaryAddress) = Declaration.ConsignorDocumentaryAddress", Declaration.ConsignorDocAddress.DocAddressType, docAddresses.GetDocAddressRequirement(DocAddressType.ConsignorDocumentaryAddress).DefaultDocAddressType);
	});

	public void TestDefaultEntryInstructionIsAddedOnSave_Export() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateInputControlList(Factory);
		RefCusCodeTestHelper.CreateProcedureList(Factory);

		var declaration = InitializeDeclarationWithHeaderAndLine(JobMessageTypeList.Codes.Export);

		Factory.Save();

		var newFactory = new BusinessObjectFactory();
		var loadDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);

		AssertEquals("CustomsEntryInstructions Count", 1, loadDeclaration.CustomsEntryInstructions.Count);
		var entryInstruction = loadDeclaration.CustomsEntryInstructions[0];

		AssertEquals("Default Entry Instruction CEI_Style", UniversalReferenceConstants.InputControlCodes.Ordinary, entryInstruction.CEI_Style);
		AssertEquals("Default Entry Instruction CEI_ProcedureCode", RefCusCodeTestHelper.ProcedureCodeDefaultValueExp, entryInstruction.CEI_Procedure);
		AssertEquals("Default Entry Instruction CEI_Description", $"{RefCusCodeTestHelper.ProcedureCodeDefaultValueExp} - {RefCusCodeTestHelper.CEIStyleDefaultDescriptionEXP}", entryInstruction.CEI_Description);
	});

	public void TestDefaultEntryInstructionIsAddedOnSave_EDA() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateInputControlList(Factory);
		RefCusCodeTestHelper.CreateProcedureList(Factory);

		var declaration = InitializeDeclarationWithHeaderAndLine(CHJobMessageTypeList.Codes.ExportDeclarationActivation);

		Factory.Save();

		var newFactory = new BusinessObjectFactory();
		var loadDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);
		AssertEquals("CustomsEntryInstructions Count", 1, loadDeclaration.CustomsEntryInstructions.Count);
		var entryInstruction = loadDeclaration.CustomsEntryInstructions[0];

		AssertEquals("Default Entry Instruction CEI_Style", UniversalReferenceConstants.InputControlCodes.Ordinary, entryInstruction.CEI_Style);
		AssertEquals("Default Entry Instruction CEI_SubStyle", UniversalReferenceConstants.DeclarationTimeCodes.PresentationToCustoms, entryInstruction.CEI_SubStyle);
		AssertEquals("Default Entry Instruction CEI_Description", $"{RefCusCodeTestHelper.ProcedureCodeDefaultValueExp} - {RefCusCodeTestHelper.CEIStyleDefaultDescriptionEXP}", entryInstruction.CEI_Description);
	});

	public void TestNoDefaultEntryInstructionIsAddedOnSave_Import() => AssertNoDefaultEntryInstructionIsAddedOnSave(JobMessageTypeList.Codes.Import);
	public void TestNoDefaultEntryInstructionIsAddedOnSave_Export() => AssertNoDefaultEntryInstructionIsAddedOnSave(JobMessageTypeList.Codes.Export);
	public void TestNoDefaultEntryInstructionIsAddedOnSave_EDA() => AssertNoDefaultEntryInstructionIsAddedOnSave(CHJobMessageTypeList.Codes.ExportDeclarationActivation);

	public void AssertNoDefaultEntryInstructionIsAddedOnSave(ZString messageType) => CombineAssertions(() =>
	{
		var entryInstructionStyle = UniversalReferenceConstants.DeclarationTypeCodes.Provisional;
		Declaration.JE_MessageType = messageType;

		var invoiceHeader = Factory.NewWithValidTestData<JobComInvoiceHeader>();
		Declaration.Invoices.Add(invoiceHeader);

		var entryInstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
		Declaration.CustomsEntryInstructions.Add(entryInstruction);
		entryInstruction.CEI_Style = entryInstructionStyle;

		var invoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();

		invoiceLine.JI_JZ = invoiceHeader.PK;
		invoiceHeader.InvoiceLines.Add(invoiceLine);

		invoiceLine.JI_CEI = entryInstruction.PK;

		Factory.Save();

		var newFactory = new BusinessObjectFactory();
		var loadedDeclaration = newFactory.Load<JobDeclaration>(Declaration.PK);
		AssertEquals("CustomsEntryInstructions Count", 1, loadedDeclaration.CustomsEntryInstructions.Count);
		var loadedInvoiceLine = loadedDeclaration.InvoiceLines[0];

		AssertEquals("Entry Instruction CEI_Style", entryInstructionStyle, loadedInvoiceLine.EntryInstruction.CEI_Style);
	});

	public void TestNoDefaultEntryInstructionIsAddedOnSave_FakeDeclaration()
	{
		var standaloneInv = Factory.New<JobComInvoiceHeader>();
		var fakeDec = new FakeDeclarationCreatorForInvoice(standaloneInv);

		standaloneInv.InvoiceLines.AddNew();

		AssertEquals("CustomsEntryInstructions Count", 0, ((JobDeclaration)fakeDec.HeaderData).CustomsEntryInstructions.Count);
	}

	public void TestJE_MessageType_DefaultForLIImporter()
	{
		var orgImporter = Factory.NewWithValidTestData<OrgHeader>();
		orgImporter.OH_RL_NKClosestPort = "LIVDZ";
		Declaration.JE_OH_Importer = orgImporter.PK;

		AssertEquals(JobMessageTypeList.Codes.Import, Declaration.JE_MessageType);
	}

	public void TestJE_GoodsDestination_Export() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		AssertEquals("Destination Country empty on new", ZString.Empty, Declaration.JE_GoodsDestination);

		Declaration.JE_RL_NKPortOfArrival = "CHAES";
		AssertEquals("Destination Country default", "CH", Declaration.JE_GoodsDestination);

		Declaration.JE_RL_NKPortOfArrival = "DEBER";
		AssertEquals("Destination Country no override", "CH", Declaration.JE_GoodsDestination);
	});

	public void TestJE_GoodsDestination_Export_Default() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		AssertEquals("Destination Country empty on new", ZString.Empty, Declaration.JE_GoodsDestination);

		Declaration.JE_RL_NKPortOfArrival = "CHAES";
		AssertEquals("Destination Country default", "CH", Declaration.JE_GoodsDestination);

		Declaration.JE_RL_NKPortOfArrival = "DEBER";
		AssertEquals("Destination Country no override", "CH", Declaration.JE_GoodsDestination);
	});

	public void TestJE_MergeBy() => CombineAssertions(() =>
	{
		AssertEquals("default", OrgConstants.MergeInvoiceLines.NotMerge, Declaration.JE_MergeBy);
		AssertEquals("readonly", true, Declaration.JE_MergeByInfo.ReadOnly);
	});

	public void TestDefaultJE_MergeByFromLocalParty() => CombineAssertions(() =>
	{
		var mergeByCodes = Declaration.Lookups.MergeByList.GetAllCodes().Where(x => x != OrgConstants.MergeInvoiceLines.NotMerge && x != OrgConstants.MergeInvoiceLines.Default);
		Env.Registry.SetCommercialInvoiceLineMergeMethod(GlbCompany.CurrentCompany.PK.ToGuid(), mergeByCodes.ElementAt(0));
		var org = Factory.New<OrgHeader>();

		Declaration.JE_OH_Importer = org.PK;
		Declaration.JE_OH_Supplier = org.PK;
		AssertEquals("Should not get default from registry", OrgConstants.MergeInvoiceLines.NotMerge, Declaration.JE_MergeBy);

		Declaration.JE_OH_Importer = ZGuid.Empty;
		Declaration.JE_OH_Supplier = ZGuid.Empty;

		org.MiscServ.OM_EXMergeCustomsInvoiceLinesBy = mergeByCodes.ElementAt(1);
		Declaration.JE_OH_Importer = org.PK;
		Declaration.JE_OH_Supplier = org.PK;
		AssertEquals("Should not get default from supplier/importer", OrgConstants.MergeInvoiceLines.NotMerge, Declaration.JE_MergeBy);
	});

	public void TestRebuildAdditionalTaxes() => CombineAssertions(() =>
	{
		var additionalTaxTariffHelper = new AdditionalTaxTariffTestHelper(Factory);
		var parentTariff = additionalTaxTariffHelper.CreateParentImportTariff();
		additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode();
		additionalTaxTariffHelper.CreateAdditionalTaxTariffWithRelationship(parentTariff);

		var invoiceLine = Declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		invoiceLine.JI_CountryOfOrigin = AdditionalTaxTariffTestHelper.DefaultTradeGroupCountry;
		invoiceLine.JI_Tariff = parentTariff.ZZ1_TariffCode;

		AssertEquals("No Additional Taxes created for Export job", 0, invoiceLine.AdditionalTaxes.Count);

		invoiceLine.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("Additional Taxes created for Import job", 2, invoiceLine.AdditionalTaxes.Count);

		invoiceLine.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		Factory.Save();
		AssertEquals("Additional Taxes deleted on saving for Export job", 0, invoiceLine.AdditionalTaxes.Count);
	});

	public void TestSetDefaultValuesCH()
	{
		AssertEquals(Declaration.Branch.OrgProxy.MainAddress.PK, Declaration.JE_OA_DeclarantAddress);
	}

	JobDeclaration InitializeDeclarationWithHeaderAndLine(string messageType)
	{
		Declaration.JE_MessageType = messageType;

		var invoiceHeader = Factory.NewWithValidTestData<JobComInvoiceHeader>();
		Declaration.Invoices.Add(invoiceHeader);

		var invoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();

		invoiceLine.JI_JZ = invoiceHeader.PK;
		invoiceHeader.InvoiceLines.Add(invoiceLine);

		return Declaration;
	}

	public override void TestGetSupportingDocSendingObject()
	{
		AssertType<SupportingDocSendingObject>(Declaration.GetSupportingDocSendingObject());
	}

	public void TestMergeManager()
	{
		AssertType<MergeManager>(Declaration.MergeManager);
	}

	public void TestDoMergeSkippedForEDA() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		Factory.Save();

		var sendsMessagesToCustomsShutterUpperer = new SendsMessagesToCustomsShutterUpperer();

		declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		AssertEquals("DoMerge result", false, declaration.DoMerge(sendsMessagesToCustomsShutterUpperer));
		AssertEquals($"{declaration.JE_MessageType}: No entries created", false, declaration.CustomsEntryHeaders.Any(x => x.MergedLines.Count > 0));

		declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		declaration.DoMerge(sendsMessagesToCustomsShutterUpperer);
		AssertEquals($"{declaration.JE_MessageType}: Entries created", true, declaration.CustomsEntryHeaders.Any(x => x.MergedLines.Count > 0));
	});

	public void TestOnSaveCusEntryHeaderCreatedForEDA() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		Factory.Save();

		var entryHeader = declaration.CustomsEntryHeaders.FirstOrDefault();
		AssertNotNull("EntryHeader created", entryHeader);
		AssertEquals("EntryHeader linked to EntryInstruction", instruction.PK, entryHeader.CH_CEI_Instruction);
		AssertNotEquals("LRN created", entryHeader.CH_BGMReference);

		entryHeader.AllEntryLines.AddNew();
		Factory.Save();

		AssertEquals("No new EntryHeader", 1, declaration.CustomsEntryHeaders.Count);
		AssertSame("Same EntryHeader", entryHeader, declaration.CustomsEntryHeaders.First());
		AssertEquals("EntryLines deleted", 0, entryHeader.AllEntryLines.Count);
	});

	public void TestOnSaveCusEntryHeaderNotCreatedForNonEDA() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		declaration.CustomsEntryInstructions.AddNew();
		Factory.Save();
		AssertEquals("No EntryHeader created", 0, declaration.CustomsEntryHeaders.Count);

		declaration.CustomsEntryHeaders.AddNew().AllEntryLines.AddNew();
		Factory.Save();
		AssertEquals("EntryHeader still exists", 1, declaration.CustomsEntryHeaders.Count);
		AssertEquals("EntryLine still exists", 1, declaration.CustomsEntryHeaders.First().AllEntryLines.Count);
	});

	public void TestIsExportOrExportDeclarationActivation() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		AssertEquals("Import", false, Declaration.IsExportOrExportDeclarationActivation);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		AssertEquals("Export", true, Declaration.IsExportOrExportDeclarationActivation);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		AssertEquals("Export", true, Declaration.IsExportOrExportDeclarationActivation);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.ExportDeclarationByExternalBroker;
		AssertEquals("Export Declaration By External Broker", true, Declaration.IsExportOrExportDeclarationActivation);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertEquals("Miscellaneous Customs", false, Declaration.IsExportOrExportDeclarationActivation);
	});

	public void TestIsExportDeclarationActivation() => CombineAssertions(() =>
		{
			Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
			AssertEquals("Import", false, Declaration.IsExportDeclarationActivation);

			Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
			AssertEquals("Export", false, Declaration.IsExportDeclarationActivation);

			Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
			AssertEquals("Export Declaration Activation", true, Declaration.IsExportDeclarationActivation);
		});

	public void TestIsExportActivationPassar() => CombineAssertions(() =>
	{
		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Edec;
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		AssertEquals("Message Type:Import & Message Sub Type:Edec", false, Declaration.IsExportActivationPassar);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		AssertEquals("Export & Message Sub Type:Edec", false, Declaration.IsExportActivationPassar);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		AssertEquals("Export Declaration Activation & Message Sub Type:Edec", false, Declaration.IsExportActivationPassar);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Passar;
		AssertEquals("Export Declaration Activation & Message Sub Type: Passar", true, Declaration.IsExportActivationPassar);
	});

	public void TestIsExportActivationEdec() => CombineAssertions(() =>
	{
		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Passar;
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		AssertEquals("Message Type:Import & Message Sub Type: Passar", false, Declaration.IsExportActivationEdec);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		AssertEquals("Export & Message Sub Type: Passar", false, Declaration.IsExportActivationEdec);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		AssertEquals("Export Declaration Activation & Message Sub Type: Passar", false, Declaration.IsExportActivationEdec);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		Declaration.JE_MessageSubType = ActivationTypeList.Codes.Edec;
		AssertEquals("Export Declaration Activation & Message Sub Type: Edec", true, Declaration.IsExportActivationEdec);
	});

	public void TestIsExportAndOwnPropulsion() => CombineAssertions(() =>
		{
			Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
			AssertEquals("Import", false, Declaration.IsExportAndOwnPropulsion);

			Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
			AssertEquals("Export", false, Declaration.IsExportAndOwnPropulsion);

			Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
			AssertEquals("Export Declaration Activation", false, Declaration.IsExportAndOwnPropulsion);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.OwnPropulsion;
			AssertEquals("Export Declaration Activation / OWN", true, Declaration.IsExportAndOwnPropulsion);

			Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
			AssertEquals("Export / OWN", true, Declaration.IsExportAndOwnPropulsion);
		});

	public void TestIsImportDomicile() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		Declaration.JE_ClearanceLocation = ZString.Empty;
		AssertEquals($"JE_ClearanceLocation={Declaration.JE_ClearanceLocation}", false, Declaration.IsImportDomicile);
		Declaration.JE_ClearanceLocation = ClearanceLocation.Domicile;
		AssertEquals($"JE_ClearanceLocation={Declaration.JE_ClearanceLocation}", true, Declaration.IsImportDomicile);
		Declaration.JE_ClearanceLocation = ClearanceLocation.CustomsOffice;
		AssertEquals($"JE_ClearanceLocation={Declaration.JE_ClearanceLocation}", false, Declaration.IsImportDomicile);
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		Declaration.JE_ClearanceLocation = ClearanceLocation.Domicile;
		AssertEquals($"JE_MessageType={Declaration.JE_MessageType}", false, Declaration.IsImportDomicile);
	});

	public void TestDeclarationNumberReadOnly() => CombineAssertions(() =>
		{
			Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
			AssertEquals("Import", true, Declaration.DeclarationNumberInfo.ReadOnly);

			Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
			AssertEquals("Export", true, Declaration.DeclarationNumberInfo.ReadOnly);

			Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
			AssertEquals("Export Declaration Activation", false, Declaration.DeclarationNumberInfo.ReadOnly);

			Declaration.JE_MessageType = CHJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("Miscellaneous Customs", true, Declaration.DeclarationNumberInfo.ReadOnly);
		});

	public void TestPackingGroups() => AssertType<DeclarationLevelPackingGroupCollection>(Factory.New<JobDeclaration>().PackingGroups);

	public void TestPackagesCollectionIsRightType()
	{
		AssertEquals("Declaration.Packages.GetType()", typeof(BaseDeclarationLevelPackageCollection<Package>), Declaration.Packages.GetType());
	}

	public void TestConsignorDefaultContact() => AssertDefaultContact(Declaration.ConsignorDocAddress);

	public void TestSupplierDefaultContact() => AssertDefaultContact(Declaration.SupplierDocumentaryAddress);

	void AssertDefaultContact(JobDocAddress address)
	{
		var org1 = Factory.NewWithValidTestData<OrgHeader>();
		var org1Contact = org1.Contacts.AddNew();
		org1Contact.OC_ContactName = "Contact1";

		var org2 = Factory.NewWithValidTestData<OrgHeader>();
		var org2Contact = org2.Contacts.AddNew();
		org2Contact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
		org2Contact.OC_ContactName = "Contact2";

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		address.OrganisationPK = org1.PK;
		AssertEquals("Supplier without CUS Contact", ZString.Empty, address.E2_Contact);

		address.OrganisationPK = org2.PK;
		AssertEquals("Supplier with CUS Contact (EXP)", "Contact2", address.E2_Contact);

		address.OrganisationPK = ZGuid.Empty;
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		address.OrganisationPK = org2.PK;
		AssertEquals("Supplier with CUS Contact (IMP)", ZString.Empty, address.E2_Contact);
	}

	public void TestJE_UCR()
	{
		AssertEquals("JE_UCR Caption", "Ref.No./UCR", DataBoundResourceStrings.GetDataForProperty(Declaration.JE_UCRInfo).Caption);
	}

	public void TestIsGoodsDestinationInNCL0147CountryList()
	{
		RefCusTradeGroupTestHelper.CreateTestNCL0147CountryList(Factory);

		Declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Austria;
		Assert(Declaration.IsGoodsDestinationInNCL0147CountryList);
		Declaration.JE_GoodsDestination = Core.Constants.CountryCodes.UnitedKingdom;
		AssertEquals(false, Declaration.IsGoodsDestinationInNCL0147CountryList);
		Declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Australia;
		AssertEquals(false, Declaration.IsGoodsDestinationInNCL0147CountryList);
	}

	public override void TestMergeByDefaultsFromClientWhenClientChanges()
	{
		Assert("Change of JE_MergeBy on client change has been disabled", true);
	}

	public void TestDeclarationNumberAsExportDeclarationActivationEvokesHasChanges() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		Declaration.Factory.Save();
		Declaration.DeclarationNumber = ZString.Empty;
		AssertEquals("Empty DeclarationNumber should have HasChanges return false", false, Declaration.HasChanges);
		Declaration.DeclarationNumber = "ABC123";
		AssertEquals("Filled DeclarationNumber should have HasChanges return true", true, Declaration.HasChanges);
		Declaration.Factory.Save();
		AssertEquals("Saved Declaration and unchanged DeclarationNumber should have HasChanges return false", false, Declaration.HasChanges);
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		Declaration.Factory.Save();
		Declaration.DeclarationNumber = "321BCA";
		AssertEquals("HasChanges should return false since it is only editable under EDA Message Type", false, Declaration.HasChanges);
	});

	public void TestExportDeclarationActivationEntryHeadersIsRemovedOnMessageTypeChange() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		Declaration.Factory.Save();
		AssertEquals("ActiveEntryHeader does not contain a EntryHeader", 0, Declaration.ActiveEntryHeaders.Count);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		Declaration.Factory.Save();
		AssertEquals("Setting the MessageType to EDA creates an CusEntryHeader", 1, Declaration.ActiveEntryHeaders.Count);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		Declaration.Factory.Save();
		AssertEquals("Changing the MessageType removes the CusEntryHeader", 0, Declaration.ActiveEntryHeaders.Count);
	});

	public void TestWasExportActivation() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		Declaration.Factory.Save();
		AssertEquals("The Declaration just went through its maiden Save, thus it returns false", false, Declaration.WasExportDeclaration);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		Declaration.Factory.Save();
		AssertEquals("Message Type original value is IMP, thus it returns false", false, Declaration.WasExportDeclaration);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		AssertEquals("Message Type original value is EDA, thus it returns true", true, Declaration.WasExportDeclaration);

		Declaration.Factory.Save();
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		AssertEquals("Message Type original value is EXP, thus it returns false", false, Declaration.WasExportDeclaration);
	});

	public void TestConsignorDocAddressRequirement_ValidateOrganisationPK_NP70172() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		var consignorDocAddress = Declaration.ConsignorDocAddress;
		var consignor = Factory.New<OrgHeader>();
		Declaration.ConsignorDocAddress.OrganisationPK = consignor.PK;

		consignorDocAddress.Address.OA_RL_NKRelatedPortCode = "FRPAR";
		consignorDocAddress.Address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;
		Declaration.ConsignorDocAddress.Validation.ValidateOrganisationPK();
		AssertHasMessageError(Declaration.ConsignorDocAddress.OrganisationPKInfo, PassarValidationMessages.MessageNP70172_Consignor);

		consignorDocAddress.Address.OA_RL_NKRelatedPortCode = "DEBER";
		consignorDocAddress.Address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
		Declaration.ConsignorDocAddress.Validation.ValidateOrganisationPK();
		AssertNoMessageError(Declaration.ConsignorDocAddress.OrganisationPKInfo, PassarValidationMessages.MessageNP70172_Consignor);

		consignorDocAddress.Address.OA_RL_NKRelatedPortCode = "CHABL";
		consignorDocAddress.Address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;
		Declaration.ConsignorDocAddress.Validation.ValidateOrganisationPK();
		AssertNoMessageError(Declaration.ConsignorDocAddress.OrganisationPKInfo, PassarValidationMessages.MessageNP70172_Consignor);

		consignorDocAddress.Address.OA_RL_NKRelatedPortCode = "LIBAZ";
		consignorDocAddress.Address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Liechtenstein;
		Declaration.ConsignorDocAddress.Validation.ValidateOrganisationPK();
		AssertNoMessageError(Declaration.ConsignorDocAddress.OrganisationPKInfo, PassarValidationMessages.MessageNP70172_Consignor);
	});

	public void TestHasTransportDocuments() => CombineAssertions(() =>
	{
		var invoiceHeader1 = Declaration.Invoices.AddNew();
		var invoiceHeader2 = Declaration.Invoices.AddNew();
		AssertEquals("Transport Document exists", false, Declaration.HasTransportDocuments);

		invoiceHeader1.TransportDocuments.AddNew();
		AssertEquals("No Transport Document exists IH1", true, Declaration.HasTransportDocuments);

		invoiceHeader2.TransportDocuments.AddNew();
		invoiceHeader2.TransportDocuments.AddNew();
		AssertEquals("No Transport Document exists IH2 ", true, Declaration.HasTransportDocuments);

		invoiceHeader1.TransportDocuments.RemoveAndDeleteAll();
		AssertEquals("No Transport Document exists IH3", true, Declaration.HasTransportDocuments);
	});

	public void TestConsignorDocAddressRequirement_ValidateOrganisationPK_NS30003() => CombineAssertions(() =>
	{
		const string warning = "[NS30003] Consignor in Simplified Declaration will not be sent to Customs.";

		var consignorDocAddress = Factory.New<JobDocAddress>();
		consignorDocAddress.E2_AddressType = DocAddressTypes.Codes.ConsignorDocumentaryAddress;
		consignorDocAddress.E2_AddressOverride = true;
		Declaration.CustomsEntryInstructions.AddNew();
		var entryInstruction = Declaration.CustomsEntryInstructions.AddNew();

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		entryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
		Declaration.ConsignorDocAddress.OrganisationPK = consignorDocAddress.Organisation.PK;
		AssertNoWarning("Simplified Import", Declaration.ConsignorDocAddress.OrganisationPKInfo, warning);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		Declaration.ConsignorDocAddress.Validation.ValidateOrganisationPK();
		AssertNoWarning("Simplified ExportDeclarationActivation", Declaration.ConsignorDocAddress.OrganisationPKInfo, warning);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		Declaration.ConsignorDocAddress.Validation.ValidateOrganisationPK();
		AssertHasWarning("Simplified Export", Declaration.ConsignorDocAddress.OrganisationPKInfo, warning);

		entryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
		Declaration.ConsignorDocAddress.Validation.ValidateOrganisationPK();
		AssertNoWarning("Ordinary Export", Declaration.ConsignorDocAddress.OrganisationPKInfo, warning);

		entryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
		Declaration.ConsignorDocAddress.OrganisationPK = ZGuid.Empty;
		Declaration.ConsignorDocAddress.Validation.ValidateOrganisationPK();
		AssertNoWarning("Simplified Export without ConsignorDocAddress", Declaration.ConsignorDocAddress.OrganisationPKInfo, warning);
	});

	public void TestJobDeclarationSynchroniser()
	{
		var shipment = Factory.New<ForwardingShipment>();
		Declaration.JE_JS = shipment.PK;

		AssertType<JobDeclarationSynchroniser>("JobDeclarationSynchroniser", Declaration.ShipmentSynchroniser);
	}

	public void TestJobDeclarationHasNoChangesAfterInvoiceLinePackageLinkedChangedAndSaved() => CombineAssertions(() =>
	{
		var declaration = InitializeDeclarationWithHeaderAndLine(JobMessageTypeList.Codes.Import);
		var bill = declaration.Bills.AddNew();
		bill.CU_BillNum = "ABC";
		var package = declaration.Packages.AddNew();
		package.CW_HouseBill = bill.CU_BillUniqueCode;
		package.CW_PackType = RefCusCodeTestHelper.UNPKGCodeAttributeYes;
		package.CW_PackQty = 0;
		Factory.Save();
		AssertEquals("Declaration prepared and saved", false, Declaration.HasChanges);

		var bindingLine1 = declaration.InvoiceLines[0].PackagesForInvoiceLinesForBindingOnly[0];
		bindingLine1.IsLinked = true;
		AssertEquals("Changed IsLinked", true, Declaration.HasChanges);
		Factory.Save();
		AssertEquals("Declaration saved after IsLinked change", false, Declaration.HasChanges);
	});

	public void TestMultipleKeysToUse()
	{
		var multipleKeySupport = (ISupportMultipleResourceStringData)Declaration;
		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;
			AssertSequencesEqual($"JE_MessageType={Declaration.JE_MessageType}", new[] { MessageTypeCodeList.Codes.Export }, multipleKeySupport.MultipleKeysToUse);
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.Import;
			AssertSequencesEqual($"JE_MessageType={Declaration.JE_MessageType}", new[] { MessageTypeCodeList.Codes.Import }, multipleKeySupport.MultipleKeysToUse);
			Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
			AssertSequencesEqual($"JE_MessageType={Declaration.JE_MessageType}", new[] { CHJobMessageTypeList.Codes.Export }, multipleKeySupport.MultipleKeysToUse);
		});
	}

	public void TestJE_OA_Representative_Default() => CombineAssertions(() =>
	{
		var declarant = Factory.NewWithValidTestData<OrgHeader>();
		var enteredAuthorizedConsignee = Factory.NewWithValidTestData<OrgHeader>();

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		Declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

		Declaration.JE_OA_Representative = ZGuid.Empty;

		Declaration.JE_ClearanceLocation = ClearanceLocation.CustomsOffice;
		AssertEquals("ClearanceLocation set to 1=CustomsOffice", ZGuid.Empty, Declaration.JE_OA_Representative);

		Declaration.JE_ClearanceLocation = ClearanceLocation.Domicile;
		AssertEquals("ClearanceLocation set to 2=Domicile", declarant.MainAddress.PK, Declaration.JE_OA_Representative);

		Declaration.JE_OA_Representative = enteredAuthorizedConsignee.MainAddress.PK;
		Declaration.JE_ClearanceLocation = ZString.Empty;
		Declaration.JE_ClearanceLocation = ClearanceLocation.Domicile;
		AssertEquals("Entered authorized consignee not changed", enteredAuthorizedConsignee.MainAddress.PK, Declaration.JE_OA_Representative);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		Declaration.JE_OA_Representative = ZGuid.Empty;
		Declaration.JE_ClearanceLocation = ZString.Empty;
		Declaration.JE_ClearanceLocation = ClearanceLocation.Domicile;
		AssertEquals("Not set when not Import", ZGuid.Empty, Declaration.JE_OA_Representative);
	});

	public void TestGetTemplateCopyStrategy()
	{
		var declaration = Factory.New<JobDeclarationForTesting>();
		var strategy = declaration.GetTemplateCopyStrategyExposed(declaration.Factory, CloneType.TemplateCopy);
		AssertType<JobDeclarationDeepCloneStrategy>(strategy);
	}

	JobDeclaration Declaration => declaration ??= Factory.NewWithValidTestData<JobDeclaration>();
	JobDeclaration declaration;

	protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

	class JobDeclarationForTesting : JobDeclaration
	{
		public JobDeclarationForTesting(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		public BusinessObject GetAddInfoChildExposed() => base.GetAddInfoChild();

		public SchemaGuidColumn GetChildForeignKeyColumnExposed() => base.GetChildForeignKeyColumn();

		internal bool SupportsChcPivotBetweenInvoiceLineAndPackingCoreExposed => base.SubscribeToDataRefreshOnInstantiationCore;

		public Customs.Business.JobDeclarationDeepCloneStrategy GetTemplateCopyStrategyExposed(BusinessObjectFactory alternateFactory, CloneType cloneType) => base.GetTemplateCopyStrategy(alternateFactory, cloneType);
	}
}
