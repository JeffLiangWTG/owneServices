using CargoWise.EntityFramework;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business
{
	[ModuleID(ModuleId.BRLPCO)]
	public class CusLPCOHeaderCollection : ActiveBusinessObjectCollection<CusLPCOHeader>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter constants")]
		public static class FilterConstants
		{
			public const string LPCOHolder = "LPCO Holder";
			public const string LPCONumber = "LPCO Number";
			public const string LPCOMessageStatus = "Message Status";
			public const string LPCOJobNumber = "Permit Reference Number";
			public const string LPCORetroactiveDate = "Reference Date";
			public const string LPCOCustomsStatus = "Permit Status";
			public const string StartDate = "Start Date";
			public const string EndDate = "End Date";
			public const string ReferenceDate = "Reference Date";
			public const string UnitOfMeasure = "Unit Of Measure";
		}

		public CusLPCOHeaderCollection(BusinessObjectFactory factory)
			: base(factory, new ZQuery())
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return base.CreateRelationshipFilter()
				.AddToFilter(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Operational)
				.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, Core.Constants.CountryCodes.Brazil)
				.AddToFilter(CusPermitHeaderSchema.CPH_GC_Company, GlbCompany.CurrentCompany.PK)
				.AddToFilter(CusPermitHeaderSchema.CPH_Type, PermitTypeList.Codes.LPC);
		}
	}
}
