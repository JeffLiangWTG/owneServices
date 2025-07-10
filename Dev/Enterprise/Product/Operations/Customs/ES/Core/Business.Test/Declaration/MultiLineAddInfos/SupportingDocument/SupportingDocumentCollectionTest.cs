using System;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	[TestedType(typeof(SupportingDocumentCollection))]
	public class SupportingDocumentCollectionTest : EU.Business.Declaration.MultiLineAddInfos.Testing.SupportingDocumentCollectionTest
	{
		protected override CusSupportingInfoCollection<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument> GetCusSupportingInfoCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new SupportingDocumentCollection(declaration);
		}

		protected override Type ExpectedHelperType => typeof(ESSupportingDocumentHelper);
	}
}
