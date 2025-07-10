using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class JobDeclarationValidation : Customs.Business.BaseJobDeclarationValidation
	{
		public JobDeclarationValidation(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public JobDeclaration JobDeclaration
		{
			get { return Parent as JobDeclaration; }
		}

		protected override void CheckJE_TotalWeightUnit()
		{
			base.CheckJE_TotalWeightUnit();
			ListValidation.ErrorIfInvalidCode(JobDeclaration.JE_TotalWeightUnitInfo, JobDeclaration.Lookups.WeightUnitList);
		}

		protected override void CheckJE_HouseBill()
		{
			base.CheckJE_HouseBill();
			BillValidation.CheckNotEmptyForEdifact(JobDeclaration.JE_HouseBillInfo);
		}

		protected override void CheckJE_OwnerRef()
		{
			base.CheckJE_OwnerRef();

			var declaration = JobDeclaration;
			if (declaration.JE_UseOwnerRefAsQuarantineRef && declaration.JE_OwnerRef.IsEmpty && declaration.IsNEXDOCSActive)
			{
				declaration.JE_OwnerRefInfo.AddMessageError(Res.GetString("6EC1287A-0EE2-49F7-AFC3-6E115F366A49", "Owners Reference is required when the 'Use Owners Reference as Exporters Reference' option is ticked."));
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateJE_PaidUnderProtestStatement();
			ValidateJE_PartShipConsignmentReference();
		}

		public void ValidateJE_PartShipConsignmentReference()
		{
			ValidateCalculatedProperty(JobDeclaration.JE_PartShipConsignmentReferenceInfo);
		}

		protected virtual void CheckJE_PartShipConsignmentReference()
		{
			if (JobDeclaration.IsPartShipConsignmentReferenceRelevant)
			{
				var primaryHouseBill = JobDeclaration.PrimaryHouseBill;

				if (primaryHouseBill != null)
				{
					JobDeclaration.JE_PartShipConsignmentReferenceInfo.AddAllNotificationsFrom(primaryHouseBill.CU_fPartShipConsignmentReferenceInfo);
				}
			}
		}

		public void ValidateJE_PaidUnderProtestStatement()
		{
			ValidateCalculatedProperty(JobDeclaration.JE_PaidUnderProtestStatementInfo);
		}

		protected virtual void CheckJE_PaidUnderProtestStatement()
		{
		}

		public void ValidateJE_AmberStatement()
		{
			ValidateCalculatedProperty(JobDeclaration.JE_AmberStatementInfo);
		}

		protected virtual void CheckJE_AmberStatement()
		{
		}

		protected override bool ShouldValidatePackagesActualPackageCount
		{
			get { return false; }
		}

		protected override void CheckJE_MessageType()
		{
			base.CheckJE_MessageType();
			if (JobDeclaration.IsQuarantine && !AUCustomsDataRegistry.Instance.EnableAQISDeclarationMessaging.Value)
			{
				JobDeclaration.JE_MessageTypeInfo.AddError(@"Quarantine Entries require a registry change. 
Customs->Australia->Quarantine Declaration->Enable Quarantine Declaration Messaging.
Messaging charges are incurred for permits.
Please refer to your system administrator.");
			}
		}

		#region ExportDeclarationNumber
		bool exportDeclarationNumberChecked;
		public void ValidateExportDeclarationNumber()
		{
			if (!exportDeclarationNumberChecked)
			{
				exportDeclarationNumberChecked = true;
				JobDeclaration.DeclarationNumberInfo.ClearAllNotifications();
				CheckExportDeclarationNumber();
			}
		}

		protected virtual void CheckExportDeclarationNumber()
		{
		}
		#endregion

		#region Implementation

		public new MessageValidation MessageValidation
		{
			get { return (MessageValidation)base.MessageValidation; }
		}

		protected override Customs.Business.ExternalMessageValidation GetNewExternalMessageValidation()
		{
			return new MessageValidation(JobDeclaration);
		}

		#endregion
	}
}
