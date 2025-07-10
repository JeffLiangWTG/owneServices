using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC561C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES
{
	public class CC561CProvider
	{
		public CC561CProvider(Cc561C xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Cc561C xmlObject;

		public ZString MovementReferenceNumber => xmlObject.ExportOperation?.Mrn;
		public ZDateTime ControlNotificationDateAndTime => new ZDateTime(xmlObject.ExportOperation?.ControlNotificationDateAndTime);
		public IReadOnlyCollection<ControlTypeProvider> ControlTypes => controlTypes ?? (controlTypes = xmlObject.TypeOfControls?.Select(x => new ControlTypeProvider()
		{
			SequenceNumber = x.SequenceNumber,
			Type = x.Type,
			Text = x.Text
		}).ToArray() ?? Array.Empty<ControlTypeProvider>());
		ControlTypeProvider[] controlTypes;
	}
}
