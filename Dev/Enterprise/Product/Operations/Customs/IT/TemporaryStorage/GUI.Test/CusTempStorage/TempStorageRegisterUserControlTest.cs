using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using CusTempStorageRegHeader = Enterprise.Customs.IT.TemporaryStorage.Business.CusTempStorageRegHeader;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI.Testing;

sealed class TempStorageRegisterUserControlTest : TestCaseWithFactory
{
	public void TestHeaderGroupBox()
	{
		using (var control = new TempStorageRegisterUserControl())
		{
			var headerGroupBox = control.FindSingle<ZGroupBox>("HeaderGroupBox");
			CombineAssertions(() =>
			{
				AssertEquals("HeaderGroupBox Caption", "Header", headerGroupBox.CaptionResourceString.Caption);

				headerGroupBox.AssertContainsControl<ZTextBox>("ReferenceNumberTextBox", x => x.WithCaption("Register Reference").WithBindTo(CusTempStorageRegHeader.Schema.SRH_Reference));
				headerGroupBox.AssertContainsControl<ZTextBox>("InternalReferenceTextBox", x => x.WithCaption("TS Job Reference").WithBindTo(CusTempStorageRegHeader.Schema.SRH_InternalReference));
				headerGroupBox.AssertContainsControl<ZDropEdit>("StatusDropEdit", x => x.WithCaption("Status").WithBindTo(CusTempStorageRegHeader.Schema.SRH_Status));

				headerGroupBox.AssertContainsControl<ZDateEdit>("PresentationDateEdit", x => x.WithCaption("Presentation Date").WithBindTo(CusTempStorageRegHeader.Schema.SRH_PresentationDate));
				headerGroupBox.AssertContainsControl<ZDropEdit>("PreviousReferenceTypeDropEdit", x => x.WithCaption("Reference Type").WithBindTo(CusTempStorageRegHeader.Schema.SRH_PreviousReferenceType));
				headerGroupBox.AssertContainsControl<ZTextBox>("PreviousReferenceNumberTextBox", x => x.WithCaption("MRN").WithBindTo(CusTempStorageRegHeader.Schema.SRH_PreviousReference));

				headerGroupBox.AssertContainsControl<ZTextBox>("CustomsOfficeTextBox", x => x.WithCaption("Customs Office").WithBindTo(CusTempStorageRegHeader.Schema.SRH_CustomsOffice));
				headerGroupBox.AssertContainsControl<ZTextBox>("TransportIDTextBox", x => x.WithCaption("Transport ID").WithBindTo(CusTempStorageRegHeader.Schema.SRH_TransportID));
			});
		}
	}

	public void TestItemDetailsGroupBox()
	{
		using (var control = new TempStorageRegisterUserControl())
		{
			var lineDetailsGroupBox = control.FindSingle<ZGroupBox>("LineDetailsGroupBox");
			var prefix = nameof(CusTempStorageRegHeader.CusTempStorageRegLines) + ".";
			CombineAssertions(() =>
			{
				AssertEquals("LinesDetailGroupBox Caption", "Item Details", lineDetailsGroupBox.CaptionResourceString.Caption);

				lineDetailsGroupBox.AssertContainsControl<ZTextBox>("BillNumberTextBox", x => x.WithCaption("Bill Number").WithBindTo("CusTempStorageRegLines.RegLineItemPivots.RegLineItem.SRI_HouseBill"));
				lineDetailsGroupBox.AssertContainsControl<ZCalcEdit>("LineNumberCalcEdit", x => x.WithCaption("Item Number").WithBindTo(prefix + CusTempStorageRegLine.Schema.SRL_LineNumber));
				lineDetailsGroupBox.AssertContainsControl<ZTextBox>("LocationofGoodsTextBox", x => x.WithCaption("Location of Goods").WithBindTo(prefix + CusTempStorageRegLine.Schema.SRL_LocationOfGoods));
				lineDetailsGroupBox.AssertContainsControl<ZTextBox>("OwnerReferenceNumberTextBox", x => x.WithCaption("Owner Reference Number").WithBindTo(prefix + CusTempStorageRegLine.Schema.SRL_OwnerReference));
				lineDetailsGroupBox.AssertContainsControl<ZTextBox>("GoodsDescriptionTextBox", x => x.WithCaption("Goods Description").WithBindTo(prefix + CusTempStorageRegLine.Schema.SRL_GoodsDescription));
				lineDetailsGroupBox.AssertContainsControl<ZDropEdit>("PackageTypeDropEdit", x => x.WithCaption("Package Type").WithBindTo(prefix + CusTempStorageRegLine.Schema.SRL_PackageType));
				lineDetailsGroupBox.AssertContainsControl<ZTextBox>("GrossWeightUQTextBox", x => x.WithCaption("Weight UQ").WithBindTo(prefix + CusTempStorageRegLine.Schema.SRL_GrossWeightUQ));

				lineDetailsGroupBox.AssertContainsControl<ZDateEdit>("LimitDateEdit", x => x.WithCaption("Limit Date").WithBindTo(prefix + CusTempStorageRegLine.Schema.SRL_LimitDate));
				lineDetailsGroupBox.AssertContainsControl<ZDropEdit>("OwnerReferenceTypeDropEdit", x => x.WithCaption("Owner Reference Type").WithBindTo(prefix + CusTempStorageRegLine.Schema.SRL_OwnerReferenceType));
				lineDetailsGroupBox.AssertContainsControl<ZTextBox>("PackagesRemainingTextBox", x => x.WithCaption("Packages Remaining").WithBindTo("CusTempStorageRegLines.PackagesRemainingCalculated"));
				lineDetailsGroupBox.AssertContainsControl<ZTextBox>("GrossWeightRemainingTextBox", x => x.WithCaption("Gross Weight Remaining").WithBindTo("CusTempStorageRegLines.GrossWeightRemainingCalculated"));
			});
		}
	}

