using System;
using CargoWise.Data;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	public abstract class ValueProviderWithLoadControlFactory : ValueProvider
	{
		public ValueProviderWithLoadControlFactory()
		{
		}

		internal void ReplaceFactoryIfRequired()
		{
			if (ShouldResetFactory && NumberOfObjectsInFactory > MaxNumberOfObjectsCanBeHeldByFactory)
			{
				ForceReplaceFactory();
			}
		}

		protected void ForceReplaceFactory()
		{
			factory = null;
		}

		protected override void PerformPreReplacementSetup()
		{
			base.PerformPreReplacementSetup();
			ReplaceFactoryIfRequired();
		}

		internal int NumberOfObjectsInFactory
		{
			get
			{
				using (Db.DisposableActionForDbConnection())
				{
					return ((IBusinessObjectFactoryInternals)Factory).NumberOfBusinessObjects;
				}
			}
		}

		internal int MaxNumberOfObjectsCanBeHeldByFactory
		{
			get;
#if DEBUG
			set;
#endif
		} = 100;

		protected internal virtual bool ShouldResetFactory => true;

		protected BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory() { NameForDebugging = "ValueProvider : " + GetType().FullName }); }
		}

		[ThreadStatic]
		static BusinessObjectFactory factory;

#if DEBUG
		public BusinessObjectFactory FactoryForTesting
		{
			get { return Factory; }
		}

		internal static void ResetFactoryForTesting() => factory = null;
#endif

	}
}
