#if DEBUG

namespace Enterprise.Accounting.DataTransfer
{
	public partial class CASSHOTFileFormat
	{
		public string GetClientSpecificFileExtension_ForTestOnly()
		{
			return GetClientSpecificFileExtension();
		}
	}
}

#endif
