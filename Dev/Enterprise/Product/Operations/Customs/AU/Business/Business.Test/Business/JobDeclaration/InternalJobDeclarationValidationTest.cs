using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class InternalJobDeclarationValidationTest : JobDeclarationValidationTest
	{
		public override void TestMergeByForExport()
		{
			Assert("AU Export does not merge", true);
		}

		public void TestValidateJE_PartShipConsignmentReference()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			declaration.JE_TransportMode = "AIR";
			declaration.JE_PartShipConsignmentReference = "X1";
			AssertHasWarning(declaration.JE_PartShipConsignmentReferenceInfo, BillValidation.ConsignReferenceEntered);

			using (declaration.GetValidationSuspender())
			{
				declaration.JE_TransportMode = "SEA";
				AssertEquals("PreCondition", string.Empty, declaration.JE_PartShipConsignmentReference);
				AssertHasWarning(declaration.JE_PartShipConsignmentReferenceInfo, BillValidation.ConsignReferenceEntered);
			}

			declaration.Validation.ValidateAll();
			AssertNoWarning(declaration.JE_PartShipConsignmentReferenceInfo, BillValidation.ConsignReferenceEntered);
		}

		public void TestCheckJE_TotalWeightUnit()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_TotalWeightUnit = "CS";
			AssertHasErrorContaining(declaration.JE_TotalWeightUnitInfo, "Enter a valid Total Weight UQ");
			declaration.JE_TotalWeightUnit = "KG";
			AssertNoErrorContaining(declaration.JE_TotalWeightUnitInfo, "Enter a valid Total Weight UQ");
		}

		public void TestHouseBillsValidation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_HouseBill = "";
			Assert("Pre-Condition, The HouseBill is blank and there are no message errors", !declaration.JE_HouseBillInfo.HasMessageErrors());
			declaration.JE_HouseBill = "12345, 32424";
			Assert("The HouseBills are filled in correctly and there are no message errors", !declaration.JE_HouseBillInfo.HasMessageErrors());
			declaration.JE_HouseBill = "12345";
			Assert("The HouseBill is filled in correctly and there are no message errors", !declaration.JE_HouseBillInfo.HasMessageErrors());
			declaration.JE_HouseBill = "@@##$$";
			Assert("The HouseBill is incorrectly filled in with invalid characters there should be message errors.", declaration.JE_HouseBillInfo.HasMessageErrors());
		}

		public void TestValidateJE_ContainerMode()
		{
			// Mandatory
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			AssertEquals(false, declaration.JE_ContainerModeInfo.HasNotifications());
			declaration.JE_ContainerMode = "";
			AssertEquals(true, declaration.JE_ContainerModeInfo.HasMessageErrors());
			declaration.JE_ContainerMode = Core.Constants.TransportModes.Air;
			AssertEquals(false, declaration.JE_ContainerModeInfo.HasErrors());
			declaration.JE_ContainerMode = "XZ";
			AssertEquals(true, declaration.HasMessageErrors);

			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Other;
			AssertEquals("Transport Mode of OTH should not cause validation errors for defaulted Container value", false, declaration.JE_ContainerModeInfo.HasNotifications());
		}

		public void TestTotalNoPacksAndUnitEntered()
		{
			declaration.JE_TotalNoOfPacks = 2;
			declaration.JE_TotalNoOfPacksPackType = "";
			AssertEquals("Unit is required", true, declaration.JE_TotalNoOfPacksPackTypeInfo.HasNotifications());

			declaration.JE_TotalNoOfPacksPackType = "PKG";
			AssertEquals("Unit is required", false, declaration.JE_TotalNoOfPacksPackTypeInfo.HasNotifications());
		}

		[TestDate(2004, 9, 20)]
		public void TestTotalNoPacksAndUnitEntered2()
		{
			declaration.JE_TotalNoOfPacksPackType = "PKG";
			AssertEquals("Unit is required", true, declaration.JE_TotalNoOfPacksInfo.HasNotifications());

			declaration.JE_TotalNoOfPacks = 2;
			AssertEquals("Unit is required", false, declaration.JE_TotalNoOfPacksInfo.HasNotifications());
		}

		public void TestValidJE_DateOfArrival()
		{
			declaration.JE_ExportDate = ZDateTime.Today;
			declaration.JE_DateOfFirstArrival = declaration.JE_ExportDate.AddDays(4);
			declaration.JE_DateOfArrival = declaration.JE_DateOfFirstArrival.AddDays(7);
			AssertEquals("JE_DateOfArrival should have no notifications", false, declaration.JE_DateOfArrivalInfo.HasNotifications());
		}

		public void TestDateOfArrivalLessThanExportDate()
		{
			declaration.JE_ExportDate = ZDateTime.Today;
			declaration.JE_DateOfArrival = declaration.JE_ExportDate.AddDays(-23);
			AssertEquals("JE_DateOfArrival should have message errors", true, declaration.JE_DateOfArrivalInfo.HasMessageErrors());
		}

		public void TestDateOfArrivalLessThanFirstArrival()
		{
			declaration.JE_ExportDate = ZDateTime.Today;
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_RL_NKPortOfFirstArrival = "AUMEL";

			declaration.JE_DateOfFirstArrival = declaration.JE_ExportDate.AddDays(5);
			declaration.JE_DateOfArrival = declaration.JE_DateOfFirstArrival.AddDays(-2);
			AssertEquals("JE_DateOfArrival should have message errors", true, declaration.JE_DateOfArrivalInfo.HasMessageErrors());

			declaration.JE_DateOfArrival = declaration.JE_DateOfFirstArrival.AddDays(1);
			AssertEquals("JE_DateOfArrival should have message errors", false, declaration.JE_DateOfArrivalInfo.HasMessageErrors());
		}

		public void TestAutoUpdateArrivalDates()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_RL_NKPortOfFirstArrival = "AUSYD";
			declaration.JE_DateOfArrival = new ZDateTime(2014, 1, 20);
			AssertEquals("DateOfArrival should autopopulate an empty DateOfFirstArrival", declaration.JE_DateOfArrival, declaration.JE_DateOfFirstArrival);
			declaration.JE_DateOfFirstArrival = new ZDateTime(2014, 1, 25);
			AssertEquals("DateOfFirstArrival change should should be reflected in DateOfFirstArrival", declaration.JE_DateOfArrival, declaration.JE_DateOfFirstArrival);
			declaration.JE_DateOfArrival = new ZDateTime(2014, 2, 1);
			AssertEquals("DateOfFirstArrival change should should be reflected in DateOfFirstArrival", declaration.JE_DateOfArrival, declaration.JE_DateOfFirstArrival);
		}

		public void TestCheckJE_MessageType()
		{
			AUCustomsDataRegistry.Instance.EnableAQISDeclarationMessaging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			AssertHasError(declaration.JE_MessageTypeInfo, @"Quarantine Entries require a registry change. 
Customs->Australia->Quarantine Declaration->Enable Quarantine Declaration Messaging.
Messaging charges are incurred for permits.
Please refer to your system administrator.");
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertNoError(declaration.JE_MessageTypeInfo, @"Quarantine Entries require a registry change. 
Customs->Australia->Quarantine Declaration->Enable Quarantine Declaration Messaging.
Messaging charges are incurred for permits.
Please refer to your system administrator.");
			AUCustomsDataRegistry.Instance.EnableAQISDeclarationMessaging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			AssertNoError(declaration.JE_MessageTypeInfo, @"Quarantine Entries require a registry change. 
Customs->Australia->Quarantine Declaration->Enable Quarantine Declaration Messaging.
Messaging charges are incurred for permits.
Please refer to your system administrator.");
		}

		public void TestCheckJE_OwnerRef()
		{
			using (AUCustomsDataRegistry.Instance.EnableAQISDeclarationMessaging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
				declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

				declaration.QuarantineInvoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
				declaration.JE_UseOwnerRefAsQuarantineRef = true;
				declaration.JE_OwnerRef = "";
				declaration.Validation.ValidateJE_OwnerRef();

				Assert("NEXDOCS not active", !declaration.IsNEXDOCSActive);
				AssertNoMessageError(declaration.JE_OwnerRefInfo, "Owners Reference is required when the 'Use Owners Reference as Exporters Reference' option is ticked.");

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
				{
					Assert("NEXDOCS is active", declaration.IsNEXDOCSActive);

					declaration.JE_UseOwnerRefAsQuarantineRef = true;
					declaration.JE_OwnerRef = "REF";
					AssertNoMessageError(declaration.JE_OwnerRefInfo, "Owners Reference is required when the 'Use Owners Reference as Exporters Reference' option is ticked.");
					declaration.JE_OwnerRef = "";
					AssertHasMessageError(declaration.JE_OwnerRefInfo, "Owners Reference is required when the 'Use Owners Reference as Exporters Reference' option is ticked.");

					declaration.JE_UseOwnerRefAsQuarantineRef = false;
					declaration.Validation.ValidateJE_OwnerRef();
					AssertNoMessageError(declaration.JE_OwnerRefInfo, "Owners Reference is required when the 'Use Owners Reference as Exporters Reference' option is ticked.");

					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					declaration.JE_UseOwnerRefAsQuarantineRef = true;
					declaration.Validation.ValidateJE_OwnerRef();

					Assert("NEXDOCS not active", !declaration.IsNEXDOCSActive);
					AssertNoMessageError(declaration.JE_OwnerRefInfo, "Owners Reference is required when the 'Use Owners Reference as Exporters Reference' option is ticked.");
				}
			}
		}

		#region Port Codes

		public void TestValidateJE_RL_NKFinalDestination()
		{
			declaration.JE_RL_NKFinalDestination = "?????";
			Assert(declaration.JE_RL_NKFinalDestinationInfo.HasMessageErrors());
		}

		public void TestValidateJE_RL_NKOrigin()
		{
			Assert("Should start without notification", !declaration.JE_RL_NKOriginInfo.HasNotifications());
			declaration.JE_RL_NKOrigin = "";
			Assert("Should have message error after validating origin", declaration.JE_RL_NKOriginInfo.HasMessageErrors());
			declaration.JE_RL_NKOrigin = "?????";
			Assert(declaration.JE_RL_NKOriginInfo.HasMessageErrors());
			declaration.JE_RL_NKOrigin = "AUSYD";
			Assert("Should clear error after setting origin", !declaration.JE_RL_NKOriginInfo.HasNotifications());
		}

		public void TestValidateJE_RL_NKPortOfArrival()
		{
			declaration.JE_RL_NKPortOfArrival = "?????";
			Assert(declaration.JE_RL_NKPortOfArrivalInfo.HasMessageErrors());
		}

		public void TestValidateJE_RL_NKPortOfFirstArrival()
		{
			declaration.JE_RL_NKPortOfFirstArrival = "?????";
			Assert(declaration.JE_RL_NKPortOfFirstArrivalInfo.HasMessageErrors());
		}

		public void TestValidateJE_RL_NKPortOfLoading()
		{
			declaration.JE_RL_NKPortOfLoading = "?????";
			Assert(declaration.JE_RL_NKPortOfLoadingInfo.HasMessageErrors());
		}

		#endregion

		#region Implementation

		protected override JobDeclarationValidation GetNewValidationProvider(JobDeclaration jobDeclaration)
		{
			return new JobDeclarationValidationTestRig(jobDeclaration);
		}

		#endregion
	}
}
