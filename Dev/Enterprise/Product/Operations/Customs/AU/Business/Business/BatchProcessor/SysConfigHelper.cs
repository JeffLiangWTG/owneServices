using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SysConfigHelper
	{
		#region Construction
		public static SysConfigHelper Instance
		{
			get { return instance ?? (instance = new SysConfigHelper()); }
		}
		[ThreadStatic]
		static SysConfigHelper instance;

		SysConfigHelper()
		{
		}
		#endregion

		#region CustomsCCFEmailAddress

		public ZString AUCCustomsCCFCurrentEmailAddress => RefSysConfigLoader.GetStringValue(AUConstants.RefSysConfigCodes.CustomsCCFCurrentEmailAddress, ZDateTime.Today);

		public ZString AUCCustomsCCFPreviousEmailAddress => RefSysConfigLoader.GetStringValue(AUConstants.RefSysConfigCodes.CustomsCCFPreviousEmailAddress, ZDateTime.Today);

		#endregion

		#region Implementation

		Universal.RefSysConfig.Loader RefSysConfigLoader
		{
			get
			{
				if (refSysConfigLoader == null)
				{
					refSysConfigLoader = new Universal.RefSysConfig.Loader(Factory);
				}
				return refSysConfigLoader;
			}
		}
		Universal.RefSysConfig.Loader refSysConfigLoader;

		BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = new BusinessObjectFactory();
				}
				return fFactory;
			}
		}
		BusinessObjectFactory fFactory;

		#endregion
	}
}
