using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentScanning.Business
{
	/// <summary>
	/// Does not actually write to CD. This object is used when the user chooses to 
	/// burn their own CD. All it does is write the documents to CD.
	/// </summary>
	public class UserChoiceCDWriter : CDWriter
	{
		public UserChoiceCDWriter(DocumentFactory factory, ArchiveEDocsManager archiver)
			: base(factory, archiver)
		{
		}

		public override ZBool IsUserControlled
		{
			get { return true; }
		}

		[BusinessObjectTestExclude()]
		public override ZString CurrentDrive
		{
			get { return new ZString(); }
			set { }
		}

		[BusinessObjectTestExclude()]
		public override CodeDescriptionPairList DriveList
		{
			get { return new CodeDescriptionPairList(); }
		}

		[BusinessObjectTestExclude()]
		public override ZString CurrentWriteSpeed
		{
			get { return new ZString(); }
			set { }
		}

		[BusinessObjectTestExclude()]
		public override CodeDescriptionPairList WriteSpeedList
		{
			get { return new CodeDescriptionPairList(); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "Openining a directory not currently supported by Remote Desktop Services integration")]
		public override void Burn()
		{
			base.Burn();

#if DEBUG
			if (!IsTesting)
			{
#endif
				System.Diagnostics.Process.Start(CDArchiveTempDirectory);// Openining a directory not currently supported by Remote Desktop Services integration
#if DEBUG
			}
#endif
		}
	}
}
