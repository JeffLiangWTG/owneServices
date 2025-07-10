using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public abstract class AddInfo : AutoEUEMCSAddInfo
	{
		public new abstract class Schema : AutoEUEMCSAddInfo.Schema
		{
			public const string Prefix = "ZG";
		}

		protected AddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public override bool HasChanges
		{
			get { return base.HasChanges; }
			set
			{
				if (!Parent.IsSettingHasChangesSuspended && !IsSettingHasChangesSuspended)
				{
					base.HasChanges = value;
					if (HasChanges && !Parent.IsMarkingAsNeedingValidationSuspended)
					{
						Parent.MarkAsNeedingValidation();
					}
				}
			}
		}
	}
}
