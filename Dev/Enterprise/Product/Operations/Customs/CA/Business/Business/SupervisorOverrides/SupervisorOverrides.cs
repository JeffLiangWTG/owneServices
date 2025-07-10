namespace Enterprise.Customs.CA.Business
{
	using System.Globalization;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.Business;
	using Enterprise.Environment;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Security;
	using Enterprise.ZArchitecture.Schema;

	public class SupervisorOverrides : Customs.Business.SupervisorOverrides
	{
		#region Constants
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Constants : Customs.Business.SupervisorOverrides.Constants
		{
			public const string ChangeAccountDate = "Accounting Date: '{0}' -> '{1}'";
			public const string ManualCancelRelease = "Manual Release & Cancel";
			public const string ForceSendEntryMessage = "Force Send Entry Message";
		}

		#endregion

		public SupervisorOverrides(IBusiness businessEntity, string context)
				: base(businessEntity, context)
		{
		}

		protected override void CreateMessagesCore()
		{
			if (context == SupervisorOverridesContext.ManualCancelRelease)
			{
				if (CheckSecurityRightForCheckpointRequired(Env.Security.CAManualCancelRelease))
				{
					AddMessageLog(Env.Security.CAManualCancelRelease, Constants.ManualCancelRelease);
				}
			}
			else if (context == SupervisorOverridesContext.ForceSendB3CADMessage)
			{
				if (CheckSecurityRightForCheckpointRequired(Env.Security.CAForceSendB3Message))
				{
					AddMessageLog(Env.Security.CAForceSendB3Message, Constants.ForceSendEntryMessage);
				}
			}
			else
			{
				base.CreateMessagesCore();
			}
		}

		protected override void CheckSavingDeclaration(BaseJobDeclaration declaration)
		{
			base.CheckSavingDeclaration(declaration);
			CheckAccountDates(declaration);
		}

		protected override void CheckMergeBy(BaseJobDeclaration declaration)
		{
			var newdeclaration = (JobDeclaration)declaration;
			var addInfo = newdeclaration.GetAddInfo();
			var checkMergeByRequired = addInfo.HasChangesSinceLastSaving(CAAddInfoSchema.CA_MergeBy) || !newdeclaration.IsInDatabase;
			if (checkMergeByRequired)
			{
				var mergeBy = newdeclaration.Importer != null ? newdeclaration.GetEffectiveMergeCustomsInvoiceLinesBy(declaration.Importer) : ZString.Empty;
				var defaultJeMergeBy = mergeBy.IsEmpty ? new ZString(OrgConstants.MergeInvoiceLines.Tariff) : mergeBy;
				checkMergeByRequired = newdeclaration.CA_MergeBy != defaultJeMergeBy;

				if (checkMergeByRequired && CheckSecurityRightForCheckpointRequired(Env.Security.MergeByDefault))
				{
					AddMessageLog(Env.Security.MergeByDefault, string.Format(CultureInfo.CurrentCulture, Constants.MergeBy, defaultJeMergeBy, newdeclaration.CA_MergeBy));
				}
			}
		}

		protected override ZBool CheckSecurityRightForCheckpointRequired(SecurityCheckpoint checkpoint)
		{
			return checkpoint.IsAllowed || AnyStaffOrGroupHasSecurityRight(checkpoint.Code);
		}

		public ZBool AnyStaffHasAccountModifyRight()
		{
			return Env.Security.CAAccountingDates.IsAllowed || AnyStaffOrGroupHasSecurityRight(Env.Security.CAAccountingDates.Code);
		}

		public bool AccountDateHasChanged(JobDeclaration declaration)
		{
			var valueHasBeenChanged = false;
			var addInfo = declaration.GetAddInfo();
			if (addInfo.HasChangesSinceLastSaving(ZArchitecture.Schema.CAAddInfoSchema.CA_K84AccountingDate))
			{
				valueHasBeenChanged = true;
			}
			return valueHasBeenChanged;
		}

		void CheckAccountDates(BaseJobDeclaration declaration)
		{
			var newdeclaration = (JobDeclaration)declaration;

			var valueHasBeenChanged = AccountDateHasChanged(newdeclaration);
			if (valueHasBeenChanged && AnyStaffHasAccountModifyRight())
			{
				AddMessageLog(Env.Security.CAAccountingDates, string.Format(CultureInfo.CurrentCulture, Constants.ChangeAccountDate, (ZDateTime)newdeclaration.GetAddInfo().GetOriginalValue(ZArchitecture.Schema.CAAddInfoSchema.CA_K84AccountingDate), newdeclaration.CA_K84AccountingDate));
			}
		}
	}
}
