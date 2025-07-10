using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.AUREG)]
	public class CLREGInfoProviderAddInfo : AutoAUCLREGInfoProviderAddInfo
	{
		public CLREGInfoProviderAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public CLREGInfoProvider CLREGInfoProvider
		{
			get;
			set;
		}

		public override bool HasChanges
		{
			get { return base.HasChanges; }
			set
			{
				base.HasChanges = value;
				if (HasChanges && Parent != null && !Parent.IsMarkingAsNeedingValidationSuspended)
				{
					Parent.MarkAsNeedingValidation();
				}
			}
		}
	}
}
