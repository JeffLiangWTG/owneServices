using System;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PackageWrapperFromFreightPackage))]
	sealed class PackageWrapperFromFreightPackageTest : PackageWrapperTest
	{
		#region PackLineId

		public void TestPackLineId()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var pack = shipment.OuterPackLines.AddNew();
			pack.JL_PackLineId = "PL0001";
			var wrapper = new PackageWrapperFromFreightPackage(pack, Factory);
			AssertEquals("Pack Line Id", "PL0001", wrapper.PackLineId);
		}

		#endregion

		#region InnerPackages

		public void TestInnerPackages()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var pack = shipment.OuterPackLines.AddNew();

			var wrapper = new PackageWrapperFromFreightPackage(pack, Factory);
			AssertEquals(0, wrapper.InnerPackages.Count);

			var innerPack1 = shipment.InnerPackLines.AddNew();
			var innerPack2 = shipment.InnerPackLines.AddNew();
			wrapper = new PackageWrapperFromFreightPackage(pack, Factory);
			AssertEquals(0, wrapper.InnerPackages.Count);

			innerPack1.JL_JL_OuterPackLine = pack.PK;
			wrapper = new PackageWrapperFromFreightPackage(pack, Factory);
			AssertEquals(1, wrapper.InnerPackages.Count);
			AssertEquals(innerPack1.PK, wrapper.InnerPackages[0].Identifier);

			innerPack2.JL_JL_OuterPackLine = pack.PK;
			wrapper = new PackageWrapperFromFreightPackage(pack, Factory);
			AssertEquals(2, wrapper.InnerPackages.Count);
			AssertEquals(innerPack1.PK, wrapper.InnerPackages[0].Identifier);
			AssertEquals(innerPack2.PK, wrapper.InnerPackages[1].Identifier);
		}

		#endregion

		#region Consol

		public void TestGetParentConsol()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "CNCAN";
			shipment.JS_RL_NKDestination = "AUSYD";
			var pack = shipment.OuterPackLines.AddNew();

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_RL_NKLoadPort = "CNCAN";
			consol1.JK_RL_NKDischargePort = "JPOSA";

			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "CONT001";
			container1.PackLines.Add(pack);

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_RL_NKLoadPort = "JPOSA";
			consol2.JK_RL_NKDischargePort = "AUSYD";

			var container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = "CONT002";
			container2.PackLines.Add(pack);
			Factory.Save();

			AssertEquals("Pre-condition:", consol1, shipment.DepartureConsolForDocuments);
			AssertEquals("Pre-condition:", consol2, shipment.ArrivalConsolForDocuments);

			pack.CurrentConsol = consol2;

			var wrapper1 = new PackageWrapperFromFreightPackage(pack, Factory);
			AssertEquals("Expected wrapper to use current consol's container", "CONT002", wrapper1.ContainerNo);

			pack.CurrentConsol = null;
			shipment.CurrentConsolForDocuments = null;

			var departureWrapper = new PackageWrapperFromFreightPackage(pack, Factory);
			departureWrapper.SetDocumentDirectionForTesting(nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.DEP));
			AssertEquals("DocumentDirection DEP: using departure consol's container", "CONT001", departureWrapper.ContainerNo);

			var arrivalWrapper = new PackageWrapperFromFreightPackage(pack, Factory);
			arrivalWrapper.SetDocumentDirectionForTesting(nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.ARV));
			AssertEquals("DocumentDirection ARV: using arrival consol's container", "CONT002", arrivalWrapper.ContainerNo);

			var anyDocumentDirectionWrapper = new PackageWrapperFromFreightPackage(pack, Factory);
			anyDocumentDirectionWrapper.SetDocumentDirectionForTesting(nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.ANY));
			AssertEquals("DocumentDirection ANY: using departure consol's container", "CONT001", anyDocumentDirectionWrapper.ContainerNo);

			var consol3 = shipment.Consols.AddNew();
			var container3 = consol3.Containers.AddNew();
			container3.JC_ContainerNum = "CONT003";
			container3.PackLines.Add(pack);

			shipment.CurrentConsolForDocuments = consol3;

			var wrapper2 = new PackageWrapperFromFreightPackage(pack, Factory);
			AssertEquals("Expected wrapper to use contatiner3 as it belongs to the current consol", "CONT003", wrapper2.ContainerNo);
		}

		#endregion

		#region Containers

		public void TestContainerNo()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			PackLine pack = shipment.OuterPackLines.AddNew();
			CommonConsol consol1 = shipment.Consols.AddNew();
			CommonConsol consol2 = shipment.Consols.AddNew();
			CommonContainer container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "CONTAINER 1";
			container1.PackLines.Add(pack);
			CommonContainer container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = "CONTAINER 2";
			container2.PackLines.Add(pack);

			PackageWrapperFromFreightPackage wrapper = new PackageWrapperFromFreightPackage(pack, Factory);
			wrapper.SetParentConsol(consol1);
			AssertEquals("CONTAINER 1", wrapper.ContainerNo);
			wrapper = new PackageWrapperFromFreightPackage(pack, Factory);
			wrapper.SetParentConsol(consol2);
			AssertEquals("CONTAINER 2", wrapper.ContainerNo);
			wrapper.SetParentConsol(null);
			AssertEquals(pack.JL_Calc_ContainerNum, wrapper.ContainerNo);
		}

		public void TestContainerJobID()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();

			var pack = shipment.OuterPackLines.AddNew();
			var consol1 = shipment.Consols.AddNew();
			var consol2 = shipment.Consols.AddNew();
			var consol3 = shipment.Consols.AddNew();

			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerJobID = "D00001000";
			container1.PackLines.Add(pack);
			var container2 = consol2.Containers.AddNew();
			container2.JC_ContainerJobID = "D00001001";
			container2.PackLines.Add(pack);
			var container3 = consol3.Containers.AddNew();
			container3.JC_ContainerJobID = ZString.Empty;
			container3.PackLines.Add(pack);

			var wrapper = new PackageWrapperFromFreightPackage(pack, Factory);
			wrapper.SetParentConsol(consol1);
			AssertEquals("D00001000", wrapper.ContainerJobID);

			wrapper = new PackageWrapperFromFreightPackage(pack, Factory);
			wrapper.SetParentConsol(consol2);
			AssertEquals("D00001001", wrapper.ContainerJobID);

			wrapper = new PackageWrapperFromFreightPackage(pack, Factory);
			wrapper.SetParentConsol(consol3);
			AssertEquals(pack.JL_Calc_ContainerNum, wrapper.ContainerJobID);

			wrapper.SetParentConsol(null);
			AssertEquals(pack.JL_Calc_ContainerNum, wrapper.ContainerJobID);
		}

		#endregion

		public void TestMarksAndNumberAndDescription()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			PackLine pack = shipment.OuterPackLines.AddNew();

			shipment.JS_GoodsDescription = "JS_GoodsDescription";
			shipment.JS_MarksAndNumbers = "JS_MarksAndNumbers";

			PackageWrapperFromFreightPackage w = new PackageWrapperFromFreightPackage(pack, Factory);
			AssertEquals("JS_GoodsDescription", w.Description);
			AssertEquals("JS_MarksAndNumbers", w.MarksAndNumbers);
		}

		#region Harmonized Codes

		public void TestHarmonizedCode()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "DEHAM";

			var pack = shipment.OuterPackLines.AddNew();
			var nzHC = pack.HarmonisedCodes.AddNew();
			var deHC1 = pack.HarmonisedCodes.AddNew();
			var deHC2 = pack.HarmonisedCodes.AddNew();

			pack.JL_HarmonisedCode = "1111";
			nzHC.JLH_RN_NKCountry = "NZ";
			nzHC.JLH_Code = "2222";
			deHC1.JLH_RN_NKCountry = "DE";
			deHC1.JLH_Code = "3333";
			deHC2.JLH_RN_NKCountry = "DE";
			deHC2.JLH_Code = "4444";

			var wrapper = new PackageWrapperFromFreightPackage(pack, Factory);
			AssertEquals("Precondition: Document direction not set", "", wrapper.DocumentDirection);
			AssertEquals("1111", wrapper.HarmonizedCode);

			wrapper.SetDocumentDirectionForTesting(nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.DEP));
			AssertEquals("2222", wrapper.HarmonizedCode);

			wrapper.SetDocumentDirectionForTesting(nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.ARV));
			AssertEquals("3333, 4444", wrapper.HarmonizedCode);
		}

		#endregion

		public override void TestWrapperMappingsEmpty()
		{
			PackageWrapper wrapperEmpty = (PackageWrapper)GetNewDocumentWrapper();
			AssertEquals("wrapperEmpty.Packages.Value", ZDecimal.Zero, wrapperEmpty.Packages.Value);
			AssertEquals("wrapperEmpty.Packages.Unit.Code", ZString.Empty, wrapperEmpty.Packages.Unit.Code);
			AssertEquals("wrapperEmpty.OutturnPackages.Value", ZDecimal.Zero, wrapperEmpty.OutturnedPackages.Value);
			AssertEquals("wrapperEmpty.OutturnPackages.Unit.Code", ZString.Empty, wrapperEmpty.OutturnedPackages.Unit.Code);
			AssertEquals("wrapperEmpty.PillagedPackages.Value", ZDecimal.Zero, wrapperEmpty.PillagedPackages.Value);
			AssertEquals("wrapperEmpty.PillagedPackages.Unit.Code", ZString.Empty, wrapperEmpty.PillagedPackages.Unit.Code);
			AssertEquals("wrapperEmpty.DamagedPackages.Value", ZDecimal.Zero, wrapperEmpty.DamagedPackages.Value);
			AssertEquals("wrapperEmpty.DamagedPackages.Unit.Code", ZString.Empty, wrapperEmpty.DamagedPackages.Unit.Code);
			AssertEquals("wrapperEmpty.CartonGroupAndSize", ZString.Empty, wrapperEmpty.CartonGroupAndSize);
			AssertEquals("wrapperEmpty.ContainerNo", ZString.Empty, wrapperEmpty.ContainerNo);
			AssertEquals("wrapperEmpty.ContainerJobID", ZString.Empty, wrapperEmpty.ContainerJobID);
			AssertEquals("wrapperEmpty.HouseBill", ZString.Empty, wrapperEmpty.HouseBill);
			AssertEquals("wrapperEmpty.MasterBill", ZString.Empty, wrapperEmpty.MasterBill);
			AssertEquals("wrapperEmpty.UNDGSubstance.UNNumber", 0, wrapperEmpty.UNDGSubstances.Count);
			AssertEquals("wrapperEmpty.Volume.Value", ZDecimal.Zero, wrapperEmpty.Volume.Value);
			AssertEquals("wrapperEmpty.Volume.Unit.Code", "M3", wrapperEmpty.Volume.Unit.Code);
			AssertEquals("wrapperEmpty.Weight.Value", ZDecimal.Zero, wrapperEmpty.Weight.Value);
			AssertEquals("wrapperEmpty.Weight.Unit.Code", "KG", wrapperEmpty.Weight.Unit.Code);
			AssertEquals("wrapperEmpty.OutturnWeight.Value", ZDecimal.Zero, wrapperEmpty.OutturnedWeight.Value);
			AssertEquals("wrapperEmpty.OutturnWeight.Unit.Code", "KG", wrapperEmpty.OutturnedWeight.Unit.Code);
			AssertEquals("wrapperEmpty.Description", ZString.Empty, wrapperEmpty.Description);
			AssertEquals("wrapperEmpty.MarksAndNumbers", ZString.Empty, wrapperEmpty.MarksAndNumbers);
			AssertEquals("wrapperEmpty.Commodity.Code", ZString.Empty, wrapperEmpty.Commodity.Code);
			AssertEquals("wrapperEmpty.Dimensions.Height", ZDecimal.Zero, wrapperEmpty.Dimensions.Height);
			AssertEquals("wrapperEmpty.Dimensions.Width", ZDecimal.Zero, wrapperEmpty.Dimensions.Width);
			AssertEquals("wrapperEmpty.Dimensions.Length", ZDecimal.Zero, wrapperEmpty.Dimensions.Length);
			AssertEquals("wrapperEmpty.Dimensions.Unit.Code", "M", wrapperEmpty.Dimensions.Unit.Code);
			AssertEquals("wrapperEmpty.RefNumber", ZString.Empty, wrapperEmpty.RefNumber);
			AssertEquals("wrapperEmpty.ExportRefNumber", ZString.Empty, wrapperEmpty.ExportRefNumber);
			AssertEquals("wrapperEmpty.ImportRefNumber", ZString.Empty, wrapperEmpty.ImportRefNumber);
			AssertEquals("wrapperEmpty.PackageBarcode", ZString.Empty, wrapperEmpty.PackageBarcode);
			AssertEquals("wrapperEmpty.PackageBarcodeWithOptimisedEncoding", ZString.Empty, wrapperEmpty.PackageBarcodeWithOptimisedEncoding);
			AssertEquals("wrapperEmpty.Parent", null, wrapperEmpty.Parent);
			AssertEquals("wrapperEmpty.LinePrice", ZDecimal.Zero, wrapperEmpty.LinePrice);
			AssertEquals("wrapperEmpty.ItemNumber", ZShort.Zero, wrapperEmpty.ItmNumber);
			AssertEquals("wrapperEmpty.HarmonizedCode", ZString.Empty, wrapperEmpty.HarmonizedCode);
			AssertEquals("wrapperEmpty.Origin", ZString.Empty, wrapperEmpty.Origin.UNLOCO);
			AssertEquals("wrapperEmpty.CustomText1", ZString.Empty, wrapperEmpty.CustomAttribute1);
			AssertEquals("wrapperEmpty.CustomText2", ZString.Empty, wrapperEmpty.CustomAttribute2);
			AssertEquals("wrapperEmpty.CustomText3", ZString.Empty, wrapperEmpty.CustomAttribute3);
			AssertEquals("wrapperEmpty.CustomText4", ZString.Empty, wrapperEmpty.CustomAttribute4);
			AssertEquals("wrapperEmpty.CustomDate1", ZDateTime.Empty, wrapperEmpty.CustomDate1);
			AssertEquals("wrapperEmpty.CustomDate2", ZDateTime.Empty, wrapperEmpty.CustomDate2);
			AssertEquals("wrapperEmpty.CustomDecimal1", ZDecimal.Zero, wrapperEmpty.CustomDecimal1);
			AssertEquals("wrapperEmpty.CustomDecimal2", ZDecimal.Zero, wrapperEmpty.CustomDecimal2);
			AssertEquals("wrapperEmpty.CustomFlag1", false, wrapperEmpty.CustomFlag1);
			AssertEquals("wrapperEmpty.CustomFlag2", false, wrapperEmpty.CustomFlag2);
			AssertEquals("wrapperEmpty.Products", PackProductWrapperCollection.Empty, wrapperEmpty.Products);
			AssertEquals("wrapperEmpty.Indent", "", wrapperEmpty.Indent);
			AssertEquals("wrapperEmpty.Inners", 0, wrapperEmpty.Inners);
			AssertEquals("wrapperEmpty.InnersDetail", "", wrapperEmpty.InnersDetail);
			AssertEquals("wrapperEmpty.PackedItemCount", 0, wrapperEmpty.PackedItemCount);
			AssertEquals("wrapperEmpty.IsExclusive", false, wrapperEmpty.IsExclusive);
			AssertEquals("wrapperEmpty.IsExpiryUsed", false, wrapperEmpty.IsExpiryUsed);
			AssertEquals("wrapperEmpty.IsPackingDateUsed", false, wrapperEmpty.IsPackingDateUsed);
			AssertEquals("wrapperEmpty.IsPartAttrib1Used", false, wrapperEmpty.IsPartAttrib1Used);
			AssertEquals("wrapperEmpty.IsPartAttrib2Used", false, wrapperEmpty.IsPartAttrib2Used);
			AssertEquals("wrapperEmpty.IsPartAttrib3Used", false, wrapperEmpty.IsPartAttrib3Used);
			AssertEquals("wrapperEmpty.IsTrackedSerialUsed", false, wrapperEmpty.IsTrackedSerialUsed);
			AssertEquals("wrapperEmpty.DisplayOrder", "", wrapperEmpty.DisplayOrder);
			AssertEquals("wrapperEmpty.IsTopLevelPackage", true, wrapperEmpty.IsTopLevelPackage);
			AssertEquals("wrapperEmpty.HasSingleProduct", false, wrapperEmpty.HasSingleProduct);
			AssertEquals("wrapperEmpty.HasPackedItem", false, wrapperEmpty.HasPackedItem);
			AssertNull("wrapperEmpty.PackedItem", wrapperEmpty.PackedItem);
			AssertEquals("wrapperEmpty.PostcodeBarcodeNumber", "", wrapperEmpty.PostcodeBarcodeNumber);
			AssertEquals("wrapperEmpty.OutterPackageSequence", ZShort.Zero, wrapperEmpty.OutterPackageSequence);
			AssertEquals("wrapperEmpty.OutterPackagesCount", ZShort.Zero, wrapperEmpty.OutterPackagesCount);
			AssertEquals("wrapperEmpty.PickLocation", ZString.Empty, wrapperEmpty.PickLocation);
			AssertEquals("wrapperEmpty.PickMethod", ZString.Empty, wrapperEmpty.PickMethod);
			AssertEquals("wrapperEmpty.StarTrack_QRCodeText", ZString.Empty, wrapperEmpty.StarTrack_QRCodeText);
		}

		public void TestUNDGTechnicalName()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingContainer container = consol.Containers.AddNew();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			ForwardingPackLine package = shipment.OuterPackLines.AddNew();
			UNDGDataItem dgItem = package.UNDGs.AddNew();
			package.JL_DetailedDescription = "PackLine Name";
			dgItem.DI_TechnicalName = "Tech Name";

			UNDGSubstance dGSubstance = Factory.New<UNDGSubstance>();
			dGSubstance.DG_Code = "9999";
			dGSubstance.DG_UNNO = "9999";
			dgItem.DI_DG = dGSubstance.PK;

			PackageWrapperFromFreightPackage wrapper = new PackageWrapperFromFreightPackage(package, Factory);
			AssertEquals(1, wrapper.UNDGSubstances.Count);
			AssertEquals("Tech Name", wrapper.UNDGSubstances[0].TechnicalName);

			dgItem.DI_TechnicalName = ZString.Empty;
			wrapper = new PackageWrapperFromFreightPackage(package, Factory);
			AssertEquals(1, wrapper.UNDGSubstances.Count);
			AssertEquals("", wrapper.UNDGSubstances[0].TechnicalName);
		}

		public void TestDescription()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_GoodsDescription = "Shipment Description";

			var package1 = shipment.OuterPackLines.AddNew();
			var package2 = shipment.OuterPackLines.AddNew();
			var package3 = shipment.OuterPackLines.AddNew();

			package1.JL_Description = "Pack 1 Short Desc";
			package1.JL_DetailedDescription = "Pack 1 Detailed Desc";

			package2.JL_Description = "Pack 2 Short Desc";

			var wrapper1 = new PackageWrapperFromFreightPackage(package1, Factory);
			AssertEquals("Pack 1 Detailed Desc", wrapper1.Description);

			var wrapper2 = new PackageWrapperFromFreightPackage(package2, Factory);
			AssertEquals("Pack 2 Short Desc", wrapper2.Description);

			var wrapper3 = new PackageWrapperFromFreightPackage(package3, Factory);
			AssertEquals("Shipment Description", wrapper3.Description);
		}

		public void TestReferenceNumberHeaderText()
		{
			AssertReferenceNumberHeaderText(Core.Constants.CountryCodes.Australia, "SGSIN", "AUMEL", "IMPORT REF NUMBER");
			AssertReferenceNumberHeaderText(Core.Constants.CountryCodes.Australia, "AUMEL", "SGSIN", "EXPORT REF NUMBER");
			AssertReferenceNumberHeaderText(Core.Constants.CountryCodes.Australia, "CNXXX", "SGSIN", "EXPORT REF NUMBER");
			AssertReferenceNumberHeaderText(Core.Constants.CountryCodes.Australia, "TWXXX", "SGSIN", "EXPORT REF NUMBER");
			AssertReferenceNumberHeaderText(Core.Constants.CountryCodes.Australia, "HKXXX", "SGSIN", "EXPORT REF NUMBER");

			AssertReferenceNumberHeaderText(Core.Constants.CountryCodes.China, "AUMEL", "SGSIN", "EXPORT REF NUMBER");
			AssertReferenceNumberHeaderText(Core.Constants.CountryCodes.China, "SGSIN", "AUMEL", "EXPORT REF NUMBER");
			AssertReferenceNumberHeaderText(Core.Constants.CountryCodes.China, "AUMEL", "CNSHA", "IMPORT REF NUMBER");
			AssertReferenceNumberHeaderText(Core.Constants.CountryCodes.China, "CNXXX", "AUMEL", "SHIPPING ORDER/SHI LIAN DAN");
			AssertReferenceNumberHeaderText(Core.Constants.CountryCodes.China, "TWXXX", "AUMEL", "SHIPPING ORDER/SHI LIAN DAN");
			AssertReferenceNumberHeaderText(Core.Constants.CountryCodes.China, "HKXXX", "AUMEL", "SHIPPING ORDER/SHI LIAN DAN");

			AssertReferenceNumberHeaderText(Core.Constants.CountryCodes.Taiwan, "AUMEL", "SGSIN", "EXPORT REF NUMBER");
			AssertReferenceNumberHeaderText(Core.Constants.CountryCodes.Taiwan, "SGSIN", "AUMEL", "EXPORT REF NUMBER");
			AssertReferenceNumberHeaderText(Core.Constants.CountryCodes.Taiwan, "AUMEL", "TWXXX", "IMPORT REF NUMBER");
			AssertReferenceNumberHeaderText(Core.Constants.CountryCodes.Taiwan, "CNXXX", "AUMEL", "SHIPPING ORDER/SHI LIAN DAN");
			AssertReferenceNumberHeaderText(Core.Constants.CountryCodes.Taiwan, "TWXXX", "AUMEL", "SHIPPING ORDER/SHI LIAN DAN");
			AssertReferenceNumberHeaderText(Core.Constants.CountryCodes.Taiwan, "HKXXX", "AUMEL", "SHIPPING ORDER/SHI LIAN DAN");

			AssertReferenceNumberHeaderText(Core.Constants.CountryCodes.HongKong, "AUMEL", "SGSIN", "EXPORT REF NUMBER");
			AssertReferenceNumberHeaderText(Core.Constants.CountryCodes.HongKong, "SGSIN", "AUMEL", "EXPORT REF NUMBER");
			AssertReferenceNumberHeaderText(Core.Constants.CountryCodes.HongKong, "AUMEL", "HKXXX", "IMPORT REF NUMBER");
			AssertReferenceNumberHeaderText(Core.Constants.CountryCodes.HongKong, "CNXXX", "AUMEL", "SHIPPING ORDER/SHI LIAN DAN");
			AssertReferenceNumberHeaderText(Core.Constants.CountryCodes.HongKong, "TWXXX", "AUMEL", "SHIPPING ORDER/SHI LIAN DAN");
			AssertReferenceNumberHeaderText(Core.Constants.CountryCodes.HongKong, "HKXXX", "AUMEL", "SHIPPING ORDER/SHI LIAN DAN");
		}

		void AssertReferenceNumberHeaderText(string currentCompanyCountryCode, string origin, string destination, string expectedReferenceNumberHeaderText)
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			var package = shipment.OuterPackLines.AddNew();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(currentCompanyCountryCode))
			{
				var wrapper = new PackageWrapperFromFreightPackage(package, Factory);
				AssertEquals("Reference Number Header Text", expectedReferenceNumberHeaderText, wrapper.PackageReferenceHeaderText);
			}
		}

		public override void TestWrapperMappingFull()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "MASTERCAT";
			ForwardingContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "OOCL0000026";
			container.JC_ContainerJobID = "D00001000";

			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "HOUSECAT";

			ForwardingPackLine package = shipment.OuterPackLines.AddNew();
			package.JL_PackageCount = 23;
			package.JL_Outturn = 56;
			package.JL_Pillaged = 187;
			package.JL_Damaged = 289;
			package.JL_F3_NKPackType = "PKG";
			package.JL_JC = container.PK;
			package.JL_LinePrice = 66;
			package.JL_ItemNo = 2;
			package.JL_EndItemNo = 3;
			package.JL_HarmonisedCode = "CDE";
			package.JL_RN_NKOrigin = "AU";
			package.JL_CustomAttrib1 = "CustomText1";
			package.JL_CustomAttrib2 = "CustomText2";
			package.JL_CustomAttrib3 = "CustomText3";
			package.JL_CustomAttrib4 = "CustomText4";
			package.JL_CustomDate1 = new ZDateTime(2011, 11, 11);
			package.JL_CustomDate2 = new ZDateTime(2012, 12, 12);
			package.JL_CustomDecimal1 = 111m;
			package.JL_CustomDecimal2 = 222m;
			package.JL_CustomFlag1 = true;
			package.JL_CustomFlag2 = false;
			var product1 = package.Products.AddNew();
			product1.D2_ProductCode = "AAA";
			var product2 = package.Products.AddNew();
			product2.D2_ProductCode = "BBB";

			UNDGDataItem dgDataItem = package.UNDGs.AddNew();
			UNDGSubstance subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "123";
			dgDataItem.DI_DG = subs.PK;

			package.JL_Height = 1.11m;
			package.JL_Length = 2.22m;
			package.JL_Width = 3.33m;
			package.JL_UnitOfDimension = "M";
			package.JL_ActualVolume = 200m;
			package.JL_OutturnedVolume = 124.78m;
			package.JL_ActualVolumeUQ = "M3";
			package.JL_ActualWeight = 300m;
			package.JL_OutturnedWeight = 234.56m;
			package.JL_ActualWeightUQ = "LB";
			package.JL_Description = "TEST DESCRIPTION";
			package.JL_MarksAndNumbers = "TEST MANDN";
			package.JL_RH_NKCommodityCode = "HAZ";
			package.JL_OutturnComment = "THIS IS SOME COMMENT FOR THE OUTTURN TO TEST THAT IT IS RETURNED";
			package.JL_RefNumber = "VIN Number";
			package.JL_ExportRefNumber = "Export Ref Number";
			package.JL_ImportRefNumber = "Import Ref Number";

			PackageWrapper wrapperFull = new PackageWrapperFromFreightPackage(package, Factory);
			AssertEquals("wrapperFull.Packages.Value", 23m, wrapperFull.Packages.Value);
			AssertEquals("wrapperFull.Packages.Unit.Code", "PKG", wrapperFull.Packages.Unit.Code);
			AssertEquals("wrapperFull.OutturnPackages.Value", 56m, wrapperFull.OutturnedPackages.Value);
			AssertEquals("wrapperFull.OutturnPackages.Unit.Code", "PKG", wrapperFull.OutturnedPackages.Unit.Code);
			AssertEquals("wrapperFull.PillagedPackages.Value", 187m, wrapperFull.PillagedPackages.Value);
			AssertEquals("wrapperFull.PillagedPackages.Unit.Code", "PKG", wrapperFull.PillagedPackages.Unit.Code);
			AssertEquals("wrapperFull.DamagedPackages.Value", 289m, wrapperFull.DamagedPackages.Value);
			AssertEquals("wrapperFull.DamagedPackages.Unit.Code", "PKG", wrapperFull.DamagedPackages.Unit.Code);
			AssertEquals("wrapperFull.CartonGroupAndSize", ZString.Empty, wrapperFull.CartonGroupAndSize);
			AssertEquals("wrapperFull.ContainerNo", "OOCL0000026", wrapperFull.ContainerNo);
			AssertEquals("wrapperFull.ContainerJobID", "D00001000", wrapperFull.ContainerJobID);
			AssertEquals("wrapperFull.HouseBill", "HOUSECAT", wrapperFull.HouseBill);
			AssertEquals("wrapperFull.MasterBill", "MASTERCAT", wrapperFull.MasterBill);
			AssertEquals("wrapperFull.UNDGSubstances[0].UNNumber", "123", wrapperFull.UNDGSubstances[0].UNNumber);
			AssertEquals("wrapperFull.Volume.Value", 188.733m, wrapperFull.Volume.Value);
			AssertEquals("wrapperFull.Volume.Unit.Code", "M3", wrapperFull.Volume.Unit.Code);
			AssertEquals("wrapperFull.OutturnVolume.Value", 124.78m, wrapperFull.OutturnedVolume.Value);
			AssertEquals("wrapperFull.OutturnVolume.Unit.Code", "M3", wrapperFull.OutturnedVolume.Unit.Code);
			AssertEquals("wrapperFull.Weight.Value", 300.0m, wrapperFull.Weight.Value);
			AssertEquals("wrapperFull.Weight.Unit.Code", "LB", wrapperFull.Weight.Unit.Code);
			AssertEquals("wrapperFull.OutturnWeight.Value", 234.56m, wrapperFull.OutturnedWeight.Value);
			AssertEquals("wrapperFull.OutturnWeight.Unit.Code", "LB", wrapperFull.OutturnedWeight.Unit.Code);
			AssertEquals("wrapperFull.Description", "TEST DESCRIPTION", wrapperFull.Description);
			AssertEquals("wrapperFull.MarksAndNumbers", "TEST MANDN", wrapperFull.MarksAndNumbers);
			AssertEquals("wrapperFull.Commodity.Description", "HAZARDOUS GOODS", wrapperFull.Commodity.Description);
			AssertEquals("wrapperFull.OutturnComment", "THIS IS SOME COMMENT FOR THE OUTTURN TO TEST THAT IT IS RETURNED", wrapperFull.OutturnComment);
			AssertEquals("wrapperFull.Height", 1.11m, wrapperFull.Dimensions.Height);
			AssertEquals("wrapperFull.Length", 2.22m, wrapperFull.Dimensions.Length);
			AssertEquals("wrapperFull.Width", 3.33m, wrapperFull.Dimensions.Width);
			AssertEquals("wrapperFull.Dimension.Unit.Code", "M", wrapperFull.Dimensions.Unit.Code);
			AssertEquals("wrapperFull.RefNumber", "VIN Number", wrapperFull.RefNumber);
			AssertEquals("wrapperFull.ExportRefNumber", "Export Ref Number", wrapperFull.ExportRefNumber);
			AssertEquals("wrapperFull.ImportRefNumber", "Import Ref Number", wrapperFull.ImportRefNumber);
			AssertEquals("wrapperFull.Parent", "HOUSECAT", wrapperFull.Parent.HouseBill);
			AssertEquals("wrapperFull.LinePrice", 66m, wrapperFull.LinePrice);
			AssertEquals("wrapperFull.ItemNumber", (ZShort)2, wrapperFull.ItmNumber);
			AssertEquals("wrapperFull.HarmonizedCode", "CDE", wrapperFull.HarmonizedCode);
			AssertEquals("wrapperFull.Origin", "AU", wrapperFull.Origin.UNLOCO);
			AssertEquals("wrapperFull.CustomText1", "CustomText1", wrapperFull.CustomAttribute1);
			AssertEquals("wrapperFull.CustomText2", "CustomText2", wrapperFull.CustomAttribute2);
			AssertEquals("wrapperFull.CustomText3", "CustomText3", wrapperFull.CustomAttribute3);
			AssertEquals("wrapperFull.CustomText4", "CustomText4", wrapperFull.CustomAttribute4);
			AssertEquals("wrapperFull.CustomDate1", new ZDateTime(2011, 11, 11), wrapperFull.CustomDate1);
			AssertEquals("wrapperFull.CustomDate2", new ZDateTime(2012, 12, 12), wrapperFull.CustomDate2);
			AssertEquals("wrapperFull.CustomDecimal1", 111m, wrapperFull.CustomDecimal1);
			AssertEquals("wrapperFull.CustomDecimal2", 222m, wrapperFull.CustomDecimal2);
			AssertEquals("wrapperFull.CustomFlag1", true, wrapperFull.CustomFlag1);
			AssertEquals("wrapperFull.CustomFlag2", false, wrapperFull.CustomFlag2);
			AssertEquals("wrapperFull.Products", 2, wrapperFull.Products.Count);
			AssertEquals("wrapperFull.Products", "AAA", wrapperFull.Products[0].ProductCode);
			AssertEquals("wrapperFull.Products", "BBB", wrapperFull.Products[1].ProductCode);
			AssertEquals("wrapperFull.Indent", "", wrapperFull.Indent);
			AssertEquals("wrapperFull.Inners", 0, wrapperFull.Inners);
			AssertEquals("wrapperFull.InnersDetail", "", wrapperFull.InnersDetail);
			AssertEquals("wrapperFull.PackedItemCount", 0, wrapperFull.PackedItemCount);
			AssertEquals("wrapperFull.IsExclusive", false, wrapperFull.IsExclusive);
			AssertEquals("wrapperFull.IsExpiryUsed", false, wrapperFull.IsExpiryUsed);
			AssertEquals("wrapperFull.IsPackingDateUsed", false, wrapperFull.IsPackingDateUsed);
			AssertEquals("wrapperFull.IsPartAttrib1Used", false, wrapperFull.IsPartAttrib1Used);
			AssertEquals("wrapperFull.IsPartAttrib2Used", false, wrapperFull.IsPartAttrib2Used);
			AssertEquals("wrapperFull.IsPartAttrib3Used", false, wrapperFull.IsPartAttrib3Used);
			AssertEquals("wrapperFull.IsTrackedSerialUsed", false, wrapperFull.IsTrackedSerialUsed);
			AssertEquals("wrapperFull.DisplayOrder", "", wrapperFull.DisplayOrder);
			AssertEquals("wrapperFull.IsTopLevelPackage", true, wrapperFull.IsTopLevelPackage);
			AssertEquals("wrapperFull.HasSingleProduct", false, wrapperFull.HasSingleProduct);
			AssertEquals("wrapperFull.HasPackedItem", false, wrapperFull.HasPackedItem);
			AssertNull("wrapperFull.PackedItem", wrapperFull.PackedItem);
			AssertEquals("wrapperFull.FreightPackLine", package, wrapperFull.FreightPackLine);
			AssertEquals("wrapperFull.OutterPackageSequence", ZShort.Zero, wrapperFull.OutterPackageSequence);
			AssertEquals("wrapperFull.OutterPackagesCount", ZShort.Zero, wrapperFull.OutterPackagesCount);
			AssertEquals("wrapperFull.PickLocation", ZString.Empty, wrapperFull.PickLocation);
			AssertEquals("wrapperFull.PickMethod", ZString.Empty, wrapperFull.PickMethod);
		}

		public void TestWeightVolumeDecimalPlaces()
		{
			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfDecimals_SeaWeight = collection.AddNew();
			defaultNumberOfDecimals_SeaWeight.TransportMode = Core.Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaWeight.UnitOfMeasure = Core.Constants.Weight.Kilograms;
			defaultNumberOfDecimals_SeaWeight.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaWeight.RoundingMode = RoundingModes.Up;
			var defaultNumberOfDecimals_SeaVolume = collection.AddNew();
			defaultNumberOfDecimals_SeaVolume.TransportMode = Core.Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaVolume.UnitOfMeasure = Core.Constants.Volume.CubicMetres;
			defaultNumberOfDecimals_SeaVolume.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaVolume.RoundingMode = RoundingModes.Down;

			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			var package = shipment.OuterPackLines.AddNew();
			package.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			package.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			package.JL_UnitOfDimension = Core.Constants.Length.Metres;
			package.JL_PackageCount = 1;
			package.JL_ActualWeight = 315.264m;
			package.JL_OutturnedWeight = 234.564m;
			package.JL_OutturnedVolume = 124.568m;
			package.JL_Length = 2.25m;
			package.JL_Width = 1.32m;
			package.JL_Height = 0.58m;

			var wrapper = new PackageWrapperFromFreightPackage(package, Factory);

			AssertEquals(315.27m, wrapper.Weight.Value);
			AssertEquals("KG", wrapper.Weight.Unit.Code);
			AssertEquals("315.27 KG", wrapper.Weight.ValueAndUnitCode);

			AssertEquals(234.57m, wrapper.OutturnedWeight.Value);
			AssertEquals("KG", wrapper.OutturnedWeight.Unit.Code);
			AssertEquals("234.57 KG", wrapper.OutturnedWeight.ValueAndUnitCode);

			AssertEquals(1.72m, wrapper.Volume.Value);
			AssertEquals("M3", wrapper.Volume.Unit.Code);
			AssertEquals("1.72 M3", wrapper.Volume.ValueAndUnitCode);

			AssertEquals(124.56m, wrapper.OutturnedVolume.Value);
			AssertEquals("M3", wrapper.OutturnedVolume.Unit.Code);
			AssertEquals("124.56 M3", wrapper.OutturnedVolume.ValueAndUnitCode);
		}

		public void TestDocManagerBarCodeWithUniqueID()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var package = shipment.OuterPackLines.AddNew();

			var wrapper = new PackageWrapperFromFreightPackage(package, Factory);

			wrapper.PackageNumber = 3;
			AssertEquals("BarcodeTextWithUniqueID", "EDIDAT-00003", wrapper.BarcodeTextWithUniqueID);

			wrapper.PackageNumber = 333;
			AssertEquals("BarcodeTextWithUniqueID", "EDIDAT-00333", wrapper.BarcodeTextWithUniqueID);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new PackageWrapperFromFreightPackage(null, Factory);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Commodity : HAZ - HAZARDOUS GOODS
