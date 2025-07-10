namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	using System;
	using CargoWise.Types;
	using Enterprise.Customs.Business.MessageProcessors;
	using Enterprise.Customs.Common.MessageBuilders;
	using Enterprise.Freight.CFS.Business;
	using Enterprise.Freight.Forwarding.Business;
	using Enterprise.ZArchitecture.Modules;

	class LinkProvider
	{
		internal static ZString GetLink(IEDIFACTMessageAttachee linkedObject)
		{
			if (linkedObject != null)
			{
				var job = linkedObject.TopLevelBusinessObject;

				var declaration = job as JobDeclaration;
				if (declaration != null)
				{
					return EmailDefBuilder.GetJobLink(declaration, declaration.JE_DeclarationReference);
				}

				var shipment = job as ForwardingShipment;
				if (shipment != null)
				{
					IControllerIDProvider controllerIDProvider = shipment;
					if (shipment.JS_UniqueConsignRef.StartsWith("H", StringComparison.OrdinalIgnoreCase))
					{
						controllerIDProvider = (IControllerIDProvider)shipment.Factory.Load<CFSShipment>(shipment.PK) ?? shipment;
					}
					return EmailDefBuilder.GetJobLink(controllerIDProvider, shipment.JS_UniqueConsignRef);
				}

				var consol = job as ForwardingConsol;
				if (consol != null)
				{
					return EmailDefBuilder.GetJobLink(consol, consol.JK_UniqueConsignRef);
				}
			}
			return ZString.Empty;
		}
	}
}
