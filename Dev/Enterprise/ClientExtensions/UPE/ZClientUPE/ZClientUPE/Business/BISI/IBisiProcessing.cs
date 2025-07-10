using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.BISI
{
	public interface IBisiUpload
	{
		ZDateTime TransferredDateTime { get; set; }
		void OnBeforeBisiUpload();
	}

	public interface IBisiDownload
	{
		ZDateTime TransferredDateTime { get; set; }
		void OnAfterBisiDownload();
	}
}
