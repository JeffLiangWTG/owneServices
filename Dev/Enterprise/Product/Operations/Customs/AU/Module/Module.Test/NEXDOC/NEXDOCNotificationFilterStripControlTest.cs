using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Module.NEXDOC.Testing
{
	sealed class NEXDOCNotificationFilterStripControlTest : TestCaseWithFactory
	{
		public void TestInitializeAdditionalColumns()
		{
			var notificationCollection = new QuarantineNexDocNotificationCollection(Factory);
			var filterBO = new NEXDOCNotificationFilterStripBusinessObject();
			using (var form = new ZForm())
			using (var filterControl = new NEXDOCNotificationFilterStripControl(notificationCollection, filterBO))
			{
				form.Controls.Add(filterControl);
				form.Show();
				var grid = filterControl.FilteredGrid;
				AssertEquals("QN_RexNumber", ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[4]).ColumnName);
				AssertEquals("QN_ExporterReference", ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[5]).ColumnName);
				AssertEquals("QN_NotificationType", ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[6]).ColumnName);
				AssertEquals("QN_AcknowledgeStatus", ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[7]).ColumnName);
				AssertEquals("QN_MessageStatus", ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[8]).ColumnName);
				AssertEquals("QN_ReceivedDate", ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[9]).ColumnName);
				AssertEquals("QN_SystemCreateUser", ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[10]).ColumnName);
				AssertEquals("QN_ForwardingGroupID", ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[11]).ColumnName);
				AssertEquals("QN_ReceivingExporterID", ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[12]).ColumnName);
				AssertEquals("QN_RexStatus", ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[13]).ColumnName);
				AssertEquals("QN_TransferringExporterID", ((ZGridColumnInfo)filterControl.FilteredGrid.ColumnStyles[14]).ColumnName);
			}
		}
	}
}
