using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Duimp.Outgoing;

namespace Enterprise.Customs.BR.Business.Duimp
{
	public class DuimpDiagnosisProvider : IDuimpDiagnosis
	{
		public DuimpDiagnosisProvider(DuimpMessageSendingObject sendingObject)
		{
			this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
		}
		readonly DuimpMessageSendingObject sendingObject;

		public int TotalItem => sendingObject.Header.MergedLines.Count;
	}
}
