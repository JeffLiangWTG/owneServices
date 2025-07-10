using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ARTransferController))]
	class ARTransferControllerTest : TransferControllerTest
	{
		protected override Type TypeOfFromRow
		{
			get { return typeof(ARTransferFromRow); }
		}

		protected override Type TypeOfToRow
		{
			get { return typeof(ARTransferToRow); }
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ARTransfer;
		}

		protected override BusinessObject ParentTransactionHeaderRow
		{
			get { return ARFromRow; }
		}

		protected override BusinessObject ParentTransactionHeaderRowNotSaved
		{
			get { return new BusinessObjectFactory().New(typeof(ARTransferFromRow)) as ARTransferFromRow; }
		}

		protected override BusinessObject GetFormBusinessEntity()
		{
			return Transfer.New(typeof(ARTransfer), Factory);
		}

		protected override SecurityCheckpoint ExpectedCheckPointForView
		{
			get { return Env.Security.ViewReceivablesTransfer; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForEdit
		{
			get { return Env.Security.ViewReceivablesTransfer; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForNew
		{
			get { return Env.Security.NewReceivablesTransfer; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForDelete
		{
			get { return Env.Security.ReverseReceivablesTransfer; }
		}

		protected override void SetupTransactionHeaderRows()
		{
			ARFromRow = Factory.New(typeof(ARTransferFromRow)) as ARTransferFromRow;
			ARToRow = Factory.New(typeof(ARTransferToRow)) as ARTransferToRow;
			ARFromRow.AH_TransactionCount = 1;
			ARFromRow.AH_TransactionNum = "000";
			ARFromRow.AH_OSExTaxAmount = 10M;
			ARFromRow.AH_PostDate = Env.Time.CurrentLocalDate;
			ARFromRow.AH_InvoiceDate = Env.Time.CurrentLocalDate;
			ARFromRow.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();

			ARToRow.AH_TransactionCount = 2;
			ARToRow.AH_TransactionNum = "000";
			ARToRow.AH_OSExTaxAmount = 10M;
			ARToRow.AH_PostDate = Env.Time.CurrentLocalDate;
			ARToRow.AH_InvoiceDate = Env.Time.CurrentLocalDate;
			ARToRow.AH_TransactionBelongsToGroup = ARFromRow.AH_TransactionBelongsToGroup;
			Factory.Save();
		}

		protected ARTransferFromRow ARFromRow;
		protected ARTransferToRow ARToRow;

		protected override string CountryCode
		{
			get { return "AU"; }
		}
	}
}
