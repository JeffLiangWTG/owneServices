using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(CusGoodsLocation))]
	[TestsSubclassesOf(typeof(CusGoodsLocation))]
	public class CusGoodsLocationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTypeDecider()
		{
			AssertType<CusGoodsLocationTypeDecider>(CusGoodsLocation.TypeDecider);
		}

		public void TestParent()
		{
			var bill = Factory.New<AsycudaBill>();
			AssertType<AsycudaBill>(bill.CusGoodsLocation.Parent);
		}

		public void TestLookup()
		{
			var bill = Factory.New<AsycudaBill>();
			var cusGoodsLocation = bill.CusGoodsLocation;

			AssertType(LookupType, cusGoodsLocation.Lookups);
		}

		public void TestUNLOCODEMaxLength()
		{
			AssertHasCustomAttribute<MaxLengthAttribute>(typeof(CusGoodsLocation), nameof(CusGoodsLocation.Unlocode), false, r => r.MaxLength == 17);
		}

		public void TestCustomizedHumanReadableName()
		{
			var location = Factory.New<CusGoodsLocation>();
			AssertEquals("Location of Goods: Type", location.CGL_TypeInfo.HumanReadableName);
			AssertEquals("Location of Goods: Qualifier", location.CGL_QualifierInfo.HumanReadableName);
			AssertEquals("Location of Goods: Additional Identifier", location.CGL_AdditionalIdentifierInfo.HumanReadableName);
		}

		public void TestCloneCusGoodsLocation()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var cusGoodsLocation = bill.CusGoodsLocation;
			cusGoodsLocation.Unlocode = "234";
			var address = cusGoodsLocation.Address;
			address.E2_Contact = "contact";

			var clonedGoodsLocation = (CusGoodsLocation)cusGoodsLocation.Clone();

			AssertEquals("234", clonedGoodsLocation.Unlocode);
			AssertEquals("contact", clonedGoodsLocation.Address.E2_Contact);
		}

		protected override BusinessObject GetNewBusinessObject() => CreateCusGoodsLocation();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => CreateCusGoodsLocation();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateCusGoodsLocation();

		protected virtual Type LookupType => typeof(CusGoodsLocationLookups);

		BusinessObject CreateCusGoodsLocation()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			return bill.CusGoodsLocation;
		}
	}
}
