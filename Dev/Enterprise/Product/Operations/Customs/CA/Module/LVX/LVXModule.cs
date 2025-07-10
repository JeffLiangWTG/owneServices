using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	public class LVXModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public LVXModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.CA.CALVXJobs);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new LVXFilterStripBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new LVXFilterStripControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);

		public override ModuleIdentifier ID => ModuleIDs.Customs.CA.CALVXJobs;

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;

		public override Security.SecurityCheckpoint SecurityCheckpoint => Env.Security.CALVXJobs;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorBrokerageAttachedCode;

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems())
			{
				new ZMenuItem("-")
			};
			result.Insert(result.Count, new ZMenuItem(ResString.GetMultilingualString("816f0893-4800-4388-8b37-a8c7b05b8298", "Bulk Consolidate"), new EventHandler(Consolidate_Click)));
			return result.ToArray();
		}

		void Consolidate_Click(object sender, EventArgs e)
		{
			var log = new NoUIActionLog();
			var supporter = new LVXJobsConsolidateRunner(new OperationalActionSectionLogWrapper(log), Factory);
			var selectionCriteria = new LVXSelectionCriteriaBO(Factory);

			using (var selectionForm = new LVXSelectionForm(selectionCriteria))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(selectionForm) == DialogResult.OK)
				{
					var selectedLVXJobs = supporter.SelectLVXJobPKs(selectionCriteria);
					if (selectedLVXJobs == null || !selectedLVXJobs.Any())
					{
						Globals.Message.Show(Res.GetString("a34bf300-e2ab-450c-afc4-d2a68496d0bb", "There are no Courier LVS Declaration Jobs that match the selected criteria."));
					}
					else if (Globals.Message.Show(
						Res.GetString("f83091f8-92be-48da-abbd-3326efd07ad0", "There are {0} Courier LVS Declaration Job(s) that match the selected criteria.\r\nDo you wish to continue?", selectedLVXJobs.Count()),
						Res.GetString("0b39868e-6149-45e8-9e8b-799e4ecc84f8", "Courier LVS Declaration Jobs"), MessageBoxButtons.OKCancel, DialogResult.Cancel) == DialogResult.OK)
					{
						using (var form = new BulkConsolidationProgressForm(new LVXBulkConsolidateProcessor(selectedLVXJobs)))
						{
							ZFormModaliser.ShowDialogWithoutDispose(form);
						}
					}
				}
			}
		}

		public OperationalActionSupporter OperationalActionSupporter => new LVXOperationalActionSupporter();

		#region NoUIActionLog

		class NoUIActionLog : IOperationalActionSectionLog
		{
			public NoUIActionLog()
			{
				resultBuilder = new ZStringBuilder();
			}

			readonly ZStringBuilder resultBuilder;

			public ZString Logs
			{
				get
				{
					return resultBuilder.ToStringWithNewLineBetweenAppends();
				}
			}

			#region IOperationalActionSectionLog Members

			public void BumpSectionProgress()
			{
			}

			public void SetSectionProgressMax(int max)
			{
			}

			public void Notify(OperationalActionLogErrorLevel errorLevel, string text)
			{
				resultBuilder.Append(string.Format("{0}:{1}", errorLevel, text));
			}

			public void NotifyFormat(OperationalActionLogErrorLevel errorLevel, string format, params object[] args)
			{
				var plainArgs = args.Select(a => a is LogControllerLink ? ((LogControllerLink)a).Text : a);
				Notify(errorLevel, string.Format(format, plainArgs.ToArray()));
			}

			#endregion

			#endregion

		}
	}
}
