using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Services;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;

namespace Enterprise.Customs.CA.Business
{
	public class AIRSIIDValidationQueriedLine : AIRSValidationQueriedLine, IAIRSIIDValidationQueriedLine
	{
		public AIRSIIDValidationQueriedLine(JobComInvoiceLine invoiceLine) : base(invoiceLine)
		{
			var cfia = invoiceLine.CFIAPGAHeader
				?? throw new InvalidOperationException("CFIAPGAHeader should not be null for IID.");
			AirsCode = cfia.CA_AIRSExtensionCode.PadLeft(6, '0');
			var consigneeAddress = invoiceLine.ConsigneeAddress;
			DeliveryPartyProvince = consigneeAddress != null ? consigneeAddress.StateCode : ZString.Empty;
			OriginCountry = invoiceLine.CA_RN_NKSource;
			OriginState = invoiceLine.CA_StateOfSource;
			EndUse = cfia.CA_AIRSEndUse;
			Miscellaneous = cfia.CA_AIRSMiscellaneous;

			foreach (AIRSRegistrationNumber regNum in cfia.AIRSRegistrationNumbers)
			{
				var code = regNum.CY_Code;
				if (!code.IsEmpty)
				{
					RegistrationNumbers.AddNew(code, AIRSValidationQueriedLineRegistrationTypes.Normal);
				}
			}

			foreach (LPCOView lpco in cfia.LPCOViews)
			{
				var code = lpco.CLP_Type;
				if (!code.IsEmpty && RegistrationNumbers.OfType<AIRSValidationQueriedLineRegistration>().FirstOrDefault(x => x.RegistrationId == code) == null)
				{
					RegistrationNumbers.AddNew(code, GetAIRSValidationTypeByRegistrationType(cfia.Factory, code, invoiceLine.CA_RN_NKSource));
				}
			}
		}

		AIRSValidationQueriedLineRegistrationTypes GetAIRSValidationTypeByRegistrationType(BusinessObjectFactory factory, ZString registrationType, ZString countryCode)
		{
			var result = AIRSValidationQueriedLineRegistrationTypes.DematerializedLpco;
			var caCFIARegTypes = RegistrationNumberHelper.LoadCFIALPCOType(factory, registrationType);
			if (caCFIARegTypes?.Attributes.HasAttribute(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CFIAMaterialized, YesNoList.Codes.Yes) ?? false)
			{
				result = AIRSValidationQueriedLineRegistrationTypes.MaterializedLpco;
				if (caCFIARegTypes.GetAttribute(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CFIADematerializedCountry).Contains(countryCode))
				{
					result = AIRSValidationQueriedLineRegistrationTypes.DematerializedLpco;
				}
			}
			return result;
		}

		public string DeliveryPartyProvince { get; }
		string IAIRSIIDValidationQueriedLine.DeliveryPartyProvince
		{
			get { return DeliveryPartyProvince; }
		}

		public override bool Equals(object obj)
		{
			var compareObj = obj as AIRSIIDValidationQueriedLine;
			return compareObj == null ? base.Equals(obj)
				: base.Equals(obj) &&
				this.DeliveryPartyProvince == compareObj.DeliveryPartyProvince;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode() ^ DeliveryPartyProvince.GetHashCode();
		}
	}
}
