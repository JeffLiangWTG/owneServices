#if DEBUG
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Integration
{
	public interface IAccountingTestDataCreator
	{
		void CreatePeriods(int periodNumber, ZDateTime startDate, ZDateTime endDate, bool isClosed);

		void CreateAccountingData(ZGuid jobHeader1PK, ZGuid jobHeader2PK, ZDateTime postDate);

		void CreateAccountingData(ZGuid jobHeaderPK, ZDateTime postDate, bool isHotChequeCancelled, bool isHotChequeLinkedToAH);

		void CreateConsolAccountingData(ZGuid jobHeaderPK, ZGuid jobConsolPK, ZDateTime postDate, bool isHotChequeCancelled, bool isHotChequeLinkedToAH);

		void CreateJobHeader(BusinessObject jobHeaderParent, ZGuid organisationPKLocalCharges);

		void AddChargeLineToCreatedJobHeader(ZString chargeCode, ZString description, ZDecimal amount, ZString currencyCode);

		void CreateApprovalRequest(BusinessObject parent, ZGuid menuItemPK, int[] authorizationLevel, ZString description);
	}
}
#endif
