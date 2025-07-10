using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessagesWrappers.CIN;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.CIN
{
	public partial class ExportConsolIntegrationMenu : EDIMenu
	{
		readonly CINExportConsolntegrationWrapper wrapper;
		readonly ZMenuItem exp745Menu;
		public ExportConsolIntegrationMenu(CINExportConsolntegrationWrapper wrapper)
		{
			this.wrapper = Argument.NotNull(wrapper, nameof(wrapper));

			Text = Res.GetString("D69B5E80-204B-4A56-B243-7DCF22C31FCB", "CIN Exports");

			MenuItems.Clear();
			exp745Menu = new ZMenuItem(Res.GetString("63AF5B7E-7C14-4935-B1D2-CF215FBF0F3C", "Send 745"), SendCINExport745_Click);
			MenuItems.AddRange(new MenuItem[]
			{
				exp745Menu
			});
		}

		void SendCINExport745_Click(object sender, EventArgs args)
		{
			SendCINMessage(wrapper.ForwardingConsol, Customs.FR.Business.EntryActionCodeList.Codes.CIN745);
		}

		void SendCINMessage(ForwardingConsol consol, string messageType)
		{
			var declarations = GetDeclarations(consol);
			if (declarations.Any())
			{
				foreach (var declaration in declarations)
				{
					SendCINMessage(declaration, messageType);
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("42512C49-1334-44D4-B66B-B4EEBD123ED0", "There is no declaration to be sent."));
			}
		}

		IEnumerable<JobDeclaration> GetDeclarations(ForwardingConsol consol)
		{
			return consol?.Shipments.Cast<ForwardingShipment>().Select(x => x.GetDeclaration()).WhereNotNull().Cast<JobDeclaration>().Where(x => x.IsExport) ?? Enumerable.Empty<JobDeclaration>();
		}

		public override void RefreshMenu()
		{
			var isCInExportMenuEnabled = wrapper.IsCINExportMenuEnabled;

			exp745Menu.Enabled = isCInExportMenuEnabled;
		}
	}
}
