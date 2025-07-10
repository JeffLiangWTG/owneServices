using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM416;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging.AIS;

namespace Enterprise.Customs.IE.Messaging
{
	public class IM416Provider : IIM416Provider
	{
		public IM416Provider(Im416 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im416 xmlObject;

		public ZString AdditionalDeclarationType => xmlObject.ImportOperation?.AdditionalDeclarationType;

		public ZString LocalReferenceNumber => xmlObject.ImportOperation?.Lrn;

		public ZDateTime RejectionDate => (xmlObject.ImportOperation?.RejectionDate).ConvertToZDateTime();

		public ZString RejectionMotivationText => xmlObject.ImportOperation?.RejectionMotivationText;

		public IReadOnlyCollection<MFunctionalError01Provider> FunctionalErrors => functionalErrors ?? (functionalErrors = xmlObject.FunctionalError?.Select(x => new MFunctionalError01Provider(x)).ToArray() ?? Array.Empty<MFunctionalError01Provider>());
		IReadOnlyCollection<MFunctionalError01Provider> functionalErrors;

		public ZBool HasFunctionalErrors => xmlObject.FunctionalError?.Count > 0;

		public string EntryStatus
		{
			get
			{
				if (AdditionalDeclarationType.EqualsIgnoringCase(AdditionalDeclarationTypeList.X) ||
					AdditionalDeclarationType.EqualsIgnoringCase(AdditionalDeclarationTypeList.Y) ||
					AdditionalDeclarationType.EqualsIgnoringCase(AdditionalDeclarationTypeList.Z))
				{
					return AISEntryStatusList.Codes.AwaitingSupplementaryDeclaration;
				}
				else
				{
					return AISEntryStatusList.Codes.Rejected;
				}
			}
		}
	}
}
