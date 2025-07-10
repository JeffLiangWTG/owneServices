using System;

namespace Enterprise.ZArchitecture.Web.ServerServices
{
	public class VolumeCalculatorParameters : WebServiceParameters
	{
		#region Properties

		public string VolumeControlID { get; set; }
		public string DefaultVolume { get; set; }
		public string Pieces { get; set; }
		public string Length { get; set; }
		public string Width { get; set; }
		public string Height { get; set; }
		public string DimUnit { get; set; }
		public string VolumeUnit { get; set; }

		#endregion

		#region Override

		protected override void ValidateCore()
		{
			base.ValidateCore();
			if (string.IsNullOrEmpty(VolumeControlID))
			{
				throw new ArgumentNullException("VolumeControlID");
			}
		}

		#endregion
	}
}
