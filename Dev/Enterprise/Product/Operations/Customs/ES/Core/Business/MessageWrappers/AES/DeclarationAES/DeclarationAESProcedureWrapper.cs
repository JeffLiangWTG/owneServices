using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class DeclarationAESProcedureWrapper : IDeclarationAESProcedure
{
	public DeclarationAESProcedureWrapper(JobComInvoiceLine invoiceLine)
	{
		this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
	}
	readonly JobComInvoiceLine invoiceLine;

	public ZString RequestedCPC => invoiceLine.JI_FormattedProcedure.SubstringSafe(0, 2);

	public ZString PreviousCPC => invoiceLine.JI_FormattedProcedure.SubstringSafe(2, 2);

	public IReadOnlyCollection<ICommonAdditionalCode> AdditionalProcedures
	{
		get
		{
			if (additionalProcedures == null)
			{
				var additionalProceduresList = new List<CommonAdditionalCodeWrapper>();

				var additionalCodesList = invoiceLine.GetAdditionalProcedureCodesListForExportUccMessage();

				ZShort seqNum = 1;
				foreach (var addCode in additionalCodesList)
				{
					additionalProceduresList.Add(new CommonAdditionalCodeWrapper(seqNum, addCode));
					seqNum++;
				}
				additionalProcedures = additionalProceduresList.AsReadOnly();
			}
			return additionalProcedures;
		}
	}
	IReadOnlyCollection<CommonAdditionalCodeWrapper> additionalProcedures;
}
