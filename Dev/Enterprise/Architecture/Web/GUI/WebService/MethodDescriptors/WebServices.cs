using System;
using System.Web.UI;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI;

#if DEBUG

using Enterprise.ZArchitecture.Web.GUI.Testing;

#endif

namespace Enterprise.ZArchitecture.Web.ServerServices
{
	public sealed class WebServices
	{
		#region Constructors

		WebServices()
		{
		}

		#endregion

		#region Instance

		public static WebServices Instance
		{
			get
			{
				lock (StaticLock)
				{
					if (instance == null)
					{
						instance = new WebServices();
					}
					return instance;
				}
			}
		}

		[ThreadStatic]
		static WebServices instance;

		#endregion

		#region Properties

		public ServiceReference SharedWebServiceReference
		{
			get
			{
				lock (InstanceLock)
				{
					if (sharedWebServiceReference == null)
					{
						ZGlobal appInstance = WebEnv.AppInstance as ZGlobal;
#if DEBUG
						if (Enterprise.ZArchitecture.Environment.Globals.IsTest)
						{
							appInstance = GetNewTestGlobal();
						}
#endif
						sharedWebServiceReference = new ServiceReference(appInstance.ApplicationRoot + "WebService/WebServiceShared.asmx");
					}
					return sharedWebServiceReference;
				}
			}
		}

		ServiceReference sharedWebServiceReference;

		#endregion

		#region Implementation

#if DEBUG

		ZGlobal GetNewTestGlobal()
		{
			return new ZTestGlobal();
		}

#endif

		readonly static object StaticLock = new object();
		readonly object InstanceLock = new object();

		#endregion
	}
}
