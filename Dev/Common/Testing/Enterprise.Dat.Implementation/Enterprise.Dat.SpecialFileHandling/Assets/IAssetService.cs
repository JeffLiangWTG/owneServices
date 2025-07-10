using System.Threading.Tasks;

namespace Enterprise.Dat.SpecialFileHandling.Assets
{
	public interface IAssetService
	{
		public Task UploadAsync(string assetName, string fileName);
	}
}
