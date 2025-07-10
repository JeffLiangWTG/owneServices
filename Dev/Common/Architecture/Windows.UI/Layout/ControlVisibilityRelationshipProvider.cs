using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common;

namespace CargoWise.Windows.UI.Layout
{
	/// <summary>
	/// An IExtenderProvider component that provides the VisibleDependentOn property to controls.
	/// The VisibleDependentOn property makes the visibility of a control dependent on the
	/// visibility of another control.
	/// </summary>
	[ProvideProperty("VisibleDependentOn", typeof(Control))]
	public class ControlVisibilityRelationshipProvider : Component, IExtenderProvider
	{
		/// <summary>
		/// Get the control the visibiility of the given control depends on.
		/// </summary>
		[Category(DesignerConstants.Category)]
		[Description("The control the visibility of this control depends on.")]
		[DefaultValue(null)]
		public Control GetSourceControl(Control control)
		{
			ControlVisibilityRelationship relationship = GetRelationship(control);
			IControlVisibilityProvider visibleDependentOn = relationship == null ? null : relationship.VisibleDependentOn as IControlVisibilityProvider;
			return visibleDependentOn == null ? null : visibleDependentOn.SourceControl;
		}

		/// <summary>
		/// Set the control the visibiility of the given control depends on.
		/// When value.Visible changes, control.Visible is set to value.Visible.
		/// </summary>
		public void SetDependency(Control control, Control sourceControl)
		{
			SetDependency(control, new ControlVisibilityProvider(sourceControl));
		}

		public void SetDependency(Control control, IVisibilityProvider visibilitySource)
		{
			Argument.NotNull(control, "control");
			var previousRelationship = GetRelationship(control);

			if (previousRelationship == null || previousRelationship.VisibleDependentOn != visibilitySource)
			{
				if (previousRelationship != null)
				{
					previousRelationship.Dispose();
				}

				relationships[control] = (visibilitySource == null) ? null : new ControlVisibilityRelationship(control, visibilitySource);
			}
		}

		public void ClearDependency(Control control)
		{
			SetDependency(control, (IVisibilityProvider)null);
		}

		#region IExtenderProvider Members

		bool IExtenderProvider.CanExtend(object extendee)
		{
			return CanExtend(extendee);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "extendee")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		protected bool CanExtend(object extendee)
		{
			return extendee is Control;
		}

		#endregion

		#region Implementation

		readonly Dictionary<Control, ControlVisibilityRelationship> relationships = new Dictionary<Control, ControlVisibilityRelationship>();

		ControlVisibilityRelationship GetRelationship(Control control)
		{
			ControlVisibilityRelationship item = null;
			relationships.TryGetValue(control, out item);
			return item;
		}

		#endregion
	}
}
