using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.CA.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.CANRCanPGAHeader)]
	public class NRCanPGAHeaderAddInfo : AutoNRCanPGAHeaderAddInfo
	{
		public NRCanPGAHeaderAddInfo(ZPropertyInfo addInfoProperty) : base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public override bool HasChanges
		{
			get => base.HasChanges;
			set
			{
				base.HasChanges = value;

				if (HasChanges && Parent != null && !Parent.IsMarkingAsNeedingValidationSuspended)
				{
					Parent.MarkAsNeedingValidation();
				}
			}
		}

		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo)
		{
			return base.IsValidationEnabledCore(propertyInfo) && (Parent as ICADeclarationProvider).IsValidationEnabled;
		}
	}
}
