using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PackageWrapperFromOneOffContainer))]
	sealed class PackageWrapperFromOneOffContainerTest : PackageWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			var commodity = Factory.LoadTop1<RefCommodityCode>(new ZQuery());

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);

			var pack = quote.CurrentOneOffQuote.LooseCargo.AddNew();
			quote.CurrentOneOffQuote.TT_RH_NKCommodity = commodity.RH_Code;

			Factory.Save();

			PackageWrapper wrapper = new PackageWrapperFromOneOffContainer(pack, Factory);

			CombineAssertions(delegate
			{
				AssertEquals("wrapper.Packages.Value", 0m, wrapper.Packages.Value);
				AssertEquals("wrapper.Packages.Unit.Code", Constants.PkgUnit.Pallet, wrapper.Packages.Unit.Code);
				AssertEquals("wrapper.OutturnPackages.Value", 0m, wrapper.OutturnedPackages.Value);
				AssertEquals("wrapper.OutturnPackages.Unit.Code", Constants.PkgUnit.Pallet, wrapper.OutturnedPackages.Unit.Code);
				AssertEquals("wrapper.PillagedPackages.Value", 0m, wrapper.PillagedPackages.Value);
				AssertEquals("wrapper.PillagedPackages.Unit.Code", Constants.PkgUnit.Pallet, wrapper.PillagedPackages.Unit.Code);
				AssertEquals("wrapper.DamagedPackages.Value", 0m, wrapper.DamagedPackages.Value);
				AssertEquals("wrapper.DamagedPackages.Unit.Code", Constants.PkgUnit.Pallet, wrapper.DamagedPackages.Unit.Code);
				AssertEquals("wrapper.CartonGroupAndSize", ZString.Empty, wrapper.CartonGroupAndSize);
				AssertEquals("wrapper.ContainerNo", "", wrapper.ContainerNo);
				AssertEquals("wrapper.ContainerJobID", "", wrapper.ContainerJobID);
				AssertEquals("wrapper.HouseBill", "", wrapper.HouseBill);
				AssertEquals("wrapper.MasterBill", "", wrapper.MasterBill);
				AssertEquals("wrapper.UNDGSubstances.Count", 0, wrapper.UNDGSubstances.Count);
				AssertEquals("wrapper.Volume.Value", 0m, wrapper.Volume.Value);
				AssertEquals("wrapper.Volume.Unit.Code", Constants.Volume.CubicMetres, wrapper.Volume.Unit.Code);
				AssertEquals("wrapper.OutturnVolume.Value", 0m, wrapper.OutturnedVolume.Value);
				AssertEquals("wrapper.OutturnVolume.Unit.Code", Constants.Volume.CubicMetres, wrapper.OutturnedVolume.Unit.Code);
				AssertEquals("wrapper.Weight.Value", 0m, wrapper.Weight.Value);
				AssertEquals("wrapper.Weight.Unit.Code", Constants.Weight.Kilograms, wrapper.Weight.Unit.Code);
				AssertEquals("wrapper.OutturnWeight.Value", 0m, wrapper.OutturnedWeight.Value);
				AssertEquals("wrapper.OutturnWeight.Unit.Code", Constants.Weight.Kilograms, wrapper.OutturnedWeight.Unit.Code);
				AssertEquals("wrapper.Description", "", wrapper.Description);
				AssertEquals("wrapper.MarksAndNumbers", "", wrapper.MarksAndNumbers);
				AssertEquals("wrapper.Commodity.Description", commodity.RH_Description, wrapper.Commodity.Description);
				AssertEquals("wrapper.OutturnComment", "", wrapper.OutturnComment);
				AssertEquals("wrapper.Height", 0m, wrapper.Dimensions.Height);
				AssertEquals("wrapper.Length", 0m, wrapper.Dimensions.Length);
				AssertEquals("wrapper.Width", 0m, wrapper.Dimensions.Width);
				AssertEquals("wrapper.DimensionUnit", Constants.Length.Metres, wrapper.Dimensions.Unit.Code);
				AssertEquals("wrapper.Parent", "", wrapper.Parent.GoodsDescription);
				AssertEquals("wrapper.LinePrice", 0m, wrapper.LinePrice);
				AssertEquals("wrapper.ItemNumber", (short)0, wrapper.ItmNumber);
				AssertEquals("wrapper.HarmonizedCode", "", wrapper.HarmonizedCode);
				AssertEquals("wrapper.Origin", "", wrapper.Origin.UNLOCO);
				AssertEquals("wrapper.CustomText1", "", wrapper.CustomAttribute1);
				AssertEquals("wrapper.CustomText2", "", wrapper.CustomAttribute2);
				AssertEquals("wrapper.CustomText3", "", wrapper.CustomAttribute3);
				AssertEquals("wrapper.CustomText4", "", wrapper.CustomAttribute4);
				AssertEquals("wrapper.CustomDate1", ZDateTime.Empty, wrapper.CustomDate1);
				AssertEquals("wrapper.CustomDate2", ZDateTime.Empty, wrapper.CustomDate2);
				AssertEquals("wrapper.CustomDecimal1", 0m, wrapper.CustomDecimal1);
				AssertEquals("wrapper.CustomDecimal2", 0m, wrapper.CustomDecimal2);
				AssertEquals("wrapper.CustomFlag1", false, wrapper.CustomFlag1);
				AssertEquals("wrapper.CustomFlag2", false, wrapper.CustomFlag2);
				AssertEquals("wrapper.Products.Count", 0, wrapper.Products.Count);
				AssertEquals("wrapper.RefNumber", ZString.Empty, wrapper.RefNumber);
				AssertEquals("wrapper.ExportRefNumber", ZString.Empty, wrapper.ExportRefNumber);
				AssertEquals("wrapper.ImportRefNumber", ZString.Empty, wrapper.ImportRefNumber);
				AssertEquals("wrapper.PackageBarcode", ZString.Empty, wrapper.PackageBarcode);
				AssertEquals("wrapper.PackageBarcodeWithOptimisedEncoding", ZString.Empty, wrapper.PackageBarcodeWithOptimisedEncoding);
				AssertEquals("wrapper.Indent", "", wrapper.Indent);
				AssertEquals("wrapper.Inners", 0, wrapper.Inners);
				AssertEquals("wrapper.InnersDetail", "", wrapper.InnersDetail);
				AssertEquals("wrapper.PackedItemCount", 0, wrapper.PackedItemCount);
				AssertEquals("wrapper.IsExclusive", false, wrapper.IsExclusive);
				AssertEquals("wrapper.IsExpiryUsed", false, wrapper.IsExpiryUsed);
				AssertEquals("wrapper.IsPackingDateUsed", false, wrapper.IsPackingDateUsed);
				AssertEquals("wrapper.IsPartAttrib1Used", false, wrapper.IsPartAttrib1Used);
				AssertEquals("wrapper.IsPartAttrib2Used", false, wrapper.IsPartAttrib2Used);
				AssertEquals("wrapper.IsPartAttrib3Used", false, wrapper.IsPartAttrib3Used);
				AssertEquals("wrapper.IsTrackedSerialUsed", false, wrapper.IsTrackedSerialUsed);
				AssertEquals("wrapper.DisplayOrder", "", wrapper.DisplayOrder);
				AssertEquals("wrapper.IsTopLevelPackage", true, wrapper.IsTopLevelPackage);
				AssertEquals("wrapper.HasSingleProduct", false, wrapper.HasSingleProduct);
				AssertEquals("wrapper.HasPackedItem", false, wrapper.HasPackedItem);
				AssertNull("wrapper.PackedItem", wrapper.PackedItem);
				AssertEquals("wrapper.PostcodeBarcodeNumber", "", wrapper.PostcodeBarcodeNumber);
				AssertEquals("wrapper.OutterPackageSequence", ZShort.Zero, wrapper.OutterPackageSequence);
				AssertEquals("wrapper.OutterPackagesCount", ZShort.Zero, wrapper.OutterPackagesCount);
				AssertEquals("wrapper.PickLocation", ZString.Empty, wrapper.PickLocation);
				AssertEquals("wrapper.PickMethod", ZString.Empty, wrapper.PickMethod);
			});
		}

		public override void TestWrapperMappingFull()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var pack = quote.CurrentOneOffQuote.LooseCargo.AddNew();
			pack.TPL_PackLineCount = 5;
			pack.TPL_F3_NKPackType = Constants.PkgUnit.Pallet;
			pack.TPL_Weight = 15000;
			pack.TPL_Volume = 120;
			pack.TPL_Length = 4;
			pack.TPL_Width = 3;
			pack.TPL_Height = 2;
			pack.TPL_WeightUQ = Constants.Weight.Kilograms;
			pack.TPL_VolumeUQ = Constants.Volume.CubicMetres;
			pack.TPL_DimensionUQ = Constants.Length.Metres;

			Factory.Save();

			PackageWrapper wrapper = new PackageWrapperFromOneOffContainer(pack, Factory);

			CombineAssertions(delegate
			{
				AssertEquals("wrapper.Packages.Value", 5m, wrapper.Packages.Value);
				AssertEquals("wrapper.Packages.Unit.Code", Constants.PkgUnit.Pallet, wrapper.Packages.Unit.Code);
				AssertEquals("wrapper.OutturnPackages.Value", 0m, wrapper.OutturnedPackages.Value);
				AssertEquals("wrapper.OutturnPackages.Unit.Code", Constants.PkgUnit.Pallet, wrapper.OutturnedPackages.Unit.Code);
				AssertEquals("wrapper.PillagedPackages.Value", 0m, wrapper.PillagedPackages.Value);
				AssertEquals("wrapper.PillagedPackages.Unit.Code", Constants.PkgUnit.Pallet, wrapper.PillagedPackages.Unit.Code);
				AssertEquals("wrapper.DamagedPackages.Value", 0m, wrapper.DamagedPackages.Value);
				AssertEquals("wrapper.DamagedPackages.Unit.Code", Constants.PkgUnit.Pallet, wrapper.DamagedPackages.Unit.Code);
				AssertEquals("wrapper.CartonGroupAndSize", ZString.Empty, wrapper.CartonGroupAndSize);
				AssertEquals("wrapper.ContainerNo", "", wrapper.ContainerNo);
				AssertEquals("wrapper.ContainerJobID", "", wrapper.ContainerJobID);
				AssertEquals("wrapper.HouseBill", "", wrapper.HouseBill);
				AssertEquals("wrapper.MasterBill", "", wrapper.MasterBill);
				AssertEquals("wrapper.UNDGSubstances.Count", 0, wrapper.UNDGSubstances.Count);
				AssertEquals("wrapper.Volume.Value", 120m, wrapper.Volume.Value);
				AssertEquals("wrapper.Volume.Unit.Code", Constants.Volume.CubicMetres, wrapper.Volume.Unit.Code);
				AssertEquals("wrapper.OutturnVolume.Value", 0m, wrapper.OutturnedVolume.Value);
				AssertEquals("wrapper.OutturnVolume.Unit.Code", Constants.Volume.CubicMetres, wrapper.OutturnedVolume.Unit.Code);
				AssertEquals("wrapper.Weight.Value", 15000m, wrapper.Weight.Value);
				AssertEquals("wrapper.Weight.Unit.Code", Constants.Weight.Kilograms, wrapper.Weight.Unit.Code);
				AssertEquals("wrapper.OutturnWeight.Value", 0m, wrapper.OutturnedWeight.Value);
				AssertEquals("wrapper.OutturnWeight.Unit.Code", Constants.Weight.Kilograms, wrapper.OutturnedWeight.Unit.Code);
				AssertEquals("wrapper.Description", "", wrapper.Description);
				AssertEquals("wrapper.MarksAndNumbers", "", wrapper.MarksAndNumbers);
				AssertEquals("wrapper.Commodity.Description", "", wrapper.Commodity.Description);
				AssertEquals("wrapper.OutturnComment", "", wrapper.OutturnComment);
				AssertEquals("wrapper.Height", 2m, wrapper.Dimensions.Height);
				AssertEquals("wrapper.Length", 4m, wrapper.Dimensions.Length);
				AssertEquals("wrapper.Width", 3m, wrapper.Dimensions.Width);
				AssertEquals("wrapper.DimensionUnit", Constants.Length.Metres, wrapper.Dimensions.Unit.Code);
				AssertEquals("wrapper.Parent", "", wrapper.Parent.GoodsDescription);
				AssertEquals("wrapper.LinePrice", 0m, wrapper.LinePrice);
				AssertEquals("wrapper.ItemNumber", (short)0, wrapper.ItmNumber);
				AssertEquals("wrapper.HarmonizedCode", ZString.Empty, wrapper.HarmonizedCode);
				AssertEquals("wrapper.Origin", ZString.Empty, wrapper.Origin.UNLOCO);
				AssertEquals("wrapper.CustomText1", ZString.Empty, wrapper.CustomAttribute1);
				AssertEquals("wrapper.CustomText2", ZString.Empty, wrapper.CustomAttribute2);
				AssertEquals("wrapper.CustomText3", ZString.Empty, wrapper.CustomAttribute3);
				AssertEquals("wrapper.CustomText4", ZString.Empty, wrapper.CustomAttribute4);
				AssertEquals("wrapper.CustomDate1", ZDateTime.Empty, wrapper.CustomDate1);
				AssertEquals("wrapper.CustomDate2", ZDateTime.Empty, wrapper.CustomDate2);
				AssertEquals("wrapper.CustomDecimal1", 0m, wrapper.CustomDecimal1);
				AssertEquals("wrapper.CustomDecimal2", 0m, wrapper.CustomDecimal2);
				AssertEquals("wrapper.CustomFlag1", false, wrapper.CustomFlag1);
				AssertEquals("wrapper.CustomFlag2", false, wrapper.CustomFlag2);
				AssertEquals("wrapper.Products.Count", 0, wrapper.Products.Count);
				AssertEquals("wrapper.Indent", "", wrapper.Indent);
				AssertEquals("wrapper.Inners", 0, wrapper.Inners);
				AssertEquals("wrapper.InnersDetail", "", wrapper.InnersDetail);
				AssertEquals("wrapper.PackedItemCount", 0, wrapper.PackedItemCount);
				AssertEquals("wrapper.IsExclusive", false, wrapper.IsExclusive);
				AssertEquals("wrapper.IsExpiryUsed", false, wrapper.IsExpiryUsed);
				AssertEquals("wrapper.IsPackingDateUsed", false, wrapper.IsPackingDateUsed);
				AssertEquals("wrapper.IsPartAttrib1Used", false, wrapper.IsPartAttrib1Used);
				AssertEquals("wrapper.IsPartAttrib2Used", false, wrapper.IsPartAttrib2Used);
				AssertEquals("wrapper.IsPartAttrib3Used", false, wrapper.IsPartAttrib3Used);
				AssertEquals("wrapper.IsTrackedSerialUsed", false, wrapper.IsTrackedSerialUsed);
				AssertEquals("wrapper.DisplayOrder", "", wrapper.DisplayOrder);
				AssertEquals("wrapper.IsTopLevelPackage", true, wrapper.IsTopLevelPackage);
				AssertEquals("wrapper.HasSingleProduct", false, wrapper.HasSingleProduct);
				AssertEquals("wrapper.HasPackedItem", false, wrapper.HasPackedItem);
				AssertNull("wrapper.PackedItem", wrapper.PackedItem);
				AssertNull("wrapper.FreightPackLine", wrapper.FreightPackLine);
				AssertEquals("wrapper.OutterPackageSequence", ZShort.Zero, wrapper.OutterPackageSequence);
				AssertEquals("wrapper.OutterPackagesCount", ZShort.Zero, wrapper.OutterPackagesCount);
				AssertEquals("wrapper.PickLocation", ZString.Empty, wrapper.PickLocation);
				AssertEquals("wrapper.PickMethod", ZString.Empty, wrapper.PickMethod);
			});
		}

		public void TestGetCommodity()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var pack = quote.CurrentOneOffQuote.LooseCargo.AddNew();

			Factory.Save();

			var wrapper = new PackageWrapperFromOneOffContainer(pack, Factory);

			AssertEquals("Expected no commodity", ZString.Empty, wrapper.Commodity.Code);
			AssertEquals("Expected no commodity", ZString.Empty, wrapper.Commodity.Description);

			var commodity = Factory.LoadTop1<RefCommodityCode>(new ZQuery());
			quote.CurrentOneOffQuote.TT_RH_NKCommodity = commodity.RH_Code;

			wrapper = new PackageWrapperFromOneOffContainer(pack, Factory);

			AssertEquals(commodity.RH_Code, wrapper.Commodity.Code);
			AssertEquals(commodity.RH_Description, wrapper.Commodity.Description);

			quote.CurrentOneOffQuote.TT_RH_NKCommodity = "XXYY";

			wrapper = new PackageWrapperFromOneOffContainer(pack, Factory);

			AssertEquals("Parent code is invalid so should not appear on pack", ZString.Empty, wrapper.Commodity.Code);
			AssertEquals("Parent code is invalid so should not appear on pack", ZString.Empty, wrapper.Commodity.Description);
		}

		public void TestWrapperThreeDecimalPlaces()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);

			var pack = quote.CurrentOneOffQuote.LooseCargo.AddNew();
			pack.TPL_Length = 4.521;
			pack.TPL_Width = 3.623;
			pack.TPL_Height = 2.324;
			pack.TPL_DimensionUQ = Constants.Length.Metres;

			Factory.Save();

			PackageWrapper wrapper = new PackageWrapperFromOneOffContainer(pack, Factory);

			AssertEquals("wrapper.Height", 2.324m, wrapper.Dimensions.Height);
			AssertEquals("wrapper.Length", 4.521m, wrapper.Dimensions.Length);
			AssertEquals("wrapper.Width", 3.623m, wrapper.Dimensions.Width);
		}

		#region Implementation

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var pack = quote.CurrentOneOffQuote.LooseCargo.AddNew();
			return new PackageWrapperFromOneOffContainer(pack, Factory);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Commodity : 
