using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.EMCS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSDocument))]
	sealed class EMCSDocumentTest : EMCSDocumentAbstractTest<EMCSDocument>
	{
		protected override IEnumerable<EMCSDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<EMCSJobDeclaration>();
			yield return (EMCSDocument)declaration.Documents.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject() => document;

		protected override Type GetValidationType => typeof(EMCSDocumentValidation);

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.NewWithValidTestData<EMCSJobDeclaration>();
			document = (EMCSDocument)declaration.Documents.AddNew();
		}
		EMCSDocument document;
	}
}
