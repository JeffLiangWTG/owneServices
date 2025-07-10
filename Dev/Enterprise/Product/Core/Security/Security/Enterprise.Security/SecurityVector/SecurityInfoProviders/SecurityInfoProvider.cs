using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common.Enumeration;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Security.Provider
{
	abstract class SecurityInfoProvider
	{
		public SecurityInfoProvider(SecurityInfoProvider parent, ISecurityCheckpoint checkpoint)
		{
			this.parent = parent;
			this.root = new Lazy<SecurityInfoProvider>(() => ZEnumerable.Iterate(parent, p => p.parent).First(p => p == null || p.IsRoot));
			Checkpoint = checkpoint;
		}

		public virtual bool IsRoot { get { return false; } }
		public ISecurityCheckpoint Checkpoint { get; private set; }
		public abstract string Name { get; }
		public abstract IEnumerable<SecurityInfoProvider> GetChildren();

		readonly Lazy<SecurityInfoProvider> root;
		SecurityInfoProvider Root { get { return root.Value; } }

		public virtual SecurityCore Security
		{
			get { return Root == null ? null : Root.Security; }
		}

		protected virtual BusinessObjectFactory Factory
		{
			get { return Root == null ? null : Root.Factory; }
		}

		protected static SecurityCheckpoint GetSecurityCheckpoint(SecurityCore security, ISecurityCheckpoint checkpoint)
		{
			SecurityCheckpoint result = security.FindCheckPoint(checkpoint.LookupKey);
			if (result != null && checkpoint != null && result.DisplayText.GetUnresolvedString() != checkpoint.DisplayText.GetUnresolvedString())
			{
				result.SetDisplayText(checkpoint.DisplayText);
			}
			return result;
		}

		readonly SecurityInfoProvider parent;

		public virtual void FetchForGetChildren() { }
	}
}
