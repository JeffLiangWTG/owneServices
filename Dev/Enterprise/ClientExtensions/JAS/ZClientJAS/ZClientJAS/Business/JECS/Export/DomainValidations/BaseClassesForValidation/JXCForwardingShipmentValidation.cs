using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations
{
	public class JXCForwardingShipmentValidation : AutoJobShipmentValidation
	{
		public JXCForwardingShipmentValidation(JASForwardingShipment parent)
			: base(parent)
		{
		}

		public new JASForwardingShipment Parent
		{
			get { return (JASForwardingShipment)base.Parent; }
		}

		public virtual JobDocAddressValidation JobDocAddressValidation(JobDocAddress addressToValidate)
		{
			return null;
		}
	}
}

#region Implementation
#endregion
