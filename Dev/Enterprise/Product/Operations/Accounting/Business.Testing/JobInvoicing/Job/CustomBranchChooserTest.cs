using System;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	class CustomBranchChooserTest : CustomChooserBaseTest
	{
		protected override void SetUp()
		{
			base.SetUp();

			TestBranch = Creator.CreateBranch("AAA", GlbCompany.CurrentCompany);
			Creator.CreateBranch("BBB", GlbCompany.CurrentCompany);
			Factory.Save();
		}
		public void TestWhenPluginIsNotBusinessObject()
		{
			AssertEquals("Branch code should be blank", ZString.Empty, Obj.GetCodeWithConfiguration(new DummyJobInvoicingPluginIsNotBusinessObject(), CustomConfig1));
			AssertEquals("Branch code should be equal", "AAA", Obj.GetCodeWithConfiguration(new DummyJobInvoicingPluginIsBusinessObjectProviderForDocumentWrapper(Creator.CreateShipment("S00001000")), CustomConfig1));
			AssertEquals("Branch code should be blank", ZString.Empty, Obj.GetCodeWithConfiguration(new DummyJobInvoicingPluginIsBusinessObjectProviderForDocumentWrapper(null), CustomConfig1));
		}

		public void TestValidateConfigWithJob()
		{
			var jobWithoutParent = Creator.Job1;
			var job = Creator.CreateJob(Creator.CreateShipment("S00001000"));
			Factory.Save();

			var result = Obj.ValidateConfigWithJob(ZGuid.Empty, "return 'AAA'");
			AssertEquals("Validation result", "Please select a Job to validate.", result);

			result = Obj.ValidateConfigWithJob(ZGuid.NewZGuid(), "return 'AAA'");
			AssertEquals("Validation result", "Error validating with selected Job, please select a different one.", result);

			result = Obj.ValidateConfigWithJob(jobWithoutParent.PK, "return 'AAA'");
			AssertEquals("Validation result", "Error validating with selected Job, please select a different one.", result);

			result = Obj.ValidateConfigWithJob(job.PK, "***");
			AssertEquals("Validation result", "Error/No Result, will default branch as per other registry settings.", result);

			TestBranch.GB_IsActive = false;
			Factory.Save();
			result = Obj.ValidateConfigWithJob(job.PK, "return 'AAA'");
			AssertEquals("Validation result", "AAA (Inactive), will default to blank branch.", result);

			result = Obj.ValidateConfigWithJob(job.PK, "return 'FFF'");
			AssertEquals("Validation result", "FFF (Invalid), will default to blank branch.", result);

			TestBranch.GB_IsActive = true;
			Factory.Save();
			result = Obj.ValidateConfigWithJob(job.PK, "return 'AAA'");
			AssertEquals("Validation result", TestBranch.GB_Code, result);
		}

		public void TestGetBranchCode()
		{
			var shipment = Creator.CreateShipment("S00001000");

			SetCustomDefaultBranchConfigInRegistry(CustomConfig1);
			AssertEquals("Branch codes should be equal", "AAA", Obj.GetCodeWithConfiguration(shipment));

			SetCustomDefaultBranchConfigInRegistry(CustomConfig2);
			AssertEquals("Branch codes should be equal", "BBB", Obj.GetCodeWithConfiguration(shipment));

			SetCustomDefaultBranchConfigInRegistry(CustomConfigForNonExistingPropertyAccessed);
			AssertEquals("Branch code should be blank", ZString.Empty, Obj.GetCodeWithConfiguration(shipment));

			SetCustomDefaultBranchConfigInRegistry(CustomConfigWithoutNullCheck);
			AssertEquals("Branch code should be blank", ZString.Empty, Obj.GetCodeWithConfiguration(shipment));

			SetCustomDefaultBranchConfigInRegistry(CustomConfigWithoutReturnValue);
			AssertEquals("Branch code should be blank", ZString.Empty, Obj.GetCodeWithConfiguration(shipment));

			SetCustomDefaultBranchConfigInRegistry(CustomConfigReturningEmptyString);
			AssertEquals("Branch code should be blank", ZString.Empty, Obj.GetCodeWithConfiguration(shipment));

			SetCustomDefaultBranchConfigInRegistry(ZString.Empty);
			AssertEquals("Branch code should be blank", ZString.Empty, Obj.GetCodeWithConfiguration(shipment));

			SetCustomDefaultBranchConfigInRegistry(CustomConfig2);
			AssertEquals("Branch codes should be equal", "BBB", Obj.GetCodeWithConfiguration(shipment, CustomConfig2));

			AssertEquals("Branch codes should be equal", ZString.Empty, Obj.GetCodeWithConfiguration(null, CustomConfig2));
		}

		public void TestObjectsPassedAsArguments()
		{
			var shipment = Creator.CreateShipment("S00001000");

			SetCustomDefaultBranchConfigInRegistry(@"return obj.BaseShipment.JS_UniqueConsignRef");
			AssertEquals("Branch codes should be equal", shipment.JS_UniqueConsignRef, Obj.GetCodeWithConfiguration(shipment));

			SetCustomDefaultBranchConfigInRegistry(@"return company.GC_Code");
			AssertEquals("Branch codes should be equal", GlbCompany.CurrentCompany.GC_Code, Obj.GetCodeWithConfiguration(shipment));

			SetCustomDefaultBranchConfigInRegistry(@"return branch.GB_Code");
			AssertEquals("Branch codes should be equal", GlbBranch.CurrentBranch.GB_Code, Obj.GetCodeWithConfiguration(shipment));

			SetCustomDefaultBranchConfigInRegistry(@"return department.GE_Code");
			AssertEquals("Branch codes should be equal", GlbDepartment.CurrentDepartment.GE_Code, Obj.GetCodeWithConfiguration(shipment));
		}

		void SetCustomDefaultBranchConfigInRegistry(ZString config)
		{
			var customDefaultBranchConfiguration = new CustomDefaultBranchConfiguration(FallbackLevel, Creator.Factory);
			customDefaultBranchConfiguration.ConfigAsString = ORtfTextUtil.TextToRtf(config);
			AccountingConfigurationRegistry.Instance.CustomDefaultBranchConfiguration.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, customDefaultBranchConfiguration);
		}

		CustomBranchChooser Obj
		{
			get
			{
				if (obj == null)
				{
					obj = new CustomBranchChooser(Factory);
				}
				return obj;
			}
		}
		CustomBranchChooser obj;

		GlbBranch TestBranch;

		ZString CustomConfig1
		{
			get
			{
				return @"dept = ''
if obj.BaseShipment.InvoicingSupporter.ConsumerType.Code == 'SHP' and obj.ShipmentTransportMode.Code == 'SEA'and obj.ShipmentContainerMode.Code == 'LCL' and obj.BaseShipment.InvoicingSupporter.ConsolType == 'NCN' and obj.ControllingAgent.MainAddress.Country.Code == '' and not obj.BaseShipment.InvoicingSupporter.IsCrossTrade:
	dept = 'AAA'
else:
	dept = 'BBB'
return dept";
			}
		}

		ZString CustomConfig2
		{
			get
			{
				return @"dept = ''
if obj.BaseShipment.InvoicingSupporter.ConsumerType.Code == 'SHP' and obj.ShipmentTransportMode.Code == 'SEA'and obj.ShipmentContainerMode.Code == 'LCL' and obj.BaseShipment.InvoicingSupporter.ConsolType == 'NCN' and obj.ControllingAgent.MainAddress.Country.Code == '' and not obj.BaseShipment.InvoicingSupporter.IsCrossTrade:
	dept = 'BBB'
else:
	dept = 'AAA'
return dept";
			}
		}

		ZString CustomConfigForNonExistingPropertyAccessed
		{
			get
			{
				return @"dept = ''
if obj.BaseShipment.SomePropertyThatDoesNotExist == 'XXX':
	dept = 'AAA'
else:
	dept = 'BBB'
return dept";
			}
		}

		ZString CustomConfigWithoutNullCheck
		{
			get
			{
				return @"dept = ''
if obj.BaseShipment.InvoicingSupporter.ConsumerType.Code == 'SHP' and obj.ShipmentTransportMode.Code == 'SEA'and and obj.ControllingAgent.MainAddress.Country.Code == '' and not obj.BaseShipment.InvoicingSupporter.IsCrossTrade:
	dept = 'FES'
else:
	dept = 'BRN'
return dept";
			}
		}

		ZString CustomConfigWithoutReturnValue
		{
			get
			{
				return @"dept = ''
if obj.BaseShipment.InvoicingSupporter.ConsumerType.Code == 'SHP':
	dept = 'FES'
else:
	dept = 'BRN'";
			}
		}

		ZString CustomConfigReturningEmptyString
		{
			get
			{
				return @"dept = ''
if obj.BaseShipment.InvoicingSupporter.ConsumerType.Code == 'SHP' and obj.ShipmentTransportMode.Code == 'SEA':
	dept = 'FES'
else:
	dept = 'BRN'
return ''";
			}
		}
	}
}