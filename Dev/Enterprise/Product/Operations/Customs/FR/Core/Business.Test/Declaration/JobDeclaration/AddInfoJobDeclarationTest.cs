using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(JobDeclaration))]
	sealed class AddInfoJobDeclarationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestZG_VATCANACodeList()
		{
			AssertEquals("AddInfoLookups.VatCanaList", GetJobDeclaration().ZG_VATCANACodeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
		}

		public void TestZG_VATCANACode_MaxLength()
		{
			AssertEquals(4, declaration.ZG_VATCANACodeInfo.MaxLength);
		}

		public void TestZG_VATCANACode_Caption()
		{
			AssertEquals("Caption", "VAT CANA Code", DataBoundResourceStrings.GetDataForProperty(declaration.ZG_VATCANACodeInfo).Caption);
		}

		public void TestZG_VATDeferNumber_MaxLength()
		{
			AssertEquals("Max length of the property ZG_VATDeferNumber should match the max length of CPH_Number", AutoCusPermitHeader.Schema.CPH_NumberMaxLength, declaration.ZG_VATDeferNumberInfo.MaxLength);
		}

		protected override BusinessObject GetNewBusinessObject() => GetJobDeclaration();

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;

		JobDeclaration GetJobDeclaration() => declaration;
	}
}
