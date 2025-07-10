using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.JP.Common.Testing;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(MoveInDestinationLookups))]
	sealed class MoveInDestinationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList()
		{
			JobDeclaration.JE_CustomsOffice = "1B";
			JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var collection = MoveInDestination.Lookups.CodeList as ZZRefCusCodeListCombinedCollection;

			CombineAssertions(() =>
			{
				collection.AssertFilterBusinessObjectDefault("Transport Mode:Property", ModuleTextFilter.ComparisonConstants.Exact, Core.Constants.TransportModes.Sea, true);

				collection.AssertFilterBusinessObjectDefault("Code:Property", ModuleTextFilter.ComparisonConstants.StartsWith, "1", true);

				collection.AssertFilterBusinessObjectDefault("List Type:Property", string.Empty, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanBondedAreaCode, false);

				collection.AssertFilterBusinessObjectDefault("Country/Region or Grouping:Property", string.Empty, Core.Constants.CountryCodes.Japan, false);
			});
		}

		public void TestViaList()
		{
			JobDeclaration.JE_CustomsOffice = "1B";
			JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var collection = MoveInDestination.Lookups.ViaList as ZZRefCusCodeListCombinedCollection;

			CombineAssertions(() =>
			{
				collection.AssertFilterBusinessObjectDefault("Transport Mode:Property", ModuleTextFilter.ComparisonConstants.Exact, Core.Constants.TransportModes.Sea, true);

				collection.AssertFilterBusinessObjectDefault("Code:Property", ModuleTextFilter.ComparisonConstants.StartsWith, "1", true);

				collection.AssertFilterBusinessObjectDefault("List Type:Property", string.Empty, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanBondedAreaCode, true);

				collection.AssertFilterBusinessObjectDefault("Country/Region or Grouping:Property", string.Empty, Core.Constants.CountryCodes.Japan, true);
			});
		}

		MoveInDestination MoveInDestination => moveInDestination ??= CusEntryInstruction.MoveInDestinationInfos.AddNew();
		MoveInDestination moveInDestination;

		CusEntryInstruction CusEntryInstruction => cusEntryInstruction ??= JobDeclaration.CustomsEntryInstructions.AddNew();
		CusEntryInstruction cusEntryInstruction;

		JobDeclaration JobDeclaration => jobDeclaration ??= Factory.New<JobDeclaration>();
		JobDeclaration jobDeclaration;
	}
}
