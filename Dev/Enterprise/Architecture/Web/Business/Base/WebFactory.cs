using System;
#if NETFRAMEWORK
using System.Web;
#elif NET
using Microsoft.AspNetCore.Http;
#endif
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Web.Business
{
	/// <summary>
	/// BusinessObjectFactory for the WebUser.
	/// Doesn't keep a strong reference to the factory between requests.
	/// Keeps the same factory for the duration of a request.
	/// </summary>
	internal sealed class WebFactory : IFactoryProvider
	{
		internal WebFactory(BusinessObjectFactory factory = null)
		{
			factory = factory ?? new BusinessObjectFactory() { NameForDebugging = "WebFactory" };
			SetRequestFactory(factory);
			factoryRef = new WeakReference<BusinessObjectFactory>(factory);
		}

		readonly WeakReference<BusinessObjectFactory> factoryRef;

		internal BusinessObjectFactory TryGetFactory()
		{
			if (factoryRef.TryGetTarget(out var result))
			{
				return result;
			}
			return null;
		}

		public BusinessObjectFactory Factory
		{
			get
			{
				var result = TryGetFactory();
				if (result == null)
				{
					result = GetOrCreatePerRequestFactory();
					factoryRef.SetTarget(result);
				}

				return result;
			}
		}

		/// <summary>
		/// If there is a HttpContext:
		///		If there is no per-request factory yet, and a factory is given, then it becomes the per request factory.
		///		If there is a per request factory, return that in preference to the given factory.
		/// If there is no HttpContext:
		///		Returns the given factory if provided, otherwise a new factory every time.
		/// </summary>
		public static BusinessObjectFactory GetOrCreatePerRequestFactory(BusinessObjectFactory factory = null)
		{
			BusinessObjectFactory result = null;
			var httpContext = GetHttpContext();
			if (httpContext != null)
			{
				result = (BusinessObjectFactory)httpContext.Items[FactoryKey];
				if (result == null)
				{
					result = factory ?? new BusinessObjectFactory() { NameForDebugging = "WebFactory" };
					SetRequestFactory(result);
				}
			}
			else
			{
				result = factory ?? new BusinessObjectFactory();
			}

			return result;
		}

		static void SetRequestFactory(BusinessObjectFactory factory)
		{
			var httpContext = GetHttpContext();
			if (httpContext != null)
			{
				httpContext.Items[FactoryKey] = factory;
			}
		}

		internal const string FactoryKey = "WebFactory";

#if DEBUG
		public void DiscardAnyFactoryReferenceForTest()
		{
			factoryRef.SetTarget(null);
		}
#endif

		static HttpContext GetHttpContext()
		{
#if NETFRAMEWORK
			return HttpContext.Current;
#elif NET
			return WebEnv.HttpContextAccessor?.HttpContext;
#endif
		}
	}
}
