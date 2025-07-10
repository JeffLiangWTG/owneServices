using System;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public class DummyModuleFilterValidation : ModuleFilterValidation
	{
		public DummyModuleFilterValidation(ModuleFilter parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
		}

		public override Type AutoValidationType
		{
			get { return GetType(); }
		}
	}
}
