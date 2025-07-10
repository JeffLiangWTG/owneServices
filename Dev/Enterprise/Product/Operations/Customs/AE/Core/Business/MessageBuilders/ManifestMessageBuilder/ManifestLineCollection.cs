using Enterprise.DataTransfer.Business;

namespace Enterprise.Customs.AE.Business;

public class ManifestLineCollection : FlatFileDataRowCollection
{
	public new ManifestLine this[int index]
	{
		get { return (ManifestLine)base[index]; }
	}
}
