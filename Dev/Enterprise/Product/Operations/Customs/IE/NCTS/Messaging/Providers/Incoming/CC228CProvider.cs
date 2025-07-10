using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC228C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC228CProvider
	{
		Cc228CType XmlObject { get; }

		public CC228CProvider(Cc228CType xmlObject)
		{
			XmlObject = Argument.NotNull(xmlObject, nameof(xmlObject));
			guaranteeReference = XmlObject.GuaranteeReference?.ToArray() ?? Array.Empty<GuaranteeReferenceType10>();
			guarantor = XmlObject.Guarantor;
		}
		readonly IReadOnlyCollection<GuaranteeReferenceType10> guaranteeReference;
		readonly GuarantorType03 guarantor;

		public IReadOnlyCollection<CC228CGuaranteeReferenceProvider> GuaranteeReferences => guaranteeReferencesCached ?? (guaranteeReferencesCached = guaranteeReference.Select(CC228CGuaranteeReferenceProvider.Create).ToArray());
		IReadOnlyCollection<CC228CGuaranteeReferenceProvider> guaranteeReferencesCached;

		public CC228CGuarantorProvider Guarantor => guarantor == null ? null : guarantorCached ?? (guarantorCached = new CC228CGuarantorProvider(guarantor));
		CC228CGuarantorProvider guarantorCached;
	}
}
