using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using Enterprise.Core.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using WTG.SecurityRights;
using Res = Enterprise.Security.Core.Res;

namespace Enterprise.Security
{
	/// <summary>
	/// Use this CheckPoint wrapper if there's a need to either display a proper text in staff/group form or if there's functional hierachy required.
	/// </summary>
	public class SecurityCheckpoint : ISecurityCheckpoint, ICheckpoint
	{
		public SecurityCheckpoint(string code, MultilingualString displayText, ISecurityCheckpoint parent, IZSecurity security, string country)
			: this(code, displayText, parent, security, true, country, Guid.Empty)
		{
		}

		public SecurityCheckpoint(string code, MultilingualString displayText, ISecurityCheckpoint parent, IZSecurity security, Guid itemGuid)
			: this(code, displayText, parent, security, true, string.Empty, itemGuid)
		{
		}

		public SecurityCheckpoint(string code, MultilingualString displayText, ISecurityCheckpoint parent, IZSecurity security)
			: this(code, displayText, parent, security, true, string.Empty, Guid.Empty)
		{
		}

		public SecurityCheckpoint(string code, MultilingualString displayText, ISecurityCheckpoint parent, IZSecurity security, bool addToLookUpTable)
			: this(code, displayText, parent, security, addToLookUpTable, string.Empty, Guid.Empty)
		{
		}

		public SecurityCheckpoint(string code, MultilingualString displayText, ISecurityCheckpoint parent, IZSecurity security, bool addToLookUpTable, string country, Guid itemGuid)
		{
			if (code != null && code.Length > GlbSecuritySchema.GU_SecurityRight.MaxLength)
			{
				throw new ArgumentException("The Code of a security checkpoint must have a maximum length of " + GlbSecuritySchema.GU_SecurityRight.MaxLength + " characters. [" + code + "] is invalid.");
			}

			fCountry = country;
			fCode = code;
			fDisplayText = displayText;
			fSecurity = security;
			fItemGuid = itemGuid;

			if (addToLookUpTable)
			{
				if (!security.AddCheckPoint(LookupKey, this))
				{
					Globals.Message.ShowDeveloperErrorOnce("SecurityCheckpointInstantiatedTwice_" + code, "The security checkpoint " + code + " with ItemGuid '" + itemGuid.ToString() + "' has been instantiated twice.", "Duplicate Security Checkpoint");
				}
			}

			if (parent != null)
			{
				parent.AddChild(this);
			}
		}

		public ISecurityCheckpoint FindChild(string childCode)
		{
			return ChildCheckPoints.FirstOrDefault(x => x.Code == childCode);
		}

		#region Properties

		public string DefaultSecurityOverrideMessage
		{
			get { return Res.GetString("3eb2c58d-1f9e-4889-b62f-36f88a21d86b", "To override this security, a user with security rights to [{0}] must login. Please enter username and password details below.", DisplayText); }
		}

		public SecurityCheckpoint Parent
		{
			get { return fParent; }
		}

		public string Code
		{
			get { return fCode; }
		}

		public Guid ItemGuid
		{
			get { return fItemGuid; }
		}

		public ZArchitecture.Modules.CheckpointLookupKey LookupKey
		{
			get { return new ZArchitecture.Modules.CheckpointLookupKey(fCode, fItemGuid); }
		}

		public MultilingualString DisplayText
		{
			get
			{
				MultilingualString result = (NoResString)string.Empty;

				if (fDisplayText != null)
				{
					result = fDisplayText;
				}

				if (!string.IsNullOrEmpty(fDisplayTextOverride.Value))
				{
					result = fDisplayTextOverride.Value;
				}

				return result;
			}
		}

		public IEnumerable<SecurityCheckpoint> ChildCheckPoints
		{
			get { return fChildCheckPoints ?? Enumerable.Empty<SecurityCheckpoint>(); }
		}

		bool visible = true;

		public virtual bool Visible
		{
			get { return visible && (string.IsNullOrEmpty(Country) || (Country == EnvProxy.Instance.CurrentCompany.Country.Code)); }
		}

		public void Hide()
		{
			visible = false;
		}

		public string Country
		{
			get { return fCountry; }
#if DEBUG
			set { fCountry = value; }
#endif
		}

		#endregion

		#region Implementation

		List<SecurityCheckpoint> fChildCheckPoints;
		SecurityCheckpoint fParent;
		readonly Guid fItemGuid;
		string fCountry;
		readonly string fCode;
		internal MultilingualString fDisplayText;
		internal readonly Overridable<MultilingualString> fDisplayTextOverride = new Overridable<MultilingualString>(null);
		readonly IZSecurity fSecurity;
		bool? fIsAllowed;

