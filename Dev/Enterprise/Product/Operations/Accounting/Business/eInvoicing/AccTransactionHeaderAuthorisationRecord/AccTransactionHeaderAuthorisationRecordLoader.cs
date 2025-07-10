using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing.Fiji;
using Enterprise.Accounting.Business.EInvoicing.India;
using Enterprise.Accounting.Business.EInvoicing.Samoa;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.EInvoicing
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Will be used from DocWrapper")]
	public static class AccTransactionHeaderAuthorisationRecordLoader
	{
		public static AccTransactionHeaderAuthorisationRecord LoadByParentID(BusinessObjectFactory factory, ZGuid parentID, string countryCode)
		{
			AccTransactionHeaderAuthorisationRecord result = null;

			switch (countryCode)
			{
				case Core.Constants.CountryCodes.Fiji:
					result = factory.LoadTop1<FijiAccTransactionHeaderAuthorisationRecord>(new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, parentID));
					break;
				case Core.Constants.CountryCodes.WesternSamoa:
					result = factory.LoadTop1<SamoaAccTransactionHeaderAuthorisationRecord>(new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, parentID));
					break;
				case Core.Constants.CountryCodes.India:
					result = factory.LoadTop1<IndiaAccTransactionHeaderAuthorisationRecord>(new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, parentID));
					break;
				default:
					result = factory.LoadTop1<AccTransactionHeaderAuthorisationRecord>(new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, parentID));
					break;
			}

			return result;
		}
	}
}
