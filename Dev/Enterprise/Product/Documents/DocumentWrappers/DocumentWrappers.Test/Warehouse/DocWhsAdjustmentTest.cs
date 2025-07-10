using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsAdjustment))]
	sealed class DocWhsAdjustmentTest : DocWhsDocketTest<WhsAdjustment, DocWhsAdjustment>
	{
		protected override DocWhsAdjustment CreateWhsDocketWrapper(WhsDocketLabelControl docketLabel)
		{
			return DocWhsAdjustment.New(docketLabel, Factory);
		}

		protected override DocWhsAdjustment CreateWhsDocketWrapper(WhsAdjustment docket)
		{
			return DocWhsAdjustment.New(docket, Factory);
		}
	}
}
