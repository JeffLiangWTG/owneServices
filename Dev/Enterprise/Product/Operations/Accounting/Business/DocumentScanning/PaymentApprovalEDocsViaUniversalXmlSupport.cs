using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.Business
{
	/// <summary>
	/// This call can be used for AssemblyData classes inherited from PaymentApproval
	/// </summary>
	internal class PaymentApprovalEDocsViaUniversalXmlSupport<T> : IEDocsViaUniversalXmlSupport where T : PaymentApprovalBase
	{
		public class Constants
		{
			public const string ExpectedCodeFormat = "PaymentApprovalReference";
			public const string ExampleCodeFormat = "100001";
		}

		public PaymentApprovalEDocsViaUniversalXmlSupport(string ledger)
		{
			Ledger = Argument.NotNullOrWhitespace(ledger, nameof(ledger));
		}

		string Ledger { get; }

		public ZString ExpectedCodeFormat => Constants.ExpectedCodeFormat;

		public ZString ExampleCodeFormat => Constants.ExampleCodeFormat;

		public BusinessObject LoadBusinessObjectFromCode(BusinessObjectFactory factory, ZString code)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNullOrWhitespace(code, nameof(code));

			var query = new ZQuery(AccPaymentApprovalSchema.AV_GC, Env.CurrentCompany.PK)
				.AddToFilter(AccPaymentApprovalSchema.AV_Ledger, Ledger)
				.AddToFilter(AccPaymentApprovalSchema.AV_PaymentApprovalReference, code);

			return factory.LoadTop1<T>(query);
		}
	}
}
