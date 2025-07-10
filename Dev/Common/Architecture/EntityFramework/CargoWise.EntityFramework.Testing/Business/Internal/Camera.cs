namespace CargoWise.EntityFramework.Testing
{
	internal abstract class Camera
	{
		public Lens Lens
		{
			get { return null; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Testing")]
		bool PrivateBool
		{
			get { return true; }
		}
	}
}
