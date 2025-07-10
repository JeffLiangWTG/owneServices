using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class KnownTopLevelDataObjectWriter : ITopLevelDataObjectWriter
	{
		public KnownTopLevelDataObjectWriter(ITopLevelDataObject dataObject)
		{
			this.dataObject = dataObject;
		}

		readonly ITopLevelDataObject dataObject;

		ZString ITopLevelDataObjectWriter.EDIMessageSubType => EDIMessageSubTypeList.Codes.XmlUniversalShipment;

		ITopLevelDataObject ITopLevelDataObjectWriter.GetDataObject(BusinessObject sourceBO) => dataObject;

		ZString ITopLevelDataObjectWriter.RootElementName => typeof(Shipment).GetAttribute<RootElementAttribute>().RootElementName;

		DataContextType ITopLevelDataObjectWriter.TopLevelDataContextType => throw new NotSupportedException();
	}
}
