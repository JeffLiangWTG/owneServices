using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business
{
	[CodeProperty("Code"), DescriptionProperty("Name")]
	public class Shed : NonPersistentBusinessObject
	{
		public Shed(ZZRefCusCodeListCombined cusCodeList)
			: base(cusCodeList.Factory)
		{
			this.cusCodeList = cusCodeList;
		}
		readonly ZZRefCusCodeListCombined cusCodeList;

		public ZString Code => cusCodeList.ZZD_Code;

		public ZString PortCode => Code.Left(3);

		public ZString ShedCode => Code.Right(3);

		public ZString Name => cusCodeList.ZZD_Description;

		public ZBool IsAtLondonHeathrow => Code.StartsWith(LondonHeathrow, System.StringComparison.OrdinalIgnoreCase);

		public ZBool IsEtsfAtLondonHeathrow => IsETSF && IsAtLondonHeathrow;

		public bool IsETSF => cusCodeList.HasAttribute(UniversalReferenceConstants.ShedAttributes.ETSF);

		public bool IsDEP => cusCodeList.HasAttribute(UniversalReferenceConstants.ShedAttributes.DEP);

		public ZString ChiefShed => cusCodeList.GetAttribute(UniversalReferenceConstants.ShedAttributes.ChiefShed);

		public ZString ACPCode => cusCodeList.GetAttribute(UniversalReferenceConstants.ShedAttributes.ACPCode);

		public ZString ChiefPort => cusCodeList.GetAttribute(UniversalReferenceConstants.ShedAttributes.Chief);
		public ZString SiteCode => cusCodeList.GetAttribute(UniversalReferenceConstants.ShedAttributes.SITECODE);

		public ZString AirportName => cusCodeList.GetAttribute(UniversalReferenceConstants.ShedAttributes.AirportName);

		public static ZQuery GetFilter(BusinessObjectFactory factory, ZString dataGroupingCode) => ZZRefCusCodeListCombined.Loader.GetFilter(factory, dataGroupingCode, UniversalReferenceConstants.RefCusCodeListType.Shed, ZDateTime.Now, (ZQuery)null, true);

		public static Shed LoadByCode(BusinessObjectFactory factory, ZString dataGroupingCode, ZString shedCode)
		{
			var query = GetFilter(factory, dataGroupingCode);
			query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_Code, shedCode);

			var cusCodeList = factory.LoadTop1<ZZRefCusCodeListCombined>(query);
			return cusCodeList != null ? new Shed(cusCodeList) : null;
		}

		const string LondonHeathrow = "LHR";
	}
}
