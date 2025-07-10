using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Integration;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.EntityFramework
{
	public sealed class FactorySaveAlerter : Disposable, ITransactionParticipantListener
	{
		[ThreadSafe]
		static readonly ThreadLocal<bool> overrideSaveReporter = new ThreadLocal<bool>();
		readonly Func<string> context;
		readonly Action<ITransactionParticipant[]> onSavingStarted;
		readonly string errorMessage;

		public FactorySaveAlerter(Func<string> context, string errorMessage)
		{
			BusinessObjectFactory.RegisterListener(this);
			this.context = context;
			this.errorMessage = errorMessage;
		}

		public FactorySaveAlerter(Action<ITransactionParticipant[]> onSavingStarted)
		{
			BusinessObjectFactory.RegisterListener(this);
			this.onSavingStarted = onSavingStarted;
		}

		void ITransactionParticipantListener.FactorySaveBeginning(ITransactionParticipant[] factories)
		{
			if (onSavingStarted != null)
			{
				onSavingStarted(factories);
			}
			else if (!overrideSaveReporter.Value)
			{
				ErrorReporter.ReportOnce(FormattableString.Invariant($"Save in [{context.Invoke()}]"), errorMessage);
			}
		}

		void ITransactionParticipantListener.FactorySaveCompleted(ITransactionParticipant[] factories, bool successful)
		{
			// Don't care.
		}

		protected override void Dispose(bool isDisposing) => BusinessObjectFactory.UnRegisterListener(this);

		internal static IDisposable TemporarilyOverride()
		{
			return new Override();
		}

		sealed class Override : IDisposable
		{
			public Override()
			{
				DisposableLeakListener.Instance.RegisterDisposable(this, true);
				overrideSaveReporter.Value = true;
			}

			public void Dispose()
			{
				overrideSaveReporter.Value = false;
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
			}
		}
	}
}
