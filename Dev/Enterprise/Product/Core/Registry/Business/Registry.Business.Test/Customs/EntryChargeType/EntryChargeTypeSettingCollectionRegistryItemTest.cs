using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Customs.Testing
{
	[TestedType(typeof(EntryChargeTypeSettingCollectionRegistryItem))]
	sealed class EntryChargeTypeSettingCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<EntryChargeTypeSettingCollection>
	{
		public void TestGetAllChargeCodesIncludingDefault()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			BusinessObject chargeCode1 = factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IAccChargeCode>());
			BusinessObject chargeCode2 = factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IAccChargeCode>());
			BusinessObject chargeCode3 = factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IAccChargeCode>());
			factory.Save();

			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, chargeCode1.PK.ToGuid());

			EntryChargeTypeSettingCollectionRegistryItem registry = (EntryChargeTypeSettingCollectionRegistryItem)GetNewRegistryItem();

			EntryChargeTypeSettingCollection coll = new EntryChargeTypeSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), factory);

			EntryChargeTypeSetting chargeType = coll.AddNew();
			chargeType.AC_ChargeCode = chargeCode2.PK;
			chargeType.ChargeType = AU.EntryChargeTypeList.Codes.AQISContainerCharges;

			chargeType = coll.AddNew();
			chargeType.AC_ChargeCode = chargeCode3.PK;
			chargeType.ChargeType = AU.EntryChargeTypeList.Codes.DutyAmount;

			registry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, coll);

			List<ZGuid> result = registry.GetAllChargeCodesIncludingDefault();
			AssertEquals(3, result.Count);
			AssertEquals(true, result.Contains(chargeCode1.PK));
			AssertEquals(true, result.Contains(chargeCode2.PK));
			AssertEquals(true, result.Contains(chargeCode3.PK));
		}

		protected override StronglyTypedRegistryItem<EntryChargeTypeSettingCollection, EntryChargeTypeSettingCollection> GetNewRegistryItem()
		{
			return new EntryChargeTypeSettingCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
		}
	}
}
