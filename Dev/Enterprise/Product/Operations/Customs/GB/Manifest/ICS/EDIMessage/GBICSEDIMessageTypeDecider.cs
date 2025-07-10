using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.ICS
{
	public class GBICSEDIMessageTypeDecider : TypeDecider, Integration.Customs.GB.GBICS.IGBICSEDIMessageTypeDecider
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
			var appCode = row[EDIMessageSchema.Constants.EM_ApplicationCode].ToString().Trim();
			Type result = null;
			switch (appCode)
			{
				case ApplicationCodeList.Codes.GbMessageICSGreatBritain:
					result = typeof(IcsSsGreatBritainEDIMessage);
					break;

				case ApplicationCodeList.Codes.GbMessageICSNorthernIreland:
					result = typeof(IcsNorthernIrelandEDIMessage);
					break;
			}
			return result;
		}
	}
}
