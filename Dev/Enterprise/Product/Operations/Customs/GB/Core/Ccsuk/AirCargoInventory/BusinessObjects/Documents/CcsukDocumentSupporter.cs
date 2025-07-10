namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	using System.Collections.Generic;
	using CargoWise.Definitions;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.DocumentEngineCore.DocumentSupport;
	using Enterprise.DocumentEngineCore.DocWrappers;
	using Enterprise.Environment;
	using Enterprise.MasterFiles.Integration;
	using DataContext = Core.Constants.DataContext;

	public class CcsukDocumentSupporter : DocumentSupporter
	{
		public CcsukDocumentSupporter(ICcsukCusAwb awb)
			: base((BusinessObject)awb)
		{
			this.awb = awb;
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			return "Customisations to documents in the CCS-UK module are restricted and customised documents generally should not be used; please contact support for advice.";
		}

		public override string GetFilterValue(DocumentFilters filterName)
		{
			switch (filterName)
			{
				case DocumentFilters.CTY:
					return Core.Constants.CountryCodes.UnitedKingdom;

				default:
					return base.GetFilterValue(filterName);
			}
		}

		public override DocumentSupporterDataState GetDataStateBeforeRun(IStmMenuItem commandBeingRun)
		{
			lastReasonForNotRunning = ZString.Empty;
			if (IsRRA(commandBeingRun))
			{
				if (awb.HasSplits)
				{
					lastReasonForNotRunning = "This AWB is split, open the split you wish to deliver and re-run the document command.";
				}
				else
				{
					var mawbOrBasic = awb as CusMAWB;
					if (mawbOrBasic != null && !mawbOrBasic.IsBasic)
					{
						lastReasonForNotRunning = "You cannot print a release note for a consol.";
					}
				}
			}
			return lastReasonForNotRunning.IsEmpty ? base.GetDataStateBeforeRun(commandBeingRun) : new DocumentSupporterDataState(false, lastReasonForNotRunning);
		}
		ZString lastReasonForNotRunning;

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			lastReasonForNotRunning = ZString.Empty;
			if (dataContext == DataContext.GbCcsuk)
			{
				if (IsTFM(commandBeingRun))
				{
					if (awb.IsThroughAwb)
					{
						return GetCcsukWrapperForOnDemandHumandRendering("CcsukWrapper");
					}
				}
				else if (IsRRA(commandBeingRun))
				{
					if (!awb.HasSplits)
					{
						var mawbOrBasic = awb as CusMAWB;
						if (mawbOrBasic != null && !mawbOrBasic.IsBasic)
						{
							//cannot print a release note for a consol
						}
						else
						{
							return GetCcsukWrapperForOnDemandHumandRendering("CcsukWrapper");
						}
					}
				}
				else if (IsConsignmentDetailsReport(commandBeingRun))
				{
					return GetCcsukWrapperForOnDemandHumandRendering("CcsukWrapperForConsignmentReport");
				}
			}
			return null;
		}

		DocumentWrapper[] GetCcsukWrapperForOnDemandHumandRendering(string className)
		{
			var result = new List<DocumentWrapper>();
			var deliveryWrapper = DocumentWrapperFactory.CreateWrapperWithoutException("Enterprise.Customs.GB.DocumentWrappers.Ccsuk." + className + ", Enterprise.Customs.GB.DocumentWrappers", (BusinessObject)awb);
			if (deliveryWrapper != null)
			{
				result.Add(deliveryWrapper);
			}
			return result.ToArray();
		}

		bool IsRRA(IStmMenuItem menu)
		{
			return menu.SU_MenuName.Contains("Release Removal Authority");
		}

		bool IsTFM(IStmMenuItem menu)
		{
			return menu.SU_MenuName.Contains("Transfer Freight Manifest");
		}

		bool IsConsignmentDetailsReport(IStmMenuItem menu)
		{
			return menu.SU_MenuName.Contains(ConsignmentDetailsReportName);
		}

		public const string ConsignmentDetailsReportName = "Consignment Details Report";

		protected override DataContext[] GetSupportedDataContexts()
		{
			return new List<DataContext>() { DataContext.GbCcsuk }.ToArray();
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CusMAWB; }
		}

		public override ZArchitecture.Modules.ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		readonly ICcsukCusAwb awb;
	}
}
