using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(LocalTransportBookedMoveWrapper))]
	sealed class LocalTransportBookedMoveWrapperTest : GenericWrapperTest
	{
		#region TestProperties

		public void TestProperties()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			var move = cartage.BookedMovesCollection[0];
			Helper.SetupBookedMove(move, 1, Constants.PkgUnit.Bag, 2.1m, Constants.Weight.Grams, 0.005m, Constants.Volume.CubicFeet);
			move.EW_BookedHeight = 1.1m;
			move.EW_BookedLength = 2.2m;
			move.EW_BookedWidth = 3.3m;
			move.EW_DimUnit = Constants.Length.Inches;
			move.EW_DropMode = "HEY";
			move.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			move.EW_E2WaitPointAddressID = cartage.SecondDocAddress.PK;
			move.EW_DisplayOrder = 3;

			var moveWrapper = new LocalTransportBookedMoveWrapper(move, Factory);
			AssertEquals(1, moveWrapper.BookedPackages);
			AssertEquals(Constants.PkgUnit.Bag, moveWrapper.BookedPackType);
			AssertEquals(2.1m, moveWrapper.BookedWeight);
			AssertEquals(Constants.Weight.Grams, moveWrapper.BookedWeightUnit);
			AssertEquals(0.005m, moveWrapper.BookedVolume);
			AssertEquals(Constants.Volume.CubicFeet, moveWrapper.BookedVolumeUnit);

			AssertEquals(1.1m, moveWrapper.BookedHeight);
			AssertEquals(2.2m, moveWrapper.BookedLength);
			AssertEquals(3.3m, moveWrapper.BookedWidth);
			AssertEquals(Constants.Length.Inches, moveWrapper.BookedDimensionUnits);
			AssertEquals("HEY", moveWrapper.DropMode);
			AssertEquals(cartage.FirstDocAddress.PK, moveWrapper.PickupDocAddress.WrappedObjectPK);
			AssertEquals(cartage.SecondDocAddress.PK, moveWrapper.DeliveryDocAddress.WrappedObjectPK);
			AssertEquals(3, moveWrapper.DisplayOrder.ToZInt());
		}

		#endregion

		#region TestPickupCartageLeg

		public void TestPickupCartageLeg()
		{
		}

		#endregion

		#region Overrides

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
LocalTransportBookedMove
======================================================================
Name                                    Type
----------------------------------------------------------------------
DeliveryDocAddress                      Address
PickupDocAddress                        Address
Container                               Container
Cartage                                 Freight
BookedDimensionUnits                    String
BookedHeight                            Decimal
BookedLength                            Decimal
BookedPackages                          Int
BookedPackType                          String
BookedVolume                            Decimal
BookedVolumeUnit                        String
BookedWeight                            Decimal
BookedWeightUnit                        String
BookedWidth                             Decimal
DisplayOrder                            Short
DropMode                                String

LocalTransportLegs                      LocalTransportLeg Collection
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return
@"Cartage : 
Container : 
DeliveryDocAddress : 
PickupDocAddress : 
Registry : (No Default Field Value Available on Registry)";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new LocalTransportBookedMoveWrapper(Factory.New<CommonBookedCtgMove>(), Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new LocalTransportBookedMoveWrapper(Factory.New<CommonBookedCtgMove>(), Factory);
		}

		public override void TestWrapperMappingsEmpty()
		{
			LocalTransportBookedMoveWrapper wrapperEmpty = new LocalTransportBookedMoveWrapper(null, Factory);
			AssertEquals("LocalTransportLegs", 0, wrapperEmpty.LocalTransportLegs.Count);
			AssertEquals("BookedPackages", 0, wrapperEmpty.BookedPackages);
			AssertEquals("BookedPackType", "PLT", wrapperEmpty.BookedPackType);
			AssertEquals("BookedWeight", 0m, wrapperEmpty.BookedWeight);
			AssertEquals("BookedWeightUnit", "KG", wrapperEmpty.BookedWeightUnit);
			AssertEquals("BookedVolume", 0m, wrapperEmpty.BookedVolume);
			AssertEquals("BookedVolumeUnit", "M3", wrapperEmpty.BookedVolumeUnit);
			AssertEquals("BookedHeight", 0m, wrapperEmpty.BookedHeight);
			AssertEquals("BookedLength", 0m, wrapperEmpty.BookedLength);
			AssertEquals("BookedWidth", 0m, wrapperEmpty.BookedWidth);
			AssertEquals("BookedDimensionUnits", "M", wrapperEmpty.BookedDimensionUnits);
			AssertEquals("DropMode", "", wrapperEmpty.DropMode);
			AssertEquals("Cartage", "", wrapperEmpty.Cartage.JobNumber);
			AssertEquals("PickupDocAddress", "", wrapperEmpty.PickupDocAddress.CompanyName);
			AssertEquals("DeliveryDocAddress", "", wrapperEmpty.DeliveryDocAddress.CompanyName);
		}

		#endregion

		#region Helper

		LocalCartageTestHelper Helper
		{
			get { return new LocalCartageTestHelper(Factory); }
		}

		#endregion
	}
}
