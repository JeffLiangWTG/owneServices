using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.JP;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(EntryCreationStrategy))]
	sealed class EntryCreationStrategyTest : TestCaseWithFactory
	{
		public void TestGetExistingEntryHeader_ReuseChildlessHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MergeBy = "TRF";
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			cusEntryHeader.CH_CEI_Instruction = cusEntryInstruction.PK;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_Description = "Test";

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = cusEntryInstruction.PK;
			invoiceLine.JI_NACCSCode = "X";

			Factory.Save();

			var childlessEntryHeader = declaration.ActiveEntryHeaders.FirstOrDefault();

			CombineAssertions(() =>
			{
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals("Should be only 1 entry header", 1, declaration.ActiveEntryHeaders.Count);

				var entryHeader = declaration.ActiveEntryHeaders.FirstOrDefault();
				AssertEquals("Should reuse childlessEntryHeader", invoiceLine.CusEntryLine.Header, childlessEntryHeader);
			});
		}

		public void TestGetKeyForLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			declaration.JE_MergeBy = "TRF";
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			CombineAssertions(() =>
			{
				AssertMergeKeyForLineContains<ZString>(x => x.JI_Description, "A");
				AssertMergeKeyForLineContains<ZString>(x => x.JI_CountryOfOrigin, "A");
				AssertMergeKeyForLineContains<ZString>(x => x.JI_PrimaryPreference, "A");
				AssertMergeKeyForLineContains<ZString>(x => x.JI_SecondaryPreference, "A");
				AssertMergeKeyForLineContains<ZString>(x => x.JI_FEFTAArticle48, "A");
				AssertMergeKeyForLineContains<ZBool>(x => x.JI_DomesticConsumptionTaxExemptionIsPartial, true);

				declaration.JE_MessageType = JPJobMessageTypeList.Codes.Export;
				invoiceLine1.JI_FEFTAArticle48 = "";
				invoiceLine2.JI_FEFTAArticle48 = "";
				AssertMergeKeyForLineContains<ZString>(x => x.JI_NACCSCode, "A");

				var cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
				cusEntryInstruction.CEI_ValueType = ValueTypeList.Codes.L;
				invoiceLine1.JI_CEI = cusEntryInstruction.PK;
				invoiceLine2.JI_CEI = cusEntryInstruction.PK;
				AssertMergeKeyForLineContains<ZString>(x => x.JI_CustomsUnitQty, "A");
				AssertMergeKeyForLineContains<ZString>(x => x.JI_CustomsSecondUnitQty, "A");
			});

			void AssertMergeKeyForLineContains<TValue>(Expression<Func<JobComInvoiceLine, TValue>> valueSelector, TValue validValue, TValue emptyValue = default) where TValue : IZType
			{
				var propInfo = (valueSelector.Body as MemberExpression).Member as PropertyInfo;
				var propName = propInfo.Name;
				var getter1 = () => (TValue)propInfo.GetValue(invoiceLine1);
				var setter1 = (TValue value) => propInfo.SetValue(invoiceLine1, value, null);
				var originalValue1 = getter1();

				var getter2 = () => (TValue)propInfo.GetValue(invoiceLine2);
				var setter2 = (TValue value) => propInfo.SetValue(invoiceLine2, value, null);
				var originalValue2 = getter2();

				setter1(validValue);
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals(propName, 2, declaration.CustomsEntryHeaders[0].MergedLines.Count);
				setter2(validValue);
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				AssertEquals(propName, 1, declaration.CustomsEntryHeaders[0].MergedLines.Count);
			}
		}

		public void TestGetValuesForPreventMerging_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ExportDate = new ZDateTime(2004, 12, 12);
			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invHead1 = declaration.Invoices.AddNew();

			JobComInvoiceLine NewInvoiceLineWithPropertySet(Action<JobComInvoiceLine> setProperty)
			{
				var result = invHead1.JobComInvoiceLines.AddNew();
				result.JI_CEI = entryInstruction.PK;
				setProperty(result);
				return result;
			}

			var invLineThatCanBeMergedByDefaultKeys = NewInvoiceLineWithPropertySet(l => { });
			var invLinesThatCannotBeMerged = new[]
			{
				(NewInvoiceLineWithPropertySet(l => { l.JI_DutyReductionExemptionRefundCode = "ABC"; }), nameof(JobComInvoiceLine.JI_DutyReductionExemptionRefundCode)),
				(NewInvoiceLineWithPropertySet(l => { l.JI_TradeControlOrderAppendix = "A"; }), nameof(JobComInvoiceLine.JI_TradeControlOrderAppendix)),
				(NewInvoiceLineWithPropertySet(l => { l.JI_DomesticConsumptionTaxExemptionCode = "A"; }), nameof(JobComInvoiceLine.JI_DomesticConsumptionTaxExemptionCode)),
				(NewInvoiceLineWithPropertySet(l => { l.JI_StorageType = "A"; }), nameof(JobComInvoiceLine.JI_StorageType)),
				(NewInvoiceLineWithPropertySet(l => { l.JI_AdvanceRulingOnOrigin = "ABC"; }), nameof(JobComInvoiceLine.JI_AdvanceRulingOnOrigin)),
				(NewInvoiceLineWithPropertySet(l => { l.JI_AdvanceRulingOnClassification = "ABC"; }), nameof(JobComInvoiceLine.JI_AdvanceRulingOnClassification)),
				(NewInvoiceLineWithPropertySet(l => { l.JI_NACCSCode = "A"; }), nameof(JobComInvoiceLine.JI_NACCSCode)),
				(NewInvoiceLineWithPropertySet(l => { l.JI_CustomsUnitQty = "A"; }), nameof(JobComInvoiceLine.JI_CustomsUnitQty)),
				(NewInvoiceLineWithPropertySet(l => { l.JI_CustomsSecondUnitQty = "A"; }), nameof(JobComInvoiceLine.JI_CustomsSecondUnitQty)),
			};

			var entryCreationStrategy = new EntryCreationStrategy(declaration);
			var keyForLineJP = entryCreationStrategy.GetKeyForLine(invLineThatCanBeMergedByDefaultKeys);

			CombineAssertions(() =>
			{
				Assert($"{nameof(JobComInvoiceLine.JI_DomesticConsumptionTaxExemptionIsPartial)} is an unconditional key", keyForLineJP.Contains(ZBool.False));
				keyForLineJP.Remove(ZBool.False);
				keyForLineJP.Remove(invHead1.PK);
				foreach ((var line, var name) in invLinesThatCannotBeMerged)
				{
					Assert($"Presence of {name} should make invoice line unmergeable", entryCreationStrategy.GetKeyForLine(line).Contains(line.PK));
				}
			});
		}

		public void TestGetValuesForPreventMerging_Export()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ExportDate = new ZDateTime(2004, 12, 12);

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invHead1 = declaration.Invoices.AddNew();

			JobComInvoiceLine NewInvoiceLineWithPropertySet(Action<JobComInvoiceLine> setProperty)
			{
				var result = invHead1.JobComInvoiceLines.AddNew();
				result.JI_CEI = entryInstruction.PK;
				setProperty(result);
				return result;
			}

			var invLineThatCanBeMergedByDefaultKeys = NewInvoiceLineWithPropertySet(l => { });
			var invLinesThatCannotBeMerged = new[]
			{
				(NewInvoiceLineWithPropertySet(l => { l.JI_NACCSCode = "X"; }), nameof(JobComInvoiceLine.JI_DutyReductionExemptionRefundCode)),
				(NewInvoiceLineWithPropertySet(l => { l.OtherLaws.AddNew(); }), nameof(JobComInvoiceLine.OtherLaws)),
				(NewInvoiceLineWithPropertySet(l => { l.JI_FEFTAArticle48 = "A"; }), nameof(JobComInvoiceLine.JI_FEFTAArticle48)),
				(NewInvoiceLineWithPropertySet(l => { l.JI_TradeControlOrderAppendix = "A"; }), nameof(JobComInvoiceLine.JI_TradeControlOrderAppendix)),
				(NewInvoiceLineWithPropertySet(l => { l.JI_DutyReductionExemptionRefundCode = "A"; }), nameof(JobComInvoiceLine.JI_DutyReductionExemptionRefundCode)),
				(NewInvoiceLineWithPropertySet(l => { l.JI_DomesticConsumptionTaxExemptionCode = "A"; }), nameof(JobComInvoiceLine.JI_DomesticConsumptionTaxExemptionCode)),
			};

			var entryCreationStrategy = new EntryCreationStrategy(declaration);

			CombineAssertions(() =>
			{
				foreach ((var line, var name) in invLinesThatCannotBeMerged)
				{
					Assert($"Presence of {name} should make invoice line unmergeable", entryCreationStrategy.GetKeyForLine(line).Contains(line.PK));
				}
			});
		}
	}
}
