using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.GUI.SDF
{
	/// <summary>
	/// Documents plugin for System Defined Fields (SDF). Provides additional fields to override for documents as defined by EDI.
	/// </summary>
	public class DocumentSDFPlugIn : DocumentPlugin
	{
		public DocumentSDFPlugIn(IBusiness hostBusinessEntity)
			: base(hostBusinessEntity)
		{
			SDFControl = new DocumentSDFControl();
		}

		public override string Name
		{
			get { return (NoResString)"System Defined Data"; }
		}

		#region Implementation

		readonly DocumentSDFControl SDFControl;

		protected override Control GetNewUserControl()
		{
			return SDFControl;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (SDFControl != null)
				{
					SDFControl.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return Note;
		}

		public override BusinessObjectFactory Factory
		{
			get { return (Note != null) ? Note.Factory : null; }
		}

		#endregion
	}
}
