using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class ECCNCodeCollection : CusCodeDataCollection<ECCNCode>
{
	public ECCNCodeCollection(BusinessObject master) : base(master, CusCodeDataTypeList.Codes.ExportControlClassificationNumber)
	{
		var maxCount = 9;
		MaxCountValidationEnable(maxCount);
	}

	public new ZString AsString
	{
		get
		{
			return Factory.GetValue(ref asStringCached, () => ZString.Join(",", this.Cast<ECCNCode>().OrderBy(x => x.CY_Code).Select(x => x.CY_Code).ToArray()));
		}
		set
		{
			RemoveAndDeleteAll();
			foreach (var code in value.Trim().Split(','))
			{
				if (!code.IsEmpty)
				{
					var item = AddNew();
					item.CY_Code = code.Left(ECCNCode.Schema.CY_CodeMaxLength).Trim();
				}
			}
		}
	}
	CachedProperty<ZString> asStringCached;

	protected new JobComInvoiceLine Master => (JobComInvoiceLine)base.Master;
}
