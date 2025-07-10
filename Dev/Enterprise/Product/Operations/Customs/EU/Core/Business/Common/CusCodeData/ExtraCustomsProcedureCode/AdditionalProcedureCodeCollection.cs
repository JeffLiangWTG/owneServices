using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business
{
	public class AdditionalProcedureCodeCollection : CusCodeDataCollection<AdditionalProcedureCode>
	{
		public AdditionalProcedureCodeCollection(IAdditionalProcedureParent master) : base(master.BusinessObject, CusCodeDataTypeList.Codes.AdditionalProcedureCode)
		{
			MaxCountValidationEnable(Master.MaxNumberOfAdditionalProcedureCode);
		}

		public new ZString AsString
		{
			get
			{
				return Factory.GetValue(ref asStringCached, () => ZString.Join(",", this.Cast<AdditionalProcedureCode>().OrderBy(x => x.CY_Code).Select(x => x.CY_Code).ToArray()));
			}
			set
			{
				RemoveAndDeleteAll();
				foreach (var code in value.Trim().Split(','))
				{
					if (!code.IsEmpty)
					{
						var item = AddNew();
						item.CY_Code = code.Left(AdditionalProcedureCode.Schema.CY_CodeMaxLength).Trim();
					}
				}
			}
		}
		CachedProperty<ZString> asStringCached;

		protected new IAdditionalProcedureParent Master => (IAdditionalProcedureParent)base.Master;

		protected override bool AllowNewCore => Count < MaxCount;
	}
}
