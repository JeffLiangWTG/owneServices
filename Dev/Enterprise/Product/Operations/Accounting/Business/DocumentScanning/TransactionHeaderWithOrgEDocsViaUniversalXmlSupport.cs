using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.Business
{
	/// <summary>
	/// This call can be used for AssemblyData classes for Transaction Headers NOT matching the following consition
	/// [AH_Ledger]<>'AP' AND [AH_Ledger]<>'UA' AND [AH_Ledger]<>'IN' AND [AH_Ledger]<>'PA' of the [NR_UX__AH_GC_AH_Ledger_AH_TransactionType_AH_TransactionNum_AH_TransactionCount] unique index
	/// They will be applicable to the [NR_UX__AH_GC_AH_Ledger_AH_OH_AH_TransactionType_AH_TransactionNum_AH_TransactionCount] unique index and their 'code' should include Org Code together with Transaction Number 
	/// </summary>
	internal class TransactionHeaderWithOrgEDocsViaUniversalXmlSupport : IEDocsViaUniversalXmlSupport
	{
		public TransactionHeaderWithOrgEDocsViaUniversalXmlSupport(AssemblyData parent)
		{
			Parent = Argument.NotNull(parent, nameof(parent));
		}

		AssemblyData Parent { get; }

		public ZString ExpectedCodeFormat => new ZString("OrgCode|TransactionNumber");

		public ZString ExampleCodeFormat => new ZString("ABIGAS|100001");

		public BusinessObject LoadBusinessObjectFromCode(BusinessObjectFactory factory, ZString code)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNullOrWhitespace(code, nameof(code));

			var codeParts = code.Split('|');
			if (codeParts.Length == 2)
			{
				var org = factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, codeParts[0]);
				if (org != null)
				{
					var collection = Parent.GetBusinessObjectCollection(factory);
					var filter = collection.CompleteFilter;
					filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, Env.CurrentCompany.PK); // This method is called after switching Environment to the right Company
					filter.AddToFilter(AccTransactionHeaderSchema.AH_OH, org.PK);
					filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, codeParts[1]);

					return factory.LoadTop1<TransactionHeader>(filter);
				}
			}
			return null;
		}
	}
}
