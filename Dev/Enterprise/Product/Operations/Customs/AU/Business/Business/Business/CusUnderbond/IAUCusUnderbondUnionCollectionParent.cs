namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IAUCusUnderbondUnionCollectionParent : Customs.Business.Interfaces.ICusUnderbondUnionCollectionParent
	{
		new CusUnderbondUnionCollection AllUnderbonds { get; }
	}
}