CommonCurrency : 
Container :  is null
DamagedPackages : 
DamagedReason : 
Dimensions : 4 x 3 x 2 M
FumigatedPackages : 
HandlingUnit :  is null
HeatTreatedPackages : 
ISPMPalletPackages : 
MostRecentAudit :  is null
NonStackablePackages : 
Origin : 
OutturnedPackages : 
OutturnedVolume : 
OutturnedWeight : 
PackageOrderReference : 
Packages : 5 PLT
PackageState :  is null
PackedItem :  is null
Parent : 1000
PillagedPackages : 
Registry : (No Default Field Value Available on Registry)
TopLevelHandlingUnit :  is null
TopLoadOnlyPackages : 
UOMType : 
Volume : 120.000 M3
Weight : 15000.000 KG
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);

			var pack = quote.CurrentOneOffQuote.LooseCargo.AddNew();
			pack.TPL_PackLineCount = 5;
			pack.TPL_F3_NKPackType = Constants.PkgUnit.Pallet;
			pack.TPL_Weight = 15000;
			pack.TPL_Volume = 120;
			pack.TPL_Length = 4;
			pack.TPL_Width = 3;
			pack.TPL_Height = 2;
			pack.TPL_WeightUQ = Constants.Weight.Kilograms;
			pack.TPL_VolumeUQ = Constants.Volume.CubicMetres;
			pack.TPL_DimensionUQ = Constants.Length.Metres;

			Factory.Save();

			return new PackageWrapperFromOneOffContainer(pack, Factory);
		}

		#endregion
	}
}
