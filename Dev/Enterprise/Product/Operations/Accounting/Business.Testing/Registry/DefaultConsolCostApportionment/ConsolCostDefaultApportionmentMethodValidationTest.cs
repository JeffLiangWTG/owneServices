using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	public class ConsolCostDefaultApportionmentMethodValidationTest : TestCaseWithFactory
	{
		const string selectionErr = "Enter a valid selection.";
		const string duplicateString = "Duplicated apportionment methods";
		const string conflictString = "Other conflicting apportionment method rules exist";
		public void TestValidModule()
		{
			var method = CreateMethodAndCollection();
			AssertNoErrors(method.ModuleInfo);

			method.Module = "MTH";
			AssertHasError(method.ModuleInfo, selectionErr);

			method.Module = "";
			AssertHasError(method.ModuleInfo, "Please enter a Module.");

			method.Module = ApportionmentMethodModules.Forwarding;
			AssertNoErrors(method.ModuleInfo);

			AssertDuplicate(method.ContainerModeInfo.Name, "FOR", "DTB");
			AssertConflict(method.ContainerModeInfo.Name, "FOR");
		}

		public void TestValidTransportMode()
		{
			var method = CreateMethodAndCollection();
			method.TransportMode = "MTH";
			AssertHasError(method.TransportModeInfo, selectionErr);

			method.TransportMode = "";
			AssertHasError(method.TransportModeInfo, "Please enter a Transport Mode.");

			method.TransportMode = "AIR";
			AssertNoErrors(method.TransportModeInfo);

			AssertDuplicate(method.ContainerModeInfo.Name, "AIR", "ROA");
			AssertConflict(method.ContainerModeInfo.Name, "AIR");
		}

		public void TestValidContainerMode()
		{
			var method = CreateMethodAndCollection();
			method.ContainerMode = "MTH";
			AssertHasError(method.ContainerModeInfo, selectionErr);

			method.ContainerMode = "";
			AssertHasError(method.ContainerModeInfo, "Please enter a Container Mode.");

			method.ContainerMode = "ALL";
			AssertNoErrors(method.ContainerModeInfo);

			AssertDuplicate(method.ContainerModeInfo.Name, "OTH", "BBK");
			AssertConflict(method.ContainerModeInfo.Name, "OTH");
		}

		public void TestValidConsolType()
		{
			var method = CreateMethodAndCollection();
			method.ConsolType = "MTH";
			AssertHasError(method.ConsolTypeInfo, selectionErr);

			method.ConsolType = "";
			AssertHasError(method.ConsolTypeInfo, "Please enter a Consol Type.");

			method.ConsolType = "ALL";
			AssertNoErrors(method.ConsolTypeInfo);

			AssertDuplicate(method.ConsolTypeInfo.Name, "OTH", "CLD");
			AssertConflict(method.ConsolTypeInfo.Name, "OTH");
		}

		public void TestValidDirection()
		{
			var method = CreateMethodAndCollection();
			method.Direction = "AAA";
			AssertHasError(method.DirectionInfo, selectionErr);

			method.Direction = "";
			AssertHasError(method.DirectionInfo, "Please enter a Direction.");

			method.Direction = "ALL";
			AssertNoErrors(method.DirectionInfo);

			method.Direction = "IMP";
			AssertNoErrors(method.DirectionInfo);

			AssertDuplicate(method.DirectionInfo.Name, "IMP", "OTH");
			AssertConflict(method.DirectionInfo.Name, "IMP");
		}

		void AssertDuplicate(string assertProptyName, string validChangedValueNotAllForMethodA, string validChangedValueNotAllForMethodB)
		{
			var coll = new ConsolCostDefaultApportionmentMethodCollection();
			coll.RemoveAll();
			var method = coll.AddNew();
			var method2 = coll.AddNew();

			method.Apportionment = method2.Apportionment = "GWT";
			coll.RunPreSaveValidation();
			AssertHasRowError(method, duplicateString);
			AssertHasRowError(method2, duplicateString);

			method.FindPropertyInfo(assertProptyName).SetValueFromString(validChangedValueNotAllForMethodA);
			method2.FindPropertyInfo(assertProptyName).SetValueFromString(validChangedValueNotAllForMethodB);
			coll.RunPreSaveValidation();
			AssertNoRowError(method, duplicateString);
			AssertNoRowError(method2, duplicateString);
		}

		void AssertConflict(string assertProptyName, string validChangedValueNotAll)
		{
			var coll = new ConsolCostDefaultApportionmentMethodCollection();
			coll.RemoveAll();
			var method = coll.AddNew();
			var method2 = coll.AddNew();

			method.Apportionment = "MAN";
			method2.Apportionment = "GWT";
			coll.RunPreSaveValidation();
			AssertHasError(method.ApportionmentInfo, conflictString);
			AssertHasError(method2.ApportionmentInfo, conflictString);

			method.FindPropertyInfo(assertProptyName).SetValueFromString(validChangedValueNotAll);
			coll.RunPreSaveValidation();
			AssertNoErrors(method.ApportionmentInfo);
			AssertNoErrors(method2.ApportionmentInfo);
		}

		public void TestValidApportionment()
		{
			var method = CreateMethodAndCollection();
			method.Apportionment = "MTH";
			AssertHasError(method.ApportionmentInfo, selectionErr);

			method.Apportionment = "";
			AssertHasError(method.ApportionmentInfo, "Please enter an Apportionment.");

			method.Apportionment = "CHG";
			AssertNoErrors(method.ApportionmentInfo);
		}

		public void TestValidCollectionCombination()
		{
			var coll = new ConsolCostDefaultApportionmentMethodCollection();
			coll.RemoveAll();
			var methodAlpha = coll.AddNew();
			methodAlpha.Apportionment = "CHG";

			var methodBravo = coll.AddNew();
			methodBravo.Apportionment = "MAN";
			coll.RunPreSaveValidation();
			AssertHasError(methodAlpha.ApportionmentInfo, conflictString);
			AssertHasError(methodBravo.ApportionmentInfo, conflictString);

			var methodCharlie = coll.AddNew();
			methodCharlie.Apportionment = "SHP";
			coll.RunPreSaveValidation();
			AssertHasError(methodAlpha.ApportionmentInfo, conflictString);
			AssertHasError(methodBravo.ApportionmentInfo, conflictString);
			AssertHasError(methodCharlie.ApportionmentInfo, conflictString);

			methodCharlie.TransportMode = "AIR";
			coll.RunPreSaveValidation();
			AssertHasError(methodAlpha.ApportionmentInfo, conflictString);
			AssertHasError(methodBravo.ApportionmentInfo, conflictString);
			AssertNoErrors(methodCharlie.ApportionmentInfo);

			methodBravo.ConsolType = "DRT";
			coll.RunPreSaveValidation();
			AssertNoErrors(methodAlpha.ApportionmentInfo);
			AssertNoErrors(methodBravo.ApportionmentInfo);
			AssertNoErrors(methodCharlie.ApportionmentInfo);

			methodAlpha.Module = "FOR";
			coll.RunPreSaveValidation();
			AssertNoErrors(methodAlpha.ApportionmentInfo);
			AssertNoErrors(methodBravo.ApportionmentInfo);
			AssertNoErrors(methodCharlie.ApportionmentInfo);

			methodBravo.TransportMode = "AIR";
			methodCharlie.ConsolType = "DRT";
			coll.RunPreSaveValidation();
			AssertNoErrors(methodAlpha.ApportionmentInfo);
			AssertHasError(methodBravo.ApportionmentInfo, conflictString);
			AssertHasError(methodCharlie.ApportionmentInfo, conflictString);

			methodCharlie.Apportionment = "MAN";
			coll.RunPreSaveValidation();
			AssertNoErrors(methodAlpha.ApportionmentInfo);
			AssertHasRowError(methodBravo, duplicateString);
			AssertHasRowError(methodCharlie, duplicateString);

			methodCharlie.ConsolType = "COU";
			coll.RunPreSaveValidation();
			AssertNoErrors(methodAlpha.ApportionmentInfo);
			AssertNoErrors(methodBravo.ApportionmentInfo);
			AssertNoErrors(methodCharlie.ApportionmentInfo);
			AssertNoRowError(methodAlpha, duplicateString);
			AssertNoRowError(methodBravo, duplicateString);
			AssertNoRowError(methodCharlie, duplicateString);

			methodCharlie.ConsolType = "ALL";
			coll.RunPreSaveValidation();
			AssertNoRowError(methodAlpha, duplicateString);
			AssertHasRowError(methodBravo, duplicateString);
			AssertHasRowError(methodCharlie, duplicateString);
			AssertNoErrors(methodAlpha.ApportionmentInfo);
			AssertNoErrors(methodBravo.ApportionmentInfo);
			AssertNoErrors(methodCharlie.ApportionmentInfo);
		}

		public void TestDTBTransportValidation()
		{
			var coll = new ConsolCostDefaultApportionmentMethodCollection();
			coll.RemoveAll();
			var methodAlpha = coll.AddNew();
			methodAlpha.ConsolType = "OTH";
			methodAlpha.Module = "DTB";
			AssertNoError(methodAlpha.ConsolTypeInfo, selectionErr);

			methodAlpha.ConsolType = "OTH";
			AssertHasError(methodAlpha.ConsolTypeInfo, selectionErr);

			methodAlpha.Module = "FOR";
			coll.RunPreSaveValidation();
			AssertNoError(methodAlpha.ConsolTypeInfo, selectionErr);
		}

		public void TestDTBDirectionValidation()
		{
			var coll = new ConsolCostDefaultApportionmentMethodCollection();
			coll.RemoveAll();
			var methodAlpha = coll.AddNew();
			methodAlpha.Direction = "OTH";
			methodAlpha.Module = "DTB";
			AssertEquals(methodAlpha.Direction, "ALL");
			AssertNoError(methodAlpha.DirectionInfo, selectionErr);

			methodAlpha.Direction = "OTH";
			AssertHasError(methodAlpha.DirectionInfo, selectionErr);

			methodAlpha.Module = "FOR";
			coll.RunPreSaveValidation();
			AssertNoError(methodAlpha.DirectionInfo, selectionErr);
		}

		public void TestTRWDirectionValidation()
		{
			var coll = new ConsolCostDefaultApportionmentMethodCollection();
			coll.RemoveAll();
			var methodAlpha = coll.AddNew();
			methodAlpha.Direction = "OTH";
			methodAlpha.Module = "TRW";
			AssertEquals(methodAlpha.Direction, "ALL");
			AssertNoError(methodAlpha.DirectionInfo, selectionErr);

			methodAlpha.Direction = "OTH";
			AssertHasError(methodAlpha.DirectionInfo, selectionErr);

			methodAlpha.Module = "FOR";
			coll.RunPreSaveValidation();
			AssertNoError(methodAlpha.DirectionInfo, selectionErr);
		}

		ConsolCostDefaultApportionmentMethod CreateMethodAndCollection()
		{
			var coll = new ConsolCostDefaultApportionmentMethodCollection();
			return coll.AddNew();
		}
	}
}
