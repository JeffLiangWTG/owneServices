using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.Resources.Handlers;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocBillOfLading))]
	sealed class DocBillOfLadingTest : NonPersistentBusinessObjectTestCase
	{
		#region Main Body Section

		#region Testing Local Settings

		public void TestFollowOnBodyBottomSectionHeight()
		{
			AssertEquals("FollowOnBodyBottomSectionHeight", 0, BOLWrapper.FollowOnBodyBottomSectionHeight);

			AddGoodsDescriptionToShipment("goods goods goods");
			AddContainerWithValues("c1", 0, 0, 0, "blah");
			AddContainerWithValues("c2", 0, 0, 0, "blah");
			AddContainerWithValues("c3", 0, 0, 0, "blah");
			AddContainerWithValues("c4", 0, 0, 0, "blah");
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 4);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 6);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 3);
			ResetBOLWrapper();
			AssertEquals("FollowOnBodyBottomSectionHeight", 2, BOLWrapper.FollowOnBodyBottomSectionHeight);

			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 3);
			ResetBOLWrapper();
			AssertEquals("FollowOnBodyBottomSectionHeight", 3, BOLWrapper.FollowOnBodyBottomSectionHeight);
		}

		public void TestFollowOnBodyTopSectionHeight()
		{
			AssertEquals("FollowOnBodyTopSectionHeight", 0, BOLWrapper.FollowOnBodyTopSectionHeight);

			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 3);
			AddGoodsDescriptionToShipment("goods goods goods");
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 6);
			AssertEquals("FollowOnBodyTopSectionHeight", 0, BOLWrapper.FollowOnBodyTopSectionHeight);

			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 2);
			ResetBOLWrapper();
			AssertEquals("FollowOnBodyTopSectionHeight", 1, BOLWrapper.FollowOnBodyTopSectionHeight);

			//Top section will use some of the container rows
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 3);
			ResetBOLWrapper();
			AssertEquals("FollowOnBodyTopSectionHeight", 0, BOLWrapper.FollowOnBodyTopSectionHeight);
		}

		public void TestMainBodyBottomSectionHeight()
		{
			AssertEquals("MainBodyBottomSectionHeight", 0, BOLWrapper.MainBodyBottomSectionHeight);

			AddContainerWithValues("c1", 0, 0, 0, "blah");
			AddGoodsDescriptionToShipment("goods goods goods");
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 6);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 2);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 4); //1 for the header, 1 for the blank line
			ResetBOLWrapper();
			AssertEquals("MainBodyBottomSectionHeight", 1, BOLWrapper.MainBodyBottomSectionHeight);

			AddContainerWithValues("c2", 0, 0, 0, "blah");
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 6);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 2);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 4);
			ResetBOLWrapper();
			AssertEquals("MainBodyBottomSectionHeight", 2, BOLWrapper.MainBodyBottomSectionHeight);

			AddContainerWithValues("c3", 0, 0, 0, "blah");
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 6);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 4);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 4);
			ResetBOLWrapper();
			AssertEquals("MainBodyBottomSectionHeight", 3, BOLWrapper.MainBodyBottomSectionHeight);

			ResetBOLWrapper();
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 10);
			AssertEquals("MainBodyBottomSectionHeight", 3, BOLWrapper.MainBodyBottomSectionHeight);

			FreightDataRegistry.Instance.BOLClause.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "hello hello hello hello hello hello hello hello hello hello hello hello hello hello hello hello hello hello hello hello hello hello hello hello hello hello hello hello hello hello");
			ResetBOLWrapper();
			AssertEquals("MainBodyBottomSectionHeight", 2, BOLWrapper.MainBodyBottomSectionHeight);

			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.IncludeBOLClauseInGoodsDescription, false);
			ResetBOLWrapper();
			AssertEquals("BOL Clause is not included with Goods Description", 3, BOLWrapper.MainBodyBottomSectionHeight);
		}

		public void TestMainBodySectionHeights()
		{
			//0 Containers | 0 lines of Goods Description
			AssertEquals("MainBodyTopSectionHeight", 0, BOLWrapper.MainBodyTopSectionHeight);
			AssertEquals("MainBodyBottomSectionHeight", 0, BOLWrapper.MainBodyBottomSectionHeight);

			//1 Container | 0 lines of Goods Description
			AddContainerWithValues("c1", 0, 0, 0, "blah");
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 10);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 5);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 6); //1 for the header, 1 for the blank line, 4 Container lines
			ResetBOLWrapper();
			AssertEquals("MainBodyTopSectionHeight", 0, BOLWrapper.MainBodyTopSectionHeight);
			AssertEquals("MainBodyBottomSectionHeight", 1, BOLWrapper.MainBodyBottomSectionHeight);

			//1 Container | 3 lines of Goods Description
			AddGoodsDescriptionToShipment("Goods\nGoods\nGoods");
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 10);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 5);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 6);
			ResetBOLWrapper();
			AssertEquals("MainBodyTopSectionHeight", 3, BOLWrapper.MainBodyTopSectionHeight);
			AssertEquals("MainBodyBottomSectionHeight", 1, BOLWrapper.MainBodyBottomSectionHeight);

			//1 Container | 6 lines of Goods Description
			Shipment.DetailedGoodsDescriptionNoteText = "Goods\nGoods\nGoods\nGoods\nGoods\nGoods";
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 10);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 5);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 6);
			ResetBOLWrapper();
			AssertEquals("MainBodyTopSectionHeight - uses some of container height", 6, BOLWrapper.MainBodyTopSectionHeight);
			AssertEquals("MainBodyBottomSectionHeight", 1, BOLWrapper.MainBodyBottomSectionHeight);

			//2 Containers | 6 lines of Goods Description
			AddContainerWithValues("c2", 0, 0, 0, "blah");
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 10);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 2);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 6);
			ResetBOLWrapper();
			AssertEquals("MainBodyTopSectionHeight - uses 2 lines from container height", 4, BOLWrapper.MainBodyTopSectionHeight);
			AssertEquals("MainBodyBottomSectionHeight", 2, BOLWrapper.MainBodyBottomSectionHeight);

			//2 Containers | 6 lines of Goods Description
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 10);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 10);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 6);
			ResetBOLWrapper();
			AssertEquals("MainBodyTopSectionHeight", 6, BOLWrapper.MainBodyTopSectionHeight);
			AssertEquals("MainBodyBottomSectionHeight", 2, BOLWrapper.MainBodyBottomSectionHeight);

			//3 Containers | 6 lines of Goods Description
			AddContainerWithValues("c3", 0, 0, 0, "blah");
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 10);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 10);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 6);
			ResetBOLWrapper();
			AssertEquals("MainBodyTopSectionHeight", 6, BOLWrapper.MainBodyTopSectionHeight);
			AssertEquals("MainBodyBottomSectionHeight", 3, BOLWrapper.MainBodyBottomSectionHeight);

			//3 Containers | 6 lines of Goods Description
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 10);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 10);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 3);
			ResetBOLWrapper();
			AssertEquals("MainBodyTopSectionHeight", 6, BOLWrapper.MainBodyTopSectionHeight);
			AssertEquals("MainBodyBottomSectionHeight - uses 2 lines from Top Section height", 3, BOLWrapper.MainBodyBottomSectionHeight);

			//3 Containers | 6 lines of Goods Description
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 10);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 6);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 3);
			ResetBOLWrapper();
			AssertEquals("MainBodyTopSectionHeight", 6, BOLWrapper.MainBodyTopSectionHeight);
			AssertEquals("MainBodyBottomSectionHeight - no lines to use", 1, BOLWrapper.MainBodyBottomSectionHeight);

			//5 Containers | 6 lines of Goods Description
			AddContainerWithValues("c4", 0, 0, 0, "blah");
			AddContainerWithValues("c5", 0, 0, 0, "blah");
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 10);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 20);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 6);
			ResetBOLWrapper();
			AssertEquals("MainBodyTopSectionHeight", 6, BOLWrapper.MainBodyTopSectionHeight);
			AssertEquals("MainBodyBottomSectionHeight", 5, BOLWrapper.MainBodyBottomSectionHeight);
		}

		public void TestMainBodyTopSectionHeight()
		{
			AssertEquals("MainBodyTopSectionHeight", 0, BOLWrapper.MainBodyTopSectionHeight);

			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 20);
			AddGoodsDescriptionToShipment("goods goods goods");
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 6);
			ResetBOLWrapper();
			AssertEquals("MainBodyTopSectionHeight", 3, BOLWrapper.MainBodyTopSectionHeight);

			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 2);
			ResetBOLWrapper();
			AssertEquals("MainBodyTopSectionHeight", 2, BOLWrapper.MainBodyTopSectionHeight);

			AddContainerWithValues("c1", 0, 0, 0, "blah");
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 4);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 2);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 6);
			ResetBOLWrapper();
			AssertEquals("MainBodyTopSectionHeight", 3, BOLWrapper.MainBodyTopSectionHeight);

			AddContainerWithValues("c2", 0, 0, 0, "blah");
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 4);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 2);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 6);
			ResetBOLWrapper();
			AssertEquals("MainBodyTopSectionHeight", 2, BOLWrapper.MainBodyTopSectionHeight);
		}

		public void TestMainBodyTopSectionHeight_ShowPackages()
		{
			Shipment.JS_OuterPacks = 5;
			Shipment.JS_F3_NKPackType = "ENV";

			ResetBOLWrapper();
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.IncludePackageCountInBOLGoodsDescription, 0);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 10);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.PackagesWidth, 10);
			AssertEquals("MainBodyTopSectionHeight", 0, BOLWrapper.MainBodyTopSectionHeight);

			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ShowPackageCount, 1);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.IncludePackageCountInBOLGoodsDescription, 0);
			ResetBOLWrapper();
			AssertEquals("MainBodyTopSectionHeight", 3, BOLWrapper.MainBodyTopSectionHeight);
		}

		public void TestMainBodyTopSectionHeight_ShowPackages_LineBreakPosition()
		{
			Shipment.JS_OuterPacks = 5;
			Shipment.JS_F3_NKPackType = "PAI";
			Shipment.JS_GoodsDescription = "some goods desc here";

			ResetBOLWrapper();
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ShowPackageCount, 1);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.IncludePackageCountInBOLGoodsDescription, 0);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 10);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.PackagesWidth, 11);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 30);
			AssertEquals("Main Body Top Section", true, BOLWrapper.MainBodyTopSection.StartsWith("    5 Pail(s)     some goods desc here"));
		}

		public void TestHeightNeededForTopSection()
		{
			AssertEquals("HeightNeededForTopSection", 0, BOLWrapper.HeightNeededForTopSection);

			AddMarksAndNumbersToShipment("blah blah blah");
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 10);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersWidth, 5);
			ResetBOLWrapper();
			AssertEquals("HeightNeededForTopSection", 3, BOLWrapper.HeightNeededForTopSection);

			Shipment = Factory.New<ForwardingShipment>();
			AddGoodsDescriptionToShipment("goods goods goods goods");
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 6);
			ResetBOLWrapper();
			AssertEquals("HeightNeededForTopSection", 4, BOLWrapper.HeightNeededForTopSection);
		}
		#endregion

		#region Testing Sections

		public void TestGettingTopSections()
		{
			AssertEquals("Main Body Top Section", ZString.Empty, BOLWrapper.GetMainBodyTopSection());
			AssertEquals("Follow On Body Top Section", ZString.Empty, BOLWrapper.GetFollowOnBodyTopSection().ToString());

			AddMarksAndNumbersToShipment("Marks and numbers Line One\nLine Two");
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersWidth, 31);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 1);
			ResetBOLWrapper();

			ZString expectedStringForMainBody = BOLWrapper.AlignToWidth("Marks and numbers Line One", ShipmentWrapper.MarksAndNumbersWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.MarksAndNumbersAndDescGap)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GoodsDescWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GoodsDescriptionAndGrossWeightGap)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GrossWeightWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GrossWeightAndMeasurementGap)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.VolumeMeasurementWidth);

			ZString expectedStringForFollowOnBody = BOLWrapper.MarksAndNumbersGoodsDescriptionHeaders + "\n"
				+ BOLWrapper.AlignToWidth("Line Two", ShipmentWrapper.MarksAndNumbersWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.MarksAndNumbersAndDescGap)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GoodsDescWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GoodsDescriptionAndGrossWeightGap)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GrossWeightWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GrossWeightAndMeasurementGap)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.VolumeMeasurementWidth);

			AssertEquals("Main Body Top Section", expectedStringForMainBody, BOLWrapper.GetMainBodyTopSection());
			AssertEquals("Follow On Body Top Section", expectedStringForFollowOnBody, BOLWrapper.GetFollowOnBodyTopSection().ToString());
		}

		public void TestGettingTopSections_ShowingPackages()
		{
			AssertEquals("Main Body Top Section", ZString.Empty, BOLWrapper.GetMainBodyTopSection());
			AssertEquals("Follow On Body Top Section", ZString.Empty, BOLWrapper.GetFollowOnBodyTopSection().ToString());

			AddMarksAndNumbersToShipment("Marks and numbers Line One\nLine Two");
			AddGoodsDescriptionToShipment("GoodsDescriptionTest", Shipment);
			Shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			Shipment.JS_OuterPacks = 10;
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersWidth, 31);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 1);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 25);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ShowPackageCount, 1);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.IncludePackageCountInBOLGoodsDescription, 0);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.PackagesWidth, 15);
			ResetBOLWrapper();

			ZString expectedStringForMainBody = BOLWrapper.AlignToWidth("Marks and numbers Line One", ShipmentWrapper.MarksAndNumbersWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.MarksAndNumbersAndDescGap)
				+ BOLWrapper.AlignToWidth("10 Pallet(s)  ", ShipmentWrapper.PackagesWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.MarksAndNumbersAndDescGap)
				+ BOLWrapper.AlignToWidth("GoodsDescriptionTest", ShipmentWrapper.GoodsDescWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GoodsDescriptionAndGrossWeightGap)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GrossWeightWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GrossWeightAndMeasurementGap)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.VolumeMeasurementWidth);

			ZString expectedStringForFollowOnBody = BOLWrapper.MarksAndNumbersGoodsDescriptionHeaders + "\n"
				+ BOLWrapper.AlignToWidth("Line Two", ShipmentWrapper.MarksAndNumbersWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.MarksAndNumbersAndDescGap)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.PackagesWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.MarksAndNumbersAndDescGap)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GoodsDescWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GoodsDescriptionAndGrossWeightGap)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GrossWeightWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GrossWeightAndMeasurementGap)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.VolumeMeasurementWidth);

			AssertEquals("Main Body Top Section with packages showing", expectedStringForMainBody, BOLWrapper.GetMainBodyTopSection());
			AssertEquals("Follow On Body Top Section with packages showing", expectedStringForFollowOnBody, BOLWrapper.GetFollowOnBodyTopSection().ToString());
		}

		public void TestGettingTopSections_ForColoadMaster()
		{
			Shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			CommonContainer container1 = Consol.Containers.AddNew();

			var subShipment1 = Factory.New<ForwardingShipment>();
			var subShipment2 = Factory.New<ForwardingShipment>();
			var subShipment3 = Factory.New<ForwardingShipment>();

			subShipment1.JS_JS_ColoadMasterShipment = Shipment.PK;
			subShipment2.JS_JS_ColoadMasterShipment = Shipment.PK;
			subShipment3.JS_JS_ColoadMasterShipment = Shipment.PK;

			subShipment1.JS_HouseBill = "1";
			subShipment2.JS_HouseBill = "2";
			subShipment3.JS_HouseBill = "3";

			var shipPackLine1 = (PackLine)subShipment1.OuterPackLines.AddNew();
			shipPackLine1.JL_ActualWeight = 10M;
			shipPackLine1.JL_ActualVolume = 10M;
			shipPackLine1.JL_PackageCount = 10;
			shipPackLine1.SetContainer(Consol, container1);

			var shipPackLine2 = (PackLine)subShipment2.OuterPackLines.AddNew();
			shipPackLine2.JL_ActualWeight = 2M;
			shipPackLine2.JL_ActualVolume = 2M;
			shipPackLine2.JL_PackageCount = 2;
			shipPackLine2.SetContainer(Consol, container1);

			var shipPackLine3 = (PackLine)subShipment3.OuterPackLines.AddNew();
			shipPackLine3.JL_ActualWeight = 1M;
			shipPackLine3.JL_ActualVolume = 1M;
			shipPackLine3.JL_PackageCount = 1;
			shipPackLine3.SetContainer(Consol, container1);

			AssertEquals("package counts", 13, BOLWrapper.Containers[0].Packs);
			AssertEquals("weights", 13M, BOLWrapper.Containers[0].Weight);
			AssertEquals("volumes", 13M, BOLWrapper.Containers[0].Volume);

			AddMarksAndNumbersToShipment("Master: Marks and numbers Line One\nLine Two", Shipment);
			AddMarksAndNumbersToShipment("Sub1: Marks&Numbers", subShipment1);
			AddMarksAndNumbersToShipment("Sub2: Marks&Numbers\nLine Two\nLine Three", subShipment2);
			AddMarksAndNumbersToShipment("Sub3: Marks&Numbers\nLine Two", subShipment3);

			AddGoodsDescriptionToShipment("Master: Goods Descriptione\nLine Two", Shipment);
			AddGoodsDescriptionToShipment("Sub1: Goods Description", subShipment1);
			AddGoodsDescriptionToShipment("Sub2: Goods Description\nLine Two", subShipment2);
			AddGoodsDescriptionToShipment("Sub3: Goods Description\nLine Two\nLine Three", subShipment3);

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersWidth, 31);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 5);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 31);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GrossWeightAndMeasurementGap, 1);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GrossWeightWidth, 10);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.VolumeMeasurementWidth, 10);

			foreach (DocForwardingShipment sub in ShipmentWrapper.ColoadShipments)
			{
				sub.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersWidth, 31);
				sub.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 5);
				sub.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 31);
				sub.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GrossWeightAndMeasurementGap, 1);
				sub.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GrossWeightWidth, 10);
				sub.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.VolumeMeasurementWidth, 10);
			}

			ResetBOLWrapper();

			ZString expectedStringForMainBody = BOLWrapper.AlignToWidth("Sub1: Marks&Numbers", ShipmentWrapper.MarksAndNumbersWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.MarksAndNumbersAndDescGap)
				+ BOLWrapper.AlignToWidth("Sub1: Goods Description", ShipmentWrapper.GoodsDescWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GoodsDescriptionAndGrossWeightGap)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GrossWeightWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GrossWeightAndMeasurementGap)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.VolumeMeasurementWidth)
				+ "\n";

			expectedStringForMainBody += BOLWrapper.AlignToWidth("-------------------------------", ShipmentWrapper.MarksAndNumbersWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.MarksAndNumbersAndDescGap)
				+ BOLWrapper.AlignToWidth("--------------------------------", ShipmentWrapper.GoodsDescWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GoodsDescriptionAndGrossWeightGap)
				+ BOLWrapper.AlignToWidth("----------", ShipmentWrapper.GrossWeightWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GrossWeightAndMeasurementGap)
				+ BOLWrapper.AlignToWidth("----------", ShipmentWrapper.VolumeMeasurementWidth)
				+ "\n";

			expectedStringForMainBody += BOLWrapper.AlignToWidth("Sub2: Marks&Numbers", ShipmentWrapper.MarksAndNumbersWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.MarksAndNumbersAndDescGap)
				+ BOLWrapper.AlignToWidth("Sub2: Goods Description", ShipmentWrapper.GoodsDescWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GoodsDescriptionAndGrossWeightGap)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GrossWeightWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GrossWeightAndMeasurementGap)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.VolumeMeasurementWidth)
				+ "\n";

			expectedStringForMainBody += BOLWrapper.AlignToWidth("Line Two", ShipmentWrapper.MarksAndNumbersWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.MarksAndNumbersAndDescGap)
				+ BOLWrapper.AlignToWidth("Line Two", ShipmentWrapper.GoodsDescWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GoodsDescriptionAndGrossWeightGap)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GrossWeightWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GrossWeightAndMeasurementGap)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.VolumeMeasurementWidth)
				+ "\n";

			expectedStringForMainBody += BOLWrapper.AlignToWidth("Line Three", ShipmentWrapper.MarksAndNumbersWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.MarksAndNumbersAndDescGap)
				+ BOLWrapper.AlignToWidth("", ShipmentWrapper.GoodsDescWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GoodsDescriptionAndGrossWeightGap)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GrossWeightWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GrossWeightAndMeasurementGap)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.VolumeMeasurementWidth)
				+ "\n";

			ZString expectedStringForFollowOnBody = BOLWrapper.MarksAndNumbersGoodsDescriptionHeaders + "\n"
				+ BOLWrapper.AlignToWidth("-------------------------------", ShipmentWrapper.MarksAndNumbersWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.MarksAndNumbersAndDescGap)
				+ BOLWrapper.AlignToWidth("--------------------------------", ShipmentWrapper.GoodsDescWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GoodsDescriptionAndGrossWeightGap)
				+ BOLWrapper.AlignToWidth("----------", ShipmentWrapper.GrossWeightWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GrossWeightAndMeasurementGap)
				+ BOLWrapper.AlignToWidth("----------", ShipmentWrapper.VolumeMeasurementWidth)
				+ "\n";

			expectedStringForFollowOnBody += BOLWrapper.AlignToWidth("Sub3: Marks&Numbers", ShipmentWrapper.MarksAndNumbersWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.MarksAndNumbersAndDescGap)
				+ BOLWrapper.AlignToWidth("Sub3: Goods Description", ShipmentWrapper.GoodsDescWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GoodsDescriptionAndGrossWeightGap)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GrossWeightWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GrossWeightAndMeasurementGap)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.VolumeMeasurementWidth)
				+ "\n";

			expectedStringForFollowOnBody += BOLWrapper.AlignToWidth("Line Two", ShipmentWrapper.MarksAndNumbersWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.MarksAndNumbersAndDescGap)
				+ BOLWrapper.AlignToWidth("Line Two", ShipmentWrapper.GoodsDescWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GoodsDescriptionAndGrossWeightGap)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GrossWeightWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GrossWeightAndMeasurementGap)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.VolumeMeasurementWidth)
				+ "\n";

			expectedStringForFollowOnBody += BOLWrapper.AlignToWidth("", ShipmentWrapper.MarksAndNumbersWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.MarksAndNumbersAndDescGap)
				+ BOLWrapper.AlignToWidth("Line Three", ShipmentWrapper.GoodsDescWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GoodsDescriptionAndGrossWeightGap)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GrossWeightWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.GrossWeightAndMeasurementGap)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.VolumeMeasurementWidth)
				+ "\n";

			AssertMultilineEquals("Main Body Top Section", expectedStringForMainBody, BOLWrapper.GetMainBodyTopSection(), '\n');
			AssertMultilineEquals("Follow On Body Top Section", expectedStringForFollowOnBody, BOLWrapper.GetFollowOnBodyTopSection().ToString(), '\n');
		}

		public void TestGettingBottomSection()
		{
			AssertEquals("Main Body Top Section", ZString.Empty, BOLWrapper.GetMainBodyBottomSection());
			AssertEquals("Follow On Body Bottom Section", ZString.Empty, BOLWrapper.GetFollowOnBodyBottomSection().ToString());

			#region SetUp Test Data
			CommonContainer containerInMainBody = AddContainerWithValues("Container 1", 10, 10M, 10M, "CY");
			CommonContainer containerInFollowOnBody = AddContainerWithValues("Container 2", 11, 11M, 11M, "blah");

			AddMarksAndNumbersToShipment("Marks and numbers Line One\nLine Two");
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 3);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 2);
			ZString expectedString = "";
			ZString tempShipperLoadAndCount = "* Shipper Load and Count";
			ZString containersHeaders = BOLWrapper.ContainersColumnHeaders + "\n";
			ZString deliveryModeInMainBodyContainer = BOLWrapper.AlignToWidth(containerInMainBody.JC_DeliveryMode + "*", ShipmentWrapper.ContainerModeWidth);
			ZString deliveryModeInFollowOnBodyContainer = BOLWrapper.AlignToWidth(containerInFollowOnBody.JC_DeliveryMode, ShipmentWrapper.ContainerModeWidth);

			ZString expectedStringMainBody = BOLWrapper.AlignToWidth("CONTAINER 1", ShipmentWrapper.ContainerNumberWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap)
					+ BOLWrapper.AlignToWidth("-", ShipmentWrapper.ContainerSealWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealAndTypeGap)
					+ BOLWrapper.AlignToWidth("-", ShipmentWrapper.ContainerTypeWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeAndWeightGap)
					+ BOLWrapper.AlignToWidth(ShipmentWrapper.FormatNumber(10M, Env.Registry.WeightMinimumDecimalPlacesToDisplay), ShipmentWrapper.ContainerWeightWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightAndVolumeGap)
					+ BOLWrapper.AlignToWidth(ShipmentWrapper.FormatNumber(10M, Env.Registry.VolumeMinimumDecimalPlacesToDisplay), ShipmentWrapper.ContainerVolumeWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerVolumeAndPackagesGap)
					+ BOLWrapper.AlignToWidth("10 PLT", ShipmentWrapper.ContainerPackagesWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerPackagesAndModeGap);

			ZString expectedStringFollowOnBody = BOLWrapper.AlignToWidth("CONTAINER 2", ShipmentWrapper.ContainerNumberWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap)
					+ BOLWrapper.AlignToWidth("-", ShipmentWrapper.ContainerSealWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealAndTypeGap)
					+ BOLWrapper.AlignToWidth("-", ShipmentWrapper.ContainerTypeWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeAndWeightGap)
					+ BOLWrapper.AlignToWidth(ShipmentWrapper.FormatNumber(11M, Env.Registry.WeightMinimumDecimalPlacesToDisplay), ShipmentWrapper.ContainerWeightWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightAndVolumeGap)
					+ BOLWrapper.AlignToWidth(ShipmentWrapper.FormatNumber(11M, Env.Registry.VolumeMinimumDecimalPlacesToDisplay), ShipmentWrapper.ContainerVolumeWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerVolumeAndPackagesGap)
					+ BOLWrapper.AlignToWidth("11 PLT", ShipmentWrapper.ContainerPackagesWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerPackagesAndModeGap);
			#endregion

			expectedString = containersHeaders + expectedStringMainBody + deliveryModeInMainBodyContainer;
			AssertEquals("Main Body Bottom Section", expectedString, BOLWrapper.GetMainBodyBottomSection());
			AssertEquals("Main Body Shipper Load and Count", tempShipperLoadAndCount, BOLWrapper.MainBodyShipperLoadAndCount);

			expectedString = containersHeaders + expectedStringFollowOnBody + deliveryModeInFollowOnBodyContainer;
			AssertEquals("Follow On Body Bottom Section", expectedString, BOLWrapper.GetFollowOnBodyBottomSection().ToString());
			AssertEquals("Follow On Body Shipper Load and Count", ZString.Empty, BOLWrapper.FollowOnBodyShipperLoadAndCount);

			containerInMainBody.JC_DeliveryMode = "blah";
			containerInFollowOnBody.JC_DeliveryMode = "CY";
			deliveryModeInMainBodyContainer = BOLWrapper.AlignToWidth(containerInMainBody.JC_DeliveryMode, ShipmentWrapper.ContainerModeWidth);
			deliveryModeInFollowOnBodyContainer = BOLWrapper.AlignToWidth(containerInFollowOnBody.JC_DeliveryMode + "*", ShipmentWrapper.ContainerModeWidth);
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 3);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 2);

			expectedString = containersHeaders + expectedStringMainBody + deliveryModeInMainBodyContainer;
			AssertEquals("Main Body Bottom Section", expectedString, BOLWrapper.GetMainBodyBottomSection());
			AssertEquals("Main Body Shipper Load and Count", ZString.Empty, BOLWrapper.MainBodyShipperLoadAndCount);

			expectedString = containersHeaders + expectedStringFollowOnBody + deliveryModeInFollowOnBodyContainer;
			AssertEquals("Follow On Body Bottom Section", expectedString, BOLWrapper.GetFollowOnBodyBottomSection().ToString());
			AssertEquals("Follow On Body Shipper Load and Count", tempShipperLoadAndCount, BOLWrapper.FollowOnBodyShipperLoadAndCount);

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 40);

			expectedString = containersHeaders
				+ expectedStringMainBody + deliveryModeInMainBodyContainer + "\n"
				+ expectedStringFollowOnBody + deliveryModeInFollowOnBodyContainer;

			AssertEquals("Main Body Bottom Section", expectedString, BOLWrapper.GetMainBodyBottomSection());
			AssertEquals("Main Body Shipper Load and Count", tempShipperLoadAndCount, BOLWrapper.MainBodyShipperLoadAndCount);
			AssertEquals("Follow On Body Bottom Section", ZString.Empty, BOLWrapper.GetFollowOnBodyBottomSection().ToString());
			AssertEquals("Follow On Body Shipper Load and Count", ZString.Empty, BOLWrapper.FollowOnBodyShipperLoadAndCount);
		}

		public void TestGettingBottomSectionWithHideSettingsON()
		{
			CommonContainer containerInMainBody = AddContainerWithValues("Container 1", 10, 10M, 10M, "CY");
			CommonContainer containerInFollowOnBody = AddContainerWithValues("Container 2", 11, 11M, 11M, "blah");

			AddMarksAndNumbersToShipment("Marks and numbers Line One\nLine Two");
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerMode, 0);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 3);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 2);

			ZString expectedStringMainBody = BOLWrapper.AlignToWidth("CONTAINER 1", ShipmentWrapper.ContainerNumberWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap)
				+ BOLWrapper.AlignToWidth("-", ShipmentWrapper.ContainerSealWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealAndTypeGap)
				+ BOLWrapper.AlignToWidth("-", ShipmentWrapper.ContainerTypeWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeAndWeightGap)
				+ BOLWrapper.AlignToWidth(ShipmentWrapper.FormatNumber(10M, Env.Registry.WeightMinimumDecimalPlacesToDisplay), ShipmentWrapper.ContainerWeightWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightAndVolumeGap)
				+ BOLWrapper.AlignToWidth(ShipmentWrapper.FormatNumber(10M, Env.Registry.VolumeMinimumDecimalPlacesToDisplay), ShipmentWrapper.ContainerVolumeWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerVolumeAndPackagesGap)
				+ BOLWrapper.AlignToWidth("10 PLT", ShipmentWrapper.ContainerPackagesWidth);

			ZString expectedString = BOLWrapper.ContainersColumnHeaders + "\n" + expectedStringMainBody;
			AssertEquals("Container sections", expectedString, BOLWrapper.GetMainBodyBottomSection().ToString());

			ResetBOLWrapper();
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerPackages, 0);
			expectedStringMainBody = BOLWrapper.AlignToWidth("CONTAINER 1", ShipmentWrapper.ContainerNumberWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap)
				+ BOLWrapper.AlignToWidth("-", ShipmentWrapper.ContainerSealWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealAndTypeGap)
				+ BOLWrapper.AlignToWidth("-", ShipmentWrapper.ContainerTypeWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeAndWeightGap)
				+ BOLWrapper.AlignToWidth(ShipmentWrapper.FormatNumber(10M, Env.Registry.WeightMinimumDecimalPlacesToDisplay), ShipmentWrapper.ContainerWeightWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightAndVolumeGap)
				+ BOLWrapper.AlignToWidth(ShipmentWrapper.FormatNumber(10M, Env.Registry.VolumeMinimumDecimalPlacesToDisplay), ShipmentWrapper.ContainerVolumeWidth);

			expectedString = BOLWrapper.ContainersColumnHeaders + "\n" + expectedStringMainBody;
			AssertEquals("Container sections", expectedString, BOLWrapper.GetMainBodyBottomSection());

			ResetBOLWrapper();
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerVolume, 0);
			expectedStringMainBody = BOLWrapper.AlignToWidth("CONTAINER 1", ShipmentWrapper.ContainerNumberWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap)
				+ BOLWrapper.AlignToWidth("-", ShipmentWrapper.ContainerSealWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealAndTypeGap)
				+ BOLWrapper.AlignToWidth("-", ShipmentWrapper.ContainerTypeWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeAndWeightGap)
				+ BOLWrapper.AlignToWidth(ShipmentWrapper.FormatNumber(10M, Env.Registry.WeightMinimumDecimalPlacesToDisplay), ShipmentWrapper.ContainerWeightWidth);

			expectedString = BOLWrapper.ContainersColumnHeaders + "\n" + expectedStringMainBody;
			AssertEquals("Container sections", expectedString, BOLWrapper.GetMainBodyBottomSection());

			ResetBOLWrapper();
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerWeight, 0);
			expectedStringMainBody = BOLWrapper.AlignToWidth("CONTAINER 1", ShipmentWrapper.ContainerNumberWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap)
				+ BOLWrapper.AlignToWidth("-", ShipmentWrapper.ContainerSealWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealAndTypeGap)
				+ BOLWrapper.AlignToWidth("-", ShipmentWrapper.ContainerTypeWidth);

			expectedString = BOLWrapper.ContainersColumnHeaders + "\n" + expectedStringMainBody;
			AssertEquals("Container sections", expectedString, BOLWrapper.GetMainBodyBottomSection());

			ResetBOLWrapper();
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerType, 0);
			expectedStringMainBody = BOLWrapper.AlignToWidth("CONTAINER 1", ShipmentWrapper.ContainerNumberWidth)
				+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap)
				+ BOLWrapper.AlignToWidth("-", ShipmentWrapper.ContainerSealWidth);

			expectedString = BOLWrapper.ContainersColumnHeaders + "\n" + expectedStringMainBody;
			AssertEquals("Container sections", expectedString, BOLWrapper.GetMainBodyBottomSection());

			ResetBOLWrapper();
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerSeal, 0);
			expectedStringMainBody = BOLWrapper.AlignToWidth("CONTAINER 1", ShipmentWrapper.ContainerNumberWidth);

			expectedString = BOLWrapper.ContainersColumnHeaders + "\n" + expectedStringMainBody;
			AssertEquals("Container sections", expectedString, BOLWrapper.GetMainBodyBottomSection());
		}

		public void TestGettingBottomSection_WithPackLineBreakdown()
		{
			AssertEquals("Main Body Top Section", ZString.Empty, BOLWrapper.GetMainBodyBottomSection());
			AssertEquals("Follow On Body Bottom Section", ZString.Empty, BOLWrapper.GetFollowOnBodyBottomSection().ToString());

			#region SetUp Test Data
			CommonContainer containerInMainBody = AddContainerWithValues("Container 1", 10, 10M, 10M, "CY");
			CommonContainer containerInFollowOnBody = AddContainerWithValues("Container 2", 11, 11M, 11M, "blah");

			AddMarksAndNumbersToShipment("Marks and numbers Line One\nLine Two");
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 3);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 2);
			ZString expectedString = "";
			ZString tempShipperLoadAndCount = "* Shipper Load and Count";
			ZString containersHeaders = BOLWrapper.ContainersColumnHeaders + "\n";
			ZString deliveryModeInMainBodyContainer = BOLWrapper.AlignToWidth(containerInMainBody.JC_DeliveryMode + "*", ShipmentWrapper.ContainerModeWidth);
			ZString deliveryModeInFollowOnBodyContainer = BOLWrapper.AlignToWidth(containerInFollowOnBody.JC_DeliveryMode, ShipmentWrapper.ContainerModeWidth);

			ZString expectedStringMainBody = BOLWrapper.AlignToWidth("CONTAINER 1", ShipmentWrapper.ContainerNumberWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap)
					+ BOLWrapper.AlignToWidth("-", ShipmentWrapper.ContainerSealWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealAndTypeGap)
					+ BOLWrapper.AlignToWidth("-", ShipmentWrapper.ContainerTypeWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeAndWeightGap)
					+ BOLWrapper.AlignToWidth(ShipmentWrapper.FormatNumber(10M, Env.Registry.WeightMinimumDecimalPlacesToDisplay), ShipmentWrapper.ContainerWeightWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightAndVolumeGap)
					+ BOLWrapper.AlignToWidth(ShipmentWrapper.FormatNumber(10M, Env.Registry.VolumeMinimumDecimalPlacesToDisplay), ShipmentWrapper.ContainerVolumeWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerVolumeAndPackagesGap)
					+ BOLWrapper.AlignToWidth("10 PLT", ShipmentWrapper.ContainerPackagesWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerPackagesAndModeGap);

			ZString expectedStringFollowOnBody = BOLWrapper.AlignToWidth("CONTAINER 2", ShipmentWrapper.ContainerNumberWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap)
					+ BOLWrapper.AlignToWidth("-", ShipmentWrapper.ContainerSealWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealAndTypeGap)
					+ BOLWrapper.AlignToWidth("-", ShipmentWrapper.ContainerTypeWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeAndWeightGap)
					+ BOLWrapper.AlignToWidth(ShipmentWrapper.FormatNumber(11M, Env.Registry.WeightMinimumDecimalPlacesToDisplay), ShipmentWrapper.ContainerWeightWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightAndVolumeGap)
					+ BOLWrapper.AlignToWidth(ShipmentWrapper.FormatNumber(11M, Env.Registry.VolumeMinimumDecimalPlacesToDisplay), ShipmentWrapper.ContainerVolumeWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerVolumeAndPackagesGap)
					+ BOLWrapper.AlignToWidth("11 PLT", ShipmentWrapper.ContainerPackagesWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerPackagesAndModeGap);
			#endregion

			expectedString = containersHeaders + expectedStringMainBody + deliveryModeInMainBodyContainer;
			AssertEquals("Main Body Bottom Section", expectedString, BOLWrapper.GetMainBodyBottomSection().ToString());
			AssertEquals("Main Body Shipper Load and Count", tempShipperLoadAndCount, BOLWrapper.MainBodyShipperLoadAndCount);

			expectedString = containersHeaders + expectedStringFollowOnBody + deliveryModeInFollowOnBodyContainer;
			AssertEquals("Follow On Body Bottom Section", expectedString, BOLWrapper.GetFollowOnBodyBottomSection().ToString());
			AssertEquals("Follow On Body Shipper Load and Count", ZString.Empty, BOLWrapper.FollowOnBodyShipperLoadAndCount);

			containerInMainBody.JC_DeliveryMode = "blah";
			containerInFollowOnBody.JC_DeliveryMode = "CY";
			deliveryModeInMainBodyContainer = BOLWrapper.AlignToWidth(containerInMainBody.JC_DeliveryMode, ShipmentWrapper.ContainerModeWidth);
			deliveryModeInFollowOnBodyContainer = BOLWrapper.AlignToWidth(containerInFollowOnBody.JC_DeliveryMode + "*", ShipmentWrapper.ContainerModeWidth);
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 3);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 2);

			expectedString = containersHeaders + expectedStringMainBody + deliveryModeInMainBodyContainer;
			AssertEquals("Main Body Bottom Section", expectedString, BOLWrapper.GetMainBodyBottomSection());
			AssertEquals("Main Body Shipper Load and Count", ZString.Empty, BOLWrapper.MainBodyShipperLoadAndCount);

			expectedString = containersHeaders + expectedStringFollowOnBody + deliveryModeInFollowOnBodyContainer;
			AssertEquals("Follow On Body Bottom Section", expectedString, BOLWrapper.GetFollowOnBodyBottomSection().ToString());
			AssertEquals("Follow On Body Shipper Load and Count", tempShipperLoadAndCount, BOLWrapper.FollowOnBodyShipperLoadAndCount);

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 40);

			expectedString = containersHeaders
				+ expectedStringMainBody + deliveryModeInMainBodyContainer + "\n"
				+ expectedStringFollowOnBody + deliveryModeInFollowOnBodyContainer;

			AssertEquals("Main Body Bottom Section", expectedString, BOLWrapper.GetMainBodyBottomSection());
			AssertEquals("Main Body Shipper Load and Count", tempShipperLoadAndCount, BOLWrapper.MainBodyShipperLoadAndCount);
			AssertEquals("Follow On Body Bottom Section", ZString.Empty, BOLWrapper.GetFollowOnBodyBottomSection().ToString());
			AssertEquals("Follow On Body Shipper Load and Count", ZString.Empty, BOLWrapper.FollowOnBodyShipperLoadAndCount);

			FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 3);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 2);

			expectedString = containersHeaders + expectedStringMainBody + deliveryModeInMainBodyContainer;
			AssertMultilineEquals("Main Body Bottom Section", expectedString, BOLWrapper.GetMainBodyBottomSection(), '\n');
			AssertEquals("Main Body Shipper Load and Count", ZString.Empty, BOLWrapper.MainBodyShipperLoadAndCount);

			expectedString = "Container       Seals                Type     Weight(KG)     Volume(M3)     Packages     Mode      \n    10 PLT - 10 KG - GEN\nCONTAINER 2     -                    -        11             11             11 PLT       CY*       \n    11 PLT - 11 KG - GEN";
			AssertMultilineEquals("Follow On Body Bottom Section", expectedString, BOLWrapper.GetFollowOnBodyBottomSection().ToString(), '\n');
			AssertEquals("Follow On Body Shipper Load and Count", tempShipperLoadAndCount, BOLWrapper.FollowOnBodyShipperLoadAndCount);
		}

		public void TestGettingBottomSection_MultipleSeals()
		{
			AssertEquals("Main Body Top Section", ZString.Empty, BOLWrapper.GetMainBodyBottomSection());
			AssertEquals("Follow On Body Bottom Section", ZString.Empty, BOLWrapper.GetFollowOnBodyBottomSection().ToString());

			#region SetUp Test Data
			CommonContainer containerInMainBody = AddContainerWithValues("Container 1", 10, 10M, 10M, "CY");
			CommonContainer containerInFollowOnBody = AddContainerWithValues("Container 2", 11, 11M, 11M, "blak");

			containerInMainBody.JC_SealNum = "ContainerInMainBody1";
			containerInMainBody.JC_AdditionalSealNum = "MainCodyAS1";
			containerInMainBody.JC_Additional2SealNum = "MainDodyAS2";

			containerInFollowOnBody.JC_SealNum = "FollowContainer_1";
			containerInFollowOnBody.JC_AdditionalSealNum = "FollowBodyAS1";
			containerInFollowOnBody.JC_Additional2SealNum = "FollowBodyAS23456789";

			AddMarksAndNumbersToShipment("Marks and numbers Line One\nLine Two");
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 3);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 2);
			ZString expectedString = "";
			ZString tempShipperLoadAndCount = "* Shipper Load and Count";
			ZString containersHeaders = BOLWrapper.ContainersColumnHeaders + "\n";
			ZString deliveryModeInMainBodyContainer = BOLWrapper.AlignToWidth(containerInMainBody.JC_DeliveryMode + "*", ShipmentWrapper.ContainerModeWidth);
			ZString deliveryModeInFollowOnBodyContainer = BOLWrapper.AlignToWidth(containerInFollowOnBody.JC_DeliveryMode, ShipmentWrapper.ContainerModeWidth);

			ZString expectedStringMainBody = BOLWrapper.AlignToWidth("CONTAINER 1", ShipmentWrapper.ContainerNumberWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap)
					+ BOLWrapper.AlignToWidth("ContainerInMainBody1", ShipmentWrapper.ContainerSealWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealAndTypeGap)
					+ BOLWrapper.AlignToWidth("-", ShipmentWrapper.ContainerTypeWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeAndWeightGap)
					+ BOLWrapper.AlignToWidth(ShipmentWrapper.FormatNumber(10M, Env.Registry.WeightMinimumDecimalPlacesToDisplay), ShipmentWrapper.ContainerWeightWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightAndVolumeGap)
					+ BOLWrapper.AlignToWidth(ShipmentWrapper.FormatNumber(10M, Env.Registry.VolumeMinimumDecimalPlacesToDisplay), ShipmentWrapper.ContainerVolumeWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerVolumeAndPackagesGap)
					+ BOLWrapper.AlignToWidth("10 PLT", ShipmentWrapper.ContainerPackagesWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerPackagesAndModeGap);

			ZString expectedStringFollowOnBody = BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberWidth)
					+ " " + BOLWrapper.AlignToWidth(", MainCodyAS1, MainD", ShipmentWrapper.ContainerSealWidth)
					+ "\n"
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberWidth)
					+ " " + BOLWrapper.AlignToWidth("odyAS2", ShipmentWrapper.ContainerSealWidth)
					+ "\n"
					+ BOLWrapper.AlignToWidth("CONTAINER 2", ShipmentWrapper.ContainerNumberWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap)
					+ BOLWrapper.AlignToWidth("FollowContainer_1, F", ShipmentWrapper.ContainerSealWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealAndTypeGap)
					+ BOLWrapper.AlignToWidth("-", ShipmentWrapper.ContainerTypeWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeAndWeightGap)
					+ BOLWrapper.AlignToWidth(ShipmentWrapper.FormatNumber(11M, Env.Registry.WeightMinimumDecimalPlacesToDisplay), ShipmentWrapper.ContainerWeightWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightAndVolumeGap)
					+ BOLWrapper.AlignToWidth(ShipmentWrapper.FormatNumber(11M, Env.Registry.VolumeMinimumDecimalPlacesToDisplay), ShipmentWrapper.ContainerVolumeWidth)
					+ BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerVolumeAndPackagesGap)
					+ BOLWrapper.AlignToWidth("11 PLT", ShipmentWrapper.ContainerPackagesWidth)
					+ " " + deliveryModeInFollowOnBodyContainer
					+ "\n"
					+ "                ollowBodyAS1, Follow"
					+ "\n"
						+ "                BodyAS23456789      ";

			#endregion

			expectedString = containersHeaders + expectedStringMainBody + deliveryModeInMainBodyContainer;
			AssertEquals("Main Body Bottom Section", expectedString, BOLWrapper.GetMainBodyBottomSection());
			AssertEquals("Main Body Shipper Load and Count", tempShipperLoadAndCount, BOLWrapper.MainBodyShipperLoadAndCount);

			expectedString = containersHeaders + expectedStringFollowOnBody;
			AssertEquals("Follow On Body Bottom Section", expectedString, BOLWrapper.GetFollowOnBodyBottomSection().ToString());
			AssertEquals("Follow On Body Shipper Load and Count", "", BOLWrapper.FollowOnBodyShipperLoadAndCount);

			containerInMainBody.JC_DeliveryMode = "blah";
			containerInFollowOnBody.JC_DeliveryMode = "CY";
			deliveryModeInMainBodyContainer = BOLWrapper.AlignToWidth(containerInMainBody.JC_DeliveryMode, ShipmentWrapper.ContainerModeWidth);
			deliveryModeInFollowOnBodyContainer = BOLWrapper.AlignToWidth(containerInFollowOnBody.JC_DeliveryMode + "*", ShipmentWrapper.ContainerModeWidth);
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 3);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 2);

			expectedString = containersHeaders + expectedStringMainBody + deliveryModeInMainBodyContainer;
			AssertEquals("Main Body Bottom Section", expectedString, BOLWrapper.GetMainBodyBottomSection());
			AssertEquals("Main Body Shipper Load and Count", ZString.Empty, BOLWrapper.MainBodyShipperLoadAndCount);

			expectedString = containersHeaders + expectedStringFollowOnBody;
			expectedString = expectedString.Replace("blak", "CY* ");
			AssertEquals("Follow On Body Bottom Section", expectedString, BOLWrapper.GetFollowOnBodyBottomSection().ToString());
			AssertEquals("Follow On Body Shipper Load and Count", tempShipperLoadAndCount, BOLWrapper.FollowOnBodyShipperLoadAndCount);

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 40);

			expectedString = containersHeaders
				+ expectedStringMainBody + deliveryModeInMainBodyContainer + "\n"
				+ expectedStringFollowOnBody + deliveryModeInFollowOnBodyContainer;
		}

		#region Shipper Load And Count Not Cleared When Resetting BoL

		public void TestFollowOnBodyShipperLoadAndCountNotClearedWhenBoLIsReset()
		{
			Assert("Pre-condition: Follow On Body Shipper Load and Count should default to empty", BOLWrapper.FollowOnBodyShipperLoadAndCount.IsEmpty);

			CommonContainer mainBodyContainer = AddContainerWithValues("Container 1", 10, 10M, 10M, "CY");
			CommonContainer followOnBodyContainer1 = AddContainerWithValues("Container 2", 10, 10M, 10M, "blak");
			CommonContainer followOnBodyContainer2 = AddContainerWithValues("Container 3", 12, 12M, 12M, "CY");
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 1);
			ResetWrappers();

			Assert("Follow On Body section should not be empty as the number of containers exceeds the NumberOfContainerRows", !BOLWrapper.FollowOnBodySections.IsEmpty);
			AssertEquals("Follow On Body Shipper Load and Count should have been set", ShipperLoadAndCountDefaultText, BOLWrapper.FollowOnBodyShipperLoadAndCount);

			ResetWrappers();

			AssertEquals("Follow On Body Shipper Load and Count should still not be empty", ShipperLoadAndCountDefaultText, BOLWrapper.FollowOnBodyShipperLoadAndCount);
		}

		public void TestMainBodyShipperLoadAndCountNotClearedWhenBoLIsReset()
		{
			Assert("Pre-condition: Main Body Shipper Load and Count should default to empty", BOLWrapper.MainBodyShipperLoadAndCount.IsEmpty);

			CommonContainer mainBodyContainer = AddContainerWithValues("Container 1", 10, 10M, 10M, "CY");
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 2);

			ResetWrappers();

			AssertEquals("Main Body Shipper Load and Count", ShipperLoadAndCountDefaultText, BOLWrapper.MainBodyShipperLoadAndCount);

			ResetWrappers();

			AssertEquals("Main Body Shipper Load and Count should still not be empty", ShipperLoadAndCountDefaultText, BOLWrapper.MainBodyShipperLoadAndCount);
		}

		const string ShipperLoadAndCountDefaultText = "* Shipper Load and Count";

		#endregion

		public void TestWrapSections()
		{
			FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			AssertEquals("Main Body Top Section", ZString.Empty, BOLWrapper.GetMainBodyBottomSection());
			AssertEquals("Follow On Body Bottom Section", ZString.Empty, BOLWrapper.GetFollowOnBodyBottomSection().ToString());

			PackLine packLine1 = Shipment.OuterPackLines.AddNew();
			packLine1.JL_FreightMode = FreightConstants.OuterPackType;
			packLine1.JL_PackageCount = 2;
			packLine1.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine1.JL_Description = "Goods\nDescription";
			packLine1.JL_RH_NKCommodityCode = "HAZ";
			packLine1.JL_ActualWeight = 22.3m;
			packLine1.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;

			PackLine packLine2 = Shipment.OuterPackLines.AddNew();
			packLine2.JL_FreightMode = FreightConstants.OuterPackType;
			packLine2.JL_PackageCount = 3;
			packLine2.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine2.JL_Description = "Description";
			packLine2.JL_RH_NKCommodityCode = "GEN";
			packLine2.JL_ActualWeight = 22.4m;
			packLine2.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;

			UNDGSubstance undgSubstance = Factory.New<UNDGSubstance>();
			undgSubstance.DG_UNNO = "2";
			undgSubstance.DG_Variant = "b";
			undgSubstance.DG_PSN = "PSN";
			undgSubstance.DG_Class = "1.1";
			undgSubstance.DG_PG = "PG";
			packLine1.UNDGs.AddNew().DI_DG = undgSubstance.PK;

			CommonContainer container1 = AddContainerWithValues("Container 1", 10, 10M, 10M, "CY");
			CommonContainer container2 = AddContainerWithValues("Container 2", 11, 11M, 11M, "XX");

			packLine1.Containers.Add(container1);
			packLine2.Containers.Add(container1);

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();

			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 3);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 2);

			AssertEquals(3, BOLWrapper.MainBodyBottomColumnTextSection.Height);
			AssertEquals("CONTAINER 1     -                    -        54.7           10             15 PLT       CY*       \n" +
				"     2 PLT - 22.3 KG - HAZ - UN2, PSN, class 1.1, PG PG - Goods Description\n" +
				"     3 PLT - 22.4 KG - GEN - Description",
				BOLWrapper.MainBodyBottomColumnTextSection.ToString());
			AssertEquals(4, BOLWrapper.FollowOnBodyBottomSection.Height);
			AssertEquals("Container       Seals                Type     Weight(KG)     Volume(M3)     Packages     Mode      \n" +
				"    10 PLT - 10 KG - GEN\n" +
				"CONTAINER 2     -                    -        11             11             11 PLT       XX        \n" +
				"    11 PLT - 11 KG - GEN",
				BOLWrapper.FollowOnBodyBottomSection.ToString());

			UNDGDataItem undgDataItem = packLine1.UNDGs.AddNew();
			undgDataItem.DI_DG = undgSubstance.PK;
			undgDataItem.DI_TechnicalName = "Ranked from most to least dangerous, the most dangerous substances is deemed to be heroin aka smack.";

			ResetBOLWrapper();

			AssertEquals(3, BOLWrapper.MainBodyBottomColumnTextSection.Height);
			AssertEquals("CONTAINER 1     -                    -        54.7           10             15 PLT       CY*       \n" +
				"     2 PLT - 22.3 KG - HAZ - UN2, PSN, class 1.1, PG PG\r\n" +
				"UN2, PSN (Ranked from most to least dangerous, the most dangerous substances is deemed to be heroin aka smack.), class 1.1, PG PG - Goods Description",
				BOLWrapper.MainBodyBottomColumnTextSection.ToString());
			AssertEquals(5, BOLWrapper.FollowOnBodyBottomSection.Height);
			AssertEquals("Container       Seals                Type     Weight(KG)     Volume(M3)     Packages     Mode      \n" +
				"     3 PLT - 22.4 KG - GEN - Description\n" +
				"    10 PLT - 10 KG - GEN\n" +
				"CONTAINER 2     -                    -        11             11             11 PLT       XX        \n" +
				"    11 PLT - 11 KG - GEN",
				BOLWrapper.FollowOnBodyBottomSection.ToString());
		}

		public void TestMultiLinePacklinesDontGetLost()
		{
			Shipment.JS_GoodsDescription = "chemicals";
			Shipment.JS_PackingMode = "FCL";

			RefContainer refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			ForwardingContainer container1 = Consol.Containers.AddNew();
			ForwardingContainer container2 = Consol.Containers.AddNew();
			ForwardingContainer container3 = Consol.Containers.AddNew();

			container1.JC_ContainerMode = "FCL";
			container1.JC_ContainerNum = "FFFF3838383";
			container1.JC_DeliveryMode = "CY/CY";
			container1.JC_RC = refContainer.PK;

			container2.JC_ContainerMode = "FCL";
			container2.JC_ContainerNum = "GGGG4848484";
			container2.JC_DeliveryMode = "CY/CY";
			container2.JC_RC = refContainer.PK;

			container3.JC_ContainerMode = "FCL";
			container3.JC_ContainerNum = "HHHH5858585";
			container3.JC_DeliveryMode = "CY/CY";
			container3.JC_RC = refContainer.PK;

			Consol.Containers.Add(container1);
			Consol.Containers.Add(container2);
			Consol.Containers.Add(container3);

			Shipment.OuterPackLines.RemoveAll();

			ForwardingPackLine pack1 = Shipment.OuterPackLines.AddNew();
			pack1.JL_PackageCount = 1;
			pack1.JL_ActualWeight = 20;
			pack1.JL_ActualVolume = 2;
			pack1.Containers.RemoveAll();
			pack1.Containers.Add(container1);

			ForwardingPackLine pack2 = Shipment.OuterPackLines.AddNew();
			pack2.JL_PackageCount = 1;
			pack2.JL_ActualWeight = 20;
			pack2.JL_ActualVolume = 2;
			pack2.Containers.RemoveAll();
			pack2.Containers.Add(container2);

			ForwardingPackLine pack3 = Shipment.OuterPackLines.AddNew();
			pack3.JL_PackageCount = 1;
			pack3.JL_ActualWeight = 20;
			pack3.JL_ActualVolume = 2;
			pack3.JL_ContainerPackingOrder = 1;
			pack3.Containers.RemoveAll();
			pack3.Containers.Add(container3);

			ForwardingPackLine pack4 = Shipment.OuterPackLines.AddNew();
			pack4.JL_PackageCount = 1;
			pack4.JL_ActualWeight = 30;
			pack4.JL_ActualVolume = 3;
			pack4.JL_ContainerPackingOrder = 2;
			pack4.Containers.RemoveAll();
			pack4.Containers.Add(container3);

			ForwardingPackLine pack5 = Shipment.OuterPackLines.AddNew();
			pack5.JL_PackageCount = 1;
			pack5.JL_ActualWeight = 40;
			pack5.JL_ActualVolume = 4;
			pack5.JL_ContainerPackingOrder = 3;
			pack5.Containers.RemoveAll();
			pack5.Containers.Add(container3);

			UNDGDataItem undg1 = Factory.New<UNDGDataItem>();
			undg1.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "1001", "", "IMO").First().PK;

			UNDGDataItem undg2 = Factory.New<UNDGDataItem>();
			undg2.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "2000", "", "IMO").First().PK;

			var subs3005a = UNDGSubstanceLoader.LoadSubstances(Factory, "3005", "A", "IMO").First();
			UNDGDataItem undg3 = Factory.New<UNDGDataItem>();
			undg3.DI_DG = subs3005a.PK;

			UNDGDataItem undg4 = Factory.New<UNDGDataItem>();
			undg4.DI_DG = subs3005a.PK;

			UNDGDataItem undg5 = Factory.New<UNDGDataItem>();
			undg5.DI_DG = subs3005a.PK;

			pack1.UNDGs.Add(undg1);
			pack2.UNDGs.Add(undg2);
			pack3.UNDGs.Add(undg3);
			pack4.UNDGs.Add(undg4);
			pack5.UNDGs.Add(undg5);

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);

			ResetBOLWrapper();

			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 6);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 19);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 55);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersWidth, 30);

			FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			FreightDataRegistry.Instance.BOLClause.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				"These commodities, technology or software were exported from the United States in accordance with the Export Administration Regulations. Diversion contrary to U.S. law is prohibited.");

			AssertEquals(10, BOLWrapper.MainBodyBottomColumnTextSection.Height);
			AssertMultilineASCIIEquals("MainBodyBottomColumnTextSection",
				"FFFF3838383     -                    20GP     20             2280       2300       2              1 PLT        CY/CY*    \n" +
				"     1 PLT - 20 KG - GEN - UN1001, ACETYLENE, DISSOLVED, class 2.1 - chemicals\n" +
				"GGGG4848484     -                    20GP     20             2280       2300       2              1 PLT        CY/CY*    \n" +
				"     1 PLT - 20 KG - GEN - UN2000, CELLULOID, class 4.1, PG III - chemicals\n" +
				"HHHH5858585     -                    20GP     90             2280       2370       9              3 PLT        CY/CY*    \n" +
				"     1 PLT - 20 KG - GEN - UN3005, THIOCARBAMATE PESTICIDE, LIQUID, TOXIC, FLAMMABLE, class 6.1 (3), PG I, (23.0C c.c.) - chemicals",
				BOLWrapper.MainBodyBottomColumnTextSection.ToString());

			AssertEquals(6, BOLWrapper.FollowOnBodyBottomSection.Height);
			AssertMultilineASCIIEquals("FollowOnBodyBottomSection",
				"Container       Seals                Type     Weight(KG)     Tare(KG)   Gross(KG)  Volume(M3)     Packages     Mode      \n" +
				"     1 PLT - 30 KG - GEN - UN3005, THIOCARBAMATE PESTICIDE, LIQUID, TOXIC, FLAMMABLE, class 6.1 (3), PG I, (23.0C c.c.) - chemicals\n" +
				"     1 PLT - 40 KG - GEN - UN3005, THIOCARBAMATE PESTICIDE, LIQUID, TOXIC, FLAMMABLE, class 6.1 (3), PG I, (23.0C c.c.) - chemicals",
				BOLWrapper.FollowOnBodyBottomSection.ToString());
		}

		#endregion

		#region Testing Tools & Collections
		public void TestMarksAndNumbersStringCollection()
		{
			AssertEquals("Marks and numbers", 0, BOLWrapper.MarksAndNumbersStringCollection.Count);

			AddMarksAndNumbersToShipment("Marks and numbers Line One\nLine Two");
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersWidth, 31);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 20);
			ResetBOLWrapper();

			AssertEquals("Marks and numbers", 2, BOLWrapper.MarksAndNumbersStringCollection.Count);
			AssertEquals("Marks and numbers", BOLWrapper.AlignToWidth("Marks and numbers Line One", 31), BOLWrapper.MarksAndNumbersStringCollection[0]);
			AssertEquals("Marks and numbers", BOLWrapper.AlignToWidth("Line Two", 31), BOLWrapper.MarksAndNumbersStringCollection[1]);
		}

		public void TestGoodsDescriptionStringCollection()
		{
			Action resetWrappers = () =>
			{
				ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
				ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 40);
				ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 20);
				ResetBOLWrapper();
			};

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			Shipment.JS_RL_NKOrigin = "AUSYD";
			Shipment.JS_RL_NKDestination = "NZAKL";
			CountryExportStatementSettingCollection countrySettingCollection = FreightDataRegistry.Instance.ExportStatementSettings.Value;
			CountryExportStatementSetting countrySetting = countrySettingCollection.AddNew();
			countrySetting.CountryCode = GlbBranch.CurrentBranch.Country.Code;
			ExportStatementSetting statementSetting = countrySetting.Statements.AddNew();
			statementSetting.Code = "AES";
			statementSetting.Statement = "STATEMENT FOR TESTING \r\n NEW LINE STATEMENT";
			statementSetting.Visibility = "UDF";
			statementSetting.UseOnHouseBillOfLading = true;
			FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, countrySettingCollection);

			AssertEquals("Goods desc collection", 0, BOLWrapper.GoodsDescriptionStringCollection.Count);

			AddGoodsDescriptionToShipment("description of goods");
			AddContainerToShipment();
			resetWrappers();

			AssertEquals("Goods desc collection", 2, BOLWrapper.GoodsDescriptionStringCollection.Count);
			AssertEquals("Goods desc collection", BOLWrapper.AlignToWidth("1 x 20NOR CONTAINER", 40), BOLWrapper.GoodsDescriptionStringCollection[0]);
			AssertEquals("Goods desc collection", BOLWrapper.AlignToWidth("description of goods", 40), BOLWrapper.GoodsDescriptionStringCollection[1]);

			AddAnotherContainerToShipment();
			resetWrappers();
			AssertEquals("Goods desc collection", 3, BOLWrapper.GoodsDescriptionStringCollection.Count);
			AssertEquals("Goods desc collection", BOLWrapper.AlignToWidth("1 x 20NOR CONTAINER", 40), BOLWrapper.GoodsDescriptionStringCollection[0]);
			AssertEquals("Goods desc collection", BOLWrapper.AlignToWidth("1 x 40FR CONTAINER", 40), BOLWrapper.GoodsDescriptionStringCollection[1]);
			AssertEquals("Goods desc collection", BOLWrapper.AlignToWidth("description of goods", 40), BOLWrapper.GoodsDescriptionStringCollection[2]);

			Shipment.DocsAndCartage.JP_ExportStatement = statementSetting.Code;
			resetWrappers();

			ZString[] statements = statementSetting.Statement.Split(new char[] { '\r', '\n' });

			AssertEquals("Goods desc collection", 5, BOLWrapper.GoodsDescriptionStringCollection.Count);
			AssertEquals("Goods desc collection", BOLWrapper.AlignToWidth("1 x 20NOR CONTAINER", 40), BOLWrapper.GoodsDescriptionStringCollection[0]);
			AssertEquals("Goods desc collection", BOLWrapper.AlignToWidth("1 x 40FR CONTAINER", 40), BOLWrapper.GoodsDescriptionStringCollection[1]);
			AssertEquals("Goods desc collection", BOLWrapper.AlignToWidth("description of goods", 40), BOLWrapper.GoodsDescriptionStringCollection[2]);
			AssertContains("Goods desc collection contains first line of export statement", statements[0], BOLWrapper.GoodsDescriptionStringCollection[3]);
			AssertContains("Goods desc collection contains second line of export statement", statements[1], BOLWrapper.GoodsDescriptionStringCollection[4]);

			statementSetting.UseOnHouseBillOfLading = false;
			FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, countrySettingCollection);
			resetWrappers();
			AssertEquals("Goods desc collection", 3, BOLWrapper.GoodsDescriptionStringCollection.Count);
			AssertEquals("Goods desc collection", BOLWrapper.AlignToWidth("1 x 20NOR CONTAINER", 40), BOLWrapper.GoodsDescriptionStringCollection[0]);
			AssertEquals("Goods desc collection", BOLWrapper.AlignToWidth("1 x 40FR CONTAINER", 40), BOLWrapper.GoodsDescriptionStringCollection[1]);
			AssertEquals("Goods desc collection", BOLWrapper.AlignToWidth("description of goods", 40), BOLWrapper.GoodsDescriptionStringCollection[2]);

			CountryExportStatementSettingCollection defaultValue = new CountryExportStatementSettingCollection();
			CountryExportStatementSetting sEDSetting = defaultValue.AddNew();
			sEDSetting.CountryCode = Core.Constants.CountryCodes.Australia;
			sEDSetting.Statements.Add(new ExportStatementSetting(sEDSetting, "GBH", "A user defined statement", "", "", "", "UDF", true, true, true, true, true, true));
			sEDSetting.Statements.Add(new ExportStatementSetting(sEDSetting, "MAT", "A mandatory statement", "", "", "", "MAN", true, true, true, true, true, true));
			FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValue);
			Factory.Save();

			resetWrappers();

			AssertContains("A mandatory statement", BOLWrapper.GoodsDescriptionStringCollection[3]);

			CusEntryNumber cus1 = Shipment.CusEntryNumbers.AddNew();
			cus1.CE_EntryType = "ABC";
			cus1.CE_EntryNum = "1122334455";

			resetWrappers();

			Assert(!BOLWrapper.GoodsDescriptionStringCollection.Cast<string>().Any(s => s.Contains("ABC: 1122334455")));

			CusEntryNumber cus2 = Shipment.CusEntryNumbers.AddNew();
			cus2.CE_EntryType = "XYZ";
			cus2.CE_EntryNum = "9988776655";

			resetWrappers();

			AssertContains("ABC: 1122334455, XYZ: 9988776655", BOLWrapper.GoodsDescriptionStringCollection[4]);
		}

		public void TestShipmentWeightCollection()
		{
			AssertEquals("Weight collection", 0, BOLWrapper.ShipmentWeightCollection.Count);

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();
			Shipment.JS_ActualWeight = 123.456M;
			Shipment.JS_UnitOfWeight = "KG";
			ZString expected = ShipmentWrapper.FormatNumber(Shipment.JS_ActualWeight, Env.Registry.WeightMinimumDecimalPlacesToDisplay);
			AssertEquals("Weight collection", 1, BOLWrapper.ShipmentWeightCollection.Count);
			AssertEquals("Weight collection", expected + " KG", BOLWrapper.ShipmentWeightCollection[0]);

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();
			Shipment.JS_ActualWeight = 123456M;
			Shipment.JS_UnitOfWeight = "G";
			AssertEquals("Weight collection", 2, BOLWrapper.ShipmentWeightCollection.Count);
			AssertEquals("Weight collection", expected + " KG", BOLWrapper.ShipmentWeightCollection[0]);
			AssertEquals("Weight collection", "(123456 G)", BOLWrapper.ShipmentWeightCollection[1]);
		}

		public void TestShipmentVolumeCollection()
		{
			AssertEquals("Volume collection", 0, BOLWrapper.ShipmentVolumeCollection.Count);

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();
			Shipment.JS_ActualVolume = 123.456M;
			Shipment.JS_UnitOfVolume = "M3";
			ZString expected = ShipmentWrapper.FormatNumber(Shipment.JS_ActualVolume, Env.Registry.VolumeMinimumDecimalPlacesToDisplay);
			AssertEquals("Volume collection", 1, BOLWrapper.ShipmentVolumeCollection.Count);
			AssertEquals("Volume", expected + " M3", BOLWrapper.ShipmentVolumeCollection[0]);

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();
			Shipment.JS_ActualVolume = 123456M;
			Shipment.JS_UnitOfVolume = "L";
			AssertEquals("Volume collection", 2, BOLWrapper.ShipmentVolumeCollection.Count);
			AssertEquals("Volume", expected + " M3", BOLWrapper.ShipmentVolumeCollection[0]);
			AssertEquals("Volume", "(123456 L)", BOLWrapper.ShipmentVolumeCollection[1]);
		}

		public void TestMarksAndNumbersGoodsDescriptionHeaders()
		{
			ZString expectedString = "Marks & Numbers" + BOLWrapper.FillWithSpaces(ShipmentWrapper.MarksAndNumbersWidth - "Marks & Numbers".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.MarksAndNumbersAndDescGap);
			expectedString += "Goods Description" + BOLWrapper.FillWithSpaces(ShipmentWrapper.GoodsDescWidth - "Goods Description".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.GoodsDescriptionAndGrossWeightGap);
			expectedString += "Gross Wt." + BOLWrapper.FillWithSpaces(ShipmentWrapper.GrossWeightWidth - "Gross Wt.".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.GrossWeightAndMeasurementGap);
			expectedString += "Volume" + BOLWrapper.FillWithSpaces(ShipmentWrapper.VolumeMeasurementWidth - "Volume".Length);

			AssertEquals("Marks and numbers, goods description headers", expectedString, BOLWrapper.MarksAndNumbersGoodsDescriptionHeaders);
		}

		public void TestContainersColumnHeaders()
		{
			ZString expectedString = "Container" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberWidth - "Container".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap);
			expectedString += "Seals" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealWidth - "Seals".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealAndTypeGap);
			expectedString += "Type" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeWidth - "Type".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeAndWeightGap);
			expectedString += "Weight(KG)" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightWidth - "Weight(KG)".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightAndVolumeGap);
			expectedString += "Volume(M3)" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerVolumeWidth - "Volume(M3)".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerVolumeAndPackagesGap);
			expectedString += "Packages" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerPackagesWidth - "Packages".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerPackagesAndModeGap);
			expectedString += "Mode" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerModeWidth - "Mode".Length);

			Shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			AssertEquals("Container column headers", expectedString, BOLWrapper.ContainersColumnHeaders);
		}

		public void TestContainersColumnHeadersWithHideSettingsON()
		{
			ZString expectedString = "Container" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberWidth - "Container".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap);
			expectedString += "Seals" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealWidth - "Seals".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealAndTypeGap);
			expectedString += "Type" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeWidth - "Type".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeAndWeightGap);
			expectedString += "Weight(KG)" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightWidth - "Weight(KG)".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightAndTareGap);
			expectedString += "Tare(KG)" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTareWidth - "Tare(KG)".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTareAndGrossGap);
			expectedString += "Gross(KG)" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerGrossWidth - "Gross(KG)".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerGrossAndVolumeGap);
			expectedString += "Volume(M3)" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerVolumeWidth - "Volume(M3)".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerVolumeAndPackagesGap);
			expectedString += "Packages" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerPackagesWidth - "Packages".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerPackagesAndModeGap);
			expectedString += "Mode" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerModeWidth - "Mode".Length);

			Shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			AssertEquals("Container column headers", expectedString, BOLWrapper.ContainersColumnHeaders);

			ResetBOLWrapper();
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerMode, 0);
			expectedString = "Container" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberWidth - "Container".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap);
			expectedString += "Seals" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealWidth - "Seals".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealAndTypeGap);
			expectedString += "Type" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeWidth - "Type".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeAndWeightGap);
			expectedString += "Weight(KG)" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightWidth - "Weight(KG)".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightAndTareGap);
			expectedString += "Tare(KG)" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTareWidth - "Tare(KG)".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTareAndGrossGap);
			expectedString += "Gross(KG)" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerGrossWidth - "Gross(KG)".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerGrossAndVolumeGap);
			expectedString += "Volume(M3)" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerVolumeWidth - "Volume(M3)".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerVolumeAndPackagesGap);
			expectedString += "Packages" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerPackagesWidth - "Packages".Length);
			AssertEquals("Container column headers", expectedString, BOLWrapper.ContainersColumnHeaders);

			ResetBOLWrapper();
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerPackages, 0);
			expectedString = "Container" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberWidth - "Container".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap);
			expectedString += "Seals" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealWidth - "Seals".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealAndTypeGap);
			expectedString += "Type" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeWidth - "Type".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeAndWeightGap);
			expectedString += "Weight(KG)" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightWidth - "Weight(KG)".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightAndTareGap);
			expectedString += "Tare(KG)" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTareWidth - "Tare(KG)".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTareAndGrossGap);
			expectedString += "Gross(KG)" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerGrossWidth - "Gross(KG)".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerGrossAndVolumeGap);
			expectedString += "Volume(M3)" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerVolumeWidth - "Volume(M3)".Length);
			AssertEquals("Container column headers", expectedString, BOLWrapper.ContainersColumnHeaders);

			ResetBOLWrapper();
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerPackages, 0);
			expectedString = "Container" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberWidth - "Container".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap);
			expectedString += "Seals" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealWidth - "Seals".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealAndTypeGap);
			expectedString += "Type" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeWidth - "Type".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeAndWeightGap);
			expectedString += "Weight(KG)" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightWidth - "Weight(KG)".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightAndTareGap);
			expectedString += "Tare(KG)" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTareWidth - "Tare(KG)".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTareAndGrossGap);
			expectedString += "Gross(KG)" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerGrossWidth - "Gross(KG)".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerGrossAndVolumeGap);
			expectedString += "Volume(M3)" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerVolumeWidth - "Volume(M3)".Length);
			AssertEquals("Container column headers", expectedString, BOLWrapper.ContainersColumnHeaders);

			ResetBOLWrapper();
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerVolume, 0);
			expectedString = "Container" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberWidth - "Container".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap);
			expectedString += "Seals" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealWidth - "Seals".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealAndTypeGap);
			expectedString += "Type" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeWidth - "Type".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeAndWeightGap);
			expectedString += "Weight(KG)" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightWidth - "Weight(KG)".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightAndTareGap);
			expectedString += "Tare(KG)" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTareWidth - "Tare(KG)".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTareAndGrossGap);
			expectedString += "Gross(KG)" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerGrossWidth - "Gross(KG)".Length);
			AssertEquals("Container column headers", expectedString, BOLWrapper.ContainersColumnHeaders);

			ResetBOLWrapper();
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerGross, 0);
			expectedString = "Container" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberWidth - "Container".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap);
			expectedString += "Seals" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealWidth - "Seals".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealAndTypeGap);
			expectedString += "Type" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeWidth - "Type".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeAndWeightGap);
			expectedString += "Weight(KG)" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightWidth - "Weight(KG)".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightAndTareGap);
			expectedString += "Tare(KG)" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTareWidth - "Tare(KG)".Length);
			AssertEquals("Container column headers", expectedString, BOLWrapper.ContainersColumnHeaders);

			ResetBOLWrapper();
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerTare, 0);
			expectedString = "Container" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberWidth - "Container".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap);
			expectedString += "Seals" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealWidth - "Seals".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealAndTypeGap);
			expectedString += "Type" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeWidth - "Type".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeAndWeightGap);
			expectedString += "Weight(KG)" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightWidth - "Weight(KG)".Length);
			AssertEquals("Container column headers", expectedString, BOLWrapper.ContainersColumnHeaders);

			ResetBOLWrapper();
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerWeight, 0);
			expectedString = "Container" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberWidth - "Container".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap);
			expectedString += "Seals" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealWidth - "Seals".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealAndTypeGap);
			expectedString += "Type" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeWidth - "Type".Length);
			AssertEquals("Container column headers", expectedString, BOLWrapper.ContainersColumnHeaders);

			ResetBOLWrapper();
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerType, 0);
			expectedString = "Container" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberWidth - "Container".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap);
			expectedString += "Seals" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealWidth - "Seals".Length);
			AssertEquals("Container column headers", expectedString, BOLWrapper.ContainersColumnHeaders);

			ResetBOLWrapper();
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerSeal, 0);
			expectedString = "Container" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberWidth - "Container".Length);
			AssertEquals("Container column headers", expectedString, BOLWrapper.ContainersColumnHeaders);
		}

		public void TestContainersColumnHeaders_ForFCL()
		{
			ZString expectedString = "Container" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberWidth - "Container".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap);
			expectedString += "Seals" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealWidth - "Seals".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealAndTypeGap);
			expectedString += "Type" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeWidth - "Type".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeAndWeightGap);
			expectedString += "Weight(KG)" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightWidth - "Weight(KG)".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightAndTareGap);
			expectedString += "Tare(KG)" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTareWidth - "Tare(KG)".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTareAndGrossGap);
			expectedString += "Gross(KG)" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerGrossWidth - "Gross(KG)".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerGrossAndVolumeGap);
			expectedString += "Volume(M3)" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerVolumeWidth - "Volume(M3)".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerVolumeAndPackagesGap);
			expectedString += "Packages" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerPackagesWidth - "Packages".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerPackagesAndModeGap);
			expectedString += "Mode" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerModeWidth - "Mode".Length);

			Shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			AssertEquals("Container column headers", expectedString, BOLWrapper.ContainersColumnHeaders);
		}

		public void TestContainersColumnHeaders_ForFCL_NoGrossNoTare()
		{
			ZString expectedString = "Container" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberWidth - "Container".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap);
			expectedString += "Seals" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealWidth - "Seals".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealAndTypeGap);
			expectedString += "Type" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeWidth - "Type".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeAndWeightGap);
			expectedString += "Weight(KG)" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightWidth - "Weight(KG)".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightAndVolumeGap);
			expectedString += "Volume(M3)" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerVolumeWidth - "Volume(M3)".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerVolumeAndPackagesGap);
			expectedString += "Packages" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerPackagesWidth - "Packages".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerPackagesAndModeGap);
			expectedString += "Mode" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerModeWidth - "Mode".Length);

			Shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerGross, 0);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerTare, 0);

			AssertEquals("Container column headers", expectedString, BOLWrapper.ContainersColumnHeaders);
		}

		public void TestContainerColumnDetails()
		{
			Shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "20GP"));
			CommonContainer container = Consol.Containers.AddNew();
			container.JC_ContainerNum = "123456";
			container.JC_SealNum = "654321";
			container.JC_RC = containerCode.PK;
			container.JC_DeliveryMode = "bob";

			var packLine = (PackLine)Shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 10;
			packLine.JL_ActualVolume = 22M;
			packLine.JL_ActualWeight = 11M;
			packLine.SetContainer(Consol, container);

			ZDecimal grossWeight = containerCode.RC_TareWeight + packLine.JL_ActualWeight;

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();

			ZString expectedString = "123456" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberWidth - "123456".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap);
			expectedString += "654321" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealWidth - "654321".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealAndTypeGap);
			expectedString += containerCode.RC_Code + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeWidth - containerCode.RC_Code.Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeAndWeightGap);
			expectedString += "11" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightWidth - "11".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightAndTareGap);
			expectedString += containerCode.RC_TareWeight.ToStringTrimZeros() + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTareWidth - containerCode.RC_TareWeight.ToStringTrimZeros().Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTareAndGrossGap);
			expectedString += grossWeight.ToStringTrimZeros() + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerGrossWidth - grossWeight.ToStringTrimZeros().Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerGrossAndVolumeGap);
			expectedString += "22" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerVolumeWidth - "22".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerVolumeAndPackagesGap);
			expectedString += "10 PLT" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerPackagesWidth - "10 PLT".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerPackagesAndModeGap);
			expectedString += "bob" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerModeWidth - "bob".Length);

			AssertMultilineEquals("Container column details", expectedString, BOLWrapper.ContainerColumnsSection[0].ToString(), '\n');
		}

		public void TestContainerColumnDetails_MultipleSeal()
		{
			Shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "20GP"));
			CommonContainer container = Consol.Containers.AddNew();
			container.JC_ContainerNum = "123456";
			container.JC_SealNum = "A123456789a123456789";
			container.JC_AdditionalSealNum = "B123456789b123456789";
			container.JC_Additional2SealNum = "C12345";

			container.JC_RC = containerCode.PK;
			container.JC_DeliveryMode = "bob";

			var packLine = (PackLine)Shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 10;
			packLine.JL_ActualVolume = 22M;
			packLine.JL_ActualWeight = 11M;
			packLine.SetContainer(Consol, container);

			ZDecimal grossWeight = containerCode.RC_TareWeight + packLine.JL_ActualWeight;

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();

			ZString expectedString = "123456" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberWidth - "123456".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap);
			expectedString += "A123456789a123456789" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealWidth - "A123456789a123456789".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealAndTypeGap);
			expectedString += containerCode.RC_Code + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeWidth - containerCode.RC_Code.Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeAndWeightGap);
			expectedString += "11" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightWidth - "11".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightAndTareGap);
			expectedString += containerCode.RC_TareWeight.ToStringTrimZeros() + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTareWidth - containerCode.RC_TareWeight.ToStringTrimZeros().Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTareAndGrossGap);
			expectedString += grossWeight.ToStringTrimZeros() + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerGrossWidth - grossWeight.ToStringTrimZeros().Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerGrossAndVolumeGap);
			expectedString += "22" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerVolumeWidth - "22".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerVolumeAndPackagesGap);
			expectedString += "10 PLT" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerPackagesWidth - "10 PLT".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerPackagesAndModeGap);
			expectedString += "bob" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerModeWidth - "bob".Length);

			AssertMultilineEquals("Container column details", expectedString, BOLWrapper.ContainerColumnsSection[0].ToString(), '\n');
			AssertMultilineEquals("Container column details", "                , B123456789b1234567", BOLWrapper.ContainerColumnsSection[1].ToString(), '\n');
			AssertMultilineEquals("Container column details", "                89, C12345          ", BOLWrapper.ContainerColumnsSection[2].ToString(), '\n');
		}

		public void TestContainerColumnDetails_ForFCL_NoTareNoGross()
		{
			Shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "20NOR"));
			CommonContainer container = Consol.Containers.AddNew();
			container.JC_ContainerNum = "123456";
			container.JC_SealNum = "654321";
			container.JC_RC = containerCode.PK;
			container.JC_DeliveryMode = "bob";

			var packLine = (PackLine)Shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 10;
			packLine.JL_ActualVolume = 22M;
			packLine.JL_ActualWeight = 11M;
			packLine.SetContainer(Consol, container);

			ZDecimal grossWeight = containerCode.RC_TareWeight + packLine.JL_ActualWeight;

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerGross, 0);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerTare, 0);
			ResetBOLWrapper();

			ZString expectedString = "123456" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberWidth - "123456".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap);
			expectedString += "654321" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealWidth - "654321".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerSealAndTypeGap);
			expectedString += containerCode.RC_Code + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeWidth - containerCode.RC_Code.Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeAndWeightGap);
			expectedString += "11" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightWidth - "11".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightAndVolumeGap);
			expectedString += "22" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerVolumeWidth - "22".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerVolumeAndPackagesGap);
			expectedString += "10 PLT" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerPackagesWidth - "10 PLT".Length);
			expectedString += BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerPackagesAndModeGap);
			expectedString += "bob" + BOLWrapper.FillWithSpaces(ShipmentWrapper.ContainerModeWidth - "bob".Length);

			AssertMultilineEquals("Container column details", expectedString, BOLWrapper.ContainerColumnsSection[0].ToString(), '\n');
		}

		public void TestContainerColumnDetails_ForFCL_NonStandardUnits()
		{
			Shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "20NOR"));
			CommonContainer container = Consol.Containers.AddNew();
			container.JC_ContainerNum = "123456";
			container.JC_SealNum = "654321";
			container.JC_RC = containerCode.PK;
			container.JC_DeliveryMode = "bob";
			container.JC_GrossWeightUQ = Core.Constants.Weight.Pounds;
			container.JC_GrossWeight = 1000m;
			container.JC_TareWeight = 300m;

			var packLine = (PackLine)Shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 10;
			packLine.JL_ActualVolume = 22M;
			packLine.JL_ActualWeight = 11M;
			packLine.SetContainer(Consol, container);

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();
			AssertContains("Weight in kg", "136.078", BOLWrapper.ContainerColumnsSection[0].ToString());
		}

		public void TestConvertToStringCollection()
		{
			ZString testString = "blah"; // single word
			ZString expectedString = "blah";
			AssertEquals(1, BOLWrapper.ConvertToStringCollection(testString, 4).Count);
			AssertEquals(expectedString, BOLWrapper.ConvertToStringCollection(testString, 4)[0]);

			testString = "blah blah"; // spaces
			expectedString = "blah";
			AssertEquals(2, BOLWrapper.ConvertToStringCollection(testString, 4).Count);
			AssertEquals(expectedString, BOLWrapper.ConvertToStringCollection(testString, 4)[0]);
			AssertEquals(expectedString, BOLWrapper.ConvertToStringCollection(testString, 4)[1]);

			testString = "blah\nblah"; // new line
			expectedString = "blah";
			AssertEquals(2, BOLWrapper.ConvertToStringCollection(testString, 4).Count);
			AssertEquals(expectedString, BOLWrapper.ConvertToStringCollection(testString, 4)[0]);
			AssertEquals(expectedString, BOLWrapper.ConvertToStringCollection(testString, 4)[1]);

			testString = "blah\r\nblah"; // new line
			expectedString = "blah";
			AssertEquals(2, BOLWrapper.ConvertToStringCollection(testString, 4).Count);
			AssertEquals(expectedString, BOLWrapper.ConvertToStringCollection(testString, 4)[0]);
			AssertEquals(expectedString, BOLWrapper.ConvertToStringCollection(testString, 4)[1]);

			testString = "blahblah"; // long word
			expectedString = "blah";
			AssertEquals(2, BOLWrapper.ConvertToStringCollection(testString, 4).Count);
			AssertEquals(expectedString, BOLWrapper.ConvertToStringCollection(testString, 4)[0]);
			AssertEquals(expectedString, BOLWrapper.ConvertToStringCollection(testString, 4)[1]);

			testString = "blah\tblah"; // tabs
			expectedString = "blah";
			AssertEquals(2, BOLWrapper.ConvertToStringCollection(testString, 4).Count);
			AssertEquals(expectedString, BOLWrapper.ConvertToStringCollection(testString, 4)[0]);
			AssertEquals(expectedString, BOLWrapper.ConvertToStringCollection(testString, 4)[1]);
		}

		public void TestAlignToWidth()
		{
			AssertEquals("If width is smaller than the text the crop the text to width", "bl", BOLWrapper.AlignToWidth("blah", 2));
			AssertEquals("blah      ", BOLWrapper.AlignToWidth("blah", 10));
		}

		public void TestContainersFromNonMasterCoLoad()
		{
			//var ContainerCode = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "40FR"));
			CommonContainer container = Consol.Containers.AddNew();
			container.JC_ContainerNum = "Container number 1";
			container.JC_DeliveryMode = "bob";

			var shipPackLine = (PackLine)Shipment.OuterPackLines.AddNew();
			shipPackLine.JL_PackageCount = 10;
			shipPackLine.JL_ActualVolume = 10M;
			shipPackLine.JL_ActualWeight = 10M;
			shipPackLine.SetContainer(Consol, container);

			shipPackLine = Shipment.OuterPackLines.AddNew();
			shipPackLine.JL_PackageCount = 10;
			shipPackLine.JL_ActualVolume = 10M;
			shipPackLine.JL_ActualWeight = 10M;
			shipPackLine.SetContainer(Consol, container);

			AssertEquals("Should contain one container with the sum of 2 pack lines", 1, BOLWrapper.Containers.Count);
			AssertEquals("Should have 10 x 2 package counts", 20, BOLWrapper.Containers[0].Packs);
			AssertEquals("Should have 10 x 2 weights", 20M, BOLWrapper.Containers[0].Weight);
			AssertEquals("Should have 10 x 2 volumes", 20M, BOLWrapper.Containers[0].Volume);

			//			ContainerCode = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.NotEqual, "40FR"));
			container = Consol.Containers.AddNew();
			container.JC_ContainerNum = "Container number 2";
			container.JC_DeliveryMode = "boo";

			shipPackLine = Shipment.OuterPackLines.AddNew();
			shipPackLine.JL_PackageCount = 11;
			shipPackLine.JL_ActualVolume = 11M;
			shipPackLine.JL_ActualWeight = 11M;
			shipPackLine.SetContainer(Consol, container);

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();

			AssertEquals("Should contain two containers", 2, BOLWrapper.Containers.Count);
			AssertEquals("package counts", 11, BOLWrapper.Containers[1].Packs);
			AssertEquals("weights", 11M, BOLWrapper.Containers[1].Weight);
			AssertEquals("volumes", 11M, BOLWrapper.Containers[1].Volume);

			AssertEquals("delivery mode", "bob", BOLWrapper.Containers[0].DeliveryMode);
			AssertEquals("delivery mode", "boo", BOLWrapper.Containers[1].DeliveryMode);

			Shipment.JS_HBLContainerPackModeOverride = "blah";
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();

			AssertEquals("delivery mode", "blah", BOLWrapper.Containers[0].DeliveryMode);
			AssertEquals("delivery mode", "blah", BOLWrapper.Containers[1].DeliveryMode);
		}

		public void TestContainersFromMasterCoLoad()
		{
			Shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			CommonContainer container = Consol.Containers.AddNew();
			container.JC_DeliveryMode = "bob";
			container.JC_ContainerNum = "container number 1";

			var subShipment = Factory.New<ForwardingShipment>();
			subShipment.JS_JS_ColoadMasterShipment = Shipment.PK;

			var shipPackLine = (PackLine)subShipment.OuterPackLines.AddNew();
			shipPackLine.JL_ActualWeight = 10M;
			shipPackLine.JL_ActualVolume = 10M;
			shipPackLine.JL_PackageCount = 10;
			shipPackLine.SetContainer(Consol, container);

			subShipment = Factory.New<ForwardingShipment>();
			subShipment.JS_JS_ColoadMasterShipment = Shipment.PK;

			shipPackLine = subShipment.OuterPackLines.AddNew();
			shipPackLine.JL_ActualWeight = 1M;
			shipPackLine.JL_ActualVolume = 1M;
			shipPackLine.JL_PackageCount = 1;
			shipPackLine.SetContainer(Consol, container);

			AssertEquals("Should only have one container", 1, BOLWrapper.Containers.Count);
			AssertEquals("package counts", 11, BOLWrapper.Containers[0].Packs);
			AssertEquals("weights", 11M, BOLWrapper.Containers[0].Weight);
			AssertEquals("volumes", 11M, BOLWrapper.Containers[0].Volume);

			container = Consol.Containers.AddNew();
			container.JC_DeliveryMode = "boo";
			container.JC_ContainerNum = "container number 2";

			shipPackLine = subShipment.OuterPackLines.AddNew();
			shipPackLine.JL_ActualWeight = 3M;
			shipPackLine.JL_ActualVolume = 3M;
			shipPackLine.JL_PackageCount = 3;
			shipPackLine.SetContainer(Consol, container);

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();

			AssertEquals("Should have two containers", 2, BOLWrapper.Containers.Count);
			AssertEquals("package counts", 11, BOLWrapper.Containers[0].Packs);
			AssertEquals("weights", 11M, BOLWrapper.Containers[0].Weight);
			AssertEquals("volumes", 11M, BOLWrapper.Containers[0].Volume);
			AssertEquals("package counts", 3, BOLWrapper.Containers[1].Packs);
			AssertEquals("weights", 3M, BOLWrapper.Containers[1].Weight);
			AssertEquals("volumes", 3M, BOLWrapper.Containers[1].Volume);

			Shipment.JS_HBLContainerPackModeOverride = "blah";
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();

			AssertEquals("delivery mode", "blah", BOLWrapper.Containers[0].DeliveryMode);
			AssertEquals("delivery mode", "blah", BOLWrapper.Containers[1].DeliveryMode);
		}
		#endregion

		#endregion

		#region BodySection

		public void AddAContainerToShipment()
		{
			var containerCode1 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "20NOR"));
			var shipPackLine1 = (PackLine)Shipment.OuterPackLines.AddNew();
			CommonContainer container1 = Consol.Containers.AddNew();
			container1.JC_RC = containerCode1.PK;
			shipPackLine1.SetContainer(Consol, container1);
			Factory.Save();
		}

		public void TestBodySections()
		{
			AssertEquals("BodySections: Empty", 0, BOLWrapper.BodySections.Count);

			AddAContainerToShipment();
			AddAContainerToShipment();
			AddAContainerToShipment();
			AddAContainerToShipment();
			AddAContainerToShipment();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 2);
			ResetBOLWrapper();

			AssertEquals("BodySections: Count - Derived from the bottom part of BodySection", 3, BOLWrapper.BodySections.Count);

			Shipment = Factory.New<ForwardingShipment>();
			Consol = Shipment.Consols.AddNew();

			AddMarksAndNumbersToShipment();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersWidth, 10);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 2);
			ResetBOLWrapper();

			AssertEquals("BodySections: Count - Derived from the top part of BodySection", 2, BOLWrapper.BodySections.Count);

			Shipment = Factory.New<ForwardingShipment>();
			Consol = Shipment.Consols.AddNew();

			AddMarksAndNumbersToShipment();
			AddAContainerToShipment();
			AddAContainerToShipment();
			AddAContainerToShipment();
			AddAContainerToShipment();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersWidth, 10);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 2);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 1);

			ResetBOLWrapper();

			AssertEquals("BodySections: Count - Derived from both top and bottom part of BodySection", 4, BOLWrapper.BodySections.Count);
		}

		public void TestExtractBlock()
		{
			ZString testStringSource1 = "blah1\nblah2\nblah3\nblah4";
			ZString testStringSource2 = "";
			ZString testStringSource3 = "blahblah";
			ZString testStringSource4 = "bleh\n";

			AssertEquals("ExtractBlock: Extract string from source1", new ZString("blah1\nblah2\nblah3\n"), BOLWrapper.ExtractBlock(ref testStringSource1, 3));
			AssertEquals("ExtractBlock: Source1 after extraction", new ZString("blah4"), testStringSource1);

			AssertEquals("ExtractBlock: Extract string from source2", ZString.Empty, BOLWrapper.ExtractBlock(ref testStringSource2, 5));
			AssertEquals("ExtractBlock: Source2 after extraction", ZString.Empty, testStringSource2);

			AssertEquals("ExtractBlock: Extract string from source3", new ZString("blahblah\n"), BOLWrapper.ExtractBlock(ref testStringSource3, 5));
			AssertEquals("ExtractBlock: Source3 after extraction", ZString.Empty, testStringSource3);

			AssertEquals("ExtractBlock: Extract string from source4", new ZString("bleh\n"), BOLWrapper.ExtractBlock(ref testStringSource4, 8));
			AssertEquals("ExtractBlock: Source4 after extraction", ZString.Empty, testStringSource4);
		}

		#endregion

		#region BOL Fields

		public void TestOnForwardingPortOfLoading()
		{
			AssertEquals("OnForwardingPortOfLoading", "", BOLWrapper.OnForwardingPortOfLoading);

			Transport transport1 = Consol.Transports.AddNew();
			AssertEquals("OnForwardingVesselVoyage", "", BOLWrapper.OnForwardingVesselVoyage);

			transport1.JW_TransportType = OnForwardingType;
			transport1.JW_RL_NKLoadPort = "AUMEL";
			transport1.JW_RL_NKDiscPort = "SGSIN";
			AssertEquals("OnForwardingPortOfLoading", "MELBOURNE, AUSTRALIA", BOLWrapper.OnForwardingPortOfLoading);

			Transport transport2 = Shipment.Transports.AddNew();
			transport2.JW_TransportType = OnForwardingType;
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "JPOSA";
			AssertEquals("OnForwardingPortOfLoading", "SINGAPORE, SINGAPORE", BOLWrapper.OnForwardingPortOfLoading);

			transport2.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			AssertEquals("OnForwardingPortOfLoading", "MELBOURNE, AUSTRALIA", BOLWrapper.OnForwardingPortOfLoading);
		}

		public void TestHouseBillNumber()
		{
			Shipment.JS_HouseBill = "1234";
			AssertEquals("House bill number", Shipment.JS_HouseBill, BOLWrapper.HouseBillNumber);

			Shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals("Master House bill number", "MASTER HBL: " + Shipment.JS_HouseBill, BOLWrapper.HouseBillNumber);

			Shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertEquals("House bill number", Shipment.JS_HouseBill, BOLWrapper.HouseBillNumber);

			Shipment.CoLoadShipments.AddNew();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();
			AssertEquals("Master House bill number", "MASTER HBL: " + Shipment.JS_HouseBill, BOLWrapper.HouseBillNumber);

			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ShowMasterHeadingWithBillNumber, 0);
			AssertEquals("Master House bill number", Shipment.JS_HouseBill, BOLWrapper.HouseBillNumber);

			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ShowMasterHeadingWithBillNumber, 1);
			AssertEquals("Master House bill number", "MASTER HBL: " + Shipment.JS_HouseBill, BOLWrapper.HouseBillNumber);
		}

		public void TestMasterHouseBillNumberHeading()
		{
			Shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			Shipment.CoLoadShipments.RemoveAll();
			AssertEquals("Master House bill number heading", "MASTER HBL: ", BOLWrapper.MasterHouseBillNumberHeading);

			Shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertEquals("Master House bill number heading", ZString.Empty, BOLWrapper.MasterHouseBillNumberHeading);

			Shipment.CoLoadShipments.AddNew();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();
			AssertEquals("Master House bill number heading", "MASTER HBL: ", BOLWrapper.MasterHouseBillNumberHeading);
		}

		public void TestHBLTypeDescription()
		{
			Shipment.JS_HouseBillOfLadingType = ZString.Empty;
			AssertEquals(ZString.Empty, BOLWrapper.HBLTypeDescription);

			Shipment.JS_HouseBillOfLadingType = "TTL";
			ZString expected = Shipment.Lookups.JS_HouseBillOfLadingType_List.GetDescriptionFromCode(Shipment.JS_HouseBillOfLadingType);
			AssertEquals(expected, BOLWrapper.HBLTypeDescription);
		}

		public void TestWeight()
		{
			AssertEquals("Weight", ZString.Empty, BOLWrapper.Weight);

			Shipment.JS_ActualWeight = 123.456M;
			Shipment.JS_UnitOfWeight = "KG";
			ZString expected = ShipmentWrapper.FormatNumber(Shipment.JS_ActualWeight, Env.Registry.WeightMinimumDecimalPlacesToDisplay);
			AssertEquals("Weight", expected + " KG\n", BOLWrapper.Weight);

			Shipment.JS_ActualWeight = 123456.000M;
			Shipment.JS_UnitOfWeight = "G";
			AssertEquals("Weight", expected + " KG\n(123456 G)\n", BOLWrapper.Weight);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ConvertUnits, "N");
			ShipmentWrapper.SetTemplateConstants(constants);

			AssertEquals("No conversion", "123456 G\n", BOLWrapper.Weight);
		}

		public void TestVolume()
		{
			AssertEquals("Volume", ZString.Empty, BOLWrapper.Volume);

			Shipment.JS_ActualVolume = 123.456M;
			Shipment.JS_UnitOfVolume = "M3";
			ZString expected = ShipmentWrapper.FormatNumber(Shipment.JS_ActualVolume, Env.Registry.VolumeMinimumDecimalPlacesToDisplay);
			AssertEquals("Volume", expected + " M3\n", BOLWrapper.Volume);

			Shipment.JS_ActualVolume = 123456.000M;
			Shipment.JS_UnitOfVolume = "L";
			AssertEquals("Volume", expected + " M3\n(123456 L)\n", BOLWrapper.Volume);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ConvertUnits, "N");
			ShipmentWrapper.SetTemplateConstants(constants);

			AssertEquals("No conversion", "123456 L\n", BOLWrapper.Volume);
		}

		public void TestDeliveryAgent()
		{
			BOLWrapper.ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));

			AssertNull("Delivery agent is null", BOLWrapper.DeliveryAgent);

			OrgHeader receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader receivingForwarder2 = Factory.NewWithValidTestData<OrgHeader>();
			Consol.SetDefaultReceivingForwarderAddress(receivingForwarder);

			ResetWrappers();
			AssertNotNull("Delivery agent is not null", BOLWrapper.DeliveryAgent);
			AssertEquals("Delivery agent is of type DocOrganisation", typeof(DocOrganisation), BOLWrapper.DeliveryAgent.GetType());
			AssertEquals("Delivery agent = receivingForwarder", receivingForwarder.OH_Code, BOLWrapper.DeliveryAgent.Code);

			CommonConsol arrivalConsol = Shipment.Consols.AddNew();
			arrivalConsol.JK_RL_NKLoadPort = Consol.JK_RL_NKDischargePort;
			arrivalConsol.SetDefaultReceivingForwarderAddress(receivingForwarder2);

			ResetWrappers();
			AssertNotNull("It will pick the Consol. Ports are empty and I cannot predict which one exactly", BOLWrapper.DeliveryAgent);
			AssertEquals("Delivery agent is of type DocOrganisation", typeof(DocOrganisation), BOLWrapper.DeliveryAgent.GetType());

			Consol.JK_RL_NKLoadPort = "AUSYD";
			Consol.JK_RL_NKDischargePort = "SGSIN";
			arrivalConsol.JK_RL_NKLoadPort = "SGSIN";
			arrivalConsol.JK_RL_NKDischargePort = "USLAX";
			Consol.SetDefaultReceivingForwarderAddress(receivingForwarder);
			arrivalConsol.SetDefaultReceivingForwarderAddress(receivingForwarder2);

			ResetWrappers();
			AssertNotNull("Delivery agent is not null", BOLWrapper.DeliveryAgent);
			AssertEquals("Delivery agent is of type DocOrganisation", typeof(DocOrganisation), BOLWrapper.DeliveryAgent.GetType());
			AssertEquals("Delivery agent = receivingForwarder2", receivingForwarder2.OH_Code, BOLWrapper.DeliveryAgent.Code);

			OrgHeader deliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			Shipment.JS_OH_DeliveryAgent = deliveryAgent.PK;

			ResetWrappers();
			AssertNotNull("Delivery agent is not null", BOLWrapper.DeliveryAgent);
			AssertEquals("Delivery agent is of type DocOrganisation", typeof(DocOrganisation), BOLWrapper.DeliveryAgent.GetType());
			AssertEquals("Delivery agent = deliveryAgent", deliveryAgent.OH_Code, BOLWrapper.DeliveryAgent.Code);
		}

		public void TestNotifyParty()
		{
			AssertEquals("Notify Party is taken from registry", Env.Registry.NotifyPartyDefaultText, BOLWrapper.NotifyParty);

			var header = Factory.New<OrgHeader>();
			header.MainAddress.OA_Address1 = "address 1";
			header.OH_Code = "CCC";
			Shipment.ConsigneePK = header.PK;

			var notifyParty = Factory.New<OrgContact>();
			notifyParty.OC_ContactName = "Andrew Smith";
			notifyParty.OC_OH = header.PK;
			Shipment.NotifyPartyDocumentaryAddress.ContactPK = notifyParty.PK;
			ResetBOLWrapper();
			AssertEquals("Notify Party from shipment", ShipmentWrapper.NotifyParty.PostalAddress, BOLWrapper.NotifyParty);
		}

		public void TestSendingForwarderAddress()
		{
			CommonConsol consol = Shipment.Consols.AddNew();
			OrgHeader sendingForwarder = Factory.New<OrgHeader>();
			consol.SetDefaultSendingForwarderAddress(sendingForwarder);

			var glbBranchAddress = GlbBranch.CurrentBranch.OrgProxy.Addresses.MainAddress;
			glbBranchAddress.OA_OH = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			var wrapper = DocDocAddress.New(glbBranchAddress, Factory);

			FreightDataRegistry.Instance.BOLSendingForwarderCurrentBranchOrgProxy.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(wrapper.Address1, glbBranchAddress.OA_Address1);
			AssertEquals("SendingForwarderAddress is of type DocDocAddress", typeof(DocDocAddress), ShipmentWrapper.BillOfLading.SendingForwarderAddress.GetType());

			FreightDataRegistry.Instance.BOLSendingForwarderCurrentBranchOrgProxy.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertNotEquals(wrapper.Address1, glbBranchAddress.OA_Address2);
			AssertNotNull(GlbBranch.CurrentBranch.OrgProxy);
		}

		public void TestCountOfContainers()
		{
			AssertEquals("CountOfContainers", 0, BOLWrapper.CountOfContainers);

			var containerCode20NR = Factory.LoadTop1<RefContainer>(new ZQuery());

			#region Container 1 - 20NR

			PackLine shipPackLine1 = Shipment.OuterPackLines.AddNew();

			CommonContainer container1 = Consol.Containers.AddNew();
			container1.JC_RC = containerCode20NR.PK;
			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			shipPackLine1.SetContainer(Consol, container1);

			Factory.Save();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();
			AssertEquals("CountOfContainers", 1, BOLWrapper.CountOfContainers);

			#endregion

			#region Container 2 40FR

			var containerCode1 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "40FR"));
			PackLine shipPackLine2 = Shipment.OuterPackLines.AddNew();
			CommonContainer container2 = Consol.Containers.AddNew();
			container2.JC_RC = containerCode1.PK;
			container2.JC_ContainerNum = "Container 2";
			container2.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			shipPackLine2.SetContainer(Consol, container2);

			Factory.Save();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();
			AssertEquals("CountOfContainers", 2, BOLWrapper.CountOfContainers);

			#endregion

			#region Container 3 - 20NR

			PackLine shipPackLine3 = Shipment.OuterPackLines.AddNew();
			CommonContainer container3 = Consol.Containers.AddNew();
			container3.JC_RC = containerCode20NR.PK;
			container3.JC_ContainerNum = "Container 3";
			container3.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			shipPackLine3.SetContainer(Consol, container3);

			Factory.Save();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();
			AssertEquals("CountOfContainers", 3, BOLWrapper.CountOfContainers);

			#endregion

			AssertContainsExactElementsInAnyOrder(new ZString[] { "2 x " + containerCode20NR.RC_Code + " CONTAINER", "1 x 40FR CONTAINER" }, BOLWrapper.GetContainerTypeCountTestMethod().TrimEnd().Split('\n'));
		}

		public void TestCountOfLCLPackages()
		{
			Shipment.JS_OuterPacks = 0;
			DocBillOfLadingTestClass bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("CountOfLCLPackages", 0, bOL.CountOfLCLPackages);

			Shipment.JS_OuterPacks = 120;
			AssertEquals("CountOfLCLPackages", 120, bOL.CountOfLCLPackages);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("CountOfLCLPackages", 120, bOL.CountOfLCLPackages);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			var shipPackLine = (PackLine)Shipment.OuterPackLines[0];
			shipPackLine.JL_PackageCount = 120;
			CommonContainer container = Consol.Containers.AddNew();
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			shipPackLine.SetContainer(Consol, container);
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("CountOfLCLPackages", 0, bOL.CountOfLCLPackages);

			shipPackLine.JL_PackageCount = 200;
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("CountOfLCLPackages", 0, bOL.CountOfLCLPackages);

			shipPackLine.JL_PackageCount = 100;
			Factory.Save();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("CountOfLCLPackages", 20, bOL.CountOfLCLPackages);

			var shipPackLine2 = (PackLine)Shipment.OuterPackLines.AddNew();
			shipPackLine2.JL_PackageCount = 120;
			CommonContainer container2 = Consol.Containers.AddNew();
			container2.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			shipPackLine2.SetContainer(Consol, container2);
			Factory.Save();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("CountOfLCLPackages", 20, bOL.CountOfLCLPackages);
		}

		public void TestIssuedBy()
		{
			AssertEquals("Current company branch UNLOCO", GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort, BOLWrapper.IssuedByUNLOCO.Code);
			AssertEquals("Current company name", GlbCompany.CurrentCompany.OrgProxy.OH_FullName.ToUpper(), BOLWrapper.IssuedByName);
			AssertEquals("Current company address line 1", GlbCompany.CurrentCompany.OrgProxy.MainAddress.OA_Address1.ToUpper(), BOLWrapper.IssuedByAddress1);
			AssertEquals("Current company address line 2", GlbCompany.CurrentCompany.OrgProxy.MainAddress.OA_Address2.ToUpper(), BOLWrapper.IssuedByAddress2);
			AssertEquals("Current company city", GlbCompany.CurrentCompany.OrgProxy.MainAddress.OA_City.ToUpper(), BOLWrapper.IssuedByCity);
			AssertEquals("Current company state", GlbCompany.CurrentCompany.OrgProxy.MainAddress.OA_State.ToUpper(), BOLWrapper.IssuedByState);
			AssertEquals("Current company post code", GlbCompany.CurrentCompany.OrgProxy.MainAddress.OA_PostCode.ToUpper(), BOLWrapper.IssuedByPostCode);

			string port = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;

			GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort = "USLAX";
			AssertEquals("Takes current company orgproxy", "USLAX", BOLWrapper.IssuedByUNLOCO.Code);

			GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort = port;
		}

		public void TestIssuedByUNLOCOIsNull()
		{
			var temporaryCompany = Factory.New<GlbCompany>();
			var temporaryBranch = Factory.New<GlbBranch>();
			var temporaryDepartment = Factory.New<GlbDepartment>();
			temporaryCompany.Branches.Add(temporaryBranch);

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, temporaryBranch.PK.ToGuid(), temporaryDepartment.PK.ToGuid()))
			{
				SetUp();

				AssertNull("IssuedByUNLOCO should be null", BOLWrapper.IssuedByUNLOCO);
				AssertEquals("IssuedByCountry should be empty", string.Empty, BOLWrapper.IssuedByCountry);
			}
		}

		public void TestLawAndJurisdictionClauseForITC()
		{
			Shipment.JS_HouseBillOfLadingType = "IAU";
			string port = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;

			GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort = "";
			AssertEquals("No clause", "", BOLWrapper.LawAndJurisdictionClauseForITC);

			GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort = "AUBNE";
			ZString expected = "The Contract evidenced by or contained in this " + BOLWrapper.Title + " shall be governed by Australian law and any claim or dispute arising hereunder or in connection herewith shall (without prejudice to the Carrier's rights to commence proceedings in any other jurisdiction) be subject to the jurisdiction of the Courts of Australia.";
			AssertEquals("Australian clause", expected, BOLWrapper.LawAndJurisdictionClauseForITC);

			GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort = "HKHKG";
			expected = "The Contract evidenced by or contained in this " + BOLWrapper.Title + " shall be governed by the law in Hong Kong and any claim or dispute arising hereunder or in connection herewith shall (without prejudice to the Carrier's rights to commence proceedings in any other jurisdiction) be subject to the jurisdiction of the Courts of Hong Kong.";
			AssertEquals("Hong Kong clause", expected, BOLWrapper.LawAndJurisdictionClauseForITC);

			GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort = port;

			Shipment.JS_HouseBillOfLadingType = "INN";
			AssertEquals("This HBL Type requires no ITC Law clause", ZString.Empty, BOLWrapper.LawAndJurisdictionClauseForITC);

			Shipment.JS_HouseBillOfLadingType = "ITP";
			AssertEquals("This HBL Type requires no ITC Law clause", ZString.Empty, BOLWrapper.LawAndJurisdictionClauseForITC);
		}

		public void TestLawAndJurisdictionClauseForTTC()
		{
			Shipment.JS_HouseBillOfLadingType = "TTC";
			string port = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;

			GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort = "";
			AssertEquals("No clause", "", BOLWrapper.LawAndJurisdictionClauseForTTC);

			GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort = "AUBNE";
			ZString expected = "JURISDICTION AND LAW CLAUSE - If this " + BOLWrapper.Title + " is issued in Australia, the contract evidenced by or contained herein shall be governed by the law of the State or Territory in which it is issued and any claim or dispute arising hereunder or in connection herewith shall at the Carriers sole option be determined by the Courts of that State or Territory & no other Court. In all other cases any such claim or dispute shall be determined at the Carriers sole option either in the place where this " + BOLWrapper.Title + " is issued (& subject to the laws of that place) or at the place where the Carrier has its principal place of business (and subject to the laws of that place).";
			AssertEquals("Australian clause", expected, BOLWrapper.LawAndJurisdictionClauseForTTC);

			GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort = "HKHKG";
			expected = "JURISDICTION AND LAW CLAUSE - The contract evidenced by or contained in this " + BOLWrapper.Title + " is governed by the law of Hong Kong and any claim or dispute arising hereunder or in connection herewith shall be determined by the Courts in Hong Kong and no other Court.";
			AssertEquals("Hong Kong clause", expected, BOLWrapper.LawAndJurisdictionClauseForTTC);

			Shipment.JS_HouseBillOfLadingType = "TTP";
			AssertEquals("No clause", "", BOLWrapper.LawAndJurisdictionClauseForTTC);

			GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort = port;
		}

		public void TestContainerModeOverride()
		{
			var containerCode1 = Factory.LoadTop1<RefContainer>(new ZQuery());
			var shipPackLine1 = (PackLine)Shipment.OuterPackLines.AddNew();
			CommonContainer container1 = Consol.Containers.AddNew();
			container1.JC_RC = containerCode1.PK;
			container1.JC_ContainerNum = "Container";
			container1.JC_DeliveryMode = "CY/CY";
			shipPackLine1.SetContainer(Consol, container1);

			var shipPackLine3 = (PackLine)Shipment.OuterPackLines.AddNew();
			CommonContainer container2 = Consol.Containers.AddNew();
			container2.JC_RC = containerCode1.PK;
			container2.JC_ContainerNum = "Container2";
			container2.JC_DeliveryMode = "CFS";
			shipPackLine3.SetContainer(Consol, container2);

			var shipPackLine4 = (PackLine)Shipment.OuterPackLines.AddNew();
			CommonContainer container3 = Consol.Containers.AddNew();
			container3.JC_RC = containerCode1.PK;
			container3.JC_ContainerNum = "Container3";
			shipPackLine4.SetContainer(Consol, container3);
			Factory.Save();

			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 4);
			ResetBOLWrapper();
			ZString expectedResult = container1.JC_DeliveryMode + "*\n" + container2.JC_DeliveryMode + "\n-" + "\n";
			ZString containerColumn = BOLWrapper.ContainerNumberColumn;
			Assert("Container Number column is not empty", !containerColumn.IsEmpty);
			AssertEquals("Container Mode", expectedResult, BOLWrapper.ContainerModeColumn);

			Shipment.JS_HBLContainerPackModeOverride = "CY/CFS";
			//Factory.Save();
			ResetBOLWrapper();
			expectedResult = Shipment.JS_HBLContainerPackModeOverride + "*\n" + Shipment.JS_HBLContainerPackModeOverride + "*\n" + Shipment.JS_HBLContainerPackModeOverride + "*\n";
			containerColumn = BOLWrapper.ContainerNumberColumn;
			Assert("Container Number column is not empty", !containerColumn.IsEmpty);
			AssertEquals("Container Mode has been overriden", expectedResult, BOLWrapper.ContainerModeColumn);
		}

		public void TestShipperLoadAndCountDefault()
		{
			AssertEquals("Registry default", FreightDataRegistry.Instance.ShipperLoadAndCount.Value, BOLWrapper.ShipperLoadAndCountDefault);
		}

		public void TestMiniBillNamePreprinted()
		{
			Shipment.JS_HouseBillOfLadingType = "EAP";
			AssertEquals("Mini Bill Name Pre-printed", "(PP)", BOLWrapper.MiniBillNamePreprinted);

			Shipment.JS_HouseBillOfLadingType = "EAG";
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			Consol = Shipment.Consols.AddNew();
			ResetBOLWrapper();
			AssertEquals("Mini Bill Name Pre-printed", ZString.Empty, BOLWrapper.MiniBillNamePreprinted);
		}

		public void TestContainerized()
		{
			AssertEquals("Show as Containerized", ZBool.False, BOLWrapper.Containerized);
			AssertEquals("Show as Containerized", "N", BOLWrapper.Containerized.ToString());

			Shipment.JS_PackingMode = "FCL";
			AssertEquals("Show as Containerized", "Y", BOLWrapper.Containerized.ToString());

			Shipment.JS_PackingMode = "LCL";
			AssertEquals("Show as Containerized", "N", BOLWrapper.Containerized.ToString());

			Shipment.JS_PackingMode = "BCN";
			AssertEquals("Show as Containerized", "Y", BOLWrapper.Containerized.ToString());
		}

		public void TestInterimReceipt()
		{
			Shipment.JS_InterimReceipt = "blah";
			AssertEquals("Shipment Interim Receipt", "blah", BOLWrapper.InterimReceipt);

			Shipment.JS_InterimReceipt = "blah2";
			AssertEquals("Shipment Interim Receipt", "blah2", BOLWrapper.InterimReceipt);
		}

		public void TestTitle()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";
			Shipment.JS_HouseBillOfLadingType = "IAU";
			FreightDataRegistry.Instance.EnableHouseBillOfLadingRegistryItems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			HouseBillOfLadingTypeCollection hBLRegistryObjectCollection = FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.Value;
			HouseBillOfLadingType hBLRegistryObject = hBLRegistryObjectCollection.FindByCode("IAU") as HouseBillOfLadingType;
			hBLRegistryObject.PrePrinted = ZBool.True;
			FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, hBLRegistryObjectCollection);
			AssertEquals("IsPreprinted true", ZBool.True, BOLWrapper.IsPreprinted);
			AssertEquals("Title is empty", ZString.Empty, BOLWrapper.Title);

			hBLRegistryObject.PrePrinted = ZBool.False;
			FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, hBLRegistryObjectCollection);
			ResetBOLWrapper();
			AssertEquals("Title is HBL", DocConstants.Resources.BillTitles.HBL, BOLWrapper.Title);

			Shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.SeaWaybill;
			ResetBOLWrapper();
			AssertEquals("Title is SWB", DocConstants.Resources.BillTitles.SWB, BOLWrapper.Title);
		}

		public void TestBillSurrenderedToHeading()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";
			Shipment.JS_HouseBillOfLadingType = "IAU";
			FreightDataRegistry.Instance.EnableHouseBillOfLadingRegistryItems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			HouseBillOfLadingTypeCollection hBLRegistryObjectCollection = FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.Value;
			HouseBillOfLadingType hBLRegistryObject = hBLRegistryObjectCollection.FindByCode("IAU") as HouseBillOfLadingType;
			hBLRegistryObject.PrePrinted = ZBool.True;
			FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, hBLRegistryObjectCollection);
			AssertEquals("IsPreprinted true", ZBool.True, BOLWrapper.IsPreprinted);
			AssertEquals("BillSurrenderedToHeading is empty", ZString.Empty, BOLWrapper.BillSurrenderedToHeading);

			hBLRegistryObject.PrePrinted = ZBool.False;
			FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, hBLRegistryObjectCollection);
			ResetBOLWrapper();
			AssertEquals("BillSurrenderedToHeading", DocConstants.Resources.BillTerms.BillSurrenderedToHeading, BOLWrapper.BillSurrenderedToHeading);

			Shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.SeaWaybill;
			ResetBOLWrapper();
			AssertEquals("BillSurrenderedToHeading", DocConstants.Resources.BillTerms.DeliveryAgentHeading, BOLWrapper.BillSurrenderedToHeading);
		}

		public void TestNotNegotiableText()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";
			Shipment.JS_HouseBillOfLadingType = "IAU";
			FreightDataRegistry.Instance.EnableHouseBillOfLadingRegistryItems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			HouseBillOfLadingTypeCollection hBLRegistryObjectCollection = FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.Value;
			HouseBillOfLadingType hBLRegistryObject = hBLRegistryObjectCollection.FindByCode("IAU") as HouseBillOfLadingType;
			hBLRegistryObject.PrePrinted = ZBool.True;
			FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, hBLRegistryObjectCollection);
			AssertEquals("IsPreprinted true", ZBool.True, BOLWrapper.IsPreprinted);
			AssertEquals("NotNegotiableText is empty", ZString.Empty, BOLWrapper.NotNegotiableText);

			hBLRegistryObject.PrePrinted = ZBool.False;
			FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, hBLRegistryObjectCollection);
			ResetBOLWrapper();
			AssertEquals("NotNegotiableText", DocConstants.Resources.BillTerms.NotNegotiableText, BOLWrapper.NotNegotiableText);

			Shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.SeaWaybill;
			ResetBOLWrapper();
			AssertEquals("NotNegotiableText should be empty", ZString.Empty, BOLWrapper.NotNegotiableText);
		}

		[TestDate(2016, 10, 19)]
		public void TestBOLClause_BeforeDCSEffectiveDate()
		{
			FreightDataRegistry.Instance.BOLClause.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "BOLClause text");
			FreightDataRegistry.Instance.BOLClauseITAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "These weapons are authorized by the U.S. Government for export only to <BillOfLading.DeclarationDestinationCountry> for use by <BillOfLading.USUltimateConsignee>.");

			Shipment.JS_RL_NKOrigin = "USLAX";
			Shipment.JS_RL_NKDestination = "GBLON";

			OrgHeader ultimateConsignee = Factory.NewWithValidTestData<OrgHeader>();
			ultimateConsignee.OH_FullName = "Ultimate Consignee";

			var declaration = Factory.New<Enterprise.Integration.Customs.US.IJobDeclaration>();
			declaration.JE_JS = Shipment.PK;
			declaration.JE_OverrideFreightDefaults = true;
			declaration.JE_RL_NKFinalDestination = "ALTIA";

			var invoiceHeader = (Enterprise.Integration.Customs.US.IJobComInvoiceHeader)declaration.Invoices.AddNew();
			invoiceHeader.JZ_OH_Buyer = ultimateConsignee.PK;
			invoiceHeader.US_LicenseNo = "1111";
			invoiceHeader.US_DDTCRegistrationNo = "2222";

			Factory.Save();

			string storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
				ResetBOLWrapper();
				AssertEquals("USCountryOfUltimateDestination", "Albania", BOLWrapper.DeclarationDestinationCountry);
				AssertEquals("USUltimateConsignee", "Ultimate Consignee", BOLWrapper.USUltimateConsignee);
				AssertEquals("Standard BOLCLause", "BOLClause text", BOLWrapper.BOLClause);

				GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.UnitedStates);
				ResetBOLWrapper();
				AssertEquals("USCountryOfUltimateDestination", "Albania", BOLWrapper.DeclarationDestinationCountry);
				AssertEquals("USUltimateConsignee", "Ultimate Consignee", BOLWrapper.USUltimateConsignee);
				AssertEquals("ITAR BOLCLause", "These weapons are authorized by the U.S. Government for export only to Albania for use by Ultimate Consignee.", BOLWrapper.BOLClause);

				Shipment.JS_RL_NKOrigin = "AUSYD";
				ResetBOLWrapper();
				AssertEquals("Standard BOLCLause when not US export shipment", "BOLClause text", BOLWrapper.BOLClause);

				Shipment.JS_RL_NKOrigin = "USLAX";
				invoiceHeader.US_DDTCRegistrationNo = "";
				ResetBOLWrapper();
				AssertEquals("Standard BOLCLause when US_DDTCRegistrationNo is empty", "BOLClause text", BOLWrapper.BOLClause);

				invoiceHeader.US_LicenseNo = "";
				invoiceHeader.US_DDTCRegistrationNo = "2222";
				ResetBOLWrapper();
				AssertEquals("Standard BOLCLause when US_LicenseNo is empty", "BOLClause text", BOLWrapper.BOLClause);

				invoiceHeader.US_LicenseNo = "1111";
				ResetBOLWrapper();
				AssertEquals("ITAR BOLCLause", "These weapons are authorized by the U.S. Government for export only to Albania for use by Ultimate Consignee.", BOLWrapper.BOLClause);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(storedCountry);
			}
		}

		[TestDate(2016, 11, 15)]
		public void TestBOLClause_AfterDCSEffectiveDate()
		{
			FreightDataRegistry.Instance.BOLClause.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "BOLClause text");
			FreightDataRegistry.Instance.BOLClauseITAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "These weapons are authorized by the U.S. Government for export only to <BillOfLading.DeclarationDestinationCountry> for use by <BillOfLading.USUltimateConsignee>.");

			Shipment.JS_RL_NKOrigin = "USLAX";
			Shipment.JS_RL_NKDestination = "GBLON";

			OrgHeader ultimateConsignee = Factory.NewWithValidTestData<OrgHeader>();
			ultimateConsignee.OH_FullName = "Ultimate Consignee";

			var declaration = Factory.New<Enterprise.Integration.Customs.US.IJobDeclaration>();
			declaration.JE_JS = Shipment.PK;
			declaration.JE_OverrideFreightDefaults = true;
			declaration.JE_RL_NKFinalDestination = "ALTIA";

			var invoiceHeader = (Enterprise.Integration.Customs.US.IJobComInvoiceHeader)declaration.Invoices.AddNew();
			invoiceHeader.JZ_OH_Buyer = ultimateConsignee.PK;
			invoiceHeader.US_LicenseNo = "1111";
			invoiceHeader.US_DDTCRegistrationNo = "2222";

			Factory.Save();

			string storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
				ResetBOLWrapper();
				AssertEquals("USCountryOfUltimateDestination", "Albania", BOLWrapper.DeclarationDestinationCountry);
				AssertEquals("USUltimateConsignee", "Ultimate Consignee", BOLWrapper.USUltimateConsignee);
				AssertEquals("Standard BOLCLause", "BOLClause text", BOLWrapper.BOLClause);

				GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.UnitedStates);
				ResetBOLWrapper();
				AssertEquals("USCountryOfUltimateDestination", "Albania", BOLWrapper.DeclarationDestinationCountry);
				AssertEquals("USUltimateConsignee", "Ultimate Consignee", BOLWrapper.USUltimateConsignee);
				AssertEquals("Standard BOLCLause", "BOLClause text", BOLWrapper.BOLClause);

				Shipment.JS_RL_NKOrigin = "AUSYD";
				ResetBOLWrapper();
				AssertEquals("Standard BOLCLause when not US export shipment", "BOLClause text", BOLWrapper.BOLClause);

				Shipment.JS_RL_NKOrigin = "USLAX";
				invoiceHeader.US_DDTCRegistrationNo = "";
				ResetBOLWrapper();
				AssertEquals("Standard BOLCLause when US_DDTCRegistrationNo is empty", "BOLClause text", BOLWrapper.BOLClause);

				invoiceHeader.US_LicenseNo = "";
				invoiceHeader.US_DDTCRegistrationNo = "2222";
				ResetBOLWrapper();
				AssertEquals("Standard BOLCLause when US_LicenseNo is empty", "BOLClause text", BOLWrapper.BOLClause);

				invoiceHeader.US_LicenseNo = "1111";
				ResetBOLWrapper();
				AssertEquals("Standard BOLCLause", "BOLClause text", BOLWrapper.BOLClause);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(storedCountry);
			}
		}

		#endregion

		#region Images

		public void TestImageNamesToRemove()
		{
			AssertEquals("Should return empty collection", 0, BOLWrapper.ImageNamesToRemove.Length);

			Shipment.JS_HouseBillOfLadingType = "FIP";
			AssertEquals("Should return 1 string: BillOfLading.FaceImage", 1, BOLWrapper.ImageNamesToRemove.Length);
			AssertEquals("Should return 1 string: BillOfLading.FaceImage", "BillOfLading.FaceImage", BOLWrapper.ImageNamesToRemove[0]);
		}
		public void TestHouseBillLogo()
		{
			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Bill Of Lading");
			ShipmentWrapper.SetTemplateConstants(constants);
			if (Env.Registry.HouseBillOfLadingLogo != null)
			{
				AssertEquals("Logo", Env.Registry.HouseBillOfLadingLogo.Size, BOLWrapper.HouseBillLogo.Size);
			}
			else
			{
				AssertEquals("Logo", null, BOLWrapper.HouseBillLogo);
			}

			constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Bill Of Lading Preprinted");
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.SetTemplateConstants(constants);
			ResetBOLWrapper();

			AssertEquals("No Logo", null, BOLWrapper.HouseBillLogo);
		}

		public void TestFIATATextLogo()
		{
			ImageHandler handler = new ImageHandler();
			Shipment.JS_HouseBillOfLadingType = "IAU";
			FreightDataRegistry.Instance.FIATAAuthorised.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			FreightDataRegistry.Instance.EnableHouseBillOfLadingRegistryItems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			HouseBillOfLadingTypeCollection hBLRegistryObjectCollection = FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.Value;
			HouseBillOfLadingType hBLRegistryObject = hBLRegistryObjectCollection.FindByCode("IAU") as HouseBillOfLadingType;
			hBLRegistryObject.PrePrinted = ZBool.True;
			FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, hBLRegistryObjectCollection);
			AssertEquals("IsPreprinted true", ZBool.True, BOLWrapper.IsPreprinted);
			AssertNull("FIATA Text Logo should be null for preprinted", BOLWrapper.FIATATextLogo);

			hBLRegistryObject.PrePrinted = ZBool.False;
			Shipment.JS_HouseBillOfLadingType = "FIA";
			Shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.OriginalReqSurrender;
			ReadOnlyCodeDescriptionPairList textLogoPaths = DocConstants.Resources.FIATA.GetTextLogoPaths();
			Image expectedImage = handler.GetImageWithResourcePath(textLogoPaths.GetDescriptionFromCode(DocConstants.Resources.FIATA.TextLogoCodes.HBL));
			ResetBOLWrapper();
			AssertNotNull("FIATA HBL text logo", BOLWrapper.FIATATextLogo);
			AssertEquals("It should return the HBL logo", true, Utilities.IsImageEqual(BOLWrapper.FIATATextLogo, expectedImage));

			Shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.SeaWaybill;
			expectedImage = handler.GetImageWithResourcePath(textLogoPaths.GetDescriptionFromCode(DocConstants.Resources.FIATA.TextLogoCodes.SWB));
			ResetBOLWrapper();
			AssertNotNull("FIATA SWB text logo", BOLWrapper.FIATATextLogo);
			AssertEquals("It should return the SWB logo", true, Utilities.IsImageEqual(BOLWrapper.FIATATextLogo, expectedImage));

			FreightDataRegistry.Instance.FIATAAuthorised.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			expectedImage = handler.GetImageWithResourcePath(textLogoPaths.GetDescriptionFromCode(DocConstants.Resources.FIATA.TextLogoCodes.FWB));
			ResetBOLWrapper();
			AssertNotNull("FIATA FWB text logo", BOLWrapper.FIATATextLogo);
			AssertEquals("It should return the FWB logo", true, Utilities.IsImageEqual(BOLWrapper.FIATATextLogo, expectedImage));

			Shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.OriginalReqSurrender;
			expectedImage = handler.GetImageWithResourcePath(textLogoPaths.GetDescriptionFromCode(DocConstants.Resources.FIATA.TextLogoCodes.ICC));
			ResetBOLWrapper();
			AssertNotNull("FIATA ICC text logo", BOLWrapper.FIATATextLogo);
			AssertEquals("It should return the ICC logo", true, Utilities.IsImageEqual(BOLWrapper.FIATATextLogo, expectedImage));
		}

		public void TestFIATALogo()
		{
			ImageHandler handler = new ImageHandler();
			FreightDataRegistry.Instance.FIATAAuthorised.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "XxXxX";
			AssertEquals("FIATA Logo should be null", null, BOLWrapper.FIATALogo);

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";
			Shipment.JS_HouseBillOfLadingType = "IAU";
			FreightDataRegistry.Instance.EnableHouseBillOfLadingRegistryItems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			HouseBillOfLadingTypeCollection hBLRegistryObjectCollection = FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.Value;
			HouseBillOfLadingType hBLRegistryObject = hBLRegistryObjectCollection.FindByCode("IAU") as HouseBillOfLadingType;
			hBLRegistryObject.PrePrinted = ZBool.True;
			FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, hBLRegistryObjectCollection);
			AssertEquals("IsPreprinted true", ZBool.True, BOLWrapper.IsPreprinted);
			AssertNull("FIATA Logo should be null for preprinted", BOLWrapper.FIATALogo);

			Shipment.JS_HouseBillOfLadingType = "FIA";
			hBLRegistryObject.PrePrinted = ZBool.False;
			AssertNull("FIATA Logo should be null because it is not authorised", BOLWrapper.FIATALogo);

			FreightDataRegistry.Instance.FIATAAuthorised.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Image expectedImage = handler.GetImageWithResourcePath(DocConstants.Resources.FIATA.GetGraphicLogoPaths().GetDescriptionFromCode(Core.Constants.CountryCodes.Australia));
			ResetBOLWrapper();
			AssertNotNull("FIATA logo should not be null", BOLWrapper.FIATALogo);
			Assert("It should return the Australian FIATA logo", BOLWrapper.FIATALogo.Size == expectedImage.Size);

			Assert(DocConstants.Resources.FIATA.GetGraphicLogoPaths().GetDescriptionFromCode(Core.Constants.CountryCodes.VietNam).EndsWith("FIATAlogo-VLA.gif"));
		}

		public void TestBillTermsImage()
		{
			BOLWrapper.ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.HBLCode, "IAU");
			Shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.SeaWaybill;
			ZString resourcePath = DocConstants.Resources.BillTerms.StandardBillTermsBasePath + BOLWrapper.ShipmentWrapper.HBLCode + "_SWB.gif";
			Image expectedImage = new ImageHandler().GetImageWithResourcePath(resourcePath);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.HBLCode, "IAU");
			ResetBOLWrapper();
			AssertNotNull("Bill terms image should not be null", BOLWrapper.BillTermsImage);
			Assert("Bill terms images should be the same", expectedImage.Size == BOLWrapper.BillTermsImage.Size);

			Shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.OriginalReqSurrender;
			resourcePath = DocConstants.Resources.BillTerms.StandardBillTermsBasePath + BOLWrapper.ShipmentWrapper.HBLCode + "_HBL.gif";
			expectedImage = new ImageHandler().GetImageWithResourcePath(resourcePath);
			ResetBOLWrapper();
			AssertNotNull("Bill terms image should not be null", BOLWrapper.BillTermsImage);
			Assert("Bill terms images should be the same", expectedImage.Size == BOLWrapper.BillTermsImage.Size);
		}

		public void TestGetImageHandler()
		{
			ImageHandler imageHandler = BOLWrapper.ImageHandlerForTest;
			AssertNotNull(imageHandler);
			AssertEquals(typeof(ImageHandler), imageHandler.GetType());
		}

		public void TestBillTermsImagePath()
		{
			AssertEquals(DocConstants.Resources.BillTerms.StandardBillTermsBasePath, BOLWrapper.BillTermsImagePathForTest);
		}

		public void TestIsSeaWaybill()
		{
			AssertEquals("IsSeaWaybill false", ZBool.False, BOLWrapper.IsSeaWaybill);

			Shipment.JS_HouseBillOfLadingType = "IAU";
			Shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.SeaWaybill;
			ResetBOLWrapper();
			AssertEquals("IsSeaWaybill true", ZBool.True, BOLWrapper.IsSeaWaybill);
		}

		public void TestIsPreprinted()
		{
			AssertEquals("IsPreprinted false", ZBool.False, BOLWrapper.IsPreprinted);

			Shipment.JS_HouseBillOfLadingType = "IAU";

			FreightDataRegistry.Instance.EnableHouseBillOfLadingRegistryItems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			HouseBillOfLadingTypeCollection hBLRegistryObjectCollection = FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.Value;
			HouseBillOfLadingType hBLRegistryObject = hBLRegistryObjectCollection.FindByCode("IAU") as HouseBillOfLadingType;
			hBLRegistryObject.PrePrinted = ZBool.True;
			FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, hBLRegistryObjectCollection);

			AssertEquals("IsPreprinted true", ZBool.True, BOLWrapper.IsPreprinted);
		}

		public void TestShowTCImage()
		{
			AssertEquals("Show TC Image false TC null", ZBool.False, BOLWrapper.ShowTCImage);
			//This is used in Bill templates as #if "<BillOfLading.ShowTCImage>" == "Y" or "N"
			AssertEquals("Show TC Image false TC null", "N", BOLWrapper.ShowTCImage.ToString());
		}

		public void TestTCImage()
		{
			AssertNull("TC Image null", BOLWrapper.TCImage);

			Shipment.JS_HouseBillOfLadingType = "IAU";
			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.DeliveryMode, "ALL");
			ShipmentWrapper.SetTemplateConstants(constants);

			FreightDataRegistry.Instance.EnableHouseBillOfLadingRegistryItems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			HouseBillOfLadingTypeCollection hBLRegistryObjectCollection = FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.Value;
			HouseBillOfLadingType hBLRegistryObject = hBLRegistryObjectCollection.FindByCode("IAU") as HouseBillOfLadingType;
			hBLRegistryObject.PrePrinted = ZBool.False;
			FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, hBLRegistryObjectCollection);

			AssertNotNull("TC Image not null", BOLWrapper.TCImage);
			AssertEquals("Show TC Image true TC not null", "Y", BOLWrapper.ShowTCImage.ToString());

			Shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.SeaWaybill;
			Shipment.JS_HouseBillOfLadingType = "IAU";
			var tC = FreightDataRegistry.Instance.HouseBillOfLadingTermsAndConditionsImages.Value.FindByCodeForDeliveryModeALL("SWB");
			ResetBOLWrapper();
			AssertEquals("It should return the SWB T&C", true, Utilities.IsImageEqual(BOLWrapper.TCImage, tC.Image));

			hBLRegistryObject.PrePrinted = ZBool.True;
			FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, hBLRegistryObjectCollection);

			ResetBOLWrapper();
			AssertNull("TC Image null", BOLWrapper.TCImage);
			AssertEquals("Show TC Image false TC null", "N", BOLWrapper.ShowTCImage.ToString());
		}

		public void TestTCImageForSeaWayBillOfHouseBillOfLadingTypeFIA()
		{
			FreightDataRegistry.Instance.FIATAAuthorised.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.DeliveryMode, "ALL");
			ShipmentWrapper.SetTemplateConstants(constants);

			Shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.SeaWaybill;
			Shipment.JS_HouseBillOfLadingType = "FIA";

			var expectedTermsAndConditionsImage = FreightDataRegistry.Instance.HouseBillOfLadingTermsAndConditionsImages.Value.FindByCodeForDeliveryModeALL("FWB").Image;
			AssertNotNull(expectedTermsAndConditionsImage);

			var wrapper = new DocBillOfLading(ShipmentWrapper);
			AssertEquals("It should return the FWB T&C", true, Utilities.IsImageEqual(wrapper.TCImage, expectedTermsAndConditionsImage));
		}

		public void TestLogo()
		{
			Shipment.JS_HouseBillOfLadingType = "IAU";
			FreightDataRegistry.Instance.EnableHouseBillOfLadingRegistryItems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			if (Env.Registry.HouseBillOfLadingLogo != null)
			{
				AssertEquals("Logo", Env.Registry.HouseBillOfLadingLogo.Size, BOLWrapper.Logo.Size);
			}
			else
			{
				AssertEquals("Logo", null, BOLWrapper.Logo);
			}

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Bill of Lading To Preprinted");
			ShipmentWrapper.SetTemplateConstants(constants);
			ResetBOLWrapper();
			AssertEquals("Logo should be null for preprinted menu", null, BOLWrapper.Logo);

			constants.Clear();
			ShipmentWrapper.SetTemplateConstants(constants);

			FreightDataRegistry.Instance.EnableHouseBillOfLadingRegistryItems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			HouseBillOfLadingTypeCollection hBLRegistryObjectCollection = FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.Value;
			HouseBillOfLadingType hBLRegistryObject = hBLRegistryObjectCollection.FindByCode("IAU") as HouseBillOfLadingType;
			hBLRegistryObject.PrintLogo = ZBool.False;

			ResetBOLWrapper();
			AssertNull("Logo Image null", BOLWrapper.Logo);

			hBLRegistryObject.PrintLogo = ZBool.True;

			RegistryImageCollection coll = new RegistryImageCollection();
			var newImage = coll.AddNew();
			newImage.Code = "NEW";
			newImage.Description = (NoResString)"NEW TEST IMAGE";
			newImage.Image = new Bitmap(1, 1);

			hBLRegistryObject.LogoCode = "NEW";

			FreightDataRegistry.Instance.HouseBillOfLadingLogoImages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, coll);
			FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, hBLRegistryObjectCollection);
			ResetBOLWrapper();
			AssertEquals("Logo Image from registry", new Size(1, 1), BOLWrapper.Logo.Size);
		}

		public void TestLogoWithClientBranding()
		{
			OrgHeader cnee = OrgHeader.New(Factory);
			cnee.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 11);
			Shipment.ConsigneePK = cnee.PK;

			OrgHeader cnor = OrgHeader.New(Factory);
			cnor.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 55);
			Shipment.ConsignorPK = cnor.PK;

			FreightDataRegistry.Instance.EnableHouseBillOfLadingRegistryItems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			DocumentsDataRegistry.Instance.EnableClientBranding.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, true);
			DocumentsDataRegistry.Instance.HBLAndHAWBBrandingOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, HBLAndHAWBBrandingOptionEditorInfo.ClientBranded);
			BrandingTestHelperClass.SetHybridBrandRegistryImage(DocumentsDataRegistry.Instance.HBLAgentBrandingImage);

			ResetBOLWrapper();
			AssertNotNull("Registry items should have returned a logo.", BOLWrapper.Logo);
			AssertEquals("Logo Image from Cnor's brand image", new Size(8, 8), BOLWrapper.Logo.Size);
			var header = Factory.NewJobForTesting<JobHeader>();
			OrgHeader client = OrgHeader.New(Factory);
			client.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 11);
			header.LocalChargesPK = client.PK;
			header.JH_ParentID = Shipment.PK;
			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();
			AssertEquals("Logo Image from Client's brand image", new Size(9, 9), BOLWrapper.Logo.Size);
		}

		public void TestLogoIsTakenFromControllingBranchWhenRegistryHasThisSetup()
		{
			Action<GlbBranch, string, int> setupImageRegistryForBranch = (branch, imageCode, imageSize) =>
			{
				IDisposable setupContext = new TemporaryUserContext() { BranchPK = branch.PK.ToGuid() }.Set();
				using (setupContext)
				{
					var imageCollection = new RegistryImageCollection();
					var logoImage = imageCollection.AddNew();
					logoImage.Code = imageCode;
					logoImage.Description = (NoResString)"Dummy image";
					logoImage.Image = new Bitmap(imageSize, imageSize);

					FreightDataRegistry.Instance.HouseBillOfLadingLogoImages.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, imageCollection);

					var hBLTypeCollection = FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.Value;
					var hBLType = hBLTypeCollection.FindByCode("IAU") as HouseBillOfLadingType;
					hBLType.LogoCode = imageCode;

					FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, hBLTypeCollection);
				}
			};

			GlbBranch controllingBranch = Factory.NewWithValidTestData<GlbBranch>();
			controllingBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			setupImageRegistryForBranch(GlbBranch.CurrentBranch, "AAA", 10);
			setupImageRegistryForBranch(controllingBranch, "BBB", 20);

			Shipment.JS_HouseBillOfLadingType = "IAU";
			FreightDataRegistry.Instance.EnableHouseBillOfLadingRegistryItems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.UseLoginBranchLogoForFreight.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			JobHeader jobHeader = new JobHeader.Loader(Shipment).TryLoadOrCreate();
			jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			ResetBOLWrapper();
			AssertEquals("Logo Image from current branch's registry", new Size(10, 10), BOLWrapper.Logo.Size);

			jobHeader.JH_GB = controllingBranch.PK;
			ResetBOLWrapper();
			AssertEquals("Logo Image from controlling branch's registry", new Size(20, 20), BOLWrapper.Logo.Size);

			SystemDataRegistry.Instance.UseLoginBranchLogoForFreight.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ResetBOLWrapper();
			AssertEquals("Logo Image from current branch's registry", new Size(10, 10), BOLWrapper.Logo.Size);
		}

		#endregion

		#region Consol

		ForwardingConsol AddNewConsol(string transportMode, string loadPort, string dischargePort)
		{
			string vessel = "Test Vessel";
			string voyage = "AA123456";

			ForwardingConsol consol = Shipment.Consols.AddNew();
			consol.JK_TransportMode = transportMode;
			consol.JK_RL_NKLoadPort = loadPort;
			consol.JK_RL_NKDischargePort = dischargePort;

			Transport consolTransport2 = consol.Transports[0];
			consolTransport2.JW_IsLinked = false;
			consolTransport2.JW_Vessel = vessel + "2";
			consolTransport2.JW_VoyageFlight = voyage + "2";
			consolTransport2.JW_TransportType = Core.Constants.TransportPlanningType.Other;

			Transport consolTransport = consol.Transports.AddNew();
			consolTransport.JW_IsLinked = false;
			consolTransport.JW_Vessel = vessel;
			consolTransport.JW_VoyageFlight = voyage;
			consolTransport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			consolTransport.JW_RL_NKLoadPort = loadPort;
			consolTransport.JW_RL_NKDiscPort = dischargePort;

			Transport consolTransport3 = consol.Transports.AddNew();
			consolTransport3.JW_IsLinked = false;
			consolTransport3.JW_Vessel = vessel + "3";
			consolTransport3.JW_VoyageFlight = voyage + "3";

			return consol;
		}

		void AssertBOLPullsDataFromTheConsol(ForwardingConsol consol, string loadPort, string dischargePort)
		{
			ResetWrappers();
			AssertEquals(loadPort, BOLWrapper.PortOfLoading.Code);
			AssertEquals(dischargePort, BOLWrapper.PortOfDischarge.Code);
			AssertEquals(consol.JK_TransportMode, BOLWrapper.TransportMode);
			AssertEquals("Test Vessel", BOLWrapper.VesselName);
			AssertEquals("AA123456", BOLWrapper.VoyageNumber);
		}

		public void TestConsolImport()
		{
			ZString initialDirection = BOLWrapper.ShipmentWrapper.DocumentDirection;
			BOLWrapper.ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			try
			{
				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
				Shipment.Consols.RemoveAll();
				Shipment.Transports.RemoveAndDeleteAll();
				Shipment.JS_TransportMode = Constants.TransportModes.Sea;

				ForwardingConsol consol1 = AddNewConsol("SEA", "AUSYD", "USLAX");
				AssertBOLPullsDataFromTheConsol(consol1, "AUSYD", "USLAX");

				ForwardingConsol consol2 = AddNewConsol("ROA", "USLAX", "SGSIN");
				AssertBOLPullsDataFromTheConsol(consol1, "AUSYD", "USLAX");

				Shipment.JS_TransportMode = Constants.TransportModes.Road;
				AssertBOLPullsDataFromTheConsol(consol2, "USLAX", "SGSIN");

				ForwardingConsol consol4 = AddNewConsol("AIR", "USNYC", "AUMEL");
				ForwardingConsol consol3 = AddNewConsol("ROA", "SGSIN", "USNYC");
				ForwardingConsol consol5 = AddNewConsol("AIR", "AUMEL", "UAIEV");

				AssertBOLPullsDataFromTheConsol(consol3, "SGSIN", "USNYC");

				Shipment.JS_TransportMode = Constants.TransportModes.Air;
				AssertBOLPullsDataFromTheConsol(consol5, "AUMEL", "UAIEV");

				Shipment.JS_TransportMode = Constants.TransportModes.SeaAir;
				AssertBOLPullsDataFromTheConsol(consol5, "AUMEL", "UAIEV");

				Shipment.JS_TransportMode = Constants.TransportModes.AirSea;
				AssertBOLPullsDataFromTheConsol(consol1, "AUSYD", "USLAX");

				Shipment.JS_TransportMode = Constants.TransportModes.Rail;
				AssertBOLPullsDataFromTheConsol(consol5, "AUMEL", "UAIEV");

				consol4.JK_RL_NKLoadPort = "GBLON";
				consol4.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "GBLON";
				Shipment.JS_TransportMode = Constants.TransportModes.Road;
				AssertBOLPullsDataFromTheConsol(consol3, "SGSIN", "USNYC"); //Road consols should still be chained because their chain is not broken. And it should return the last one here
			}
			finally
			{
				BOLWrapper.ShipmentWrapper.SetDocumentDirectionForTesting(initialDirection);
			}
		}

		public void TestConsolExport()
		{
			ZString initialDirection = BOLWrapper.ShipmentWrapper.DocumentDirection;
			BOLWrapper.ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			try
			{
				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
				Shipment.Consols.RemoveAll();
				Shipment.Transports.RemoveAndDeleteAll();
				Shipment.JS_TransportMode = Constants.TransportModes.Sea;

				ForwardingConsol consol1 = AddNewConsol("SEA", "AUSYD", "USLAX");
				consol1.Transports.DepartureTransport.JW_ATD = ZDateTime.Now;
				AssertBOLPullsDataFromTheConsol(consol1, "AUSYD", "USLAX");

				ForwardingConsol consol2 = AddNewConsol("ROA", "USLAX", "SGSIN");
				AssertBOLPullsDataFromTheConsol(consol1, "AUSYD", "USLAX");

				Shipment.JS_TransportMode = Constants.TransportModes.Road;

				ForwardingConsol consol4 = AddNewConsol("AIR", "USNYC", "AUMEL");
				ForwardingConsol consol5 = AddNewConsol("AIR", "AUMEL", "UAIEV");
				ForwardingConsol consol3 = AddNewConsol("ROA", "SGSIN", "USNYC");

				AssertBOLPullsDataFromTheConsol(consol2, "USLAX", "SGSIN");

				Shipment.JS_TransportMode = Constants.TransportModes.Air;
				AssertBOLPullsDataFromTheConsol(consol4, "USNYC", "AUMEL");

				Shipment.JS_TransportMode = Constants.TransportModes.SeaAir;
				AssertBOLPullsDataFromTheConsol(consol1, "AUSYD", "USLAX");

				Shipment.JS_TransportMode = Constants.TransportModes.AirSea;
				AssertBOLPullsDataFromTheConsol(consol4, "USNYC", "AUMEL");

				Shipment.JS_TransportMode = Constants.TransportModes.Rail;
				AssertBOLPullsDataFromTheConsol(consol1, "AUSYD", "USLAX");

				consol5.JK_RL_NKLoadPort = "GBLON";
				consol5.Transports[1].JW_RL_NKLoadPort = "GBLON";
				AssertBOLPullsDataFromTheConsol(consol1, "AUSYD", "USLAX");

				Shipment.JS_TransportMode = Constants.TransportModes.Air;
				AssertBOLPullsDataFromTheConsol(consol4, "USNYC", "AUMEL");
			}
			finally
			{
				BOLWrapper.ShipmentWrapper.SetDocumentDirectionForTesting(initialDirection);
			}
		}

		#endregion

		#region Transports

		public void TestPreprintedHeadings()
		{
			Shipment.JS_HouseBillOfLadingType = "EAP";

			FreightDataRegistry.Instance.EnableHouseBillOfLadingRegistryItems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			HouseBillOfLadingTypeCollection hBLRegistryObjectCollection = FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.Value;
			HouseBillOfLadingType hBLRegistryObject = hBLRegistryObjectCollection.FindByCode("EAP") as HouseBillOfLadingType;
			hBLRegistryObject.PrePrinted = ZBool.True;
			FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, hBLRegistryObjectCollection);

			AssertEquals("Vessel Heading Pre-printed", ZString.Empty, BOLWrapper.VesselHeading);
			AssertEquals("Voyage Heading Pre-printed", ZString.Empty, BOLWrapper.VoyageHeading);
		}

		public void TestVesselVoyageHeading()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			Shipment.Consols.RemoveAll();
			Shipment.Transports.RemoveAndDeleteAll();
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			Shipment.JS_HouseBillOfLadingType = "IAU";

			FreightDataRegistry.Instance.EnableHouseBillOfLadingRegistryItems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			HouseBillOfLadingTypeCollection hBLRegistryObjectCollection = FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.Value;
			HouseBillOfLadingType hBLRegistryObject = hBLRegistryObjectCollection.FindByCode("IAU") as HouseBillOfLadingType;
			hBLRegistryObject.PrePrinted = ZBool.True;
			FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, hBLRegistryObjectCollection);

			AssertEquals("Vessel Heading empty for preprinted", "", BOLWrapper.VesselHeading);
			AssertEquals("Voyage Heading empty for preprinted", "", BOLWrapper.VoyageHeading);

			hBLRegistryObject.PrePrinted = ZBool.False;
			FreightDataRegistry.Instance.HouseBillOfLadingLogoTypes.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, hBLRegistryObjectCollection);
			ResetWrappers();
			AssertEquals("Vessel Heading", "Vessel", BOLWrapper.VesselHeading);
			AssertEquals("Voyage Heading", "Voyage", BOLWrapper.VoyageHeading);

			Consol = Shipment.Consols.AddNew();
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			ResetWrappers();
			AssertEquals("Vessel Heading", "Vessel", BOLWrapper.VesselHeading);
			AssertEquals("Voyage Heading", "Voyage", BOLWrapper.VoyageHeading);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Rail;
			ResetWrappers();
			AssertEquals("Vessel Heading", "Journey", BOLWrapper.VesselHeading);
			AssertEquals("Voyage Heading", "Journey No.", BOLWrapper.VoyageHeading);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			ResetWrappers();
			AssertEquals("Vessel Heading", "", BOLWrapper.VesselHeading);
			AssertEquals("Voyage Heading", "Truck Ref.", BOLWrapper.VoyageHeading);

			Transport mainLeg = AddNewLeg(Shipment, MainVesselType, "AUBNE", "SGSIN", "", "");
			mainLeg.JW_TransportMode = Core.Constants.TransportModes.Sea;
			mainLeg.JW_Vessel = "123vessel";
			ResetWrappers();
			AssertEquals("Vessel Heading", "Vessel", BOLWrapper.VesselHeading);
			AssertEquals("Voyage Heading", "Voyage", BOLWrapper.VoyageHeading);
		}

		public void TestPreCarriageVesselVoyage()
		{
			AssertEquals("PreCarriageVesselVoyage", "", BOLWrapper.PreCarriageVesselVoyage);

			Transport transport1 = Consol.Transports.AddNew();
			AssertEquals("PreCarriageVesselVoyage", "", BOLWrapper.PreCarriageVesselVoyage);

			transport1.JW_TransportType = Core.Constants.TransportPlanningType.PreCarriage;
			transport1.JW_Vessel = "Vessel 1";
			transport1.JW_VoyageFlight = "1111";
			AssertEquals("PreCarriageVesselVoyage", "Vessel 1 / 1111", BOLWrapper.PreCarriageVesselVoyage);

			Transport transport2 = Shipment.Transports.AddNew();
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.PreCarriage;
			transport2.JW_Vessel = "Vessel 2";
			transport2.JW_VoyageFlight = "2222";
			AssertEquals("PreCarriageVesselVoyage", "Vessel 2 / 2222", BOLWrapper.PreCarriageVesselVoyage);

			transport2.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			AssertEquals("PreCarriageVesselVoyage (from consol)", "Vessel 1 / 1111", BOLWrapper.PreCarriageVesselVoyage);
		}

		public void TestMainVesselVoyageLoadAndDischargePort()
		{
			AssertEquals("MainVesselVoyage", "", BOLWrapper.MainVesselVoyage);
			AssertEquals("Main Vessel", "", BOLWrapper.VesselName);
			AssertEquals("Main VoyageNumber", "", BOLWrapper.VoyageNumber);
			AssertNull("Main Port of Loading", BOLWrapper.PortOfLoading);
			AssertNull("Main Port of Discharge", BOLWrapper.PortOfDischarge);
			AssertEquals("Default Port of Loading", "", BOLWrapper.PortOfLoadingDefault);
			AssertEquals("Default Port of Discharge", "", BOLWrapper.PortOfDischargeDefault);

			Consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUBNE";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.JW_Vessel = "Consol Vessel";
			transport.JW_VoyageFlight = "ConsolVoy";

			CombineAssertions(delegate
			{
				AssertEquals("MainVesselVoyage", "Consol Vessel / ConsolVoy", BOLWrapper.MainVesselVoyage);
				AssertEquals("Main Vessel", "Consol Vessel", BOLWrapper.VesselName);
				AssertEquals("Main VoyageNumber", "ConsolVoy", BOLWrapper.VoyageNumber);
				AssertEquals("Main Port of Loading", "AUBNE", BOLWrapper.PortOfLoading.Code);
				AssertEquals("Main Port of Discharge", "SGSIN", BOLWrapper.PortOfDischarge.Code);
				AssertEquals("Default Port of Loading", "BRISBANE, AUSTRALIA", BOLWrapper.PortOfLoadingDefault);
				AssertEquals("Default Port of Discharge", "SINGAPORE", BOLWrapper.PortOfDischargeDefault);
			});

			//Main Consol		: AUBNE-SGSIN, Consol Vessel / ConsolVoy
			//MAI Leg in Consol	: SGSIN-JPOSA, MainLeg Vessel / MainLegVoy
			Consol.Transports.RemoveAndDeleteAll();
			Transport transport1 = AddNewLeg(Consol, MainVesselType, "SGSIN", "JPOSA", "MainLeg Vessel", "MainLegVoy");

			CombineAssertions(delegate
			{
				AssertEquals("MainVesselVoyage", "MainLeg Vessel / MainLegVoy", BOLWrapper.MainVesselVoyage);
				AssertEquals("Main Vessel", "MainLeg Vessel", BOLWrapper.VesselName);
				AssertEquals("Main VoyageNumber", "MainLegVoy", BOLWrapper.VoyageNumber);
				AssertEquals("Main Port of Loading", "SGSIN", BOLWrapper.PortOfLoading.Code);
				AssertEquals("Main Port of Discharge", "JPOSA", BOLWrapper.PortOfDischarge.Code);
				AssertEquals("Default Port of Loading", "SINGAPORE", BOLWrapper.PortOfLoadingDefault);
				AssertEquals("Default Port of Discharge", "OSAKA, JAPAN", BOLWrapper.PortOfDischargeDefault);
			});

			//Main Consol		: AUBNE-SGSIN, Consol Vessel / ConsolVoy
			//MAI Leg in Consol	: SGSIN-JPOSA, MainLeg Vessel / MainLegVoy
			//PRE Leg in Consol	: AUMEL-SGSIN, Pre Vessel / PreLegVoy
			Transport transport2 = AddNewLeg(Consol, PreCarriageType, "AUMEL", "SGSIN", "Pre Vessel", "PreeLegVoy");

			CombineAssertions(delegate
			{
				AssertEquals("MainVesselVoyage", "MainLeg Vessel / MainLegVoy", BOLWrapper.MainVesselVoyage);
				AssertEquals("Main Vessel", "MainLeg Vessel", BOLWrapper.VesselName);
				AssertEquals("Main VoyageNumber", "MainLegVoy", BOLWrapper.VoyageNumber);
				AssertEquals("Port of Loading - from PRE", "AUMEL", BOLWrapper.PortOfLoading.Code);
				AssertEquals("Port of Discharge - from MAIN", "JPOSA", BOLWrapper.PortOfDischarge.Code);
				AssertEquals("Default Port of Loading", "MELBOURNE, AUSTRALIA", BOLWrapper.PortOfLoadingDefault);
				AssertEquals("Default Port of Discharge", "OSAKA, JAPAN", BOLWrapper.PortOfDischargeDefault);
			});

			//Main Consol			: AUBNE-SGSIN, Consol Vessel / ConsolVoy
			//MAI Leg in Consol		: SGSIN-JPOSA, MainLeg Vessel / MainLegVoy
			//PRE Leg in Consol		: AUMEL-SGSIN, Pre Vessel / PreLegVoy
			//MAI Leg in Shipment	: SGSIN-MYPKG, Shipment MAI Vessel / SHIPMAIVOY
			Shipment.Transports.RemoveAndDeleteAll();
			Transport transport3 = AddNewLeg(Shipment, MainVesselType, "SGSIN", "MYPKG", "Shipment MAI Vessel", "SHPMAIVoy");

			CombineAssertions(delegate
			{
				AssertEquals("MainVesselVoyage", "MainLeg Vessel / MainLegVoy", BOLWrapper.MainVesselVoyage);
				AssertEquals("Main Vessel", "MainLeg Vessel", BOLWrapper.VesselName);
				AssertEquals("Main VoyageNumber", "MainLegVoy", BOLWrapper.VoyageNumber);
				AssertEquals("Port of Loading - from PRE in Consol", "AUMEL", BOLWrapper.PortOfLoading.Code);
				AssertEquals("Port of Discharge - from MAI in Consol", "JPOSA", BOLWrapper.PortOfDischarge.Code);
				AssertEquals("Default Port of Loading", "MELBOURNE, AUSTRALIA", BOLWrapper.PortOfLoadingDefault);
				AssertEquals("Default Port of Discharge", "OSAKA, JAPAN", BOLWrapper.PortOfDischargeDefault);
			});

			//Main Consol			: AUBNE-SGSIN, Consol Vessel / ConsolVoy
			//MAI Leg in Consol		: SGSIN-JPOSA, MainLeg Vessel / MainLegVoy
			//PRE Leg in Consol		: AUMEL-SGSIN, Pre Vessel / PreLegVoy
			//MAI Leg in Shipment	: SGSIN-MYPKG, Shipment MAI Vessel / SHIPMAIVOY
			//ONF Leg in Consol		: MYPKG-MYKUL, ONF Vessel / ONFLegVoy
			Transport transport4 = AddNewLeg(Consol, OnForwardingType, "MYPKG", "MYKUL", "ONF Vessel", "ONFLegVoy");

			CombineAssertions(delegate
			{
				AssertEquals("MainVesselVoyage", "MainLeg Vessel / MainLegVoy", BOLWrapper.MainVesselVoyage);
				AssertEquals("Main Vessel", "MainLeg Vessel", BOLWrapper.VesselName);
				AssertEquals("Main VoyageNumber", "MainLegVoy", BOLWrapper.VoyageNumber);
				AssertEquals("Port of Loading - from PRE in Consol", "AUMEL", BOLWrapper.PortOfLoading.Code);
				AssertEquals("Port of Discharge - from MAI in Consol", "JPOSA", BOLWrapper.PortOfDischarge.Code);
				AssertEquals("Default Port of Loading", "MELBOURNE, AUSTRALIA", BOLWrapper.PortOfLoadingDefault);
				AssertEquals("Default Port of Discharge", "OSAKA, JAPAN", BOLWrapper.PortOfDischargeDefault);
			});
		}

		public void TestOnForwardingVesselVoyage()
		{
			AssertEquals("OnForwardingVesselVoyage", "", BOLWrapper.OnForwardingVesselVoyage);

			Transport transport1 = Consol.Transports.AddNew();
			AssertEquals("OnForwardingVesselVoyage", "", BOLWrapper.OnForwardingVesselVoyage);

			transport1.JW_TransportType = OnForwardingType;
			transport1.JW_Vessel = "Vessel 1";
			transport1.JW_VoyageFlight = "1111";
			AssertEquals("OnForwardingVesselVoyage", "Vessel 1 / 1111", BOLWrapper.OnForwardingVesselVoyage);

			Transport transport2 = Shipment.Transports.AddNew();
			transport2.JW_TransportType = OnForwardingType;
			transport2.JW_Vessel = "Vessel 2";
			transport2.JW_VoyageFlight = "2222";
			AssertEquals("OnForwardingVesselVoyage", "Vessel 2 / 2222", BOLWrapper.OnForwardingVesselVoyage);

			transport2.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			AssertEquals("OnForwardingVesselVoyage", "Vessel 1 / 1111", BOLWrapper.OnForwardingVesselVoyage);
		}

		public void TestWhenIsBooking()
		{
			Shipment.JS_IsBooking = true;

			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUMEL";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.JW_Vessel = "Consol Vessel";
			transport.JW_VoyageFlight = "ConsolVoy";

			AssertEquals("Main Vessel", "Consol Vessel", BOLWrapper.VesselName);

			Shipment.JS_IsForwardRegistered = false;
			AssertEquals("MainVesselVoyage", "", BOLWrapper.MainVesselVoyage);
			AssertEquals("Main Vessel", "", BOLWrapper.VesselName);
			AssertEquals("Main VoyageNumber", "", BOLWrapper.VoyageNumber);
			AssertNull("Main Port of Loading", BOLWrapper.PortOfLoading);
			AssertNull("Main Port of Discharge", BOLWrapper.PortOfDischarge);
			AssertEquals("Default Port of Loading", "", BOLWrapper.PortOfLoadingDefault);
			AssertEquals("Default Port of Discharge", "", BOLWrapper.PortOfDischargeDefault);

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Sailing Vessel";

			JobVoyage testVoyage = Factory.NewWithValidTestData<JobVoyage>();
			testVoyage.JV_RV_NKVessel = vessel.RV_FK;
			testVoyage.JV_VoyageFlight = "SailingVoy";

			VoyageOrigin testOrigin = Factory.NewWithValidTestData<VoyageOrigin>();
			testOrigin.JA_RL_NKPortOfLoading = "AUBNE";
			testOrigin.JA_JV = testVoyage.PK;
			VoyageDestination testDestination = Factory.NewWithValidTestData<VoyageDestination>();
			testDestination.JB_RL_NKPortOfDischarge = "SGSIN";

			JobSailing testSailing = Factory.NewWithValidTestData<JobSailing>();
			testSailing.JX_JA = testOrigin.PK;
			testSailing.JX_JB = testDestination.PK;
			Shipment.JS_JX = testSailing.PK;

			AssertEquals("MainVesselVoyage", "Sailing Vessel / SailingVoy", BOLWrapper.MainVesselVoyage);
			AssertEquals("Main Vessel", "Sailing Vessel", BOLWrapper.VesselName);
			AssertEquals("Main VoyageNumber", "SailingVoy", BOLWrapper.VoyageNumber);
			AssertEquals("Main Port of Loading", "AUBNE", BOLWrapper.PortOfLoading.Code);
			AssertEquals("Main Port of Discharge", "SGSIN", BOLWrapper.PortOfDischarge.Code);
			AssertEquals("Default Port of Loading", "BRISBANE, AUSTRALIA", BOLWrapper.PortOfLoadingDefault);
			AssertEquals("Default Port of Discharge", "SINGAPORE", BOLWrapper.PortOfDischargeDefault);
		}

		public void TestPortOfLoading()
		{
			AssertNull("Main Port of Loading", BOLWrapper.PortOfLoading);
			AssertEquals("Default Port of Loading", "", BOLWrapper.PortOfLoadingDefault);

			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUBNE";
			transport.JW_RL_NKDiscPort = "SGSIN";
			AssertEquals("Main Port of Loading", "AUBNE", BOLWrapper.PortOfLoading.Code);
			AssertEquals("Default Port of Loading", "BRISBANE, AUSTRALIA", BOLWrapper.PortOfLoadingDefault);

			Transport transport2 = AddNewLeg(Consol, PreCarriageType, "AUMEL", "SGSIN", "", "");
			AssertEquals("Main Port of Loading", "AUMEL", BOLWrapper.PortOfLoading.Code);
			AssertEquals("Default Port of Loading", "MELBOURNE, AUSTRALIA", BOLWrapper.PortOfLoadingDefault);
		}

		public void TestPortOfDischarge()
		{
			AssertNull("Main Port of Discharge", BOLWrapper.PortOfDischarge);
			AssertEquals("Default Port of Discharge", "", BOLWrapper.PortOfDischargeDefault);

			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUBNE";
			transport.JW_RL_NKDiscPort = "SGSIN";
			AssertEquals("Main Port of Discharge", "SGSIN", BOLWrapper.PortOfDischarge.Code);
			AssertEquals("Default Port of Discharge", "SINGAPORE", BOLWrapper.PortOfDischargeDefault);

			Transport transport2 = AddNewLeg(Consol, OnForwardingType, "AUMEL", "SGSIN", "", "");
			AssertEquals("Main Port of Discharge", "SGSIN", BOLWrapper.PortOfDischarge.Code);
			AssertEquals("Default Port of Discharge", "SINGAPORE", BOLWrapper.PortOfDischargeDefault);
		}

		public void TestPreCarriageLeg()
		{
			Shipment.Transports.RemoveAndDeleteAll();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();
			AssertNull("No pre-carriage leg", BOLWrapper.PreCarriageLeg);

			AddNewLeg(Shipment, PreCarriageType, "AUBNE", "SGSIN", "", "");
			AssertEquals("Pre-carriage leg", "SGSIN", BOLWrapper.PreCarriageLeg.PortOfDischargeCode);

			AddNewLeg(Shipment, PreCarriageType, "SGSIN", "MYPEN", "", "");
			AssertEquals("Pre-carriage leg from first PRE", "SGSIN", BOLWrapper.PreCarriageLeg.PortOfDischargeCode);

			AddNewLeg(Shipment, PreCarriageType, "MYPEN", "HKHKG", "", "");
			AssertEquals("Pre-carriage leg from first PRE", "SGSIN", BOLWrapper.PreCarriageLeg.PortOfDischargeCode);

			Shipment.Transports.RemoveAndDeleteAll();
			AssertNull("No pre-carriage leg", BOLWrapper.PreCarriageLeg);

			AddNewLeg(Shipment, OnForwardingType, "MYPEN", "JPOSA", "", "");
			AssertNull("No pre-carriage leg", BOLWrapper.PreCarriageLeg);

			AddNewLeg(Consol, PreCarriageType, "AUMEL", "AUPER", "", "");
			AssertEquals("Pre-carriage leg from Consol", "AUPER", BOLWrapper.PreCarriageLeg.PortOfDischargeCode);

			AddNewLeg(Consol, PreCarriageType, "AUPER", "SGSIN", "", "");
			AssertEquals("Pre-carriage leg from Consol from first PRE", "AUPER", BOLWrapper.PreCarriageLeg.PortOfDischargeCode);

			AddNewLeg(Consol, PreCarriageType, "SGSIN", "MYPEN", "", "");
			AssertEquals("Pre-carriage leg from Consol from first PRE", "AUPER", BOLWrapper.PreCarriageLeg.PortOfDischargeCode);
		}

		public void TestMainVesselLeg()
		{
			Shipment.Transports.RemoveAndDeleteAll();
			Consol.Transports.RemoveAndDeleteAll();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();
			AssertNull("No main leg", BOLWrapper.MainVesselLeg);

			Transport transport1 = AddNewLeg(Shipment, MainVesselType, "", "SGSIN", "", "");
			AssertNull("Main leg not valid cos there's no vessel", BOLWrapper.MainVesselLeg);

			transport1.JW_Vessel = "Vessel1";
			AssertEquals("Main leg", "SGSIN", BOLWrapper.MainVesselLeg.PortOfDischargeCode);

			Transport transport2 = AddNewLeg(Shipment, MainVesselType, "SGSIN", "MYPEN", "", "");
			AssertEquals("Main leg - still the first MAI, new MAI not valid", "SGSIN", BOLWrapper.MainVesselLeg.PortOfDischargeCode);

			transport2.JW_Vessel = "Vessel2";
			AssertEquals("Main leg - the first MAI", "SGSIN", BOLWrapper.MainVesselLeg.PortOfDischargeCode);

			GlbBranch.CurrentBranch.SetCountry("SG");

			AssertEquals("Main leg - the second MAI", "MYPEN", BOLWrapper.MainVesselLeg.PortOfDischargeCode);

			Shipment.Transports.RemoveAndDeleteAll();
			AssertNull("No main leg", BOLWrapper.MainVesselLeg);

			Transport transport3 = AddNewLeg(Shipment, Core.Constants.TransportPlanningType.Other, "MYPEN", "JPOSA", "Vessel3", "");
			AssertNull("No main leg", BOLWrapper.MainVesselLeg);

			Transport transport4 = AddNewLeg(Consol, MainVesselType, "AUMEL", "AUPER", "", "");
			AssertNull("No main leg - consol MAI not valid cos no vessel defined", BOLWrapper.MainVesselLeg);

			transport4.JW_Vessel = "Vessel4";
			AssertEquals("MainVessel leg from Consol", "AUPER", BOLWrapper.MainVesselLeg.PortOfDischargeCode);

			Transport transport5 = AddNewLeg(Consol, MainVesselType, "AUPER", "SGSIN", "", "");
			AssertEquals("MainVessel leg from Consol - the first MAI", "AUPER", BOLWrapper.MainVesselLeg.PortOfDischargeCode);

			transport5.JW_Vessel = "Vessel5";
			AssertEquals("MainVessel leg from first Consol MAI", "AUPER", BOLWrapper.MainVesselLeg.PortOfDischargeCode);
		}

		public void TestOnForwardingLeg()
		{
			Shipment.Transports.RemoveAndDeleteAll();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();
			AssertNull("No onforwarding leg", BOLWrapper.OnForwardingLeg);

			AddNewLeg(Shipment, OnForwardingType, "AUPER", "AUBNE", "", "");
			AssertEquals("OnForwarding Leg", "AUPER", BOLWrapper.OnForwardingLeg.PortOfLoadingCode);

			AddNewLeg(Shipment, OnForwardingType, "AUBNE", "NZAKL", "", "");
			AssertEquals("OnForwarding Leg - should be the last of the ONF legs", "AUBNE", BOLWrapper.OnForwardingLeg.PortOfLoadingCode);

			Shipment.Transports.RemoveAndDeleteAll();
			AssertNull("No onforwarding leg", BOLWrapper.OnForwardingLeg);

			AddNewLeg(Consol, OnForwardingType, "AUPER", "AUBNE", "", "");
			AssertEquals("OnForwarding Leg", "AUPER", BOLWrapper.OnForwardingLeg.PortOfLoadingCode);

			AddNewLeg(Consol, OnForwardingType, "AUBNE", "NZAKL", "", "");
			AssertEquals("OnForwarding Leg - should be the first of the ONF legs", "AUBNE", BOLWrapper.OnForwardingLeg.PortOfLoadingCode);
		}

		#endregion

		#region Charges

		public void TestChargesAndFollowOn_NON()
		{
			AssertChargesAndFollowOn(DocumentsDataRegistry.HBLChargesDisplayTypes.NoCharges);
		}

		public void TestChargesAndFollowOn_SHW()
		{
			AssertChargesAndFollowOn(DocumentsDataRegistry.HBLChargesDisplayTypes.CollectCharges);
		}

		public void TestChargesAndFollowOn_PPD()
		{
			AssertChargesAndFollowOn(DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidCharges);
		}

		public void TestChargesAndFollowOn_AGR()
		{
			AssertChargesAndFollowOn(DocumentsDataRegistry.HBLChargesDisplayTypes.AsAgreed);
		}

		public void TestChargesAndFollowOn_ALL()
		{
			AssertChargesAndFollowOn(DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges);
		}

		public void TestChargesAndFollowOn_CCL()
		{
			AssertChargesAndFollowOn_DifferentForOriginalAndCopy(DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithCollectCharges);
		}

		public void TestChargesAndFollowOn_CPP()
		{
			AssertChargesAndFollowOn_DifferentForOriginalAndCopy(DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidCharges);
		}

		public void TestChargesAndFollowOn_CAL()
		{
			AssertChargesAndFollowOn_DifferentForOriginalAndCopy(DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidAndCollectCharges);
		}

		public void TestChargesAndFollowOnForLumpSumDisplay_NON()
		{
			DocumentsDataRegistry.Instance.BOLLumpSumDisplayCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Brazil });
			GlbCompany.CurrentCompany.SetCountry("AU");

			SetExchangeRate(0.5M, "USD");
			Factory.Save();

			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.NoCharges;
			Shipment.JS_RL_NKDestination = "BRRIO";

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
			ResetBOLWrapper();

			AssertEquals("Charges should be empty", ZString.Empty, BOLWrapper.PrepaidAndCollectCharges);
			AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
			Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);

			AddChargesToShipment_SameCurrency();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
			ResetBOLWrapper();

			AssertEquals("Charges should be empty", ZString.Empty, BOLWrapper.PrepaidAndCollectCharges);
			AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
			Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);

			AddChargesToShipment_MixedCurrency();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
			ResetBOLWrapper();

			AssertEquals("Charges should be empty", ZString.Empty, BOLWrapper.PrepaidAndCollectCharges);
			AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
			Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);
		}

		public void TestChargesAndFollowOnForLumpSumDisplay_SHW()
		{
			DocumentsDataRegistry.Instance.BOLLumpSumDisplayCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Brazil });
			GlbCompany.CurrentCompany.SetCountry("AU");

			SetExchangeRate(0.5M, "USD");
			Factory.Save();

			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.CollectCharges;
			Shipment.JS_RL_NKDestination = "BRRIO";
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
			ResetBOLWrapper();

			AssertEquals("Charges should be empty", ZString.Empty, BOLWrapper.PrepaidAndCollectCharges);
			AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
			Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);

			AddChargesToShipment_SameCurrency();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
			ResetBOLWrapper();

			AssertEquals("Charges should show lump sum", "FREIGHT LUMP SUM: 40.00 AUD FORTY DOLLARS", BOLWrapper.PrepaidAndCollectCharges);
			AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
			Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);
		}

		public void TestChargesAndFollowOnForLumpSumDisplay_SHW_MixedCurrency()
		{
			DocumentsDataRegistry.Instance.BOLLumpSumDisplayCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Brazil });
			GlbCompany.CurrentCompany.SetCountry("AU");

			Factory.Save();

			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.CollectCharges;
			Shipment.JS_RL_NKDestination = "BRRIO";
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
			ResetBOLWrapper();

			AssertEquals("Charges should be empty", ZString.Empty, BOLWrapper.PrepaidAndCollectCharges);
			AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
			Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);

			var exRates = (BusinessObjectCollection)JobHeaderBisObj["ExchangeRates"];
			var rate = exRates.AddNew();
			rate["JF_RX_NKRateCurrency"] = "USD";
			rate["JF_BaseRate"] = 0.5;

			AddChargesToShipment_MixedCurrency();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
			ResetBOLWrapper();

			AssertEquals("Charges should show lump sum", "FREIGHT LUMP SUM: 35.00 USD THIRTY FIVE DOLLARS", BOLWrapper.PrepaidAndCollectCharges);
			AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
			Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);
		}

		public void TestChargesAndFollowOnForLumpSumDisplay_PPD()
		{
			DocumentsDataRegistry.Instance.BOLLumpSumDisplayCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Brazil });
			GlbCompany.CurrentCompany.SetCountry("AU");

			SetExchangeRate(0.5M, "USD");
			Factory.Save();

			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidCharges;
			Shipment.JS_RL_NKDestination = "BRRIO";
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
			ResetBOLWrapper();

			AssertEquals("Charges should be empty", ZString.Empty, BOLWrapper.PrepaidAndCollectCharges);
			AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
			Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);

			AddChargesToShipment_SameCurrency();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
			ResetBOLWrapper();

			AssertEquals("Charges should show lump sum", "FREIGHT LUMP SUM: 60.00 AUD SIXTY DOLLARS", BOLWrapper.PrepaidAndCollectCharges);
			AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
			Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);
		}

		public void TestChargesAndFollowOnForLumpSumDisplay_PPD_MixedCurrency()
		{
			DocumentsDataRegistry.Instance.BOLLumpSumDisplayCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Brazil });
			GlbCompany.CurrentCompany.SetCountry("AU");

			Factory.Save();

			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidCharges;
			Shipment.JS_RL_NKDestination = "BRRIO";
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
			ResetBOLWrapper();

			AssertEquals("Charges should be empty", ZString.Empty, BOLWrapper.PrepaidAndCollectCharges);
			AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
			Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);

			var exRates = (BusinessObjectCollection)JobHeaderBisObj["ExchangeRates"];
			var rate = exRates.AddNew();
			rate["JF_RX_NKRateCurrency"] = "USD";
			rate["JF_BaseRate"] = 0.5;

			AddChargesToShipment_MixedCurrency();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
			ResetBOLWrapper();

			AssertEquals("Charges should show lump sum", "FREIGHT LUMP SUM: 35.00 USD THIRTY FIVE DOLLARS", BOLWrapper.PrepaidAndCollectCharges);
			AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
			Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);
		}

		public void TestChargesAndFollowOnForLumpSumDisplay_MixedCurrency_FromJobHeader()
		{
			using (DocumentsDataRegistry.Instance.BOLLumpSumDisplayCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.China }))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				SetExchangeRate(0.8M, "USD");
				Factory.Save();

				Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidCharges;
				Shipment.JS_RL_NKDestination = "CNCAN";

				var exRates = (BusinessObjectCollection)JobHeaderBisObj["ExchangeRates"];
				var rate = exRates.AddNew();
				rate["JF_RX_NKRateCurrency"] = "USD";
				rate["JF_BaseRate"] = 0.7;

				AddChargesToShipment_MixedCurrency();

				ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
				ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
				ResetBOLWrapper();

				AssertEquals("FREIGHT LUMP SUM: 45.00 USD FORTY FIVE DOLLARS", BOLWrapper.PrepaidAndCollectCharges);
			}
		}

		public void TestChargesAndFollowOnForLumpSumDisplay_AGR()
		{
			DocumentsDataRegistry.Instance.BOLLumpSumDisplayCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Brazil });
			GlbCompany.CurrentCompany.SetCountry("AU");

			SetExchangeRate(0.5M, "USD");
			Factory.Save();

			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.AsAgreed;
			Shipment.JS_RL_NKDestination = "BRRIO";
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
			ResetBOLWrapper();

			AssertEquals("Charges contains As Agreed", "As Agreed", BOLWrapper.PrepaidAndCollectCharges);
			AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
			Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);

			AddChargesToShipment_SameCurrency();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
			ResetBOLWrapper();

			AssertEquals("Charges contains As Agreed", "As Agreed", BOLWrapper.PrepaidAndCollectCharges);
			AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
			Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);

			AddChargesToShipment_MixedCurrency();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
			ResetBOLWrapper();

			AssertEquals("Charges contains As Agreed", "As Agreed", BOLWrapper.PrepaidAndCollectCharges);
			AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
			Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);
		}

		public void TestChargesAndFollowOnForLumpSumDisplay_ALL()
		{
			DocumentsDataRegistry.Instance.BOLLumpSumDisplayCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Brazil });
			GlbCompany.CurrentCompany.SetCountry("AU");

			SetExchangeRate(0.5M, "USD");
			Factory.Save();

			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges;
			Shipment.JS_RL_NKDestination = "BRRIO";
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
			ResetBOLWrapper();

			AssertEquals("Charges should be empty", ZString.Empty, BOLWrapper.PrepaidAndCollectCharges);
			AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
			Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);

			AddChargesToShipment_SameCurrency();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
			ResetBOLWrapper();

			AssertEquals("Charges should show lump sum", "FREIGHT LUMP SUM: 100.00 AUD ONE HUNDRED DOLLARS", BOLWrapper.PrepaidAndCollectCharges);
			AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
			Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);
		}

		public void TestChargesAndFollowOnForLumpSumDisplay_ALL_MixedCurrency()
		{
			DocumentsDataRegistry.Instance.BOLLumpSumDisplayCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Brazil });
			GlbCompany.CurrentCompany.SetCountry("AU");

			SetExchangeRate(0.5M, "USD");
			Factory.Save();

			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges;
			Shipment.JS_RL_NKDestination = "BRRIO";
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
			ResetBOLWrapper();

			AssertEquals("Charges should be empty", ZString.Empty, BOLWrapper.PrepaidAndCollectCharges);
			AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
			Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);
		}

		public void TestChargesAndFollowOnForLumpSumDisplay_CCL()
		{
			AssertChargesAndFollowOnForLumpSumDisplay_DifferentForOriginalAndCopy(DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithCollectCharges);
		}

		public void TestChargesAndFollowOnForLumpSumDisplay_CCL_MixedCurrency()
		{
			AssertChargesAndFollowOnForLumpSumDisplay_DifferentForOriginalAndCopy_MixedCurrency(DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithCollectCharges);
		}

		public void TestChargesAndFollowOnForLumpSumDisplay_CPP()
		{
			AssertChargesAndFollowOnForLumpSumDisplay_DifferentForOriginalAndCopy(DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidCharges);
		}

		public void TestChargesAndFollowOnForLumpSumDisplay_CPP_MixedCurrency()
		{
			AssertChargesAndFollowOnForLumpSumDisplay_DifferentForOriginalAndCopy_MixedCurrency(DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidCharges);
		}

		public void TestChargesAndFollowOnForLumpSumDisplay_CAL()
		{
			AssertChargesAndFollowOnForLumpSumDisplay_DifferentForOriginalAndCopy(DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidAndCollectCharges);
		}

		public void TestChargesAndFollowOnForLumpSumDisplay_CAL_MixedCurrency()
		{
			AssertChargesAndFollowOnForLumpSumDisplay_DifferentForOriginalAndCopy_MixedCurrency(DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidAndCollectCharges);
		}

		public void TestTotalPrepaidAndCollectChargesCurrency_NullReferenceException()
		{
			Shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var currentOrganization = Factory.NewWithValidTestData<OrgHeader>();
			currentOrganization.OH_Code = "EDICUSBNE";
			currentOrganization.OH_RL_NKClosestPort = ZString.Empty;

			var currentBranch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			currentBranch.GB_OH_OrgProxy = currentOrganization.PK;

			Factory.Save();

			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges;
			var shipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			var wrapper = new DocBillOfLading(shipmentWrapper);

			AssertNoExceptionThrown("Accessing Total Prepaid Charges Currency", () => { var test = wrapper.TotalPrepaidChargesCurrency; });
			AssertNoExceptionThrown("Accessing Total Collect Charges Currency", () => { var test = wrapper.TotalCollectChargesCurrency; });
		}

		public void TestChargesIfChargeIsZero()
		{
			try
			{
				GlbCompany.CurrentCompany.SetCountry("US");

				Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges;
				Shipment.JS_RL_NKDestination = "BRRIO";
				AssertEquals("Charges is empty", ZString.Empty, BOLWrapper.PrepaidAndCollectCharges);

				JobHeader jobHeaderBisObj = new JobHeader.Loader(Shipment).TryLoadOrCreate();
				jobHeaderBisObj.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				jobHeaderBisObj.JH_ParentID = Shipment.PK;
				jobHeaderBisObj.JH_GB = GlbBranch.CurrentBranch.PK;

				GlbDepartment isDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FIS"));
				AssertNotNull("Precondition: FIS Department should exists", isDepartment);
				jobHeaderBisObj.JH_GE = isDepartment.PK;
				jobHeaderBisObj.JH_JobNum = Shipment.JobNumber;
				jobHeaderBisObj.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

				jobHeaderBisObj.AgentCollectPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;

				OrgHeader orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, jobHeaderBisObj.AgentCollectPK));
				AssertNotNull("Precondition: OrgHeader should exists", orgHeader);
				jobHeaderBisObj.LocalChargesPK = orgHeader.PK;

				JobCharge lineCharge1 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 20.000M, CUSDSBChargeCode.PK, "AUD");
				JobCharge lineCharge2 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 00.000M, CUSDSBChargeCode.PK, "AUD");

				ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
				ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 2);
				ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ChargesDiscriptionWidth, 25);

				ResetBOLWrapper();

				AssertEquals("Should return Custom 20.00 AUD", "Customs Disbursement Char   20.00 AUD\n", BOLWrapper.PrepaidAndCollectCharges);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry("AU");
			}
		}

		public void TestChargesTotalAndCurrency()
		{
			Shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			DocBillOfLading wrapper = new DocBillOfLading(ShipmentWrapper);

			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.NoCharges;
			Shipment.JS_RL_NKDestination = "NZAKL";

			JobHeader jobHeaderBisObj = new JobHeader.Loader(Shipment).TryLoadOrCreate();
			jobHeaderBisObj.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeaderBisObj.JH_ParentID = Shipment.PK;
			jobHeaderBisObj.JH_GB = GlbBranch.CurrentBranch.PK;

			GlbDepartment isDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FIS"));
			AssertNotNull("Precondition: FIS Department should exists", isDepartment);
			jobHeaderBisObj.JH_GE = isDepartment.PK;
			jobHeaderBisObj.JH_JobNum = Shipment.JobNumber;
			jobHeaderBisObj.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			jobHeaderBisObj.AgentCollectPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			OrgHeader orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, jobHeaderBisObj.AgentCollectPK));
			AssertNotNull("Precondition: OrgHeader should exists", orgHeader);
			jobHeaderBisObj.LocalChargesPK = orgHeader.PK;

			AccChargeCode originCharge = Factory.NewWithValidTestData<AccChargeCode>();
			originCharge.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 10m, 5m, originCharge.PK, "NZD");
			CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 20m, 10m, originCharge.PK, "NZD");

			AccChargeCode destinationCharge = Factory.NewWithValidTestData<AccChargeCode>();
			destinationCharge.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.AgentCollectPK, 30m, 15m, destinationCharge.PK, "USD");
			CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.AgentCollectPK, 40m, 20m, destinationCharge.PK, "USD");

			AssertEquals("collect charges total", 0m, wrapper.TotalCollectCharges);
			AssertEquals("pre-paid charges total", 0m, wrapper.TotalPrepaidCharges);

			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.AsAgreed;
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			wrapper = new DocBillOfLading(ShipmentWrapper);
			AssertEquals("collect charges total", 0m, wrapper.TotalCollectCharges);
			AssertEquals("pre-paid charges total", 0m, wrapper.TotalPrepaidCharges);

			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges;
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			wrapper = new DocBillOfLading(ShipmentWrapper);
			AssertEquals("collect charges total", 70m, wrapper.TotalCollectCharges);
			AssertEquals("collect charges total currency", "USD", wrapper.TotalCollectChargesCurrency.Code);
			AssertEquals("pre-paid charges total", 30m, wrapper.TotalPrepaidCharges);
			AssertEquals("pre-paid charges total currency", "NZD", wrapper.TotalPrepaidChargesCurrency.Code);
		}

		public void TestChargesTotalAndCurrency2()
		{
			Shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			DocBillOfLading wrapper = new DocBillOfLading(ShipmentWrapper);

			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.NoCharges;
			Shipment.JS_RL_NKDestination = "NZAKL";

			JobHeader jobHeaderBisObj = new JobHeader.Loader(Shipment).TryLoadOrCreate();
			jobHeaderBisObj.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeaderBisObj.JH_ParentID = Shipment.PK;
			jobHeaderBisObj.JH_GB = GlbBranch.CurrentBranch.PK;

			GlbDepartment isDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FIS"));
			AssertNotNull("Precondition: FIS Department should exists", isDepartment);
			jobHeaderBisObj.JH_GE = isDepartment.PK;
			jobHeaderBisObj.JH_JobNum = Shipment.JobNumber;
			jobHeaderBisObj.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			jobHeaderBisObj.AgentCollectPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			OrgHeader orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, jobHeaderBisObj.AgentCollectPK));
			AssertNotNull("Precondition: OrgHeader should exists", orgHeader);
			jobHeaderBisObj.LocalChargesPK = orgHeader.PK;

			AccChargeCode originCharge = Factory.NewWithValidTestData<AccChargeCode>();
			originCharge.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 10m, 5m, originCharge.PK, "NZD");
			CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 20m, 10m, originCharge.PK, "NZD");

			AccChargeCode destinationCharge = Factory.NewWithValidTestData<AccChargeCode>();
			destinationCharge.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.AgentCollectPK, 30m, 15m, destinationCharge.PK, "USD");
			CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.AgentCollectPK, 40m, 20m, destinationCharge.PK, "USD");

			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.AsAgreed;
			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges;

			CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.AgentCollectPK, 100m, 100m, destinationCharge.PK, "AUD");
			CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 100m, 100m, originCharge.PK, "AUD");
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			wrapper = new DocBillOfLading(ShipmentWrapper);

			AssertEquals("collect charges total", 135m, wrapper.TotalCollectCharges);
			AssertEquals("collect charges total currency", "AUD", wrapper.TotalCollectChargesCurrency.Code);
			AssertEquals("pre-paid charges total", 115m, wrapper.TotalPrepaidCharges);
			AssertEquals("pre-paid charges total currency", "AUD", wrapper.TotalPrepaidChargesCurrency.Code);
		}

		public void TestPrepaidCollectTotalShowCorrectAmountWhenCalledMultipleTimes()
		{
			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges;
			AddChargesToShipment_SameCurrency();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();

			AssertEquals(40m, BOLWrapper.TotalCollectCharges);
			AssertEquals(60m, BOLWrapper.TotalPrepaidCharges);

			AssertEquals(40m, BOLWrapper.TotalCollectCharges);
			AssertEquals(60m, BOLWrapper.TotalPrepaidCharges);
		}

		public void TestChargesInEnglishOnly()
		{
			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges;

			JobCharge lineCharge = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.AgentCollectPK, 10.000M, FRTChargeCode.PK, "AUD");
			FRTChargeCode.AC_LocalLanguageDescription = "役";
			lineCharge.JR_Desc = FRTChargeCode.AC_LocalLanguageDescription;

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 3);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ChargesDiscriptionWidth, 25);
			ResetBOLWrapper();

			AssertContains("INTERNATIONAL FREIGHT       10.00 AUD", BOLWrapper.PrepaidAndCollectCharges.ToUpper());
		}

		public void TestFollowOnChargesWithZeroSellAmount()
		{
			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges;
			AssertEquals("Charges should be empty", ZString.Empty, BOLWrapper.PrepaidAndCollectCharges);
			AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
			Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);

			CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.AgentCollectPK, 10.000M, FRTChargeCode.PK, "AUD");
			CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.AgentCollectPK, 20.000M, CUSDSBChargeCode.PK, "AUD");
			CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.AgentCollectPK, 0.000M, FRTChargeCode.PK, "AUD");
			CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.AgentCollectPK, 0.000M, CUSDSBChargeCode.PK, "AUD");
			CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.AgentCollectPK, 40.000M, FRTChargeCode.PK, "AUD");
			CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.AgentCollectPK, 60.000M, CUSDSBChargeCode.PK, "AUD");

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 4);
			ResetBOLWrapper();

			var charges = BOLWrapper.FollowOnPrepaidAndCollectCharges.Split('\n');

			AssertEquals("There should be 1 item in total - an empty line.", 1, charges.Length);
			AssertEquals("There only item in FollowOnCollectCharges collection should be an empty line.", ZString.Empty, charges[0]);
			Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);
		}

		public void TestFollowOnChargesWithZeroSellAmount_MixedChargesAndEmptyLines()
		{
			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges;
			AssertEquals("Charges should be empty", ZString.Empty, BOLWrapper.PrepaidAndCollectCharges);
			AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
			Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);

			CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.AgentCollectPK, 10.000M, FRTChargeCode.PK, "AUD");
			CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.AgentCollectPK, 20.000M, CUSDSBChargeCode.PK, "AUD");
			CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.AgentCollectPK, 0.000M, FRTChargeCode.PK, "AUD");
			CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.AgentCollectPK, 0.000M, CUSDSBChargeCode.PK, "AUD");
			CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.AgentCollectPK, 40.000M, FRTChargeCode.PK, "AUD");
			CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.AgentCollectPK, 60.000M, CUSDSBChargeCode.PK, "AUD");

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 4);
			ResetBOLWrapper();

			CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.AgentCollectPK, 50.000M, FRTChargeCode.PK, "AUD");
			CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.AgentCollectPK, 70.000M, CUSDSBChargeCode.PK, "AUD");

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 4);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ChargesDiscriptionWidth, 25);
			ResetBOLWrapper();

			var charges = BOLWrapper.FollowOnPrepaidAndCollectCharges.Split('\n');

			AssertEquals("There should be 3 items in total - 2 charges and an empty line.", 3, charges.Length);
			AssertEquals(ZString.Empty, charges[2]);
			Assert("Follow On Body Section not empty", BOLWrapper.HasFollowOnBodySections);
			AssertEquals(4, BOLWrapper.FollowOnBodySectionsCollection.Length);
			AssertEquals("Charges", BOLWrapper.FollowOnBodySectionsCollection[0]);
			AssertEquals("Follow on page charges desc defaults to 80 characters", "International Freight                                                              50.00 AUD", charges[0]);
			AssertEquals("Follow on page charges desc defaults to 80 characters", "Customs Disbursement Charges                                                       70.00 AUD", charges[1]);
			AssertEquals("Follow on page charges desc defaults to 80 characters", "International Freight                                                              50.00 AUD", BOLWrapper.FollowOnBodySectionsCollection[1]);
			AssertEquals("Follow on page charges desc defaults to 80 characters", "Customs Disbursement Charges                                                       70.00 AUD", BOLWrapper.FollowOnBodySectionsCollection[2]);
			AssertEquals("", BOLWrapper.FollowOnBodySectionsCollection[3]);
		}

		public void TestChargesFormat()
		{
			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges;
			AssertEquals("Charges should be empty", ZString.Empty, BOLWrapper.PrepaidAndCollectCharges);
			AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
			Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);

			CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.AgentCollectPK, 20.000M, CUSDSBChargeCode.PK, "AUD");
			CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 10.000M, FRTChargeCode.PK, "AUD");

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 2);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ChargesDiscriptionWidth, 25);
			ResetBOLWrapper();

			ZString expectedResult = "Customs Disbursement Char   20.00 AUD\nInternational Freight       10.00 AUD\n";
			ZString actualResult = BOLWrapper.PrepaidAndCollectCharges;
			AssertEquals("Default formatting applies: charges info length should include default number of extra padding spaces", expectedResult.Length, actualResult.Length);
			AssertEquals("Default formatting applies: charges info text should include default number of extra padding spaces", expectedResult, actualResult);

			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ChargesDiscriptionWidth, 21);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ChargesDiscriptionAndPrepaidChargesGap, 5);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.PrepaidChargesColumnWidth, 10);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ChargesDiscriptionAndCollectChargesGap, 20);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.CollectChargesColumnWidth, 10);
			ResetBOLWrapper();

			expectedResult = "Customs Disbursement                      20.00 AUD\nInternational Freight      10.00 AUD\n";
			actualResult = BOLWrapper.PrepaidAndCollectCharges;
			AssertEquals("Formatting constants set: charges info length should include extra padding spaces", expectedResult.Length, actualResult.Length);
			AssertEquals("Formatting constants set: charges info text should include extra padding spaces", expectedResult, actualResult);
		}

		public void TestFollowOnChargesFormat_PrepaidAndCollectCharges()
		{
			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges;
			AssertEquals("Pre-condition: Charges should be empty", ZString.Empty, BOLWrapper.PrepaidAndCollectCharges);
			AssertEquals("Pre-condition: Follow On charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
			Assert("Pre-condition: should not have a follow on section yet", !BOLWrapper.HasFollowOnBodySections);

			CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.AgentCollectPK, 20.000M, CUSDSBChargeCode.PK, "AUD"); //not being tested as it's in the main body
			CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 10.000M, FRTChargeCode.PK, "AUD");
			CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 888.88M, CUSDSBChargeCode.PK, "AUD");

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ChargesDiscriptionAndCollectChargesGap, 1);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ChargesDiscriptionAndPrepaidChargesGap, 1);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.CollectChargesColumnWidth, 10);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.PrepaidChargesColumnWidth, 10);
			ResetBOLWrapper();

			Assert("Expected BOLWrapper to have a follow on body section as only 1 charge row allowed. Follow on text should include second and third charge codes but not the first", BOLWrapper.HasFollowOnBodySections);

			ZString expectedResult = "International Freight                                                             10.00 AUD\nCustoms Disbursement Charges                                                     888.88 AUD\n";
			ZString actualResult = BOLWrapper.FollowOnPrepaidAndCollectCharges;
			AssertEquals("Default formatting applies: follow on charges info length should include default number of extra padding spaces plus the 11 other chars for the gap and price", expectedResult.Length, actualResult.Length);
			AssertEquals("Default formatting applies: follow on charges info text should include default number of extra padding spaces ", expectedResult, actualResult);

			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ChargesDescriptionFollowOnPageWidth, 20);
			ResetBOLWrapper();

			expectedResult = "International Freigh  10.00 AUD\nCustoms Disbursement 888.88 AUD\n";
			actualResult = BOLWrapper.FollowOnPrepaidAndCollectCharges;
			AssertEquals("Default formatting applies: follow on charges info length should include default number of extra padding spaces", expectedResult.Length, actualResult.Length);
			AssertEquals("Default formatting applies: follow on charges info text should include default number of extra padding spaces", expectedResult, actualResult);
		}

		#region Charges - Implementation

		void AssertChargesAndFollowOn(string hBLAWBChargesDisplay)
		{
			Shipment.JS_HBLAWBChargesDisplay = hBLAWBChargesDisplay;

			if (hBLAWBChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.AsAgreed)
			{
				AssertEquals("Charges contains As Agreed", "As Agreed", BOLWrapper.PrepaidAndCollectCharges);
			}
			else
			{
				AssertEquals("Charges should be empty", ZString.Empty, BOLWrapper.PrepaidAndCollectCharges);
			}

			AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
			Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);

			AddChargesToShipment_MixedCurrency();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ChargesDiscriptionWidth, 25);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ChargesDescriptionFollowOnPageWidth, 25);

			ResetBOLWrapper();

			switch (hBLAWBChargesDisplay)
			{
				case DocumentsDataRegistry.HBLChargesDisplayTypes.NoCharges:
					AssertEquals("Charges should be empty", ZString.Empty, BOLWrapper.PrepaidAndCollectCharges);
					AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
					Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);
					break;

				case DocumentsDataRegistry.HBLChargesDisplayTypes.AsAgreed:
					AssertEquals("Charges contains As Agreed", "As Agreed", BOLWrapper.PrepaidAndCollectCharges);
					AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
					Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);
					break;

				case DocumentsDataRegistry.HBLChargesDisplayTypes.CollectCharges:
					Assert("Charges doesn't contain As Agreed", !BOLWrapper.PrepaidAndCollectCharges.Contains("As Agreed"));
					AssertEquals("Charges contains Collect Charges only", "International Freight       10.00 AUD\n", BOLWrapper.PrepaidAndCollectCharges);
					AssertEquals("Follow On Charges contains Collect Charges only", "Customs Disbursement Char   30.00 USD\n", BOLWrapper.FollowOnPrepaidAndCollectCharges);

					Assert("Follow On Body Section not empty", BOLWrapper.HasFollowOnBodySections);
					AssertEquals(3, BOLWrapper.FollowOnBodySectionsCollection.Length);
					AssertEquals("Follow On Body Section contains Collect Charges only", "Charges", BOLWrapper.FollowOnBodySectionsCollection[0]);
					AssertEquals("Follow On Body Section contains Collect Charges only", "Customs Disbursement Char   30.00 USD", BOLWrapper.FollowOnBodySectionsCollection[1]);
					AssertEquals("", BOLWrapper.FollowOnBodySectionsCollection[2]);
					break;

				case DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidCharges:
					Assert("Charges doesn't contain As Agreed", !BOLWrapper.PrepaidAndCollectCharges.Contains("As Agreed"));
					AssertEquals("Charges contains Prepaid Charges only", "Customs Disbursement Char   10.00 USD\n", BOLWrapper.PrepaidAndCollectCharges);
					AssertEquals("Follow On Charges contains Prepaid Charges only", "International Freight       50.00 AUD\n", BOLWrapper.FollowOnPrepaidAndCollectCharges);

					Assert("Follow On Body Section not empty", BOLWrapper.HasFollowOnBodySections);
					AssertEquals(3, BOLWrapper.FollowOnBodySectionsCollection.Length);
					AssertEquals("Follow On Body Section contains Prepaid Charges only", "Charges", BOLWrapper.FollowOnBodySectionsCollection[0]);
					AssertEquals("Follow On Body Section contains Prepaid Charges only", "International Freight       50.00 AUD", BOLWrapper.FollowOnBodySectionsCollection[1]);
					AssertEquals("", BOLWrapper.FollowOnBodySectionsCollection[2]);
					break;

				case DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges:
					Assert("Charges doesn't contain As Agreed", !BOLWrapper.PrepaidAndCollectCharges.Contains("As Agreed"));
					AssertEquals("Charges contains Collect and Prepaid Charges only", "International Freight       10.00 AUD\n", BOLWrapper.PrepaidAndCollectCharges);
					AssertEquals("Follow On Charges contains Collect and Prepaid Charges only", "Customs Disbursement Char   30.00 USD\nCustoms Disbursement Char   10.00 USD\nInternational Freight       50.00 AUD\n", BOLWrapper.FollowOnPrepaidAndCollectCharges);

					Assert("Follow On Body Section not empty", BOLWrapper.HasFollowOnBodySections);
					AssertEquals(5, BOLWrapper.FollowOnBodySectionsCollection.Length);
					AssertEquals("Follow On Body Section contains Collect and Prepaid Charges only", "Charges", BOLWrapper.FollowOnBodySectionsCollection[0]);
					AssertEquals("Follow On Body Section contains Collect and Prepaid Charges only", "Customs Disbursement Char   30.00 USD", BOLWrapper.FollowOnBodySectionsCollection[1]);
					AssertEquals("Follow On Body Section contains Collect and Prepaid Charges only", "Customs Disbursement Char   10.00 USD", BOLWrapper.FollowOnBodySectionsCollection[2]);
					AssertEquals("Follow On Body Section contains Collect and Prepaid Charges only", "International Freight       50.00 AUD", BOLWrapper.FollowOnBodySectionsCollection[3]);
					AssertEquals("", BOLWrapper.FollowOnBodySectionsCollection[4]);
					break;

				default:
					break;
			}
		}

		void AssertChargesAndFollowOn_DifferentForOriginalAndCopy(string hBLAWBChargesDisplay)
		{
			Shipment.JS_HBLAWBChargesDisplay = hBLAWBChargesDisplay;
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ChargesDiscriptionWidth, 25);
			ShipmentWrapper.SetReportNameForTesting("ORIGINAL");
			ResetBOLWrapper();

			AssertEquals("Charges contains As Agreed", "As Agreed", BOLWrapper.PrepaidAndCollectCharges);
			AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
			Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ChargesDiscriptionWidth, 25);
			ShipmentWrapper.SetReportNameForTesting("COPY");
			ResetBOLWrapper();

			AssertEquals("Charges should be empty", ZString.Empty, BOLWrapper.PrepaidAndCollectCharges);
			AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
			Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);

			AddChargesToShipment_MixedCurrency();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ChargesDiscriptionWidth, 25);
			ShipmentWrapper.SetReportNameForTesting("ORIGINAL");
			ResetBOLWrapper();

			AssertEquals("Charges contains As Agreed", "As Agreed", BOLWrapper.PrepaidAndCollectCharges);
			AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
			Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ChargesDiscriptionWidth, 25);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ChargesDescriptionFollowOnPageWidth, 25);
			ShipmentWrapper.SetReportNameForTesting("COPY");
			ResetBOLWrapper();

			switch (hBLAWBChargesDisplay)
			{
				case DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithCollectCharges:
					Assert("Charges doesn't contain As Agreed", !BOLWrapper.PrepaidAndCollectCharges.Contains("As Agreed"));
					AssertEquals("Charges contains Collect Charges only", "International Freight       10.00 AUD\n", BOLWrapper.PrepaidAndCollectCharges);
					AssertEquals("Follow On Charges contains Collect Charges only", "Customs Disbursement Char   30.00 USD\n", BOLWrapper.FollowOnPrepaidAndCollectCharges);

					Assert("Follow On Body Section not empty", BOLWrapper.HasFollowOnBodySections);
					AssertEquals(3, BOLWrapper.FollowOnBodySectionsCollection.Length);
					AssertEquals("Follow On Body Section contains Collect Charges only", "Charges", BOLWrapper.FollowOnBodySectionsCollection[0]);
					AssertEquals("Follow On Body Section contains Collect Charges only", "Customs Disbursement Char   30.00 USD", BOLWrapper.FollowOnBodySectionsCollection[1]);
					AssertEquals("", BOLWrapper.FollowOnBodySectionsCollection[2]);
					break;

				case DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidCharges:
					Assert("Charges doesn't contain As Agreed", !BOLWrapper.PrepaidAndCollectCharges.Contains("As Agreed"));
					AssertEquals("Charges contains Prepaid Charges only", "Customs Disbursement Char   10.00 USD\n", BOLWrapper.PrepaidAndCollectCharges);
					AssertEquals("Follow On Charges contains Prepaid Charges only", "International Freight       50.00 AUD\n", BOLWrapper.FollowOnPrepaidAndCollectCharges);

					Assert("Follow On Body Section not empty", BOLWrapper.HasFollowOnBodySections);
					AssertEquals(3, BOLWrapper.FollowOnBodySectionsCollection.Length);
					AssertEquals("Follow On Body Section contains Prepaid Charges only", "Charges", BOLWrapper.FollowOnBodySectionsCollection[0]);
					AssertEquals("Follow On Body Section contains Prepaid Charges only", "International Freight       50.00 AUD", BOLWrapper.FollowOnBodySectionsCollection[1]);
					AssertEquals("", BOLWrapper.FollowOnBodySectionsCollection[2]);
					break;

				case DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidAndCollectCharges:
					Assert("Charges doesn't contain As Agreed", !BOLWrapper.PrepaidAndCollectCharges.Contains("As Agreed"));
					AssertEquals("Charges contains Collect and Prepaid Charges only", "International Freight       10.00 AUD\n", BOLWrapper.PrepaidAndCollectCharges);
					AssertEquals("Follow On Charges contains Collect and Prepaid Charges only", "Customs Disbursement Char   30.00 USD\nCustoms Disbursement Char   10.00 USD\nInternational Freight       50.00 AUD\n", BOLWrapper.FollowOnPrepaidAndCollectCharges);

					Assert("Follow On Body Section not empty", BOLWrapper.HasFollowOnBodySections);
					AssertEquals(5, BOLWrapper.FollowOnBodySectionsCollection.Length);
					AssertEquals("Follow On Body Section contains Collect and Prepaid Charges only", "Charges", BOLWrapper.FollowOnBodySectionsCollection[0]);
					AssertEquals("Follow On Body Section contains Collect and Prepaid Charges only", "Customs Disbursement Char   30.00 USD", BOLWrapper.FollowOnBodySectionsCollection[1]);
					AssertEquals("Follow On Body Section contains Collect and Prepaid Charges only", "Customs Disbursement Char   10.00 USD", BOLWrapper.FollowOnBodySectionsCollection[2]);
					AssertEquals("Follow On Body Section contains Collect and Prepaid Charges only", "International Freight       50.00 AUD", BOLWrapper.FollowOnBodySectionsCollection[3]);
					AssertEquals("", BOLWrapper.FollowOnBodySectionsCollection[4]);
					break;

				default:
					break;
			}
		}

		void AssertChargesAndFollowOnForLumpSumDisplay_DifferentForOriginalAndCopy(string hBLChargesDisplay)
		{
			DocumentsDataRegistry.Instance.BOLLumpSumDisplayCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Brazil });
			GlbCompany.CurrentCompany.SetCountry("AU");

			SetExchangeRate(0.5M, "USD");
			Factory.Save();

			Shipment.JS_HBLAWBChargesDisplay = hBLChargesDisplay;
			Shipment.JS_RL_NKDestination = "BRRIO";
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
			ShipmentWrapper.SetReportNameForTesting("ORIGINAL");
			ResetBOLWrapper();

			AssertEquals("Charges contains As Agreed", "As Agreed", BOLWrapper.PrepaidAndCollectCharges);
			AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
			Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
			ShipmentWrapper.SetReportNameForTesting("COPY");
			ResetBOLWrapper();

			AssertEquals("Charges should be empty", ZString.Empty, BOLWrapper.PrepaidAndCollectCharges);
			AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
			Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);

			AddChargesToShipment_SameCurrency();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
			ShipmentWrapper.SetReportNameForTesting("ORIGINAL");
			ResetBOLWrapper();

			AssertEquals("Charges contains As Agreed", "As Agreed", BOLWrapper.PrepaidAndCollectCharges);
			AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
			Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
			ShipmentWrapper.SetReportNameForTesting("COPY");
			ResetBOLWrapper();

			if (hBLChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithCollectCharges)
			{
				AssertEquals("Charges should show lump sum", "FREIGHT LUMP SUM: 40.00 AUD FORTY DOLLARS", BOLWrapper.PrepaidAndCollectCharges);
				AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
				Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);
			}
			else if (hBLChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidCharges)
			{
				AssertEquals("Charges should show lump sum", "FREIGHT LUMP SUM: 60.00 AUD SIXTY DOLLARS", BOLWrapper.PrepaidAndCollectCharges);
				AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
				Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);
			}
			else if (hBLChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidAndCollectCharges)
			{
				AssertEquals("Charges should show lump sum", "FREIGHT LUMP SUM: 100.00 AUD ONE HUNDRED DOLLARS", BOLWrapper.PrepaidAndCollectCharges);
				AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
				Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);
			}
		}

		void AssertChargesAndFollowOnForLumpSumDisplay_DifferentForOriginalAndCopy_MixedCurrency(string hBLChargesDisplay)
		{
			DocumentsDataRegistry.Instance.BOLLumpSumDisplayCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Brazil });
			GlbCompany.CurrentCompany.SetCountry("AU");

			Factory.Save();

			Shipment.JS_HBLAWBChargesDisplay = hBLChargesDisplay;
			Shipment.JS_RL_NKDestination = "BRRIO";
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
			ShipmentWrapper.SetReportNameForTesting("ORIGINAL");
			ResetBOLWrapper();

			AssertEquals("Charges contains As Agreed", "As Agreed", BOLWrapper.PrepaidAndCollectCharges);
			AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
			Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
			ShipmentWrapper.SetReportNameForTesting("COPY");
			ResetBOLWrapper();

			AssertEquals("Charges should be empty", ZString.Empty, BOLWrapper.PrepaidAndCollectCharges);
			AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
			Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);

			var exRates = (BusinessObjectCollection)JobHeaderBisObj["ExchangeRates"];
			var usdRate = exRates.AddNew();
			usdRate["JF_RX_NKRateCurrency"] = "USD";
			usdRate["JF_BaseRate"] = 0.5M;

			if (hBLChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithCollectCharges)
			{
				AddChargesToShipment_MixedCurrency();

				ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
				ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
				ShipmentWrapper.SetReportNameForTesting("ORIGINAL");
				ResetBOLWrapper();

				AssertEquals("Charges contains As Agreed", "As Agreed", BOLWrapper.PrepaidAndCollectCharges);
				AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
				Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);

				ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
				ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
				ShipmentWrapper.SetReportNameForTesting("COPY");
				ResetBOLWrapper();

				AssertEquals("Charges should show lump sum", "FREIGHT LUMP SUM: 35.00 USD THIRTY FIVE DOLLARS", BOLWrapper.PrepaidAndCollectCharges);
				AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
				Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);
			}
			else if (hBLChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidCharges)
			{
				AddChargesToShipment_MixedCurrency();

				ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
				ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
				ShipmentWrapper.SetReportNameForTesting("ORIGINAL");
				ResetBOLWrapper();

				AssertEquals("Charges contains As Agreed", "As Agreed", BOLWrapper.PrepaidAndCollectCharges);
				AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
				Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);

				ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
				ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
				ShipmentWrapper.SetReportNameForTesting("COPY");
				ResetBOLWrapper();

				AssertEquals("Charges should show lump sum", "FREIGHT LUMP SUM: 35.00 USD THIRTY FIVE DOLLARS", BOLWrapper.PrepaidAndCollectCharges);
				AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
				Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);
			}
			else if (hBLChargesDisplay == DocumentsDataRegistry.HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidAndCollectCharges)
			{
				AddChargesToShipment_MixedCurrency();
				ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
				ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
				ShipmentWrapper.SetReportNameForTesting("ORIGINAL");
				ResetBOLWrapper();

				AssertEquals("Charges contains As Agreed", "As Agreed", BOLWrapper.PrepaidAndCollectCharges);
				AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
				Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);

				ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
				ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 1);
				ShipmentWrapper.SetReportNameForTesting("COPY");
				ResetBOLWrapper();

				AssertEquals("Charges should show lump sum", "FREIGHT LUMP SUM: 70.00 USD SEVENTY DOLLARS", BOLWrapper.PrepaidAndCollectCharges);
				AssertEquals("Follow On Charges should be empty", ZString.Empty, BOLWrapper.FollowOnPrepaidAndCollectCharges);
				Assert("Follow On Body Section should be empty", !BOLWrapper.HasFollowOnBodySections);
			}
		}

		#endregion

		#region Charges - TESTS FOR OBSOLETE CODE

		public void TestShouldAlignCharges_NON()
		{
			// NON

			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.NoCharges;
			Shipment.JS_INCO = "EXW";
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 4);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.CollectChargesWidth, 20);
			ResetBOLWrapper();

			AssertEquals("Align Charges should return false", false, BOLWrapper.ShouldAlignChargesTestMethod());
		}

		public void TestShouldAlignCharges_SHW()
		{
			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.CollectCharges;
			Shipment.JS_INCO = "CFR";

			JobHeader jobHeaderBisObj = new JobHeader.Loader(Shipment).TryLoadOrCreate();
			jobHeaderBisObj.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeaderBisObj.JH_ParentID = Shipment.PK;
			jobHeaderBisObj.JH_GB = GlbBranch.CurrentBranch.PK;

			jobHeaderBisObj.JH_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "FIS")).PK;
			jobHeaderBisObj.JH_JobNum = Shipment.JobNumber;
			jobHeaderBisObj.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			jobHeaderBisObj.AgentCollectPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			jobHeaderBisObj.LocalChargesPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, jobHeaderBisObj.AgentCollectPK)).PK;

			CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.AgentCollectPK, 10.000M, FRTChargeCode.PK, "AUD");
			CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.AgentCollectPK, 30.000M, CUSDSBChargeCode.PK, "AUD");

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 4);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.CollectChargesWidth, 20);
			ResetBOLWrapper();

			AssertEquals("Align Charges should return true", true, BOLWrapper.ShouldAlignChargesTestMethod());
		}

		public void TestShouldAlignCharges_SHW2()
		{
			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.CollectCharges;
			Shipment.JS_INCO = "CFR";

			JobHeader jobHeaderBisObj = new JobHeader.Loader(Shipment).TryLoadOrCreate();
			jobHeaderBisObj.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeaderBisObj.JH_ParentID = Shipment.PK;
			jobHeaderBisObj.JH_GB = GlbBranch.CurrentBranch.PK;

			jobHeaderBisObj.JH_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "FIS")).PK;
			jobHeaderBisObj.JH_JobNum = Shipment.JobNumber;
			jobHeaderBisObj.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			jobHeaderBisObj.AgentCollectPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			jobHeaderBisObj.LocalChargesPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, jobHeaderBisObj.AgentCollectPK)).PK;

			CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.AgentCollectPK, 10.000M, FRTChargeCode.PK, "AUD");
			CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.AgentCollectPK, 30.000M, CUSDSBChargeCode.PK, "AUD");

			CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.AgentCollectPK, 30.000M, CUSDSBChargeCode.PK, "AUD");

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 4);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.CollectChargesWidth, 20);
			ResetBOLWrapper();

			AssertEquals("Align Charges should return false", false, BOLWrapper.ShouldAlignChargesTestMethod());
		}

		public void TestShouldAlignCharges_All()
		{
			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges;
			Shipment.JS_INCO = "CFR";

			JobHeader jobHeaderBisObj = new JobHeader.Loader(Shipment).TryLoadOrCreate();
			jobHeaderBisObj.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeaderBisObj.JH_ParentID = Shipment.PK;
			jobHeaderBisObj.JH_GB = GlbBranch.CurrentBranch.PK;

			jobHeaderBisObj.JH_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "FIS")).PK;
			jobHeaderBisObj.JH_JobNum = Shipment.JobNumber;
			jobHeaderBisObj.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			jobHeaderBisObj.AgentCollectPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			jobHeaderBisObj.LocalChargesPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, jobHeaderBisObj.AgentCollectPK)).PK;

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 5);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.CollectChargesWidth, 20);
			ResetBOLWrapper();

			AssertEquals("Align Charges should return true", true, BOLWrapper.ShouldAlignChargesTestMethod());
		}

		public void TestShouldAlignCharges_ALL2()
		{
			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges;
			Shipment.JS_INCO = "CFR";

			JobHeader jobHeaderBisObj = new JobHeader.Loader(Shipment).TryLoadOrCreate();
			jobHeaderBisObj.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeaderBisObj.JH_ParentID = Shipment.PK;
			jobHeaderBisObj.JH_GB = GlbBranch.CurrentBranch.PK;

			jobHeaderBisObj.JH_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "FIS")).PK;
			jobHeaderBisObj.JH_JobNum = Shipment.JobNumber;
			jobHeaderBisObj.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			jobHeaderBisObj.AgentCollectPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			jobHeaderBisObj.LocalChargesPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, jobHeaderBisObj.AgentCollectPK)).PK;

			CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.AgentCollectPK, 10.000M, FRTChargeCode.PK, "AUD");
			CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.AgentCollectPK, 30.000M, CUSDSBChargeCode.PK, "AUD");
			CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.AgentCollectPK, 30.000M, CUSDSBChargeCode.PK, "AUD");
			CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 30.000M, CUSDSBChargeCode.PK, "AUD");

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 5);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.CollectChargesWidth, 20);
			ResetBOLWrapper();

			AssertEquals("Align Charges should return false", false, BOLWrapper.ShouldAlignChargesTestMethod());
		}

		public void TestAddTotalLineToAlignedCharges()
		{
			ZString alignCharges = null;
			ZDecimal totalAmount = ZDecimal.Zero;
			ZString currencyCodeUsed = null;
			ZInt maxLength = ZInt.Zero;

			AssertEquals("AddTotalLineToAlignedCharges should return empty", ZString.Empty, BOLWrapper.AddTotalLineToAlignedChargesTestMethod(alignCharges, totalAmount, currencyCodeUsed, maxLength));

			alignCharges = "blah blah\n";
			totalAmount = new ZDecimal(9.95);
			currencyCodeUsed = "AUD";
			maxLength = 22;

			ZString totalLine = "Total:      9.95 AUD";
			ZString expectedResult = alignCharges +
				BOLWrapper.GetSpaces(maxLength - BOLWrapper.TotalLineConstant.Length) + BOLWrapper.TotalLineConstant + "\n" +
				BOLWrapper.GetSpaces(maxLength - totalLine.Length) + totalLine;

			AssertEquals("AddTotalLineToAlignedCharges should return ExpectedResult", expectedResult, BOLWrapper.AddTotalLineToAlignedChargesTestMethod(alignCharges, totalAmount, currencyCodeUsed, maxLength));
		}

		public void TestAlignedCharges()
		{
			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.CollectCharges;
			AssertEquals("Charges is empty", ZString.Empty, BOLWrapper.AlignedCollectCharges);

			Shipment.JS_INCO = "EXW";

			JobHeader jobHeaderBisObj = new JobHeader.Loader(Shipment).TryLoadOrCreate();
			jobHeaderBisObj.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeaderBisObj.JH_ParentID = Shipment.PK;
			jobHeaderBisObj.JH_GB = GlbBranch.CurrentBranch.PK;

			jobHeaderBisObj.JH_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "FIS")).PK;
			jobHeaderBisObj.JH_JobNum = Shipment.JobNumber;
			jobHeaderBisObj.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			jobHeaderBisObj.AgentCollectPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;

			CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.AgentCollectPK, 10.000M, FRTChargeCode.PK, "AUD");
			CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.AgentCollectPK, 30.000M, CUSDSBChargeCode.PK, "AUD");

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 10);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.CollectChargesWidth, 20);
			ResetBOLWrapper();
			ZString[] charges = BOLWrapper.AlignedCollectCharges.Split('\n');

			// Same currencies have been used. It should be aligned based on: TruncatedDescription + EmptySpaces + AmountField
			ZString freightChargeDescTruncated = FRTChargeCode.AC_Desc.ToUpper().SubstringSafe(0, 6);
			ZString cUSDSBChargeDescTruncated = CUSDSBChargeCode.AC_Desc.ToUpper().SubstringSafe(0, 6);
			AssertEquals("First item in Charges should be International Freight+Amount+CurrencyCode", freightChargeDescTruncated + "     10.00 AUD", charges[0].ToUpper());
			AssertEquals("Second item in Charges should be Customs Disbursement+Amount+CurrencyCode", cUSDSBChargeDescTruncated + "     30.00 AUD", charges[1].ToUpper());
			AssertEquals("Last item in Charges should be Total:+TotalAmount+CurrencyCode", "Total:     40.00 AUD", charges[charges.Length - 1]);
		}

		public void TestAlignedCharges_NonTruncated()
		{
			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.CollectCharges;
			AssertEquals("Charges is empty", ZString.Empty, BOLWrapper.AlignedCollectCharges);

			Shipment.JS_INCO = "EXW";

			JobHeader jobHeaderBisObj = new JobHeader.Loader(Shipment).TryLoadOrCreate();
			jobHeaderBisObj.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeaderBisObj.JH_ParentID = Shipment.PK;
			jobHeaderBisObj.JH_GB = GlbBranch.CurrentBranch.PK;

			jobHeaderBisObj.JH_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "FIS")).PK;
			jobHeaderBisObj.JH_JobNum = Shipment.JobNumber;
			jobHeaderBisObj.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			jobHeaderBisObj.AgentCollectPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;

			CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.AgentCollectPK, 10.000M, FRTChargeCode.PK, "AUD");
			CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.AgentCollectPK, 30.000M, CUSDSBChargeCode.PK, "AUD");

			CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.AgentCollectPK, 88.000M, FRTChargeCode.PK, "USD");
			CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.AgentCollectPK, 77.000M, CUSDSBChargeCode.PK, "AUD");

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 10);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.CollectChargesWidth, 20);
			ResetBOLWrapper();
			ZString[] charges = BOLWrapper.AlignedCollectCharges.Split('\n');

			// Different currencies have been used. It should return the same way as Charges (default)
			ZString freightChargeDescTruncated = FRTChargeCode.AC_Desc.ToUpper().SubstringSafe(0, 25);
			ZString cUSDSBChargeDescTruncated = CUSDSBChargeCode.AC_Desc.ToUpper().SubstringSafe(0, 25);
			AssertEquals("First item should contain International Freight+Space+Amount", freightChargeDescTruncated + " " + "10.00 AUD", charges[0].ToUpper());
			AssertEquals("Third item should contain International Freight+Space+Amountt", freightChargeDescTruncated + " " + "88.00 USD", charges[2].ToUpper());
			AssertEquals("Fourth item should contain Customs Disbursement+Space+Amount", cUSDSBChargeDescTruncated + " " + "77.00 AUD", charges[3].ToUpper());

			AssertEquals("Charges should be the same as default", BOLWrapper.CollectCharges, BOLWrapper.AlignedCollectCharges);
		}

		public void TestFollowOnCollectChargesWithZeroSellAmount()
		{
			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges;

			AssertEquals("FollowOn Charges are empty.", ZString.Empty, BOLWrapper.FollowOnCollectCharges);

			JobHeader jobHeader = new JobHeader.Loader(Shipment).TryLoadOrCreate();
			jobHeader.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FIS")).PK;
			jobHeader.AgentCollectPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;

			CreateLineCharge(jobHeader, jobHeader.AgentCollectPK, 1.000M, FRTChargeCode.PK, "AUD");
			CreateLineCharge(jobHeader, jobHeader.AgentCollectPK, 2.000M, CUSDSBChargeCode.PK, "AUD");
			CreateLineCharge(jobHeader, jobHeader.AgentCollectPK, 3.000M, FRTChargeCode.PK, "AUD");
			CreateLineCharge(jobHeader, jobHeader.AgentCollectPK, 0.000M, CUSDSBChargeCode.PK, "AUD");
			CreateLineCharge(jobHeader, jobHeader.AgentCollectPK, 0.000M, FRTChargeCode.PK, "AUD");
			CreateLineCharge(jobHeader, jobHeader.AgentCollectPK, 4.000M, CUSDSBChargeCode.PK, "AUD");
			CreateLineCharge(jobHeader, jobHeader.AgentCollectPK, 5.000M, FRTChargeCode.PK, "AUD");

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 5);
			ResetBOLWrapper();

			ZString[] charges = BOLWrapper.FollowOnCollectCharges.Split('\n');

			AssertEquals("There should be 1 item in total - an empty line.", 1, charges.Length);
			AssertEquals("There only item in FollowOnCollectCharges collection should be an empty line.", ZString.Empty, charges[0]);
		}

		public void TestFollowOnCollectChargesWithZeroSellAmount_MixedChargesAndEmptyLines()
		{
			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges;

			AssertEquals("FollowOn Charges are empty.", ZString.Empty, BOLWrapper.FollowOnCollectCharges);

			JobHeader jobHeader = new JobHeader.Loader(Shipment).TryLoadOrCreate();
			jobHeader.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FIS")).PK;
			jobHeader.AgentCollectPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;

			CreateLineCharge(jobHeader, jobHeader.AgentCollectPK, 1.000M, FRTChargeCode.PK, "AUD");
			CreateLineCharge(jobHeader, jobHeader.AgentCollectPK, 2.000M, CUSDSBChargeCode.PK, "AUD");
			CreateLineCharge(jobHeader, jobHeader.AgentCollectPK, 3.000M, FRTChargeCode.PK, "AUD");
			CreateLineCharge(jobHeader, jobHeader.AgentCollectPK, 0.000M, CUSDSBChargeCode.PK, "AUD");
			CreateLineCharge(jobHeader, jobHeader.AgentCollectPK, 0.000M, FRTChargeCode.PK, "AUD");
			CreateLineCharge(jobHeader, jobHeader.AgentCollectPK, 4.000M, CUSDSBChargeCode.PK, "AUD");
			CreateLineCharge(jobHeader, jobHeader.AgentCollectPK, 5.000M, FRTChargeCode.PK, "AUD");

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 5);
			ResetBOLWrapper();

			CreateLineCharge(jobHeader, jobHeader.AgentCollectPK, 6.000M, CUSDSBChargeCode.PK, "AUD");
			CreateLineCharge(jobHeader, jobHeader.AgentCollectPK, 7.000M, CUSDSBChargeCode.PK, "AUD");

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 5);
			ResetBOLWrapper();

			ZString[] charges = BOLWrapper.FollowOnCollectCharges.Split('\n');

			ZString cUSDSBChargeDescTruncated = CUSDSBChargeCode.AC_Desc.ToUpper().SubstringSafe(0, 25);

			AssertEquals("There should be 3 items in total - 2 charges and an empty line.", 3, charges.Length);
			AssertEquals("First item should contain Customs Disbursement+Amount+CurrencyCode.", cUSDSBChargeDescTruncated + " 6.00 AUD", charges[0].ToUpper());
			AssertEquals("Second item should contain Customs Disbursement+Amount+CurrencyCode.", cUSDSBChargeDescTruncated + " 7.00 AUD", charges[1].ToUpper());
			AssertEquals("Last item in FollowOnCollectCharges collection should be an empty line.", ZString.Empty, charges[charges.Length - 1]);
		}

		public void TestCollectChargesIfChargeIsZero()
		{
			DocumentsDataRegistry.Instance.BOLLumpSumDisplayCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Core.Constants.CountryGuids.Brazil });

			GlbCompany.CurrentCompany.SetCountry("AU");

			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges;
			Shipment.JS_RL_NKDestination = "BRRIO";
			AssertEquals("Charges is empty", ZString.Empty, BOLWrapper.CollectCharges);

			Shipment.JS_INCO = "CFR";

			JobHeader jobHeaderBisObj = new JobHeader.Loader(Shipment).TryLoadOrCreate();
			jobHeaderBisObj.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeaderBisObj.JH_ParentID = Shipment.PK;
			jobHeaderBisObj.JH_GB = GlbBranch.CurrentBranch.PK;

			jobHeaderBisObj.JH_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "FIS")).PK;
			jobHeaderBisObj.JH_JobNum = Shipment.JobNumber;
			jobHeaderBisObj.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			jobHeaderBisObj.AgentCollectPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			jobHeaderBisObj.LocalChargesPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, jobHeaderBisObj.AgentCollectPK)).PK;

			JobCharge lineCharge1 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 20.000M, CUSDSBChargeCode.PK, "AUD");
			JobCharge lineCharge2 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 00.000M, CUSDSBChargeCode.PK, "AUD");

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 2);
			ResetBOLWrapper();

			ZString expectedResult = "FREIGHT LUMP SUM: 20.00 AUD TWENTY DOLLARS";
			AssertEquals("Should return 20.00", expectedResult, BOLWrapper.CollectCharges);
		}

		public void TestCollectChargesFormat()
		{
			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges;
			Shipment.JS_RL_NKDestination = "NZAKL";
			AssertEquals("Charges is empty", ZString.Empty, BOLWrapper.CollectCharges);

			Shipment.JS_INCO = "CFR";

			JobHeader jobHeaderBisObj = new JobHeader.Loader(Shipment).TryLoadOrCreate();
			jobHeaderBisObj.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeaderBisObj.JH_ParentID = Shipment.PK;
			jobHeaderBisObj.JH_GB = GlbBranch.CurrentBranch.PK;

			jobHeaderBisObj.JH_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "FIS")).PK;
			jobHeaderBisObj.JH_JobNum = Shipment.JobNumber;
			jobHeaderBisObj.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			jobHeaderBisObj.AgentCollectPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			jobHeaderBisObj.LocalChargesPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, jobHeaderBisObj.AgentCollectPK)).PK;

			JobCharge lineCharge1 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 20.000M, CUSDSBChargeCode.PK, "AUD");
			JobCharge lineCharge2 = CreateLineCharge(jobHeaderBisObj, jobHeaderBisObj.LocalChargesPK, 00.000M, CUSDSBChargeCode.PK, "AUD");

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 2);
			ResetBOLWrapper();

			ZString expectedResult = "Customs Disbursement Char 20.00 AUD\n";
			ZString actualResult = BOLWrapper.CollectCharges;
			AssertEquals("Length should be constant", expectedResult.Length, actualResult.Length);
			AssertEquals("No columnar formatting", expectedResult, actualResult);

			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ChargeInfoEnableColumnarFormat, true);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ChargeInfoDescriptionWidth, 30);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ChargeInfoAmountWidth, 10);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ChargeInfoGapWidth, 2);
			ResetBOLWrapper();
			expectedResult = "Customs Disbursement Charges     20.00 AUD\n";
			actualResult = BOLWrapper.CollectCharges;
			AssertEquals("Length should be constant (20 + 10 + 2)", expectedResult.Length, actualResult.Length);
			AssertEquals("Should return a columnar format", expectedResult, actualResult);
			AssertEquals("Amount should be right aligned", " 20.00 AUD", actualResult.Substring(ShipmentWrapper.ChargeInfoDescriptionWidth + ShipmentWrapper.ChargeInfoGapWidth, ShipmentWrapper.ChargeInfoAmountWidth));
		}

		public void TestChargesForSHWAndALL()
		{
			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.CollectCharges;
			AssertEquals("Charges is empty", ZString.Empty, BOLWrapper.CollectCharges);

			JobCharge lineCharge1 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.AgentCollectPK, 10.000M, FRTChargeCode.PK, "AUD");
			JobCharge lineCharge2 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.AgentCollectPK, 30.000M, CUSDSBChargeCode.PK, "USD");
			Shipment.JS_INCO = "CFR";

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 3);
			ResetBOLWrapper();

			ZString freightChargeDescTruncated = FRTChargeCode.AC_Desc.ToUpper().SubstringSafe(0, 25);
			ZString cUSDSBChargeDescTruncated = CUSDSBChargeCode.AC_Desc.ToUpper().SubstringSafe(0, 25);
			Assert("Charges should not contain International Freight", !BOLWrapper.CollectCharges.ToUpper().Contains(freightChargeDescTruncated + " " + "10.00 AUD"));
			Assert("Charges contains Customs Disbursement", BOLWrapper.CollectCharges.ToUpper().Contains(cUSDSBChargeDescTruncated + " " + "30.00 USD"));

			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges;

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 3);
			ResetBOLWrapper();

			Assert("Charges contains International Freight", BOLWrapper.CollectCharges.ToUpper().Contains(freightChargeDescTruncated + " " + "10.00 AUD"));
			Assert("Charges contains Customs Disbursement", BOLWrapper.CollectCharges.ToUpper().Contains(cUSDSBChargeDescTruncated + " " + "30.00 USD"));
		}

		#endregion

		#endregion

		#region Marks and Numbers

		public void TestMarksAndNumbers()
		{
			AddMarksAndNumbersToShipment();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersWidth, 31);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 20);
			ResetBOLWrapper();

			AssertEquals("Marks and numbers", "Marks and numbers Line One \r\nLine Two \r\n", BOLWrapper.MarksAndNumbers);
		}

		public void TestMarksAndNumbersAndFollowOnMarksAndNumbers()
		{
			AddMarksAndNumbersToShipment();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersWidth, 5);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 2);
			ResetBOLWrapper();

			AssertEquals("Marks and numbers", "Marks\r\nand \r\n", BOLWrapper.MarksAndNumbers);
			AssertEquals("FollowOnMarksAndNumbers", "numbe\r\nrs \r\nLine \r\nOne \r\nLine \r\nTwo \r\n", BOLWrapper.FollowOnMarksAndNumbers);
		}

		public void TestMarksAndNumbersWithContainersAndNoPackages()
		{
			AddMarksAndNumbersToShipment();
			AddContainerToShipment();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersWidth, 31);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 20);
			ResetBOLWrapper();

			AssertEquals("Marks and numbers with one blank line at start", "\r\nMarks and numbers Line One \r\nLine Two \r\n", BOLWrapper.MarksAndNumbers);

			AddAnotherContainerToShipment();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersWidth, 31);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 20);
			ResetBOLWrapper();
			AssertEquals("Marks and numbers with two blank lines at start", "\r\n\r\nMarks and numbers Line One \r\nLine Two \r\n", BOLWrapper.MarksAndNumbers);
		}

		public void TestMarksAndNumbersWithContainersAndOneLineForPackages()
		{
			AddMarksAndNumbersToShipment();
			AddContainerToShipment();

			Shipment.JS_OuterPacks = 12;
			Shipment.JS_F3_NKPackType = "CTN";

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersWidth, 31);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 20);
			ResetBOLWrapper();

			AssertEquals("Marks and numbers with one blank line at start", "\r\n\r\nMarks and numbers Line One \r\nLine Two \r\n", BOLWrapper.MarksAndNumbers);

			AddAnotherContainerToShipment();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersWidth, 31);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 20);
			ResetBOLWrapper();
			AssertEquals("Marks and numbers with two blank lines at start", "\r\n\r\n\r\nMarks and numbers Line One \r\nLine Two \r\n", BOLWrapper.MarksAndNumbers);
		}

		public void TestMarksAndNumbersWithContainersAndTwoLineForPackages()
		{
			AddMarksAndNumbersToShipment();

			Shipment.JS_OuterPacks = 100;
			Shipment.JS_F3_NKPackType = "CTN";

			AddContainerToShipmentWhereOuterPacksHasBeenAdded();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersWidth, 31);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 20);
			ResetBOLWrapper();

			AssertEquals("Marks and numbers with one blank line at start", "\r\n\r\n\r\nMarks and numbers Line One \r\nLine Two \r\n", BOLWrapper.MarksAndNumbers);

			AddAnotherContainerToShipment();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersWidth, 31);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 20);
			ResetBOLWrapper();
			AssertEquals("Marks and numbers with two blank lines at start", "\r\n\r\n\r\n\r\nMarks and numbers Line One \r\nLine Two \r\n", BOLWrapper.MarksAndNumbers);
		}

		public void TestMarksAndNumbersWithSeparatePackageDetails()
		{
			AddMarksAndNumbersToShipment();
			AddContainerToShipment();

			Shipment.JS_OuterPacks = 12;
			Shipment.JS_F3_NKPackType = "CTN";

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersWidth, 31);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 20);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.IncludePackageCountInBOLGoodsDescription, 0);
			ResetBOLWrapper();

			AssertEquals("Marks and numbers no blank lines at start", "Marks and numbers Line One \r\nLine Two \r\n", BOLWrapper.MarksAndNumbers);

			AddAnotherContainerToShipment();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersWidth, 31);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 20);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.IncludePackageCountInBOLGoodsDescription, 0);
			ResetBOLWrapper();
			AssertEquals("Marks and numbers no blank lines at start", "Marks and numbers Line One \r\nLine Two \r\n", BOLWrapper.MarksAndNumbers);
		}

		#endregion

		#region Number of Packages
		public void TestNumberOfPackagesNoContainer()
		{
			AddGoodsDescriptionToShipment();

			Shipment.JS_OuterPacks = 12;
			Shipment.JS_F3_NKPackType = "CTN";
			Shipment.OuterPackLines[0].JL_PackageCount = 0;

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();

			AssertEquals("Number of Packages", "12 Carton(s)\r\n", BOLWrapper.Packages);
		}

		public void TestNumberOfPackagesWithAllPacklinesAllocatedToContainer()
		{
			AddGoodsDescriptionToShipment();
			AddContainerToShipment();

			Shipment.JS_OuterPacks = 12;
			Shipment.JS_F3_NKPackType = "CTN";

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();

			AssertEquals("Number of Packages", "STC 12 Carton(s)\r\n", BOLWrapper.Packages);

			AddAnotherContainerToShipment();
			ResetBOLWrapper();
			AssertEquals("Number of Packages", "STC 12 Carton(s)\r\n", BOLWrapper.Packages);
		}

		public void TestNumberOfPackagesWithSomePacklinesAllocatedToContainer()
		{
			AddGoodsDescriptionToShipment();

			Shipment.JS_OuterPacks = 100;
			Shipment.JS_F3_NKPackType = "CTN";

			AddContainerToShipmentWhereOuterPacksHasBeenAdded();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();

			AssertEquals("Number of Packages", "STC 12 Carton(s)\r\n and 88 Carton(s) LCL Cargo\r\n", BOLWrapper.Packages);

			AddAnotherContainerToShipment();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();
			AssertEquals("Number of Packages", "STC 24 Carton(s)\r\n and 76 Carton(s) LCL Cargo\r\n", BOLWrapper.Packages);
		}

		#endregion

		#region Goods Description

		public void TestGoodsDescription()
		{
			AddGoodsDescriptionToShipment();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 40);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 20);
			ResetBOLWrapper();

			AssertEquals("Goods description", "description of goods \r\n", BOLWrapper.GoodsDescription);
		}

		public void TestGoodsDescriptionWithBOLClause()
		{
			AddGoodsDescriptionToShipment();

			FreightDataRegistry.Instance.BOLClause.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "BOL Clause text to be added to the goods description.");

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 40);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 20);
			ResetBOLWrapper();

			AssertMultilineASCIIEquals("Goods description + BOL Clause", "description of goods \n \nBOL Clause text to be added to the goods\ndescription. ", BOLWrapper.GoodsDescription);

			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.IncludeBOLClauseInGoodsDescription, false);
			ResetBOLWrapper();

			AssertEquals("Goods description without BOL Clause", "description of goods \r\n", BOLWrapper.GoodsDescription);
		}

		public void TestGoodsDescriptionAndFollowOnGoodsDescription()
		{
			AddGoodsDescriptionToShipment();

			Shipment.JS_RL_NKOrigin = "AUSYD";
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 12);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 1);
			ResetBOLWrapper();

			AssertEquals("Goods description", "description \r\n", BOLWrapper.GoodsDescription);
			AssertEquals("Goods description", "of goods \r\n", BOLWrapper.FollowOnGoodsDescription);

			ZString exportStatement = "statement";
			CountryExportStatementSettingCollection countrySettingCollection = FreightDataRegistry.Instance.ExportStatementSettings.Value;
			CountryExportStatementSetting countrySetting = countrySettingCollection.AddNew();
			countrySetting.CountryCode = Core.Constants.CountryCodes.Australia;
			ExportStatementSetting statementSetting = countrySetting.Statements.AddNew();
			statementSetting.Code = "AES";
			statementSetting.Statement = exportStatement;
			statementSetting.Visibility = "MAN";
			statementSetting.UseOnHouseBillOfLading = true;
			FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, countrySettingCollection);
			ResetBOLWrapper();
			AssertEquals("Goods description", "of goods \r\n" + exportStatement + " \r\n", BOLWrapper.FollowOnGoodsDescription);
		}

		public void TestGoodsDescriptionWtihContainersAndNoPackages()
		{
			AddGoodsDescriptionToShipment();

			AddContainerToShipment();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 40);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 20);
			ResetBOLWrapper();

			AssertEquals("Goods description", "1 x 20NOR CONTAINER \r\ndescription of goods \r\n", BOLWrapper.GoodsDescription);

			AddAnotherContainerToShipment();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 40);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 20);
			ResetBOLWrapper();

			AssertEquals("Goods description", "1 x 20NOR CONTAINER \r\n1 x 40FR CONTAINER \r\ndescription of goods \r\n", BOLWrapper.GoodsDescription);
		}

		public void TestGoodsDescriptionWtihSeparateContainerCountColumn()
		{
			Shipment.JS_TransportMode = "SEA";
			Shipment.JS_PackingMode = "FCL";
			AddGoodsDescriptionToShipment();
			AddContainerToShipment();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 40);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ShowPackageCount, 1);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 20);
			ResetBOLWrapper();

			AssertEquals("Goods description", "x 20NOR CONTAINER \r\ndescription of goods \r\n", BOLWrapper.GoodsDescription);

			AddAnotherContainerToShipment();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 40);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ShowPackageCount, 1);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 20);
			ResetBOLWrapper();
			AssertEquals("Goods description", "x 20NOR CONTAINER \r\nx 40FR CONTAINER \r\ndescription of goods \r\n", BOLWrapper.GoodsDescription);

			ResetBOLWrapper();
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ShowPackageCount, 0);
			AssertEquals("Goods description", "1 x 20NOR CONTAINER \r\n1 x 40FR CONTAINER \r\ndescription of goods \r\n", BOLWrapper.GoodsDescription);

			Shipment.JS_TransportMode = "AIR";
			ResetBOLWrapper();
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ShowPackageCount, 1);
			AssertEquals("Goods description", "1 x 20NOR CONTAINER \r\n1 x 40FR CONTAINER \r\ndescription of goods \r\n", BOLWrapper.GoodsDescription);

			Shipment.JS_TransportMode = "SEA";
			Shipment.JS_PackingMode = "LCL";
			ResetBOLWrapper();
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ShowPackageCount, 1);
			AssertEquals("Goods description", "1 x 20NOR CONTAINER \r\n1 x 40FR CONTAINER \r\ndescription of goods \r\n", BOLWrapper.GoodsDescription);

			Shipment.JS_TransportMode = "SEA";
			Shipment.JS_PackingMode = "FCL";
			ResetBOLWrapper();
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ShowPackageCount, 1);
			AssertEquals("Goods description", "x 20NOR CONTAINER \r\nx 40FR CONTAINER \r\ndescription of goods \r\n", BOLWrapper.GoodsDescription);
		}

		public void TestGoodsDescriptionWithPackagesAndNoContainers()
		{
			AddGoodsDescriptionToShipment();

			Shipment.JS_OuterPacks = 12;
			Shipment.JS_F3_NKPackType = "CTN";
			Shipment.OuterPackLines[0].JL_PackageCount = 0;

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 40);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 20);
			ResetBOLWrapper();

			AssertEquals("Goods description", "12 Carton(s) \r\ndescription of goods \r\n", BOLWrapper.GoodsDescription);
		}

		public void TestGoodsDescriptionWithContainerAndOneLineForPackages()
		{
			AddGoodsDescriptionToShipment();
			AddContainerToShipment();

			Shipment.JS_OuterPacks = 12;
			Shipment.JS_F3_NKPackType = "CTN";

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 40);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 20);
			ResetBOLWrapper();

			AssertEquals("Goods description", "1 x 20NOR CONTAINER \r\nSTC 12 Carton(s) \r\ndescription of goods \r\n", BOLWrapper.GoodsDescription);

			AddAnotherContainerToShipment();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 40);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 20);
			ResetBOLWrapper();
			AssertEquals("Goods description", "1 x 20NOR CONTAINER \r\n1 x 40FR CONTAINER \r\nSTC 12 Carton(s) \r\ndescription of goods \r\n", BOLWrapper.GoodsDescription);
		}

		public void TestGoodsDescriptionWithContainerAndTwoLinesForPackages()
		{
			AddGoodsDescriptionToShipment();

			Shipment.JS_OuterPacks = 100;
			Shipment.JS_F3_NKPackType = "CTN";

			AddContainerToShipmentWhereOuterPacksHasBeenAdded();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 40);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 20);
			ResetBOLWrapper();

			AssertEquals("Goods description", "1 x 20NOR CONTAINER \r\nSTC 12 Carton(s) \r\n and 88 Carton(s) LCL Cargo \r\ndescription of goods \r\n", BOLWrapper.GoodsDescription);

			AddAnotherContainerToShipment();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 40);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 20);
			ResetBOLWrapper();
			AssertEquals("Goods description", "1 x 20NOR CONTAINER \r\n1 x 40FR CONTAINER \r\nSTC 24 Carton(s) \r\n and 76 Carton(s) LCL Cargo \r\ndescription of goods \r\n", BOLWrapper.GoodsDescription);
		}

		public void TestGoodsDescriptionWithoutPackageDetails()
		{
			AddGoodsDescriptionToShipment();

			Shipment.JS_OuterPacks = 12;
			Shipment.JS_F3_NKPackType = "CTN";
			Shipment.OuterPackLines[0].JL_PackageCount = 0;

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 40);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 20);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.IncludePackageCountInBOLGoodsDescription, 0);
			ResetBOLWrapper();

			AssertEquals("Goods description - without package details", "description of goods \r\n", BOLWrapper.GoodsDescription);

			AddContainerToShipment();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 40);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 20);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.IncludePackageCountInBOLGoodsDescription, 0);
			ResetBOLWrapper();

			AssertEquals("Goods description - without package details", "1 x 20NOR CONTAINER \r\ndescription of goods \r\n", BOLWrapper.GoodsDescription);

			AddAnotherContainerToShipment();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 40);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 20);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.IncludePackageCountInBOLGoodsDescription, 0);
			ResetBOLWrapper();
			AssertEquals("Goods description - without package details", "1 x 20NOR CONTAINER \r\n1 x 40FR CONTAINER \r\ndescription of goods \r\n", BOLWrapper.GoodsDescription);
		}

		public void TestGoodsDescriptionWithContainersAndNoPackages()
		{
			AddGoodsDescriptionToShipment();

			Shipment.JS_OuterPacks = 100;
			Shipment.JS_F3_NKPackType = "CTN";

			AddContainerToShipmentWhereOuterPacksHasBeenAdded();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 40);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 20);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.IncludePackageCountInBOLGoodsDescription, 0);
			ResetBOLWrapper();

			AssertEquals("Goods description", "1 x 20NOR CONTAINER \r\ndescription of goods \r\n", BOLWrapper.GoodsDescription);

			AddAnotherContainerToShipment();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 40);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 20);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.IncludePackageCountInBOLGoodsDescription, 0);
			ResetBOLWrapper();
			AssertEquals("Goods description", "1 x 20NOR CONTAINER \r\n1 x 40FR CONTAINER \r\ndescription of goods \r\n", BOLWrapper.GoodsDescription);
		}

		#endregion

		#region Containers

		public void TestConatinerAtmosphereControlColumns()
		{
			AddAnotherContainerToShipment();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 10);
			ResetBOLWrapper();

			const string expected1 =
				"Container       Seals                Type     Weight(KG)     Volume(M3)     Packages     Mode      \r\n" +
				"CONTAINER 2     Seal Num             40FR     23             50             12 PLT       CFS       " +
				"";

			AssertMultilineASCIIEquals("",
				expected1,
				BOLWrapper.ContainersColumnHeaders + "\n" +
				BOLWrapper.MainBodyBottomColumnSection);

			const string expected2 =
				"Container       Seals                Type     Weight(KG)     Volume(M3)     Packages     Mode       Temp.\r\n" +
				"CONTAINER 2     Seal Num             40FR     23             50             12 PLT       CFS        5.5C " +
				"";

			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerTemperatureSetting, "Y");
			ResetBOLWrapper();

			AssertMultilineASCIIEquals("",
				expected2,
				BOLWrapper.ContainersColumnHeaders + "\n" +
				BOLWrapper.MainBodyBottomColumnSection);

			const string expected3 =
				"Container       Seals                Type     Weight(KG)     Volume(M3)     Packages     Mode       Temp. Humidity\r\n" +
				"CONTAINER 2     Seal Num             40FR     23             50             12 PLT       CFS        5.5C  10%     " +
				"";

			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerHumiditySetting, "Y");
			ResetBOLWrapper();

			AssertMultilineASCIIEquals("",
				expected3,
				BOLWrapper.ContainersColumnHeaders + "\n" +
				BOLWrapper.MainBodyBottomColumnSection);
		}

		public void TestContainersWhenNotAMasterBOLAndDeliveryModeNotOverridenAndStartsWithCY()
		{
			AddContainerToShipment();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 4);
			ResetBOLWrapper();

			AssertContainerFields("CONTAINER 1\n", "Seal Num\n", "20NOR\n", ExpectedWeight + "\n", ExpectedVolume + "\n",
				"12\n", "CY/CY*\n", "*\n", "CY/CY");
		}

		public void TestContainersWhenNotAMasterBOLAndDeliveryModeNotOverridenAndDoesNotStartWithCY()
		{
			AddAnotherContainerToShipment();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 4);
			ResetBOLWrapper();

			AssertContainerFields("CONTAINER 2\n", "Seal Num\n", "40FR\n", ExpectedWeight + "\n", ExpectedVolume + "\n",
				"12\n", "CFS\n", "\n", "CFS");
		}

		public void TestContainersWhenNotAMasterBOLAndDeliveryModeOverridenAndStartsWithCY()
		{
			AddContainerToShipment();

			Shipment.JS_HBLContainerPackModeOverride = "CYSHIP";
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 4);
			ResetBOLWrapper();

			AssertContainerFields("CONTAINER 1\n", "Seal Num\n", "20NOR\n", ExpectedWeight + "\n", ExpectedVolume + "\n",
				"12\n", "CYSHIP*\n", "*\n", "CYSHIP");
		}

		public void TestContainersWhenNotAMasterBOLAndDeliveryModeOverridenAndDoesNotStartWithCY()
		{
			AddAnotherContainerToShipment();

			Shipment.JS_HBLContainerPackModeOverride = "MODE";
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 4);
			ResetBOLWrapper();

			AssertContainerFields("CONTAINER 2\n", "Seal Num\n", "40FR\n", ExpectedWeight + "\n", ExpectedVolume + "\n",
				"12\n", "MODE\n", "\n", "MODE");
		}

		public void TestContainersWhenNotAMasterBOLAndSealNumberIsEmpty()
		{
			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "40FR"));
			var shipPackLine = (PackLine)Shipment.OuterPackLines.AddNew();
			shipPackLine.JL_PackageCount = 12;
			shipPackLine.JL_ActualWeight = 23;
			shipPackLine.JL_ActualVolume = 50;

			CommonContainer container = Consol.Containers.AddNew();
			container.JC_RC = containerCode.PK;
			container.JC_ContainerNum = "Container 2";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_DeliveryMode = "CFS";
			shipPackLine.SetContainer(Consol, container);
			Factory.Save();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 4);
			ResetBOLWrapper();
			ZString expectedWeight = ShipmentWrapper.FormatNumber(shipPackLine.JL_ActualWeight, Env.Registry.WeightMinimumDecimalPlacesToDisplay);
			ZString expectedVolume = ShipmentWrapper.FormatNumber(shipPackLine.JL_ActualVolume, Env.Registry.VolumeMinimumDecimalPlacesToDisplay);

			AssertContainerFields("CONTAINER 2\n", "-\n", "40FR\n", expectedWeight + "\n", expectedVolume + "\n",
				"12\n", "CFS\n", "\n", "CFS");
		}

		public void TestContainersWhenNotAMasterBOLAndContainerTypeIsEmpty()
		{
			var shipPackLine = (PackLine)Shipment.OuterPackLines.AddNew();
			shipPackLine.JL_PackageCount = 12;
			shipPackLine.JL_ActualWeight = 23;
			shipPackLine.JL_ActualVolume = 50;

			CommonContainer container = Consol.Containers.AddNew();
			container.JC_ContainerNum = "Container 2";
			container.JC_SealNum = "Seal Num";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_DeliveryMode = "CFS";
			shipPackLine.SetContainer(Consol, container);
			Factory.Save();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 4);
			ResetBOLWrapper();
			ZString expectedWeight = ShipmentWrapper.FormatNumber(shipPackLine.JL_ActualWeight, Env.Registry.WeightMinimumDecimalPlacesToDisplay);
			ZString expectedVolume = ShipmentWrapper.FormatNumber(shipPackLine.JL_ActualVolume, Env.Registry.VolumeMinimumDecimalPlacesToDisplay);

			AssertContainerFields("CONTAINER 2\n", "Seal Num\n", "-\n", expectedWeight + "\n", expectedVolume + "\n",
				"12\n", "CFS\n", "\n", "CFS");
		}

		public void TestContainersWhenNotAMasterBOLAndContainerWeightIsEmpty()
		{
			RefContainer containerCode = new RefContainer.Loader(Factory).LoadFromCode("40FR");
			var shipPackLine = (PackLine)Shipment.OuterPackLines.AddNew();
			shipPackLine.JL_PackageCount = 12;
			shipPackLine.JL_ActualVolume = 50;

			CommonContainer container = Consol.Containers.AddNew();
			container.JC_RC = containerCode.PK;
			container.JC_ContainerNum = "Container 2";
			container.JC_SealNum = "Seal Num";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_DeliveryMode = "CFS";
			shipPackLine.SetContainer(Consol, container);
			Factory.Save();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 4);
			ResetBOLWrapper();
			ZString expectedVolume = ShipmentWrapper.FormatNumber(shipPackLine.JL_ActualVolume, Env.Registry.VolumeMinimumDecimalPlacesToDisplay);

			AssertContainerFields("CONTAINER 2\n", "Seal Num\n", "40FR\n", "-\n", expectedVolume + "\n",
				"12\n", "CFS\n", "\n", "CFS");
		}

		public void TestContainersWhenNotAMasterBOLAndContainerVolumeIsEmpty()
		{
			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "40FR"));
			var shipPackLine = (PackLine)Shipment.OuterPackLines.AddNew();
			shipPackLine.JL_PackageCount = 12;
			shipPackLine.JL_ActualWeight = 23;

			CommonContainer container = Consol.Containers.AddNew();
			container.JC_RC = containerCode.PK;
			container.JC_ContainerNum = "Container 2";
			container.JC_SealNum = "Seal Num";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_DeliveryMode = "CFS";
			shipPackLine.SetContainer(Consol, container);
			Factory.Save();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 4);
			ResetBOLWrapper();
			ZString expectedWeight = ShipmentWrapper.FormatNumber(shipPackLine.JL_ActualWeight, Env.Registry.WeightMinimumDecimalPlacesToDisplay);

			AssertContainerFields("CONTAINER 2\n", "Seal Num\n", "40FR\n", expectedWeight + "\n", "-\n",
				"12\n", "CFS\n", "\n", "CFS");
		}

		public void TestContainersWhenNotAMasterBOLAndContainerPackagesIsEmpty()
		{
			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "40FR"));
			var shipPackLine = (PackLine)Shipment.OuterPackLines.AddNew();
			shipPackLine.JL_ActualVolume = 50;
			shipPackLine.JL_ActualWeight = 23;

			CommonContainer container = Consol.Containers.AddNew();
			container.JC_RC = containerCode.PK;
			container.JC_ContainerNum = "Container 2";
			container.JC_SealNum = "Seal Num";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_DeliveryMode = "CFS";
			shipPackLine.SetContainer(Consol, container);
			Factory.Save();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 4);
			ResetBOLWrapper();
			ZString expectedWeight = ShipmentWrapper.FormatNumber(shipPackLine.JL_ActualWeight, Env.Registry.WeightMinimumDecimalPlacesToDisplay);
			ZString expectedVolume = ShipmentWrapper.FormatNumber(shipPackLine.JL_ActualVolume, Env.Registry.VolumeMinimumDecimalPlacesToDisplay);

			AssertContainerFields("CONTAINER 2\n", "Seal Num\n", "40FR\n", expectedWeight + "\n", expectedVolume + "\n",
				"-\n", "CFS\n", "\n", "CFS");
		}

		public void TestContainersWhenNotAMasterBOLAndDeliveryModeIsEmpty()
		{
			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "40FR"));
			var shipPackLine = (PackLine)Shipment.OuterPackLines.AddNew();
			shipPackLine.JL_ActualVolume = 50;
			shipPackLine.JL_ActualWeight = 23;
			shipPackLine.JL_PackageCount = 12;

			CommonContainer container = Consol.Containers.AddNew();
			container.JC_RC = containerCode.PK;
			container.JC_ContainerNum = "Container 2";
			container.JC_SealNum = "Seal Num";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			shipPackLine.SetContainer(Consol, container);
			Factory.Save();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 4);
			ResetBOLWrapper();
			ZString expectedWeight = ShipmentWrapper.FormatNumber(shipPackLine.JL_ActualWeight, Env.Registry.WeightMinimumDecimalPlacesToDisplay);
			ZString expectedVolume = ShipmentWrapper.FormatNumber(shipPackLine.JL_ActualVolume, Env.Registry.VolumeMinimumDecimalPlacesToDisplay);

			AssertContainerFields("CONTAINER 2\n", "Seal Num\n", "40FR\n", expectedWeight + "\n", expectedVolume + "\n",
				"12\n", "-\n", "\n", "");
		}

		public void TestContainersWhenNotAMasterBOLAndOnlyContainerNumberIsNotEmpty()
		{
			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "40FR"));
			var shipPackLine = (PackLine)Shipment.OuterPackLines.AddNew();

			CommonContainer container = Consol.Containers.AddNew();
			container.JC_ContainerNum = "Container 2";
			shipPackLine.SetContainer(Consol, container);
			Factory.Save();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 4);
			ResetBOLWrapper();

			AssertContainerFields("CONTAINER 2\n", "-\n", "-\n", "-\n", "-\n", "-\n", "-\n", "\n", "");
		}

		public void TestMultipleContainers()
		{
			AddAnotherContainerToShipment();

			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "20FR"));
			var shipPackLine = (PackLine)Shipment.OuterPackLines.AddNew();
			shipPackLine.JL_PackageCount = 24;
			shipPackLine.JL_ActualWeight = 50;
			shipPackLine.JL_ActualVolume = 100;

			CommonContainer container = Consol.Containers.AddNew();
			container.JC_RC = containerCode.PK;
			container.JC_ContainerNum = "CONTAINER 3";
			container.JC_SealNum = "Seal Num";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_DeliveryMode = "CY/CY";
			shipPackLine.SetContainer(Consol, container);
			Factory.Save();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 2);
			ResetBOLWrapper();
			ZString expectedWeight2 = ShipmentWrapper.FormatNumber(shipPackLine.JL_ActualWeight, Env.Registry.WeightMinimumDecimalPlacesToDisplay);
			ZString expectedVolume2 = ShipmentWrapper.FormatNumber(shipPackLine.JL_ActualVolume, Env.Registry.VolumeMinimumDecimalPlacesToDisplay);

			//Containers
			AssertContainerFields("CONTAINER 2\nCONTAINER 3\n", "Seal Num\nSeal Num\n", "40FR\n20FR\n",
				ExpectedWeight + "\n" + expectedWeight2 + "\n", ExpectedVolume + "\n" + expectedVolume2 + "\n",
				"12\n24\n", "CFS\nCY/CY*\n", "\n*\n", "");
		}

		public void TestContainersWeightRounding()
		{
			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "40FR"));
			var shipPackLine = (PackLine)Shipment.OuterPackLines.AddNew();
			shipPackLine.JL_PackageCount = 12;
			shipPackLine.JL_ActualWeight = 23.000M;
			shipPackLine.JL_ActualVolume = 50M;

			CommonContainer container = Consol.Containers.AddNew();
			container.JC_RC = containerCode.PK;
			container.JC_ContainerNum = "Container 2";
			container.JC_SealNum = "Seal Num";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_DeliveryMode = "CFS";
			shipPackLine.SetContainer(Consol, container);
			Factory.Save();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 4);
			ResetBOLWrapper();
			ZString expectedWeight = ShipmentWrapper.FormatNumber(shipPackLine.JL_ActualWeight, Env.Registry.WeightMinimumDecimalPlacesToDisplay);
			ZString expectedVolume = ShipmentWrapper.FormatNumber(shipPackLine.JL_ActualVolume, Env.Registry.VolumeMinimumDecimalPlacesToDisplay);

			AssertContainerFields("CONTAINER 2\n", "Seal Num\n", "40FR\n", expectedWeight + "\n", expectedVolume + "\n",
				"12\n", "CFS\n", "\n", "CFS");

			shipPackLine.JL_ActualWeight = 23.123456M;
			Factory.Save();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();
			expectedWeight = ShipmentWrapper.FormatNumber(shipPackLine.JL_ActualWeight, Env.Registry.WeightMinimumDecimalPlacesToDisplay);

			AssertContainerFields("CONTAINER 2\n", "Seal Num\n", "40FR\n", expectedWeight + "\n", expectedVolume + "\n",
				"12\n", "CFS\n", "\n", "CFS");
		}

		public void TestContainersVolumeRounding()
		{
			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "40FR"));
			var shipPackLine = (PackLine)Shipment.OuterPackLines.AddNew();
			shipPackLine.JL_PackageCount = 12;
			shipPackLine.JL_ActualWeight = 23M;
			shipPackLine.JL_ActualVolume = 50.000M;

			CommonContainer container = Consol.Containers.AddNew();
			container.JC_RC = containerCode.PK;
			container.JC_ContainerNum = "Container 2";
			container.JC_SealNum = "Seal Num";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_DeliveryMode = "CFS";
			shipPackLine.SetContainer(Consol, container);
			Factory.Save();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 4);
			ResetBOLWrapper();
			ZString expectedWeight = ShipmentWrapper.FormatNumber(shipPackLine.JL_ActualWeight, Env.Registry.WeightMinimumDecimalPlacesToDisplay);
			ZString expectedVolume = ShipmentWrapper.FormatNumber(shipPackLine.JL_ActualVolume, Env.Registry.VolumeMinimumDecimalPlacesToDisplay);

			AssertContainerFields("CONTAINER 2\n", "Seal Num\n", "40FR\n", expectedWeight + "\n", expectedVolume + "\n",
				"12\n", "CFS\n", "\n", "CFS");

			shipPackLine.JL_ActualVolume = 50.123654M;
			Factory.Save();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();
			expectedVolume = ShipmentWrapper.FormatNumber(shipPackLine.JL_ActualVolume, Env.Registry.VolumeMinimumDecimalPlacesToDisplay);

			AssertContainerFields("CONTAINER 2\n", "Seal Num\n", "40FR\n", expectedWeight + "\n", expectedVolume + "\n",
				"12\n", "CFS\n", "\n", "CFS");
		}

		public void TestFollowOnContainersWhenNotAMasterBOLAndDeliveryModeNotOverridenAndStartsWithCY()
		{
			AddContainerToShipment();

			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "20FR"));
			var shipPackLine = (PackLine)Shipment.OuterPackLines.AddNew();
			shipPackLine.JL_PackageCount = 12;
			shipPackLine.JL_ActualWeight = 23;
			shipPackLine.JL_ActualVolume = 50;

			CommonContainer container = Consol.Containers.AddNew();
			container.JC_RC = containerCode.PK;
			container.JC_ContainerNum = "FOLLOWON";
			container.JC_SealNum = "Seal Num";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_DeliveryMode = "CY/CY";
			shipPackLine.SetContainer(Consol, container);
			Factory.Save();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 1);
			ResetBOLWrapper();

			AssertContainerFields("CONTAINER 1\n", "Seal Num\n", "20NOR\n", ExpectedWeight + "\n", ExpectedVolume + "\n",
				"12\n", "CY/CY*\n", "*\n", "CY/CY");
			AssertFollowOnContainerFields("FOLLOWON\n", "Seal Num\n", "20FR\n", ExpectedWeight + "\n", ExpectedVolume + "\n",
				"12\n", "CY/CY*\n", "*\n", "CY/CY");
		}

		public void TestFollowOnContainersWhenNotAMasterBOLAndDeliveryModeNotOverridenAndDoesNotStartWithCY()
		{
			AddAnotherContainerToShipment();

			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "40FR"));
			var shipPackLine = (PackLine)Shipment.OuterPackLines.AddNew();
			shipPackLine.JL_PackageCount = 12;
			shipPackLine.JL_ActualWeight = 23;
			shipPackLine.JL_ActualVolume = 50;

			CommonContainer container = Consol.Containers.AddNew();
			container.JC_RC = containerCode.PK;
			container.JC_ContainerNum = "FOLLOWON";
			container.JC_SealNum = "Seal Num";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_DeliveryMode = "CFS";
			shipPackLine.SetContainer(Consol, container);
			Factory.Save();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 1);
			ResetBOLWrapper();

			AssertContainerFields("CONTAINER 2\n", "Seal Num\n", "40FR\n", ExpectedWeight + "\n", ExpectedVolume + "\n",
				"12\n", "CFS\n", "\n", "CFS");
			AssertFollowOnContainerFields("FOLLOWON\n", "Seal Num\n", "40FR\n", ExpectedWeight + "\n", ExpectedVolume + "\n",
				"12\n", "CFS\n", "\n", "CFS");
		}

		public void TestFollowOnContainersWhenNotAMasterBOLAndDeliveryModeOverridenAndStartsWithCY()
		{
			AddContainerToShipment();

			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "20FR"));
			var shipPackLine = (PackLine)Shipment.OuterPackLines.AddNew();
			shipPackLine.JL_PackageCount = 12;
			shipPackLine.JL_ActualWeight = 23;
			shipPackLine.JL_ActualVolume = 50;

			CommonContainer container = Consol.Containers.AddNew();
			container.JC_RC = containerCode.PK;
			container.JC_ContainerNum = "FOLLOWON";
			container.JC_SealNum = "Seal Num";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_DeliveryMode = "CY/CY";
			shipPackLine.SetContainer(Consol, container);
			Factory.Save();

			Shipment.JS_HBLContainerPackModeOverride = "CYSHIP";
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 1);
			ResetBOLWrapper();

			AssertContainerFields("CONTAINER 1\n", "Seal Num\n", "20NOR\n", ExpectedWeight + "\n", ExpectedVolume + "\n",
				"12\n", "CYSHIP*\n", "*\n", "CYSHIP");
			AssertFollowOnContainerFields("FOLLOWON\n", "Seal Num\n", "20FR\n", ExpectedWeight + "\n", ExpectedVolume + "\n",
				"12\n", "CYSHIP*\n", "*\n", "CYSHIP");
		}

		public void TestFollowOnContainersWhenNotAMasterBOLAndDeliveryModeOverridenAndDoesNotStartWithCY()
		{
			AddAnotherContainerToShipment();

			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "40FR"));
			var shipPackLine = (PackLine)Shipment.OuterPackLines.AddNew();
			shipPackLine.JL_PackageCount = 12;
			shipPackLine.JL_ActualWeight = 23;
			shipPackLine.JL_ActualVolume = 50;

			CommonContainer container = Consol.Containers.AddNew();
			container.JC_RC = containerCode.PK;
			container.JC_ContainerNum = "FOLLOWON";
			container.JC_SealNum = "Seal Num";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_DeliveryMode = "CFS";
			shipPackLine.SetContainer(Consol, container);
			Factory.Save();

			Shipment.JS_HBLContainerPackModeOverride = "SHIP";
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 1);
			ResetBOLWrapper();

			AssertContainerFields("CONTAINER 2\n", "Seal Num\n", "40FR\n", ExpectedWeight + "\n", ExpectedVolume + "\n",
				"12\n", "SHIP\n", "\n", "SHIP");
			AssertFollowOnContainerFields("FOLLOWON\n", "Seal Num\n", "40FR\n", ExpectedWeight + "\n", ExpectedVolume + "\n",
				"12\n", "SHIP\n", "\n", "SHIP");
		}

		public void TestFollowOnContainersWhenNotAMasterBOLAndSealNumberIsEmpty()
		{
			AddAnotherContainerToShipment();

			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "20FR"));
			var shipPackLine = (PackLine)Shipment.OuterPackLines.AddNew();
			shipPackLine.JL_PackageCount = 12;
			shipPackLine.JL_ActualWeight = 23;
			shipPackLine.JL_ActualVolume = 50;

			CommonContainer container = Consol.Containers.AddNew();
			container.JC_RC = containerCode.PK;
			container.JC_ContainerNum = "FOLLOWON";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_DeliveryMode = "CFS";
			shipPackLine.SetContainer(Consol, container);
			Factory.Save();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 1);
			ResetBOLWrapper();

			AssertContainerFields("CONTAINER 2\n", "Seal Num\n", "40FR\n", ExpectedWeight + "\n", ExpectedVolume + "\n",
				"12\n", "CFS\n", "\n", "CFS");
			AssertFollowOnContainerFields("FOLLOWON\n", "-\n", "20FR\n", ExpectedWeight + "\n", ExpectedVolume + "\n",
				"12\n", "CFS\n", "\n", "CFS");
		}

		public void TestFollowOnWhenNotAMasterBOLAndContainerTypeIsEmpty()
		{
			AddAnotherContainerToShipment();

			var shipPackLine = (PackLine)Shipment.OuterPackLines.AddNew();
			shipPackLine.JL_PackageCount = 12;
			shipPackLine.JL_ActualWeight = 23;
			shipPackLine.JL_ActualVolume = 50;

			CommonContainer container = Consol.Containers.AddNew();
			container.JC_ContainerNum = "FOLLOWON";
			container.JC_SealNum = "Seal Num";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_DeliveryMode = "CFS";
			shipPackLine.SetContainer(Consol, container);
			Factory.Save();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 1);
			ResetBOLWrapper();

			AssertContainerFields("CONTAINER 2\n", "Seal Num\n", "40FR\n", ExpectedWeight + "\n", ExpectedVolume + "\n",
				"12\n", "CFS\n", "\n", "CFS");
			AssertFollowOnContainerFields("FOLLOWON\n", "Seal Num\n", "-\n", ExpectedWeight + "\n", ExpectedVolume + "\n",
				"12\n", "CFS\n", "\n", "CFS");
		}

		public void TestFollowOnWhenNotAMasterBOLAndContainerWeightIsEmpty()
		{
			AddAnotherContainerToShipment();

			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "20FR"));
			var shipPackLine = (PackLine)Shipment.OuterPackLines.AddNew();
			shipPackLine.JL_PackageCount = 12;
			shipPackLine.JL_ActualVolume = 50;

			CommonContainer container = Consol.Containers.AddNew();
			container.JC_RC = containerCode.PK;
			container.JC_ContainerNum = "FOLLOWON";
			container.JC_SealNum = "Seal Num";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_DeliveryMode = "CFS";
			shipPackLine.SetContainer(Consol, container);
			Factory.Save();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 1);
			ResetBOLWrapper();

			AssertContainerFields("CONTAINER 2\n", "Seal Num\n", "40FR\n", ExpectedWeight + "\n", ExpectedVolume + "\n", "12\n", "CFS\n", "\n", "CFS");
			AssertFollowOnContainerFields("FOLLOWON\n", "Seal Num\n", "20FR\n", "-\n", ExpectedVolume + "\n", "12\n", "CFS\n", "\n", "CFS");
		}

		public void TestFollowOnWhenNotAMasterBOLAndContainerVolumeIsEmpty()
		{
			AddAnotherContainerToShipment();

			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "20FR"));
			var shipPackLine = (PackLine)Shipment.OuterPackLines.AddNew();
			shipPackLine.JL_PackageCount = 12;
			shipPackLine.JL_ActualWeight = 23;

			CommonContainer container = Consol.Containers.AddNew();
			container.JC_RC = containerCode.PK;
			container.JC_ContainerNum = "FOLLOWON";
			container.JC_SealNum = "Seal Num";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_DeliveryMode = "CFS";
			shipPackLine.SetContainer(Consol, container);
			Factory.Save();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 1);
			ResetBOLWrapper();

			AssertContainerFields("CONTAINER 2\n", "Seal Num\n", "40FR\n", ExpectedWeight + "\n", ExpectedVolume + "\n",
				"12\n", "CFS\n", "\n", "CFS");

			AssertFollowOnContainerFields("FOLLOWON\n", "Seal Num\n", "20FR\n", ExpectedWeight + "\n", "-\n",
				"12\n", "CFS\n", "\n", "CFS");
		}

		public void TestFollowOnContainersWhenNotAMasterBOLAndContainerPackagesIsEmpty()
		{
			AddAnotherContainerToShipment();

			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "20FR"));
			var shipPackLine = (PackLine)Shipment.OuterPackLines.AddNew();
			shipPackLine.JL_ActualWeight = 23;
			shipPackLine.JL_ActualVolume = 50;

			CommonContainer container = Consol.Containers.AddNew();
			container.JC_RC = containerCode.PK;
			container.JC_ContainerNum = "FOLLOWON";
			container.JC_SealNum = "Seal Num";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_DeliveryMode = "CFS";
			shipPackLine.SetContainer(Consol, container);
			Factory.Save();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 1);
			ResetBOLWrapper();

			AssertContainerFields("CONTAINER 2\n", "Seal Num\n", "40FR\n", ExpectedWeight + "\n", ExpectedVolume + "\n",
				"12\n", "CFS\n", "\n", "CFS");

			AssertFollowOnContainerFields("FOLLOWON\n", "Seal Num\n", "20FR\n", ExpectedWeight + "\n", ExpectedVolume + "\n",
				"-\n", "CFS\n", "\n", "CFS");
		}

		public void TestFollowOnContainersWhenNotAMasterBOLAndDeliveryModeIsEmpty()
		{
			AddAnotherContainerToShipment();

			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "20FR"));
			var shipPackLine = (PackLine)Shipment.OuterPackLines.AddNew();
			shipPackLine.JL_PackageCount = 12;
			shipPackLine.JL_ActualWeight = 23;
			shipPackLine.JL_ActualVolume = 50;

			CommonContainer container = Consol.Containers.AddNew();
			container.JC_RC = containerCode.PK;
			container.JC_ContainerNum = "FOLLOWON";
			container.JC_SealNum = "Seal Num";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			shipPackLine.SetContainer(Consol, container);
			Factory.Save();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 1);
			ResetBOLWrapper();

			AssertContainerFields("CONTAINER 2\n", "Seal Num\n", "40FR\n", ExpectedWeight + "\n", ExpectedVolume + "\n",
				"12\n", "CFS\n", "\n", "CFS");

			AssertFollowOnContainerFields("FOLLOWON\n", "Seal Num\n", "20FR\n", ExpectedWeight + "\n", ExpectedVolume + "\n",
				"12\n", "-\n", "\n", "CFS");
		}

		public void TestFollowOnContainersWhenNotAMasterBOLAndOnlyContainerNumberIsNotEmpty()
		{
			AddAnotherContainerToShipment();

			var shipPackLine = (PackLine)Shipment.OuterPackLines.AddNew();
			CommonContainer container = Consol.Containers.AddNew();
			container.JC_ContainerNum = "FOLLOWON";
			shipPackLine.SetContainer(Consol, container);
			Factory.Save();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 1);
			ResetBOLWrapper();

			AssertContainerFields("CONTAINER 2\n", "Seal Num\n", "40FR\n", ExpectedWeight + "\n", ExpectedVolume + "\n",
				"12\n", "CFS\n", "\n", "CFS");
			AssertFollowOnContainerFields("FOLLOWON\n", "-\n", "-\n", "-\n", "-\n", "-\n", "-\n", "\n", "CFS");
		}

		public void TestContainersWhenAMasterBOLAndIsCoLoadedIsTrueAndNoSubShipments()
		{
			AddContainerToShipment();

			Shipment.JS_HBLContainerPackModeOverride = "TEST";
			Shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 1);
			ResetBOLWrapper();

			AssertContainerFields("", "", "", "", "",
				"", "", "", "TEST");
		}

		public void TestFollowOnContainersWhenContainerNumberLengthIsGreaterThanContainerColumnWidth()
		{
			AddContainerToShipment();

			var shipPackLine = (PackLine)Shipment.OuterPackLines.AddNew();
			CommonContainer container = Consol.Containers.AddNew();
			container.JC_ContainerNum = "LONGERTHAN11";
			shipPackLine.SetContainer(Consol, container);
			Factory.Save();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.ContainerNumberWidth, container.JC_ContainerNum.Length - 1);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 3);
			ResetBOLWrapper();

			try
			{
				ZString shipperLoadAndCount = BOLWrapper.ShipperLoadAndCount;
				Assert(true);
			}
			catch (IndexOutOfRangeException)
			{
				Assert("BuildContainerFollowOns() should not raise IndexOutOfRangeException", false);
			}
		}

		public void TestContainersWhenAMasterBOLAndOneSubShipmentWithNoContainers()
		{
			AddContainerToShipment();

			Shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			var subShipment = Factory.New<ForwardingShipment>();
			subShipment.JS_JS_ColoadMasterShipment = Shipment.PK;
			Factory.Save();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 1);
			ResetBOLWrapper();

			AssertContainerFields("", "", "", "", "", "", "", "", "");
		}

		public void TestContainersWhenAMasterBOLAndOneSubShipmentWithCYDeliveryMode()
		{
			AddContainerToShipment();

			Shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			var subShipment = Factory.New<ForwardingShipment>();
			subShipment.JS_JS_ColoadMasterShipment = Shipment.PK;
			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "20FR"));
			var shipPackLine = (PackLine)subShipment.OuterPackLines.AddNew();
			shipPackLine.JL_PackageCount = 12;
			shipPackLine.JL_ActualWeight = 23;
			shipPackLine.JL_ActualVolume = 50;

			CommonContainer container = Consol.Containers.AddNew();
			container.JC_RC = containerCode.PK;
			container.JC_ContainerNum = "SUBCONT";
			container.JC_SealNum = "Seal Num";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_DeliveryMode = "CY/CY";
			shipPackLine.SetContainer(Consol, container);
			Factory.Save();

			Shipment.JS_HBLContainerPackModeOverride = "TEST";
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 1);
			ResetBOLWrapper();

			AssertContainerFields("SUBCONT\n", "Seal Num\n", "20FR\n", ExpectedWeight + "\n", ExpectedVolume + "\n",
				"12\n", "CY/CY*\n", "*\n", "TEST");
		}

		public void TestContainersWhenAMasterBOLAndOneSubShipmentWithNonCYDeliveryMode()
		{
			Shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AddContainerToShipment();

			var subShipment = Factory.New<ForwardingShipment>();
			subShipment.JS_JS_ColoadMasterShipment = Shipment.PK;
			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "20FR"));
			var shipPackLine = (PackLine)subShipment.OuterPackLines.AddNew();
			shipPackLine.JL_PackageCount = 12;
			shipPackLine.JL_ActualWeight = 23;
			shipPackLine.JL_ActualVolume = 50;

			CommonContainer container = Consol.Containers.AddNew();
			container.JC_RC = containerCode.PK;
			container.JC_ContainerNum = "SUBCONT";
			container.JC_SealNum = "Seal Num";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_DeliveryMode = "MODE";
			shipPackLine.SetContainer(Consol, container);
			Factory.Save();

			Shipment.JS_HBLContainerPackModeOverride = "TEST";
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 1);
			ResetBOLWrapper();

			AssertContainerFields("SUBCONT\n", "Seal Num\n", "20FR\n", ExpectedWeight + "\n", ExpectedVolume + "\n",
				"12\n", "MODE\n", "\n", "TEST");
		}

		public void TestContainersWhenAMasterBOLAndOneSubShipmentWithOverridenDeliveryMode()
		{
			AddContainerToShipment();

			Shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			var subShipment = Factory.New<ForwardingShipment>();
			subShipment.JS_JS_ColoadMasterShipment = Shipment.PK;
			subShipment.JS_HBLContainerPackModeOverride = "SUBMODE";
			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "20FR"));
			var shipPackLine = (PackLine)subShipment.OuterPackLines.AddNew();
			shipPackLine.JL_PackageCount = 24;
			shipPackLine.JL_ActualWeight = 50;
			shipPackLine.JL_ActualVolume = 100;

			CommonContainer container = Consol.Containers.AddNew();
			container.JC_RC = containerCode.PK;
			container.JC_ContainerNum = "SUBCONT";
			container.JC_SealNum = "Seal Num";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_DeliveryMode = "MODE";
			shipPackLine.SetContainer(Consol, container);
			Factory.Save();

			Shipment.JS_HBLContainerPackModeOverride = "TEST";
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 1);
			ResetBOLWrapper();
			ZString expectedWeight = ShipmentWrapper.FormatNumber(shipPackLine.JL_ActualWeight, Env.Registry.WeightMinimumDecimalPlacesToDisplay);
			ZString expectedVolume = ShipmentWrapper.FormatNumber(shipPackLine.JL_ActualVolume, Env.Registry.VolumeMinimumDecimalPlacesToDisplay);

			AssertContainerFields("SUBCONT\n", "Seal Num\n", "20FR\n", expectedWeight + "\n", expectedVolume + "\n",
				"24\n", "SUBMODE\n", "\n", "TEST");
		}

		public void TestContainerWhenAMasterBOLAndTwoShipmentsWithTheSameDeliveryMode()
		{
			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "20FR"));
			CommonContainer container = Consol.Containers.AddNew();
			container.JC_RC = containerCode.PK;
			container.JC_ContainerNum = "SUBCONT";
			container.JC_SealNum = "Seal Num";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_DeliveryMode = "MODE";

			Shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			var subShipment1 = Factory.New<ForwardingShipment>();
			subShipment1.JS_JS_ColoadMasterShipment = Shipment.PK;
			var shipPackLine1 = (PackLine)subShipment1.OuterPackLines.AddNew();
			shipPackLine1.JL_PackageCount = 12;
			shipPackLine1.JL_ActualWeight = 23;
			shipPackLine1.JL_ActualVolume = 50;
			shipPackLine1.SetContainer(Consol, container);

			var subShipment2 = Factory.New<ForwardingShipment>();
			subShipment2.JS_JS_ColoadMasterShipment = Shipment.PK;
			var shipPackLine2 = (PackLine)subShipment2.OuterPackLines.AddNew();
			shipPackLine2.JL_PackageCount = 12;
			shipPackLine2.JL_ActualWeight = 27.589M;
			shipPackLine2.JL_ActualVolume = 25.547M;
			shipPackLine2.SetContainer(Consol, container);
			Factory.Save();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 1);
			ResetBOLWrapper();
			ZString expectedWeight = ShipmentWrapper.FormatNumber(shipPackLine1.JL_ActualWeight + shipPackLine2.JL_ActualWeight, Env.Registry.WeightMinimumDecimalPlacesToDisplay);
			ZString expectedVolume = ShipmentWrapper.FormatNumber(shipPackLine1.JL_ActualVolume + shipPackLine2.JL_ActualVolume, Env.Registry.VolumeMinimumDecimalPlacesToDisplay);

			AssertContainerFields("SUBCONT\n", "Seal Num\n", "20FR\n", expectedWeight + "\n", expectedVolume + "\n",
				"24\n", "MODE\n", "\n", "MODE");
		}

		public void TestContainerWhenAMasterBOLAndTwoShipmentsWithDifferentDeliveryModes()
		{
			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "20FR"));
			CommonContainer container = Consol.Containers.AddNew();
			container.JC_RC = containerCode.PK;
			container.JC_ContainerNum = "SUBCONT";
			container.JC_SealNum = "Seal Num";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_DeliveryMode = "MODE";

			Shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			var subShipment1 = Factory.New<ForwardingShipment>();
			subShipment1.JS_JS_ColoadMasterShipment = Shipment.PK;
			subShipment1.JS_HBLContainerPackModeOverride = "TEST";
			var shipPackLine1 = (PackLine)subShipment1.OuterPackLines.AddNew();
			shipPackLine1.JL_PackageCount = 12;
			shipPackLine1.JL_ActualWeight = 23;
			shipPackLine1.JL_ActualVolume = 50;
			shipPackLine1.SetContainer(Consol, container);

			var subShipment2 = Factory.New<ForwardingShipment>();
			subShipment2.JS_JS_ColoadMasterShipment = Shipment.PK;
			var shipPackLine2 = (PackLine)subShipment2.OuterPackLines.AddNew();
			shipPackLine2.JL_PackageCount = 20;
			shipPackLine2.JL_ActualWeight = 27.589M;
			shipPackLine2.JL_ActualVolume = 25.547M;
			shipPackLine2.SetContainer(Consol, container);
			Factory.Save();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 2);
			ResetBOLWrapper();
			ZString expectedWeight1 = ShipmentWrapper.FormatNumber(shipPackLine1.JL_ActualWeight, Env.Registry.WeightMinimumDecimalPlacesToDisplay);
			ZString expectedVolume1 = ShipmentWrapper.FormatNumber(shipPackLine1.JL_ActualVolume, Env.Registry.VolumeMinimumDecimalPlacesToDisplay);
			ZString expectedWeight2 = ShipmentWrapper.FormatNumber(shipPackLine2.JL_ActualWeight, Env.Registry.WeightMinimumDecimalPlacesToDisplay);
			ZString expectedVolume2 = ShipmentWrapper.FormatNumber(shipPackLine2.JL_ActualVolume, Env.Registry.VolumeMinimumDecimalPlacesToDisplay);

			AssertEquals("Container number", "SUBCONT\nSUBCONT\n", BOLWrapper.ContainerNumberColumn);
			AssertEquals("Container Seal number", "Seal Num\nSeal Num\n", BOLWrapper.ContainerSealNumColumn);
			AssertEquals("Container Type", "20FR\n20FR\n", BOLWrapper.ContainerTypeColumn);
			Assert("Container Weight", BOLWrapper.ContainerWeightColumn == (expectedWeight1 + "\n" + expectedWeight2 + "\n") || BOLWrapper.ContainerWeightColumn == (expectedWeight2 + "\n" + expectedWeight1 + "\n"));
			Assert("Container Volume", BOLWrapper.ContainerVolumeColumn == (expectedVolume1 + "\n" + expectedVolume2 + "\n") || BOLWrapper.ContainerVolumeColumn == (expectedVolume2 + "\n" + expectedVolume1 + "\n"));
			Assert("Container Package count", BOLWrapper.ContainerPackagesColumn == "12\n20\n" || BOLWrapper.ContainerPackagesColumn == "20\n12\n");
			Assert("Container Mode", BOLWrapper.ContainerModeColumn == "MODE\nTEST\n" || BOLWrapper.ContainerModeColumn == "TEST\nMODE\n");
			AssertEquals("Container Asterisk", "\n\n", BOLWrapper.ContainerAsteriskColumn);
		}

		#endregion

		#region MasterCoLoadPackLines
		public void TestMasterCoLoadPackLines()
		{
			AssertEquals("No coload shipments for this shipment. PackLine collection is empty.", 0, BOLWrapper.MasterCoLoadPackLinesTestMethod.Count);

			var coload1 = Shipment.CoLoadShipments.AddNew();
			coload1.OuterPackLines.AddNew();
			coload1.OuterPackLines.AddNew();

			var coload2 = Shipment.CoLoadShipments.AddNew();
			coload2.OuterPackLines.AddNew();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ResetBOLWrapper();
			AssertEquals("Total of 3 ColoadPackLines for this shipment.", 3, BOLWrapper.MasterCoLoadPackLinesTestMethod.Count);
		}
		#endregion

		#region Container Count

		public void TestContainerTypeCount()
		{
			var containerCode1 = Factory.LoadTop1<RefContainer>(new ZQuery());
			var containerCode2 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.PK, SQLComparisonOperator.NotEqual, containerCode1.PK));

			PackLine line1 = Shipment.OuterPackLines.AddNew();
			line1.JL_PackageCount = 12;
			CommonContainer container1 = Consol.Containers.AddNew();
			container1.JC_RC = containerCode1.PK;
			line1.SetContainer(Consol, container1);

			ForwardingConsol consol2 = Shipment.Consols.AddNew();

			PackLine line2 = Shipment.OuterPackLines.AddNew();
			line2.JL_PackageCount = 5;
			CommonContainer container2 = consol2.Containers.AddNew();
			container2.JC_RC = containerCode2.PK;
			line2.SetContainer(consol2, container2);

			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container2.JC_ContainerMode = Core.Constants.ContainerModes.FCL;

			Consol.JK_RL_NKLoadPort = "AUSYD";
			Consol.JK_RL_NKDischargePort = "SGSIN";

			consol2.JK_RL_NKLoadPort = "SGSIN";
			consol2.JK_RL_NKDischargePort = "USLAX";

			Factory.Save();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			DocBillOfLadingTestClass bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);

			ZString expected = "1 x " + containerCode1.RC_Code + " CONTAINER\n";
			AssertEquals("Container type count", expected, bOL.GetContainerTypeCountTestMethod());
			AssertEquals("Container count", 1, bOL.FCLContainersHavingRefContainerType.Count);
		}

		public void TestContainerTypeCountForNonMasterBOL()
		{
			var containerCode1 = Factory.LoadTop1<RefContainer>(new ZQuery());
			var shipPackLine1 = (PackLine)Shipment.OuterPackLines.AddNew();
			shipPackLine1.JL_PackageCount = 12;
			var container1 = (CommonContainer)Consol.Containers.AddNew();
			container1.JC_RC = containerCode1.PK;
			shipPackLine1.SetContainer(Consol, container1);
			Factory.Save();
			DocBillOfLadingTestClass bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("Container type count", ZString.Empty, bOL.GetContainerTypeCountTestMethod());
			AssertEquals("Container count", 0, bOL.FCLContainersHavingRefContainerType.Count);

			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			Factory.Save();
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			ZString expected = "1 x " + containerCode1.RC_Code + " CONTAINER\n";
			AssertEquals("Container type count", expected, bOL.GetContainerTypeCountTestMethod());
			AssertEquals("Container count", 1, bOL.FCLContainersHavingRefContainerType.Count);
		}

		public void TestContainerTypeCountForMasterBOLWithOneSubShipments()
		{
			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery());
			CommonContainer container = Consol.Containers.AddNew();
			container.JC_RC = containerCode.PK;
			container.JC_ContainerMode = Core.Constants.ContainerModes.LCL;

			Shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			var subShipment1 = Factory.New<ForwardingShipment>();
			subShipment1.JS_JS_ColoadMasterShipment = Shipment.PK;
			var shipPackLine1 = (PackLine)subShipment1.OuterPackLines.AddNew();
			shipPackLine1.SetContainer(Consol, container);
			Factory.Save();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			DocBillOfLadingTestClass bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("Container type count", ZString.Empty, bOL.GetContainerTypeCountTestMethod());
			AssertEquals("Container count", 0, bOL.FCLContainersHavingRefContainerType.Count);

			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			Factory.Save();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);

			ZString expected = "1 x " + containerCode.RC_Code + " CONTAINER\n";
			AssertEquals("Container type count", expected, bOL.GetContainerTypeCountTestMethod());
			AssertEquals("Container count", 1, bOL.FCLContainersHavingRefContainerType.Count);
		}

		public void TestContainerTypeCountForMasterBOLWithTwoSubShipmentsWithTheSameContainer()
		{
			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery());
			CommonContainer container = Consol.Containers.AddNew();
			container.JC_RC = containerCode.PK;
			container.JC_ContainerNum = "Container1";
			container.JC_ContainerMode = Core.Constants.ContainerModes.LCL;

			Shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			var subShipment1 = Factory.New<ForwardingShipment>();
			subShipment1.JS_JS_ColoadMasterShipment = Shipment.PK;
			var shipPackLine1 = (PackLine)subShipment1.OuterPackLines.AddNew();
			shipPackLine1.SetContainer(Consol, container);

			var subShipment2 = Factory.New<ForwardingShipment>();
			subShipment2.JS_JS_ColoadMasterShipment = Shipment.PK;
			var shipPackLine2 = (PackLine)subShipment2.OuterPackLines.AddNew();
			shipPackLine2.SetContainer(Consol, container);
			Factory.Save();

			DocBillOfLadingTestClass bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("Container type count", ZString.Empty, bOL.GetContainerTypeCountTestMethod());
			AssertEquals("Container count", 0, bOL.FCLContainersHavingRefContainerType.Count);

			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			Factory.Save();
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);

			ZString expected = "1 x " + containerCode.RC_Code + " CONTAINER\n";
			AssertEquals("Container type count", expected, bOL.GetContainerTypeCountTestMethod());
			AssertEquals("Container count", 1, bOL.FCLContainersHavingRefContainerType.Count);
		}

		public void TestContainerTypeCountForMasterBOLWithTwoSubShipmentsWithDifferentContainers()
		{
			var containerCode1 = Factory.LoadTop1<RefContainer>(new ZQuery());
			CommonContainer container1 = Consol.Containers.AddNew();
			container1.JC_RC = containerCode1.PK;
			container1.JC_ContainerNum = "Container 1";
			container1.JC_ContainerMode = Core.Constants.ContainerModes.LCL;

			var containerCode2 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.PK, SQLComparisonOperator.NotEqual, containerCode1.PK));
			CommonContainer container2 = Consol.Containers.AddNew();
			container2.JC_RC = containerCode2.PK;
			container2.JC_ContainerNum = "Container 2";
			container2.JC_ContainerMode = Core.Constants.ContainerModes.LCL;

			Shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			var subShipment1 = Factory.New<ForwardingShipment>();
			subShipment1.JS_JS_ColoadMasterShipment = Shipment.PK;
			var shipPackLine1 = (PackLine)subShipment1.OuterPackLines.AddNew();
			shipPackLine1.SetContainer(Consol, container1);

			var subShipment2 = Factory.New<ForwardingShipment>();
			subShipment2.JS_JS_ColoadMasterShipment = Shipment.PK;
			var shipPackLine2 = (PackLine)subShipment2.OuterPackLines.AddNew();
			shipPackLine2.SetContainer(Consol, container2);
			Factory.Save();

			DocBillOfLadingTestClass bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("Container type count", ZString.Empty, bOL.GetContainerTypeCountTestMethod());
			AssertEquals("Container count", 0, bOL.FCLContainersHavingRefContainerType.Count);

			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container2.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			Factory.Save();
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);

			var expected = new ZString[] { "1 x " + containerCode1.RC_Code + " CONTAINER", "1 x " + containerCode2.RC_Code + " CONTAINER" };
			AssertContainsExactElementsInAnyOrder("Container type count", expected, bOL.GetContainerTypeCountTestMethod().TrimEnd().Split('\n'));
			AssertEquals("Container count", 2, bOL.FCLContainersHavingRefContainerType.Count);
		}

		public void TestContainerTypeCountForMasterBOLWithTwoSubShipmentsWithDifferentContainersOfTheSameType()
		{
			var containerCode1 = Factory.LoadTop1<RefContainer>(new ZQuery());
			CommonContainer container1 = Consol.Containers.AddNew();
			container1.JC_RC = containerCode1.PK;
			container1.JC_ContainerNum = "Container 1";
			container1.JC_ContainerMode = Core.Constants.ContainerModes.LCL;

			CommonContainer container2 = Consol.Containers.AddNew();
			container2.JC_RC = containerCode1.PK;
			container2.JC_ContainerNum = "Container 2";
			container2.JC_ContainerMode = Core.Constants.ContainerModes.LCL;

			Shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			var subShipment1 = Factory.New<ForwardingShipment>();
			subShipment1.JS_JS_ColoadMasterShipment = Shipment.PK;
			var shipPackLine1 = (PackLine)subShipment1.OuterPackLines.AddNew();
			shipPackLine1.SetContainer(Consol, container1);

			var subShipment2 = Factory.New<ForwardingShipment>();
			subShipment2.JS_JS_ColoadMasterShipment = Shipment.PK;
			var shipPackLine2 = (PackLine)subShipment2.OuterPackLines.AddNew();
			shipPackLine2.SetContainer(Consol, container2);
			Factory.Save();

			DocBillOfLadingTestClass bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("Container type count", ZString.Empty, bOL.GetContainerTypeCountTestMethod());
			AssertEquals("Container count", 0, bOL.FCLContainersHavingRefContainerType.Count);

			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container2.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			Factory.Save();
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);

			ZString expected = "2 x " + containerCode1.RC_Code + " CONTAINER\n";
			AssertEquals("Container type count", expected, bOL.GetContainerTypeCountTestMethod());
			AssertEquals("Container count", 2, bOL.FCLContainersHavingRefContainerType.Count);
		}

		public void TestContainerTypeCountForMasterBOLWithTwoSubShipmentsWithSameContainerTypesOneFCLAndOneLCLContainer()
		{
			var containerCode1 = Factory.LoadTop1<RefContainer>(new ZQuery());
			CommonContainer container1 = Consol.Containers.AddNew();
			container1.JC_RC = containerCode1.PK;
			container1.JC_ContainerNum = "Container 1";
			container1.JC_ContainerMode = Core.Constants.ContainerModes.LCL;

			CommonContainer container2 = Consol.Containers.AddNew();
			container2.JC_RC = containerCode1.PK;
			container2.JC_ContainerNum = "Container 2";
			container2.JC_ContainerMode = Core.Constants.ContainerModes.LCL;

			Shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			var subShipment1 = Factory.New<ForwardingShipment>();
			subShipment1.JS_JS_ColoadMasterShipment = Shipment.PK;
			var shipPackLine1 = (PackLine)subShipment1.OuterPackLines.AddNew();
			shipPackLine1.SetContainer(Consol, container1);

			var subShipment2 = Factory.New<ForwardingShipment>();
			subShipment2.JS_JS_ColoadMasterShipment = Shipment.PK;
			var shipPackLine2 = (PackLine)subShipment2.OuterPackLines.AddNew();
			shipPackLine2.SetContainer(Consol, container2);
			Factory.Save();

			DocBillOfLadingTestClass bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("Container type count", ZString.Empty, bOL.GetContainerTypeCountTestMethod());
			AssertEquals("Container count", 0, bOL.FCLContainersHavingRefContainerType.Count);

			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			Factory.Save();
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);

			ZString expected = "1 x " + containerCode1.RC_Code + " CONTAINER\n";
			AssertEquals("Container type count", expected, bOL.GetContainerTypeCountTestMethod());
			AssertEquals("Container count", 1, bOL.FCLContainersHavingRefContainerType.Count);
		}

		public void TestContainerTypeCountForShipmentWhoseConsolHasContainerCountSet()
		{
			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery());
			CommonContainer container = Consol.Containers.AddNew();
			container.JC_RC = containerCode.PK;
			container.JC_ContainerCount = 4;
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;

			Consol.Shipments.Add(Shipment);

			var shipPackLine1 = (PackLine)Shipment.OuterPackLines.AddNew();
			shipPackLine1.JL_PackageCount = 12;
			shipPackLine1.JL_ActualWeight = 23;
			shipPackLine1.JL_ActualVolume = 50;

			Factory.Save();
			DocBillOfLadingTestClass bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);

			ZString expected = "4 x " + containerCode.RC_Code + " CONTAINER\n";
			AssertEquals("Container type count", expected, bOL.GetContainerTypeCountTestMethod());
		}

		#endregion

		#region Package Count

		public void TestGetPackageCountNonMasterBOL()
		{
			Shipment.JS_OuterPacks = 0;
			DocBillOfLadingTestClass bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("Package count", ZString.Empty, bOL.GetPackageCountTestMethod());

			Shipment.JS_OuterPacks = 120;
			Shipment.JS_F3_NKPackType = ZString.Empty;
			AssertEquals("Package count with no UQ", "120\r\n", bOL.GetPackageCountTestMethod());

			Shipment.JS_F3_NKPackType = "PLT";
			AssertEquals("Package count with UQ", "120 Pallet(s)\r\n", bOL.GetPackageCountTestMethod());

			var shipPackLine = (PackLine)Shipment.OuterPackLines[0];
			shipPackLine.JL_PackageCount = 120;
			CommonContainer container = Consol.Containers.AddNew();
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			shipPackLine.SetContainer(Consol, container);
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("Package count with UQ", "STC 120 Pallet(s)\r\n", bOL.GetPackageCountTestMethod());

			shipPackLine.JL_PackageCount = 200;
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("Package count with UQ", "STC 120 Pallet(s)\r\n", bOL.GetPackageCountTestMethod());

			shipPackLine.JL_PackageCount = 100;
			Factory.Save();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("Package count with UQ", "STC 100 Pallet(s)\r\n and 20 Pallet(s) LCL Cargo\r\n", bOL.GetPackageCountTestMethod());

			var shipPackLine2 = (PackLine)Shipment.OuterPackLines.AddNew();
			shipPackLine2.JL_PackageCount = 120;
			CommonContainer container2 = Consol.Containers.AddNew();
			container2.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			shipPackLine2.SetContainer(Consol, container2);
			Factory.Save();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("Package count with UQ", "STC 100 Pallet(s)\r\n and 20 Pallet(s) LCL Cargo\r\n", bOL.GetPackageCountTestMethod());
		}

		public void TestGetPackageCountForMasterBOLForOneSubShipment()
		{
			Shipment.JS_OuterPacks = 0;
			DocBillOfLadingTestClass bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("Package count", ZString.Empty, bOL.GetPackageCountTestMethod());

			Shipment.JS_OuterPacks = 120;
			Shipment.JS_F3_NKPackType = ZString.Empty;
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("Package count with no UQ", "120\r\n", bOL.GetPackageCountTestMethod());

			Shipment.JS_F3_NKPackType = "PLT";
			AssertEquals("Package count with UQ", "120 Pallet(s)\r\n", bOL.GetPackageCountTestMethod());

			Shipment = Factory.New<ForwardingShipment>();
			Shipment.JS_OuterPacks = 120;
			Shipment.JS_F3_NKPackType = "PLT";

			var containerCode1 = Factory.LoadTop1<RefContainer>(new ZQuery());
			CommonContainer container1 = Consol.Containers.AddNew();
			container1.JC_RC = containerCode1.PK;
			container1.JC_ContainerNum = "Container 1";
			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;

			Shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			ForwardingShipment subShipment1 = Consol.Shipments.AddNew();
			subShipment1.JS_JS_ColoadMasterShipment = Shipment.PK;
			PackLine shipPackLine = subShipment1.OuterPackLines.AddNew();
			shipPackLine.JL_PackageCount = 120;
			shipPackLine.SetContainer(Consol, container1);
			Factory.Save();

			subShipment1.UpdateShipmentFromOuterPackLines();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("Package count with UQ", "STC 120 Pallet(s)\r\n", bOL.GetPackageCountTestMethod());

			shipPackLine.JL_PackageCount = 200;
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("Package count with UQ", "STC 120 Pallet(s)\r\n", bOL.GetPackageCountTestMethod());

			shipPackLine.JL_PackageCount = 100;
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("Package count with UQ", "STC 100 Pallet(s)\r\n and 20 Pallet(s) LCL Cargo\r\n", bOL.GetPackageCountTestMethod());

			var shipPackLine2 = (PackLine)subShipment1.OuterPackLines.AddNew();
			shipPackLine2.JL_PackageCount = 120;
			CommonContainer container2 = Consol.Containers.AddNew();
			container2.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			shipPackLine2.SetContainer(Consol, container2);
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("Package count with UQ", "STC 100 Pallet(s)\r\n and 20 Pallet(s) LCL Cargo\r\n", bOL.GetPackageCountTestMethod());
		}

		public void TestGetPackageCountForMasterBOLForTwoSubShipments()
		{
			Shipment.JS_OuterPacks = 0;
			DocBillOfLadingTestClass bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("Package count", ZString.Empty, bOL.GetPackageCountTestMethod());

			Shipment.JS_OuterPacks = 120;
			Shipment.JS_F3_NKPackType = ZString.Empty;
			AssertEquals("Package count with no UQ", "120\r\n", bOL.GetPackageCountTestMethod());

			Shipment.JS_F3_NKPackType = "PLT";
			AssertEquals("Package count with UQ", "120 Pallet(s)\r\n", bOL.GetPackageCountTestMethod());

			Shipment = Factory.New<ForwardingShipment>();
			Shipment.JS_OuterPacks = 120;
			Shipment.JS_F3_NKPackType = "PLT";

			var containerCode1 = Factory.LoadTop1<RefContainer>(new ZQuery());
			CommonContainer container1 = Consol.Containers.AddNew();
			container1.JC_RC = containerCode1.PK;
			container1.JC_ContainerNum = "Container 1";
			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;

			Shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			ForwardingShipment subShipment1 = Consol.Shipments.AddNew();
			subShipment1.JS_JS_ColoadMasterShipment = Shipment.PK;
			var shipPackLine = (PackLine)subShipment1.OuterPackLines.AddNew();
			shipPackLine.JL_PackageCount = 100;
			shipPackLine.SetContainer(Consol, container1);

			ForwardingShipment subShipment2 = Consol.Shipments.AddNew();
			subShipment2.JS_JS_ColoadMasterShipment = Shipment.PK;
			var shipPackLine2 = (PackLine)subShipment2.OuterPackLines.AddNew();
			shipPackLine2.JL_PackageCount = 20;
			shipPackLine2.SetContainer(Consol, container1);

			subShipment1.UpdateShipmentFromOuterPackLines();
			subShipment2.UpdateShipmentFromOuterPackLines();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("Package count with UQ", "STC 120 Pallet(s)\r\n", bOL.GetPackageCountTestMethod());

			shipPackLine.JL_PackageCount = 200;
			AssertEquals("Package count with UQ", "STC 120 Pallet(s)\r\n", bOL.GetPackageCountTestMethod());

			shipPackLine.JL_PackageCount = 50;
			AssertEquals("Package count with UQ", "STC 70 Pallet(s)\r\n and 50 Pallet(s) LCL Cargo\r\n", bOL.GetPackageCountTestMethod());

			var shipPackLine3 = (PackLine)subShipment1.OuterPackLines.AddNew();
			shipPackLine3.JL_PackageCount = 120;
			CommonContainer container2 = Consol.Containers.AddNew();
			container2.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			shipPackLine3.SetContainer(Consol, container2);
			AssertEquals("Package count with UQ", "STC 70 Pallet(s)\r\n and 50 Pallet(s) LCL Cargo\r\n", bOL.GetPackageCountTestMethod());

			container1.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("Package count with UQ", "120 Pallet(s)\r\n", bOL.GetPackageCountTestMethod());
		}

		public void TestLCLPackageType()
		{
			Shipment.JS_OuterPacks = 120;
			Shipment.JS_F3_NKPackType = "PLT";

			var shipPackLine = (PackLine)Shipment.OuterPackLines[0];
			shipPackLine.JL_PackageCount = 70;
			CommonContainer container = Consol.Containers.AddNew();
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_ContainerNum = "CONTAINER1";
			shipPackLine.SetContainer(Consol, container);

			var shipPackLine2 = (PackLine)Shipment.OuterPackLines.AddNew();
			shipPackLine2.JL_PackageCount = 50;
			shipPackLine2.SetContainer(Consol, null);
			Factory.Save();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			DocBillOfLadingTestClass bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("LCL Package type", "Pallet(s)", bOL.LCLPackagesType);

			shipPackLine2.JL_F3_NKPackType = "CTN";
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("LCL Package type", "Carton(s)", bOL.LCLPackagesType);

			shipPackLine2.JL_PackageCount = 30;
			var shipPackLine3 = (PackLine)Shipment.OuterPackLines.AddNew();
			shipPackLine3.JL_PackageCount = 20;
			shipPackLine3.JL_F3_NKPackType = "CTN";
			shipPackLine3.SetContainer(Consol, null);

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("LCL Package type", "Carton(s)", bOL.LCLPackagesType);

			shipPackLine3.JL_F3_NKPackType = "Box";
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("LCL Package type", "Package(s)", bOL.LCLPackagesType);

			shipPackLine2.SetContainer(Consol, container);
			shipPackLine3.SetContainer(Consol, container);
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			bOL = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
			AssertEquals("LCL Package type", "Pallet(s)", bOL.LCLPackagesType);
		}

		#endregion

		#region Goods Details
		public void TestGoodsDetailsMarksAndNumbers()
		{
			AddMarksAndNumbersToShipment();
			AddGoodsDescriptionToShipment();
			Shipment.JS_OuterPacks = 12;
			Shipment.JS_F3_NKPackType = "CTN";
			Shipment.JS_ActualWeight = 300;
			Shipment.JS_UnitOfWeight = "KG";
			Shipment.JS_ActualVolume = 500;
			Shipment.JS_UnitOfVolume = "M3";

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			SetWidthAndHeightConstants(31, 40, 12, 10, 5);
			ResetBOLWrapper();

			ZString expected =
				@"                                  12 Carton(s)                            300 KG      500 M3    " + "\r\n" +
				"Marks and numbers Line One        description of goods                                          " + "\r\n" +
				"Line Two                                                                                        " + "\r\n";
			AssertEquals("Goods Details", expected, BOLWrapper.GoodsDetails);
			AssertEquals("Follow on marks empty", "", BOLWrapper.FollowOnMarksAndNums);
			AssertEquals("Follow on goods desc empty", "", BOLWrapper.FollowOnGoodsDesc);
		}

		public void TestSeparatePackageDetails()
		{
			AddMarksAndNumbersToShipment();
			AddGoodsDescriptionToShipment();
			Shipment.JS_OuterPacks = 10;
			Shipment.JS_F3_NKPackType = "CTN";
			Shipment.JS_ActualWeight = 300;
			Shipment.JS_UnitOfWeight = "KG";
			Shipment.JS_ActualVolume = 500;
			Shipment.JS_UnitOfVolume = "M3";

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			SetWidthAndHeightConstants(31, 30, 12, 10, 5);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.IncludePackageCountInBOLGoodsDescription, 0);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.PackagesWidth, 10);
			ResetBOLWrapper();

			ZString expected =
				@"Marks and numbers Line One        10        description of goods          300 KG      500 M3    " + "\r\n" +
				"Line Two                          Carton(s)                                                     " + "\r\n";
			AssertEquals("Goods Details", expected, BOLWrapper.GoodsDetails);

			var shipPackLine = (PackLine)Shipment.OuterPackLines[0];
			shipPackLine.JL_PackageCount = 10;
			CommonContainer container = Consol.Containers.AddNew();
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			shipPackLine.SetContainer(Consol, container);

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			SetWidthAndHeightConstants(31, 30, 12, 10, 5);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.IncludePackageCountInBOLGoodsDescription, 0);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.PackagesWidth, 10);
			ResetBOLWrapper();

			expected =
				@"Marks and numbers Line One        STC 10    description of goods          300 KG      500 M3    " + "\r\n" +
				"Line Two                          Carton(s)                                                     " + "\r\n\r\n" +
				"Container      Seal                Type    Weight(KG)    Volume(M3)    Packages    Mode      " + "\r\n" +
				"               -                   -       300           500           10          -         " + "\r\n" +
				"                                                                                             " + "\r\n";
			AssertEquals("Goods Details", expected, BOLWrapper.GoodsDetails);

			shipPackLine.JL_PackageCount = 8;
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			SetWidthAndHeightConstants(31, 30, 12, 10, 10);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.IncludePackageCountInBOLGoodsDescription, 0);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.PackagesWidth, 10);
			ResetBOLWrapper();
			expected =
				@"Marks and numbers Line One        STC 8     description of goods          300 KG      500 M3    " + "\r\n" +
				"Line Two                          Carton(s)                                                     " + "\r\n" +
				"                                   and 2                                                        " + "\r\n" +
				"                                  Carton(s)                                                     " + "\r\n" +
				"                                  LCL Cargo                                                     " + "\r\n\r\n" +
				"Container      Seal                Type    Weight(KG)    Volume(M3)    Packages    Mode      " + "\r\n" +
				"               -                   -       300           500           8           -         " + "\r\n" +
				"                                                                                             " + "\r\n";
			AssertEquals("Goods Details", expected, BOLWrapper.GoodsDetails);

			Shipment.JS_RL_NKDestination = "USLAX";
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			SetWidthAndHeightConstants(31, 30, 12, 10, 10);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.IncludePackageCountInBOLGoodsDescription, 0);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.PackagesWidth, 10);
			ResetBOLWrapper();
			expected =
				@"Marks and numbers Line One        8         description of goods          300 KG      500 M3    " + "\r\n" +
				"Line Two                          Carton(s)                                                     " + "\r\n" +
				"                                   and 2                                                        " + "\r\n" +
				"                                  Carton(s)                                                     " + "\r\n" +
				"                                  LCL Cargo                                                     " + "\r\n\r\n" +
				"Container      Seal                Type    Weight(KG)    Volume(M3)    Packages    Mode      " + "\r\n" +
				"               -                   -       300           500           8           -         " + "\r\n" +
				"                                                                                             " + "\r\n";
			AssertEquals("GoodsDetails when Shipment is bound for US should not show STC", expected, BOLWrapper.GoodsDetails);
		}

		public void TestContainersInGoodsDetails()
		{
			AssertEquals("Nothing entered - Blank goods Details", "                         \r\n", BOLWrapper.GoodsDetails);

			AddMarksAndNumbersToShipment();
			AddGoodsDescriptionToShipment();
			AddAnotherContainerToShipment();

			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "20FR"));
			var shipPackLine = (PackLine)Shipment.OuterPackLines.AddNew();
			shipPackLine.JL_PackageCount = 24;
			shipPackLine.JL_ActualWeight = 50;
			shipPackLine.JL_ActualVolume = 100;

			CommonContainer container = Consol.Containers.AddNew();
			container.JC_RC = containerCode.PK;
			container.JC_ContainerNum = "CONTAINER 3";
			container.JC_SealNum = "Seal Num";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_DeliveryMode = "CY/CY";
			shipPackLine.SetContainer(Consol, container);
			Factory.Save();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			SetWidthAndHeightConstants(31, 57, 12, 10, 5);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 5);
			ResetBOLWrapper();
			ZString expectedWeight2 = ShipmentWrapper.FormatNumber(shipPackLine.JL_ActualWeight, Env.Registry.WeightMinimumDecimalPlacesToDisplay);
			ZString expectedVolume2 = ShipmentWrapper.FormatNumber(shipPackLine.JL_ActualVolume, Env.Registry.VolumeMinimumDecimalPlacesToDisplay);

			ZString expectedResult =
				@"                                  1 x 20FR CONTAINER                                                             " + "\r\n" +
				"                                  1 x 40FR CONTAINER                                                             " + "\r\n" +
				"Marks and numbers Line One        description of goods                                                           " + "\r\n" +
				"Line Two                                                                                                         " + "\r\n" +
				"\r\n" +
				"Container      Seal                Type    Weight(KG)    Volume(M3)    Packages    Mode      " + "\r\n" +
				"CONTAINER 2    Seal Num            40FR    23            50            12          CFS       " + "\r\n" +
				"CONTAINER 3    Seal Num            20FR    50            100           24          CY/CY*    " + "\r\n" +
				"                                                                                             " + "\r\n";
			AssertEquals("Container number", expectedResult, BOLWrapper.GoodsDetails);
			Assert("Follow on container number", BOLWrapper.FollowOnContainerNumber.IsEmpty);
			Assert("Follow on container seal", BOLWrapper.FollowOnContainerSealNum.IsEmpty);
			Assert("Follow on container type", BOLWrapper.FollowOnContainerType.IsEmpty);
			Assert("Follow on container weight", BOLWrapper.FollowOnContainerWeight.IsEmpty);
			Assert("Follow on container volume", BOLWrapper.FollowOnContainerVolume.IsEmpty);
			Assert("Follow on container packages", BOLWrapper.FollowOnContainerPackages.IsEmpty);
			Assert("Follow on container mode", BOLWrapper.FollowOnContainerMode.IsEmpty);
			AssertEquals("Shippers Load and Count", "* Shipper Load and Count", BOLWrapper.ShipperLoadAndCount);
		}

		public void TestFollowOnGoodsDesc()
		{
			StmNote goods = AddGoodsDescriptionToShipment();
			goods.ST_NoteDataAsText =
				@"Goods Desc Line 1
Goods Desc line 2
Goods Desc line 3
Goods Desc line 4
Testing a long text to make sure they dont break
This should be the second line of the follow on goods desc";

			Shipment.JS_OuterPacks = 12;
			Shipment.JS_F3_NKPackType = "CTN";
			Shipment.JS_ActualWeight = 300;
			Shipment.JS_UnitOfWeight = "KG";
			Shipment.JS_ActualVolume = 500;
			Shipment.JS_UnitOfVolume = "M3";

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			SetWidthAndHeightConstants(31, 40, 12, 10, 5);
			ResetBOLWrapper();

			ZString expected =
				@"                                  12 Carton(s)                            300 KG      500 M3    " + "\r\n" +
				"                                  Goods Desc Line 1                                             " + "\r\n" +
				"                                  Goods Desc line 2                                             " + "\r\n" +
				"                                  Goods Desc line 3                                             " + "\r\n" +
				"                                  Goods Desc line 4                                             " + "\r\n";
			AssertEquals("First Page Marks", expected, BOLWrapper.GoodsDetails);
			AssertEquals("Follow On Goods Desc", "Testing a long text to make sure they dont break \r\n This should be the second line of the follow on goods desc", BOLWrapper.FollowOnGoodsDesc);
		}

		public void TestFollowOnMarksAndNumbers()
		{
			StmNote marks = AddMarksAndNumbersToShipment();
			marks.ST_NoteDataAsText =
				@"Marks and numbers Line 1
Marks and numbers line 2
Marks and numbers line 3
Marks and numbers line 4
Line 5 and Line 6 is the start of follow on marks and nums
This string should not be broken or wrapped.";

			Shipment.JS_OuterPacks = 12;
			Shipment.JS_F3_NKPackType = "CTN";
			Shipment.JS_ActualWeight = 300;
			Shipment.JS_UnitOfWeight = "KG";
			Shipment.JS_ActualVolume = 500;
			Shipment.JS_UnitOfVolume = "M3";

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			SetWidthAndHeightConstants(31, 40, 12, 10, 5);
			ResetBOLWrapper();

			ZString expected =
				@"                                  12 Carton(s)                            300 KG      500 M3    " + "\r\n" +
				"Marks and numbers Line 1                                                                        " + "\r\n" +
				"Marks and numbers line 2                                                                        " + "\r\n" +
				"Marks and numbers line 3                                                                        " + "\r\n" +
				"Marks and numbers line 4                                                                        " + "\r\n";
			AssertEquals("First Page Marks", expected, BOLWrapper.GoodsDetails);
			AssertEquals("Follow On Marks", "Line 5 and Line 6 is the start of follow on marks and nums \r\n This string should not be broken or wrapped.", BOLWrapper.FollowOnMarksAndNums);
		}

		public void TestFollowOnContainers()
		{
			AddAnotherContainerToShipment();

			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "20FR"));
			var shipPackLine = (PackLine)Shipment.OuterPackLines.AddNew();
			shipPackLine.JL_PackageCount = 24;
			shipPackLine.JL_ActualWeight = 50;
			shipPackLine.JL_ActualVolume = 100;

			CommonContainer container = Consol.Containers.AddNew();
			container.JC_RC = containerCode.PK;
			container.JC_ContainerNum = "CONTAINER 3";
			container.JC_SealNum = "Seal Num";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_DeliveryMode = "CY/CY";
			shipPackLine.SetContainer(Consol, container);
			Factory.Save();

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			SetWidthAndHeightConstants(31, 57, 12, 10, 3);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 3);
			ResetBOLWrapper();
			ZString expectedWeight2 = ShipmentWrapper.FormatNumber(shipPackLine.JL_ActualWeight, Env.Registry.WeightMinimumDecimalPlacesToDisplay);
			ZString expectedVolume2 = ShipmentWrapper.FormatNumber(shipPackLine.JL_ActualVolume, Env.Registry.VolumeMinimumDecimalPlacesToDisplay);

			ZString expectedResult =
				@"                                  1 x 20FR CONTAINER                                                             " + "\r\n" +
				"                                  1 x 40FR CONTAINER                                                             " + "\r\n" +
				"                                                                                                                 " + "\r\n\r\n" +
				"Container      Seal                Type    Weight(KG)    Volume(M3)    Packages    Mode      " + "\r\n" +
				"CONTAINER 2    Seal Num            40FR    23            50            12          CFS       " + "\r\n";
			AssertEquals("Container number", expectedResult, BOLWrapper.GoodsDetails);
			AssertEquals("Follow on container number", "CONTAINER 3 ", BOLWrapper.FollowOnContainerNumber);
			AssertEquals("Follow on container seal", "Seal Num ", BOLWrapper.FollowOnContainerSealNum);
			AssertEquals("Follow on container type", "20FR ", BOLWrapper.FollowOnContainerType);
			AssertEquals("Follow on container weight", "50 ", BOLWrapper.FollowOnContainerWeight);
			AssertEquals("Follow on container volume", "100 ", BOLWrapper.FollowOnContainerVolume);
			AssertEquals("Follow on container packages", "24 ", BOLWrapper.FollowOnContainerPackages);
			AssertEquals("Follow on container mode", "CY/CY* ", BOLWrapper.FollowOnContainerMode);
			AssertEquals("Shippers load and count", "", BOLWrapper.ShipperLoadAndCount);
			AssertEquals("Follow on Shippers load and count", "* Shipper Load and Count", BOLWrapper.FollowOnShipperLoadAndCount);
		}

		public void TestNumberOfRowsForContainersLessThan2()
		{
			AddAnotherContainerToShipment();
			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, ""));
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			SetWidthAndHeightConstants(0, 0, 0, 0, 1);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 1);
			ResetBOLWrapper();

			ZString expectedResult =
				"   " + "\r\n\r\n" +
				"Container      Seal                Type    Weight(KG)    Volume(M3)    Packages    Mode      " + "\r\n";

			AssertEquals("Container number", expectedResult, BOLWrapper.GoodsDetails);
		}

		#endregion

		#region Order Numbers

		public void TestOrderNumbers()
		{
			var organisation1 = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var order1 = Shipment.AttachedOrders.AddNew();
			order1.JD_OrderNumber = "Order number 1";
			order1.JD_InvoiceNumber = "123";
			order1.SupplierPK = organisation1.PK;

			var organisation2 = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var order2 = Shipment.AttachedOrders.AddNew();
			order2.JD_OrderNumber = "Order number 2";
			order2.JD_InvoiceNumber = "321";
			order2.SupplierPK = organisation2.PK;

			AssertEquals("Order Numbers", "Order number 1,Order number 2", (string)ShipmentWrapper.OrderNumbers);
		}

		#endregion

		#region Charges To

		public void TestChargesTo()
		{
			Shipment.JS_INCO = "";
			AssertEquals("IsPrepaid", false, ShipmentWrapper.IsPrepaid);
			AssertEquals("IsCollect", false, ShipmentWrapper.IsCollect);
			AssertNull("JobHeader", ShipmentWrapper.JobHeader);
			AssertEquals("ChargesTo should be empty", ZString.Empty, BOLWrapper.ChargesTo);

			JobHeader header = new JobHeader.Loader(Shipment).TryLoadOrCreate();
			header.JH_GE = GlbDepartment.CurrentDepartment.PK;
			var localCharges = Factory.LoadTop1<OrgHeader>(new ZQuery());
			header.LocalChargesPK = localCharges.PK;
			DocOrganisation localChargesWrapper = DocOrganisation.New(localCharges, Factory);

			var agentCollect = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, localCharges.PK));
			header.AgentCollectPK = agentCollect.PK;
			DocOrganisation agentCollectWrapper = DocOrganisation.New(agentCollect, Factory);

			Factory.Save();

			Shipment.JS_INCO = Constants.IncoTerms.CostAndFreight;
			AssertEquals("IsPrepaid", true, ShipmentWrapper.IsPrepaid);
			AssertEquals("ChargesTo", localChargesWrapper.PostalAddress, BOLWrapper.ChargesTo);

			Shipment.JS_INCO = Constants.IncoTerms.ExWorks;
			AssertEquals("IsCollect", true, ShipmentWrapper.IsCollect);
			AssertEquals("ChargesTo", agentCollectWrapper.PostalAddress, BOLWrapper.ChargesTo);
		}

		#endregion

		#region ShouldPrintChargesAsLumpSum

		public void TestShouldPrintChargesAsLumpSum()
		{
			Shipment.JS_RL_NKDestination = "DEHAM";

			Assert("Precondition", !BOLWrapper.ShouldPrintChargesAsLumpSum);

			var lumpSumCountries = new List<Guid>();
			lumpSumCountries.Add(Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "FR").PK.ToGuid());
			DocumentsDataRegistry.Instance.BOLLumpSumDisplayCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, lumpSumCountries.ToArray());

			Assert("Shipment is not destined for a lump sum country", !BOLWrapper.ShouldPrintChargesAsLumpSum);

			lumpSumCountries.Add(Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "DE").PK.ToGuid());
			DocumentsDataRegistry.Instance.BOLLumpSumDisplayCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, lumpSumCountries.ToArray());

			Assert("Shipment is destined for a lump sum country", BOLWrapper.ShouldPrintChargesAsLumpSum);
		}

		#endregion

		#region ShouldPrintTotalCharges

		public void TestShouldPrintTotalCharges()
		{
			var lumpSumCountries = new List<Guid>();
			lumpSumCountries.Add(Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "DE").PK.ToGuid());
			DocumentsDataRegistry.Instance.BOLLumpSumDisplayCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, lumpSumCountries.ToArray());

			DocumentsDataRegistry.Instance.BOLPrintTotalCharges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Shipment.JS_RL_NKDestination = "DEHAM";
			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.AsAgreed;

			AssertEquals("Precondition", true, BOLWrapper.ShouldPrintChargesAsLumpSum);
			AssertEquals("Precondition", "AGR", ShipmentWrapper.ChargesDisplay);
			AssertEquals("Precondition", false, BOLWrapper.ShouldPrintTotalCharges);

			Shipment.JS_RL_NKDestination = "AUSYD";
			AssertEquals(false, BOLWrapper.ShouldPrintChargesAsLumpSum);
			AssertEquals(false, BOLWrapper.ShouldPrintTotalCharges);

			Shipment.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges;
			AssertEquals("ALL", ShipmentWrapper.ChargesDisplay);
			AssertEquals(true, BOLWrapper.ShouldPrintTotalCharges);

			DocumentsDataRegistry.Instance.BOLPrintTotalCharges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, BOLWrapper.ShouldPrintTotalCharges);
		}

		#endregion

		#region GetTransportPlanning

		public void TestGetTransportPlanningForType()
		{
			Shipment.Transports.RemoveAndDeleteAll();
			AssertEquals("No transport", 0, BOLWrapper.GetTransportLegsForParticularType(Core.Constants.TransportPlanningType.PreCarriage).Count);

			Transport transport1 = Shipment.Transports.AddNew();
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.PreCarriage;
			Transport transport2 = Shipment.Transports.AddNew();
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.OnForwarding;
			Transport transport3 = Shipment.Transports.AddNew();
			transport3.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;

			DocTransportCollection coll = BOLWrapper.GetTransportLegsForParticularType(Core.Constants.TransportPlanningType.PreCarriage);
			AssertEquals("1 PreCarriage", 1, coll.Count);
			AssertEquals("1 PreCarriage", Core.Constants.TransportPlanningType.PreCarriage, coll[0].TransportType);

			Shipment.Transports.RemoveAndDeleteAll();
			Transport consolTransport1 = Consol.Transports[0];
			consolTransport1.JW_TransportType = Core.Constants.TransportPlanningType.PreCarriage;

			Transport consolTransport2 = Consol.Transports.AddNew();
			consolTransport2.JW_TransportType = Core.Constants.TransportPlanningType.OnForwarding;

			Transport consolTransport3 = Consol.Transports.AddNew();
			consolTransport3.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;

			coll = BOLWrapper.GetTransportLegsForParticularType(Core.Constants.TransportPlanningType.MainVessel);
			AssertEquals("1 Main Leg", 1, coll.Count);
			AssertEquals("1 Main", Core.Constants.TransportPlanningType.MainVessel, coll[0].TransportType);
		}

		#endregion

		#region Addresses

		public void TestConsignorAddressIsInEnglish()
		{
			using (Env.Instance.SetTemporaryUserContext(StaffDE.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals("No consignor, address should be empty", "", BOLWrapper.ConsignorAddress);

				var consignor = Factory.New<OrgHeader>();
				consignor.OH_FullName = "CONSIGNOR";
				consignor.Addresses[0].OA_Address1 = "Address1";
				consignor.Addresses[0].OA_Address2 = "Address2";
				consignor.Addresses[0].OA_RL_NKRelatedPortCode = "DEHAM";
				Shipment.ConsignorPK = consignor.PK;

				AssertEquals("Should use Consignor's default address", "CONSIGNOR\nADDRESS1\nADDRESS2\nGERMANY", BOLWrapper.ConsignorAddress);

				var addr1 = consignor.Addresses.AddNew();
				addr1.OA_Address1 = "Blah1";
				addr1.OA_Address2 = "Blah2";
				addr1.OA_RL_NKRelatedPortCode = "DEHAM";
				Shipment.ConsignorDocumentaryAddress.E2_OA_Address = addr1.PK;

				AssertEquals("Should use DocsCartage Consignor address", "CONSIGNOR\nBLAH1\nBLAH2\nGERMANY", BOLWrapper.ConsignorAddress);
			}
		}

		public void TestConsignorAddressForManufacturerBillOfLading()
		{
			((IBODocDataProvider)ShipmentWrapper).SetDocWrapperContext(new Dictionary<string, object>
				{
					{ DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Legacy Manufacturer Bill Of Lading" }
				});

			using (Env.Instance.SetTemporaryUserContext(StaffDE.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertMultilineASCIIEquals("No manufacturer, address should be empty", ZString.Empty, BOLWrapper.ConsignorAddress);

				var consignor = Factory.New<OrgHeader>();
				consignor.OH_FullName = "CONSIGNOR";
				consignor.Addresses[0].OA_Address1 = "ConsignorAddress1";
				consignor.Addresses[0].OA_Address2 = "ConsignorAddress2";
				consignor.Addresses[0].OA_RL_NKRelatedPortCode = "DEHAM";
				Shipment.ConsignorPK = consignor.PK;

				AssertMultilineASCIIEquals("No manufacturer, address should be empty", ZString.Empty, BOLWrapper.ConsignorAddress);

				var manufacturer = Factory.New<OrgHeader>();
				manufacturer.OH_FullName = "MANUFACTURER";
				manufacturer.Addresses[0].OA_Address1 = "ManufacturerAddress1";
				manufacturer.Addresses[0].OA_Address2 = "ManufacturerAddress2";
				manufacturer.Addresses[0].OA_RL_NKRelatedPortCode = "CRAPO";
				Shipment.ManufacturerDocAddress.OrganisationPK = manufacturer.PK;

				AssertMultilineASCIIEquals("Should use shipment Manufacturer DocAddress",
@"MANUFACTURER
MANUFACTURERADDRESS1
MANUFACTURERADDRESS2
A
COSTA RICA",
BOLWrapper.ConsignorAddress);
			}
		}

		public void TestConsigneeAddressForManufacturerBillOfLading()
		{
			((IBODocDataProvider)ShipmentWrapper).SetDocWrapperContext(new Dictionary<string, object>
				{
					{ DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Legacy Manufacturer Bill Of Lading" }
				});

			using (Env.Instance.SetTemporaryUserContext(StaffDE.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertMultilineASCIIEquals("No manufacturer, address should be empty", ZString.Empty, BOLWrapper.ConsignorAddress);

				var consignor = Factory.New<OrgHeader>();
				consignor.OH_FullName = "CONSIGNOR";
				consignor.Addresses[0].OA_Address1 = "ConsignorAddress1";
				consignor.Addresses[0].OA_Address2 = "ConsignorAddress2";
				consignor.Addresses[0].OA_RL_NKRelatedPortCode = "DEHAM";
				Shipment.ConsignorPK = consignor.PK;

				AssertMultilineASCIIEquals("Should use shipment Manufacturer Consignor DocAddress",
@"CONSIGNOR
CONSIGNORADDRESS1
CONSIGNORADDRESS2
GERMANY",
BOLWrapper.ConsigneeAddress);
			}
		}

		public void TestConsigneeAddressIsInEnglish()
		{
			using (Env.Instance.SetTemporaryUserContext(StaffDE.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals("No consignor, address should be empty", "", BOLWrapper.ConsignorAddress);

				var consignee = Factory.New<OrgHeader>();
				consignee.OH_FullName = "CONSIGNEE";
				consignee.Addresses[0].OA_Address1 = "Address1";
				consignee.Addresses[0].OA_Address2 = "Address2";
				consignee.Addresses[0].OA_RL_NKRelatedPortCode = "DEHAM";
				Shipment.ConsigneePK = consignee.PK;

				AssertEquals("Should use Consignee's default address", "CONSIGNEE\nADDRESS1\nADDRESS2\nGERMANY", BOLWrapper.ConsigneeAddress);

				var addr1 = consignee.Addresses.AddNew();
				addr1.OA_Address1 = "Blah1";
				addr1.OA_Address2 = "Blah2";
				addr1.OA_RL_NKRelatedPortCode = "DEHAM";
				Shipment.ConsigneeDocumentaryAddress.E2_OA_Address = addr1.PK;

				AssertEquals("Should use DocsCartage Consignee address", "CONSIGNEE\nBLAH1\nBLAH2\nGERMANY", BOLWrapper.ConsigneeAddress);
			}
		}

		#endregion

		#region Utilities Test

		public void TestWrappingLongWord()
		{
			ZString text = "Testing wraptext utility in DocBillOfLading." + "\n" +
				"Long word here: 12345678901234567890123456789012345678901234567890123456789012345678901234567890" + " " +
				"That long word should be broken into several rows.";

			ZString expected = "Testing wraptext utility in DocBillOfLading. " + "\n" +
				"Long word here: " + "\n" +
				"123456789012345678901234567890123456789012345" + "\n" +
				"67890123456789012345678901234567890 That " + "\n" +
				"long word should be broken into several rows.";

			ZString result = BOLWrapper.WrapTextForAColumn(text, 45);
			AssertEquals("Long word should be broken into several rows", expected, result);
		}

		public void TestWrapTextForAColumn()
		{
			ZString value = "Testing the wrap text for\t a column\nmethod in the wrapper \n which is for the BOL.";
			ZString expectedValue = "Testing the wrap\ntext for      a\ncolumn\nmethod in the\nwrapper\nwhich is for the\nBOL.";
			AssertEquals(expectedValue, BOLWrapper.ShipmentWrapper.WrapTextForAColumn(value, 20));

			expectedValue = "Testing\nthe wrap\ntext for\na\ncolumn\nmethod in\nthe\nwrapper\nwhich is\nfor the\nBOL.";
			AssertEquals(expectedValue, BOLWrapper.ShipmentWrapper.WrapTextForAColumn(value, 10));
		}

		#endregion

		public void TestCarriersAgentForPortOfLoading()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			DocForwardingShipment wrapper = DocForwardingShipment.New(shipment, Factory);
			AssertEquals(null, wrapper.BillOfLading.CarriersAgentForPortOfLoading);

			ForwardingConsol consol = shipment.Consols.AddNew();
			wrapper = DocForwardingShipment.New(shipment, Factory);
			AssertEquals(null, wrapper.BillOfLading.CarriersAgentForPortOfLoading);

			OrgHeader carrier = Factory.New<OrgHeader>();
			OrgHeader agent = Factory.New<OrgHeader>();
			OrgCarrierAppointedAgentPorts appointedAgentPort = carrier.CarrierAppointedAgentPorts_Agency.AddNew();
			appointedAgentPort.OrganisationPK = agent.PK;
			appointedAgentPort.O5_PortOrCountry = "AUSYD";

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USSFO";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			wrapper = DocForwardingShipment.New(shipment, Factory);
			AssertEquals(agent, wrapper.BillOfLading.CarriersAgentForPortOfLoading.OrgHeader);

			consol.JK_RL_NKLoadPort = "AUMEL";
			wrapper = DocForwardingShipment.New(shipment, Factory);
			AssertEquals(null, wrapper.BillOfLading.CarriersAgentForPortOfLoading);

			consol.JK_RL_NKLoadPort = "SGSIN";
			wrapper = DocForwardingShipment.New(shipment, Factory);
			AssertEquals(null, wrapper.BillOfLading.CarriersAgentForPortOfLoading);

			OrgHeader agent2 = Factory.New<OrgHeader>();
			OrgCarrierAppointedAgentPorts appointedAgentPort2 = carrier.CarrierAppointedAgentPorts_Agency.AddNew();
			appointedAgentPort2.OrganisationPK = agent2.PK;
			appointedAgentPort2.O5_PortOrCountry = "AU";

			consol.JK_RL_NKLoadPort = "AUMEL";
			wrapper = DocForwardingShipment.New(shipment, Factory);
			AssertEquals(agent2, wrapper.BillOfLading.CarriersAgentForPortOfLoading.OrgHeader);
		}

		[ExpectNoExceptions]
		public void TestNoExceptionThrownWhenWidthConstantsIsZero()
		{
			var marks = AddMarksAndNumbersToShipment();
			marks.ST_NoteDataAsText = @"Marks and numbers Line 1
Marks and numbers line 2
Marks and numbers line 3
Marks and numbers line 4";

			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersWidth, 0);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 5);

			AssertEquals("MarksAndNumbers", string.Empty, BOLWrapper.MarksAndNumbers);
			AssertEquals("MarksAndNumbersStringCollection.Count", 0, BOLWrapper.MarksAndNumbersStringCollection.Count);
		}

		public void TestSignatureImage()
		{
			var stream = typeof(DocAWB).Assembly.GetManifestResourceStream("Enterprise.DocumentWrappers.Freight.Signature.png");
			var image = Image.FromStream(stream);
			FreightDataRegistry.Instance.PrintSignatureForHBLDocuments.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var staff = GlbStaff.CurrentUser;

			var shipment = Factory.New<ForwardingShipment>();
			var wrapper = DocForwardingShipment.New(shipment, Factory);
			var documentUsageReporter = new DocumentEngine.DocumentUsageReporter();
			Factory.ServiceContainer.AddService(new DocumentEngine.DocumentUsageDetailsCollector(documentUsageReporter));

			AssertNull("Current user has no signature image.", wrapper.BillOfLading.SignatureImage);

			staff.SignatureImage = image;

			wrapper = DocForwardingShipment.New(shipment, Factory);
			AssertNotNull("Current user signature image.", wrapper.BillOfLading.SignatureImage);
			Assert("IsUserSignatureUsed", documentUsageReporter.IsUserSignatureUsed);

			FreightDataRegistry.Instance.PrintSignatureForHBLDocuments.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			wrapper = DocForwardingShipment.New(shipment, Factory);
			AssertNull("PrintSignatureForHBLDocuments registry is set to false.", wrapper.BillOfLading.SignatureImage);
			Assert("Previously collected IsUserSignatureUsed", documentUsageReporter.IsUserSignatureUsed);
		}

		#region Implementation

		ForwardingShipment Shipment;
		DocForwardingShipment ShipmentWrapper;
		ForwardingConsol Consol;
		DocBillOfLadingTestClass BOLWrapper;
		ZString ExpectedWeight;
		ZString ExpectedVolume;
		readonly ZString PreCarriageType = Core.Constants.TransportPlanningType.PreCarriage;
		readonly ZString MainVesselType = Core.Constants.TransportPlanningType.MainVessel;
		readonly ZString OnForwardingType = Core.Constants.TransportPlanningType.OnForwarding;

		JobHeader JobHeaderBisObj
		{
			get
			{
				if (jobHeaderBisObj == null)
				{
					jobHeaderBisObj = new JobHeader.Loader(Shipment).TryLoadOrCreate();
					jobHeaderBisObj.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
					jobHeaderBisObj.JH_ParentID = Shipment.PK;
					jobHeaderBisObj.JH_GB = GlbBranch.CurrentBranch.PK;

					jobHeaderBisObj.JH_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "FIS")).PK;
					jobHeaderBisObj.JH_JobNum = Shipment.JobNumber;
					jobHeaderBisObj.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

					jobHeaderBisObj.AgentCollectPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
					jobHeaderBisObj.LocalChargesPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, JobHeaderBisObj.AgentCollectPK)).PK;
				}
				return jobHeaderBisObj;
			}
		}
		JobHeader jobHeaderBisObj;

		protected override void SetUp()
		{
			Shipment = Factory.New<ForwardingShipment>();
			var documentShipment = new DocumentShipment(Shipment, Core.Constants.DataContext.Shipment, true);
			ShipmentWrapper = DocForwardingShipment.New(documentShipment, Factory);
			Consol = Shipment.Consols.AddNew();
			ResetBOLWrapper();

			base.SetUp();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocBillOfLading(ShipmentWrapper);
		}

		void ResetBOLWrapper()
		{
			BOLWrapper = new DocBillOfLadingTestClass(ShipmentWrapper, Shipment.PK);
		}

		void ResetWrappers()
		{
			var documentShipment = new DocumentShipment(Shipment, Core.Constants.DataContext.Shipment, true);
			ShipmentWrapper = DocForwardingShipment.New(documentShipment, Factory);
			ResetBOLWrapper();
		}

		void SetWidthAndHeightConstants(ZInt marksWidth, ZInt goodsDescWidth, ZInt weightWidth, ZInt volumeWidth, ZInt marksHeight)
		{
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersWidth, marksWidth);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, goodsDescWidth);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.GrossWeightWidth, weightWidth);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.VolumeMeasurementWidth, volumeWidth);
			ShipmentWrapper.WriteCustomConstant(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, marksHeight);
		}
		StmNote AddMarksAndNumbersToShipment()
		{
			return AddMarksAndNumbersToShipment("Marks and numbers Line One\nLine Two", Shipment);
		}

		StmNote AddMarksAndNumbersToShipment(ZString @string)
		{
			return AddMarksAndNumbersToShipment(@string, Shipment);
		}

		StmNote AddMarksAndNumbersToShipment(ZString @string, ForwardingShipment shipment)
		{
			var marksAndNumberNote = shipment.Notes.AddNew();
			marksAndNumberNote.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			marksAndNumberNote.ST_ParentID = shipment.PK;
			marksAndNumberNote.ST_Table = shipment.TableName;
			marksAndNumberNote.ST_NoteDataAsText = @string;
			return marksAndNumberNote;
		}

		StmNote AddGoodsDescriptionToShipment()
		{
			return AddGoodsDescriptionToShipment("description of goods", Shipment);
		}

		StmNote AddGoodsDescriptionToShipment(ZString @string)
		{
			return AddGoodsDescriptionToShipment(@string, Shipment);
		}

		StmNote AddGoodsDescriptionToShipment(ZString @string, ForwardingShipment shipment)
		{
			var goodDescription = shipment.Notes.AddNew();
			goodDescription.ST_ParentID = shipment.PK;
			goodDescription.ST_Table = shipment.TableName;
			goodDescription.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
			goodDescription.ST_NoteDataAsText = @string;
			return goodDescription;
		}

		void AddContainerToShipment()
		{
			var containerCode1 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "20NOR"));
			var shipPackLine1 = (PackLine)Shipment.OuterPackLines.AddNew();
			shipPackLine1.JL_PackageCount = 12;
			shipPackLine1.JL_ActualWeight = 23;
			shipPackLine1.JL_ActualVolume = 50;

			CommonContainer container1 = Consol.Containers.AddNew();
			container1.JC_RC = containerCode1.PK;
			container1.JC_ContainerNum = "Container 1";
			container1.JC_SealNum = "Seal Num";
			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container1.JC_DeliveryMode = "CY/CY";
			shipPackLine1.SetContainer(Consol, container1);
			Factory.Save();

			ExpectedWeight = ShipmentWrapper.FormatNumber(shipPackLine1.JL_ActualWeight, Env.Registry.WeightMinimumDecimalPlacesToDisplay);
			ExpectedVolume = ShipmentWrapper.FormatNumber(shipPackLine1.JL_ActualVolume, Env.Registry.VolumeMinimumDecimalPlacesToDisplay);
		}

		CommonContainer AddContainerWithValues(ZString containerNumber, ZInt packCount, ZDecimal weight, ZDecimal volume, ZString deliveryMode)
		{
			CommonContainer container = Consol.Containers.AddNew();
			container.JC_ContainerNum = containerNumber;
			container.JC_DeliveryMode = deliveryMode;

			var shipPackLine = (PackLine)Shipment.OuterPackLines.AddNew();
			shipPackLine.JL_PackageCount = packCount;
			shipPackLine.JL_ActualVolume = volume;
			shipPackLine.JL_ActualWeight = weight;
			shipPackLine.SetContainer(Consol, container);
			return container;
		}

		void AddContainerToShipmentWhereOuterPacksHasBeenAdded()
		{
			var containerCode1 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "20NOR"));
			var shipPackLine1 = (PackLine)Shipment.OuterPackLines[0];
			shipPackLine1.JL_PackageCount = 12;
			shipPackLine1.JL_ActualWeight = 23;
			shipPackLine1.JL_ActualVolume = 50;

			CommonContainer container1 = Consol.Containers.AddNew();
			container1.JC_RC = containerCode1.PK;
			container1.JC_ContainerNum = "Container 1";
			container1.JC_SealNum = "Seal Num";
			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container1.JC_DeliveryMode = "CY/CY";
			shipPackLine1.SetContainer(Consol, container1);
			Factory.Save();
		}

		void AddAnotherContainerToShipment()
		{
			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.Equal, "40FR"));
			var shipPackLine = (PackLine)Shipment.OuterPackLines.AddNew();
			shipPackLine.JL_PackageCount = 12;
			shipPackLine.JL_ActualWeight = 23;
			shipPackLine.JL_ActualVolume = 50;

			CommonContainer container = Consol.Containers.AddNew();
			container.JC_RC = containerCode.PK;
			container.JC_ContainerNum = "Container 2";
			container.JC_SealNum = "Seal Num";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_DeliveryMode = "CFS";
			container.JC_SetPointTemp = 5.5;
			container.JC_SetPointTempUnit = "C";
			container.JC_HumidityPercent = 10;

			shipPackLine.SetContainer(Consol, container);
			Factory.Save();

			ExpectedWeight = ShipmentWrapper.FormatNumber(shipPackLine.JL_ActualWeight, Env.Registry.WeightMinimumDecimalPlacesToDisplay);
			ExpectedVolume = ShipmentWrapper.FormatNumber(shipPackLine.JL_ActualVolume, Env.Registry.VolumeMinimumDecimalPlacesToDisplay);
		}

		void AddChargesToShipment_MixedCurrency()
		{
			var lineCharge1 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.AgentCollectPK, 10.000M, FRTChargeCode.PK, "AUD");
			var lineCharge2 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.AgentCollectPK, 30.000M, CUSDSBChargeCode.PK, "USD");
			var lineCharge3 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 10.000M, CUSDSBChargeCode.PK, "USD");
			var lineCharge4 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 50.000M, FRTChargeCode.PK, "AUD");

			var thirdPartyOrg = Factory.New<OrgHeader>();
			var lineCharge5 = CreateLineCharge(JobHeaderBisObj, thirdPartyOrg.PK, 20.000M, FRTChargeCode.PK, "AUD");
		}

		void AddChargesToShipment_SameCurrency()
		{
			var lineCharge1 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.AgentCollectPK, 10.000M, FRTChargeCode.PK, "AUD");
			var lineCharge2 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.AgentCollectPK, 30.000M, CUSDSBChargeCode.PK, "AUD");
			var lineCharge3 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 10.000M, CUSDSBChargeCode.PK, "AUD");
			var lineCharge4 = CreateLineCharge(JobHeaderBisObj, JobHeaderBisObj.LocalChargesPK, 50.000M, FRTChargeCode.PK, "AUD");

			var thirdPartyOrg = Factory.New<OrgHeader>();
			var lineCharge5 = CreateLineCharge(JobHeaderBisObj, thirdPartyOrg.PK, 20.000M, FRTChargeCode.PK, "AUD");
		}

		JobCharge CreateLineCharge(JobHeader jobHeaderBisObj, ZGuid localChargesPK, ZDecimal oSSellAmount, ZGuid chargeCodePK, ZString currencyCode)
		{
			return CreateLineCharge(jobHeaderBisObj, localChargesPK, oSSellAmount, 0m, chargeCodePK, currencyCode);
		}

		JobCharge CreateLineCharge(JobHeader jobHeaderBisObj, ZGuid localChargesPK, ZDecimal oSSellAmount, ZDecimal localSellAmount, ZGuid chargeCodePK, ZString currencyCode)
		{
			var lineCharge = Factory.New<JobCharge>();
			lineCharge.JR_JH = jobHeaderBisObj.PK;
			lineCharge.JR_GE = jobHeaderBisObj.JH_GE;
			lineCharge.JR_GB = jobHeaderBisObj.JH_GB;
			lineCharge.JR_AC = chargeCodePK;
			lineCharge.JR_OH_SellAccount = localChargesPK;
			lineCharge.JR_RX_NKSellCurrency = currencyCode;
			lineCharge.JR_OSSellAmt = oSSellAmount;
			if (!localSellAmount.IsEmpty)
			{
				var exRateVal = oSSellAmount != localSellAmount ? oSSellAmount / localSellAmount : 1m;
				var exRateWrap = lineCharge.GetType().GetProperty("RevenueExchangeRate").GetValue(lineCharge);
				exRateWrap?.GetType().GetMethod("SetBuyRate_ForTestOnly").Invoke(exRateWrap, new object[] { exRateVal });
				lineCharge.JR_LocalSellAmt = localSellAmount;
			}

			return lineCharge;
		}

		void SetExchangeRate(ZDecimal exchangeRate, ZString foreignCurrency)
		{
			RefExchangeRate exchangeRateDuty = Factory.New<RefExchangeRate>();
			exchangeRateDuty.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
			exchangeRateDuty.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRateDuty.RE_GC = GlbCompany.CurrentCompany.PK;
			exchangeRateDuty.RE_RX_NKExCurrency = foreignCurrency;
			exchangeRateDuty.RE_StartDate = ZDateTime.Today.AddDays(-1);
			exchangeRateDuty.RE_SellRate = exchangeRate;
			Factory.Save();

			ZArchitecture.Core.ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		void AssertContainerFields(ZString containerNum, ZString seal, ZString type, ZString weight, ZString volume, ZString packages, ZString mode, ZString asterisk, ZString singleMode)
		{
			AssertEquals("Container number", containerNum, BOLWrapper.ContainerNumberColumn);
			AssertEquals("Container Seal number", seal, BOLWrapper.ContainerSealNumColumn);
			AssertEquals("Container Type", type, BOLWrapper.ContainerTypeColumn);
			AssertEquals("Container Weight", weight, BOLWrapper.ContainerWeightColumn);
			AssertEquals("Container Volume", volume, BOLWrapper.ContainerVolumeColumn);
			AssertEquals("Container Package count", packages, BOLWrapper.ContainerPackagesColumn);
			AssertEquals("Container Mode", mode, BOLWrapper.ContainerModeColumn);
			AssertEquals("Container Asterisk", asterisk, BOLWrapper.ContainerAsteriskColumn);
			AssertEquals("Single Container Mode", singleMode, BOLWrapper.SingleContainerMode);
		}

		void AssertFollowOnContainerFields(ZString containerNum, ZString seal, ZString type, ZString weight, ZString volume, ZString packages, ZString mode, ZString asterisk, ZString singleMode)
		{
			AssertEquals("Container number", containerNum, BOLWrapper.FollowOnContainerNumberColumn);
			AssertEquals("Container Seal number", seal, BOLWrapper.FollowOnContainerSealNumColumn);
			AssertEquals("Container Type", type, BOLWrapper.FollowOnContainerTypeColumn);
			AssertEquals("Container Weight", weight, BOLWrapper.FollowOnContainerWeightColumn);
			AssertEquals("Container Volume", volume, BOLWrapper.FollowOnContainerVolumeColumn);
			AssertEquals("Container Package count", packages, BOLWrapper.FollowOnContainerPackagesColumn);
			AssertEquals("Container Mode", mode, BOLWrapper.FollowOnContainerModeColumn);
			AssertEquals("Container Asterisk", asterisk, BOLWrapper.FollowOnContainerAsteriskColumn);
		}

		Transport AddNewLeg(ForwardingShipment shipment, ZString type, ZString load, ZString discharge, ZString vessel, ZString voyage)
		{
			Transport transportLeg = shipment.Transports.AddNew();
			SetLegSettings(transportLeg, type, load, discharge, vessel, voyage);
			return transportLeg;
		}

		Transport AddNewLeg(ForwardingConsol consol, ZString type, ZString load, ZString discharge, ZString vessel, ZString voyage)
		{
			Transport transportLeg = consol.Transports.AddNew();
			SetLegSettings(transportLeg, type, load, discharge, vessel, voyage);
			return transportLeg;
		}

		void SetLegSettings(Transport transportLeg, ZString type, ZString load, ZString discharge, ZString vessel, ZString voyage)
		{
			transportLeg.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transportLeg.JW_TransportType = type;
			transportLeg.JW_RL_NKLoadPort = load;
			transportLeg.JW_RL_NKDiscPort = discharge;
			transportLeg.JW_Vessel = vessel;
			transportLeg.JW_VoyageFlight = voyage;
		}

		AccChargeCode fFRTChargeCode;
		AccChargeCode fCUSDSBChargeCode;
		public AccChargeCode FRTChargeCode
		{
			get
			{
				if (fFRTChargeCode == null)
				{
					fFRTChargeCode = GetAccChargeCode("FRT");
				}
				return fFRTChargeCode;
			}
		}

		public AccChargeCode CUSDSBChargeCode
		{
			get
			{
				if (fCUSDSBChargeCode == null)
				{
					fCUSDSBChargeCode = GetAccChargeCode("CUSDSB");
				}
				return fCUSDSBChargeCode;
			}
		}

		AccChargeCode GetAccChargeCode(string code)
		{
			var query = new ZQuery(AccChargeCodeSchema.AC_Code, SQLComparisonOperator.Equal, code);
			query.AddToFilter(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.Equal, Env.CurrentCompanyPK);

			return Factory.LoadTop1<AccChargeCode>(query);
		}

		GlbStaff StaffDE
		{
			get
			{
				if (staffDE == null)
				{
					staffDE = Factory.New<GlbStaff>();
					staffDE.GS_Code = "ABC";
					staffDE.GS_LoginName = "Dieter";
					staffDE.GS_FullName = "Dieter";
					staffDE.GS_WorkingLanguage = Core.SharedConstants.Languages.German;

					Factory.Save();
				}

				return staffDE;
			}
		}
		GlbStaff staffDE;

		#endregion
	}
}
