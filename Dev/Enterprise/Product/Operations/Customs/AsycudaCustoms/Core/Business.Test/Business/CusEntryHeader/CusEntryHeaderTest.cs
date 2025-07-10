using System;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(CusEntryHeader))]
	sealed class CusEntryHeaderTest : Customs.Business.Testing.CusEntryHeaderTest
	{
		public void TestCustomsValue_ResourceStringDataAttribute()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CusEntryHeader), nameof(CusEntryHeader.CustomsValue), false, attribute => attribute.Caption == "Customs Value");
		}

		public void TestValueForVAT_ResourceStringDataAttribute()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CusEntryHeader), nameof(CusEntryHeader.ValueForVAT), false, attribute => attribute.Caption == "VAT/GST Value");
		}

		public void TestNetWeightKilograms()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_NetWeight = 1.12m;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CEI = instruction.PK;
			AssertEquals(1.12m, entryHeader.NetWeightKilograms);
		}

		public void TestCustomsQuantity()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CustomsQuantity = 2.24m;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CEI = instruction.PK;
			AssertEquals(2.24m, entryHeader.CustomsQuantity);
		}

		public void TestDocumentSupporter()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			AssertType<CusEntryHeaderDocumentSupporter>(entryHeader.DocumentSupporter);
		}

		public void TestReferenceNumber_EntryNumberFirst()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.EntryNumber = "233";
			AssertEquals("233", entryHeader.ReferenceNumber);
		}

		public void TestReferenceNumber_FallBackToBGMReferenceIfEntryNumberIsEmpty()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "BGM";
			AssertEquals("BGM", entryHeader.ReferenceNumber);
		}

		[UseSnapshotProtection]
		public void TestSetBGMReferenceNumber()
		{
			BGMReferenceCounterProvider.Instance.ResetValue();

			var factory1 = new BusinessObjectFactory();
			var declaration = factory1.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001000";
			var entry1OfDec = declaration.ActiveEntryHeaders.AddNew();
			factory1.Save();

			AssertEquals("B00001000/1", entry1OfDec.CH_BGMReference);

			var factory2 = new BusinessObjectFactory();
			var dec2 = factory2.New<JobDeclaration>();
			dec2.JE_DeclarationReference = "B00001002";
			var entry1OfDec2 = dec2.ActiveEntryHeaders.AddNew();
			factory2.Save();

			AssertEquals("B00001002/1", entry1OfDec2.CH_BGMReference);

			var entry2OfDec = declaration.ActiveEntryHeaders.AddNew();
			factory1.Save();
			AssertEquals("B00001000/2", entry2OfDec.CH_BGMReference);

			var decInFactory2 = factory2.Load<JobDeclaration>(declaration.PK);
			var entry3OfDecInFactory2 = decInFactory2.ActiveEntryHeaders.AddNew();
			factory2.Save();
			AssertEquals("B00001000/3", entry3OfDecInFactory2.CH_BGMReference);

			var entry4OfDec = declaration.ActiveEntryHeaders.AddNew();
			var entry5OfDec = declaration.ActiveEntryHeaders.AddNew();
			var entry6OfDec = declaration.ActiveEntryHeaders.AddNew();
			entry6OfDec.CH_BGMReference = "ExistedValue";
			factory1.Save();
			AssertEquals("B00001000/4", entry4OfDec.CH_BGMReference);
			AssertEquals("B00001000/5", entry5OfDec.CH_BGMReference);
			AssertEquals("ExistedValue", entry6OfDec.CH_BGMReference);
		}

		public void TestIsEntryNumberReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			entryHeader.CH_HasManualWhsUpdate = false;
			entryHeader.EntryNumber = ZString.Empty;
			AssertEquals(false, entryHeader.EntryNumberInfo.ReadOnly);

			entryHeader.EntryNumber = "123";
			AssertEquals(false, entryHeader.EntryNumberInfo.ReadOnly);

			entryHeader.CH_HasManualWhsUpdate = true;
			AssertEquals(true, entryHeader.EntryNumberInfo.ReadOnly);

			entryHeader.EntryNumber = ZString.Empty;
			AssertEquals(false, entryHeader.EntryNumberInfo.ReadOnly);
		}

		public void TestSetMovementReferenceNumber()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.MovementReferenceNumber = "TEST1";
			var mrnNumber = CusEntryNumber.Load(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			AssertNotNull(mrnNumber);
			AssertEquals("TEST1", mrnNumber.CE_EntryNum);
			entryHeader.MovementReferenceNumber = "TEST2";
			AssertEquals("TEST2", mrnNumber.CE_EntryNum);
			entryHeader.MovementReferenceNumber = " ";
			AssertEquals("", mrnNumber.CE_EntryNum);
			AssertEquals(ZDateTime.Empty, mrnNumber.CE_IssueDate);
		}

		public void TestTransitPermitCount()
		{
			var entryHeader = (CusEntryHeader)base.GetNewBusinessObjectForDeleteTest(Factory);
			entryHeader.CH_CEI_Instruction = entryHeader.Declaration.CustomsEntryInstructions.AddNew().PK;
			CombineAssertions(() =>
			{
				AssertEquals("no permits", 0, entryHeader.TransitPermitCount);
				entryHeader.EntryInstruction.CusInBondPermitsHeaders.AddNew();
				entryHeader.EntryInstruction.CusInBondPermitsHeaders.AddNew();
				AssertEquals("permits added", 2, entryHeader.TransitPermitCount);
			});
		}

		public void TestTransitPermitInTransitCount()
		{
			var entryHeader = (CusEntryHeader)base.GetNewBusinessObjectForDeleteTest(Factory);
			entryHeader.CH_CEI_Instruction = entryHeader.Declaration.CustomsEntryInstructions.AddNew().PK;
			CombineAssertions(() =>
			{
				AssertEquals("no TransitPermit In Transit", 0, entryHeader.TransitPermitInTransitCount);
				entryHeader.EntryInstruction.CusInBondPermitsHeaders.AddNew();
				var permit = entryHeader.EntryInstruction.CusInBondPermitsHeaders.AddNew();
				AssertEquals("2 TransitPermits In Transit", 2, entryHeader.TransitPermitInTransitCount);
				permit.BM_ArrivalDate = ZDateTime.BrettsBirthday;
				AssertEquals("1 TransitPermit In Transit", 1, entryHeader.TransitPermitInTransitCount);
			});
		}

		public void TestExpired()
		{
			var entryHeader = (CusEntryHeader)base.GetNewBusinessObjectForDeleteTest(Factory);
			entryHeader.CH_CEI_Instruction = entryHeader.Declaration.CustomsEntryInstructions.AddNew().PK;
			CombineAssertions(() =>
			{
				var permit1 = entryHeader.EntryInstruction.CusInBondPermitsHeaders.AddNew();
				var permit2 = entryHeader.EntryInstruction.CusInBondPermitsHeaders.AddNew();
				Assert("not expired", !entryHeader.Expired);
				permit1.BM_ArrivalDate = ZDateTime.Empty;
				permit1.BM_Calc_ValidityDate = ZDateTime.Today.AddDays(-2);
				Assert("expired", entryHeader.Expired);
			});
		}

		public void TestEarliestExpiryDate()
		{
			var entryHeader = (CusEntryHeader)base.GetNewBusinessObjectForDeleteTest(Factory);
			entryHeader.CH_CEI_Instruction = entryHeader.Declaration.CustomsEntryInstructions.AddNew().PK;
			CombineAssertions(() =>
			{
				AssertEquals("should be empty when no permit", ZDateTime.Empty, entryHeader.EarliestExpiryDate);
				var permit1 = entryHeader.EntryInstruction.CusInBondPermitsHeaders.AddNew();
				var permit2 = entryHeader.EntryInstruction.CusInBondPermitsHeaders.AddNew();
				var permit3 = entryHeader.EntryInstruction.CusInBondPermitsHeaders.AddNew();
				var permit4 = entryHeader.EntryInstruction.CusInBondPermitsHeaders.AddNew();
				permit1.BM_Calc_ValidityDate = ZDateTime.Today.AddDays(-3);
				permit1.BM_ArrivalDate = ZDateTime.BrettsBirthday;
				permit2.BM_Calc_ValidityDate = ZDateTime.Today.AddDays(-2);
				permit2.BM_ArrivalDate = ZDateTime.Empty;
				permit3.BM_Calc_ValidityDate = ZDateTime.Today.AddDays(2);
				permit3.BM_ArrivalDate = ZDateTime.Empty;
				permit4.BM_Calc_ValidityDate = ZDateTime.Empty;
				permit4.BM_ArrivalDate = ZDateTime.Empty;
				AssertEquals("should be the earliest in not arrived permits", permit2.BM_Calc_ValidityDate, entryHeader.EarliestExpiryDate);
			});
		}

		public void TestCompleted()
		{
			var entryHeader = (CusEntryHeader)base.GetNewBusinessObjectForDeleteTest(Factory);
			entryHeader.CH_CEI_Instruction = entryHeader.Declaration.CustomsEntryInstructions.AddNew().PK;
			CombineAssertions(() =>
			{
				Assert("should be incomplete when no permit", !entryHeader.Completed);
				var permit1 = entryHeader.EntryInstruction.CusInBondPermitsHeaders.AddNew();
				var permit2 = entryHeader.EntryInstruction.CusInBondPermitsHeaders.AddNew();
				permit1.BM_ArrivalDate = ZDateTime.Empty;
				permit2.BM_ArrivalDate = ZDateTime.BrettsBirthday;
				Assert("should be incomplete when one record does not have an arrival date", !entryHeader.Completed);
				permit1.BM_ArrivalDate = ZDateTime.BrettsBirthday;
				Assert("should be complete when all have arrival dates", entryHeader.Completed);
			});
		}

		public void TestAddExtraRequiredFieldsMessageErrorForBondedWarehouse()
		{
			const string entryHeaderErrorMsg = "An Entry Header marked for Inventory Management must have a valid entry number; not all Entry Headers marked for Inventory Management have a valid entry number specified.";
			const string previousEntryLineNumberErrorMsg = "An Entry Header marked for Inventory Management must have a valid entry number; not all Entry Headers marked for Inventory Management have a valid entry number specified.";
			const string previousEntryNumberErrorMsg = "An Entry Header marked for Inventory Management must have a valid entry number; not all Entry Headers marked for Inventory Management have a valid entry number specified.";

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "BOB1";
			importer.OH_FullName = "BOB THE BUILDER";
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			var warehouse1 = Factory.New<OrgHeader>();
			warehouse1.OH_Code = "WAR1";
			warehouse1.OH_FullName = "WAREHOUSE 1";
			warehouse1.OH_RL_NKClosestPort = "AUSYD";
			warehouse1.MainAddress.OA_Address1 = "ADD 1";
			var procedure1 = Factory.New<RefCusProcedure>();
			procedure1.ZZ6_ProcedureCode = "AB";
			procedure1.ZZ6_Description = "AB DESC";
			procedure1.ZZ6_PreviousProcedureCode = "10";
			procedure1.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Botswana;
			procedure1.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.No;
			procedure1.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "AB";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = "AB10";
			invoiceLine.JI_LineNo = 1;
			entryInstruction.CEI_OA_Warehouse = ZGuid.Empty;

			DoMerge(declaration);
			var entry = declaration.CustomsEntryHeaders.First() as CusEntryHeader;
			CombineAssertions(() =>
			{
				AssertContains(entryHeaderErrorMsg, entry.GetMessageErrorOfRequiredFieldsForBondedWarehousing(false, false, false));
				AssertContains(previousEntryNumberErrorMsg, entry.GetMessageErrorOfRequiredFieldsForBondedWarehousing(false, false, false));
				AssertContains(previousEntryLineNumberErrorMsg, entry.GetMessageErrorOfRequiredFieldsForBondedWarehousing(false, false, false));

				entry.EntryNumber = "ENT123";
				AssertNotContains(entryHeaderErrorMsg, entry.GetMessageErrorOfRequiredFieldsForBondedWarehousing(false, false, false));

				invoiceLine.JI_PreviousEntryNumber = "ENT122";
				AssertNotContains(previousEntryNumberErrorMsg, entry.GetMessageErrorOfRequiredFieldsForBondedWarehousing(false, false, false));

				invoiceLine.JI_PreviousEntryLineNumber = 1;
				AssertNotContains(previousEntryLineNumberErrorMsg, entry.GetMessageErrorOfRequiredFieldsForBondedWarehousing(false, false, false));
			});
		}

		public void TestShouldLogEntryStatusBeingTrue()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			AssertEquals("The value returned by ShouldLogEntryStatus should be true", true, entryHeader.ShouldLogEntryStatus);
		}

		public void TestIsOutOfRegime_True()
		{
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateOrFindExistingRefCusProcedure(countryCode, "IM", "90", "71", "F61", "", "IMP", "10P");
			procedure1.ZZ6_OutOfWarehouse = YesNoList.Codes.Yes;
			var procedure2 = helper.CreateOrFindExistingRefCusProcedure(countryCode, "IM", "91", "71", "F61", "", "IMP", "10P");
			procedure2.ZZ6_OutOfInwardProcessing = YesNoList.Codes.Yes;
			var procedure3 = helper.CreateOrFindExistingRefCusProcedure(countryCode, "IM", "92", "71", "F61", "", "IMP", "10P");
			procedure3.ZZ6_OutofOutwardProcessing = YesNoList.Codes.Yes;
			var procedure4 = helper.CreateOrFindExistingRefCusProcedure(countryCode, "IM", "93", "71", "F61", "", "IMP", "10P");
			procedure4.ZZ6_OutOfTemporaryImport = YesNoList.Codes.Yes;
			var procedure5 = helper.CreateOrFindExistingRefCusProcedure(countryCode, "IM", "94", "71", "F61", "", "IMP", "10P");
			procedure5.ZZ6_OutOfTemporaryExport = YesNoList.Codes.Yes;
			var procedures = new[] { procedure1, procedure2, procedure3, procedure4, procedure5 };
			Factory.Save();

			CombineAssertions(() =>
			{
				var index = 0;
				foreach (var procedure in procedures)
				{
					var entryHeader = CreateJobDeclarationWithEntryHeader(JobMessageTypeList.Codes.Import, $"EntryNum{index++}", procedure);
					AssertEquals(procedure.FullCodeCurrentPlusPreviousPlusConcession, true, entryHeader.IsOutOfRegime);
				}
			});
		}

		public void TestIsOutOfRegime_NonOutOfRegime()
		{
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(countryCode, "IM", "80", "71", "F61", "", "IMP", "10P");
			procedure.ZZ6_OutOfWarehouse = YesNoList.Codes.No;
			procedure.ZZ6_OutOfInwardProcessing = YesNoList.Codes.No;
			procedure.ZZ6_OutofOutwardProcessing = YesNoList.Codes.No;
			procedure.ZZ6_OutOfTemporaryImport = YesNoList.Codes.No;
			procedure.ZZ6_OutOfTemporaryExport = YesNoList.Codes.No;
			Factory.Save();

			var entryHeader = CreateJobDeclarationWithEntryHeader(JobMessageTypeList.Codes.Import, "EntryNum", procedure);
			AssertEquals(false, entryHeader.IsOutOfRegime);
		}

		public void TestIsOutOfRegime_NoProcedureCode()
		{
			var entryHeader = CreateJobDeclarationWithEntryHeader(JobMessageTypeList.Codes.Import, "EntryNum", null);
			AssertEquals(false, entryHeader.IsOutOfRegime);
		}

		public void TestIsOutOfRegime_NoInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = JobMessageTypeList.Codes.Import;
			entryHeader.CH_CEI_Instruction = instruction.PK;
			entryHeader.EntryNumber = "EntryNum";
			AssertEquals(false, entryHeader.IsOutOfRegime);
		}

		public void TestLiabilityClearedEventLog_RiskValueZeroWhenFirstSaved()
		{
			using (CreateDisposableRiskEnabledSwitcher(true))
			{
				var declaration = Factory.New<JobDeclaration>();
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = instruction.PK;
				entryHeader.CH_BGMReference = "BGM";
				var entryLine = entryHeader.AllEntryLines.AddNew();
				entryLine.CL_CustomsValue = 10m;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_CEI = instruction.PK;
				invoiceLine.JI_NetWeight = 5m;
				invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
				invoiceLine.JI_CustomsQuantity = 3m;

				var riskManagement = instruction.RiskManagements.AddNew();
				riskManagement.CSI_Value = 10m;
				riskManagement.CSI_Quantity = 5m;
				riskManagement.CSI_Quantity2 = 3m;
				Factory.Save();

				CombineAssertions(() =>
				{
					AssertEquals("risk management tab is visible", true, instruction.IsRiskTabPageTabVisible);
					AssertEquals("have no risk value", false, instruction.HasRiskValue);

					var logs = GetLLREventLogs(entryHeader);
					AssertEquals("one LLR log exists", 1, logs.Length);
					AssertEquals("not canceled", false, logs[0].IsCancelled);

					Factory.Save();

					var logs2 = GetLLREventLogs(entryHeader);
					AssertEquals("still only one LLR log after saving again", 1, logs2.Length);
					AssertEquals("still not canceled", false, logs2[0].IsCancelled);
				});
			}
		}

		public void TestLiabilityClearedEventLog_RiskValuePositiveWhenFirstSaved()
		{
			using (CreateDisposableRiskEnabledSwitcher(true))
			{
				var declaration = Factory.New<JobDeclaration>();
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = instruction.PK;
				entryHeader.CH_BGMReference = "BGM";
				var entryLine = entryHeader.AllEntryLines.AddNew();
				entryLine.CL_CustomsValue = 10m;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_CEI = instruction.PK;
				invoiceLine.JI_NetWeight = 5m;
				invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
				invoiceLine.JI_CustomsQuantity = 3m;

				var riskManagement = instruction.RiskManagements.AddNew();
				riskManagement.CSI_Value = 6m;
				riskManagement.CSI_Quantity = 5m;
				riskManagement.CSI_Quantity2 = 3m;
				Factory.Save();

				CombineAssertions(() =>
				{
					AssertEquals("risk management tab is visible", true, instruction.IsRiskTabPageTabVisible);
					AssertEquals("have risk value(s)", true, instruction.HasRiskValue);
					AssertEquals("no LLR log", 0, GetLLREventLogs(entryHeader).Length);
				});
			}
		}

		public void TestLiabilityClearedEventLog_WhenRiskValueBecomeZeroFromPositive()
		{
			using (CreateDisposableRiskEnabledSwitcher(true))
			{
				var declaration = Factory.New<JobDeclaration>();
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = instruction.PK;
				entryHeader.CH_BGMReference = "BGM";
				var entryLine = entryHeader.AllEntryLines.AddNew();
				entryLine.CL_CustomsValue = 10m;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_CEI = instruction.PK;
				invoiceLine.JI_NetWeight = 5m;
				invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
				invoiceLine.JI_CustomsQuantity = 3m;

				var riskManagement = instruction.RiskManagements.AddNew();
				riskManagement.CSI_Value = 10m;
				riskManagement.CSI_Quantity = 4m;
				riskManagement.CSI_Quantity2 = 3m;
				Factory.Save();

				CombineAssertions(() =>
				{
					riskManagement.CSI_Quantity = 5m;
					Factory.Save();

					AssertEquals("risk management tab is visible", true, instruction.IsRiskTabPageTabVisible);
					AssertEquals("have no risk value", false, instruction.HasRiskValue);

					var logs = GetLLREventLogs(entryHeader);
					AssertEquals("one LLR log exists", 1, logs.Length);
					AssertEquals("not canceled", false, logs[0].IsCancelled);

					Factory.Save();

					var logs2 = GetLLREventLogs(entryHeader);
					AssertEquals("still one LLR log after saving again", 1, logs2.Length);
					AssertEquals("still not canceled", false, logs2[0].IsCancelled);
				});
			}
		}

		public void TestLiabilityClearedEventLog_WhenRiskValueBecomePositiveFromZero()
		{
			using (CreateDisposableRiskEnabledSwitcher(true))
			{
				var declaration = Factory.New<JobDeclaration>();
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = instruction.PK;
				entryHeader.CH_BGMReference = "BGM";
				var entryLine = entryHeader.AllEntryLines.AddNew();
				entryLine.CL_CustomsValue = 10m;

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine2 = invoice.InvoiceLines.AddNew();
				invoiceLine2.JI_CL = entryLine.PK;
				invoiceLine2.JI_CEI = instruction.PK;
				invoiceLine2.JI_NetWeight = 5m;
				invoiceLine2.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
				invoiceLine2.JI_CustomsQuantity = 3m;

				var riskManagement = instruction.RiskManagements.AddNew();
				riskManagement.CSI_Value = 10m;
				riskManagement.CSI_Quantity = 5m;
				riskManagement.CSI_Quantity2 = 3m;
				Factory.Save();

				CombineAssertions(() =>
				{
					var logsHavingRiskValue = GetLLREventLogs(entryHeader);
					AssertEquals("one LLR log exists", 1, logsHavingRiskValue.Length);
					var eventTimeOfNotCanceledLog = logsHavingRiskValue[0].SL_EventTime;

					ResetEntryLineCustomsValue(entryHeader, entryLine, 12m);
					Factory.Save();

					AssertEquals("risk management tab is visible", true, instruction.IsRiskTabPageTabVisible);
					AssertEquals("have risk value(s)", true, instruction.HasRiskValue);

					var logs = GetLLREventLogs(entryHeader);
					AssertEquals("one LLR log exists", 1, logs.Length);
					AssertEquals("canceled", true, logs[0].IsCancelled);
					AssertNotEquals("event time should differ comparing to preceding", logs[0].SL_EventTime, eventTimeOfNotCanceledLog);

					Factory.Save();

					var logs2 = GetLLREventLogs(entryHeader);
					AssertEquals("still one LLR log after saving again", 1, logs2.Length);
					AssertEquals("still canceled", true, logs2[0].IsCancelled);
					AssertEquals("event time should keep same", logs[0].SL_EventTime, logs2[0].SL_EventTime);
				});
			}
		}

		public void TestLiabilityClearedEventLog_WhenRiskValueBecomeZeroFromPositiveAgain()
		{
			using (CreateDisposableRiskEnabledSwitcher(true))
			{
				var declaration = Factory.New<JobDeclaration>();
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = instruction.PK;
				entryHeader.CH_BGMReference = "BGM";
				var entryLine = entryHeader.AllEntryLines.AddNew();
				entryLine.CL_CustomsValue = 10m;

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine2 = invoice.InvoiceLines.AddNew();
				invoiceLine2.JI_CL = entryLine.PK;
				invoiceLine2.JI_CEI = instruction.PK;
				invoiceLine2.JI_NetWeight = 5m;
				invoiceLine2.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
				invoiceLine2.JI_CustomsQuantity = 3m;

				var riskManagement = instruction.RiskManagements.AddNew();
				riskManagement.CSI_Value = 10m;
				riskManagement.CSI_Quantity = 5m;
				riskManagement.CSI_Quantity2 = 3m;
				Factory.Save();

				ResetEntryLineCustomsValue(entryHeader, entryLine, 12m);
				Factory.Save();

				CombineAssertions(() =>
				{
					riskManagement.CSI_Value = 12m;
					Factory.Save();

					AssertEquals("risk management tab is visible", true, instruction.IsRiskTabPageTabVisible);
					AssertEquals("have no risk value", false, instruction.HasRiskValue);

					var logs = GetLLREventLogs(entryHeader);
					AssertEquals("two LLR logs exist", 2, logs.Length);
					AssertEquals("recent one is not canceled", false, logs[0].IsCancelled);
					AssertEquals("the other is canceled", true, logs[1].IsCancelled);

					Factory.Save();

					var logs2 = GetLLREventLogs(entryHeader);
					AssertEquals("still two LLR logs exist after saving again", 2, logs2.Length);
					AssertEquals("recent one is still not canceled", false, logs2[0].IsCancelled);
					AssertEquals("the other is still canceled", true, logs2[1].IsCancelled);
				});
			}
		}

		public void TestLiabilityClearedEventLog_WhenRiskManagementDisabled()
		{
			using (CreateDisposableRiskEnabledSwitcher(false))
			{
				var declaration = Factory.New<JobDeclaration>();
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = instruction.PK;
				entryHeader.CH_BGMReference = "BGM";
				var entryLine = entryHeader.AllEntryLines.AddNew();
				entryLine.CL_CustomsValue = 10m;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_CEI = instruction.PK;
				invoiceLine.JI_NetWeight = 5m;
				invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
				invoiceLine.JI_CustomsQuantity = 3m;
				Factory.Save();

				CombineAssertions(() =>
				{
					AssertEquals("have risk value(s)", true, instruction.HasRiskValue);
					AssertEquals("no LLR log", 0, GetLLREventLogs(entryHeader).Length);

					ResetEntryLineCustomsValue(entryHeader, entryLine, ZDecimal.Zero);
					invoiceLine.JI_NetWeight = ZDecimal.Zero;
					invoiceLine.JI_CustomsQuantity = ZDecimal.Zero;
					Factory.Save();

					AssertEquals("have no risk value", false, instruction.HasRiskValue);
					AssertEquals("still no LLR log even when no risk value", 0, GetLLREventLogs(entryHeader).Length);
				});
			}
		}

		public void TestLiabilityClearedEventLog_WhenRiskManagementInvisible()
		{
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var intoRegimeProcedure = helper.CreateOrFindExistingRefCusProcedure(countryCode, "IM", "20", "71", "F61", "2071F61", "IMP", "10P");
			intoRegimeProcedure.ZZ6_IntoWarehouse = YesNoList.Codes.Yes;
			var nonIntoRegimeProcedure = helper.CreateOrFindExistingRefCusProcedure(countryCode, "IM", "21", "71", "F61", "2171F61", "IMP", "10P");
			nonIntoRegimeProcedure.ZZ6_IntoWarehouse = YesNoList.Codes.No;
			nonIntoRegimeProcedure.ZZ6_IntoInwardProcessing = YesNoList.Codes.No;
			nonIntoRegimeProcedure.ZZ6_IntoOutwardProcessing = YesNoList.Codes.No;
			nonIntoRegimeProcedure.ZZ6_IntoTemporaryImport = YesNoList.Codes.No;
			nonIntoRegimeProcedure.ZZ6_IntoTemporaryExport = YesNoList.Codes.No;
			nonIntoRegimeProcedure.ZZ6_IsTransit = YesNoList.Codes.No;
			Factory.Save();

			using (CreateDisposableRiskEnabledSwitcher(true))
			{
				var declaration = Factory.New<JobDeclaration>();
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = instruction.PK;
				entryHeader.CH_BGMReference = "BGM";
				var entryLine = entryHeader.AllEntryLines.AddNew();
				entryLine.CL_CustomsValue = 10m;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_CEI = instruction.PK;
				invoiceLine.JI_NetWeight = 5m;
				invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
				invoiceLine.JI_CustomsQuantity = 3m;
				SetInvoiceLineProcedure(invoiceLine, intoRegimeProcedure);
				Factory.Save();

				CombineAssertions(() =>
				{
					AssertEquals("risk management tab is visible", true, instruction.IsRiskTabPageTabVisible);
					AssertEquals("have risk value(s)", true, instruction.HasRiskValue);
					AssertEquals("no LLR log", 0, GetLLREventLogs(entryHeader).Length);

					ResetEntryLineCustomsValue(entryHeader, entryLine, ZDecimal.Zero);
					invoiceLine.JI_NetWeight = ZDecimal.Zero;
					invoiceLine.JI_CustomsQuantity = ZDecimal.Zero;
					SetInvoiceLineProcedure(invoiceLine, nonIntoRegimeProcedure);
					Factory.Save();

					AssertEquals("risk management tab is invisible", false, instruction.IsRiskTabPageTabVisible);
					AssertEquals("have no risk value", false, instruction.HasRiskValue);
					AssertEquals("still no LLR log even when no risk value", 0, GetLLREventLogs(entryHeader).Length);
				});
			}
		}

		public void TestHumanReadableName()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.EntryNumber = "EN0001";
			entryHeader.CH_BGMReference = "BR0001";
			AssertEquals("Entry HumanReadableNameCore", "Customs Entry EN0001-BR0001", entryHeader.HumanReadableName);
		}

		public void TestCodeProperty()
		{
			var codeProperty = CodePropertyAttribute.CodePropertyNameFromType(GetExpectedBusinessObjectType());
			AssertEquals(nameof(CusEntryHeader.CodeProperty), codeProperty);

			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.EntryNumber = "EN0001";
			entryHeader.CH_BGMReference = "BR0001";
			AssertEquals("CodeProperty when EntryNumber is not empty ", "BR0001-EN0001", entryHeader.CodeProperty);
			entryHeader.EntryNumber = "";
			AssertEquals("CodeProperty when EntryNumber is empty", "BR0001", entryHeader.CodeProperty);
		}

		CusEntryHeader CreateJobDeclarationWithEntryHeader(
			string messageType,
			string entryNumber,
			RefCusProcedure procedure)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = messageType;
			entryHeader.CH_CEI_Instruction = instruction.PK;
			entryHeader.EntryNumber = entryNumber;

			var entryLine = entryHeader.AllEntryLines.AddNew();

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CEI = instruction.PK;

			if (procedure != null)
			{
				invoiceLine.JI_Procedure = procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode + procedure.ZZ6_Concession;
			}

			return entryHeader;
		}

		void ResetEntryLineCustomsValue(CusEntryHeader entryHeader, CusEntryLine entryLine, ZDecimal value)
		{
			entryHeader.ResetTotalsAndCachedValues();
			entryLine.CL_CustomsValue = value;
		}

		StmALog[] GetLLREventLogs(CusEntryHeader entryHeader)
			=> entryHeader.Logs
			.Find(log => log.SL_SE_NKEvent == Events.LiabilityCleared.Code)
			.OrderByDescending(log => log.SL_PostedTimeUtc)
			.ToArray();

		IDisposable CreateDisposableRiskEnabledSwitcher(bool riskEnabled)
		{
			return ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
				Universal.Constants.FunctionalityTypes.Risk,
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode,
				ZDateTime.Today,
				value: riskEnabled);
		}

		static void SetInvoiceLineProcedure(BaseJobComInvoiceLine invoiceLine, RefCusProcedure procedure)
		{
			invoiceLine.JI_Procedure = procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode + procedure.ZZ6_Concession;
		}

		protected override Type ExpectedChargeCollectionType => typeof(CusEntryHeaderChargesCollection<CusEntryHeaderCharges>);

		protected override Type ExpectedChargeType => typeof(CusEntryHeaderCharges);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = (CusEntryHeader)base.GetNewBusinessObjectForDeleteTest(factory);
			var instruction = header.Declaration.CustomsEntryInstructions.AddNew();
			header.CH_CEI_Instruction = instruction.PK;
			return header;
		}
	}
}
