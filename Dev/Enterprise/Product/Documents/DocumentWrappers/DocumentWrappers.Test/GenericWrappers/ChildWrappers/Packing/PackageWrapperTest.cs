using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	public abstract class PackageWrapperTest : Base.Testing.GenericWrapperTest
	{
		public abstract void TestWrapperMappingFull();

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Package                                      (Default Field: Packages)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Commodity                               CodeAndDescription
DamagedReason                           CodeAndDescription
UOMType                                 CodeAndDescription
Container                               Container
CommonCurrency                          Currency
Dimensions                              Dimensions
Parent                                  Freight
Origin                                  Location
HandlingUnit                            Package
TopLevelHandlingUnit                    Package
MostRecentAudit                         PackageAuditWrapper
PackageOrderReference                   PackageOrderReferenceWrapper
PackageState                            PackageState
PackedItem                              PackedItem
DamagedPackages                         PackQTY
FumigatedPackages                       PackQTY
HeatTreatedPackages                     PackQTY
ISPMPalletPackages                      PackQTY
NonStackablePackages                    PackQTY
OutturnedPackages                       PackQTY
Packages                                PackQTY
PillagedPackages                        PackQTY
TopLoadOnlyPackages                     PackQTY
OutturnedVolume                         Volume
Volume                                  Volume
OutturnedWeight                         Weight
Weight                                  Weight
AreaName                                String
CartonGroupAndSize                      String
ContainerJobID                          String
ContainerNo                             String
CustomAttribute1                        String
CustomAttribute2                        String
CustomAttribute3                        String
CustomAttribute4                        String
CustomDate1                             DateTime
CustomDate2                             DateTime
CustomDecimal1                          Decimal
CustomDecimal2                          Decimal
CustomFlag1                             Bool
CustomFlag2                             Bool
Description                             String
DisplayOrder                            String
ExportRefNumber                         String
HarmonizedCode                          String
HasPackedItem                           Bool
HasSingleAttribute1                     Bool
HasSingleAttribute2                     Bool
HasSingleAttribute3                     Bool
HasSingleExpiryDate                     Bool
HasSinglePackingDate                    Bool
HasSingleProduct                        Bool
HouseBill                               String
ImportRefNumber                         String
Indent                                  String
InnerPackagesCount                      Short
Inners                                  Int
InnersDetail                            String
IsExclusive                             Bool
IsExpiryUsed                            Bool
IsOuterPackage                          Bool
IsPackageIdValidSSCCBarCode             Bool
IsPackingDateUsed                       Bool
IsPartAttrib1Used                       Bool
IsPartAttrib2Used                       Bool
IsPartAttrib3Used                       Bool
IsTopLevelNonContainerisedPackage       Bool
IsTopLevelPackage                       Bool
IsTrackedSerialUsed                     Bool
ItmNumber                               Short
LinePrice                               Decimal
MarksAndNumbers                         String
MasterBill                              String
NMFC                                    String
OutterPackagesCount                     Short
OutterPackageSequence                   Short
OutturnComment                          String
PackageBarcode                          String
PackageBarcodeWithOptimisedEncoding     String
PackageBarcodeWithSSCCPrefix            String
PackageBarcodeWithSSCCPrefixNonOptimisedEncoding  String
PackageNumber                           Int
PackageReferenceHeaderText              String
PackageTemperatures                     String
PackedItemCount                         Int
PackingOrder                            Int
PackLineId                              String
PickGroup                               String
PickLocation                            String
PickMethod                              String
PostcodeBarcode                         String
PostcodeBarcodeNumber                   String
PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefixBarcodeText  String
PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefix  String
PostcodeBarcodeNumberWithPrefix         String
PostCodeISONumericCountryCodeAndConsigneeAddressShortCodeBarcodeText  String
RefNumber                               String
RefNumberWithSSCCPrefix                 String
Seals                                   String
StarTrack_QRCodeText                    String
CustomsEntries                          CustomsEntry Collection
HarmonizedCodes                         HarmonisedCode Collection
FirstLevelPackedPackages                Package Collection
InnerPackages                           Package Collection
PackedPackages                          Package Collection
PackedItems                             PackedItem Collection
Products                                PackProductWrapper Collection
UNDGSubstances                          UNDGSubstance Collection

";
			}
		}

		#region TestHasSingleAttribute

		public void TestHasSingleProduct()
		{
			AssertEquals(false, Wrapper.HasSingleProduct);
		}

		public void TestHasSingleAttribute1()
		{
			AssertEquals(false, Wrapper.HasSingleAttribute1);
		}

		public void TestHasSingleAttribute2()
		{
			AssertEquals(false, Wrapper.HasSingleAttribute2);
		}

		public void TestHasSingleAttribute3()
		{
			AssertEquals(false, Wrapper.HasSingleAttribute3);
		}

		public void TestHasSingleExpiryDate()
		{
			AssertEquals(false, Wrapper.HasSingleExpiryDate);
		}

		public void TestHasSinglePackingDate()
		{
			AssertEquals(false, Wrapper.HasSinglePackingDate);
		}

		#endregion

		#region TestIdentifier

		public void TestIdentifier()
		{
			PackageWrapperFromFreightPackage wrapper = new PackageWrapperFromFreightPackage(null, Factory);
			AssertEquals("Identifier for null is the generated ZGuid", false, wrapper.Identifier.IsEmpty);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ForwardingPackLine package = shipment.OuterPackLines.AddNew();

			wrapper = new PackageWrapperFromFreightPackage(package, Factory);
			AssertEquals("Identifier is the wrapped package's PK", package.PK, wrapper.Identifier);
		}

		#endregion

		#region TestIsPackageIdValidSSCCBarCode

		public void TestIsPackageIdValidSSCCBarCode()
		{
			AssertEquals(false, Wrapper.IsPackageIdValidSSCCBarCode);

			var package = Factory.New<PkgPackage>();
			var wrapper = new PackageWrapperFromPkgPackage(package, Factory);
			package.KP_PackageID = "123465454654654659";
			AssertEquals(true, wrapper.IsPackageIdValidSSCCBarCode);
		}

		#endregion

		#region TestRefNumberWithSSCCPrefix

		public void TestRefNumberWithSSCCPrefix()
		{
			AssertEquals("(00)", Wrapper.RefNumberWithSSCCPrefix);

			var package = Factory.New<PkgPackage>();
			var wrapper = new PackageWrapperFromPkgPackage(package, Factory);
			package.KP_PackageID = "abc";
			AssertEquals("(00)abc", wrapper.RefNumberWithSSCCPrefix);
		}

		#endregion

		#region TestPackageBarcodeWithSSCCPrefix

		public virtual void TestPackageBarcodeWithSSCCPrefix()
		{
			AssertEquals("ÈÆ00pÊ", Wrapper.PackageBarcodeWithSSCCPrefix);

			var package = Factory.New<PkgPackage>();
			var wrapper = new PackageWrapperFromPkgPackage(package, Factory);

			package.KP_PackageID = "ab";
			AssertEquals("ÈÆ00abTÊ", wrapper.PackageBarcodeWithSSCCPrefix);

			package.KP_PackageID = "12";
			AssertEquals("ÉÆ¯,EÊ", wrapper.PackageBarcodeWithSSCCPrefix); // numbers should be compressed (using the optimisation flag)
		}

		#endregion

		#region TestPostcodeBarcode

		public void TestPostcodeBarcode()
		{
			AssertEquals("", Wrapper.PostcodeBarcode);
		}

		#endregion

		#region TestPostcodeBarcodeNumber

		public void TestPostcodeBarcodeNumber()
		{
			AssertEquals("", Wrapper.PostcodeBarcodeNumber);
		}

		#endregion

		#region TestPickLocation

		public void TestPickLocation()
		{
			AssertEquals("", Wrapper.PickLocation);
		}

		#endregion

		#region TestPickMethod

		public void TestPickMethod()
		{
			AssertEquals("", Wrapper.PickMethod);
		}

		#endregion

		#region TestPickGroup

		public void TestPickGroup()
		{
			AssertEquals(ZString.Empty, Wrapper.PickGroup);
		}

		#endregion

		#region TestAreaName

		public void TestAreaName()
		{
			AssertEquals(ZString.Empty, Wrapper.AreaName);
		}

		#endregion

		#region TestCommonCurrency

		public void TestCommonCurrency()
		{
			CommonCurrencyTestCore();
		}

		protected virtual void CommonCurrencyTestCore()
		{
			AssertEquals(ZString.Empty, Wrapper.CommonCurrency.Code);
		}

		#endregion

		#region TestPostcodeBarcodeNumberWithPrefix

		public void TestPostcodeBarcodeNumberWithPrefix()
		{
			AssertEquals("", Wrapper.PostcodeBarcodeNumberWithPrefix);
			AssertEquals("", Wrapper.PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefix);
			AssertEquals("", Wrapper.PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefixBarcodeText);
			AssertEquals("", Wrapper.PostCodeISONumericCountryCodeAndConsigneeAddressShortCodeBarcodeText);
		}

		#endregion

		#region TestPostCodeISONumericCountryCodeAndConsigneeAddressShortCodeBarcodeTextWithConsigneeAddress

		public void TestPostCodeISONumericCountryCodeAndConsigneeAddressShortCodeBarcodeTextWithConsigneeAddress()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			consignee.MainAddress.OA_PostCode = "2213";

			data.Org1.MainAddress.OA_Code = "#1";
			data.Org1.MainAddress.OA_RN_NKCountryCode = "";
			data.Whs1.WW_OA_WarehouseAddress = consignee.MainAddress.PK;

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, 1, Core.Constants.PkgUnit.Box, "B1");
			package.KP_PackageID = "TI1";

			var wrapper = new PackageWrapperFromPkgPackage(package, Factory);
			AssertEquals("ÈÆ(90)#1<Ê", wrapper.PostCodeISONumericCountryCodeAndConsigneeAddressShortCodeBarcodeText);
		}

		#endregion

		#region TestStarTrack_QRCodeText

		public void TestStarTrack_QRCodeText()
		{
			AssertEquals("", Wrapper.StarTrack_QRCodeText);
		}

		#endregion

		#region TestCustomsEntries

		public void TestCustomsEntries() => TestCustomsEntriesCore();

		protected virtual void TestCustomsEntriesCore()
		{
			AssertNotNull(Wrapper.CustomsEntries);
			AssertEquals(0, Wrapper.CustomsEntries.Count);
		}

		#endregion

		#region TestPackagedPackages

		public void TestPackagedPackages()
		{
			AssertNotNull(Wrapper.PackedPackages);
			AssertEquals(0, Wrapper.PackedPackages.Count);
		}

		#endregion

		#region TestPackagedPackages

		public void TestFirstLevelPackedPackages()
		{
			AssertNotNull(Wrapper.FirstLevelPackedPackages);
			AssertEquals(0, Wrapper.FirstLevelPackedPackages.Count);
		}

		#endregion

		#region TestUOMTypes

		public void TestUOMType()
		{
			AssertEquals("", Wrapper.UOMType.Code);
			AssertEquals("", Wrapper.UOMType.Description);
		}

		#endregion

		#region Implementation

		protected new PackageWrapper Wrapper
		{
			get { return (PackageWrapper)base.Wrapper; }
		}

		#region PackingHelper

		protected PackingTestHelper PackingHelper
		{
			get { return packingHelper ?? (packingHelper = new PackingTestHelper(Factory)); }
		}

		PackingTestHelper packingHelper;

		#endregion

		#endregion
	}
}
