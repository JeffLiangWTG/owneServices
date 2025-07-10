using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.AUREGContact)]
	public class CLREGContactInfoProviderAddInfo : AutoAUCLREGContactInfoProviderAddInfo
	{
		public CLREGContactInfoProviderAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public CLREGContactInfoProvider CLREGContactInfoProvider
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
				if (HasChanges)
				{
					var contactInfoprovider = Parent as CLREGContactInfoProvider;
					if (contactInfoprovider != null && !contactInfoprovider.IsMarkingAsNeedingValidationSuspended)
					{
						contactInfoprovider.CLREGProvider?.MarkAsNeedingValidation();
					}
				}
			}
		}
	}
}
