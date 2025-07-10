using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	[TestedType(typeof(NonPersistentCheckInAllChildPieces))]
	class NonPersistentCheckInAllChildPiecesTests : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			mawb = Factory.New<CusMAWB>();
			hawb = mawb.ChildBills.AddNew();
			var split = hawb.Splits.AddNew();
			nonPersistentCheckInAllChildPieces = new NonPersistentCheckInAllChildPieces(mawb);
			return nonPersistentCheckInAllChildPieces;
		}

		public void TestPackagesUnits()
		{
			var mockTypeCodes = new List<string> { "PK", "ZZ" };

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes,
				"Package types for test");
			mockTypeCodes.ForEach(li => helper.CreateCusCodeList(
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				li,
				$"Package type {li} for test",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTime));
			Factory.Save();

			GetNewBusinessObject();
			CombineAssertions(() =>
			{
				AssertNoMessageErrors(nonPersistentCheckInAllChildPieces.PackagesUnitsInfo);
				nonPersistentCheckInAllChildPieces.PackagesUnits = "ZZ";
				AssertNoMessageErrors(nonPersistentCheckInAllChildPieces.PackagesUnitsInfo);
				nonPersistentCheckInAllChildPieces.PackagesUnits = "MM";
				nonPersistentCheckInAllChildPieces.Validation.ValidatePackagesUnits();
				AssertHasMessageErrorContaining(nonPersistentCheckInAllChildPieces.PackagesUnitsInfo, ListValidation.InvalidCodeMessageError);
				nonPersistentCheckInAllChildPieces.PackagesUnits = "";
				AssertNoMessageErrors(nonPersistentCheckInAllChildPieces.PackagesUnitsInfo);
			});
		}

		[TestDate(2010, 11, 12)]
		public void TestConstructorDefaults()
		{
			GetNewBusinessObject();
			var npbo = new NonPersistentCheckInAllChildPieces(mawb);
			CombineAssertions("NonPersistentCheckInAllChildPieces defaults", () =>
			{
				AssertEquals("PK", npbo.PackagesUnits);
				AssertEquals(ZDateTime.Today, npbo.ReceivedDate);
				AssertEquals("", npbo.MarksAndNumbers);
				AssertEquals(ZGuid.Empty, npbo.ShedStorageLocationId);
				AssertEquals(false, npbo.IsBeingReleasedNow);
				AssertEquals("", npbo.GoodsDescription);
				AssertEquals("", npbo.ContainerNumber);
				AssertEquals("", npbo.ContainerSeal);
				AssertEquals(false, npbo.IsDamaged);
			});
		}

		public void TestValidationObjectType()
		{
			var nonPersistentCheckInAllChildPieces = (NonPersistentCheckInAllChildPieces)GetNewBusinessObject();
			AssertType(typeof(NonPersistentCheckInAllChildPiecesValidation), nonPersistentCheckInAllChildPieces.Validation);
		}

		NonPersistentCheckInAllChildPieces nonPersistentCheckInAllChildPieces;
		CusMAWB mawb;
		CusHAWB hawb;
	}
}
