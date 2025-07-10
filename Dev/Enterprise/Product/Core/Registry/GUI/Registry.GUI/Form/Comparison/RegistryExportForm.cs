using System;
using System.IO;
#if NETCOREAPP
using System.Linq;
#endif
using System.Windows.Forms;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Registry;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class RegistryExportForm : RegistryComparisonForm
	{
		readonly IRegistryItemSaveHandler saveHandler;

		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public RegistryExportForm()
		{
			InitializeComponent();
		}

		public RegistryExportForm(RegistryComparisonBusinessObject bo, IRegistryItemSaveHandler saveHandler, bool hideInactiveChildren = false)
			: base(bo, hideInactiveChildren)
		{
			CargoWise.Common.Argument.NotNull(saveHandler, nameof(saveHandler));

			this.saveHandler = saveHandler;
			InitializeComponent();
		}

		void btnExport_Click(object sender, EventArgs e)
		{
			using (var saveDialog = new ZSaveFileDialog { Filter = CommonFileDialogFilters.XML, AddExtension = true, DefaultExt = (NoResString)"xml" })
			{
				if (ZFormModaliser.ShowCommonDialogWithoutDispose(saveDialog) == DialogResult.OK)
				{
					try
					{
						using (var stream = saveDialog.OpenFile())
						{
							// Some registry items appear in multiple categories, however we only want to export them once, so we 'Distinct'
							var distinctItems = CheckedRegistryItems.DistinctBy(items => items.Name);

							saveHandler.SaveItems(distinctItems, BusinessEntity.OverrideLevel, stream);
						}
					}
					catch (OperationCanceledException)
					{
						DeleteCreatedFiles(saveDialog);
					}
					catch (IOException)
					{
						DeleteCreatedFiles(saveDialog);

						Globals.Message.ShowError(Res.GetString("0BDFBC8D-EF33-447D-A328-158D2F2747D5", "A problem was encountered while trying to write to the file. Please try writing to a different directory."));
					}
				}
			}
		}

		void DeleteCreatedFiles(ZSaveFileDialog saveDialog)
		{
			if (File.Exists(saveDialog.UnmappedFileName))
			{
				File.Delete(saveDialog.UnmappedFileName);
			}
		}

		void btnApplyToAnotherLevel_Click(object sender, EventArgs e)
		{
			ShowApplyFormForAnotherLevel(CheckedRegistryItems, BusinessEntity.OverrideLevel);
		}
	}
}
