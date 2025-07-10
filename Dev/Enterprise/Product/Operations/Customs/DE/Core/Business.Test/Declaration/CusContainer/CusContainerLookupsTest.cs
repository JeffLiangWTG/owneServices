namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class CusContainerLookupsTest : Customs.Business.Testing.CusContainerLookupsTest
	{
		public void TestCO_FCL_LCL_NCT_List_TransportModeAir()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			var list = lookups.CO_FCL_LCL_NCT_List;
			CombineAssertions(() =>
			{
				AssertEquals("List", "FCL, LCL, FCX, BBK, ULD", list.CodesAsString);
				AssertSame("Cached", list, lookups.CO_FCL_LCL_NCT_List);
			});
		}

		public void TestCO_FCL_LCL_NCT_List_TransportModeNotAir()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var list = lookups.CO_FCL_LCL_NCT_List;
			CombineAssertions(() =>
			{
				AssertEquals("List", "FCL, LCL, FCX, BBK", list.CodesAsString);
				AssertSame("Cached", list, lookups.CO_FCL_LCL_NCT_List);
			});
		}

		public void TestTestCO_FCL_LCL_NCT_List_DeclarationIsNull()
		{
			var cusContainer = Factory.GetNull<CusContainer>();
			AssertEquals("FCL, LCL, FCX, BBK, ULD", cusContainer.Lookups.CO_FCL_LCL_NCT_List.CodesAsString);
		}

		protected override Customs.Business.CusContainerLookups GetCusContainerLookups() => lookups;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			var cusContainer = declaration.CusContainers.AddNew();
			lookups = new CusContainerLookups(cusContainer);
		}
		JobDeclaration declaration;
		CusContainerLookups lookups;
	}
}
