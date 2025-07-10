using System;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations
{
	public class JXCForwardingConsolTransportValidation : ConsolTransportValidation
	{
		public JXCForwardingConsolTransportValidation(Transport parent)
			: base(parent)
		{
		}

		public override Type AutoValidationType
		{
			get { return typeof(JXCForwardingConsolTransportValidation); }
		}

		public bool RequiresValidation(ZString transportMode)
		{
			bool result = false;

			if (Consol != null)
			{
				Transport firstTransportByTransportMode = Consol.Transports.FirstTransportWithTransportMode(transportMode);
				if (firstTransportByTransportMode != null)
				{
					result = firstTransportByTransportMode == Parent;
				}
				else
				{
					result = Consol.Transports.MostInterestingTransport == Parent;
				}
			}

			return result;
		}

		protected ValidationHelper ValidationHelper
		{
			get
			{
				return validationHelper ?? (validationHelper = new ValidationHelper());
			}
		}
		ValidationHelper validationHelper;
	}
}

#region Implementation
#endregion