		protected IZSecurity Security
		{
			get { return fSecurity; }
		}

		public bool IsAncestorOf(SecurityCheckpoint checkPoint)
		{
			if (ChildCheckPoints != null)
			{
				foreach (SecurityCheckpoint child in ChildCheckPoints)
				{
					if (checkPoint == child || child.IsAncestorOf(checkPoint))
					{
						return true;
					}
				}
			}
			return false;
		}

		protected virtual internal Enterprise.Core.Environment.SecurityState IsStaffAllowed
		{
			get
			{
				var result = Security.IsStaffAllowed(this);
				if ((result == Enterprise.Core.Environment.SecurityState.Implicit) && (Parent != null))
				{
					result = Parent.IsStaffAllowed;
				}
				return result;
			}
		}

		protected virtual internal Enterprise.Core.Environment.SecurityState IsGroupAllowed
		{
			get
			{
				var result = Security.IsGroupAllowed(this);
				if ((result == Enterprise.Core.Environment.SecurityState.Implicit) && (Parent != null))
				{
					result = Parent.IsGroupAllowed;
				}
				return result;
			}
		}

		public override string ToString()
		{
			string result = Code;
			SecurityCheckpoint root = Parent;

			while (root != null)
			{
				result = root.Code + "." + result;
				root = root.Parent;
			}
			return result;
		}

		public MultilingualString DisplayTextPathToSecurityRight
		{
			get
			{
				MultilingualString result = DisplayText;
				SecurityCheckpoint root = Parent;

				while (root != null)
				{
					if (!root.DisplayText.IsEmpty)
					{
						result = MultilingualString.Join(" -> ", root.DisplayText, result);
					}
					root = root.Parent;
				}
				return result;
			}
		}

		public void SetDisplayText(MultilingualString displayText)
		{
			fDisplayTextOverride.Value = displayText;
		}

		public virtual MultilingualString HumanReadableName
		{
			get
			{
				MultilingualString result = DisplayText;
				if ((result.Equals(Res.GetString("13285d5d-e4db-4114-81cb-2f11e8ca8704", "Edit"))
					|| result.Equals(Res.GetString("75b0879f-a5d6-4045-907c-3e0fb6828cd1", "View"))
					|| result.Equals(Res.GetString("485be17f-bd4a-4826-bd04-2d157005dc43", "Modify"))
					|| result.Equals(Res.GetString("a0853312-a8dc-4e2f-8902-f10f35ec5e74", "New"))
					|| result.Equals(Res.GetString("2eca1bb2-8063-47a3-a1a9-b67ea0de8da5", "Print"))
					|| result.Equals(Res.GetString("ddbf8f38-d89e-416b-9a49-7eb18775bbdd", "Delete"))
					|| result.Equals(Res.GetString("894e55cf-b21e-40bb-954b-c99e2ef58014", "Reports")))
					&& Parent != null)
				{
					result = MultilingualString.Join(" -> ", Parent.DisplayText, result);
				}

				return result;
			}
		}

		public virtual void AddChild(SecurityCheckpoint child)
		{
			if (fChildCheckPoints == null)
			{
				fChildCheckPoints = new List<SecurityCheckpoint>();
			}

			SetParentOnChild(child);
			fChildCheckPoints.Add(child);
		}

		protected virtual void SetParentOnChild(SecurityCheckpoint child)
		{
			child.fParent = this;
		}

		public void SortAlphabeticallyRecursively(bool categoriesAtEnd)
		{
			if (fChildCheckPoints != null)
			{
				if (categoriesAtEnd)
				{
					fChildCheckPoints.Sort((x, y) =>
					{
						if (x.ChildCheckPoints.Any() == y.ChildCheckPoints.Any())
						{
							return x.DisplayText.CompareTo(y.DisplayText);
						}
						else
						{
							return x.ChildCheckPoints.Any() ? 1 : -1;
						}
					});
				}
				else
				{
					fChildCheckPoints.Sort((x, y) => x.DisplayText.CompareTo(y.DisplayText));
				}
				foreach (var child in fChildCheckPoints)
				{
					child.SortAlphabeticallyRecursively(categoriesAtEnd);
				}
			}
		}

		#region Test Only
#if DEBUG

		public void ClearOverriddenSecurityValue()
		{
			OverriddenSecurityValues = OverriddenSecurityValues.Remove(this);
		}

