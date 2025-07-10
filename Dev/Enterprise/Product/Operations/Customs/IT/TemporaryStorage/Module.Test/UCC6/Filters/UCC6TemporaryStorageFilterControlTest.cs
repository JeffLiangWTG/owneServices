using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.TemporaryStorage.Module.Testing;

sealed class UCC6TemporaryStorageFilterControlTest : TestCaseWithFactory
{
	public void TestColumns()
	{
		using var form = new ZForm();
		var filterControl = new UCC6TemporaryStorageFilterControl(new TemporaryStorageHeaderCollection(Factory), new UCC6TemporaryStorageFilterStripBusinessObject());
		form.Controls.Add(filterControl);
		form.Show();

		var columnStyles = filterControl.Grid.ColumnStyles;
		CombineAssertions(() => TestCases.ForEach(x => AssertColumn(columnStyles, x.Name, x.Caption, x.GroupName, x.Width)));
	}

	void AssertColumn(ArrayList columnStyles, string columnName, string columnCaption, string groupName, int? width)
	{
		var column = columnStyles.OfType<ZGridColumnInfo>().FirstOrDefault(info => info.ColumnName == columnName);

		AssertNotNull($"Column {columnName} should be found in Grid.ColumnStyles", column);
		AssertEquals($"Column {columnName} should have caption {columnCaption}", columnCaption, column.CaptionResourceString.Caption);
		if (groupName is not null)
		{
			AssertEquals($"Column {columnName} should have group name {groupName}", groupName, column.GroupName.Caption);
		}
		if (width.HasValue)
		{
			AssertEquals($"Column {columnName} should match width {width.Value}", width.Value, column.Width);
		}
		if (columnName == "CustomsStatusDate")
		{
			var dateColumn = column as ZDateEditColumnStyleInfo;
			AssertNotNull($"Column {columnName} should be a date column", dateColumn);
			AssertEquals($"Column {columnName} should have DateTimeFormat as Short", ZDateTimePickerFormat.Short, dateColumn.DateTimeFormat);
		}
	}

	List<ColumnDefinition> TestCases =>
	[
		new(AsycudaManifestHeaderSchema.Constants.AMA_JobReference, "Job #"),
		new(AsycudaManifestHeaderSchema.Constants.AMA_DateAtCustomsOffice, "Presentation Date"),
		new(AsycudaManifestHeaderSchema.Constants.AMA_CustomsOffice, "Supervising Customs Office", 120),
		new(nameof(TemporaryStorageHeader.PresentationCustomsOffice), "Presentation Customs Office"),
		new(AsycudaManifestHeaderSchema.Constants.AMA_GB, "Branch"),
		new(AsycudaManifestHeaderSchema.Constants.AMA_OA_Presenter, "Presenter"),
		new(AsycudaManifestHeaderSchema.Constants.AMA_OA_Carrier, "Carrier"),
		new(nameof(TemporaryStorageHeader.PlaceOfUnloading), "Place of Unloading"),
		new(nameof(TemporaryStorageHeader.CRN), "CRN"),
		new(AsycudaManifestHeaderSchema.Constants.AMA_MessageType, "Message Type"),
		new(nameof(TemporaryStorageHeader.CustomsStatus), "Customs Status"),
		new(AsycudaManifestHeaderSchema.Constants.AMA_TransportMode, "Transportation Mode"),
		new(nameof(TemporaryStorageHeader.TransportType), "Transport Type"),
		new(nameof(TemporaryStorageHeader.ArrivalTransportMeansCode), "Transport ID"),
		new(AsycudaManifestHeaderSchema.Constants.AMA_OA_Declarant, "Declarant Address", "Declarant", 120),
		new("Declarant+Header+OH_Code", "Declarant Code", "Declarant", 120),
		new("Declarant+CompanyName", "Declarant Name", "Declarant", 120),
		new(AsycudaManifestHeaderSchema.Constants.AMA_OA_Representative, "Representative Address", "Representative", 120),
		new("Representative+Header+OH_Code", "Representative Code", "Representative", 120),
		new("Representative+CompanyName", "Representative Name", "Representative", 120),
		new("GoodsLocation+Address+AuthorisationNumber", "Authorization No.", 120),
		new("GoodsLocation+Address+IdentificationHolder+MainAddress+OA_Address1", "Organization Address", "Location of Goods: Organization", 120),
		new("GoodsLocation+Address+IdentificationHolder+OH_Code", "Organization Code", "Location of Goods: Organization", 120),
		new("GoodsLocation+Address+IdentificationHolder+OH_FullNameTruncated", "Organization Name", "Location of Goods: Organization", 120),
		new("GoodsLocation+CGL_AdditionalIdentifier", "Place ID", 120),
		new(AsycudaManifestHeaderSchema.Constants.AMA_MessageStatus, "Message Status", 100),
		new("CustomsStatusDate", "Customs Status Date", 100),
		new("CustomsStatusDescription", "Customs Status Description", 120),
		new("MessageStatusDescription", "Message Status Description", 120),
		new(AsycudaManifestHeaderSchema.Constants.AMA_AgentType, "Repres. Qual.", 120)
	];
}

sealed class ColumnDefinition(string name, string caption, string groupName = null, int? width = null)
{
	public ColumnDefinition(string name, string caption, int width) : this(name, caption, null, width)
	{
	}

	public string Name { get; } = name;
	public string Caption { get; } = caption;
	public string GroupName { get; } = groupName;
	public int? Width { get; } = width;
}
