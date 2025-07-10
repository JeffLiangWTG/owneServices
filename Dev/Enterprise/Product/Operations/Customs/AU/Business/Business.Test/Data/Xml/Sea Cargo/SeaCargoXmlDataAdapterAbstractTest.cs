using Enterprise.DataTransfer.DataAdapters;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class SeaCargoXmlDataAdapterAbstractTest<TBusinessObject, TValueObject> : BaseCargoXmlDataAdapterTest<TBusinessObject, TValueObject>
			where TBusinessObject : CusSCAOceanBill
			where TValueObject : Xsd.Consol
	{
		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var oceanBill = Factory.New<TBusinessObject>();
			return new BusinessObjectAndExpectedOutputFileName(oceanBill, GetEmbeddedResourcePath("SeaCargoExample.xml"), ValidationKind.None, "Empty OceanBill");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects() => System.Array.Empty<BusinessObjectAndExpectedOutputFileName>();

		protected override ValueObjectDataAdapter<TBusinessObject, TValueObject> GetNewBizObjXmlDataAdapter() => new SeaCargoXmlDataAdapter<TBusinessObject, TValueObject>();
	}
}
