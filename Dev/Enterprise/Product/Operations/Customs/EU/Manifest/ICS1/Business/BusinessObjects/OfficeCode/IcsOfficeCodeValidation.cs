using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Manifest.Business
{
	public class IcsOfficeCodeValidation : EU.Business.EuOfficeCodeValidation
	{
		public IcsOfficeCodeValidation(IcsOfficeCode parent) : base(parent)
		{
		}

		protected override CargoWise.ComponentModel.INotificationType OfficeTypeRepeatedMoreThanMaxNotificationType => NotificationType.Error;
	}
}
