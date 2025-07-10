using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC037C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC037CProvider
	{
		public CC037CProvider(Cc037CType xmlObject)
		{
			Argument.NotNull(xmlObject, nameof(xmlObject));
			this.guaranteeReference = xmlObject.GuaranteeReference;
			this.requester = xmlObject.Requester;
		}
		readonly Collection<GuaranteeReferenceType07> guaranteeReference;
		readonly RequesterType02 requester;

		public ZString RequesterIdentificationNumber => requester?.IdentificationNumber;

		public ZString RequesterIdentificationRole => requester?.Role;

		public IReadOnlyCollection<CC037CGuaranteeReferenceProvider> GuaranteeReferences => guaranteeReferencesCached ?? (guaranteeReferencesCached = guaranteeReference?.Select(x => new CC037CGuaranteeReferenceProvider(x)).ToArray() ?? Array.Empty<CC037CGuaranteeReferenceProvider>());
		IReadOnlyCollection<CC037CGuaranteeReferenceProvider> guaranteeReferencesCached;
	}
}
