using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Licensing;
using Enterprise.Security;

namespace Enterprise.Services.OperationalActions.Support
{
	[System.Diagnostics.DebuggerDisplay("Name = {Name}")]
	public abstract class OperationalActionMethod
	{
		protected OperationalActionMethod(ZGuid methodID)
		{
			this.methodID = methodID;
		}

		public ZGuid MethodID
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return methodID; }
		}

		public abstract string Name { get; }
		public abstract string Description { get; }

		public virtual bool IsRunAgainDisabled
		{
			get { return false; }
		}

		public virtual bool HasControl
		{
			get { return false; }
		}
		public virtual IComponent NewGuiControl()
		{
			throw new NotSupportedException();
		}
		public abstract OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings);

		public virtual bool HasSettings
		{
			get { return false; }
		}
		public virtual IComponent NewSettingsControl()
		{
			throw new NotSupportedException();
		}
		public virtual OperationalActionMethodSettings NewSetting(BusinessObjectFactory factory)
		{
			throw new NotSupportedException();
		}

		public virtual SecurityCheckpoint[] GetRequiredSecurityCheckpoints()
		{
			return Array.Empty<SecurityCheckpoint>();
		}
		public virtual LicenceCheckpoint[] GetRequiredLicenceCheckpoints()
		{
			return Array.Empty<LicenceCheckpoint>();
		}

		public virtual FilterRequirementList GetFilterRequirements()
		{
			return new FilterRequirementList();
		}

		public virtual bool RunWithoutUI
		{
			get { return false; }
		}

		public virtual bool RunWithoutResultLogging
		{
			get { return false; }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly ZGuid methodID;
	}
}
