using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(DV1DocDataObject))]
	class DV1DocDataObjectTest : NonPersistentBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestEntries()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header1 = declaration.CustomsEntryHeaders.AddNew();
			var header2 = declaration.CustomsEntryHeaders.AddNew();

			var dV1Certificate = new DV1CertificateWrapper(declaration) as IDV1Certificate;
			var dV1DocDataObject = new DV1DocDataObject(dV1Certificate);

			NUnit.Framework.Assert.That(dV1DocDataObject.Entries.Count, NUnit.Framework.Is.EqualTo(2), "Precondition: Entries count");
			NUnit.Framework.Assert.That(dV1DocDataObject.Entries.Select(x => x.EntryHeader), NUnit.Framework.Is.EqualTo(new[] { header1, header2 }), "Entries in DV1DocDataObject");
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new DV1DocDataObject(new DV1CertificateWrapper(declaration.CustomsEntryHeaders.AddNew()));
		}
	}
}
