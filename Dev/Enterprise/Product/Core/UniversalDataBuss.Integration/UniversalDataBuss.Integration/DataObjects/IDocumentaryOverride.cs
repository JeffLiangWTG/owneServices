using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IDocumentaryOverride
	{
		ZString? DocumentName { get; }
		ICodeDescriptionDataObject Purpose { get; }
		ZBool? IsSystemDefined { get; }
		ZInt? DataVersion { get; set; }
		ZInt? SubmissionVersion { get; set; }
	}
}