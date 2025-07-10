using System.Collections.Generic;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class CusEntryHeaderDocumentSupporter : EU.Business.Declaration.CusEntryHeaderDocumentSupporter
	{
		public CusEntryHeaderDocumentSupporter(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		#region DataContext Consts

		public const string GbCDSEntryHeader = "GbCDSEntryHeader";

		#endregion

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			var result = new List<DocumentWrapper>();
			switch (dataContext)
			{
				case DataContext.GbCDSEntryHeader:
					var cdsEntryHeaderWrapper = DocumentWrapperFactory.CreateWrapperWithoutException("Enterprise.Customs.GB.DocumentWrappers.DocCDSEntryHeaderWrapper, Enterprise.Customs.GB.DocumentWrappers", EntryHeader);
					if (cdsEntryHeaderWrapper != null)
					{
						result.Add(cdsEntryHeaderWrapper);
					}
					break;
				case DataContext.SADH:
					var docSADHWrapper = DocumentWrapperFactory.CreateWrapperWithoutException("Enterprise.Customs.GB.DocumentWrappers.DocSADH, Enterprise.Customs.GB.DocumentWrappers", EntryHeader);
					if (docSADHWrapper != null)
					{
						result.Add(docSADHWrapper);
					}
					break;
			}
			return result.Count > 0 ? result.ToArray() : base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			var result = new List<DataContext>(base.GetSupportedDataContexts())
			{
				DataContext.ChiefEAD,
				DataContext.GbCDSEntryHeader
			};
			return result.ToArray();
		}
	}
}
