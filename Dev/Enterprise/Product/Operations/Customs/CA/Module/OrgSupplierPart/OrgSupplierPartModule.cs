using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using ServiceManager.Integration.Abstractions;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	public class OrgSupplierPartModule : Customs.Module.OrgSupplierPartModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new OrgSupplierPartFilterStripBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new OrgSupplierPartFilterStripControl(GridCollection, (OrgSupplierPartFilterStripBusinessObject)FilterBusinessObject);
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.OrgSupplierPartWorkflowDescriptorCode; }
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = base.GetNewActionMenuItems().ToList();

			var copyOGDToPGADataMenuItem = new ZMenuItem(Res.GetString("b6a5d448-1263-4eb1-9b5d-f4f1016f51e7", "Copy OGD to PGA Data"));
			copyOGDToPGADataMenuItem.Click += CopyOGDToPGAData_Click;
			result.Add(copyOGDToPGADataMenuItem);

			return result.ToArray();
		}

		void CopyOGDToPGAData_Click(object sender, EventArgs e)
		{
			var indicationResult = CopyOGDToPGADataForProductsProcessor.GetIndicationForCopyingOGDToPGAData();
			if (indicationResult != null && !indicationResult.Item1.IsEmpty)
			{
				if (indicationResult.Item2)
				{
					ShowWithScheduleServiceTaskMessage(indicationResult.Item1);
				}
				else
				{
					Globals.Message.ShowInformation(indicationResult.Item1);
				}
			}
		}

		static void ShowWithScheduleServiceTaskMessage(ZString message)
		{
			var sb = new ZStringBuilder(message);
			try
			{
				var tasks = new[] { CopyOGDToPGAServiceTaskCode };
				ObjectFactory.Get<INudgingController>().ScheduleTasks(tasks, true);
				sb.Append(Res.GetString("fde30ff5-4c6b-4052-b0e7-2305b4dddae8", "CCP service task was scheduled"));
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				sb.Append(Res.GetString("4b1135d6-e74a-45dd-a9a9-6d910a442af0", "Exceptions while scheduling CCP service task:"));
				sb.Append(e.Message);
			}

			Globals.InteractiveNotification.ShowInformation(sb.ToStringWithNewLineBetweenAppends());
		}

		const string CopyOGDToPGAServiceTaskCode = "CCP";
	}
}
