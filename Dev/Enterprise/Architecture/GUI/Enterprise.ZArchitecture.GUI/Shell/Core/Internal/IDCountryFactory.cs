using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using CargoWise.Common;
using Enterprise.ZArchitecture.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.Modules
{
	class IDCountryFactoryWeakObserver
	{
		public IDCountryFactoryWeakObserver(WeakReference<IDCountryFactoryBase> weakref)
		{
			this.weakref = weakref;
			ClientHookLoader.Instance.ClientHookChanged += OnClientHookChanged;
		}

		void OnClientHookChanged(object o, EventArgs e)
		{
			IDCountryFactoryBase value = null;
			if (this.weakref.TryGetTarget(out value))
			{
				value.ClearRegistrationList();
			}
			else
			{
				ClientHookLoader.Instance.ClientHookChanged -= OnClientHookChanged;
			}
		}

		readonly WeakReference<IDCountryFactoryBase> weakref;
	}

	public abstract class IDCountryFactoryBase
	{
		protected IDCountryFactoryBase()
		{
			new IDCountryFactoryWeakObserver(new WeakReference<IDCountryFactoryBase>(this));
			var counter_local = Interlocked.Increment(ref counter);
			if (counter_local == 10000)
			{
				ErrorReporter.ReportOnce("TooManyIDCountryFactories", string.Format(CultureInfo.InvariantCulture,
					"10000 IDCountryFactories is probably more than any CW1 instance needs. Let's see if we can make less. Stack trace: {0}",
					new System.Diagnostics.StackTrace()));
			}
		}

		protected virtual internal void ClearRegistrationList()
		{
		}

		[ThreadSafe]
		static int counter = 0;
	}

	[ImmutableObject(true)]
	public abstract class IDCountryFactory<TRegistrationIdentifier, TRegistrationInfo, TRegistrationList> : IDCountryFactoryBase
		where TRegistrationIdentifier : RegistrationIdentifier
		where TRegistrationInfo : RegistrationInfo
		where TRegistrationList : RegistrationList<TRegistrationIdentifier, TRegistrationInfo>, new()
	{
		protected IDCountryFactory() : base()
		{
		}

		protected internal object CreateNew(TRegistrationIdentifier iD, object[] @params)
		{
			return CreateNewWithCountry(iD, CountryCode, @params);
		}

		protected internal object CreateNewWithCountry(TRegistrationIdentifier iD, string countryCode)
		{
			return CreateNewWithCountry(iD, countryCode, Array.Empty<object>());
		}

		protected internal object CreateNewWithCountry(TRegistrationIdentifier iD, string countryCode, object[] @params)
		{
			if (iD == null)
			{
				throw new ModuleIDIsNullException("Cannot create Module, because ModuleID is null");
			}

			if (iD == ModuleIDs.NotAssigned)
			{
				throw new ModuleNotAssignedIDException("Cannot create Module from ModuleID: " + iD);
			}

			object result = null;

			var info = RegistrationList[iD, countryCode];

			if (info != null)
			{
				var createType = Type.GetType(info.TypePath, true);

				result = Activator.CreateInstance(createType, @params);

				if (result != null && result is IModuleCountryAcceptor)
				{
					((IModuleCountryAcceptor)result).CountryCode = countryCode;
				}
			}

			return result;
		}

		protected internal object CreateNew(TRegistrationIdentifier iD)
		{
			return CreateNew(iD, Array.Empty<object>());
		}

		protected string[] GetCountryOverridesRegistered(TRegistrationIdentifier iD)
		{
			return RegistrationList.GetCountryOverridesRegistered(iD);
		}

		#region Implementation

		public TRegistrationIdentifier GetRegisteredIdentifierByName(string identifier)
		{
			return RegistrationList.GetRegisteredIdentifierByName(identifier);
		}

		protected TRegistrationList RegistrationList
		{
			get
			{
				if (registrationList == null)
				{
					registrationList = new TRegistrationList();
				}
				return registrationList;
			}
		}

		[ThreadSafe]
		internal static TRegistrationList registrationList = new TRegistrationList();

		protected override internal void ClearRegistrationList()
		{
			registrationList = null;
		}

		protected string CountryCode
		{
			get
			{
				// this has been structured like this to help track down an exception coming from the web.

				var instance = StaticCurrentFetcher.Instance
					?? throw new InvalidOperationException("StaticCurrentFetcher.Instance is null.");

				var company = instance.CurrentCompany
					?? throw new InvalidOperationException("StaticCurrentFetcher.Instance.CurrentCompany is null.");

				var countryCode = company.GC_RN_NKCountryCode;
				if (countryCode.IsEmpty)
				{
					throw new InvalidOperationException("StaticCurrentFetcher.Instance.CurrentCompany.GC_RN_NKCountryCode is empty.");
				}

				return countryCode;
			}
		}

		#endregion
	}
}
