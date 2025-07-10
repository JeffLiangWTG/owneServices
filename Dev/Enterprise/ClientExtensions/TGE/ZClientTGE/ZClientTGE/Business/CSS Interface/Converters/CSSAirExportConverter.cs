using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Client.TGE.Business
{
	internal class CSSAirExportConverter : CSSConverter
	{
		public CSSAirExportConverter(INotifications notification, BusinessObjectFactory factory) : base(notification, factory) { }

		protected override BusinessObject CastBusinessObject(BusinessObject bizObj)
		{
			if (bizObj.GetType() != typeof(JobDeclaration) && bizObj.GetType() != typeof(ForwardingShipment))
			{
				throw new ArgumentException("Unknown argument type.  Expecting either a JobDeclaration or ForwardingShipment object.", nameof(bizObj));
			}
			return bizObj;
		}

		protected override CSSMapper GetMapper()
		{
			return new CSSAirExportMapper();
		}
	}
}
