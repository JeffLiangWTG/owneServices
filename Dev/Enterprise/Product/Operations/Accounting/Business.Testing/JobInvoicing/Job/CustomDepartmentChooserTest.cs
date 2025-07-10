using System;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	class CustomDepartmentChooserTest : CustomChooserBaseTest
	{
		public void TestWhenPluginIsNotBusinessObject()
		{
			AssertEquals("Department code should be blank", ZString.Empty, Obj.GetCodeWithConfiguration(new DummyJobInvoicingPluginIsNotBusinessObject(), CustomConfig1));
			AssertEquals("Department codes should be equal", "FES", Obj.GetCodeWithConfiguration(new DummyJobInvoicingPluginIsBusinessObjectProviderForDocumentWrapper(Creator.CreateShipment("S00001000")), CustomConfig1));
			AssertEquals("Department code should be blank", ZString.Empty, Obj.GetCodeWithConfiguration(new DummyJobInvoicingPluginIsBusinessObjectProviderForDocumentWrapper(null), CustomConfig1));
		}

		public void TestValidateConfigWithJob()
		{
			var jobWithoutParent = Creator.Job1;
			var job = Creator.CreateJob(Creator.CreateShipment("S00001000"));
			Factory.Save();

			var result = Obj.ValidateConfigWithJob(ZGuid.Empty, "return 'BRN'");
			AssertEquals("Validation result", "Please select a Job to validate.", result);

			result = Obj.ValidateConfigWithJob(ZGuid.NewZGuid(), "return 'BRN'");
			AssertEquals("Validation result", "Error validating with selected Job, please select a different one.", result);

			result = Obj.ValidateConfigWithJob(jobWithoutParent.PK, "return 'BRN'");
			AssertEquals("Validation result", "Error validating with selected Job, please select a different one.", result);

			result = Obj.ValidateConfigWithJob(job.PK, "***");
			AssertEquals("Validation result", "Error/No Result, will default department as per other registry settings.", result);

			Creator.FEADepartment.GE_IsActive = false;
			Factory.Save();
			result = Obj.ValidateConfigWithJob(job.PK, "return 'FEA'");
			AssertEquals("Validation result", "FEA (Inactive), will default to blank department.", result);

			result = Obj.ValidateConfigWithJob(job.PK, "return 'FFF'");
			AssertEquals("Validation result", "FFF (Invalid), will default to blank department.", result);

			result = Obj.ValidateConfigWithJob(job.PK, "return 'FES'");
			AssertEquals("Validation result", Creator.FESDepartment.GE_Code, result);
		}

		public void TestGetDepartmentCode()
		{
			var shipment = Creator.CreateShipment("S00001000");

			SetCustomDefaultDepartmentConfigInRegistry(CustomConfig1);
			AssertEquals("Department codes should be equal", "FES", Obj.GetCodeWithConfiguration(shipment));

			SetCustomDefaultDepartmentConfigInRegistry(CustomConfig2);
			AssertEquals("Department codes should be equal", "BRN", Obj.GetCodeWithConfiguration(shipment));

			SetCustomDefaultDepartmentConfigInRegistry(CustomConfigForNonExistingPropertyAccessed);
			AssertEquals("Department code should be blank", ZString.Empty, Obj.GetCodeWithConfiguration(shipment));

			SetCustomDefaultDepartmentConfigInRegistry(CustomConfigWithoutNullCheck);
			AssertEquals("Department code should be blank", ZString.Empty, Obj.GetCodeWithConfiguration(shipment));

			SetCustomDefaultDepartmentConfigInRegistry(CustomConfigWithoutReturnValue);
			AssertEquals("Department code should be blank", ZString.Empty, Obj.GetCodeWithConfiguration(shipment));

			SetCustomDefaultDepartmentConfigInRegistry(CustomConfigReturningEmptyString);
			AssertEquals("Department code should be blank", ZString.Empty, Obj.GetCodeWithConfiguration(shipment));

			SetCustomDefaultDepartmentConfigInRegistry(ZString.Empty);
			AssertEquals("Department code should be blank", ZString.Empty, Obj.GetCodeWithConfiguration(shipment));

			SetCustomDefaultDepartmentConfigInRegistry(CustomConfig2);
			AssertEquals("Department codes should be equal", "FES", Obj.GetCodeWithConfiguration(shipment, CustomConfig1));

			AssertEquals("Department codes should be equal", ZString.Empty, Obj.GetCodeWithConfiguration(null, CustomConfig1));
		}

		public void TestObjectsPassedAsArguments()
		{
			var shipment = Creator.CreateShipment("S00001000");

			SetCustomDefaultDepartmentConfigInRegistry(@"return obj.BaseShipment.JS_UniqueConsignRef");
			AssertEquals("Department codes should be equal", shipment.JS_UniqueConsignRef, Obj.GetCodeWithConfiguration(shipment));

			SetCustomDefaultDepartmentConfigInRegistry(@"return company.GC_Code");
			AssertEquals("Department codes should be equal", GlbCompany.CurrentCompany.GC_Code, Obj.GetCodeWithConfiguration(shipment));

			SetCustomDefaultDepartmentConfigInRegistry(@"return branch.GB_Code");
			AssertEquals("Department codes should be equal", GlbBranch.CurrentBranch.GB_Code, Obj.GetCodeWithConfiguration(shipment));

			SetCustomDefaultDepartmentConfigInRegistry(@"return department.GE_Code");
			AssertEquals("Department codes should be equal", GlbDepartment.CurrentDepartment.GE_Code, Obj.GetCodeWithConfiguration(shipment));
		}

		void SetCustomDefaultDepartmentConfigInRegistry(ZString config)
		{
			CustomDefaultDepartmentConfiguration customDefaultDepartmentConfiguration = new CustomDefaultDepartmentConfiguration(FallbackLevel, Creator.Factory);
			customDefaultDepartmentConfiguration.ConfigAsString = ORtfTextUtil.TextToRtf(config);
			AccountingConfigurationRegistry.Instance.CustomDefaultDepartmentConfiguration.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, customDefaultDepartmentConfiguration);
		}

		CustomDepartmentChooser Obj
		{
			get
			{
				if (obj == null)
				{
					obj = new CustomDepartmentChooser(Factory);
				}
				return obj;
			}
		}
		CustomDepartmentChooser obj;

		ZString CustomConfig1
		{
			get
			{
				return @"debtorGroup = obj.ControllingCustomer.Organisation.CompanyData.ARDebtorGroup
mainImpCmdty = obj.JobHeaderLocalClient.Organisation.MiscServ.CMMainImportCmdty
dept = ''
if obj.BaseShipment.InvoicingSupporter.ConsumerType.Code == 'SHP' and obj.ShipmentTransportMode.Code == 'SEA'and obj.ShipmentContainerMode.Code == 'LCL' and obj.BaseShipment.InvoicingSupporter.ConsolType == 'NCN' and obj.ControllingAgent.MainAddress.Country.Code == '' and (debtorGroup is None or debtorGroup.OJ_Code == '') and (mainImpCmdty is None or mainImpCmdty.RH_Code == '') and not obj.BaseShipment.InvoicingSupporter.IsCrossTrade:
	dept = 'FES'
else:
	dept = 'BRN'
return dept";
			}
		}

		ZString CustomConfig2
		{
			get
			{
				return @"debtorGroup = obj.ControllingCustomer.Organisation.CompanyData.ARDebtorGroup
mainImpCmdty = obj.JobHeaderLocalClient.Organisation.MiscServ.CMMainImportCmdty
dept = ''
if obj.BaseShipment.InvoicingSupporter.ConsumerType.Code == 'SHP' and obj.ShipmentTransportMode.Code == 'SEA'and obj.ShipmentContainerMode.Code == 'LCL' and obj.BaseShipment.InvoicingSupporter.ConsolType == 'NCN' and obj.ControllingAgent.MainAddress.Country.Code == '' and (debtorGroup is None or debtorGroup.OJ_Code == '') and (mainImpCmdty is None or mainImpCmdty.RH_Code == '') and obj.BaseShipment.InvoicingSupporter.IsCrossTrade:
	dept = 'FES'
else:
	dept = 'BRN'
return dept";
			}
		}

		ZString CustomConfigForNonExistingPropertyAccessed
		{
			get
			{
				return @"debtorGroup = obj.ControllingCustomer.Organisation.CompanyData.ARDebtorGroup.SomePropertyThatDoesNotExist
mainImpCmdty = obj.JobHeaderLocalClient.Organisation.MiscServ.CMMainImportCmdty
dept = ''
if obj.BaseShipment.InvoicingSupporter.ConsumerType.Code == 'SHP' and obj.ShipmentTransportMode.Code == 'SEA'and obj.ShipmentContainerMode.Code == 'LCL' and obj.BaseShipment.InvoicingSupporter.ConsolType == 'NCN' and obj.ControllingAgent.MainAddress.Country.Code == '' and (debtorGroup is None or debtorGroup.OJ_Code == '') and (mainImpCmdty is None or mainImpCmdty.RH_Code == '') and not obj.BaseShipment.InvoicingSupporter.IsCrossTrade:
	dept = 'FES'
else:
	dept = 'BRN'
return dept";
			}
		}

		ZString CustomConfigWithoutNullCheck
		{
			get
			{
				return @"debtorGroup = obj.ControllingCustomer.Organisation.CompanyData.ARDebtorGroup
mainImpCmdty = obj.JobHeaderLocalClient.Organisation.MiscServ.CMMainImportCmdty
dept = ''
if obj.BaseShipment.InvoicingSupporter.ConsumerType.Code == 'SHP' and obj.ShipmentTransportMode.Code == 'SEA'and obj.ShipmentContainerMode.Code == 'LCL' and obj.BaseShipment.InvoicingSupporter.ConsolType == 'NCN' and obj.ControllingAgent.MainAddress.Country.Code == '' and (debtorGroup is None or debtorGroup.OJ_Code == '') and (mainImpCmdty.RH_Code == '') and not obj.BaseShipment.InvoicingSupporter.IsCrossTrade:
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
				return @"debtorGroup = obj.ControllingCustomer.Organisation.CompanyData.ARDebtorGroup
mainImpCmdty = obj.JobHeaderLocalClient.Organisation.MiscServ.CMMainImportCmdty
dept = ''
if obj.BaseShipment.InvoicingSupporter.ConsumerType.Code == 'SHP' and obj.ShipmentTransportMode.Code == 'SEA'and obj.ShipmentContainerMode.Code == 'LCL' and obj.BaseShipment.InvoicingSupporter.ConsolType == 'NCN' and obj.ControllingAgent.MainAddress.Country.Code == '' and (debtorGroup is None or debtorGroup.OJ_Code == '') and (mainImpCmdty is None or mainImpCmdty.RH_Code == '') and not obj.BaseShipment.InvoicingSupporter.IsCrossTrade:
	dept = 'FES'
else:
	dept = 'BRN'";
			}
		}

		ZString CustomConfigReturningEmptyString
		{
			get
			{
				return @"debtorGroup = obj.ControllingCustomer.Organisation.CompanyData.ARDebtorGroup
mainImpCmdty = obj.JobHeaderLocalClient.Organisation.MiscServ.CMMainImportCmdty
dept = ''
if obj.BaseShipment.InvoicingSupporter.ConsumerType.Code == 'SHP' and obj.ShipmentTransportMode.Code == 'SEA'and obj.ShipmentContainerMode.Code == 'LCL' and obj.BaseShipment.InvoicingSupporter.ConsolType == 'NCN' and obj.ControllingAgent.MainAddress.Country.Code == '' and (debtorGroup is None or debtorGroup.OJ_Code == '') and (mainImpCmdty is None or mainImpCmdty.RH_Code == '') and not obj.BaseShipment.InvoicingSupporter.IsCrossTrade:
	dept = 'FES'
else:
	dept = 'BRN'
return ''";
			}
		}
	}
}