using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitReportItemTypeDecider : CountrySpecificTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => Array.Empty<CountrySpecificType>();

		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Integration.Customs.EUExitControl.ICusExitReportItem>();

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusExitReportItem);

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory) => GetTypeForCountryCode(GetCusExitHeaderCountryCode(row, factory));

		ZString GetCusExitHeaderCountryCode(DataRow row, BusinessObjectFactory factory)
		{
			CusExitHeader header = null;
			if (row != null)
			{
				var reportPK = new ZGuid(row[CusExitReportItem.Schema.ERI_CER_Report]);
				if (reportPK.IsValid)
				{
					header = factory.Load<CusExitReport>(reportPK)?.Header;
				}
			}
			return header?.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}
	}
}
