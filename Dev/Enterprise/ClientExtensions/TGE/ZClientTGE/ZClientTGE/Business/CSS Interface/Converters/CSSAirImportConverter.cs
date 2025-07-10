using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Client.TGE.Business
{
	internal class CSSAirImportConverter : CSSConverter
	{
		public CSSAirImportConverter(INotifications notification, BusinessObjectFactory factory) : base(notification, factory) { }

		protected override BusinessObject CastBusinessObject(BusinessObject bizObj)
		{
			if (bizObj.GetType() != typeof(CusHAWB))
			{
				throw new ArgumentException("Unknown argument type.  Expecting a CusHAWB object.", nameof(bizObj));
			}
			return bizObj;
		}

		protected override CSSMapper GetMapper()
		{
			return new CSSAirImportMapper();
		}
	}
}
