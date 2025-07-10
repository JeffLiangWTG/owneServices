using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Customs.Testing
{
	[TestedType(typeof(EntryChargeTypeSettingCollection))]
	sealed class EntryChargeTypeSettingCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<EntryChargeTypeSettingCollection>
	{
		public void TestParentCollectionGetsSet()
		{
			GlbCompanyTestHelper.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand);

			var collection = new EntryChargeTypeSettingCollection(this.Collection.CurrentFallbackLevel, Factory);
			var chargeType1 = collection.AddNew();
			chargeType1.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			chargeType1.ChargeType = "CH1";
			chargeType1.AC_ChargeCode = AccChargeCode.PK;
			AssertEquals(collection, chargeType1.ParentCollection);
			var chargeType2 = new EntryChargeTypeSetting(this.Collection.CurrentFallbackLevel, Factory, null);
			chargeType2.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			chargeType2.ChargeType = "CH2";
			chargeType2.AC_ChargeCode = AccChargeCode.PK;
			collection.Add(chargeType2);
			AssertEquals(collection, chargeType2.ParentCollection);

			var item1 = new EntryChargeTypeSettingCollectionRegistryItem("Freddddddd", (NoResString)"TESTING NOLY", (NoResString)"LALALA", (NoResString)"No thanks I ate at the office", RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue);
			item1.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);
			collection = item1.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);

			AssertEquals(2, collection.Count);
			var item1st = collection[0];
			AssertEquals(collection, item1st.ParentCollection);
			item1st.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			AssertEquals("Enterprise.Customs.NZ.Registry.EntryChargeTypeList", item1st.ChargeType_List.GetType().FullName);
			var item2nd = collection[1];
			AssertEquals(collection, item2nd.ParentCollection);
			item2nd.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			AssertEquals("Enterprise.Customs.NZ.Registry.EntryChargeTypeList", item2nd.ChargeType_List.GetType().FullName);
		}

		#region Implementation
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override EntryChargeTypeSettingCollection GetCollectionToTest()
		{
			return new EntryChargeTypeSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new EntryChargeTypeSetting();
		}

		#region AccChargeCode
		BusinessObject AccChargeCode
		{
			get
			{
				if (fAccChargeCode == null)
				{
					System.Reflection.Assembly masterFilesAssembly = System.Reflection.Assembly.Load("Enterprise.MasterFiles.Business");
					Type accChargeCodeType = masterFilesAssembly.GetType("Enterprise.MasterFiles.Business.AccChargeCode");
					fAccChargeCode = Factory.New(accChargeCodeType);
				}
				return fAccChargeCode;
			}
		}
		BusinessObject fAccChargeCode;
		#endregion

		#endregion
	}
}
