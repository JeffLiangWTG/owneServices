using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHMasterDocumentSupporter : DocumentSupporter
	{
		public CusCAeMHMasterDocumentSupporter(CusCAeMHMaster parent)
			: base(parent)
		{ }

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			var result = base.GetSupportedBODataSources();
			result.Add(new DataContextValue(HouseBillsDataContext));
			result.Add(new DataContextValue(MasterBillsDataContext));
			result.Add(new DataContextValue(UniversalEventMessageDocumentSupporter.UniversalEventReport));
			return result;
		}

		CusCAeMHMaster MasterBill
		{
			get { return (CusCAeMHMaster)base.BusinessObject; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CAeManifest; }
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Core.Constants.DataContext[] { Core.Constants.DataContext.CAeManifest };
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = null;
			if (dataContext == Core.Constants.DataContext.CAeManifest)
			{
				result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, MasterBill);
			}
			return result;
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.ConsolCAeManifestCustomiseDocuments; }
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			if (dataContextValue.FullDataContext == HouseBillsDataContext)
			{
				return MasterBill.HouseBills.Select(x => BODocDataProvider.Get(new CusCAeMHHouseWrapper(x))).ToArray();
			}
			else if (dataContextValue.FullDataContext == UniversalEventMessageDocumentSupporter.UniversalEventReport)
			{
				var d4MessagesStatus = MasterBill.D4MessagesOnMasterBillAndHouseBills;
				if (commandBeingRun != null && commandBeingRun.SU_MenuName == DeConsolidationCloseMenuItemName)
				{
					d4MessagesStatus = d4MessagesStatus.Where(x => x.StatusCodes.Contains(Deconsolidation));
				}
				return d4MessagesStatus.Select(x => BODocDataProvider.Get(new D4NoticeDocumentWrapper(x, YesNoList.Codes.No))).ToArray();
			}
			else if (dataContextValue.FullDataContext == MasterBillsDataContext)
			{
				var wrapper = new CusCAeMHMasterDocumentWrapper(MasterBill);
				if (wrapper?.LatestACIAcceptedMessage != null)
				{
					return new[] { BODocDataProvider.Get(wrapper) };
				}
			}
			return base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
		}

		const string Deconsolidation = "8000";
		const string HouseBillsDataContext = ".HouseBills";
		const string MasterBillsDataContext = ".MasterBills";
		const string DeConsolidationCloseMenuItemName = "Deconsolidation Close(D4)";      // Hard Coded Document Menu Names

		public override CargoWise.Types.ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			if (dataContextValue.FullDataContext == HouseBillsDataContext)
			{
				return Res.GetString("83CBE78D-C6F0-436B-A6A0-4716144893B4", "This eManifest does not have any house bill.");
			}
			else if (dataContextValue.FullDataContext == MasterBillsDataContext)
			{
				return Res.GetString("07AC64E8-0081-4CA9-996F-1B4CAFC13CC5", "This eManifest does not have any accepted message.");
			}
			else if (dataContextValue.FullDataContext == UniversalEventMessageDocumentSupporter.UniversalEventReport)
			{
				return Res.GetString("F8BC6DCB-E35E-42B5-BD3D-B5106C366967", "There is no D4 message on this eManifest nor house bills.");
			}
			return base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
		}
	}
}
