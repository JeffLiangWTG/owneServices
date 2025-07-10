using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(NewConfiguration))]
	sealed class NewConfigurationNameTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new NewConfiguration(new ColumnConfigurationsManager(ZGuid.NewZGuid(), false));
		}

		public void TestNewManager()
		{
			LookupField lookupField = new LookupField(Factory);
			lookupField.SetCollectionProvider(new RefUNLOCOCollectionProvider(Factory));
			lookupField.DisplayName = "BOB";
			ColumnConfigurationsManager manager = new ColumnConfigurationsManager(ZGuid.NewZGuid(), false);
			manager.AddLinkedField(lookupField);
			manager.SaveToFilterField = "BOB";

			RefUNLOCO loco1 = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			RefUNLOCO loco2 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.PK, SQLComparisonOperator.NotEqual, loco1.PK));

			new CombinedConfigurationManager(manager, loco1.PK, loco1.RL_Code, loco1.RL_PortName, "SAVED").Save();
			new CombinedConfigurationManager(manager, "yeah baby").Save();

			NewConfiguration newConfig = new NewConfiguration(manager);
			newConfig.NewName = "SAVED";
			newConfig.LinkedField.Value = loco1.PK.ToGuid();
			newConfig.RunPreSaveValidation();
			AssertHasRowError("Should have saved exists for link1 message", newConfig, "A configuration exists for the chosen Description and or Link.  Please choose another link or description");

			newConfig.LinkedField.Value = loco2.PK.ToGuid();
			newConfig.RunPreSaveValidation();
			AssertNoRowError(newConfig, "config exists");

			newConfig.LinkedField.Value = Guid.Empty;
			newConfig.NewName = "yeah baby";
			newConfig.RunPreSaveValidation();
			AssertHasRowError("Config should exist", newConfig, "A configuration exists for the chosen Description and or Link.  Please choose another link or description");

			newConfig.LinkedField.Value = Guid.Empty;
			newConfig.NewName = "no baby";
			newConfig.RunPreSaveValidation();
			AssertNoRowError(newConfig, "config exists");
		}
	}
}
