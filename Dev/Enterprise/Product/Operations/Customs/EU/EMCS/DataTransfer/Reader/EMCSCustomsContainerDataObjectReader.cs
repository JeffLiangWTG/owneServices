using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.EU.EMCS.DataTransfer
{
	public class EMCSCustomsContainerDataObjectReader : CustomsContainerDataObjectReader<BaseJobDeclaration, BaseCusContainer>
	{
		public EMCSCustomsContainerDataObjectReader(Container containerDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, EMCSJobDeclaration declaration, ILandedCostDataReader landedCostDataReader = null)
			: base(containerDataObject, logger, helper, declaration, landedCostDataReader)
		{
		}

		protected override void FillCustomizedFields(BaseCusContainer container, Container dataObject)
		{
			if (container is EMCSCusContainer cnt)
			{
				var addInfos = dataObject.AddInfoCollection ?? new List<AddInfo>();
				if (addInfos.Any())
				{
					var sealDetails = addInfos.GetZStringValue(Constants.AddInfo.Keys.SealDetails) ?? ZString.Empty;
					if (!sealDetails.IsEmpty)
					{
						cnt.SealDetails = sealDetails.Left(EMCSCusContainer.Schema.SealDetails_MaxLength);
					}

					var comment = addInfos.GetZStringValue(Constants.AddInfo.Keys.Comment) ?? ZString.Empty;
					if (!comment.IsEmpty)
					{
						cnt.Comment = comment.Left(EMCSCusContainer.Schema.SealDetails_MaxLength);
					}
				}
			}
		}
	}
}
