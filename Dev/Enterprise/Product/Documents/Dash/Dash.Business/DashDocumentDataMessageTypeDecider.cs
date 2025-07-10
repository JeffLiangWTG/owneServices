using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Dash.Integration;

namespace Enterprise.Dash.Business
{
	public class DashDocumentDataMessageTypeDecider : TypeDecider, IDashDocumentDataMessageTypeDecider
	{
		public override Type GetTypeForNew()
		{
			return null;
		}

		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			return typeof(DashDocumentDataMessage);
		}
	}
}
