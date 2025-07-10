using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using NotificationType = CargoWise.EntityFramework.NotificationType;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public abstract class CommonExportOfficeCodeValidation : OfficeCodeValidation
	{
		protected CommonExportOfficeCodeValidation(OfficeCode parent) : base(parent)
		{
		}

		protected override bool WasDataChanged(JobDeclaration declaration, ZString code)
		{
			var result = false;
			if (code == EuOfficeCodesTypes.Codes.OfficeOfExit)
			{
				result = IsOfficeCodeChanged(declaration.OriginalExitOffice);
			}
			return result;
		}

		protected override INotificationType NotificationTypeForEmptyCY_Data => NotificationType.MessageError;
	}
}
