using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	[CodeProperty(ZZRefCusCodeListWrapper.Schema.Code, nameof(CACSubLocation.BuildCodeQuery)), DescriptionProperty(ZZRefCusCodeListWrapper.Schema.Description, nameof(CACSubLocation.BuildDescriptionQuery))]
	public class CACSubLocation : ZZRefCusCodeListWrapper
	{
		public CACSubLocation(ZZRefCusCodeListCombined cusCodeList)
			: base(cusCodeList)
		{
		}

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Schema : ZZRefCusCodeListWrapper.Schema
		{
			public const string SL_Address = "SL_Address";
			public const string SL_City = "SL_City";
			public const string SL_Division = "SL_Division";
			public const string SL_EffectiveDate = "SL_EffectiveDate";
			public const string SL_Port = "SL_Port";
			public const string SL_PostCode = "SL_PostCode";
			public const string SL_Province = "SL_Province";
			public const string SL_Type = "SL_Type";

			public const int CodeMaxLength = 4;
		}

		#endregion

		#region Properties

		public ZDateTime SL_EffectiveDate => CusCodeList.ZZD_StartDate;
		public ZPropertyInfo SL_EffectiveDateInfo => GetZPropertyInfo(CACSubLocation.Schema.SL_EffectiveDate);

		public ZString SL_Address => CusCodeList.GetAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Names.Street);
		public ZPropertyInfo SL_AddressInfo => GetZPropertyInfo(CACSubLocation.Schema.SL_Address);

		public ZString SL_City => CusCodeList.GetAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Names.City);
		public ZPropertyInfo SL_CityInfo => GetZPropertyInfo(CACSubLocation.Schema.SL_City);

		public ZString SL_Division => CusCodeList.GetAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Names.Division);
		public ZPropertyInfo SL_DivisionInfo => GetZPropertyInfo(CACSubLocation.Schema.SL_Division);

		public ZString SL_Port => CusCodeList.GetAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Names.Port);
		public ZPropertyInfo SL_PortInfo => GetZPropertyInfo(CACSubLocation.Schema.SL_Port);

		public ZString SL_PostCode => CusCodeList.GetAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Names.PostCode);
		public ZPropertyInfo SL_PostCodeInfo => GetZPropertyInfo(CACSubLocation.Schema.SL_PostCode);

		public ZString SL_Province => CusCodeList.GetAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Names.Province);
		public ZPropertyInfo SL_ProvenceInfo => GetZPropertyInfo(CACSubLocation.Schema.SL_Province);

		public ZString SL_Type => CusCodeList.GetAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Names.Type);
		public ZPropertyInfo SL_TypeInfo => GetZPropertyInfo(CACSubLocation.Schema.SL_Type);

		#endregion

		public static CACSubLocation Load(BusinessObjectFactory factory, ZString code)
		{
			var cusCodeList = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, code, Core.Constants.CountryCodes.Canada, UniversalReferenceConstants.RefCusCodeListType.Codes.SubLocation, ZDateTime.Now);
			return cusCodeList == null ? null : new CACSubLocation(cusCodeList);
		}

		public static ZQuery BuildCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, comparisonOperator, value);
		}

		public static ZQuery BuildDescriptionQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Description, comparisonOperator, value);
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("03146ede-387a-47fc-b356-40bb69f8047d", "Sub-Location Code: '{0}'", Code); }
		}
	}
}
