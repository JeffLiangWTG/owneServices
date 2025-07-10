using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Client.MFI.CaroTrans.Export;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.MFI.GUI
{
	public class MFIMenu : ActionDataMenuItem
	{
		public MFIMenu(IDataBoundControl owner)
			: base(owner)
		{
		}

		public new static ActionDataMenuItem New(IDataBoundControl owner)
		{
			return new MFIMenu(owner);
		}

		public static void Initialise()
		{
			OverridableNewDelegate.Value = new ConstructorDelegate(New);
		}

		protected override void AddCustomMenuItems(IBusiness businessEntity)
		{
			base.AddCustomMenuItems(businessEntity);

			if (businessEntity is CommonConsol)
			{
				MenuItem caroTransMenuItem = new ZMenuItem("Export to CaroTrans", new EventHandler(ExportToCaroTrans));
				MenuItems.Add(caroTransMenuItem);
			}
		}

		void ExportToCaroTrans(object sender, EventArgs e)
		{
			ShowExportConsolForm(new CaroTransFlatFileDataExporter(BusinessEntity.Factory));
		}

		void ShowExportConsolForm(FlatFileDataExporter exporter)
		{
			MainFormConsolCollection collection = new MainFormConsolCollection(new BusinessObjectFactory());
			collection.Add((BusinessObject)BusinessEntity);

			CollectionWrapperBusinessObjectReader reader = new CollectionWrapperBusinessObjectReader(collection);

			using (MFIDataExportForm exportForm = new MFIDataExportForm(exporter, reader))
			{
				exportForm.GetSetDefaultFileNameMethod = delegate(ZSaveFileDialog dialog)
				{
					dialog.FileName = GetDefaultFileNameForExport((CommonConsol)BusinessEntity);
				};

				ZFormModaliser.ShowDialogWithoutDispose(exportForm);
			}
		}

		internal class MFIDataExportForm : DataExportForm
		{
			public MFIDataExportForm(FlatFileDataExporter exporter, BusinessObjectReader readerObject)
				: base(exporter, readerObject)
			{ }

			protected override void Export()
			{
				if (MFIDataRegistry.Instance.CaroTransExportFileExtensions.Count == 0)
				{
					OutputTextbox.Text = CargoWiseOne.ResourceStrings.Res.GetString("D39CA561-3984-4B11-8A04-52AD33539208",
						"File extenion has not been set. Export terminated. \r\nPlease set up registry at MFI Client Extensions -> CaroTrans Tracking Export -> Export File Extension.");
				}
				else
				{
					base.Export();
				}
			}
		}

		public
 ZString GetDefaultFileNameForExport(CommonConsol consol)
		{
			ZString consolNumber = consol.JK_UniqueConsignRef;
			consolNumber = consolNumber.Remove(0, 1);
			consolNumber = consolNumber.TrimStart('0');

			return consol.JK_RL_NKDischargePort.Right(3) + consolNumber;
		}
	}
}
