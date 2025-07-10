using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	class AutoLayoutGroupBoxTabControl : ZTabControl
	{
		internal AutoLayoutGroupBoxCollection FilterGroupBoxes = new AutoLayoutGroupBoxCollection();

		internal void RearrangeGroupBoxesInNColumns(int n)
		{
			FilterGroupBoxes.RearrangeGroupsInNColumns(n);
		}

		internal int DesiredWidth
		{
			get { return FilterGroupBoxes.MaxDesiredWidth; }
		}

		internal int DesiredHeight
		{
			get { return FilterGroupBoxes.MaxDesiredHeight; }
		}

		internal AutoLayoutGroupBox AddPageAndGroupBox(string groupName, string groupDescription)
		{
			ZTabPage newPage = new ZTabPage();
			newPage.Name = groupName;
			newPage.Text = string.IsNullOrEmpty(groupName) ? ResString.GetMultilingualString("7873D89A-8F91-49B6-B594-33FABD898DC9", "Primary Filters") : groupName;
			newPage.AutoScroll = true;
			newPage.AutoSize = true;

			AutoLayoutGroupBox filterGroup = new AutoLayoutGroupBox();
			filterGroup.Text = string.IsNullOrEmpty(groupName) ? ResString.GetMultilingualString("7873D89A-8F91-49B6-B594-33FABD898DC9", "Primary Filters") : groupDescription;
			filterGroup.Name = groupName;

			FilterGroupBoxes.Add(filterGroup);
			newPage.Controls.Add(filterGroup);
			TabPages.Add(newPage);
			newPage.TabIndex = 0;

			return filterGroup;
		}

		internal void AddControlToFilterGroup(string groupName, Control controlToAdd)
		{
			if (FilterGroupBoxes.Contains(groupName))
			{
				FilterGroupBoxes[groupName].AddControl(controlToAdd);
			}
		}

		internal void MakeTabControlFitGroups()
		{
			if (!IsHandleCreated)
			{
				CreateHandle(); // so that ItemHeight can calculate the tab height
			}
			foreach (AutoLayoutGroupBox filterGroup in FilterGroupBoxes)
			{
				ControlDpiScalingHelper.SetWidth(filterGroup, ClientSize.Width - TabBorders, false);
				filterGroup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
				if (FilterGroupBoxes.MaxDesiredHeight > MaxPageHeight)
				{
					filterGroup.ForceHeight(Math.Max(MaxPageHeight, filterGroup.DesiredHeight));
				}
				else
				{
					filterGroup.ForceHeight(FilterGroupBoxes.MaxDesiredHeight);
				}
			}
			if (FilterGroupBoxes.MaxDesiredHeight > MaxPageHeight)
			{
				ClientSize = ControlDpiScalingHelper.NewScaledSize(ClientSize.Width, MaxPageHeight + MysteryPixelsFudge + ItemSize.Height, false);
			}
			else
			{
				ClientSize = ControlDpiScalingHelper.NewScaledSize(ClientSize.Width, FilterGroupBoxes.MaxDesiredHeight + ItemSize.Height + MysteryPixelsFudge, false);
			}
		}

		[DpiState(DpiState.ScaleY)]
		internal readonly int MysteryPixelsFudge = ControlDpiScalingHelper.ScaleToCurrentDpiY(8);

		[DpiState(DpiState.ScaleX)]
		readonly int TabBorders = ControlDpiScalingHelper.MarkAsScaled(8); //Tab pages are automatically placed at left = 4 with a width of parent.width-8. 

		internal int MaxPageHeight
		{
			get { return ControlDpiScalingHelper.ScaleToCurrentDpiY(500); }
		}

		internal bool AreGroupsScrolling
		{
			get { return FilterGroupBoxes.MaxDesiredHeight > MaxPageHeight; }
		}

		internal void MakeVisibleIfGroupsContainControls()
		{
			FilterGroupBoxes.MakeElementsVisibleIfTheyContainControls();
		}

		internal bool NeedsToShow
		{
			get { return FilterGroupBoxes.ContainsControls; }
		}
	}
}
