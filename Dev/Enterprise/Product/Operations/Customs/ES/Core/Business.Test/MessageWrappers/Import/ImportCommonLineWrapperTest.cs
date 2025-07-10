using System;
using System.Linq;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	class ImportCommonLineWrapperTest : WrapperHelperTest<ImportCommonLineWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("CusEntryLine", () => new ImportCommonLineWrapper(null));
				AssertExceptionThrown<ArgumentOutOfRangeException>("No MergedLines", () => new ImportCommonLineWrapper(Factory.New<CusEntryLine>()));
			});
		}

		public void TestLineNumber()
		{
			entryLine.CL_LineNumber = 2;
			AssertEquals("Expected filled LineNumber", 2, wrapper.LineNumber);
		}

		public void TestContainers()
		{
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			foreach (var tag in ContainerTagsWithEmpty)
			{
				var container = declaration.CusContainers.AddNew();
				container.CO_ContainerNumber = tag;
				invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(tag).IsForInvoiceLine = true;
				invoiceLine2.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(tag).IsForInvoiceLine = true;
			}

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

			wrapper = new ImportCommonLineWrapper(entryLine);
			var containers = wrapper.Containers;
			CombineAssertions(() =>
			{
				AssertArrayEqualsByElements("Expected filled Containers", ContainerTags, containers.ToArray());
				AssertSame("Cached Containers", wrapper.Containers, containers);
			});
		}

		public void TestGoodsDescription()
		{
			invoiceLine.JI_Description = EntryLineData.GoodsDescription;
			AssertEquals("Expected filled GoodsDescription", EntryLineData.GoodsDescription, wrapper.GoodsDescription);
		}

		public void TestTariffCode()
		{
			invoiceLine.JI_Tariff = EntryLineData.Tariff;
			AssertEquals("Expected filled TariffCode", EntryLineData.Tariff, wrapper.TariffCode);
		}

		public void TestOriginCountry()
		{
			invoiceLine.JI_CountryOfOrigin = EntryLineData.CountryOfOrigin;
			AssertEquals("Expected filled OriginCountry", EntryLineData.CountryOfOrigin, wrapper.OriginCountry);
		}

		public void TestRequestedCPC()
		{
			invoiceLine.JI_Procedure = EntryLineData.Procedure;
			AssertEquals("Expected filled RequestedCPC", EntryLineData.ProcedurePart1, wrapper.RequestedCPC);
		}

		public void TestPreviousCPC()
		{
			invoiceLine.JI_Procedure = EntryLineData.Procedure;
			AssertEquals("Expected filled PreviousCPC", EntryLineData.ProcedurePart2, wrapper.PreviousCPC);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			Universal.RefCusProcedure cusProcedure1 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Spain, "", EntryLineData.ProcedurePart1, EntryLineData.ProcedurePart2, EntryLineData.ProcedureConcessionPart, "AB DESC 1", "EXP", intoWarehouse: true, outOfWarehouse: true, group: "EFD");
			cusProcedure1.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Spain;

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();

			AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

			wrapper = new ImportCommonLineWrapper(entryLine);
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		CusEntryLine entryLine;
		ImportCommonLineWrapper wrapper;

		protected override ImportCommonLineWrapper GetProvider() => wrapper;
	}
}
