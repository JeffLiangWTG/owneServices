using System.Collections.ObjectModel;
using CargoWise.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP.DF_MSG10000_ImportDeclaration;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP.NS3;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business
{
	public class ImportDeclarationResponseAdaptor
	{
		public ImportDeclarationResponseAdaptor(DfNg2754Msg10004ImportDeclarationResponse importDeclarationResponse)
		{
			Argument.NotNull(importDeclarationResponse, nameof(importDeclarationResponse));
			response = importDeclarationResponse.Response;
		}

		public ZString ExternalDeclarationID => response?.Declaration?.DmExtensions?.ExternalDeclarationId?.Value;

		public ZString CustomsStatusNameCode => response?.Status?.NameCode?.Value;

		public ZString DeclarationVersionID => response?.Declaration?.DmExtensions?.VersionId?.Value;

		public ZString CustomsDeclarationNumber => response?.Declaration?.Id?.Value;

		public bool IsResponsePresent => response != null;

		public Collection<DeclarationGoodsShipment> GoodsShipmentList => response?.Declaration?.GoodsShipment;

		public Collection<DeclarationDutyTaxFee> DeclarationDutyTaxFeeList => response?.Declaration?.DutyTaxFee;

		readonly Response response;
	}
}
