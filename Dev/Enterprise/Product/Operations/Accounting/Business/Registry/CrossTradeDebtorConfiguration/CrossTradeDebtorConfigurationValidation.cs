using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public class CrossTradeDebtorConfigurationValidation : JobConfigurationSelectorValidation
	{
		public CrossTradeDebtorConfigurationValidation(CrossTradeDebtorConfiguration parent)
					: base(parent)
		{
		}

		protected new CrossTradeDebtorConfiguration Parent
		{
			get { return (CrossTradeDebtorConfiguration)base.Parent; }
		}

		#region Job Type

		public override void ValidateJobType()
		{
			MandatoryValidation.CheckEntered(Parent.JobTypeInfo, (IMultilingualString)ResString.GetMultilingualString("FBCC20D3-C115-48AA-BC17-7802A87FBA73", "Job Type"));
			ListValidation.ErrorIfInvalidCode(Parent.JobTypeInfo, Parent.JobTypeList);

			if (!Parent.JobTypeInfo.HasErrors())
			{
				CheckDuplicatedConfiguration();
			}
		}

		#endregion

		#region TransportMode

		public override void ValidateMode()
		{
			MandatoryValidation.CheckEntered(Parent.ModeInfo, (IMultilingualString)ResString.GetMultilingualString("CD0FDC6C-3FA4-40F8-B825-AA8B77B98882", "Mode"));
			ListValidation.ErrorIfInvalidCode(Parent.ModeInfo, Parent.ModeList);

			if (!Parent.ModeInfo.HasErrors())
			{
				CheckDuplicatedConfiguration();
			}
		}

		#endregion

		#region ChargePaymentType

		public void ValidateChargePaymentType()
		{
			MandatoryValidation.CheckEntered(Parent.ChargePaymentTypeInfo, (IMultilingualString)ResString.GetMultilingualString("ADBA316B-6A69-4EA9-BF7F-07E1B112E115", "Charges Prepaid/Collect"));
			ListValidation.ErrorIfInvalidCode(Parent.ChargePaymentTypeInfo, Parent.ChargePaymentTypeList);

			if (!Parent.ChargePaymentTypeInfo.HasErrors())
			{
				CheckDuplicatedConfiguration();
			}	
		}

		#endregion

		#region Debtor

		public void ValidateDebtor()
		{
			MandatoryValidation.CheckEntered(Parent.DebtorInfo, (IMultilingualString)ResString.GetMultilingualString("BC6B95C7-F9FB-4CBB-8D28-989260BBD50B", "Default Debtor"));
			ListValidation.ErrorIfInvalidCode(Parent.DebtorInfo, Parent.DebtorOptionList);

			if (!Parent.DebtorInfo.HasErrors())
			{
				CheckDuplicatedConfiguration();
			}
		}

		#endregion

		protected override string DuplicateJobParametersError
		{
			get
			{
				return Res.GetString("119C3F4F-7FC4-414D-8CFD-F8E41CD104C6", "A record already exists with the same configuration.");
			}
		}

		string ChargePaymentTypeMessages
		{
			get
			{
				return Res.GetString("03E8A324-D977-4E82-B420-60B264919BE0", "PPD or CCX cannot be used in combination with Charges Prepaid/Collect - ALL for the same configuration.\r\nPlease either create a single configuration using Charges Prepaid/Collect - ALL, or a configuration for each PPD and CCX.");
			}
		}

		public void CheckDuplicatedConfiguration()
		{
			Parent.ClearRowNotifications();

			foreach (var configuration in Parent.CollectionForValidation)
			{
				if (configuration != Parent && configuration.JobType == Parent.JobType && configuration.Mode == Parent.Mode)
				{
					if (configuration.ChargePaymentType == Parent.ChargePaymentType)
					{
						Parent.AddRowError(DuplicateJobParametersError);
						break;
					}
					else if
					(
						(configuration.ChargePaymentType == PrepaidCollectFreightForwardingList.Codes.All && (Parent.ChargePaymentType == PrepaidCollectFreightForwardingList.Codes.PPD || Parent.ChargePaymentType == PrepaidCollectFreightForwardingList.Codes.CCX)) ||
						(Parent.ChargePaymentType == PrepaidCollectFreightForwardingList.Codes.All && (configuration.ChargePaymentType == PrepaidCollectFreightForwardingList.Codes.PPD || configuration.ChargePaymentType == PrepaidCollectFreightForwardingList.Codes.CCX))
					)
					{
						Parent.AddRowError(ChargePaymentTypeMessages);
						break;
					}
				}
			}
		}
	}
}