CommonCurrency : 
Container : 
DamagedPackages : 289 PKG
DamagedReason : 
Dimensions : 2 x 1 x 0.5 M
FumigatedPackages : 
HandlingUnit :  is null
HeatTreatedPackages : 
ISPMPalletPackages : 
MostRecentAudit :  is null
NonStackablePackages : 
Origin : 
OutturnedPackages : 56 PKG
OutturnedVolume : 124.780 M3
OutturnedWeight : 234.560 LB
PackageOrderReference : 
Packages : 23 PKG
PackageState :  is null
PackedItem :  is null
Parent : 
PillagedPackages : 187 PKG
Registry : (No Default Field Value Available on Registry)
TopLevelHandlingUnit :  is null
TopLoadOnlyPackages : 
UOMType : 
Volume : 23.000 M3
Weight : 300.000 LB
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingContainer container = consol.Containers.AddNew();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			ForwardingPackLine package = shipment.OuterPackLines.AddNew();
			package.JL_PackageCount = 23;
			package.JL_Outturn = 56;
			package.JL_Pillaged = 187;
			package.JL_Damaged = 289;
			package.JL_F3_NKPackType = "PKG";
			package.JL_JC = container.PK;

			UNDGDataItem dgDataItem = package.UNDGs.AddNew();
			UNDGSubstance subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "123";
			dgDataItem.DI_DG = subs.PK;

			package.JL_OutturnedVolume = 124.78m;
			package.JL_ActualVolumeUQ = "M3";
			package.JL_ActualWeight = 300m;
			package.JL_OutturnedWeight = 234.56m;
			package.JL_ActualWeightUQ = "LB";
			package.JL_RH_NKCommodityCode = "HAZ";

			package.JL_Length = 2m;
			package.JL_Width = 1m;
			package.JL_Height = 0.5m;
			package.JL_UnitOfDimension = "M";

			return new PackageWrapperFromFreightPackage(package, Factory);
		}
	}
}
