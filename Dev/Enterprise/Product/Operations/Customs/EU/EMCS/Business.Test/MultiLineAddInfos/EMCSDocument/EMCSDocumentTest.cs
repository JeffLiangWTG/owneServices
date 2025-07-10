using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.EU;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	public abstract class EMCSDocumentAbstractTest<T> : Customs.Business.Testing.CusSupportingInfoTest<T> where T : EMCSDocument
	{
		public void TestValidationType()
		{
			var document = Factory.New<T>();
			AssertEquals(GetValidationType, document.Validation.GetType());
		}

		protected virtual Type GetValidationType => typeof(EMCSDocumentValidation);
	}

	[TestedType(typeof(EMCSDocument))]
	sealed class EMCSDocumentTest : EMCSDocumentAbstractTest<EMCSDocument>
	{
		public void TestCanDelete()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Declaration", true, Factory.New<EMCSDocument>().CanDelete);
				AssertEquals("Declaration no messaging", true, document.CanDelete);
				declaration.JE_MessageStatus = EDIMessage.Status.Sent;
				AssertEquals("Declaration with messaging", false, document.CanDelete);
			});
		}

		public void TestCSI_Description_ReadOnly()
		{
			AssertPropertyReadOnly(() => Factory.New<EMCSDocument>().CSI_DescriptionInfo.ReadOnly, () => document.CSI_DescriptionInfo.ReadOnly);
		}

		public void TestCSI_ReferenceNumber_ReadOnly()
		{
			AssertPropertyReadOnly(() => Factory.New<EMCSDocument>().CSI_ReferenceNumberInfo.ReadOnly, () => document.CSI_ReferenceNumberInfo.ReadOnly);
		}

		public void TestCSI_SubType_ReadOnly()
		{
			AssertPropertyReadOnly(() => Factory.New<EMCSDocument>().CSI_SubTypeInfo.ReadOnly, () => document.CSI_SubTypeInfo.ReadOnly);
		}

		public void TestCheckCSI_ReferenceNumber_MaxLength()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CSI_SubType is empty", 100, document.CSI_ReferenceNumberInfo.MaxLength);
				document.CSI_SubType = "0";
				AssertEquals("CSI_SubType not empty", 35, document.CSI_ReferenceNumberInfo.MaxLength);
			});
		}

		public void TestHumanReadableName()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CSI_SubType", "Document Type", document.CSI_SubTypeInfo.HumanReadableName);
				AssertEquals("CSI_ReferenceNumber", "Reference", document.CSI_ReferenceNumberInfo.HumanReadableName);
				AssertEquals("CSI_Description", "Description", document.CSI_DescriptionInfo.HumanReadableName);
			});
		}

		public void TestSetDefaultValues()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CSI_Type", CusSupportingInfoTypeList.Codes.Certificate, document.CSI_Type);
				AssertEquals("CSI_Code", CusSupportingInfoTypeList.Codes.Certificate, document.CSI_Code);
			});
		}

		protected override IEnumerable<EMCSDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.NewWithValidTestData<EMCSJobDeclaration>();
			yield return declaration.Documents.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject() => document;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<EMCSJobDeclaration>();
			document = declaration.Documents.AddNew();
		}
		EMCSJobDeclaration declaration;
		EMCSDocument document;

		void AssertPropertyReadOnly(Func<bool> noDeclarationPropertyReadOnly, Func<bool> declarationPropertyReadOnly)
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Declaration", false, noDeclarationPropertyReadOnly.Invoke());
				AssertEquals("Declaration no messaging", false, declarationPropertyReadOnly.Invoke());
				declaration.JE_MessageStatus = EDIMessage.Status.Sent;
				AssertEquals("Declaration with messaging", true, declarationPropertyReadOnly.Invoke());
			});
		}
	}
}
