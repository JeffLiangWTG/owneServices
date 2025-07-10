using System;
using System.IO;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Organizations.CodeGeneration;
using Enterprise.DataTransfer.Native.Business.Update.CodeMappings;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.DataTransfer.Native.Business.Testing
{
	class OrgCodeGenerationWithCodeMappingTest : TestCaseWithFactory
	{
		#region Default Tests

		public void TestInsertOrganisation()
		{
			// Arrange.
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues();

			// Act.
			var processingLogText = ImportData(testValues);

			// Assert.
			AssertContains("OrgHeader - 1 inserts, 0 updates, 0 deletes", processingLogText);

			AssertCodeMapping("FREFUDTAL");
		}

		public void TestInsertOrganisation_CreatePreExistingOrg()
		{
			// Arrange.
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues();
			CreatePreExistingOrganisation();

			// Act.
			var processingLogText = ImportData(testValues);

			// Assert.
			AssertContains("Error - The value of Organization Code must be unique on Organization. The duplicate value(s) are: (FFINC).", processingLogText);
		}

		public void TestInsertOrganisationWithCodeMappingOff_CreatePreExistingOrg()
		{
			// Arrange.
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues();
			CreatePreExistingOrganisation();

			// Act.
			var processingLogText = ImportData(testValues);

			// Assert.
			AssertContains("Error - The value of Organization Code must be unique on Organization. The duplicate value(s) are: (FFINC).", processingLogText);

			AssertCodeMapping("FREFUDTAL");
		}

		public void TestMergeOrganisation()
		{
			// Arrange.
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				Action = "MERGE"
			};

			// Act.
			var processingLogText = ImportData(testValues);

			// Assert.
			AssertContains("OrgHeader - 1 inserts, 0 updates, 0 deletes", processingLogText);
			AssertCodeMapping("FREFUDTAL");
		}

		public void TestUpdateOrganisation()
		{
			// Arrange.
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				Action = "UPDATE"
			};

			// Act.
			var processingLogText = ImportData(testValues);

			// Assert.
			AssertContains("Error - There is no Organization with the following values:", processingLogText);
		}

		public void TestDeleteOrganisation()
		{
			// Arrange.
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				Action = "DELETE"
			};

			// Act.
			var processingLogText = ImportData(testValues);

			// Assert.
			AssertContains("Error - There is no Organization with the following values:", processingLogText);
		}

		public void TestMergeOrganisation_CreatePreExistingOrg()
		{
			// Arrange.
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				Action = "MERGE"
			};
			CreatePreExistingOrganisation();

			// Act.
			var processingLogText = ImportData(testValues);

			// Assert.
			AssertContains("OrgHeader - 0 inserts, 1 updates, 0 deletes", processingLogText);
			AssertCodeMapping("FREFUDTAL");
		}

		public void TestUpdateOrganisation_CreatePreExistingOrg()
		{
			// Arrange.
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				Action = "UPDATE"
			};
			CreatePreExistingOrganisation();

			// Act.
			var processingLogText = ImportData(testValues);

			// Assert.
			AssertContains("OrgHeader - 0 inserts, 1 updates, 0 deletes", processingLogText);
		}

		public void TestDeleteOrganisation_CreatePreExistingOrg()
		{
			// Arrange.
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				Action = "DELETE"
			};
			CreatePreExistingOrganisation();

			// Act.
			var processingLogText = ImportData(testValues);

			// Assert.
			AssertContains("OrgHeader - 0 inserts, 0 updates, 1 deletes", processingLogText);
		}

		#endregion

		#region Without UNLOCO

		#region Default Registry Settings

		public void TestInsertOrganisationWithoutUNLOCOWithCodeMappingOn()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				RefUNLOCO = null
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("Error - The UNLOCO provided in the [ClosestPort.Code] element must not be empty when Organization Code Generation is enabled", processingLogText);
		}

		public void TestInsertOrganisationWithoutUNLOCOWithCodeMappingOff()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				EnableCodeMapping = false,
				RefUNLOCO = null
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("Error - The UNLOCO provided in the [ClosestPort.Code] element must not be empty when Organization Code Generation is enabled", processingLogText);
		}

		public void TestMergeOrganisationWithoutUNLOCOWithCodeMappingOn()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				Action = "MERGE",
				RefUNLOCO = null
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("Error - The UNLOCO provided in the [ClosestPort.Code] element must not be empty when Organization Code Generation is enabled", processingLogText);
		}

		public void TestMergeOrganisationWithoutUNLOCOWithCodeMappingOff()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				Action = "MERGE",
				RefUNLOCO = null,
				EnableCodeMapping = false
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("Error - The UNLOCO provided in the [ClosestPort.Code] element must not be empty when Organization Code Generation is enabled", processingLogText);
		}

		public void TestMergeOrganisationWithoutUNLOCOWithCodeMappingOn_CreatePreExistingOrg()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				Action = "MERGE",
				RefUNLOCO = null
			};
			CreatePreExistingOrganisation();

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("OrgHeader - 0 inserts, 1 updates, 0 deletes", processingLogText);
			AssertCodeMapping("FREFUDTAL");
		}

		public void TestMergeOrganisationWithoutUNLOCOWithCodeMappingOff_CreatePreExistingOrg()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				Action = "MERGE",
				RefUNLOCO = null,
				EnableCodeMapping = false
			};
			CreatePreExistingOrganisation();

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("OrgHeader - 0 inserts, 1 updates, 0 deletes", processingLogText);
			AssertCodeMapping("FREFUDTAL");
		}

		public void TestUpdateOrganisationWithoutUNLOCOWithCodeMappingOn()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				Action = "UPDATE",
				RefUNLOCO = null
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("Error - There is no Organization with the following values:", processingLogText);
		}

		public void TestUpdateOrganisationWithoutUNLOCOWithCodeMappingOff()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				Action = "UPDATE",
				RefUNLOCO = null,
				EnableCodeMapping = false
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("Error - There is no Organization with the following values:", processingLogText);
		}

		public void TestUpdateOrganisationWithoutUNLOCOWithCodeMappingOn_CreatePreExistingOrg()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				Action = "UPDATE",
				RefUNLOCO = null
			};
			CreatePreExistingOrganisation();

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("OrgHeader - 0 inserts, 1 updates, 0 deletes", processingLogText);
			AssertCodeMapping("FREFUDTAL");
		}

		public void TestUpdateOrganisationWithoutUNLOCOWithCodeMappingOff_CreatePreExistingOrg()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				Action = "UPDATE",
				RefUNLOCO = null,
				EnableCodeMapping = false
			};
			CreatePreExistingOrganisation();

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("OrgHeader - 0 inserts, 1 updates, 0 deletes", processingLogText);
			AssertCodeMapping("FREFUDTAL");
		}

		public void TestDeleteOrganisationWithoutUNLOCOWithCodeMappingOn()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				Action = "DELETE",
				RefUNLOCO = null
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("Error - There is no Organization with the following values:", processingLogText);
		}

		public void TestDeleteOrganisationWithoutUNLOCOWithCodeMappingOff()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				Action = "DELETE",
				RefUNLOCO = null,
				EnableCodeMapping = false
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("Error - There is no Organization with the following values:", processingLogText);
		}

		public void TestDeleteOrganisationWithoutUNLOCOWithCodeMappingOn_CreatePreExistingOrg()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				Action = "DELETE",
				RefUNLOCO = null
			};
			CreatePreExistingOrganisation();

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("OrgHeader - 0 inserts, 0 updates, 1 deletes", processingLogText);
		}

		public void TestDeleteOrganisationWithoutUNLOCOWithCodeMappingOff_CreatePreExistingOrg()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				Action = "DELETE",
				RefUNLOCO = null,
				EnableCodeMapping = false
			};
			CreatePreExistingOrganisation();

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("OrgHeader - 0 inserts, 0 updates, 1 deletes", processingLogText);
		}

		#endregion

		#region Allow Code Changes Registry Setting

		public void TestInsertOrganisationWithoutUNLOCOWithCodeMappingOn_AllowCodeChanges()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(enableCodeChanges: true);
			var testValues = new TestValues
			{
				RefUNLOCO = null
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("Error - The UNLOCO provided in the [ClosestPort.Code] element must not be empty when Organization Code Generation is enabled", processingLogText);
		}

		public void TestMergeOrganisationWithoutUNLOCOWithCodeMappingOn_AllowCodeChanges()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(enableCodeChanges: true);
			var testValues = new TestValues
			{
				Action = "MERGE",
				RefUNLOCO = null
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("Error - The UNLOCO provided in the [ClosestPort.Code] element must not be empty when Organization Code Generation is enabled", processingLogText);
		}

		public void TestMergeOrganisationWithoutUNLOCOWithCodeMappingOn_CreatePreExistingOrg_AllowCodeChanges()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(enableCodeChanges: true);
			var testValues = new TestValues
			{
				Action = "MERGE",
				RefUNLOCO = null
			};
			CreatePreExistingOrganisation();

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("OrgHeader - 0 inserts, 1 updates, 0 deletes", processingLogText);
			AssertCodeMapping("FREFUDTAL");
		}

		#endregion

		#region No UNLOCO in code generation

		public void TestInsertOrganisationWithoutUNLOCOWithCodeMappingOn_NoUNLOCOUsedForCodeGeneration()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(shouldUseIata: false);
			var testValues = new TestValues
			{
				RefUNLOCO = null
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("OrgHeader - 1 inserts, 0 updates, 0 deletes", processingLogText);
			AssertCodeMapping("FREFUD");
		}

		public void TestMergeOrganisationWithoutUNLOCOWithCodeMappingOn_NoUNLOCOUsedForCodeGeneration()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(shouldUseIata: false);
			var testValues = new TestValues
			{
				Action = "MERGE",
				RefUNLOCO = null
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("OrgHeader - 1 inserts, 0 updates, 0 deletes", processingLogText);
			AssertCodeMapping("FREFUD");
		}

		public void TestMergeOrganisationWithoutUNLOCOWithCodeMappingOn_CreatePreExistingOrg_NoUNLOCOUsedForCodeGeneration()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(shouldUseIata: false);
			var testValues = new TestValues
			{
				Action = "MERGE",
				RefUNLOCO = null
			};
			CreatePreExistingOrganisation();

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("OrgHeader - 0 inserts, 1 updates, 0 deletes", processingLogText);
			AssertCodeMapping("FREFUDTAL");
		}

		#endregion

		#endregion

		#region Without Full Name

		#region Default Registry Settings

		public void TestInsertOrganisationWithoutFullNameWithCodeMappingOn()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				FullName = null
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("Error - Full Name could not be empty when Organization Code Generation is enabled", processingLogText);
		}

		public void TestInsertOrganisationWithoutFullNameWithCodeMappingOff()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				EnableCodeMapping = false,
				FullName = null
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("Error - Full Name could not be empty when Organization Code Generation is enabled", processingLogText);
		}

		public void TestMergeOrganisationWithoutFullNameWithCodeMappingOn()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				Action = "MERGE",
				FullName = null
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("Error - Full Name could not be empty when Organization Code Generation is enabled", processingLogText);
		}

		public void TestMergeOrganisationWithoutFullNameWithCodeMappingOff()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				Action = "MERGE",
				FullName = null,
				EnableCodeMapping = false
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("Error - Full Name could not be empty when Organization Code Generation is enabled", processingLogText);
		}

		public void TestMergeOrganisationWithoutFullNameWithCodeMappingOn_CreatePreExistingOrg()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				Action = "MERGE",
				FullName = null
			};
			CreatePreExistingOrganisation();

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("OrgHeader - 0 inserts, 0 updates, 0 deletes", processingLogText);
		}

		public void TestMergeOrganisationWithoutFullNameWithCodeMappingOff_CreatePreExistingOrg()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				Action = "MERGE",
				FullName = null,
				EnableCodeMapping = false
			};
			CreatePreExistingOrganisation();

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("OrgHeader - 0 inserts, 0 updates, 0 deletes", processingLogText);
		}

		public void TestUpdateOrganisationWithoutFullNameWithCodeMappingOn()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				Action = "UPDATE",
				FullName = null
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("Error - There is no Organization with the following values:", processingLogText);
		}

		public void TestUpdateOrganisationWithoutFullNameWithCodeMappingOff()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				Action = "UPDATE",
				FullName = null,
				EnableCodeMapping = false
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("Error - There is no Organization with the following values:", processingLogText);
		}

		public void TestUpdateOrganisationWithoutFullNameWithCodeMappingOn_CreatePreExistingOrg()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				Action = "UPDATE",
				FullName = null
			};
			CreatePreExistingOrganisation();

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("OrgHeader - 0 inserts, 0 updates, 0 deletes", processingLogText);
		}

		public void TestUpdateOrganisationWithoutFullNameWithCodeMappingOff_CreatePreExistingOrg()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				Action = "UPDATE",
				FullName = null,
				EnableCodeMapping = false
			};
			CreatePreExistingOrganisation();

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("OrgHeader - 0 inserts, 0 updates, 0 deletes", processingLogText);
		}

		public void TestDeleteOrganisationWithoutFullNameWithCodeMappingOn()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				Action = "DELETE",
				FullName = null
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("Error - There is no Organization with the following values:", processingLogText);
		}

		public void TestDeleteOrganisationWithoutFullNameWithCodeMappingOff()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				Action = "DELETE",
				FullName = null,
				EnableCodeMapping = false
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("Error - There is no Organization with the following values:", processingLogText);
		}

		public void TestDeleteOrganisationWithoutFullNameWithCodeMappingOn_CreatePreExistingOrg()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				Action = "DELETE",
				FullName = null
			};
			CreatePreExistingOrganisation();

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("OrgHeader - 0 inserts, 0 updates, 1 deletes", processingLogText);
		}

		public void TestDeleteOrganisationWithoutFullNameWithCodeMappingOff_CreatePreExistingOrg()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault();
			var testValues = new TestValues
			{
				Action = "DELETE",
				FullName = null,
				EnableCodeMapping = false
			};
			CreatePreExistingOrganisation();

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("OrgHeader - 0 inserts, 0 updates, 1 deletes", processingLogText);
		}

		#endregion

		#region Allow Code Changes Registry Setting

		public void TestInsertOrganisationWithoutFullNameWithCodeMappingOn_AllowCodeChanges()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(enableCodeChanges: true);
			var testValues = new TestValues
			{
				FullName = null
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("Error - Full Name could not be empty when Organization Code Generation is enabled", processingLogText);
		}

		public void TestMergeOrganisationWithoutFullNameWithCodeMappingOn_AllowCodeChanges()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(enableCodeChanges: true);
			var testValues = new TestValues
			{
				Action = "MERGE",
				FullName = null
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("Error - Full Name could not be empty when Organization Code Generation is enabled", processingLogText);
		}

		public void TestMergeOrganisationWithoutFullNameWithCodeMappingOn_CreatePreExistingOrg_AllowCodeChanges()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(enableCodeChanges: true);
			var testValues = new TestValues
			{
				Action = "MERGE",
				FullName = null
			};
			CreatePreExistingOrganisation();

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("OrgHeader - 0 inserts, 0 updates, 0 deletes", processingLogText);
		}

		#endregion

		#region No Name in code generation

		public void TestInsertOrganisationWithoutFullNameWithCodeMappingOn_NoNameUsedForCodeGeneration()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(shouldUseFullName: false);
			var testValues = new TestValues
			{
				FullName = null
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("OrgHeader - 1 inserts, 0 updates, 0 deletes", processingLogText);
			AssertCodeMapping("TAL");
		}

		public void TestMergeOrganisationWithoutFullNameWithCodeMappingOn_NoNameUsedForCodeGeneration()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(shouldUseFullName: false);
			var testValues = new TestValues
			{
				Action = "MERGE",
				FullName = null
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("OrgHeader - 1 inserts, 0 updates, 0 deletes", processingLogText);
			AssertCodeMapping("TAL");
		}

		public void TestMergeOrganisationWithoutFullNameWithCodeMappingOn_CreatePreExistingOrg_NoNameUsedForCodeGeneration()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(shouldUseFullName: false);
			var testValues = new TestValues
			{
				Action = "MERGE",
				FullName = null
			};
			CreatePreExistingOrganisation();

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("OrgHeader - 0 inserts, 0 updates, 0 deletes", processingLogText);
		}

		#endregion

		#endregion

		#region Without Country Code

		#region Default Registry Settings

		public void TestInsertOrganisationWithoutCountryCodeWithCodeMappingOn()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(shouldUseIata: false, shouldUseCountry: true);
			var testValues = new TestValues
			{
				CountryCode = null
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("CountryCode should be taken from UNLOCO.", "OrgHeader - 1 inserts, 0 updates, 0 deletes", processingLogText);
			AssertCodeMapping("FREFUDUS");
		}

		public void TestInsertOrganisationWithoutCountryCodeOrUNLOCOWithCodeMappingOn()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(shouldUseIata: false, shouldUseCountry: true);
			var testValues = new TestValues
			{
				CountryCode = null,
				RefUNLOCO = null
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("Due to Organization Code Generation settings, either the UNLOCO provided in the [ClosestPort.Code] element must not be empty, or a Country must be provided.", processingLogText);
		}

		public void TestInsertOrganisationWithoutCountryCodeOrUNLOCOWithCodeMappingOff()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(shouldUseIata: false, shouldUseCountry: true);
			var testValues = new TestValues
			{
				EnableCodeMapping = false,
				CountryCode = null,
				RefUNLOCO = null
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("Error - Due to Organization Code Generation settings, either the UNLOCO provided in the [ClosestPort.Code] element must not be empty, or a Country must be provided.", processingLogText);
		}

		public void TestMergeOrganisationWithoutCountryCodeOrUNLOCOWithCodeMappingOn()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(shouldUseIata: false, shouldUseCountry: true);
			var testValues = new TestValues
			{
				Action = "MERGE",
				CountryCode = null,
				RefUNLOCO = null
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("Due to Organization Code Generation settings, either the UNLOCO provided in the [ClosestPort.Code] element must not be empty, or a Country must be provided.", processingLogText);
		}

		public void TestMergeOrganisationUsingCountryCodeWithoutUNLOCOWithCodeMappingOff()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(shouldUseIata: false, shouldUseCountry: true);
			var testValues = new TestValues
			{
				Action = "MERGE",
				EnableCodeMapping = false,
				RefUNLOCO = null
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("Error - Due to Organization Code Generation settings, either the UNLOCO provided in the [ClosestPort.Code] element must not be empty, or a Country must be provided.", processingLogText);
		}

		public void TestMergeOrganisationWithoutCountryCodeOrUNLOCOWithCodeMappingOn_CreatePreExistingOrg()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(shouldUseIata: false, shouldUseCountry: true);
			var testValues = new TestValues
			{
				Action = "MERGE",
				CountryCode = null,
				RefUNLOCO = null
			};
			CreatePreExistingOrganisation(); // Code is set to be FREFUDTAL here AFTER code generation is run.

			// Act
			var processingLogText = ImportData(testValues); // code generation should trigger here, using the registry settings 

			// Assert
			AssertContains("OrgHeader - 0 inserts, 1 updates, 0 deletes", processingLogText);
			AssertCodeMapping("FREFUDTAL");
		}

		public void TestMergeOrganisationWithoutCountryCodeOrUNLOCOWithCodeMappingOff_CreatePreExistingOrg()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(shouldUseIata: false, shouldUseCountry: true);
			var testValues = new TestValues
			{
				Action = "MERGE",
				CountryCode = null,
				EnableCodeMapping = false,
				RefUNLOCO = null
			};
			CreatePreExistingOrganisation(); // Code is set to be FREFUDTAL here AFTER code generation is run.

			// Act
			var processingLogText = ImportData(testValues); // code generation should trigger here, using the registry settings 

			// Assert
			AssertContains("OrgHeader - 0 inserts, 1 updates, 0 deletes", processingLogText);
			AssertCodeMapping("FREFUDTAL");
		}

		public void TestUpdateOrganisationWithoutCountryCodeOrUNLOCOWithCodeMappingOn()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(shouldUseIata: false, shouldUseCountry: true);
			var testValues = new TestValues
			{
				Action = "UPDATE",
				CountryCode = null,
				RefUNLOCO = null
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("Error - There is no Organization with the following values:", processingLogText);
		}

		public void TestUpdateOrganisationWithoutCountryCodeOrUNLOCOWithCodeMappingOff()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(shouldUseIata: false, shouldUseCountry: true);
			var testValues = new TestValues
			{
				Action = "UPDATE",
				CountryCode = null,
				EnableCodeMapping = false,
				RefUNLOCO = null
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("Error - There is no Organization with the following values:", processingLogText);
		}

		public void TestUpdateOrganisationWithoutCountryCodeOrUNLOCOWithCodeMappingOn_CreatePreExistingOrg()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(shouldUseIata: false, shouldUseCountry: true);
			var testValues = new TestValues
			{
				Action = "UPDATE",
				CountryCode = null,
				RefUNLOCO = null
			};
			CreatePreExistingOrganisation(); // Code is set to be FREFUDTAL here AFTER code generation is run.

			// Act
			var processingLogText = ImportData(testValues); // code generation should trigger here, using the registry settings

			// Assert
			AssertContains("OrgHeader - 0 inserts, 1 updates, 0 deletes", processingLogText);
			AssertCodeMapping("FREFUDTAL");
		}

		public void TestUpdateOrganisationWithoutCountryCodeOrUNLOCOWithCodeMappingOff_CreatePreExistingOrg()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(shouldUseIata: false, shouldUseCountry: true);
			var testValues = new TestValues
			{
				Action = "UPDATE",
				CountryCode = null,
				EnableCodeMapping = false,
				RefUNLOCO = null
			};
			CreatePreExistingOrganisation(); // Code is set to be FREFUDTAL here AFTER code generation is run.

			// Act
			var processingLogText = ImportData(testValues); // code generation should trigger here, using the registry settings

			// Assert
			AssertContains("OrgHeader - 0 inserts, 1 updates, 0 deletes", processingLogText);
			AssertCodeMapping("FREFUDTAL");
		}

		public void TestDeleteOrganisationWithoutCountryCodeOrUNLOCOWithCodeMappingOn()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(shouldUseIata: false, shouldUseCountry: true);
			var testValues = new TestValues
			{
				Action = "DELETE",
				CountryCode = null,
				RefUNLOCO = null
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("Error - There is no Organization with the following values:", processingLogText);
		}

		public void TestDeleteOrganisationWithoutCountryCodeOrUNLOCOWithCodeMappingOff()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(shouldUseIata: false, shouldUseCountry: true);
			var testValues = new TestValues
			{
				Action = "DELETE",
				CountryCode = null,
				EnableCodeMapping = false,
				RefUNLOCO = null
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("Error - There is no Organization with the following values:", processingLogText);
		}

		public void TestDeleteOrganisationWithoutCountryCodeOrUNLOCOWithCodeMappingOn_CreatePreExistingOrg()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(shouldUseIata: false, shouldUseCountry: true);
			var testValues = new TestValues
			{
				Action = "DELETE",
				CountryCode = null,
				RefUNLOCO = null
			};
			CreatePreExistingOrganisation();

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("OrgHeader - 0 inserts, 0 updates, 1 deletes", processingLogText);
		}

		public void TestDeleteOrganisationWithoutCountryCodeOrUNLOCOWithCodeMappingOff_CreatePreExistingOrg()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(shouldUseIata: false, shouldUseCountry: true);
			var testValues = new TestValues
			{
				Action = "DELETE",
				CountryCode = null,
				EnableCodeMapping = false,
				RefUNLOCO = null
			};
			CreatePreExistingOrganisation();

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("OrgHeader - 0 inserts, 0 updates, 1 deletes", processingLogText);
		}

		#endregion

		#region Allow Code Changes Registry Setting

		public void TestInsertOrganisationWithoutCountryCodeOrUNLOCOWithCodeMappingOn_AllowCodeChanges()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(shouldUseIata: false, enableCodeChanges: true, shouldUseCountry: true);
			var testValues = new TestValues
			{
				CountryCode = null,
				RefUNLOCO = null
			};
			CreatePreExistingOrganisation(); // Code is set to be FREFUDTAL here AFTER code generation is run.

			// Act
			var processingLogText = ImportData(testValues); // code generation should trigger here, using the registry settings

			// Assert
			AssertContains("Error - The value of Organization Code must be unique on Organization. The duplicate value(s) are: (FFINC).", processingLogText);
		}

		public void TestMergeOrganisationWithoutCountryCodeOrUNLOCOWithCodeMappingOn_AllowCodeChanges()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(shouldUseIata: false, enableCodeChanges: true, shouldUseCountry: true);
			var testValues = new TestValues
			{
				Action = "MERGE",
				CountryCode = null,
				RefUNLOCO = null
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("Error - Due to Organization Code Generation settings, either the UNLOCO provided in the [ClosestPort.Code] element must not be empty, or a Country must be provided.", processingLogText);
		}

		public void TestMergeOrganisationWithoutCountryCodeOrUNLOCOWithCodeMappingOn_CreatePreExistingOrg_AllowCodeChanges()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(shouldUseIata: false, enableCodeChanges: true, shouldUseCountry: true);
			var testValues = new TestValues
			{
				Action = "MERGE",
				CountryCode = null,
				RefUNLOCO = null
			};
			CreatePreExistingOrganisation();

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("OrgHeader - 0 inserts, 1 updates, 0 deletes", processingLogText);
			AssertCodeMapping("FREFUDTAL");
		}

		#endregion

		#region No Country Code in code generation

		public void TestInsertOrganisationWithoutCountryCodeOrUNLOCOWithCodeMappingOn_NoCodeUsedForCodeGeneration()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(shouldUseIata: false, shouldUseCountry: true);
			var testValues = new TestValues
			{
				CountryCode = null,
				RefUNLOCO = null
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("Error - Due to Organization Code Generation settings, either the UNLOCO provided in the [ClosestPort.Code] element must not be empty, or a Country must be provided.", processingLogText);
		}

		public void TestMergeOrganisationWithoutCountryCodeOrUNLOCOWithCodeMappingOn_NoCodeUsedForCodeGeneration()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(shouldUseIata: false, shouldUseCountry: true);
			var testValues = new TestValues
			{
				Action = "MERGE",
				CountryCode = null
			};

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("OrgHeader - 1 inserts, 0 updates, 0 deletes", processingLogText);
			AssertCodeMapping("FREFUDUS");
		}

		public void TestMergeOrganisationWithoutCountryCodeOrUNLOCOWithCodeMappingOn_CreatePreExistingOrg_NoCodeUsedForCodeGeneration()
		{
			// Arrange
			SetupOrgCodeGenerationRegistryDefault(shouldUseIata: false, shouldUseCountry: true);
			var testValues = new TestValues
			{
				Action = "MERGE",
				CountryCode = null
			};
			CreatePreExistingOrganisation();

			// Act
			var processingLogText = ImportData(testValues);

			// Assert
			AssertContains("OrgHeader - 0 inserts, 1 updates, 0 deletes", processingLogText);
			AssertCodeMapping("FREFUDTAL");
		}

		#endregion

		#endregion

		#region Implementation

		public class TestValues
		{
			public bool EnableCodeMapping = true;
			public string Action = "INSERT";
			public Guid? PK = Guid.NewGuid();
			public string Code = "FFINC";
			public string FullName = "Freddy Fudrucker Inc";
			public string RefUNLOCO = "USTAL";
			public string CountryCode = "US";
		}

		void CreatePreExistingOrganisation()
		{
			var org = Factory.New<OrgHeader>();

			org.OH_FullName = "Freddy Fudrucker Inc";
			org.OH_RL_NKClosestPort = "USTAL";
			org.OH_Code = "FREFUDTAL";

			var defaultOrgPK = CodeMappingRepository.DefaultOrgPK(Factory);

			var codeMapping = new CodeMappingRepository(Factory).New();
			codeMapping.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			codeMapping.OO_LocalGuid = org.PK;
			codeMapping.OO_LocalCode = org.OH_Code;

			codeMapping.OO_ForeignCode = "FFINC";
			codeMapping.OO_OH = defaultOrgPK;

			Factory.Save();
			foreach (var orgInvoiceRollupOrGroup in Factory.Load<OrgInvoiceRollupOrGroup>(new ZQuery()))
			{
				orgInvoiceRollupOrGroup.Delete();
			}
			Factory.Save();
		}

		void AssertCodeMapping(string expectedMapping)
		{
			var mapping = new CodeMappingRepository(Factory).Load(Core.Constants.OrgPatternMatchOverrideRelationships.Organisation, "FFINC", CodeMappingRepository.DefaultOrgPK(Factory));
			AssertEquals(expectedMapping, mapping?.OO_LocalCode);
		}

		void SetupOrgCodeGenerationRegistryDefault(bool shouldUseIata = true, bool shouldUseFullName = true, bool shouldUseCountry = false, bool enableCodeChanges = false, bool regenerateCodeOnChanges = true)
		{
			var orgCodeAlgorithm = new OrgCodeAlgorithm
			{
				AlgorithmType = OrgCodeAlgorithmType.Default,
				RegenerateOrgCodeOnChanges = regenerateCodeOnChanges
			};

			if (shouldUseFullName)
			{
				orgCodeAlgorithm.Elements[OrgCodeElementDescription.FirstName].Order = 1;
				orgCodeAlgorithm.Elements[OrgCodeElementDescription.SecondName].Order = 2;
				orgCodeAlgorithm.Elements[OrgCodeElementDescription.FirstName].Length = 3;
				orgCodeAlgorithm.Elements[OrgCodeElementDescription.SecondName].Length = 3;
			}
			if (shouldUseIata)
			{
				orgCodeAlgorithm.Elements[OrgCodeElementDescription.IataCode].Order = 3;
				orgCodeAlgorithm.Elements[OrgCodeElementDescription.IataCode].Length = 3;
			}
			if (shouldUseCountry)
			{
				orgCodeAlgorithm.Elements[OrgCodeElementDescription.CountryCode].Order = 4;
				orgCodeAlgorithm.Elements[OrgCodeElementDescription.CountryCode].Length = 2;
			}
			Environment.Env.Registry.CanUserEditOrganisationCode = enableCodeChanges;
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, orgCodeAlgorithm);
		}

		string ImportData(TestValues defaultValues)
		{
			var xml = $@"
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>YOUAUS_AU</OwnerCode>
    <EnableCodeMapping>{defaultValues.EnableCodeMapping}</EnableCodeMapping>
  </Header>
  <Body>
    <Organization version=""2.0"">
      <OrgHeader Action=""{defaultValues.Action}"">
        <Code>{defaultValues.Code}</Code>
        {(defaultValues.FullName != null ? $"<FullName>{defaultValues.FullName}</FullName>" : string.Empty)}

        {(defaultValues.RefUNLOCO != null || defaultValues.CountryCode != null ? $@"<ClosestPort TableName=""RefUNLOCO"">
          <Code>{defaultValues.RefUNLOCO}</Code>" : string.Empty)}
        {(defaultValues.RefUNLOCO != null || defaultValues.CountryCode != null ? $@"<Country>{defaultValues.CountryCode}</Country>
        </ClosestPort>" : string.Empty)}
      </OrgHeader>
    </Organization>
  </Body>
</Native>";

			var xmlSessionTracker = new DummyXmlImportLogger();
			var handler = new NativeXmlRequestHandler(NativeDataTypeList.Codes.Organization, xmlSessionTracker);
			var request = handler.CreateRequestMessage();
			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(xml)))
			{
				request.SetMessageTextSource(stream);
				request.Save();
				try
				{
					using (handler.Process(request))
					{
					}
				}
				catch (ZDataException)
				{
				}

				return xmlSessionTracker.Logs;
			}
		}

		#endregion
	}
}
