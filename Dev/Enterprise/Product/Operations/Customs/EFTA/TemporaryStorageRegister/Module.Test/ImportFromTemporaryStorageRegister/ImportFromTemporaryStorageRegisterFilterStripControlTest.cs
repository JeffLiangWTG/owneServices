using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.AutoCusTempStorageRegLine.Schema;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Module.Testing;

[TestedType(typeof(ImportFromTemporaryStorageRegisterFilterStripControl))]
sealed class ImportFromTemporaryStorageRegisterFilterStripControlTest : TestCaseWithFactory
{
	[RequiresSTA]
	public void TestAvailableGridColumns()
	{
		using var userControl = new ImportFromTemporaryStorageRegisterFilterStripControl(new(Factory), new());
		var expectedColumns = new[]
		{
		SRL_SystemCreateUser,
		SRL_SystemCreateTimeUtc,
		SRL_SystemLastEditUser,
		SRL_SystemLastEditTimeUtc,
		ReferenceColumnName,
		InternalReferenceColumnName,
		SRL_LineNumber,
		SRL_GoodsDescription,
		SRL_OwnerReferenceType,
		SRL_OwnerReference,
		SRL_PackagesRemaining,
		SRL_PackageType
		};
		AssertContainsExactElementsInAnyOrder(expectedColumns, userControl.FilteredGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
	}

	[RequiresSTA]
	public void TestColumn_ReferenceColumnName()
	{
		using (var form = new ZForm())
		{
			form.Controls.Add(userControl);
			form.Show();
			AssertEquals("Width", 130, filteredGrid.GetColumnStyle(ReferenceColumnName).Width);
			AssertEquals("Caption", "TSD Number", filteredGrid.GetColumnCaption(ReferenceColumnName));
		}
	}

	[RequiresSTA]
	public void TestColumn_InternalReferenceColumnName()
	{
		using (var form = new ZForm())
		{
			form.Controls.Add(userControl);
			form.Show();
			AssertEquals("Width", 130, filteredGrid.GetColumnStyle(InternalReferenceColumnName).Width);
			AssertEquals("Caption", "Job Reference", filteredGrid.GetColumnCaption(InternalReferenceColumnName));
		}
	}

	[RequiresSTA]
	public void TestColumn_SRL_LineNumber()
	{
		var columnInfo = filteredGrid.GetColumnStyle(SRL_LineNumber);
		AssertEquals(60, columnInfo.Width);
	}

	[RequiresSTA]
	public void TestColumn_SRL_GoodsDescription()
	{
		var columnInfo = filteredGrid.GetColumnStyle(SRL_GoodsDescription);
		AssertEquals(130, columnInfo.Width);
	}

	[RequiresSTA]
	public void TestColumn_SRL_OwnerReferenceType()
	{
		var columnInfo = filteredGrid.GetColumnStyle(SRL_OwnerReferenceType);
		AssertEquals(120, columnInfo.Width);
	}

	[RequiresSTA]
	public void TestColumn_SRL_OwnerReference()
	{
		var columnInfo = filteredGrid.GetColumnStyle(SRL_OwnerReference);
		AssertEquals(140, columnInfo.Width);
	}

	[RequiresSTA]
	public void TestColumn_SRL_PackagesRemaining()
	{
		var columnInfo = filteredGrid.GetColumnStyle(SRL_PackagesRemaining);
		AssertEquals(120, columnInfo.Width);
	}

	[RequiresSTA]
	public void TestColumn_SRL_PackageType()
	{
		var columnInfo = filteredGrid.GetColumnStyle(SRL_PackageType);
		AssertEquals(80, columnInfo.Width);
	}

	protected override void SetUp()
	{
		base.SetUp();
		userControl = new (new (Factory), new ());
		filteredGrid = userControl.FilteredGrid;
	}
	ImportFromTemporaryStorageRegisterFilterStripControl userControl;
	ZGrid filteredGrid;

	protected override void TearDown()
	{
		base.TearDown();
		userControl.Dispose();
	}

	const string RegHeader = nameof(CusTempStorageRegLine.RegHeader);

	const string ReferenceColumnName = RegHeader + "+" + nameof(CusTempStorageRegHeader.SRH_Reference);

	const string InternalReferenceColumnName = RegHeader + "+" + nameof(CusTempStorageRegHeader.SRH_InternalReference);
}
