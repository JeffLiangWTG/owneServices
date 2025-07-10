using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(AccChargeCodeListCollection))]
	sealed class AccChargeCodeListCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AccChargeCodeListCollection>
	{
		public void TestFilter()
		{
			var chargeCode1 = Factory.New<AccChargeCode>();
			var chargeCode2 = Factory.New<AccChargeCode>();
			var chargeCode3 = Factory.New<AccChargeCode>();
			var chargeCode4 = Factory.New<AccChargeCode>();
			var chargeCode5 = Factory.New<AccChargeCode>();

			chargeCode1.AC_ChargeGroup = "ABC";
			chargeCode2.AC_ChargeGroup = "FRT";
			chargeCode3.AC_ChargeGroup = "ABC";
			chargeCode4.AC_ChargeGroup = "ABC";
			chargeCode5.AC_ChargeGroup = "ABC";

			chargeCode1.AC_ChargeType = "ABC";
			chargeCode2.AC_ChargeType = "ABC";
			chargeCode3.AC_ChargeType = "DSB";
			chargeCode4.AC_ChargeType = "MRG";
			chargeCode5.AC_ChargeType = "MJA";

			AccChargeCodeListCollection collection = new AccChargeCodeListCollection("", RegistryFindBoxFilter.None, Factory, Env.CurrentCompany.PK);
			((AccChargeCodeCollection)collection.AccChargeCodeCollection).Load();
			AssertEquals("AccChargeCodeCollection.Contains(ChargeCode1)", true, collection.AccChargeCodeCollection.Contains(chargeCode1));
			AssertEquals("AccChargeCodeCollection.Contains(ChargeCode2)", true, collection.AccChargeCodeCollection.Contains(chargeCode2));
			AssertEquals("AccChargeCodeCollection.Contains(ChargeCode3)", true, collection.AccChargeCodeCollection.Contains(chargeCode3));
			AssertEquals("AccChargeCodeCollection.Contains(ChargeCode4)", true, collection.AccChargeCodeCollection.Contains(chargeCode4));
			AssertEquals("AccChargeCodeCollection.Contains(ChargeCode5)", true, collection.AccChargeCodeCollection.Contains(chargeCode5));

			collection = new AccChargeCodeListCollection("", RegistryFindBoxFilter.FreightChargeCode, Factory, Env.CurrentCompany.PK);
			((AccChargeCodeCollection)collection.AccChargeCodeCollection).Load();
			AssertEquals("AccChargeCodeCollection.Contains(ChargeCode1)", false, collection.AccChargeCodeCollection.Contains(chargeCode1));
			AssertEquals("AccChargeCodeCollection.Contains(ChargeCode2)", true, collection.AccChargeCodeCollection.Contains(chargeCode2));
			AssertEquals("AccChargeCodeCollection.Contains(ChargeCode3)", false, collection.AccChargeCodeCollection.Contains(chargeCode3));
			AssertEquals("AccChargeCodeCollection.Contains(ChargeCode4)", false, collection.AccChargeCodeCollection.Contains(chargeCode4));
			AssertEquals("AccChargeCodeCollection.Contains(ChargeCode5)", false, collection.AccChargeCodeCollection.Contains(chargeCode5));

			collection = new AccChargeCodeListCollection("", RegistryFindBoxFilter.MrgDsbOrMjaChargeCode, Factory, Env.CurrentCompany.PK);
			((AccChargeCodeCollection)collection.AccChargeCodeCollection).Load();
			AssertEquals("AccChargeCodeCollection.Contains(ChargeCode1)", false, collection.AccChargeCodeCollection.Contains(chargeCode1));
			AssertEquals("AccChargeCodeCollection.Contains(ChargeCode2)", false, collection.AccChargeCodeCollection.Contains(chargeCode2));
			AssertEquals("AccChargeCodeCollection.Contains(ChargeCode3)", true, collection.AccChargeCodeCollection.Contains(chargeCode3));
			AssertEquals("AccChargeCodeCollection.Contains(ChargeCode4)", true, collection.AccChargeCodeCollection.Contains(chargeCode4));
			AssertEquals("AccChargeCodeCollection.Contains(ChargeCode5)", true, collection.AccChargeCodeCollection.Contains(chargeCode5));
		}

		public void TestAdditionalFilterWithGCAndIsActive()
		{
			var collection = new AccChargeCodeListCollection("", RegistryFindBoxFilter.None, Factory, Env.CurrentCompany.PK);
			var additionalFilter = ((ILegacyBusinessObjectCollectionInternals)collection.AccChargeCodeCollection).AdditionalFilter;
			AssertEquals("AdditionalFilter contains AC_GC filter - None", true, additionalFilter.Params.Any(param => param.SchemaColumn == AccChargeCodeSchema.AC_GC && (Guid)param.Value == Env.CurrentCompany.PK));
			AssertEquals("AdditionalFilter contains AC_IsActive filter - None", true, additionalFilter.Params.Any(param => param.SchemaColumn == AccChargeCodeSchema.AC_IsActive && (int)param.Value == 1));

			collection = new AccChargeCodeListCollection("", RegistryFindBoxFilter.FreightChargeCode, Factory, Env.CurrentCompany.PK);
			additionalFilter = ((ILegacyBusinessObjectCollectionInternals)collection.AccChargeCodeCollection).AdditionalFilter;
			AssertEquals("AdditionalFilter contains AC_GC filter - FreightChargeCode", true, additionalFilter.Params.Any(param => param.SchemaColumn == AccChargeCodeSchema.AC_GC && (Guid)param.Value == Env.CurrentCompany.PK));
			AssertEquals("AdditionalFilter contains AC_IsActive filter - FreightChargeCode", true, additionalFilter.Params.Any(param => param.SchemaColumn == AccChargeCodeSchema.AC_IsActive && (int)param.Value == 1));

			collection = new AccChargeCodeListCollection("", RegistryFindBoxFilter.MrgDsbOrMjaChargeCode, Factory, Env.CurrentCompany.PK);
			additionalFilter = ((ILegacyBusinessObjectCollectionInternals)collection.AccChargeCodeCollection).AdditionalFilter;
			AssertEquals("AdditionalFilter contains AC_GC filter - MrgDsbOrMjaChargeCode", true, additionalFilter.Params.Any(param => param.SchemaColumn == AccChargeCodeSchema.AC_GC && (Guid)param.Value == Env.CurrentCompany.PK));
			AssertEquals("AdditionalFilter contains AC_IsActive filter - MrgDsbOrMjaChargeCode", true, additionalFilter.Params.Any(param => param.SchemaColumn == AccChargeCodeSchema.AC_IsActive && (int)param.Value == 1));
		}

		public void TestLoadingWithNoCompanyPK()
		{
			var globalChargeCode = CreateGlobalChargeCodeSetup();
			var localChargeCodeInThisCompany = globalChargeCode.ChildChargeCodes.ToArray().Where(c => c.AC_GC.ToGuid() == Env.CurrentCompany.PK).Single();

			var collection = new AccChargeCodeListCollection(globalChargeCode.PK.ToString(), RegistryFindBoxFilter.None, Factory, Guid.Empty);
			AssertEquals("Should contain only one element as per data string", 1, collection.Count);
			AssertEquals("It should contain the correct global charge code and not a local one", globalChargeCode.PK, collection[0].ChargeCode);

			collection = new AccChargeCodeListCollection(localChargeCodeInThisCompany.PK.ToString(), RegistryFindBoxFilter.None, Factory, Guid.Empty);
			AssertEquals("Should contain only one element as per data string", 1, collection.Count);
			AssertEquals("It should contain the local one. Will appear as invalid and require user to fix.", localChargeCodeInThisCompany.PK, collection[0].ChargeCode);
		}

		public void TestLoadingWithCompanyPK()
		{
			var globalChargeCode = CreateGlobalChargeCodeSetup();
			var localChargeCodeInThisCompany = globalChargeCode.ChildChargeCodes.ToArray().Where(c => c.AC_GC.ToGuid() == Env.CurrentCompany.PK).Single();

			var collection = new AccChargeCodeListCollection(globalChargeCode.PK.ToString(), RegistryFindBoxFilter.None, Factory, Env.CurrentCompany.PK);
			((AccChargeCodeCollection)collection.AccChargeCodeCollection).Load();
			AssertEquals("Should contain only one element as per data string", 1, collection.Count);
			AssertEquals("It should contain the equivalent local charge code", localChargeCodeInThisCompany.PK, collection[0].ChargeCode);

			collection = new AccChargeCodeListCollection(localChargeCodeInThisCompany.PK.ToString(), RegistryFindBoxFilter.None, Factory, Env.CurrentCompany.PK);
			((AccChargeCodeCollection)collection.AccChargeCodeCollection).Load();
			AssertEquals("Should contain only one element as per data string", 1, collection.Count);
			AssertEquals("It should contain the equivalent local charge code", localChargeCodeInThisCompany.PK, collection[0].ChargeCode);
		}

		public void TestLoadingWithCompanyPK_NoLocalChargeCode()
		{
			var globalChargeCode = CreateGlobalChargeCodeSetup();
			var localChargeCodeInThisCompany = globalChargeCode.ChildChargeCodes.ToArray().Where(c => c.AC_GC.ToGuid() == Env.CurrentCompany.PK).Single();
			localChargeCodeInThisCompany.Delete();
			Factory.Save();

			var collection = new AccChargeCodeListCollection(globalChargeCode.PK.ToString(), RegistryFindBoxFilter.None, Factory, Env.CurrentCompany.PK);
			((AccChargeCodeCollection)collection.AccChargeCodeCollection).Load();
			AssertEquals("Should contain only no elements as there is no local charge for the global one", 0, collection.Count);
		}

		#region Setup

		AccChargeCode CreateGlobalChargeCodeSetup()
		{
			var globalChargeCode = Factory.New<AccChargeCode>();
			globalChargeCode.AC_GC = ZGuid.Empty;
			globalChargeCode.AC_Code = "CC1";
			globalChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.NonJobRelated;
			globalChargeCode.AC_ChargeType = Enterprise.Core.Constants.ChargeType.Comment;
			globalChargeCode.AC_Desc = "CC1";
			globalChargeCode = Factory.New<AccChargeCode>();
			globalChargeCode.AC_GC = ZGuid.Empty;
			globalChargeCode.AC_Code = "CC2";
			globalChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.NonJobRelated;
			globalChargeCode.AC_ChargeType = Enterprise.Core.Constants.ChargeType.Comment;
			globalChargeCode.AC_Desc = "CC2";
			Factory.Save();
			return globalChargeCode;
		}

		#endregion

		#region Implementation

		protected override AccChargeCodeListCollection GetCollectionToTest()
		{
			return new AccChargeCodeListCollection("", RegistryFindBoxFilter.None, Factory, Env.CurrentCompany.PK);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AccChargeCodeListElement(ZGuid.NewZGuid(), Collection);
		}

		#endregion
	}
}
