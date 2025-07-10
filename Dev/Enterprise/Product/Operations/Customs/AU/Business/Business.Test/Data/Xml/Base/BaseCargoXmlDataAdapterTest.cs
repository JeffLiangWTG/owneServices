using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.DataTransfer.Xml.Testing;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class BaseCargoXmlDataAdapterTest<TBusinessObject, TValueObject> : ValueObjectDataAdapterTest<TBusinessObject, TValueObject>
			where TBusinessObject : BusinessObject
			where TValueObject : Xsd.Consol
	{
		protected override bool IsExportToValueObjectSupported => false;

		protected override bool IsImportFromValueObjectSupported => true;

		protected override string ExpectedRootCollectionElementName => "Consols";

		protected override string ExpectedRootElementName => "Consol";

		protected override bool IsExportToCollectionSupported => false;

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample() => GetEmptyBizObjSample();

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample() => GetEmptyBizObjSample();

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects() => System.Array.Empty<BusinessObjectAndExpectedOutputFileName>();

		protected string GetEmbeddedResourcePath(string fileName) => "Enterprise.Customs.AU.Declaration.Business.Testing.Data.Xml.Testing." + fileName;

		protected override void SetUp()
		{
			base.SetUp();
			embeddedResourceRetriever = new EmbeddedResourceRetriever();
		}

		protected override void TearDown()
		{
			base.TearDown();
			embeddedResourceRetriever.Dispose();
		}
		protected EmbeddedResourceRetriever embeddedResourceRetriever;
	}
}
