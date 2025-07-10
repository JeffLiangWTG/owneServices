using System.IO;
using CargoWise.EntityFramework;

namespace Enterprise.ArchiveManager.Business.Records
{
	public class VolumeSelectionValidation : AutoVolumeSelectionValidation
	{
		public VolumeSelectionValidation(AutoVolumeSelection parent)
			: base(parent) { }

		#region Implementation

		public new VolumeSelection Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (VolumeSelection)base.Parent; }
		}

		protected override void CheckVolumeLocation()
		{
			base.CheckVolumeLocation();
			MandatoryValidation.CheckEntered(Parent.VolumeLocationInfo);

			if (!string.IsNullOrEmpty(Parent.VolumeLocation))
			{
				if (!Directory.Exists(Parent.VolumeLocation))
				{
					Parent.VolumeLocationInfo.AddError(Res.GetString("36d3ed8b-831f-4e24-ad7f-de2640e30ef5", "Directory or Drive does not exist. Please enter a different path."));
				}
				else
				{
					if (!Parent.VolumeManager.VolumeExists(Parent.VolumeNoToFind, VolumeState.Closed))
					{
						Parent.VolumeLocationInfo.AddError(Res.GetString("36d3ed8b-831f-4e24-ad7f-de2640e31ee5", "Directory or Drive does not contain volume file No: {0}", Parent.VolumeNoToFind));
					}
				}
			}
		}

		#endregion
	}
}
