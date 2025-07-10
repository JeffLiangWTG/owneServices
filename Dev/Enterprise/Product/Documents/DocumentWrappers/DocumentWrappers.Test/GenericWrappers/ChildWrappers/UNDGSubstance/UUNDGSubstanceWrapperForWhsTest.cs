using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(UNDGSubstanceWrapperForWhs))]
	sealed class UUNDGSubstanceWrapperForWhsTest : UNDGSubstanceWrapperTest
	{
		#region TestConstructor_LinkedToProduct

		public void TestConstructor_LinkedToProduct()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			helper.SetProductWeightAndVolume(data.Part1, 1m, Constants.Weight.Tonnes, 1m, Constants.Volume.CubicMetres);
			var undgItem1 = Factory.New<UNDGDataItem>();
			var undgItem2 = helper.CreateUNDGDataItem(data.Part1, "2808", 0m, Constants.Weight.Grams, 0m, Constants.Volume.CubicCentimeters);

			AssertNoExceptionThrown("When no DG item is passed into constructor, there should be no exceptions.", () => new UNDGSubstanceWrapperForWhs(null, Factory, 0m));
			AssertExceptionThrown("When passed in DG item is not linked to a product, throw exception.", typeof(ArgumentException), "UNDG data item should be linked to a Product.", () => new UNDGSubstanceWrapperForWhs(undgItem1, Factory, 0m));
			AssertNoExceptionThrown("When passed in DG item is linked to a product, there should be no exceptions.", () =>
				{
					var wrapper = new UNDGSubstanceWrapperForWhs(undgItem2, Factory, 0m);
					AssertEquals("When qty passed is 0, then weight should be 0 as well even if it is populated on product.", 0m, wrapper.Weight.Value);
					AssertEquals("When qty passed is 0, then volume should be 0 as well even if it is populated on product.", 0m, wrapper.Volume.Value);
				});
		}

		#endregion

		#region TestWeightVolume_Fallback

		public void TestWeightVolume_Fallback()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);

			helper.SetProductWeightAndVolume(data.Part1, 7m, Constants.Weight.Tonnes, 8m, Constants.Volume.CubicMetres);
			var undgItem1 = helper.CreateUNDGDataItem(data.Part1, "2808", 0.1m, Constants.Weight.Kilograms, 0.1m, Constants.Volume.CubicCentimeters);
			var ungdSubstanceWrapper1 = new UNDGSubstanceWrapperForWhs(undgItem1, Factory, 5m);
			AssertEquals("UNDG item has Weight is populated, so it should be used to calculate Weight.", 0.5m, ungdSubstanceWrapper1.Weight.Value);
			AssertEquals("UNDG item has Weight is populated, so UNGD items Weigh UQ should be used.", Constants.Weight.Kilograms, ungdSubstanceWrapper1.Weight.Unit.Code);
			AssertEquals("UNDG item has Volume is populated, so it should be used to calculate Volume.", 0.5m, ungdSubstanceWrapper1.Volume.Value);
			AssertEquals("UNDG item has Volume is populated, so UNGD items Volume UQ should be used..", Constants.Volume.CubicCentimeters, ungdSubstanceWrapper1.Volume.Unit.Code);

			helper.SetProductWeightAndVolume(data.Part2, 1.5m, Constants.Weight.Kilograms, 0.15m, Constants.Volume.CubicMetres);
			var undgItem2 = helper.CreateUNDGDataItem(data.Part2, "2808", 0m, Constants.Weight.Grams, 0m, Constants.Volume.CubicCentimeters);
			var ungdSubstanceWrapper2 = new UNDGSubstanceWrapperForWhs(undgItem2, Factory, 5m);
			AssertEquals("UNDG item has no Weight, so fallback products Weight should be used for calculation.", 7.5m, ungdSubstanceWrapper2.Weight.Value);
			AssertEquals("UNDG item has no Weight, so fallback products Weigh UQ should be used.", Constants.Weight.Kilograms, ungdSubstanceWrapper2.Weight.Unit.Code);
			AssertEquals("UNDG item has no Volume, so fallback products Volume should be used for calculation.", 0.75m, ungdSubstanceWrapper2.Volume.Value);
			AssertEquals("UNDG item has no Volume, so fallback products Volume UQ should be used..", Constants.Volume.CubicMetres, ungdSubstanceWrapper2.Volume.Unit.Code);
		}

		#endregion

		#region Implementations

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new UNDGSubstanceWrapperForWhs(null, Factory, 0m);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
UNDGSubstance                     (Default Field: UNNumberWithVariant)
======================================================================
Name                                    Type
----------------------------------------------------------------------
DGContact                               Contact
ContainingPackage                       Package
Packages                                PackQTY
Volume                                  Volume
Weight                                  Weight
EMSCode                                 String
FlashPoint                              String
IMOClass                                String
IsLimitedQuantity                       Bool
LocalName                               String
LtdQty                                  String
MarinePollutantWarning                  String
PackingGroup                            String
PackingInstructions                     String
ProperShippingName                      String
PSAGroup                                String
PSAGroupWithLabel                       String
StorageCategorySummary                  String
SubLabel1                               String
SubLabel2                               String
Summary                                 String
SummaryWithContainingPackageID          String
SummaryWithEMSCode                      String
SummaryWithPacks                        String
TankStorageRetentionTrayRequired        Bool
TechnicalName                           String
UNNumber                                String
UNNumberWithVariant                     String
Variant                                 String
Variation                               String
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"ContainingPackage : 
DGContact : 
Packages : 
Registry : (No Default Field Value Available on Registry)
Volume : 
Weight :
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var dgSubstance = Factory.LoadTop1<UNDGSubstance>(new ZQuery());
			var part = Factory.New<OrgSupplierPart>();
			var dgItem = Factory.New<UNDGDataItem>();
			dgItem.DI_DG = dgSubstance.PK;
			dgItem.DI_ParentID = part.PK;
			dgItem.DI_ParentTableCode = part.TablePrefix;

			return new UNDGSubstanceWrapperForWhs(dgItem, Factory, 1m);
		}

		#endregion
	}
}
