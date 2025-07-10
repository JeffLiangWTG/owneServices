using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Client.WLG
{
	class WLGInvoicingBaseDocumentSupporter : InvoicingBaseDocumentSupporter
	{
		public WLGInvoicingBaseDocumentSupporter(InvoicingBase invoice)
			: base(invoice)
		{
		}

		public new static InvoicingBaseDocumentSupporter New(InvoicingBase invoice)
		{
			return (invoice == null) ? (null) : new WLGInvoicingBaseDocumentSupporter(invoice);
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}

		public override string GetMenuTemplateFilterValue(MenuTemplateFilterType filterType, IBODocDataProvider docWrapperForCurrentPivot)
		{
			string result = null;

			switch (filterType)
			{
				case MenuTemplateFilterType.PrintStandardInvoice:
					result = new ZBool(!UseClientSpecific).ToString();
					break;
				case MenuTemplateFilterType.PrintClientSpecificInvoice:
					result = UseClientSpecific.ToString();
					break;
				default:
					result = base.GetMenuTemplateFilterValue(filterType, docWrapperForCurrentPivot);
					break;
			}

			return result;
		}

		public override DocumentSupporterDataState GetDataStateBeforeRun(IStmMenuItem commandAboutToBeRun)
		{
			CurrentCommand = commandAboutToBeRun;
			return base.GetDataStateBeforeRun(commandAboutToBeRun);
		}

		public ZBool UseClientSpecific
		{
			get
			{
				ZBool result = ZBool.False;

				if (CurrentCommand != null)
				{
					string menuName = CurrentCommand.SU_MenuName;

					if (menuName.ToUpper().Equals("CLASS A INVOICE") &&
						GlbBranch.CurrentBranch.GB_RL_NKHomePort != "CNSHA")
					{
						result = ZBool.True;
					}
				}

				return result;
			}
		}

		IStmMenuItem fCurrentCommand;
#if DEBUG
		public
#endif
 IStmMenuItem CurrentCommand
		{
			get { return fCurrentCommand; }
			set { fCurrentCommand = value; }
		}
	}
}

#region Implementation
#endregion
