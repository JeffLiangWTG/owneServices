using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.Modules
{
	public sealed class ZWebModuleFactory : IDCountryFactory<ModuleIdentifier, ModuleInfo, WebModuleList>
	{
		ZWebModuleFactory()
		{
		}

		static readonly ZWebModuleFactory Instance = new ZWebModuleFactory();

		#region Internal Properties

		internal WebModuleList RegistrationListInternal => RegistrationList;
		internal static ZWebModuleFactory InstanceInternal => Instance;

		#endregion

		/// <summary>
		/// You must Dispose any module that you create.
		/// </summary>
		public static ZWebModule Create(WebModuleID iD, BusinessObjectFactory factory)
		{
			return (ZWebModule)Instance.CreateNewWithCountry(iD, "", new object[] { factory });   //none of web modules are country specific -- so we don`t use CreateNew() here
		}

		public static ZFilterGridModule Create(WebModuleID iD, BusinessObjectFactory factory, ZPage page)
		{
			return (ZFilterGridModule)Instance.CreateNewWithCountry(iD, "", new object[] { factory, page });  //none of web modules are country specific -- so we don`t use CreateNew() here
		}

		public static WebModuleID GetWebModuleIDByName(string name)
		{
			return (WebModuleID)Instance.GetRegisteredIdentifierByName(name);
		}
	}
}
