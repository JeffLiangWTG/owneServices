using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface ITemplateConditional
	{
		ZString TemplateCondition1 { get; set; }
		ZString TemplateCondition2 { get; set; }
		ZString TemplateCondition2Value { get; set; }

		ZString OriginCountryCode { get; set; }
		ZString DestinationCountryCode { get; set; }
	}
}
