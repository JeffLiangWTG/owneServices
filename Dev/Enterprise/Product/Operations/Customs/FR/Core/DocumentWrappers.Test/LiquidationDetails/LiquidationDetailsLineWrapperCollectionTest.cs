using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.DocumentWrappers.Testing;
using NUnit.Framework;
using FRCusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.FR.Business.Declaration.CusEntryLine>;

namespace Enterprise.Customs.FR.DocumentWrappers.LiquidationDetails.Testing;

[TestedType(typeof(LiquidationDetailsLineWrapperCollection))]
sealed class LiquidationDetailsLineWrapperCollectionTest : DocBaseWrapperCollectionTest<LiquidationDetailsLineWrapperCollection>
{
	protected override LiquidationDetailsLineWrapperCollection GetNewDocumentWrapperCollection()
	{
		return new LiquidationDetailsLineWrapperCollection(EntryLineCollection, Factory);
	}

	protected override object GetNewObjectToWrap()
	{
		return null;
	}

	FRCusEntryLineCollection EntryLineCollection
	{
		get
		{
			if (entryHeaderCollection == null)
			{
				var declaration = Factory.New<JobDeclaration>();
				var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				invoiceHeader.JobComInvoiceLines.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.MergedLines.AddNew();

				entryHeaderCollection = entryHeader.MergedLines;
			}
			return entryHeaderCollection;
		}
	}
	FRCusEntryLineCollection entryHeaderCollection;
}
