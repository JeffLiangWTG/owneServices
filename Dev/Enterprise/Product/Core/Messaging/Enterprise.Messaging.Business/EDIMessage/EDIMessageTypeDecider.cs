using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Messaging.Business
{
	[Immutable]
	public class EDIMessageTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			string applicationCode = row[EDIMessage.Schema.EM_ApplicationCode].ToString().Trim();
			return GetTypeForApplicationCode(applicationCode, row, factory);
		}

		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForNew()
		{
			return null;
		}

		#region Implementation

		protected internal Type GetTypeForApplicationCode(string applicationCode, DataRow row, BusinessObjectFactory factory)
		{
			//TODO: Push case statement out to each country
			switch (applicationCode)
			{
				case EDIMessage.ApplicationCodes.USeManifest:
					return ((TypeDecider)ObjectFactory.Get<Enterprise.Integration.Customs.US.eManifest.IEDIMessageTypeDecider>()).GetTypeForLoad(row, factory);
				case EDIMessage.ApplicationCodes.AMS:
					TypeDecider aMSTypeDecider = (TypeDecider)ObjectFactory.Get<Enterprise.Integration.Customs.US.USAMS.IEDIMessageTypeDecider>();
					return aMSTypeDecider.GetTypeForLoad(row, factory);
				case EDIMessage.ApplicationCodes.CACustoms:
				case EDIMessage.ApplicationCodes.CAACI:
				case EDIMessage.ApplicationCodes.CAEXP:
				case EDIMessage.ApplicationCodes.CAIMP:
					Type cATypeDeciderType = ObjectFactory.GetType<Enterprise.Integration.Customs.CA.IEDIMessageTypeDecider>();
					TypeDecider cATypeDecider = (TypeDecider)Activator.CreateInstance(cATypeDeciderType);
					return cATypeDecider.GetTypeForLoad(row, factory);
				case EDIMessage.ApplicationCodes.SingaporeTradenet4:
				case EDIMessage.ApplicationCodes.SGCustomsTradenetXML:
				case EDIMessage.ApplicationCodes.SingaporeNationalTradePlatform:
					TypeDecider typeDecider = (TypeDecider)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.Integration.Customs.SG.ISGEDIMessageTypeDecider>());
					return typeDecider.GetTypeForLoad(row, factory);
				case EDIMessage.ApplicationCodes.CMR:
					Type cMRTypeDeciderType = ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICMRTypeDecider>();
					TypeDecider cMRTypeDecider = (TypeDecider)Activator.CreateInstance(cMRTypeDeciderType);
					return cMRTypeDecider.GetTypeForLoad(row, factory);
				case EDIMessage.ApplicationCodes.EXDOC:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.AU.IEXDOCMessage>();
				case EDIMessage.ApplicationCodes.NEXDOCS:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.AU.INEXDOCMessage>();
				case EDIMessage.ApplicationCodes.COLS:
					return ((TypeDecider)ObjectFactory.Get<Enterprise.Integration.Customs.AU.IEDIMessageTypeDecider>()).GetTypeForLoad(row, factory);
				case EDIMessage.ApplicationCodes.Traxon:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.HK.ITraxonMessage>();
				case EDIMessage.ApplicationCodes.NewZealandCustoms:
					var decider = TypeDecider.GetTypeDeciderFromType(ObjectFactory.GetType<Enterprise.Integration.Customs.NZ.INZCMessage>());
					return decider.GetTypeForLoad(row, factory);
				case EDIMessage.ApplicationCodes.SouthAfricanCustoms:
					TypeDecider zATypeDecider = (TypeDecider)ObjectFactory.Get<Enterprise.Integration.Customs.ZA.IEDIMessageTypeDecider>();
					return zATypeDecider.GetTypeForLoad(row, factory);
				case EDIMessage.ApplicationCodes.USCustomsImport:
				case EDIMessage.ApplicationCodes.USCustomsExport:
				case EDIMessage.ApplicationCodes.USeBond:
					TypeDecider uSTypeDecider = (TypeDecider)ObjectFactory.Get<Enterprise.Integration.Customs.US.IEDIMessageTypeDecider>();
					return uSTypeDecider.GetTypeForLoad(row, factory);
				case EDIMessage.ApplicationCodes.USAMA:
					return ((TypeDecider)ObjectFactory.Get<Enterprise.Integration.Customs.US.IAIMEDIMessageTypeDecider>()).GetTypeForLoad(row, factory);
				case EDIMessage.ApplicationCodes.USExportManifest:
					return ((TypeDecider)ObjectFactory.Get<Enterprise.Integration.Customs.US.IUEMEDIMessageTypeDecider>()).GetTypeForLoad(row, factory);
				case EDIMessage.ApplicationCodes.eNett:
					TypeDecider eNettTypeDecider = (TypeDecider)ObjectFactory.Get<IeNettEDIMessageTypeDecider>();
					return eNettTypeDecider.GetTypeForLoad(row, factory);
				case EDIMessage.ApplicationCodes.GbEdifactShared:
				case EDIMessage.ApplicationCodes.GbMcpPortHealth:
				case EDIMessage.ApplicationCodes.GbMcpR01AndR11:
				case EDIMessage.ApplicationCodes.GbMcpR12:
				case EDIMessage.ApplicationCodes.GbCcsuk:
				case EDIMessage.ApplicationCodes.GbNesAllMessageTypes:
				case EDIMessage.ApplicationCodes.GbPentant:
				case EDIMessage.ApplicationCodes.GbCnsEdifactOutboundOnly:
				case EDIMessage.ApplicationCodes.GbMcpEdifactOutboundOnly:
				case EDIMessage.ApplicationCodes.GbCnsCompass:
				case EDIMessage.ApplicationCodes.GbCDSViaCCSUK:
				case EDIMessage.ApplicationCodes.GbCustomsDeclarationServices:
				case EDIMessage.ApplicationCodes.GbCDSDISQuery:
				case EDIMessage.ApplicationCodes.GbMessageICSNorthernIreland:
				case EDIMessage.ApplicationCodes.GbMessageICSGreatBritain:
				case EDIMessage.ApplicationCodes.GbCustomsEMCS:
				case EDIMessage.ApplicationCodes.GbCustomsNCTS:
					return ObjectFactory.Get<Enterprise.Integration.Customs.GB.IGBEDIMessageTypeDecider>().GetTypeForLoad(row, factory);
				case EDIMessage.ApplicationCodes.UniversalDataMessaging:
					var result = typeof(XmlEDIMessage);
					if (Env.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Canada)
					{
						var caXcmlEDIMessageTypeDecider = ObjectFactory.Get<Enterprise.Integration.Customs.CA.IXmlEDIMessageTypeDecider>();
						result = caXcmlEDIMessageTypeDecider.GetXmlEDIMessageType(row, factory);
					}
					return result;
				case EDIMessage.ApplicationCodes.XMS:
				case EDIMessage.ApplicationCodes.NativeDataMessaging:
					return typeof(XmlEDIMessage);
				case EDIMessage.ApplicationCodes.StowPlan:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.US.USAMS.IStowPlanMessage>();
				case EDIMessage.ApplicationCodes.USCustomsDIS:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.US.DIS.IEDIMessage>();
				case EDIMessage.ApplicationCodes.CIM:
					return ObjectFactory.GetType<Enterprise.Freight.Integration.Forwarding.ICIMEDIMessage>();
				case EDIMessage.ApplicationCodes.DECustomsAesSystem:
				case EDIMessage.ApplicationCodes.DECustomsAtlasSystem:
				case EDIMessage.ApplicationCodes.DECustomsEmcsSystem:
					return ((TypeDecider)ObjectFactory.Get<Enterprise.Integration.Customs.DE.IEDIMessageTypeDecider>()).GetTypeForLoad(row, factory);
				case EDIMessage.ApplicationCodes.IECustomsExport:
				case EDIMessage.ApplicationCodes.IECustomsImport:
				case EDIMessage.ApplicationCodes.IECustomsUCC5Import:
				case EDIMessage.ApplicationCodes.IECustomsEMCS:
				case EDIMessage.ApplicationCodes.IECustomsNCTS:
				case EDIMessage.ApplicationCodes.IECustomsAndExcise:
				case EDIMessage.ApplicationCodes.IECustomsPBN:
					return ((TypeDecider)ObjectFactory.Get<Enterprise.Integration.Customs.IE.IEDIMessageTypeDecider>()).GetTypeForLoad(row, factory);
				case EDIMessage.ApplicationCodes.FRCustomsMessage:
					return ObjectFactory.Get<Enterprise.Integration.Customs.FR.IFREDIMessageTypeDecider>().GetTypeForLoad(row, factory);//TODO Change to typeof(FRCINEDImessage) when that type is available
				case EDIMessage.ApplicationCodes.TaiwanCustoms:
					TypeDecider twTypeDecider = (TypeDecider)ObjectFactory.Get<Enterprise.Integration.Customs.TW.IEDIMessageTypeDecider>();
					return twTypeDecider.GetTypeForLoad(row, factory);
				case EDIMessage.ApplicationCodes.TRCustoms:
					return ((TypeDecider)ObjectFactory.Get<Enterprise.Integration.Customs.TR.IEDIMessageTypeDecider>()).GetTypeForLoad(row, factory);
				case EDIMessage.ApplicationCodes.UYCustoms:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.UY.IUYMessage>();
				case EDIMessage.ApplicationCodes.JPCustoms:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.JP.IEDIMessage>();
				case EDIMessage.ApplicationCodes.KRCustoms:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.KR.IEDIMessage>();
				case EDIMessage.ApplicationCodes.ESCustomsMessage:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.ES.IEDIMessage>();
				case EDIMessage.ApplicationCodes.ITCustoms:
				case EDIMessage.ApplicationCodes.ITCustomsXTrade:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.IT.IEDIMessage>();
				case EDIMessage.ApplicationCodes.MXCustoms:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.MX.IMXMessage>();
				case EDIMessage.ApplicationCodes.CLCustoms:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.CL.ICLMessage>();
				case EDIMessage.ApplicationCodes.BRCustoms:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.BR.IEDIMessage>();
				case EDIMessage.ApplicationCodes.NLCustoms:
					TypeDecider nlTypeDecider = (TypeDecider)ObjectFactory.Get<Enterprise.Integration.Customs.NL.IEDIMessageTypeDecider>();
					return nlTypeDecider.GetTypeForLoad(row, factory);
				case EDIMessage.ApplicationCodes.PLCustoms:
				case EDIMessage.ApplicationCodes.PLCustomsPUESCEmailSystem:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.PL.IEDIMessage>();
				case EDIMessage.ApplicationCodes.PLCustomsNCTS:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.PLNCTS.IEDIMessage>();
				case EDIMessage.ApplicationCodes.PLCustomsExitControl:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.PLExitControl.IEDIMessage>();
				case EDIMessage.ApplicationCodes.UsageData:
					return typeof(UsageEDIMessage);
				case EDIMessage.ApplicationCodes.CNCustomsSingleWindow:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.CN.IEDIMessage>();
				case EDIMessage.ApplicationCodes.ARCustoms:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.AR.IARMessage>();
				case EDIMessage.ApplicationCodes.CHCustomsEdec:
				case EDIMessage.ApplicationCodes.CHCustomsPassar:
				case EDIMessage.ApplicationCodes.CHCustomsCharteraOutput:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.CH.IEDIMessage>();
				case EDIMessage.ApplicationCodes.BECustoms:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.BE.IEDIMessage>();
				case EDIMessage.ApplicationCodes.AirCargoAdvanceScreening:
					return ((TypeDecider)ObjectFactory.Get<eTail.Integration.IHVLVEDIMessageTypeDecider>()).GetTypeForLoad(row, factory);
				case EDIMessage.ApplicationCodes.IC2:
					return ((TypeDecider)ObjectFactory.Get<Enterprise.Integration.Customs.EU.IEDIMessageTypeDecider>()).GetTypeForLoad(row, factory);
				case EDIMessage.ApplicationCodes.ShipamaxIntegration:
					return ((TypeDecider)ObjectFactory.Get<Enterprise.DocumentScanning.Integration.IEDocsShipamaxMessageTypeDecider>()).GetTypeForLoad(row, factory);
				case EDIMessage.ApplicationCodes.DashDocumentDataProcessing:
					return ((TypeDecider)ObjectFactory.Get<Enterprise.Dash.Integration.IDashDocumentDataMessageTypeDecider>()).GetTypeForLoad(row, factory);
				case ApplicationCodeList.Codes.INCustoms:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.IN.IINMessage>();
				case EDIMessage.ApplicationCodes.ILCustoms:
					return ObjectFactory.Get<Enterprise.Integration.Customs.IL.IEDIMessageTypeDecider>().GetTypeForLoad(row, factory);
				case ApplicationCodeList.Codes.UAECustoms:
					return ObjectFactory.Get<Enterprise.Integration.Customs.AE.IEDIMessageTypeDecider>().GetTypeForLoad(row, factory);

				default:
					return typeof(EDIMessage);
			}
		}

		#endregion
	}
}
