using Enterprise.Core.Forms;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Licensing
{
	public static class ILicenceCheckpointExtensions
	{
		public static void ShowLastError(this ILicenceCheckpoint checkpoint)
		{
			using (var form = new LicenceErrorForm(checkpoint.DisplayName, checkpoint.LastReasonForNotAllowing))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}
	}
}
