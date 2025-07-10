using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	public static class EnterpriseBusinessObjectFactoryExtensions
	{
		public static CodeDescriptionPairList GetCachedCodeDescriptionPairList(this BusinessObjectFactory factory, OLookUpEditType type)
		{
			return factory.GetCachedValue(type.ToString(), new GetValueDelegate<CodeDescriptionPairList>(delegate
			{
				return new CodeDescriptionPairList(type);
			}));
		}

		public static CodeDescriptionPairListHolder GetCodeDescriptionPairListHolder(this BusinessObjectFactory factory)
		{
			CodeDescriptionPairListHolder result = factory.ServiceContainer.GetService<CodeDescriptionPairListHolder>();
			if (result == null)
			{
				result = new CodeDescriptionPairListHolder();
				factory.ServiceContainer.AddService(result);
			}
			return result;
		}

		public static void AddInTransactionAction(this BusinessObjectFactory factory, Action action)
		{
			InTransactionAction.SubscribeSave(factory, action);
		}

		sealed class InTransactionAction : SaveInTransactionAction
		{
			internal static void SubscribeSave(BusinessObjectFactory parentFactory, Action inTransactionAction)
			{
				parentFactory.SaveInTransactionActions.Add(new InTransactionAction(parentFactory, inTransactionAction));
			}

			InTransactionAction(BusinessObjectFactory parentFactory, Action inTransactionAction)
			{
				this.parentFactory = parentFactory;

				bool hasRecursed = false;
				this.inTransactionAction = () =>
				{
					if (hasRecursed)
					{
						throw new InvalidOperationException("Recursion is not supported");
					}

					try
					{
						hasRecursed = true;
						inTransactionAction.Invoke();
					}
					finally
					{
						hasRecursed = false;
					}
				};
			}

			readonly BusinessObjectFactory parentFactory;
			readonly Action inTransactionAction;

			protected override bool IsInTransaction => parentFactory.IsInTransaction;

			protected override bool AllowTransactionWithOtherParticipant => true;

			protected override ITransactionManager BeginTransactionWithManager() => new NullTransaction();

			protected override IChangedTableNames SaveInTransaction()
			{
				inTransactionAction.Invoke();
				return new ChangedTableNames(Array.Empty<string>());
			}

			#region Null Transaction Impl

			sealed class NullTransaction : ITransactionManager
			{
				void ITransactionManager.CommitTransaction() { }
				void ITransactionManager.RollbackTransaction() { }
				void IDisposable.Dispose() { }
			}

			#endregion
		}
	}
}
