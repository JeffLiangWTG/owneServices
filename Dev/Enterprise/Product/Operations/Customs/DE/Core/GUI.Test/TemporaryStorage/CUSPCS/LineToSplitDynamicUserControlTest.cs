using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.DE.Messaging;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	public class LineToSplitDynamicUserControlTest : TestCaseWithFactory
	{
		public void TestLineToSplitDynamicUserControlWithIdentificationTypeREG()
		{
			var storageJobHeader = Factory.New<CusTempStorageJobHeader>();
			var storageDec = storageJobHeader.CUSPCSCusTempStorageDecs.AddNew();
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;

			using (var form = new ZForm())
			using (var control = new LineToSplitDynamicUserControl())
			{
				control.SetDataBinding(storageDec, ZString.Empty);
				form.Controls.Add(control);
				form.Show();

				Assert(!control.Controls.Find("AWBLineToSplitUserControl", true).First().Visible);
				Assert(control.Controls.Find("REGLineToSplitUserControl", true).First().Visible);
			}
		}

		public void TestLineToSplitDynamicUserControlWithIdentificationTypeAWB()
		{
			var storageJobHeader = Factory.New<CusTempStorageJobHeader>();
			var storageDec = storageJobHeader.CUSPCSCusTempStorageDecs.AddNew();
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;

			using (var form = new ZForm())
			using (var control = new LineToSplitDynamicUserControl())
			{
				control.SetDataBinding(storageDec, ZString.Empty);
				form.Controls.Add(control);
				form.Show();

				Assert(control.Controls.Find("AWBLineToSplitUserControl", true).First().Visible);
				Assert(!control.Controls.Find("REGLineToSplitUserControl", true).First().Visible);
			}
		}
	}
}
