using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Customs.Testing
{
	sealed class EntryChargeTypeListTest : TestCaseWithFactory
	{
		public void TestGetCachedList()
		{
			var list1 = EntryChargeTypeList.GetCachedList(Factory, Core.Constants.CountryCodes.Australia);
			var list2 = EntryChargeTypeList.GetCachedList(Factory, Core.Constants.CountryCodes.Australia);
			AssertEquals(list1, list2);
			var list3 = EntryChargeTypeList.GetCachedList(Factory, Core.Constants.CountryCodes.SouthAfrica);
			AssertNotEquals(list1, list3);
		}

		public void TestGetList()
		{
			AssertType<EmptyEntryChargeTypeList>(EntryChargeTypeList.GetList(""));
			var list1 = EntryChargeTypeList.GetList("A!");
			AssertType<ZZEntryChargeTypeList>(list1);
			list1 = EntryChargeTypeList.GetList(Core.Constants.CountryCodes.Australia);
			AssertNotNull(list1);
			var list2 = EntryChargeTypeList.GetList(Core.Constants.CountryCodes.Australia);
			AssertNotNull(list2);
			AssertEquals(false, object.ReferenceEquals(list1, list2));
			var list3 = EntryChargeTypeList.GetList(Core.Constants.CountryCodes.SouthAfrica);
			AssertNotNull(list3);
			AssertEquals(false, object.ReferenceEquals(list1, list3));
		}

		public void TestRegistryItem()
		{
			AssertEquals(RatingDataRegistry.Instance.EntryChargeTypesAndCodes, ChargeTypeList.RegistryItem);
		}

		public void TestAddWith3Params()
		{
			ChargeTypeList.Add("Code", "Description", true, "FAT");
			var element = ChargeTypeList[0];
			AssertEquals("Code", "Code", element.Code);
			AssertEquals("Description", "Description", element.Description);
			AssertEquals("IsPaidWhenMessageClears", true, element.IsPaidWhenMessageClears);
			AssertEquals("ParentCodeForGSTOnARInvoice", "FAT", element.ParentCodeForGSTOnARInvoice);
		}

		public void TestAddIfNotExistsWith3Params()
		{
			CombineAssertions(() =>
			{
				ChargeTypeList.AddIfNotExists("Code", "Description", true, "");
				AssertEquals("Add Code", "Code", ChargeTypeList.CodesAsString);

				ChargeTypeList.AddIfNotExists("Code", "Description", true, "");
				AssertEquals("Not add if existed", "Code", ChargeTypeList.CodesAsString);

				ChargeTypeList.AddIfNotExists("Code1", "Description1", true, "");
				AssertEquals("Add Code1 which not existed in list", "Code, Code1", ChargeTypeList.CodesAsString);
			});
		}

		public void TestUsingEntryChargeTypeElementsInList()
		{
			EntryChargeType firstElement = new EntryChargeType(ChargeTypeList, "Code", "Description", true, "FOG");
			ChargeTypeList.Add(firstElement);
			AssertEquals("Count", 1, ChargeTypeList.Count);
			AssertEquals("in list", firstElement, ChargeTypeList[0]);

			foreach (EntryChargeType chargeType in ChargeTypeList)
			{
				AssertEquals("Code", "Code", chargeType.Code);
				AssertEquals("Description", "Description", chargeType.Description);
				AssertEquals("IsPaidWhenMessageClears", true, chargeType.IsPaidWhenMessageClears);
				AssertEquals("ParentCodeForGSTOnARInvoice", "FOG", chargeType.ParentCodeForGSTOnARInvoice);
			}
		}

		public void TestGetAllChargeCodePKsOf()
		{
			GlbCompanyTestHelper.TemporarilySetCountry(Core.Constants.CountryCodes.Australia);

			var chargeCode1 = Factory.NewWithValidTestData(CargoWise.Application.ObjectFactory.GetType(typeof(Enterprise.MasterFiles.Integration.IAccChargeCode)));
			var chargeCode2 = Factory.NewWithValidTestData(CargoWise.Application.ObjectFactory.GetType(typeof(Enterprise.MasterFiles.Integration.IAccChargeCode)));
			var chargeCode3 = Factory.NewWithValidTestData(CargoWise.Application.ObjectFactory.GetType(typeof(Enterprise.MasterFiles.Integration.IAccChargeCode)));
			var chargeCode4 = Factory.NewWithValidTestData(CargoWise.Application.ObjectFactory.GetType(typeof(Enterprise.MasterFiles.Integration.IAccChargeCode)));

			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, chargeCode1.PK.ToGuid());
			RatingDataRegistry.Instance.CustomDeferredChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, chargeCode3.PK.ToGuid());

			var coll = new EntryChargeTypeSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
			var setting = coll.AddNew();
			setting.AC_ChargeCode = chargeCode2.PK;
			setting.ChargeType = AU.EntryChargeTypeList.Codes.AQISContainerCharges;

			RatingDataRegistry.Instance.EntryChargeTypesAndCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, coll);

			var result = new List<ZGuid>(EntryChargeTypeList.GetAllChargeCodePKsOf(Env.CurrentCompany.PK, Core.Constants.CountryCodes.Australia));

			AssertEquals(3, result.Count);
			Assert(result.Contains(chargeCode1.PK));
			Assert(result.Contains(setting.AC_ChargeCode));
			Assert(result.Contains(chargeCode3.PK));

			GlbCompanyTestHelper.TemporarilySetCountry(Core.Constants.CountryCodes.Japan);
			result = new List<ZGuid>(EntryChargeTypeList.GetAllChargeCodePKsOf(Env.CurrentCompany.PK, ""));
			AssertEquals(2, result.Count);
			Assert(result.Contains(chargeCode1.PK));
			Assert(result.Contains(chargeCode3.PK));

			GlbCompanyTestHelper.TemporarilySetCountry(Core.Constants.CountryCodes.Australia);
			RatingDataRegistry.Instance.CustomsQuarantineChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new ChargeCodeWithDate() { ChargeCode = chargeCode4.PK.ToGuid() });
			result = new List<ZGuid>(EntryChargeTypeList.GetAllChargeCodePKsOf(Env.CurrentCompany.PK, ""));
			AssertEquals(2, result.Count);
			Assert(result.Contains(chargeCode1.PK));
			Assert(result.Contains(chargeCode3.PK));
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestCannotAddNormalCodeDescriptionPairs()
		{
			ChargeTypeList.Add(new CodeDescriptionPair("a", "b"));
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestCannotAddPairs()
		{
			ChargeTypeList.AddPair("a", "b");
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestCannotAddPairsWithPK()
		{
			ChargeTypeList.AddPair(Guid.NewGuid(), "a", "b");
		}

		#region Implementation
		EntryChargeTypeList ChargeTypeList
		{
			get
			{
				if (fChargeTypeList == null)
				{
					fChargeTypeList = new EmptyEntryChargeTypeList();
				}

				return fChargeTypeList;
			}
		}
		EntryChargeTypeList fChargeTypeList;
		#endregion
	}
}
