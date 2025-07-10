using System;

namespace Enterprise.ZArchitecture.Modules
{
	[WTG.StaticAnalysis.Annotation.Immutable]
	public class ControllerID : RegistrationIdentifier
	{
		public ControllerID(string iD) : base(iD)
		{
		}

		public override bool Equals(object obj)
		{
			return obj is ControllerID && base.Equals(obj);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required for Equals override")]
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}

	[WTG.StaticAnalysis.Annotation.Immutable]
	public class ClientControllerID : ControllerID
	{
		public ClientControllerID(string iD) : base(iD)
		{
		}
	}

	[WTG.StaticAnalysis.Annotation.Immutable]
	public class ClientOverrideControllerID : ControllerID
	{
		public ClientOverrideControllerID(ControllerID controllerIDToOverride) : base(controllerIDToOverride.ToString())
		{
			OverridenControllerID = controllerIDToOverride;
		}

		public readonly ControllerID OverridenControllerID;
	}

	public class ControllerInfo : RegistrationInfo
	{
		public ControllerInfo(ControllerID iD, Type controllerType) : base(iD, controllerType)
		{
		}

		public ControllerInfo(ControllerID iD, Type controllerType, string countryCode) : base(iD, controllerType, countryCode)
		{
		}

		public ControllerInfo(ControllerID iD, string controllerAssemblyName, string controllerClassFullName) : base(iD, controllerAssemblyName, controllerClassFullName, null)
		{
		}

		public ControllerInfo(ControllerID iD, string controllerAssemblyName, string controllerClassFullName, string countryCode) : base(iD, controllerAssemblyName, controllerClassFullName, countryCode)
		{
		}

		public new ControllerID ID
		{
			get { return (ControllerID)base.ID; }
		}
	}

	public class ClientOverrideControllerInfo : ControllerInfo
	{
		public ClientOverrideControllerInfo(ClientOverrideControllerID iD, Type controllerType) : base(iD, controllerType)
		{
		}

		public ClientOverrideControllerInfo(ClientOverrideControllerID iD, Type controllerType, string countryCode) : base(iD, controllerType, countryCode)
		{
		}

		public ClientOverrideControllerInfo(ClientOverrideControllerID iD, string controllerAssemblyName, string controllerClassFullName) : base(iD, controllerAssemblyName, controllerClassFullName, null)
		{
		}

		public ClientOverrideControllerInfo(ClientOverrideControllerID iD, string controllerAssemblyName, string controllerClassFullName, string countryCode) : base(iD, controllerAssemblyName, controllerClassFullName, countryCode)
		{
		}

		public ClientOverrideControllerInfo(ClientOverrideControllerID iD, string controllerAssemblyName, string controllerClassFullName, bool canOverrideForClientWithNoCountryWhenOtherCountryOverridesExist) : this(iD, controllerAssemblyName, controllerClassFullName)
		{
			fCanOverrideForClientWithNoCountryWhenOtherCountryOverridesExist = canOverrideForClientWithNoCountryWhenOtherCountryOverridesExist;
		}

		public override bool IsClientOverride
		{
			get { return true; }
		}

		public sealed override bool CanOverrideForClientWithNoCountryWhenOtherCountryOverridesExist
		{
			get { return fCanOverrideForClientWithNoCountryWhenOtherCountryOverridesExist; }
		}
		readonly bool fCanOverrideForClientWithNoCountryWhenOtherCountryOverridesExist;
	}
}
