using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.CusTempStorage.Testing
{
	class TempStorageRegisterUserControlTest : TestCaseWithFactory
	{
		public void TestHeaderGroupBox()
		{
			using (var control = new TempStorageRegisterUserControl())
			{
				var headerGroupBox = control.FindSingle<ZGroupBox>("HeaderGroupBox");
				CombineAssertions(() =>
				{
					AssertEquals("HeaderGroupBox Caption", "Header", headerGroupBox.CaptionResourceString.Caption);
					AssertEquals("DDTNumberTextBox BindTo", CusTempStorageRegHeader.Schema.SRH_Reference, headerGroupBox.FindSingle<ZTextBox>("DDTNumberTextBox").BindTo);
					AssertEquals("InternalReferenceTextBox BindTo", CusTempStorageRegHeader.Schema.SRH_InternalReference, headerGroupBox.FindSingle<ZTextBox>("InternalReferenceTextBox").BindTo);
					AssertEquals("PreviousReferenceTypeDropEdit BindTo", CusTempStorageRegHeader.Schema.SRH_PreviousReferenceType, headerGroupBox.FindSingle<ZDropEdit>("PreviousReferenceTypeDropEdit").BindTo);
					AssertEquals("StatusDropEdit BindTo", CusTempStorageRegHeader.Schema.SRH_Status, headerGroupBox.FindSingle<ZDropEdit>("StatusDropEdit").BindTo);
					AssertEquals("PreviousReferenceNumberTextBox BindTo", CusTempStorageRegHeader.Schema.SRH_PreviousReference, headerGroupBox.FindSingle<ZTextBox>("PreviousReferenceNumberTextBox").BindTo);

					var arrivalDateEdit = headerGroupBox.FindSingle<ZDateEdit>("ArrivalDateEdit");
					AssertEquals("ArrivalDateEdit BindTo", CusTempStorageRegHeader.Schema.SRH_ArrivalDate, arrivalDateEdit.BindTo);
					AssertEquals("ArrivalDateEdit DateTimeFormat", ZDateTimePickerFormat.Short, arrivalDateEdit.DateTimeFormat);

					var presentationDateEdit = headerGroupBox.FindSingle<ZDateEdit>("PresentationDateEdit");
					AssertEquals("PresentationDateEdit BindTo", CusTempStorageRegHeader.Schema.SRH_PresentationDate, presentationDateEdit.BindTo);
					AssertEquals("PresentationDateEdit DateTimeFormat", ZDateTimePickerFormat.Long, presentationDateEdit.DateTimeFormat);
				});
			}
		}

		public void TestLinesDetailsGroupBox()
		{
			using (var control = new TempStorageRegisterUserControl())
			{
				var lineDetailsGroupBox = control.FindSingle<ZGroupBox>("LineDetailsGroupBox");
				var prefix = nameof(CusTempStorageRegHeader.CusTempStorageRegLines) + ".";
				CombineAssertions(() =>
				{
					AssertEquals("LineDetailsGroupBox Caption", "Line Details", lineDetailsGroupBox.CaptionResourceString.Caption);
					AssertEquals("LineNumberCalcEdit BindTo", prefix + CusTempStorageRegLine.Schema.SRL_LineNumber, lineDetailsGroupBox.FindSingle<ZCalcEdit>("LineNumberCalcEdit").BindTo);
					AssertEquals("LocationofGoodsTextBox BindTo", prefix + CusTempStorageRegLine.Schema.SRL_LocationOfGoods, lineDetailsGroupBox.FindSingle<ZTextBox>("LocationofGoodsTextBox").BindTo);
					AssertEquals("OwnerReferenceNumberTextBox BindTo", prefix + CusTempStorageRegLine.Schema.SRL_OwnerReference, lineDetailsGroupBox.FindSingle<ZTextBox>("OwnerReferenceNumberTextBox").BindTo);
					AssertEquals("GoodsDescriptionTextBox BindTo", prefix + CusTempStorageRegLine.Schema.SRL_GoodsDescription, lineDetailsGroupBox.FindSingle<ZTextBox>("GoodsDescriptionTextBox").BindTo);
					AssertEquals("GrossWeightUQTextBox BindTo", prefix + CusTempStorageRegLine.Schema.SRL_GrossWeightUQ, lineDetailsGroupBox.FindSingle<ZTextBox>("GrossWeightUQTextBox").BindTo);
					AssertEquals("PackagesRemainingTextBox BindTo", prefix + CusTempStorageRegLine.Schema.SRL_PackagesRemaining, lineDetailsGroupBox.FindSingle<ZTextBox>("PackagesRemainingTextBox").BindTo);
					AssertEquals("PackageTypeDropEdit BindTo", prefix + CusTempStorageRegLine.Schema.SRL_PackageType, lineDetailsGroupBox.FindSingle<ZDropEdit>("PackageTypeDropEdit").BindTo);
					AssertEquals("OwnerReferenceTypeDropEdit BindTo", prefix + CusTempStorageRegLine.Schema.SRL_OwnerReferenceType, lineDetailsGroupBox.FindSingle<ZDropEdit>("OwnerReferenceTypeDropEdit").BindTo);
					AssertEquals("UnionStatusDropEdit BindTo", prefix + CusTempStorageRegLine.Schema.SRL_UnionStatus, lineDetailsGroupBox.FindSingle<ZDropEdit>("UnionStatusDropEdit").BindTo);

					var limitDateEdit = lineDetailsGroupBox.FindSingle<ZDateEdit>("LimitDateEdit");
					AssertEquals("LimitDateEdit BindTo", prefix + CusTempStorageRegLine.Schema.SRL_LimitDate, limitDateEdit.BindTo);
					AssertEquals("LimitDateEdit DateTimeFormat", ZDateTimePickerFormat.Short, limitDateEdit.DateTimeFormat);
				});
			}
		}

		public void TestLinesGrid()
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
					AssertEquals("LinesGroupBox Caption", "Lines", linesGroupBox.CaptionResourceString.Caption);
					AssertEquals("Column Count", 9, linesGrid.ColumnStyles.Count);

					AssertEquals("SRL_LineNumber Width", 88, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_LineNumber).Width);
					AssertEquals("SRL_LineNumber Caption", "Line Number", linesGrid.GetColumnCaption(CusTempStorageRegLine.Schema.SRL_LineNumber));

					var ownerReferenceTypeColumnInfo = linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_OwnerReferenceType);
					var ownerReferenceTypeGroupName = ownerReferenceTypeColumnInfo.GroupName;
					AssertEquals("SRL_OwnerReferenceType GroupName", "Owner Reference", ownerReferenceTypeGroupName.Caption);
					AssertEquals("SRL_OwnerReferenceType Width", 134, ownerReferenceTypeColumnInfo.Width);
					AssertEquals("SRL_OwnerReferenceType Caption", "Owner Reference Type", linesGrid.GetColumnCaption(CusTempStorageRegLine.Schema.SRL_OwnerReferenceType));

					var ownerReferenceColumnInfo = linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_OwnerReference);
					AssertEquals("SRL_OwnerReference GroupName is the same as SRL_OwnerReferenceType", ownerReferenceTypeGroupName.Key, ownerReferenceColumnInfo.GroupName.Key);
					AssertEquals("SRL_OwnerReference Width", 149, ownerReferenceColumnInfo.Width);
					AssertEquals("SRL_OwnerReference Caption", "Owner Reference Number", linesGrid.GetColumnCaption(CusTempStorageRegLine.Schema.SRL_OwnerReference));

					AssertEquals("SRL_LocationOfGoods Width", 110, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_LocationOfGoods).Width);
					AssertEquals("SRL_LocationOfGoods Caption", "Location of Goods", linesGrid.GetColumnCaption(CusTempStorageRegLine.Schema.SRL_LocationOfGoods));

					AssertEquals("SRL_GoodsDescription Width", 150, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_GoodsDescription).Width);
					AssertEquals("SRL_GoodsDescription Caption", "Goods Description", linesGrid.GetColumnCaption(CusTempStorageRegLine.Schema.SRL_GoodsDescription));

					var limitDateColumnInfo = (ZDateEditColumnStyleInfo)linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_LimitDate);
					AssertEquals("SRL_LimitDate DateTimeFormat", ZDateTimePickerFormat.Short, limitDateColumnInfo.DateTimeFormat);
					AssertEquals("SRL_LimitDate Width", 73, limitDateColumnInfo.Width);
					AssertEquals("SRL_LimitDate Caption", "Limit Date", linesGrid.GetColumnCaption(CusTempStorageRegLine.Schema.SRL_LimitDate));

					AssertEquals("SRL_PackagesRemaining Width", 126, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_PackagesRemaining).Width);
					AssertEquals("SRL_PackagesRemaining Caption", "Packages Remaining", linesGrid.GetColumnCaption(CusTempStorageRegLine.Schema.SRL_PackagesRemaining));

					AssertEquals("SRL_PackageType Width", 90, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_PackageType).Width);
					AssertEquals("SRL_PackageType Caption", "Package Type", linesGrid.GetColumnCaption(CusTempStorageRegLine.Schema.SRL_PackageType));

					AssertEquals("SRL_UnionStatus Width", 85, linesGrid.GetColumnStyle(CusTempStorageRegLine.Schema.SRL_UnionStatus).Width);
					AssertEquals("SRL_UnionStatus Caption", "Union Status", linesGrid.GetColumnCaption(CusTempStorageRegLine.Schema.SRL_UnionStatus));
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
					AssertEquals("Column Count", 10, transactionsGrid.ColumnStyles.Count);

					var transactionTypeColumnInfo = transactionsGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_TransactionType);
					var transactionTypeGroupName = transactionTypeColumnInfo.GroupName;
					AssertEquals("SRT_TransactionType GroupName", "Transaction Type", transactionTypeGroupName.Caption);
					AssertEquals("SRT_TransactionType Width", 107, transactionTypeColumnInfo.Width);
					AssertEquals("SRT_TransactionType Caption", "Transaction Type", transactionsGrid.GetColumnCaption(CusTempStorageRegLineTransaction.Schema.SRT_TransactionType));

					var transactionTypeDescriptionColumnInfo = transactionsGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.TransactionTypeDescription);
					AssertEquals("TransactionTypeDescription GroupName is the same as SRT_TransactionType", transactionTypeGroupName.Key, transactionTypeDescriptionColumnInfo.GroupName.Key);
					AssertEquals("TransactionTypeDescription Width", 164, transactionTypeDescriptionColumnInfo.Width);
					AssertEquals("TransactionTypeDescription Caption", "Transaction Type Description", transactionsGrid.GetColumnCaption(CusTempStorageRegLineTransaction.Schema.TransactionTypeDescription));

					AssertEquals("SRT_GrossWeight Width", 125, transactionsGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_GrossWeight).Width);
					AssertEquals("SRT_GrossWeight Caption", "Gross Weight", transactionsGrid.GetColumnCaption(CusTempStorageRegLineTransaction.Schema.SRT_GrossWeight));

					AssertEquals("SRT_PackageQty Width", 111, transactionsGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_PackageQty).Width);
					AssertEquals("SRT_PackageQty Caption", "Package Quantity", transactionsGrid.GetColumnCaption(CusTempStorageRegLineTransaction.Schema.SRT_PackageQty));

					var referenceTypeColumnInfo = transactionsGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_ReferenceType);
					var referenceTypeGroupName = referenceTypeColumnInfo.GroupName;
					AssertEquals("SRT_ReferenceType GroupName", "Reference", referenceTypeGroupName.Caption);
					AssertEquals("SRT_ReferenceType Width", 99, referenceTypeColumnInfo.Width);
					AssertEquals("SRT_ReferenceType Caption", "Reference Type", transactionsGrid.GetColumnCaption(CusTempStorageRegLineTransaction.Schema.SRT_ReferenceType));

					var referenceColumnInfo = transactionsGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_Reference);
					AssertNotEquals("SRT_Reference GroupName is the same as SRT_ReferenceType", referenceTypeGroupName.Key, referenceColumnInfo.GroupName.Key);
					AssertEquals("SRT_Reference Width", 150, referenceColumnInfo.Width);
					AssertEquals("SRT_Reference Caption", "TSD Number", transactionsGrid.GetColumnCaption(CusTempStorageRegLineTransaction.Schema.SRT_Reference));

					var commentColumnInfo = transactionsGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_Comments);
					AssertEquals("SRT_Comments CharacterCasing", System.Windows.Forms.CharacterCasing.Normal, commentColumnInfo.CharacterCasing);
					AssertEquals("SRT_Comments Width", 150, commentColumnInfo.Width);
					AssertEquals("SRT_Comments Caption", "Comments", transactionsGrid.GetColumnCaption(CusTempStorageRegLineTransaction.Schema.SRT_Comments));

					var createTimeColumnInfo = (ZDateEditColumnStyleInfo)transactionsGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_SystemCreateTimeUtc);
					AssertEquals("SRT_SystemCreateTimeUtc Width", 90, createTimeColumnInfo.Width);
					AssertEquals("SRT_SystemCreateTimeUtc DateTimeFormat", ZDateTimePickerFormat.Long, createTimeColumnInfo.DateTimeFormat);
					AssertEquals("SRT_SystemCreateTimeUtc Caption", "Create Time", transactionsGrid.GetColumnCaption(CusTempStorageRegLineTransaction.Schema.SRT_SystemCreateTimeUtc));

					AssertEquals("SRT_SystemCreateUser Width", 80, transactionsGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_SystemCreateUser).Width);
					AssertEquals("SRT_SystemCreateUser Caption", "Create User", transactionsGrid.GetColumnCaption(CusTempStorageRegLineTransaction.Schema.SRT_SystemCreateUser));

					var internalReferenceNumberColumnInfo = transactionsGrid.GetColumnStyle(CusTempStorageRegLineTransaction.Schema.SRT_InternalReferenceNumber);
					AssertEquals("SRT_InternalReferenceNumber GroupName is the same as SRT_ReferenceType", referenceTypeGroupName.Key, internalReferenceNumberColumnInfo.GroupName.Key);
					AssertEquals("SRT_InternalReferenceNumber Width", 140, internalReferenceNumberColumnInfo.Width);
					AssertEquals("SRT_InternalReferenceNumber Caption", "Job Reference", transactionsGrid.GetColumnCaption(CusTempStorageRegLineTransaction.Schema.SRT_InternalReferenceNumber));
				});
			}
		}
	}
}
