using System.Linq;
using Enterprise.Customs.CA.Services;

namespace Enterprise.Customs.CA.Business
{
	public class AIRSOGDValidationQueriedLine : AIRSValidationQueriedLine, IAIRSOGDValidationQueriedLine
	{
		public AIRSOGDValidationQueriedLine(JobComInvoiceLine invoiceLine) : base(invoiceLine)
		{
			RequirementId = invoiceLine.CA_RequirementID;
			RequirementVersion = invoiceLine.CA_RequirementVer;
			DestinationProvince = invoiceLine.CA_DestinationProvince;
			AirsCode = invoiceLine.CA_AirsCode.PadLeft(6, '0');
			OriginCountry = invoiceLine.CA_RN_NKCFIAOrigin;
			OriginState = invoiceLine.CA_CFIAUSStateOfOrigin;
			EndUse = invoiceLine.CA_EndUse;
			Miscellaneous = invoiceLine.CA_MiscID;
			foreach (CFIARegistrationNumber number in invoiceLine.CFIARegistrationNumbers)
			{
				var registrationNumber = number.CY_Code;
				if (!registrationNumber.IsEmpty && !RegistrationNumbers.Cast<AIRSValidationQueriedLineRegistration>().Any(r => r.RegistrationId == registrationNumber))
				{
					RegistrationNumbers.AddNew(registrationNumber, AIRSValidationQueriedLineRegistrationTypes.Normal);
				}
			}
		}

		public string RequirementId { get; }
		public string RequirementVersion { get; }
		public string DestinationProvince { get; }

		string IAIRSOGDValidationQueriedLine.DestinationProvince
		{
			get { return DestinationProvince; }
		}

		string IAIRSOGDValidationQueriedLine.RequirementId
		{
			get { return RequirementId; }
		}

		string IAIRSOGDValidationQueriedLine.RequirementVersion
		{
			get { return RequirementVersion; }
		}

		public override bool Equals(object obj)
		{
			var compareObj = obj as AIRSOGDValidationQueriedLine;
			return compareObj == null ? base.Equals(obj)
				: base.Equals(obj) &&
				this.RequirementId == compareObj.RequirementId &&
				this.RequirementVersion == compareObj.RequirementVersion &&
				this.DestinationProvince == compareObj.DestinationProvince;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode() ^
				RequirementId.GetHashCode() ^
				RequirementVersion.GetHashCode() ^
				DestinationProvince.GetHashCode();
		}
	}
}
