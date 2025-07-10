using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataConverters
{
	public class OrgMatcher
	{
		public OrgMatcher(ZString accountCode, ZString deliveranceSystemIDCode, BusinessObjectFactory factory)
		{
			if (!accountCode.IsEmpty)
			{
				OriginalCode = accountCode;
				OrgHeader result = null;
				if (!deliveranceSystemIDCode.IsEmpty)
				{
					ZQuery filter = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.DeliveranceCode);
					filter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					filter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, deliveranceSystemIDCode + "_" + accountCode);
					OrgCusCode deliveranceCodeRecord = factory.LoadTop1<OrgCusCode>(filter);
					if (deliveranceCodeRecord != null)
					{
						result = deliveranceCodeRecord.Header;
					}
				}
				else
				{
					result = OrgHeader.FindByAccountID(factory, accountCode);
				}

				if (result == null)
				{
					result = OrgHeader.LoadFromCode(factory, accountCode);
				}
				if (result != null)
				{
					MatchedPK = result.PK;
					MatchedCode = result.OH_Code;
				}
			}
		}
		public readonly ZGuid MatchedPK;
		public readonly ZString MatchedCode;
		public readonly ZString OriginalCode;
	}
}
