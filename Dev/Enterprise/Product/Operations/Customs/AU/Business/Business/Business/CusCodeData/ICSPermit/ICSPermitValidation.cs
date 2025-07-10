using CargoWise.EntityFramework;
using Enterprise.Customs.Common.AU;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ICSPermitValidation : Customs.Business.CusCodeDataValidation
	{
		public ICSPermitValidation(ICSPermit parent)
			: base(parent)
		{ }

		protected new ICSPermit Parent => (ICSPermit)base.Parent;
		protected override void CheckCY_Code()
		{
		}

		protected override void CheckCY_Type()
		{
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			var bizObj = Parent?.Parent;
			if (bizObj != null)
			{
				if (Parent.CY_Data.IsEmpty)
				{
					Parent.CY_DataInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(Parent.CY_DataInfo.HumanReadableName));
				}
				else
				{
					var query = new ZQuery(CusCodeDataSchema.CY_ParentID, bizObj.PK);
					query.AddToFilter(CusCodeDataSchema.CY_Type, CusCodeDataTypeList.Codes.ICSPermit);
					query.AddToFilter(CusCodeDataSchema.CY_Code, CusCodeDataTypeList.Codes.ICSPermit);
					query.AddToFilter(CusCodeDataSchema.CY_Data, Parent.CY_Data);
					query.AddToFilter(CusCodeDataSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
					query.FetchOnlyFromLocalCache = !bizObj.IsInDatabase;
					if (Parent.Factory.Load<ICSPermit>(query).Length > 0)
					{
						Parent.CY_DataInfo.AddMessageError(System.FormattableString.Invariant($"'{Parent.CY_Data}' has been entered multiple times."));
					}
				}
			}
		}
	}
}
