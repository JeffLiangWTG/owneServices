using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Registry;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.Module
{
	public partial class SimplifiedDeclarationModuleForm : ZChildForm
	{
		public SimplifiedDeclarationModuleForm()
		{
			InitializeComponent();
		}

		void Cancel_Button_Click(object sender, EventArgs e)
		{
			Close();
		}

		void OK_Button_Click(object sender, EventArgs e)
		{
			var selectedEntries = FilterControlPanel.FindSingle<ZFilterStripControl>().FilteredGrid.SelectedElements.Cast<CusReconEntry>().ToArray();

			if (!selectedEntries.Any())
			{
				Globals.Message.ShowError(Res.GetString("7ece1767-efe9-4e5c-8e76-9873c8a3de3a", "Please select entries."));
			}
			else if ((selectedEntries.Sum(entry => entry.CusReconEntryLines.Count) + ReconDeclaration.CusReconEntries.Sum(entry => entry.CusReconEntryLines.Count)) > DECustomsDataRegistry.Instance.MonthlyClosingMaximumNumberOfLines.Value)
			{
				Globals.Message.ShowError(Res.GetString("08fa1e77-57dc-4df9-8e82-05137e4232e3", "Number of entry lines exceeds the configured number in registry."));
			}
			else
			{
				LinkCusReconEntriesToDeclaration(selectedEntries, ReconDeclaration);
				Close();
			}
		}

		CusReconDeclaration ReconDeclaration => (Owner as ZForm).BusinessEntity as CusReconDeclaration;

		void LinkCusReconEntriesToDeclaration(CusReconEntry[] entries, CusReconDeclaration declaration)
		{
			var entryLines = declaration.CusReconEntries.SelectMany(e => e.CusReconEntryLines).ToArray();
			var largestLineNumber = entryLines.Any() ? entryLines.Max(l => l.CRL_LineNumber) : ZShort.Zero;
			var declarationPK = declaration.PK;

			entries.ForEach(LinkCusReconEntryToDeclaration);

			void LinkCusReconEntryToDeclaration(CusReconEntry entry)
			{
				foreach (var line in entry.CusReconEntryLines)
				{
					line.CRL_CustomsStatus = ZString.Empty;
					line.CRL_LineNumber = ++largestLineNumber;
				}
				entry.CRE_CRD = declarationPK;
			}
		}
	}
}
