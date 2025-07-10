using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class RemissionsUserControl : ZUserControl
	{
		public RemissionsUserControl()
		{
			InitializeComponent();
			HookEvent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			if (InvoiceLine != null && (InvoiceLine.Declaration?.IsCADEnabled ?? false))
			{
				TRSNumberTextBox.Visible = false;
				RemissionTypeDropEdit.Visible = true;
				AuthorityNumberCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("AF09D0DA-5185-4939-B13D-1BC24B4496D9", "DRL/OIC/PMT");
			}
			else
			{
				TRSNumberTextBox.Visible = true;
				RemissionTypeDropEdit.Visible = false;
			}
		}

		void HookEvent()
		{
			AuthorityNumberCodeFindBox.OnCreatingRuling -= OnCreatingRuling;
			AuthorityNumberCodeFindBox.OnCreatingRuling += OnCreatingRuling;
		}

		void UnHookEvent()
		{
			AuthorityNumberCodeFindBox.OnCreatingRuling -= OnCreatingRuling;
		}

		void OnCreatingRuling(object sender, EventArgs e)
		{
			var ruling = sender as CACusRuling;
			var invoiceLine = InvoiceLine;

			if (ruling != null && invoiceLine != null)
			{
				ruling.ZZX_RulingType = ruling.Lookups.RulingTypeList.ContainsCode(invoiceLine.CA_CalculationMethod)
					? invoiceLine.CA_CalculationMethod
					: ZString.Empty;

				ruling.Configurations.RemoveAndDeleteAll();

				foreach (var config in invoiceLine.RulingConfigurations.Cast<CusRulingConfigCombined>())
				{
					ruling.Configurations.AddNew(config.ZZY_Category, config.ZZY_Type, config.ZZY_Rate, config.ZZY_Value);
				}
			}
		}

		JobComInvoiceLine InvoiceLine => CurrentDataItem as JobComInvoiceLine;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			UnHookEvent();

			if (disposing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
