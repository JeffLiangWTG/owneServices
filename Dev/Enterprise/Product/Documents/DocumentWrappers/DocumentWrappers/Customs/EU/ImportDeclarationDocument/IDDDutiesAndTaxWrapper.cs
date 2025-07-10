using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentWrappers.Customs.EU.ImportDeclarationDocument
{
	public class IDDDutiesAndTaxWrapper<T> : DocBaseWrapper, IIDDDutiesAndTax
	{
		public IDDDutiesAndTaxWrapper(T dutiesAndTax, BusinessObjectFactory factory) : base(dutiesAndTax, factory)
		{
		}

		public virtual ZString NationalTaxType => ZString.Empty;

		public virtual ZString TaxType => ZString.Empty;

		public virtual ZString Amount => ZString.Empty;

		public virtual ZString TaxRate => ZString.Empty;

		public virtual ZString TaxAmount => ZString.Empty;

		public virtual ZString PayableTaxAmount => ZString.Empty;

		public virtual ZString Status => ZString.Empty;
	}
}
