namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	abstract class TokenJobNetworkActionTestCaseForChildActionsTestedWithParent<T> : JobNetworkActionTestCase<T>
		where T : JobNetworkAction
	{
		public void TestIsNestedTypeWithinJobNetworkActionDescendent()
		{
			var declaredType = typeof(T).DeclaringType;

			AssertNotNull(string.Format("[{0}] should be declared within another type", typeof(T).Name), declaredType);
			Assert(string.Format("The outer class of [{0}] should be declared a descendent of [{1}]", typeof(T).Name, nameof(JobNetworkAction)), typeof(JobNetworkAction).IsAssignableFrom(typeof(T)));
		}

		#region JobNetworkActionTestCase Overrides

		protected override void TestExecuteCore()
		{
			Assert(true);
		}

		protected override void TestIsApplicableCore()
		{
			Assert(true);
		}

		protected override void TestIsEnabledCore()
		{
			Assert(true);
		}

		protected override void TestGetNameCore()
		{
			Assert(true);
		}

		protected override void TestGetIconCore()
		{
			Assert(true);
		}

		protected override void TestGetDescriptionCore()
		{
			Assert(true);
		}

		public override void TestCanPerformOnApprovedDiagram()
		{
			Assert(true);
		}

		public override void TestCanPerformOnApprovedShape()
		{
			Assert(true);
		}

		public override void TestChildActions()
		{
			Assert(true);
		}

		#endregion
	}
}
