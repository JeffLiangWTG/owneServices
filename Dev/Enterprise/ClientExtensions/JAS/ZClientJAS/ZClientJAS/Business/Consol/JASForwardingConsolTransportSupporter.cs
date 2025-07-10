using CargoWise.EntityFramework;
using Enterprise.Client.JAS.Business.JXC.Export.Validations;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Client.JAS.Business
{
	internal sealed class JASForwardingConsolTransportSupporter : ForwardingConsolTransportSupporter<JASForwardingConsol>
	{
		public JASForwardingConsolTransportSupporter(JASForwardingConsol parent)
			: base(parent) { }

		public override JobConsolTransportValidation GetNewTransportValidator(Transport transport)
		{
			JobConsolTransportValidation result = base.GetNewTransportValidator(transport);
			result.Add(GetJXCTransportValidation(transport));
			return result;
		}

		internal
 JXCForwardingConsolTransportValidation GetJXCTransportValidation(Transport transport)
		{
			if (transport.Factory.HasDomainValidation)
			{
				foreach (DomainValidationGroup validationGroup in transport.Factory.Validation.AllValidationGroups)
				{
					ZValidation[] validations = validationGroup.CreateDomainValidation(transport);
					foreach (ZValidation validation in validations)
					{
						JXCForwardingConsolTransportValidation result = validation as JXCForwardingConsolTransportValidation;
						if (result != null)
						{
							return result;
						}
					}
				}
			}

			return new JXCForwardingConsolTransportValidation(transport);
		}
	}
}
