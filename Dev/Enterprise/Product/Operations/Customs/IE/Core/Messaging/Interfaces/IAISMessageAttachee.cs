using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IE.Messaging
{
	public interface IAISMessageAttachee : IMessageAttachee
	{
		void MovementReferenceNumberSetter(ZString mrn, ZDateTime? issueDate = null, ZString? entryStatus = null, ZDateTime? expiryDate = null);

		void SetSimplifiedDeclarationMRN(ZString mrn);

		void SetCustomsRegistrationNumber(ZString crn);

		void SetEntryReleaseDate(ZDateTime releaseDate);

		void PopulateConfirmedDutiesAndTaxes(IEnumerable<IGoodsItemProvider> goodsItems);

		Logs Logs { get; }

		IRequestedDocumentsProvider RequestedDocumentsProvider { get; }
	}
}
