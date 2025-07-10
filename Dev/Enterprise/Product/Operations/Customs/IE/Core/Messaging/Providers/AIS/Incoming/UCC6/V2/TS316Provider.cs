using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS316;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Messaging
{
	public class TS316Provider
	{
		public TS316Provider(Ts316 xmlObject)
		{
			this.xmlObject = xmlObject;
			declaration = Argument.NotNull(xmlObject.Declaration, nameof(xmlObject.Declaration));
		}
		readonly Ts316 xmlObject;
		readonly DeclarationType11 declaration;

		public ZString LRN => declaration.Lrn;

		public ZDateTime RejectionDate => declaration.RejectionDate.ConvertToZDateTime();

		public ZString RejectionMotivationText => declaration.RejectionMotivationText;

		public IReadOnlyCollection<MFunctionalError01Provider> FunctionalErrors => functionalErrors ??= xmlObject.FunctionalError?.Select(x => new MFunctionalError01Provider(x)).ToArray() ?? Array.Empty<MFunctionalError01Provider>();
		IReadOnlyCollection<MFunctionalError01Provider> functionalErrors;
	}
}
