using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public class EcsMessageMenuProvider
	{
		protected EcsMessageMenuProvider(CusExitControlHeader exitHeader)
		{
			ExitHeader = Argument.NotNull(exitHeader, nameof(exitHeader));
		}
		protected readonly CusExitControlHeader ExitHeader;

		public virtual bool CreateArrivalMessages() => false;

		public virtual bool CreateDepartureMessages() => false;

		public virtual IEnumerable<ZMenuItem> CreateMenuItems()
		{
			yield return new ZMenuItem(ResString.GetMultilingualString("7BD568E5-3AC9-4238-A1D9-437BBADED3C4", "Arrive at Exit Location"), ArriveAtExitLocationClick);
			yield return new ZMenuItem(ResString.GetMultilingualString("6277636E-731F-4D83-8FAF-BC9B894571B0", "Depart from Exit Location"), DepartFromExitLocationClick);
			yield return new ZMenuItem(ResString.GetMultilingualString("EF48DA68-603C-47DC-BCB7-8A55206B1775", "Capture MRNs"), CaptureMRNsClick);
		}

		public static EcsMessageMenuProvider New(CusExitControlHeader exitHeader)
		{
			Argument.NotNull(exitHeader, nameof(exitHeader));

			var providers = ObjectFactory.Get<Hashtable>("EcsMessageMenuProviders");
			var providerHandle = (ObjectHandle)providers[exitHeader.DataGrouping.ToString()];

			return providerHandle != null ? (EcsMessageMenuProvider)providerHandle.GetObject(exitHeader) : new EcsMessageMenuProvider(exitHeader);
		}

		protected void ArriveAtExitLocationClick(object sender, EventArgs e)
		{
			if (PreSaveBeforeSendingMessages(sender as ZMenuItem))
			{
				CreateArrivalMessages();
			}
		}

		void DepartFromExitLocationClick(object sender, EventArgs e)
		{
			if (PreSaveBeforeSendingMessages(sender as ZMenuItem))
			{
				CreateDepartureMessages();
			}
		}

		void CaptureMRNsClick(object sender, EventArgs e)
		{
		}

		protected bool PreSaveBeforeSendingMessages(ZMenuItem menu)
		{
			var result = true;

			var form = (ZForm)menu?.GetMainMenu()?.GetForm();
			if (form != null && ExitHeader.HasChanges)
			{
				result = Globals.Message.Show(
					Res.GetString("840289EC-97F6-4012-A56C-3F17886C5730", "The Job has not yet been saved. Do you want to save and proceed?"),
					Res.GetString("52FDF7B2-7031-4520-B24E-51E0D7226871", "Save Job"),
					MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.Yes;

				if (result)
				{
					result = form.FireSaveButton() == ContinueWithSave.Yes && !ExitHeader.HasChanges;
				}
			}

			return result;
		}
	}
}
