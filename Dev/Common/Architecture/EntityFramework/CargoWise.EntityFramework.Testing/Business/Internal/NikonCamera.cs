namespace CargoWise.EntityFramework.Testing
{
	internal class NikonCamera : Camera
	{
		public new Nikkor70_200VR Lens
		{
			get { return new Nikkor70_200VR(); }
		}
	}
}
