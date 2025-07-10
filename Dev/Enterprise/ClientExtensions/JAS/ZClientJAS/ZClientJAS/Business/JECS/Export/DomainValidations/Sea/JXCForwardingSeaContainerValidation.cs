
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations
{
	public class JXCForwardingSeaContainerValidation : AutoJobContainerValidation
	{
		public JXCForwardingSeaContainerValidation(ForwardingContainer parent)
			: base(parent)
		{
		}

		protected override void CheckJC_ContainerNum()
		{
			new ValidationHelper().AddJXCWarningIfNotEntered(Parent.JC_ContainerNumInfo);
		}
	}
}
