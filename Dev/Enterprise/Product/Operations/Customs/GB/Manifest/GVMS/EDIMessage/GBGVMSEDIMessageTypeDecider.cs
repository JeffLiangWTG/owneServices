using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.GVMS
{
	public class GBGVMSEDIMessageTypeDecider : TypeDecider, Integration.Customs.GB.GBGVMS.IGBGVMSEDIMessageTypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return null;
		}
		public override Type GetTypeForNew()
		{
			return null;
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			return typeof(GVMSEDIMessage);
		}
	}
}
