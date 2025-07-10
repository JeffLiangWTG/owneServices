using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using BasePackage = Enterprise.Customs.Business.BasePackage;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsBillCustomsEntryIntegratorTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(
				"When houseConsignment is null",
				() => new NctsBillCustomsEntryIntegrator(houseConsignment: null));

			AssertExceptionThrown<ArgumentNullException>(
				"When headerContainer.Header is null",
				() => new NctsBillCustomsEntryIntegrator(Factory.New<NctsBill>()));
		}

		public void TestCopyCustomsEntryGuardClauses()
		{
			AssertExceptionThrown<ArgumentNullException>(
				"When entryHeader is null",
				() => customsEntryIntegrator.CopyCustomsEntry(entryHeader: null));

			AssertExceptionThrown<ArgumentNullException>(
				"When entryHeader.JobDeclaration is null",
				() => customsEntryIntegrator.CopyCustomsEntry(Factory.New<CusEntryHeader>()));
		}

		public void TestAllCustomsEntryLinesAreCopiedIntoNewGoodsItems()
		{
			AssertEquals("PRE-CONDITION: GoodsItems Count", 0, houseConsignment.GoodsItems.Count);

			entryHeader.MergedLines.AddNew();
			entryHeader.MergedLines.AddNew();
			entryHeader.MergedLines.AddNew();
			customsEntryIntegrator.CopyCustomsEntry(entryHeader);
			AssertEquals("POST-CONDITION: GoodsItems Count", 3, houseConsignment.GoodsItems.Count);
		}

		public void TestCustomsEntryLinesAreIntegratedOrderedByLineNumber()
		{
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			var invoiceLine2 = entryLine2.InvoiceLines.AddNew();
			invoiceLine2.JI_Description = "DESC 2";

			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var invoiceLine1 = entryLine1.InvoiceLines.AddNew();
			invoiceLine1.JI_Description = "DESC 1";

			customsEntryIntegrator.CopyCustomsEntry(entryHeader);
			CombineAssertions(() =>
			{
				var goodsItems = houseConsignment.GoodsItems;
				AssertEquals("BY_Description [0]", "DESC 1", goodsItems[0].BY_Description);
				AssertEquals("BY_Description [1]", "DESC 2", goodsItems[1].BY_Description);
			});
		}

		public void TestCopyGoodsDescription()
		{
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine1 = entryLine.InvoiceLines.AddNew();
			invoiceLine1.JI_Description = "DESC 1";
			var invoiceLine2 = entryLine.InvoiceLines.AddNew();
			invoiceLine2.JI_Description = "DESC 2";

			customsEntryIntegrator.CopyCustomsEntry(entryHeader);
			AssertEquals("BY_Description", "DESC 1", houseConsignment.GoodsItems[0].BY_Description);
		}

		public void TestCopyGrossWeight()
		{
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine1 = entryLine.InvoiceLines.AddNew();
			invoiceLine1.JI_Weight = 100.1236m;
			invoiceLine1.JI_WeightUQ = "KG";
			var invoiceLine2 = entryLine.InvoiceLines.AddNew();
			invoiceLine2.JI_Weight = 1500m;
			invoiceLine2.JI_WeightUQ = "G";

			customsEntryIntegrator.CopyCustomsEntry(entryHeader);
			CombineAssertions(() =>
			{
				var goodsItem = houseConsignment.GoodsItems[0];
				AssertEquals("BY_GrossWeight", new ZDecimal(101.624m), goodsItem.BY_GrossWeight);
				AssertEquals("BY_GrossWeightUnit", "KG", goodsItem.BY_GrossWeightUnit);
			});
		}

		public void TestCopyNetWeight()
		{
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine1 = entryLine.InvoiceLines.AddNew();
			invoiceLine1.JI_NetWeight = 100.1236m;
			invoiceLine1.JI_NetWeightUQ = "KG";
			var invoiceLine2 = entryLine.InvoiceLines.AddNew();
			invoiceLine2.JI_NetWeight = 1500m;
			invoiceLine2.JI_NetWeightUQ = "G";

			customsEntryIntegrator.CopyCustomsEntry(entryHeader);
			CombineAssertions(() =>
			{
				var goodsItem = houseConsignment.GoodsItems[0];
				AssertEquals("BY_NetWeight", new ZDecimal(101.624m), goodsItem.BY_NetWeight);
				AssertEquals("BY_NetWeightUnit", "KG", goodsItem.BY_NetWeightUnit);
			});
		}

		public void TestCopyTariff()
		{
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine1 = entryLine.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "00000";
			var invoiceLine2 = entryLine.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "11111";

			customsEntryIntegrator.CopyCustomsEntry(entryHeader);
			AssertEquals("BY_HarmonisedTariff", "00000", houseConsignment.GoodsItems[0].BY_HarmonisedTariff);
		}

		public void TestCopySupplementaryQuantity()
		{
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine1 = entryLine.InvoiceLines.AddNew();
			invoiceLine1.JI_CustomsSecondQuantity = 100.123456m;
			invoiceLine1.JI_CustomsSecondUnitQty = "ASV";
			var invoiceLine2 = entryLine.InvoiceLines.AddNew();
			invoiceLine2.JI_CustomsSecondQuantity = 150m;
			invoiceLine2.JI_CustomsSecondUnitQty = "ASV";

			customsEntryIntegrator.CopyCustomsEntry(entryHeader);
			CombineAssertions(() =>
			{
				var goodsItem = houseConsignment.GoodsItems[0];
				AssertEquals("BY_CustomsSecondQuantity", new ZDecimal(250.123456m), goodsItem.BY_CustomsSecondQuantity);
				AssertEquals("BY_CustomsSecondUnitQty", "ASV", goodsItem.BY_CustomsSecondUnitQty);
			});
		}

		public void TestCopyDangerousGoods()
		{
			var dangerousSubstance = UNDGSubstanceLoader.LoadSubstances(declarationFactory, "0004", "a", "IMO").First();

			var invoice = declaration.Invoices.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;

			var undg1 = invoiceLine1.UNDGs.AddNew();
			undg1.DI_DG = dangerousSubstance.PK;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			var undg2 = invoiceLine2.UNDGs.AddNew();
			undg2.DI_DG = dangerousSubstance.PK;

			customsEntryIntegrator.CopyCustomsEntry(entryHeader);
			CombineAssertions(() =>
			{
				var goodsItem = houseConsignment.GoodsItems[0];
				var goodsItemUNDGs = goodsItem.UNDGs;
				AssertEquals("UNDGs Count", 2, goodsItemUNDGs.Count);
				AssertEquals("GoodsItem UNDGsAsString", "0004a,0004a", goodsItem.UNDGsAsString);

				AssertContainsExactElementsInAnyOrder(
					"UNDGs DI_ParentID",
					new[] { goodsItem.PK },
					goodsItemUNDGs.Select(x => x.DI_ParentID).Distinct().ToArray());

				AssertContainsExactElementsInAnyOrder(
					"UNDGs DI_ParentTableCode",
					new[] { "BY" },
					goodsItemUNDGs.Select(x => x.DI_ParentTableCode).Distinct().ToArray());
			});
		}

		public void TestCopyCusCode()
		{
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			invoiceLine1.ZG_CusNumber = "CUS1";
			var invoiceLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			invoiceLine2.ZG_CusNumber = "CUS2";

			customsEntryIntegrator.CopyCustomsEntry(entryHeader);
			AssertEquals("BY_CusC4Number", "CUS1", houseConsignment.GoodsItems[0].BY_CusC4Number);
		}

		public void TestCopyCustomsQuantityIfAllInvoiceLinesHaveKGMUnit()
		{
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine1 = entryLine.InvoiceLines.AddNew();
			invoiceLine1.JI_CustomsQuantity = 100m;
			invoiceLine1.JI_CustomsUnitQty = "KGM";
			var invoiceLine2 = entryLine.InvoiceLines.AddNew();
			invoiceLine2.JI_CustomsQuantity = 150m;
			invoiceLine2.JI_CustomsUnitQty = "KGM";

			customsEntryIntegrator.CopyCustomsEntry(entryHeader);
			AssertEquals("CustomsFirstQuantityInKilograms", new ZDecimal(250m), houseConsignment.GoodsItems[0].CustomsFirstQuantityInKilograms);
		}

		public void TestCopyCustomsQuantityIfAllInvoiceLinesDoNotHaveKGMUnit()
		{
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine1 = entryLine.InvoiceLines.AddNew();
			invoiceLine1.JI_CustomsQuantity = 100m;
			invoiceLine1.JI_CustomsUnitQty = "ABC";
			var invoiceLine2 = entryLine.InvoiceLines.AddNew();
			invoiceLine2.JI_CustomsQuantity = 150m;
			invoiceLine2.JI_CustomsUnitQty = "KGM";

			customsEntryIntegrator.CopyCustomsEntry(entryHeader);
			AssertEquals("CustomsFirstQuantityInKilograms", new ZDecimal(0m), houseConsignment.GoodsItems[0].CustomsFirstQuantityInKilograms);
		}

		public void TestCopyLinePrice()
		{
			var entryLine = entryHeader.MergedLines.AddNew();

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "EUR";
			var invoiceLine1 = entryLine.InvoiceLines.AddNew();
			invoiceLine1.JI_JZ = invoice1.PK;
			invoiceLine1.JI_LinePrice = 100m;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = "EUR";
			var invoiceLine2 = entryLine.InvoiceLines.AddNew();
			invoiceLine2.JI_JZ = invoice2.PK;
			invoiceLine2.JI_LinePrice = 150m;

			customsEntryIntegrator.CopyCustomsEntry(entryHeader);
			CombineAssertions(() =>
			{
				var goodsItem = houseConsignment.GoodsItems[0];
				AssertEquals("BY_MonetaryValue", new ZDecimal(250m), goodsItem.BY_MonetaryValue);
				AssertEquals("BY_RX_NKCurrency", "EUR", goodsItem.BY_RX_NKCurrency);
			});
		}

		public void TestCopyLinePriceWithCurrencyConversion()
		{
			var newCurrency = RefCurrency.New(declarationFactory);
			newCurrency.RX_Code = "XYZ";
			newCurrency.SetCustomsRate(ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, 0.5m);

			var entryLine = entryHeader.MergedLines.AddNew();

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "EUR";
			var invoiceLine1 = entryLine.InvoiceLines.AddNew();
			invoiceLine1.JI_JZ = invoice1.PK;
			invoiceLine1.JI_LinePrice = 100m;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = "XYZ";
			var invoiceLine2 = entryLine.InvoiceLines.AddNew();
			invoiceLine2.JI_JZ = invoice2.PK;
			invoiceLine2.JI_LinePrice = 150m;

			customsEntryIntegrator.CopyCustomsEntry(entryHeader);
			CombineAssertions(() =>
			{
				var goodsItem = houseConsignment.GoodsItems[0];
				AssertEquals("BY_MonetaryValue", new ZDecimal(400m), goodsItem.BY_MonetaryValue);
				AssertEquals("BY_RX_NKCurrency", "EUR", goodsItem.BY_RX_NKCurrency);
			});
		}

		public void TestAddNewN830MovementReferenceNumberIfExportAtHouseConsignmentLevel_InTransitionPeriod()
		{
			using (SetTransitionPeriod(isActive: true))
			{
				declaration.JE_MessageType = "EXP";
				entryHeader.MovementReferenceNumberSetter("MyMRN");
				entryHeader.MergedLines.AddNew();

				customsEntryIntegrator.CopyCustomsEntry(entryHeader);

				var previousDocuments = houseConsignment.PreviousDocuments;
				AssertEquals("PreviousDocuments Count", 0, previousDocuments.Count);
			}
		}

		public void TestAddNewN830MovementReferenceNumberIfExportAtGoodsItemLevel_InTransitionPeriod()
		{
			using (SetTransitionPeriod(isActive: true))
			{
				declaration.JE_MessageType = "EXP";
				entryHeader.MovementReferenceNumberSetter("MyMRN");
				var entryLine0 = entryHeader.MergedLines.AddNew();
				entryLine0.CL_LineNumber = 1;
				var entryLine1 = entryHeader.MergedLines.AddNew();
				entryLine1.CL_LineNumber = 2;

				customsEntryIntegrator.CopyCustomsEntry(entryHeader);
				CombineAssertions(() =>
				{
					var previousDocuments = houseConsignment.GoodsItems[0].PreviousDocuments;
					AssertEquals("PreviousDocuments Count", 1, previousDocuments.Count);

					var previousDocument = previousDocuments[0];
					AssertEquals("CSI_Code", "N830", previousDocument.CSI_Code);
					AssertEquals("CSI_ReferenceNumber", "MyMRN", previousDocument.CSI_ReferenceNumber);
					AssertEquals("CSI_ItemNumber", entryLine0.CL_LineNumber, previousDocument.CSI_ItemNumber);

					previousDocuments = houseConsignment.GoodsItems[1].PreviousDocuments;
					AssertEquals("PreviousDocuments Count", 1, previousDocuments.Count);

					previousDocument = previousDocuments[0];
					AssertEquals("CSI_Code", "N830", previousDocument.CSI_Code);
					AssertEquals("CSI_ReferenceNumber", "MyMRN", previousDocument.CSI_ReferenceNumber);
					AssertEquals("CSI_ItemNumber", entryLine1.CL_LineNumber, previousDocument.CSI_ItemNumber);
				});
			}
		}

		public void TestDoNotCreateN830PreviousDocumentAtGoodsItemLevelIfMrnIsEmpty()
		{
			using (SetTransitionPeriod(isActive: true))
			{
				declaration.JE_MessageType = "EXP";
				entryHeader.MovementReferenceNumberSetter("");
				entryHeader.MergedLines.AddNew();

				customsEntryIntegrator.CopyCustomsEntry(entryHeader);

				var previousDocuments = houseConsignment.GoodsItems[0].PreviousDocuments;
				AssertEquals("PreviousDocuments Count", 0, previousDocuments.Count);
			}
		}

		public void TestCreateN830PreviousDocumentAtHouseConsignmentLevel()
		{
			using (SetTransitionPeriod(isActive: false))
			{
				declaration.JE_MessageType = "EXP";
				entryHeader.MovementReferenceNumberSetter("MyMRN");
				entryHeader.MergedLines.AddNew();

				customsEntryIntegrator.CopyCustomsEntry(entryHeader);

				var previousDocuments = houseConsignment.PreviousDocuments;
				AssertEquals("PreviousDocuments Count", 1, previousDocuments.Count);
				CombineAssertions(() =>
				{
					AssertEquals("CSI_Code", "N830", previousDocuments[0].CSI_Code);
					AssertEquals("CSI_ReferenceNumber", "MyMRN", previousDocuments[0].CSI_ReferenceNumber);
				});
			}
		}

		public void TestDoNotCreateN830PreviousDocumentIfDeclarationIsImport()
		{
			declaration.JE_MessageType = "IMP";
			entryHeader.MovementReferenceNumberSetter("MyMRN");
			entryHeader.MergedLines.AddNew();

			customsEntryIntegrator.CopyCustomsEntry(entryHeader);

			CombineAssertions(() =>
			{
				AssertEquals("HouseConsignment PreviousDocuments Count", 0, houseConsignment.PreviousDocuments.Count);
				AssertEquals("GoodsItem PreviousDocuments Count", 0, houseConsignment.GoodsItems[0].PreviousDocuments.Count);
			});
		}

		public void TestCreateNewHeaderContainers()
		{
			var cusContainer1 = AddNewCusContainer(declaration, "CNT1", "S1", "S2");
			AddNewBasePackageWithContainer(declaration, "1A", 1, "MARK1", cusContainer1.PK);
			var cusContainer2 = AddNewCusContainer(declaration, "CNT2", "", "");
			AddNewBasePackageWithContainer(declaration, "2A", 2, "MARK2", cusContainer2.PK);

			var entryLine = entryHeader.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = entryLine.InvoiceLines.AddNew();
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;
			invoiceLine.PackagesForInvoiceLinesForBindingOnly[1].IsLinked = true;

			customsEntryIntegrator.CopyCustomsEntry(entryHeader);

			AssertEquals("HeaderContainers Count", 2, nctsHeader.DepartureHeaderContainers.Count);
			CombineAssertions(() =>
			{
				AssertNctsHeaderContainer(nctsHeader.DepartureHeaderContainers[0], "CNT1", "CNT", "S1", "S2", Array.Empty<string>());
				AssertNctsHeaderContainer(nctsHeader.DepartureHeaderContainers[1], "CNT2", "CNT", "", "", Array.Empty<string>());
			});
		}

		public void TestDoNotCreateHeaderContainersForUnlinkedItems()
		{
			var cusContainer1 = AddNewCusContainer(declaration, "CNT1", "S1", "S2");
			AddNewBasePackageWithContainer(declaration, "1A", 1, "MARK1", cusContainer1.PK);

			var entryLine = entryHeader.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = entryLine.InvoiceLines.AddNew();
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = false;

			customsEntryIntegrator.CopyCustomsEntry(entryHeader);

			AssertEquals("HeaderContainers Count", 0, nctsHeader.DepartureHeaderContainers.Count);
		}

		public void TestUpdateExistingHeaderContainers()
		{
			var nctsHeaderContainer1 = AddNewNctsHeaderContainer(nctsHeader, "CNT1", "CNT", "", "", Array.Empty<string>());
			var nctsHeaderContainer2 = AddNewNctsHeaderContainer(nctsHeader, "CNT2", "CNT", "SX", "", Array.Empty<string>());
			var nctsHeaderContainer3 = AddNewNctsHeaderContainer(nctsHeader, "CNT3", "CNT", "SX", "SY", Array.Empty<string>());
			var cusContainer1 = AddNewCusContainer(declaration, "CNT1", "S1", "S2");
			AddNewBasePackageWithContainer(declaration, "1A", 1, "MARK1", cusContainer1.PK);
			var cusContainer2 = AddNewCusContainer(declaration, "CNT2", "S1", "S2");
			AddNewBasePackageWithContainer(declaration, "2A", 1, "MARK2", cusContainer2.PK);
			var cusContainer3 = AddNewCusContainer(declaration, "CNT3", "S1", "S2");
			AddNewBasePackageWithContainer(declaration, "3A", 1, "MARK3", cusContainer3.PK);

			var entryLine = entryHeader.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = entryLine.InvoiceLines.AddNew();
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;
			invoiceLine.PackagesForInvoiceLinesForBindingOnly[1].IsLinked = true;
			invoiceLine.PackagesForInvoiceLinesForBindingOnly[2].IsLinked = true;

			customsEntryIntegrator.CopyCustomsEntry(entryHeader);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(
					"HeaderContainers Items",
					new[] { nctsHeaderContainer1, nctsHeaderContainer2, nctsHeaderContainer3 },
					nctsHeader.DepartureHeaderContainers);
				AssertNctsHeaderContainer(nctsHeaderContainer1, "CNT1", "CNT", "S1", "S2", Array.Empty<string>());
				AssertNctsHeaderContainer(nctsHeaderContainer2, "CNT2", "CNT", "SX", "S1", new[] { "S2" });
				AssertNctsHeaderContainer(nctsHeaderContainer3, "CNT3", "CNT", "SX", "SY", new[] { "S1", "S2" });
			});
		}

		public void TestCopyPackagingDetailsWithContainers()
		{
			var cusContainer1 = AddNewCusContainer(declaration, "CNT1", "", "");
			AddNewBasePackageWithContainer(declaration, "1A", 1, "MARK1", cusContainer1.PK);
			var cusContainer2 = AddNewCusContainer(declaration, "CNT2", "", "");
			AddNewBasePackageWithContainer(declaration, "2A", 2, "MARK2", cusContainer2.PK);
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = entryLine.InvoiceLines.AddNew();
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;
			invoiceLine.PackagesForInvoiceLinesForBindingOnly[0].PackQty = 1;
			invoiceLine.PackagesForInvoiceLinesForBindingOnly[1].IsLinked = false;

			customsEntryIntegrator.CopyCustomsEntry(entryHeader);

			var goodsItemPackages = houseConsignment.GoodsItems[0].Packages;
			AssertEquals("Goods Item Packages Count", 1, goodsItemPackages.Count);
			CombineAssertions(() => AssertNctsPackage(goodsItemPackages[0], "1A", 1, "MARK1", new[] { "CNT1" }));
		}

		public void TestCopyPackagingDetailsWithoutContainer()
		{
			AddNewBasePackage(declaration, "1A", 1, "MARK1");
			AddNewBasePackage(declaration, "2A", 2, "MARK2");
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = entryLine.InvoiceLines.AddNew();
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;
			invoiceLine.PackagesForInvoiceLinesForBindingOnly[0].PackQty = 1;
			invoiceLine.PackagesForInvoiceLinesForBindingOnly[1].IsLinked = false;

			customsEntryIntegrator.CopyCustomsEntry(entryHeader);

			var goodsItemPackages = houseConsignment.GoodsItems[0].Packages;
			AssertEquals("Goods Item Packages Count", 1, goodsItemPackages.Count);
			CombineAssertions(() => AssertNctsPackage(goodsItemPackages[0], "1A", 1, "MARK1", Array.Empty<string>()));
		}

		public void TestCopyPackagingDetailsWithoutContainer_WhenActiveCopyFromPreviousLine() => CombineAssertions(() =>
		{
			using (CustomsDataRegistry.Instance.AlwaysCopyFromPreviousLine.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AddNewBasePackage(declaration, "1A", 1, "JPB001");
				AddNewBasePackage(declaration, "2A", 2, "JPB002");

				var entryLine1 = entryHeader.MergedLines.AddNew();
				var entryLine2 = entryHeader.MergedLines.AddNew();
				var invoice = declaration.Invoices.AddNew();

				var invoiceLine1 = entryLine1.InvoiceLines.AddNew();
				invoiceLine1.JI_JZ = invoice.PK;
				invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;
				invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0].PackQty = 1;
				invoiceLine1.PackagesForInvoiceLinesForBindingOnly[1].IsLinked = false;

				var invoiceLine2 = entryLine2.InvoiceLines.AddNew();
				invoiceLine2.JI_JZ = invoice.PK;
				invoiceLine2.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;
				invoiceLine2.PackagesForInvoiceLinesForBindingOnly[1].IsLinked = true;

				customsEntryIntegrator.CopyCustomsEntry(entryHeader);

				var goodItem = houseConsignment.GoodsItems;
				AssertEquals("Expected 1 package in the first Good Item", 1, goodItem[0].Packages.Count);
				AssertEquals("Expected 2 packages in the second Good Item", 2, goodItem[1].Packages.Count);
			}
		});

		protected override void SetUp()
		{
			base.SetUp();

			declarationFactory = new BusinessObjectFactory();
			declaration = declarationFactory.New<JobDeclaration>();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			houseConsignment = nctsHeader.Bills.AddNew();

			customsEntryIntegrator = new NctsBillCustomsEntryIntegrator(houseConsignment);
		}

		BusinessObjectFactory declarationFactory;
		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		NctsHeader nctsHeader;
		NctsBill houseConsignment;
		INctsCustomsEntryIntegrator customsEntryIntegrator;

		static IDisposable SetTransitionPeriod(bool isActive)
			=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
				Constants.FunctionalityTypes.NCTSTransitionPeriod,
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				ZDate.Today,
				isActive);

		static void AssertNctsHeaderContainer(
			NctsDepartureHeaderContainer nctsHeaderContainer,
			string expectedContainerNum,
			string expectedContainerMode,
			string expectedSeal1,
			string expectedSeal2,
			string[] expectedAdditionalSeals)
		{
			AssertEquals("BC_ContainerNum", expectedContainerNum, nctsHeaderContainer.BC_ContainerNum);
			AssertEquals("BC_Mode", expectedContainerMode, nctsHeaderContainer.BC_Mode);
			AssertEquals("BC_Seal1", expectedSeal1, nctsHeaderContainer.BC_Seal1);
			AssertEquals("BC_Seal2", expectedSeal2, nctsHeaderContainer.BC_Seal2);
			var actualAdditionalSeals = nctsHeaderContainer.AdditionalSeals.Select(x => x.BK_SealNumber);
			AssertContainsExactElementsInAnyOrder("AdditionalSeals", expectedAdditionalSeals, actualAdditionalSeals);
		}

		static NctsDepartureHeaderContainer AddNewNctsHeaderContainer(
			NctsHeader nctsHeader,
			string containerNumber,
			string containerMode,
			string seal1,
			string seal2,
			string[] additionalSeals)
		{
			var container = nctsHeader.DepartureHeaderContainers.AddNew();
			container.BC_ContainerNum = containerNumber;
			container.BC_Mode = containerMode;
			container.BC_Seal1 = seal1;
			container.BC_Seal2 = seal2;
			additionalSeals.ForEach(x => container.AdditionalSeals.AddNew().BK_SealNumber = x);
			return container;
		}

		static CusContainer AddNewCusContainer(
			JobDeclaration declaration,
			string containerNumber,
			string seal1,
			string seal2)
		{
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = containerNumber;
			container.CO_Seal = seal1;
			container.CO_SecondSeal = seal2;
			return container;
		}

		static BasePackage AddNewBasePackage(
			JobDeclaration declaration,
			ZString packType,
			ZInt packQuantity,
			ZString marksAndNos)
		{
			var package = declaration.Packages.AddNew();
			package.CW_PackType = packType;
			package.CW_PackQty = packQuantity;
			package.CW_MarksAndNos = marksAndNos;
			return package;
		}

		static BasePackage AddNewBasePackageWithContainer(
			JobDeclaration declaration,
			ZString packType,
			ZInt packQuantity,
			ZString marksAndNos,
			ZGuid containerPK)
		{
			var packGroup = declaration.PackingGroups.AddNew();
			packGroup.CR_CO_Container = containerPK;

			var package = AddNewBasePackage(declaration, packType, packQuantity, marksAndNos);
			package.CW_CR_HouseContainer = packGroup.PK;
			return package;
		}

		static void AssertNctsPackage(
			NctsPackage package,
			string expectedUnitType,
			int expectedUnitCount,
			string expectedMarksAndNumbers,
			string[] expectedContainersSelected)
		{
			AssertEquals("B5_UnitType", expectedUnitType, package.B5_UnitType);
			AssertEquals("B5_UnitCount", expectedUnitCount, package.B5_UnitCount);
			AssertEquals("B5_MarksAndNumbers", expectedMarksAndNumbers, package.B5_MarksAndNumbers);
			AssertContainsExactElementsInAnyOrder("Containers Selected", expectedContainersSelected, package.ContainersSelected);
		}
	}
}
