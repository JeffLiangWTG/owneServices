using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class DeferredPaymentWrapper : IDeferredPayment
	{
		DeferredPaymentWrapper(JobDeclaration jobDeclaration)
		{
			this.jobDeclaration = jobDeclaration;
		}

		readonly JobDeclaration jobDeclaration;

		public static DeferredPaymentWrapper New(JobDeclaration jobDeclaration) => jobDeclaration == null ? null : new DeferredPaymentWrapper(jobDeclaration);

		public string CcQualifier => ZString.Empty;

		public string DeferredPayment => (jobDeclaration.JE_PaymentMethod == MethodOfPaymentList.Codes.R || jobDeclaration.JE_PaymentMethod == MethodOfPaymentList.Codes.M) ? jobDeclaration.JE_DefermentAccountNumber.ToString() : string.Empty;
	}
}
