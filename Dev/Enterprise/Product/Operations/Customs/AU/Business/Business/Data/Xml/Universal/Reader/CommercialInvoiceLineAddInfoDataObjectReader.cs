using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CommercialInvoiceLineAddInfoDataObjectReader : AddInfoDataObjectReader
	{
		public CommercialInvoiceLineAddInfoDataObjectReader(IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, SchemaStringColumn addInfoColumn)
			: base(logger, helper, addInfoColumn)
		{
		}

		protected override void ReadCore(IAddInfoManager addInfoManager, IColumnIndexer row, IEnumerable<UniversalAddInfo> addInfoCollection,
			IOrganizationAddressCollectionParent organizationAddresContainer, IDictionary<ZString, AddInfoPropertyNameAndValueParser> infoMappings, Dictionary<string, ValueSetter> delaySetters)
		{
			var newAddInfoCollection = new List<UniversalAddInfo>(addInfoCollection);
			var tilvAddInfo = newAddInfoCollection.GetTILV4Warehouse();
			if (tilvAddInfo != null)
			{
				newAddInfoCollection.Remove(tilvAddInfo);
			}
			base.ReadCore(addInfoManager, row, newAddInfoCollection, organizationAddresContainer, infoMappings, delaySetters);
		}

		protected override void SetValue(IAddInfoManager addInfoManager, IColumnIndexer row, ZString key, ZString value, string propertyName, Dictionary<string, ValueSetter> delaySetters)
		{
			if (propertyName == AUAddInfoSchema.ZA_RelatedExportPermitDate_Hidden.Name)
			{
				value = ((ZDateTime)BaseAddInfo.ConvertToZType(typeof(ZDateTime), value)).ToString(AUAddInfo.DateFormat, CultureInfo.InvariantCulture);
			}

			base.SetValue(addInfoManager, row, key, value, propertyName, delaySetters);
		}
	}
}
