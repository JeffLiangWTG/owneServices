using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;
using OrgAddress = Enterprise.MasterFiles.Business.OrgAddress;

namespace Enterprise.Customs.ES.Business.Declaration.Testing;

[TestedType(typeof(CusEntryInstruction))]
sealed class CusEntryInstructionTest : Customs.Business.Testing.CusEntryInstructionAbstractTest
{
	public void TestJobDeclaration()
	{
		var dec = Factory.New<JobDeclaration>();
		cusEntryInstruction.CEI_JE = dec.PK;
		AssertType<JobDeclaration>(cusEntryInstruction.JobDeclaration);
	}

	public void TestLookups()
	{
		AssertType<CusEntryInstructionLookups>(cusEntryInstruction.Lookups);
	}

	public void TestValidation()
	{
		AssertType<CusEntryInstructionValidation>(cusEntryInstruction.Validation);
	}

	public void TestAddInfo()
	{
		AssertType<AddInfoCusEntryInstruction>(cusEntryInstruction.AddInfo);
	}

	public void TestSupportingDocuments()
	{
		AssertType<SupportingDocumentCollection>(cusEntryInstruction.SupportingDocuments);
	}

	public void TestAdditionalInfos()
	{
		AssertType<AdditionalInfoCollection>(cusEntryInstruction.AdditionalInfos);
	}

	public void TestPreviousDocuments()
	{
		AssertType<PreviousDocumentCollection>(cusEntryInstruction.PreviousDocuments);
	}

