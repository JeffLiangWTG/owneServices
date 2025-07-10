using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.CA;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	class JobDeclarationDocumentSupporter : BaseJobDeclarationDocumentSupporter
	{
		public JobDeclarationDocumentSupporter(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
		}

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			var result = base.GetSupportedBODataSources();
			result.Add(new DataContextValue(B3ImportDataContext));
			result.Add(new DataContextValue(CADImportDataContext));
			result.Add(new DataContextValue(B3XAdjustmentsDataContext));
			result.Add(new DataContextValue(B2AdjustmentsDataContext));
			result.Add(new DataContextValue(CADEXLeadSheet));
			result.Add(new DataContextValue(B2AdjustmentsForIM2DataContext));
			return result;
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return base.GetSupportedDataContexts()
				.Concat(new[] { Constants.DataContext.GenericFreightJobByReleaseStatus })
				.Concat(new[] { Constants.DataContext.GenericFreightJobRouting }).ToArray();
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			if (dataContextValue.FullDataContext == B3ImportDataContext)
			{
				// GetEntryHeaderFor method can possibly return null if no custom entry headers whose header.CH_MessageType == entryType are found.
				var entryHeader = ((JobDeclaration)BusinessObject).B3EntryHeader;
				// This would prevent null entryHeader from being passed to the B3ImportDocumentWrapper constructor.
				if (entryHeader == null || !entryHeader.IsB3C)
				{
					return Array.Empty<IBODocDataProvider>();
				}
				else
				{
					if (commandBeingRun != null && commandBeingRun.SU_MenuName == ForwardingShipmentDocumentSupporter.CACustomsDocList.B3AsLodged)
					{
						var lastMessage = B3Message.GetLastSentAcceptedB3Message(entryHeader);
						return lastMessage != null ? new[] { BODocDataProvider.Get(new B3ImportDocumentWrapper(lastMessage)) } : Array.Empty<IBODocDataProvider>();
					}
					else
					{
						return new[] { BODocDataProvider.Get(new B3ImportDocumentWrapper(entryHeader)) };
					}
				}
			}
			else if (dataContextValue.FullDataContext == CADImportDataContext)
			{
				var entryHeader = ((JobDeclaration)BusinessObject).B3EntryHeader;
				if (entryHeader == null || !entryHeader.IsCAD)
				{
					return Array.Empty<IBODocDataProvider>();
				}
				else
				{
					if (commandBeingRun != null && commandBeingRun.SU_MenuName == ForwardingShipmentDocumentSupporter.CACustomsDocList.CADAsLodged)
					{
						var lastMessage = CADMessage.GetLatestCADResponseMessage(entryHeader);
						return lastMessage != null ? new[] { BODocDataProvider.Get(new CADImportDocumentWrapper(lastMessage)) } : Array.Empty<IBODocDataProvider>();
					}
					else
					{
						return new[] { BODocDataProvider.Get(new CADImportDocumentWrapper(entryHeader)) };
					}
				}
			}
			else if (dataContextValue.FullDataContext == B3XAdjustmentsDataContext)
			{
				if (Declaration.IsB3X)
				{
					return new[] { BODocDataProvider.Get(new B3XAdjustmentsDocumentWrapper(Declaration)) };
				}
				else
				{
					return Array.Empty<IBODocDataProvider>();
				}
			}
			else if (dataContextValue.FullDataContext == B2AdjustmentsDataContext)
			{
				if (Declaration.IsB2Adjustments)
				{
					return new[] { BODocDataProvider.Get(new B2AdjustmentsDocumentWrapper(Declaration)) };
				}
				else
				{
					return Array.Empty<IBODocDataProvider>();
				}
			}
			else if (dataContextValue.FullDataContext == B2AdjustmentsForIM2DataContext)
			{
				if (Declaration.IsIM2)
				{
					return new[] { BODocDataProvider.Get(new IM2AdjustmentsDocumentWrapper(Declaration)) };
				}
				else
				{
					return Array.Empty<IBODocDataProvider>();
				}
			}
			else if (dataContextValue.FullDataContext == CADEXLeadSheet)
			{
				return new[] { BODocDataProvider.Get(new LeadSheetDocumentWrapper(Declaration)) };
			}
			return base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			switch (dataContextValue.FullDataContext)
			{
				case B3XAdjustmentsDataContext:
					return Res.GetString("00B56869-514E-41CE-A764-5D6BBF53E395", "Declaration must be a B3X Adjustments.");
				case B2AdjustmentsDataContext:
					return Res.GetString("43A75E58-6FC7-489F-9D97-9AB5C6FA7724", "Declaration must be a B2 Adjustments.");
				case B2AdjustmentsForIM2DataContext:
					return Res.GetString("7E31514A-3149-4C56-B756-F93965A065B3", "Declaration must be a IM2 Adjustments.");
				default:
					return base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override DocumentSupporterDataState GetDataStateBeforeRunCore(IStmMenuItem commandAboutToBeRun)
		{
			if (commandAboutToBeRun != null)
			{
				if (Declaration.ApportionmentDirty)
				{
					return new DocumentSupporterDataState(false, Res.GetString("5bd7411b-9c91-4d2e-a985-f260c61bc0e7", "Apportionment must be run. Please follow Brokerage->Perform Apportionment."));
				}

				switch (commandAboutToBeRun.SU_MenuName)
				{
					case ForwardingShipmentDocumentSupporter.CACustomsDocList.B3CurrentData:
					case ForwardingShipmentDocumentSupporter.CACustomsDocList.B3AsLodged:
						var documentSupporterDataStateForB3 = GetDataStateBeforeRunForB3AndCADDocument(commandAboutToBeRun, ForwardingShipmentDocumentSupporter.CACustomsDocList.B3CurrentData, ForwardingShipmentDocumentSupporter.CACustomsDocList.B3AsLodged, MessageTypeList.Codes.B3CUSDEC, B3Message.GetLastSentAcceptedB3Message);
						if (documentSupporterDataStateForB3 != null)
						{
							return documentSupporterDataStateForB3;
						}
						break;
					case ForwardingShipmentDocumentSupporter.CACustomsDocList.CADCurrentData:
					case ForwardingShipmentDocumentSupporter.CACustomsDocList.CADAsLodged:
						var documentSupporterDataStateForCAD = GetDataStateBeforeRunForB3AndCADDocument(commandAboutToBeRun, ForwardingShipmentDocumentSupporter.CACustomsDocList.CADCurrentData, ForwardingShipmentDocumentSupporter.CACustomsDocList.CADAsLodged, MessageTypeList.Codes.CommercialAccountingDeclaration, CADMessage.GetLatestCADResponseMessage);
						if (documentSupporterDataStateForCAD != null)
						{
							return documentSupporterDataStateForCAD;
						}
						break;
					case LVSIdentifierDetailsDocument:
						var dataState = GetDataStateForLinesToPrintDocument();
						if (dataState != null)
						{
							return dataState;
						}
						break;
					case ForwardingShipmentDocumentSupporter.CACustomsDocList.ReleaseStatusDocument:
						var count = Declaration.ReleaseStatusesToPrint.Count;
						if (count == 0)
						{
							return new DocumentSupporterDataState(false, Res.GetString("71e03720-f575-4c05-aae6-fa69a4631de5", "No release status update has been found to print {0}.", ForwardingShipmentDocumentSupporter.CACustomsDocList.ReleaseStatusDocument));
						}
						if (count > 1)
						{
							var args = new CancelEventArgs();
							Declaration.FireOnGetReleaseStatusesToPrint(args);
							return args.Cancel ? new DocumentSupporterDataState(false, Res.GetString("F5B3E93B-241C-430D-838C-EF19BD6E537C", "Printing of this document was canceled.")) : new DocumentSupporterDataState(true, string.Empty);
						}
						break;
					case A8AInBondRoutingDocument:
						dataState = GetDataStateForTransportToPrintDocument();
						if (dataState != null)
						{
							return dataState;
						}
						break;
					case B2AdjustmentDocument:
						if (Declaration.IsIM2)
						{
							var newInvoiceLines = Declaration.InvoiceLines.Cast<JobComInvoiceLine>();
							var previousInvoiceLines = Declaration.PreviousJob.InvoiceLines.Cast<JobComInvoiceLine>();

							var invoiceLinesNotInPreviousJob = from newInvoiceLine in newInvoiceLines
															   where !(from previousInvoiceLine in previousInvoiceLines select previousInvoiceLine.JI_LineNo.ToZInt()).Contains(newInvoiceLine.CA_PreviousLineNo)
															   select newInvoiceLine;
							if (invoiceLinesNotInPreviousJob.Any())
							{
								return new DocumentSupporterDataState(false, Res.GetString("BE802ECC-AF86-47D7-9C7D-D2354CBB6046", "At least one invoice line was deleted from previous job(Previous Declaration : {0}).", Declaration.PreviousJob.JE_DeclarationReference));
							}
						}
						break;
				}
			}
			return base.GetDataStateBeforeRunCore(commandAboutToBeRun);
		}

		DocumentSupporterDataState GetDataStateBeforeRunForB3AndCADDocument(IStmMenuItem commandAboutToBeRun, ZString currentData, ZString asLodged, ZString messageType, Func<CusEntryHeader, EDIMessage> getMessage)
		{
			var entryHeader = Declaration.B3EntryHeader;
			if (Declaration.IsInDatabase
				&& !Declaration.IsConsolidatedLVS
				&& commandAboutToBeRun.SU_MenuName == currentData
				&& entryHeader != null)
			{
				if ((messageType == MessageTypeList.Codes.CommercialAccountingDeclaration && entryHeader.CH_EntryStatus != CADEntryStatusList.Codes.Approved)
					|| (messageType == MessageTypeList.Codes.B3CUSDEC && entryHeader.CH_EntryStatus != B3EntryStatusList.Codes.Accepted && entryHeader.CH_EntryStatus != B3EntryStatusList.Codes.Confirmed))
				{
					return base.GetDataStateBeforeRunCore(commandAboutToBeRun);
				}
			}
			if (entryHeader == null || Declaration.CA_RequiresMerge)
			{
				return new DocumentSupporterDataState(false, Res.GetString("a8669d68-279f-48fd-aace-b28d0abb8e1a", "{0} entry doesn't exist, or requires merge. Please follow Brokerage->Generate Entries (Merge) to generate entries.", messageType));
			}
			if (commandAboutToBeRun.SU_MenuName == asLodged
					&& getMessage(entryHeader) == null)
			{
				if (messageType == MessageTypeList.Codes.B3CUSDEC)
				{
					return new DocumentSupporterDataState(false, Res.GetString("a727b36d-f55f-4461-8b91-ae95feb10040", "No accepted {0} message has been found to print {1}.", messageType, asLodged));
				}
				else
				{
					return new DocumentSupporterDataState(false, Res.GetString("eb3898f2-d7a2-4e66-81d5-dc81ec59d058", "No response {0} message has been found to print {1}.", messageType, asLodged));
				}
			}
			return null;
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (commandBeingRun != null)
			{
				if (commandBeingRun.SU_MenuName == LVSIdentifierDetailsDocument)
				{
					return (from JobComInvoiceHeaderToPrint invoice in Declaration.LinesToPrint
							where invoice.ShouldBePrinted
							let wrapper = DocumentWrapperFactory.CreateCustomsWrapper(Constants.DataContext.JobComInvoiceHeader, invoice.InvoiceHeader, Declaration.Country.Code)
							where wrapper != null
							select wrapper).ToArray();
				}
				switch (dataContext)
				{
					case Constants.DataContext.GenericFreightJobByReleaseStatus:
						var releaseStatusesToPrint = Declaration.ReleaseStatusesToPrint;
						return (from ReleaseStatus releaseStatus in releaseStatusesToPrint
								  where releaseStatus.RL_ShouldBePrinted || releaseStatusesToPrint.Count == 1
								  from wrapper in DocumentWrapperFactory.GenerateGenericWrappers(Constants.DataContext.GenericFreightJob, Declaration, releaseStatus)
								  select wrapper).ToArray();
					case Constants.DataContext.GenericFreightJobRouting:
						var transportToPrint = Declaration.TransportToPrint;
						return transportToPrint != null ? DocumentWrapperFactory.GenerateGenericWrappers(Core.Constants.DataContext.GenericFreightJobRouting, Declaration, transportToPrint) : null;
					case Constants.DataContext.Declaration:
						return new DocumentWrapper[] { DocDeclaration.New(Declaration, Declaration.Factory) };
					case Constants.DataContext.GenericFreightJob:
						return new DocumentWrapper[] { DocumentWrappers.GenericWrappers.FreightWrapperFromDeclaration.New(Declaration, null, Declaration.Factory) };
				}
			}
			return base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
		}

		public override string GetFilterValue(DocumentFilters filterName)
		{
			var declaration = Declaration;
			var result = base.GetFilterValue(filterName);

			switch (filterName)
			{
				case DocumentFilters.CAAsAccountedDataSupport:
					if (declaration.MessageTypeForDocumentFilter == JobMessageTypeList.Codes.Import ||
						declaration.MessageTypeForDocumentFilter == JobMessageTypeList.Codes.LowValueShipments)
					{
						if (declaration.IsCADEnabled)
						{
							result = MessageTypeList.Codes.CommercialAccountingDeclaration;
						}
						else
						{
							result = MessageTypeList.Codes.B3CUSDEC;
						}
					}
					break;
				case DocumentFilters.CACurrentDataSupport:
					if (declaration.MessageTypeForDocumentFilter == JobMessageTypeList.Codes.Import ||
						declaration.MessageTypeForDocumentFilter == JobMessageTypeList.Codes.LowValueShipments ||
						declaration.MessageTypeForDocumentFilter == JobMessageTypeList.Codes.LVSForConsolidation)
					{
						if (declaration.IsCADEnabled)
						{
							result = MessageTypeList.Codes.CommercialAccountingDeclaration;
						}
						else
						{
							result = MessageTypeList.Codes.B3CUSDEC;
						}
					}
					break;
				case DocumentFilters.CAIM2SUPPORT:
					result = declaration.MessageTypeForDocumentFilter == JobMessageTypeList.Codes.ImportCopyforB2 && !declaration.JE_IsCancelled ? "Y" : "N";
					break;
			}

			return result;
		}

		protected override IDocumentEventsHandler[] GetDocumentEventsHandlers()
		{
			var result = base.GetDocumentEventsHandlers().ToList();
			result.Add(new JobDeclarationDocumentEventsHandler(Declaration));
			return result.ToArray();
		}

		#region Implementation

		JobDeclaration Declaration
		{
			get { return (JobDeclaration)BusinessObject; }
		}

		internal const string B3ImportDataContext = ".B3ImportEntry";
		internal const string CADImportDataContext = ".CADImportEntry";
		internal const string B3XAdjustmentsDataContext = ".B3XAdjustments";
		internal const string B2AdjustmentsDataContext = ".B2Adjustments";
		internal const string B2AdjustmentsForIM2DataContext = ".B2AdjustmentsForIM2";
		internal const string CADEXLeadSheet = ".CADEXLeadSheet";
		internal const string LVSIdentifierDetailsDocument = "LVS Identifier Details";
		internal const string A8AInBondRoutingDocument = "A8A In Bond (Routing)";
		internal const string B2AdjustmentDocument = "B2 Adjustment";

		#endregion
	}
}
