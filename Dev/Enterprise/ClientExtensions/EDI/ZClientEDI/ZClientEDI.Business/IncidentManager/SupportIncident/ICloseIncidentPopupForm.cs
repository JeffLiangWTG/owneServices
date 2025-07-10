using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public interface ICloseIncidentPopupForm
	{
		ZDialogResult ShowDialogAndDispose();
	}
}
