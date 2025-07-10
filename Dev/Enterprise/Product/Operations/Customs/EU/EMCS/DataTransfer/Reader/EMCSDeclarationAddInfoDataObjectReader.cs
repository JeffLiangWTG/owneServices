using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.EU.EMCS.DataTransfer
{
	public class EMCSDeclarationAddInfoDataObjectReader : DeclarationAddInfoDataObjectReader
	{
		public EMCSDeclarationAddInfoDataObjectReader(IXmlImportLogger logger, UniversalDataObjectReaderHelper helper) : base(logger, helper)
		{
		}

		protected override void ReadCore(IAddInfoManager addInfoManager, IColumnIndexer row, IEnumerable<AddInfo> addInfoCollection, IOrganizationAddressCollectionParent organizationAddresContainer, IDictionary<ZString, AddInfoPropertyNameAndValueParser> infoMappings, Dictionary<string, ValueSetter> delaySetters)
		{
			base.ReadCore(addInfoManager, row, FixJourneyTime(addInfoCollection), organizationAddresContainer, infoMappings, delaySetters);
		}

		static IEnumerable<AddInfo> FixJourneyTime(IEnumerable<AddInfo> addInfoCollection)
		{
			var addInfos = addInfoCollection.ToArray();
			var journeyTimeAddInfo = addInfos.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfo.Keys.JourneyTime);
			if (journeyTimeAddInfo != null)
			{
				var journeyTimeStr = journeyTimeAddInfo.Value.GetValueOrDefault();
				if (!journeyTimeStr.IsEmpty)
				{
					journeyTimeAddInfo.Value = journeyTimeStr.PadLeft(3, ' ');
				}
			}

			return addInfos;
		}
	}
}
