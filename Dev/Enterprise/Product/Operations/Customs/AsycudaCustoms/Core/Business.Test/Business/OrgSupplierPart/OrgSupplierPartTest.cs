using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(OrgSupplierPart))]
	class OrgSupplierPartTest : Customs.Business.Testing.OrgSupplierPartTest
	{
		protected override Type ExpectedClassificationCollectionType => typeof(ClassificationCollection<CusClassification>);

		protected override BusinessObject GetNewBusinessObject() => OrgSupplierPart.New(Factory);
	}
}
