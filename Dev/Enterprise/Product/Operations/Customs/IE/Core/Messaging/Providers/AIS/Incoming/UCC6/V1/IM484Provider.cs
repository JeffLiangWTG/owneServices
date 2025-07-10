using System.Collections.Generic;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM484;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Messaging.UCC6.V1
{
	public class IM484Provider : IIM484Provider
	{
		public IM484Provider(Im484 xmlObject)
		{
			this.xmlObject = xmlObject;
		}

		readonly Im484 xmlObject;

		public ZString MovementReferenceNumber => xmlObject.Declaration?.Mrn ?? ZString.Empty;

		public ZString LocalReferenceNumber => xmlObject.Declaration?.Lrn ?? ZString.Empty;

		public ZDateTime RequestDate => DateTimeProviderHelper.ConvertAisUcc5DateStringToZDateTime(xmlObject.Declaration?.RequestDate);

		public ZDateTime DateLimit => DateTimeProviderHelper.ConvertAisUcc5DateStringToZDateTime(xmlObject.Declaration?.DateLimit);

		public IReadOnlyCollection<IDocumentAdditionalInformationProvider> AdditionalInformations
		{
			get
			{
				if (additionalInformations == null)
				{
					var additionalInformationList = new List<DocumentAdditionalInformationProvider>();
					foreach (var additionalInformation in xmlObject?.GoodsShipment)
					{
						additionalInformationList.Add(new DocumentAdditionalInformationProvider(additionalInformation));
					}

					additionalInformations = additionalInformationList.ToArray();
				}

				return additionalInformations;
			}
		}
		IReadOnlyCollection<IDocumentAdditionalInformationProvider> additionalInformations;
	}
}
