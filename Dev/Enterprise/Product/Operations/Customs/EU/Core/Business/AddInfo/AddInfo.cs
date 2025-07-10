using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business
{
	public abstract class AddInfo : AutoEUAddInfo
	{
		public new abstract class Schema : AutoEUAddInfo.Schema
		{
			public const string Prefix = "ZG";
		}

		protected AddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		[List(nameof(Lookups) + "." + nameof(Declaration.JobDeclarationLookups.ShipmentTypeList))]
		public override ZString ZG_ShipmentType { get => base.ZG_ShipmentType; set => base.ZG_ShipmentType = value; }

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