	public void TestGetCusSupportingInfoTypes_PreviousDocument()
	{
		AssertEquals(typeof(EU.Business.Declaration.MultiLineAddInfos.PreviousDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)cusEntryInstruction).GetCusSupportingInfoTypes()[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument]);
	}

	public void TestGetCusSupportingInfoTypes_SupportingDocument()
	{
		AssertEquals(typeof(SupportingDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)cusEntryInstruction).GetCusSupportingInfoTypes()[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument]);
	}

	public void TestGetCusSupportingInfoTypes_AdditionalInfo()
	{
		AssertEquals(typeof(AdditionalInfo), ((Integration.Customs.ICusSupportingInfoTypeSupporter)cusEntryInstruction).GetCusSupportingInfoTypes()[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
	}

	public void TestCodeProperty()
	{
		cusEntryInstruction.CEI_Style = "AA";
		cusEntryInstruction.CEI_SubStyle = "BB";
		cusEntryInstruction.CEI_Description = "CC";
		var collection = new CodeDescriptionPairList();
		collection.AddRange(cusEntryInstruction);
		AssertNotEquals("style1", "CC", collection.GetDescriptionFromCode("AA"));
		AssertEquals("substyle", "CC", collection.GetDescriptionFromCode("BB"));
	}

	public void TestOrgCusCodeTypeForWarehouse()
	{
		var entryInstruction = Factory.New<CusEntryInstructionForTest>();
		AssertEquals(OrgCusCode.CodeTypes.ControlledPremisesID, entryInstruction.OrgCusCodeTypeForWarehouseExposed);
	}

	public void TestAddNewSupportingDocument()
	{
		CombineAssertions(() =>
		{
			AssertEquals("No supporting documents in invocieLine", false, cusEntryInstruction.SupportingDocuments.Any());

			cusEntryInstruction.AddNewSupportingDocument("AAA", "reference");
			AssertEquals("1 supporting documents in entryInstruction after calling method", 1, cusEntryInstruction.SupportingDocuments.Count);
			AssertEquals("SupportingDocument CSI_Code", "AAA", cusEntryInstruction.SupportingDocuments[0].CSI_Code);
			AssertEquals("SupportingDocument CSI_ReferenceNumber", "reference", cusEntryInstruction.SupportingDocuments[0].CSI_ReferenceNumber);
			AssertEquals("SupportingDocument CSI_DataModel", "ES", cusEntryInstruction.SupportingDocuments[0].CSI_DataModel);
		});
	}

	public void TestIsSubStyleBOrC()
	{
		CombineAssertions(() =>
		{
			cusEntryInstruction.CEI_SubStyle = "A";
			AssertEquals("IsSubStyleBOrC is false because substyle is A", false, cusEntryInstruction.IsSubStyleBOrC);

			cusEntryInstruction.CEI_SubStyle = "B";
			AssertEquals("IsSubStyleBOrC is true because substyle is B", true, cusEntryInstruction.IsSubStyleBOrC);

			cusEntryInstruction.CEI_SubStyle = "T2L";
			AssertEquals("IsSubStyleBOrC is false because substyle is T2L", false, cusEntryInstruction.IsSubStyleBOrC);

			cusEntryInstruction.CEI_SubStyle = "C";
			AssertEquals("IsSubStyleBOrC is true because substyle is C", true, cusEntryInstruction.IsSubStyleBOrC);
		});
	}

	public void TestIsSubStyleAOrBOrC()
		{
			CombineAssertions(() =>
			{
				cusEntryInstruction.CEI_SubStyle = "A";
				AssertEquals("IsSubStyleAOrBOrC is true because substyle is A", true, cusEntryInstruction.IsSubStyleAOrBOrC);

				cusEntryInstruction.CEI_SubStyle = "X";
				AssertEquals("IsSubStyleAOrBOrC is false because substyle is X", false, cusEntryInstruction.IsSubStyleAOrBOrC);

				cusEntryInstruction.CEI_SubStyle = "B";
				AssertEquals("IsSubStyleAOrBOrC is true because substyle is B", true, cusEntryInstruction.IsSubStyleAOrBOrC);

				cusEntryInstruction.CEI_SubStyle = "T2L";
				AssertEquals("IsSubStyleAOrBOrC is false because substyle is T2L", false, cusEntryInstruction.IsSubStyleAOrBOrC);

				cusEntryInstruction.CEI_SubStyle = "C";
				AssertEquals("IsSubStyleAOrBOrC is true because substyle is C", true, cusEntryInstruction.IsSubStyleAOrBOrC);
			});
		}

	public void TestIsSubStyleEXS()
		{
			CombineAssertions(() =>
			{
				cusEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				AssertEquals("IsSubStyleBOrC is false because substyle is A", false, cusEntryInstruction.IsEXS);

			cusEntryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
			AssertEquals("IsSubStyleBOrC is true because substyle is EXS", true, cusEntryInstruction.IsEXS);

			cusEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			AssertEquals("IsSubStyleBOrC is false because substyle is T2L", false, cusEntryInstruction.IsEXS);
		});
	}

	public void TestIsSubStyleBOrZ()
	{
		CombineAssertions(() =>
		{
			cusEntryInstruction.CEI_SubStyle = "A";
			AssertEquals("IsSubStyleBOrZ is false because substyle is A", false, cusEntryInstruction.IsSubStyleBOrZ);

			cusEntryInstruction.CEI_SubStyle = "B";
			AssertEquals("IsSubStyleBOrZ is true because substyle is B", true, cusEntryInstruction.IsSubStyleBOrZ);

			cusEntryInstruction.CEI_SubStyle = "T2L";
			AssertEquals("IsSubStyleBOrZ is false because substyle is T2L", false, cusEntryInstruction.IsSubStyleBOrZ);

			cusEntryInstruction.CEI_SubStyle = "Z";
			AssertEquals("IsSubStyleBOrZ is true because substyle is Z", true, cusEntryInstruction.IsSubStyleBOrZ);
		});
	}

	public void TestIsSubStyleYOrZ()
	{
		CombineAssertions(() =>
		{
			cusEntryInstruction.CEI_SubStyle = "A";
			AssertEquals("IsSubStyleYOrZ is false because substyle is A", false, cusEntryInstruction.IsSubStyleYOrZ);

			cusEntryInstruction.CEI_SubStyle = "Y";
			AssertEquals("IsSubStyleYOrZ is true because substyle is Y", true, cusEntryInstruction.IsSubStyleYOrZ);

			cusEntryInstruction.CEI_SubStyle = "T2L";
			AssertEquals("IsSubStyleYOrZ is false because substyle is T2L", false, cusEntryInstruction.IsSubStyleYOrZ);

			cusEntryInstruction.CEI_SubStyle = "Z";
			AssertEquals("IsSubStyleYOrZ is true because substyle is Z", true, cusEntryInstruction.IsSubStyleYOrZ);
		});
	}

	public void TestIsSubStyleBOrCOrZ()
	{
		CombineAssertions(() =>
		{
			cusEntryInstruction.CEI_SubStyle = "A";
			AssertEquals("IsSubStyleBOrCOrZ is false because substyle is A", false, cusEntryInstruction.IsSubStyleBOrCOrZ);

			cusEntryInstruction.CEI_SubStyle = "B";
			AssertEquals("IsSubStyleBOrCOrZ is true because substyle is B", true, cusEntryInstruction.IsSubStyleBOrCOrZ);

			cusEntryInstruction.CEI_SubStyle = "T2L";
			AssertEquals("IsSubStyleBOrCOrZ is false because substyle is T2L", false, cusEntryInstruction.IsSubStyleBOrCOrZ);

			cusEntryInstruction.CEI_SubStyle = "C";
			AssertEquals("IsSubStyleBOrCOrZ is true because substyle is C", true, cusEntryInstruction.IsSubStyleBOrCOrZ);

			cusEntryInstruction.CEI_SubStyle = "Y";
			AssertEquals("IsSubStyleBOrCOrZ is false because substyle is Y", false, cusEntryInstruction.IsSubStyleBOrCOrZ);

			cusEntryInstruction.CEI_SubStyle = "Z";
			AssertEquals("IsSubStyleBOrCOrZ is true because substyle is Z", true, cusEntryInstruction.IsSubStyleBOrCOrZ);
		});
	}

	public void TestIsSubStyleCOrYOrZ()
	{
		CombineAssertions(() =>
		{
			cusEntryInstruction.CEI_SubStyle = "A";
			AssertEquals("IsSubStyleCOrYOrZ is false because substyle is A", false, cusEntryInstruction.IsSubStyleCOrYOrZ);

			cusEntryInstruction.CEI_SubStyle = "C";
			AssertEquals("IsSubStyleCOrYOrZ is true because substyle is C", true, cusEntryInstruction.IsSubStyleCOrYOrZ);

			cusEntryInstruction.CEI_SubStyle = "T2L";
			AssertEquals("IsSubStyleCOrYOrZ is false because substyle is T2L", false, cusEntryInstruction.IsSubStyleCOrYOrZ);

			cusEntryInstruction.CEI_SubStyle = "Y";
			AssertEquals("IsSubStyleCOrYOrZ is true because substyle is Y", true, cusEntryInstruction.IsSubStyleCOrYOrZ);

			cusEntryInstruction.CEI_SubStyle = "B";
			AssertEquals("IsSubStyleCOrYOrZ is false because substyle is B", false, cusEntryInstruction.IsSubStyleCOrYOrZ);

			cusEntryInstruction.CEI_SubStyle = "Z";
			AssertEquals("IsSubStyleCOrYOrZ is true because substyle is Z", true, cusEntryInstruction.IsSubStyleCOrYOrZ);
		});
	}

	public void TestIsSubStyleAOrBOrCOrZ()
	{
		CombineAssertions(() =>
		{
			cusEntryInstruction.CEI_SubStyle = "A";
			AssertEquals("IsSubStyleAOrBOrCOrZ is true because substyle is A", true, cusEntryInstruction.IsSubStyleAOrBOrCOrZ);

			cusEntryInstruction.CEI_SubStyle = "T2L";
			AssertEquals("IsSubStyleAOrBOrCOrZ is false because substyle is T2L", false, cusEntryInstruction.IsSubStyleAOrBOrCOrZ);

			cusEntryInstruction.CEI_SubStyle = "B";
			AssertEquals("IsSubStyleAOrBOrCOrZ is true because substyle is B", true, cusEntryInstruction.IsSubStyleAOrBOrCOrZ);

			cusEntryInstruction.CEI_SubStyle = "T2C";
			AssertEquals("IsSubStyleAOrBOrCOrZ is false because substyle is T2C", false, cusEntryInstruction.IsSubStyleAOrBOrCOrZ);

			cusEntryInstruction.CEI_SubStyle = "C";
			AssertEquals("IsSubStyleAOrBOrCOrZ is true because substyle is C", true, cusEntryInstruction.IsSubStyleAOrBOrCOrZ);

			cusEntryInstruction.CEI_SubStyle = ZString.Empty;
			AssertEquals("IsSubStyleAOrBOrCOrZ is false because substyle is empty", false, cusEntryInstruction.IsSubStyleAOrBOrCOrZ);

			cusEntryInstruction.CEI_SubStyle = "Z";
			AssertEquals("IsSubStyleAOrBOrCOrZ is true because substyle is Z", true, cusEntryInstruction.IsSubStyleAOrBOrCOrZ);
		});
	}

	public void TestIsSubStyleAOrBOrCOrYOrZ()
	{
		CombineAssertions(() =>
		{
			cusEntryInstruction.CEI_SubStyle = "A";
			AssertEquals("IsSubStyleAOrBOrCOrYOrZ is true because substyle is A", true, cusEntryInstruction.IsSubStyleAOrBOrCOrYOrZ);

			cusEntryInstruction.CEI_SubStyle = "T2L";
			AssertEquals("IsSubStyleAOrBOrCOrYOrZ is false because substyle is T2L", false, cusEntryInstruction.IsSubStyleAOrBOrCOrYOrZ);

			cusEntryInstruction.CEI_SubStyle = "B";
			AssertEquals("IsSubStyleAOrBOrCOrYOrZ is true because substyle is B", true, cusEntryInstruction.IsSubStyleAOrBOrCOrYOrZ);

			cusEntryInstruction.CEI_SubStyle = "T2C";
			AssertEquals("IsSubStyleAOrBOrCOrYOrZ is false because substyle is T2C", false, cusEntryInstruction.IsSubStyleAOrBOrCOrYOrZ);

			cusEntryInstruction.CEI_SubStyle = "C";
			AssertEquals("IsSubStyleAOrBOrCOrYOrZ is true because substyle is C", true, cusEntryInstruction.IsSubStyleAOrBOrCOrYOrZ);

			cusEntryInstruction.CEI_SubStyle = "X";
			AssertEquals("IsSubStyleAOrBOrCOrYOrZ is false because substyle is X", false, cusEntryInstruction.IsSubStyleAOrBOrCOrYOrZ);

			cusEntryInstruction.CEI_SubStyle = "Y";
			AssertEquals("IsSubStyleAOrBOrCOrYOrZ is true because substyle is Y", true, cusEntryInstruction.IsSubStyleAOrBOrCOrYOrZ);

			cusEntryInstruction.CEI_SubStyle = ZString.Empty;
			AssertEquals("IsSubStyleAOrBOrCOrYOrZ is false because substyle is empty", false, cusEntryInstruction.IsSubStyleAOrBOrCOrYOrZ);

			cusEntryInstruction.CEI_SubStyle = "Z";
			AssertEquals("IsSubStyleAOrBOrCOrYOrZ is true because substyle is Z", true, cusEntryInstruction.IsSubStyleAOrBOrCOrYOrZ);
		});
	}

	public void TestIsSubStyleAOrBOrCOrXOrYOrZ()
	{
		CombineAssertions(() =>
		{
			cusEntryInstruction.CEI_SubStyle = "A";
			AssertEquals("IsSubStyleAOrBOrCOrXOrYOrZ is true because substyle is A", true, cusEntryInstruction.IsSubStyleAOrBOrCOrXOrYOrZ);

			cusEntryInstruction.CEI_SubStyle = "T2L";
			AssertEquals("IsSubStyleAOrBOrCOrXOrYOrZ is false because substyle is T2L", false, cusEntryInstruction.IsSubStyleAOrBOrCOrXOrYOrZ);

			cusEntryInstruction.CEI_SubStyle = "B";
			AssertEquals("IsSubStyleAOrBOrCOrXOrYOrZ is true because substyle is B", true, cusEntryInstruction.IsSubStyleAOrBOrCOrXOrYOrZ);

			cusEntryInstruction.CEI_SubStyle = "T2C";
			AssertEquals("IsSubStyleAOrBOrCOrXOrYOrZ is false because substyle is T2C", false, cusEntryInstruction.IsSubStyleAOrBOrCOrXOrYOrZ);

			cusEntryInstruction.CEI_SubStyle = "C";
			AssertEquals("IsSubStyleAOrBOrCOrXOrYOrZ is true because substyle is C", true, cusEntryInstruction.IsSubStyleAOrBOrCOrXOrYOrZ);

			cusEntryInstruction.CEI_SubStyle = ZString.Empty;
			AssertEquals("IsSubStyleAOrBOrCOrXOrYOrZ is false because substyle is empty", false, cusEntryInstruction.IsSubStyleAOrBOrCOrXOrYOrZ);

			cusEntryInstruction.CEI_SubStyle = "X";
			AssertEquals("IsSubStyleAOrBOrCOrXOrYOrZ is true because substyle is X", true, cusEntryInstruction.IsSubStyleAOrBOrCOrXOrYOrZ);

			cusEntryInstruction.CEI_SubStyle = ZString.Empty;
			AssertEquals("IsSubStyleAOrBOrCOrXOrYOrZ is false because substyle is empty", false, cusEntryInstruction.IsSubStyleAOrBOrCOrXOrYOrZ);

			cusEntryInstruction.CEI_SubStyle = "Y";
			AssertEquals("IsSubStyleAOrBOrCOrXOrYOrZ is true because substyle is Y", true, cusEntryInstruction.IsSubStyleAOrBOrCOrXOrYOrZ);

			cusEntryInstruction.CEI_SubStyle = ZString.Empty;
			AssertEquals("IsSubStyleAOrBOrCOrXOrYOrZ is false because substyle is empty", false, cusEntryInstruction.IsSubStyleAOrBOrCOrXOrYOrZ);

			cusEntryInstruction.CEI_SubStyle = "Z";
			AssertEquals("IsSubStyleAOrBOrCOrXOrYOrZ is true because substyle is Z", true, cusEntryInstruction.IsSubStyleAOrBOrCOrXOrYOrZ);
		});
	}

	public void TestIsSubStyleAOrBOrXOrZ()
	{
		CombineAssertions(() =>
		{
			cusEntryInstruction.CEI_SubStyle = "A";
			AssertEquals("IsSubStyleAOrBOrXOrZ is true because substyle is A", true, cusEntryInstruction.IsSubStyleAOrBOrXOrZ);

			cusEntryInstruction.CEI_SubStyle = ZString.Empty;
			AssertEquals("IsSubStyleAOrBOrXOrZ is false because substyle is empty", false, cusEntryInstruction.IsSubStyleAOrBOrXOrZ);

			cusEntryInstruction.CEI_SubStyle = "B";
			AssertEquals("IsSubStyleAOrBOrXOrZ is true because substyle is B", true, cusEntryInstruction.IsSubStyleAOrBOrXOrZ);

			cusEntryInstruction.CEI_SubStyle = "T2C";
			AssertEquals("IsSubStyleAOrBOrXOrZ is false because substyle is T2C", false, cusEntryInstruction.IsSubStyleAOrBOrXOrZ);

			cusEntryInstruction.CEI_SubStyle = "X";
			AssertEquals("IsSubStyleAOrBOrXOrZ is true because substyle is X", true, cusEntryInstruction.IsSubStyleAOrBOrXOrZ);

			cusEntryInstruction.CEI_SubStyle = "T2L";
			AssertEquals("IsSubStyleAOrBOrXOrZ is false because substyle is T2L", false, cusEntryInstruction.IsSubStyleAOrBOrXOrZ);

			cusEntryInstruction.CEI_SubStyle = "Z";
			AssertEquals("IsSubStyleAOrBOrXOrZ is true because substyle is Z", true, cusEntryInstruction.IsSubStyleAOrBOrXOrZ);
		});
	}

	public void TestIsStyleEmpty()
	{
		CombineAssertions(() =>
		{
			cusEntryInstruction.CEI_Style = "A";
			AssertEquals("IsStyleEmpty is true because style is A", false, cusEntryInstruction.IsStyleEmpty);

			cusEntryInstruction.CEI_Style = ZString.Empty;
			AssertEquals("IsStyleEmpty is false because style is empty", true, cusEntryInstruction.IsStyleEmpty);
		});
	}

	public void TestIsT2C()
	{
		CombineAssertions(() =>
		{
			cusEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			AssertEquals("IsT2C is true because substyle is T2C", true, cusEntryInstruction.IsT2C);

			cusEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			AssertEquals("IsT2C is false because substyle is T2L", false, cusEntryInstruction.IsT2C);
		});
	}

	public void TestIsT2L()
	{
		CombineAssertions(() =>
		{
			cusEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			AssertEquals("IsT2L is true because substyle is T2L", true, cusEntryInstruction.IsT2L);

			cusEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			AssertEquals("IsT2L is false because substyle is T2C", false, cusEntryInstruction.IsT2L);
		});
	}

	public void TestIsEXS()
	{
		CombineAssertions(() =>
		{
			cusEntryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
			AssertEquals("IsEXS is true because substyle is EXS", true, cusEntryInstruction.IsEXS);

			cusEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			AssertEquals("IsEXS is false because substyle is T2L", false, cusEntryInstruction.IsEXS);
		});
	}

	public void TestIsH2()
	{
		CombineAssertions(() =>
		{
			cusEntryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.A;
			AssertEquals("IsH2 is false because style is null", false, cusEntryInstruction.IsH2);

			cusEntryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
			AssertEquals("IsH2 is true because style is H2", true, cusEntryInstruction.IsH2);

			cusEntryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.IM;
			AssertEquals("IsH2 is false because style is not H2", false, cusEntryInstruction.IsH2);
		});
	}

	public void TestDJPProcedureAvailable()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.ActiveEntryHeaders.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
			entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
			AssertEquals("Case1: DJPProcedureAvailable is ture", true, entryInstruction.DJPProcedureAvailable);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Z;
			entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
			AssertEquals("Case2: DJPProcedureAvailable is ture", true, entryInstruction.DJPProcedureAvailable);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Z;
			entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
			AssertEquals("Case3: DJPProcedureAvailable is false", false, entryInstruction.DJPProcedureAvailable);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
			AssertEquals("Case4: DJPProcedureAvailable is false", false, entryInstruction.DJPProcedureAvailable);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;
			AssertEquals("Case5: DJPProcedureAvailable is false", false, entryInstruction.DJPProcedureAvailable);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
			entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
			entryHeader.CH_CEI_Instruction = ZGuid.Empty;
			AssertEquals("Case6: DJPProcedureAvailable is false", false, entryInstruction.DJPProcedureAvailable);
		});
	}

	public void TestActivateByOperator()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ZG_ActivateByOperator default is false", false, cusEntryInstruction.ZG_ActivateByOperator);

				cusEntryInstruction.ZG_ActivateByOperator = true;
				AssertEquals("ZG_ActivateByOperator", true, cusEntryInstruction.ZG_ActivateByOperator);

				var captionResourceString = DataBoundResourceStrings.GetDataForProperty(cusEntryInstruction.ZG_ActivateByOperatorInfo);
				AssertEquals("Caption", "Activate Pre Declaration by Operator", captionResourceString.Caption);
				AssertEquals("MediumCaption", "Active. Pre Dec. by Op.", captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", "Active. by Op.", captionResourceString.ShortCaption);
			});
		}

	public void TestActivateByOperatorReadOnly()
		{
			CombineAssertions(() =>
			{
				cusEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				AssertEquals("CEI_SubStyle is A and ZG_ActivateByOperator should not be readonly", false, cusEntryInstruction.ZG_ActivateByOperatorInfo.ReadOnly);

				cusEntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.X;
				AssertEquals("CEI_SubStyle is X and ZG_ActivateByOperator should be readonly", true, cusEntryInstruction.ZG_ActivateByOperatorInfo.ReadOnly);
			});
		}

	public void TestIncludeRoutingSecurityData()
		{
			AssertEquals("IncludeRoutingSecurityData default is false", false, cusEntryInstruction.IncludeRoutingSecurityData);

		cusEntryInstruction.IncludeRoutingSecurityData = true;
		AssertEquals("IncludeRoutingSecurityData", true, cusEntryInstruction.IncludeRoutingSecurityData);
	}

	public void TestCusSupplyChainActorReferences()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		AssertType<CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>>(nameof(entryInstruction.CusSupplyChainActorReferences), entryInstruction.CusSupplyChainActorReferences);
	}

	public void TestGetAuthorisationHeaderForDocs()
	{
		GetAutorisationForDocs(SupportingDocumentType.CentralizedClearance, CusAuthorizationHeaderTypeList.Codes.CentralizedClearance);
		GetAutorisationForDocs(SupportingDocumentType.EntryOfDataInDeclarantsRecords, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords);
	}

	void GetAutorisationForDocs(string docType, string authType)
	{
		var holder = Factory.New<OrgHeader>();
		holder.OH_Code = "AA";

		var authNumber = "55885598";

		var authorisation = Factory.New<CusAuthorisationHeader>();
		authorisation.CPH_OH_PermitHolder = holder.PK;
		authorisation.CPH_Type = authType;
		authorisation.CPH_StartDate = new ZDate(2021, 11, 03);
		authorisation.CPH_Number = authNumber;

		CombineAssertions(() =>
		{
			AssertNull("For Doc Type " + docType + " method returns null when entryInstruction CPH_Type = " + authType, cusEntryInstruction.GetAuthorisationHeaderForDocs(docType));

			var auth1 = cusEntryInstruction.CusAuthorizationUsages.AddNew();
			auth1.AGC_Code = authType;
			auth1.AGC_Number = authNumber;
			auth1.AGC_OH_Owner = holder.PK;
			AssertEquals("For Doc Type " + docType + " method returns authNumber when entryInstruction CPH_Type = " + authType, authNumber, cusEntryInstruction.GetAuthorisationHeaderForDocs(docType).CPH_Number);
		});
	}

	public void TestGetAutorisationRuleFor5018Doc()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.LocationsInAuthorisations, "LOCAT");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.LocationsInAuthorisations, "A001", "ES LOCAT 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.LocationsInAuthorisations, "A002", "ES LOCAT 2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		var holder = Factory.New<OrgHeader>();
		holder.OH_Code = "AA";

		var authType = "CW1";
		var authNumber = "Number";

		var authorisation = Factory.New<CusAuthorisationHeader>();
		authorisation.CPH_OH_PermitHolder = holder.PK;
		authorisation.CPH_Type = authType;
		authorisation.CPH_StartDate = new ZDate(2021, 11, 03);
		authorisation.CPH_Number = authNumber;

		CombineAssertions(() =>
		{
			AssertNull("Method returns null when entryInstruction has no CusAuthorizationUsages", cusEntryInstruction.GetAutorisationRuleFor5018Doc());

			var auth1 = cusEntryInstruction.CusAuthorizationUsages.AddNew();
			auth1.AGC_Code = authType;
			auth1.AGC_Number = authNumber;
			auth1.AGC_OH_Owner = holder.PK;

			var authorisationRule = authorisation.CusAuthorisationRules.AddNew();
			authorisationRule.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
			authorisationRule.CPR_ValueFrom = ZString.Empty;
			AssertNull("Method returns rule when entryInstruction has one CusAuthorizationUsage associated to one Authorisation but LOC rule is not correct (empty)", cusEntryInstruction.GetAutorisationRuleFor5018Doc());

			authorisationRule.CPR_ValueFrom = "ES123456789";
			AssertNull("Method returns rule when entryInstruction has one CusAuthorizationUsage associated to one Authorisation but LOC rule is not correct (not in database)", cusEntryInstruction.GetAutorisationRuleFor5018Doc());

			var authorisationRule2 = authorisation.CusAuthorisationRules.AddNew();
			authorisationRule2.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
			authorisationRule2.CPR_ValueFrom = "A001";
			var returnedRule = cusEntryInstruction.GetAutorisationRuleFor5018Doc();
			AssertNotNull("Method returns rule when entryInstruction has one CusAuthorizationUsage associated to one Authorisation with only one correct LOC rule", returnedRule);
			AssertEquals("Returned rule is the correct one", authorisationRule2, returnedRule);

			var authorisationRule3 = authorisation.CusAuthorisationRules.AddNew();
			authorisationRule3.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
			authorisationRule3.CPR_ValueFrom = "A002";
			AssertNull("Method returns null when entryInstruction has one CusAuthorizationUsage associated to one Authorisation with multiple correct LOC rules", cusEntryInstruction.GetAutorisationRuleFor5018Doc());

			auth1.AGC_Code = "AAA";
			AssertNull("Method returns null when entryInstruction has one CusAuthorizationUsage but the code is not correct", cusEntryInstruction.GetAutorisationRuleFor5018Doc());
		});
	}

	public void TestGetGoodsLocation()
	{
		var goodsLocation = cusEntryInstruction.GoodsLocation;
		CombineAssertions(() =>
		{
			AssertType<CusGoodsLocation>(goodsLocation);
			AssertEquals("CGL_ParentID", cusEntryInstruction.PK, goodsLocation.CGL_ParentID);
			AssertEquals("CGL_ParentTableCode", ZArchitecture.Schema.CusEntryInstructionSchema.Constants.Prefix, goodsLocation.CGL_ParentTableCode);
			AssertEquals("CGL_LocationUse", CusGoodsLocationUseList.Codes.EntryInstruction, goodsLocation.CGL_LocationUse);
			AssertSame("Cached", goodsLocation, cusEntryInstruction.GoodsLocation);
			AssertEquals("IsRegisteredEditableChildObject", true, cusEntryInstruction.IsRegisteredEditableChildObject(goodsLocation));
		});
	}

	public void TestSetGoodsLocationReadOnly()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var goodsLocation = entryInstruction.GoodsLocation;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		SetUpPremises();

		var testCases = new[]
		{
			(messageType: MessageTypeList.Codes.Import, entrySubStyle: EntrySubStyleList.Codes.A, entryStatus: EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, messageStatus: MessageStatusList.Codes.NotSent, shouldBeReadOnly: true, assertMessageText: "For Import Declaration, Message Status not AWR and Entry Status CLP"),
			(messageType: MessageTypeList.Codes.Import, entrySubStyle: EntrySubStyleList.Codes.A, entryStatus: EntryStatusCodes.ClearedWithPendingDocuments, messageStatus: MessageStatusList.Codes.NotSent, shouldBeReadOnly: false, assertMessageText: "For Import Declaration, Message Status not AWR but Entry Status not CLP or CLR"),
			(messageType: MessageTypeList.Codes.Import, entrySubStyle: EntrySubStyleList.Codes.T2C, entryStatus: EntryStatusCodes.Cleared, messageStatus: MessageStatusList.Codes.NotSent, shouldBeReadOnly: true, assertMessageText: "For Import Declaration, Message Status not AWR but Entry Status CLR"),
			(messageType: MessageTypeList.Codes.Import, entrySubStyle: EntrySubStyleList.Codes.A, entryStatus: EntryStatusCodes.ClearedWithPendingDocuments, messageStatus: MessageStatusList.Codes.SentAndRejected, shouldBeReadOnly: false, assertMessageText: "For Import Declaration, Entry Status not CLP or CLR and Message Status no AWR"),
			(messageType: MessageTypeList.Codes.Import, entrySubStyle: EntrySubStyleList.Codes.T2L, entryStatus: EntryStatusCodes.ClearedWithPendingDocuments, messageStatus: MessageStatusList.Codes.AwaitingResponse, shouldBeReadOnly: true, assertMessageText: "For Import Declaration, Entry Status not CLP or CLR and Message Status AWR"),
			(messageType: MessageTypeList.Codes.Export, entrySubStyle: EntrySubStyleList.Codes.A, entryStatus: EntryStatusCodes.Cleared, messageStatus: MessageStatusList.Codes.AwaitingResponse, shouldBeReadOnly: false, assertMessageText: "For Export Declaration not EXS, Entry Status CLR, Message Status AWR"),
			(messageType: MessageTypeList.Codes.Export, entrySubStyle: ExsEntrySubStyleList.Codes.EXS, entryStatus: EntryStatusCodes.ClearedWithPendingDocuments, messageStatus: MessageStatusList.Codes.AwaitingResponse, shouldBeReadOnly: true, assertMessageText: "For Export Declaration EXS, Entry Status not CLR, Message Status AWR"),
			(messageType: MessageTypeList.Codes.Export, entrySubStyle: ExsEntrySubStyleList.Codes.EXS, entryStatus: EntryStatusCodes.ClearedWithPendingDocuments, messageStatus: MessageStatusList.Codes.NotSent, shouldBeReadOnly: false, assertMessageText: "For Export Declaration EXS, Entry Status not CLR, Message Status not AWR"),
			(messageType: MessageTypeList.Codes.Export, entrySubStyle: ExsEntrySubStyleList.Codes.EXS, entryStatus: EntryStatusCodes.Cleared, messageStatus: MessageStatusList.Codes.NotSent, shouldBeReadOnly: true, assertMessageText: "For Export Declaration EXS, Entry Status CLR, Message Status not AWR"),
		};

		foreach (var (messageType, entrySubStyle, entryStatus, messageStatus, shouldBeReadOnly, assertMessageText) in testCases)
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = messageType;
				entryInstruction.CEI_SubStyle = entrySubStyle;
				goodsLocation.Address.AuthorisationNumber = "9999000002";
				entryHeader.CH_EntryStatus = entryStatus;
				entryHeader.CH_Status = messageStatus;
				var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;

				if (shouldBeReadOnly)
				{
					using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
					{
						entryInstruction.SetGoodsLocationReadOnly();
						AssertEquals($"{assertMessageText} Temporary Storage not enable, GoodsLocation should not be readOnly", false, goodsLocation.ReadOnly);
					}
				}

				using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					entryInstruction.SetGoodsLocationReadOnly();
					AssertEquals($"{assertMessageText} Temporary Storage enable and Location manage in premises, GoodsLocation readOnly should be {shouldBeReadOnly}", shouldBeReadOnly, goodsLocation.ReadOnly);
					if (shouldBeReadOnly)
					{
						goodsLocation.Address.AuthorisationNumber = "9999000000";
						entryInstruction.SetGoodsLocationReadOnly();
						AssertEquals($"{assertMessageText} Temporary Storage enable but Location not manage in premises, GoodsLocation should not be readOnly", false, goodsLocation.ReadOnly);
					}
				}
			});
		}

		void SetUpPremises()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";

			var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
			premises.SRP_Type = "ADT";
			premises.SRP_CustomsLocation = "9999000002";
			premises.SRP_Code = "X";
			premises.SRP_Description = "DESC";
			premises.SRP_OA_PremisesAddress = orgAddress.PK;
		}
	}

	protected override void SetUp()
	{
		cusEntryInstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
	}
	CusEntryInstruction cusEntryInstruction;
}

class CusEntryInstructionForTest : CusEntryInstruction
{
	public CusEntryInstructionForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public ZString OrgCusCodeTypeForWarehouseExposed => OrgCusCodeTypeForWarehouse;
}
