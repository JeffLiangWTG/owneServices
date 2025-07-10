using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class BorderTransportCollection : CusCodeDataCollection<BorderTransport>
{
	public BorderTransportCollection(BusinessObject master)
		: base(master, CusCodeDataTypeList.Codes.TransportAtBorder)
	{
		var maxCountForValidation = 1;

		MaxCountValidationEnable(maxCountForValidation);
	}

	public new JobDeclaration Master => (JobDeclaration)base.Master;
}
