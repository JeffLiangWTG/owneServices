using CargoWise.Common;
using CargoWise.Customs.IT.MessageDefinitions.SingleWindow;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public class SingleWindowStatusCustomsResponseWrapper : ISingleWindowStatusCustomsResponse
{
	public SingleWindowStatusCustomsResponseWrapper(esito_bolletta singleWindowCustomsResponse)
	{
		this.singleWindowCustomsResponse = Argument.NotNull(singleWindowCustomsResponse, nameof(singleWindowCustomsResponse));
	}

	readonly esito_bolletta singleWindowCustomsResponse;

	ZString ISingleWindowStatusCustomsResponse.ControlChannel => singleWindowCustomsResponse.controllo_doganale?.flag_ctrl_dog.ToString() ?? ZString.Empty;

	ZString ISingleWindowStatusCustomsResponse.ReleaseCode => singleWindowCustomsResponse.svincolo?.cod_svincolo ?? ZString.Empty;

	ZDateTime ISingleWindowStatusCustomsResponse.ReleaseDate => singleWindowCustomsResponse.svincolo?.data_codice_svincolo ?? ZDateTime.Empty;
}
