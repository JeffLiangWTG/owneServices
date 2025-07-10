using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AE.Business;

public class ManifestMessageBuilder
{
	public ManifestMessageBuilder(ForwardingConsol consol)
	{
		this.consol = consol;
	}

	public ZString GetMessage()
	{
		BuildMessage();
		return Root.ToString().Trim();
	}

	public string Errors
	{
		get
		{
			Mapper.Map(consol);
			return Mapper.Errors.ToStringWithNewLineBetweenAppends();
		}
	}

	void BuildMessage()
	{
		ManifestLayout layout = ManifestLayout.FCL;

		if (consol.JK_ConsolMode != Core.Constants.ContainerModes.FCL
			&& consol.JK_ConsolMode != Core.Constants.ContainerModes.LCL
			&& consol.JK_ConsolMode != Core.Constants.ContainerModes.BuyersConsol
			&& consol.JK_ConsolMode != Core.Constants.ContainerModes.Groupage)
		{
			layout = ManifestLayout.General;
		}

		new LayoutBuilder().BuildLayout(Root, consol, layout);
	}

	ManifestLine Root
	{
		get { return root ?? (root = Mapper.Root ?? Mapper.Map(consol)); }
	}
	ManifestLine root;

	ManifestMapper Mapper
	{
		get { return mapper ?? (mapper = new ManifestMapper()); }
	}
	ManifestMapper mapper;
	readonly ForwardingConsol consol;
}
