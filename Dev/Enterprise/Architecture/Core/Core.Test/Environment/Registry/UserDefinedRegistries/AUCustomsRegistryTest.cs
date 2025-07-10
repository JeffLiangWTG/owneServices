using System;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class AUCustomsRegistryTest : TransactionedTestCase
	{
		#region Edifice

		public void TestEdificeSendErrors()
		{
			AssertEquals("EdificeSendErrors", "ESG", Registry.AUCustoms.EdificeSendErrors);
			Registry.AUCustoms.EdificeSendErrors = "ENG";
			AssertEquals("EdificeSendErrors", "ENG", Registry.AUCustoms.EdificeSendErrors);

			Registry.RawRegistry.EdificeSendErrors.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "NOE");
			AssertEquals("Value", "NOE", Registry.AUCustoms.EdificeSendErrors);
		}

		public void TestEdificeSendErrorsToGroup()
		{
			AssertEquals("EdificeSendErrorsToGroup", Guid.Empty, Registry.AUCustoms.EdificeSendErrorsToGroup);
			Guid newGuid = Guid.NewGuid();
			Registry.AUCustoms.EdificeSendErrorsToGroup = newGuid;
			AssertEquals("EdificeSendErrorsToGroup", newGuid, Registry.AUCustoms.EdificeSendErrorsToGroup);

			newGuid = Guid.NewGuid();
			Registry.RawRegistry.EdificeSendErrorsToGroup.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, newGuid);
			AssertEquals("Value", newGuid, Registry.AUCustoms.EdificeSendErrorsToGroup);
		}

		public void TestEdificeSendAcknowledgements()
		{
			AssertEquals("EdificeSendAcknowledgements", "ESG", Registry.AUCustoms.EdificeSendAcknowledgements);
			Registry.AUCustoms.EdificeSendAcknowledgements = "ENG";
			AssertEquals("EdificeSendAcknowledgements", "ENG", Registry.AUCustoms.EdificeSendAcknowledgements);

			Registry.RawRegistry.EdificeSendAcknowledgements.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "NOE");
			AssertEquals("Value", "NOE", Registry.AUCustoms.EdificeSendAcknowledgements);
		}

		public void TestEdificeSendAcknowledgementsToGroup()
		{
			AssertEquals("EdificeSendAcknowledgementsToGroup", Guid.Empty, Registry.AUCustoms.EdificeSendAcknowledgementsToGroup);
			Guid newGuid = Guid.NewGuid();
			Registry.AUCustoms.EdificeSendAcknowledgementsToGroup = newGuid;
			AssertEquals("EdificeSendAcknowledgementsToGroup", newGuid, Registry.AUCustoms.EdificeSendAcknowledgementsToGroup);

			newGuid = Guid.NewGuid();
			Registry.RawRegistry.EdificeSendAcknowledgementsToGroup.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, newGuid);
			AssertEquals("Value", newGuid, Registry.AUCustoms.EdificeSendAcknowledgementsToGroup);
		}

		public void TestEdificeSendErrorsToGroupForBranch()
		{
			var companyPK = EnvProxy.Instance.CurrentCompany.PK;
			var branchPK = EnvProxy.Instance.CurrentBranch.PK;
			AssertEquals("EdificeSendErrorsToGroup", Guid.Empty, Registry.AUCustoms.EdificeSendErrorsToGroup);
			AssertEquals("EdificeSendErrorsToGroupForBranch", Guid.Empty, Registry.AUCustoms.EdificeSendErrorsToGroupForBranch(companyPK, branchPK));

			var newGuid = Guid.NewGuid();
			Registry.AUCustoms.EdificeSendErrorsToGroup = newGuid;
			AssertEquals("EdificeSendErrorsToGroup", newGuid, Registry.AUCustoms.EdificeSendErrorsToGroup);
			AssertEquals("EdificeSendErrorsToGroupForBranch", newGuid, Registry.AUCustoms.EdificeSendErrorsToGroupForBranch(companyPK, branchPK));

			newGuid = Guid.NewGuid();
			Registry.RawRegistry.EdificeSendErrorsToGroup.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, newGuid);
			AssertEquals("Value", newGuid, Registry.AUCustoms.EdificeSendErrorsToGroup);
			AssertEquals("EdificeSendErrorsToGroupForBranch", newGuid, Registry.AUCustoms.EdificeSendErrorsToGroupForBranch(companyPK, branchPK));
		}

		public void TestEdificeSendAcknowledgementsToGroupForBranch()
		{
			var companyPK = EnvProxy.Instance.CurrentCompany.PK;
			var branchPK = EnvProxy.Instance.CurrentBranch.PK;
			AssertEquals("EdificeSendAcknowledgementsToGroup", Guid.Empty, Registry.AUCustoms.EdificeSendAcknowledgementsToGroup);
			AssertEquals("EdificeSendAcknowledgementsToGroupForBranch", Guid.Empty, Registry.AUCustoms.EdificeSendAcknowledgementsToGroupForBranch(companyPK, branchPK));

			var newGuid = Guid.NewGuid();
			Registry.AUCustoms.EdificeSendAcknowledgementsToGroup = newGuid;
			AssertEquals("EdificeSendAcknowledgementsToGroup", newGuid, Registry.AUCustoms.EdificeSendAcknowledgementsToGroup);
			AssertEquals("EdificeSendAcknowledgementsToGroupForBranch", newGuid, Registry.AUCustoms.EdificeSendAcknowledgementsToGroupForBranch(companyPK, branchPK));

			newGuid = Guid.NewGuid();
			Registry.RawRegistry.EdificeSendAcknowledgementsToGroup.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, newGuid);
			AssertEquals("Value", newGuid, Registry.AUCustoms.EdificeSendAcknowledgementsToGroup);
			AssertEquals("EdificeSendAcknowledgementsToGroupForBranch", newGuid, Registry.AUCustoms.EdificeSendAcknowledgementsToGroupForBranch(companyPK, branchPK));
		}

		public void TestEdificeSendImpedimentsToGroupForBranch()
		{
			var companyPK = EnvProxy.Instance.CurrentCompany.PK;
			var branchPK = EnvProxy.Instance.CurrentBranch.PK;
			AssertEquals("EdificeSendImpedimentsToGroup", Guid.Empty, Registry.AUCustoms.EdificeSendImpedimentsToGroup);
			AssertEquals("EdificeSendImpedimentsToGroupForBranch", Guid.Empty, Registry.AUCustoms.EdificeSendImpedimentsToGroupForBranch(companyPK, branchPK));

			var newGuid = Guid.NewGuid();
			Registry.AUCustoms.EdificeSendImpedimentsToGroup = newGuid;
			AssertEquals("EdificeSendImpedimentsToGroup", newGuid, Registry.AUCustoms.EdificeSendImpedimentsToGroup);
			AssertEquals("EdificeSendImpedimentsToGroupForBranch", newGuid, Registry.AUCustoms.EdificeSendImpedimentsToGroupForBranch(companyPK, branchPK));

			newGuid = Guid.NewGuid();
			Registry.RawRegistry.EdificeSendImpedimentsToGroup.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, newGuid);
			AssertEquals("Value", newGuid, Registry.AUCustoms.EdificeSendImpedimentsToGroup);
			AssertEquals("EdificeSendImpedimentsToGroupForBranch", newGuid, Registry.AUCustoms.EdificeSendImpedimentsToGroupForBranch(companyPK, branchPK));
		}

		public void TestExportDeclarationSendErrorsToGroupForBranch()
		{
			var companyPK = EnvProxy.Instance.CurrentCompany.PK;
			var branchPK = EnvProxy.Instance.CurrentBranch.PK;
			AssertEquals("ExportDeclarationSendErrorsToGroup", Guid.Empty, Registry.AUCustoms.ExportDeclarationSendErrorsToGroup);
			AssertEquals("ExportDeclarationSendErrorsToGroupForBranch", Guid.Empty, Registry.AUCustoms.ExportDeclarationSendErrorsToGroupForBranch(companyPK, branchPK));

			var newGuid = Guid.NewGuid();
			Registry.AUCustoms.ExportDeclarationSendErrorsToGroup = newGuid;
			AssertEquals("ExportDeclarationSendErrorsToGroup", newGuid, Registry.AUCustoms.ExportDeclarationSendErrorsToGroup);
			AssertEquals("ExportDeclarationSendErrorsToGroupForBranch", newGuid, Registry.AUCustoms.ExportDeclarationSendErrorsToGroupForBranch(companyPK, branchPK));

			newGuid = Guid.NewGuid();
			Registry.RawRegistry.ExportDeclarationSendErrorsToGroup.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, newGuid);
			AssertEquals("Value", newGuid, Registry.AUCustoms.ExportDeclarationSendErrorsToGroup);
			AssertEquals("ExportDeclarationSendErrorsToGroupForBranch", newGuid, Registry.AUCustoms.ExportDeclarationSendErrorsToGroupForBranch(companyPK, branchPK));
		}

		public void TestExportDeclarationSendAcknowledgementsToGroupForBranch()
		{
			var companyPK = EnvProxy.Instance.CurrentCompany.PK;
			var branchPK = EnvProxy.Instance.CurrentBranch.PK;
			AssertEquals("ExportDeclarationSendAcknowledgementsToGroup", Guid.Empty, Registry.AUCustoms.ExportDeclarationSendAcknowledgementsToGroup);
			AssertEquals("ExportDeclarationSendAcknowledgementsToGroupForBranch", Guid.Empty, Registry.AUCustoms.ExportDeclarationSendAcknowledgementsToGroupForBranch(companyPK, branchPK));

			var newGuid = Guid.NewGuid();
			Registry.AUCustoms.ExportDeclarationSendAcknowledgementsToGroup = newGuid;
			AssertEquals("ExportDeclarationSendAcknowledgementsToGroup", newGuid, Registry.AUCustoms.ExportDeclarationSendAcknowledgementsToGroup);
			AssertEquals("ExportDeclarationSendAcknowledgementsToGroupForBranch", newGuid, Registry.AUCustoms.ExportDeclarationSendAcknowledgementsToGroupForBranch(companyPK, branchPK));

			newGuid = Guid.NewGuid();
			Registry.RawRegistry.ExportDeclarationSendAcknowledgementsToGroup.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, newGuid);
			AssertEquals("Value", newGuid, Registry.AUCustoms.ExportDeclarationSendAcknowledgementsToGroup);
			AssertEquals("ExportDeclarationSendAcknowledgementsToGroupForBranch", newGuid, Registry.AUCustoms.ExportDeclarationSendAcknowledgementsToGroupForBranch(companyPK, branchPK));
		}

		public void TestExportDeclarationSendImpedimentsToGroupForBranch()
		{
			var companyPK = EnvProxy.Instance.CurrentCompany.PK;
			var branchPK = EnvProxy.Instance.CurrentBranch.PK;
			AssertEquals("ExportDeclarationSendImpedimentsToGroup", Guid.Empty, Registry.AUCustoms.ExportDeclarationSendImpedimentsToGroup);
			AssertEquals("ExportDeclarationSendImpedimentsToGroupForBranch", Guid.Empty, Registry.AUCustoms.ExportDeclarationSendImpedimentsToGroupForBranch(companyPK, branchPK));

			var newGuid = Guid.NewGuid();
			Registry.AUCustoms.ExportDeclarationSendImpedimentsToGroup = newGuid;
			AssertEquals("ExportDeclarationSendImpedimentsToGroup", newGuid, Registry.AUCustoms.ExportDeclarationSendImpedimentsToGroup);
			AssertEquals("ExportDeclarationSendImpedimentsToGroupForBranch", newGuid, Registry.AUCustoms.ExportDeclarationSendImpedimentsToGroupForBranch(companyPK, branchPK));

			newGuid = Guid.NewGuid();
			Registry.RawRegistry.ExportDeclarationSendImpedimentsToGroup.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, newGuid);
			AssertEquals("Value", newGuid, Registry.AUCustoms.ExportDeclarationSendImpedimentsToGroup);
			AssertEquals("ExportDeclarationSendImpedimentsToGroupForBranch", newGuid, Registry.AUCustoms.ExportDeclarationSendImpedimentsToGroupForBranch(companyPK, branchPK));
		}

		public void TestEdificeSendImpediments()
		{
			AssertEquals("EdificeSendImpediments", "ESG", Registry.AUCustoms.EdificeSendImpediments);
			Registry.AUCustoms.EdificeSendImpediments = "ENG";
			AssertEquals("EdificeSendImpediments", "ENG", Registry.AUCustoms.EdificeSendImpediments);

			Registry.RawRegistry.EdificeSendImpediments.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "NOE");
			AssertEquals("Value", "NOE", Registry.AUCustoms.EdificeSendImpediments);
		}

		public void TestEdificeSendImpedimentsToGroup()
		{
			AssertEquals("EdificeSendImpedimentsToGroup", Guid.Empty, Registry.AUCustoms.EdificeSendImpedimentsToGroup);
			Guid newGuid = Guid.NewGuid();
			Registry.AUCustoms.EdificeSendImpedimentsToGroup = newGuid;
			AssertEquals("EdificeSendImpedimentsToGroup", newGuid, Registry.AUCustoms.EdificeSendImpedimentsToGroup);

			newGuid = Guid.NewGuid();
			Registry.RawRegistry.EdificeSendImpedimentsToGroup.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, newGuid);
			AssertEquals("Value", newGuid, Registry.AUCustoms.EdificeSendImpedimentsToGroup);
		}

		#endregion

		#region Export Declaration

		public void TestExportDeclarationSendErrors()
		{
			AssertEquals("ExportDeclarationSendErrors", "ESG", Registry.AUCustoms.ExportDeclarationSendErrors);
			Registry.AUCustoms.ExportDeclarationSendErrors = "ENG";
			AssertEquals("ExportDeclarationSendErrors", "ENG", Registry.AUCustoms.ExportDeclarationSendErrors);

			Registry.RawRegistry.ExportDeclarationSendErrors.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "NOE");
			AssertEquals("Value", "NOE", Registry.AUCustoms.ExportDeclarationSendErrors);
		}

		public void TestExportDeclarationSendErrorsToGroup()
		{
			AssertEquals("ExportDeclarationSendErrorsToGroup", Guid.Empty, Registry.AUCustoms.ExportDeclarationSendErrorsToGroup);
			Guid newGuid = Guid.NewGuid();
			Registry.AUCustoms.ExportDeclarationSendErrorsToGroup = newGuid;
			AssertEquals("ExportDeclarationSendErrorsToGroup", newGuid, Registry.AUCustoms.ExportDeclarationSendErrorsToGroup);

			newGuid = Guid.NewGuid();
			Registry.RawRegistry.ExportDeclarationSendErrorsToGroup.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, newGuid);
			AssertEquals("Value", newGuid, Registry.AUCustoms.ExportDeclarationSendErrorsToGroup);
		}

		public void TestExportDeclarationSendAcknowledgements()
		{
			AssertEquals("ExportDeclarationSendAcknowledgements", "ESG", Registry.AUCustoms.ExportDeclarationSendAcknowledgements);
			Registry.AUCustoms.ExportDeclarationSendAcknowledgements = "ENG";
			AssertEquals("ExportDeclarationSendAcknowledgements", "ENG", Registry.AUCustoms.ExportDeclarationSendAcknowledgements);

			Registry.RawRegistry.ExportDeclarationSendAcknowledgements.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "NOE");
			AssertEquals("Value", "NOE", Registry.AUCustoms.ExportDeclarationSendAcknowledgements);
		}

		public void TestExportDeclarationSendAcknowledgementsToGroup()
		{
			AssertEquals("ExportDeclarationSendAcknowledgementsToGroup", Guid.Empty, Registry.AUCustoms.ExportDeclarationSendAcknowledgementsToGroup);
			Guid newGuid = Guid.NewGuid();
			Registry.AUCustoms.ExportDeclarationSendAcknowledgementsToGroup = newGuid;
			AssertEquals("ExportDeclarationSendAcknowledgementsToGroup", newGuid, Registry.AUCustoms.ExportDeclarationSendAcknowledgementsToGroup);

			newGuid = Guid.NewGuid();
			Registry.RawRegistry.ExportDeclarationSendAcknowledgementsToGroup.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, newGuid);
			AssertEquals("Value", newGuid, Registry.AUCustoms.ExportDeclarationSendAcknowledgementsToGroup);
		}

		public void TestExportDeclarationSendImpediments()
		{
			AssertEquals("ExportDeclarationSendImpediments", "ESG", Registry.AUCustoms.ExportDeclarationSendImpediments);
			Registry.AUCustoms.ExportDeclarationSendImpediments = "ENG";
			AssertEquals("ExportDeclarationSendImpediments", "ENG", Registry.AUCustoms.ExportDeclarationSendImpediments);

			Registry.RawRegistry.ExportDeclarationSendImpediments.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "NOE");
			AssertEquals("Value", "NOE", Registry.AUCustoms.ExportDeclarationSendImpediments);
		}

		public void TestExportDeclarationSendImpedimentsToGroup()
		{
			AssertEquals("ExportDeclarationSendImpedimentsToGroup", Guid.Empty, Registry.AUCustoms.ExportDeclarationSendImpedimentsToGroup);
			Guid newGuid = Guid.NewGuid();
			Registry.AUCustoms.ExportDeclarationSendImpedimentsToGroup = newGuid;
			AssertEquals("ExportDeclarationSendImpedimentsToGroup", newGuid, Registry.AUCustoms.ExportDeclarationSendImpedimentsToGroup);

			newGuid = Guid.NewGuid();
			Registry.RawRegistry.ExportDeclarationSendImpedimentsToGroup.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, newGuid);
			AssertEquals("Value", newGuid, Registry.AUCustoms.ExportDeclarationSendImpedimentsToGroup);
		}

		public void TestNEXDOCSDisableQRPView()
		{
			AssertEquals("ExportDeclarationNexdocDisableQRPView Default Value", true, Registry.AUCustoms.NEXDOCSDisableQRPView);
			using (Registry.RawRegistry.NEXDOCSDisableQRPView.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("ExportDeclarationNexdocDisableQRPView Configured Value", false, Registry.AUCustoms.NEXDOCSDisableQRPView);
			}
		}

		#endregion

		public void TestLocalCustomsBranchIdentifier()
		{
			Registry.RawRegistry.LocalCustomsBranchIdentifier.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "Bah");
			AssertEquals("LocalCustomsBranchIdentifier", "Bah", Registry.AUCustoms.LocalCustomsBranchIdentifier);
		}

		public void TestSetLocalCustomsBranchIdentifier()
		{
			Registry.AUCustoms.SetLocalCustomsBranchIdentifierForBranch(EnvProxy.Instance.CurrentBranch.PK, "Test");
			AssertEquals("LocalCustomsBranchIdentifier", "Test", Registry.AUCustoms.LocalCustomsBranchIdentifier);
		}

		public void TestGetLocalCustomsBranchIdentifier()
		{
			var branchPK = EnvProxy.Instance.CurrentBranch.PK;
			Registry.AUCustoms.SetLocalCustomsBranchIdentifierForBranch(branchPK, "Test");
			AssertEquals("LocalCustomsBranchIdentifier", "Test", Registry.AUCustoms.GetLocalCustomsBranchIdentifierForBranch(branchPK));
		}

		public void TestLastDateCertificatesChecked()
		{
			Registry.AUCustoms.LastDateCertificatesChecked = EnvProxy.Instance.Time.CurrentLocalDate;
			AssertEquals("LastDateCertificatesChecked", EnvProxy.Instance.Time.CurrentLocalDate, Registry.AUCustoms.LastDateCertificatesChecked);
		}

		public void TestCarrierMovementAdviceGroup()
		{
			Guid myGuid = Guid.NewGuid();
			Registry.AUCustoms.CarrierMovementAdviceGroup = myGuid;
			AssertEquals("Guid", myGuid, Registry.AUCustoms.CarrierMovementAdviceGroup);
			Registry.AUCustoms.CarrierMovementAdviceGroup = Guid.Empty;
			AssertEquals("Guid", Guid.Empty, Registry.AUCustoms.CarrierMovementAdviceGroup);
		}

		public void TestGetCarrierMovementAdviceGroupForBranchOrCurrentCompany()
		{
			Guid guid1 = Guid.NewGuid();
			Guid guid2 = Guid.NewGuid();
			Registry.AUCustoms.SetCarrierMovementAdviceGroupForBranch(EnvProxy.Instance.CurrentBranch.PK, guid1);
			AssertEquals("Value", guid1, Registry.AUCustoms.GetCarrierMovementAdviceGroupForBranchOrCurrentCompany(EnvProxy.Instance.CurrentBranch.PK));
			Registry.AUCustoms.CarrierMovementAdviceGroup = guid2;
			AssertEquals("Value", guid1, Registry.AUCustoms.GetCarrierMovementAdviceGroupForBranchOrCurrentCompany(EnvProxy.Instance.CurrentBranch.PK));
			Registry.AUCustoms.DeleteCarrierMovementAdviceGroupForBranch(EnvProxy.Instance.CurrentBranch.PK);
			AssertEquals("Value", guid2, Registry.AUCustoms.GetCarrierMovementAdviceGroupForBranchOrCurrentCompany(EnvProxy.Instance.CurrentBranch.PK));
		}

		public void TestSetCarrierMovementAdviceGroupForBranch()
		{
			Guid guid1 = Guid.NewGuid();
			Registry.AUCustoms.SetCarrierMovementAdviceGroupForBranch(EnvProxy.Instance.CurrentBranch.PK, guid1);
			AssertEquals("Value", guid1, Registry.AUCustoms.GetCarrierMovementAdviceGroupForBranchOrCurrentCompany(EnvProxy.Instance.CurrentBranch.PK));
		}

		public void TestAlertCarrierMovementLoad()
		{
			Registry.AUCustoms.AlertCarrierMovementLoad = false;
			AssertEquals("AlertCarrierMovementLoad", false, Registry.AUCustoms.AlertCarrierMovementLoad);
			Registry.AUCustoms.AlertCarrierMovementLoad = true;
			AssertEquals("AlertCarrierMovementLoad", true, Registry.AUCustoms.AlertCarrierMovementLoad);
		}

		public void TestSpecialReporterNumber()
		{
			Registry.AUCustoms.HVLVSpecialReporterNumber = "654321";
			AssertEquals("SpecialReporterNumber", "654321", Registry.AUCustoms.HVLVSpecialReporterNumber);
		}

		public void TestAirCargoShipmentType()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.Add(new CodeDescriptionPair("ABC", "ABC Shipment"));
			Registry.RawRegistry.AirCargoShipmentType.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, list);
			ReadOnlyCodeDescriptionPairList obtainedList = Registry.AUCustoms.AirCargoShipmentType;
			AssertEquals("ObtainedList.Count", 1, obtainedList.Count);
			AssertEquals("GetDescriptionFromCode(\"ABC\")", "ABC Shipment", obtainedList.GetDescriptionFromCode("ABC"));
		}

		public void TestAirCargoCommercialStatus()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.Add(new CodeDescriptionPair("ABC", "ABC Shipment"));
			Registry.RawRegistry.AirCargoCommercialStatus.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, list);
			ReadOnlyCodeDescriptionPairList obtainedList = Registry.AUCustoms.AirCargoCommercialStatus;
			AssertEquals("ObtainedList.Count", 1, obtainedList.Count);
			AssertEquals("GetDescriptionFromCode(\"ABC\")", "ABC Shipment", obtainedList.GetDescriptionFromCode("ABC"));
		}

		public void TestSeaCargoCommercialStatus()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.Add(new CodeDescriptionPair("ABC", "ABC Shipment"));
			Registry.RawRegistry.SeaCargoCommercialStatus.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, list);
			ReadOnlyCodeDescriptionPairList obtainedList = Registry.AUCustoms.SeaCargoCommercialStatus;
			AssertEquals("ObtainedList.Count", 1, obtainedList.Count);
			AssertEquals("GetDescriptionFromCode(\"ABC\")", "ABC Shipment", obtainedList.GetDescriptionFromCode("ABC"));
		}

		public void TestPreLodgementLicenceCode()
		{
			AssertEquals("PreLodgementLicenceCode", "", Registry.AUCustoms.PreLodgementLicenceCode);
			Registry.AUCustoms.PreLodgementLicenceCode = "12345";
			AssertEquals("PreLodgementLicenceCode", "12345", Registry.AUCustoms.PreLodgementLicenceCode);

			Registry.RawRegistry.PreLodgementLicenceCode.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, "55555");
			AssertEquals("Value", "55555", Registry.AUCustoms.PreLodgementLicenceCode);
		}

		public void TestAgentsReferenceDefaulting()
		{
			AssertEquals("AgentsReferenceDefaulting", Constants.AgentsReferenceDefaulting.DEF, Registry.AUCustoms.AgentsReferenceDefaulting);
			Registry.AUCustoms.AgentsReferenceDefaulting = Constants.AgentsReferenceDefaulting.FAR;
			AssertEquals("AgentsReferenceDefaulting", Constants.AgentsReferenceDefaulting.FAR, Registry.AUCustoms.AgentsReferenceDefaulting);

			Registry.RawRegistry.AgentsReferenceDefaulting.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.AgentsReferenceDefaulting.NSR);
			AssertEquals("AgentsReferenceDefaulting", Constants.AgentsReferenceDefaulting.NSR, Registry.AUCustoms.AgentsReferenceDefaulting);
		}

		public void TestMessageDeliveryUnderbonds()
		{
			AssertEquals("Default Acknowledgements", Constants.EmailTo.StaffMemberAndNominatedGroup, Registry.AUCustoms.UnderbondSendAcknowledgements);
			AssertEquals("Default Acknowledgements group", Guid.Empty, Registry.AUCustoms.UnderbondSendAcknowledgementsToGroup);
			AssertEquals("Default Errors", Constants.EmailTo.StaffMemberAndNominatedGroup, Registry.AUCustoms.UnderbondSendErrors);
			AssertEquals("Default Errors Group", Guid.Empty, Registry.AUCustoms.UnderbondSendErrorsToGroup);
			AssertEquals("Default Impediments", Constants.EmailTo.StaffMemberAndNominatedGroup, Registry.AUCustoms.UnderbondSendImpediments);
			AssertEquals("Default Impediments Group", Guid.Empty, Registry.AUCustoms.UnderbondSendImpedimentsToGroup);
		}

		public void TestMessageDeliveryCargoStatus()
		{
			AssertEquals("Default Acknowledgements", Constants.EmailTo.StaffMemberAndNominatedGroup, Registry.AUCustoms.CargoStatusSendAcknowledgements);
			AssertEquals("Default Acknowledgements group", Guid.Empty, Registry.AUCustoms.CargoStatusSendAcknowledgementsToGroup);
			AssertEquals("Default Errors", Constants.EmailTo.StaffMemberAndNominatedGroup, Registry.AUCustoms.CargoStatusSendErrors);
			AssertEquals("Default Errors Group", Guid.Empty, Registry.AUCustoms.CargoStatusSendErrorsToGroup);
			AssertEquals("Default Impediments", Constants.EmailTo.StaffMemberAndNominatedGroup, Registry.AUCustoms.CargoStatusSendImpediments);
			AssertEquals("Default Impediments Group", Guid.Empty, Registry.AUCustoms.CargoStatusSendImpedimentsToGroup);
		}

		#region HVLV Air Cargo Send

		public void TestHVLVAirCargoSendAcknowledgements()
		{
			AssertEquals("Default value", Constants.EmailTo.StaffMemberAndNominatedGroup, Registry.AUCustoms.HVLVAirCargoSendAcknowledgements);
			Registry.RawRegistry.HVLVAirCargoSendAcknowledgements.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.EmailTo.StaffMember);
			AssertEquals("Get value", Constants.EmailTo.StaffMember, Registry.AUCustoms.HVLVAirCargoSendAcknowledgements);
		}

		public void TestHVLVAirCargoSendErrors()
		{
			AssertEquals("Default value", Constants.EmailTo.StaffMemberAndNominatedGroup, Registry.AUCustoms.HVLVAirCargoSendErrors);
			Registry.RawRegistry.HVLVAirCargoSendErrors.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.EmailTo.StaffMember);
			AssertEquals("Get value", Constants.EmailTo.StaffMember, Registry.AUCustoms.HVLVAirCargoSendErrors);
		}

		public void TestHVLVAirCargoSendImpediments()
		{
			AssertEquals("Default value", Constants.EmailTo.StaffMemberAndNominatedGroup, Registry.AUCustoms.HVLVAirCargoSendImpediments);
			Registry.RawRegistry.HVLVAirCargoSendImpediments.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.EmailTo.StaffMember);
			AssertEquals("Get value", Constants.EmailTo.StaffMember, Registry.AUCustoms.HVLVAirCargoSendImpediments);
		}

		#endregion

		#region HVLV Sea Cargo Send

		public void TestHVLVSeaCargoSendAcknowledgements()
		{
			AssertEquals("Default value", Constants.EmailTo.StaffMemberAndNominatedGroup, Registry.AUCustoms.HVLVSeaCargoSendAcknowledgements);
			Registry.RawRegistry.HVLVSeaCargoSendAcknowledgements.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.EmailTo.StaffMember);
			AssertEquals("Get value", Constants.EmailTo.StaffMember, Registry.AUCustoms.HVLVSeaCargoSendAcknowledgements);
		}

		public void TestHVLVSeaCargoSendErrors()
		{
			AssertEquals("Default value", Constants.EmailTo.StaffMemberAndNominatedGroup, Registry.AUCustoms.HVLVSeaCargoSendErrors);
			Registry.RawRegistry.HVLVSeaCargoSendErrors.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.EmailTo.StaffMember);
			AssertEquals("Get value", Constants.EmailTo.StaffMember, Registry.AUCustoms.HVLVSeaCargoSendErrors);
		}

		public void TestHVLVSeaCargoSendImpediments()
		{
			AssertEquals("Default value", Constants.EmailTo.StaffMemberAndNominatedGroup, Registry.AUCustoms.HVLVSeaCargoSendImpediments);
			Registry.RawRegistry.HVLVSeaCargoSendImpediments.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.EmailTo.StaffMember);
			AssertEquals("Get value", Constants.EmailTo.StaffMember, Registry.AUCustoms.HVLVSeaCargoSendImpediments);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Registry = new DataRegistry();
		}

		DataRegistry Registry;

		#endregion

	}
}
