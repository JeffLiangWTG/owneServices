using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.Business
{
	/// <summary>
	/// This call can be used for AssemblyData classes for Transaction Headers matching the following consition
	/// [AH_Ledger]<>'AP' AND [AH_Ledger]<>'UA' AND [AH_Ledger]<>'IN' AND [AH_Ledger]<>'PA'
	/// to use the [NR_UX__AH_GC_AH_Ledger_AH_TransactionType_AH_TransactionNum_AH_TransactionCount] unique index
	/// </summary>
	internal class TransactionHeaderEDocsViaUniversalXmlSupport : IEDocsViaUniversalXmlSupport
	{
		public TransactionHeaderEDocsViaUniversalXmlSupport(AssemblyData parent)
		{
			Parent = Argument.NotNull(parent, nameof(parent));
		}

		AssemblyData Parent { get; }

		public ZString ExpectedCodeFormat => new ZString("TransactionNumber");

		public ZString ExampleCodeFormat => new ZString("100001");

		public BusinessObject LoadBusinessObjectFromCode(BusinessObjectFactory factory, ZString code)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNullOrWhitespace(code, nameof(code));

			var collection = Parent.GetBusinessObjectCollection(factory);
			var filter = collection.CompleteFilter;
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, Env.CurrentCompany.PK); // This method is called after switching Environment to the right Company
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, code);

			return factory.LoadTop1<TransactionHeader>(filter);
		}
	}
}
