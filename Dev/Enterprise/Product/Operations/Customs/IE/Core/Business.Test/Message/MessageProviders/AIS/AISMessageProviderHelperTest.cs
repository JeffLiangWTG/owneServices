using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class AISMessageProviderHelperTest : TestCaseWithFactory
	{
		public void TestGetIM415TransportEquipments()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			var entryLine2 = entryHeader.MergedLines.AddNew();
			var entryLine3 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine2.CL_LineNumber = 2;
			entryLine3.CL_LineNumber = 3;
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine3.JI_CL = entryLine3.PK;

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CNT1";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CNT2";

			var package1 = declaration.Packages.AddNew();
			package1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
			var package2 = declaration.Packages.AddNew();
			package2.CW_ContainerNoOrEquipmentNo = container2.CO_ContainerNumber;
			var package3 = declaration.Packages.AddNew();
			package3.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;

			invoiceLine1.PackagesPivot.AddPivotFor(package1);
			invoiceLine2.PackagesPivot.AddPivotFor(package2);
			invoiceLine3.PackagesPivot.AddPivotFor(package3);

			var equipments = AISMessageProviderHelper.GetIM415TransportEquipments(entryHeader);

			AssertEquals("Count", 2, equipments.Count);
			var expectedEquipments = new (string ContainerNumber, string[] GoodsReferences)[]
			{
				("CNT1", new[] { "1", "3" }),
				("CNT2", new[] { "2" }),
			};

			var index = 0;
			foreach (var equipment in equipments)
			{
				AssertEquipment($"Equipment {index + 1}", expectedEquipments[index++], equipment);
			}

			void AssertEquipment(string message, (string ContainerNumber, string[] GoodsReferences) expectedEquipment, IMTransportEquipment equipment)
			{
				CombineAssertions(message, () =>
				{
					AssertEquals("ContainerId", expectedEquipment.ContainerNumber, equipment.ContainerId);

					var goodReferences = equipment.GoodsReferences;
					AssertEquals("GoodsReferences Count", expectedEquipment.GoodsReferences.Length, goodReferences.Count);

					int indexInAssertion = 0;
					foreach (var goodReference in goodReferences)
					{
						var expectedGoodsReference = expectedEquipment.GoodsReferences[indexInAssertion++];
						AssertEquals("GoodsReferenc", expectedGoodsReference, goodReference);
					}
				});
			}
		}

		public void TestToMergeKey()
		{
			var goodsLocation = Factory.New<JobDeclaration>().CustomsEntryInstructions.AddNew().GoodsLocation;
			goodsLocation.CGL_Type = "A";
			goodsLocation.CGL_Qualifier = "U";
			goodsLocation.CGL_AdditionalIdentifier = "AddID";

			var testMergeKey = new MergeKey();
			testMergeKey.Add(goodsLocation.CGL_Type);
			testMergeKey.Add(goodsLocation.CGL_Qualifier);

			var testResult = AISMessageProviderHelper.ToMergeKey(goodsLocation, new[] { CusGoodsLocationSchema.Constants.CGL_Type, CusGoodsLocationSchema.Constants.CGL_Qualifier });
			AssertEquals("ToMergeKey", testMergeKey, testResult);
		}

		public void TestAggregateDocuments()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			entryInstruction.PreviousDocuments.AddNew().CSI_Code = "C1";
			entryInstruction.PreviousDocuments.AddNew().CSI_Code = "C2";

			entryLine.CL_LineNumber = 1;
			invoiceLine.PreviousDocuments.AddNew().CSI_Code = "C11";
			invoiceLine.PreviousDocuments.AddNew().CSI_Code = "C12";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.PreviousDocuments.AddNew().CSI_Code = "C11";
			invoiceLine2.PreviousDocuments.AddNew().CSI_Code = "C22";
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			invoiceLine2.JI_CL = entryLine2.PK;

			var provider = new UCC5.IM432GoodsShipmentProvider(new EntryHeaderWrapper(entryHeader));

			CombineAssertions("Aggregate Documents", () =>
			{
				AssertEquals("Header should have 3 items.", 3, provider.DocumentsAuthorisations.Count);
				AssertEquals("Document from header level", true, provider.DocumentsAuthorisations.Any(doc => doc.PreviousDocumentType == "C1"));
				AssertEquals("Another Document from header level", true, provider.DocumentsAuthorisations.Any(doc => doc.PreviousDocumentType == "C2"));
				AssertEquals("Document promoted from lines(can be found from all the line objects)", true, provider.DocumentsAuthorisations.Any(doc => doc.PreviousDocumentType == "C11"));

				var item1 = provider.GoodsShipmentItems.First(item => item.GoodsItemNumber == "1");
				AssertEquals("1 document left in Line level(the other promoted to Header)", 1, item1.DocumentsAuthorisations.Count);
				AssertEquals("Item can only be found in aggregated Line object should be left.", "C12", item1.DocumentsAuthorisations.First().PreviousDocumentType);

				var item2 = provider.GoodsShipmentItems.First(item => item.GoodsItemNumber == "2");
				AssertEquals("1 document left in Line level(the other promoted to Header)", 1, item2.DocumentsAuthorisations.Count);
				AssertEquals("Item can only be found in aggregated Line object should be left.", "C22", item2.DocumentsAuthorisations.First().PreviousDocumentType);
			});
		}

		public void TestGetValuationIndicator()
		{
			(_, var entryLineWrapper) = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);

			var invoiceHeader = entryLineWrapper.RandomInvoiceHeader;
			var invoiceLine = entryLineWrapper.RandomInvoiceLine;
			AssertEquals("Default", "0000", AISMessageProviderHelper.GetValuationIndicator(invoiceHeader, invoiceLine));

			CombineAssertions("1st character", () =>
			{
				invoiceHeader.RelatedIndicator = true;
				AssertEquals("RelatedIndicator is true, JI_RelatedIndicator is empty", "1000", AISMessageProviderHelper.GetValuationIndicator(invoiceHeader, invoiceLine));

				invoiceLine.JI_RelatedIndicator = "N";
				AssertEquals("RelatedIndicator is true, JI_RelatedIndicator is N", "0000", AISMessageProviderHelper.GetValuationIndicator(invoiceHeader, invoiceLine));

				invoiceLine.JI_RelatedIndicator = "Y";
				AssertEquals("RelatedIndicator is true, JI_RelatedIndicator is Y", "1000", AISMessageProviderHelper.GetValuationIndicator(invoiceHeader, invoiceLine));
			});

			CombineAssertions("2nd character", () =>
			{
				invoiceHeader.RelatedIndicator2 = true;
				AssertEquals("RelatedIndicator2 is true, ZG_RelatedIndicator2 is empty", "1100", AISMessageProviderHelper.GetValuationIndicator(invoiceHeader, invoiceLine));

				invoiceLine.ZG_RelatedIndicator2 = "N";
				AssertEquals("RelatedIndicator2 is true, ZG_RelatedIndicator2 is N", "1000", AISMessageProviderHelper.GetValuationIndicator(invoiceHeader, invoiceLine));

				invoiceLine.ZG_RelatedIndicator2 = "Y";
				AssertEquals("RelatedIndicator2 is true, ZG_RelatedIndicator2 is Y", "1100", AISMessageProviderHelper.GetValuationIndicator(invoiceHeader, invoiceLine));
			});

			CombineAssertions("3rd character", () =>
			{
				invoiceHeader.RelatedIndicator3 = true;
				AssertEquals("RelatedIndicator3 is true, ZG_RelatedIndicator3 is empty", "1110", AISMessageProviderHelper.GetValuationIndicator(invoiceHeader, invoiceLine));

				invoiceLine.ZG_RelatedIndicator3 = "N";
				AssertEquals("RelatedIndicator3 is true, ZG_RelatedIndicator3 is N", "1100", AISMessageProviderHelper.GetValuationIndicator(invoiceHeader, invoiceLine));

				invoiceLine.ZG_RelatedIndicator3 = "Y";
				AssertEquals("RelatedIndicator3 is true, ZG_RelatedIndicator3 is Y", "1110", AISMessageProviderHelper.GetValuationIndicator(invoiceHeader, invoiceLine));
			});

			CombineAssertions("4th character", () =>
			{
				invoiceHeader.RelatedIndicator4 = true;
				AssertEquals("RelatedIndicator4 is true, ZG_RelatedIndicator4 is empty", "1111", AISMessageProviderHelper.GetValuationIndicator(invoiceHeader, invoiceLine));

				invoiceLine.ZG_RelatedIndicator4 = "N";
				AssertEquals("RelatedIndicator4 is true, ZG_RelatedIndicator4 is N", "1110", AISMessageProviderHelper.GetValuationIndicator(invoiceHeader, invoiceLine));

				invoiceLine.ZG_RelatedIndicator4 = "Y";
				AssertEquals("RelatedIndicator4 is true, ZG_RelatedIndicator4 is Y", "1111", AISMessageProviderHelper.GetValuationIndicator(invoiceHeader, invoiceLine));
			});
		}

		public void TestGetAdditionsAndDeductions_ConvertToLocalCurrency()
		{
			(_, var entryLineWrapper) = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);

			var invoiceHeader = entryLineWrapper.RandomInvoiceHeader;
			var invoiceLine = entryLineWrapper.RandomInvoiceLine;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();

			var apportionedCharge1 = invoiceLine.ApportionedCharges.AddNew();
			apportionedCharge1.J7_ChargeType = "AD";
			apportionedCharge1.J7_Amount = 1m;
			apportionedCharge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

			var apportionedCharge2 = invoiceLine.ApportionedCharges.AddNew();
			apportionedCharge2.J7_ChargeType = "BD";
			apportionedCharge2.J7_Amount = 2m;
			apportionedCharge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;

			var additionsAndDeductions = AISMessageProviderHelper.GetAdditionsAndDeductions(invoiceHeader.InvoiceLines.Cast<JobComInvoiceLine>());

			AssertEquals("EUR remain itself", 1m, additionsAndDeductions.FirstOrDefault(a => a.Code == "AD").Amount);
			var actualBDAmount = new Money(2m, RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates));
			var expectedBDAmount = invoiceHeader.CurrencyConverter.ConvertExact(actualBDAmount, RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.EuropeanUnion));
			AssertEquals("Other currency should convert to EUR", expectedBDAmount.Amount, additionsAndDeductions.FirstOrDefault(a => a.Code == "BD").Amount);
		}

		public void TestGetAdditionsAndDeductionsUCC5()
		{
			var invoiceHeader = GetAdditionsAndDeductionsTestData();
			using var tempConfig = EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(invoiceHeader.JobDeclaration, "IsUCC5Core", true);

			var additionsAndDeductions = AISMessageProviderHelper.GetAdditionsAndDeductions(invoiceHeader.InvoiceLines.Cast<JobComInvoiceLine>()).ToArray();
			CombineAssertions("GetAdditionsAndDeductions UCC5", () =>
			{
				AssertEquals("AdditionsAndDeduction Count", 7, additionsAndDeductions.Length);
				AssertEquals("AdditionsAndDeduction AD amount", 61m, additionsAndDeductions.FirstOrDefault(a => a.Code == "AD").Amount);
				AssertEquals("AdditionsAndDeduction BD amount", 5m, additionsAndDeductions.FirstOrDefault(a => a.Code == "BD").Amount);
				AssertEquals("AdditionsAndDeduction BA amount", 0m, additionsAndDeductions.FirstOrDefault(a => a.Code == "BA").Amount);
				AssertEquals("AdditionsAndDeduction 1X amount", 0m, additionsAndDeductions.FirstOrDefault(a => a.Code == "1X").Amount);
				AssertEquals("AdditionsAndDeduction BC amount", 0m, additionsAndDeductions.FirstOrDefault(a => a.Code == "BC").Amount);
				AssertEquals("AdditionsAndDeduction ONS amount", 135.79m, additionsAndDeductions.FirstOrDefault(a => a.Code == "ONS").Amount);
				AssertEquals("AdditionsAndDeduction AK amount", 23.45m, additionsAndDeductions.FirstOrDefault(a => a.Code == "AK").Amount);
			});
		}

		public void TestGetAdditionsAndDeductionsUCC6()
		{
			var invoiceHeader = GetAdditionsAndDeductionsTestData();
			var additionsAndDeductions = AISMessageProviderHelper.GetAdditionsAndDeductions(invoiceHeader.InvoiceLines.Cast<JobComInvoiceLine>());
			CombineAssertions("GetAdditionsAndDeductions UCC6", () =>
			{
				AssertEquals("AdditionsAndDeduction Count, BC not valid when UCC6.", 6, additionsAndDeductions.Count);
				AssertEquals("AdditionsAndDeduction AD amount", 61m, additionsAndDeductions.FirstOrDefault(a => a.Code == "AD").Amount);
				AssertEquals("AdditionsAndDeduction BD amount", 5m, additionsAndDeductions.FirstOrDefault(a => a.Code == "BD").Amount);
				AssertEquals("AdditionsAndDeduction BA amount", 0m, additionsAndDeductions.FirstOrDefault(a => a.Code == "BA").Amount);
				AssertEquals("AdditionsAndDeduction 1X amount", 0m, additionsAndDeductions.FirstOrDefault(a => a.Code == "1X").Amount);
				AssertEquals("AdditionsAndDeduction ONS amount", 135.79m, additionsAndDeductions.FirstOrDefault(a => a.Code == "ONS").Amount);
				AssertEquals("AdditionsAndDeduction AK amount", 23.45m, additionsAndDeductions.FirstOrDefault(a => a.Code == "AK").Amount);
			});
		}

		JobComInvoiceHeader GetAdditionsAndDeductionsTestData()
		{
			(_, var entryLineWrapper) = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);

			var invoiceHeader = entryLineWrapper.RandomInvoiceHeader;
			var invoiceLine = entryLineWrapper.RandomInvoiceLine;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();

			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;

			var invoiceHeaderZeroCharge = invoiceHeader.Charges.AddNew();
			invoiceHeaderZeroCharge.J7_ChargeType = "BA";
			invoiceHeaderZeroCharge.J7_Amount = 0m;

			var invoiceHeaderCharge2 = invoiceHeader.Charges.AddNew();
			invoiceHeaderCharge2.J7_ChargeType = "BD";
			invoiceHeaderCharge2.J7_Amount = 5m;

			var groupHeaderZeroCharge = invoiceHeader.GroupHeader.Charges.AddNew();
			groupHeaderZeroCharge.J7_ChargeType = "1X";
			groupHeaderZeroCharge.J7_Amount = 0m;

			var groupHeaderCharge2 = invoiceHeader.GroupHeader.Charges.AddNew();
			groupHeaderCharge2.J7_ChargeType = "AD";
			groupHeaderCharge2.J7_Amount = 1m;

			var groupHeaderZeroCharge2 = invoiceHeader.GroupHeader.Charges.AddNew();
			groupHeaderZeroCharge2.J7_ChargeType = "BC";
			groupHeaderZeroCharge2.J7_Amount = 0.00m;

			var charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = "AD";
			charge.J7_Amount = 10m;

			var charge2 = invoiceLine.Charges.AddNew();
			charge2.J7_ChargeType = "AD";
			charge2.J7_Amount = 20m;

			var invoiceLineONSDutiable = invoiceLine.Charges.AddNew("ONS");
			invoiceLineONSDutiable.J7_IsDutiable = true;
			invoiceLineONSDutiable.J7_Amount = 123.45m;

			var invoiceLineONSNondutiable = invoiceLine.Charges.AddNew("ONS");
			invoiceLineONSNondutiable.J7_IsDutiable = false;
			invoiceLineONSNondutiable.J7_Amount = 12.34m;

			var invoiceLineAK = invoiceLine.Charges.AddNew("AK");
			invoiceLineAK.J7_Amount = 23.45m;

			var charge3 = invoiceLine2.Charges.AddNew();
			charge3.J7_ChargeType = "AD";
			charge3.J7_Amount = 30m;

			var charge4 = invoiceLine2.Charges.AddNew();
			charge4.J7_ChargeType = "BC";

			var charge5 = invoiceLine2.Charges.AddNew();
			charge5.J7_Amount = 40m;

			var apportionedCharge1 = invoiceLine.ApportionedCharges.AddNew();
			apportionedCharge1.J7_ChargeType = "AD";
			apportionedCharge1.J7_Amount = 1m;
			apportionedCharge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

			var apportionedCharge2 = invoiceLine.ApportionedCharges.AddNew();
			apportionedCharge2.J7_ChargeType = "BD";
			apportionedCharge2.J7_Amount = 2m;
			apportionedCharge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

			var apportionedCharge3 = invoiceLine2.ApportionedCharges.AddNew();
			apportionedCharge3.J7_ChargeType = "BD";
			apportionedCharge3.J7_Amount = 3m;
			apportionedCharge3.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

			var apportionedCharge4 = invoiceLine2.ApportionedCharges.AddNew();
			apportionedCharge4.J7_ChargeType = "BC";
			apportionedCharge4.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

			var apportionedCharge5 = invoiceLine2.ApportionedCharges.AddNew();
			apportionedCharge5.J7_Amount = 4m;
			apportionedCharge5.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

			return invoiceHeader;
		}
	}
}
