#if DEBUG
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// When using Windows Forms visual form inheritance, a .net designer issue may cause controls to
	/// move around incorrectly when Anchor is set to something other than Top + Left.
	/// This is a Microsoft bug that this class fixes for forms and user controls.
	/// </summary>
	internal sealed class DesignerInheritedFormsSizeAndLocationFixer : CodeDomVisitor
	{
		DesignerInheritedFormsSizeAndLocationFixer(Control containerControl, IServiceProvider serviceProvider)
		{
			this.containerControl = containerControl;
			this.serviceProvider = serviceProvider;
		}

		#region Fix

		bool rootControlClientSizeOnly;

		/// <summary>
		/// Fixes size and location after deserialize of a form.
		/// Call this method in the setter of each form and user control's Site property.
		/// </summary>
		public static void Fix(Control containerControl)
		{
			if (containerControl.Site != null && containerControl.Site.DesignMode)
			{
				new DesignerInheritedFormsSizeAndLocationFixer(containerControl, containerControl.Site).FixCore();
			}
		}

		void FixCore()
		{
			var designerHost = (IDesignerHost)serviceProvider.GetService(typeof(IDesignerHost));
			FixNow(true); // fix ClientSize only when the Site property is assigned
			if (designerHost != null && designerHost.Loading)
			{
				// fix children when the designer surface has finished loading
				designerHost.LoadComplete += new EventHandler(DesignerHost_LoadComplete);
			}
		}

		void DesignerHost_LoadComplete(object sender, EventArgs e)
		{
			((IDesignerHost)sender).LoadComplete -= new EventHandler(DesignerHost_LoadComplete);
			FixNow(false);
		}

		void FixNow(bool rootControlClientSizeOnly)
		{
			if (IsRootDesignedComponent(containerControl))
			{
				this.rootControlClientSizeOnly = rootControlClientSizeOnly;
				var designerHost = serviceProvider == null ? null : (IDesignerHost)serviceProvider.GetService(typeof(IDesignerHost));
				if (designerHost != null)
				{
					using (var transaction = designerHost.CreateTransaction())
					{
						var codeType = serviceProvider.GetService(typeof(CodeTypeDeclaration)) as CodeTypeDeclaration;
						if (codeType != null)
						{
							VisitMember(codeType);
						}
						if (changesWereMade)
						{
							transaction.Commit();
							changesWereMade = false;
						}
					}
				}
				ReapplyAnchors();
			}
		}

		void ReapplyAnchors()
		{
			foreach (var anchor in savedAnchors)
			{
				anchor.Key.Anchor = anchor.Value;
			}
			savedAnchors.Clear();
		}

		readonly List<KeyValuePair<Control, AnchorStyles>> savedAnchors = new List<KeyValuePair<Control, AnchorStyles>>();

		#endregion

		#region Visitor

		protected override void VisitMember(CodeTypeMember member)
		{
			if (member is CodeTypeDeclaration ||
				(member is CodeMemberMethod &&
				 (member.Name == "InitializeComponent" || member.Name.EndsWith(ControlCodeDomSerializerWithDelayedTabCreate.TabPageMethodNamePostfix))))
			{
				base.VisitMember(member);
			}
		}

		protected override void VisitStatement(CodeStatement statement)
		{
			base.VisitStatement(statement);
			var assignStatement = statement as CodeAssignStatement;
			if (assignStatement != null)
			{
				var left = assignStatement.Left as CodePropertyReferenceExpression;
				var controlField = left == null ? null : left.TargetObject as CodeFieldReferenceExpression;
				var right = assignStatement.Right as CodeObjectCreateExpression;
				if (assignStatement != null &&
					left != null &&
					(controlField != null || left.TargetObject is CodeThisReferenceExpression) &&
					right != null)
				{
					UpdateControlSizeAndLocation(containerControl, controlField == null ? null : controlField.FieldName, left, right);
				}
			}
		}

		#endregion

		#region Implementation

		readonly Control containerControl;
		readonly IServiceProvider serviceProvider;
		bool changesWereMade;

		void UpdateControlSizeAndLocation(Control control, string controlName, CodePropertyReferenceExpression left, CodeObjectCreateExpression right)
		{
			var isPropertySetOnRootContainerControl = controlName == null;
			if (((isPropertySetOnRootContainerControl && rootControlClientSizeOnly && left.PropertyName == "ClientSize" && IsRootDesignedComponent(control)) ||
				(!isPropertySetOnRootContainerControl && !rootControlClientSizeOnly && control.Name == controlName)))
			{
				if ((left.PropertyName == "Location" || left.PropertyName == "Size" || left.PropertyName == "ClientSize") &&
					right.Parameters.Count == 2)
				{
					var arg1Expr = right.Parameters[0] as CodePrimitiveExpression;
					var arg2Expr = right.Parameters[1] as CodePrimitiveExpression;
					var arg1 = arg1Expr == null ? null : arg1Expr.Value;
					var arg2 = arg2Expr == null ? null : arg2Expr.Value;

					var designerHost = (IDesignerHost)serviceProvider.GetService(typeof(IDesignerHost));
					if (left.PropertyName == "ClientSize")
					{
						// setting ClientSize directly cause Location to disappear from other controls!
						var clientSizeDifference = control.Size - control.ClientSize;
						SetPropertyIfRequired("Size", control, ControlDpiScalingHelper.NewScaledSize(
							(int)arg1 - clientSizeDifference.Width,
							(int)arg2 - clientSizeDifference.Height, false));
						changesWereMade = true;
					}
					else if (left.PropertyName == "Size" && control.Dock == DockStyle.None && !(control is TabPage))
					{
						SetPropertyIfRequired("Size", control, ControlDpiScalingHelper.NewScaledSize((int)arg1, (int)arg2, false));
						changesWereMade = true;
					}
					else if (left.PropertyName == "Location" && control.Dock == DockStyle.None && !(control is TabPage))
					{
						SetPropertyIfRequired("Location", control, ControlDpiScalingHelper.NewScaledPoint((int)arg1, (int)arg2, false));
						changesWereMade = true;
					}
				}
			}
			if (IsRootDesignedComponent(control) || control is Panel || control is GroupBox || control is TabControl)
			{
				foreach (Control child in control.Controls)
				{
					UpdateControlSizeAndLocation(child, controlName, left, right);
				}
			}
		}

		bool IsRootDesignedComponent(Control control)
		{
			// IDesignerHost.RootComponent isn't always appropriate because it may be null sometimes.
			// control == containerControl isn't appropriate because both UserControl and Form Site properties invoke this fixer class.
			// control.Parent != null isn't appropriate because System.Windows.Forms.Design.DesignerFrame is shimmed in between.

			var designerHost = (IDesignerHost)serviceProvider.GetService(typeof(IDesignerHost));
			if (designerHost != null && designerHost.RootComponent != null)
			{
				return control == containerControl && control == designerHost.RootComponent;
			}
			else
			{
				return
					control == containerControl &&
					(control.Parent == null ||
					 control.Parent.GetType().FullName.StartsWith("System.Windows.Forms.Design."));
			}
		}

		void SetPropertyIfRequired(string propertyName, Control control, object value)
		{
			var property = TypeDescriptor.GetProperties(control)[propertyName];
			var existingValue = property.GetValue(control);
			if (!object.Equals(existingValue, value))
			{
				property.SetValue(control, value);
			}
		}

		#endregion
	}
}
#endif
