using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.Customs.CA.Business
{
	public class DIFReferenceNumberFountainStrategy : IDISReferenceNumberFountainStrategy
	{
		public DIFReferenceNumberFountainStrategy(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
		}
		readonly BusinessObjectFactory factory;

		IDISReferenceNumberFountainStrategyState IDISReferenceNumberFountainStrategy.CheckState()
		{
			return CheckStateCore(DISReferenceNumberFountainStrategyStateSeverity.Warning);
		}

		IDISReferenceNumberFountainStrategyState IDISReferenceNumberFountainStrategy.CheckStateForAddingNewRecord()
		{
			return CheckStateCore(DISReferenceNumberFountainStrategyStateSeverity.Error);
		}

		IUniqueIndexFailureHandler IDISReferenceNumberFountainStrategy.GetUniqueIndexFailureHandler(BusinessObject businessObject)
		{
			IUniqueIndexFailureHandler result = null;
			if (businessObject is JobRequiredDocumentAddInfo documentAddInfo)
			{
				result = new CAEntryNumberFountainUniqueIndexFailureHandler(documentAddInfo, this);
			}
			return result;
		}

		IDISReferenceNumberFountainStrategyState CheckStateCore(DISReferenceNumberFountainStrategyStateSeverity emptyRangeSeverity)
		{
			var securityNo = GetAccountSecurityNo();
			if (securityNo.IsEmpty)
			{
				return new FountainState(DISReferenceNumberFountainStrategyStateSeverity.Error, TransactionNumberMessages.AsecNumberNotSet());
			}

			var difFountain = GetNumberFountain();
			long difNextValue = difFountain.PeekPreliminaryLongOrZero();
			long maxValue = difFountain.GetMaxValue();

			if (difNextValue == 0)
			{
				return new FountainState(DISReferenceNumberFountainStrategyStateSeverity.Error, TransactionNumberMessages.DIFNotConfigured(securityNo));
			}

			if (difNextValue > maxValue)
			{
				return new FountainState(emptyRangeSeverity, TransactionNumberMessages.DIFNumberRangeIsEmpty());
			}

			long remainingCapacity = maxValue - difNextValue + 1;
			if (remainingCapacity > 0 && remainingCapacity < TransactionNumber.MinRemainingCapacity)
			{
				return new FountainState(DISReferenceNumberFountainStrategyStateSeverity.Warning, TransactionNumberMessages.NumberRangeIsAlmostEmpty(true));
			}

			return new FountainState(DISReferenceNumberFountainStrategyStateSeverity.Ok, null);
		}

		public ZString GetDISReferenceNumber()
		{
			return GetNumberFountain().GetNextReferenceNumber();
		}

		protected ZInt GetAccountSecurityNoLength() => GetAccountSecurityNo().Length;

		DIFReferenceNumberFountain GetNumberFountain()
		{
			var securityNo = GetAccountSecurityNo();
			if (securityNo.IsEmpty)
			{
				throw new InvalidOperationException(TransactionNumberMessages.AsecNumberNotSet());
			}
			return new DIFReferenceNumberFountain(securityNo, factory);
		}

		ZString GetAccountSecurityNo() => CACustomsDataRegistry.Instance.AccountSecurityNo.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

		class FountainState : IDISReferenceNumberFountainStrategyState
		{
			public FountainState(DISReferenceNumberFountainStrategyStateSeverity severity, string message)
			{
				Severity = severity;
				Message = message;
			}

			public DISReferenceNumberFountainStrategyStateSeverity Severity { get; }
			public string Message { get; }
		}

		protected class CAEntryNumberFountainUniqueIndexFailureHandler : NumberFountainUniqueIndexFailureHandler
		{
			public CAEntryNumberFountainUniqueIndexFailureHandler(JobRequiredDocumentAddInfo documentAddInfo, DIFReferenceNumberFountainStrategy strategy)
				: base(JobRequiredDocumentAddInfoSchema.Constants.Indexes.NR_UX__EX_GC_Company_EX_ReferenceNumber_EX_ApplicationCode, documentAddInfo)
			{
				this.strategy = strategy;
			}
			readonly DIFReferenceNumberFountainStrategy strategy;

			protected override INumberFountainProxy NumberFountainToFix
			{
				get { return this.strategy.GetNumberFountain().GetFountain(); }
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
			protected override CargoWise.Data.DbCommand CommandToFindMaxValueInDatabase(CargoWise.Data.DbConnection connection)
			{
				var sqlCommand = string.Format(CultureInfo.InvariantCulture, "SELECT substring(MAX({0}), {1}, 8) From {2} WHERE {3} = '{4}'",
				JobRequiredDocumentAddInfo.Schema.EX_ReferenceNumber,
				this.strategy.GetAccountSecurityNoLength() + 1,
				JobRequiredDocumentAddInfoSchema.Constants.TableName,
				JobRequiredDocumentAddInfo.Schema.EX_ApplicationCode,
				Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF);
				return connection.Command(sqlCommand);
			}
		}
	}
}
