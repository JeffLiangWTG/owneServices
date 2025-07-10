using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.Business.Testing
{
	public class LineMergerTest : Customs.Business.Testing.LineMergerTest
	{
		public void TestLandedCostOnly()
		{
			using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, true))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var procedureACode = "A";
				var procedureA = helper.CreateOrFindExistingRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, "", procedureACode, "", "", "", MessageTypeList.Codes.Import, "");
				procedureA.ZZ6_CalculateDuty = false;
				procedureA.ZZ6_LandedCost = true;
				var procedureBCode = "B";
				var procedureB = helper.CreateOrFindExistingRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, "", procedureBCode, "", "", "", MessageTypeList.Codes.Import, "");
				procedureB.ZZ6_CalculateDuty = true;
				procedureB.ZZ6_LandedCost = false;

				var declaration = GetLineMergerTestHelper().CreateTestDeclarationAndInvoiceLines("", "", Common.CustomsChargeTypeList.Codes.AdditionCharge);

				var lineMergerMock = new Mock<LineMerger>(declaration);
				lineMergerMock.CallBase = true;
				var mergeStrategy = new Mock<Customs.Business.IDutyCalculatorStrategy>();

				mergeStrategy.Setup(x => x.CalculateDuties()).Callback(() =>
				{
					foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
					{
						foreach (var entryLine in entry.MergedLines)
						{
							entryLine.Fees.AddNew();
						}
					}
				});
				lineMergerMock.Protected().Setup<Customs.Business.IDutyCalculatorStrategy>("GetNewDutyCalculatorStrategy")
					.Returns(mergeStrategy.Object);
				lineMergerMock.Object.DoMerge();
				var entryHeaders = declaration.ActiveEntryHeaders;

				CombineAssertions(() =>
				{
					var feeA = entryHeaders[0].AllEntryLines.First(x => x.RandomLine.ProcedureCode == procedureACode).Fees[0];
					var feeB = entryHeaders[0].AllEntryLines.First(x => x.RandomLine.ProcedureCode == procedureBCode).Fees[0];
					AssertEquals("Procedure A", true, feeA.CF_IsLandedCostOnly);
					AssertEquals("Procedure B", false, feeB.CF_IsLandedCostOnly);
				});
			}
		}

		public void TestOnMergingResetReadOnlySupportingDocuments()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>();

			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				new string[] { importCodeType, exportCodeType }, "1234", "1234", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "BLT";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_CL = entryLine.PK;

			AssertEquals("ReadOnly Supporting Documents count", 0, entryLine.ReadOnlySupportingDocuments.Count);

			invoiceLine.SupportingDocuments.Add(GetSupportingDoc(1, "REF111", false));
			invoiceLine.SupportingDocuments.Add(GetSupportingDoc(2, "REF222", false));

			AssertEquals("ReadOnly Supporting Documents count", 0, entryLine.ReadOnlySupportingDocuments.Count);
			DoMerge(declaration);

			AssertEquals("ReadOnly Supporting Documents count", 2, entryLine.ReadOnlySupportingDocuments.Count);
		}

		public void TestOnMerged()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Tariff = "100101";

			CombineAssertions(() =>
			{
				new LineMerger(declaration).DoMerge();
				AssertNoRowErrorContaining(instruction, "Cannot delete Entry lines from a Declared or Canceled Entry");

				var entryHeader = declaration.ActiveEntryHeaders[0];
				entryHeader.CH_HighestLineNumber = 2;
				ActivateLockEntryLines((CusEntryHeader)entryHeader);
				new LineMerger(declaration).DoMerge();
				if (declaration.Configuration.LockNumberOfEntryLinesForRegisteredEntry)
				{
					AssertEquals(expected: true, instruction.ShouldKeepNotAllowDeleteEntryLinesErrors);
					AssertHasRowErrorContaining(instruction, "Cannot delete Entry lines from a Declared or Canceled Entry");
				}
				else
				{
					AssertEquals(expected: false, instruction.ShouldKeepNotAllowDeleteEntryLinesErrors);
					AssertNoRowErrorContaining(instruction, "Cannot delete Entry lines from a Declared or Canceled Entry");
				}
			});
		}

		protected virtual void ActivateLockEntryLines(CusEntryHeader entryHeader) => entryHeader.EntryNumber = "123";

		protected override bool AllowDeleteEntryLineForRegistedEntry => !((JobDeclaration)TestJobDeclaration).Configuration.LockNumberOfEntryLinesForRegisteredEntry;

		protected virtual void CustomizeSupportingDocumentForLocalCountry(SupportingDocument supportingDocument) { }

		protected virtual Type GetSupportingDocumentType() => typeof(SupportingDocument);

		protected override Customs.Business.BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

		protected override Type ExpectedDutyCalculatorStrategyType => typeof(DutyCalculatorStrategy);

		protected override Type[] ExpectedEntryCreationStrategiesType => new[] { typeof(EntryCreationStrategy) };

		protected override Type ExpectedLandedCostOnlyConfigurationProviderType => typeof(LandedCostOnlyConfigurationProviderEU);

		protected override Customs.Business.LineMerger GetNewLineMerger(Customs.Business.BaseJobDeclaration declaration) => new LineMerger((JobDeclaration)declaration);

		protected override string GetClearStatus() => MessageStatusList.Codes.OK;

		protected virtual LineMergerTestHelper GetLineMergerTestHelper() => new LineMergerTestHelper(Factory);

		void DoMerge(JobDeclaration declaration) => GetNewLineMerger(declaration).DoMerge();

		SupportingDocument GetSupportingDoc(int i, ZString refNumber, bool alternateSubType, decimal qty3 = 10.0m, int flag = 0)
		{
			var supDoc = (SupportingDocument)Factory.New(GetSupportingDocumentType());
			supDoc.SuspendValidation();

			supDoc.CSI_Code = "1234";
			supDoc.CSI_ReferenceNumber = refNumber;
			supDoc.CSI_SubType = alternateSubType ? "B" : "A"; //Part

			supDoc.CSI_Quantity = i * 10;
			supDoc.CSI_UnitOfQuantity = "BAG";
			supDoc.CSI_Quantity2 = i * 10.1;
			supDoc.CSI_UnitOfQuantity2 = "PKT";
			supDoc.CSI_Value = i * 1000;
			supDoc.CSI_RX_NKCurrency = "GBP";
			supDoc.CSI_DateOfIssue = new ZDateTime(2020, 01, 01);
			supDoc.CSI_DateOfExpiry = new ZDateTime(2020, 12, 31);
			supDoc.CSI_Quantity3 = qty3;

			supDoc.CSI_Description = "Testing"; //reason
			supDoc.CSI_ReferenceNumber2 = "REFNUM2"; //Issueing Authority
			supDoc.CSI_AdditionalDescription = "AddDescr";
			supDoc.CSI_CustomsOffice = "ABC";
			supDoc.CSI_Procedure = "X";
			supDoc.CSI_RN_NKCountryCode = "GB";
			supDoc.CSI_Status = "QWE";
			supDoc.CSI_Tariff = "12345";
			supDoc.CSI_Type = "SUP";
			supDoc.CSI_UnitOfQuantity3 = "U3";

			CustomizeSupportingDocumentForLocalCountry(supDoc);

			return supDoc;
		}
	}
}
