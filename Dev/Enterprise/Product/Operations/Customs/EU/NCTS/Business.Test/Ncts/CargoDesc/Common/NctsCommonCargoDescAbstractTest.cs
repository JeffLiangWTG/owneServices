using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestsSubclassesOf(typeof(NctsCommonCargoDesc))]
	public abstract class NctsCommonCargoDescAbstractTest<TMaster> : EnterpriseBusinessObjectTestCase
		where TMaster : NctsHeader
	{
		public virtual int CountSupportingInfoTypes => 3;

		public void TestGetCusSupportingInfoTypes()
		{
			var goodsItem = (NctsCommonCargoDesc)GetNewBusinessObject();
			var cusSupportingInfoTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)goodsItem).GetCusSupportingInfoTypes();
			CombineAssertions(() =>
			{
				AssertEquals("Count", CountSupportingInfoTypes, cusSupportingInfoTypes.Count);
				AssertEquals("PRE", goodsItem.GetType().GetProperty("PreviousDocumentType", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(goodsItem), cusSupportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument]);
				AssertEquals("SUP", goodsItem.GetType().GetProperty("SupportingDocumentType", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(goodsItem), cusSupportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument]);
				AssertEquals("OTH", goodsItem.GetType().GetProperty("AdditionalInfoType", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(goodsItem), cusSupportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
			});
		}

		public void TestOverrideAdditionalInfoTypeForOptimisation()
		{
			var goodsItem = (NctsCommonCargoDesc)GetNewBusinessObject();
			var goodsItemType = goodsItem.GetType();
			if (goodsItemType != typeof(NctsCommonCargoDesc))
			{
				var cusSupportingInfoTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)goodsItem).GetCusSupportingInfoTypes();
				if (cusSupportingInfoTypes.TryGetValue(Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo, out var additionalInfoType)
					&& additionalInfoType != typeof(NctsAdditionalInfo)
					&& goodsItemType.GetProperty("AdditionalInfoType", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly) == null)
				{
					Fail($"Override {goodsItemType.FullName}.AdditionalInfoType to return {additionalInfoType.FullName} for better performance instead of relying on NctsTypeDecider");
				}
			}
			Assert(true);
		}

		public void TestCorrectTypeDecideForLoad()
		{
			var goodsItem = GetNewBusinessObject();
			Factory.Save();
			var goodsItemType = goodsItem.GetType();
			CombineAssertions(() =>
			{
				AssertType("Base", goodsItemType, new BusinessObjectFactory().Load<CusInBondCargoDesc>(goodsItem.PK));
				AssertType("EU Common Goods Item", goodsItem.GetType(), new BusinessObjectFactory().Load<NctsCommonCargoDesc>(goodsItem.PK));
				AssertAdditionalTypes(goodsItem);
			});
		}

		protected virtual void AssertAdditionalTypes(BusinessObject goodsItem) { }

		protected abstract ZString CountryCode { get; }

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNctsDeclarationTypeList(CountryCode);
			Factory.Save();

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VQ", "VQ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"BG", "BG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);

			Factory.Save();
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<TMaster>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return header.MovementHeader.GoodsItems.AddNew();
		}

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
		{
			return new NctsCommonCargoDescLightValidationTester(bizObjToTest);
		}

		sealed class NctsCommonCargoDescLightValidationTester : LightValidationTester
		{
			public NctsCommonCargoDescLightValidationTester(BusinessObject bo) : base(bo)
			{
			}

			protected override bool ShouldTestProperty(ZPropertyInfo info)
			{
				var bizo = info.BizObj;
				return !(bizo is CusEntryNumber) && !(bizo is JobDocAddress)
					&& info.Name != nameof(CusInBondHeader.BH_SystemCreateTimeUtc)
					&& base.ShouldTestProperty(info);
			}
		}
	}
}
