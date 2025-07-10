using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business.Declaration;
using JobDeclaration = Enterprise.Customs.ES.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class AESCommonTransportEquipmentWrapperTest : WrapperHelperTest<AESCommonTransportEquipmentWrapper>
	{
		public void TestSequenceNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected filled SequenceNumber for first wrapper", "1", wrapper1.SequenceNumber);

				AssertEquals("Expected filled SequenceNumber for second wrapper", "2", wrapper2.SequenceNumber);

				AssertEquals("Expected filled SequenceNumber for third wrapper", "3", wrapper3.SequenceNumber);

				AssertEquals("Expected filled SequenceNumber for fourth wrapper", "4", wrapper4.SequenceNumber);

				AssertEquals("Expected filled SequenceNumber for fifth wrapper", "5", wrapper5.SequenceNumber);

				AssertEquals("Expected filled SequenceNumber for sixth wrapper", "6", wrapper6.SequenceNumber);

				AssertEquals("Expected filled SequenceNumber for seventh wrapper", "7", wrapper7.SequenceNumber);
			});
		}

		public void TestContainerNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected filled ContainerNumber for first wrapper", "CONT1", wrapper1.ContainerNumber);

				AssertEquals("Expected filled ContainerNumber for second wrapper", "CONT2", wrapper2.ContainerNumber);

				AssertEquals("Expected filled ContainerNumber for third wrapper", "CONT3", wrapper3.ContainerNumber);

				AssertEquals("Expected filled ContainerNumber for fourth wrapper", "CONT4", wrapper4.ContainerNumber);

				AssertEquals("Expected empty ContainerNumber for fifth wrapper", ZString.Empty, wrapper5.ContainerNumber);

				AssertEquals("Expected empty ContainerNumber for sixth wrapper", ZString.Empty, wrapper6.ContainerNumber);

				AssertEquals("Expected empty ContainerNumber for seventh wrapper", ZString.Empty, wrapper7.ContainerNumber);
			});
		}

		public void TestNumberOfSeals()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected filled NumberOfSeals for first wrapper", "1", wrapper1.NumberOfSeals);

				AssertEquals("Expected filled NumberOfSeals for second wrapper", "2", wrapper2.NumberOfSeals);

				AssertEquals("Expected filled NumberOfSeals for third wrapper", "2", wrapper3.NumberOfSeals);

				AssertEquals("Expected filled NumberOfSeals for fourth wrapper", "0", wrapper4.NumberOfSeals);

				AssertEquals("Expected filled NumberOfSeals for fifth wrapper", "3", wrapper5.NumberOfSeals);

				AssertEquals("Expected filled NumberOfSeals for sixth wrapper", "3", wrapper6.NumberOfSeals);

				AssertEquals("Expected filled NumberOfSeals for seventh wrapper", "0", wrapper7.NumberOfSeals);
			});
		}

		public void TestSeals()
		{
			CombineAssertions(() =>
			{
				var seals1 = wrapper1.Seals.ToList();
				AssertEquals("Expected filled Seals for first wrapper with count 1", 1, seals1.Count);
				AssertEquals("Expected filled Seals for first wrapper, SequenceNumber", "1", seals1[0].SequenceNumber);
				AssertEquals("Expected filled Seals for first wrapper, SealNumber", "SEAL11", seals1[0].SealNumber);

				var seals2 = wrapper2.Seals.ToList();
				AssertEquals("Expected filled Seals for second wrapper with count 2", 2, seals2.Count);
				AssertEquals("Expected filled Seals for second wrapper, SequenceNumber", "1", seals2[0].SequenceNumber);
				AssertEquals("Expected filled Seals for second wrapper, SealNumber", "SEAL22", seals2[0].SealNumber);
				AssertEquals("Expected filled Seals for second wrapper, SequenceNumber", "2", seals2[1].SequenceNumber);
				AssertEquals("Expected filled Seals for second wrapper, SealNumber", "SEALR", seals2[1].SealNumber);

				var seals3 = wrapper3.Seals.ToList();
				AssertEquals("Expected filled Seals for third wrapper with count 2", 2, seals3.Count);
				AssertEquals("Expected filled Seals for third wrapper, first SequenceNumber", "1", seals3[0].SequenceNumber);
				AssertEquals("Expected filled Seals for third wrapper, first SealNumber", "SEAL31", seals3[0].SealNumber);
				AssertEquals("Expected filled Seals for third wrapper, second SequenceNumber", "2", seals3[1].SequenceNumber);
				AssertEquals("Expected filled Seals for third wrapper, second SealNumber", "SEAL32", seals3[1].SealNumber);

				AssertEquals("Expected zero Seals for fourth wrapper with count 0", 0, wrapper4.Seals.Count);

				var seals5 = wrapper5.Seals.ToList();
				AssertEquals("Expected filled Seals for fifth wrapper with count 3", 3, seals5.Count);
				AssertEquals("Expected filled Seals for fifth wrapper, first SequenceNumber", "1", seals5[0].SequenceNumber);
				AssertEquals("Expected filled Seals for fifth wrapper, first SealNumber", "SEALE1", seals5[0].SealNumber);
				AssertEquals("Expected filled Seals for fifth wrapper, second SequenceNumber", "2", seals5[1].SequenceNumber);
				AssertEquals("Expected filled Seals for fifth wrapper, second SealNumber", "SEALE2", seals5[1].SealNumber);
				AssertEquals("Expected filled Seals for fifth wrapper, third SequenceNumber", "3", seals5[2].SequenceNumber);
				AssertEquals("Expected filled Seals for fifth wrapper, third SealNumber", "SEALE3", seals5[2].SealNumber);

				var seals6 = wrapper6.Seals.ToList();
				AssertEquals("Expected filled Seals for sixth wrapper with count 3", 3, seals6.Count);
				AssertEquals("Expected filled Seals for sixth wrapper, first SequenceNumber", "1", seals6[0].SequenceNumber);
				AssertEquals("Expected filled Seals for sixth wrapper, first SealNumber", "SEALE4", seals6[0].SealNumber);
				AssertEquals("Expected filled Seals for sixth wrapper, second SequenceNumber", "2", seals6[1].SequenceNumber);
				AssertEquals("Expected filled Seals for sixth wrapper, second SealNumber", "SEALE5", seals6[1].SealNumber);
				AssertEquals("Expected filled Seals for sixth wrapper, second SequenceNumber", "3", seals6[2].SequenceNumber);
				AssertEquals("Expected filled Seals for sixth wrapper, second SealNumber", "SEALR", seals6[2].SealNumber);

				AssertEquals("Expected zero Seals for seventh wrapper with count 0", 0, wrapper7.Seals.Count);
			});
		}

		public void TestGoodsReference()
		{
			CombineAssertions(() =>
			{
				var goodsreference1 = wrapper1.GoodsReference.ToList();
				AssertEquals("Expected filled GoodsReference for first wrapper with count 1", 1, goodsreference1.Count);
				AssertEquals("Expected filled GoodsReference for first wrapper, SequenceNumber", "1", goodsreference1[0].SequenceNumber);
				AssertEquals("Expected filled GoodsReference for first wrapper, GoodsItemNumber", "1", goodsreference1[0].GoodsItemNumber);

				var goodsreference2 = wrapper2.GoodsReference.ToList();
				AssertEquals("Expected filled GoodsReference for second wrapper with count 1", 1, goodsreference2.Count);
				AssertEquals("Expected filled GoodsReference for second wrapper, SequenceNumber", "1", goodsreference2[0].SequenceNumber);
				AssertEquals("Expected filled GoodsReference for second wrapper, GoodsItemNumber", "2", goodsreference2[0].GoodsItemNumber);

				var goodsreference3 = wrapper3.GoodsReference.ToList();
				AssertEquals("Expected filled GoodsReference for third wrapper with count 2", 2, goodsreference3.Count);
				AssertEquals("Expected filled GoodsReference for third wrapper, first SequenceNumber", "1", goodsreference3[0].SequenceNumber);
				AssertEquals("Expected filled GoodsReference for third wrapper, first GoodsItemNumber", "1", goodsreference3[0].GoodsItemNumber);
				AssertEquals("Expected filled GoodsReference for third wrapper, second SequenceNumber", "2", goodsreference3[1].SequenceNumber);
				AssertEquals("Expected filled GoodsReference for third wrapper, second GoodsItemNumber", "2", goodsreference3[1].GoodsItemNumber);

				var goodsreference4 = wrapper4.GoodsReference.ToList();
				AssertEquals("Expected filled GoodsReference for fourth wrapper with count 1", 1, goodsreference4.Count);
				AssertEquals("Expected filled GoodsReference for fourth wrapper, SequenceNumber", "1", goodsreference4[0].SequenceNumber);
				AssertEquals("Expected filled GoodsReference for fourth wrapper, GoodsItemNumber", "1", goodsreference4[0].GoodsItemNumber);

				var goodsreference5 = wrapper5.GoodsReference.ToList();
				AssertEquals("Expected filled GoodsReference for fifth wrapper with count 1", 1, goodsreference5.Count);
				AssertEquals("Expected filled GoodsReference for fifth wrapper, SequenceNumber", "1", goodsreference5[0].SequenceNumber);
				AssertEquals("Expected filled GoodsReference for fifth wrapper, GoodsItemNumber", "1", goodsreference5[0].GoodsItemNumber);

				var goodsreference6 = wrapper6.GoodsReference.ToList();
				AssertEquals("Expected filled GoodsReference for sixth wrapper with count 2", 2, goodsreference6.Count);
				AssertEquals("Expected filled GoodsReference for sixth wrapper, first SequenceNumber", "1", goodsreference6[0].SequenceNumber);
				AssertEquals("Expected filled GoodsReference for sixth wrapper, first GoodsItemNumber", "1", goodsreference6[0].GoodsItemNumber);
				AssertEquals("Expected filled GoodsReference for sixth wrapper, second SequenceNumber", "2", goodsreference6[1].SequenceNumber);
				AssertEquals("Expected filled GoodsReference for sixth wrapper, second GoodsItemNumber", "2", goodsreference6[1].GoodsItemNumber);

				var goodsreference7 = wrapper7.GoodsReference.ToList();
				AssertEquals("Expected filled GoodsReference for seventh wrapper with count 1", 1, goodsreference7.Count);
				AssertEquals("Expected filled GoodsReference for seventh wrapper, SequenceNumber", "1", goodsreference7[0].SequenceNumber);
				AssertEquals("Expected filled GoodsReference for seventh wrapper, GoodsItemNumber", "2", goodsreference7[0].GoodsItemNumber);
			});
		}

		public void TestGetTransportEquipmentList_NullEntryHeader()
		{
			var wrapperList = AESCommonTransportEquipmentWrapper.GetTransportEquipmentList(null);
			AssertEquals("Expected empty wrapper list", 0, wrapperList.Count);
		}

		public void TestGetTransportEquipmentList_NoContainersOrEquipment()
		{
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var wrapperList = AESCommonTransportEquipmentWrapper.GetTransportEquipmentList(entryHeader);
			AssertEquals("Expected empty wrapper list", 0, wrapperList.Count);
		}

		public void TestSealCodesOnlyContainersWithSeals()
		{
			declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine.JI_CL = entryLine.PK;

			var wrapperList = AESCommonTransportEquipmentWrapper.GetTransportEquipmentList(entryHeader);

			CombineAssertions(() =>
			{
				AssertEquals("Expected empty", 0, wrapperList.Count);

				foreach (var seal in ContainerSeals)
				{
					var package = declaration.Packages.AddNew();
					var ctnNumber = "CTN1" + seal;
					var container = CreateContainer(declaration, ctnNumber, seal: seal);
					package.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
					invoiceLine.ContainersPivot.AddNew().C2_CO = container.PK;
					invoiceLine.PackagesPivot.AddPivotFor(package);
				}
				wrapperList = AESCommonTransportEquipmentWrapper.GetTransportEquipmentList(entryHeader);
	
				AssertContainsExactElementsInAnyOrder("Expected filled Seals", ContainerSeals, wrapperList.SelectMany(x => x.Seals).Select(s => s.SealNumber).ToArray());
			});
		}

		public void TestSealCodesOnlyContainersWithSecondSeals()
		{
			declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine.JI_CL = entryLine.PK;

			var wrapperList = AESCommonTransportEquipmentWrapper.GetTransportEquipmentList(entryHeader);

			CombineAssertions(() =>
			{
				AssertEquals("Expected empty", 0, wrapperList.Count);

				foreach (var seal in SecondContainerSeals)
				{
					var package = declaration.Packages.AddNew();
					var ctnNumber = "CTN1" + seal;
					var container = CreateContainer(declaration, ctnNumber, secondSeal: seal);
					package.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
					invoiceLine.ContainersPivot.AddNew().C2_CO = container.PK;
					invoiceLine.PackagesPivot.AddPivotFor(package);
				}
				wrapperList = AESCommonTransportEquipmentWrapper.GetTransportEquipmentList(entryHeader);

				AssertContainsExactElementsInAnyOrder("Expected filled Seals", SecondContainerSeals, wrapperList.SelectMany(x => x.Seals).Select(s => s.SealNumber).ToArray());
			});
		}

		public void TestSealCodesOnlyContainersWithAdditionalSeals()
		{
			declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine.JI_CL = entryLine.PK;

			var wrapperList = AESCommonTransportEquipmentWrapper.GetTransportEquipmentList(entryHeader);

			CombineAssertions(() =>
			{
				AssertEquals("Expected empty", 0, wrapperList.Count);

				foreach (var seal in SecondContainerSeals)
				{
					var package = declaration.Packages.AddNew();
					var ctnNumber = "CTN1" + seal;
					var container = CreateContainer(declaration, ctnNumber, additSeal1: seal);
					package.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
					invoiceLine.ContainersPivot.AddNew().C2_CO = container.PK;
					invoiceLine.PackagesPivot.AddPivotFor(package);
				}
				wrapperList = AESCommonTransportEquipmentWrapper.GetTransportEquipmentList(entryHeader);

				AssertContainsExactElementsInAnyOrder("Expected filled Seals", SecondContainerSeals, wrapperList.SelectMany(x => x.Seals).Select(s => s.SealNumber).ToArray());
			});
		}

		public void TestSealCodesOnlyEquipmentsWithSeals()
		{
			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				declaration.CustomsEntryInstructions.AddNew();
				var invoiceHeader = declaration.Invoices.AddNew();
				var packingGroups = declaration.Bills.AddNew().PackingGroups.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var entryLine = entryHeader.MergedLines.AddNew();
				entryLine.CL_LineNumber = 1;
				invoiceLine.JI_CL = entryLine.PK;

				var wrapperList = AESCommonTransportEquipmentWrapper.GetTransportEquipmentList(entryHeader);

				CombineAssertions(() =>
				{
					AssertEquals("Expected empty", 0, wrapperList.Count);

					foreach (var seal in ContainerSeals)
					{
						var equNumber = "EQUIP1" + seal;
						var equipment = CreateEquipment(declaration, equNumber, seal1: seal);
						var pack1 = packingGroups.Packages.AddNew();
						pack1.CW_PackQty = 1;
						pack1.CW_ContainerNoOrEquipmentNo = equipment.CEQ_IdentificationNumber;
						invoiceLine.PackagesPivot.AddPivotFor(pack1);
					}
					wrapperList = AESCommonTransportEquipmentWrapper.GetTransportEquipmentList(entryHeader);

					AssertContainsExactElementsInAnyOrder("Expected filled Seals", ContainerSeals, wrapperList.SelectMany(x => x.Seals).Select(s => s.SealNumber).ToArray());
				});
			}
		}

		public void TestGetTransportEquipmentList_NoContainers()
		{
			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				var declaration = Factory.New<JobDeclaration>();

				var packingGroups = declaration.Bills.AddNew().PackingGroups.AddNew();

				var pack1 = packingGroups.Packages.AddNew();
				var pack2 = packingGroups.Packages.AddNew();
				var pack3 = packingGroups.Packages.AddNew();

				var entryHeader = declaration.CustomsEntryHeaders.AddNew();

				var entryLineEquipment = entryHeader.AllEntryLines.AddNew();

				var equipment1 = CreateEquipment(declaration, "EQUIP1", seal1: "SEAL1", seal2: "SEAL2");
				var equipment2 = CreateEquipment(declaration, "EQUIP2", seal1: "SEAL3", seal2: "SEAL2");
				var equipment3 = CreateEquipment(declaration, "EQUIP3", seal1: "SEAL4", seal2: "SEAL4");

				pack1.CW_PackQty = 1;
				pack1.CW_ContainerNoOrEquipmentNo = equipment1.CEQ_IdentificationNumber;

				pack2.CW_PackQty = 1;
				pack2.CW_ContainerNoOrEquipmentNo = equipment2.CEQ_IdentificationNumber;

				pack3.CW_PackQty = 1;
				pack3.CW_ContainerNoOrEquipmentNo = equipment3.CEQ_IdentificationNumber;

				var invoiceLine1 = entryLineEquipment.InvoiceLines.AddNew();
				invoiceLine1.PackagesPivot.AddPivotFor(pack1);

				var invoiceLine2 = entryLineEquipment.InvoiceLines.AddNew();
				invoiceLine2.PackagesPivot.AddPivotFor(pack2);
				invoiceLine2.PackagesPivot.AddPivotFor(pack3);

				var wrapperList = AESCommonTransportEquipmentWrapper.GetTransportEquipmentList(entryHeader).ToList();

				TestLineWrapper(wrapperList[0], "1", ZString.Empty, "2", new ZString[] { "SEAL1", "SEAL2" }, "E");
				TestLineWrapper(wrapperList[1], "2", ZString.Empty, "2", new ZString[] { "SEAL2", "SEAL3" }, "E");
				TestLineWrapper(wrapperList[2], "3", ZString.Empty, "1", new ZString[] { "SEAL4" }, "E");
			}
		}

		public void TestGetTransportEquipmentList_NoEquipment()
		{
			var declaration = Factory.New<JobDeclaration>();

			var package1 = declaration.Packages.AddNew();
			var package2 = declaration.Packages.AddNew();
			var package3 = declaration.Packages.AddNew();
			var package4 = declaration.Packages.AddNew();

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var entryLineContainer = entryHeader.MergedLines.AddNew();
			entryLineContainer.CL_LineNumber = 1;

			var container1 = CreateContainer(declaration, "CONT1", seal: "SEAL1", secondSeal: "SEAL2", additSeal1: "ADDSEAL1", additSeal2: "ADDSEAL2");
			var container2 = CreateContainer(declaration, "CONT2", seal: "SEAL3", secondSeal: "SEAL2", additSeal1: "ADDSEAL3", additSeal2: "ADDSEAL2");
			var container3 = CreateContainer(declaration, "CONT3", seal: "SEAL4", secondSeal: "SEAL4");
			var container4 = CreateContainer(declaration, "CONT4");

			package1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
			package2.CW_ContainerNoOrEquipmentNo = container2.CO_ContainerNumber;
			package3.CW_ContainerNoOrEquipmentNo = container3.CO_ContainerNumber;
			package4.CW_ContainerNoOrEquipmentNo = container4.CO_ContainerNumber;

			var invoiceLine1 = entryLineContainer.InvoiceLines.AddNew();
			invoiceLine1.ContainersPivot.AddNew().C2_CO = container1.PK;
			invoiceLine1.PackagesPivot.AddPivotFor(package1);

			var invoiceLine2 = entryLineContainer.InvoiceLines.AddNew();
			invoiceLine2.ContainersPivot.AddNew().C2_CO = container2.PK;
			invoiceLine2.ContainersPivot.AddNew().C2_CO = container3.PK;
			invoiceLine2.ContainersPivot.AddNew().C2_CO = container4.PK;
			invoiceLine2.PackagesPivot.AddPivotFor(package2);
			invoiceLine2.PackagesPivot.AddPivotFor(package3);
			invoiceLine2.PackagesPivot.AddPivotFor(package4);

			package1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
			invoiceLine1.PackagesPivot.AddPivotFor(package1);

			var wrapperList = AESCommonTransportEquipmentWrapper.GetTransportEquipmentList(entryHeader).ToList();

			TestLineWrapper(wrapperList[0], "1", "CONT1", "4", new ZString[] { "SEAL1", "SEAL2", "ADDSEAL1", "ADDSEAL2" });
			TestLineWrapper(wrapperList[1], "2", "CONT2", "4", new ZString[] { "SEAL3", "SEAL2", "ADDSEAL2", "ADDSEAL3" });
			TestLineWrapper(wrapperList[2], "3", "CONT3", "1", new ZString[] { "SEAL4" });
		}

		void TestLineWrapper(AESCommonTransportEquipmentWrapper wrapper, string sequence, string container, string numberOfSeals, ZString[] sealsExpected, string typeOfTest = "C")
		{
			CombineAssertions(() =>
			{
				AssertEquals("Sequence " + sequence + " expected filled SequenceNumber", sequence, wrapper.SequenceNumber);
				AssertEquals("Sequence " + sequence + " expected filled ContainerNumber", container, wrapper.ContainerNumber);
				AssertEquals("Sequence " + sequence + " expected filled NumberOfSeals", numberOfSeals, wrapper.NumberOfSeals);

				var seals = wrapper.Seals.ToList();
				var line = 0;
				AssertEquals("Sequence " + sequence + " expected filled Seals with count", numberOfSeals, seals.Count.ToString());
				for (int i = 1; i <= sealsExpected.Length; i++)
				{
					line = i - 1;
					AssertEquals("Sequence " + sequence + " expected filled Seals SequenceNumber, line " + line, i.ToString(), seals[line].SequenceNumber);
					AssertEquals("Sequence " + sequence + " expected filled Seals SealNumber, line " + line, sealsExpected[line], seals[line].SealNumber);
				}

				var goodsreference = wrapper.GoodsReference.ToList();
				AssertEquals("Sequence " + sequence + " expected filled GoodsReference with count 1", 1, goodsreference.Count);
				for (int i = 1; i <= goodsreference.Count; i++)
				{
					line = i - 1;
					AssertEquals("Sequence " + sequence + " expected filled GoodsReference SequenceNumber " + line, "1", goodsreference[line].SequenceNumber);
					if (typeOfTest == "C")
					{
						AssertEquals("Sequence " + sequence + " expected filled GoodsReference GoodsItemNumber " + line, "1", goodsreference[line].GoodsItemNumber);
					}
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				declaration = Factory.New<JobDeclaration>();
				declaration.JE_ContainerMode = "ULD";
				var package1 = declaration.Packages.AddNew();
				var package2 = declaration.Packages.AddNew();
				var package3 = declaration.Packages.AddNew();
				var package4 = declaration.Packages.AddNew();
				var package5 = declaration.Packages.AddNew();
				var package6 = declaration.Packages.AddNew();
				var package7 = declaration.Packages.AddNew();

				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var entryLine1 = entryHeader.MergedLines.AddNew();
				var entryLine2 = entryHeader.MergedLines.AddNew();
				entryLine1.CL_LineNumber = 1;
				entryLine2.CL_LineNumber = 2;

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_CL = entryLine1.PK;
				var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_CL = entryLine2.PK;

				var container1 = CreateContainer(declaration, "CONT1", seal: "seal11");
				var container2 = CreateContainer(declaration, "CONT2", secondSeal: "seal22", additSeal1: "SEALR");
				var container3 = CreateContainer(declaration, "CONT3", additSeal1: "SEAL31", additSeal2: "SEAL32");
				var container4 = CreateContainer(declaration, "CONT4");

				var equipment1 = CreateEquipment(declaration, "EQUIPMENT1", seal1: "SEALE1", seal2: "SEALE2", seal3: "SEALE3");
				var equipment2 = CreateEquipment(declaration, "EQUIPMENT2", seal1: "SEALE4", seal2: "SEALE5", seal3: "SEALR");
				var equipment3 = CreateEquipment(declaration, "EQUIPMENT3");

				package1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
				package2.CW_ContainerNoOrEquipmentNo = container2.CO_ContainerNumber;
				package3.CW_ContainerNoOrEquipmentNo = container3.CO_ContainerNumber;
				package4.CW_ContainerNoOrEquipmentNo = container4.CO_ContainerNumber;
				package5.CW_ContainerNoOrEquipmentNo = equipment1.CEQ_IdentificationNumber;
				package6.CW_ContainerNoOrEquipmentNo = equipment2.CEQ_IdentificationNumber;
				package7.CW_ContainerNoOrEquipmentNo = equipment3.CEQ_IdentificationNumber;

				invoiceLine1.PackagesPivot.AddPivotFor(package1);
				invoiceLine1.PackagesPivot.AddPivotFor(package3);
				invoiceLine1.PackagesPivot.AddPivotFor(package4);
				invoiceLine1.PackagesPivot.AddPivotFor(package5);
				invoiceLine1.PackagesPivot.AddPivotFor(package6);

				invoiceLine2.PackagesPivot.AddPivotFor(package2);
				invoiceLine2.PackagesPivot.AddPivotFor(package3);
				invoiceLine2.PackagesPivot.AddPivotFor(package6);
				invoiceLine2.PackagesPivot.AddPivotFor(package7);

				var wrapperList = AESCommonTransportEquipmentWrapper.GetTransportEquipmentList(entryHeader).ToList();
				wrapper1 = wrapperList[0];
				wrapper2 = wrapperList[1];
				wrapper3 = wrapperList[2];
				wrapper4 = wrapperList[3];
				wrapper5 = wrapperList[4];
				wrapper6 = wrapperList[5];
				wrapper7 = wrapperList[6];
			}
		}

		JobDeclaration declaration;
		AESCommonTransportEquipmentWrapper wrapper1;
		AESCommonTransportEquipmentWrapper wrapper2;
		AESCommonTransportEquipmentWrapper wrapper3;
		AESCommonTransportEquipmentWrapper wrapper4;
		AESCommonTransportEquipmentWrapper wrapper5;
		AESCommonTransportEquipmentWrapper wrapper6;
		AESCommonTransportEquipmentWrapper wrapper7;

		CusContainer CreateContainer(JobDeclaration declaration, string containerNumber, string seal = null, string secondSeal = null, string additSeal1 = null, string additSeal2 = null)
		{
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = containerNumber;
			container.CO_Seal = seal;
			container.CO_SecondSeal = secondSeal;
			if (additSeal1 != null)
			{
				container.AdditionalSeals.AddNew().BK_SealNumber = additSeal1;
			}
			if (additSeal2 != null)
			{
				container.AdditionalSeals.AddNew().BK_SealNumber = additSeal2;
			}

			return container;
		}

		CusEquipment CreateEquipment(JobDeclaration declaration, string equipmentNumber, string seal1 = null, string seal2 = null, string seal3 = null)
		{
			var equipment = declaration.Equipments.AddNew();
			equipment.CEQ_IdentificationNumber = equipmentNumber;
			if (seal1 != null)
			{
				equipment.Seals.AddNew().BK_SealNumber = seal1;
			}
			if (seal2 != null)
			{
				equipment.Seals.AddNew().BK_SealNumber = seal2;
			}
			if (seal3 != null)
			{
				equipment.Seals.AddNew().BK_SealNumber = seal3;
			}

			return equipment;
		}

		protected override AESCommonTransportEquipmentWrapper GetProvider() => wrapper1;
	}
}
