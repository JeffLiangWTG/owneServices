using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI
{
	public class BMFilterStripWrapperControl : FilterStripWrapperControl
	{
		public bool ReadOnly
		{
			get => readOnly;
			set
			{
				readOnly = value;

				if (stripControl != null)
				{
					stripControl.ReadOnly = value;
				}

				var fbo = stripControl?.FilterBusinessObject;

				if (fbo != null)
				{
					fbo.ReadOnly = value;
				}
			}
		}
		bool readOnly;

		protected override string NewFilterStripString => ModuleName == ModuleIDs.ProcessHeader.Name || ModuleName == ModuleIDs.BMFilterRule.Name ? "IProcessHeaderFilterStrip" : base.NewFilterStripString;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message only")]
		internal const string UnidentifiedFilterControlExceptionMessage = "An attempt was made to get additional preview filters for a FilterStripWrapperControl that was unidentified. Ensure that the wrapper control has its FilterControlIdentifier property set to something unique for the form that it appears on, and then handle that identifier in the form's GetObjectForPreview method.";
	}
}
