using System;
using CargoWise.Common;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.UniversalCopy.GUI
{
	public class UniversalCopySecurity
	{
		ZModule module;

		public UniversalCopySecurity(ModuleIdentifier moduleId)
		{
			using (var module = ZModuleFactory.Instance.Create(moduleId))
			{
				Initialize(module);
			}
		}

		public UniversalCopySecurity(ZModule module)
		{
			Initialize(module);
		}

		void Initialize(ZModule module)
		{
			this.module = module;
		}

		public ISecurityCheckpoint UniversalCopyCheckpoint
		{
			get
			{
				try
				{
					return module == null || module.SecurityCheckpoint == null || module.SecurityCheckpoint == EnvProxy.Instance.Security.None
						? null
						: EnvProxy.Instance.Security.FindOrCreateUniversalCopyCheckpoint(module.SecurityCheckpoint);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					return null;
				}
			}
		}

		public ISecurityCheckpoint UniversalCopyRunCheckpoint
		{
			get
			{
				return UniversalCopyCheckpoint != null ? EnvProxy.Instance.Security.FindOrCreateUniversalCopyRunCheckpoint(UniversalCopyCheckpoint) : null;
			}
		}

		public ISecurityCheckpoint UniversalCopyCEDPrivateCheckpoint
		{
			get { return UniversalCopyCheckpoint != null ? EnvProxy.Instance.Security.FindOrCreateUniversalCopyCEDPrivateCheckpoint(UniversalCopyCheckpoint) : null; }
		}

		public ISecurityCheckpoint UniversalCopyEditPublishCheckpoint
		{
			get { return UniversalCopyCheckpoint != null ? EnvProxy.Instance.Security.FindOrCreateUniversalCopyEditPublishCheckpoint(UniversalCopyCheckpoint) : null; }
		}

		public ISecurityCheckpoint UniversalCopyDeletePublishCheckpoint
		{
			get { return UniversalCopyCheckpoint != null ? EnvProxy.Instance.Security.FindOrCreateUniversalCopyDeletePublishCheckpoint(UniversalCopyCheckpoint) : null; }
		}

		public bool CanRun
		{
			get { return UniversalCopyRunCheckpoint != null && UniversalCopyRunCheckpoint.IsAllowed; }
		}

		public bool CanCreateEditDeletePrivate
		{
			get { return UniversalCopyCEDPrivateCheckpoint != null && UniversalCopyCEDPrivateCheckpoint.IsAllowed; }
		}

		public bool CanEditPublic
		{
			get { return UniversalCopyEditPublishCheckpoint != null && UniversalCopyEditPublishCheckpoint.IsAllowed; }
		}

		public bool CanDeletePublic
		{
			get { return UniversalCopyDeletePublishCheckpoint != null && UniversalCopyDeletePublishCheckpoint.IsAllowed; }
		}
	}
}
