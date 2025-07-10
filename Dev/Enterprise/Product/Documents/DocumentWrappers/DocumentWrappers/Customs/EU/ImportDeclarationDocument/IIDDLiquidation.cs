namespace Enterprise.DocumentWrappers.Customs.EU.ImportDeclarationDocument
{
	public interface IIDDLiquidation<TIDDDutiesAndTaxesWrapper>
		where TIDDDutiesAndTaxesWrapper : DocBaseWrapper, IIDDDutiesAndTax
	{
		DocBaseWrapperCollection<TIDDDutiesAndTaxesWrapper> DutiesAndTax { get; }
	}
}
