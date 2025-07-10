using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class CusEntryHeaderDocumentSupporter : EU.Business.Declaration.CusEntryHeaderDocumentSupporter
	{
		public CusEntryHeaderDocumentSupporter(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		new CusEntryHeader EntryHeader => (CusEntryHeader)base.EntryHeader;

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			switch (dataContext)
			{
				case DataContext.IEEAD:
				case DataContext.SADH:
				case DataContext.IEImportAccompanyingDocument:
				case DataContext.IEIADClearanceSlip:
					return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapperWithoutException(supportedWrappers[dataContext], EntryHeader, "Enterprise.Customs.IE.DocumentWrappers") };
			}

			return base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
		}

		readonly ImmutableDictionary<DataContext, ZString> supportedWrappers = new Dictionary<DataContext, ZString>()
		{
			{ DataContext.IEEAD, "Enterprise.Customs.IE.DocumentWrappers.IEDocEAD" },
			{ DataContext.SADH, "Enterprise.Customs.IE.DocumentWrappers.IEDocEAD" },
			{ DataContext.IEImportAccompanyingDocument, "Enterprise.Customs.IE.DocumentWrappers.ImportAccompanyingDocumentWrapper" },
			{ DataContext.IEIADClearanceSlip, "Enterprise.Customs.IE.DocumentWrappers.IADClearanceSlip" },
		}.ToImmutableDictionary();

		protected override DataContext[] GetSupportedDataContexts()
		{
			var result = new List<DataContext>(base.GetSupportedDataContexts())
			{
				DataContext.IEEAD,
				DataContext.SADH,
				DataContext.IEImportAccompanyingDocument,
				DataContext.IEIADClearanceSlip,
			};
			return result.ToArray();
		}

		public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun) => false;

		public override string GetFilterValue(DocumentFilters filterName) => filterName == DocumentFilters.CTYEGSADH ? CountryCodes.Ireland : base.GetFilterValue(filterName);
	}
}
