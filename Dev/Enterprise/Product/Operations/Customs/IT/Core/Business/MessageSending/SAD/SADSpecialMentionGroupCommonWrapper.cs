using CargoWise.Common;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class SADSpecialMentionGroupCommonWrapper : ISpecialMentionGroup
{
	public SADSpecialMentionGroupCommonWrapper(CusEntryLine entryLine)
	{
		this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
	}
	protected readonly CusEntryLine entryLine;

	public ISpecialMentionEoriInfo Eori => new SADSpecialMentionEoriInfoWrapper(entryLine.Header);

	public ISpecialMentionUnloadingDataInfo UnloadingData
	{
		get
		{
			if (PreviousProcedureDocument != null)
			{
				return new SADSpecialMentionUnloadingDataInfoWrapper(PreviousProcedureDocument.CSI_Tariff, PreviousProcedureDocument.NetMass, PreviousProcedureDocument.SupplementaryQuantity);
			}
			return SADSpecialMentionUnloadingDataInfoWrapper.Empty();
		}
	}

	#region Implementation

	protected MergedPreviousDocument PreviousProcedureDocument => previousProcedureDocument ?? (previousProcedureDocument = entryLine.GetUnloadingDataPreviousProcedureDocument());
	MergedPreviousDocument previousProcedureDocument;

	#endregion
}
