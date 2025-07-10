using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.ExitControl.Business;

public class CusGoodsLocationTypeDecider : CountrySpecificTypeDecider
{
	public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
	{
		CusExitReport cusExitReport = null;
		var parentTableCode = row[CusGoodsLocationSchema.Constants.CGL_ParentTableCode].ToString();
		var parentID = new ZGuid(row[CusGoodsLocationSchema.Constants.CGL_ParentID]);

		if (parentTableCode == CusExitReportSchema.Constants.Prefix)
		{
			cusExitReport = factory.Load<CusExitReport>(parentID);
		}

		return GetTypeForCountryCode(cusExitReport?.CountryCode ?? ZString.Empty);
	}

	protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => Array.Empty<CountrySpecificType>();

	protected override Type DefaultTypeForUnsupportedCountry => typeof(CusGoodsLocation);
}
