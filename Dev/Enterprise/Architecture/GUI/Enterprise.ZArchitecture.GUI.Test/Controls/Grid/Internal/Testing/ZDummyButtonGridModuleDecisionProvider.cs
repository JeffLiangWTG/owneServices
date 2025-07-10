namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZDummyButtonGridModuleDecisionProvider : ButtonGridModuleDecisionProvider
	{
		public static int ConstructorCallCount;

		public ZDummyButtonGridModuleDecisionProvider(IFindBox findBox) : base(findBox)
		{
			ConstructorCallCount++;
		}
	}
}
