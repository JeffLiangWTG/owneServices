using System;

namespace Enterprise.ZArchitecture.Business
{
	public class ModuleCurrentDateOffsetFilterValidation : ModuleFilterValidation
	{
		public ModuleCurrentDateOffsetFilterValidation(ModuleCurrentDateOffsetFilter parent)
			: base(parent)
		{
			Parent = parent;
		}

		protected readonly ModuleCurrentDateOffsetFilter Parent;

		public override Type AutoValidationType
		{
			get { return typeof(ModuleCurrentDateOffsetFilterValidation); }
		}

		#region Validate Offset

		public void ValidateOffset()
		{
			ValidateCalculatedProperty(Parent.OffsetInfo);
		}

		protected void CheckOffset()
		{
			if (Parent.Offset < 0)
			{
				Parent.OffsetInfo.AddError(Res.GetString("7231a793-6d35-4679-af10-697b5c19db7e", "Must be non-negative"));
			}
		}

		#endregion
		public override void ValidateAll()
		{
			ValidateOffset();
		}
	}
}
