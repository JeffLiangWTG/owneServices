using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.DocumentVisualizer.GUI
{
	public sealed class DocumentVisualizerPlugIn : ZPlugIn
	{
		public DocumentVisualizerPlugIn(IBusiness hostEntity)
			: base(hostEntity)
		{
			Argument.NotNull(hostEntity, nameof(hostEntity));

			this.hostEntity = hostEntity;
			Enabled = false;
		}

		readonly IBusiness hostEntity;

		#region Plugin Overrides

		protected override ZBool HasUserControl => false;

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.AlwaysAllow;

		public override string Name => "DocumentVisualizer";

		protected override IBusiness GetBusinessEntityForPlugIn() => hostEntity;

		#endregion
	}
}
