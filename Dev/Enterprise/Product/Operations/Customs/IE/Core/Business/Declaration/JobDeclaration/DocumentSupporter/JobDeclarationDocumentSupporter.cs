using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class JobDeclarationDocumentSupporter : EU.Business.Declaration.JobDeclarationDocumentSupporter
	{
		public JobDeclarationDocumentSupporter(JobDeclaration declaration) : base(declaration)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Quantity constants")]
		static class DocumentMenuItemText
		{
			public const string IADClearanceSlip = "IAD Clearance Slip";
			public const string IAD = "Import Accompanying Document";
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			switch (dataContext)
			{
				case DataContext.IEEAD:
				case DataContext.SADH:
				case DataContext.IEImportAccompanyingDocument:
				case DataContext.IEIADClearanceSlip:
					var result = new List<DocumentWrapper>();
					var wrapperStrongName = supportedWrappers[dataContext];
					foreach (CusEntryHeader entryHeader in Declaration.CustomsEntryHeaders)
					{
						result.Add(DocumentWrapperFactory.CreateWrapperWithoutException(wrapperStrongName, entryHeader, "Enterprise.Customs.IE.DocumentWrappers"));
					}
					return result.ToArray();
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

		public override MultilingualString GetCustomWatermarkText(IDocumentCommand documentCommand, IBODocDataProvider docDataProvider)
		{
			switch (documentCommand.SU_MenuName)
			{
				case DocumentMenuItemText.IAD:
				case DocumentMenuItemText.IADClearanceSlip:
					if (docDataProvider.ParentBusinessObject is CusEntryHeader header)
					{
						if (ShouldDisplayWatermark_UnderAmendment(header))
						{
							var wtrmk = (NoResString)watermarkTextUnderAmendment;
							return wtrmk;
						}
						else if (documentCommand.SU_MenuName == DocumentMenuItemText.IAD && !header.HasAnyConfirmedFeesOnAnyMergedLine)
						{
							return (NoResString)watermarkcalculatedDutiesOrEstimated;
						}
					}
					break;
			}

			return null;
		}
		readonly ZString watermarkTextUnderAmendment = Res.GetString("52A625D6-41F5-4B80-9BAC-7F96BC689E75", "UNCONFIRMED\r\nBY CUSTOMS\r\nUnder Amendment");
		readonly ZString watermarkcalculatedDutiesOrEstimated = Res.GetString("DDE22D9F-8A42-4846-A75E-8617E1D9F8DF", "calculated duties\r\nestimated");

		static readonly HashSet<ZString> lockUnlockCodes = new HashSet<ZString> {
			AutoEvents.LockForEdit.Code,
			AutoEvents.UnlockForEdit.Code };

		ZBool ShouldDisplayWatermark_UnderAmendment(CusEntryHeader header)
		{
			if (header.CH_EntryStatus == AISEntryStatusList.Codes.Released && header.CH_Status == LogicalStatusList.Codes.Accepted)
			{
				return Declaration.Logs?
					.GetAllLogs()
					.Where(l => lockUnlockCodes.Contains(l.SL_SE_NKEvent))
					.OrderByDescending(l => l.SL_EventTimeUtc)
					.FirstOrDefault()?
					.SL_SE_NKEvent.Equals(AutoEvents.UnlockForEdit.Code) ?? false;
			}
			return false;
		}
	}
}
