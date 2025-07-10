namespace Enterprise.Customs.IT.Messaging.SAD;

public interface ISpecialMentionGroup
{
	ISpecialMentionEoriInfo Eori { get; }
	ISpecialMentionUnloadingDataInfo UnloadingData { get; }
}
