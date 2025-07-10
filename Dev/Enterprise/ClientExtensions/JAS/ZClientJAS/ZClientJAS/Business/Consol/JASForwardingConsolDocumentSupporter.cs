using System.Collections.Generic;
using System.ComponentModel;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Client.JAS.Business
{
	public class JASForwardingConsolDocumentSupporter : ForwardingConsolDocumentSupporter
	{
		public JASForwardingConsolDocumentSupporter(JASForwardingConsol consol)
			: base(consol)
		{
		}

		#region JXC Air Message Export On DocumentPrinted

		protected override void InitialiseCore(IDocumentEvents documentEventSource)
		{
			base.InitialiseCore(documentEventSource);
			documentEventSource.DocumentPrinted += new DocumentPrintedEventHandler(DocumentEventSource_DocumentPrinted);
		}

		new void DocumentEventSource_DocumentPrinted(object sender, DocumentPrintedEventArgs args)
		{
			ExportJXCAirOrOceanMessageIfApplicable(args);
		}

		void ExportJXCAirOrOceanMessageIfApplicable(DocumentPrintedEventArgs args)
		{
			IStmMenuItem menuItem = args.MenuItem;
			if (ShouldExportAirOrOceanJXCMessage(menuItem))
			{
				ForwardingConsol.ExportJXCAirOrOceanMessage();
			}
		}

		#endregion

		#region GetDataStateBeforeRun

		bool hasValidChild;

		public override DocumentSupporterDataState GetDataStateBeforeRun(IStmMenuItem commandAboutToBeRun)
		{
			DocumentSupporterDataState result = base.GetDataStateBeforeRun(commandAboutToBeRun);

			var command = commandAboutToBeRun as StmMenuItemBase;
			if (command != null && command.SU_IsDocPack)
			{
				hasValidChild = false;

				foreach (StmMenuMenuPivotBase child in command.ChildMenus)
				{
					var childCommand = Factory.Load<DocumentCommand>(child.SF_SU_Outward);
					if (ShouldExportAirOrOceanJXCMessage(childCommand))
					{
						hasValidChild = true;
						break;
					}
				}

				command.ChildMenus.CopyToList(ChildList);
			}

			if (result.IsValid && ShouldExportAirOrOceanJXCMessage(commandAboutToBeRun))
			{
				CancelEventArgs eventArgs = new CancelEventArgs();
				OnGettingDataStateBeforeRun(eventArgs);
				if (eventArgs.Cancel)
				{
					childList = null;
					result = new DocumentSupporterDataState(false, "");
				}
			}

			return result;
		}

		void OnGettingDataStateBeforeRun(CancelEventArgs eventArgs)
		{
			if (GettingDataStateBeforeRun != null)
			{
				GettingDataStateBeforeRun(this, eventArgs);
			}
		}

		internal bool ShouldExportAirOrOceanJXCMessage(IStmMenuItem command)
		{
			return WillTriggerAutoJXCAirOrOceanMessageExporting(command) && ForwardingConsol.IsSuitableForJXCAirOrOceanMessage() == JASForwardingConsol.SuitableForJXCAirOrOceanMessage.Suitable;
		}

		List<StmMenuMenuPivotBase> childList;
		internal List<StmMenuMenuPivotBase> ChildList
		{
			get
			{
				return childList ?? (childList = new List<StmMenuMenuPivotBase>());
			}
		}

		bool WillTriggerAutoJXCAirOrOceanMessageExporting(IStmMenuItem command)
		{
			bool isChild = false;

			for (int i = ChildList.Count - 1; i >= 0 && !command.SU_IsDocPack; i--)
			{
				if (Factory.Load<DocumentCommand>(ChildList[i].SF_SU_Outward) == command)
				{
					isChild = true;
					ChildList.RemoveAt(i);
					break;
				}
			}

			return
				JASDataRegistry.Instance.EnableAutoJXCMessaging &&
				(WillTriggerAutoAirMessageExporting(command, isChild) ||
				WillTriggerAutoOceanMessageExporting(command, isChild));
		}

		bool WillTriggerAutoAirMessageExporting(IStmMenuItem command, bool isChild)
		{
			return ForwardingConsol.IsAir && ((command.SU_MenuName == PrintFinalMaster && !isChild) || (command.SU_MenuPath == ExportDocumentPackPath && hasValidChild));
		}

		bool WillTriggerAutoOceanMessageExporting(IStmMenuItem command, bool isChild)
		{
			return ForwardingConsol.IsSea && ((command.SU_MenuPath == ExportManifestPath && !isChild) || (command.SU_MenuPath == ExportDocumentPackPath && hasValidChild));
		}

		protected new JASForwardingConsol ForwardingConsol
		{
			get { return (JASForwardingConsol)base.ForwardingConsol; }
		}

		public event CancelEventHandler GettingDataStateBeforeRun;
		const string PrintFinalMaster = "Print Final Master";
		const string ExportManifestPath = "Departure/Manifest";
		const string ExportDocumentPackPath = "Departure/Document Pack";

		#endregion
	}
}