	public void TestItemsGrid()
	{
		using (var form = new ZForm(Factory.New<CusTempStorageRegHeader>()))
		using (var control = new TempStorageRegisterUserControl())
		{
			form.Controls.Add(control);
			form.Show();
			var linesGroupBox = control.FindSingle<ZGroupBox>("LinesGroupBox");
			var linesGrid = linesGroupBox.FindSingle<ZGrid>("LinesGrid");

			CombineAssertions(() =>
			{
				AssertEquals("ItemsGroupBox Caption", "Items", linesGroupBox.CaptionResourceString.Caption);
				AssertEquals("Column Count", 8, linesGrid.ColumnStyles.Count);

				AssertEquals("SRL_LineNumber Caption", "Item Number", linesGrid.GetColumnCaption(CusTempStorageRegLine.Schema.SRL_LineNumber));

				var ownerReferenceTypeColumnInfo = linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_OwnerReferenceType);
				var ownerReferenceTypeGroupName = ownerReferenceTypeColumnInfo.GroupName;
				AssertEquals("SRL_OwnerReferenceType GroupName", "Owner Reference", ownerReferenceTypeGroupName.Caption);
				AssertEquals("SRL_OwnerReferenceType Caption", "Owner Reference Type", linesGrid.GetColumnCaption(CusTempStorageRegLine.Schema.SRL_OwnerReferenceType));

				var ownerReferenceColumnInfo = linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_OwnerReference);
				AssertEquals("SRL_OwnerReference GroupName is the same as SRL_OwnerReferenceType", ownerReferenceTypeGroupName.Key, ownerReferenceColumnInfo.GroupName.Key);
				AssertEquals("SRL_OwnerReference Caption", "Owner Reference Number", linesGrid.GetColumnCaption(CusTempStorageRegLine.Schema.SRL_OwnerReference));

				AssertEquals("SRL_LocationOfGoods Caption", "Location of Goods", linesGrid.GetColumnCaption(CusTempStorageRegLine.Schema.SRL_LocationOfGoods));

				AssertEquals("SRL_GoodsDescription Caption", "Goods Description", linesGrid.GetColumnCaption(CusTempStorageRegLine.Schema.SRL_GoodsDescription));

				var limitDateColumnInfo = (ZDateEditColumnStyleInfo)linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_LimitDate);
				AssertEquals("SRL_LimitDate DateTimeFormat", ZDateTimePickerFormat.Short, limitDateColumnInfo.DateTimeFormat);
				AssertEquals("SRL_LimitDate Caption", "Limit Date", linesGrid.GetColumnCaption(CusTempStorageRegLine.Schema.SRL_LimitDate));

				AssertEquals("SRL_PackagesRemaining Caption", "Packages Remaining", linesGrid.GetColumnCaption(CusTempStorageRegLine.Schema.SRL_PackagesRemaining));

				AssertEquals("SRL_PackageType Caption", "Package Type", linesGrid.GetColumnCaption(CusTempStorageRegLine.Schema.SRL_PackageType));
			});
		}
	}

	public void TestTransactionsGrid()
	{
		using (var form = new ZForm(Factory.New<CusTempStorageRegHeader>()))
		using (var control = new TempStorageRegisterUserControl())
		{
			form.Controls.Add(control);
			form.Show();
			var transactionsGroupBox = control.FindSingle<ZGroupBox>("TransactionsGroupBox");
			var transactionsGrid = transactionsGroupBox.FindSingle<ZGrid>("TransactionsGrid");
			CombineAssertions(() =>
			{
				AssertEquals("TransactionsGroupBox Caption", "Transactions", transactionsGroupBox.CaptionResourceString.Caption);
				AssertEquals("Column Count", 11, transactionsGrid.ColumnStyles.Count);

				var transactionTypeColumnInfo = transactionsGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_TransactionType);
				var transactionTypeGroupName = transactionTypeColumnInfo.GroupName;
				AssertEquals("SRT_TransactionType GroupName", "Transaction Type", transactionTypeGroupName.Caption);
				AssertEquals("SRT_TransactionType Caption", "Transaction Type", transactionsGrid.GetColumnCaption(CusTempStorageRegLineTransaction.Schema.SRT_TransactionType));

				var transactionTypeDescriptionColumnInfo = transactionsGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.TransactionTypeDescription);
				AssertEquals("TransactionTypeDescription GroupName is the same as SRT_TransactionType", transactionTypeGroupName.Key, transactionTypeDescriptionColumnInfo.GroupName.Key);
				AssertEquals("TransactionTypeDescription Caption", "Transaction Type Description", transactionsGrid.GetColumnCaption(CusTempStorageRegLineTransaction.Schema.TransactionTypeDescription));

				AssertEquals("SRT_GrossWeight Caption", "Gross Weight", transactionsGrid.GetColumnCaption(CusTempStorageRegLineTransaction.Schema.SRT_GrossWeight));

				AssertEquals("SRT_PackageQty Caption", "Package Quantity", transactionsGrid.GetColumnCaption(CusTempStorageRegLineTransaction.Schema.SRT_PackageQty));

				var referenceTypeColumnInfo = transactionsGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_ReferenceType);
				var referenceTypeGroupName = referenceTypeColumnInfo.GroupName;
				AssertEquals("SRT_ReferenceType GroupName", "Reference Type", referenceTypeGroupName.Caption);
				AssertEquals("SRT_ReferenceType Caption", "Reference Type", transactionsGrid.GetColumnCaption(CusTempStorageRegLineTransaction.Schema.SRT_ReferenceType));

				var referenceColumnInfo = transactionsGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_Reference);
				AssertEquals("SRT_Reference Caption", "Job Reference", transactionsGrid.GetColumnCaption(CusTempStorageRegLineTransaction.Schema.SRT_Reference));

				var internalReferenceTypeColumnInfo = transactionsGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_InternalReferenceType);
				AssertEquals("SRT_InternalReferenceType Caption", "Previous Document Type", transactionsGrid.GetColumnCaption(CusTempStorageRegLineTransaction.Schema.SRT_InternalReferenceType));

				var internalReferenceNumberColumnInfo = transactionsGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_InternalReferenceNumber);
				AssertEquals("SRT_InternalReferenceNumber Caption", "Previous Document", transactionsGrid.GetColumnCaption(CusTempStorageRegLineTransaction.Schema.SRT_InternalReferenceNumber));

				var commentColumnInfo = transactionsGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_Comments);
				AssertEquals("SRT_Comments CharacterCasing", System.Windows.Forms.CharacterCasing.Normal, commentColumnInfo.CharacterCasing);
				AssertEquals("SRT_Comments Caption", "Comments", transactionsGrid.GetColumnCaption(CusTempStorageRegLineTransaction.Schema.SRT_Comments));

				var dateColumnInfo = (ZDateEditColumnStyleInfo)transactionsGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_TransactionDate);
				AssertEquals("SRT_TransactionDate DateTimeFormat", ZDateTimePickerFormat.Long, dateColumnInfo.DateTimeFormat);
				AssertEquals("SRT_TransactionDate Caption", "Create Time", transactionsGrid.GetColumnCaption(CusTempStorageRegLineTransaction.Schema.SRT_TransactionDate));

				AssertEquals("SRT_SystemCreateUser Caption", "Create User", transactionsGrid.GetColumnCaption(CusTempStorageRegLineTransaction.Schema.SRT_SystemCreateUser));
			});
		}
	}

	public void TestContainersGrid()
	{
		using (var form = new ZForm(Factory.New<CusTempStorageRegHeader>()))
		using (var control = new TempStorageRegisterUserControl())
		{
			form.Controls.Add(control);
			form.Show();
			var containersGroupBox = control.FindSingle<ZGroupBox>("ContainersGroupBox");
			var containersGrid = containersGroupBox.FindSingle<ZGrid>("ContainersGrid");

			CombineAssertions(() =>
			{
				AssertEquals("ContainersGroupBox Caption", "Containers", containersGroupBox.CaptionResourceString.Caption);
				AssertEquals("Column Count", 1, containersGrid.ColumnStyles.Count);

				var containerNumberColumnInfo = containersGrid.GetColumnStyle(CusCodeData.Schema.CY_Code);
				AssertEquals("CusCodeData.CY_Code GroupName", "Container Number", containerNumberColumnInfo.GroupName.Caption);
				AssertEquals("CusCodeData.CY_Code Caption", "Container Number", containerNumberColumnInfo.CaptionResourceString.Caption);
			});
		}
	}
}
