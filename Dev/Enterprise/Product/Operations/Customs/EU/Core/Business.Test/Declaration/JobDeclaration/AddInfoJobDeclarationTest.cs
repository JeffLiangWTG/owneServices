using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(JobDeclaration))]
	public class AddInfoJobDeclarationBOTest : NonPersistentBusinessObjectTestCase
	{
		public void TestZG_VATDeferTypeList()
		{
			AssertEquals("AddInfoLookups.DeferTypeList", GetJobDeclaration().ZG_VATDeferTypeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
		}

		public void TestZG_VATDeferTypeMaxLength()
		{
			AssertEquals(1, GetJobDeclaration().ZG_VATDeferTypeInfo.MaxLength);
		}

		public void TestZG_BorderTransportMeans_List()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("AddInfoLookups.BorderTransportMeansList", declaration.ZG_BorderTransportMeansInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
		}

		public void TestZG_SpecificCircumstanceIndicatorList()
		{
			AssertEquals("AddInfoLookups.SpecificCircumstanceIndicatorList", GetJobDeclaration().ZG_SpecificCircumstanceIndicatorInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
		}

		public void TestZG_AgreedPlace_ValidUNLOCODE()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ShipmentIncoTermPlace = "abc";
				declaration.ZG_AgreedPlaceCode = Core.Constants.CountryCodes.Belgium;
				AssertEquals("AgreedPlaceCode is country", "abc", declaration.JE_ShipmentIncoTermPlace);

				declaration.ZG_AgreedPlaceCode = "BEAAA";
				AssertEquals("Invalid Unloco Code", "abc", declaration.JE_ShipmentIncoTermPlace);

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					declaration.JE_ShipmentIncoTermPlace = "abc";
					declaration.ZG_AgreedPlaceCode = "BEANR";
					AssertEquals("AgreedPlaceCode enabled - Valid Unloco Code", ZString.Empty, declaration.JE_ShipmentIncoTermPlace);
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
				{
					declaration.JE_ShipmentIncoTermPlace = "abc";
					declaration.ZG_AgreedPlaceCode = "BEANR";
					AssertEquals("AgreedPlaceCode disabled - Valid Unloco Code", "abc", declaration.JE_ShipmentIncoTermPlace);
				}
			});
		}

		protected override BusinessObject GetNewBusinessObject() => GetJobDeclaration();

		JobDeclaration GetJobDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}
	}
}
