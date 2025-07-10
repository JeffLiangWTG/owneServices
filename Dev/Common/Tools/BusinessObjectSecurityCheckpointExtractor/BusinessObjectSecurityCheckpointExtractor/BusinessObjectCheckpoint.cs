using System.Xml.Serialization;

namespace BusinessObjectSecurityCheckpointExtractor;
public class BusinessObjectCheckpoint
{
	[XmlElement("checkpoint")]
	public string Checkpoint { get; set; }

	[XmlElement("edit")]
	public string Edit { get; set; }

	[XmlElement("delete")]
	public string Delete { get; set; }

	[XmlElement("new")]
	public string New { get; set; }

	[XmlElement("view")]
	public string View { get; set; }

	[XmlElement("controller")]
	public string Controller { get; set; }

	[XmlElement("module")]
	public string Module { get; set; }

	public void CleanEmptyStrings()
	{
		if (string.IsNullOrWhiteSpace(Checkpoint))
		{
			Checkpoint = null;
		}
		if (string.IsNullOrWhiteSpace(Edit))
		{
			Edit = null;
		}
		if (string.IsNullOrWhiteSpace(Delete))
		{
			Delete = null;
		}
		if (string.IsNullOrWhiteSpace(New))
		{
			New = null;
		}
		if (string.IsNullOrWhiteSpace(View))
		{
			View = null;
		}
	}

	public bool IsEmpty()
	{
		return Checkpoint == null && Edit == null && Delete == null && New == null && View == null;
	}
}
