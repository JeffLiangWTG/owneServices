using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.ExitControl.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.ExitControl.GUI
{
	public class ExitControlLockUnlockForEditMenuCreator
	{
		public ExitControlLockUnlockForEditMenuCreator(CusExitHeader header)
		{
			this.header = Argument.NotNull(header, nameof(header));
		}

		readonly CusExitHeader header;

		public ZMenuItem Create()
		{
			var obj = new ZMenuItem(LockUnlockMenuItemCaption, LockUnlockExitControlMenu_Click);
			return obj;
		}

		string LockUnlockMenuItemCaption
			=> ResString.GetMultilingualString("3488A2AF-6F2C-42C1-A156-53E5F77B6362", "Lock/Unlock Exit Control");

		string LockMenuItemCaption
			=> ResString.GetMultilingualString("D14BEC84-09E0-4BC4-BAB4-A8DAD3499471", "Lock Exit Control");

		string UnlockMenuItemCaption
			=> ResString.GetMultilingualString("9F615996-346E-4C4E-A9EB-2423DE5FC4F5", "Unlock Exit Control");

		void LockUnlockExitControlMenu_Click(object sender, EventArgs e)
		{
			if (Globals.IsTest)
			{
				WriteLockUnlockLog(new BusinessObject[] { header }, string.Empty);
			}
			else
			{
				var isLocked = ((ICustomsFileParent)header).IsLocked;
				using (var form = new Customs.GUI.CustomsWriteToLogForm(header, new BusinessObject[] { header }, isLocked ? UnlockMenuItemCaption : LockMenuItemCaption, WriteLockUnlockLog))
				{
					form.ShowDialog();
				}
			}
		}

		void WriteLockUnlockLog(BusinessObject[] businessObjects, ZString reference)
		{
			var isLocked = ((ICustomsFileParent)header).IsLocked;
			var message = isLocked ? header.UnlockExitHeader(reference) : header.LockExitHeader(reference);
			if (!message.IsEmpty)
			{
				Globals.Message.ShowInformation(message);
				header.Factory.Save();
			}
		}
	}
}
