using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.CA.Services;

namespace Enterprise.Customs.CA.Business
{
	public abstract class AIRSValidationQueriedLine : IAIRSValidationQueriedLine
	{
		protected AIRSValidationQueriedLine(JobComInvoiceLine invoiceLine)
		{
			CommodityGroup = invoiceLine.Declaration.JE_DeclarationReference;
			HSNumber = invoiceLine.JI_Tariff.SubstringSafe(0, 6);
		}
		#region Properties

		public ZString CommodityGroup { get; set; }
		public ZInt Commodity { get; set; }
		public ZString HSNumber { get; set; }
		public ZString AirsCode { get; set; }
		public ZString OriginCountry { get; set; }
		public ZString OriginState { get; set; }
		public ZString EndUse { get; set; }
		public ZString Miscellaneous { get; set; }
		public ZString ValidationResponse { get; set; }
		public ZString ValidationFaultMessage { get; set; }
		public ZBool ValidationCompleted { get; set; }
		public ValidationFaultMessageType ValidationFaultMessageType { get; set; }

		#endregion

		#region Collections

		public AIRSValidationQueriedLineRegistrationCollection RegistrationNumbers
		{
			get
			{
				if (fRegistrationNumbers == null)
				{
					fRegistrationNumbers = new AIRSValidationQueriedLineRegistrationCollection();
				}
				return fRegistrationNumbers;
			}
		}
		AIRSValidationQueriedLineRegistrationCollection fRegistrationNumbers;

		public List<ZGuid> InvoiceLinePKs
		{
			get
			{
				if (fInvoiceLinePKs == null)
				{
					fInvoiceLinePKs = new List<ZGuid>();
				}
				return fInvoiceLinePKs;
			}
		}
		List<ZGuid> fInvoiceLinePKs;

		#endregion

		#region IAIRSValidationQueriedLine Members

		string IAIRSValidationQueriedLine.AirsCode
		{
			get { return AirsCode; }
		}

		string IAIRSValidationQueriedLine.Commodity
		{
			get { return Commodity.ToString(); }
		}

		string IAIRSValidationQueriedLine.CommodityGroup
		{
			get { return CommodityGroup; }
		}

		string IAIRSValidationQueriedLine.EndUse
		{
			get { return EndUse; }
		}

		string IAIRSValidationQueriedLine.HSNumber
		{
			get { return HSNumber; }
		}

		string IAIRSValidationQueriedLine.Miscellaneous
		{
			get { return Miscellaneous; }
		}

		string IAIRSValidationQueriedLine.OriginCountry
		{
			get { return OriginCountry; }
		}

		string IAIRSValidationQueriedLine.OriginState
		{
			get { return OriginState; }
		}

		string IAIRSValidationQueriedLine.ValidationResponse
		{
			get { return ValidationResponse; }
			set { ValidationResponse = value; }
		}

		ZString IAIRSValidationQueriedLine.ValidationFaultMessage
		{
			get { return ValidationFaultMessage; }
			set { ValidationFaultMessage = value; }
		}

		bool IAIRSValidationQueriedLine.ValidationCompleted
		{
			get { return ValidationCompleted; }
			set { ValidationCompleted = value; }
		}
		IEnumerable<IAIRSValidationQueriedLineRegistration> IAIRSValidationQueriedLine.Registrations
		{
			get { return RegistrationNumbers; }
		}

		#endregion

		public override bool Equals(object obj)
		{
			var compareObj = obj as AIRSValidationQueriedLine;
			return compareObj == null ? base.Equals(obj)
				: this.AirsCode == compareObj.AirsCode &&
				this.CommodityGroup == compareObj.CommodityGroup &&
				this.EndUse == compareObj.EndUse &&
				this.HSNumber == compareObj.HSNumber &&
				this.Miscellaneous == compareObj.Miscellaneous &&
				this.OriginCountry == compareObj.OriginCountry &&
				this.OriginState == compareObj.OriginState &&
				this.RegistrationNumbers.Equals(compareObj.RegistrationNumbers);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode() ^
				AirsCode.GetHashCode() ^
				CommodityGroup.GetHashCode() ^
				EndUse.GetHashCode() ^
				HSNumber.GetHashCode() ^
				Miscellaneous.GetHashCode() ^
				OriginCountry.GetHashCode() ^
				OriginState.GetHashCode() ^
				RegistrationNumbers.GetHashCode();
		}
	}
}
