namespace Enterprise.DocumentScanning.GUI
{
	public class RefreshManager
	{
		public RefreshManager()
		{
		}

		public void RegisterForceRefresh()
		{
			fPageChanged = true;
			fRefreshMagnifyingGlass = true;
			fForceRefresh = true;
			fParentResized = true;
		}

		public void RegisterViewUpdated()
		{
			fForceRefresh = false;
			fPageChanged = true;
			fRefreshMagnifyingGlass = true;
			fParentResized = false;
		}

		#region ForceRefresh
		bool fForceRefresh = true;
		public bool ForceRefresh
		{
			get { return fForceRefresh; }
		}
		#endregion

		#region PageChanged
		bool fPageChanged = true;
		public bool PageChanged
		{
			get
			{
				return fPageChanged;
			}
			set
			{
				if (value)
				{
					fRefreshMagnifyingGlass = true;
				}
				fPageChanged = value;
			}
		}
		#endregion

		#region RefreshMagnifyingGlass

		bool fRefreshMagnifyingGlass = true;
		public bool RefreshMagnifyingGlass
		{
			get { return fRefreshMagnifyingGlass; }
			set { fRefreshMagnifyingGlass = value; }
		}
		#endregion

		#region Force Thumbnail Scroll IntoView

		bool fForceThumbnailScrollIntoView;
		public bool ForceThumbnailScrollIntoView
		{
			get { return fForceThumbnailScrollIntoView; }
			set { fForceThumbnailScrollIntoView = value; }
		}
		#endregion

		#region Parent Resized

		bool fParentResized = true;
		public bool ParentResized
		{
			get { return fParentResized; }
			set { fParentResized = value; }
		}
		#endregion
	}
}
