using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Customs.AE.Manifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.GUI.Testing;

[TestedType(typeof(SupportingDocSendingForm))]
sealed class SupportingDocSendingFormTest : ZFormBasherTest
{
	protected override bool AllowSaveOnFormForTestHasChanges => false;

	protected override bool AllowHasChangesOnFormOpen => true;

	protected override Form GetFormToBashCore()
	{
		return new SupportingDocSendingForm(new ManifestSupportingDocSendingObjectParent(Manifest));
	}

	public void TestColumns()
	{
		using (var form = new SupportingDocSendingForm(new ManifestSupportingDocSendingObjectParent(Manifest)))
		{
			var messageSendingObjectsGrid = form.Controls.Find("MessageSendingObjectsGrid", true)[0] as ZGrid;
			var columnStyles = messageSendingObjectsGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();

			CombineAssertions(() =>
			{
				AssertColumnStyle(columnStyles, "CUSRES", typeof(ZDropEditColumnStyleInfo));
				AssertColumnStyle(columnStyles, "CUSCAR", typeof(ZTextBoxColumnStyleInfo));
				AssertColumnStyle(columnStyles, "EDocFileSizeInMB", typeof(ZCalcEditColumnStyleInfo));
			});
		}

		void AssertColumnStyle(IEnumerable<ZGridColumnInfo> columnsStyleList, string columnName, Type type)
		{
			var columnStyle = columnsStyleList.FirstOrDefault(x => x.ColumnName == columnName);
			AssertNotNull($"{columnName} is missing", columnStyle);
			AssertType($"{columnName} Type", type, columnStyle);
		}
	}

	AsycudaManifestHeader Manifest
	{
		get
		{
			if (manifest == null)
			{
				manifest = Factory.New<AsycudaManifestHeader>();
			}
			return manifest;
		}
	}
	AsycudaManifestHeader manifest;
}
