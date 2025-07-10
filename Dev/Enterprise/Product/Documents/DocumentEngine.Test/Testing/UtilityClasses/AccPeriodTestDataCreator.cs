using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	static class AccPeriodTestDataCreator
	{
		public static void Create(BusinessObjectFactory factory)
		{
			ZGuid companyPK = GlbCompany.CurrentCompany.PK;
			AccPeriodManagement accPeriod;
			AccPeriodManagementCollection collection = new AccPeriodManagementCollection(factory);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 200601;
			accPeriod.AM_Year = 2006;
			accPeriod.AM_StartDate = new ZDateTime(2006, 1, 1);
			accPeriod.AM_EndDate = new ZDateTime(2006, 3, 31, 23, 59, 00);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 200604;
			accPeriod.AM_Year = 2006;
			accPeriod.AM_StartDate = new ZDateTime(2006, 4, 1);
			accPeriod.AM_EndDate = new ZDateTime(2006, 6, 30, 23, 59, 00);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 200607;
			accPeriod.AM_Year = 2006;
			accPeriod.AM_StartDate = new ZDateTime(2006, 7, 1);
			accPeriod.AM_EndDate = new ZDateTime(2006, 9, 30, 23, 59, 00);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 200610;
			accPeriod.AM_Year = 2006;
			accPeriod.AM_StartDate = new ZDateTime(2006, 10, 1);
			accPeriod.AM_EndDate = new ZDateTime(2006, 12, 31, 23, 59, 00);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 200701;
			accPeriod.AM_Year = 2007;
			accPeriod.AM_StartDate = new ZDateTime(2007, 1, 1);
			accPeriod.AM_EndDate = new ZDateTime(2007, 3, 31, 23, 59, 00);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 200704;
			accPeriod.AM_Year = 2007;
			accPeriod.AM_StartDate = new ZDateTime(2007, 4, 1);
			accPeriod.AM_EndDate = new ZDateTime(2007, 6, 30, 23, 59, 00);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 200707;
			accPeriod.AM_Year = 2007;
			accPeriod.AM_StartDate = new ZDateTime(2007, 7, 1);
			accPeriod.AM_EndDate = new ZDateTime(2007, 9, 30, 23, 59, 00);

			accPeriod = collection.AddNew();
			accPeriod.AM_GC_Company = companyPK;
			accPeriod.AM_Period = 200710;
			accPeriod.AM_Year = 2007;
			accPeriod.AM_StartDate = new ZDateTime(2007, 10, 1);
			accPeriod.AM_EndDate = new ZDateTime(2007, 12, 31, 23, 59, 00);

			factory.Save();
		}
	}
}
