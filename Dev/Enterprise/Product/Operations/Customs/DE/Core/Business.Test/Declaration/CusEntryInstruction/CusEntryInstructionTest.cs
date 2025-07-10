using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryInstruction))]
	sealed class CusEntryInstructionTest : Customs.Business.Testing.CusEntryInstructionAbstractTest
	{
		public void TestUniversalCopy()
		{
			var cusAuthorizationUsages = typeof(CusEntryInstruction).GetProperty(nameof(CusEntryInstruction.CusAuthorizationUsages), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			Assert(Attribute.IsDefined(cusAuthorizationUsages, typeof(UniversalCopyCollectionEntityAttribute)));
		}

		public void TestDocAddresses()
		{
			AssertEquals(true, cei.IsRegisteredEditableChildObject(cei.DocAddresses));
		}

		public void TestIDocAddresses()
		{
			CombineAssertions(() =>
			{
				var provider = (IDocAddresses)cei;
				AssertEquals("GetCanOverrideCheckpoint", Env.Security.None, provider.GetCanOverrideCheckpoint(cei.MainAccountingAddress));
				AssertEquals("HumanReadableName", cei.HumanReadableName, provider.HumanReadableName);
				AssertType<CusEntryInstructionJobDocAddressValidation>("PiggyBackedDocAddressValidation", provider.PiggyBackedDocAddressValidation(cei.MainAccountingAddress));
				AssertContainsExactElementsInAnyOrder("SupportedAddressTypes", new[] { DocAddressType.MainAccountingAddress }, provider.SupportedAddressTypes);
				AssertNull("GetDocAddressRequirement", provider.GetDocAddressRequirement(DocAddressType.MainAccountingAddress));
				AssertEquals("CanDeleteAddress", false, provider.CanDeleteAddress(null));
				AssertNull("GetOrgHeaderList", provider.GetOrgHeaderList(DocAddressType.MainAccountingAddress));
			});
		}

		public void TestMainAccountingAddress()
		{
			AssertNotNull(cei.MainAccountingAddress);
		}

		public void TestIsMainAccountingAddressAvailable()
		{
			CombineAssertions(() =>
			{
				var instruction = Factory.CreateInwardProcessingInstruction();
				AssertEquals("default", false, instruction.IsMainAccountingAddressAvailable);
				instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.J;
				AssertEquals("J", true, instruction.IsMainAccountingAddressAvailable);
				instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.N;
				AssertEquals("N", false, instruction.IsMainAccountingAddressAvailable);
			});
		}

		public void TestISequenceNumberHeader()
		{
			var instruction = Factory.CreateInwardProcessingInstruction();
			var sequenceHeader = (ISequenceNumberHeader)instruction;
			AssertEquals(0, sequenceHeader.Lines.Count());
			instruction.InwardProcessingPlaces.AddNew();
			AssertEquals(1, sequenceHeader.Lines.Count());
		}

		public void TestEffectiveCode_Import()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			var instruction = dec.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
			instruction.CEI_SubStyle = ImportSubStyleList.Codes.A;
			AssertEquals("AAV", CodePropertyAttribute.CodeFromBusinessObject(instruction));
		}

		public void TestEffectiveCode_Export()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = MessageTypeList.Codes.Export;
			var instruction = dec.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000100;
			instruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._00;
			AssertEquals("00|000100", CodePropertyAttribute.CodeFromBusinessObject(instruction));
		}

		public void TestEffectiveDescription_Import()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			var instruction = dec.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
			instruction.CEI_SubStyle = ImportSubStyleList.Codes.A;
			instruction.CEI_Description = "Micro-Star international";
			AssertEquals("A - Micro-Star international", DescriptionPropertyAttribute.DescriptionFromBusinessObject(instruction));
		}

		public void TestEffectiveDescription_Export()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = MessageTypeList.Codes.Export;
			var instruction = dec.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000100;
			instruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._00;
			instruction.CEI_Description = "Micro-Star international";
			AssertEquals("Micro-Star international", DescriptionPropertyAttribute.DescriptionFromBusinessObject(instruction));
		}

		public void TestEnabledInwardProcessing()
		{
			var instruction = Factory.CreateInwardProcessingInstruction();
			CombineAssertions(() =>
			{
				AssertEquals("Inward Processing Declaration", true, instruction.EnabledInwardProcessing);
				instruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
				AssertEquals("Invalid Style", false, instruction.EnabledInwardProcessing);
				instruction.JobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("Export Job", false, instruction.EnabledInwardProcessing);
			});
		}

		public void TestClearInwardProcessingDetailsIfNeeded_OnCEI_StyleChanged()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var instruction = Factory.CreateInwardProcessingInstruction();
			instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.J;
			instruction.CEI_AuthorisationNumber = "XX";
			instruction.CEI_CompletionDuration = 22;
			instruction.CEI_CriteriaType = "0";
			instruction.CEI_InwardProcessingAdditionalInformation = "Add";
			instruction.CEI_InwardProcessingDescription = "Description";
			instruction.CompletionCustomsOffices.AddNew();
			instruction.InwardProcessingPlaces.AddNew();
			instruction.MainAccountingAddress.OrganisationPK = orgHeader.PK;

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
			CombineAssertions(() =>
			{
				AssertEquals("CEI_SimplifiedGrantAuthorization", ZString.Empty, instruction.CEI_SimplifiedGrantAuthorization);
				AssertEquals("CEI_AuthorisationNumber", ZString.Empty, instruction.CEI_AuthorisationNumber);
				AssertEquals("CEI_CompletionDuration", ZInt.Zero, instruction.CEI_CompletionDuration);
				AssertEquals("CEI_CriteriaType", ZString.Empty, instruction.CEI_CriteriaType);
				AssertEquals("CEI_InwardProcessingAdditionalInformation", ZString.Empty, instruction.CEI_InwardProcessingAdditionalInformation);
				AssertEquals("CEI_InwardProcessingDescription", ZString.Empty, instruction.CEI_InwardProcessingDescription);
				AssertEquals("CompletionCustomsOffices", 0, instruction.CompletionCustomsOffices.Count);
				AssertEquals("InwardProcessingPlaces", 0, instruction.InwardProcessingPlaces.Count);
				AssertEquals("MainAccountingAddress", ZGuid.Empty, instruction.MainAccountingAddress.OrganisationPK);
			});
		}

		public void TestClearInwardProcessingDetailsIfNeeded_OnFactorySaving()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var newFactory = new BusinessObjectFactory();
			var instruction = newFactory.CreateInwardProcessingInstruction();
			instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.J;
			instruction.CEI_AuthorisationNumber = "XX";
			instruction.CEI_CompletionDuration = 22;
			instruction.CEI_CriteriaType = "0";
			instruction.CEI_InwardProcessingAdditionalInformation = "Add";
			instruction.CEI_InwardProcessingDescription = "Description";
			instruction.CompletionCustomsOffices.AddNew();
			instruction.InwardProcessingPlaces.AddNew();
			instruction.MainAccountingAddress.OrganisationPK = orgHeader.PK;
			instruction.JobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			newFactory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("CEI_SimplifiedGrantAuthorization", ZString.Empty, instruction.CEI_SimplifiedGrantAuthorization);
				AssertEquals("CEI_AuthorisationNumber", ZString.Empty, instruction.CEI_AuthorisationNumber);
				AssertEquals("CEI_CompletionDuration", ZInt.Zero, instruction.CEI_CompletionDuration);
				AssertEquals("CEI_CriteriaType", ZString.Empty, instruction.CEI_CriteriaType);
				AssertEquals("CEI_InwardProcessingAdditionalInformation", ZString.Empty, instruction.CEI_InwardProcessingAdditionalInformation);
				AssertEquals("CEI_InwardProcessingDescription", ZString.Empty, instruction.CEI_InwardProcessingDescription);
				AssertEquals("CompletionCustomsOffices", 0, instruction.CompletionCustomsOffices.Count);
				AssertEquals("InwardProcessingPlaces", 0, instruction.InwardProcessingPlaces.Count);
				AssertEquals("MainAccountingAddress", ZGuid.Empty, instruction.MainAccountingAddress.OrganisationPK);
			});
		}

		public void TestCEI_SimplifiedGrantAuthorizationForValueJ()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var instruction = Factory.CreateInwardProcessingInstruction();
			instruction.CEI_AuthorisationNumber = "XX";
			instruction.CEI_CompletionDuration = 22;
			instruction.CEI_CriteriaType = "0";
			instruction.CEI_InwardProcessingDescription = "Description";
			instruction.MainAccountingAddress.OrganisationPK = orgHeader.PK;
			instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.J;
			CombineAssertions(() =>
			{
				AssertEquals("CEI_AuthorizationNumberInfo.ReadOnly", true, instruction.CEI_AuthorisationNumberInfo.ReadOnly);
				AssertEquals("CEI_CompletionDurationInfo.ReadOnly", false, instruction.CEI_CompletionDurationInfo.ReadOnly);
				AssertEquals("CEI_CriteriaTypeInfo.ReadOnly", false, instruction.CEI_CriteriaTypeInfo.ReadOnly);
				AssertEquals("CEI_InwardProcessingDescriptionInfo.ReadOnly", false, instruction.CEI_InwardProcessingDescriptionInfo.ReadOnly);
				AssertEquals("CEI_AuthorisationNumber", ZString.Empty, instruction.CEI_AuthorisationNumber);
				AssertEquals("CEI_CompletionDuration", 22, instruction.CEI_CompletionDuration);
				AssertEquals("CEI_CriteriaType", "0", instruction.CEI_CriteriaType);
				AssertEquals("CEI_InwardProcessingDescription", "Description", instruction.CEI_InwardProcessingDescription);
				AssertEquals("MainAccountingAddress is not cleared", orgHeader.PK, instruction.MainAccountingAddress.OrganisationPK);
			});
		}

		public void TestCEI_SimplifiedGrantAuthorizationForValueN()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var instruction = Factory.CreateInwardProcessingInstruction();
			instruction.CEI_AuthorisationNumber = "XX";
			instruction.CEI_CompletionDuration = 22;
			instruction.CEI_CriteriaType = "0";
			instruction.CEI_InwardProcessingDescription = "Description";
			instruction.MainAccountingAddress.OrganisationPK = orgHeader.PK;
			instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.N;
			CombineAssertions(() =>
			{
				AssertEquals("CEI_AuthorizationNumberInfo.ReadOnly", false, instruction.CEI_AuthorisationNumberInfo.ReadOnly);
				AssertEquals("CEI_CompletionDurationInfo.ReadOnly", true, instruction.CEI_CompletionDurationInfo.ReadOnly);
				AssertEquals("CEI_CriteriaTypeInfo.ReadOnly", true, instruction.CEI_CriteriaTypeInfo.ReadOnly);
				AssertEquals("CEI_InwardProcessingDescriptionInfo.ReadOnly", true, instruction.CEI_InwardProcessingDescriptionInfo.ReadOnly);
				AssertEquals("CEI_AuthorisationNumber", "XX", instruction.CEI_AuthorisationNumber);
				AssertEquals("CEI_CompletionDuration", ZInt.Zero, instruction.CEI_CompletionDuration);
				AssertEquals("CEI_CriteriaType", ZString.Empty, instruction.CEI_CriteriaType);
				AssertEquals("CEI_InwardProcessingDescription", ZString.Empty, instruction.CEI_InwardProcessingDescription);
				AssertEquals("MainAccountingAddress is cleared", ZGuid.Empty, instruction.MainAccountingAddress.OrganisationPK);
			});
		}

		public void TestCEI_SimplifiedGrantAuthorization_ClearinvoiceLineInwardProcessingDetails()
		{
			var instruction = Factory.CreateInwardProcessingInstruction();
			instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.J;
			var invoiceHeader = instruction.JobDeclaration.Invoices.AddNew();
			var invoiceLine1 = CreateJobComInvoiceLine();
			var invoiceLine2 = CreateJobComInvoiceLine();

			instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.N;
			CombineAssertions(() =>
			{
				AssertInvoiceLineCleared("CEI_SimplifiedGrantAuthorization = 'N', invoiceLine1", invoiceLine1);
				AssertInvoiceLineCleared("CEI_SimplifiedGrantAuthorization = 'N', invoiceLine2", invoiceLine2);

				var invoiceLine3 = CreateJobComInvoiceLine();
				instruction.CEI_SimplifiedGrantAuthorization = ZString.Empty;
				AssertInvoiceLineCleared("CEI_SimplifiedGrantAuthorization empty, invoiceLine3", invoiceLine3);
			});

			JobComInvoiceLine CreateJobComInvoiceLine()
			{
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
				invoiceLine.ZG_EconomicConditions = "01";
				invoiceLine.ZG_IdentificationMeansType = IdentificationMeansTypeList.Codes.D;
				invoiceLine.JI_ExtraInfoForClassification = "EXTRA INFORMATION";
				invoiceLine.InwardProcessingProducts.AddNew();
				return invoiceLine;
			}

			void AssertInvoiceLineCleared(string message, JobComInvoiceLine invoiceLine)
			{
				AssertEquals($"{message} -> Economic Conditions", ZString.Empty, invoiceLine.ZG_EconomicConditions);
				AssertEquals($"{message} -> Identification Means Type", ZString.Empty, invoiceLine.ZG_IdentificationMeansType);
				AssertEquals($"{message} -> Extra Info For Classification", ZString.Empty, invoiceLine.JI_ExtraInfoForClassification);
				AssertEquals($"{message} -> Inward Processing Products", 0, invoiceLine.InwardProcessingProducts.Count);
			}
		}

		public void TestCEI_SimplifiedGrantAuthorizationForValueEmpty()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var instruction = Factory.CreateInwardProcessingInstruction();
			instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.N;
			instruction.CEI_AuthorisationNumber = "XX";
			instruction.CEI_CompletionDuration = 22;
			instruction.CEI_CriteriaType = "0";
			instruction.CEI_InwardProcessingDescription = "Description";
			instruction.MainAccountingAddress.OrganisationPK = orgHeader.PK;
			instruction.CEI_SimplifiedGrantAuthorization = ZString.Empty;
			CombineAssertions(() =>
			{
				AssertEquals("Empty->CEI_AuthorizationNumberInfo.ReadOnly", true, instruction.CEI_AuthorisationNumberInfo.ReadOnly);
				AssertEquals("Empty->CEI_CompletionDurationInfo.ReadOnly", true, instruction.CEI_CompletionDurationInfo.ReadOnly);
				AssertEquals("Empty->CEI_CriteriaTypeInfo.ReadOnly", true, instruction.CEI_CriteriaTypeInfo.ReadOnly);
				AssertEquals("Empty->CEI_InwardProcessingDescriptionInfo.ReadOnly", true, instruction.CEI_InwardProcessingDescriptionInfo.ReadOnly);
				AssertEquals("Empty->CEI_AuthorisationNumber", ZString.Empty, instruction.CEI_AuthorisationNumber);
				AssertEquals("Empty->CEI_CompletionDuration", ZInt.Zero, instruction.CEI_CompletionDuration);
				AssertEquals("Empty->CEI_CriteriaType", ZString.Empty, instruction.CEI_CriteriaType);
				AssertEquals("Empty->CEI_InwardProcessingDescription", ZString.Empty, instruction.CEI_InwardProcessingDescription);
				AssertEquals("MainAccountingAddress is cleared", ZGuid.Empty, instruction.MainAccountingAddress.OrganisationPK);
			});
		}

		public void TestInwardProcessingPlaces()
		{
			AssertEquals(true, cei.IsRegisteredEditableChildObject(cei.InwardProcessingPlaces));
		}

		public void TestCompletionCustomsOffices()
		{
			AssertEquals(true, cei.IsRegisteredEditableChildObject(cei.CompletionCustomsOffices));
		}

		public void TestLookups()
		{
			AssertType<CusEntryInstructionLookups>(cei.Lookups);
		}

		public void TestJobDeclaration()
		{
			var dec = Factory.New<JobDeclaration>();
			cei.CEI_JE = dec.PK;
			AssertType<JobDeclaration>(cei.JobDeclaration);
		}

		public void TestIsEarlyClearanceFlagApplicable()
		{
			var earlyClearanceFlagApplicable = new HashSet<string>
			{
				ImportDeclarationTypeList.Codes.AZL,
				ImportDeclarationTypeList.Codes.EZL,
				ImportDeclarationTypeList.Codes.LUZ,
				ImportDeclarationTypeList.Codes.VZL
			};
			CombineAssertions(() =>
			{
				AssertEquals("CEI_Style empty", false, cei.IsEarlyClearanceFlagApplicable);
				foreach (var style in new ImportDeclarationTypeList().GetAllCodes())
				{
					cei.CEI_Style = style;
					AssertEquals($"CEI_Style {style}", earlyClearanceFlagApplicable.Contains(style), cei.IsEarlyClearanceFlagApplicable);
				}
			});
		}

		public void TestIsInwardMovementApplicable()
		{
			var inwardMovementApplicableStyles = new HashSet<string>
			{
				ImportDeclarationTypeList.Codes.AAV,
				ImportDeclarationTypeList.Codes.AZL,
				ImportDeclarationTypeList.Codes.EAV,
				ImportDeclarationTypeList.Codes.EZL,
				ImportDeclarationTypeList.Codes.LUZ,
				ImportDeclarationTypeList.Codes.VAV,
				ImportDeclarationTypeList.Codes.VZL
			};
			CombineAssertions(() =>
			{
				AssertEquals("CEI_Style empty", false, cei.IsInwardMovementApplicable);
				foreach (var style in new ImportDeclarationTypeList().GetAllCodes())
				{
					cei.CEI_Style = style;
					AssertEquals($"CEI_Style {style}", inwardMovementApplicableStyles.Contains(style), cei.IsInwardMovementApplicable);
				}
			});
		}

		public void TestIsSimplifiedDeclaration()
		{
			var simplifiedDeclarationStyles = new HashSet<string>
			{
				ImportDeclarationTypeList.Codes.AZ,
				ImportDeclarationTypeList.Codes.AAV,
				ImportDeclarationTypeList.Codes.AZL,
			};
			CombineAssertions(() =>
			{
				AssertEquals("CEI_Style empty", false, cei.IsSimplifiedDeclaration);
				foreach (var style in new ImportDeclarationTypeList().GetAllCodes())
				{
					cei.CEI_Style = style;
					AssertEquals($"CEI_Style {style}", simplifiedDeclarationStyles.Contains(style), cei.IsSimplifiedDeclaration);
				}
			});
		}

		public void TestUpdateEarlyClearanceFlagIfNeeded_Cleared()
		{
			CombineAssertions(() =>
			{
				foreach (var style in new ImportDeclarationTypeList().GetAllCodes())
				{
					cei.CEI_Style = style;
					AssertEarlyClearanceFlagCleared($"CEI_Style {style}", !cei.IsEarlyClearanceFlagApplicable);
				}
			});

			void AssertEarlyClearanceFlagCleared(string assertMessage, bool expectedCleared)
			{
				cei.CEI_EarlyClearanceFlag = EarlyClearanceFlagsList.Codes.J;
				cei.UpdateEarlyClearanceFlagIfNeeded();
				if (expectedCleared)
				{
					AssertEquals($"{assertMessage} -> CEI_EarlyClearanceFlag", ZString.Empty, cei.CEI_EarlyClearanceFlag);
				}
				else
				{
					AssertEquals($"{assertMessage} -> CEI_EarlyClearanceFlag", EarlyClearanceFlagsList.Codes.J, cei.CEI_EarlyClearanceFlag);
				}
			}
		}

		public void TestUpdateEarlyClearanceFlagIfNeeded_Set()
		{
			CombineAssertions(() =>
			{
				foreach (var style in new ImportDeclarationTypeList().GetAllCodes())
				{
					cei.CEI_Style = style;
					AssertEarlyClearanceFlagSet($"CEI_Style {style}", cei.IsEarlyClearanceFlagApplicable);
				}
			});

			void AssertEarlyClearanceFlagSet(string assertMessage, bool expectedSet)
			{
				cei.CEI_EarlyClearanceFlag = ZString.Empty;
				cei.UpdateEarlyClearanceFlagIfNeeded();
				if (expectedSet)
				{
					AssertEquals($"{assertMessage} -> CEI_EarlyClearanceFlag", EarlyClearanceFlagsList.Codes.N, cei.CEI_EarlyClearanceFlag);
				}
				else
				{
					AssertEquals($"{assertMessage} -> CEI_EarlyClearanceFlag", ZString.Empty, cei.CEI_EarlyClearanceFlag);
				}
			}
		}

		public void TestCEI_EarlyClearanceFlag_ReadOnly()
		{
			CombineAssertions(() =>
			{
				foreach (var style in new ImportDeclarationTypeList().GetAllCodes())
				{
					cei.CEI_Style = style;
					AssertEquals($"CEI_Style {style}", !cei.IsEarlyClearanceFlagApplicable, cei.CEI_EarlyClearanceFlag_ReadOnly);
				}
			});
		}

		public void TestClearLocalClearanceDate()
		{
			var dec = Factory.New<JobDeclaration>();
			cei.CEI_JE = dec.PK;
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			cei.CEI_LocalClearanceDate = new ZDateTime(2019, 01, 01);
			cei.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
			AssertEquals(new ZDateTime(2019, 01, 01), cei.CEI_LocalClearanceDate);
			cei.CEI_Style = ImportDeclarationTypeList.Codes.EGN;
			AssertEquals(ZDateTime.Empty, cei.CEI_LocalClearanceDate);
		}

		public void TestClearCEI_SubStyleWhenReadOnly()
		{
			cei.CEI_SubStyle = ImportSubStyleList.Codes.A;
			cei.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;
			AssertEquals("clear substype when CEI_SubStyle_ReadOnly", ZString.Empty, cei.CEI_SubStyle);
		}

		public void TestCEI_Procedure_Caption()
		{
			AssertEquals("CPC", DataBoundResourceStrings.GetDataForProperty(cei.CEI_ProcedureInfo).Caption);
		}

		public void TestUpdateCEI_ProcedureIfNeeded()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(CountryCodes.Germany, "", "71", "00", "", "DES1", "IMP", group: "EZL");
			helper.CreateRefCusProcedure(CountryCodes.Germany, "", "71", "78", "", "DES2", "IMP", group: "EZL");
			helper.CreateRefCusProcedure(CountryCodes.Germany, "", "49", "00", "C22", "DES3", "IMP", group: "EZA");
			helper.CreateRefCusProcedure(CountryCodes.Germany, "", "42", "71", "C18", "DES4", "IMP", group: "EZA");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var cei = declaration.CustomsEntryInstructions.AddNew();
			CombineAssertions(() =>
			{
				cei.CEI_Style = ImportDeclarationTypeList.Codes.EZL;
				AssertEquals("Only one value, update default", "71", cei.CEI_Procedure);
				cei.CEI_Procedure = ZString.Empty;
				cei.CEI_Style = ImportDeclarationTypeList.Codes.EZA;
				AssertEquals("Multiple values, doesn't update default", ZString.Empty, cei.CEI_Procedure);
			});
		}

		public void TestIsLocalClearanceDateReadonly()
		{
			AssertEquals(false, cei.IsLocalClearanceDateReadonly);
			cei.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
			AssertEquals(false, cei.IsLocalClearanceDateReadonly);
			cei.CEI_Style = ImportDeclarationTypeList.Codes.AZ;
			AssertEquals(false, cei.IsLocalClearanceDateReadonly);
			cei.CEI_Style = ImportDeclarationTypeList.Codes.AZL;
			AssertEquals(false, cei.IsLocalClearanceDateReadonly);
			cei.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
			AssertEquals(true, cei.IsLocalClearanceDateReadonly);
			cei.CEI_Style = ImportDeclarationTypeList.Codes.VZA;
			AssertEquals(true, cei.IsLocalClearanceDateReadonly);
			cei.CEI_Style = ImportDeclarationTypeList.Codes.VZL;
			AssertEquals(true, cei.IsLocalClearanceDateReadonly);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Entry Instruction", cei.HumanReadableName);
		}

		public void TestReimportCountries()
		{
			AssertType<ReimportCountryCodeCollection>(cei.ReimportCountryCodes);
		}

		public void TestIdentificationMeans()
		{
			AssertType<IdentificationMeansCodeCollection>(cei.IdentificationMeanCodes);
		}

		public void TestProducts()
		{
			AssertType<ProductSupportingInfoCollection>(cei.Products);
		}

		public void TestPreviousDocuments()
		{
			AssertType<PreviousDocumentCollection>(cei.PreviousDocuments);
		}

		public void TestCusAuthorizationUsages()
		{
			AssertType<CusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction>>(cei.CusAuthorizationUsages);
		}

		public void TestPreviousDocumentMaster()
		{
			AssertNotNull(cei.PreviousDocumentMaster);
		}

		public void TestEnabledOutwardProcessing()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CombineAssertions(() =>
			{
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				AssertEquals("CEI_Style doesn't start with 1", false, instruction.EnabledOutwardProcessing);

				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._110000;
				AssertEquals("CEI_Style starts with 1", true, instruction.EnabledOutwardProcessing);
			});
		}

		public void TestClearOutwardProcessingFieldsIfNeed()
		{
			var newFactory = new BusinessObjectFactory();
			var declaration = newFactory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000100;
			instruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._00;

			instruction.ReimportCountryCodes.AddNew();
			instruction.IdentificationMeanCodes.AddNew();
			instruction.Products.AddNew();

			newFactory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("ReimportCountryCodes.Count", 0, instruction.ReimportCountryCodes.Count);
				AssertEquals("IdentificationMeanCodes.Count", 0, instruction.IdentificationMeanCodes.Count);
				AssertEquals("Products.Count", 0, instruction.Products.Count);
			});
		}

		public void TestProcedureSubTypePopulatesDescriptionIfEmptyExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var cei = declaration.CustomsEntryInstructions.AddNew();

			CombineAssertions(() =>
			{
				cei.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._10;
				AssertEquals("should be set if empty", "Subsequent export declaration", cei.CEI_Description);
				cei.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._12;
				AssertEquals("should change according to CEI_SubStyle", "Subsequent export declaration from the emergency p", cei.CEI_Description);
				cei.CEI_Description = "TEST";
				cei.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._20;
				AssertEquals("should not change after user edit", "TEST", cei.CEI_Description);
			});
		}

		public void TestProcedureTypePopulatesDescriptionIfEmptyImport()
		{
			CreateRefProcedure(new ImportDeclarationTypeList(), MessageTypeList.Codes.Import);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			var cei = declaration.CustomsEntryInstructions.AddNew();

			CombineAssertions(() =>
			{
				cei.CEI_Style = ImportDeclarationTypeList.Codes.AZL;
				AssertEquals("should be set if empty", "Local clearance procedure for entry into the custo", cei.CEI_Description);
				cei.CEI_Style = ImportDeclarationTypeList.Codes.AZ;
				AssertEquals("should change according to CEI_Style", "Local clearance procedure for release of goods for", cei.CEI_Description);
				cei.CEI_Description = "TEST";
				cei.CEI_Style = ImportDeclarationTypeList.Codes.BA;
				AssertEquals("should not change after user edit", "TEST", cei.CEI_Description);
			});
		}

		void CreateRefProcedure(CodeDescriptionPairList list, string typeOfDeclaration)
		{
			var codes = string.Join(",", list.GetAllCodes());
			var prc = Factory.New<RefCusProcedure>();
			prc.ZZ6_ZZZ_NKDataGrouping = CountryCodes.Germany;
			prc.ZZ6_ShipmentType = typeOfDeclaration;
			prc.ZZ6_ProcedureCode = "99";
			prc.ZZ6_Group = codes;
			prc.ZZ6_Description = "for testing";
			prc.ZZ6_StartDate = ZDateTime.Now.AddDays(-2);
			prc.ZZ6_EndDate = ZDateTime.Now.AddDays(2);
		}

		public void TestGetCusSupportingInfoTypes()
		{
			CombineAssertions(() =>
			{
				var types = ((Integration.Customs.ICusSupportingInfoTypeSupporter)cei).GetCusSupportingInfoTypes();
				AssertEquals("types.Count", 5, types.Count);
				AssertEquals(true, types.ContainsKey(ProductSupportingInfo.CusSupportingInfoType));
				AssertEquals(true, types.ContainsKey(Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument));
				AssertEquals(true, types.ContainsKey(Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument));
				AssertEquals(true, types.ContainsKey(Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo));
				AssertEquals(true, types.ContainsKey(Common.EU.CusSupportingInfoTypeList.Codes.InstructionRequestedDocument));
			});
		}

		public void TestGetCusSupportingInfoTypes_SupportingDocument()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			var supportingInfoTypeSupporter = (Integration.Customs.ICusSupportingInfoTypeSupporter)instruction;
			var supportedTypes = supportingInfoTypeSupporter.GetCusSupportingInfoTypes();
			AssertEquals(typeof(EU.Business.Declaration.MultiLineAddInfos.SupportingDocument), supportedTypes[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument]);
		}

		[TestDate(2020, 07, 16)]
		public void TestCEI_AuthorisationNumber()
		{
			var declarant = Factory.New<OrgHeader>();
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "1234567890");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			CombineAssertions(() =>
			{
				instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.N;
				AssertEquals("No IPO authorization and CEI_SimplifiedGrantAuthorization is N", ZString.Empty, instruction.CEI_AuthorisationNumber);

				declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
				instruction.CEI_SimplifiedGrantAuthorization = ZString.Empty;
				instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.N;
				AssertEquals("One IPO authorization and CEI_SimplifiedGrantAuthorization is N", "1234567890", instruction.CEI_AuthorisationNumber);

				instruction.CEI_AuthorisationNumber = ZString.Empty;
				instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.J;
				AssertEquals("One IPO authorization and CEI_SimplifiedGrantAuthorization is not N", ZString.Empty, instruction.CEI_AuthorisationNumber);

				declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "0123456789");
				var cachedKey = string.Join("|", "GetCachedAuthorizationNumbers|DE", CusAuthorizationHeaderTypeList.Codes.InwardProcessing, declarant.PK, ZDate.Today);
				Factory.ClearCachedValue<CodeDescriptionPairList>(cachedKey);
				instruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.N;
				AssertEquals("Multiple IPO authorizations and CEI_SimplifiedGrantAuthorization is N", ZString.Empty, instruction.CEI_AuthorisationNumber);
			});
		}

		public void TestCEI_AuthorisationNumberInfo_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			CombineAssertions(() =>
			{
				instruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
				AssertEquals("Not Readonly as CEI_Style is AAV", false, instruction.CEI_AuthorisationNumberInfo.ReadOnly);

				instruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
				AssertEquals("Not Readonly as CEI_Style is VAV", false, instruction.CEI_AuthorisationNumberInfo.ReadOnly);

				instruction.CEI_Style = ImportDeclarationTypeList.Codes.AZL;
				AssertEquals("Readonly as CEI_Style not AAV or VAV", true, instruction.CEI_AuthorisationNumberInfo.ReadOnly);
			});
		}

		public void TestCEI_SubStyleMaxLength_Export()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals(2, instruction.CEI_SubStyleInfo.MaxLength);
		}

		public void TestCEI_SubStyleMaxLength_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals(1, instruction.CEI_SubStyleInfo.MaxLength);
		}

		public void TestCEI_SubStyle_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			CombineAssertions(() =>
			{
				foreach (var style in new ImportDeclarationTypeList().GetAllCodes())
				{
					if (style != ImportDeclarationTypeList.Codes.AVABR)
					{
						instruction.CEI_SubStyle = "X";
						instruction.CEI_Style = style;
						var expectedSubStyle = instruction.IsSimplifiedDeclaration ? ImportSubStyleList.Codes.C : ImportDeclarationTypeList.IsLUZ(style) ? string.Empty : "X";
						var expectedReadonly = instruction.IsSimplifiedDeclaration || ImportDeclarationTypeList.IsLUZ(style);
						AssertEquals($"When CEI_Style={instruction.CEI_Style} value", expectedSubStyle, instruction.CEI_SubStyle);
						AssertEquals($"When CEI_Style={instruction.CEI_Style} readonly", expectedReadonly, instruction.CEI_SubStyleInfo.ReadOnly);
					}
				}
			});
		}

		public void TestCEI_StyleMaxLength_Export()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals(6, instruction.CEI_StyleInfo.MaxLength);
		}

		public void TestCEI_StyleMaxLength_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals(7, instruction.CEI_StyleInfo.MaxLength);
		}

		public void TestClearAuthorisationNumberFieldsIfNeeded()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			CombineAssertions(() =>
			 {
				 instruction.CEI_AuthorisationNumber = "1234567";
				 instruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
				 AssertEquals("Not cleaned, as enabled on EntryInstruction", "1234567", instruction.CEI_AuthorisationNumber);

				 instruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
				 AssertEquals("Not cleaned, as enabled on EntryInstruction", "1234567", instruction.CEI_AuthorisationNumber);

				 instruction.CEI_Style = ImportDeclarationTypeList.Codes.EAV;
				 AssertEquals("Cleaned, as not enabled", ZString.Empty, instruction.CEI_AuthorisationNumber);

				 instruction.CEI_AuthorisationNumber = "3456789";

				 instruction.CEI_Style = ImportDeclarationTypeList.Codes.AZ;
				 AssertEquals("Cleaned, as not enabled", ZString.Empty, instruction.CEI_AuthorisationNumber);
			 });
		}

		public void TestUpdateCEI_Procedure_InwardProcessingNotEnabled()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AZ;
			instruction.CEI_Procedure = "42";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var reloadedInstruction = newFactory.Load<CusEntryInstruction>(instruction.PK);

			CombineAssertions(() =>
			{
				AssertEquals("CPC initially set to 42", "42", reloadedInstruction.CEI_Procedure);

				instruction.CEI_Procedure = "40";
				Factory.Save();

				reloadedInstruction = newFactory.Load<CusEntryInstruction>(instruction.PK);
				AssertEquals("CPC updated to 40", "40", reloadedInstruction.CEI_Procedure);
			});
		}

		public void TestZG_ExitDate_Caption()
		{
			AssertEquals("Exit Date", DataBoundResourceStrings.GetDataForProperty(cei.ZG_ExitDateInfo).Caption);
		}

		public void TestCEI_Style_ClearCountryOfOriginIfNeeded()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.France;
				invoiceLine.JI_CEI = entryInstruction.PK;

				AssertEquals("Not cleaned as CEI_Style = LÜZ", Core.Constants.CountryCodes.France, invoiceLine.JI_RN_NKCountryOfExport);

				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
				AssertEquals("Cleaned as CEI_Style != LÜZ", ZString.Empty, invoiceLine.JI_RN_NKCountryOfExport);
			});
		}

		public void TestCEI_Style_ClearDecisiveDateIfNeeded()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_CustomDate1 = ZDate.Today;
				invoiceLine.JI_CEI = entryInstruction.PK;

				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;
				AssertEquals("Not cleaned as CEI_Style = LÜZ", ZDate.Today, invoiceLine.JI_CustomDate1);

				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
				AssertEquals("Cleaned as CEI_Style != LÜZ", ZDate.Empty, invoiceLine.JI_CustomDate1);
			});
		}

		public void TestUpdateCGL_Qualifier()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var goodsLocation = entryInstruction.GoodsLocation;

				entryInstruction.CEI_Style = ZString.Empty;
				AssertEquals("CGL_Qualifier is empty, as CEI_Style is empty", ZString.Empty, goodsLocation.CGL_Qualifier);

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000200;
				AssertEquals("CGL_Qualifier is empty, as CEI_Style is not in '9, 1, 3, 4'", ZString.Empty, goodsLocation.CGL_Qualifier);

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000100;
				AssertEquals("CGL_Qualifier = 'V' for CEI_Style = '***1**'", CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier, goodsLocation.CGL_Qualifier);

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000901;
				AssertEquals("CGL_Qualifier = 'V' for CEI_Style = '***9**'", CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier, goodsLocation.CGL_Qualifier);

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._001300;
				AssertEquals("CGL_Qualifier = 'Y' for CEI_Style = '***3**'", CusGoodsLocationQualifierList.Codes.AuthorizationNumber, goodsLocation.CGL_Qualifier);

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000410;
				AssertEquals("CGL_Qualifier = 'Y' for CEI_Style = '***4**'", CusGoodsLocationQualifierList.Codes.AuthorizationNumber, goodsLocation.CGL_Qualifier);

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000200;
				AssertEquals("CGL_Qualifier stays unchanged, as CEI_Style is not in '9, 1, 3, 4'", CusGoodsLocationQualifierList.Codes.AuthorizationNumber, goodsLocation.CGL_Qualifier);
			});
		}

		public void TestSequenceGenerator()
		{
			CombineAssertions(() =>
			{
				AssertEquals("SequenceStartingNumber", 0, cei.SequenceGenerator.SequenceStartingNumber);
				AssertEquals("SequenceMaxNumber", 255, cei.SequenceGenerator.SequenceMaxNumber);
			});
		}

		public void TestGoodsLocationType()
		{
			AssertType<CusGoodsLocation>(cei.GoodsLocation);
		}

		public void TestCEI_OA_Warehouse_UpdateAuthorizationNumber_Import()
		{
			var previousDocumentMaster = cei.PreviousDocumentMaster;
			var declaration = Factory.New<JobDeclaration>();
			cei.CEI_JE = declaration.PK;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;

			var fromWarehouse = Factory.New<OrgHeader>();
			fromWarehouse.OH_Code = "EDIBRNMAZ";
			var fromWarehouseAddress = fromWarehouse.MainAddress;
			fromWarehouseAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "NUMBER1");
			previousDocumentMaster.CSI_Procedure = PreviousProcedureList.Codes._ATZL;

			CombineAssertions(() =>
			{
				AssertEquals("Before setting Warehouse", ZString.Empty, previousDocumentMaster.AuthorizationNumber);

				cei.CEI_OA_Warehouse = fromWarehouseAddress.PK;
				AssertEquals("After Warehouse has been set", "NUMBER1", previousDocumentMaster.AuthorizationNumber);
			});
		}

		public void TestCEI_OA_Warehouse_UpdateAuthorizationNumber_Export()
		{
			var previousDocumentMaster = cei.PreviousDocumentMaster;
			var declaration = Factory.New<JobDeclaration>();
			cei.CEI_JE = declaration.PK;
			declaration.JE_MessageType = MessageTypeList.Codes.Export;

			var fromWarehouse = Factory.New<OrgHeader>();
			fromWarehouse.OH_Code = "EDIBRNMAZ";
			var fromWarehouseAddress = fromWarehouse.MainAddress;
			fromWarehouseAddress.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "NUMBER1");

			cei.PreviousDocuments.AddNew();
			previousDocumentMaster.AuthorizationNumber = "Test";
			cei.CEI_OA_Warehouse = fromWarehouse.PK;
			AssertEquals("After Warehouse has been set => remains", "Test", previousDocumentMaster.AuthorizationNumber);
		}

		public void TestAdditionalInformation_MaxLength()
		{
			var maxLength100Styles = new HashSet<string>
			{
				ImportDeclarationTypeList.Codes.EAV,
				ImportDeclarationTypeList.Codes.EZL,
			};

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			cei.CEI_JE = declaration.PK;

			CombineAssertions(() =>
			{
				foreach (var style in new ImportDeclarationTypeList().GetAllCodes())
				{
					cei.CEI_Style = style;
					if (maxLength100Styles.Contains(style))
					{
						AssertEquals($"CEI_Style {style}", 100, cei.AdditionalInformationInfo.MaxLength);
					}
					else
					{
						AssertEquals($"CEI_Style {style}", 2000, cei.AdditionalInformationInfo.MaxLength);
					}
				}

				declaration.JE_MessageType = ZString.Empty;
				AssertEquals("Declaration is not Import", 350, cei.AdditionalInformationInfo.MaxLength);
			});
		}

		public void TestCEI_Procedure_DefaultValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			cei.CEI_JE = declaration.PK;
			cei.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
			AssertEquals("Default CEI_Procedure ofr CEI_Style 'AVABR'", "40", cei.CEI_Procedure);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cei = Factory.NewWithValidTestData<CusEntryInstruction>();
		}
		CusEntryInstruction cei;
	}
}
