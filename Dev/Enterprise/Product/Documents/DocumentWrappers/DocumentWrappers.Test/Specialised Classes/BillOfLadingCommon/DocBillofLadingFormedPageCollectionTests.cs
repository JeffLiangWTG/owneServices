using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocBillOfLadingFormedPageCollection))]
	sealed class DocBillofLadingFormedPageCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocBillOfLadingFormedPageCollection>
	{
		#region Supporter

		class TestSupporter : IFormedPagesSupporter
		{
			internal TestSupporter(DocShipment shipmentWrapper, BusinessObjectFactory factory)
				: base()
			{
				this.shipmentWrapper = shipmentWrapper;
				this.factory = factory;
			}

			readonly DocShipment shipmentWrapper;
			readonly BusinessObjectFactory factory;

			public DocBillOfLadingFormedPageCollection FormedPages
			{
				get
				{
					if (formedPages == null)
					{
						formedPages = new DocBillOfLadingFormedPageCollection(shipmentWrapper, this, factory);
					}

					return formedPages;
				}
			}
			DocBillOfLadingFormedPageCollection formedPages;

			public ZString[] FollowOnSection
			{
				get { return FormedPages.FollowOnSection; }
			}

			public ZBool HasFollowOnSection
			{
				get { return FormedPages.HasFollowOnSection; }
			}

			public DocFormedPagesShipmentCollection Shipments
			{
				get
				{
					if (shipments == null)
					{
						DocFormedPagesShipment shipment = new DocFormedPagesShipment()
						{
							MarksAndNumbers = shipmentWrapper.MarksAndNumbers,
							GoodsDescription = shipmentWrapper.DescriptionForGoods,
							PackageCount = shipmentWrapper.OuterPacks + " " + shipmentWrapper.OuterPacksPackType,
							Weight = shipmentWrapper.Weight + " " + shipmentWrapper.WeightUnit,
							Volume = shipmentWrapper.Volume + " " + shipmentWrapper.VolumeUnit,
						};

						shipments = new DocFormedPagesShipmentCollection(factory);
						shipments.Add(shipment);
					}

					return shipments;
				}
			}
			DocFormedPagesShipmentCollection shipments;

			public DocFormedPagesContainerCollection Containers
			{
				get
				{
					if (containers == null)
					{
						containers = new DocFormedPagesContainerCollection(factory);

						foreach (IDocContainer container in shipmentWrapper.Containers)
						{
							containers.Add(new DocFormedPagesContainer(container));
						}

						containers.Sort("ContainerNumber");
					}

					return containers;
				}
			}
			DocFormedPagesContainerCollection containers;

			public DocFormedPagesTopLevelPackCollection TopLevelPacks
			{
				get { return topLevelPacks ?? (topLevelPacks = new DocFormedPagesTopLevelPackCollection(factory)); }
			}
			DocFormedPagesTopLevelPackCollection topLevelPacks;

			public ZString BOLClause { get; set; }

			public bool DisplayContainers { get; set; }

			public bool HideContainerGrossWeight { get; set; }

			public bool HideContainerTareWeight { get; set; }

			public bool HidePackLinesInContainersSection { get; set; }

			public DocPackLinesCollection PackLines
			{
				get { return shipmentWrapper.OuterPackLineCollection; }
			}

			public DocJobChargeCollection AllCharges
			{
				get { return shipmentWrapper.JobHeader != null && shipmentWrapper.JobHeader.JobCharges != null ? shipmentWrapper.JobHeader.JobCharges : DocJobChargeCollection.GetCollection(shipmentWrapper.JobHeader, "TEST"); }
			}

			public DocJobChargeCollection CollectCharges
			{
				get { return shipmentWrapper.JobHeader != null && shipmentWrapper.JobHeader.JobChargesForAgentCollect != null ? shipmentWrapper.JobHeader.JobChargesForAgentCollect : DocJobChargeCollection.GetCollection(shipmentWrapper.JobHeader, "TEST"); }
			}

			public bool ShouldPrintChargesAsLumpSum { get; set; }

			public bool ShouldPrintTotalCharges { get; set; }

			public void Reset()
			{
				formedPages = null;
			}

			public bool IsOriginal
			{
				get { return false; }
			}

			public bool IsCopy
			{
				get { return false; }
			}
		}

		TestSupporter Supporter
		{
			get
			{
				if (supporter == null)
				{
					supporter = new TestSupporter(ShipmentWrapper, Factory);
				}

				return supporter;
			}
		}
		TestSupporter supporter;

		public CommonShipment Shipment
		{
			get { return shipment ?? (shipment = Factory.New<CommonShipment>()); }
		}
		CommonShipment shipment;

		public Dictionary<string, object> Constants
		{
			get { return constants ?? (constants = new Dictionary<string, object>()); }
		}
		Dictionary<string, object> constants;

		public DocShipment ShipmentWrapper
		{
			get
			{
				DocShipment wrapper = DocShipment.New(Shipment, Factory);

				if (constants != null)
				{
					wrapper.SetTemplateConstants(constants);
				}

				return wrapper;
			}
		}

		void Reset()
		{
			ShipmentWrapper.SetTemplateConstants(constants);
			Supporter.Reset();
		}

		#endregion

		public void TestPageNo()
		{
			GetNewHeader(Shipment);

			DocBillOfLadingFormedPage page1 = new DocBillOfLadingFormedPage();
			DocBillOfLadingFormedPage page2 = new DocBillOfLadingFormedPage();
			DocBillOfLadingFormedPage page3 = new DocBillOfLadingFormedPage();

			DocBillOfLadingFormedPageCollection collection = new DocBillOfLadingFormedPageCollection(ShipmentWrapper, Supporter, Factory);
			collection.RemoveAll();

			collection.Add(page1);
			collection.Add(page2);
			collection.Add(page3);

			AssertEquals("page 1", 1, page1.PageNo);
			AssertEquals("page 2", 2, page2.PageNo);
			AssertEquals("page 3", 3, page3.PageNo);
		}

		public void TestDetailsSection()
		{
			GetNewHeader(Shipment);

			Shipment.JS_MarksAndNumbers =
				"Marks and Numbers Line 1\n" +
				"Marks and Numbers Line 2\n" +
				"Marks and Numbers Line 3\n" +
				"";

			Shipment.DetailedGoodsDescriptionNoteText =
				"Goods Description Line 1\n" +
				"Goods Description Line 2\n" +
				"Goods Description Line 3\n" +
				"";

			Shipment.JS_UnitOfWeight = Core.Constants.Weight.Tonnes;
			Shipment.JS_ActualWeight = 5.5m;

			Shipment.JS_UnitOfVolume = Core.Constants.Volume.Litre;
			Shipment.JS_ActualVolume = 2500;

			Shipment.JS_OuterPacks = 20;
			Shipment.JS_F3_NKPackType = "BAG";

			Constants["MarksAndNumbersLeftPadding"] = 0;
			Constants["MarksAndNumbersWidth"] = 20;
			Constants["GoodsDescLeftPadding"] = 1;
			Constants["GoodsDescriptionWidth"] = 20;
			Constants["GrossWeightLeftPadding"] = 1;
			Constants["GrossWeightWidth"] = 12;
			Constants["VolumeMeasurementLeftPadding"] = 1;
			Constants["VolumeMeasurementWidth"] = 12;
			Constants["PackagesWidth"] = 8;
			Constants["PackagesIndex"] = 6;

			Constants["MarksAndNumbersAndGoodsDescriptionHeight"] = 3;
			Constants["ShowDetailHeadingInMainBody"] = "N";

			const string expectedHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"Marks & Numbers      Goods Description        Gross Wt       Volume    Packs\n" +
				"";

			const string expectedBody =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5..
				"Marks and Numbers    Goods Description           5.5 T       2500 L   20 BAG\n" +
				"Line 1               Line 1                                                 \n" +
				"Marks and Numbers    Goods Description                                      \n" +
				"";

			const string expectedFollowOn =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5..
				"Line 2               Line 2                                                 \n" +
				"Marks and Numbers    Goods Description                                      \n" +
				"Line 3               Line 3                                                 \n" +
				"";

			AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);

			AssertMultilineASCIIEquals("Heading", expectedHeading, Supporter.FormedPages.DetailsSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedBody, Supporter.FormedPages[0].MainBodyDetailsSection);
			AssertMultilineASCIIEquals("FollowOn", expectedHeading + expectedFollowOn, ZString.Join("\n", Supporter.FollowOnSection));
			AssertEquals("HasFollowOn", true, Supporter.HasFollowOnSection);

			Constants["MarksAndNumbersAndGoodsDescriptionHeight"] = 7;
			Constants["ShowDetailHeadingInMainBody"] = "Y";
			Reset();

			AssertMultilineASCIIEquals("Heading", expectedHeading, Supporter.FormedPages.DetailsSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedHeading + expectedBody + expectedFollowOn, Supporter.FormedPages[0].MainBodyDetailsSection);
			AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", Supporter.FollowOnSection));
			AssertEquals("HasFollowOn", false, Supporter.HasFollowOnSection);
		}

		public void TestContainersSection()
		{
			GetNewHeader(Shipment);

			CommonConsol consol = Factory.New<CommonConsol>();
			Shipment.Consols.Add(consol);

			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_SealNum = "Seal";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_TareWeight = 2200;

			PackLine packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine.JL_PackageCount = 4;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_ActualWeight = 1500;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualVolume = 2.5;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Keg;
			packLine.JL_PackageCount = 20;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_ActualWeight = 10000;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualVolume = 7.5;

			container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100027";
			container.JC_SealNum = "Seal2";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			container.JC_TareWeight = 3600;
			container.JC_DeliveryMode = "CY";
			container.JC_SetPointTemp = -12;
			container.JC_HumidityPercent = 40;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine.JL_PackageCount = 8;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			packLine.JL_ActualWeight = 15;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualVolume = 8;

			container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100032";
			container.JC_SealNum = "Seal3";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			container.JC_TareWeight = 2300m;
			container.JC_SetPointTemp = 4;
			container.JC_HumidityPercent = 20;
			container.JC_DeliveryMode = "X";

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine.JL_PackageCount = 12;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			packLine.JL_ActualWeight = 17;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualVolume = 5;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.Containers.RemoveAll();
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			packLine.JL_PackageCount = 8;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			packLine.JL_ActualWeight = 111;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicDecimetres;
			packLine.JL_ActualVolume = 888;

			Constants["ContainerNumberLeftPadding"] = 0;
			Constants["ContainerNumberWidth"] = 11;
			Constants["ContainerSealLeftPadding"] = 1;
			Constants["ContainerSealWidth"] = 5;
			Constants["ContainerTypeLeftPadding"] = 1;
			Constants["ContainerTypeWidth"] = 8;
			Constants["ContainerWeightLeftPadding"] = 1;
			Constants["ContainerWeightWidth"] = 11;
			Constants["ContainerGrossLeftPadding"] = 1;
			Constants["ContainerGrossWidth"] = 11;
			Constants["ContainerTareLeftPadding"] = 1;
			Constants["ContainerTareWidth"] = 11;
			Constants["ContainerVolumeLeftPadding"] = 1;
			Constants["ContainerVolumeWidth"] = 11;
			Constants["ContainerPackagesLeftPadding"] = 1;
			Constants["ContainerPackagesWidth"] = 11;
			Constants["ContainerModeIndex"] = 9;
			Constants["ContainerTemperatureSettingIndex"] = 10;
			Constants["ContainerHumiditySettingIndex"] = 11;

			Constants["NumberOfContainerRows"] = 4;
			Constants["ShowContainerHeadingInMainBody"] = "Y";

			Supporter.DisplayContainers = true;

			const string expectedHeading =
				//                                                                                                   1    1    1
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0..
				"Cn. No      Seal  Type        Net (kg)   Tare (kg)  Gross (kg) Volume (M3)       Packs       Mode Temp. Humidity\n" +
				"";

			const string expectedBody =
				//                                                                                                   1    1    1
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0..
				"FAKE4100011 Seal  20GP           11500        2200       13700          10      24 PCE          -               \n" +
				"FAKE4100027 Seal2 40RE           15000        3600       18600           8       8 PLT        CY*  -12C      40%\n" +
				"FAKE4100032 Seal3 20RE           17000        2300       19300           5      12 PLT          X    4C      20%\n" +
				"";

			const string expectedFollowOn =
				//                                                                                                   1    1    1
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0..
				"-           -     -             111000           -      111000       0.888       8 PKG          -     -        -\n" +
				"";

			const string expectedHeadingWithoutContainerWeights =
				//                                                                                                   1    1    1
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0..
				"Cn. No      Seal  Type        Net (kg) Volume (M3)       Packs       Mode Temp. Humidity\n" +
				"";

			const string expectedBodyWithoutContainerWeights =
				//                                                                                                   1    1    1
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0..
				"FAKE4100011 Seal  20GP           11500          10      24 PCE          -               \n" +
				"FAKE4100027 Seal2 40RE           15000           8       8 PLT        CY*  -12C      40%\n" +
				"FAKE4100032 Seal3 20RE           17000           5      12 PLT          X    4C      20%\n" +
				"";

			const string expectedFollowOnWithoutContainerWeights =
				//                                                                                                   1    1    1
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0..
				"-           -     -             111000       0.888       8 PKG          -     -        -\n" +
				"";

			AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);
			AssertMultilineASCIIEquals("Heading", expectedHeading, Supporter.FormedPages.ContainersSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedHeading + expectedBody, Supporter.FormedPages[0].MainBodyContainersSection);
			AssertMultilineASCIIEquals("FollowOn", expectedHeading + expectedFollowOn, ZString.Join("\n", Supporter.FollowOnSection));
			AssertEquals("HasFollowOn", true, Supporter.HasFollowOnSection);

			Constants["ShowContainerHeadingInMainBody"] = "N";
			Reset();

			AssertMultilineASCIIEquals("Heading", expectedHeading, Supporter.FormedPages.ContainersSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedBody + expectedFollowOn, Supporter.FormedPages[0].MainBodyContainersSection);
			AssertMultilineASCIIEquals("FollowOn", ZString.Empty, ZString.Join("\n", Supporter.FollowOnSection));
			AssertEquals("HasFollowOn", false, Supporter.HasFollowOnSection);

			Supporter.HideContainerGrossWeight = true;
			Supporter.HideContainerTareWeight = true;
			Reset();

			AssertMultilineASCIIEquals("Heading", expectedHeadingWithoutContainerWeights, Supporter.FormedPages.ContainersSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedBodyWithoutContainerWeights + expectedFollowOnWithoutContainerWeights, Supporter.FormedPages[0].MainBodyContainersSection);
			AssertMultilineASCIIEquals("FollowOn", ZString.Empty, ZString.Join("\n", Supporter.FollowOnSection));
			AssertEquals("HasFollowOn", false, Supporter.HasFollowOnSection);
		}

		public void TestContainersSection_NoContainerNumber()
		{
			GetNewHeader(Shipment);

			var consol = Factory.New<CommonConsol>();
			Shipment.Consols.Add(consol);

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "";
			container.JC_SealNum = "Seal";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_ContainerCount = 1;

			var packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_PackageCount = 4;
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine.JL_ActualWeight = 1500;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_ActualVolume = 2.5;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;

			Constants["ContainerNumberLeftPadding"] = 0;
			Constants["ContainerNumberWidth"] = 11;
			Constants["ContainerSealLeftPadding"] = 1;
			Constants["ContainerSealWidth"] = 5;
			Constants["ContainerTypeLeftPadding"] = 1;
			Constants["ContainerTypeWidth"] = 8;
			Constants["ContainerWeightLeftPadding"] = 1;
			Constants["ContainerWeightWidth"] = 11;
			Constants["ContainerGrossLeftPadding"] = 1;
			Constants["ContainerGrossWidth"] = 11;
			Constants["ContainerTareLeftPadding"] = 1;
			Constants["ContainerTareWidth"] = 11;
			Constants["ContainerVolumeLeftPadding"] = 1;
			Constants["ContainerVolumeWidth"] = 11;
			Constants["ContainerPackagesLeftPadding"] = 1;
			Constants["ContainerPackagesWidth"] = 11;
			Constants["ContainerModeIndex"] = 9;
			Constants["ContainerTemperatureSettingIndex"] = 10;
			Constants["ContainerHumiditySettingIndex"] = 11;

			Constants["NumberOfContainerRows"] = 4;
			Constants["ShowContainerHeadingInMainBody"] = "Y";
			Constants["InterleavePacksAndContainers"] = "Y";

			Supporter.DisplayContainers = true;
			Supporter.HidePackLinesInContainersSection = false;

			const string expectedHeading =
				"Cn. No      Seal  Type        Net (kg)   Tare (kg)  Gross (kg) Volume (M3)       Packs       Mode Temp. Humidity\n";

			const string expectedBody =
				"            Seal  20GP            1500        2280        3780         2.5       4 PLT          -               \n" +
				"                             4           1500            2.5       0.000       0.000       0.000          0.000\n";

			AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);
			AssertMultilineASCIIEquals("Heading", expectedHeading, Supporter.FormedPages.ContainersSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedHeading + expectedBody, Supporter.FormedPages[0].MainBodyContainersSection);
			AssertEquals("HasFollowOn", false, Supporter.HasFollowOnSection);
		}

		public void TestContainersSection_MultiPage()
		{
			GetNewHeader(Shipment);

			CommonConsol consol = Factory.New<CommonConsol>();
			Shipment.Consols.Add(consol);

			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_SealNum = "Seal";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_TareWeight = 2200;

			PackLine packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine.JL_PackageCount = 4;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_ActualWeight = 1500;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualVolume = 2.5;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Keg;
			packLine.JL_PackageCount = 20;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_ActualWeight = 10000;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualVolume = 7.5;

			container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100027";
			container.JC_SealNum = "Seal2";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			container.JC_TareWeight = 3600;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine.JL_PackageCount = 8;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			packLine.JL_ActualWeight = 15;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualVolume = 8;

			container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100032";
			container.JC_SealNum = "Seal3";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			container.JC_TareWeight = 2300m;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			packLine.JL_PackageCount = 13;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			packLine.JL_ActualWeight = 11;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicDecimetres;
			packLine.JL_ActualVolume = 88;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.Containers.RemoveAll();
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			packLine.JL_PackageCount = 8;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			packLine.JL_ActualWeight = 111;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicDecimetres;
			packLine.JL_ActualVolume = 888;

			Constants["UseMultiPage"] = "Y";
			Constants["ContainerNumberLeftPadding"] = 0;
			Constants["ContainerNumberWidth"] = 11;
			Constants["ContainerSealLeftPadding"] = 1;
			Constants["ContainerSealWidth"] = 5;
			Constants["ContainerTypeLeftPadding"] = 1;
			Constants["ContainerTypeWidth"] = 8;
			Constants["ContainerWeightLeftPadding"] = 1;
			Constants["ContainerWeightWidth"] = 11;
			Constants["ContainerGrossLeftPadding"] = 1;
			Constants["ContainerGrossWidth"] = 11;
			Constants["ContainerTareLeftPadding"] = 1;
			Constants["ContainerTareWidth"] = 11;
			Constants["ContainerVolumeLeftPadding"] = 1;
			Constants["ContainerVolumeWidth"] = 11;
			Constants["ContainerPackagesLeftPadding"] = 1;
			Constants["ContainerPackagesWidth"] = 11;

			Constants["NumberOfContainerRows"] = 4;
			Constants["ShowContainerHeadingInMainBody"] = "Y";

			Supporter.DisplayContainers = true;

			const string expectedHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"Cn. No      Seal  Type        Net (kg)   Tare (kg)  Gross (kg) Volume (M3)       Packs\n" +
				"";

			const string expectedBody1 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"FAKE4100011 Seal  20GP           11500        2200       13700          10      24 PCE\n" +
				"FAKE4100027 Seal2 40RE           15000        3600       18600           8       8 PLT\n" +
				"FAKE4100032 Seal3 20RE           11000        2300       13300       0.088      13 PKG\n" +
				"";

			const string expectedBody2 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"-           -     -             111000           -      111000       0.888       8 PKG\n" +
				"";

			AssertEquals("should have 2 formed pages", 2, Supporter.FormedPages.Count);
			AssertMultilineASCIIEquals("Body1", expectedHeading + expectedBody1, Supporter.FormedPages[0].MainBodyContainersSection);
			AssertMultilineASCIIEquals("Body2", expectedHeading + expectedBody2, Supporter.FormedPages[1].MainBodyContainersSection);
			AssertMultilineASCIIEquals("FollowOn", ZString.Empty, ZString.Join("\n", Supporter.FollowOnSection));
			AssertEquals("HasFollowOn", false, Supporter.HasFollowOnSection);
		}

		public void TestContainersSection_Interleave()
		{
			GetNewHeader(Shipment);

			CommonConsol consol = Factory.New<CommonConsol>();
			Shipment.Consols.Add(consol);

			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_SealNum = "Seal";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_TareWeight = 2200;

			PackLine packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_DetailedDescription = "Detailed Description 1";
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine.JL_PackageCount = 4;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_ActualWeight = 1500;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualVolume = 2.5;
			packLine.JL_HarmonisedCode = "HSCODE1";
			packLine.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0440", "", "IMO").First().PK;
			packLine.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "2305", "", "IMO").First().PK;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_DetailedDescription = "Detailed Description 2";
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Keg;
			packLine.JL_PackageCount = 20;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_ActualWeight = 10000;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualVolume = 7.5;
			packLine.JL_HarmonisedCode = "HSCODE2";
			packLine.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0138", "", "IMO").First().PK;

			container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100027";
			container.JC_SealNum = "Seal2";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			container.JC_TareWeight = 3600;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_DetailedDescription = "Detailed Description 3";
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine.JL_PackageCount = 8;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			packLine.JL_ActualWeight = 15;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualVolume = 8;
			packLine.JL_HarmonisedCode = "HSCODE3";

			container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100032";
			container.JC_SealNum = "Seal3";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			container.JC_TareWeight = 2300m;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_DetailedDescription = "Detailed Description 4";
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			packLine.JL_PackageCount = 8;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			packLine.JL_ActualWeight = 111;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicDecimetres;
			packLine.JL_ActualVolume = 888;
			packLine.JL_HarmonisedCode = "HSCODE4";

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.JL_DetailedDescription = "Detailed Description 5";
			packLine.Containers.RemoveAll();
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			packLine.JL_PackageCount = 2;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			packLine.JL_ActualWeight = 11;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicDecimetres;
			packLine.JL_ActualVolume = 84;
			packLine.JL_HarmonisedCode = "HSCODE5";

			Constants["ContainerNumberLeftPadding"] = 0;
			Constants["ContainerNumberWidth"] = 11;
			Constants["ContainerSealLeftPadding"] = 1;
			Constants["ContainerSealWidth"] = 5;
			Constants["ContainerTypeLeftPadding"] = 1;
			Constants["ContainerTypeWidth"] = 8;
			Constants["ContainerWeightLeftPadding"] = 4;
			Constants["ContainerWeightWidth"] = 11;
			Constants["ContainerGrossLeftPadding"] = 1;
			Constants["ContainerGrossWidth"] = 19;
			Constants["ContainerTareLeftPadding"] = 1;
			Constants["ContainerTareWidth"] = 19;
			Constants["ContainerVolumeLeftPadding"] = 1;
			Constants["ContainerVolumeWidth"] = 11;
			Constants["ContainerPackagesLeftPadding"] = 12;
			Constants["ContainerPackagesWidth"] = 11;

			Constants["PackRefNumberColumnWidth"] = 0;
			Constants["PackLengthColumnWidth"] = 0;
			Constants["PackWidthColumnWidth"] = 0;
			Constants["PackHeightColumnWidth"] = 0;
			Constants["PackAreaColumnWidth"] = 0;

			Constants["PackDescriptionColumnLeftPadding"] = 3;
			Constants["PackDescriptionColumnWidth"] = 23;
			Constants["PackDescriptionColumnIndex"] = 1;
			Constants["PackWeightColumnIndex"] = 2;
			Constants["PackWeightAndUQColumnWidth"] = 11;
			Constants["PackWeightAndUQColumnIndex"] = 3;
			Constants["PackUNDGColumnWidth"] = 23;
			Constants["PackUNDGColumnIndex"] = 4;
			Constants["PackVolumeColumnIndex"] = 5;
			Constants["PackVolumeColumnLeftPadding"] = 2;
			Constants["PackVolumeAndUQColumnLeftPadding"] = 1;
			Constants["PackVolumeAndUQColumnWidth"] = 11;
			Constants["PackVolumeAndUQColumnIndex"] = 6;
			Constants["PackCountColumnWidth"] = 10;
			Constants["PackCountColumnIndex"] = 7;
			Constants["PackHarmonizedCodeColumnWidth"] = 7;
			Constants["PackHarmonizedCodeColumnIndex"] = 8;

			Constants["NumberOfContainerRows"] = 8;
			Constants["ShowContainerHeadingInMainBody"] = "Y";
			Constants["InterleavePacksAndContainers"] = "Y";

			const string expectedHeading =
				//                                                                                                   1    1    1    1
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1    1
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....
				"Cn. No      Seal  Type           Net (kg)           Tare (kg)          Gross (kg) Volume (M3)                  Packs\n" +
				"";

			const string expectedBodyContainer1 =
				//                                                                                                   1    1    1    1
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1    1
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....
				"FAKE4100011 Seal  20GP              11500                2200               13700          10                 24 PCE\n" +
				"";

			const string expectedBodyPack1 =
				//                                                                                                   1    1    1    1
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1    1
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....
				"   Detailed Description 1            1500     1500 KG UN0440, CHARGES,                    2.5      2.5 M3          4 HSCODE1\n" +
				"                                                      SHAPED, class 1.4D                                                    \n" +
				"                                                      UN2305,                                                               \n" +
				"                                                      NITROBENZENESULPHONIC                                                 \n" +
				"                                                      ACID, class 8, PG II                                                  \n" +
				"   Detailed Description 2           10000    10000 KG UN0138, MINES, class                7.5      7.5 M3         20 HSCODE2\n" +
				"";

			const string expectedBodyPack1LastLine =
				//                                                                                                   1    1    1    1
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1    1
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....
				"                                                      1.2D                                                                  \n" +
				"";

			const string expectedBodyContainer2 =
				//                                                                                                   1    1    1    1
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1    1
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....
				"FAKE4100027 Seal2 40RE              15000                3600               18600           8                  8 PLT\n" +
				"";

			const string expectedBodyPack2 =
				//                                                                                                   1    1    1    1
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1    1
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....
				"   Detailed Description 3           15000    15000 KG                                       8        8 M3          8 HSCODE3\n" +
				"";

			const string expectedBodyContainer3 =
				//                                                                                                   1    1    1    1
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1    1
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....
				"FAKE4100032 Seal3 20RE             111000                2300              113300       0.888                  8 PKG\n" +
				"";

			const string expectedBodyPack3 =
				//                                                                                                   1    1    1    1
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1    1
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....
				"   Detailed Description 4          111000   111000 KG                                   0.888    0.888 M3          8 HSCODE4\n" +
				"";

			const string expectedBodyUnpackedContainer =
				//                                                                                                   1    1    1    1
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1    1
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....
				"-           -     -                 11000                   -               11000       0.084                  2 PKG\n" +
				"";

			const string expectedBodyUnpackedLine =
				"   Detailed Description 5           11000    11000 KG                                   0.084    0.084 M3          2 HSCODE5\n" +
				"";

			Supporter.DisplayContainers = true;

			AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);
			AssertMultilineASCIIEquals("Heading", expectedHeading, Supporter.FormedPages.ContainersSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedHeading + expectedBodyContainer1 + expectedBodyPack1, Supporter.FormedPages[0].MainBodyContainersSection);
			AssertMultilineASCIIEquals("FollowOn",
					expectedHeading + expectedBodyPack1LastLine + expectedBodyContainer2 + expectedBodyPack2 + expectedBodyContainer3 + expectedBodyPack3 + expectedBodyUnpackedContainer + expectedBodyUnpackedLine,
					ZString.Join("\n", Supporter.FollowOnSection));
			AssertEquals("HasFollowOn", true, Supporter.HasFollowOnSection);

			Constants["ShowContainerHeadingInMainBody"] = "N";
			Reset();

			AssertMultilineASCIIEquals("Heading", expectedHeading, Supporter.FormedPages.ContainersSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedBodyContainer1 + expectedBodyPack1 + expectedBodyPack1LastLine, Supporter.FormedPages[0].MainBodyContainersSection);
			AssertMultilineASCIIEquals("FollowOn",
					expectedHeading + expectedBodyContainer2 + expectedBodyPack2 + expectedBodyContainer3 + expectedBodyPack3 + expectedBodyUnpackedContainer + expectedBodyUnpackedLine,
					ZString.Join("\n", Supporter.FollowOnSection));
			AssertEquals("HasFollowOn", true, Supporter.HasFollowOnSection);

			Supporter.HidePackLinesInContainersSection = true;
			Reset();

			AssertMultilineASCIIEquals("Heading", expectedHeading, Supporter.FormedPages.ContainersSectionHeader);
			AssertMultilineASCIIEquals("HidePackLinesInContainersSection overrides template constant",
					expectedBodyContainer1 + expectedBodyContainer2 + expectedBodyContainer3 + expectedBodyUnpackedContainer,
					Supporter.FormedPages[0].MainBodyContainersSection);
			AssertEquals("HasFollowOn", false, Supporter.HasFollowOnSection);
		}

		public void TestContainersSection_PacklineCommodity()
		{
			GetNewHeader(Shipment);

			var consol = Factory.New<CommonConsol>();
			Shipment.Consols.Add(consol);

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_SealNum = "Seal";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_TareWeight = 2200;

			var packLine = Shipment.OuterPackLines.AddNew();
			packLine.JL_DetailedDescription = "Detailed Description 1";
			packLine.SetContainer(container.PK);
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine.JL_PackageCount = 4;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_ActualWeight = 1500;
			packLine.JL_Length = 5;
			packLine.JL_Width = 4;
			packLine.JL_Height = 2;
			packLine.JL_UnitOfDimension = Core.Constants.Length.Metres;
			packLine.JL_ActualVolume = 40;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_RH_NKCommodityCode = string.Empty;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.JL_DetailedDescription = "Detailed Description 2";
			packLine.SetContainer(container.PK);
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Keg;
			packLine.JL_PackageCount = 20;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_ActualWeight = 10000;
			packLine.JL_Length = 70;
			packLine.JL_Width = 60;
			packLine.JL_UnitOfDimension = Core.Constants.Length.Inches;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualVolume = 7.5;
			packLine.JL_RH_NKCommodityCode = "GEN";

			Constants["ContainerNumberLeftPadding"] = 0;
			Constants["ContainerNumberWidth"] = 11;
			Constants["ContainerSealLeftPadding"] = 1;
			Constants["ContainerSealWidth"] = 5;
			Constants["ContainerTypeLeftPadding"] = 1;
			Constants["ContainerTypeWidth"] = 8;
			Constants["ContainerWeightLeftPadding"] = 4;
			Constants["ContainerWeightWidth"] = 11;
			Constants["ContainerGrossLeftPadding"] = 1;
			Constants["ContainerGrossWidth"] = 19;
			Constants["ContainerTareLeftPadding"] = 1;
			Constants["ContainerTareWidth"] = 19;
			Constants["ContainerVolumeLeftPadding"] = 1;
			Constants["ContainerVolumeWidth"] = 11;
			Constants["ContainerPackagesLeftPadding"] = 12;
			Constants["ContainerPackagesWidth"] = 11;

			Constants["PackRefNumberColumnWidth"] = 0;
			Constants["PackLengthColumnWidth"] = 0;
			Constants["PackWidthColumnWidth"] = 0;
			Constants["PackHeightColumnWidth"] = 0;
			Constants["PackAreaColumnWidth"] = 0;

			Constants["PackDescriptionColumnLeftPadding"] = 3;
			Constants["PackDescriptionColumnWidth"] = 23;
			Constants["PackDescriptionColumnIndex"] = 1;
			Constants["PackWeightColumnIndex"] = 2;
			Constants["PackWeightAndUQColumnWidth"] = 11;
			Constants["PackWeightAndUQColumnIndex"] = 3;
			Constants["PackUNDGColumnWidth"] = 23;
			Constants["PackUNDGColumnIndex"] = 4;
			Constants["PackVolumeColumnIndex"] = 5;
			Constants["PackVolumeColumnLeftPadding"] = 2;
			Constants["PackCountColumnWidth"] = 10;
			Constants["PackCountColumnIndex"] = 6;
			Constants["PackCommodityCodeColumnLeftPadding"] = 5;
			Constants["PackCommodityCodeColumnIndex"] = 7;
			Constants["PackCommodityCodeColumnWidth"] = 3;
			Constants["NumberOfContainerRows"] = 8;
			Constants["ShowContainerHeadingInMainBody"] = "Y";
			Constants["InterleavePacksAndContainers"] = "Y";

			const string expectedHeading =
				//                                                                                                   1    1    1    1
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1    1
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....
				"Cn. No      Seal  Type           Net (kg)           Tare (kg)          Gross (kg) Volume (M3)                  Packs\n" +
				"";

			const string expectedBody =
				//                                                                                                   1    1    1    1
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1    1
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....
				"FAKE4100011 Seal  20GP              11500                2200               13700       167.5                 24 PCE\n" +
				"   Detailed Description 1            1500     1500 KG                                     160          4        \n" +
				"   Detailed Description 2           10000    10000 KG                                     7.5         20     GEN" +
				"";

			Supporter.DisplayContainers = true;

			AssertMultilineASCIIEquals("Body", expectedHeading + expectedBody, Supporter.FormedPages[0].MainBodyContainersSection);
		}

		public void TestContainersSection_CombinedColumnsFCL()
		{
			GetNewHeader(Shipment);

			CommonConsol consol = Factory.New<CommonConsol>();
			Shipment.Consols.Add(consol);

			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_SealNum = "Seal";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_TareWeight = 2200;

			PackLine packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_DetailedDescription = "Detailed Description 1";
			packLine.JL_Description = "Short Description 1";
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine.JL_PackageCount = 4;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_ActualWeight = 1500;
			packLine.JL_Length = 5;
			packLine.JL_Width = 4;
			packLine.JL_Height = 2;
			packLine.JL_UnitOfDimension = Core.Constants.Length.Metres;
			packLine.JL_ActualVolume = 40;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0440", "", "IMO").First().PK;
			packLine.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "2305", "", "IMO").First().PK;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_DetailedDescription = "Detailed Description 2";
			packLine.JL_Description = "Short Description 2";
			packLine.JL_MarksAndNumbers = "M&N for the Kegs";
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Keg;
			packLine.JL_PackageCount = 20;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_ActualWeight = 10000;
			packLine.JL_Length = 70;
			packLine.JL_Width = 60;
			packLine.JL_UnitOfDimension = Core.Constants.Length.Inches;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualVolume = 7.5;
			packLine.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0138", "", "IMO").First().PK;

			container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100027";
			container.JC_SealNum = "Seal2";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			container.JC_TareWeight = 3600;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_DetailedDescription = "Detailed Description 3";
			packLine.JL_Description = "Short Description 3";
			packLine.JL_RefNumber = "Ref for the Pallets";
			packLine.JL_MarksAndNumbers = "M&N for the pallets";
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine.JL_PackageCount = 8;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			packLine.JL_ActualWeight = 15;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualVolume = 8;

			container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100032";
			container.JC_SealNum = "Seal3";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			container.JC_TareWeight = 2300m;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_DetailedDescription = "Detailed Description 4";
			packLine.JL_Description = "Short Description 4";
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			packLine.JL_PackageCount = 8;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			packLine.JL_ActualWeight = 111;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicDecimetres;
			packLine.JL_ActualVolume = 888;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.JL_DetailedDescription = "Detailed Description 5";
			packLine.JL_Description = "Short Description 5";
			packLine.Containers.RemoveAll();
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			packLine.JL_PackageCount = 2;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			packLine.JL_ActualWeight = 11;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicDecimetres;
			packLine.JL_ActualVolume = 84;

			Constants["ContainerNumberLeftPadding"] = 0;
			Constants["ContainerNumberWidth"] = 11;
			Constants["ContainerSealLeftPadding"] = 1;
			Constants["ContainerSealWidth"] = 5;
			Constants["ContainerTypeLeftPadding"] = 1;
			Constants["ContainerTypeWidth"] = 8;
			Constants["ContainerWeightLeftPadding"] = 4;
			Constants["ContainerWeightWidth"] = 11;
			Constants["ContainerGrossLeftPadding"] = 1;
			Constants["ContainerGrossWidth"] = 19;
			Constants["ContainerTareLeftPadding"] = 1;
			Constants["ContainerTareWidth"] = 19;
			Constants["ContainerVolumeLeftPadding"] = 1;
			Constants["ContainerVolumeWidth"] = 11;
			Constants["ContainerPackagesLeftPadding"] = 12;
			Constants["ContainerPackagesWidth"] = 11;

			Constants["PackRefNumberColumnWidth"] = 0;
			Constants["PackMarksAndNumbersColumnWidth"] = 0;
			Constants["PackLengthColumnWidth"] = 0;
			Constants["PackWidthColumnWidth"] = 0;
			Constants["PackHeightColumnWidth"] = 0;
			Constants["PackAreaColumnWidth"] = 0;

			Constants["PackDescriptionAndUNDGColumnIndex"] = 1;
			Constants["PackDescriptionAndUNDGColumnLeftPadding"] = 3;
			Constants["PackDescriptionAndUNDGColumnWidth"] = 43;
			Constants["PackDimensionsColumnIndex"] = 2;
			Constants["PackDimensionsColumnLeftPadding"] = 2;
			Constants["PackDimensionsColumnWidth"] = 24;
			Constants["PackRefNumberAndMarksAndNumbersColumnIndex"] = 3;
			Constants["PackRefNumberAndMarksAndNumbersColumnWidth"] = 24;
			Constants["PackRefNumberAndMarksAndNumbersColumnLeftPadding"] = 1;
			Constants["PackUNDGAndShortDescriptionColumnIndex"] = 4;
			Constants["PackUNDGAndShortDescriptionColumnLeftPadding"] = 5;
			Constants["PackUNDGAndShortDescriptionColumnWidth"] = 44;

			Constants["PackDescriptionColumnIndex"] = 0;
			Constants["PackWeightColumnIndex"] = 0;
			Constants["PackVolumeColumnIndex"] = 0;
			Constants["PackUNDGColumnIndex"] = 0;
			Constants["PackCountColumnIndex"] = 0;

			Constants["NumberOfContainerRows"] = 8;
			Constants["ShowContainerHeadingInMainBody"] = "Y";
			Constants["InterleavePacksAndContainers"] = "Y";
			Constants["MarksAndNumbersWidth"] = 0;
			Constants["GoodsDescriptionWidth"] = 0;
			Constants["DimensionsDecimalPlaces"] = 2;

			const string expectedHeading =
				//                                                                                                   1    1    1    1    1    1    1    1    1    1    1    
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1    1    2    2    3    3    4    4    5    
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....
				"Cn. No      Seal  Type           Net (kg)           Tare (kg)          Gross (kg) Volume (M3)                  Packs\n" +
				"";

			const string expectedBody1 =
				//                                                                                                   1    1    1    1    1    1    1    1    1    1    1    
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1    1    2    2    3    3    4    4    5    
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....
				"FAKE4100011 Seal  20GP              11500                2200               13700       167.5                 24 PCE\n" +
				"   Detailed Description 1                        160 M3 5.00x4.00x2.00 M                              UN0440, CHARGES, SHAPED, class 1.4D         \n" +
				"   UN0440, CHARGES, SHAPED, class 1.4D                                                                UN2305, NITROBENZENESULPHONIC ACID, class 8,\n" +
				"   UN2305, NITROBENZENESULPHONIC ACID, class                                                          PG II                                       \n" +
				"   8, PG II                                                                                           Short Description 1                         \n" +
				"   Detailed Description 2                           7.5 M3 1.778x1.524 M M&N for the Kegs             UN0138, MINES, class 1.2D                   \n" +
				"   UN0138, MINES, class 1.2D                                                                          Short Description 2                         \n" +
				"";

			const string expectedBody2 =
				//                                                                                                   1    1    1    1    1    1    1    1    1    1    1    
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1    1    2    2    3    3    4    4    5    
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....
				"FAKE4100027 Seal2 40RE              15000                3600               18600           8                  8 PLT\n" +
				"";

			const string expectedFollowOn =
				//                                                                                                   1    1    1    1    1    1    1    1    1    1    1    
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1    1    2    2    3    3    4    4    5    
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....
				"   Detailed Description 3                                           8 M3 Ref for the Pallets          Short Description 3                         \n" +
				"                                                                         M&N for the pallets                                                      \n" +
				"FAKE4100032 Seal3 20RE             111000                2300              113300       0.888                  8 PKG\n" +
				"   Detailed Description 4                                       0.888 M3                              Short Description 4                         \n" +
				"-           -     -                 11000                   -               11000       0.084                  2 PKG\n" +
				"   Detailed Description 5                                       0.084 M3                              Short Description 5                         \n" +
				"";

			Supporter.DisplayContainers = true;

			AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);
			AssertMultilineASCIIEquals("Heading", expectedHeading, Supporter.FormedPages.ContainersSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedHeading + expectedBody1, Supporter.FormedPages[0].MainBodyContainersSection);
			AssertMultilineASCIIEquals("FollowOn", expectedHeading + expectedBody2 + expectedFollowOn, ZString.Join("\n", Supporter.FollowOnSection));
			AssertEquals("HasFollowOn", true, Supporter.HasFollowOnSection);

			Constants["ShowContainerHeadingInMainBody"] = "N";
			Reset();

			AssertMultilineASCIIEquals("Heading", expectedHeading, Supporter.FormedPages.ContainersSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedBody1 + expectedBody2, Supporter.FormedPages[0].MainBodyContainersSection);
			AssertMultilineASCIIEquals("FollowOn", expectedHeading + expectedFollowOn, ZString.Join("\n", Supporter.FollowOnSection));
			AssertEquals("HasFollowOn", true, Supporter.HasFollowOnSection);
		}

		public void TestHazardous()
		{
			GetNewHeader(Shipment);

			CommonConsol consol = Factory.New<CommonConsol>();
			Shipment.Consols.Add(consol);

			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_SealNum = "Seal";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_TareWeight = 2200;

			PackLine packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_DetailedDescription = "Detailed Description 1";
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine.JL_PackageCount = 4;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_ActualWeight = 1500;
			packLine.JL_Length = 5;
			packLine.JL_Width = 4;
			packLine.JL_Height = 2;
			packLine.JL_UnitOfDimension = Core.Constants.Length.Metres;
			packLine.JL_ActualVolume = 40;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0440", "", "IMO").First().PK;
			packLine.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "2305", "", "IMO").First().PK;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_DetailedDescription = "Detailed Description 2";
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Keg;
			packLine.JL_PackageCount = 20;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_ActualWeight = 10000;
			packLine.JL_Length = 70;
			packLine.JL_Width = 60;
			packLine.JL_UnitOfDimension = Core.Constants.Length.Inches;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualVolume = 7.5;
			packLine.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0138", "", "IMO").First().PK;

			var dgContact = Factory.New<OrgContact>();
			dgContact.OC_ContactName = "Contact Name";
			dgContact.OC_Phone = "123456";
			packLine.UNDGs[0].DI_OC_DGContact = dgContact.PK;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_DetailedDescription = "Non Hazardous";
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Bundle;
			packLine.JL_PackageCount = 14;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_ActualWeight = 1000;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualVolume = 4.5;

			container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100027";
			container.JC_SealNum = "Seal2";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			container.JC_TareWeight = 3600;

			Constants["ContainerNumberLeftPadding"] = 0;
			Constants["ContainerNumberWidth"] = 11;
			Constants["ContainerSealLeftPadding"] = 1;
			Constants["ContainerSealWidth"] = 5;
			Constants["ContainerTypeLeftPadding"] = 1;
			Constants["ContainerTypeWidth"] = 8;
			Constants["ContainerWeightLeftPadding"] = 4;
			Constants["ContainerWeightWidth"] = 11;
			Constants["ContainerGrossLeftPadding"] = 1;
			Constants["ContainerGrossWidth"] = 19;
			Constants["ContainerTareLeftPadding"] = 1;
			Constants["ContainerTareWidth"] = 19;
			Constants["ContainerVolumeLeftPadding"] = 1;
			Constants["ContainerVolumeWidth"] = 11;
			Constants["ContainerPackagesLeftPadding"] = 12;
			Constants["ContainerPackagesWidth"] = 11;

			Constants["PackRefNumberColumnWidth"] = 0;
			Constants["PackLengthColumnWidth"] = 0;
			Constants["PackWidthColumnWidth"] = 0;
			Constants["PackHeightColumnWidth"] = 0;
			Constants["PackAreaColumnWidth"] = 0;
			Constants["PackWeightColumnIndex"] = 0;
			Constants["PackVolumeColumnIndex"] = 0;
			Constants["PackCountColumnIndex"] = 0;

			Constants["PackContainsUNDGColumnIndex"] = 1;
			Constants["PackUNDGColumnIndex"] = 2;
			Constants["PackUNDGColumnLeftPadding"] = 3;
			Constants["PackUNDGColumnWidth"] = 43;
			Constants["PackDescriptionColumnIndex"] = 3;
			Constants["PackDescriptionColumnWidth"] = 28;

			Constants["NumberOfContainerRows"] = 8;
			Constants["ShowContainerHeadingInMainBody"] = "Y";
			Constants["InterleavePacksAndContainers"] = "Y";

			const string expectedHeading =
				//                                                                                                   1    1    1    1
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1    1
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....
				"Cn. No      Seal  Type           Net (kg)           Tare (kg)          Gross (kg) Volume (M3)                  Packs\n" +
				"";

			const string expectedBody1 =
				//                                                                                                   1    1    1    1
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1    1
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....
				"FAKE4100011 Seal  20GP              12500                2200               14700         172                 38 PCE\n" +
				" X   UN0440, CHARGES, SHAPED, class 1.4D        Detailed Description 1      \n" +
				"     UN2305, NITROBENZENESULPHONIC ACID, class                              \n" +
				"     8, PG II                                                               \n" +
				" X   UN0138, MINES, class 1.2D                  Detailed Description 2      \n" +
				"";

			const string expectedEmergencyContact =
				//                                                                                                   1    1    1    1
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1    1
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....
				"     EMERGENCY CONTACT: Contact Name, phone:                                \n" +
				"     123456                                                                 \n" +
				"";

			const string expectedBody2 =
				//                                                                                                   1    1    1    1
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1    1
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....
				"                                                Non Hazardous               \n" +
				"";

			Supporter.DisplayContainers = true;

			AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);
			AssertMultilineASCIIEquals("Heading", expectedHeading, Supporter.FormedPages.ContainersSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedHeading + expectedBody1 + expectedBody2, Supporter.FormedPages[0].MainBodyContainersSection);
			AssertEquals("HasFollowOn", false, Supporter.HasFollowOnSection);

			Constants["IncludeEmergencyContactWithUNDG"] = "Y";
			Reset();

			AssertMultilineASCIIEquals("Heading", expectedHeading, Supporter.FormedPages.ContainersSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedHeading + expectedBody1 + expectedEmergencyContact, Supporter.FormedPages[0].MainBodyContainersSection);
			AssertEquals("HasFollowOn", true, Supporter.HasFollowOnSection);
			AssertMultilineASCIIEquals("FollowOn", expectedHeading + expectedBody2, ZString.Join("\n", Supporter.FollowOnSection));
		}

		public void TestImperialUnitsInPacksAndContainers()
		{
			GetNewHeader(Shipment);

			CommonConsol consol = Factory.New<CommonConsol>();
			Shipment.Consols.Add(consol);

			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_SealNum = "Seal";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_TareWeight = 4850.17;
			container.JC_GrossWeightUQ = "LB";

			PackLine packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_DetailedDescription = "Detailed Description 1";
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine.JL_PackageCount = 4;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_ActualWeight = 1500;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualVolume = 2.5;
			packLine.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0440", "", "IMO").First().PK;
			packLine.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "2305", "", "IMO").First().PK;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_DetailedDescription = "Detailed Description 2";
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Keg;
			packLine.JL_PackageCount = 20;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Pounds;
			packLine.JL_ActualWeight = 10000;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicFeet;
			packLine.JL_ActualVolume = 7.5;
			packLine.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0138", "", "IMO").First().PK;

			container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100027";
			container.JC_SealNum = "Seal2";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			container.JC_TareWeight = 3600;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_DetailedDescription = "Detailed Description 3";
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine.JL_PackageCount = 8;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			packLine.JL_ActualWeight = 15;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicYards;
			packLine.JL_ActualVolume = 8;

			container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100032";
			container.JC_SealNum = "Seal3";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			container.JC_TareWeight = 2300m;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_DetailedDescription = "Detailed Description 4";
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			packLine.JL_PackageCount = 8;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.LongTons;
			packLine.JL_ActualWeight = 111;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicFeet;
			packLine.JL_ActualVolume = 888;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.JL_DetailedDescription = "Detailed Description 5";
			packLine.Containers.RemoveAll();
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			packLine.JL_PackageCount = 2;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			packLine.JL_ActualWeight = 11;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicFeet;
			packLine.JL_ActualVolume = 84;

			Constants["UseImperialUnits"] = "Y";

			Constants["ContainerNumberLeftPadding"] = 0;
			Constants["ContainerNumberWidth"] = 11;
			Constants["ContainerSealLeftPadding"] = 1;
			Constants["ContainerSealWidth"] = 5;
			Constants["ContainerTypeLeftPadding"] = 1;
			Constants["ContainerTypeWidth"] = 8;
			Constants["ContainerWeightLeftPadding"] = 4;
			Constants["ContainerWeightWidth"] = 11;
			Constants["ContainerWeightCaption"] = "Net (lb)";
			Constants["ContainerGrossLeftPadding"] = 1;
			Constants["ContainerGrossWidth"] = 19;
			Constants["ContainerGrossCaption"] = "Gross (lb)";
			Constants["ContainerTareLeftPadding"] = 1;
			Constants["ContainerTareWidth"] = 19;
			Constants["ContainerTareCaption"] = "Tare (lb)";
			Constants["ContainerVolumeLeftPadding"] = 1;
			Constants["ContainerVolumeWidth"] = 11;
			Constants["ContainerVolumeCaption"] = "Volume (CF)";
			Constants["ContainerPackagesLeftPadding"] = 12;
			Constants["ContainerPackagesWidth"] = 11;
			Constants["ContainerWeightAndUQWidth"] = 13;
			Constants["ContainerWeightAndUQIndex"] = 9;
			Constants["ContainerWeightAndUQLeftPadding"] = 2;
			Constants["ContainerWeightAndUQCaption"] = "Weight+UQ";
			Constants["ContainerTareAndUQWidth"] = 12;
			Constants["ContainerTareAndUQIndex"] = 10;
			Constants["ContainerTareAndUQLeftPadding"] = 1;
			Constants["ContainerTareAndUQCaption"] = "Tare+UQ";
			Constants["ContainerGrossAndUQWidth"] = 15;
			Constants["ContainerGrossAndUQIndex"] = 11;
			Constants["ContainerGrossAndUQLeftPadding"] = 2;
			Constants["ContainerGrossAndUQCaption"] = "Gross+UQ";
			Constants["ContainerVolumeAndUQWidth"] = 16;
			Constants["ContainerVolumeAndUQIndex"] = 12;
			Constants["ContainerVolumeAndUQLeftPadding"] = 3;
			Constants["ContainerVolumeAndUQCaption"] = "Volume+UQ";

			Constants["PackRefNumberColumnWidth"] = 0;
			Constants["PackLengthColumnWidth"] = 0;
			Constants["PackWidthColumnWidth"] = 0;
			Constants["PackHeightColumnWidth"] = 0;
			Constants["PackAreaColumnWidth"] = 0;

			Constants["PackDescriptionColumnLeftPadding"] = 3;
			Constants["PackDescriptionColumnWidth"] = 23;
			Constants["PackDescriptionColumnIndex"] = 1;
			Constants["PackWeightColumnIndex"] = 2;
			Constants["PackWeightAndUQColumnWidth"] = 11;
			Constants["PackWeightAndUQColumnIndex"] = 3;
			Constants["PackUNDGColumnWidth"] = 23;
			Constants["PackUNDGColumnIndex"] = 4;
			Constants["PackVolumeColumnIndex"] = 5;
			Constants["PackVolumeColumnLeftPadding"] = 2;
			Constants["PackVolumeAndUQColumnLeftPadding"] = 1;
			Constants["PackVolumeAndUQColumnWidth"] = 11;
			Constants["PackVolumeAndUQColumnIndex"] = 6;
			Constants["PackCountColumnWidth"] = 10;
			Constants["PackCountColumnIndex"] = 7;

			Constants["NumberOfContainerRows"] = 8;
			Constants["ShowContainerHeadingInMainBody"] = "Y";
			Constants["InterleavePacksAndContainers"] = "Y";

			const string expectedHeading =
				//                                                                                                   1    1    1    1    1    1    1    1    1    1    1    1    1    1    1    1    1    1
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....
				"Cn. No      Seal  Type           Net (lb)           Tare (lb)          Gross (lb) Volume (CF)                  Packs      Weight+UQ      Tare+UQ         Gross+UQ          Volume+UQ\n" +
				"";

			const string expectedBody1 =
				//                                                                                                   1    1    1    1    1    1    1    1    1    1    1    1    1    1    1    1    1    1
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....
				"FAKE4100011 Seal  20GP          13306.934             4850.17           18157.104      95.773                 24 PCE   13306.934 LB   4850.17 LB     18157.104 LB          95.773 CF\n" +
				"   Detailed Description 1        3306.934 3306.934 LB UN0440, CHARGES,                 88.287   88.287 CF          4\n" +
				"                                                      SHAPED, class 1.4D                                            \n" +
				"                                                      UN2305,                                                       \n" +
				"                                                      NITROBENZENESULPHONIC                                         \n" +
				"                                                      ACID, class 8, PG II                                          \n" +
				"   Detailed Description 2           10000    10000 LB UN0138, MINES, class                7.5      7.5 CF         20\n" +
				"";

			const string expectedBody2 =
				//                                                                                                   1    1    1    1    1    1    1    1    1    1    1    1    1    1    1    1    1    1
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....
				"                                                      1.2D                                                          \n" +
				"";

			const string expectedFollowOn =
				//                                                                                                   1    1    1    1    1    1    1    1    1    1    1    1    1    1    1    1    1    1
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....
				"FAKE4100027 Seal2 40RE          33069.339            7936.641           41005.981         216                  8 PLT   33069.339 LB  7936.641 LB     41005.981 LB             216 CF\n" +
				"   Detailed Description 3       33069.339   33069.339                                     216      216 CF          8\n" +
				"                                                   LB                                                               \n" +
				"FAKE4100032 Seal3 20RE             248640            5070.632          253710.632         888                  8 PKG      248640 LB  5070.632 LB    253710.632 LB             888 CF\n" +
				"   Detailed Description 4          248640   248640 LB                                     888      888 CF          8\n" +
				"-           -     -             24250.849                   -           24250.849          84                  2 PKG   24250.849 LB            -     24250.849 LB              84 CF\n" +
				"   Detailed Description 5       24250.849   24250.849                                      84       84 CF          2\n" +
				"                                                   LB                                                               \n" +
				"";

			Supporter.DisplayContainers = true;

			AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);
			AssertMultilineASCIIEquals("Heading", expectedHeading, Supporter.FormedPages.ContainersSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedHeading + expectedBody1, Supporter.FormedPages[0].MainBodyContainersSection);
			AssertMultilineASCIIEquals("FollowOn", expectedHeading + expectedBody2 + expectedFollowOn, ZString.Join("\n", Supporter.FollowOnSection));
			AssertEquals("HasFollowOn", true, Supporter.HasFollowOnSection);

			Constants["ShowContainerHeadingInMainBody"] = "N";
			Reset();

			AssertMultilineASCIIEquals("Heading", expectedHeading, Supporter.FormedPages.ContainersSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedBody1 + expectedBody2, Supporter.FormedPages[0].MainBodyContainersSection);
			AssertMultilineASCIIEquals("FollowOn", expectedHeading + expectedFollowOn, ZString.Join("\n", Supporter.FollowOnSection));
			AssertEquals("HasFollowOn", true, Supporter.HasFollowOnSection);
		}

		public void TestWithoutUnitConversion()
		{
			GetNewHeader(Shipment);

			CommonConsol consol = Factory.New<CommonConsol>();
			Shipment.Consols.Add(consol);

			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_SealNum = "Seal";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_TareWeight = 4850.17;
			container.JC_GrossWeightUQ = "LB";

			PackLine packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_DetailedDescription = "Detailed Description 1";
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine.JL_PackageCount = 4;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_ActualWeight = 1500;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualVolume = 2.5;
			packLine.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0440", "", "IMO").First().PK;
			packLine.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "2305", "", "IMO").First().PK;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_DetailedDescription = "Detailed Description 2";
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Keg;
			packLine.JL_PackageCount = 20;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Pounds;
			packLine.JL_ActualWeight = 10000;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicFeet;
			packLine.JL_ActualVolume = 7.5;
			packLine.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0138", "", "IMO").First().PK;

			container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100027";
			container.JC_SealNum = "Seal2";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			container.JC_TareWeight = 3600;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_DetailedDescription = "Detailed Description 3";
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine.JL_PackageCount = 8;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			packLine.JL_ActualWeight = 15;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicYards;
			packLine.JL_ActualVolume = 8;

			container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100032";
			container.JC_SealNum = "Seal3";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			container.JC_TareWeight = 2300m;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_DetailedDescription = "Detailed Description 4";
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			packLine.JL_PackageCount = 8;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.LongTons;
			packLine.JL_ActualWeight = 111;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicFeet;
			packLine.JL_ActualVolume = 888;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.JL_DetailedDescription = "Detailed Description 5";
			packLine.Containers.RemoveAll();
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			packLine.JL_PackageCount = 2;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			packLine.JL_ActualWeight = 11;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicFeet;
			packLine.JL_ActualVolume = 84;

			Constants["ConvertUnits"] = "N";

			Constants["ContainerNumberLeftPadding"] = 0;
			Constants["ContainerNumberWidth"] = 11;
			Constants["ContainerSealLeftPadding"] = 1;
			Constants["ContainerSealWidth"] = 5;
			Constants["ContainerTypeLeftPadding"] = 1;
			Constants["ContainerTypeWidth"] = 8;
			Constants["ContainerWeightLeftPadding"] = 4;
			Constants["ContainerWeightWidth"] = 11;
			Constants["ContainerWeightCaption"] = "Net (KG)";
			Constants["ContainerGrossLeftPadding"] = 1;
			Constants["ContainerGrossWidth"] = 19;
			Constants["ContainerGrossCaption"] = "Gross (KG)";
			Constants["ContainerTareLeftPadding"] = 1;
			Constants["ContainerTareWidth"] = 19;
			Constants["ContainerTareCaption"] = "Tare (KG)";
			Constants["ContainerVolumeLeftPadding"] = 1;
			Constants["ContainerVolumeWidth"] = 11;
			Constants["ContainerVolumeCaption"] = "Volume (M3)";
			Constants["ContainerPackagesLeftPadding"] = 12;
			Constants["ContainerPackagesWidth"] = 11;
			Constants["ContainerWeightAndUQWidth"] = 13;
			Constants["ContainerWeightAndUQIndex"] = 9;
			Constants["ContainerWeightAndUQLeftPadding"] = 2;
			Constants["ContainerWeightAndUQCaption"] = "Weight+UQ";
			Constants["ContainerTareAndUQWidth"] = 12;
			Constants["ContainerTareAndUQIndex"] = 10;
			Constants["ContainerTareAndUQLeftPadding"] = 1;
			Constants["ContainerTareAndUQCaption"] = "Tare+UQ";
			Constants["ContainerGrossAndUQWidth"] = 15;
			Constants["ContainerGrossAndUQIndex"] = 11;
			Constants["ContainerGrossAndUQLeftPadding"] = 2;
			Constants["ContainerGrossAndUQCaption"] = "Gross+UQ";
			Constants["ContainerVolumeAndUQWidth"] = 16;
			Constants["ContainerVolumeAndUQIndex"] = 12;
			Constants["ContainerVolumeAndUQLeftPadding"] = 3;
			Constants["ContainerVolumeAndUQCaption"] = "Volume+UQ";

			Constants["PackRefNumberColumnWidth"] = 0;
			Constants["PackLengthColumnWidth"] = 0;
			Constants["PackWidthColumnWidth"] = 0;
			Constants["PackHeightColumnWidth"] = 0;
			Constants["PackAreaColumnWidth"] = 0;

			Constants["PackDescriptionColumnLeftPadding"] = 3;
			Constants["PackDescriptionColumnWidth"] = 23;
			Constants["PackDescriptionColumnIndex"] = 1;
			Constants["PackWeightColumnIndex"] = 2;
			Constants["PackWeightAndUQColumnWidth"] = 11;
			Constants["PackWeightAndUQColumnIndex"] = 3;
			Constants["PackUNDGColumnWidth"] = 23;
			Constants["PackUNDGColumnIndex"] = 4;
			Constants["PackVolumeColumnIndex"] = 5;
			Constants["PackVolumeColumnLeftPadding"] = 2;
			Constants["PackVolumeAndUQColumnLeftPadding"] = 1;
			Constants["PackVolumeAndUQColumnWidth"] = 11;
			Constants["PackVolumeAndUQColumnIndex"] = 6;
			Constants["PackCountColumnWidth"] = 10;
			Constants["PackCountColumnIndex"] = 7;

			Constants["NumberOfContainerRows"] = 8;
			Constants["ShowContainerHeadingInMainBody"] = "Y";
			Constants["InterleavePacksAndContainers"] = "Y";

			const string expectedHeading =
				//                                                                                                   1    1    1    1    1    1    1    1    1    1    1    1    1    1    1    1    1    1
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....
				"Cn. No      Seal  Type           Net (KG)           Tare (KG)          Gross (KG) Volume (M3)                  Packs      Weight+UQ      Tare+UQ         Gross+UQ          Volume+UQ\n" +
				"";

			const string expectedBody1 =
				//                                                                                                   1    1    1    1    1    1    1    1    1    1    1    1    1    1    1    1    1    1
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....
				"FAKE4100011 Seal  20GP           6035.924                2200            8235.924       2.712                 24 PCE   13306.934 LB   4850.17 LB     18157.104 LB           2.712 M3\n" +
				"   Detailed Description 1            1500     1500 KG UN0440, CHARGES,                    2.5      2.5 M3          4\n" +
				"                                                      SHAPED, class 1.4D                                            \n" +
				"                                                      UN2305,                                                       \n" +
				"                                                      NITROBENZENESULPHONIC                                         \n" +
				"                                                      ACID, class 8, PG II                                          \n" +
				"   Detailed Description 2        4535.924    10000 LB UN0138, MINES, class              0.212      7.5 CF         20\n" +
				"";

			const string expectedBody2 =
				//                                                                                                   1    1    1    1    1    1    1    1    1    1    1    1    1    1    1    1    1    1
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....
				"                                                      1.2D                                                          \n" +
				"";

			const string expectedFollowOn =
				//                                                                                                   1    1    1    1    1    1    1    1    1    1    1    1    1    1    1    1    1    1
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....
				"FAKE4100027 Seal2 40RE              15000                3600               18600       6.116                  8 PLT       15000 KG      3600 KG         18600 KG               8 CY\n" +
				"   Detailed Description 3           15000        15 T                                   6.116        8 CY          8\n" +
				"FAKE4100032 Seal3 20RE         112781.207                2300          115081.207      25.145                  8 PKG  112781.207 KG      2300 KG    115081.207 KG             888 CF\n" +
				"   Detailed Description 4      112781.207      111 TL                                  25.145      888 CF          8\n" +
				"-           -     -                 11000                   -               11000       2.379                  2 PKG           11 T            -             11 T              84 CF\n" +
				"   Detailed Description 5           11000        11 T                                   2.379       84 CF          2\n" +
				"";

			Supporter.DisplayContainers = true;

			AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);
			AssertMultilineASCIIEquals("Heading", expectedHeading, Supporter.FormedPages.ContainersSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedHeading + expectedBody1, Supporter.FormedPages[0].MainBodyContainersSection);
			AssertMultilineASCIIEquals("FollowOn", expectedHeading + expectedBody2 + expectedFollowOn, ZString.Join("\n", Supporter.FollowOnSection));
			AssertEquals("HasFollowOn", true, Supporter.HasFollowOnSection);

			Constants["ShowContainerHeadingInMainBody"] = "N";
			Reset();

			AssertMultilineASCIIEquals("Heading", expectedHeading, Supporter.FormedPages.ContainersSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedBody1 + expectedBody2, Supporter.FormedPages[0].MainBodyContainersSection);
			AssertMultilineASCIIEquals("FollowOn", expectedHeading + expectedFollowOn, ZString.Join("\n", Supporter.FollowOnSection));
			AssertEquals("HasFollowOn", true, Supporter.HasFollowOnSection);
		}

		public void TestExtraSection()
		{
			GetNewHeader(Shipment);

			Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			Shipment.JS_RL_NKOrigin = "NLAMS";
			Shipment.JS_RL_NKDestination = "AUBNE";

			Constants["NoOfExtraSectionRows"] = 3;
			Constants["ExtraSection-Count"] = 3;
			Constants["ShowExtraSectionHeadingInMainBody"] = "Y";

			Constants["ExtraSection-Heading1"] = "Heading 1";
			Constants["ExtraSection-Path1"] = "Plain Text";
			Constants["ExtraSection-LeftPadding1"] = 0;
			Constants["ExtraSection-Width1"] = 10;

			Constants["ExtraSection-Heading2"] = "Heading Two";
			Constants["ExtraSection-Path2"] = "<OriginLoco.PortName> -> <DestinationLoco.PortName>";
			Constants["ExtraSection-LeftPadding2"] = 1;
			Constants["ExtraSection-Width2"] = 20;

			Constants["ExtraSection-Heading3"] = "Third Heading";
			Constants["ExtraSection-Path3"] = "Line1\r\nLine2\r\nLine3";
			Constants["ExtraSection-LeftPadding3"] = 1;
			Constants["ExtraSection-Width3"] = 15;

			const string expectedHeading1 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"Heading 1  Heading Two          Third Heading  \n" +
				"";

			const string expectedBody1 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"Plain Text Amsterdam ->         Line1          \n" +
				"           Brisbane             Line2          \n" +
				"";

			const string expectedFollowOn =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"                                Line3          \n" +
				"";

			AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);

			AssertMultilineASCIIEquals("Heading", expectedHeading1, Supporter.FormedPages.ExtraSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedHeading1 + expectedBody1, Supporter.FormedPages[0].MainBodyExtraSection);
			AssertMultilineASCIIEquals("FollowOn", expectedHeading1 + expectedFollowOn, ZString.Join("\n", Supporter.FollowOnSection));
			AssertEquals("HasFollowOn", true, Supporter.HasFollowOnSection);

			Constants["ShowExtraSectionHeadingInMainBody"] = "N";
			Reset();

			AssertMultilineASCIIEquals("Heading", expectedHeading1, Supporter.FormedPages.ExtraSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedBody1 + expectedFollowOn, Supporter.FormedPages[0].MainBodyExtraSection);
			AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", Supporter.FollowOnSection));
			AssertEquals("HasFollowOn", false, Supporter.HasFollowOnSection);

			Constants["ExtraSection-Path2"] = ZString.Empty;
			Constants["ExtraSection-HideHeadingWhenNoData2"] = ZBool.True;
			Reset();

			const string expectedHeading2 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"Heading 1                       Third Heading  \n" +
				"";

			const string expectedBody2 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"Plain Text                      Line1          \n" +
				"                                Line2          \n" +
				"";

			AssertMultilineASCIIEquals("Heading", expectedHeading2, Supporter.FormedPages.ExtraSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedBody2 + expectedFollowOn, Supporter.FormedPages[0].MainBodyExtraSection);
			AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", Supporter.FollowOnSection));
			AssertEquals("HasFollowOn", false, Supporter.HasFollowOnSection);
		}

		public void TestIncludeExtraSectionInDetailSection()
		{
			GetNewHeader(Shipment);

			Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			Shipment.JS_RL_NKOrigin = "NLAMS";
			Shipment.JS_RL_NKDestination = "AUBNE";

			Shipment.JS_MarksAndNumbers =
				"Marks and Numbers Line 1\n" +
				"Marks and Numbers Line 2\n" +
				"Marks and Numbers Line 3\n" +
				"";

			Shipment.DetailedGoodsDescriptionNoteText =
				"Goods Description Line 1\n" +
				"Goods Description Line 2\n" +
				"Goods Description Line 3\n" +
				"";

			Shipment.JS_ActualWeight = shipment.OuterPackLines.TotalWeight;
			Shipment.JS_ActualVolume = shipment.OuterPackLines.TotalVolume;

			Constants["MarksAndNumbersAndGoodsDescriptionHeight"] = 4;
			Constants["MarksAndNumbersWidth"] = 20;
			Constants["MarksAndNumbersAndGoodsDescriptionGap"] = 1;
			Constants["GoodsDescriptionWidth"] = 20;
			Constants["GoodsDescriptionAndGrossWeightGap"] = 1;
			Constants["GrossWeightWidth"] = 12;
			Constants["GrossWeightAndMeasurementGap"] = 1;
			Constants["VolumeMeasurementWidth"] = 12;
			Constants["ShowDetailHeadingInMainBody"] = "Y";
			Constants["IncludeExtraSectionInMarksAndNumbersSection"] = "Y";
			Constants["ShowExtraSectionHeadingInMainBody"] = "Y";

			Constants["NoOfExtraSectionRows"] = 6;
			Constants["ExtraSection-Count"] = 3;

			Constants["ExtraSection-Heading1"] = "Heading 1";
			Constants["ExtraSection-Path1"] = "Plain Text";
			Constants["ExtraSection-LeftPadding1"] = 0;
			Constants["ExtraSection-Width1"] = 10;

			Constants["ExtraSection-Heading2"] = "Heading Two";
			Constants["ExtraSection-Path2"] = "<OriginLoco.PortName> -> <DestinationLoco.PortName>";
			Constants["ExtraSection-LeftPadding2"] = 1;
			Constants["ExtraSection-Width2"] = 20;

			Constants["ExtraSection-Heading3"] = "Third Heading";
			Constants["ExtraSection-Path3"] = "Line1\r\nLine2\r\nLine3";
			Constants["ExtraSection-LeftPadding3"] = 1;
			Constants["ExtraSection-Width3"] = 15;

			const string expectedDetailHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"Marks & Numbers      Goods Description        Gross Wt       Volume\n" +
				"";

			const string expectedDetailBody1 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"Marks and Numbers    Goods Description            0 KG         0 M3\n" +
				"Line 1               Line 1                                        \n" +
				"Marks and Numbers    Goods Description                             \n" +
				"";

			const string expectedDetailBody2 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"Line 2               Line 2                                        \n" +
				"Marks and Numbers    Goods Description                             \n" +
				"Line 3               Line 3                                        \n" +
				"";

			const string expectedExtraSectionHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"Heading 1  Heading Two          Third Heading  \n" +
				"";

			const string expectedExtraSectionBody1 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"Plain Text Amsterdam ->         Line1          \n" +
				"           Brisbane             Line2          \n" +
				"";

			const string expectedExtraSectionBody2 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0
				"                                Line3          \n" +
				"";

			AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);

			AssertMultilineASCIIEquals("Detail Heading", expectedDetailHeading, Supporter.FormedPages.DetailsSectionHeader);
			AssertMultilineASCIIEquals("Extra Section Heading", expectedExtraSectionHeading, Supporter.FormedPages.ExtraSectionHeader);

			AssertMultilineASCIIEquals("Detail Body",
				expectedDetailHeading +
				expectedDetailBody1,
				Supporter.FormedPages[0].MainBodyDetailsSection);

			AssertMultilineASCIIEquals("Extra Section Body", "", Supporter.FormedPages[0].MainBodyExtraSection);

			AssertMultilineASCIIEquals("FollowOn",
				expectedDetailHeading +
				expectedDetailBody2 +
				"\n" +
				expectedExtraSectionHeading +
				expectedExtraSectionBody1 +
				expectedExtraSectionBody2,
				ZString.Join("\n", Supporter.FollowOnSection));

			Constants["MarksAndNumbersAndGoodsDescriptionHeight"] = 11;
			Reset();

			AssertMultilineASCIIEquals("Detail Heading", expectedDetailHeading, Supporter.FormedPages.DetailsSectionHeader);
			AssertMultilineASCIIEquals("Extra Section Heading", expectedExtraSectionHeading, Supporter.FormedPages.ExtraSectionHeader);

			AssertMultilineASCIIEquals("Detail Body",
				expectedDetailHeading +
				expectedDetailBody1 +
				expectedDetailBody2 +
				"\n" +
				expectedExtraSectionHeading +
				expectedExtraSectionBody1,
				Supporter.FormedPages[0].MainBodyDetailsSection);

			AssertMultilineASCIIEquals("Container Body", "", Supporter.FormedPages[0].MainBodyExtraSection);

			AssertMultilineASCIIEquals("FollowOn",
				expectedExtraSectionHeading +
				expectedExtraSectionBody2,
				ZString.Join("\n", Supporter.FollowOnSection));
		}

		public void TestChargesSection_MultiPage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
				Shipment.JS_HBLAWBChargesDisplay = "ALL";

				var header = GetNewHeader(Shipment);

				var exRates = (BusinessObjectCollection)header["ExchangeRates"];
				var usdRate = exRates.AddNew();
				usdRate["JF_RX_NKRateCurrency"] = "USD";
				usdRate["JF_BaseRate"] = 1.2;

				var auRate = exRates.AddNew();
				auRate["JF_RX_NKRateCurrency"] = "AUD";
				auRate["JF_BaseRate"] = 1.1;

				CreateLineCharge(header, header.AgentCollectPK, 10, "FRT", "AUD", "SHORTFRTDESC");
				CreateLineCharge(header, header.AgentCollectPK, 30, "OLAB", "AUD", "SHORTOLABDESC");
				CreateLineCharge(header, header.LocalChargesPK, 50, 50, "DLAB", "EUR", "SHORTDLABDESC");

				Constants["UseMultiPage"] = "Y";
				Constants["NumberOfCollectChargesRows"] = 3;
				Constants["ChargeCodeColumnLeftPadding"] = 0;
				Constants["ChargeCodeColumnWidth"] = 6;
				Constants["ChargeCodeIndex"] = 1;
				Constants["ChargeDescriptionLeftPadding"] = 1;
				Constants["ChargeDescriptionColumnWidth"] = 25;
				Constants["ChargeDescriptionCaption"] = "Charge Desc";
				Constants["ChargeDescriptionIndex"] = 2;
				Constants["CollectChargesColumnLeftPadding"] = 1;
				Constants["CollectChargesColumnWidth"] = 8;
				Constants["CollectChargesColumnCaption"] = "CCT";
				Constants["CollectChargesColumnIndex"] = 3;
				Constants["CollectCurrencyColumnLeftPadding"] = 1;
				Constants["CollectCurrencyColumnWidth"] = 3;
				Constants["CollectCurrencyColumnCaption"] = "CX";
				Constants["CollectCurrencyColumnIndex"] = 4;
				Constants["PrepaidChargesColumnLeftPadding"] = 1;
				Constants["PrepaidChargesColumnWidth"] = 8;
				Constants["PrepaidChargesColumnCaption"] = "PPD";
				Constants["PrepaidChargesColumnIndex"] = 5;
				Constants["PrepaidCurrencyColumnLeftPadding"] = 1;
				Constants["PrepaidCurrencyColumnWidth"] = 3;
				Constants["PrepaidCurrencyColumnCaption"] = "PX";
				Constants["PrepaidCurrencyColumnIndex"] = 6;

				Constants["ChargeCodeDescLeftPadding"] = 1;
				Constants["ChargeCodeDescColumnWidth"] = 25;
				Constants["ChargeCodeDescCaption"] = "ChargeCodeDesc";
				Constants["ChargeCodeDescIndex"] = 7;

				Constants["ShowChargesHeadingInMainBody"] = "Y";

				const string expectedHeading =
					//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
					//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
					"Code   Charge Desc                    CCT CX       PPD PX  ChargeCodeDesc           \n" +
					"";

				string expectedBody1 =
					//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
					//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
					"FRT    International Freight        10,00 AUD              SHORTFRTDESC             \n" +
					"OLAB   Origin Labour Charges        30,00 AUD              SHORTOLABDESC            \n" +
					"";

				string expectedBody2 =
					//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
					//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
					"DLAB   Destination Labour                        50,00 EUR SHORTDLABDESC            \n" +
					"       Charges                                                                      \n" +
					"";

				AssertEquals("should have 2 formed pages", 2, Supporter.FormedPages.Count);
				AssertMultilineASCIIEquals("Body1", expectedHeading + expectedBody1, Supporter.FormedPages[0].ChargesSection);
				AssertMultilineASCIIEquals("Body2", expectedHeading + expectedBody2, Supporter.FormedPages[1].ChargesSection);
				AssertMultilineASCIIEquals("FollowOn", ZString.Empty, ZString.Join("\n", Supporter.FollowOnSection));
				AssertEquals("HasFollowOn", false, Supporter.HasFollowOnSection);

				AssertEquals("Prepaid total", "50,00 EUR", Supporter.FormedPages.PrepaidChargesTotal.AmountAndCurrencyCode);
				AssertEquals("Collect total", "36,36 EUR", Supporter.FormedPages.CollectChargesTotal.AmountAndCurrencyCode); //(30 + 10) /1.1 = 36.36;
				AssertEquals("All Charges total", "86,36 EUR", Supporter.FormedPages.ChargesTotal.AmountAndCurrencyCode); //50+36.36 = 86.86

				Reset();
				Supporter.ShouldPrintChargesAsLumpSum = false;
				Supporter.ShouldPrintTotalCharges = true;

				expectedBody1 =
					//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
					//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
					"FRT    International Freight        10,91 USD              SHORTFRTDESC             \n" +
					"OLAB   Origin Labour Charges        32,73 USD              SHORTOLABDESC            \n" +
					"";

				expectedBody2 =
					//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
					//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
					"DLAB   Destination Labour                        60,00 USD SHORTDLABDESC            \n" +
					"       Charges                                                                      \n" +
					"";

				AssertMultilineASCIIEquals("Body1", expectedHeading + expectedBody1, Supporter.FormedPages[0].ChargesSection);
				AssertMultilineASCIIEquals("Body2", expectedHeading + expectedBody2, Supporter.FormedPages[1].ChargesSection);
				AssertMultilineASCIIEquals("FollowOn", ZString.Empty, ZString.Join("\n", Supporter.FollowOnSection));
			}
		}

		public void TestChargesSection_CollectCharges()
		{
			Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			Shipment.JS_HBLAWBChargesDisplay = "SHW";

			JobHeader header = GetNewHeader(Shipment);

			CreateLineCharge(header, header.AgentCollectPK, 1000, 1000, "FRT", "ERN", "Modified Desc");
			CreateLineCharge(header, header.LocalChargesPK, 500, 700, "OLAB", "AUD");
			CreateLineCharge(header, header.AgentCollectPK, 750, 750, "DLAB", "ERN");

			AssertCollectCharges_DisplayCorrectly();
		}

		public void TestChargesSection_CollectCharges_DoesNotIncludeProfitShare()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "PPP";
			chargeCode.AC_Desc = "Profit Share Charges from Registry";

			AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());

			Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			Shipment.JS_HBLAWBChargesDisplay = "SHW";

			var header = GetNewHeader(Shipment);

			CreateLineCharge(header, header.AgentCollectPK, 1000, 1000, "FRT", "ERN", "Modified Desc");
			CreateLineCharge(header, header.LocalChargesPK, 500, 700, "OLAB", "AUD");
			CreateLineCharge(header, header.AgentCollectPK, 750, 750, "DLAB", "ERN");
			CreateLineCharge(header, header.AgentCollectPK, 500, 500, chargeCode.AC_Code, "AUD");

			AssertCollectCharges_DisplayCorrectly();
		}

		void AssertCollectCharges_DisplayCorrectly()
		{
			Constants["NumberOfCollectChargesRows"] = 3;
			Constants["ChargeDescriptionLeftPadding"] = 0;
			Constants["ChargeDescriptionColumnWidth"] = 25;
			Constants["CollectChargesColumnLeftPadding"] = 1;
			Constants["CollectChargesColumnWidth"] = 8;
			Constants["CollectCurrencyColumnLeftPadding"] = 1;
			Constants["CollectCurrencyColumnWidth"] = 3;
			Constants["PrepaidChargesColumnLeftPadding"] = 1;
			Constants["PrepaidChargesColumnWidth"] = 8;
			Constants["PrepaidCurrencyColumnLeftPadding"] = 1;
			Constants["PrepaidCurrencyColumnWidth"] = 3;
			Constants["ShowChargesHeadingInMainBody"] = "Y";

			const string expectedHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"Charge Description         Collect                 \n" +
				"";

			const string expectedBody =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"International Freight     1,000.00 ERN             \n" +
				"Destination Labour          750.00 ERN             \n" +
				"";

			const string expectedFollowOn =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"Charges                                            \n" +
				"";

			string expectedTotalChargesBody =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"--------------------      --------                 \n" +
				"Total Charges             1,750.00 ERN             \n" +
				"";

			AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);

			AssertMultilineASCIIEquals("Heading", expectedHeading, Supporter.FormedPages.ChargesSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedHeading + expectedBody, Supporter.FormedPages[0].ChargesSection);
			AssertMultilineASCIIEquals("FollowOn", expectedHeading + expectedFollowOn, ZString.Join("\n", Supporter.FollowOnSection));
			AssertEquals("HasFollowOn", true, Supporter.HasFollowOnSection);

			Constants["NumberOfCollectChargesRows"] = 7;
			Constants["ShowChargesHeadingInMainBody"] = "N";
			Reset();

			AssertMultilineASCIIEquals("Heading", expectedHeading, Supporter.FormedPages.ChargesSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedBody + expectedFollowOn, Supporter.FormedPages[0].ChargesSection);
			AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", Supporter.FollowOnSection));
			AssertEquals("HasFollowOn", false, Supporter.HasFollowOnSection);

			AssertEquals("Prepaid total should be empty", ZString.Empty, Supporter.FormedPages.PrepaidChargesTotal.AmountAndCurrencyCode);
			AssertEquals("Collect total", "1,750.00 ERN", Supporter.FormedPages.CollectChargesTotal.AmountAndCurrencyCode);
			AssertEquals("All Charges total", "1,750.00 ERN", Supporter.FormedPages.ChargesTotal.AmountAndCurrencyCode);

			Reset();

			Supporter.ShouldPrintChargesAsLumpSum = false;
			Supporter.ShouldPrintTotalCharges = true;

			AssertMultilineASCIIEquals("Heading", expectedHeading, Supporter.FormedPages.ChargesSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedBody + expectedFollowOn + expectedTotalChargesBody, Supporter.FormedPages[0].ChargesSection);
			AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", Supporter.FollowOnSection));
		}

		public void TestChargesSection_PrepaidCharges()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
				Shipment.JS_HBLAWBChargesDisplay = "PPD";

				JobHeader header = GetNewHeader(Shipment);

				CreateLineCharge(header, header.LocalChargesPK, 1000, 12500, "FRT", "AUD");
				CreateLineCharge(header, header.AgentCollectPK, 500, 600, "OLAB", "AUD");
				CreateLineCharge(header, header.LocalChargesPK, 800, 1000, "DLAB", "AUD", "Modified desc for DLAB charges");

				AssertPrepaidCharges_DisplayCorrectly();
			}
		}

		public void TestChargesSection_PrepaidCharges_DoesNotIncludeProfitShare()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "PPP";
			chargeCode.AC_Desc = "Profit Share Charges from Registry";

			AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
				Shipment.JS_HBLAWBChargesDisplay = "PPD";

				JobHeader header = GetNewHeader(Shipment);

				CreateLineCharge(header, header.LocalChargesPK, 1000, 12500, "FRT", "AUD");
				CreateLineCharge(header, header.AgentCollectPK, 500, 600, "OLAB", "AUD");
				CreateLineCharge(header, header.LocalChargesPK, 800, 1000, "DLAB", "AUD", "Modified desc for DLAB charges");
				CreateLineCharge(header, header.AgentCollectPK, 500, 500, chargeCode.AC_Code, "AUD");

				AssertPrepaidCharges_DisplayCorrectly();
			}
		}

		public void AssertPrepaidCharges_DisplayCorrectly()
		{
			Constants["NumberOfCollectChargesRows"] = 3;
			Constants["ChargeDescriptionLeftPadding"] = 0;
			Constants["ChargeDescriptionColumnWidth"] = 25;
			Constants["CollectChargesColumnLeftPadding"] = 1;
			Constants["CollectChargesColumnWidth"] = 8;
			Constants["CollectCurrencyColumnLeftPadding"] = 1;
			Constants["CollectCurrencyColumnWidth"] = 3;
			Constants["PrepaidChargesColumnLeftPadding"] = 1;
			Constants["PrepaidChargesColumnWidth"] = 8;
			Constants["PrepaidCurrencyColumnLeftPadding"] = 1;
			Constants["PrepaidCurrencyColumnWidth"] = 3;
			Constants["ShowChargesHeadingInMainBody"] = "Y";

			const string expectedHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"Charge Description                      Prepaid    \n" +
				"";

			const string expectedBody =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"International Freight                  1,000.00 AUD\n" +
				"Destination Labour                       800.00 AUD\n" +
				"";

			const string expectedFollowOn =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"Charges                                            \n" +
				"";

			const string expectedTotalChargesBody =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"--------------------                   --------    \n" +
				"Total Charges                          1,800.00 AUD\n" +
				"";

			AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);

			AssertMultilineASCIIEquals("Heading", expectedHeading, Supporter.FormedPages.ChargesSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedHeading + expectedBody, Supporter.FormedPages[0].ChargesSection);
			AssertMultilineASCIIEquals("FollowOn", expectedHeading + expectedFollowOn, ZString.Join("\n", Supporter.FollowOnSection));
			AssertEquals("HasFollowOn", true, Supporter.HasFollowOnSection);

			Constants["NumberOfCollectChargesRows"] = 5;
			Constants["ShowChargesHeadingInMainBody"] = "N";
			Reset();

			AssertMultilineASCIIEquals("Heading", expectedHeading, Supporter.FormedPages.ChargesSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedBody + expectedFollowOn, Supporter.FormedPages[0].ChargesSection);
			AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", Supporter.FollowOnSection));
			AssertEquals("HasFollowOn", false, Supporter.HasFollowOnSection);

			AssertEquals("Prepaid total", "2,250.00 USD", Supporter.FormedPages.PrepaidChargesTotal.AmountAndCurrencyCode);
			AssertEquals("Collect total should be empty", ZString.Empty, Supporter.FormedPages.CollectChargesTotal.AmountAndCurrencyCode);
			AssertEquals("All Charges total", "2,250.00 USD", Supporter.FormedPages.ChargesTotal.AmountAndCurrencyCode);

			Reset();

			Supporter.ShouldPrintChargesAsLumpSum = false;
			Supporter.ShouldPrintTotalCharges = true;

			AssertMultilineASCIIEquals("Heading", expectedHeading, Supporter.FormedPages.ChargesSectionHeader);
			AssertMultilineASCIIEquals("Body", expectedBody + expectedFollowOn + expectedTotalChargesBody, Supporter.FormedPages[0].ChargesSection);
			AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", Supporter.FollowOnSection));
		}

		public void TestChargesSection_CollectCharges_PrintTotalChargesInMultiCurrency()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
				Shipment.JS_HBLAWBChargesDisplay = "SHW";

				var header = GetNewHeader(Shipment);

				var exRates = (BusinessObjectCollection)header["ExchangeRates"];
				var usdRate = exRates.AddNew();
				usdRate["JF_RX_NKRateCurrency"] = "USD";
				usdRate["JF_BaseRate"] = 1.2;

				CreateLineCharge(header, header.AgentCollectPK, 10000, "FRT", "AUD");
				CreateLineCharge(header, header.LocalChargesPK, 500, "OLAB", "AUD");
				CreateLineCharge(header, header.AgentCollectPK, 800, "DLAB", "USD", "Modified desc for DLAB charges");

				Constants["NumberOfCollectChargesRows"] = 7;
				Constants["ChargeDescriptionLeftPadding"] = 0;
				Constants["ChargeDescriptionColumnWidth"] = 25;
				Constants["CollectChargesColumnLeftPadding"] = 1;
				Constants["CollectChargesColumnWidth"] = 8;
				Constants["CollectCurrencyColumnLeftPadding"] = 1;
				Constants["CollectCurrencyColumnWidth"] = 3;
				Constants["PrepaidChargesColumnLeftPadding"] = 1;
				Constants["PrepaidChargesColumnWidth"] = 8;
				Constants["PrepaidCurrencyColumnLeftPadding"] = 1;
				Constants["PrepaidCurrencyColumnWidth"] = 3;
				Constants["ShowChargesHeadingInMainBody"] = "N";

				const string expectedHeading =
					//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
					//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
					"Charge Description         Collect                 \n" +
					"";

				const string expectedBody =
					//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
					//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
					"International Freight     12,000.0 USD             \n" +
					"                                 0                 \n" +
					"Destination Labour          800.00 USD             \n" +
					"";

				const string expectedFollowOn =
					//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
					//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
					"Charges                                            \n" +
					"";

				const string expectedTotalChargesBody =
					//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
					//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
					"--------------------      --------                 \n" +
					"Total Charges             12,800.0 USD             \n" +
					"                                 0                 \n" +
					"";

				Supporter.ShouldPrintChargesAsLumpSum = false;
				Supporter.ShouldPrintTotalCharges = true;

				AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);
				AssertMultilineASCIIEquals("Heading", expectedHeading, Supporter.FormedPages.ChargesSectionHeader);
				AssertMultilineASCIIEquals("Body", expectedBody + expectedFollowOn + expectedTotalChargesBody, Supporter.FormedPages[0].ChargesSection);
				AssertMultilineASCIIEquals("FollowOn", ZString.Empty, ZString.Join("\n", Supporter.FollowOnSection));
				AssertEquals("HasFollowOn", false, Supporter.HasFollowOnSection);
			}
		}

		public void TestChargesSection_PrepaidCharges_PrintTotalChargesInMultiCurrency()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
				Shipment.JS_HBLAWBChargesDisplay = "PPD";

				var header = GetNewHeader(Shipment);

				var exRates = (BusinessObjectCollection)header["ExchangeRates"];
				var usdRate = exRates.AddNew();
				usdRate["JF_RX_NKRateCurrency"] = "USD";
				usdRate["JF_BaseRate"] = 1.2;

				CreateLineCharge(header, header.LocalChargesPK, 10000, "FRT", "AUD");
				CreateLineCharge(header, header.AgentCollectPK, 500, "OLAB", "AUD");
				CreateLineCharge(header, header.LocalChargesPK, 750, "DLAB", "USD", "Modified desc for DLAB charges");

				Constants["NumberOfCollectChargesRows"] = 7;
				Constants["ChargeDescriptionLeftPadding"] = 0;
				Constants["ChargeDescriptionColumnWidth"] = 25;
				Constants["CollectChargesColumnLeftPadding"] = 1;
				Constants["CollectChargesColumnWidth"] = 8;
				Constants["CollectCurrencyColumnLeftPadding"] = 1;
				Constants["CollectCurrencyColumnWidth"] = 3;
				Constants["PrepaidChargesColumnLeftPadding"] = 1;
				Constants["PrepaidChargesColumnWidth"] = 8;
				Constants["PrepaidCurrencyColumnLeftPadding"] = 1;
				Constants["PrepaidCurrencyColumnWidth"] = 3;

				const string expectedHeading =
					//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
					//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
					"Charge Description                      Prepaid    \n" +
					"";

				const string expectedBody =
					//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
					//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
					"International Freight                  12,000.0 USD\n" +
					"                                              0    \n" +
					"Destination Labour                       750.00 USD\n" +
					"";

				const string expectedFollowOn =
					//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
					//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
					"Charges                                            \n" +
					"";

				const string expectedTotalChargesBody =
					//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
					//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
					"--------------------                   --------    \n" +
					"Total Charges                          12,750.0 USD\n" +
					"                                              0    \n" +
					"";

				Supporter.ShouldPrintChargesAsLumpSum = false;
				Supporter.ShouldPrintTotalCharges = true;

				AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);

				AssertMultilineASCIIEquals("Heading", expectedHeading, Supporter.FormedPages.ChargesSectionHeader);
				AssertMultilineASCIIEquals("Body", expectedBody + expectedFollowOn + expectedTotalChargesBody, Supporter.FormedPages[0].ChargesSection);
				AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", Supporter.FollowOnSection));
				AssertEquals("HasFollowOn", false, Supporter.HasFollowOnSection);
			}
		}

		public void TestChargesSection_CollectCharges_PrintTotalChargesInMultiCurrency_ForGermany()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
				Shipment.JS_HBLAWBChargesDisplay = "SHW";

				var header = GetNewHeader(Shipment);

				var exRates = (BusinessObjectCollection)header["ExchangeRates"];
				var usdRate = exRates.AddNew();
				usdRate["JF_RX_NKRateCurrency"] = "USD";
				usdRate["JF_BaseRate"] = 1.2;

				CreateLineCharge(header, header.AgentCollectPK, 10000, "FRT", "AUD");
				CreateLineCharge(header, header.LocalChargesPK, 500, "OLAB", "AUD");
				CreateLineCharge(header, header.AgentCollectPK, 800, "DLAB", "USD", "Modified desc for DLAB charges");

				Constants["NumberOfCollectChargesRows"] = 7;
				Constants["ChargeDescriptionLeftPadding"] = 0;
				Constants["ChargeDescriptionColumnWidth"] = 25;
				Constants["CollectChargesColumnLeftPadding"] = 1;
				Constants["CollectChargesColumnWidth"] = 8;
				Constants["CollectCurrencyColumnLeftPadding"] = 1;
				Constants["CollectCurrencyColumnWidth"] = 3;
				Constants["PrepaidChargesColumnLeftPadding"] = 1;
				Constants["PrepaidChargesColumnWidth"] = 8;
				Constants["PrepaidCurrencyColumnLeftPadding"] = 1;
				Constants["PrepaidCurrencyColumnWidth"] = 3;
				Constants["ShowChargesHeadingInMainBody"] = "N";

				const string expectedHeading =
					//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
					//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
					"Charge Description         Collect                 \n" +
					"";

				const string expectedBody =
					//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
					//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
					"International Freight     12.000,0 USD             \n" +
					"                                 0                 \n" +
					"Destination Labour          800,00 USD             \n" +
					"";

				const string expectedFollowOn =
					//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
					//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
					"Charges                                            \n" +
					"";

				const string expectedTotalChargesBody =
					//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
					//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
					"--------------------      --------                 \n" +
					"Total Charges             12.800,0 USD             \n" +
					"                                 0                 \n" +
					"";

				Supporter.ShouldPrintChargesAsLumpSum = false;
				Supporter.ShouldPrintTotalCharges = true;

				AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);
				AssertMultilineASCIIEquals("Heading", expectedHeading, Supporter.FormedPages.ChargesSectionHeader);
				AssertMultilineASCIIEquals("Body", expectedBody + expectedFollowOn + expectedTotalChargesBody, Supporter.FormedPages[0].ChargesSection);
				AssertMultilineASCIIEquals("FollowOn", ZString.Empty, ZString.Join("\n", Supporter.FollowOnSection));
				AssertEquals("HasFollowOn", false, Supporter.HasFollowOnSection);
			}
		}

		public void TestChargesSection_PrepaidCharges_PrintTotalChargesInMultiCurrency_ForGermany()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
				Shipment.JS_HBLAWBChargesDisplay = "PPD";

				var header = GetNewHeader(Shipment);

				var exRates = (BusinessObjectCollection)header["ExchangeRates"];
				var usdRate = exRates.AddNew();
				usdRate["JF_RX_NKRateCurrency"] = "USD";
				usdRate["JF_BaseRate"] = 1.2;

				CreateLineCharge(header, header.LocalChargesPK, 10000, "FRT", "AUD");
				CreateLineCharge(header, header.AgentCollectPK, 500, "OLAB", "AUD");
				CreateLineCharge(header, header.LocalChargesPK, 750, "DLAB", "USD", "Modified desc for DLAB charges");

				Constants["NumberOfCollectChargesRows"] = 7;
				Constants["ChargeDescriptionLeftPadding"] = 0;
				Constants["ChargeDescriptionColumnWidth"] = 25;
				Constants["CollectChargesColumnLeftPadding"] = 1;
				Constants["CollectChargesColumnWidth"] = 8;
				Constants["CollectCurrencyColumnLeftPadding"] = 1;
				Constants["CollectCurrencyColumnWidth"] = 3;
				Constants["PrepaidChargesColumnLeftPadding"] = 1;
				Constants["PrepaidChargesColumnWidth"] = 8;
				Constants["PrepaidCurrencyColumnLeftPadding"] = 1;
				Constants["PrepaidCurrencyColumnWidth"] = 3;

				const string expectedHeading =
					//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
					//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
					"Charge Description                      Prepaid    \n" +
					"";

				const string expectedBody =
					//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
					//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
					"International Freight                  12.000,0 USD\n" +
					"                                              0    \n" +
					"Destination Labour                       750,00 USD\n" +
					"";

				const string expectedFollowOn =
					//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
					//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
					"Charges                                            \n" +
					"";

				const string expectedTotalChargesBody =
					//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
					//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
					"--------------------                   --------    \n" +
					"Total Charges                          12.750,0 USD\n" +
					"                                              0    \n" +
					"";

				Supporter.ShouldPrintChargesAsLumpSum = false;
				Supporter.ShouldPrintTotalCharges = true;

				AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);

				AssertMultilineASCIIEquals("Heading", expectedHeading, Supporter.FormedPages.ChargesSectionHeader);
				AssertMultilineASCIIEquals("Body", expectedBody + expectedFollowOn + expectedTotalChargesBody, Supporter.FormedPages[0].ChargesSection);
				AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", Supporter.FollowOnSection));
				AssertEquals("HasFollowOn", false, Supporter.HasFollowOnSection);
			}
		}

		public void TestChargesSection_AsAgreed()
		{
			Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			Shipment.JS_HBLAWBChargesDisplay = "AGR";

			JobHeader header = GetNewHeader(Shipment);

			CreateLineCharge(header, header.LocalChargesPK, 10000, "FRT", "AUD");
			CreateLineCharge(header, header.AgentCollectPK, 500, "OLAB", "AUD");
			CreateLineCharge(header, header.LocalChargesPK, 750, "DLAB", "EUR");

			Constants["NumberOfCollectChargesRows"] = 3;
			Constants["ChargeDescriptionLeftPadding"] = 0;
			Constants["ChargeDescriptionColumnWidth"] = 25;
			Constants["CollectChargesColumnLeftPadding"] = 1;
			Constants["CollectChargesColumnWidth"] = 12;
			Constants["PrepaidChargesColumnLeftPadding"] = 1;
			Constants["PrepaidChargesColumnWidth"] = 12;
			Constants["ShowChargesHeadingInMainBody"] = "Y";

			AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);

			AssertMultilineASCIIEquals("Heading", "", Supporter.FormedPages.ChargesSectionHeader.Trim());
			AssertMultilineASCIIEquals("Body", "As Agreed", Supporter.FormedPages[0].ChargesSection);
			AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", Supporter.FollowOnSection));
			AssertEquals("HasFollowOn", false, Supporter.HasFollowOnSection);

			AssertEquals("Prepaid total should be empty", ZString.Empty, Supporter.FormedPages.PrepaidChargesTotal.AmountAndCurrencyCode);
			AssertEquals("Collect total should be empty", ZString.Empty, Supporter.FormedPages.CollectChargesTotal.AmountAndCurrencyCode);
			AssertEquals("All Charges total should be empty", ZString.Empty, Supporter.FormedPages.ChargesTotal.AmountAndCurrencyCode);
		}

		public void TestChargesSection_NoCharges()
		{
			Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			Shipment.JS_HBLAWBChargesDisplay = "NON";

			JobHeader header = GetNewHeader(Shipment);

			CreateLineCharge(header, header.LocalChargesPK, 10000, "FRT", "AUD");
			CreateLineCharge(header, header.AgentCollectPK, 500, "OLAB", "AUD");
			CreateLineCharge(header, header.LocalChargesPK, 750, "DLAB", "EUR");

			Constants["NumberOfCollectChargesRows"] = 3;
			Constants["ChargeDescriptionLeftPadding"] = 0;
			Constants["ChargeDescriptionWidth"] = 25;
			Constants["CollectChargesColumnLeftPadding"] = 1;
			Constants["CollectChargesColumnWidth"] = 12;
			Constants["PrepaidChargesColumnLeftPadding"] = 1;
			Constants["PrepaidChargesColumnWidth"] = 12;
			Constants["ShowChargesHeadingInMainBody"] = "Y";

			AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);
			AssertMultilineASCIIEquals("Heading", "", Supporter.FormedPages.ChargesSectionHeader);
			AssertMultilineASCIIEquals("Body", "", Supporter.FormedPages[0].ChargesSection);
			AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", Supporter.FollowOnSection));
			AssertEquals("HasFollowOn", false, Supporter.HasFollowOnSection);

			AssertEquals("Prepaid total should be empty", ZString.Empty, Supporter.FormedPages.PrepaidChargesTotal.AmountAndCurrencyCode);
			AssertEquals("Collect total should be empty", ZString.Empty, Supporter.FormedPages.CollectChargesTotal.AmountAndCurrencyCode);
			AssertEquals("All Charges total should be empty", ZString.Empty, Supporter.FormedPages.ChargesTotal.AmountAndCurrencyCode);

			Reset();
			Supporter.ShouldPrintChargesAsLumpSum = false;
			Supporter.ShouldPrintTotalCharges = true;

			AssertMultilineASCIIEquals("Heading", "", Supporter.FormedPages.ChargesSectionHeader);
			AssertMultilineASCIIEquals("Body", "", Supporter.FormedPages[0].ChargesSection);
			AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", Supporter.FollowOnSection));
		}

		public void TestChargesSection_OriginalAsAgreedCopyWithCollectCharges()
		{
			Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			Shipment.JS_HBLAWBChargesDisplay = "CCL";

			JobHeader header = GetNewHeader(Shipment);

			CreateLineCharge(header, header.AgentCollectPK, 10000, 10000, "FRT", "ERN");
			CreateLineCharge(header, header.LocalChargesPK, 500, 700, "OLAB", "AUD");
			CreateLineCharge(header, header.AgentCollectPK, 750, 750, "DLAB", "ERN");

			Constants["NumberOfCollectChargesRows"] = 3;
			Constants["ChargeDescriptionLeftPadding"] = 0;
			Constants["ChargeDescriptionColumnWidth"] = 25;
			Constants["CollectChargesColumnLeftPadding"] = 1;
			Constants["CollectChargesColumnWidth"] = 8;
			Constants["CollectCurrencyColumnLeftPadding"] = 1;
			Constants["CollectCurrencyColumnWidth"] = 3;
			Constants["PrepaidChargesColumnLeftPadding"] = 1;
			Constants["PrepaidChargesColumnWidth"] = 8;
			Constants["PrepaidCurrencyColumnLeftPadding"] = 1;
			Constants["PrepaidCurrencyColumnWidth"] = 3;
			Constants["ShowChargesHeadingInMainBody"] = "Y";

			AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);
			AssertMultilineASCIIEquals("Heading", "", Supporter.FormedPages.ChargesSectionHeader);
			AssertMultilineASCIIEquals("Body", "", Supporter.FormedPages[0].ChargesSection);
			AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", Supporter.FollowOnSection));
			AssertEquals("HasFollowOn", false, Supporter.HasFollowOnSection);

			AssertEquals("Prepaid total should be empty", ZString.Empty, Supporter.FormedPages.PrepaidChargesTotal.AmountAndCurrencyCode);
			AssertEquals("Collect total should be empty", "10,750.00 ERN", Supporter.FormedPages.CollectChargesTotal.AmountAndCurrencyCode);
			AssertEquals("All Charges total should be empty", "10,750.00 ERN", Supporter.FormedPages.ChargesTotal.AmountAndCurrencyCode);

			Reset();
			Supporter.ShouldPrintChargesAsLumpSum = false;
			Supporter.ShouldPrintTotalCharges = true;

			AssertMultilineASCIIEquals("Heading", "", Supporter.FormedPages.ChargesSectionHeader);
			AssertMultilineASCIIEquals("Body", "", Supporter.FormedPages[0].ChargesSection);
			AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", Supporter.FollowOnSection));
		}

		public void TestChargesSection_OriginalAsAgreedCopyWithPrepaidCharges()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
				Shipment.JS_HBLAWBChargesDisplay = "CPP";

				JobHeader header = GetNewHeader(Shipment);

				CreateLineCharge(header, header.LocalChargesPK, 10000, 10500, "FRT", "AUD");
				CreateLineCharge(header, header.AgentCollectPK, 500, 525, "OLAB", "AUD");
				CreateLineCharge(header, header.LocalChargesPK, 750, 950, "DLAB", "EUR");

				Constants["NumberOfCollectChargesRows"] = 3;
				Constants["ChargeDescriptionLeftPadding"] = 0;
				Constants["ChargeDescriptionColumnWidth"] = 25;
				Constants["CollectChargesColumnLeftPadding"] = 1;
				Constants["CollectChargesColumnWidth"] = 8;
				Constants["CollectCurrencyColumnLeftPadding"] = 1;
				Constants["CollectCurrencyColumnWidth"] = 3;
				Constants["PrepaidChargesColumnLeftPadding"] = 1;
				Constants["PrepaidChargesColumnWidth"] = 8;
				Constants["PrepaidCurrencyColumnLeftPadding"] = 1;
				Constants["PrepaidCurrencyColumnWidth"] = 3;
				Constants["ShowChargesHeadingInMainBody"] = "Y";

				AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);
				AssertMultilineASCIIEquals("Heading", "", Supporter.FormedPages.ChargesSectionHeader);
				AssertMultilineASCIIEquals("Body", "", Supporter.FormedPages[0].ChargesSection);
				AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", Supporter.FollowOnSection));
				AssertEquals("HasFollowOn", false, Supporter.HasFollowOnSection);

				AssertEquals("Prepaid total should be empty", "11,450.00 USD", Supporter.FormedPages.PrepaidChargesTotal.AmountAndCurrencyCode);
				AssertEquals("Collect total should be empty", ZString.Empty, Supporter.FormedPages.CollectChargesTotal.AmountAndCurrencyCode);
				AssertEquals("All Charges total should be empty", "11,450.00 USD", Supporter.FormedPages.ChargesTotal.AmountAndCurrencyCode);

				Reset();
				Supporter.ShouldPrintChargesAsLumpSum = false;
				Supporter.ShouldPrintTotalCharges = true;

				AssertMultilineASCIIEquals("Heading", "", Supporter.FormedPages.ChargesSectionHeader);
				AssertMultilineASCIIEquals("Body", "", Supporter.FormedPages[0].ChargesSection);
				AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", Supporter.FollowOnSection));
			}
		}

		public void TestChargesSection_OriginalAsAgreedCopyWithPrepaidAndCollectCharges()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
				Shipment.JS_HBLAWBChargesDisplay = "CAL";

				JobHeader header = GetNewHeader(Shipment);

				CreateLineCharge(header, header.LocalChargesPK, 10000, 10500, "FRT", "AUD");
				CreateLineCharge(header, header.AgentCollectPK, 500, 525, "OLAB", "AUD");
				CreateLineCharge(header, header.LocalChargesPK, 750, 950, "DLAB", "EUR");

				Constants["NumberOfCollectChargesRows"] = 3;
				Constants["ChargeDescriptionLeftPadding"] = 0;
				Constants["ChargeDescriptionColumnWidth"] = 25;
				Constants["CollectChargesColumnLeftPadding"] = 1;
				Constants["CollectChargesColumnWidth"] = 8;
				Constants["CollectCurrencyColumnLeftPadding"] = 1;
				Constants["CollectCurrencyColumnWidth"] = 3;
				Constants["PrepaidChargesColumnLeftPadding"] = 1;
				Constants["PrepaidChargesColumnWidth"] = 8;
				Constants["PrepaidCurrencyColumnLeftPadding"] = 1;
				Constants["PrepaidCurrencyColumnWidth"] = 3;
				Constants["ShowChargesHeadingInMainBody"] = "Y";

				AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);
				AssertMultilineASCIIEquals("Heading", "", Supporter.FormedPages.ChargesSectionHeader);
				AssertMultilineASCIIEquals("Body", "", Supporter.FormedPages[0].ChargesSection);
				AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", Supporter.FollowOnSection));
				AssertEquals("HasFollowOn", false, Supporter.HasFollowOnSection);

				AssertEquals("Prepaid total should be empty", "11,450.00 USD", Supporter.FormedPages.PrepaidChargesTotal.AmountAndCurrencyCode);
				AssertEquals("Collect total should be empty", "525.00 USD", Supporter.FormedPages.CollectChargesTotal.AmountAndCurrencyCode);
				AssertEquals("All Charges total should be empty", "11,975.00 USD", Supporter.FormedPages.ChargesTotal.AmountAndCurrencyCode);

				Reset();
				Supporter.ShouldPrintChargesAsLumpSum = false;
				Supporter.ShouldPrintTotalCharges = true;

				AssertMultilineASCIIEquals("Heading", "", Supporter.FormedPages.ChargesSectionHeader);
				AssertMultilineASCIIEquals("Body", "", Supporter.FormedPages[0].ChargesSection);
				AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", Supporter.FollowOnSection));
			}
		}

		public void TestChargesSection_SuppressZeroCharges()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
				Shipment.JS_HBLAWBChargesDisplay = "ALL";

				JobHeader header = GetNewHeader(Shipment);

				CreateLineCharge(header, header.AgentCollectPK, 10, 8.5, "FRT", "AUD");
				CreateLineCharge(header, header.AgentCollectPK, 0, 0, "OLAB", "AUD");

				Constants["UseMultiPage"] = "Y";
				Constants["NumberOfCollectChargesRows"] = 3;
				Constants["ChargeCodeColumnLeftPadding"] = 0;
				Constants["ChargeCodeColumnWidth"] = 6;
				Constants["ChargeCodeIndex"] = 1;
				Constants["ChargeDescriptionLeftPadding"] = 1;
				Constants["ChargeDescriptionColumnWidth"] = 25;
				Constants["ChargeDescriptionCaption"] = "Charge Desc";
				Constants["ChargeDescriptionIndex"] = 2;
				Constants["CollectChargesColumnLeftPadding"] = 1;
				Constants["CollectChargesColumnWidth"] = 8;
				Constants["CollectChargesColumnCaption"] = "CCT";
				Constants["CollectChargesColumnIndex"] = 3;
				Constants["CollectCurrencyColumnLeftPadding"] = 1;
				Constants["CollectCurrencyColumnWidth"] = 3;
				Constants["CollectCurrencyColumnCaption"] = "CX";
				Constants["CollectCurrencyColumnIndex"] = 4;
				Constants["PrepaidChargesColumnLeftPadding"] = 1;
				Constants["PrepaidChargesColumnWidth"] = 8;
				Constants["PrepaidChargesColumnCaption"] = "PPD";
				Constants["PrepaidChargesColumnIndex"] = 5;
				Constants["PrepaidCurrencyColumnLeftPadding"] = 1;
				Constants["PrepaidCurrencyColumnWidth"] = 3;
				Constants["PrepaidCurrencyColumnCaption"] = "PX";
				Constants["PrepaidCurrencyColumnIndex"] = 6;

				Constants["ShowChargesHeadingInMainBody"] = "Y";

				const string expectedHeading =
					//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
					//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
					"Code   Charge Desc                    CCT CX       PPD PX \n" +
					"";

				const string expectedBody1 =
					//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
					//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
					"FRT    International Freight        10,00 AUD             \n" +
					"";

				const string expectedZeroCharge =
					"OLAB   Origin Labour Charges         0,00 AUD             \n" +
					"";

				AssertEquals("should have 2 formed pages", 1, Supporter.FormedPages.Count);
				AssertMultilineASCIIEquals("Body1", expectedHeading + expectedBody1 + expectedZeroCharge, Supporter.FormedPages[0].ChargesSection);
				AssertMultilineASCIIEquals("FollowOn", ZString.Empty, ZString.Join("\n", Supporter.FollowOnSection));
				AssertEquals("HasFollowOn", false, Supporter.HasFollowOnSection);

				Constants["SuppressZeroCharges"] = "Y";

				Reset();

				AssertEquals("should have 1 formed page", 1, Supporter.FormedPages.Count);
				AssertMultilineASCIIEquals("Body1", expectedHeading + expectedBody1, Supporter.FormedPages[0].ChargesSection);
				AssertMultilineASCIIEquals("FollowOn", ZString.Empty, ZString.Join("\n", Supporter.FollowOnSection));
				AssertEquals("HasFollowOn", false, Supporter.HasFollowOnSection);
			}
		}

		public void TestChargesSection_LumpSum()
		{
			Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			Shipment.JS_HBLAWBChargesDisplay = "PPD";

			JobHeader header = GetNewHeader(Shipment);

			CreateLineCharge(header, header.LocalChargesPK, 10000, "FRT", "AUD");
			CreateLineCharge(header, header.AgentCollectPK, 500, "OLAB", "AUD");
			CreateLineCharge(header, header.LocalChargesPK, 750.50, "DLAB", "AUD");

			Constants["NumberOfCollectChargesRows"] = 3;
			Constants["ChargeDescriptionLeftPadding"] = 0;
			Constants["ChargeDescriptionColumnWidth"] = 12;
			Constants["CollectChargesColumnLeftPadding"] = 1;
			Constants["CollectChargesColumnWidth"] = 12;
			Constants["PrepaidChargesColumnLeftPadding"] = 1;
			Constants["PrepaidChargesColumnWidth"] = 12;
			Constants["ShowChargesHeadingInMainBody"] = "Y";

			const string expectedBodyPrepaid =
				 //         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				 //1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				 "FREIGHT LUMP SUM: 10,750.50 AUD               \n" +
				 "TEN THOUSAND, SEVEN HUNDRED AND FIFTY DOLLARS \n" +
				 "AND 50 CENTS                                  \n" +
				 "";

			const string expectedBodyCollect =
				 //         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				 //1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				 "FREIGHT LUMP SUM: 500.00 AUD                  \n" +
				 "FIVE HUNDRED DOLLARS                          \n" +
				 "";

			const string expectedBodyAllCharges =
				 //         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				 //1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				 "FREIGHT LUMP SUM: 11,250.50 AUD               \n" +
				 "ELEVEN THOUSAND, TWO HUNDRED AND FIFTY DOLLARS\n" +
				 "AND 50 CENTS                                  \n" +
				 "";

			Supporter.ShouldPrintChargesAsLumpSum = true;

			AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);

			AssertMultilineASCIIEquals("Heading", "", Supporter.FormedPages.ChargesSectionHeader.Trim());
			AssertMultilineASCIIEquals("Body", expectedBodyPrepaid, Supporter.FormedPages[0].ChargesSection);
			AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", Supporter.FollowOnSection));
			AssertEquals("HasFollowOn", false, Supporter.HasFollowOnSection);

			Shipment.JS_HBLAWBChargesDisplay = "SHW";
			Reset();

			AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);

			AssertMultilineASCIIEquals("Heading", "", Supporter.FormedPages.ChargesSectionHeader.Trim());
			AssertMultilineASCIIEquals("Body", expectedBodyCollect, Supporter.FormedPages[0].ChargesSection);
			AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", Supporter.FollowOnSection));
			AssertEquals("HasFollowOn", false, Supporter.HasFollowOnSection);

			Shipment.JS_HBLAWBChargesDisplay = "ALL";
			Reset();

			AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);

			AssertMultilineASCIIEquals("Heading", "", Supporter.FormedPages.ChargesSectionHeader.Trim());
			AssertMultilineASCIIEquals("Body", expectedBodyAllCharges, Supporter.FormedPages[0].ChargesSection);
			AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", Supporter.FollowOnSection));
			AssertEquals("HasFollowOn", false, Supporter.HasFollowOnSection);
		}

		public void TestChargesSection_LumpSum_DoesNotIncludeProfitShare()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "PPP";
			chargeCode.AC_Desc = "Profit Share Charges from Registry";

			AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());

			Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			Shipment.JS_HBLAWBChargesDisplay = "PPD";

			var header = GetNewHeader(Shipment);

			CreateLineCharge(header, header.LocalChargesPK, 10000, "FRT", "AUD");
			CreateLineCharge(header, header.AgentCollectPK, 500, chargeCode.AC_Code, "AUD");

			Constants["NumberOfCollectChargesRows"] = 3;
			Constants["ChargeDescriptionLeftPadding"] = 0;
			Constants["ChargeDescriptionColumnWidth"] = 12;
			Constants["ShowChargesHeadingInMainBody"] = "Y";

			const string expectedBodyAllCharges =
				 //         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				 //1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				 "FREIGHT LUMP SUM: 10,000.00 AUD             \n" +
				 "TEN THOUSAND DOLLARS                        \n" +
				 "";

			Supporter.ShouldPrintChargesAsLumpSum = true;
			Shipment.JS_HBLAWBChargesDisplay = "ALL";
			Reset();

			CombineAssertions("Should not include the $500 profit share charges", () =>
			{
				AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);
				AssertMultilineASCIIEquals("Heading", "", Supporter.FormedPages.ChargesSectionHeader.Trim());
				AssertMultilineASCIIEquals("Body", expectedBodyAllCharges, Supporter.FormedPages[0].ChargesSection);
				AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", Supporter.FollowOnSection));
				AssertEquals("HasFollowOn", false, Supporter.HasFollowOnSection);
			});
		}

		public void TestChargesSection_MultiCurrencyLumpSum()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
				Shipment.JS_HBLAWBChargesDisplay = "PPD";

				JobHeader header = GetNewHeader(Shipment);

				var exRates = (BusinessObjectCollection)header["ExchangeRates"];
				var usdRate = exRates.AddNew();
				usdRate["JF_RX_NKRateCurrency"] = "USD";
				usdRate["JF_BaseRate"] = 1.2;

				var euroRate = exRates.AddNew();
				euroRate["JF_RX_NKRateCurrency"] = "EUR";
				euroRate["JF_BaseRate"] = 1.1;

				CreateLineCharge(header, header.LocalChargesPK, 10000, "FRT", "AUD");
				CreateLineCharge(header, header.AgentCollectPK, 500, "OLAB", "AUD");
				CreateLineCharge(header, header.LocalChargesPK, 750, "DLAB", "EUR");
				CreateLineCharge(header, header.AgentCollectPK, 450, "OPCH", "USD");

				Constants["NumberOfCollectChargesRows"] = 3;
				Constants["ChargeDescriptionLeftPadding"] = 0;
				Constants["ChargeDescriptionColumnWidth"] = 12;
				Constants["CollectChargesColumnLeftPadding"] = 1;
				Constants["CollectChargesColumnWidth"] = 12;
				Constants["PrepaidChargesColumnLeftPadding"] = 1;
				Constants["PrepaidChargesColumnWidth"] = 12;
				Constants["ShowChargesHeadingInMainBody"] = "Y";

				const string expectedBodyPrepaid =
					 //         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
					 //1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
					 "FREIGHT LUMP SUM: 12,818.18 USD               \n" +
					 "TWELVE THOUSAND, EIGHT HUNDRED AND EIGHTEEN   \n" +
					 "DOLLARS AND 18 CENTS                          \n" +
					 "";

				const string expectedBodyCollect =
					 //         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
					 //1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
					 "FREIGHT LUMP SUM: 1,050.00 USD                \n" +
					 "ONE THOUSAND, FIFTY DOLLARS                   \n" +
					 "";

				const string expectedBodyAllCharges =
					 //         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
					 //1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
					 "FREIGHT LUMP SUM: 13,868.18 USD               \n" +
					 "THIRTEEN THOUSAND, EIGHT HUNDRED AND SIXTY    \n" +
					 "EIGHT DOLLARS AND 18 CENTS                    \n" +
					 "";

				Supporter.ShouldPrintChargesAsLumpSum = true;

				AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);

				AssertMultilineASCIIEquals("Heading", "", Supporter.FormedPages.ChargesSectionHeader.Trim());
				AssertMultilineASCIIEquals("Body", expectedBodyPrepaid, Supporter.FormedPages[0].ChargesSection);
				AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", Supporter.FollowOnSection));
				AssertEquals("HasFollowOn", false, Supporter.HasFollowOnSection);

				Shipment.JS_HBLAWBChargesDisplay = "SHW";
				Reset();

				AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);

				AssertMultilineASCIIEquals("Heading", "", Supporter.FormedPages.ChargesSectionHeader.Trim());
				AssertMultilineASCIIEquals("Body", expectedBodyCollect, Supporter.FormedPages[0].ChargesSection);
				AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", Supporter.FollowOnSection));
				AssertEquals("HasFollowOn", false, Supporter.HasFollowOnSection);

				Shipment.JS_HBLAWBChargesDisplay = "ALL";
				Reset();

				AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);

				AssertMultilineASCIIEquals("Heading", "", Supporter.FormedPages.ChargesSectionHeader.Trim());
				AssertMultilineASCIIEquals("Body", expectedBodyAllCharges, Supporter.FormedPages[0].ChargesSection);
				AssertMultilineASCIIEquals("FollowOn", "", ZString.Join("\n", Supporter.FollowOnSection));
				AssertEquals("HasFollowOn", false, Supporter.HasFollowOnSection);
			}
		}

		public void TestChargesSection_LumpSumUserCurrencyConverterFromHeader()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
				Shipment.JS_HBLAWBChargesDisplay = "PPD";

				JobHeader header = GetNewHeader(Shipment);
				var exRates = (BusinessObjectCollection)header["ExchangeRates"];
				var rate = exRates.AddNew();
				rate["JF_RX_NKRateCurrency"] = "USD";
				rate["JF_BaseRate"] = 0.8;

				CreateLineCharge(header, header.LocalChargesPK, 200, "FRT", "USD");
				CreateLineCharge(header, header.LocalChargesPK, 100, "DLAB", "AUD");
				CreateLineCharge(header, header.AgentCollectPK, 400, "OLAB", "AUD");
				CreateLineCharge(header, header.AgentCollectPK, 300, "FRT", "USD");

				Constants["NumberOfCollectChargesRows"] = 3;
				Constants["ChargeDescriptionLeftPadding"] = 0;
				Constants["ChargeDescriptionColumnWidth"] = 12;
				Constants["CollectChargesColumnLeftPadding"] = 1;
				Constants["CollectChargesColumnWidth"] = 12;
				Constants["PrepaidChargesColumnLeftPadding"] = 1;
				Constants["PrepaidChargesColumnWidth"] = 12;
				Constants["ShowChargesHeadingInMainBody"] = "Y";

				const string expectedBodyPrepaid =
					 //         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
					 //1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
					 "FREIGHT LUMP SUM: 280.00 USD                  \n" +
					 "TWO HUNDRED AND EIGHTY DOLLARS                \n" +
					 "";

				const string expectedBodyCollect =
					 //         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
					 //1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
					 "FREIGHT LUMP SUM: 620.00 USD                  \n" +
					 "SIX HUNDRED AND TWENTY DOLLARS                \n" +
					 "";

				const string expectedBodyAllCharges =
					 //         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
					 //1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
					 "FREIGHT LUMP SUM: 900.00 USD                  \n" +
					 "NINE HUNDRED DOLLARS                          \n" +
					 "";

				Supporter.ShouldPrintChargesAsLumpSum = true;
				AssertMultilineASCIIEquals("Body", expectedBodyPrepaid, Supporter.FormedPages[0].ChargesSection);

				Shipment.JS_HBLAWBChargesDisplay = "SHW";
				Reset();
				AssertMultilineASCIIEquals("Body", expectedBodyCollect, Supporter.FormedPages[0].ChargesSection);

				Shipment.JS_HBLAWBChargesDisplay = "ALL";
				Reset();
				AssertMultilineASCIIEquals("Body", expectedBodyAllCharges, Supporter.FormedPages[0].ChargesSection);
			}
		}

		public void TestRORSectionWithDifferntPackingMode()
		{
			GetNewHeader(Shipment);

			AssertFormedPageDetails(Core.Constants.ContainerModes.FCL);
			AssertFormedPageDetails(Core.Constants.ContainerModes.Liquid);
			AssertFormedPageDetails(Core.Constants.ContainerModes.Bulk);
			AssertFormedPageDetails(Core.Constants.ContainerModes.BreakBulk);
			AssertFormedPageDetails(Core.Constants.ContainerModes.RollOnRollOff);
		}

		void AssertFormedPageDetails(string packingMode)
		{
			Shipment.JS_PackingMode = packingMode;
			Shipment.JS_MarksAndNumbers = "Marks Line 1";
			Shipment.DetailedGoodsDescriptionNoteText = "Description Line 1";

			Shipment.JS_ActualWeight = 1522m;
			Shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;

			Shipment.JS_ActualVolume = 6000.48m;
			Shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;

			var container1 = Factory.New<AgencyShipmentContainer>();
			container1.JC_ContainerMode = packingMode;
			container1.JC_ContainerNum = "VIN NUMBER 1";
			container1.JC_ContainerCount = 1;

			container1.JC_VehicleColor = "Green";
			container1.JC_VehicleMake = "AC";
			container1.JC_VehicleModel = "Cobra";
			container1.JC_VehicleNumberOfDoors = 2;
			container1.JC_VehicleTransmission = "MAN";
			container1.JC_VehicleYear = 1966;

			container1.JC_GrossVolumeUQ = Core.Constants.Volume.CubicMetres;
			container1.JC_GrossVolume = 6000m;

			container1.JC_GrossWeightUQ = Core.Constants.Weight.Tonnes;
			container1.JC_GrossWeight = 1.5m;

			container1.JC_TotalUnitOfMeasure = Core.Constants.Length.Kilometres;
			container1.JC_TotalLength = 0.01;
			container1.JC_TotalWidth = 0.02;
			container1.JC_TotalHeight = 0.03;

			var container2 = Factory.New<AgencyShipmentContainer>();
			container2.JC_ContainerMode = packingMode;
			container2.JC_ContainerCount = 4;

			container2.JC_VehicleColor = "Silver";
			container2.JC_VehicleMake = "Aston Martin";
			container2.JC_VehicleModel = "DB5";
			container2.JC_VehicleNumberOfDoors = 2;
			container2.JC_VehicleTransmission = "MAN";
			container2.JC_VehicleYear = 1967;

			container2.JC_GrossVolumeUQ = Core.Constants.Volume.CubicMetres;
			container2.JC_GrossVolume = 0.48m;

			container2.JC_GrossWeightUQ = Core.Constants.Weight.Grams;
			container2.JC_GrossWeight = 22000m;

			container2.JC_TotalUnitOfMeasure = Core.Constants.Length.Millimetres;
			container2.JC_TotalLength = 400;
			container2.JC_TotalWidth = 500;
			container2.JC_TotalHeight = 600;

			Supporter.TopLevelPacks.Add(new DocFormedPagesTopLevelPack(container1));
			Supporter.TopLevelPacks.Add(new DocFormedPagesTopLevelPack(container2));

			Constants["MarksAndNumbersWidth"] = 20;
			Constants["MarksAndNumbersAndGoodsDescriptionGap"] = 1;
			Constants["GoodsDescriptionWidth"] = 20;
			Constants["GoodsDescriptionAndGrossWeightGap"] = 1;
			Constants["GrossWeightWidth"] = 12;
			Constants["GrossWeightAndMeasurementGap"] = 1;
			Constants["VolumeMeasurementWidth"] = 12;

			Constants["PackRefNumberColumnWidth"] = 20;
			Constants["PackCountColumnWidth"] = 5;
			Constants["PackWeightColumnWidth"] = 11;
			Constants["PackVolumeColumnWidth"] = 11;
			Constants["PackLengthColumnWidth"] = 10;
			Constants["PackWidthColumnWidth"] = 10;
			Constants["PackHeightColumnWidth"] = 10;
			Constants["PackAreaColumnCaption"] = "Area (m2)";
			Constants["PackAreaColumnWidth"] = 10;

			Constants["PackVehicleColorColumnIndex"] = 9;
			Constants["PackVehicleMakeColumnIndex"] = 10;
			Constants["PackVehicleModelColumnIndex"] = 11;
			Constants["PackVehicleNumberOfDoorsColumnIndex"] = 12;
			Constants["PackVehicleTransmissionColumnIndex"] = 13;
			Constants["PackVehicleYearColumnIndex"] = 14;

			const string expectedDetailHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"Marks & Numbers      Goods Description        Gross Wt       Volume\n" +
				"";

			const string expectedDetailBody =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"Marks Line 1         Description Line 1        1522 KG   6000.48 M3\n" +
				"";

			const string expectedRORHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9   10   10   11   11   12   12   13   13   14   14   15   15    
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"VIN/Serial           Count Weight (KG) Volume (M3) Length (M)  Width (M) Height (M)  Area (m2) Color       Make           Model          Doors Trans. Year\n" +
				"";

			const string expectedRORBody1 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9   10   10   11   11   12   12   13   13   14   14   15   15    
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"VIN NUMBER 1             1        1500        6000     10.000     20.000     30.000    200.000 Green       AC             Cobra              2 MAN    1966\n" +
				"";

			const string expectedRORBody2 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9   10   10   11   11   12   12   13   13   14   14   15   15    
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"                         4          22        0.48      0.400      0.500      0.600      0.800 Silver      Aston Martin   DB5                2 MAN    1967\n" +
				"";

			Constants["MarksAndNumbersAndGoodsDescriptionHeight"] = 5;
			Constants["ShowDetailHeadingInMainBody"] = "Y";
			Constants["IncludeRORInMarksAndNumbersSection"] = "Y";
			Constants["ShowRORHeadingInMainBody"] = "Y";
			Reset();

			AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);
			AssertMultilineASCIIEquals("Detail Heading", expectedDetailHeading, Supporter.FormedPages.DetailsSectionHeader);
			AssertMultilineASCIIEquals("Detail Body", expectedDetailHeading + expectedDetailBody + "\n" + expectedRORHeading + expectedRORBody1, Supporter.FormedPages[0].MainBodyDetailsSection);
			AssertMultilineASCIIEquals("ROR Heading", expectedRORHeading, Supporter.FormedPages.PackRORSectionHeader);
			AssertMultilineASCIIEquals("ROR Body", ZString.Empty, Supporter.FormedPages[0].PackRORSection);
			AssertMultilineASCIIEquals("FollowOn", expectedRORHeading + expectedRORBody2, ZString.Join("\n", Supporter.FollowOnSection));

			Constants["MarksAndNumbersAndGoodsDescriptionHeight"] = 6;
			Reset();

			AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);
			AssertMultilineASCIIEquals("Detail Heading", expectedDetailHeading, Supporter.FormedPages.DetailsSectionHeader);
			AssertMultilineASCIIEquals("Detail Body", expectedDetailHeading + expectedDetailBody + "\n" + expectedRORHeading + expectedRORBody1 + expectedRORBody2, Supporter.FormedPages[0].MainBodyDetailsSection);
			AssertMultilineASCIIEquals("ROR Heading", expectedRORHeading, Supporter.FormedPages.PackRORSectionHeader);
			AssertMultilineASCIIEquals("ROR Body", ZString.Empty, Supporter.FormedPages[0].PackRORSection);
			AssertMultilineASCIIEquals("FollowOn", ZString.Empty, ZString.Join("\n", Supporter.FollowOnSection));

			Constants["ShowRORHeadingInMainBody"] = "N";
			Reset();

			AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);
			AssertMultilineASCIIEquals("Detail Heading", expectedDetailHeading, Supporter.FormedPages.DetailsSectionHeader);
			AssertMultilineASCIIEquals("Detail Body", expectedDetailHeading + expectedDetailBody + "\n" + expectedRORBody1 + expectedRORBody2, Supporter.FormedPages[0].MainBodyDetailsSection);
			AssertMultilineASCIIEquals("ROR Heading", expectedRORHeading, Supporter.FormedPages.PackRORSectionHeader);
			AssertMultilineASCIIEquals("ROR Body", ZString.Empty, Supporter.FormedPages[0].PackRORSection);
			AssertMultilineASCIIEquals("FollowOn", ZString.Empty, ZString.Join("\n", Supporter.FollowOnSection));

			Constants["ShowRORHeadingInMainBody"] = "Y";
			Constants["IncludeRORInMarksAndNumbersSection"] = "N";
			Reset();

			AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);
			AssertMultilineASCIIEquals("Detail Heading", expectedDetailHeading, Supporter.FormedPages.DetailsSectionHeader);
			AssertMultilineASCIIEquals("Detail Body", expectedDetailHeading + expectedDetailBody, Supporter.FormedPages[0].MainBodyDetailsSection);
			AssertMultilineASCIIEquals("ROR Heading", expectedRORHeading, Supporter.FormedPages.PackRORSectionHeader);
			AssertMultilineASCIIEquals("ROR Body", expectedRORHeading + expectedRORBody1 + expectedRORBody2, Supporter.FormedPages[0].PackRORSection);
			AssertMultilineASCIIEquals("FollowOn", ZString.Empty, ZString.Join("\n", Supporter.FollowOnSection));
		}

		public void TestRORSection_CombinedColumns()
		{
			GetNewHeader(Shipment);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.RollOnRollOff;
			Shipment.JS_MarksAndNumbers = "Marks Line 1";
			Shipment.DetailedGoodsDescriptionNoteText = "Description Line 1";

			Shipment.JS_ActualWeight = 1522m;
			Shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;

			Shipment.JS_ActualVolume = 6000.48m;
			Shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;

			var container1 = Factory.New<AgencyShipmentContainer>();
			container1.JC_ContainerMode = Core.Constants.ContainerModes.RollOnRollOff;
			container1.JC_ContainerNum = "VIN NUMBER 1";
			container1.JC_ContainerCount = 1;
			container1.JC_MarksAndNumbers = "VEHICLE 1 M&N";

			container1.JC_VehicleColor = "Green";
			container1.JC_VehicleMake = "AC";
			container1.JC_VehicleModel = "Cobra";
			container1.JC_VehicleNumberOfDoors = 2;
			container1.JC_VehicleTransmission = "MAN";
			container1.JC_VehicleYear = 1966;

			container1.JC_GrossVolumeUQ = Core.Constants.Volume.CubicMetres;
			container1.JC_GrossVolume = 6000m;

			container1.JC_GrossWeightUQ = Core.Constants.Weight.Tonnes;
			container1.JC_GrossWeight = 1.5m;

			container1.JC_TotalUnitOfMeasure = Core.Constants.Length.Kilometres;
			container1.JC_TotalLength = 0.01;
			container1.JC_TotalWidth = 0.02;
			container1.JC_TotalHeight = 0.03;

			container1.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0440", "", "IMO").First().PK;

			var container2 = Factory.New<AgencyShipmentContainer>();
			container2.JC_ContainerMode = Core.Constants.ContainerModes.RollOnRollOff;
			container2.JC_ContainerCount = 4;

			container2.JC_VehicleColor = "Silver";
			container2.JC_VehicleMake = "Aston Martin";
			container2.JC_VehicleModel = "DB5";
			container2.JC_VehicleNumberOfDoors = 2;
			container2.JC_VehicleTransmission = "MAN";
			container2.JC_VehicleYear = 1967;

			container2.JC_GrossVolumeUQ = Core.Constants.Volume.CubicMetres;
			container2.JC_GrossVolume = 0.48m;

			container2.JC_GrossWeightUQ = Core.Constants.Weight.Grams;
			container2.JC_GrossWeight = 22000m;

			container2.JC_TotalUnitOfMeasure = Core.Constants.Length.Millimetres;
			container2.JC_TotalLength = 400;
			container2.JC_TotalWidth = 500;
			container2.JC_TotalHeight = 600;

			Supporter.TopLevelPacks.Add(new DocFormedPagesTopLevelPack(container1));
			Supporter.TopLevelPacks.Add(new DocFormedPagesTopLevelPack(container2));

			Constants["MarksAndNumbersWidth"] = 20;
			Constants["MarksAndNumbersAndGoodsDescriptionGap"] = 1;
			Constants["GoodsDescriptionWidth"] = 20;
			Constants["GoodsDescriptionAndGrossWeightGap"] = 1;
			Constants["GrossWeightWidth"] = 12;
			Constants["GrossWeightAndMeasurementGap"] = 1;
			Constants["VolumeMeasurementWidth"] = 12;

			Constants["PackRefNumberColumnIndex"] = 0;
			Constants["PackRefNumberAndMarksAndNumbersColumnIndex"] = 1;
			Constants["PackRefNumberAndMarksAndNumbersColumnWidth"] = 20;
			Constants["PackRefNumberAndMarksAndNumbersColumnCaption"] = "VIN";
			Constants["PackVehicleFullDetailsColumnIndex"] = 2;
			Constants["PackVehicleFullDetailsColumnWidth"] = 40;
			Constants["PackVehicleFullDetailsLeftPadding"] = 3;
			Constants["PackVehicleFullDetailsColumnCaption"] = "Details Section";
			Constants["PackDimensionsColumnIndex"] = 3;
			Constants["PackDimensionsColumnWidth"] = 24;
			Constants["PackDimensionsColumnCaption"] = "Dimensions";
			Constants["PackDimensionsColumnLeftPadding"] = 5;

			Constants["PackCountColumnIndex"] = 0;
			Constants["PackWeightColumnIndex"] = 0;
			Constants["PackVolumeColumnIndex"] = 0;
			Constants["PackLengthColumnIndex"] = 0;
			Constants["PackWidthColumnIndex"] = 0;
			Constants["PackHeightColumnIndex"] = 0;
			Constants["PackAreaColumnIndex"] = 0;

			const string expectedDetailHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"Marks & Numbers      Goods Description        Gross Wt       Volume\n" +
				"";

			const string expectedDetailBody =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"Marks Line 1         Description Line 1        1522 KG   6000.48 M3\n" +
				"";

			const string expectedRORHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9   10   10   11   11   12   12   13   13   14   14   15   15    
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"VIN                  Details Section                                            Dimensions\n" +
				"";

			const string expectedRORBody1 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9   10   10   11   11   12   12   13   13   14   14   15   15    
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"VIN NUMBER 1         1966 AC Cobra Green 2 door MAN                                6000 M3\n" +
				"";

			const string expectedRORBody2 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9   10   10   11   11   12   12   13   13   14   14   15   15    
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"VEHICLE 1 M&N        UN0440, CHARGES, SHAPED, class 1.4D               10.00x20.00x30.00 M\n" +
				"";

			const string expectedFollowOn =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9   10   10   11   11   12   12   13   13   14   14   15   15    
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"                     1967 Aston Martin DB5 Silver 2 door MAN      0.48 M3 0.40x0.50x0.60 M\n" +
				"";

			Constants["MarksAndNumbersAndGoodsDescriptionHeight"] = 5;
			Constants["ShowDetailHeadingInMainBody"] = "Y";
			Constants["IncludeRORInMarksAndNumbersSection"] = "Y";
			Constants["ShowRORHeadingInMainBody"] = "Y";
			Constants["DimensionsDecimalPlaces"] = 2;
			Reset();

			AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);
			AssertMultilineASCIIEquals("Detail Heading", expectedDetailHeading, Supporter.FormedPages.DetailsSectionHeader);
			AssertMultilineASCIIEquals("Detail Body", expectedDetailHeading + expectedDetailBody + "\n" + expectedRORHeading + expectedRORBody1, Supporter.FormedPages[0].MainBodyDetailsSection);
			AssertMultilineASCIIEquals("ROR Heading", expectedRORHeading, Supporter.FormedPages.PackRORSectionHeader);
			AssertMultilineASCIIEquals("ROR Body", ZString.Empty, Supporter.FormedPages[0].PackRORSection);
			AssertMultilineASCIIEquals("FollowOn", expectedRORHeading + expectedRORBody2 + expectedFollowOn, ZString.Join("\n", Supporter.FollowOnSection));

			Constants["MarksAndNumbersAndGoodsDescriptionHeight"] = 6;
			Reset();

			AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);
			AssertMultilineASCIIEquals("Detail Heading", expectedDetailHeading, Supporter.FormedPages.DetailsSectionHeader);
			AssertMultilineASCIIEquals("Detail Body", expectedDetailHeading + expectedDetailBody + "\n" + expectedRORHeading + expectedRORBody1 + expectedRORBody2, Supporter.FormedPages[0].MainBodyDetailsSection);
			AssertMultilineASCIIEquals("ROR Heading", expectedRORHeading, Supporter.FormedPages.PackRORSectionHeader);
			AssertMultilineASCIIEquals("ROR Body", ZString.Empty, Supporter.FormedPages[0].PackRORSection);
			AssertMultilineASCIIEquals("FollowOn", expectedRORHeading + expectedFollowOn, ZString.Join("\n", Supporter.FollowOnSection));
		}

		public void TestBOLClauseSection()
		{
			Shipment.JS_MarksAndNumbers = "Marks Line 1";
			Shipment.DetailedGoodsDescriptionNoteText = "Description Line 1";

			GetNewHeader(Shipment);

			PackLine packLine = Shipment.OuterPackLines.AddNew();
			packLine.JL_RefNumber = "VIN Number 1";
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine.JL_PackageCount = 1;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			packLine.JL_ActualWeight = 1.5;
			packLine.JL_UnitOfDimension = Core.Constants.Length.Kilometres;
			packLine.JL_Length = 0.01;
			packLine.JL_Width = 0.02;
			packLine.JL_Height = 0.03;

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Keg;
			packLine.JL_PackageCount = 4;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Grams;
			packLine.JL_ActualWeight = 22000;
			packLine.JL_UnitOfDimension = Core.Constants.Length.Millimetres;
			packLine.JL_Length = 400;
			packLine.JL_Width = 500;
			packLine.JL_Height = 600;

			Shipment.UpdateShipmentFromOuterPackLines();

			Supporter.BOLClause = "BOL Clause for testing line 1 of 5\n"
				+ "BOL Clause for testing line 2 of 5\n"
				+ "BOL Clause for testing line 3 of 5\n"
				+ "BOL Clause for testing line 4 of 5\n"
				+ "BOL Clause for testing line 5 of 5";

			Constants["MarksAndNumbersWidth"] = 20;
			Constants["MarksAndNumbersAndGoodsDescriptionGap"] = 1;
			Constants["GoodsDescriptionWidth"] = 20;
			Constants["GoodsDescriptionAndGrossWeightGap"] = 1;
			Constants["GrossWeightWidth"] = 12;
			Constants["GrossWeightAndMeasurementGap"] = 1;
			Constants["VolumeMeasurementWidth"] = 12;

			Constants["PackRefNumberColumnWidth"] = 20;
			Constants["PackCountColumnWidth"] = 5;
			Constants["PackWeightColumnWidth"] = 11;
			Constants["PackVolumeColumnWidth"] = 11;
			Constants["PackLengthColumnWidth"] = 10;
			Constants["PackWidthColumnWidth"] = 10;
			Constants["PackHeightColumnWidth"] = 10;
			Constants["PackAreaColumnCaption"] = "Area (m2)";
			Constants["PackAreaColumnWidth"] = 10;

			const string expectedDetailHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"Marks & Numbers      Goods Description        Gross Wt       Volume\n" +
				"";

			const string expectedDetailBody =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"Marks Line 1         Description Line 1        1522 KG   6000.48 M3\n" +
				"";

			const string expectedBOLClauseHeading =
				//         1    1    2    2    3 
				//1...5....0....5....0....5....0.
				"Bill of Lading Clause   \n" +
				"";

			const string expectedBOLClauseBody1 =
				//         1    1    2    2    3 
				//1...5....0....5....0....5....0.
				"BOL Clause for testing  \n";

			const string expectedBOLClauseBody2 =
				//         1    1    2    2    3 
				//1...5....0....5....0....5....0.
				"line 1 of 5             \n" +
				"BOL Clause for testing  \n" +
				"line 2 of 5             \n" +
				"BOL Clause for testing  \n" +
				"line 3 of 5             \n" +
				"BOL Clause for testing  \n" +
				"line 4 of 5             \n" +
				"BOL Clause for testing  \n" +
				"line 5 of 5             \n";

			Constants["MarksAndNumbersAndGoodsDescriptionHeight"] = 5;
			Constants["ShowDetailHeadingInMainBody"] = "Y";
			Constants["IncludeBOLClauseInGoodsDescription"] = "N";
			Constants["IncludeBOLClauseSectionInMarksAndNumbersSection"] = "Y";
			Constants["ShowBOLClauseSectionHeadingInMainBody"] = "Y";
			Constants["BOLClauseColumnCaption"] = "Bill of Lading Clause";
			Constants["BOLClauseColumnWidth"] = 24;
			Constants["NumberOfBOLClauseRows"] = 2;
			Reset();

			AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);
			AssertMultilineASCIIEquals("Detail Heading", expectedDetailHeading, Supporter.FormedPages.DetailsSectionHeader);
			AssertMultilineASCIIEquals("Detail Body", expectedDetailHeading + expectedDetailBody + "\n" + expectedBOLClauseHeading + expectedBOLClauseBody1, Supporter.FormedPages[0].MainBodyDetailsSection);
			AssertMultilineASCIIEquals("BOL Clause Heading", expectedBOLClauseHeading, Supporter.FormedPages.BOLClauseSectionHeader);
			AssertMultilineASCIIEquals("BOL Clause Body", ZString.Empty, Supporter.FormedPages[0].BOLClauseSection);
			AssertMultilineASCIIEquals("FollowOn", expectedBOLClauseHeading + expectedBOLClauseBody2, ZString.Join("\n", Supporter.FollowOnSection));

			Constants["MarksAndNumbersAndGoodsDescriptionHeight"] = 14;
			Reset();

			AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);
			AssertMultilineASCIIEquals("Detail Heading", expectedDetailHeading, Supporter.FormedPages.DetailsSectionHeader);
			AssertMultilineASCIIEquals("Detail Body", expectedDetailHeading + expectedDetailBody + "\n" + expectedBOLClauseHeading + expectedBOLClauseBody1 + expectedBOLClauseBody2, Supporter.FormedPages[0].MainBodyDetailsSection);
			AssertMultilineASCIIEquals("BOL Clause Heading", expectedBOLClauseHeading, Supporter.FormedPages.BOLClauseSectionHeader);
			AssertMultilineASCIIEquals("BOL Clause Body", ZString.Empty, Supporter.FormedPages[0].BOLClauseSection);
			AssertMultilineASCIIEquals("FollowOn", ZString.Empty, ZString.Join("\n", Supporter.FollowOnSection));

			Constants["ShowBOLClauseSectionHeadingInMainBody"] = "N";
			Reset();

			AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);
			AssertMultilineASCIIEquals("Detail Heading", expectedDetailHeading, Supporter.FormedPages.DetailsSectionHeader);
			AssertMultilineASCIIEquals("Detail Body", expectedDetailHeading + expectedDetailBody + "\n" + expectedBOLClauseBody1 + expectedBOLClauseBody2, Supporter.FormedPages[0].MainBodyDetailsSection);
			AssertMultilineASCIIEquals("BOL Clause Heading", expectedBOLClauseHeading, Supporter.FormedPages.BOLClauseSectionHeader);
			AssertMultilineASCIIEquals("BOL Clause Body", ZString.Empty, Supporter.FormedPages[0].BOLClauseSection);
			AssertMultilineASCIIEquals("FollowOn", ZString.Empty, ZString.Join("\n", Supporter.FollowOnSection));

			Constants["ShowBOLClauseSectionHeadingInMainBody"] = "Y";
			Constants["IncludeBOLClauseSectionInMarksAndNumbersSection"] = "N";
			Constants["NumberOfBOLClauseRows"] = 12;
			Reset();

			AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);
			AssertMultilineASCIIEquals("Detail Heading", expectedDetailHeading, Supporter.FormedPages.DetailsSectionHeader);
			AssertMultilineASCIIEquals("Detail Body", expectedDetailHeading + expectedDetailBody, Supporter.FormedPages[0].MainBodyDetailsSection);
			AssertMultilineASCIIEquals("BOL Clause Heading", expectedBOLClauseHeading, Supporter.FormedPages.BOLClauseSectionHeader);
			AssertMultilineASCIIEquals("BOL Clause Body", expectedBOLClauseHeading + expectedBOLClauseBody1 + expectedBOLClauseBody2, Supporter.FormedPages[0].BOLClauseSection);
			AssertMultilineASCIIEquals("FollowOn", ZString.Empty, ZString.Join("\n", Supporter.FollowOnSection));

			Constants["IncludeBOLClauseInGoodsDescription"] = "Y";
			Constants["IncludeBOLClauseSectionInMarksAndNumbersSection"] = "N";
			Constants["ShowBOLClauseSectionHeadingInMainBody"] = "N";
			Reset();

			AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);
			AssertMultilineASCIIEquals("Detail Heading", expectedDetailHeading, Supporter.FormedPages.DetailsSectionHeader);
			AssertMultilineASCIIEquals("Detail Body", expectedDetailHeading + expectedDetailBody, Supporter.FormedPages[0].MainBodyDetailsSection);
			AssertMultilineASCIIEquals("BOL Clause Heading", expectedBOLClauseHeading, Supporter.FormedPages.BOLClauseSectionHeader);
			AssertMultilineASCIIEquals("BOL Clause Body", ZString.Empty, Supporter.FormedPages[0].BOLClauseSection);
			AssertMultilineASCIIEquals("FollowOn", ZString.Empty, ZString.Join("\n", Supporter.FollowOnSection));
		}

		public void TestDetailSectionColumns()
		{
			GetNewHeader(Shipment);

			Constants["VolumeMeasurementLeftPadding"] = 3;
			Constants["VolumeMeasurementWidth"] = 6;
			Constants["VolumeMeasurementCaption"] = "MyVol";
			Constants["VolumeMeasurementIndex"] = 1;

			Constants["GrossWeightLeftPadding"] = 4;
			Constants["GrossWeightWidth"] = 7;
			Constants["GrossWeightCaption"] = "MyWt";
			Constants["GrossWeightIndex"] = 2;

			Constants["GoodsDescLeftPadding"] = 5;
			Constants["GoodsDescriptionWidth"] = 14;
			Constants["GoodsDescCaption"] = "MyDesc";
			Constants["GoodsDescIndex"] = 3;

			Constants["MarksAndNumbersLeftPadding"] = 7;
			Constants["MarksAndNumbersWidth"] = 16;
			Constants["MarksAndNumbersCaption"] = "MyMarks";
			Constants["MarksAndNumbersIndex"] = 4;

			Constants["PackagesWidth"] = 8;
			Constants["PackagesLeftPadding"] = 6;
			Constants["PackagesCaption"] = "MyPacks";
			Constants["PackagesIndex"] = 5;

			const string expectedHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"    MyVol       MyWt     MyDesc               MyMarks                MyPacks\n" +
				"";

			AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);

			AssertMultilineASCIIEquals("Heading", expectedHeading, Supporter.FormedPages.DetailsSectionHeader);
		}

		public void TestContainerSectionColumns()
		{
			GetNewHeader(Shipment);

			Constants["ContainerPackagesLeftPadding"] = 2;
			Constants["ContainerPackagesWidth"] = 8;
			Constants["ContainerPackagesIndex"] = 1;
			Constants["ContainerPackagesCaption"] = "MyPacks";

			Constants["ContainerVolumeLeftPadding"] = 3;
			Constants["ContainerVolumeWidth"] = 10;
			Constants["ContainerVolumeIndex"] = 2;
			Constants["ContainerVolumeCaption"] = "MyVol";

			Constants["ContainerGrossLeftPadding"] = 4;
			Constants["ContainerGrossWidth"] = 11;
			Constants["ContainerGrossIndex"] = 3;
			Constants["ContainerGrossCaption"] = "MyGross";

			Constants["ContainerTareLeftPadding"] = 5;
			Constants["ContainerTareWidth"] = 12;
			Constants["ContainerTareIndex"] = 4;
			Constants["ContainerTareCaption"] = "MyTare";

			Constants["ContainerWeightLeftPadding"] = 4;
			Constants["ContainerWeightWidth"] = 13;
			Constants["ContainerWeightIndex"] = 5;
			Constants["ContainerWeightCaption"] = "MyWt";

			Constants["ContainerTypeLeftPadding"] = 7;
			Constants["ContainerTypeWidth"] = 8;
			Constants["ContainerTypeIndex"] = 6;
			Constants["ContainerTypeCaption"] = "MyType";

			Constants["ContainerSealLeftPadding"] = 8;
			Constants["ContainerSealWidth"] = 8;
			Constants["ContainerSealIndex"] = 7;
			Constants["ContainerSealCaption"] = "MySeal";

			Constants["ContainerNumberLeftPadding"] = 9;
			Constants["ContainerNumberWidth"] = 11;
			Constants["ContainerNumberIndex"] = 8;
			Constants["ContainerNumberCaption"] = "MyNumber";

			Constants["ContainerModeLeftPadding"] = 2;
			Constants["ContainerModeWidth"] = 7;
			Constants["ContainerModeIndex"] = 9;
			Constants["ContainerModeCaption"] = "MyMode";

			Constants["ContainerTemperatureSettingLeftPadding"] = 3;
			Constants["ContainerTemperatureSettingWidth"] = 6;
			Constants["ContainerTemperatureSettingIndex"] = 10;
			Constants["ContainerTemperatureSettingCaption"] = "MyTemp";

			Constants["ContainerHumiditySettingLeftPadding"] = 4;
			Constants["ContainerHumiditySettingWidth"] = 9;
			Constants["ContainerHumiditySettingIndex"] = 11;
			Constants["ContainerHumiditySettingCaption"] = "MyHumid";

			Supporter.DisplayContainers = true;

			const string expectedHeading =
				//                                                                                                   1    1    1    1    1    1    1    1    1    1    1
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8    9    9    0    0    1    1    2    2    3    3    4    4    5    
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0...
				"   MyPacks        MyVol        MyGross           MyTare             MyWt       MyType          MySeal           MyNumber      MyMode   MyTemp      MyHumid\n" +
				"";

			AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);
			AssertMultilineASCIIEquals("Heading", expectedHeading, Supporter.FormedPages.ContainersSectionHeader);
		}

		public void TestChargeSectionColumns()
		{
			Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			Shipment.JS_HBLAWBChargesDisplay = "ALL";

			JobHeader header = GetNewHeader(Shipment);

			CreateLineCharge(header, header.LocalChargesPK, 10000, "FRT", "AUD");
			CreateLineCharge(header, header.AgentCollectPK, 500, "OLAB", "AUD");
			CreateLineCharge(header, header.LocalChargesPK, 750, "DLAB", "EUR");

			Constants["PrepaidCurrencyColumnIndex"] = 1;
			Constants["PrepaidCurrencyColumnLeftPadding"] = 2;
			Constants["PrepaidCurrencyColumnWidth"] = 4;
			Constants["PrepaidCurrencyColumnCaption"] = "MyPX";

			Constants["PrepaidChargesColumnIndex"] = 2;
			Constants["PrepaidChargesColumnLeftPadding"] = 3;
			Constants["PrepaidChargesColumnWidth"] = 6;
			Constants["PrepaidChargesColumnCaption"] = "MyPPD";

			Constants["CollectCurrencyColumnIndex"] = 3;
			Constants["CollectCurrencyColumnLeftPadding"] = 4;
			Constants["CollectCurrencyColumnWidth"] = 5;
			Constants["CollectCurrencyColumnCaption"] = "MyCX";

			Constants["CollectChargesColumnIndex"] = 4;
			Constants["CollectChargesColumnLeftPadding"] = 4;
			Constants["CollectChargesColumnWidth"] = 7;
			Constants["CollectChargesColumnCaption"] = "MyCCX";

			Constants["ChargeDescriptionIndex"] = 5;
			Constants["ChargeDescriptionLeftPadding"] = 5;
			Constants["ChargeDescriptionColumnWidth"] = 12;
			Constants["ChargeDescriptionCaption"] = "MyDesc";

			Constants["ChargeCodeIndex"] = 6;
			Constants["ChargeCodeLeftPadding"] = 7;
			Constants["ChargeCodeColumnWidth"] = 6;
			Constants["ChargeCodeCaption"] = "MyCode";

			const string expectedHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7    8    8
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"  MyPX    MyPPD    MyCX       MyCCX     MyDesc             MyCode\n" +
				"";

			AssertEquals("Should only have 1 formed page", 1, Supporter.FormedPages.Count);
			AssertMultilineASCIIEquals("Heading", expectedHeading, Supporter.FormedPages.ChargesSectionHeader);
		}

		public void TestShipperLoadAndCount()
		{
			Constants["ContainerModeIndex"] = 0;

			var consol = Factory.New<ForwardingConsol>();
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container1.JC_DeliveryMode = "CFS/CFS";
			container1.JC_ContainerMode = "FCL";
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			container2.JC_DeliveryMode = "CFS/CFS";
			container2.JC_ContainerMode = "FCL";

			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_PackingMode = "FCL";
			var pack1 = shipment.OuterPackLines.AddNew();
			pack1.JL_PackageCount = 100;
			pack1.SetContainer(container1.PK);
			var pack2 = shipment.OuterPackLines.AddNew();
			pack2.SetContainer(container2.PK);
			pack2.JL_PackageCount = 200;

			DocForwardingShipment docShipment = DocForwardingShipment.New(shipment, Factory);
			DocBillOfLading docBOL = new DocBillOfLading(docShipment);
			AssertEquals("No CY mode containers, Container mode column is hidden", "", docBOL.FormedPages.ShipperLoadAndCount);

			container2.JC_DeliveryMode = "CY/CY";

			docBOL = new DocBillOfLading(docShipment);
			AssertEquals("Container Mode column is hidden", "", docBOL.FormedPages.ShipperLoadAndCount);

			Constants["ContainerModeIndex"] = 1;
			container2.JC_DeliveryMode = "CFS/CFS";
			Reset();

			docBOL = new DocBillOfLading(docShipment);
			AssertEquals("No CY mode containers, Container mode column is shown", "", docBOL.FormedPages.ShipperLoadAndCount);

			container2.JC_DeliveryMode = "CY/CY";

			docBOL = new DocBillOfLading(docShipment);
			AssertEquals("CY mode containers, Container mode column is shown", "* Shipper Load and Count", docBOL.FormedPages.ShipperLoadAndCount);

			container2.JC_DeliveryMode = "DOOR/CY";

			docBOL = new DocBillOfLading(docShipment);
			AssertEquals("CY mode containers, Container mode column is shown", "* Shipper Load and Count", docBOL.FormedPages.ShipperLoadAndCount);
		}

		#region Implementation

		JobHeader GetNewHeader(IJobHeaderParent parent)
		{
			JobHeader header = new JobHeader.Loader(Shipment).TryLoadOrCreate();
			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			header.JH_ParentID = Shipment.PK;
			header.JH_GB = GlbBranch.CurrentBranch.PK;
			header.JH_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "FIS")).PK;
			header.JH_JobNum = Shipment.JobNumber;
			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			header.AgentCollectPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;

			return header;
		}

		JobCharge CreateLineCharge(JobHeader header, ZGuid sellAccountPK, ZDecimal osSellAmount, ZString chargeCode, ZString currencyCode)
		{
			return CreateLineCharge(header, sellAccountPK, osSellAmount, 0m, chargeCode, currencyCode, ZString.Empty, ZString.Empty);
		}

		JobCharge CreateLineCharge(JobHeader header, ZGuid sellAccountPK, ZDecimal osSellAmount, ZString chargeCode, ZString currencyCode, ZString chargeCodeDesc)
		{
			return CreateLineCharge(header, sellAccountPK, osSellAmount, 0m, chargeCode, currencyCode, ZString.Empty, chargeCodeDesc);
		}

		JobCharge CreateLineCharge(JobHeader header, ZGuid sellAccountPK, ZDecimal osSellAmount, ZDecimal localSellAmount, ZString chargeCode, ZString currencyCode)
		{
			return CreateLineCharge(header, sellAccountPK, osSellAmount, localSellAmount, chargeCode, currencyCode, ZString.Empty, ZString.Empty);
		}

		JobCharge CreateLineCharge(JobHeader header, ZGuid sellAccountPK, ZDecimal osSellAmount, ZDecimal localSellAmount, ZString chargeCode, ZString currencyCode, ZString chargeCodeDesc)
		{
			return CreateLineCharge(header, sellAccountPK, osSellAmount, localSellAmount, chargeCode, currencyCode, ZString.Empty, chargeCodeDesc);
		}

		JobCharge CreateLineCharge(JobHeader header, ZGuid sellAccountPK, ZDecimal osSellAmount, ZDecimal localSellAmount, ZString chargeCode, ZString currencyCode, ZString chargeDesc, ZString chargeCodeDesc)
		{
			var query = new ZQuery(AccChargeCodeSchema.AC_Code, SQLComparisonOperator.Equal, chargeCode);
			query.AddToFilter(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.Equal, header.JH_GC);

			var accChargeCode = Factory.LoadTop1<AccChargeCode>(query);

			var lineCharge = Factory.New<JobCharge>();
			lineCharge.JR_JH = header.PK;
			lineCharge.JR_GE = header.JH_GE;
			lineCharge.JR_GB = header.JH_GB;
			lineCharge.JR_AC = accChargeCode.PK;
			lineCharge.JR_OH_SellAccount = sellAccountPK;
			lineCharge.JR_RX_NKSellCurrency = currencyCode;
			lineCharge.JR_OSSellAmt = osSellAmount;
			var exRateValue = localSellAmount.IsEmpty ? 0m : osSellAmount != localSellAmount ? osSellAmount / localSellAmount : 1m;
			//setting rate correctly
			if (exRateValue != 0)
			{
				var exRateLink = lineCharge.GetType().GetProperty("RevenueExchangeRate").GetValue(lineCharge);
				exRateLink?.GetType().GetMethod("SetBuyRate_ForTestOnly").Invoke(exRateLink, new object[] { exRateValue });
			}
			if (!localSellAmount.IsEmpty)
			{
				lineCharge.JR_LocalSellAmt = localSellAmount;
			}
			lineCharge.JR_Desc = chargeDesc.IsEmpty ? accChargeCode.AC_DescMultilingual : chargeDesc;
			lineCharge.ChargeCode.AC_Desc = chargeCodeDesc;

			return lineCharge;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DocBillOfLadingFormedPage();
		}

		protected override DocBillOfLadingFormedPageCollection GetCollectionToTest()
		{
			return new DocBillOfLadingFormedPageCollection(null, null, Factory);
		}

		#endregion
	}
}
