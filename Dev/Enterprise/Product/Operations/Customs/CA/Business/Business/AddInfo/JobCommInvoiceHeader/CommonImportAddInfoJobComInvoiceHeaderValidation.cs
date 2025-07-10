using CargoWise.EntityFramework;
using Enterprise.Core;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	class CommonImportAddInfoJobComInvoiceHeaderValidation : AddInfoJobComInvoiceHeaderValidation
	{
		public CommonImportAddInfoJobComInvoiceHeaderValidation(AddInfoJobComInvoiceHeader parent)
			: base(parent)
		{
		}

		protected JobComInvoiceHeader InvoiceHeader
		{
			get { return Parent.Parent; }
		}

		#region CheckCA_USStateOfExport

		protected override void CheckCA_USStateOfExport()
		{
			base.CheckCA_USStateOfExport();
			if (InvoiceHeader.CA_RN_NKExport == Constants.CountryCodes.UnitedStates)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_USStateOfExportInfo, Lookups.StatesOfExport);
			}
		}

		protected override void CheckCA_RL_NKLastPort()
		{
			base.CheckCA_RL_NKLastPort();

			if (Parent.CA_RL_NKLastPort.Substring(0, 2) == Core.Constants.CountryCodes.Canada)
			{
				Parent.CA_RL_NKLastPortInfo.AddMessageError(PlaceOfDirectShipmentCannotBeCanada);
			}
		}
		#endregion

		#region CheckCA_TreatmentCode

		protected override void CheckCA_TreatmentCode()
		{
			base.CheckCA_TreatmentCode();
			if (!Parent.CA_TreatmentCode.IsEmpty && IsTreatmentCodeRequired)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CA_TreatmentCodeInfo, Lookups.TreatmentCodes);
			}
		}

		protected virtual bool IsTreatmentCodeRequired { get { return true; } }

		#endregion

		#region CheckCA_TimeLimit (validate if entered)

		protected override void CheckCA_TimeLimit()
		{
			base.CheckCA_TimeLimit();
			MandatoryValidation.MessageErrorIfIsNegative(Parent.CA_TimeLimitInfo);
			if (InvoiceHeader.CA_TimeLimit > 99)
			{
				Parent.CA_TimeLimitInfo.AddMessageError(Res.GetString("fe7c9e8f-cc3f-4ec6-a3b1-0af911db9d16", "Time Limit value should not be greater than 99, change time unit to make period greater."));
			}
			if (!InvoiceHeader.CA_TimeLimitCode.IsEmpty && InvoiceHeader.CA_TimeLimit.IsEmpty)
			{
				Parent.CA_TimeLimitInfo.AddMessageError(Res.GetString("b7d6112d-4f47-4c15-b208-6840d3bdda3f", "You must enter a time limit when you have entered a time limit code"));
			}
		}

		#endregion

		#region CheckCA_TimeLimitCode (validate if entered)

		protected override void CheckCA_TimeLimitCode()
		{
			base.CheckCA_TimeLimitCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_TimeLimitCodeInfo, Lookups.TimeLimitUnits);
			MandatoryValidation.MessageErrorIfUnitNotEntered(Parent.CA_TimeLimitCodeInfo, Parent.CA_TimeLimitInfo);
			ValidateCA_TimeLimit();
		}

		#endregion
	}
}
