using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class CusTempStorageJobHeaderFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public CusTempStorageJobHeaderFetchStrategy(EnterpriseBusinessObject businessObject) : base(businessObject)
		{
		}

		protected new CusTempStorageJobHeader BusinessObject
		{
			get { return (CusTempStorageJobHeader)base.BusinessObject; }
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			if (columns.Any(x => x.ColumnName == CusTempStorageJobHeader.Schema.DDTNumber))
			{
				Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, BusinessObject.PK);
			}
			if (columns.Any(x => x.ColumnName.StartsWith(PropertyNames.Presenter, StringComparison.OrdinalIgnoreCase)))
			{
				Factory.AddFetchHint(OrgAddressSchema.PK, BusinessObject.SJH_OA_Presenter);
			}
			if (columns.Any(x => x.ColumnName.StartsWith(PropertyNames.Representative, StringComparison.OrdinalIgnoreCase)))
			{
				Factory.AddFetchHint(OrgAddressSchema.PK, BusinessObject.SJH_OA_Representative);
			}
			if (columns.Any(x => x.ColumnName.StartsWith(PropertyNames.Customer, StringComparison.OrdinalIgnoreCase)))
			{
				Factory.AddFetchHint(OrgHeaderSchema.PK, BusinessObject.SJH_OH_Customer);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		static class PropertyNames
		{
			public const string Presenter = "Presenter";
			public const string Representative = "Representative";
			public const string Customer = "Customer";
		}
	}
}
