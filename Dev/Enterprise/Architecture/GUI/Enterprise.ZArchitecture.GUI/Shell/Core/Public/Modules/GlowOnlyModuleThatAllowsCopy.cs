namespace Enterprise.ZArchitecture.Modules
{
	public abstract class GlowOnlyModuleThatAllowsCopy : GlowOnlyModule
	{
		public override bool AllowNew => true;
	}
}
