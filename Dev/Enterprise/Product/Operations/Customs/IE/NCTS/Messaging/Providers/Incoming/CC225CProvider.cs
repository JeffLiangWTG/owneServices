using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC225C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC225CProvider
	{
		Cc225CType XmlObject { get; }

		public CC225CProvider(Cc225CType xmlObject)
		{
			XmlObject = Argument.NotNull(xmlObject, nameof(xmlObject));
			guaranteeReference = XmlObject.GuaranteeReference?.ToArray() ?? Array.Empty<GuaranteeReferenceType09>();
		}
		readonly IReadOnlyCollection<GuaranteeReferenceType09> guaranteeReference;

		public IReadOnlyCollection<CC225CGuaranteeReferenceProvider> GuaranteeReferences => guaranteeReferencesCached ?? (guaranteeReferencesCached = guaranteeReference.Select(CC225CGuaranteeReferenceProvider.Create).ToArray());
		IReadOnlyCollection<CC225CGuaranteeReferenceProvider> guaranteeReferencesCached;
	}
}
