using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public partial class CusLineTariffDetail : AutoBRCusLineTariffDetail
	{
		public CusLineTariffDetail(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[BusinessObjectTestExclude]
		public ZString ExNumber
		{
			get => BZ_Tariff.Split(ExTariffSeperator).ElementAtOrDefault(1);
			set
			{
				if (value.IsEmpty)
				{
					BZ_Tariff = ZString.Empty;
				}
				else if (!Parent.Tariff.IsEmpty)
				{
					BZ_Tariff = $"{Parent.Tariff}{ExTariffSeperator}{value}";
				}
			}
		}

		const char ExTariffSeperator = '_';

		public bool IsEmpty => BZ_Tariff.IsEmpty && BZ_Type.IsEmpty;

		public override bool IsSavedByFactory => IsInDatabase ? base.IsSavedByFactory : !(IsDeleted || IsEmpty);

		public override void OnSaving()
		{
			if (!IsDeleted && IsInDatabase && IsEmpty)
			{
				Delete();
			}
			base.OnSaving();
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			if (!IsInDatabase || HasChanges)
			{
				((IAddInfoManager)this).AddInfo.UpdateRelatedPropertyInfo();
			}
			return base.CloneInternal(args);
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			BZ_Tariff = "1";
			BZ_Type = "1";
		}
#endif
	}
}
