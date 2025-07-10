using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.SafetyAndSecurity.Messaging.CC315A;

namespace Enterprise.Customs.GB.ICS;

public class CC315AMessagePrettier : IcsSsGreatBritainEDIMessagePrettier
{
	public CC315AMessagePrettier(DeclarationWrapper wrapper)
	{
		this.wrapper = Argument.NotNull(wrapper, nameof(wrapper));
	}

	readonly DeclarationWrapper wrapper;

	public override ZString MakeHumanReadable()
	{
		var interpretation = MessagePrettierCss.CSS
			+ ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				("Message number", wrapper.MessageIdentification),
				("Reference number", wrapper.Header.ReferenceNumber),
				("Transport mode", wrapper.Header.TransportModeAtBorder),
				("Number of items", wrapper.Header.TotalNumberOfItems),
				("Number of packages", wrapper.Header.TotalNumberOfPackages),
				("Gross mass", wrapper.Header.TotalGrossMass.ToString()),
				("Declaration place", wrapper.Header.DeclarationPlace),
				("Commercial reference", wrapper.Header.CommercialReferenceNumber),
				("Conveyance reference", wrapper.Header.ConveyanceReferenceNumber),
				("Place of loading", wrapper.Header.PlaceOfLoading),
				("Place of unloading", wrapper.Header.PlaceOfUnloading),
			});
		return interpretation;
	}
}
