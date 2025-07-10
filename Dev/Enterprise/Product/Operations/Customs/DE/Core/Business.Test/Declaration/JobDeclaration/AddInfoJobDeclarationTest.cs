using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(JobDeclaration))]
	class AddInfoJobDeclarationBOTest : NonPersistentBusinessObjectTestCase
	{
		public void TestZG_VATDeferTypeMaxLength()
		{
			AssertEquals(3, GetJobDeclaration().ZG_VATDeferTypeInfo.MaxLength);
		}

		public void TestZG_VATDeferNumberMaxLength()
		{
			AssertEquals(10, GetJobDeclaration().ZG_VATDeferNumberInfo.MaxLength);
		}

		public void TestZG_VATDeferNumberList()
		{
			AssertEquals("AddInfoLookups.VATAccountNumberList", GetJobDeclaration().ZG_VATDeferNumberInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
		}

		public void TestZG_SpecificCircumstanceIndicatorMaxLength()
		{
			AssertEquals(3, GetJobDeclaration().ZG_SpecificCircumstanceIndicatorInfo.MaxLength);
		}

		protected override BusinessObject GetNewBusinessObject() => GetJobDeclaration();

		JobDeclaration GetJobDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}
	}
}
