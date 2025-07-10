using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public class BaseExportPlausiValidation : PlausiValidation
{
	public override void CheckCH0001(ZPropertyInfo targetPropertyInfo, IMessageSendingDeclaration declaration)
	{
		if (declaration.JE_TransportMode.ToString() is TransportTypeList.Codes.Sea or TransportTypeList.Codes.Mail)
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageCH0001);
		}
	}

	public override void CheckNS30117(ZPropertyInfo targetPropertyInfo, JobDeclaration declaration)
	{
		var declarantAddress = declaration.DeclarantAddress;
		if (declarantAddress != null && declarantAddress.Header.GetBIDNumber() != EnvironmentHelper.GetBusinessPartnerId())
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNS30117);
		}
	}

	public override void CheckNP70127(ZPropertyInfo targetPropertyInfo, OrgHeader header)
	{
		if (!(header?.CustomsCodes?.Cast<OrgCusCode>().Any(x => x.OK_CodeType == OrgCusCode.SwissCodeTypes.BID) ?? true))
		{
			targetPropertyInfo.AddMessageError(PassarValidationMessages.MessageNP70127);
		}
	}
}
