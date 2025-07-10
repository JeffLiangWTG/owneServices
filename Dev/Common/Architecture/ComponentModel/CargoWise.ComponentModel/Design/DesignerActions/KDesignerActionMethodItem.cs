using System.ComponentModel.Design;
using System.Reflection;
using System.Security;

namespace CargoWise.ComponentModel.Design
{
	/// <summary>
	/// Represents a smart tag panel item that is associated with a method in a class
	/// derived from DesignerActionList.
	/// </summary>
	[SecurityCritical]
	public class KDesignerActionMethodItem : DesignerActionMethodItem
	{
		#region Constructors

		public KDesignerActionMethodItem(DesignerActionList actionList, string memberName, string displayName)
			: base(actionList, memberName, displayName)
		{
		}

		public KDesignerActionMethodItem(DesignerActionList actionList, MethodInvoker method, string displayName)
			: base(actionList, null, displayName)
		{
			Method = method;
		}

		public KDesignerActionMethodItem(DesignerActionList actionList, string memberName, string displayName, bool includeAsDesignerVerb)
			: base(actionList, memberName, displayName, includeAsDesignerVerb)
		{
		}

		public KDesignerActionMethodItem(DesignerActionList actionList, MethodInvoker method, string displayName, bool includeAsDesignerVerb)
			: base(actionList, null, displayName, includeAsDesignerVerb)
		{
			Method = method;
		}

		public KDesignerActionMethodItem(DesignerActionList actionList, string memberName, string displayName, string category)
			: base(actionList, memberName, displayName, category)
		{
		}

		public KDesignerActionMethodItem(DesignerActionList actionList, string memberName, string displayName, string category, bool includeAsDesignerVerb)
			: base(actionList, memberName, displayName, category, includeAsDesignerVerb)
		{
		}

		public KDesignerActionMethodItem(DesignerActionList actionList, string memberName, string displayName, string category, string description)
			: base(actionList, memberName, displayName, category, description)
		{
		}

		public KDesignerActionMethodItem(DesignerActionList actionList, string memberName, string displayName, string category, string description, bool includeAsDesignerVerb)
			: base(actionList, memberName, displayName, category, description, includeAsDesignerVerb)
		{
		}

		#endregion

		#region Invoke

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
		public delegate void MethodInvoker();

		public override void Invoke()
		{
			if (Method != null)
			{
				Method();
			}
			else
			{
				try
				{
					base.Invoke();
				}
				catch (TargetInvocationException ex)
				{
					throw ex.InnerException;
				}
			}
		}

		#endregion

		#region Implementation

		readonly MethodInvoker Method;

		#endregion
	}
}
