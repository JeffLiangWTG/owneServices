using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class BaseCusSeaManOBLHeaderValidation : Customs.Business.CusSeaManOBLHeaderValidation
	{
		public BaseCusSeaManOBLHeaderValidation(BaseCusSeaManOBLHeader parent)
			: base(parent)
		{
		}

		protected override void CheckBO_RL_NKLoadPort()
		{
			base.CheckBO_RL_NKLoadPort();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.BO_RL_NKLoadPortInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.BO_RL_NKLoadPortInfo, Parent.Lookups.LoadPorts);
			ZString portValidation = MessageValidation.ValidatePortType(Parent.BO_RL_NKLoadPort, false, true);
			if (!portValidation.IsEmpty)
			{
				Parent.BO_RL_NKLoadPortInfo.AddNotification(NotificationType.Warning, portValidation);
			}
		}

		protected override void CheckBO_RL_NKDestinationPort()
		{
			base.CheckBO_RL_NKDestinationPort();

			MessageValidation.CheckEntered(Parent.BO_RL_NKDestinationPortInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.BO_RL_NKDestinationPortInfo, Parent.Lookups.DestinationPorts);
			ZString portValidation = MessageValidation.ValidatePortType(Parent.BO_RL_NKDestinationPort, false, true);
			if (!portValidation.IsEmpty)
			{
				Parent.BO_RL_NKDestinationPortInfo.AddNotification(NotificationType.Warning, portValidation);
			}
		}

		#region Implementation

		protected internal MessageValidation MessageValidation
		{
			get
			{
				if (fMessageValidation == null)
				{
					fMessageValidation = new MessageValidation(Parent);
				}
				return fMessageValidation;
			}
		}
		MessageValidation fMessageValidation;

		#endregion
	}
}
