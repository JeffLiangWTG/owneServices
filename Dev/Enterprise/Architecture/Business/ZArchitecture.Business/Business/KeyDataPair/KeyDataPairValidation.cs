namespace Enterprise.ZArchitecture.Business
{
	public class KeyDataPairValidation : AutoKeyDataPairValidation
	{
		public KeyDataPairValidation(AutoKeyDataPair parent)
			: base(parent) { }

		public new KeyDataPair Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (KeyDataPair)base.Parent; }
		}
	}
}
