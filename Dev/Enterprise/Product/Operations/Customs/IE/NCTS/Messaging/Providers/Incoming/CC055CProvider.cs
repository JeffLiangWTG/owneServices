using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC055C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC055CProvider
	{
		Cc055CType XmlObject { get; }

		public CC055CProvider(Cc055CType xmlObject)
		{
			XmlObject = xmlObject;
			this.guaranteeReference = xmlObject.GuaranteeReference;
		}
		readonly Collection<GuaranteeReferenceType08> guaranteeReference;

		public ZString MovementReferenceNumber => XmlObject.TransitOperation?.Mrn ?? ZString.Empty;

		public ZDate DeclarationAcceptanceDate => new ZDate(XmlObject.TransitOperation?.DeclarationAcceptanceDate);

		public IReadOnlyCollection<CC055CGuaranteeReferenceProvider> GuaranteeReferences => guaranteeReferencesCached ?? (guaranteeReferencesCached = guaranteeReference?.Select(x => new CC055CGuaranteeReferenceProvider(x)).ToArray() ?? Array.Empty<CC055CGuaranteeReferenceProvider>());
		IReadOnlyCollection<CC055CGuaranteeReferenceProvider> guaranteeReferencesCached;
	}
}
