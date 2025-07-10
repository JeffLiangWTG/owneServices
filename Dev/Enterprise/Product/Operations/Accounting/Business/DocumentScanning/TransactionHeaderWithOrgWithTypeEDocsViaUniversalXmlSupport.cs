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
	internal class TransactionHeaderWithOrgWithTypeEDocsViaUniversalXmlSupport : IEDocsViaUniversalXmlSupport
	{
		public TransactionHeaderWithOrgWithTypeEDocsViaUniversalXmlSupport(AssemblyData parent)
		{
			Parent = Argument.NotNull(parent, nameof(parent));
		}

		AssemblyData Parent { get; }

		public ZString ExpectedCodeFormat => new ZString("OrgCode|TransactionType|TransactionNumber");

		public ZString ExampleCodeFormat => new ZString("ABIGAS|CPA|100001");

		public BusinessObject LoadBusinessObjectFromCode(BusinessObjectFactory factory, ZString code)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNullOrWhitespace(code, nameof(code));

			var codeParts = code.Split('|');
			BusinessObject businessObject = null;

			if (codeParts.Length == 3)
			{
				var org = factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, codeParts[0]);
				if (org != null)
				{
					var collection = Parent.GetBusinessObjectCollection(factory);
					var filter = collection.CompleteFilter;
					filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, Env.CurrentCompany.PK); // This method is called after switching Environment to the right Company
					filter.AddToFilter(AccTransactionHeaderSchema.AH_OH, org.PK);
					filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, codeParts[1]);
					filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, codeParts[2]);

					businessObject = factory.LoadTop1<TransactionHeader>(filter);
				}
			}

			return businessObject;
		}
	}
}
