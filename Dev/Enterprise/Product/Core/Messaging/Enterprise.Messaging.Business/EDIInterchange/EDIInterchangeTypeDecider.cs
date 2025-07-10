using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Messaging.Business
{
	[Immutable]
	public class EDIInterchangeTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var applicationCode = row[EDIInterchange.Schema.EI_ApplicationCode].ToString().Trim();
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

		public Type GetTypeForApplicationCode(string applicationCode, DataRow row, BusinessObjectFactory factory)
		{
			switch (applicationCode)
			{
				case EDIInterchange.ApplicationCodes.CAIMP:
				case EDIInterchange.ApplicationCodes.CAEXP:
				case EDIInterchange.ApplicationCodes.CAACI:
					{
						return ObjectFactory.GetType<Enterprise.Integration.Customs.CA.IEDIInterchange>();
					}

				case EDIInterchange.ApplicationCodes.UniversalDataMessaging:
					{
						var result = typeof(EDIInterchange);

						if (row[EDIInterchangeSchema.Constants.EI_ReceiveTransmit]?.ToString() == ReceiveTransmitList.Codes.Transmit)
						{
							Guid branchPk = Guid.TryParse(row[EDIInterchangeSchema.Constants.EI_GB]?.ToString() ?? string.Empty, out branchPk) ? branchPk : Guid.Empty;
							var countryCode = factory.Load<GlbBranch>(branchPk)?.Company?.GC_RN_NKCountryCode ?? string.Empty;

							if (countryCode == Core.Constants.CountryCodes.Canada)
							{
								var typeDecider = ObjectFactory.Get<Enterprise.Integration.Customs.CA.IUDMInterchangeTypeDecider>();
								result = typeDecider.GetTypeForLoad(row, factory);
							}
						}

						return result ?? typeof(EDIInterchange);
					}

				case EDIInterchange.ApplicationCodes.CMR:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICMRInterchange>();
				case EDIInterchange.ApplicationCodes.COLS:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICOLSInterchange>();
				case EDIInterchange.ApplicationCodes.NEXDOCS:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICMRInterchange>();
				case EDIInterchange.ApplicationCodes.EXDOC:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.AU.IEXDOCInterchange>();
				case EDIInterchange.ApplicationCodes.Traxon:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.HK.ITraxonInterchange>();
				case EDIInterchange.ApplicationCodes.NewZealandCustoms:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.NZ.INZCustomsInterchange>();
				case EDIInterchange.ApplicationCodes.NewZealandMAFeBACCa:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.NZ.INZEBACCAInterchange>();
				case EDIInterchange.ApplicationCodes.USAMA:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.US.IAIMEDIInterchange>();
				case EDIInterchange.ApplicationCodes.USExportManifest:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUEMEDIInterchange>();
				case EDIInterchange.ApplicationCodes.AMS:
				case EDIInterchange.ApplicationCodes.USCustomsExport:
				case EDIInterchange.ApplicationCodes.USCustomsImport:
					var usTypeDecider = (TypeDecider)ObjectFactory.Get<Enterprise.Integration.Customs.US.ICBPEDIInterchangeTypeDecider>();
					return usTypeDecider.GetTypeForLoad(row, factory);
				case EDIInterchange.ApplicationCodes.USCustomsDIS:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.US.DIS.IEDIInterchange>();
				case EDIInterchange.ApplicationCodes.CIM:
					return ObjectFactory.GetType<Enterprise.Freight.Integration.Forwarding.ICIMEDIInterchange>();
				case EDIInterchange.ApplicationCodes.SouthAfricanCustoms:
					return ObjectFactory.GetType("ZA.IZACustomsInterchange");
				case EDIInterchange.ApplicationCodes.TaiwanCustoms:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.TW.ITWInterchange>();
				case EDIInterchange.ApplicationCodes.UYCustoms:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.UY.IUYInterchange>();
				case EDIInterchange.ApplicationCodes.ITCustoms:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.IT.IEDIInterchange>();
				case EDIInterchange.ApplicationCodes.BRCustoms:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.BR.IEDIInterchange>();
				case EDIInterchange.ApplicationCodes.ESCustoms:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.ES.IEDIInterchange>();
				case EDIInterchange.ApplicationCodes.TRCustoms:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.TR.IEDIInterchange>();
				case EDIInterchange.ApplicationCodes.CNCustomsSingleWindow:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.CN.IEDIInterchange>();
				case EDIInterchange.ApplicationCodes.BECustoms:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.BE.IEDIInterchange>();
				case EDIInterchange.ApplicationCodes.IECustomsCommon:
				case EDIInterchange.ApplicationCodes.IECustomsEMCS:
				case EDIInterchange.ApplicationCodes.IECustomsExport:
				case EDIInterchange.ApplicationCodes.IECustomsImport:
				case EDIInterchange.ApplicationCodes.IECustomsUCC5Import:
				case EDIInterchange.ApplicationCodes.IECustomsNCTS:
					return ((TypeDecider)ObjectFactory.Get<Enterprise.Integration.Customs.IE.IEDIInterchangeTypeDecider>()).GetTypeForLoad(row, factory);
				case EDIInterchange.ApplicationCodes.CHCustomsEdec:
				case EDIInterchange.ApplicationCodes.CHCustomsPassar:
				case EDIInterchange.ApplicationCodes.CHCustomsCharteraOutput:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.CH.IEDIInterchange>();
				case EDIInterchange.ApplicationCodes.KRCustoms:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.KR.IEDIInterchange>();
				case EDIInterchange.ApplicationCodes.JPCustoms:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.JP.IEDIInterchange>();
				case EDIInterchange.ApplicationCodes.NOCustomsEmma:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.NO.IEmmaEDIInterchange>();
				case EDIInterchange.ApplicationCodes.ILCustoms:
					return ObjectFactory.GetType<Enterprise.Integration.Customs.IL.IEDIInterchange>();
				default:
					return typeof(EDIInterchange);
			}
		}
	}
}
