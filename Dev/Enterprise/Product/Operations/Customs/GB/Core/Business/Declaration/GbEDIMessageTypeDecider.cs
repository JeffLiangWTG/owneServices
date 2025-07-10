using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class GbEDIMessageTypeDecider : TypeDecider, Integration.Customs.GB.IGBEDIMessageTypeDecider
	{
		public override Type GetTypeForNew() => null;

		public override Type GetTypeForBinding() => null;

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var applicationCode = row[EDIMessageSchema.Constants.EM_ApplicationCode].ToString().Trim();
			switch (applicationCode)
			{
				case EDIMessage.ApplicationCodes.GbCDSViaCCSUK:
				case EDIMessage.ApplicationCodes.GbCustomsDeclarationServices:
				case EDIMessage.ApplicationCodes.GbCDSDISQuery:
					return ObjectFactory.Get<Integration.Customs.GB.GBCDS.IGBCDSEDIMessageTypeDecider>().GetTypeForLoad(row, factory);
				case EDIMessage.ApplicationCodes.GbCustomsGVMSManifest:
					return ObjectFactory.Get<Integration.Customs.GB.GBGVMS.IGBGVMSEDIMessageTypeDecider>().GetTypeForLoad(row, factory);
				case EDIMessage.ApplicationCodes.GbMessageICSGreatBritain:
				case EDIMessage.ApplicationCodes.GbMessageICSNorthernIreland:
					return ObjectFactory.Get<Integration.Customs.GB.GBICS.IGBICSEDIMessageTypeDecider>().GetTypeForLoad(row, factory);
				case EDIMessage.ApplicationCodes.GbCustomsEMCS:
					return ObjectFactory.Get<Integration.Customs.GBEMCS.IEMCSEDIMessageTypeDecider>().GetTypeForLoad(row, factory);
				case EDIMessage.ApplicationCodes.GbCustomsNCTS:
					return ObjectFactory.Get<Integration.Customs.GBNCTS.INCTSEDIMessageTypeDecider>().GetTypeForLoad(row, factory);
				default:
					return typeof(GbEDIMessage);
			}
		}
	}
}
