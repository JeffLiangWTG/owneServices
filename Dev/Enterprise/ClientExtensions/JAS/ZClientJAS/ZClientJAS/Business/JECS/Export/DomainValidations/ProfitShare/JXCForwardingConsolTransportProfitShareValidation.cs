using System;
using Enterprise.Freight.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations
{
	public class JXCForwardingConsolTransportProfitShareValidation : JXCForwardingConsolTransportValidation
	{
		public JXCForwardingConsolTransportProfitShareValidation(Transport parent)
			: base(parent)
		{
		}

		public override Type AutoValidationType
		{
			get { return typeof(JXCForwardingConsolTransportProfitShareValidation); }
		}

		protected override void CheckJW_ETD()
		{
			if (RequiresValidation(Core.Constants.TransportModes.Air))
			{
				ValidationHelper.AddJXCWarningIfNotEntered(Parent.JW_ETDInfo);
			}
		}
	}
}
