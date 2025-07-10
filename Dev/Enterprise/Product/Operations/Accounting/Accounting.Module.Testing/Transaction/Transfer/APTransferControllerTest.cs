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
	[TestedType(typeof(APTransferController))]
	class APTransferControllerTest : TransferControllerTest
	{
		protected override Type TypeOfFromRow
		{
			get { return typeof(APTransferFromRow); }
		}

		protected override Type TypeOfToRow
		{
			get { return typeof(APTransferToRow); }
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.APTransfer;
		}

		protected override SecurityCheckpoint ExpectedCheckPointForView
		{
			get { return Env.Security.PayablesTransactions; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForNew
		{
			get { return Env.Security.NewPayablesTransfer; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForDelete
		{
			get { return Env.Security.ReversePayablesTransfer; }
		}

		protected override BusinessObject ParentTransactionHeaderRow
		{
			get { return APFromRow; }
		}

		protected override BusinessObject ParentTransactionHeaderRowNotSaved
		{
			get { return new BusinessObjectFactory().New(typeof(APTransferFromRow)) as APTransferFromRow; }
		}

		protected override BusinessObject GetFormBusinessEntity()
		{
			return Transfer.New(typeof(APTransfer), Factory);
		}

		protected override void SetupTransactionHeaderRows()
		{
			APFromRow = Factory.New(typeof(APTransferFromRow)) as APTransferFromRow;
			APToRow = Factory.New(typeof(APTransferToRow)) as APTransferToRow;
			APFromRow.AH_TransactionCount = 1;
			APFromRow.AH_TransactionNum = "000";
			APFromRow.AH_OSExTaxAmount = 10M;
			APFromRow.AH_PostDate = (ZDateTime)Env.Time.CurrentLocalDate;
			APFromRow.AH_InvoiceDate = (ZDateTime)Env.Time.CurrentLocalDate;
			APFromRow.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();

			APToRow.AH_TransactionCount = 2;
			APToRow.AH_TransactionNum = "000";
			APToRow.AH_OSExTaxAmount = 10M;
			APToRow.AH_PostDate = (ZDateTime)Env.Time.CurrentLocalDate;
			APToRow.AH_InvoiceDate = (ZDateTime)Env.Time.CurrentLocalDate;
			APToRow.AH_TransactionBelongsToGroup = APFromRow.AH_TransactionBelongsToGroup;
			Factory.Save();
		}

		protected APTransferFromRow APFromRow;
		protected APTransferToRow APToRow;
	}
}
