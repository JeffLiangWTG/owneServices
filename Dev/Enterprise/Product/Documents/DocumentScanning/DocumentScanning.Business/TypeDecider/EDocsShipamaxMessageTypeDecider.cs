using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.DocumentScanning.Integration;

namespace Enterprise.DocumentScanning.Business
{
	public class EDocsShipamaxMessageTypeDecider : TypeDecider, IEDocsShipamaxMessageTypeDecider
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
			return typeof(EDocsShipamaxMessage);
		}
	}
}
