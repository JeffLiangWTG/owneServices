using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class DeclarationAESCommodityCodeWrapper : CommodityCodeCommonWrapper, IDeclarationAESCommodityCode
{
	public DeclarationAESCommodityCodeWrapper(JobComInvoiceLine invoiceLine) : base(invoiceLine)
	{
	}

	public IReadOnlyCollection<ICommonAdditionalCode> TariffAdditionalCodes
	{
		get
		{
			if (tariffAdditionalCodes == null)
			{
				var tariffAdditionalCodesList = new List<CommonAdditionalCodeWrapper>();

				var additionalCodesList = GetCodeList(invoiceLine.JI_SupplementaryCode1, invoiceLine.JI_SupplementaryCode2);

				ZShort seqNum = 1;
				foreach (var addCode in additionalCodesList)
				{
					tariffAdditionalCodesList.Add(new CommonAdditionalCodeWrapper(seqNum, addCode));
					seqNum++;
				}
				tariffAdditionalCodes = tariffAdditionalCodesList.AsReadOnly();
			}
			return tariffAdditionalCodes;
		}
	}
	IReadOnlyCollection<CommonAdditionalCodeWrapper> tariffAdditionalCodes;

	public IReadOnlyCollection<ICommonAdditionalCode> NationalAdditionalCodes
	{
		get
		{
			if (nationalAdditionalCodes == null)
			{
				var nationalAdditionalCodesList = new List<CommonAdditionalCodeWrapper>();

				var additionalCodesList = GetCodeList(invoiceLine.AdditionalSupplementaryCodes.Select(x => x.CY_Code).ToArray());

				ZShort seqNum = 1;
				foreach (var addCode in additionalCodesList)
				{
					nationalAdditionalCodesList.Add(new CommonAdditionalCodeWrapper(seqNum, addCode));
					seqNum++;
				}
				nationalAdditionalCodes = nationalAdditionalCodesList.AsReadOnly();
			}
			return nationalAdditionalCodes;
		}
	}
	IReadOnlyCollection<CommonAdditionalCodeWrapper> nationalAdditionalCodes;

	List<ZString> GetCodeList(params ZString[] codes)
	{
		var list = new List<ZString>();
		foreach (var code in codes)
		{
			if (!code.IsEmpty)
			{
				list.Add(code);
			}
		}
		return list.OrderBy(x => x).ToList();
	}
}
