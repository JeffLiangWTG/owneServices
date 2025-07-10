using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class EMCSJobDeclarationValidation : EU.EMCS.Business.EMCSJobDeclarationValidation
	{
		public EMCSJobDeclarationValidation(EMCSJobDeclaration parent) : base(parent)
		{
		}

		protected new EMCSJobDeclaration Parent => (EMCSJobDeclaration)base.Parent;

		protected override ZBool ShouldValidateDestinationTypeMustBe1WhenGuarantorIs0 => ZBool.False;

		protected override void CheckJE_MessageSubType()
		{
			base.CheckJE_MessageSubType();
			var parent = Parent;
			var destinationType = parent.JE_MessageSubType;
			if (destinationType == EU.EMCS.Business.EMCSDestinationTypeList.Codes.DestinationRegisteredConsignee
				|| destinationType == EU.EMCS.Business.EMCSDestinationTypeList.Codes.DestinationTemporaryRegisteredConsignee
				|| destinationType == EU.EMCS.Business.EMCSDestinationTypeList.Codes.DestinationDirectDelivery)
			{
				var traderExciseNumber = parent.ImporterDocumentaryAddress.Organisation.GetCustomsRegNoIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber);
				if (traderExciseNumber.StartsWith(Core.Constants.CountryCodes.Germany, StringComparison.OrdinalIgnoreCase))
				{
					parent.JE_MessageSubTypeInfo.AddMessageError(Res.GetString("FEA4FAF2-A50A-4556-B30A-98F740BB15BD"
						, "This Destination Type is not valid if consignee has a German Trader Excise Number"));
				}
			}
		}

		protected override void CheckJE_DateAtOrigin()
		{
			base.CheckJE_DateAtOrigin();
			if (Parent.IsConsolidatedDocument())
			{
				var currentDateTime = ZDateTime.Now;
				var lastDayOfLastMonth = new ZDate(currentDateTime.Year, currentDateTime.Month, 1).AddDays(-1);
				if (Parent.JE_DateAtOrigin.Date != lastDayOfLastMonth)
				{
					Parent.JE_DateAtOriginInfo.AddMessageError(Res.GetString("76B6F4F3-2CD8-473B-B7B1-3077286CF1D9", "The Dispatch Time must be the last day of the last month."));
				}
			}
		}

		protected override void CheckJE_TransportMode()
		{
			base.CheckJE_TransportMode();
			if (Parent.IsConsolidatedDocument() && Parent.JE_TransportMode.IsEmpty)
			{
				Parent.JE_TransportModeInfo.AddWarning(Res.GetString("3D2E02CF-E365-4282-80DB-A0439A63BF3C", "If different Transport Modes have been used choose code 'OTH'."));
			}
		}
	}
}
