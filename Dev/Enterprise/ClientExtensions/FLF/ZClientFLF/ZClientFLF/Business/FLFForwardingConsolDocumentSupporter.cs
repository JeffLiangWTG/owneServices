using CargoWise.Types;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Client.FLF
{
	public class FLFForwardingConsolDocumentSupporter : ForwardingConsolDocumentSupporter
	{
		public FLFForwardingConsolDocumentSupporter(ForwardingConsol forwardingConsol)
			: base(forwardingConsol)
		{
		}

		#region Overrides

		public override string GetMenuTemplateFilterValue(MenuTemplateFilterType filterType, IBODocDataProvider docDataProvider)
		{
			string result = null;

			switch (filterType)
			{
				case MenuTemplateFilterType.PrintStandard:
					result = ((ZBool)(!PrintClientSpecific)).ToString();
					break;
				case MenuTemplateFilterType.PrintClientSpecific:
					result = PrintClientSpecific.ToString();
					break;
				default:
					result = base.GetMenuTemplateFilterValue(filterType, docDataProvider);
					break;
			}

			return result;
		}

		public override DocumentSupporterDataState GetDataStateBeforeRun(IStmMenuItem commandAboutToBeRun)
		{
			currentCommand = commandAboutToBeRun; // need this to determine the selected menu.
			return base.GetDataStateBeforeRun(commandAboutToBeRun);
		}

		#endregion

		#region Implementation
		internal IStmMenuItem currentCommand;

		internal ZBool PrintClientSpecific
		{
			get
			{
				ZBool result = ZBool.False;

				if (currentCommand != null & currentCommand.SU_MenuName.StartsWith(FLFConstants.MenuNames.AWBBarcodeLabel))
				{
					result = ZBool.True;
				}

				return result;
			}
		}

#endregion
	}
}
