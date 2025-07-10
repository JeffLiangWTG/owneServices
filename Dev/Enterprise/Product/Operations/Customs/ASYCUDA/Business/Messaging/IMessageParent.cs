
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public interface IMessageParent
	{
		Logs Logs { get; }
		string SetNewCountryMessagingStatus(ZString newStatus);
		string SetNewCountryCustomsStatus(ZString newStatus);
		void ResetMessageStatus();
		ZString RegistrationNumber { get; }
		bool HasCustomsNumbers { get; }
		bool IsCustomsCleared { get; }
		ISelectionItem SelectionItem { get; }
		ZString MessageStatus { get; }
		ZString CustomsStatus { get; }
		ZString ManifestType { get; }
	}
}
