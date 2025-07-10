using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business
{
	public class CusClassPartPivotValidation : BaseCusClassPartPivotValidation
	{
		public CusClassPartPivotValidation(CusClassPartPivot parent) : base(parent)
		{
		}

		public new CusClassPartPivot Parent
		{
			get { return (CusClassPartPivot)base.Parent; }
		}

		protected override void CheckCI_CGC_Catalog()
		{
			base.CheckCI_CGC_Catalog();
			ValidateClassTariff(Parent.CI_CGC_CatalogInfo);
			ValidateCI_TariffNum();

			if (Parent.GoodsCatalog != null)
			{
				if (Parent.GoodsCatalog.CGC_AuthorityIdentifier.IsEmpty)
				{
					Parent.CI_CGC_CatalogInfo.AddWarning(Res.GetString("FDE9D1F5-4456-4862-990E-DBF65F0FC11C", "Catalog has no Authority Code."));
				}

				switch (Parent.GoodsCatalog.CGC_MessageStatus)
				{
					case BRMessageStatusList.Codes.NotSent:
						Parent.CI_CGC_CatalogInfo.AddWarning(Res.GetString("6C6A0918-3E16-420F-9344-D5B42FB2396A", "This Goods Catalog should not be used because there might be messages that need to be sent (its Latest Message Status is currently NST - Not Sent)."));
						break;
					case BRMessageStatusList.Codes.AwaitingResponse:
						Parent.CI_CGC_CatalogInfo.AddError(Res.GetString("AAB37C2F-B70D-49F0-801F-84DC64B09542", "This Goods Catalog should not be used because there is a message waiting for response (its Latest Message Status is currently AWA - Awaiting Response)."));
						break;
					case BRMessageStatusList.Codes.Rejected:
						Parent.CI_CGC_CatalogInfo.AddWarning(Res.GetString("59C7245F-A498-4D45-B6CF-92DB03DC866C", "This Goods Catalog should not be used due to its last message being rejected (its Latest Message Status is currently REJ - Rejected)."));
						break;
				}
			}
		}

		protected override void CheckCI_CC()
		{
			base.CheckCI_CC();
			ValidateCI_CGC_Catalog();
		}

		protected override void CheckCI_TariffNum()
		{
			base.CheckCI_TariffNum();
			ValidateCI_CGC_Catalog();
		}

		protected override void ValidateClassTariff(ZPropertyInfo info)
		{
			if (Registry.BRCustomsDataRegistry.Instance.EnableCatalogModule.Value)
			{
				if (Parent.CI_CI_Parent.IsEmpty && Parent.CI_CC.IsEmpty && Parent.CI_TariffNum.IsEmpty && Parent.CI_CGC_Catalog.IsEmpty)
				{
					info.AddError(OneOfTariffOrClassificationOrCatalogIsMandatory);
				}
			}
			else if (info.Name != CusClassPartPivot.Schema.CI_CGC_Catalog)
			{
				base.ValidateClassTariff(info);
			}
		}

		static string OneOfTariffOrClassificationOrCatalogIsMandatory => Res.GetString("19FCD8C7-F525-46AF-B557-D8EE57A76017", "One of Tariff or Classification or Goods Catalog is Mandatory.");
	}
}
