using System;
using System.Collections.Generic;
using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using CusEntryHeader = Enterprise.Customs.CN.Business.CusEntryHeader;

namespace Enterprise.Customs.CN.Module.Testing
{
	class EntryHeaderFilterUserControlTest : Customs.Module.Testing.EntryHeaderFilterUserControlTest
	{
		public void TestColourDeciding()
		{
			var coll = new CNDeclarationDeadlineWarningThresholdCollection();
			var cnDeclarationDeadlineWarningThreshold = coll.AddNew();
			cnDeclarationDeadlineWarningThreshold.TransportMode = "ALL";
			cnDeclarationDeadlineWarningThreshold.FirstLevelThreshold = 1;
			cnDeclarationDeadlineWarningThreshold.FirstLevelWarningColor = ColorHelper.GetRGBbyColor(Color.Red);
			cnDeclarationDeadlineWarningThreshold.SecondLevelThreshold = 3;
			cnDeclarationDeadlineWarningThreshold.SecondLevelWarningColor = ColorHelper.GetRGBbyColor(Color.LightSalmon);
			cnDeclarationDeadlineWarningThreshold.ThirdLevelThreshold = 7;
			cnDeclarationDeadlineWarningThreshold.ThirdLevelWarningColor = ColorHelper.GetRGBbyColor(Color.LightYellow);
			cnDeclarationDeadlineWarningThreshold.DelayedWarningColor = ColorHelper.GetRGBbyColor(Color.Blue);
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.ManuallySetEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, "000000000000000001", ZDateTime.Empty);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var cusEntryHeaders = declaration.CustomsEntryHeaders;
			var filterBusinessObject = new EntryHeaderFilterBusinessObject();
			using (CNCustomsDataRegistry.Instance.CNDeclarationDeadlineWarningThresholdSetting.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, coll))
			using (var form = new ZForm())
			using (var filterStrip = new EntryHeaderFilterUserControlForTest(cusEntryHeaders, filterBusinessObject))
			{
				form.Controls.Add(filterStrip);
				form.Show();
				AssertGridRowColor(filterStrip, entryHeader, ZDateTime.Today, Color.Empty);
				AssertGridRowColor(filterStrip, entryHeader, ZDateTime.Today.AddDays(-9), Color.LightYellow);
				AssertGridRowColor(filterStrip, entryHeader, ZDateTime.Today.AddDays(-13), Color.LightSalmon);
				AssertGridRowColor(filterStrip, entryHeader, ZDateTime.Today.AddDays(-14), Color.Red);
				AssertGridRowColor(filterStrip, entryHeader, ZDateTime.Today.AddDays(-15), Color.Blue);
			}

			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			using (CNCustomsDataRegistry.Instance.CNDeclarationDeadlineWarningThresholdSetting.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, coll))
			using (var form = new ZForm())
			using (var filterStrip = new EntryHeaderFilterUserControlForTest(cusEntryHeaders, filterBusinessObject))
			{
				form.Controls.Add(filterStrip);
				form.Show();
				AssertGridRowColor(filterStrip, entryHeader, ZDateTime.Today, Color.Empty);
				AssertGridRowColor(filterStrip, entryHeader, ZDateTime.Today.AddDays(-9), Color.Empty);
				AssertGridRowColor(filterStrip, entryHeader, ZDateTime.Today.AddDays(-13), Color.Empty);
				AssertGridRowColor(filterStrip, entryHeader, ZDateTime.Today.AddDays(-14), Color.Empty);
				AssertGridRowColor(filterStrip, entryHeader, ZDateTime.Today.AddDays(-15), Color.Empty);
			}
		}

		protected override List<string> FilteredGridColumns
		{
			get
			{
				var list = new List<string>()
					{
						CusEntryHeader.Schema.DeclarationUnifiedNumber,
						CusEntryHeader.Schema.CIQNumber,
						CusEntryHeader.Schema.CIQStatus,
						CusEntryHeader.Schema.CIQStatusDescription,
						CusEntryHeader.Schema.BillOfLading,
						CusEntryHeader.Schema.CustomsProcedureCode,
						CusEntryHeader.Schema.CustomsProcedureDesc,
						CusEntryHeader.Schema.MovementReferenceNumberIssueDate,
						CusEntryHeader.Schema.ACDANumber,
						EntryHeaderFilterUserControl.Schema.BranchCode,
						EntryHeaderFilterUserControl.Schema.SupplierCode,
						EntryHeaderFilterUserControl.Schema.ImporterCode,
						EntryHeaderFilterUserControl.ColumnNames.ManufacturerCode,
						EntryHeaderFilterUserControl.ColumnNames.ManufacturerName,
						EntryHeaderFilterUserControl.ColumnNames.BuyerCode,
						EntryHeaderFilterUserControl.ColumnNames.BuyerName,
						EntryHeaderFilterUserControl.ColumnNames.CustomsOffice,
						EntryHeaderFilterUserControl.ColumnNames.OfficeOfEntryExit,
						EntryHeaderFilterUserControl.ColumnNames.ManualNo,
						EntryHeaderFilterUserControl.ColumnNames.Packages,
						EntryHeaderFilterUserControl.ColumnNames.TotalWeight,
						CusEntryHeader.Schema.ArchiveDate, CusEntryHeader.Schema.ArchiveUser,
						EntryHeaderFilterUserControl.ColumnNames.LastAuditedDate,
						EntryHeaderFilterUserControl.ColumnNames.LastAuditedUser,
						EntryHeaderFilterUserControl.ColumnNames.DeclarationDeadline,
						EntryHeaderFilterUserControl.ColumnNames.RemainingDaysForDeclaration,
						EntryHeaderFilterUserControl.ColumnNames.DaysOfDelayedDeclaration,
						EntryHeaderFilterUserControl.ColumnNames.FeeForDelayedDeclaration,
						EntryHeaderFilterUserControl.ColumnNames.ReadyForCompleteDeclaration,
					};
				list.AddRange(base.FilteredGridColumns);
				return list;
			}
		}

		protected override List<string> ColumnNamesInSortOrder => new List<string> {
			CusEntryHeader.Schema.EntryNumber,
			CusEntryHeader.Schema.DeclarationReference,
			CusEntryHeader.Schema.CH_BGMReference,
			CusEntryHeader.Schema.DeclarationUnifiedNumber,
			CusEntryHeader.Schema.CIQNumber,
			CusEntryHeader.Schema.CH_MessageType,
			CusEntryHeader.Schema.CH_MessageTypeDescription,
			CusEntryHeader.Schema.CH_EntryStatus,
			CusEntryHeader.Schema.EntryHeaderStatusDescription,
			CusEntryHeader.Schema.CH_Status,
			CusEntryHeader.Schema.MessageStatusDescription,
			CusEntryHeader.Schema.CIQStatus,
			CusEntryHeader.Schema.CIQStatusDescription,
			CusEntryHeader.Schema.BillOfLading,
			CusEntryHeader.Schema.CustomsProcedureCode,
			CusEntryHeader.Schema.CustomsProcedureDesc,
			CusEntryHeader.Schema.CH_EntrySubmittedDate,
			CusEntryHeader.Schema.MovementReferenceNumberIssueDate,
			CusEntryHeader.Schema.CH_EntryReleaseDate,
			EntryHeaderFilterUserControl.Schema.BranchCode,
			EntryHeaderFilterUserControl.Schema.SupplierCode,
			EntryHeaderFilterUserControl.Schema.SupplierName,
			EntryHeaderFilterUserControl.Schema.ImporterCode,
			EntryHeaderFilterUserControl.Schema.ImporterName,
			EntryHeaderFilterUserControl.Schema.ShipmentType,
			EntryHeaderFilterUserControl.Schema.TransportMode,
			EntryHeaderFilterUserControl.ColumnNames.CustomsOffice,
			EntryHeaderFilterUserControl.ColumnNames.DeclarationDeadline,
			EntryHeaderFilterUserControl.ColumnNames.OfficeOfEntryExit,
			EntryHeaderFilterUserControl.ColumnNames.ManualNo,
			EntryHeaderFilterUserControl.ColumnNames.Packages,
			EntryHeaderFilterUserControl.ColumnNames.RemainingDaysForDeclaration,
			EntryHeaderFilterUserControl.ColumnNames.ReadyForCompleteDeclaration,
			CusEntryHeader.Schema.CH_WarehouseTransactionStatus,
			CusEntryHeader.Schema.CH_WarehouseTransactionStatusDescription,
		};

		protected override List<string> InVisibleFilteredGridColumns => new List<string>
		{ EntryHeaderFilterUserControl.Schema.ExportDate,
			EntryHeaderFilterUserControl.Schema.CustomsAgentCode,
			EntryHeaderFilterUserControl.Schema.CustomsAgentName,
			EntryHeaderFilterUserControl.Schema.ControllingAgentCode,
			EntryHeaderFilterUserControl.Schema.ControllingAgentName,
			EntryHeaderFilterUserControl.Schema.ControllingCustomerCode,
			EntryHeaderFilterUserControl.Schema.ControllingCustomerName,
			EntryHeaderFilterUserControl.Schema.OwnerReference,
			EntryHeaderFilterUserControl.Schema.MasterBill,
			EntryHeaderFilterUserControl.Schema.HouseBill,
			EntryHeaderFilterUserControl.Schema.Vessel,
			EntryHeaderFilterUserControl.Schema.VoyageFlightNo,
			EntryHeaderFilterUserControl.Schema.OriginETD,
			EntryHeaderFilterUserControl.Schema.FinalDestinationETA,
			EntryHeaderFilterUserControl.Schema.Origin,
			EntryHeaderFilterUserControl.Schema.Destination,
			EntryHeaderFilterUserControl.Schema.Loading,
			EntryHeaderFilterUserControl.Schema.Discharge,
			EntryHeaderFilterUserControl.Schema.DeclarantCode,
			EntryHeaderFilterUserControl.Schema.DeclarantName,
			EntryHeaderFilterUserControl.Schema.DeclarationType,
			EntryHeaderFilterUserControl.Schema.AgentsReference,
			EntryHeaderFilterUserControl.Schema.DateOfArrival,
			CusEntryHeader.Schema.CH_TotalPaid,
			EntryHeaderFilterUserControl.Schema.BranchName,
			EntryHeaderFilterUserControl.ColumnNames.ManufacturerCode,
			EntryHeaderFilterUserControl.ColumnNames.ManufacturerName,
			EntryHeaderFilterUserControl.ColumnNames.BuyerCode,
			EntryHeaderFilterUserControl.ColumnNames.BuyerName,
			EntryHeaderFilterUserControl.ColumnNames.TotalWeight,
			CusEntryHeader.Schema.ArchiveDate,
			CusEntryHeader.Schema.ArchiveUser,
			EntryHeaderFilterUserControl.ColumnNames.LastAuditedDate,
			EntryHeaderFilterUserControl.ColumnNames.LastAuditedUser,
			EntryHeaderFilterUserControl.ColumnNames.DaysOfDelayedDeclaration,
			EntryHeaderFilterUserControl.ColumnNames.FeeForDelayedDeclaration,
			CusEntryHeader.Schema.CH_SystemCreateTimeUtc,
			CusEntryHeader.Schema.CH_SystemCreateUser,
			CusEntryHeader.Schema.CH_SystemLastEditTimeUtc,
			CusEntryHeader.Schema.CH_SystemLastEditUser,
			CusEntryHeader.Schema.ACDANumber,
			CusEntryHeader.Schema.CH_HasManualWhsUpdate,
		};

		protected override Customs.Module.EntryHeaderFilterUserControl GetNewEntryHeaderFilterUserControl()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var cusEntryHeaders = new CusEntryHeaderCollection<CusEntryHeader>(jobDeclaration, Factory);
			var filterBusinessObject = new EntryHeaderFilterBusinessObject();
			return new EntryHeaderFilterUserControl(cusEntryHeaders, filterBusinessObject);
		}

		static void AssertGridRowColor(EntryHeaderFilterUserControlForTest userControl, CusEntryHeader entryHeader, ZDateTime arriveDate, Color color)
		{
			entryHeader.Declaration.JE_DateOfArrival = arriveDate;
			var args = new ColourDecidingEventArgs(entryHeader);
			userControl.OnColourDecidingForTest(args);
			AssertEquals(color, args.Colour);
		}
	}

	class EntryHeaderFilterUserControlForTest : EntryHeaderFilterUserControl
	{
		public EntryHeaderFilterUserControlForTest(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
		}

		public void OnColourDecidingForTest(ColourDecidingEventArgs args) => grid_ColourDeciding(null, args);
	}
}
