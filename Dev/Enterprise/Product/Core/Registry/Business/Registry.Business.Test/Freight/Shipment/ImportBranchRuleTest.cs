using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ImportBranchRule))]
	sealed class ImportBranchRuleTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestDefaultValues()
		{
			AssertEquals("UseBranchFromXml", true, ImportBranchRule.UseBranchFromXml);
			AssertEquals("DefaultToBranchRelatedToOriginLoadPort", (short)1, ImportBranchRule.DefaultToBranchRelatedToOriginLoadPort);
			AssertEquals("DefaultToBranchRelatedToDestinationDischargePort", (short)2, ImportBranchRule.DefaultToBranchRelatedToDestinationDischargePort);
			AssertEquals("FallbackRule", ImportBranchRule.FallbackCodes.DefaultToAny, ImportBranchRule.FallbackRule);
		}

		public void TestRangeValidation()
		{
			AssertRangeValidation(ImportBranchRule.DefaultToBranchRelatedToOriginLoadPortInfo);
			AssertRangeValidation(ImportBranchRule.DefaultToBranchRelatedToDestinationDischargePortInfo);
		}

		void AssertRangeValidation(ZPropertyInfo propertyInfo)
		{
			string errorMessage = string.Format("Please enter a '{0}' within the range 0 to 2.", propertyInfo.HumanReadableName);

			propertyInfo.Value = new ZShort(-1);
			AssertHasError(propertyInfo, errorMessage);

			propertyInfo.Value = ZShort.Zero;
			AssertNoError(propertyInfo, errorMessage);

			propertyInfo.Value = new ZShort(1);
			AssertNoError(propertyInfo, errorMessage);

			propertyInfo.Value = new ZShort(2);
			AssertNoError(propertyInfo, errorMessage);

			propertyInfo.Value = new ZShort(3);
			AssertHasError(propertyInfo, errorMessage);
		}

		public void TestRunPreSaveValidation()
		{
			TestRowValidation(true, 1, 2, ImportBranchRule.FallbackCodes.DefaultToAny, null);
			TestRowValidation(true, 2, 1, ImportBranchRule.FallbackCodes.DefaultToAny, null);
			TestRowValidation(true, 1, 0, ImportBranchRule.FallbackCodes.DefaultToAny, null);
			TestRowValidation(true, 0, 1, ImportBranchRule.FallbackCodes.DefaultToAny, null);
			TestRowValidation(true, 0, 0, ImportBranchRule.FallbackCodes.DefaultToAny, null);

			TestRowValidation(false, 1, 2, ImportBranchRule.FallbackCodes.DefaultToAny, null);
			TestRowValidation(false, 2, 1, ImportBranchRule.FallbackCodes.DefaultToAny, null);
			TestRowValidation(false, 1, 0, ImportBranchRule.FallbackCodes.DefaultToAny, null);
			TestRowValidation(false, 0, 1, ImportBranchRule.FallbackCodes.DefaultToAny, null);
			TestRowValidation(false, 0, 0, ImportBranchRule.FallbackCodes.DefaultToAny, null);

			TestRowValidation(true, 1, 2, ImportBranchRule.FallbackCodes.DoNotCreate, null);
			TestRowValidation(true, 2, 1, ImportBranchRule.FallbackCodes.DoNotCreate, null);
			TestRowValidation(true, 1, 0, ImportBranchRule.FallbackCodes.DoNotCreate, null);
			TestRowValidation(true, 0, 1, ImportBranchRule.FallbackCodes.DoNotCreate, null);
			TestRowValidation(true, 0, 0, ImportBranchRule.FallbackCodes.DoNotCreate, null);

			TestRowValidation(false, 1, 2, ImportBranchRule.FallbackCodes.DoNotCreate, null);
			TestRowValidation(false, 2, 1, ImportBranchRule.FallbackCodes.DoNotCreate, null);
			TestRowValidation(false, 1, 0, ImportBranchRule.FallbackCodes.DoNotCreate, null);
			TestRowValidation(false, 0, 1, ImportBranchRule.FallbackCodes.DoNotCreate, null);
			TestRowValidation(false, 0, 0, ImportBranchRule.FallbackCodes.DoNotCreate, "According to current setting shipments will never be created.");

			TestRowValidation(true, 1, 1, ImportBranchRule.FallbackCodes.DefaultToAny, "Default to Branch Related to Destination/Discharge Port has an invalid value.");
			TestRowValidation(true, 2, 2, ImportBranchRule.FallbackCodes.DefaultToAny, "Default to Branch Related to Origin/Load Port has an invalid value.");
			TestRowValidation(true, 2, 0, ImportBranchRule.FallbackCodes.DefaultToAny, "Default to Branch Related to Origin/Load Port has an invalid value.");
			TestRowValidation(true, 0, 2, ImportBranchRule.FallbackCodes.DefaultToAny, "Default to Branch Related to Destination/Discharge Port has an invalid value.");
		}

		public void TestGetBranchSearchRules()
		{
			AssertArrayEqualsByElements(new[]
			{
				ImportBranchRule.BranchSearchRules.FromInterchange,
				ImportBranchRule.BranchSearchRules.FromOriginLoadPort,
				ImportBranchRule.BranchSearchRules.FromDestinationDischargePort,
				ImportBranchRule.BranchSearchRules.FromOriginLoadPortCountry,
				ImportBranchRule.BranchSearchRules.FromDestinationDischargePortCountry,
				ImportBranchRule.BranchSearchRules.Any
			},
			ImportBranchRule.GetBranchSearchRules().ToArray());

			ImportBranchRule.UseBranchFromXml = false;
			ImportBranchRule.DefaultToBranchRelatedToOriginLoadPort = 0;
			ImportBranchRule.DefaultToBranchRelatedToDestinationDischargePort = 1;
			ImportBranchRule.FallbackRule = ImportBranchRule.FallbackCodes.DoNotCreate;

			AssertArrayEqualsByElements(new[]
			{
				ImportBranchRule.BranchSearchRules.FromDestinationDischargePort,
				ImportBranchRule.BranchSearchRules.FromDestinationDischargePortCountry,
				ImportBranchRule.BranchSearchRules.Cancel
			},
			ImportBranchRule.GetBranchSearchRules().ToArray());

			ImportBranchRule.UseBranchFromXml = true;
			ImportBranchRule.DefaultToBranchRelatedToOriginLoadPort = 1;
			ImportBranchRule.DefaultToBranchRelatedToDestinationDischargePort = 0;
			ImportBranchRule.FallbackRule = ImportBranchRule.FallbackCodes.DefaultToAny;

			AssertArrayEqualsByElements(new[]
			{
				ImportBranchRule.BranchSearchRules.FromInterchange,
				ImportBranchRule.BranchSearchRules.FromOriginLoadPort,
				ImportBranchRule.BranchSearchRules.FromOriginLoadPortCountry,
				ImportBranchRule.BranchSearchRules.Any
			},
			ImportBranchRule.GetBranchSearchRules().ToArray());

			ImportBranchRule.UseBranchFromXml = true;
			ImportBranchRule.DefaultToBranchRelatedToOriginLoadPort = 0;
			ImportBranchRule.DefaultToBranchRelatedToDestinationDischargePort = 0;
			ImportBranchRule.FallbackRule = ImportBranchRule.FallbackCodes.DefaultToAny;

			AssertArrayEqualsByElements(new[]
			{
				ImportBranchRule.BranchSearchRules.FromInterchange,
				ImportBranchRule.BranchSearchRules.Any
			},
			ImportBranchRule.GetBranchSearchRules().ToArray());
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new ImportBranchRule();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return new ImportBranchRule();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		ImportBranchRule ImportBranchRule
		{
			get { return importBranchRule ?? (importBranchRule = new ImportBranchRule()); }
		}
		ImportBranchRule importBranchRule;

		void TestRowValidation(ZBool useBranchFromXml, ZShort defaultToBranchRelatedToOriginLoadPort, ZShort defaultToBranchRelatedToDestinationDischargePort, ZString fallbackRule, string expectedErrorMessage)
		{
			ImportBranchRule.UseBranchFromXml = useBranchFromXml;
			ImportBranchRule.DefaultToBranchRelatedToOriginLoadPort = defaultToBranchRelatedToOriginLoadPort;
			ImportBranchRule.DefaultToBranchRelatedToDestinationDischargePort = defaultToBranchRelatedToDestinationDischargePort;
			ImportBranchRule.FallbackRule = fallbackRule;

			ImportBranchRule.RunPreSaveValidation();

			if (expectedErrorMessage == null)
			{
				AssertNoErrors(ImportBranchRule);
			}
			else
			{
				IEnumerable<string> errorMessages = ImportBranchRule.RowErrors.Select(notification => notification.Message);
				AssertContains("RowErrors", expectedErrorMessage, string.Join(System.Environment.NewLine, errorMessages));
			}
		}

		#endregion
	}
}
