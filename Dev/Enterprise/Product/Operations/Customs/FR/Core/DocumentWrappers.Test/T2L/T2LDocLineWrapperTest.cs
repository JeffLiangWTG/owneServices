using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.DocumentDelivery;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.FR.DocumentWrappers.Transit.Testing;

sealed class T2LDocLineWrapperTest : DocBaseWrapperTest
{
	public void TestExpeditionExportationCustomsFallBackNumber_DeltaGFallbackIsActive_True()
	{
		var fallbackSetting = new FallbackSettings();
		fallbackSetting.End = ZDateTime.Empty;
		fallbackSetting.Start = ZDateTime.Today.AddDays(-1);
		using (FRCustomsDataRegistry.Instance.DeltaGMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting))
		{
			AssertEquals("ExpeditionExportationCustomsFallBackNumber should be captured from entryLine.Header.FRCustomsFallbackNumber if DeltaGFallbackIsActive = true.", "2300009252", Wrapper.ExpeditionExportationCustomsFallBackNumber);
		}
	}

	public void TestExpeditionExportationCustomsFallBackNumber_DeltaGFallbackIsActive_False()
	{
		var fallbackSetting = new FallbackSettings();
		fallbackSetting.End = ZDateTime.Empty;
		fallbackSetting.Start = ZDateTime.Today.AddDays(1);
		using (FRCustomsDataRegistry.Instance.DeltaGMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting))
		{
			AssertEquals("ExpeditionExportationCustomsFallBackNumber should be empty if DeltaGFallbackIsActive = false.", ZString.Empty, Wrapper.ExpeditionExportationCustomsFallBackNumber);
		}
	}

	public void TestExpeditionExportationCustomsOfficePlusDescription()
	{
		SetUpCustomsOffice();

		AssertEquals("ExpeditionExportationCustomsOfficePlusDescription should be captured from entryLine.Declaration.JE_CustomsOffice plus it's description.", "FR004000 bureau", Wrapper.ExpeditionExportationCustomsOfficePlusDescription);

		void SetUpCustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FR004000", "bureau", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.Save();
		}
	}

	public void TestBox1DocType()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		entryLine.CL_LineNumber = 1;
		invoiceLine.JI_CL = entryLine.PK;
		invoiceLine.JI_Procedure = "1071F61";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		var wrapper = T2LDocLineWrapper.New(entryLine, Factory, false);
		AssertEquals("isForT2LF is false => ocType should be constant T2L.", T2LDocLineWrapper.T2L, wrapper.Box1DocType);

		wrapper = T2LDocLineWrapper.New(entryLine, Factory, true);
		AssertEquals("isForT2LF is true => DocType should be constant T2LF.", T2LDocLineWrapper.T2LF, wrapper.Box1DocType);
	}

	public void TestBox2SupplierEoriOfMainOffice()
	{
		AssertEquals("Box2SupplierEoriOfMainOffice should be captured from entryLine.Header.SupplierEoriOfMainOffice.", "FREORI0001", Wrapper.Box2SupplierEoriOfMainOffice);
	}

	public void TestBox2Supplier()
	{
		AssertType<Enterprise.DocumentWrappers.GenericWrappers.OrganisationWrapper>("Box2Supplier should be of type OrganisationWrapper.", Wrapper.Box2Supplier);
		AssertEquals("Box2Supplier should be captured from entryLine.Declaration.SupplierDocumentaryAddress.", "EASYLOG\nSUPPLIER ADDR. 1\nSUPPLIER ADDR. 2\nCITY STATE 111\nCOLOMBIA", Wrapper.Box2Supplier.ToString());
	}

	public void TestBox5Articles()
	{
		AssertEquals("Box5Articles should be captured from entryLine.Header.MergedLineCount.", "2", Wrapper.Box5Articles);
	}

	public void TestBox6PackageCount()
	{
		AssertEquals("Box6PackageCount should be captured from entryLine.Header.PackagesCount.", "5", Wrapper.Box6PackageCount);
	}

	public void TestBox8Importer()
	{
		AssertType<Enterprise.DocumentWrappers.GenericWrappers.OrganisationWrapper>("Box8Importer should be of type OrganisationWrapper.", Wrapper.Box8Importer);
		AssertEquals("Box8Importer should be captured from entryLine.Declaration.ImporterDocumentaryAddress.", "WTG\nIMPORTER ADDR. 1\nIMPORTER ADDR. 2\nCITY2 STATE2 222", Wrapper.Box8Importer.ToString());
	}

	public void TestBox14DeclarantRepresentativeEori()
	{
		AssertEquals("Box14DeclarantRepresentativeEori should be captured from entryLine.Header.RepresentativeOrDeclarantEoriOfMainOffice.", "FREORI0003", Wrapper.Box14DeclarantRepresentativeEori);
	}

	public void TestBox14DeclarantRepresentative()
	{
		AssertType<DocAddress>("Box14DeclarantRepresentative should be of type DocAddress.", Wrapper.Box14DeclarantRepresentative);
		AssertEquals("Box14DeclarantRepresentative should be captured from entryLine.Declaration.Declarant.", "#1\nDECLARANT CITY DECLARANT STATE 23133", Wrapper.Box14DeclarantRepresentative.ToString());
	}

	public void TestBox14FooterText()
	{
		AssertEquals("Box14DeclarantType should be captured from entryLine.Declaration.JE_CustomsProfile and entryLine.Declaration.JE_DeclarantType.Description", "N° agrément : Profile, Mode de représentation : Direct Representation", Wrapper.Box14FooterText);
	}

	public void TestBox17FinalDestinationCountry()
	{
		AssertEquals("Box17FinalDestinationCountry should be captured from the country description from entryLine.Declaration.FinalDestination.", "France", Wrapper.Box17FinalDestinationCountry);
	}

	public void TestBox19HasContainer()
	{
		CombineAssertions("Box 19 should show 1 when there is at least 1 container with a number. Should be 0 otherwise.", () =>
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var wrapper = T2LDocLineWrapper.New(entryLine, Factory, false);

			AssertEquals("Should be 0 if no container.", "0", wrapper.Box19HasContainer);

			declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.FCL;
			AssertEquals("Should be 0 if no container.", "0", wrapper.Box19HasContainer);

			var container = declaration.CusContainers.AddNew();
			AssertEquals("Should be 0 if no container with ContainerNumber.", "0", wrapper.Box19HasContainer);

			container.CO_ContainerNumber = "CNT1";
			AssertEquals("Should be 1 if one container with ContainerNumber.", "1", wrapper.Box19HasContainer);

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CNT2";
			AssertEquals("Should be 1 if two containers with ContainerNumber.", "1", wrapper.Box19HasContainer);
		});
	}

	public void TestBox21TransportNationality()
	{
		AssertEquals("Box21TransportNationality should be captured from entryLine.Declaration.JE_RN_NKTransportNationality.", "CN", Wrapper.Box21TransportNationality);
	}

	public void TestBox25ModeOfTransportAtTheBorder()
	{
		AssertEquals("Box25ModeOfTransportAtTheBorder should be captured from entryLine.Declaration.ModeOfTransportAtTheBorder.", "4", Wrapper.Box25ModeOfTransportAtTheBorder);
	}

	public void TestBox31PacksMarksAndNumbersBuilder()
	{
		AssertEquals("Box31PacksMarksAndNumbersBuilder should be built from entryLine.PackagingDetails.", "EntryLine.CL_Description\r\nNombre et nature :    3  CT\r\nMarques et numéro :    IND\r\nNombre et nature :    2  RR\r\nMarques et numéro :    ARG", Wrapper.Box31PacksMarksAndNumbersBuilder);
	}

	public void TestBox32EntryLineNumber()
	{
		AssertEquals("Box32EntryLineNumber should be captured from entryLine.CL_LineNumber.", "1", Wrapper.Box32EntryLineNumber);
	}

	public void TestBox33Tariff()
	{
		AssertEquals("Box33Tariff should be captured from entryLine.Tariff.", "29291010", Wrapper.Box33Tariff);
	}

	public void TestBox35TotalInvoiceLinesGrossWeight()
	{
		AssertEquals("Box35TotalInvoiceLinesGrossWeight should be captured from entryLine.TotalInvoiceLinesGrossWeightInKG and rounded to 3 decimal places.", "13.313", Wrapper.Box35TotalInvoiceLinesGrossWeight);
	}

	public void TestBox38CustomsQuantity()
	{
		AssertEquals("Box38CustomsQuantity should be captured from entryLine.CustomsQuantity and rounded to 3 decimal places.", "20.239", Wrapper.Box38CustomsQuantity);
	}

	public void TestBox44AddInfoAndDocuments()
	{
		SetUpRefData();
		var entryLineForTest = GetEntryLineForTest();

		CombineAssertions("Box 44 content", () =>
		{
			var result = T2LDocLineWrapper.New(entryLineForTest, Factory, false).Box44AddInfoAndDocuments;
			string expected = @"
Mention(s) Spéciale(s): BOB21-MADE SENSE AT THE TIME; HIT99-RANDOM SONG TITLE; GEN13-PLENTY OF COATS; PAL01
CANA(s) : V910; V911
Document(s) joint(s) : HDR1 BILLY; 9100 3278923 10/12/2021
Disposition(s) tarifaire(s) particulière(s) : HNY1; 9120".TrimStart(new[] { '\r', '\n' });
			AssertEquals("entryLine.Box44Contents", expected, result);

			entryLineForTest.CL_LineNumber = 2;
			expected = @"Mention(s) Spéciale(s): GEN13-PLENTY OF COATS; PAL01";
			result = T2LDocLineWrapper.New(entryLineForTest, Factory, false).Box44AddInfoAndDocuments;
			AssertContains("entryLine.Box44Contents for non-first pages", expected, result);
		});

		CusEntryLine GetEntryLineForTest()
		{
			var declaration = Factory.New<JobDeclaration>();

			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Procedure = "1071F61";
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			declaration.JE_OH_Importer = importer.PK;

			var euAddInfo = EU.Business.EUOrgImpAddInfo.Get(importer, Core.Constants.CountryCodes.France);
			euAddInfo.ZO_UseFr3FiscalRepresentation = true;

			var fiscalReferenceOrganisation = Factory.New<OrgHeader>();
			fiscalReferenceOrganisation.OH_Code = "FISCALREP";

			var fiscalReferenceOrganisationAddress = fiscalReferenceOrganisation.Addresses.AddNew();
			fiscalReferenceOrganisationAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
			fiscalReferenceOrganisationAddress.OA_Address1 = "fiscal address1";
			fiscalReferenceOrganisationAddress.OA_Address2 = "fiscal address2";
			fiscalReferenceOrganisationAddress.OA_City = "fiscal city";
			fiscalReferenceOrganisationAddress.OA_PostCode = "333";
			fiscalReferenceOrganisationAddress.OA_RN_NKCountryCode = "FR";

			var fiscalReference = entryInstruction.FiscalReferences.AddNew();
			fiscalReference.CFR_Code = "FR3";
			fiscalReference.CFR_Reference = "FR33562024100133";
			fiscalReference.CFR_OA_Owner = fiscalReferenceOrganisationAddress.PK;

			var warehouseAddress = Factory.New<OrgAddress>();
			entryInstruction.CEI_OA_Warehouse = warehouseAddress.PK;

			var cusAuthorisationHeader = warehouseAddress.Factory.NewWithValidTestData<Customs.Business.CusAuthorisationHeader>();
			cusAuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			cusAuthorisationHeader.CPH_Type = Enterprise.Customs.FR.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
			cusAuthorisationHeader.CPH_OH_PermitHolder = ZGuid.Empty;
			cusAuthorisationHeader.CPH_OA_AppliesTo = warehouseAddress.PK;
			cusAuthorisationHeader.CPH_Number = "00001099";

			var suppDocHeader = invoiceHeader.SupportingDocuments.AddNew();
			suppDocHeader.CSI_Code = "HDR1";
			suppDocHeader.CSI_ReferenceNumber = "BILLY";
			suppDocHeader.CSI_SubType = "X";
			suppDocHeader.CSI_Quantity = 99;
			suppDocHeader.CSI_Description = "COZ WE WANT TO";

			var addInfoHeader = invoiceHeader.AdditionalInfos.AddNew();
			addInfoHeader.CSI_Code = "HIT99";
			addInfoHeader.CSI_Description = "RANDOM SONG TITLE";

			var suppDocGroup = declaration.SupportingDocuments.AddNew();
			suppDocGroup.CSI_Code = "HNY1";
			suppDocGroup.CSI_ReferenceNumber = "PIPER";
			suppDocGroup.CSI_SubType = "2";
			suppDocGroup.CSI_Quantity = 22;
			suppDocGroup.CSI_Description = "HONEY TO THE BEE";
			suppDocGroup.CSI_IsDTP = true;

			var addInfoGroup = declaration.AdditionalInfos.AddNew();
			addInfoGroup.CSI_Code = "BOB21";
			addInfoGroup.CSI_Description = "MADE SENSE AT THE TIME";

			var addInfo1 = invoiceLine.AdditionalInfos.AddNew();
			addInfo1.CSI_Code = "GEN13";
			addInfo1.CSI_Description = "PLENTY OF COATS";

			var addInfo2 = invoiceLine.AdditionalInfos.AddNew();
			addInfo2.CSI_Code = "PAL01";

			var cana1 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			cana1.CY_Code = "V911";

			var cana2 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			cana2.CY_Code = "V910";

			var document1 = invoiceLine.SupportingDocuments.AddNew();
			document1.CSI_Code = "9100";
			document1.CSI_ReferenceNumber = "3278923";
			document1.CSI_SubType = "1";
			document1.CSI_Quantity = 12;
			document1.CSI_Description = "I HAVE NO CLUE";
			document1.CSI_DateOfIssue = new ZDate(2021, 12, 10);

			var document2 = invoiceLine.SupportingDocuments.AddNew();
			document2.CSI_Code = "9120";
			document2.CSI_ReferenceNumber = "45982309";
			document2.CSI_Description = "9120 Test";
			document2.CSI_IsDTP = true;

			return entryLine;
		}

		void SetUpRefData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var addInfCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation;
			var attributeNameValuePairs = new Dictionary<string, string[]>();

			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "EX", "10", "71", "F61", "", "EXP", "10P");
			procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.No;
			procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;

			attributeNameValuePairs.Add("Level", new string[] { "ITEM", "HEADER" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.France, new string[] { importCodeType, exportCodeType }, "HDR1", "COZ WE WANT TO", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			attributeNameValuePairs.Clear();
			attributeNameValuePairs.Add("Level", new string[] { "HEADER" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.France, new string[] { importCodeType, exportCodeType }, "HNY1", "HONEY TO THE BEE", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.France, new string[] { addInfCodeType }, "HIT99", "RANDOM SONG TITLE", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			attributeNameValuePairs.Clear();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.France, new string[] { importCodeType, exportCodeType }, "9100", "I HAVE NO CLUE", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.France, new string[] { importCodeType, exportCodeType }, "9120", "9120 Test", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			Factory.Save();
		}
	}

	public void TestBox50DeclarantRepresentative()
	{
		AssertType<DocAddress>("Box50DeclarantRepresentative should be of type DocAddress.", Wrapper.Box50DeclarantRepresentative);
		AssertEquals("Box50DeclarantRepresentative should be captured from entryLine.Declaration.Declarant.", "#1\nDECLARANT CITY DECLARANT STATE 23133", Wrapper.Box50DeclarantRepresentative.ToString());
	}

	public void TestBox50RepresentativeCusAgentFullName()
	{
		AssertEquals("Box50RepresentativeCusAgentFullName should be captured from entryLine.Declaration.CusAgent.FullName.", "Broker Name", Wrapper.Box50RepresentativeCusAgentFullName);
	}

	public void TestBox50RepresentativeCity()
	{
		AssertEquals("Box50RepresentativeCity should be captured from entryLine.Declaration.Declarant.City,", "Declarant City", Wrapper.Box50RepresentativeCity);
	}

	public void TestBox50BAEDate()
	{
		AssertEquals("Box50BAEDate should be captured from entryLine.EntryHeader.BAEDate", "31/05/2023", Wrapper.Box50BAEDate);
	}

	public void TestBox52GuaranteeSubType()
	{
		AssertEquals("Box52GuaranteeSubType should be captured from entryLine.CustomsGuarantee.CPH_SubType.", "SBT", Wrapper.Box52GuaranteeSubType);
	}

	[TestDate(2023, 05, 31)]
	public void TestBox54DeclarationHomePortDescriptionAndTodayDate()
	{
		AssertEquals("Box54DeclarationHomePortDescriptionAndTodayDate should be captured from entryLine.Declaration.Branch.HomePort plus date of today.", "HomePort Desc.\r\n31/05/2023", Wrapper.Box54DeclarationHomePortDescriptionAndTodayDate);
	}

	public void TestBox54SignatoryNameAndPosition()
	{
		AssertEquals("Box54SignatoryNameAndPosition should be captured from entryLine.Declaration.CusAgent.FullName and title.", "Broker Name (Senior)", Wrapper.Box54SignatoryNameAndPosition);
	}

	protected override DocBaseWrapper GetNewDocumentWrapper()
	{
		var supplier = SetUpSupplier();
		var importer = SetUpImporter();
		var declarant = SetUpDeclarant();
		SetUpGuarantee();
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_CustomsOffice = "FR004000";
		declaration.JE_OH_Supplier = supplier.PK;
		declaration.JE_OH_Importer = importer.PK;
		declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
		declaration.JE_CustomsProfile = "Profile";
		declaration.JE_DeclarantType = "DIR";
		declaration.JE_RL_NKFinalDestination = "FRBOG";
		declaration.JE_RN_NKTransportNationality = "CN";
		declaration.JE_TransportMode = "AIR";
		declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
		SetUpBroker();
		SetUpBranch();

		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		invoiceHeader.FillWithValidTestData();
		invoiceHeader.JobComInvoiceLines.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.FillWithValidTestData();
		invoiceLine.JI_Tariff = "29291010";
		invoiceLine.JI_Weight = 13.3125m;
		invoiceLine.JI_WeightUQ = "KG";
		invoiceLine.JI_CustomsQuantity = 20.2385m;

		SetUpPackages();

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.FRCustomsFallbackNumber = "2300009252";
		entryHeader.CH_EntryReleaseDate = new ZDateTime(2023, 05, 31);

		var entryLine1 = entryHeader.MergedLines.AddNew();
		entryLine1.FillWithValidTestData();
		entryLine1.CL_LineNumber = 1;
		invoiceLine.JI_CL = entryLine1.PK;
		entryLine1.CL_Description = "EntryLine.CL_Description";
		var entryLine2 = entryHeader.MergedLines.AddNew();
		entryLine2.FillWithValidTestData();
		return T2LDocLineWrapper.New(entryLine1, Factory, false);

		OrgHeader SetUpSupplier()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "EasyLog";
			orgHeader.MainAddress.OA_Address1 = "Supplier Addr. 1";
			orgHeader.MainAddress.OA_Address2 = "Supplier Addr. 2";
			orgHeader.MainAddress.OA_City = "city";
			orgHeader.MainAddress.OA_State = "state";
			orgHeader.MainAddress.OA_PostCode = "111";
			orgHeader.MainAddress.OA_RL_NKRelatedPortCode = "COBOG";

			var cusCode = orgHeader.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "EORI0001";
			Factory.Save();

			return orgHeader;
		}

		OrgHeader SetUpImporter()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "WTG";
			orgHeader.MainAddress.OA_Address1 = "Importer Addr. 1";
			orgHeader.MainAddress.OA_Address2 = "Importer Addr. 2";
			orgHeader.MainAddress.OA_City = "city2";
			orgHeader.MainAddress.OA_State = "state2";
			orgHeader.MainAddress.OA_PostCode = "222";
			orgHeader.MainAddress.OA_RL_NKRelatedPortCode = "XXX";

			var cusCode = orgHeader.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "EORI0002";
			Factory.Save();

			return orgHeader;
		}

		OrgHeader SetUpDeclarant()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.City = "Declarant City";
			orgHeader.MainAddress.Postcode = "23133";
			orgHeader.MainAddress.State = "Declarant State";

			var cusCode = orgHeader.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "EORI0003";
			Factory.Save();

			return orgHeader;
		}

		void SetUpPackages()
		{
			var packingGroup = declaration.Bills.AddNew().PackingGroups.AddNew();
			packingGroup.CR_ClusterKey = 562;

			var basePackage = declaration.Packages.AddNew();
			basePackage.CW_PackQty = 3;
			basePackage.CW_PackType = "CT";
			basePackage.CW_MarksAndNos = "IND";
			basePackage.CW_ClusterKey = 562;
			basePackage.CW_CR_HouseContainer = packingGroup.PK;
			var basePackage2 = declaration.Packages.AddNew();
			basePackage2.CW_PackQty = 3;
			basePackage2.CW_PackType = "RR";
			basePackage2.CW_MarksAndNos = "ARG";
			basePackage2.CW_ClusterKey = 562;
			basePackage2.CW_CR_HouseContainer = packingGroup.PK;

			var packagesPivot = invoiceLine.PackagesPivot;
			var invoiceLinePackagePivot = packagesPivot.AddNew();
			invoiceLinePackagePivot.CHC_CW = basePackage.PK;
			invoiceLinePackagePivot.CHC_NumberOfPacks = 3;
			invoiceLinePackagePivot.CHC_JE = invoiceLine.Declaration.PK;
			invoiceLinePackagePivot.CHC_ClusterKey = 562;
			var invoiceLinePackagePivot2 = packagesPivot.AddNew();
			invoiceLinePackagePivot2.CHC_CW = basePackage2.PK;
			invoiceLinePackagePivot2.CHC_NumberOfPacks = 2;
			invoiceLinePackagePivot2.CHC_JE = invoiceLine.Declaration.PK;
			invoiceLinePackagePivot2.CHC_ClusterKey = 562;

			Factory.Save();
		}

		void SetUpBroker()
		{
			var broker = Factory.NewWithValidTestData<GlbStaff>();
			broker.GS_FullName = "Broker Name";
			broker.GS_Code = "BKC";
			broker.GS_Title = "Senior";
			declaration.JE_GS_NKCusAgent = "BKC";
		}

		void SetUpBranch()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var port = Factory.NewWithValidTestData<RefUNLOCO>();
			port.Code = "XD";
			port.Description = "HomePort Desc.";
			branch.GB_RL_NKHomePort = "XD";
			declaration.JE_GB = branch.PK;

			Factory.Save();
		}

		void SetUpGuarantee()
		{
			var guarantee = Business.Testing.GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "DGUA", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "#1", OrgCusAccountDeltaGTypeList.Codes.G1, "DECA", Core.Constants.CountryCodes.France);
			guarantee.CPH_SubType = "SBT";
			Factory.Save();
		}
	}

	[DatCapabilityRequirement("SOURCE_CODE")]
	public void TestT2LANdT2LfExcelTemplateMustHaveSamePrintArea()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.FillWithValidTestData();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.FillWithValidTestData();
		var invoiceHeader = declaration.Invoices.AddNew();

		Enumerable.Range(0, 4).ForEach(AddNewEntryLine);
		Factory.Save();

		var t2lfMenuItemPK = "a943f107-9b18-44d8-a5bc-f91d6629bc55";
		var excelT2LF = AssertNumberOfPrintedPages(1, t2lfMenuItemPK, entryHeader, "T2LF");

		var t2lMenuItemPK = "9d1eb5b1-86c8-45fb-909d-899d54d8836f";
		var excelT2L = AssertNumberOfPrintedPages(1, t2lMenuItemPK, entryHeader, "T2L");

		AssertEquals($"T2L and T2LF should be similar, if you have changed one don't forget to apply the change to the other. T2LF last modify time : {excelT2LF}, T2L last modify time : {excelT2L}", excelT2LF, excelT2L);

		ZString AssertNumberOfPrintedPages(int expectedPages, string documentMenuItemPk, BusinessObject bizO, string typeOfDocument)
		{
			if (!(bizO is IDocumentSupportable))
			{
				Assert($"The Bizo: {bizO.HumanReadableName} does not implement the {nameof(IDocumentSupportable)}. Test cannot be run", false);
			}

			if (!(bizO is IDocManagerSupport))
			{
				Assert($"The Bizo: {bizO.HumanReadableName} does not implement the {nameof(IDocManagerSupport)}. Test cannot be run", false);
			}

			var documentSupportable = bizO as IDocumentSupportable;
			var docManagerSupport = bizO as IDocManagerSupport;

			var t2lxMenuItemPK = new ZGuid(documentMenuItemPk);
			var documentCommandThatWeWillFireAsIfUserClickedIt = Factory.LoadTop1<DocumentCommand>(new DocumentZQuery(StmMenuItemSchema.PK, t2lxMenuItemPK));

			var silentDocumentPrinter = new SilentDocumentPrinter(Factory, documentSupportable, documentCommandThatWeWillFireAsIfUserClickedIt);
			silentDocumentPrinter.Print(ZGuid.Empty, 0, true, forceCorrectBizObjWhenPrintingToBothEdocsAndPaper: true, businessObjectForPrintJobParent: docManagerSupport.DocManagerInfo());
			Factory.Save();

			var filterQuery = new ZQuery(StmPrintJobSchema.SP_DocumentName, SQLComparisonOperator.Contains, typeOfDocument);

			var printedJobs = Factory.Load<StmPrintJob>(filterQuery);
			AssertGreaterThanOrEqualTo($"[PRE-CONDITION] Printed Jobs Count for {documentCommandThatWeWillFireAsIfUserClickedIt.SU_MenuName}", printedJobs.Length, 1);

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(printedJobs.FirstOrDefault().SP_CustomProperties);
				using (var stream = new MemoryStream())
				using (var pdfExport = new FlexCelPdfExportSafe(excelInterface.Xls, true))
				{
					pdfExport.Export(stream);
					var streamReader = new StreamReader(stream);

					var export = streamReader.ReadToEnd();

					var message = @"Looks like you have changed the print area by accident, if you have to do so, please double check that you have not broken the report, see discussion for details:
		https://teams.microsoft.com/l/message/19:333de70778314179b35dead529441dc1@thread.skype/1641988430883?tenantId=8b493985-e1b4-4b95-ade6-98acafdbdb01&groupId=188ba793-8136-4b10-b984-10097d7bb40c&parentMessageId=1641988430883&teamName=Development%20Customs%20Team&channelName=General&createdTime=1641988430883";
					AssertEquals(message, expectedPages, pdfExport.Progress.TotalPage);

					var file = new FileInfo(Path.Combine(TestCase.BaseSourcePath, $@"Enterprise\Product\Documents\ExcelTemplates\Documents\Customs\FR\{typeOfDocument}.xlsx"));

					if (file.Exists)
					{
						var dateOfModification = file.LastWriteTimeUtc;
					}

					return file.Exists ? file.LastWriteTimeUtc.ToString("d") : string.Empty;
				}
			}
		}

		void AddNewEntryLine(int itemNumber)
		{
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.FillWithValidTestData();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.FillWithValidTestData();
			entryLine.CL_Description = new ZString(itemNumber.ToString())
				.PadRight(CusEntryLineSchema.CL_Description.MaxLength, itemNumber.ToString().ToCharArray().First());
			invoiceLine.JI_CL = entryLine.PK;
		}
	}

	new T2LDocLineWrapper Wrapper => (T2LDocLineWrapper)base.Wrapper;
}