		static ImmutableDictionary<SecurityCheckpoint, bool> OverriddenSecurityValues
		{
			get { return overriddenSecurityValues.Value ?? (overriddenSecurityValues.Value = ImmutableDictionary<SecurityCheckpoint, bool>.Empty); }
			set { overriddenSecurityValues.Value = value; }
		}
		static readonly ThreadLocalOverridable<ImmutableDictionary<SecurityCheckpoint, bool>> overriddenSecurityValues = new ThreadLocalOverridable<ImmutableDictionary<SecurityCheckpoint, bool>>();

#endif
		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly", Justification = "Used only by a debugging tool")]
		public delegate void EventCheckpointChecked(SecurityCheckpoint checkPoint, bool isAllowed, bool wasCached);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "Used only by a debugging tool")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly", Justification = "Used only by a debugging tool")]
		public static event EventCheckpointChecked CheckpointChecked;

		public virtual bool IsAllowed
		{
			get
			{
				Security?.ThreadSentry?.EnsureCurrentThreadIsOwner();

#if DEBUG
				bool overriddenResult;
				if (OverriddenSecurityValues.TryGetValue(this, out overriddenResult))
				{
					return overriddenResult;
				}
#endif
				bool wasCached = true;

				if (!Security.CachingEnabled || fIsAllowed == null)
				{
					wasCached = false;
					bool result = true;

					var staffState = IsStaffAllowed;

					if (staffState == Enterprise.Core.Environment.SecurityState.Denied)
					{
						result = false;
					}
					else if (staffState == Enterprise.Core.Environment.SecurityState.Granted)
					{
						result = true;
					}
					else if (staffState == Enterprise.Core.Environment.SecurityState.Implicit)
					{
						var groupState = IsGroupAllowed;
						result = (groupState == Enterprise.Core.Environment.SecurityState.Granted) || (groupState == Enterprise.Core.Environment.SecurityState.Implicit);
					}

					fIsAllowed = result;
				}

				CheckpointChecked?.Invoke(this, (bool)fIsAllowed, wasCached);

				return (bool)fIsAllowed;
			}
#if DEBUG
			set
			{
				OverriddenSecurityValues = OverriddenSecurityValues.SetItem(this, value);
			}
#endif
		}

		#region IsAllowedForAllBranches

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0030:Use coalesce expression", Justification = "Condition only for testing")]
		public virtual bool IsAllowedForAllBranches
		{
			get
			{
				return
#if DEBUG
 overridenIsAlloweForAllBranches.HasValue ? overridenIsAlloweForAllBranches.Value :
#endif
 Security.IsSecurityAllowedForAllBranches(this);
			}
#if DEBUG
			set
			{
				overridenIsAlloweForAllBranches = value;
			}
#endif
		}

#if DEBUG
		bool? overridenIsAlloweForAllBranches;
#endif

		#endregion

		public void ClearIsAllowedCache()
		{
			fIsAllowed = null;
		}

		public virtual void ShowError()
		{
			Security.ShowError(this);
		}

		public virtual MultilingualString ErrorMessageForNotAllowed
		{
			get { return Security.GetErrorMessageForNotAllowed(this); }
		}

		#region Test Only
#if DEBUG

		internal void Unload()
		{
			foreach (SecurityCheckpoint checkpoint in ChildCheckPoints)
			{
				checkpoint.Unload();
				checkpoint.fParent = null;
			}
			fChildCheckPoints = null;

			Security.CheckPointLookUpTable_ForTest.Remove(LookupKey);
		}

#endif
		#endregion

		#endregion

		#region ISecurityCheckpoint Members

		IEnumerable<ISecurityCheckpoint> ISecurityCheckpoint.ChildCheckPoints
		{
			get { return ChildCheckPoints; }
		}

		void ISecurityCheckpoint.AddChild(ISecurityCheckpoint child)
		{
			AddChild((SecurityCheckpoint)child);
		}

		bool ISecurityCheckpoint.IsAncestorOf(ISecurityCheckpoint checkPoint)
		{
			return IsAncestorOf((SecurityCheckpoint)checkPoint);
		}

		ISecurityCheckpoint ISecurityCheckpoint.Parent
		{
			get { return Parent; }
		}

		#endregion

		#region ICheckpoint Members

		WTG.SecurityRights.CheckpointLookupKey ICheckpoint.LookupKey
		{
			get
			{
				return new WTG.SecurityRights.CheckpointLookupKey(fCode, fItemGuid);
			}
		}
		ICheckpoint ICheckpoint.Parent
		{
			get
			{
				return Parent;
			}
		}

		#endregion
	}
}
