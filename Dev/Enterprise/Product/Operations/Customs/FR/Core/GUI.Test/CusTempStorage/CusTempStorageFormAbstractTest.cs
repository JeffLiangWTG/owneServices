using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.CusTempStorage.Testing
{
	[TestsSubclassesOf(typeof(CINTemporyStorageUserControlForPlugin))]
	abstract class CusTempStorageFormAbstractTest : ZFormBasherTest
	{
		public void TestTemporaryStorageControlType()
		{
			using (var userControl = TemporyStorageUserControlForPlugin)
			{
				AssertNotNull(userControl.FindSingle<TemporaryStorageHeaderUserControl>("TemporaryStorageHeaderUserControl"));
			}
		}

		public void TestMessagesUserControlType()
		{
			using (var userControl = TemporyStorageUserControlForPlugin)
			{
				AssertNotNull(userControl.FindSingle<MessagesUserControl>("MessagesUserControl"));
			}
		}

		public void TestTemporyStorageDecUserControlForPluginType()
		{
			using (var userControl = TemporyStorageUserControlForPlugin)
			{
				AssertType(TemporyStorageDecUserControlType, userControl.FindSingle<ZUserControl>("CusDecTabPageUserControl"));
			}
		}

		public void TestSetReadOnlyOnShown()
		{
			var header = CusTempStorageJobHeader.New(Factory, TemporaryStorageHeaderApplicationCode);
			using (var form = new CusTempStorageForm(header))
			{
				form.Show();
				Application.DoEvents();

				var customsOfficeBox = form.FindSingle<ZCodeFindBox>("CustomsOfficeCodeFindBox");
				var readOnly = customsOfficeBox.ReadOnly;
				AssertEquals("Control is not read only if has no IST event.", false, readOnly);
			}

			header.Logs.AddNew(Events.InStore);
			using (var form = new CusTempStorageForm(header))
			{
				form.Show();
				Application.DoEvents();

				var customsOfficeBox = form.FindSingle<ZCodeFindBox>("CustomsOfficeCodeFindBox");
				var readOnly = customsOfficeBox.ReadOnly;
				AssertEquals("Control is read only if has IST event.", true, readOnly);
			}
		}

		public override Type FormToBashType => typeof(CusTempStorageForm);

		protected override Form GetFormToBashCore()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			header.SJH_AppCode = TemporaryStorageHeaderApplicationCode;
			header.CreateRelatedCusTempStorageDec();
			Factory.Save();
			return new CusTempStorageForm(header)
			{
				ControllerID = ControllerIDs.Customs.TemporaryStorage
			};
		}

		protected abstract Type TemporyStorageDecUserControlType { get; }

		protected abstract CINTemporyStorageUserControlForPlugin TemporyStorageUserControlForPlugin { get; }

		protected abstract ZString TemporaryStorageHeaderApplicationCode { get; }
	}
}
