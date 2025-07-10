using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Accounting.Export.Business
{
	/// <summary>
	/// Universal XML writer strategy which allows certain elements to be excluded based on a different Accounting specific strategy.
	/// </summary>
	public sealed class AccountingTransactionDataObjectWriterStrategy : IDataObjectWriterStrategy
	{
		public AccountingTransactionDataObjectWriterStrategy(
			IDataObjectWriterStrategy parentStrategy = null,
			IAccountingTransactionWriterStrategy accountingStrategy = null,
			string context = null
		)
		{
			ParentStrategy = parentStrategy ?? DefaultDataObjectWriterStrategy.Instance;
			AccountingStrategy = accountingStrategy ?? DefaultAccountingTransactionWriterStrategy.Instance;
			Context = string.IsNullOrEmpty(context) ? (NoResString)"None" : context;
		}

		public string Context { get; }

		public IDataObjectWriterStrategy ParentStrategy { get; }

		public IAccountingTransactionWriterStrategy AccountingStrategy { get; }

		public bool IsAllowSet(string fieldName)
		{
			if (!AccountingStrategy.IsAllowSetForAccountingContext(fieldName))
			{
				return false;
			}
			return ParentStrategy.IsAllowSet(fieldName);
		}
	}

	[Immutable]
	public interface IAccountingTransactionWriterStrategy
	{
		/// <summary>
		/// If returns false, the Universal XML field will be excluded. If true, will delegate to the parent writer strategy.
		/// </summary>
		bool IsAllowSetForAccountingContext(string fieldName);
	}

	[Immutable]
	public sealed class DefaultAccountingTransactionWriterStrategy : IAccountingTransactionWriterStrategy
	{
		public static readonly IAccountingTransactionWriterStrategy Instance = new DefaultAccountingTransactionWriterStrategy();

		public static readonly ImmutableHashSet<string> DefaultDisallowedFields = new[]
		{
			nameof(TransactionInfo.AuthorizationDetailCollection),
			nameof(TransactionInfo.TransactionHeaderReferenceCollection)
		}.ToImmutableHashSet();

		public bool IsAllowSetForAccountingContext(string fieldName)
			=> !DefaultDisallowedFields.Contains(fieldName);
	}

	[Immutable]
	public sealed class AllFieldsAllowedAccountingTransactionWriterStrategy : IAccountingTransactionWriterStrategy
	{
		public static readonly IAccountingTransactionWriterStrategy Instance = new AllFieldsAllowedAccountingTransactionWriterStrategy();

		public bool IsAllowSetForAccountingContext(string fieldName) => true;
	}

	[Immutable]
	public sealed class FieldListDisallowedAccountingTransactionWriterStrategy : IAccountingTransactionWriterStrategy
	{
		public FieldListDisallowedAccountingTransactionWriterStrategy(IReadOnlyCollection<string> disallowedFields)
		{
			DisallowedFields = (disallowedFields ?? Array.Empty<string>()).ToImmutableHashSet();
		}

		public FieldListDisallowedAccountingTransactionWriterStrategy(ImmutableHashSet<string> disallowedFields)
		{
			DisallowedFields = disallowedFields;
		}

		public ImmutableHashSet<string> DisallowedFields { get; }

		public bool IsAllowSetForAccountingContext(string fieldName)
			=> !DisallowedFields.Contains(fieldName);
	}
}
