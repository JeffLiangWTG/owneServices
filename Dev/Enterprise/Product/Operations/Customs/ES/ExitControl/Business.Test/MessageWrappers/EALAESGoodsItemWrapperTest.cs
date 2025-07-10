using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	class EALAESGoodsItemWrapperTest : WrapperHelperTest<EALAESGoodsItemWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if consignmentItem is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "consignmentItem"), () => GetWrapper(null, 0m, 0m, Enumerable.Empty<(ZShort seqNum, ZString packageType, ZString packageQuantity, ZString shippingMarks)>(), Enumerable.Empty<AdditionalInfo>()));
			});
		}

		public void TestSequenceNumber()
		{
			AssertEquals("Expected filled SequenceNumber", "3", wrapper.SequenceNumber);
		}

		public void TestReferenceNumberUCR()
		{
			CombineAssertions(() =>
			{
				exitConsignmentItem.CCI_UniqueConsignmentReference = "reference";

				exitConsignmentItem.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.Missing;
				exitConsignmentItem.CCI_UniqueConsignmentReferenceStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
				wrapper = GetWrapper(exitConsignmentItem, 0m, 0m, Enumerable.Empty<(ZShort seqNum, ZString packageType, ZString packageQuantity, ZString shippingMarks)>(), Enumerable.Empty<AdditionalInfo>());
				AssertEquals("Expected empty ReferenceNumberUCR when CCI_DiscrepancyStatus is MIS (even when CCI_UniqueConsignmentReferenceStatus is DIF)", ZString.Empty, wrapper.ReferenceNumberUCR);

				exitConsignmentItem.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
				exitConsignmentItem.CCI_UniqueConsignmentReferenceStatus = ZString.Empty;
				wrapper = GetWrapper(exitConsignmentItem, 0m, 0m, Enumerable.Empty<(ZShort seqNum, ZString packageType, ZString packageQuantity, ZString shippingMarks)>(), Enumerable.Empty<AdditionalInfo>());
				AssertEquals("Expected empty ReferenceNumberUCR when CCI_DiscrepancyStatus is not MIS but CCI_UniqueConsignmentReferenceStatus is empty", ZString.Empty, wrapper.ReferenceNumberUCR);

				exitConsignmentItem.CCI_UniqueConsignmentReferenceStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
				wrapper = GetWrapper(exitConsignmentItem, 0m, 0m, Enumerable.Empty<(ZShort seqNum, ZString packageType, ZString packageQuantity, ZString shippingMarks)>(), Enumerable.Empty<AdditionalInfo>());
				AssertEquals("Expected filled ReferenceNumberUCR when CCI_DiscrepancyStatus is not MIS and CCI_UniqueConsignmentReferenceStatus DIF", "reference", wrapper.ReferenceNumberUCR);

				exitConsignmentItem.CCI_UniqueConsignmentReferenceStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.Missing;
				wrapper = GetWrapper(exitConsignmentItem, 0m, 0m, Enumerable.Empty<(ZShort seqNum, ZString packageType, ZString packageQuantity, ZString shippingMarks)>(), Enumerable.Empty<AdditionalInfo>());
				AssertEquals("Expected empty ReferenceNumberUCR when CCI_DiscrepancyStatus is not MIS and CCI_UniqueConsignmentReferenceStatus MIS", ZString.Empty, wrapper.ReferenceNumberUCR);
			});
		}

		public void TestCommodity()
		{
			CombineAssertions(() =>
			{
				AssertNull("Expected null Commodity when CCI_DiscrepancyStatus is empty", wrapper.Commodity);

				exitConsignmentItem.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
				wrapper = GetWrapper(exitConsignmentItem, 20m, 0m, Enumerable.Empty<(ZShort seqNum, ZString packageType, ZString packageQuantity, ZString shippingMarks)>(), Enumerable.Empty<AdditionalInfo>());
				var commodity = wrapper.Commodity;
				AssertNotNull("Expected filled Commodity when CCI_DiscrepancyStatus is DIF", commodity);
				AssertSame("Cached Commodity", wrapper.Commodity, commodity);

				exitConsignmentItem.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.Missing;
				wrapper = GetWrapper(exitConsignmentItem, 20m, 0m, Enumerable.Empty<(ZShort seqNum, ZString packageType, ZString packageQuantity, ZString shippingMarks)>(), Enumerable.Empty<AdditionalInfo>());
				AssertNull("Expected null Commodity when CCI_DiscrepancyStatus is MIS", wrapper.Commodity);
			});
		}

		public void TestPackaging()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty Packaging when CCI_DiscrepancyStatus is not MIS but no packages are declared", 0, wrapper.Packaging.Count);

				var packages = new[]
				{
					((ZShort)1, new ZString("P1"), new ZString("50"), new ZString("MARK1")),
					((ZShort)3, new ZString("P2"), new ZString("60"), new ZString("MARK2")),
					((ZShort)4, new ZString("P3"), ZString.Empty, new ZString("MARK3"))
				};

				wrapper = GetWrapper(exitConsignmentItem, 20m, 0m, packages, Enumerable.Empty<AdditionalInfo>());
				var packaging = wrapper.Packaging;
				AssertEquals("Expected filled Packaging when CCI_DiscrepancyStatus is not MIS (empty)", 3, packaging.Count);
				AssertSame("Cached Packaging", wrapper.Packaging, packaging);

				exitConsignmentItem.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.Missing;
				wrapper = GetWrapper(exitConsignmentItem, 20m, 0m, packages, Enumerable.Empty<AdditionalInfo>());
				AssertEquals("Expected empty Packaging when CCI_DiscrepancyStatus is MIS, even when there are packages declared", 0, wrapper.Packaging.Count);

				exitConsignmentItem.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
				wrapper = GetWrapper(exitConsignmentItem, 20m, 0m, packages, Enumerable.Empty<AdditionalInfo>());
				AssertEquals("Expected filled Packaging when CCI_DiscrepancyStatus is not MIS (DIF)", 3, wrapper.Packaging.Count);
			});
		}

		public void TestTransportDocuments()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty TransportDocuments when CCI_DiscrepancyStatus is not MIS but no documents are declared", 0, wrapper.TransportDocuments.Count);

				var documents = new[]
				{
					Factory.NewWithValidTestData<AdditionalInfo>(),
					Factory.NewWithValidTestData<AdditionalInfo>(),
					Factory.NewWithValidTestData<AdditionalInfo>()
				};

				wrapper = GetWrapper(exitConsignmentItem, 20m, 0m, Enumerable.Empty<(ZShort seqNum, ZString packageType, ZString packageQuantity, ZString shippingMarks)>(), documents);
				var transportDocuments = wrapper.TransportDocuments;
				AssertEquals("Expected filled TransportDocuments when CCI_DiscrepancyStatus is not MIS (empty)", 3, transportDocuments.Count);
				AssertSame("Cached TransportDocuments", wrapper.TransportDocuments, transportDocuments);

				exitConsignmentItem.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.Missing;
				wrapper = GetWrapper(exitConsignmentItem, 20m, 0m, Enumerable.Empty<(ZShort seqNum, ZString packageType, ZString packageQuantity, ZString shippingMarks)>(), documents);
				AssertEquals("Expected empty TransportDocuments when CCI_DiscrepancyStatus is MIS, even when there are packages declared", 0, wrapper.TransportDocuments.Count);

				exitConsignmentItem.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
				wrapper = GetWrapper(exitConsignmentItem, 20m, 0m, Enumerable.Empty<(ZShort seqNum, ZString packageType, ZString packageQuantity, ZString shippingMarks)>(), documents);
				AssertEquals("Expected filled TransportDocuments when CCI_DiscrepancyStatus is not MIS (DIF)", 3, wrapper.TransportDocuments.Count);
			});
		}

		public void TestGetGoodsItemsList_NoDifferences()
		{
			var exitReportItem = exitReport.CusExitReportItems.AddNew();
			exitReportItem.ERI_CCI_ConsignmentItem = exitConsignmentItem.PK;

			var supdoc = exitReportItem.AdditionalInfos.AddNew();
			supdoc.CSI_ItemNumber = 1;
			supdoc.CSI_Code = "9001";
			supdoc.CSI_ReferenceNumber = "REF1";
			supdoc.CSI_SubType = "TRA";

			var goodsItems = EALAESGoodsItemWrapper.GetGoodsItemsList(exitReport);
			AssertEquals("Expected empty goodsItems", 0, goodsItems.Count);
		}

		public void TestGetGoodsItemsList_UniqueConsignmentReferenceDIFOrMIS()
		{
			exitConsignmentItem.CCI_UniqueConsignmentReference = "reference1";
			exitConsignmentItem.CCI_UniqueConsignmentReferenceStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
			exitConsignmentItem.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
			var exitConsignmentItem2 = exitConsignment.CusExitConsignmentItems.AddNew();
			exitConsignmentItem2.CCI_LineNumber = 4;
			exitConsignmentItem2.CCI_UniqueConsignmentReference = "reference2";
			exitConsignmentItem2.CCI_UniqueConsignmentReferenceStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.Missing;
			exitConsignmentItem2.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
			var exitConsignmentItem3 = exitConsignment.CusExitConsignmentItems.AddNew();
			exitConsignmentItem3.CCI_LineNumber = 5;
			exitConsignmentItem3.CCI_UniqueConsignmentReference = "reference3";
			exitConsignmentItem3.CCI_UniqueConsignmentReferenceStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
			exitConsignmentItem3.CCI_DiscrepancyStatus = ZString.Empty;
			var exitConsignmentItem4 = exitConsignment.CusExitConsignmentItems.AddNew();
			exitConsignmentItem4.CCI_LineNumber = 6;
			exitConsignmentItem4.CCI_UniqueConsignmentReference = "reference4";
			exitConsignmentItem4.CCI_UniqueConsignmentReferenceStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
			exitConsignmentItem4.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.Missing;
			var exitConsignmentItem5 = exitConsignment.CusExitConsignmentItems.AddNew();
			exitConsignmentItem5.CCI_LineNumber = 7;
			exitConsignmentItem5.CCI_UniqueConsignmentReference = "reference5";
			exitConsignmentItem5.CCI_UniqueConsignmentReferenceStatus = ZString.Empty;
			exitConsignmentItem5.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;

			var exitReportItem1 = exitReport.CusExitReportItems.AddNew();
			exitReportItem1.ERI_CCI_ConsignmentItem = exitConsignmentItem.PK;

			var exitReportItem2 = exitReport.CusExitReportItems.AddNew();
			exitReportItem2.ERI_CCI_ConsignmentItem = exitConsignmentItem.PK;

			var exitReportItem3 = exitReport.CusExitReportItems.AddNew();
			exitReportItem3.ERI_CCI_ConsignmentItem = exitConsignmentItem.PK;

			var exitReportItem4 = exitReport.CusExitReportItems.AddNew();
			exitReportItem4.ERI_CCI_ConsignmentItem = exitConsignmentItem2.PK;

			var exitReportItem5 = exitReport.CusExitReportItems.AddNew();
			exitReportItem5.ERI_CCI_ConsignmentItem = exitConsignmentItem2.PK;

			var exitReportItem6 = exitReport.CusExitReportItems.AddNew();
			exitReportItem6.ERI_CCI_ConsignmentItem = exitConsignmentItem5.PK;

			CombineAssertions(() =>
			{
				var goodsItems = EALAESGoodsItemWrapper.GetGoodsItemsList(exitReport);
				AssertEquals("Expected filled goodsItems", 5, goodsItems.Count);

				AssertEquals("For first goodsItem expected filled ReferenceNumberUCR when CCI_DiscrepancyStatus is DIF and CCI_UniqueConsignmentReferenceStatus is DIF", "reference1", goodsItems[0].ReferenceNumberUCR);
				AssertEquals("For second goodsItem expected empty ReferenceNumberUCR when CCI_DiscrepancyStatus is DIF and CCI_UniqueConsignmentReferenceStatus is MIS", ZString.Empty, goodsItems[1].ReferenceNumberUCR);
				AssertEquals("For third goodsItem expected empty ReferenceNumberUCR when CCI_DiscrepancyStatus is DIF and CCI_UniqueConsignmentReferenceStatus is DIF", "reference3", goodsItems[2].ReferenceNumberUCR);
				AssertEquals("For fourth goodsItem expected empty ReferenceNumberUCR when CCI_DiscrepancyStatus is MIS", ZString.Empty, goodsItems[3].ReferenceNumberUCR);
				AssertEquals("For fifth goodsItem expected empty ReferenceNumberUCR when CCI_DiscrepancyStatus is empty and CCI_UniqueConsignmentReferenceStatus is empty", ZString.Empty, goodsItems[4].ReferenceNumberUCR);
			});
		}

		public void TestGetGoodsItemsList_DifferentMasses()
		{
			exitConsignmentItem.CCI_GrossMass = 100m;
			exitConsignmentItem.CCI_NetMass = 50m;
			exitConsignmentItem.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
			var exitConsignmentItem2 = exitConsignment.CusExitConsignmentItems.AddNew();
			exitConsignmentItem2.CCI_LineNumber = 4;
			exitConsignmentItem2.CCI_GrossMass = 500m;
			exitConsignmentItem2.CCI_NetMass = 200m;
			exitConsignmentItem2.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
			var exitConsignmentItem3 = exitConsignment.CusExitConsignmentItems.AddNew();
			exitConsignmentItem3.CCI_LineNumber = 5;
			exitConsignmentItem3.CCI_GrossMass = 600m;
			exitConsignmentItem3.CCI_NetMass = 400m;
			exitConsignmentItem3.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
			var exitConsignmentItem4 = exitConsignment.CusExitConsignmentItems.AddNew();
			exitConsignmentItem4.CCI_LineNumber = 6;
			exitConsignmentItem4.CCI_GrossMass = 700m;
			exitConsignmentItem4.CCI_NetMass = 300m;
			exitConsignmentItem4.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.Missing;
			var exitConsignmentItem5 = exitConsignment.CusExitConsignmentItems.AddNew();
			exitConsignmentItem5.CCI_LineNumber = 7;
			exitConsignmentItem5.CCI_GrossMass = 200m;
			exitConsignmentItem5.CCI_NetMass = 100m;
			exitConsignmentItem5.CCI_DiscrepancyStatus = ZString.Empty;
			var exitConsignmentItem6 = exitConsignment.CusExitConsignmentItems.AddNew();
			exitConsignmentItem6.CCI_LineNumber = 8;
			exitConsignmentItem6.CCI_GrossMass = 200m;
			exitConsignmentItem6.CCI_NetMass = 100m;
			exitConsignmentItem6.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;

			var exitReportItem1 = exitReport.CusExitReportItems.AddNew();
			exitReportItem1.ERI_CCI_ConsignmentItem = exitConsignmentItem.PK;
			exitReportItem1.ERI_GrossMass = 50m;
			exitReportItem1.ERI_NetMass = 20m;

			var exitReportItem2 = exitReport.CusExitReportItems.AddNew();
			exitReportItem2.ERI_CCI_ConsignmentItem = exitConsignmentItem.PK;
			exitReportItem2.ERI_GrossMass = 50m;
			exitReportItem2.ERI_NetMass = 20m;

			var exitReportItem3 = exitReport.CusExitReportItems.AddNew();
			exitReportItem3.ERI_CCI_ConsignmentItem = exitConsignmentItem.PK;
			exitReportItem3.ERI_GrossMass = 50m;
			exitReportItem3.ERI_NetMass = 20m;

			var exitReportItem4 = exitReport.CusExitReportItems.AddNew();
			exitReportItem4.ERI_CCI_ConsignmentItem = exitConsignmentItem2.PK;
			exitReportItem4.ERI_GrossMass = 500m;
			exitReportItem4.ERI_NetMass = 40m;

			var exitReportItem5 = exitReport.CusExitReportItems.AddNew();
			exitReportItem5.ERI_CCI_ConsignmentItem = exitConsignmentItem2.PK;
			exitReportItem5.ERI_GrossMass = 500m;
			exitReportItem5.ERI_NetMass = 40m;

			var exitReportItem6 = exitReport.CusExitReportItems.AddNew();
			exitReportItem6.ERI_CCI_ConsignmentItem = exitConsignmentItem3.PK;
			exitReportItem6.ERI_GrossMass = 700m;
			exitReportItem6.ERI_NetMass = 400m;

			var exitReportItem9 = exitReport.CusExitReportItems.AddNew();
			exitReportItem9.ERI_CCI_ConsignmentItem = exitConsignmentItem6.PK;
			exitReportItem9.ERI_GrossMass = 0m;
			exitReportItem9.ERI_NetMass = 0m;

			CombineAssertions(() =>
			{
				var goodsItems = EALAESGoodsItemWrapper.GetGoodsItemsList(exitReport);
				AssertEquals("Expected filled goodsItems", 5, goodsItems.Count);

				AssertEquals("For first goodsItem expected filled SequenceNumber with the correct value", "3", goodsItems[0].SequenceNumber);
				AssertEquals("For first goodsItem expected filled Commodity.GoodsMeasure, having GrossWeightSpecified true when grossMass is different in reportItem", true, goodsItems[0].Commodity.GoodsMeasure.GrossWeightSpecified);
				AssertEquals("For first goodsItem expected filled Commodity.GoodsMeasure, having NetWeightSpecified true when netMass is different in reportItem", true, goodsItems[0].Commodity.GoodsMeasure.NetWeightSpecified);

				AssertEquals("For second goodsItem expected filled SequenceNumber with the correct value", "4", goodsItems[1].SequenceNumber);
				AssertEquals("For second goodsItem expected filled Commodity.GoodsMeasure, having GrossWeightSpecified false when grossMass is the same in reportItem", false, goodsItems[1].Commodity.GoodsMeasure.GrossWeightSpecified);
				AssertEquals("For second goodsItem expected filled Commodity.GoodsMeasure, having NetWeightSpecified true when netMass is different in reportItem", true, goodsItems[1].Commodity.GoodsMeasure.NetWeightSpecified);

				AssertEquals("For third goodsItem expected filled SequenceNumber with the correct value", "5", goodsItems[2].SequenceNumber);
				AssertEquals("For third goodsItem expected filled Commodity.GoodsMeasure, having GrossWeightSpecified true when grossMass is different in reportItem", true, goodsItems[2].Commodity.GoodsMeasure.GrossWeightSpecified);
				AssertEquals("For third goodsItem expected filled Commodity.GoodsMeasure, having NetWeightSpecified false when netMass is the same in reportItem", false, goodsItems[2].Commodity.GoodsMeasure.NetWeightSpecified);

				AssertEquals("For fourth goodsItem expected filled SequenceNumber with the correct value", "6", goodsItems[3].SequenceNumber);
				AssertNull("For fourth goodsItem expected null Commodity when CCI_DiscrepancyStatus is MIS", goodsItems[3].Commodity);

				AssertEquals("For third goodsItem expected filled SequenceNumber with the correct value", "8", goodsItems[4].SequenceNumber);
				AssertEquals("For third goodsItem expected filled Commodity.GoodsMeasure, having GrossWeightSpecified false when grossMass is 0", false, goodsItems[4].Commodity.GoodsMeasure.GrossWeightSpecified);
				AssertEquals("For third goodsItem expected filled Commodity.GoodsMeasure, having NetWeightSpecified false when netMass is 0", false, goodsItems[4].Commodity.GoodsMeasure.NetWeightSpecified);
			});
		}

		public void TestGetGoodsItemsList_PackagesDIFOrMIS()
		{
			exitConsignmentItem.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
			var exitConsignmentItem2 = exitConsignment.CusExitConsignmentItems.AddNew();
			exitConsignmentItem2.CCI_LineNumber = 4;
			exitConsignmentItem2.CCI_DiscrepancyStatus = ZString.Empty;
			var exitConsignmentItem3 = exitConsignment.CusExitConsignmentItems.AddNew();
			exitConsignmentItem3.CCI_LineNumber = 5;
			exitConsignmentItem3.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
			var exitConsignmentItem4 = exitConsignment.CusExitConsignmentItems.AddNew();
			exitConsignmentItem4.CCI_LineNumber = 6;
			exitConsignmentItem4.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.Missing;
			var exitConsignmentItem5 = exitConsignment.CusExitConsignmentItems.AddNew();
			exitConsignmentItem5.CCI_LineNumber = 7;
			exitConsignmentItem5.CCI_DiscrepancyStatus = ZString.Empty;

			var consignmentPackagePivot1 = exitConsignmentItem.CusExitConsignmentPackagePivots.AddNew();
			var package1 = consignmentPackagePivot1.Package;
			package1.CXP_Sequence = 1;
			package1.CXP_Quantity = 3;
			package1.CXP_PackageType = "gg";
			package1.CXP_MarksAndNumbers = "marks1";
			package1.CXP_MarksAndNumbersStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
			var consignmentPackagePivot2 = exitConsignmentItem.CusExitConsignmentPackagePivots.AddNew();
			var package2 = consignmentPackagePivot2.Package;
			package2.CXP_Sequence = 3;
			package2.CXP_Quantity = 9;
			package2.CXP_PackageType = "ff";
			package2.CXP_MarksAndNumbers = "marks2";
			package2.CXP_MarksAndNumbersStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
			var consignmentPackagePivot3 = exitConsignmentItem.CusExitConsignmentPackagePivots.AddNew();
			var package3 = consignmentPackagePivot3.Package;
			package3.CXP_Sequence = 4;
			package3.CXP_Quantity = 12;
			package3.CXP_PackageType = "ee";
			package3.CXP_MarksAndNumbers = "marks3";
			package3.CXP_MarksAndNumbersStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.Missing;

			var consignmentPackagePivot4 = exitConsignmentItem2.CusExitConsignmentPackagePivots.AddNew();
			var package4 = consignmentPackagePivot4.Package;
			package4.CXP_Sequence = 5;
			package4.CXP_Quantity = 0;
			package4.CXP_PackageType = "dd";
			package4.CXP_MarksAndNumbers = "marks4";
			package4.CXP_MarksAndNumbersStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
			var consignmentPackagePivot5 = exitConsignmentItem2.CusExitConsignmentPackagePivots.AddNew();
			var package5 = consignmentPackagePivot5.Package;
			package5.CXP_Sequence = 6;
			package5.CXP_Quantity = 10;
			package5.CXP_PackageType = "cc";
			package5.CXP_MarksAndNumbers = "marks5";
			package5.CXP_MarksAndNumbersStatus = ZString.Empty;

			var consignmentPackagePivot6 = exitConsignmentItem3.CusExitConsignmentPackagePivots.AddNew();
			var package6 = consignmentPackagePivot6.Package;
			package6.CXP_Sequence = 7;
			package6.CXP_Quantity = 10;
			package6.CXP_PackageType = "bb";
			package6.CXP_MarksAndNumbers = "marks6";
			package6.CXP_MarksAndNumbersStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;

			var consignmentPackagePivot7 = exitConsignmentItem4.CusExitConsignmentPackagePivots.AddNew();
			var package7 = consignmentPackagePivot7.Package;
			package7.CXP_Sequence = 8;
			package7.CXP_Quantity = 10;
			package7.CXP_PackageType = "aa";
			package7.CXP_MarksAndNumbers = "marks7";
			package7.CXP_MarksAndNumbersStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;

			var consignmentPackagePivot8 = exitConsignmentItem5.CusExitConsignmentPackagePivots.AddNew();
			var package8 = consignmentPackagePivot8.Package;
			package8.CXP_Sequence = 9;
			package8.CXP_Quantity = 20;
			package8.CXP_PackageType = "hh";
			package8.CXP_MarksAndNumbers = "marks8";

			var exitReportItem1 = exitReport.CusExitReportItems.AddNew();
			exitReportItem1.ERI_CCI_ConsignmentItem = exitConsignmentItem.PK;
			exitReportItem1.ERI_CXP_Package = package1.PK;
			exitReportItem1.ERI_Quantity = 20;

			var exitReportItem2 = exitReport.CusExitReportItems.AddNew();
			exitReportItem2.ERI_CCI_ConsignmentItem = exitConsignmentItem.PK;
			exitReportItem2.ERI_CXP_Package = package2.PK;
			exitReportItem2.ERI_Quantity = 9;

			var exitReportItem3 = exitReport.CusExitReportItems.AddNew();
			exitReportItem3.ERI_CCI_ConsignmentItem = exitConsignmentItem.PK;
			exitReportItem3.ERI_CXP_Package = package3.PK;
			exitReportItem3.ERI_Quantity = 0;

			var exitReportItem4 = exitReport.CusExitReportItems.AddNew();
			exitReportItem4.ERI_CCI_ConsignmentItem = exitConsignmentItem2.PK;
			exitReportItem4.ERI_CXP_Package = package4.PK;
			exitReportItem4.ERI_Quantity = 5;

			var exitReportItem5 = exitReport.CusExitReportItems.AddNew();
			exitReportItem5.ERI_CCI_ConsignmentItem = exitConsignmentItem2.PK;
			exitReportItem5.ERI_CXP_Package = package5.PK;
			exitReportItem5.ERI_Quantity = 0;

			var exitReportItem6 = exitReport.CusExitReportItems.AddNew();
			exitReportItem6.ERI_CCI_ConsignmentItem = exitConsignmentItem3.PK;
			exitReportItem6.ERI_CXP_Package = package6.PK;
			exitReportItem6.ERI_Quantity = 0;

			CombineAssertions(() =>
			{
				var goodsItems = EALAESGoodsItemWrapper.GetGoodsItemsList(exitReport);
				AssertEquals("Expected filled goodsItems", 4, goodsItems.Count);

				AssertContainsExactElementsInExactOrder("For first goodsItem expected filled Packaging with correct data, SequenceNumber", new ZString[] { "1", "3", "4" }, goodsItems[0].Packaging.Select(x => x.SequenceNumber));
				AssertContainsExactElementsInExactOrder("For first goodsItem expected filled Packaging with correct data, PackageType", new ZString[] { "gg", "ff", "" }, goodsItems[0].Packaging.Select(x => x.PackageType));
				AssertContainsExactElementsInExactOrder("For first goodsItem expected filled Packaging with correct data, NumberOfPackages", new ZString[] { "20", "", "" }, goodsItems[0].Packaging.Select(x => x.NumberOfPackages));
				AssertContainsExactElementsInExactOrder("For first goodsItem expected filled Packaging with correct data, Marks", new ZString[] { "marks1", "marks2", "" }, goodsItems[0].Packaging.Select(x => x.Marks));

				AssertContainsExactElementsInExactOrder("For second goodsItem expected filled Packaging with correct data, SequenceNumber", new ZString[] { "5" }, goodsItems[1].Packaging.Select(x => x.SequenceNumber));
				AssertContainsExactElementsInExactOrder("For second goodsItem expected filled Packaging with correct data, PackageType", new ZString[] { "dd" }, goodsItems[1].Packaging.Select(x => x.PackageType));
				AssertContainsExactElementsInExactOrder("For second goodsItem expected filled Packaging with correct data, NumberOfPackages", new ZString[] { "5" }, goodsItems[1].Packaging.Select(x => x.NumberOfPackages));
				AssertContainsExactElementsInExactOrder("For second goodsItem expected filled Packaging with correct data, Marks", new ZString[] { "marks4" }, goodsItems[1].Packaging.Select(x => x.Marks));

				AssertContainsExactElementsInExactOrder("For third goodsItem expected filled Packaging with correct data, SequenceNumber", new ZString[] { "7" }, goodsItems[2].Packaging.Select(x => x.SequenceNumber));
				AssertContainsExactElementsInExactOrder("For third goodsItem expected filled Packaging with correct data, PackageType", new ZString[] { "bb" }, goodsItems[2].Packaging.Select(x => x.PackageType));
				AssertContainsExactElementsInExactOrder("For third goodsItem expected filled Packaging with correct data, NumberOfPackages", new ZString[] { "0" }, goodsItems[2].Packaging.Select(x => x.NumberOfPackages));
				AssertContainsExactElementsInExactOrder("For third goodsItem expected filled Packaging with correct data, Marks", new ZString[] { "marks6" }, goodsItems[2].Packaging.Select(x => x.Marks));

				AssertEquals("For fourth goodsItem expected empty Packaging when CCI_DiscrepancyStatus is MIS", 0, goodsItems[3].Packaging.Count);
			});
		}

		public void TestGetGoodsItemsList_DocumentsDIFOrMIS()
		{
			exitConsignmentItem.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
			var exitConsignmentItem2 = exitConsignment.CusExitConsignmentItems.AddNew();
			exitConsignmentItem2.CCI_LineNumber = 4;
			exitConsignmentItem2.CCI_DiscrepancyStatus = ZString.Empty;
			var exitConsignmentItem3 = exitConsignment.CusExitConsignmentItems.AddNew();
			exitConsignmentItem3.CCI_LineNumber = 5;
			exitConsignmentItem3.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
			var exitConsignmentItem4 = exitConsignment.CusExitConsignmentItems.AddNew();
			exitConsignmentItem4.CCI_LineNumber = 6;
			exitConsignmentItem4.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.Missing;
			var exitConsignmentItem5 = exitConsignment.CusExitConsignmentItems.AddNew();
			exitConsignmentItem5.CCI_LineNumber = 7;
			exitConsignmentItem5.CCI_DiscrepancyStatus = ZString.Empty;

			var exitReportItem1 = exitReport.CusExitReportItems.AddNew();
			exitReportItem1.ERI_CCI_ConsignmentItem = exitConsignmentItem.PK;

			var supdoc1 = exitReportItem1.AdditionalInfos.AddNew();
			supdoc1.CSI_ItemNumber = 1;
			supdoc1.CSI_Code = "9001";
			supdoc1.CSI_ReferenceNumber = "REF1";
			supdoc1.CSI_SubType = "TRA";
			supdoc1.CSI_Status = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;

			var supdoc2 = exitReportItem1.AdditionalInfos.AddNew();
			supdoc2.CSI_ItemNumber = 2;
			supdoc2.CSI_Code = "9002";
			supdoc2.CSI_ReferenceNumber = "REF2";
			supdoc2.CSI_SubType = "TRA";

			var supdoc3 = exitReportItem1.AdditionalInfos.AddNew();
			supdoc3.CSI_ItemNumber = 3;
			supdoc3.CSI_Code = "9003";
			supdoc3.CSI_ReferenceNumber = "REF3";
			supdoc3.CSI_SubType = "INF";
			supdoc3.CSI_Status = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;

			var exitReportItem2 = exitReport.CusExitReportItems.AddNew();
			exitReportItem2.ERI_CCI_ConsignmentItem = exitConsignmentItem.PK;

			var supdoc4 = exitConsignmentItem.AdditionalInfos.AddNew();
			supdoc4.CSI_ItemNumber = 4;
			supdoc4.CSI_Code = "9004";
			supdoc4.CSI_ReferenceNumber = "REF4";
			supdoc4.CSI_SubType = "TRA";
			supdoc4.CSI_Status = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.Missing;

			var exitReportItem3 = exitReport.CusExitReportItems.AddNew();
			exitReportItem3.ERI_CCI_ConsignmentItem = exitConsignmentItem.PK;

			var exitReportItem4 = exitReport.CusExitReportItems.AddNew();
			exitReportItem4.ERI_CCI_ConsignmentItem = exitConsignmentItem2.PK;

			var supdoc5 = exitReportItem4.AdditionalInfos.AddNew();
			supdoc5.CSI_ItemNumber = 5;
			supdoc5.CSI_Code = "9005";
			supdoc5.CSI_ReferenceNumber = "REF5";
			supdoc5.CSI_SubType = "TRA";
			supdoc5.CSI_Status = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;

			var supdoc6 = exitReportItem4.AdditionalInfos.AddNew();
			supdoc6.CSI_ItemNumber = 6;
			supdoc6.CSI_Code = "9006";
			supdoc6.CSI_ReferenceNumber = "REF6";
			supdoc6.CSI_SubType = "TRA";

			var supdoc7 = exitConsignmentItem2.AdditionalInfos.AddNew();
			supdoc7.CSI_ItemNumber = 7;
			supdoc7.CSI_Code = "9007";
			supdoc7.CSI_ReferenceNumber = "REF7";
			supdoc7.CSI_SubType = "TRA";
			supdoc7.CSI_Status = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.Missing;

			var supdoc7bis = exitReportItem4.AdditionalInfos.AddNew();
			supdoc7bis.CSI_ItemNumber = 7;
			supdoc7bis.CSI_Code = "9007";
			supdoc7bis.CSI_ReferenceNumber = "REF7";
			supdoc7bis.CSI_SubType = "TRA";
			supdoc7bis.CSI_Status = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.Missing;

			var exitReportItem5 = exitReport.CusExitReportItems.AddNew();
			exitReportItem5.ERI_CCI_ConsignmentItem = exitConsignmentItem2.PK;

			var exitReportItem6 = exitReport.CusExitReportItems.AddNew();
			exitReportItem6.ERI_CCI_ConsignmentItem = exitConsignmentItem3.PK;

			var supdoc8 = exitReportItem6.AdditionalInfos.AddNew();
			supdoc8.CSI_ItemNumber = 8;
			supdoc8.CSI_Code = "9008";
			supdoc8.CSI_ReferenceNumber = "REF8";
			supdoc8.CSI_SubType = "TRA";
			supdoc8.CSI_Status = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;

			CombineAssertions(() =>
			{
				var goodsItems = EALAESGoodsItemWrapper.GetGoodsItemsList(exitReport);
				AssertEquals("Expected filled goodsItems", 4, goodsItems.Count);

				AssertContainsExactElementsInExactOrder("For first goodsItem expected filled TransportDocuments with correct data, SequenceNumber", new ZString[] { "1", "4" }, goodsItems[0].TransportDocuments.Select(x => x.SequenceNumber));
				AssertContainsExactElementsInExactOrder("For first goodsItem expected filled TransportDocuments with correct data, Name", new ZString[] { "9001", "" }, goodsItems[0].TransportDocuments.Select(x => x.Name));
				AssertContainsExactElementsInExactOrder("For first goodsItem expected filled TransportDocuments with correct data, Number", new ZString[] { "REF1", "" }, goodsItems[0].TransportDocuments.Select(x => x.Number));

				AssertContainsExactElementsInExactOrder("For second goodsItem expected filled TransportDocuments with correct data, SequenceNumber", new ZString[] { "5", "7" }, goodsItems[1].TransportDocuments.Select(x => x.SequenceNumber));
				AssertContainsExactElementsInExactOrder("For second goodsItem expected filled TransportDocuments with correct data, Name", new ZString[] { "9005", "" }, goodsItems[1].TransportDocuments.Select(x => x.Name));
				AssertContainsExactElementsInExactOrder("For second goodsItem expected filled TransportDocuments with correct data, Number", new ZString[] { "REF5", "" }, goodsItems[1].TransportDocuments.Select(x => x.Number));

				AssertContainsExactElementsInExactOrder("For third goodsItem expected filled TransportDocuments with correct data, SequenceNumber", new ZString[] { "8" }, goodsItems[2].TransportDocuments.Select(x => x.SequenceNumber));
				AssertContainsExactElementsInExactOrder("For third goodsItem expected filled TransportDocuments with correct data, Name", new ZString[] { "9008" }, goodsItems[2].TransportDocuments.Select(x => x.Name));
				AssertContainsExactElementsInExactOrder("For third goodsItem expected filled TransportDocuments with correct data, Number", new ZString[] { "REF8" }, goodsItems[2].TransportDocuments.Select(x => x.Number));

				AssertEquals("For fourth goodsItem expected empty TransportDocuments when CCI_DiscrepancyStatus is MIS", 0, goodsItems[3].TransportDocuments.Count);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
			exitConsignment = exitHeader.CusExitConsignments.AddNew();
			exitReport = exitHeader.CusExitReports.AddNew();
			exitReport.CER_CXC_Consignment = exitConsignment.PK;

			exitConsignmentItem = exitConsignment.CusExitConsignmentItems.AddNew();
			exitConsignmentItem.CCI_LineNumber = 3;

			wrapper = GetWrapper(exitConsignmentItem, 20m, 20m, Enumerable.Empty<(ZShort seqNum, ZString packageType, ZString packageQuantity, ZString shippingMarks)>(), Enumerable.Empty<AdditionalInfo>());
		}
		CusExitConsignment exitConsignment;
		CusExitReport exitReport;
		CusExitConsignmentItem exitConsignmentItem;
		EALAESGoodsItemWrapper wrapper;

		EALAESGoodsItemWrapper GetWrapper(CusExitConsignmentItem consignmentItem, ZDecimal grossMass, ZDecimal netMass, IEnumerable<(ZShort seqNum, ZString packageType, ZString packageQuantity, ZString shippingMarks)> packageData, IEnumerable<AdditionalInfo> transportDocuments)
								=> new EALAESGoodsItemWrapper(consignmentItem, grossMass, netMass, packageData, transportDocuments);
		protected override EALAESGoodsItemWrapper GetProvider() => wrapper;
	}
}
