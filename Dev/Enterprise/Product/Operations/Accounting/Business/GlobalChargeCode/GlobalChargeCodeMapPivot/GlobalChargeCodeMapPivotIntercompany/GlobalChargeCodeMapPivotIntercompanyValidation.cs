namespace Enterprise.Accounting.Business.GlobalChargeCode
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Schema;

	public class GlobalChargeCodeMapPivotIntercompanyValidation : AccGlobalChargeCodeMapPivotValidation
	{
		public GlobalChargeCodeMapPivotIntercompanyValidation(GlobalChargeCodeMapPivotIntercompany parent)
			: base(parent)
		{
		}

		protected override void CheckYP_OH_LocalClientOverride()
		{
			base.CheckYP_OH_LocalClientOverride();
			if (((GlobalChargeCodeMapPivotIntercompany)Parent).SupportLocalClientOverride)
			{
				MandatoryValidation.CheckEntered(Parent.YP_OH_LocalClientOverrideInfo);
				ZString errorMessage = GetErrorMessageIfARChargeCodeIsNotUnique();
				if (!errorMessage.IsEmpty)
				{
					Parent.YP_OH_LocalClientOverrideInfo.AddError(errorMessage);
				}
			}

			if (!Parent.YP_OH_LocalClientOverride.IsEmpty)
			{
				OrgHeader jobLocalClient = ((GlobalChargeCodeMapPivotIntercompany)Parent).JobLocalClient;
				if (jobLocalClient != null && !jobLocalClient.CompanyData.OB_IsDebtor)
				{
					Parent.YP_OH_LocalClientOverrideInfo.AddWarning(Res.GetString("D623F309-BB86-429F-92A1-0F3BF2623BD8", "In most cases, the Job Local Client should be flagged as a Receivables organization."));
				}
			}
		}

		protected override bool IsAPLedgerUnique()
		{
			bool isUnique = true;
			foreach (GlobalChargeCodeMapPivotIntercompany pivot in ((GlobalChargeCodeMapPivotIntercompany)Parent).ParentCollection)
			{
				if (pivot != Parent && pivot.YP_TYPE == ZArchitecture.Core.LedgerTypes.AccountsPayable && pivot.YP_YG == Parent.YP_YG && pivot.YP_OH_LocalClientOverride == Parent.YP_OH_LocalClientOverride)
				{
					isUnique = false;
					break;
				}
			}
			return isUnique;
		}

		protected override bool IsRecordUnique()
		{
			bool isUnique = true;
			foreach (GlobalChargeCodeMapPivotIntercompany pivot in ((GlobalChargeCodeMapPivotIntercompany)Parent).ParentCollection)
			{
				if (pivot != Parent && pivot.YP_AC == Parent.YP_AC && pivot.YP_TYPE == Parent.YP_TYPE && pivot.YP_YG == Parent.YP_YG && pivot.YP_OH_LocalClientOverride == Parent.YP_OH_LocalClientOverride)
				{
					isUnique = false;
					break;
				}
			}
			return isUnique;
		}

		protected override void CheckYP_TYPE()
		{
			base.CheckYP_TYPE();
			if (Parent.YP_TYPE == ZArchitecture.Core.LedgerTypes.AccountsReceivable && !Parent.YP_OH_LocalClientOverride.IsEmpty)
			{
				Parent.YP_TYPEInfo.AddError(Res.GetString("150443EB-CC44-404E-9653-0C4A83C41A18", "You can only select AP Ledger when Job Local Client is set."));
			}
		}

		protected override void CheckYP_AC()
		{
			base.CheckYP_AC();
			if (!Parent.YP_AC.IsEmpty && Parent.YP_TYPE == ZArchitecture.Core.LedgerTypes.AccountsReceivable)
			{
				ZString errorMessage = GetErrorMessageIfARChargeCodeIsNotUnique();
				if (!errorMessage.IsEmpty)
				{
					Parent.YP_ACInfo.AddError(errorMessage);
				}
			}
		}

		ZString GetErrorMessageIfARChargeCodeIsNotUnique()
		{
			ZString error = ZString.Empty;
			ZDBOnlySubQuery globalChargeCodeSubQuery = new ZDBOnlySubQuery(typeof(GlobalChargeCodeMap), AccGlobalChargeCodeMapSchema.PK);
			globalChargeCodeSubQuery.AddToFilter(AccGlobalChargeCodeMapSchema.YG_OH, null);
			globalChargeCodeSubQuery.AddToFilter(AccGlobalChargeCodeMapSchema.YG_IsActive, true);
			ZDBOnlyQuery pivotQuery = new ZDBOnlyQuery(typeof(GlobalChargeCodeMapPivot));
			pivotQuery.AddToFilter(AccGlobalChargeCodeMapPivotSchema.YP_TYPE, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
			pivotQuery.AddToFilter(AccGlobalChargeCodeMapPivotSchema.YP_AC, Parent.YP_AC);
			pivotQuery.AddToFilter(AccGlobalChargeCodeMapPivotSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			pivotQuery.AddSubQuery(AccGlobalChargeCodeMapPivotSchema.YP_YG, globalChargeCodeSubQuery, JoinCondition.And);

			if (!Parent.YP_OH_LocalClientOverride.IsEmpty)
			{
				pivotQuery.AddToFilter(AccGlobalChargeCodeMapPivotSchema.YP_OH_LocalClientOverride, Parent.YP_OH_LocalClientOverride);
			}
			else
			{
				pivotQuery.AddToFilter(AccGlobalChargeCodeMapPivotSchema.YP_OH_LocalClientOverride, null);
			}

			GlobalChargeCodeMapPivotIntercompany pivot = Parent.Factory.LoadTop1<GlobalChargeCodeMapPivotIntercompany>(pivotQuery);
			if (pivot != null)
			{
				if (!((GlobalChargeCodeMapPivotIntercompany)Parent).SupportLocalClientOverride)
				{
					error = Res.GetString("D0BC57CC-D2DB-44b1-B93B-5103A82F4FD2", "Current Charge Code with AR ledger is already mapped for {0} Global Charge Code.", pivot.GlobalChargeCodeMap.YG_Code);
				}
				else
				{
					error = Res.GetString("EF0B5F58-C107-4D9F-B779-CE072DD62F20", "Current Charge Code with AR ledger and Override local client is already mapped for {0} Global Charge Code.", pivot.GlobalChargeCodeMap.YG_Code);
				}
			}

			return error;
		}

		protected override string APLedgerNotUniqueErrorMessage()
		{
			if (((GlobalChargeCodeMapPivotIntercompany)Parent).SupportLocalClientOverride)
			{
				return Res.GetString("9FFA9C02-F51D-4DC6-9821-4A2320BBA6A4", "You can only create a single AP Charge Code per mapping for each override local client");
			}
			else
			{
				return base.APLedgerNotUniqueErrorMessage();
			}
		}

		protected override string ChargeCodeAndTypeAlreadyMappedErrorMessage()
		{
			if (((GlobalChargeCodeMapPivotIntercompany)Parent).SupportLocalClientOverride)
			{
				return Res.GetString("E4AEC4AD-3DFF-4211-960F-2B985ABA4F4C", "This Charge Code and Type is already mapped to this Global Code for the selected override local client");
			}
			else
			{
				return base.ChargeCodeAndTypeAlreadyMappedErrorMessage();
			}
		}
	}
}

