using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Customs.JP.AFR.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.AFR.GUI
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
	public sealed class BLLFuntionMenuHelper
	{
		public BLLFuntionMenuHelper(Func<JPAFRHeader> getCurrrentHeader, Func<JPAFRBills> getCurrrentBill)
		{
			this.getCurrrentHeader = getCurrrentHeader;
			this.getCurrrentBill = getCurrrentBill;
			RegisterSplitMenuItem = new ZMenuItem(ResString.GetMultilingualString("D2134397-B626-4C0A-836C-F822D66F5662", "Register Split Bill"),
				(sender, args) => LaunchBLLFunctionForm(BLLFunctionCode.RegisterSplit));
			RegisterSwitchMenuItem = new ZMenuItem(ResString.GetMultilingualString("23B6152D-D4C5-43FC-9891-6BD86FE3D108", "Register Switch Bill"),
				(sender, args) => LaunchBLLFunctionForm(BLLFunctionCode.RegisterSwitch));
			RegisterMergeMenuItem = new ZMenuItem(ResString.GetMultilingualString("5ABA2B64-05FB-49A5-9BBC-254A000B45A7", "Register Merge Bill"),
				(sender, args) => LaunchBLLFunctionForm(BLLFunctionCode.RegisterMerge));
			CancelSplitMenuItem = new ZMenuItem(ResString.GetMultilingualString("D4CD9730-9B0B-46B6-AEE5-CBDD2AD48FB7", "Cancel Split Bill"),
				(sender, args) => LaunchBLLFunctionForm(BLLFunctionCode.CancelSplit));
			CancelSwitchMenuItem = new ZMenuItem(ResString.GetMultilingualString("E0E24D5D-A387-4154-8CC3-6E904C55082E", "Cancel Switch Bill"),
				(sender, args) => LaunchBLLFunctionForm(BLLFunctionCode.CancelSwitch));
			CancelMergeMenuItem = new ZMenuItem(ResString.GetMultilingualString("DAD7AC1A-82CA-4135-959D-973DCC7F5447", "Cancel Merge Bill"),
				(sender, args) => LaunchBLLFunctionForm(BLLFunctionCode.CancelMerge));
		}

		void LaunchBLLFunctionForm(BLLFunctionCode functionCode)
		{
			var header = getCurrrentHeader?.Invoke();
			if (header != null)
			{
				var bill = getCurrrentBill?.Invoke();
				var masterBillNumber = bill?.BLLFunctionInfo?.JP_MasterBillNumber ?? ZString.Empty;
				if (!masterBillNumber.IsEmpty)
				{
					bill = header.Bills.FirstOrDefault(x => x.JPB_BillNumber == masterBillNumber);
				}

				using (var form = new JPAFRBLLFunctionForm(BLLFunction.New(header, functionCode, bill)))
				{
					var bllFunction = form.BusinessEntity;
					if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
					{
						var messageGenerator = new AFRMessageGenerator(header, DefaultDataObjectWriterStrategy.Instance);
						var manifestSendOK = messageGenerator.SendBLLMessageToCustoms(bllFunction);

						if (manifestSendOK)
						{
							Globals.Message.ShowInformation(ResString.GetMultilingualString("25F691E9-4A61-4165-BF56-2DAFE6102467", "Message Sending Successful\r\n"));
						}
					}
				}
			}
		}

		public ZMenuItem RegisterSplitMenuItem;
		public ZMenuItem RegisterSwitchMenuItem;
		public ZMenuItem RegisterMergeMenuItem;
		public ZMenuItem CancelSplitMenuItem;
		public ZMenuItem CancelSwitchMenuItem;
		public ZMenuItem CancelMergeMenuItem;

		readonly Func<JPAFRHeader> getCurrrentHeader;
		readonly Func<JPAFRBills> getCurrrentBill;
	}
}
