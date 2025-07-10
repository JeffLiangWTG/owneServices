using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	public class IncotermsDropEditBusinessObject : DropEditBusinessObject
	{
		public IncotermsDropEditBusinessObject(CodeDescriptionPairList list) : base(list)
		{
		}

		[MaxLength("List.MaxCodeLength")]
		public override ZString Value
		{
			get { return base.Value; }
			set
			{
				var oldValue = base.Value;
				base.Value = value;
				if (value != oldValue)
				{
					ValueInfo.ClearAllNotifications();
					IncotermValidation.Instance.WarningIfExpired(ValueInfo);
				}
			}
		}
	}
}
