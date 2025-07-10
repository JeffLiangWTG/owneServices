using System.Windows.Forms;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Causes tabbing to continue when a ContainerControl with no TabStop controls is reached.
	/// To use, override ContainerControl.Select() and call AfterSelect after calling base:
	/// 
	/// protected override void Select(bool directed, bool forward)
	/// {
	///     base.Select(directed, forward);
	///     ContainerControlWithNoChildrenTabSkipper.AfterSelect(forward);
	/// }
	///
	/// ContainerControlWithNoChildrenTabSkipper ContainerControlWithNoChildrenTabSkipper
	/// {
	///     get { return containerControlWithNoChildrenTabSkipper ?? (containerControlWithNoChildrenTabSkipper = new ContainerControlWithNoChildrenTabSkipper(this)); }
	/// }
	/// ContainerControlWithNoChildrenTabSkipper containerControlWithNoChildrenTabSkipper;
	/// 
	/// </summary>
	public class ContainerControlWithNoChildrenTabSkipper
	{
		public ContainerControlWithNoChildrenTabSkipper(ContainerControl control)
		{
			this.control = control;
		}

		readonly ContainerControl control;

		public void AfterSelect(bool forward)
		{
			if (!control.IsDesignMode() && !inAfterSelect)
			{
				ContainerControl parentContainer = control.Parent == null ? null : (ContainerControl)control.Parent.GetContainerControl();
				if (parentContainer != null && parentContainer.ActiveControl == control && control.ActiveControl == null)
				{
					if (!(parentContainer.GetNextControl(control, forward) is TabPage))
					{
						inAfterSelect = true;
						try
						{
							control.GetTopLevelNonParentedControl().SelectNextControl(control, forward, true, true, true);
							if (parentContainer.ActiveControl == null && parentContainer.Contains(control))
							{
								parentContainer.ActiveControl = control;
							}
						}
						finally
						{
							inAfterSelect = false;
						}
					}
				}
			}
		}
		bool inAfterSelect;
	}
}
