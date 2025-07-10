using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC025C;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC025CProvider
	{
		Cc025CType XmlObject { get; }

		public CC025CProvider(Cc025CType xmlObject)
		{
			XmlObject = xmlObject;
		}

		public ZString MRN => XmlObject.TransitOperation?.Mrn ?? ZString.Empty;

		public ZDateTime ReleaseDate => (XmlObject.TransitOperation?.ReleaseDate).ConvertToZDateTime();

		public ZString ReleaseIndicator => XmlObject.TransitOperation?.ReleaseIndicator ?? ZString.Empty;

		public IReadOnlyCollection<CC025CHouseConsignmentProvider> HouseConsignments => houseConsignmentsCached ?? (houseConsignmentsCached = XmlObject.Consignment?.Select(x => new CC025CHouseConsignmentProvider(x)).ToArray() ?? Array.Empty<CC025CHouseConsignmentProvider>());
		IReadOnlyCollection<CC025CHouseConsignmentProvider> houseConsignmentsCached;
	}
}
