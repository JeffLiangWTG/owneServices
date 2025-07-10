using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.ECU.ConsolExport
{
	public class ECUActionMenu : ActionDataMenuItem
	{
		public ECUActionMenu(IDataBoundControl owner)
			: base(owner)
		{
		}

		public static ActionDataMenuItem NewDelegate(IDataBoundControl owner)
		{
			return new ECUActionMenu(owner);
		}

		public static void Initialise()
		{
			OverridableNewDelegate.Value = new ConstructorDelegate(NewDelegate);
		}

		protected override void AddCustomMenuItems(IBusiness businessEntity)
		{
			base.AddCustomMenuItems(businessEntity);

			if (businessEntity is ForwardingConsol)
			{
				ForwardingConsol consol = businessEntity as ForwardingConsol;

				if (consol.JK_TransportMode == Core.Constants.TransportModes.Sea)
				{
					MenuItem eCUMenuItem = new ZMenuItem(ExportToECUFormatText, new EventHandler(ExportToECU));
					MenuItems.Add(eCUMenuItem);
				}
			}
		}

		bool IsConsolExported(Logs consolEvents)
		{
			ZString dEXCode = Enterprise.ZArchitecture.Business.Events.DataExport.Code;
			ZQuery query = new ZQuery(StmALogSchema.SL_SE_NKEvent, dEXCode);
			query.AddToFilter(new ZQuery(StmALogSchema.SL_IsCancelled, false));

			return consolEvents.Find(query).Length > 0;
		}

		void ExportToECU(object sender, EventArgs e)
		{
			ForwardingConsol consol = BusinessEntity as ForwardingConsol;
			bool shouldExport = true;

			if (IsValidToExport(consol))
			{
				if (IsConsolExported(consol.Logs))
				{
					DialogResult result = Globals.Message.Show(
						"Consol is Already Exported. Do you want to Re-export the Consol?",
						"Warning",
						MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes);

					if (result == DialogResult.No)
					{
						shouldExport = false;
					}
				}
			}
			else
			{
				shouldExport = false;
			}

			if (shouldExport)
			{
				ShowExportConsolForm(new ECUConsolExporter(BusinessEntity.Factory));
			}
		}

		protected void ShowExportConsolForm(FlatFileDataExporter exporter)
		{
			ForwardingConsolCollection collection = new ForwardingConsolCollection(new BusinessObjectFactory());
			collection.Add((BusinessObject)BusinessEntity);

			CollectionWrapperBusinessObjectReader reader = new CollectionWrapperBusinessObjectReader(collection);
			ZFormModaliser.ShowDialogAndDispose(new DataExportForm(exporter, reader));
		}

		#region IsValidToExport

		bool IsValidToExport(ForwardingConsol consol)
		{
			bool result = false;

			ZString origin = consol.JK_RL_NKLoadPort.SubstringSafe(0, 2);
			ZString dest = consol.JK_RL_NKDischargePort.SubstringSafe(0, 2);

			RefCountry currentCompanyCountry = GlbCompany.CurrentCompany.Country;

			if (origin == currentCompanyCountry.Code && dest != currentCompanyCountry.Code)
			{
				result = true;
			}
			else
			{
				Globals.Message.ShowError("This Operation is not Supported for Import Consol.");
			}

			return result;
		}

		#endregion

		const string ExportToECUFormatText = "Export to ECU Format";
	}
}
