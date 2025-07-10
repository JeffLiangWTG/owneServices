using System.Collections;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class InvoicingPluginToConsol : ZAlwaysLoadPlugIn
	{
		public InvoicingPluginToConsol(IBusiness hostEntity)
			: base(hostEntity)
		{
			if (Factory != null)
			{
				Factory.SetContext(BusinessContext.InvoicingPlugInGUI);
			}
			Consol = (IJobCostingPlugIn)hostEntity;
		}

		readonly IJobCostingPlugIn Consol;

		IJobInvoicingPlugIn PlugInParent
		{
			get { return (IJobInvoicingPlugIn)HostBusinessEntity; }
		}

		#region Job Invoicing Menu

		public override void OnMenuShown()
		{
			base.OnMenuShown();
		}

		#endregion

		#region Plugin Implementation

		public override string Name
		{
			get { return (NoResString)"Invoice Printing"; } // Hard-coded constant
		}

		BusinessObjectFactory fFactory
		{
			get { return HostBusinessEntity.Factory; }
		}

		public override bool CanDelete
		{
			get { return true; }
		}

		#region User Control

		public override void OnUserControlShown()
		{
			base.OnUserControlShown();
			PrintingFilter.RefreshJobNumbersCollection(JobPKsFromConsol);
			APPrintingFilter.RefreshInvoiceList();
			DraftInvoiceFilter.RefreshInvoiceList();
		}

		public override void RefreshData()
		{
			PrintingFilter.RefreshJobNumbersCollection(JobPKsFromConsol);
			APPrintingFilter.RefreshInvoiceList();
			DraftInvoiceFilter.RefreshInvoiceList();
		}

		ConsolInvoicePrintingUserControl userControl;
		protected override Control GetNewUserControl()
		{
			if (userControl != null)
			{
				userControl.Dispose();
			}

			userControl = new ConsolInvoicePrintingUserControl(PluginSecurity);

			if (PlugInParent != null && PlugInParent.InvoicingSupporter.ConsumerType != null && !PlugInParent.InvoicingSupporter.ConsumerType.InvoicingPrintingApplicable(PlugInParent))
			{
				userControl.ARInvoicesTabPage.Dispose();
				userControl.APInvoicesTabPage.Dispose();
			}
			else
			{
				if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.ARInvoices))
				{
					userControl.JobInvoicePrintingControl.Visible = false;
					userControl.ARJobInvoicingPrintingSecurityPanel.Visible = true;
					userControl.ARInvoicePrintingSecurityLabel.Text = SecurityHelper.GetErrorTextForSecurityCheckPoint(SecurityCore.ARInvoices);
				}
				else
				{
					userControl.JobInvoicePrintingControl.Visible = true;
					userControl.ARJobInvoicingPrintingSecurityPanel.Visible = false;
					userControl.ARInvoicePrintingSecurityLabel.Text = "";
				}

				if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.APInvoices))
				{
					userControl.APInvoicePrintingSplitContainer.Visible = false;
					userControl.APJobInvoicingPrintingSecurityPanel.Visible = true;
					userControl.APInvoicePrintingSecurityLabel.Text = SecurityHelper.GetErrorTextForSecurityCheckPoint(SecurityCore.APInvoices);
				}
				else
				{
					userControl.APInvoicePrintingSplitContainer.Visible = true;
					userControl.APJobInvoicingPrintingSecurityPanel.Visible = false;
					userControl.APInvoicePrintingSecurityLabel.Text = "";
				}
			}

			return userControl;
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		JobInvoicePrintingControl JobInvoicePrintingControl
		{
			get { return ((ConsolInvoicePrintingUserControl)UserControl).JobInvoicePrintingControl; }
		}

		APInvoicePrintingUserControl APInvoicePrintingUserControl
		{
			get { return ((ConsolInvoicePrintingUserControl)UserControl).APInvoicePrintingUserControl; }
		}

		APDraftInvoicePrintingUserControl APDraftInvoiceListUserControl
		{
			get { return ((ConsolInvoicePrintingUserControl)UserControl).APDraftInvoicePrintingUserControl; }
		}

		#endregion

		#endregion

		#region Jobs

		protected virtual JobCollection Jobs
		{
			get
			{
				JobCollection fJobs = new JobCollection(fFactory);
				fJobs.Load(JobsQuery);
				return fJobs;
			}
		}

		ZQuery JobsQuery
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.Or;
				foreach (ZGuid jobPK in JobPKsFromConsol)
				{
					query.AddToFilter(JobHeaderSchema.PK, jobPK);
				}
				return query;
			}
		}

		ZGuid[] JobPKsFromConsol
		{
			get
			{
				if (Consol.CostSupporter.ShipmentsList != null && Consol.CostSupporter.ShipmentsList.Length > 0)
				{
					int index = 1;
					ZSqlParameterCollection @params = new ZSqlParameterCollection();

					string dynamicSQL = "SELECT JH_PK FROM dbo.JobHeader WHERE (%JOBS%) AND JH_GC = @Company";
					@params.Add("@Company", GlbCompany.CurrentCompany.PK, JobHeaderSchema.JH_GC);
					string jobs = "";
					foreach (IJobInvoicingPlugIn shipment in Consol.CostSupporter.ShipmentsList)
					{
						string parameterName = "@Job" + index.ToString();
						@params.Add(parameterName, shipment.PK, JobHeaderSchema.JH_ParentID);
						jobs += JobHeaderSchema.JH_ParentID.Name + " = " + parameterName + " OR ";
						index++;
					}

					jobs = jobs.Remove(jobs.Length - 3, 2);
					dynamicSQL = dynamicSQL.Replace("%JOBS%", jobs);

					DynamicBusinessObjectCollection jobPKs = new DynamicBusinessObjectCollection(fFactory);
					jobPKs.Load(dynamicSQL, @params);
					ArrayList pKs = new ArrayList();
					foreach (DynamicBusinessObject bizO in jobPKs)
					{
						pKs.Add(bizO[JobHeaderSchema.PK.Name]);
					}
					return (ZGuid[])pKs.ToArray(typeof(ZGuid));
				}
				else
				{
					return System.Array.Empty<ZGuid>();
				}
			}
		}

		#endregion

		#region Document Pack Generation

		DocumentCommand ProfitShareMenuItem
		{
			get
			{
				if (fProfitShareMenuItem == null)
				{
					fProfitShareMenuItem = new BusinessObjectFactory().New<DocumentCommand>();
					fProfitShareMenuItem.SU_MenuName = Res.GetString("Accounting|InvoicingPluginToConsol|ProfitShareCalculation", "Profit Share Calculation");
					fProfitShareMenuItem.SU_ContactType = ContactType.Payables.Code;
					fProfitShareMenuItem.SU_IsSystemDefined = true;
					fProfitShareMenuItem.SU_SupportsVisualisation = false;
					fProfitShareMenuItem.SU_IsModifiable = false;
				}

				return fProfitShareMenuItem;
			}
		}

		DocumentCommand fProfitShareMenuItem;

		DocumentPack GetDocumentPackForOrg(PrintTask task, DocumentCommand menuItem, OrgHeader organisation)
		{
			DocumentPack pack = (DocumentPack)DocPackOrgsHash[organisation];
			if (pack == null)
			{
				pack = new DocumentPack(menuItem);
				pack.DocumentSupporter = Consol.CostSupporter.DocumentSupporter;
				pack.Organisation = organisation;
				pack.OrgHeaderContact = new OrgHeaderContact(organisation, null);
				pack.ForceBusinessObjectToLogAgainst(Consol as BusinessObject);

				DocPackOrgsHash[organisation] = pack;
				task.Add(pack);
			}

			return pack;
		}

		readonly Hashtable DocPackOrgsHash = new Hashtable();

		#endregion

		#region Refresh Invoices

		JobARInvoicePrintingFilter fPrintingFilter;
		JobARInvoicePrintingFilter PrintingFilter
		{
			get
			{
				if (fPrintingFilter == null)
				{
					fPrintingFilter = new JobARInvoicePrintingFilter(HostBusinessEntity as ForwardingConsol, JobPKsFromConsol);
					JobInvoicePrintingControl.Bind(PrintingFilter);
				}
				return fPrintingFilter;
			}
		}

		JobAPInvoicePrintingFilter fAPPrintingFilter;
		JobAPInvoicePrintingFilter APPrintingFilter
		{
			get
			{
				if (fAPPrintingFilter == null)
				{
					fAPPrintingFilter = new JobAPInvoicePrintingFilter(HostBusinessEntity as ForwardingConsol, JobPKsFromConsol);
					APInvoicePrintingUserControl.Bind(APPrintingFilter);
				}
				return fAPPrintingFilter;
			}
		}

		JobDraftInvoicePrintingFilter DraftInvoiceFilter
		{
			get
			{
				if (draftInvoiceFilter == null)
				{
					draftInvoiceFilter = new JobDraftInvoicePrintingFilter(HostBusinessEntity as BusinessObject);
					APDraftInvoiceListUserControl.Bind(DraftInvoiceFilter);
				}
				return draftInvoiceFilter;
			}
		}
		JobDraftInvoicePrintingFilter draftInvoiceFilter;

		#endregion

		#region Licence / Security

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Core; }
		}

		SecurityCheckpoint PluginSecurity
		{
			get
			{
				if (PlugInParent != null)
				{
					return PlugInParent.InvoicingSupporter.JobInvoicingSecurity;
				}
				else
				{
					return Env.Security.None;
				}
			}
		}

		JobInvoicingSecurityHelper SecurityHelper
		{
			get { return securityHelper ?? (securityHelper = new JobInvoicingSecurityHelper(() => PluginSecurity)); }
		}

		JobInvoicingSecurityHelper securityHelper;

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing && userControl != null)
			{
				userControl.Dispose();
			}
			base.Dispose(disposing);
		}

		public class JobARAPInvoicePrintingAggregator : NonPersistentBusinessObject, IObsoleteValidation
		{
			public JobARInvoicePrintingFilter PrintingFilter { get; set; }
			public JobAPInvoicePrintingFilter APPrintingFilter { get; set; }
		}
	}
}
